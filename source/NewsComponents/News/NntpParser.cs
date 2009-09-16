#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion


using System; 
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Net;
using NewsComponents.Storage;
using NewsComponents.Feed;
using System.Diagnostics;
using Org.Mime4Net.Mime.Field;
using Org.Mime4Net.Mime.Message;

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
		public static void PostCommentViaNntp(INewsItem item2post, INewsItem inReply2item, ICredentials credentials){			  
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
		public static void PostCommentViaNntp(INewsItem item2post, INewsFeed postTarget, ICredentials credentials){			  
							
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
        /// on the results of processing the feed.</param>
        /// <param name="newsgroupListStream">A stream containing an nntp news group list.</param>
        /// <param name="response">The response.</param>
        /// <param name="cacheDataService">The cache data service to store embedded binary content.</param>
        /// <param name="cachedStream">Flag states update last retrieved date on feed only
        /// if the item was not cached. Indicates whether the lastretrieved date is updated
        /// on the NewsFeed object passed in.</param>
        /// <returns>
        /// A FeedInfo containing the NewsItem objects
        /// </returns>
        internal static FeedInfo GetItemsForNewsGroup(INewsFeed f, Stream newsgroupListStream, WebResponse response, IUserCacheDataService cacheDataService, bool cachedStream) 
        {
           
			int readItems = 0;
            List<INewsItem> items = new List<INewsItem>(); 
			NewsItem item;

            StringBuilder content = new StringBuilder(); 
#if DEBUG
			// just to have the source for the item to build to track down issues:
			StringBuilder itemSource = new StringBuilder(); 
#endif
            NntpWebResponse nntpResponse = (NntpWebResponse)response;
			
			FeedInfo fi = new FeedInfo(f.id, f.cacheurl, items, f.title, f.link, String.Empty);

            try
            {
                foreach (MimeMessage msg in nntpResponse.Articles)
                {
                    string parentId;
                    string id;
                    string author = parentId = id = null;
                    DateTime pubDate = DateTime.UtcNow;
                    
                    content.Length = 0;

                    string title;
                    if (msg.Subject != null)
                        title = EscapeXML(msg.Subject.Value);
                    else
                        title = "";

                    UnstructuredField fld = msg.Header.GetField(MimeField.MessageID) as UnstructuredField;
                    if (fld != null)
                        id = fld.Value;

                    MailboxListField mfld = msg.Header.GetField(MimeField.From) as MailboxListField;
                    if (mfld != null && mfld.MailboxList.Count > 0)
                        author = mfld.MailboxList[0].AddressString;

                    fld = msg.Header.GetField(MimeField.References) as UnstructuredField;
                    if (fld != null)
                    {
                        // returns the hierarchy path: the last one is our real parent:
                        string[] singleRefs = fld.Value.Split(' ');
                        if (singleRefs.Length > 0)
                            parentId = CreateGoogleUrlFromID(singleRefs[singleRefs.Length - 1]);
                    }
                    DateTimeField dfld = msg.Header.GetField(MimeField.Date) as DateTimeField;
                    if (dfld != null)
                        pubDate = dfld.DateValue;

                    ITextBody txtBody = msg.Body as ITextBody;
                    if (txtBody != null)
                    {
                        content.Append(txtBody.Reader.ReadToEnd());

                        content = NntpClient.DecodeBody(content,
                            (fileName, bytes) =>
                            {
                                string name = PrepareEmbeddedFileUrl(fileName, id, nntpResponse.ResponseUri);
                                // we replace the uuencoded/yencoded binary content with a clickable link:
                                if (IsImage(fileName))
                                    return String.Format("<img src='{1}' alt='{0}'></img>", fileName,
                                        cacheDataService.SaveBinaryContent(name, bytes).AbsoluteUri);
                                return String.Format("<a href='{1}'>{0}</a>", fileName,
                                    cacheDataService.SaveBinaryContent(name, bytes).AbsoluteUri);
                            },
                            line =>
                            {
                                // escape HTML/XML special chars: 
                                return line.Replace("<", "&lt;").Replace("]]>", "]]&gt;");
                            });

                        content = content.Replace(Environment.NewLine, "<br>");
                    }

                    if (id != null)
                    {
                        item = new NewsItem(f, title, CreateGoogleUrlFromID(id), content.ToString(), author, pubDate, id, parentId);
                        item.FeedDetails = fi;
                        item.CommentStyle = SupportedCommentStyle.NNTP;
                        item.Enclosures = Collections.GetList<IEnclosure>.Empty;
                        items.Add(item);
                        FeedSource.ReceivingNewsChannelServices.ProcessItem(item);
                    }
                    else
                    {
#if DEBUG
                        _log.Warn("No message-id header found for item:\r\n" + itemSource.ToString());
#else
						_log.Warn("No message-id header found for item." );
#endif
                    }

                }


                //update last retrieved date on feed only if the item was not cached.)
                if (!cachedStream)
                {
                    f.lastretrieved = new DateTime(DateTime.Now.Ticks);
                    f.lastretrievedSpecified = true;
                }

                //any new items in feed? 
                if ((items.Count == 0) || (readItems == items.Count))
                {
                    f.containsNewMessages = false;
                }
                else
                {
                    f.containsNewMessages = true;
                }

                FeedSource.ReceivingNewsChannelServices.ProcessItem(fi);
                FeedSource.RelationCosmosAddRange(items);
                fi.itemsList.AddRange(items);

            }
            catch (Exception e)
            {
                _log.Error("Retriving NNTP articles from " + nntpResponse.ResponseUri + " caused an exception", e);
            }
            

            return fi;
        }
        
        private static bool IsImage(string name)
        {
            if (name != null)
            {
                name = name.ToLower();
                if (name.Length > 3)
                {
                    name = name.Substring(name.Length - 4);
                    if (name.Equals(".gif") ||
                        name.Equals(".jpg") ||
                        name.Equals(".bmp") ||
                        name.Equals(".wmf") ||
                        name.Equals(".png") ||
                        name.Equals(".ico") )
                        return true;
                }
            }
            return false;
        }

        private static string PrepareEmbeddedFileUrl(string name, string postId, Uri uri)
        {
            string path;
            postId = postId ?? "";
            
            if (uri.IsFile || uri.IsUnc)
            {
                path = String.Format("{0}.{1}.{2}", uri.GetHashCode() , postId.GetHashCode(), name);
            }
            else
            {
                path = String.Format("{0}.{1}.{2}.{3}.{4}.{5}", uri.Host, uri.Port, uri.GetHashCode(), uri.PathAndQuery.Replace("/", "-"), postId.GetHashCode(), name);
            }
            return path.Replace("-", "");
        }
        

        private static string EscapeXML(String s)
        {
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
            
        }

        /// <summary>
        /// Creates the google URL from ID.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        internal static string CreateGoogleUrlFromID(string id)
        {
            return GoogleGroupsUrl + id.Substring(1, id.Length - 2).Replace("#", "%23");
        }

        //private static Regex BQHeaderEncodingRegex = new Regex(@"=\?([^?]+)\?([^?]+)\?([^?]+)\?=" , RegexOptions.Compiled);
		
        ///// <summary>
        ///// Decodes a string according to RFC 2047
        ///// </summary>
        ///// <param name="line">The string to decode</param>
        ///// <returns>The decoded string</returns>
        //public static string HeaderDecode( string line ) {
			
        //    Match m = BQHeaderEncodingRegex.Match(line);
        //    if (m.Success) {
        //        StringBuilder ms = new StringBuilder(line);
        //        while ( m.Success ) {
        //            string oStr = m.Groups[0].ToString();
        //            string encoding = m.Groups[1].ToString();
        //            string method = m.Groups[2].ToString();
        //            string content = m.Groups[3].ToString();
					
        //            if (method == "b" || method== "B")
        //                content = Base64Decode(encoding, content);
        //            else if (method == "q" || method== "Q")
        //                content = QDecode(encoding, content);
					
        //            ms.Replace(oStr, content);
        //            m = m.NextMatch();
        //        }
        //        return ms.ToString();
        //    }
        //    return line;
        //}

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
  
        ///// <summary>
        ///// Decodes a string according to the "Q" encoding rules of RFC 2047
        ///// </summary>
        ///// <param name="encoding">The string's encoding</param>
        ///// <param name="text">The string to decode</param>
        ///// <returns>The decoded string</returns>
        //public static string QDecode(string encoding, string text){

        //    StringBuilder decoded = new StringBuilder(); 
        //    Encoding decoder = Encoding.GetEncoding(encoding);  

        //    for(int i =0; i < text.Length; i++){

        //        if(text[i] == '='){

        //            string current = String.Empty + text[i + 1] + text[i + 2];
        //            byte theByte = Byte.Parse(current, NumberStyles.HexNumber);     
        //            byte[] bytes = new byte[]{theByte};
	
        //            decoded.Append(decoder.GetString(bytes));
	
        //            i+=2; 
        //        }else if(text[i] == '_'){
	
        //            byte theByte = Byte.Parse("20", NumberStyles.HexNumber);     
        //            byte[] bytes = new byte[]{theByte};
	
        //            decoded.Append(decoder.GetString(bytes));      	 
	
        //        }else{
        //            decoded.Append(text[i]);     
        //        }
      
        //    }//for 

        //    return decoded.ToString(); 
        //}

        ///// <summary>
        ///// Decodes a string according to the "B" encoding rules of RFC 2047
        ///// </summary>
        ///// <param name="encoding">The string's encoding</param>
        ///// <param name="text">The string to decode</param>
        ///// <returns>The decoded string</returns>
        //public static string Base64Decode(string encoding, string text){

        //    Encoding decoder = Encoding.GetEncoding(encoding);   
        //    byte[] textAsByteArray = Convert.FromBase64String(text);
    
        //    return decoder.GetString(textAsByteArray);     
        //}


        ///// <summary>
        ///// Gets the indexes of encoded words within the string. 
        ///// </summary>
        ///// <param name="str">The target string</param>
        ///// <returns>An array list containing pairs of start and end indexes of encoded
        ///// words within the string</returns>
        //public static ArrayList GetEncodedWordIndexes(string str){

        //    ArrayList list = new ArrayList(); 
        //    int begin = -1; 
    
        //    for(int i =0; i < str.Length; i++){

        //        if((i != 0) && (str[i] == '?') && ((i + 1) != str.Length)){
        //            if((begin == -1) && (str[i-1]== '=')){
        //                begin = i -1;
        //            }else if(str[i+1]== '='){
        //                i++; 
        //                list.Add(new int[]{begin, i});	
        //                begin = -1; 
        //            }
        //        }//if((i!=0)...)
        //    }

        //    return list; 

        //}

	
		
	}

}
