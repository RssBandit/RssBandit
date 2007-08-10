#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Net;
using System.IO;

namespace NewsComponents.Net
{
	/// <summary>
	/// Summary description for RequestState.
	/// </summary>
	internal class RequestState
	{
		public event RequestQueuedCallback WebRequestQueued;
		public event RequestStartCallback WebRequestStarted;
		public event RequestCompleteCallback WebRequestCompleted;
		public event RequestExceptionCallback WebRequestException;
		public event RequestProgressCallback WebRequestProgress; 

		public RequestState(AsyncWebRequest asyncWebRequest)
		{
			this.myAsyncWebRequest = asyncWebRequest;
			this.Request = null;
			this.ResponseStream = null;
			this.RetryCount = 0;
			this.BufferRead = new byte[BUFFER_SIZE];
			#region experimental code
//			this.allDone= new System.Threading.AutoResetEvent(false);
			#endregion
		}

		public void OnRequestQueued() {
			this.OnRequestQueued(this.RequestUri);
		}
		public void OnRequestQueued(Uri requestUri) {
			try {
				if (WebRequestQueued != null)
					WebRequestQueued(requestUri, this.Priority);
			} catch {}
		}

		public bool OnRequestStart() {
			return this.OnRequestStart(this.RequestUri);
		}
		public bool OnRequestStart(Uri requestUri) {
			bool cancel = false;
			try {
				if (WebRequestStarted != null)
					WebRequestStarted(requestUri, ref cancel);	
			} catch {}
			return cancel;
		}

		public void OnRequestException(Exception e) {
			this.OnRequestException(this.RequestUri, e);
		}
		public void OnRequestException(Uri requestUri, Exception e) {
			myAsyncWebRequest.FinalizeWebRequest(this);
			try {
				if (this.WebRequestException != null) {
					this.WebRequestException(requestUri, e, this.Priority);
				}
			} catch { /* ignore ex. thrown in callback */ }
 		}

		public void OnRequestCompleted(string eTag, DateTime lastModfied, RequestResult result) {
			this.OnRequestCompleted(this.InitialRequestUri, this.RequestUri, eTag, lastModfied, result);
		}
		public void OnRequestCompleted(Uri newUri, string eTag, DateTime lastModfied, RequestResult result) {
			this.OnRequestCompleted(this.InitialRequestUri, newUri, eTag, lastModfied, result);
		}
		public void OnRequestCompleted(Uri requestUri,  Uri newUri, string eTag, DateTime lastModfied, RequestResult result) {

			try {
				if (WebRequestCompleted != null) {
					if (this.movedPermanently)
						WebRequestCompleted(requestUri, this.ResponseStream, newUri, eTag, lastModfied, result, this.Priority);
					else
						WebRequestCompleted(requestUri, this.ResponseStream, null, eTag, lastModfied, result, this.Priority);
				}
			} catch { /* ignore ex. thrown in callback */ }
			
		}

		public void OnRequestProgress(Uri requestUri, long bytesTransferred){
		
			try{
				if(WebRequestProgress != null){
					WebRequestProgress(requestUri, bytesTransferred); 
				}

			} catch { /* ignore ex. thrown in callback */ }
		}

		public const int MAX_RETRIES = 25;	// how often we retry, if a url was a redirect (of a redirect of a redirect...)
		public const int BUFFER_SIZE = 4096;	// 4K

		public byte[] BufferRead;
		
		public Stream RequestData {
			get { 
				if (_requestData == null)	//lazy init. "Redirects", or "Not modified" do not need it immediatly
					_requestData = new MemoryStream();
				return _requestData;
			}
		}
		private Stream _requestData;

		public bool movedPermanently = false;
		public bool requestFinalized = false;

		public RequestParameter RequestParams = null; 
		public WebRequest Request;
		public AsyncWebRequest myAsyncWebRequest = null; 
		public WebResponse Response = null;
		public Stream ResponseStream;
		
		public int RetryCount;
		public DateTime StartTime = DateTime.Now;
		public int Priority = 0;
		public Uri InitialRequestUri = null;
		public long bytesTransferred = 0;
		
		#region experimental code
		//public System.Threading.AutoResetEvent allDone;
		//public bool timeOutOnRequest;
		#endregion

		public Uri RequestUri {
			get { 
				if (Request != null) 
					  return Request.RequestUri;
				return null;
			}
		}
	}
}
