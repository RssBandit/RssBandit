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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Net;
using System.Globalization;
using System.Text.RegularExpressions;
using NewsComponents.Utils;
using NewsComponents.Feed;
using NewsComponents.RelationCosmos;

namespace NewsComponents.News{




	/// <summary>
	/// Class for parsing NNTP results sent by a server 
	/// </summary>
	public sealed class NntpParser{

		private static readonly log4net.ILog _log = RssBandit.Common.Logging.Log.GetLogger(typeof(NntpParser));
		/// <summary>
		/// Links to Google Groups used for creating permalinks for NNTP items
		/// </summary>
		private static string GoogleGroupsUrl = "http://www.google.com/groups?selm="; 


		/// <summary>
		/// Posts a comment to a newsgroup using NNTP 
		/// </summary>
		/// <param name="item2post">An NNTP item that will be posted to the website</param>
		/// <param name="inReply2item">An NNTP item that is the post parent</param>
		/// <param name="credentials">Credentials to use for post</param>
		/// <returns>The NNTP Status code returned</returns>
		/// <exception cref="WebException">If an error occurs when the POSTing the 
		/// comment</exception>
		public static void PostCommentViaNntp(NewsItem item2post, NewsItem inReply2item, ICredentials credentials){			  
			PostCommentViaNntp(item2post, inReply2item.Feed, credentials);
		}

		/// <summary>
		/// Posts a comment to a newsgroup using NNTP 
		/// </summary>
		/// <param name="item2post">An NNTP item that will be posted to the website</param>
		/// <param name="postTarget">An feed that is the post target</param>
		/// <param name="credentials">Credentials to use</param>
		/// <returns>The NNTP Status code returned</returns>
		/// <exception cref="WebException">If an error occurs when the POSTing the 
		/// comment</exception>
		public static void PostCommentViaNntp(NewsItem item2post, NewsFeed postTarget, ICredentials credentials){			  
							
			string comment = item2post.ToString(NewsItemSerializationFormat.NntpMessage);
			Encoding enc = Encoding.UTF8, unicode = Encoding.Unicode;
			byte[] encBytes = Encoding.Convert(unicode, enc, unicode.GetBytes(comment)); //enc.GetBytes(comment); enough ???
		

			NntpWebRequest request = (NntpWebRequest) WebRequest.Create(postTarget.link); 
			request.Method = "POST"; 

			if (credentials != null)
				request.Credentials = credentials;

			Stream myWriter = null; 

			try{ 

				myWriter = request.GetRequestStream();
				myWriter.Write(encBytes, 0, encBytes.Length); 

				request.GetResponse(); 

			} catch(Exception e){
			
				throw new WebException(e.Message, e); 
			}finally{
				if(myWriter != null){
					myWriter.Close(); 	
				}
			}
						

		}


		/// <summary>
		/// Parses the results of an NNTP LIST request into an ArrayList of newsgroup names.
		/// </summary>
		/// <returns>An arraylist containing a list of newsgroups</returns>
		public static StringCollection GetNewsgroupList(Stream newsgroupListStream){
		
			StringCollection col = new StringCollection();
			string newsgroup;
			int len; 

			TextReader reader    = new StreamReader(newsgroupListStream); 
			//skip NNTP status code
			reader.ReadLine();

			// Read and display lines from the file until the end of 
			// the file is reached.
			while ( ((newsgroup = reader.ReadLine()) != null) && 
				(newsgroup.StartsWith(".")!= true) && (newsgroup.Length > 0)){
				
				// see also http://www.mibsoftware.com/userkt/nntp/0023.htm 
				// Each newsgroup is sent as a line of text in the following format:
				//
				//		group last first p
				//
				// where <group> is the name of the newsgroup, <last> is the number of
				// the last known article currently in that newsgroup, <first> is the
				// number of the first article currently in the newsgroup, and <p> is
				// either 'y' or 'n' indicating whether posting to this newsgroup is
				// allowed ('y') or prohibited ('n').
				//
				// The <first> and <last> fields will always be numeric.  They may have
				// leading zeros.  If the <last> field evaluates to less than the
				// <first> field, there are no articles currently on file in the newsgroup.

				len = newsgroup.IndexOf(" "); 
				len = (len == -1 ? newsgroup.Length : len);
				col.Add(newsgroup.Substring(0, len)); 
			}		

			reader.Close(); 
		
			return col; 
		}


