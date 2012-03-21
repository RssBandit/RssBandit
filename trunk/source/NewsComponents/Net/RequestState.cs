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
        public const int MAX_RETRIES = 25;	// how often we retry, if a url was a redirect (of a redirect of a redirect...)
        public const int BUFFER_SIZE = 4096;	// 4K
        
        public event RequestStartCallback WebRequestBeforeStart;
        public event RequestCompleteCallback WebRequestCompleted;
        public event RequestExceptionCallback WebRequestException;
        public event RequestProgressCallback WebRequestProgress;

        private Stream _requestData;
        
        public byte[] ReadBuffer;
        public long BytesTransferred;

        public bool MovedPermanently;
        public bool RequestFinalized;

        public RequestParameter RequestParams;
        public WebRequest Request;
        public WebResponse Response;
        public Stream ResponseStream;

        public int RetryCount;
        public DateTime StartTime = DateTime.Now;
        public int Priority;
        public Uri InitialRequestUri;
        
        #region ctor's

        public RequestState()
        {
            this.ReadBuffer = new byte[BUFFER_SIZE];
        }

        public RequestState(WebRequest request, int priority, RequestParameter requestParameter):
            this()
        {
            if (request == null) throw new ArgumentNullException("request");
            if (requestParameter == null) throw new ArgumentNullException("requestParameter");

            this.Request = request;
            this.InitialRequestUri = request.RequestUri;
            this.Priority = priority;
            this.RequestParams = requestParameter;
        }

        #endregion

        public bool OnRequestBeforeStart()
        {
            bool cancel = false;
            try
            {
                var handler = WebRequestBeforeStart;
                if (handler != null)
                    handler(RequestUri, ref cancel);
            }
            catch { /* ignore ex. thrown in callback */ }
            return cancel;
        }
        
        public void OnRequestException(Exception e)
        {
            this.OnRequestException(this.RequestUri, e);
        }
        
        public void OnRequestException(Uri requestUri, Exception e)
        {
            try
            {
                var handler = WebRequestException;
                if (handler != null)
                {
                    handler(requestUri, e, this.Priority);
                }
            }
            catch { /* ignore ex. thrown in callback */ }
        }

        public void OnRequestCompleted(string eTag, DateTime lastModfied, RequestResult result)
        {
            this.OnRequestCompleted(this.InitialRequestUri, this.RequestUri, eTag, lastModfied, result);
        }

        public void OnRequestCompleted(Uri requestUri, Uri newUri, string eTag, DateTime lastModfied, RequestResult result)
        {
            try
            {
                var handler = WebRequestCompleted;
                if (handler != null)
                {
                    if (this.MovedPermanently)
                        handler(requestUri, this.ResponseStream, this.Response, newUri, eTag, lastModfied, result, this.Priority);
                    else
                        handler(requestUri, this.ResponseStream, this.Response, null, eTag, lastModfied, result, this.Priority);
                }
            }
            catch { /* ignore ex. thrown in callback */ }

        }

        public void OnRequestProgress(Uri requestUri, long bytesTransferred)
        {

            try
            {
                if (WebRequestProgress != null)
                {
                    WebRequestProgress(requestUri, bytesTransferred);
                }

            }
            catch { /* ignore ex. thrown in callback */ }
        }

        
        public Stream RequestData
        {
            get
            {
                if (_requestData == null)	//lazy init. "Redirects", or "Not modified" do not need it immediatly
                    _requestData = new MemoryStream();
                return _requestData;
            }
        }
        
       
        public Uri RequestUri
        {
            get
            {
                if (Request != null)
                    return Request.RequestUri;
                return null;
            }
        }
    }
}
