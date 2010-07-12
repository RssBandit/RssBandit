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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using NewsComponents.Resources;
using Org.Mime4Net.Mime.Message;

namespace NewsComponents.News {

	/// <summary>
	/// Provides an NNTP-specific implementation of the WebRequest class. This class is 
	/// NOT thread safe. 
	/// </summary>
	[ComVisible(false)]
	public class NntpWebRequest: WebRequest, IWebRequestCreate, IDisposable{

		#region public consts
		
		/// <summary>
		/// Allowed/known NNTP Uri Scheme
		/// </summary>
		public static string NntpUriScheme = "nntp";
		/// <summary>
		/// Allowed/known NNTP Uri Scheme
		/// </summary>
		public static string NntpsUriScheme = "nntps";
		/// <summary>
		/// Allowed/known NNTP Uri Scheme
		/// </summary>
		public static string NewsUriScheme = "news";
		/// <summary>
		/// Default Nntp server port.
		/// </summary>
		public static int NntpDefaultServerPort = 119;

		#endregion

		#region private fields 

		/// <summary>
		/// Hashtable used to store delegates between BeginXXX and EndXXX calls.
		/// </summary>
		private Hashtable delegateTable = new Hashtable();

		

		/// <summary>
		/// The length of time before the request times out. 
		/// </summary>
		private int timeout;

		
		/// <summary>
		/// The protocol method to use in a request
		/// </summary>
		private string method = "NEWNEWS";

		/// <summary>
		/// The number of articles the NntpWebRequest should fetch from the server on a 
		/// NEWNEWS request. 
		/// </summary>
		private int downloadCount = 500; 

		/// <summary>
		/// The credentials associated with this request.
		/// </summary>
		private ICredentials credentials;

		/// <summary>
		/// The URI of the request.
		/// </summary>
		private readonly Uri requestUri;

		/// <summary>
		/// The proxy to use
		/// </summary>
		private IWebProxy proxy;

		/// <summary>
		/// Stream for writing output to the server.  
		/// </summary>
		private MemoryStream requestStream = new MemoryStream(); 


		/// <summary>
		/// Used to specify the age of messages that should be retrieved. Default value is 
		/// one year. 
		/// </summary>
		private DateTime ifModifiedSince  = DateTime.Now - new TimeSpan(365, 0, 0 ,0);

		#endregion 


		#region Constructors 

		/// <summary>
		/// Must alkways specify a URI when creating an NNTP web request
		/// </summary>
		private NntpWebRequest(){;}


	/// <summary>
	/// Creates an NntpWebRequest with the specified request URI
	/// </summary>
	/// <param name="requestUri">The request URI</param>
		public NntpWebRequest(Uri requestUri){
			this.requestUri = requestUri; 
		}

		#endregion		

		#region delegates 

		/// <summary>
		/// Delegate for calling GetRequestStream(). 
		/// </summary>
		private delegate Stream GetRequestStreamDelegate();

		/// <summary>
		/// Delegate for calling GetResponse().
		/// </summary>
		private delegate WebResponse GetResponseDelegate(bool asyncRequest);

		#endregion 

		#region public properties

		/// <summary>
		/// Gets or sets the protocol method to use in this request. The valid values for 
		/// this property are POST, NEWNEWS and LIST. 
		/// </summary>
		/// <remarks>The following are the results of the various NNTP methods 
		///          POST - posts a message to a newsgroup
		///          LIST - lists all the newsgroups available on the server
		///          NEWNEWS - gets recent posts to the newsgroup
		/// </remarks>
		public override string Method {
			get {
				return method;
			}
			set {
				method = value.ToUpper();
			}
		}
		

		/// <summary>
		/// Used to inidcate the minimum age of messages that should be returned by the 
		/// response to this request. 
		/// </summary>
		public DateTime IfModifiedSince{
			get { return ifModifiedSince; }
			set { ifModifiedSince = value; }		
		}

		/// <summary>
		/// The number of articles the NntpWebRequest should fetch from the server on a 
		/// NEWNEWS request. This property is only used if the news server doesn't understand
		/// how to process NEWNEWS requests that specify a date. 
		/// </summary>
		public int DownloadCount {
			get {
				return downloadCount;
			}
			set {
				if (value<=0) {
					throw new ArgumentOutOfRangeException("value");
				}
				downloadCount = value;				
			}
		}

