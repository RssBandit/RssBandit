using System; 
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Net;

using NewsComponents.Utils;
using NewsComponents.Feed; 

namespace NewsComponents.News{




	/// <summary>
	/// Class for parsing NNTP results sent by a server 
	/// </summary>
	public sealed class NntpParser{

		/// <summary>
		/// Links to Google Groups used for creating permalinks for NNTP items
		/// </summary>
		private static string GoogleGroupsUrl = "http://www.google.com/groups?selm="; 


		/// <summary>
		/// Posts a comment to a newsgroup using NNTP 
		/// </summary>
		/// <param name="url">The newsgroup to post the comment to</param>
		/// <param name="item2post">An NNTP item that will be posted to the website</param>
		/// <param name="inReply2item">An NNTP item that is the post parent</param>
		/// <returns>The NNTP Status code returned</returns>
		/// <exception cref="WebException">If an error occurs when the POSTing the 
		/// comment</exception>
		public static void PostCommentViaNntp(NewsItem item2post, NewsItem inReply2item, ICredentials credentials){			  
							
			string comment = item2post.ToString(NewsItemSerializationFormat.NntpMessage);
			Encoding enc = Encoding.UTF8, unicode = Encoding.Unicode;
			byte[] encBytes = Encoding.Convert(unicode, enc, unicode.GetBytes(comment)); //enc.GetBytes(comment); enough ???
		

			NntpWebRequest request = (NntpWebRequest) WebRequest.Create(inReply2item.Feed.link); 
			request.Method = "POST"; 			

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
		public static FeedInfo GetItemsForNewsGroup(feedsFeed f, Stream newsgroupListStream, bool cachedStream) {
			
				int readItems = 0; 
				ArrayList items = new ArrayList(); 
				NewsItem item; 
				string currentLine, title, author, parentId, headerName, headerValue, id; 
				StringBuilder content = new StringBuilder(); 
				DateTime pubDate; 
				int colonPos; 
				TextReader reader    = new StreamReader(newsgroupListStream); 	
				FeedInfo fi = new FeedInfo(f.cacheurl, items, f.title, f.link, String.Empty);
			
			try{

				while(reader.ReadLine()!= null){ //skip NNTP status code
			
					/* headers loop */				
					title = author = parentId = id = null; 
					pubDate = DateTime.Now;
					content.Remove(0, content.Length); 


					while(((currentLine = reader.ReadLine()) != null) && 
						(currentLine.Trim().Length > 0)){ //TODO: Get rid of Trim() call								
						
						colonPos = currentLine.IndexOf(":");

						if(colonPos > 0){
							headerName  = currentLine.Substring(0, colonPos).ToLower();
							headerValue = currentLine.Substring(colonPos + 2); //skip whitespace after colon

							switch(headerName){
								case "subject" : 
									title = headerValue; 
									break; 
								case "from": 
									author = headerValue;
									break; 
								case "references":	//some posts may have multiple ids in References header
									int spaceIndex = headerValue.IndexOf(" "); 
									parentId = (spaceIndex == -1 ? headerValue : headerValue.Substring(0, spaceIndex)); 
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
						content.Append(currentLine.Replace("<", "&lt;").Replace("]]>", "]]&gt;")); 
						content.Append("<br>");
					}

					item = new NewsItem(f, title, GoogleGroupsUrl + id.Substring(1, id.Length - 2).Replace("#","%23"), content.ToString(), author, pubDate, id,  parentId); 					
					item.FeedDetails = fi;
					item.CommentStyle = SupportedCommentStyle.NNTP; 
					items.Add(item); 

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

#if NIGHTCRAWLER 
				NewsChannelServices.ProcessItem(fi);
#endif
				NewsHandler.RelationCosmosAddRange(items);
 
			}
			catch(Exception e)
			{
				System.Diagnostics.Trace.WriteLine(e); 
			} finally {
				reader.Close();
			}

			
			return fi; 
		}
	
	
	
	}


}