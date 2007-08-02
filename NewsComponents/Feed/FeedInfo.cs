#region CVS Version Header
/*
 * $Id: FeedInfo.cs,v 1.11 2005/03/20 03:08:38 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2005/03/20 03:08:38 $
 * $Revision: 1.11 $
 */
#endregion

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

namespace NewsComponents.Feed
{
	/// <summary>
	/// represents information about a particular rss feed. 
	/// </summary>
	public class FeedInfo : FeedDetailsInternal
	{						 

		public static readonly FeedInfo Empty = new FeedInfo(String.Empty, new ArrayList(), String.Empty,String.Empty,String.Empty);

		/* 
		internal TimeSpan maxItemAge = TimeSpan.MaxValue; //the maximum amount of time to cache items from this feed			
		/// <summary></summary>
		public TimeSpan MaxItemAge{
			get { return maxItemAge; }
			set { maxItemAge = value; }
		}
		*/ 

		internal string feedLocation; //location in the cache not on the WWW
		public string FeedLocation {
			get { return feedLocation; }
			set { feedLocation = value; }
		}

		internal ArrayList itemsList; 

		public ArrayList ItemsList {
			get { return itemsList; }
			set { itemsList = value; }
		}
		internal Hashtable optionalElements; 

		/// <summary>
		/// Overloaded. Initializer
		/// </summary>
		/// <param name="feedLocation"></param>
		/// <param name="itemsList"></param>
		public FeedInfo(string feedLocation, ArrayList itemsList){
			this.feedLocation = feedLocation;  
			this.itemsList = itemsList; 
		}

		/// <summary>
		/// Overloaded. Initializer
		/// </summary>
		/// <param name="feedLocation"></param>
		/// <param name="itemsList"></param>
		/// <param name="title"></param>
		/// <param name="link"></param>
		/// <param name="description"></param>
		public FeedInfo(string feedLocation, ArrayList itemsList, string title, string link, string description)
			:this(feedLocation, itemsList, title, link, description, new Hashtable()){			
		}

		/// <summary>
		/// Overloaded. Initializer
		/// </summary>
		/// <param name="feedLocation"></param>
		/// <param name="itemsList"></param>
		/// <param name="title"></param>
		/// <param name="link"></param>
		/// <param name="description"></param>
		/// <param name="optionalElements"></param>
		public FeedInfo(string feedLocation, ArrayList itemsList, string title, string link, string description, Hashtable optionalElements){
			this.feedLocation = feedLocation; 
			this.itemsList = itemsList; 
			this.title = title; 
			this.link = link; 
			this.description = description; 
			this.optionalElements = optionalElements; 
		}

		internal string title; 
		/// <summary></summary>
		public string Title{
			get { return title; }
		}

		internal string description; 
		/// <summary></summary>
		public string Description{
			get { return description; }
		}

		internal string link; 
		/// <summary></summary>
		public string Link{
			get { return link; }
		}

		/// <summary>
		/// Table of optional feed elements.
		/// </summary>
		public Hashtable OptionalElements{
			get{ return this.optionalElements; }
		}

		/// <summary>
		/// Gets the type of the FeedDetails
		/// </summary>
		public FeedType Type { get{ return FeedType.Rss; } }


		/// <summary>
		/// Writes this object as an RSS 2.0 feed to the specified writer
		/// </summary>
		/// <param name="writer"></param>
		public void WriteTo(XmlWriter writer){
			this.WriteTo(writer, NewsItemSerializationFormat.RssFeed, true); 
		}

		/// <summary>
		/// Writes this object as an RSS 2.0 feed to the specified writer
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="format">indicates whether we are writing a FeedDemon newspaper or an RSS feed</param>
		public void WriteTo(XmlWriter writer, NewsItemSerializationFormat format){
			this.WriteTo(writer, format, true); 
		}

		/// <summary>
		/// Writes this object as an RSS 2.0 feed to the specified writer
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="format">indicates whether we are writing a FeedDemon newspaper or an RSS feed</param>
		/// <param name="useGMTDate">Indicates whether the date should be GMT or local time</param>		
		public void WriteTo(XmlWriter writer, NewsItemSerializationFormat format, bool useGMTDate){

			//writer.WriteStartDocument(); 

			if(format == NewsItemSerializationFormat.NewsPaper){
				//<newspaper type="channel">
				writer.WriteStartElement("newspaper"); 
				writer.WriteAttributeString("type", "channel"); 
				writer.WriteElementString("title", this.title); 
			}else if(format != NewsItemSerializationFormat.Channel){	
				//<rss version="2.0">
				writer.WriteStartElement("rss"); 
				writer.WriteAttributeString("version", "2.0"); 
			}

			/* These are here because so many people cut & paste into blogs from Microsoft Word 
			writer.WriteAttributeString("xmlns","v",null,"urn:schemas-microsoft-com:office:vml");
			writer.WriteAttributeString("xmlns","x",null,"urn:schemas-microsoft-com:office:excel");
			writer.WriteAttributeString("xmlns","o",null,"urn:schemas-microsoft-com:office:office");
			writer.WriteAttributeString("xmlns","w",null,"urn:schemas-microsoft-com:office:word");
			writer.WriteAttributeString("xmlns","st1",null,"urn:schemas-microsoft-com:office:smarttags");
			writer.WriteAttributeString("xmlns","st2",null,"urn:schemas-microsoft-com:office:smarttags");
			writer.WriteAttributeString("xmlns","asp",null,"http://www.example.com/asp");
			*/    

			//<channel>
			writer.WriteStartElement("channel"); 

			//<title />
			writer.WriteElementString("title", this.Title); 

			//<link /> 
			writer.WriteElementString("link", this.Link); 

			//<description /> 
			writer.WriteElementString("description", this.Description); 

			//<rssbandit:maxItemAge />
			//writer.WriteElementString("maxItemAge", "http://www.25hoursaday.com/2003/RSSBandit/feeds/", this.maxItemAge.ToString()); 

			//other stuff
			foreach(XmlNode node in this.optionalElements.Values){
				writer.WriteRaw(node.OuterXml); 	  
			}

			//<item />
			foreach(NewsItem item in this.itemsList){													
				writer.WriteRaw(item.ToString(NewsItemSerializationFormat.RssItem, useGMTDate)); 					
			}
					
			writer.WriteEndElement();			
						
			if(format != NewsItemSerializationFormat.Channel){
				writer.WriteEndElement();
			}

			//writer.WriteEndDocument(); 
			
		}