		/// <summary>
		/// Gets or sets the credentials associated with this request.
		/// </summary>
		public override ICredentials Credentials {
			get {
				return credentials;
			}
			set {
				credentials = value;
			}
		}

	/// <summary>
	/// The URI of the request. It should either be a 'news' or 'nntp' URI. Examples include
	/// news://news.microsoft.com/microsoft.public.xml and 
	/// nntp://news.microsoft.com/microsoft.public.test.here
	/// </summary>
		public override Uri RequestUri {
			get {
				return this.requestUri;
			}
		}



		/// <summary>
		/// Gets or sets the length of time, in milliseconds, before the request times out.
		/// </summary>
		/// <remarks>The Timeout property indicates the length of time, in milliseconds, 
		/// until the request times out and throws a WebException. The Timeout property 
		/// affects only synchronous requests made with the GetResponse method. To time 
		/// out asynchronous requests, use the Abort method.</remarks>
		public override int Timeout {
			get {
				return timeout; 
			}
			set {
				if (value< 0) {
					throw new ArgumentOutOfRangeException("value");
				}
				timeout = value;
			}
		}

		/// <summary>
		/// When overridden in a descendant class, gets or sets the network proxy to use to access this Internet resource.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The <see cref="T:System.Net.IWebProxy"/> to use to access the Internet resource.
		/// </returns>
		/// <exception cref="T:System.NotImplementedException">
		/// Any attempt is made to get or set the property, when the property is not overridden in a descendant class.
		/// </exception>
		/// <PermissionSet>
		/// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/>
		/// </PermissionSet>
		public override IWebProxy Proxy
		{
			get
			{
				return proxy;
			}
			set
			{
				proxy = value;
			}
		}
		#endregion 

		#region private properties

		
		 /// <summary>
		 ///This read-only propery does a test against the object to verify that
		 ///we can send data. We can only send data with a POST everything else is disallowed.
		 ///</summary>		 		         
		private bool CanGetRequestStream {
			get {
				if(this.method.Equals("POST")){
					return true;
				}
				return false; 
			}
		}
 
        
		/// <summary>
		///This read-only propery does a test against the object to verify that
		///we can receive data as opposed to just a status code. Only LIST and 
		///GROUP requests send back a result stream. 
		///</summary>	
         private bool CanGetResponseStream {
             get {
				 if(this.method.Equals("POST")){
					 return false;
				 }
				 return true; 
             }
         }


		#endregion 

		#region private methods 

		/// <summary>
		/// Connects to the server and returns the connected NntpClient
		/// </summary>
		/// <returns>the connected NntpClient</returns>
		private NntpClient Connect(){
		
			NntpClient client = new NntpClient(requestUri.Host, requestUri.Port);		
			
			
			if((this.credentials != null)){

				NetworkCredential nc = this.credentials as NetworkCredential; 

				if(nc == null){
					throw new NntpWebException("Credentials property not an instance of NetworkCredential");
				}
				
				bool authOK;
				
				if(string.IsNullOrEmpty(nc.Domain)){
					authOK = client.AuthInfo(nc.UserName, nc.Password); 
				}else{
					authOK = client.AuthInfo(nc.Domain + "\\" + nc.UserName, nc.Password); 
				}
				
				if (!authOK)
					throw new NntpWebException(String.Format(ComponentsText.ExceptionNntpServerAuthenticationFailed, requestUri.Host));
			}

			if (proxy != null)
			{
				//TODO: to be impl. in the NntpClient
			}

			return client; 
		
		}

		#endregion 

		#region public methods 


		/// <summary>
		/// Close memory stream and open connections
		/// </summary>
		public void Dispose(){
			try{
				requestStream.Close(); 
			}catch(Exception){}
			
			requestStream = null; 
		}

		/// <summary>
		/// Provides an asynchronous version of the GetResponse method.
		/// </summary>
		/// <param name="callback">The AsyncCallback delegate</param>
		/// <param name="state">An object containing state information for this asynchronous request</param>
		/// <returns>An IAsyncResult that references the asynchronous request</returns>
		
		public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state) {
						
			GetResponseDelegate getResponse = GetResponse;

			// The return call.  Store in the hashtable.
			IAsyncResult asyncResult = getResponse.BeginInvoke(true, callback, state);
			this.delegateTable[asyncResult] = getResponse;

			// Begin the result.
			return asyncResult;

		}

