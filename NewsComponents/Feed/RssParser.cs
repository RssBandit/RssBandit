#region CVS Version Header
/*
 * $Id: RssParser.cs,v 1.25 2005/06/09 14:24:17 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2005/06/09 14:24:17 $
 * $Revision: 1.25 $
 */
#endregion

using System;
using System.Xml; 
using System.IO; 
using System.Collections;
using System.Net;
using System.Text;

using NewsComponents;
using NewsComponents.Net;
using NewsComponents.Utils;

namespace NewsComponents.Feed {

	/// <summary>
	/// Supported Feed Formats (subscription).
	/// </summary>
	public enum SyndicationFormat{
		/// <summary>
		/// Dave Winer's Family of specs including RSS 0.91 &amp; RSS 2.0
		/// </summary>
		Rss,
		/// <summary>
		/// The RDF based syndication formats such as RSS 0.9 and RSS 1.0 
		/// </summary>
		Rdf, 
		/// <summary>
		/// The Atom syndication format
		/// </summary>
		Atom, 
		/// <summary>
		/// An unknown and hence unsupported feed format
		/// </summary>
		Unknown
	}

	/// <summary>
	/// Class for managing RSS feeds. This class is NOT thread-safe.
	/// </summary>
	public class RssParser {

		private static readonly log4net.ILog _log = RssBandit.Common.Logging.Log.GetLogger(typeof(RssParser));

		/// <summary>
		/// Proxy server information used for connections when fetching feeds. 
		/// </summary>
		private static IWebProxy global_proxy = GlobalProxySelection.GetEmptyWebProxy(); 

		/// <summary>
		/// Proxy server information used for connections when fetching feeds. 
		/// </summary>
		public static IWebProxy GlobalProxy{
			set{ 
				//HACK: If ever a situation occurs where multiple RssParsers 
				//can use different proxy servers then things break down. 
				global_proxy = value;
			}
			get { return global_proxy;}		
		}
		
		/// <summary>
		/// Indicates whether the application is offline or not. 
		/// </summary>
		private bool offline = false; 

		/// <summary>
		/// Indicates whether the application is offline or not. 
		/// </summary>
		public bool Offline{
			set { offline = value; }
			get { return offline; }
		}

		#region NameTable Indexes -- See FillNameTable() for string mappings
			
		private static int nt_ispermalink = 0;
		private static int nt_description = 1;		
		private static int nt_body = 2;
		private static int nt_encoded = 3;
		private static int nt_guid = 4;
		private static int nt_link = 5;
		private static int nt_title = 6;
		private static int nt_pubdate = 7;
		private static int nt_date = 8;
		private static int nt_category = 9;
		private static int nt_subject = 10;
		private static int nt_comments = 11;
		private static int nt_flagstatus = 12;
		private static int nt_content = 13;
		private static int nt_summary = 14;
		private static int nt_rel = 15;
		private static int nt_href = 16;
		private static int nt_modified = 17;
		private static int nt_issued = 18;
		private static int nt_type = 19;
		private static int nt_rss = 20;
		private static int nt_rdf = 21;
		private static int nt_feed = 22;
		private static int nt_channel = 23;
		private static int nt_lastbuilddate = 24;
		private static int nt_image = 25;
		private static int nt_item = 26;
		private static int nt_items = 27;
		private static int nt_maxitemage = 28;
		private static int nt_tagline = 29;
		private static int nt_entry = 30;
		private static int nt_id = 31;
		private static int nt_author = 32;
		private static int nt_creator = 33;
		private static int nt_name = 34;
		private static int nt_reference = 35;
		private static int nt_ns_dc = 36;
		private static int nt_ns_xhtml = 37;
		private static int nt_ns_content = 38;
		private static int nt_ns_annotate = 39;
		private static int nt_ns_bandit_2003 = 40;
		private static int nt_ns_slash = 41;		

		private static int NT_SIZE = 1 + nt_ns_slash;	// last used + 1

		#endregion 


		/// <summary>
		/// Returns true, if the RssParser is able to process the url
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static bool CanProcessUrl(string url) {
			if (StringHelper.EmptyOrNull(url))
				return false;
			if (url.StartsWith("nntp") || url.StartsWith("news") || url.StartsWith("http") || url.StartsWith("file") || File.Exists(url))
				return true;
			return false;
		}
		
		/// <summary>
		/// XmlDocument object where nodes placed in NewsItem.OptionalElements come from.
		/// </summary>
		private static XmlDocument optionalElementsDoc    = new XmlDocument();


		//Search	impl. 

		
		/// <summary>
		/// The owner, used for internal callbacks
		/// </summary>
		private NewsHandler owner;

		#region ctor's
		/// <summary>
		/// Initializes class. 
		/// </summary>
		private RssParser(){;}
		public RssParser(NewsHandler owner){
			if (owner == null)
				throw new ArgumentNullException("owner");
			this.owner = owner;
		}
		#endregion

		/// <summary>
		/// Helper function breaks up a string containing quote characters into 
		///	a series of XPath concat() calls. 
		/// </summary>
		/// <param name="input">input string</param>
		/// <returns>broken up string</returns>
		private static string buildXPathString (string input) {
			string[] components = input.Split(new char[] { '\''});
			string result = "";
			result += "concat(''";
			for (int i = 0; i < components.Length; i++) {
				result += ", '" + components[i] + "'";
				if (i < components.Length - 1) {
					result += ", \"'\"";
				}
			}
			result += ")";
			Console.WriteLine(result);
			return result;
		}


		
		/// <summary>
		/// Constructs an NewsItem object out of an XmlNode containing an RSS &lt;item&gt;
		/// </summary>
		/// <param name="f">The feed object the NewsItem being created belongs to</param>
		/// <param name="reader">XmlReader</param>
		/// <returns>An NewsItem object containing the information in the XML node passed in.</returns>
		public static NewsItem MakeRssItem(feedsFeed f, XmlReader reader){
			//setup the NameTable used by the XmlReader
			object[] atomized_strings = FillNameTable(reader.NameTable);
			//move to 'item' element 
			if(reader.ReadState == ReadState.Initial){ 
				reader.Read(); 
			}
			return RssParser.MakeRssItem(f, reader, atomized_strings, DateTime.Now.ToUniversalTime());
		}
		