		/// <summary>
		/// Provides the XML representation of the feed as an RSS 2.0 feed. 
		/// </summary>
		/// <param name="format">Indicates whether the XML should be returned as an RSS feed or a newspaper view</param>
		/// <returns>the feed as an XML string</returns>
		public string ToString(NewsItemSerializationFormat format){
			return this.ToString(format, true);
		}

		/// <summary>
		/// Provides the XML representation of the feed as an RSS 2.0 feed. 
		/// </summary>
		/// <param name="format">Indicates whether the XML should be returned as an RSS feed or a newspaper view</param>
		/// <param name="useGMTDate">Indicates whether the date should be GMT or local time</param>
		/// <returns>the feed as an XML string</returns>
		public string ToString(NewsItemSerializationFormat format,  bool useGMTDate){
				
			StringBuilder sb     = new StringBuilder("");
			XmlTextWriter writer = new XmlTextWriter(new StringWriter(sb)); 
				
			this.WriteTo(writer, format, useGMTDate); 
				
			writer.Flush(); 
			writer.Close(); 

			return sb.ToString(); 

		}


		/// <summary>
		/// Provides the XML representation of the feed as an RSS 2.0 feed. 
		/// </summary>
		/// <returns>the feed as an XML string</returns>
		public override string ToString(){
				return this.ToString(NewsItemSerializationFormat.RssFeed); 	
		}

		/// <summary>
		/// Returns a copy of this FeedInfo. The OptionalElements and ItemsList are only a shallow copies.
		/// </summary>
		/// <returns>A copy of this FeedInfo</returns>
		public object Clone(){

			FeedInfo toReturn = new FeedInfo(this.feedLocation, (ArrayList) this.itemsList.Clone(), this.title, this.link,
												this.description, (Hashtable) this.optionalElements.Clone()); 

			return toReturn; 		
		}

	}


	/// <summary>
	/// Represents a list of FeedInfo objects. This is primarily used for generating newspaper views of multiple feeds.
	/// </summary>
	public class FeedInfoList: IEnumerable{

		#region Private Members

		/// <summary>
		/// The list of feeds
		/// </summary>
		private ArrayList feeds = new ArrayList();

		/// <summary>
		/// The title of this list when displayed in a newspaper view
		/// </summary>
		private string title;

		#endregion 

		#region Constructors 

		/// <summary>
		/// Creates a list with the specified title
		/// </summary>
		/// <param name="title">The name of the list</param>
		public FeedInfoList(string title){
			this.title = title; 
		}

		#endregion 

		#region Public properties

		/// <summary>
		/// Returns the name of the list
		/// </summary>
		public string Title{
			get { return this.title;}
		}

		#endregion 

		#region Public methods 


		/// <summary>
		/// Adds a new Feed to the list
		/// </summary>
		/// <param name="feed">The FeedInfo object to add</param>
		/// <returns>The position into which the new feed was inserted</returns>
		public int Add(FeedInfo feed){
			return this.feeds.Add(feed);
		}


		/// <summary>
		/// Removes all FeedInfo objects from the list
		/// </summary>
		public void Clear(){
			this.feeds.Clear(); 
		}

		/// <summary>
		/// Provides the XML representation of the list as a FeedDemon newspaper 
		/// </summary>
		/// <returns>the feed list as an XML string</returns>
		public override string ToString(){
				
			StringBuilder sb     = new StringBuilder("");
			XmlTextWriter writer = new XmlTextWriter(new StringWriter(sb)); 
				
			this.WriteTo(writer); 
				
			writer.Flush(); 
			writer.Close(); 

			return sb.ToString(); 

		}


		/// <summary>
		/// Writes this object as a FeedDemon group newspaper to the specified writer
		/// </summary>
		/// <param name="writer"></param>
		public void WriteTo(XmlWriter writer){
									
			writer.WriteStartElement("newspaper"); 
			writer.WriteAttributeString("type", "group"); 
			writer.WriteElementString("title", this.title); 

			foreach(FeedInfo feed in this.feeds){
				feed.WriteTo(writer, NewsItemSerializationFormat.Channel, false); 
			}
			
			writer.WriteEndElement(); 

		}


		/// <summary>
		/// Returns an enumerator used to iterate over the FeedInfo objects in the list
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator(){
			return this.feeds.GetEnumerator(); 
		}

		#endregion 
	
	}
}
