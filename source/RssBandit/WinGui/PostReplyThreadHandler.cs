#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Threading;
using System.IO;
using System.Net;

using Logger = RssBandit.Common.Logging;
using AppExceptions = Microsoft.ApplicationBlocks.ExceptionManagement;

using NewsComponents;
using NewsComponents.Feed;

namespace RssBandit.WinGui {
	/// <summary>
	/// Summary description for PostReplyThreadHandler.
	/// </summary>
	internal class PostReplyThreadHandler: EntertainmentThreadHandlerBase
	{
		
		private PostReplyThreadHandler() {	}
		/// <summary>
		/// Initializes a new instance of the <see cref="PostReplyThreadHandler"/> class.
		/// </summary>
		/// <param name="feedHandler">The feed handler.</param>
		/// <param name="commentApiUri">The comment API URI.</param>
		/// <param name="item2post">The item2post.</param>
		/// <param name="inReply2item">The in reply2item.</param>
		public PostReplyThreadHandler(NewsHandler feedHandler, string commentApiUri, NewsItem item2post, NewsItem inReply2item) {
			this.feedHandler = feedHandler;
			this.commentApiUri = commentApiUri;
			this.item2post = item2post;
			this.inReply2item = inReply2item;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="PostReplyThreadHandler"/> class.
		/// </summary>
		/// <param name="feedHandler">The feed handler.</param>
		/// <param name="item2post">The item2post.</param>
		/// <param name="postTarget">The post target.</param>
		public PostReplyThreadHandler(NewsHandler feedHandler, NewsItem item2post, INewsFeed postTarget) {
			this.feedHandler = feedHandler;
			this.item2post = item2post;
			this.postTarget = postTarget;
		}

		private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(PostReplyThreadHandler));
		private NewsHandler feedHandler = null;
		private string commentApiUri = null;
		private NewsItem item2post = null, inReply2item = null;
		private INewsFeed postTarget = null;

		/// <summary>
		/// Gets or sets the comment API URI.
		/// </summary>
		/// <value>The comment API URI.</value>
		public string CommentApiUri {
			get {	return this.commentApiUri;	}
			set {	this.commentApiUri = value;	}
		}

		/// <summary>
		/// Gets or sets the item to post.
		/// </summary>
		/// <value>The item to post.</value>
		public NewsItem ItemToPost {
			get {	return this.item2post;	}
			set {	this.item2post = value;	}
		}

		/// <summary>
		/// Gets or sets the post target.
		/// </summary>
		/// <value>The post target.</value>
		public INewsFeed PostTarget {
			get {	return this.postTarget ;	}
			set {	this.postTarget = value;	}
		}

		protected override void Run() {

			try {				
				if (this.postTarget != null)
					this.feedHandler.PostComment(item2post, postTarget) ;
				else
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