		/// <summary>
		/// Constructs an NewsItem object out of an XmlReader positioned on an RSS &lt;item&gt;
		/// </summary>
		/// <param name="f">The feed object the NewsItem being created belongs to</param>
		/// <param name="reader">The RSS item</param>
		/// <param name="atomized_strings">An object array containing the common element names that will be tested for in the feed. The objects in 
		/// the array are from the XmlReader's nametable</param>
		/// <param name="defaultItemDate">The default item date to be used if there is no/invalid date information on an item.</param>
		/// <returns>An NewsItem object containing the information in the XmlReader passed in.</returns>
		public static NewsItem MakeRssItem(feedsFeed f, XmlReader reader, object[] atomized_strings, DateTime defaultItemDate){
		
			
			ContentType ctype  = ContentType.Text; 
			string description = null; 	
			string id          = null; 
			string parentId    = null;
			string link        = null; 
			string title       = null; 
			string subject     = null; 
			string author      = null; 
			int commentCount   = NewsItem.NoComments; 
			DateTime date      = defaultItemDate; 	
			DateTime now       = date; 
			Hashtable optionalElements = new Hashtable(); 
			Flagged flagged    = Flagged.None; 
			ArrayList subjects = new ArrayList(); 
			string itemNamespaceUri = reader.NamespaceURI; //the namespace URI of the RSS item
			 			
			bool nodeRead = false; //indicates whether the last node was read using XmlDocument.ReadNode()	

			while((nodeRead || reader.Read()) && reader.NodeType != XmlNodeType.EndElement){

				nodeRead = false; 
				object localname = reader.LocalName;
				object namespaceuri = reader.NamespaceURI;
		
				if(reader.NodeType!= XmlNodeType.Element){ continue; }					
				
				/* string nodeNamespaceUri = reader.NamespaceURI;
				if (StringHelper.EmptyOrNull(nodeNamespaceUri))
					nodeNamespaceUri = itemNamespaceUri;	*/	// if in node has no namespace, assume in RSS namespace

				// save some string comparisons
				bool nodeNamespaceUriEqual2Item = reader.NamespaceURI.Equals(itemNamespaceUri);
				bool nodeNamespaceUriEqual2DC = (namespaceuri == atomized_strings[RssParser.nt_ns_dc]);


				if((description == null) || (localname == atomized_strings[RssParser.nt_body]) || (localname == atomized_strings[RssParser.nt_encoded]) ){ //prefer to replace rss:description/dc:description with content:encoded
					
					if((namespaceuri == atomized_strings[RssParser.nt_ns_xhtml])
						&& (localname == atomized_strings[RssParser.nt_body])){
						if(!reader.IsEmptyElement){
							XmlElement  elem = (XmlElement) optionalElementsDoc.ReadNode(reader); 
							nodeRead = true; 
							description = elem.InnerXml;
							elem = null; 
						}
						ctype  = ContentType.Xhtml; 					
						continue; 
					}else if((namespaceuri == atomized_strings[RssParser.nt_ns_content])
						&& (localname == atomized_strings[RssParser.nt_encoded])){
						
						if(!reader.IsEmptyElement){
							description = ReadElementString(reader); 
						}
						ctype  = ContentType.Html; 
						continue; 
					}else if((nodeNamespaceUriEqual2Item || nodeNamespaceUriEqual2DC)
						&& (localname == atomized_strings[RssParser.nt_description])){
						if(!reader.IsEmptyElement){
							description = ReadElementString(reader); 						
						}
						ctype  = ContentType.Text; 
						continue; 
					} 
				
				}
				
				if (link != null && link.Trim().Length == 0)
					link = null;	// reset on empty elements

				if((link == null) || (localname == atomized_strings[RssParser.nt_guid])) { //favor rss:guid over rss:link
				
					if(nodeNamespaceUriEqual2Item 
						&& (localname == atomized_strings[RssParser.nt_guid])){
						
						if((reader["isPermaLink"] == null) || 
							(StringHelper.AreEqualCaseInsensitive(reader["isPermaLink"], "true"))) {
							if(!reader.IsEmptyElement){
								link = ReadElementString(reader); 
							}
						}else if(StringHelper.AreEqualCaseInsensitive(reader["isPermaLink"], "false")){															
							if(!reader.IsEmptyElement){
								id = ReadElementString(reader); 
							}
						}

						continue; 
					}else if(nodeNamespaceUriEqual2Item 
						&& (localname == atomized_strings[RssParser.nt_link])){
						if(!reader.IsEmptyElement){
							link = ReadElementString(reader); 
						}
						continue; 
					}
											
				}
				
				if(title == null){
				
					if(nodeNamespaceUriEqual2Item 
						&& (localname == atomized_strings[RssParser.nt_title])){
						if(!reader.IsEmptyElement){
							title = ReadElementString(reader);
						}
						continue; 
					}
				
				}


				if((author == null) || (localname == atomized_strings[RssParser.nt_creator])){ //prefer dc:creator to <author>
				
					if(nodeNamespaceUriEqual2DC && 
						(localname == atomized_strings[RssParser.nt_creator] || 
						 localname == atomized_strings[RssParser.nt_author])){
						if(!reader.IsEmptyElement){
							author = ReadElementString(reader);
						}
						continue; 
					}else if(nodeNamespaceUriEqual2Item  && (localname == atomized_strings[RssParser.nt_author])){
						if(!reader.IsEmptyElement){
							author = ReadElementString(reader);
						}
						continue; 
					}				

				}
				
				if((parentId == null) && (localname == atomized_strings[RssParser.nt_reference])){

					if(namespaceuri == atomized_strings[RssParser.nt_ns_annotate]){
						parentId = reader.GetAttribute("resource", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"); 
					}
					continue; 
				}

				if(nodeNamespaceUriEqual2DC && (localname == atomized_strings[RssParser.nt_subject])){
					if(!reader.IsEmptyElement){
						subjects.Add(ReadElementString(reader));
					}
					continue;
				}else if(nodeNamespaceUriEqual2Item 
					&& (localname == atomized_strings[RssParser.nt_category])){
					if(!reader.IsEmptyElement){
						subjects.Add(ReadElementString(reader));
					}
					continue;
				}
			
				
				if((localname == atomized_strings[RssParser.nt_flagstatus])
					&& (namespaceuri == atomized_strings[RssParser.nt_ns_bandit_2003])){
					if(!reader.IsEmptyElement){
						flagged = (Flagged) Enum.Parse(flagged.GetType(), ReadElementString(reader)); 
					}
					continue;
				}

				if(commentCount== NewsItem.NoComments){
				
					if((localname == atomized_strings[RssParser.nt_comments])
						&& (namespaceuri == atomized_strings[RssParser.nt_ns_slash])){
						try{
							if(!reader.IsEmptyElement){
								commentCount = Int32.Parse(ReadElementString(reader));
							}							
						}catch(Exception){ /* DO NOTHING */}
						continue; 
					}
				}

				if(date == now){
				
					try{ 
						if(nodeNamespaceUriEqual2Item 
							&& (localname == atomized_strings[RssParser.nt_pubdate])){
							if(!reader.IsEmptyElement){
								date = DateTimeExt.Parse(ReadElementString(reader));
							}
							continue;
						}else if(nodeNamespaceUriEqual2DC && (localname == atomized_strings[RssParser.nt_date])){
							if(!reader.IsEmptyElement){
								date = DateTimeExt.ToDateTime(ReadElementString(reader));  
							}						
							continue;
						}

											
					}catch(FormatException fe){ /* date was improperly formated*/
						_log.Warn("Error parsing date from item {" + subject + 
							"} from feed {" + link + "}: " + fe.Message);
						continue; 
					}

				} 

				XmlQualifiedName qname = new XmlQualifiedName(reader.LocalName, reader.NamespaceURI); 
				XmlNode optionalNode   = optionalElementsDoc.ReadNode(reader); 
				nodeRead               = true;

				/* some elements occur multiple times in feeds, only the 1st is picked */				
				if(!optionalElements.Contains(qname)){
					optionalElements.Add(qname, optionalNode); 					
				}
			
			}//while				

			//HACK: Sometimes we get garbled items due to network issues, this ensures we don't send them to the UI
			if(link == null && id == null && title == null && date == now){
				return null; 
			}

			/* create Subject if any */
			for(int i = 0; i < subjects.Count; i++){			
				subject += (i > 0 ? " | " + subjects[i] : subjects[i]); 
			}

			/* set value of id to link if no guid in XML stream */ 
			id = (id == null ? link : id);  

			NewsItem newsItem = new NewsItem(f, title, link, description, date, subject, ctype, optionalElements, id, parentId);									       			    
			newsItem.FlagStatus = flagged;
			newsItem.CommentCount = commentCount; 
			newsItem.Author       = author; 
		
			return newsItem; 				
		
		}


		/// <summary>
		/// Constructs an NewsItem object out of an XmlNode containing an ATOM &lt;entry&gt;
		/// </summary>
		/// <param name="f">The feed object the NewsItem being created belongs to</param>
		/// <param name="reader">XmlReader</param>
		/// <returns>An NewsItem object containing the information in the XML node passed in.</returns>
		public static NewsItem MakeAtomItem(feedsFeed f, XmlReader reader){
			//setup the NameTable used by the XmlReader
			object[] atomized_strings = FillNameTable(reader.NameTable);
			//move to 'entry' element 
			if(reader.ReadState == ReadState.Initial){ 
				reader.Read(); 
			}
			return RssParser.MakeAtomItem(f, reader, atomized_strings, DateTime.Now.ToUniversalTime());
		}
		
		
		/// <summary>
		/// Constructs an NewsItem object out of an XmlReader positioned on an RSS &lt;item&gt;
		/// </summary>
		/// <param name="f">The feed object the NewsItem being created belongs to</param>
		/// <param name="reader">The RSS item</param>
		/// <param name="atomized_strings">An object array containing the common element names that will be tested for in the feed. The objects in 
		/// the array are from the XmlReader's nametable</param>
		/// <param name="defaultItemDate">The default item date to be used if there is no/invalid date information on an item.</param>
		/// <returns>An NewsItem object containing the information in the XmlReader passed in.</returns>
		public static NewsItem MakeAtomItem(feedsFeed f, XmlReader reader, object[] atomized_strings, DateTime defaultItemDate){
		
			
			ContentType ctype  = ContentType.Text; 
			string description = null; 	
			string author      = null; 
			string id          = null; 
			string link        = null; 
			string title       = null; 
			string subject     = null; 
			int commentCount   = NewsItem.NoComments; 
			DateTime date      = defaultItemDate; 	
			DateTime now       = date; 
			Hashtable optionalElements = new Hashtable(); 
			Flagged flagged    = Flagged.None; 
			ArrayList subjects = new ArrayList(); 
			string itemNamespaceUri = reader.NamespaceURI; 
			 			
			bool nodeRead = false; //indicates whether the last node was read using XmlDocument.ReadNode()	

			try{

				while((nodeRead || reader.Read()) && reader.NodeType != XmlNodeType.EndElement){

					nodeRead = false; 
					object localname = reader.LocalName;
					object namespaceuri = reader.NamespaceURI;
		
					if(reader.NodeType!= XmlNodeType.Element){ continue; }					
				
					//string nodeNamespaceUri = reader.NamespaceURI;

					// save some string comparisons
					bool nodeNamespaceUriEqual2Item = itemNamespaceUri.Equals(reader.NamespaceURI);				
					bool nodeNamespaceUriEqual2DC = (namespaceuri == atomized_strings[RssParser.nt_ns_dc]); 

		
					if((description == null) || (localname == atomized_strings[RssParser.nt_content])) { //prefer to replace atom:summary with atom:content
					
						if(nodeNamespaceUriEqual2Item 
							&& (localname == atomized_strings[RssParser.nt_content])){
						
							ctype       = GetMimeTypeOfAtomElement(reader);

							if((ctype != ContentType.Unknown) && (!reader.IsEmptyElement)){						
								description = GetContentFromAtomElement(reader, ref nodeRead); 								
							}										
							continue; 
						}else if(nodeNamespaceUriEqual2Item 
							&& (localname == atomized_strings[RssParser.nt_summary])){
						
							ctype       = GetMimeTypeOfAtomElement(reader);

							if((ctype != ContentType.Unknown) && (!reader.IsEmptyElement)){						
								description = GetContentFromAtomElement(reader, ref nodeRead); 								
							}										
							continue; 
						}
				
					}
				
					if (link != null && link.Trim().Length == 0)
						link = null;	// reset on empty elements
				
					if(link == null){									
				
						if(nodeNamespaceUriEqual2Item 
							&& (localname == atomized_strings[RssParser.nt_link]) && 
							((reader["rel"] == null) || 
							(reader["rel"].Equals("alternate")))) {

							if(reader["href"]!= null){
								link = reader.GetAttribute("href"); 							
							}
							continue; 
						} 
											
					}
				
					if(title == null){
				
						if(nodeNamespaceUriEqual2Item 
							&& (localname == atomized_strings[RssParser.nt_title])){
							if(!reader.IsEmptyElement){
								title = ReadElementString(reader);
							}
							continue; 
						}
				
					}

					if(id == null){
				
						if(nodeNamespaceUriEqual2Item 
							&& (localname == atomized_strings[RssParser.nt_id])){
							if(!reader.IsEmptyElement){
								id = ReadElementString(reader);
							}
							continue; 
						}
				
					}
		
		
					if(author == null){ 
				
						if(nodeNamespaceUriEqual2Item  && (localname == atomized_strings[RssParser.nt_author])){
							
							if(!reader.IsEmptyElement){
								while(reader.Read() && reader.NodeType != XmlNodeType.EndElement){

									if(reader.NodeType!= XmlNodeType.Element){ continue; }	
									 
									localname = reader.LocalName; 

									if(localname == atomized_strings[RssParser.nt_name]){
										if(!reader.IsEmptyElement){
											author = ReadElementString(reader); 											
										}
									}else{
										if(!reader.IsEmptyElement){
											ReadElementString(reader); 
										}
									}
								}//while(reader.NodeType != XmlNodeType.EndElement...) 
							}//if(!reader.IsEmptyElement)
							continue; 
						}				

					}



					if(nodeNamespaceUriEqual2DC 
						&& (localname == atomized_strings[RssParser.nt_subject])){
						if(!reader.IsEmptyElement){
							subjects.Add(ReadElementString(reader));
						}
						continue;
					}
			
				
					if((namespaceuri == atomized_strings[RssParser.nt_ns_bandit_2003]) 
						&& (localname == atomized_strings[RssParser.nt_flagstatus])){
						if(!reader.IsEmptyElement){
							flagged = (Flagged) Enum.Parse(flagged.GetType(), ReadElementString(reader)); 
						}
						continue;
					}

					if(commentCount== NewsItem.NoComments){
				
						if((namespaceuri == atomized_strings[RssParser.nt_ns_slash]) 
							&& (localname == atomized_strings[RssParser.nt_comments])){
							try{
								if(!reader.IsEmptyElement){
									commentCount = Int32.Parse(ReadElementString(reader));
								}							
							}catch(Exception){ /* DO NOTHING */}
							continue; 
						}
					}

					if(date == now){
				
						try{ 
							if(nodeNamespaceUriEqual2Item 
								&& (localname == atomized_strings[RssParser.nt_modified])){
								if(!reader.IsEmptyElement){
									date = DateTimeExt.Parse(ReadElementString(reader));
								}
								continue;
							}else if(nodeNamespaceUriEqual2Item 
								&& (localname == atomized_strings[RssParser.nt_issued])){
								if(!reader.IsEmptyElement){
									date = DateTimeExt.ToDateTime(ReadElementString(reader));  
								}						
								continue;
							}

											
						}catch(FormatException fe){ /* date was improperly formated*/
				
							_log.Warn("Error parsing date from item {" + subject + 
								"} from feed {" + link + "}: " + fe.Message);
							continue; 
						}

					} 

					XmlQualifiedName qname = new XmlQualifiedName(reader.LocalName, reader.NamespaceURI); 
					XmlNode optionalNode   = optionalElementsDoc.ReadNode(reader); 
					nodeRead               = true;

					/* some elements occur multiple times, only the 1st is picked */				
					if(!optionalElements.Contains(qname)){
						optionalElements.Add(qname, optionalNode); 					
					}
			
				}//while				

				//HACK: Sometimes we get garbled items due to network issues, this ensures we don't send them to the UI
				if(link == null && id == null && title == null && date == now){
					return null; 
				}

				/* create Subject if any */
				for(int i = 0; i < subjects.Count; i++){			
					subject += (i > 0 ? " | " + subjects[i] : subjects[i]); 
				}

				/* set value of id to link if no guid in XML stream */ 
				id = (id == null ? link : id);  

				NewsItem newsItem = new NewsItem(f, title, link, description, date, subject, ctype, optionalElements, id, null);	;				
				newsItem.FlagStatus = flagged;
				newsItem.CommentCount = commentCount; 
				newsItem.Author       = author; 
				return newsItem; 	
		
			}catch(Exception e){e.ToString();}								
		
			return null; 
		}


	  

		/// <summary>
		/// Reads the RSS feed from the feedsFeed link then caches and returns the feed items 
		/// in an array list.
		/// </summary>
		/// <param name="f">Information about the feed. This information is updated based
		/// on the results of processing the feed. </param>
		/// <returns>An arraylist of RSS items (i.e. instances of the NewsItem class)</returns>
		/// <exception cref="ApplicationException">If the RSS feed is not 
		/// version 0.91, 1.0 or 2.0</exception>
		/// <exception cref="XmlException">If an error occured parsing the 
		/// RSS feed</exception>	
		public ArrayList GetItemsForFeed(feedsFeed f){

			if (this.offline)
				return new ArrayList();

			ArrayList returnList = new ArrayList();

			using (Stream mem = AsyncWebRequest.GetSyncResponseStream(f.link, null, owner.UserAgent, owner.Proxy)) {
				returnList = RssParser.GetItemsForFeed(f, mem, false).itemsList; 
			}

			return returnList;

		}

		/// <summary>
		/// Reads the RSS feed from the feedsFeed link then caches and returns the feed items 
		/// in an array list.
		/// </summary>
		/// <param name="feedUrl">The feed Url.</param>
		/// <returns>An arraylist of RSS items (i.e. instances of the NewsItem class)</returns>
		/// <exception cref="ApplicationException">If the RSS feed is not 
		/// version 0.91, 1.0 or 2.0</exception>
		/// <exception cref="XmlException">If an error occured parsing the 
		/// RSS feed</exception>	
		public  ArrayList GetItemsForFeed(string feedUrl){

			feedsFeed f = new feedsFeed();
			f.link = feedUrl;
			return this.GetItemsForFeed(f); 
		}

	 
		/// <summary>
		/// Reads the RSS feed from the stream then caches and returns the feed items 
		/// in an array list.
		/// </summary>
		/// <remarks>If the feedUrl is currently not stored in this object's internal table 
		///	then it is added/</remarks>		
		/// <param name="f">Information about the feed. This information is updated based
		/// on the results of processing the feed. </param>
		/// <param name="feedReader">A reader containing an RSS feed.</param>				
		/// <param name="cachedStream">Flag states update last retrieved date on feed only 
		/// if the item was not cached. Indicates whether the lastretrieved date is updated
		/// on the feedsFeed object passed in. </param>
		/// <returns>A FeedInfo object which represents the feed</returns>
		/// <exception cref="ApplicationException">If the RSS feed is not 
		/// version 0.91, 1.0 or 2.0</exception>
		/// <exception cref="XmlException">If an error occured parsing the 
		/// RSS feed</exception>	
		public static FeedInfo GetItemsForFeed(feedsFeed f, XmlReader feedReader, bool cachedStream) {

			ArrayList items  = new ArrayList();
			Hashtable optionalElements = new Hashtable();					
			string feedLink = String.Empty, feedDescription= String.Empty, feedTitle= String.Empty, maxItemAge= String.Empty; 			  
			SyndicationFormat feedFormat = SyndicationFormat.Unknown; 
			DateTime defaultItemDate = RelationCosmos.RelationCosmos.UnknownPointInTime;
			DateTime channelBuildDate = DateTime.Now.ToUniversalTime();

			int readItems = 0; 
						
			string rssNamespaceUri = String.Empty; 


			//setup the NameTable used by the XmlReader
			object[] atomized_strings = FillNameTable(feedReader.NameTable);

			feedReader.MoveToContent(); 
			object localname = feedReader.LocalName;

			try{	

				if((localname == atomized_strings[RssParser.nt_rdf]) &&
					feedReader.NamespaceURI.Equals("http://www.w3.org/1999/02/22-rdf-syntax-ns#")){ //RSS 0.9 or 1.0
				
					feedReader.Read(); //go to end of 'RDF' start tag
					feedReader.MoveToContent(); //go to the next element  

					feedFormat = SyndicationFormat.Rdf; 

					localname = feedReader.LocalName;
					//figure out if 0.9 or 1.0 by peeking at namespace URI of <channel> element    
					if(localname == atomized_strings[RssParser.nt_channel]){

						rssNamespaceUri = feedReader.NamespaceURI;
					}else { //no channel, just assume RSS 1.0 
						rssNamespaceUri = "http://purl.org/rss/1.0/";
					}

				}else if(localname == atomized_strings[RssParser.nt_rss]){ //RSS 0.91 or 2.0 

					feedFormat      = SyndicationFormat.Rss; 
					rssNamespaceUri = feedReader.NamespaceURI; 	
		
					do{
						feedReader.Read(); //go to end of 'rss' start tag
						feedReader.MoveToContent(); //go to the next element		
						localname = feedReader.LocalName;
					}while(localname != atomized_strings[RssParser.nt_channel]);

				}else if(feedReader.NamespaceURI.Equals("http://purl.org/atom/ns#")
					&& (localname == atomized_strings[RssParser.nt_feed])){ //Atom 0.3
				
					rssNamespaceUri = feedReader.NamespaceURI; 	

					if( feedReader.MoveToAttribute("version") && feedReader.Value.Equals("0.3")){				
						feedFormat      = SyndicationFormat.Atom; 		
						feedReader.MoveToElement(); //move back to 'feed' start element						
					} else { 
						throw new ApplicationException(Resource.Manager["RES_ExceptionUnsupportedAtomVersion"]);
					}								
	
				}else{
					throw new ApplicationException(Resource.Manager["RES_ExceptionUnknownXmlDialect"]);
				}

			
				ProcessFeedElements(f, feedReader, atomized_strings, rssNamespaceUri, feedFormat, ref feedLink, ref feedTitle, ref feedDescription, ref maxItemAge, ref channelBuildDate, optionalElements, items, defaultItemDate); 
			
			}finally{
				feedReader.Close(); 	
			}

			feedReader = null; 
			atomized_strings = null; 

			// CacheManager works with FeedInfo only and requires a feedLink.
			// So we check/set it here if not found in the feed itself.
			// It will be updated the time the feed owner provide a valid value.
			if (StringHelper.EmptyOrNull(feedLink) && f.link.IndexOf("/") >= 0)
				feedLink = f.link.Substring(0, f.link.LastIndexOf("/")+1);	// including the slash

			FeedInfo fi = new FeedInfo(f.cacheurl, items, feedTitle, feedLink, feedDescription, optionalElements);					
#if NIGHTCRAWLER 
			NewsChannelServices.ProcessItem(fi);
#endif
			NewsHandler.RelationCosmosAddRange(items);

			int iSize = 0;	   

			bool isNntpFeed = f.link.StartsWith("news") || f.link.StartsWith("nntp");

			foreach(NewsItem ri in items){							
				if((f.storiesrecentlyviewed!= null) && f.storiesrecentlyviewed.Contains(ri.Id)){
					ri.BeenRead = true; 
					readItems++;
				}
										
				iSize += ri.GetSize();
			
				ri.FeedDetails = fi; 
				
				if (ri.Date == defaultItemDate) { 	// update with channel build date, if not added to relationCosmos (that adjust the date)
					ri.SetDate(channelBuildDate);
					channelBuildDate = channelBuildDate.AddSeconds(-1.0);	// make it a bit older then the previous
				}

				if(isNntpFeed){
					ri.CommentStyle = SupportedCommentStyle.NNTP;
				}
				
#if NIGHTCRAWLER 
				NewsChannelServices.ProcessItem(ri);
#endif
			}
		
			iSize += StringHelper.SizeOfStr(f.link);
			iSize += StringHelper.SizeOfStr(f.title);
			iSize += StringHelper.SizeOfStr(fi.Link);
			iSize += StringHelper.SizeOfStr(fi.Title);
			iSize += StringHelper.SizeOfStr(fi.Description);

			_log.Info(iSize.ToString() + "\t byte(s) contained in feed\t" + f.link);

			if(!StringHelper.EmptyOrNull(maxItemAge)){
				f.maxitemage = maxItemAge; 
			}
			
			//update last retrieved date on feed only if the item was not cached.
			if (!cachedStream) {
				f.lastretrieved = new DateTime(DateTime.Now.Ticks);
				f.lastretrievedSpecified = true;
			}
			
			//any new items in feed? 
			if(readItems == items.Count){
				f.containsNewMessages = false; 
			}else{
				f.containsNewMessages = true; 
			}
		
			return fi; 

		}

		/// <summary>
		/// This method fills a particular NameTable with the element names from RSS and
		/// ATOM that RSS Bandit checks for. After filling the name table it returns an 
		/// array containing the atomized strings.
		/// </summary>
		/// <param name="nt">The name table to fill</param>
		/// <returns>An array containing the atomized strings added to the name table</returns>
		private static object[] FillNameTable(XmlNameTable nt){
		
			//TODO: Investigate whether we can use a singleton XmlNameTable object

			/* For examples of the perf improvements from using name tables see 
			 * http://blogs.msdn.com/mfussell/archive/2004/04/28/121854.aspx
			 * http://www.tkachenko.com/blog/archives/000181.html 
			 */

			object[] atomized_names = new object[NT_SIZE]; 

			atomized_names[RssParser.nt_author] = nt.Add("author");
			atomized_names[RssParser.nt_body] = nt.Add("body"); 
			atomized_names[RssParser.nt_category] = nt.Add("category");
			atomized_names[RssParser.nt_channel] = nt.Add("channel");
			atomized_names[RssParser.nt_comments] = nt.Add("comments"); 
			atomized_names[RssParser.nt_content] = nt.Add("content");
			atomized_names[RssParser.nt_creator] = nt.Add("creator");
			atomized_names[RssParser.nt_date] = nt.Add("date"); 
			atomized_names[RssParser.nt_description] = nt.Add("description");
			atomized_names[RssParser.nt_encoded] = nt.Add("encoded");
			atomized_names[RssParser.nt_entry] = nt.Add("entry");
			atomized_names[RssParser.nt_flagstatus] = nt.Add("flag-status");
			atomized_names[RssParser.nt_feed] = nt.Add("feed");
			atomized_names[RssParser.nt_guid] = nt.Add("guid"); 
			atomized_names[RssParser.nt_href] = nt.Add("href");
			atomized_names[RssParser.nt_id] = nt.Add("id"); 
			atomized_names[RssParser.nt_image] = nt.Add("image");
			atomized_names[RssParser.nt_ispermalink] = nt.Add("isPermaLink"); 
			atomized_names[RssParser.nt_issued] = nt.Add("issued");
			atomized_names[RssParser.nt_item] = nt.Add("item");
			atomized_names[RssParser.nt_items] = nt.Add("items");
			atomized_names[RssParser.nt_lastbuilddate] = nt.Add("lastBuildDate");
			atomized_names[RssParser.nt_link] = nt.Add("link"); 
			atomized_names[RssParser.nt_maxitemage] = nt.Add("maxItemAge");
			atomized_names[RssParser.nt_modified] = nt.Add("modified");
			atomized_names[RssParser.nt_name] = nt.Add("name");
			atomized_names[RssParser.nt_pubdate] = nt.Add("pubDate"); 
			atomized_names[RssParser.nt_rdf] = nt.Add("RDF");
			atomized_names[RssParser.nt_reference] = nt.Add("reference");
			atomized_names[RssParser.nt_rel] = nt.Add("rel");
			atomized_names[RssParser.nt_rss] = nt.Add("rss");
			atomized_names[RssParser.nt_subject] = nt.Add("subject"); 
			atomized_names[RssParser.nt_summary] = nt.Add("summary");
			atomized_names[RssParser.nt_tagline] = nt.Add("tagline");
			atomized_names[RssParser.nt_title] = nt.Add("title");
			atomized_names[RssParser.nt_type] = nt.Add("type"); 
			atomized_names[RssParser.nt_ns_dc] = nt.Add("http://purl.org/dc/elements/1.1/");
			atomized_names[RssParser.nt_ns_xhtml] = nt.Add("http://www.w3.org/1999/xhtml");
			atomized_names[RssParser.nt_ns_content] = nt.Add("http://purl.org/rss/1.0/modules/content/"); 
			atomized_names[RssParser.nt_ns_annotate] = nt.Add("http://purl.org/rss/1.0/modules/annotate/");
			atomized_names[RssParser.nt_ns_bandit_2003] = nt.Add("http://www.25hoursaday.com/2003/RSSBandit/feeds/");
			atomized_names[RssParser.nt_ns_slash] = nt.Add("http://purl.org/rss/1.0/modules/slash/");
			
			return atomized_names;

		}


		/// <summary>
		/// Reads the RSS feed from the stream then caches and returns the feed items 
		/// in an array list.
		/// </summary>
		/// <remarks>If the feedUrl is currently not stored in this object's internal table 
		///	then it is added/</remarks>		
		/// <param name="f">Information about the feed. This information is updated based
		/// on the results of processing the feed. </param>
		/// <param name="feedStream">A stream containing an RSS feed.</param>				
		/// <param name="cachedStream">Flag states update last retrieved date on feed only 
		/// if the item was not cached. Indicates whether the lastretrieved date is updated
		/// on the feedsFeed object passed in. </param>
		/// <returns>A FeedInfo object which represents the feed</returns>
		/// <exception cref="ApplicationException">If the RSS feed is not 
		/// version 0.91, 1.0 or 2.0</exception>
		/// <exception cref="XmlException">If an error occured parsing the 
		/// RSS feed</exception>	
		//	[MethodImpl(MethodImplOptions.Synchronized)]
		public static FeedInfo GetItemsForFeed(feedsFeed f, Stream  feedStream, bool cachedStream) {

			//Handle entities (added due to blogs which reference Netscape RSS 0.91 DTD)			
			XmlTextReader r  = new XmlTextReader(feedStream); 
			r.WhitespaceHandling = WhitespaceHandling.Significant;
			XmlValidatingReader vr = new XmlValidatingReader(r);
			vr.ValidationType = ValidationType.None;	
			vr.XmlResolver    = new ProxyXmlUrlResolver(RssParser.GlobalProxy); 

			return RssParser.GetItemsForFeed(f, vr, cachedStream); 																	
		}


		/// <summary>
		/// Figures out how to process an ATOM content construct which is an element 
		/// that may have a specified MIME type and escaping mode. 
		/// </summary>
		/// <param name="element">The element whose string contents are being extracted</param>
		/// <param name="onNextElement">indicates whether the XML reader is positioned on the end element of 
		/// the content or on next element</param>
		/// <returns>The string content of the element after relevant escaping has been done</returns>
		private static string GetContentFromAtomElement(XmlReader element, ref bool onNextElement){
			
			string typeAttr = element.GetAttribute("type"), modeAttr = element.GetAttribute("mode"); 
			string type = (typeAttr == null ? "text/plain" : typeAttr.ToLower());
			string mode = (modeAttr == null ? "xml" : modeAttr.ToLower());			

			element.MoveToElement(); //move back to content element

			if((type.IndexOf("text")!= -1) || (type.IndexOf("html")!= -1)){

				if(mode.Equals("xml")){ 
					onNextElement = true; 
					return element.ReadInnerXml();
				}else if(mode.Equals("escaped")){ 
					//BUG: if the element contains any child elements then the XmlReader will be in the wrong position					
					onNextElement = false; 
					return ReadElementString(element); 
				}			
			}
			
			//an unsupported mode or MIME type
			element.Skip(); 
			return String.Empty; 		
		}		

		/// <summary>
		/// Returns the MIME type of an ATOM content node
		/// </summary>
		/// <param name="element">The ATOM element</param>
		/// <returns>The MIME type of the node's content</returns>
		private static ContentType GetMimeTypeOfAtomElement(XmlReader element){
		
			string mimetype; 

			if(element["type"]!= null){
				mimetype = element["type"].ToLower(); 
				if(mimetype.IndexOf("xhtml")!= -1){
					return ContentType.Xhtml; 
				}else if(mimetype.IndexOf("html")!= -1){
					return ContentType.Html; 
				}else if(mimetype.IndexOf("text")!= -1){
					return ContentType.Text;
				}else{
					return ContentType.Unknown;
				}

			} else {
				return ContentType.Text; 
			}

		}	
		
			
		/// <summary>
		/// This is a helper method to get around the fact that XmlReader.ReadString and 
		/// XmlReader.ReadElementString don't work in the face of nested markup. 
		/// </summary>
		/// <remarks>It is assumed that the XmlReader is positioned within the element</remarks>
		/// <param name="reader">The input XmlReader positioned within the element</param>
		/// <returns>The string content of the element</returns>
		private static string ReadElementString(XmlReader reader){
			string result = reader.ReadString(); 
			
			while(reader.NodeType != XmlNodeType.EndElement){
				reader.Skip(); 
				result += reader.ReadString();
			}
				
			return result; 
		}
	

		/// <summary>
		///  Processes the channel level elements of the feed. 
		/// </summary>
		/// <param name="f">The object which contains information about the feed</param>
		/// <param name="reader">The XML reader positioned within the feed. It should be positioned on the channel element.</param>
		/// <param name="atomized_strings">An object array containing the common element names that will be tested for in the feed. The objects in 
		/// the array are from the XmlReader's nametable</param>
		/// <param name="rssNamespaceUri"></param>
		/// <param name="format">The syndication format of the feed</param>
		/// <param name="feedDescription">Description of the feed</param>
		/// <param name="feedLink">The link to the homepage syndicated in the feed</param>
		/// <param name="feedTitle">The title of the homepage syndicated by the feed</param>
		/// <param name="maxItemAge">Maximum amount of time to keep items</param>
		/// <param name="channelBuildDate">The value of the lastBuildDate for the feed</param>
		/// <param name="optionalElements">Any other optional elements in the feed such as images</param>
		/// <param name="items">Items in the feed</param>
		/// <param name="defaultItemDate">Default DateTime for the items</param>
		private static void ProcessFeedElements(feedsFeed f, XmlReader reader, object[] atomized_strings, string rssNamespaceUri, SyndicationFormat format, ref string feedLink, ref string feedTitle, ref string feedDescription, ref string maxItemAge, ref DateTime channelBuildDate, Hashtable optionalElements, ArrayList items, DateTime defaultItemDate ){	  		 
			
			bool matched = false; //indicates whether this is a known element
			bool nodeRead = false; //indicates whether the last node was read using XmlDocument.ReadNode()

			if((format == SyndicationFormat.Rdf) || (format == SyndicationFormat.Rss)){		  		

				while((nodeRead || reader.Read()) && reader.NodeType != XmlNodeType.EndElement){

					object localname = reader.LocalName; 
					object namespaceuri = reader.NamespaceURI;
					matched = false; 
					nodeRead = false; 

					if(reader.NodeType!= XmlNodeType.Element){ continue; }

					if(reader.NamespaceURI.Equals(rssNamespaceUri) || reader.NamespaceURI.Equals(String.Empty)){
				
						if(localname == atomized_strings[RssParser.nt_title]){
							if(!reader.IsEmptyElement){
								feedTitle = ReadElementString(reader); 
							}						
							matched = true; 
						}else if(localname == atomized_strings[RssParser.nt_description]){
							if(!reader.IsEmptyElement){
								feedDescription = ReadElementString(reader); 
							}
							matched = true; 
						}else if(localname == atomized_strings[RssParser.nt_link]){
							if(!reader.IsEmptyElement){
								feedLink = ReadElementString(reader); 
							}
							matched = true; 
						}else if(localname == atomized_strings[RssParser.nt_lastbuilddate]){
							try {
								if(!reader.IsEmptyElement){
									channelBuildDate = DateTimeExt.Parse(ReadElementString(reader));
								}
							} catch (FormatException fex) {
								_log.Warn("Error parsing date from channel {" + feedTitle + 
									"} from feed {" + (feedLink == null ? f.title : feedLink) + "}: ", fex);
							}finally{
								matched = true; 
							}
						}else if(localname == atomized_strings[RssParser.nt_items]){
							reader.Skip(); 
							matched = true; 
						}else if((localname == atomized_strings[RssParser.nt_image]) && format == SyndicationFormat.Rdf){
							reader.Skip(); 
							matched = true; 
						}else if(localname == atomized_strings[RssParser.nt_item]){
							if(!reader.IsEmptyElement){
								NewsItem rssItem = MakeRssItem(f,reader,  atomized_strings, defaultItemDate);
								if(rssItem!= null){
									items.Add(rssItem);
								}
							}  
							matched = true; 
						}
			
					}else if(namespaceuri == atomized_strings[RssParser.nt_ns_bandit_2003]){
						if(localname == atomized_strings[RssParser.nt_maxitemage]){
							if(!reader.IsEmptyElement){
								// get the old v1.2 value from cached feed
								// We used the TimeSpan.Parse() / maxItemAge.ToString() there, so we cannot simply take over the string.
								// Instead we convert to TimeSpan, then convert to valid xs:duration datatype to proceed correctly
								f.maxitemage = XmlConvert.ToString(TimeSpan.Parse(ReadElementString(reader))); 
								//f.maxitemage = ReadElementString(reader); 
							}
							matched = true; 
						}
					}

					if(!matched){

						XmlQualifiedName qname = new XmlQualifiedName(reader.LocalName, reader.NamespaceURI); 
						XmlNode optionalNode   = optionalElementsDoc.ReadNode(reader); 

						if(!optionalElements.Contains(qname)){				
							optionalElements.Add(qname, optionalNode); 
						}

						nodeRead = true; 			
					}//if(!matched)

				}//while

			  
				if(format == SyndicationFormat.Rdf){

					reader.ReadEndElement(); //move to <image> or first <item>. 					

					do{
						object localname = reader.LocalName;
						nodeRead = false;  

						if((localname == atomized_strings[RssParser.nt_image]) &&
							reader.NamespaceURI.Equals(rssNamespaceUri) ){ //RSS 1.0 can have <image> outside <channel>
							XmlNode optionalNode   = optionalElementsDoc.ReadNode(reader); 
							((XmlElement)optionalNode).SetAttribute("xmlns", String.Empty); //change namespace decl to no namespace
							
							XmlQualifiedName qname = new XmlQualifiedName(optionalNode.LocalName, optionalNode.NamespaceURI); 														

							if(!optionalElements.Contains(qname)){				
								optionalElements.Add(qname, optionalNode); 
							}
							
							nodeRead = true; 
						}

						if((localname == atomized_strings[RssParser.nt_item]) && 
							reader.NamespaceURI.Equals(rssNamespaceUri)){
							if(!reader.IsEmptyElement){
								NewsItem rssItem = MakeRssItem(f, reader,  atomized_strings, defaultItemDate); 
								if(rssItem!= null){
									items.Add(rssItem);
								}
							}					  
						}

					}while(nodeRead || reader.Read()); 

				}// if(format == SyndicationFormat.Rdf)

			}else if(format == SyndicationFormat.Atom){
		  
				while((nodeRead || reader.Read()) && reader.NodeType != XmlNodeType.EndElement){

					object localname = reader.LocalName; 
					object namespaceuri = reader.NamespaceURI;
					matched = false; 
					nodeRead = false;

					if(reader.NodeType!= XmlNodeType.Element){ continue; }

					 
					if(reader.NamespaceURI.Equals(rssNamespaceUri) || reader.NamespaceURI.Equals(String.Empty)){
				
						if(localname == atomized_strings[RssParser.nt_title]){
							if(!reader.IsEmptyElement){
								feedTitle = ReadElementString(reader); 
							}						
							matched = true; 
						}else if(localname == atomized_strings[RssParser.nt_tagline]){
							if(!reader.IsEmptyElement){
								feedDescription = ReadElementString(reader); 
							}
							matched = true; 
						}else if(localname == atomized_strings[RssParser.nt_link]){

							string rel  = reader.GetAttribute("rel"); 
							string href = reader.GetAttribute("href"); 

							if(feedLink == String.Empty){
								if((rel != null) && (href != null) &&  
									rel.Equals("alternate")){
									feedLink = href; 
									matched = true; 
								}
							}					
												
						}else if(localname == atomized_strings[RssParser.nt_modified]){
							try {
								if(!reader.IsEmptyElement){
									channelBuildDate = DateTimeExt.Parse(ReadElementString(reader));
								}
							} catch (FormatException fex) {
								_log.Warn("Error parsing date from channel {" + feedTitle + 
									"} from feed {" + (feedLink == null ? f.title : feedLink) + "}: ", fex);
							}finally{
								matched = true; 
							}					
						}else if(localname == atomized_strings[RssParser.nt_entry]){
							if(!reader.IsEmptyElement){
								NewsItem atomItem = MakeAtomItem(f,reader,  atomized_strings, defaultItemDate);
								if(atomItem!= null){
									items.Add(atomItem);
								}
							}  
							matched = true; 
						}
			
					}else if(namespaceuri == atomized_strings[RssParser.nt_ns_bandit_2003]){
						if(localname == atomized_strings[RssParser.nt_maxitemage]){
							if(!reader.IsEmptyElement){
								// get the old v1.2 value from cached feed
								// We used the TimeSpan.Parse() / maxItemAge.ToString() there, so we cannot simply take over the string.
								// Instead we convert to TimeSpan, then convert to valid xs:duration datatype to proceed correctly
								TimeSpan maxItemAgeTS = TimeSpan.Parse(ReadElementString(reader)); 
								
								if(maxItemAgeTS != TimeSpan.MaxValue){
									f.maxitemage = XmlConvert.ToString(maxItemAgeTS); 
								}								
							}
							matched = true; 
						}
					}

					if(!matched){

						XmlQualifiedName qname = new XmlQualifiedName(reader.LocalName, reader.NamespaceURI); 
						XmlNode optionalNode   = optionalElementsDoc.ReadNode(reader); 

						if(!optionalElements.Contains(qname)){				
							optionalElements.Add(qname, optionalNode); 
						}

						nodeRead = true; 
			
					}//if(!matched)

				}//while
				
			}		  
	  
		}

		
	
		/// <summary>
		/// Returns the number of pending async. requests in the queue.
		/// </summary>
		/// <returns></returns>
		public int AsyncRequestsPending() {
			return AsyncWebRequest.PendingRequests;
		}

	  

		/// <summary>
		/// Posts a comment to a website using the CommentAPI specification described at 
		/// http://wellformedweb.org/story/9 
		/// </summary>
		/// <param name="url">The URL to post the comment to</param>
		/// <param name="item2post">An RSS item that will be posted to the website</param>
		/// <param name="inReply2item">An RSS item that is the post parent</param>
		/// <returns>The HTTP Status code returned</returns>
		/// <exception cref="WebException">If an error occurs when the POSTing the 
		/// comment</exception>
		public HttpStatusCode PostCommentViaCommentAPI(string url, NewsItem item2post, NewsItem inReply2item, ICredentials credentials){			  

			string comment = item2post.ToString(NewsItemSerializationFormat.RssItem);
			Encoding enc = Encoding.UTF8, unicode = Encoding.Unicode;
			byte[] encBytes = Encoding.Convert(unicode, enc, unicode.GetBytes(comment)); //enc.GetBytes(comment); enough ???
		
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
			request.Timeout          = 1 * 60 * 1000; //one minute timeout 
			request.UserAgent        = owner.FullUserAgent; 
			request.Proxy            = owner.Proxy;
			if (credentials == null)
				credentials = CredentialCache.DefaultCredentials;
			request.Credentials = credentials;
			request.Method = "POST";
			request.Headers.Add("charset", "UTF-8");	// see http://asg.web.cmu.edu/rfc/rfc2616.html and http://www.iana.org/assignments/character-sets
			request.ContentType = "text/xml";
			request.ContentLength = encBytes.Length; 

			_log.Info("PostCommentViaCommentAPI() post item content: "+ comment); 
		
			Stream myWriter = null; 
			try{ 
				myWriter = request.GetRequestStream();
				myWriter.Write(encBytes, 0, encBytes.Length); 

			} catch(Exception e){
			
				throw new WebException(e.Message, e); 
			}finally{
				if(myWriter != null){
					myWriter.Close(); 	
				}
			}

			 
			HttpWebResponse response = (HttpWebResponse) request.GetResponse(); 
			return response.StatusCode; 			

		}

	}//RssParser