		/// <summary>
		///  Returns the NntpWebResponse 
		/// </summary>
		/// <param name="asyncResult">An IAsyncResult that references a pending request for a response</param>
		/// <returns>the NNTP response</returns>
		public override WebResponse EndGetResponse(IAsyncResult asyncResult) {
			// Get the item from the hashtable.  If it doesn't exist, then raise an
			// exception. 
				
			GetResponseDelegate getResponse = 
				(GetResponseDelegate) this.delegateTable[asyncResult]; 

			if(getResponse == null){
				throw new NntpWebException("GetRequestStreamDelegate for " 
					+ this.requestUri + 
					"not found in delegates table of NntpWebRequest");
			}

			// Remove the item from the hashtable.
			this.delegateTable.Remove(asyncResult);

			// Finish off the call.
			return getResponse.EndInvoke(asyncResult);
		}


		/// <summary>
		/// Provides an asynchronous version of the GetRequestStream method.
		/// </summary>
		/// <param name="callback">The AsyncCallback delegate</param>
		/// <param name="state">An object containing state information for this asynchronous request</param>
		/// <returns>An IAsyncResult that references the asynchronous request</returns>
		public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state) {
			
			GetRequestStreamDelegate getRequestStream = GetRequestStream;

			// The return call.  Store in the hashtable.
			IAsyncResult asyncResult = getRequestStream.BeginInvoke(callback, state);			
			this.delegateTable[asyncResult] = getRequestStream;

			// Begin the result.
			return asyncResult;
			
		}


		/// <summary>
		/// Returns a Stream for writing data to the NNTP server.
		/// </summary>
		/// <param name="asyncResult">An IAsyncResult that references a pending request for a stream. </param>
		/// <returns>A Stream to write data to.</returns>
		public override Stream EndGetRequestStream(IAsyncResult asyncResult) {
			
			// Get the item from the hashtable.  If it doesn't exist, then raise an
			// exception. 
				
			GetRequestStreamDelegate getRequestStream = 
					(GetRequestStreamDelegate) this.delegateTable[asyncResult]; 

			if(getRequestStream == null){
				throw new NntpWebException("GetRequestStreamDelegate for " 
										+ this.requestUri + 
										"not found in delegates table of NntpWebRequest");
			}

			// Remove the item from the hashtable.
			this.delegateTable.Remove(asyncResult);

			// Finish off the call.
			return getRequestStream.EndInvoke(asyncResult);

		}


		/// <summary>
		/// Used to create an NntpWebRequest class for agiven 'news' or 'nntp' URI
		/// </summary>
		/// <param name="uri">The 'news' or 'nntp' URI</param>
		/// <returns>An NntpWebRequest</returns>
		public new WebRequest Create(Uri uri){
		

			if((!uri.Scheme.Equals(NntpUriScheme)) && (!uri.Scheme.Equals(NewsUriScheme))){
				throw new NotSupportedException(); 
			}
			//Console.WriteLine(uri.AbsolutePath.Substring(1)); 
			return new NntpWebRequest(uri); 
		}


		/// <summary>
		/// Used to create an NntpWebRequest class for agiven 'news' or 'nntp' URI
		/// </summary>
		/// <param name="uri">The 'news' or 'nntp' URI</param>
		/// <returns>An NntpWebRequest</returns>
		public new WebRequest Create(string uri){	
			return this.Create(new Uri(uri)); 
		}


		/// <summary>
		/// Returns a Stream for writing data to the Internet resource. 
		/// </summary>
		/// <remarks> Requesting the request stream causes a request to be sent to the server.
		///  The reason for doing this is so the user writes directly to the network stream.
		///  An alternate implementation could involve creating a memory stream and then copying
		///  that to the network stream but that is inefficient. </remarks>
		/// <returns>A Stream for writing data to the Internet resource</returns>
		public override Stream GetRequestStream(){

			if (!CanGetRequestStream){
				throw new ProtocolViolationException("Attempt to upload message when the method is not POST");
			}
			
             return this.requestStream;             													

		}


		/// <summary>
		/// Returns the response to the NNTP request
		/// </summary>
		/// <returns>The NNTP response</returns>
		public override WebResponse GetResponse() {
			return this.GetResponse(false);
		}


		/// <summary>
		/// Returns the response to the NNTP request
		/// </summary>
		/// <param name="asyncRequest">Indicates whether this method is being called as part of
		/// an async request. This tells us whether to use the Timeout value or not.</param>
		/// <returns>The NNTP response</returns>
		private WebResponse GetResponse(bool asyncRequest) {

			NntpWebResponse response; 
			MemoryStream newsgroupListStream;
			
			//The stream is not closed on purpose in this method
			StreamWriter sw; 

			using(NntpClient client = this.Connect()){

				if(!asyncRequest){
					client.Timeout = this.Timeout; 
				}

				try{ 

					switch(this.method){
					
						case "POST":
							requestStream.Position = 0; 						
							client.Post(new UTF8Encoding().GetString(this.requestStream.ToArray())); 						
							response = new NntpWebResponse(NntpStatusCode.OK, RequestUri);
							break;
					
						case "LIST":
							newsgroupListStream = new MemoryStream();
							sw  = new StreamWriter(newsgroupListStream); 
							client.Groups(sw); 
							sw.Flush();
							newsgroupListStream.Position = 0;
                            response = new NntpWebResponse(NntpStatusCode.OK, newsgroupListStream, RequestUri);
							break;
					
						case "NEWNEWS":
							client.SelectGroup(requestUri.PathAndQuery.Substring(1)); 
							IList<MimeMessage> msgs = client.GetNntpMessages(ifModifiedSince, downloadCount);
                            response = new NntpWebResponse(NntpStatusCode.OK, Stream.Null, RequestUri);
					        response.Articles = msgs;
							break;

						default: 
							throw new NotSupportedException(method); 
					}
				
				}catch(Exception){
					client.Dispose(); 
					throw;
				}
			
			}
			
			return response;
		}

		/// <summary>
		/// Aborts the Request
		/// </summary>
		/// <exception cref="T:System.NotImplementedException">Any attempt is made to access the method, when the method is not overridden in a descendant class. </exception>
		/// <PermissionSet>
		/// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/>
		/// </PermissionSet>
		public override void Abort()
		{
			//TODO: what to do here?
			// Now it is just there to prevent the first chance exceptions caused
			// by calls to the base implementation.
		}


		#endregion 
