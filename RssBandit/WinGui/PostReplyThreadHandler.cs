#region CVS Version Header
/*
 * $Id: PostReplyThreadHandler.cs,v 1.11 2005/05/26 18:48:09 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2005/05/26 18:48:09 $
 * $Revision: 1.11 $
 */
#endregion

using System;
using System.Threading;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Windows.Forms;

using Logger = RssBandit.Common.Logging;
using AppExceptions = Microsoft.ApplicationBlocks.ExceptionManagement;

using NewsComponents;
using NewsComponents.Feed;

namespace RssBandit.WinGui {
	/// <summary>
	/// Summary description for PostReplyThreadHandler.
	/// </summary>
	public class PostReplyThreadHandler: EntertainmentThreadHandlerBase
	{
		
		private PostReplyThreadHandler() {	}
		public PostReplyThreadHandler(NewsHandler feedHandler, string commentApiUri, NewsItem item2post, NewsItem inReply2item) {
			this.feedHandler = feedHandler;
			this.commentApiUri = commentApiUri;
			this.item2post = item2post;
			this.inReply2item = inReply2item;
		}

		private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(PostReplyThreadHandler));
		private NewsHandler feedHandler = null;
		private string commentApiUri = null;
		private NewsItem item2post = null, inReply2item = null;

		public string CommentApiUri {
			get {	return this.commentApiUri;	}
			set {	this.commentApiUri = value;	}
		}

		public NewsItem ItemToPost {
			get {	return this.item2post;	}
			set {	this.item2post = value;	}
		}

		protected override void Run() {

			try {				

				this.feedHandler.PostComment(commentApiUri, item2post, inReply2item); 

			} catch (ThreadAbortException) {
				// eat up
			} catch(WebException we) {

				//Open file for writing 
				System.Text.StringBuilder sb = new System.Text.StringBuilder(); 
				StringWriter writeStream = null; 
				StreamReader readStream = null; 

				if(we.Response != null) { 
					writeStream = new StringWriter(sb); 

					//Retrieve input stream from response and specify encoding 
					Stream receiveStream     = we.Response.GetResponseStream();
					System.Text.Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

					// Pipes the stream to a higher level stream reader with the required encoding format. 
					readStream = new StreamReader( receiveStream, encode );

					Char[] read = new Char[256]; 
    
					// Reads 256 characters at a time.    
					int count = readStream.Read( read, 0, 256 );


					while (count > 0) {
      
						// Dumps the 256 characters on a string and displays the string to the console.
						writeStream.Write(read, 0, count);
						count = readStream.Read(read, 0, 256);
      
					}   

					p_operationException = new WebException(sb.ToString(), we, we.Status, we.Response);
					// dump to log file/trace
					_log.Error(@"Error while posting a comment" , p_operationException);
					AppExceptions.ExceptionManager.Publish(p_operationException);

					p_operationException = we;

					if(writeStream != null){ writeStream.Close(); }
					if (readStream != null) { readStream.Close(); }

				} else {

					p_operationException = we;

				}
			}
			catch (Exception ex) {
				p_operationException = ex;
			}
			finally {
				WorkDone.Set();
			}
		}

	}
}