	#region ProxyXmlUrlResolver
	/// <summary>
	/// Helper class for resolving DTDs in feeds when connecting through a proxy 
	/// </summary>
	class ProxyXmlUrlResolver : System.Xml.XmlUrlResolver {

		/// <summary>
		/// Initializes the ProxyXmlUrlResolver with the specified proxy settings.
		/// </summary>
		/// <param name="proxy">The proxy connection used when resolving feeds</param>
		public ProxyXmlUrlResolver(IWebProxy proxy):base(){
			this.proxy = proxy; 
		}

		/// <summary>
		/// The proxy used when connecting. 
		/// </summary>
		private IWebProxy proxy;


		/// <summary>
		/// Maps a URI to an object containing the actual resource.
		/// </summary>
		/// <param name="absoluteUri">The URI to fetch the entity from</param>
		/// <param name="role">The current implementation does not use this parameter when resolving URIs. This is provided for future extensibility purposes. For example, this can be mapped to the 
		///  xlink:role and used as an implementation specific argument in other scenarios. </param>
		/// <param name="ofObjectToReturn">The type of object to return. The current implementation only returns System.IO.Stream objects.</param>
		/// <returns>A stream containing the requested entity</returns>
		public override object GetEntity(Uri absoluteUri, string role,
			Type ofObjectToReturn) {
			WebRequest req = WebRequest.Create(absoluteUri);
			req.Proxy = this.proxy;
			//TODO: how about GetResponse() errors?
			return req.GetResponse().GetResponseStream();
		}
	}
	#endregion

}