		/// <summary>
		/// Reads the list of articles from the stream and returns the feed item.
		/// </summary>	
		/// <param name="f">Information about the feed. This information is updated based
		/// on the results of processing the feed. </param>
		/// <param name="newsgroupListStream">A stream containing an nntp news group list.</param>				
		/// <param name="cachedStream">Flag states update last retrieved date on feed only 
		/// if the item was not cached. Indicates whether the lastretrieved date is updated
		/// on the feedsFeed object passed in. </param>
		/// <returns>A FeedInfo containing the NewsItem objects</returns>		
		public static FeedInfo GetItemsForNewsGroup(NewsFeed f, Stream newsgroupListStream, bool cachedStream) {
			
			int readItems = 0;
            List<NewsItem> items = new List<NewsItem>(); 
			NewsItem item; 
			string currentLine, title, author, parentId, headerName, headerValue, id; 
			StringBuilder content = new StringBuilder(); 
#if DEBUG
			// just to have the source for the item to build to track down issues:
			StringBuilder itemSource = new StringBuilder(); 
#endif
			DateTime pubDate; 
			int colonPos; 
			TextReader reader    = new StreamReader(newsgroupListStream); 	
			FeedInfo fi = new FeedInfo(f.id, f.cacheurl, items, f.title, f.link, String.Empty);
			
			try{

				while(reader.ReadLine()!= null){ //skip NNTP status code
			
					/* headers loop */				
					title = author = parentId = id = null; 
					pubDate = DateTime.Now;
					content.Remove(0, content.Length); 
#if DEBUG
					itemSource.Remove(0, itemSource.Length);
#endif


					while(((currentLine = reader.ReadLine()) != null) && 
						(currentLine.Trim().Length > 0)){ //TODO: Get rid of Trim() call								
#if DEBUG
						itemSource.Append(currentLine);
#endif
						colonPos = currentLine.IndexOf(":");

						if(colonPos > 0){
							headerName  = currentLine.Substring(0, colonPos).ToLower();
							headerValue = currentLine.Substring(colonPos + 2); //skip whitespace after colon

							switch(headerName){
								case "subject" : 
									title = HeaderDecode(headerValue); 
									break; 
								case "from": 
									author = HeaderDecode(headerValue);
									break; 
								case "references":	//some posts may have multiple ids in References header
									int spaceIndex = headerValue.LastIndexOf(" "); 
									spaceIndex = ((spaceIndex != - 1) && (spaceIndex + 1 < headerValue.Length) ? spaceIndex : -1); //avoid IndexOutOfBoundsException
									parentId = (spaceIndex == -1 ? headerValue : headerValue.Substring(spaceIndex + 1)); 
									break;
								case "date": 
									pubDate = DateTimeExt.Parse(headerValue); 
									break; 
								case "message-id":
									id = headerValue;
									break; 
								default: 
									break;//ignore other NNTP headers
							}

						}//if(colonPos > 0}					

					}//

					/* content loop */
				
					while(((currentLine = reader.ReadLine()) != null) && 
						(currentLine.Equals(".")!= true)){					
#if DEBUG
						itemSource.Append(currentLine);
#endif
						content.Append(currentLine.Replace("<", "&lt;").Replace("]]>", "]]&gt;")); 
						content.Append("<br>");
					}

					if (id != null) {
						item = new NewsItem(f, title, CreateGoogleUrlFromID(id), content.ToString(), author, pubDate, id,  parentId); 					
						item.FeedDetails = fi;
						item.CommentStyle = SupportedCommentStyle.NNTP;
                        item.Enclosures = NewsComponents.Collections.GetList<Enclosure>.Empty; 
						items.Add(item); 
						NewsHandler.ReceivingNewsChannelServices.ProcessItem(item);
					} else {
#if DEBUG
						_log.Warn("No message-id header found for item:\r\n" + itemSource.ToString() );
#else
						_log.Warn("No message-id header found for item." );
#endif
					}
					/* reader.ReadLine(); //read blank line after the item */ 
				}


				//update last retrieved date on feed only if the item was not cached.
				if (!cachedStream) {
					f.lastretrieved = new DateTime(DateTime.Now.Ticks);
					f.lastretrievedSpecified = true;
				}
			
				//any new items in feed? 
				if((items.Count== 0) || (readItems == items.Count)){
					f.containsNewMessages = false; 
				}else{
					f.containsNewMessages = true; 
				}

				NewsHandler.ReceivingNewsChannelServices.ProcessItem(fi);
                NewsHandler.RelationCosmosAddRange(items); 
                fi.itemsList.AddRange(items); 
 
			}
			catch(Exception e)
			{
				System.Diagnostics.Trace.WriteLine(e); 
			} finally {
				reader.Close();
			}

			
			return fi; 
		}
	
		private static Regex BQHeaderEncodingRegex = new Regex(@"=\?([^?]+)\?([^?]+)\?([^?]+)\?=" , RegexOptions.Compiled);
		
		/// <summary>
		/// Decodes a string according to RFC 2047
		/// </summary>
		/// <param name="line">The string to decode</param>
		/// <returns>The decoded string</returns>
		public static string HeaderDecode( string line ) {
			
			Match m = BQHeaderEncodingRegex.Match(line);
			if (m.Success) {
				StringBuilder ms = new StringBuilder(line);
				while ( m.Success ) {
					string oStr = m.Groups[0].ToString();
					string encoding = m.Groups[1].ToString();
					string method = m.Groups[2].ToString();
					string content = m.Groups[3].ToString();
					
					if (method == "b" || method== "B")
						content = Base64Decode(encoding, content);
					else if (method == "q" || method== "Q")
						content = QDecode(encoding, content);
					
					ms.Replace(oStr, content);
					m = m.NextMatch();
				}
				return ms.ToString();
			}
			return line;
		}

