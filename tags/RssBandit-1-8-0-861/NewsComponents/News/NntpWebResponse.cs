using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

namespace NewsComponents.News {

	/// <summary>
	/// Represents the status of an NNTP request
	/// </summary>
	public enum NntpStatusCode {
	/// <summary>
	/// Indicates that the operation was successful
	/// </summary>
	OK, 
	/// <summary>
	/// Indicates that an error occured while trying to satisfy the NNTP request
	/// </summary>
	Error
	}

	/// <summary>
	/// Provides an NNTP-specific implementation of the WebRequest class.
	/// </summary>
	[ComVisible(false)]
	public class NntpWebResponse: WebResponse, IDisposable{


		#region private fields 


	/// <summary>
	/// The status of the request
	/// </summary>
	private NntpStatusCode statusCode; 

	/// <summary>
	/// The length of the content returned by the request
	/// </summary>
       private long contentLength = 0; 

		/// <summary>
		/// Gets the content type of the response.
		/// </summary>
		private string contentType = null; 

		/// <summary>
		/// The collection of header name/value pairs associated with the request
		/// </summary>
		private WebHeaderCollection headers = null; 

		/// <summary>
		/// The URI of the response.
		/// </summary>
		private Uri responseUri = null; 
				

		/// <summary>
		/// Stream for reading response from the server.  
		/// </summary>
		private Stream responseStream = null; 

		#endregion 


		#region Constructors 

		/// <summary>
		/// Prevent creating an NntpWebResponse without a status code
		/// </summary>
		private NntpWebResponse(){;}

	/// <summary>
	/// Creates an NntpWebResponse with the given status code
	/// </summary>
	/// <param name="status">The status code of the response</param>
		internal NntpWebResponse(NntpStatusCode status){this.statusCode = status;}

		/// <summary>
		/// Creates an NntpWebResponse with the given status code and sets the response stream
		/// </summary>
		/// <param name="status">The status code of the response</param>
		/// <param name="stream">the response stream</param>
		internal NntpWebResponse(NntpStatusCode status, Stream stream):this(status){
		  this.responseStream = stream; 
		}

		#endregion


		#region public properties

		/// <summary>
		/// The status of the NNTP response from the server
		/// </summary>
		public NntpStatusCode StatusCode {
			get{ return statusCode; }
		}


        /// <summary>
        /// Gets the collection of header name/value pairs associated with the request
        /// </summary>
		public override WebHeaderCollection Headers {
			get {
				return this.headers;
			}
		}


		/// <summary>
		/// Gets the content type of the response.
		/// </summary>
		public override string ContentType {
			get {
				return this.contentType;
			}			
		}
	
		/// <summary>
		/// Gets the length of the content returned by the request.
		/// </summary>
		public override long ContentLength {
			get {
				return this.contentLength;
			}		
		}


/// <summary>
/// The URI of the request.
/// </summary>
		public override Uri ResponseUri {
			get {
				return this.responseUri;
			}
		}

		#endregion 

	

		#region public methods 

		///<summary>
		/// Close memory stream and open connections
		/// </summary>
		public void Dispose(){
			try{
				responseStream.Close(); 
			}catch(Exception){}
			
			responseStream = null; 
		}


		/// <summary>
		/// Returns the response stream sent back by the server
		/// </summary>
		/// <returns>The response stream</returns>
		public override Stream GetResponseStream() {
			return this.responseStream;
		}

		/// <summary>
		/// Closes the response stream
		/// </summary>
		public override void Close() {
			this.responseStream.Close();
		}


		#endregion 

	
	}
}