/*
		[STAThread]
		static void Main(string[] args)
		{
			

            WebRequest.Create("news://news.microsoft.com/microsoft.public.xml"); 
			//WebRequest.Create("nntp://news.microsoft.com/microsoft.public.xml"); 
			
            string server = "news.microsoft.com"; // change this to your NNTP server
            string group = "microsoft.public.test.here"; // change this to your desired group
            string username = null; // Fill these in with your account information, if nescessary
            string password = null;
            Console.WriteLine("Connecting to server {0}...", server);
			NntpClient client = new NntpClient(server);
            if(username != null && password != null)
            {
                Console.WriteLine("Authorizing as user {0}...", username);
                client.AuthInfo(username, password);
            }

			client.Post(group, "Beg For Mercy", "GUnit Fan", "The mixtape kings");

			
			Console.WriteLine("Printing available newsgroups"); 
			foreach(string s in client.Groups()){
				Console.WriteLine(s); 
			}
            Console.WriteLine("Selecting newsgroup {0}...", group);
            client.SelectGroup(group);
            int first = client.FirstMsg;
            int last = Math.Min(first + 75, client.LastMsg);    // For demo purposes, just take the first 75 messages
            Console.WriteLine("Downloading message headers {0} to {1}...", first, last);
            NntpMessages messages = client.GetNntpMessages(first, last,true);
            SortedList multiFiles = new SortedList();
            Console.WriteLine("Figuring out multipart messages...");
            client.GroupFiles(messages, multiFiles);
            Console.WriteLine("Downloading binaries...");
            for(int i = 0; i < multiFiles.Count; i++)
            {
                NntpMessage[] multipartNntpMessage = (NntpMessage[]) multiFiles.GetByIndex(i);
                Console.WriteLine("{0} part message: {1}", multipartNntpMessage.Length, multiFiles.GetKey(i));
                client.DecodeMultiPart(multipartNntpMessage, new NntpClient.StreamCreator(MyStreamCreator));
            }
            client.Dispose();
            Console.WriteLine("Done.");
			
			Console.ReadLine(); 
		}
		*/
        static void MyStreamCreator(string fileName, out Stream stream)
        {
            if( File.Exists(fileName))
            {
                Console.WriteLine("{0} already exists.", fileName);
                stream = null;
            }
            else
            {
                Console.WriteLine("Downloading {0}", fileName);
                stream = File.Create(fileName);
            }
        }
	}
}