		#region	cause IndexOutOfRange failures...
//		/// <summary>
//		/// Decodes a string according to RFC 2047
//		/// </summary>
//		/// <param name="original">The string to decode</param>
//		/// <returns>The decoded string</returns>
//		public static string Decode(string original){
//		
//
//			try{
//
//				if(original.IndexOf("=?")== -1){
//					return original; 
//				} 
//				
//				string toReplace; 
//				StringBuilder str = new StringBuilder(original); 
//				ArrayList indexes = GetEncodedWordIndexes(original); 
//
//				foreach(int[] x in indexes){
//					toReplace = original.Substring(x[0], x[1] - x[0] + 1); 
//					str.Replace(toReplace, DecodeString(toReplace));
//				}
//				toReplace = HeaderDecode(original);
//				return str.ToString(); 
//
//			}catch(Exception e) {
//				Log.Error("NntpParser.Decode() failed", e);
//			} // ignore errors caused by malformed strings
//
//			return original; 
//		}
//  
//
//		
//		/// <summary>
//		/// Helper method for the decode method. Decodes a string according to RFC 2047
//		/// </summary>
//		/// <param name="str">The string to decode</param>
//		/// <returns>The decoded string</returns>
//		private static string DecodeString(string str){
//
//			string[] tokens = str.Split(new char[]{'?'}); 
//
//			if(tokens.Length!= 5){
//				return str; 
//			}
//
//			string charset = tokens[1], encoding = tokens[2], text = tokens[3]; 
//
//			if(encoding.ToLower().Equals("b")){
//				return Base64Decode(charset, text); 
//			}else if(encoding.ToLower().Equals("q")){
//				return QDecode(charset, text); 
//			}
//			
//			return str; 
//		}
		#endregion
  
		/// <summary>
		/// Decodes a string according to the "Q" encoding rules of RFC 2047
		/// </summary>
		/// <param name="encoding">The string's encoding</param>
		/// <param name="text">The string to decode</param>
		/// <returns>The decoded string</returns>
		public static string QDecode(string encoding, string text){

			StringBuilder decoded = new StringBuilder(); 
			Encoding decoder = Encoding.GetEncoding(encoding);  

			for(int i =0; i < text.Length; i++){

				if(text[i] == '='){

					string current = String.Empty + text[i + 1] + text[i + 2];
					byte theByte = Byte.Parse(current, NumberStyles.HexNumber);     
					byte[] bytes = new byte[]{theByte};
	
					decoded.Append(decoder.GetString(bytes));
	
					i+=2; 
				}else if(text[i] == '_'){
	
					byte theByte = Byte.Parse("20", NumberStyles.HexNumber);     
					byte[] bytes = new byte[]{theByte};
	
					decoded.Append(decoder.GetString(bytes));      	 
	
				}else{
					decoded.Append(text[i]);     
				}
      
			}//for 

			return decoded.ToString(); 
		}

		/// <summary>
		/// Decodes a string according to the "B" encoding rules of RFC 2047
		/// </summary>
		/// <param name="encoding">The string's encoding</param>
		/// <param name="text">The string to decode</param>
		/// <returns>The decoded string</returns>
		public static string Base64Decode(string encoding, string text){

			Encoding decoder = Encoding.GetEncoding(encoding);   
			byte[] textAsByteArray = Convert.FromBase64String(text);
    
			return decoder.GetString(textAsByteArray);     
		}


		/// <summary>
		/// Gets the indexes of encoded words within the string. 
		/// </summary>
		/// <param name="str">The target string</param>
		/// <returns>An array list containing pairs of start and end indexes of encoded
		/// words within the string</returns>
		public static ArrayList GetEncodedWordIndexes(string str){

			ArrayList list = new ArrayList(); 
			int begin = -1; 
    
			for(int i =0; i < str.Length; i++){

				if((i != 0) && (str[i] == '?') && ((i + 1) != str.Length)){
					if((begin == -1) && (str[i-1]== '=')){
						begin = i -1;
					}else if(str[i+1]== '='){
						i++; 
						list.Add(new int[]{begin, i});	
						begin = -1; 
					}
				}//if((i!=0)...)
			}

			return list; 

		}

	
		/// <summary>
		/// Creates the google URL from ID.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		internal static string CreateGoogleUrlFromID(string id) {
			return GoogleGroupsUrl + id.Substring(1, id.Length - 2).Replace("#","%23");
		}
	}

}

#region CVS Version Log
/*
 * $Log: NntpParser.cs,v $
 * Revision 1.24  2006/10/19 19:49:05  t_rendelmann
 * fixed: title/author encoding failures (e.g. in german news groups)
 *
 * Revision 1.23  2006/08/18 19:10:57  t_rendelmann
 * added an "id" XML attribute to the feedsFeed. We need it to make the feed items (feeditem.id + feed.id) unique to enable progressive indexing (lucene)
 *
 */
#endregion
