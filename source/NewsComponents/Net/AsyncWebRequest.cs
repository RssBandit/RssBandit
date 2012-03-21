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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

using log4net;
using NewsComponents.News;
using NewsComponents.Utils;
using RssBandit.Common;
using RssBandit.Common.Logging;

// for cookie management
// for unsafeHeaderParsingFix
// used for certificate issue handling

namespace NewsComponents.Net
{
    /// <summary>
    /// Extended HTTP Response Status Codes.
    /// </summary>
    public enum HttpExtendedStatusCode
    {
        /// <summary>
        /// Instance Manipulation used - Using RFC3229 with feeds.
        /// See also http://bobwyman.pubsub.com/main/2004/09/using_rfc3229_w.html
        /// </summary>
        IMUsed = 226
    }

    /// <summary>
    /// Indicates which HTTP method is being used when making a synchronous request
    /// </summary>
    public enum HttpMethod
    {
        /// <summary>
        /// 
        /// </summary>
        DELETE,
        /// <summary>
        /// 
        /// </summary>
        GET,
        /// <summary>
        /// 
        /// </summary>
        POST,
        /// <summary>
        /// 
        /// </summary>
        PUT
    }

    // TODO: Split out Sync methods/vars to a separate SyncWebRequest or base class
    
    /// <summary>
    /// AsyncWebRequest class. 
    /// </summary>
    public sealed class AsyncWebRequest
    {
        #region consts

        /// <summary>
        /// We use our own default MinValue for web requests to
        /// prevent first chance exceptions (InvalidRangeException on
        /// assigning to Request.IfModifiedSince). This value is expected
        /// in local Time, so we don't use DateTime.MinValue! It goes out
        /// of range if converted to universal time (e.g. if we have GMT +xy)
        /// </summary>
        private static readonly DateTime MinValue = new DateTime(1981, 1, 1);

        /// <summary>
        /// Gets the default requests timeout: 2 minutes.
        /// </summary>
        internal const int DefaultTimeout = 2*60*1000;

        #endregion

        #region events

        /// <summary>
        /// Callback delegate used for OnAllRequestsComplete event.
        /// </summary>
        public delegate void RequestAllCompleteCallback();

        /// <summary>
        /// Event triggered, if all queued async. requests are done.
        /// </summary>
        public event RequestAllCompleteCallback OnAllRequestsComplete = null;

        /// <summary>
        /// Event triggered, if a not yet accepted CertificateIssue is raised by a web request.
        /// </summary>
        public static event EventHandler<CertificateIssueCancelEventArgs> OnCertificateIssue = null;

        #endregion

        #region private members

        /// <summary>
        /// Contains the url's as keys and the allowed (user interaction needed) 
        /// CertificateIssue's within an ICollection as values.
        /// </summary>
        /// <remarks>That content should be maintained completely from within
        /// the OnCertificateIssue event.</remarks>
        private static Dictionary<string, IList<CertificateIssue>> _trustedCertificateIssues =
            new Dictionary<string, IList<CertificateIssue>>(5);


        private readonly Hashtable _queuedRequests;

        /// <summary>
        /// Scheduler which controls the maximum number of threads we use to perform concurrent HTTP requests
        /// </summary>
        private readonly PrioritizingTaskScheduler _scheduler;

        private readonly TaskFactory _taskFactory;

        private static readonly ILog Log = DefaultLog.GetLogger(typeof (AsyncWebRequest));

        #endregion

        #region ctor's

        /// <summary>
        /// Constructor initialize a AsyncWebRequest instance
        /// </summary>
        public AsyncWebRequest()
        {
            _queuedRequests = Hashtable.Synchronized(new Hashtable(17));
            _scheduler = new PrioritizingTaskScheduler();
            _taskFactory = new TaskFactory(new CancellationTokenSource().Token,
                                           TaskCreationOptions.PreferFairness,
                                           TaskContinuationOptions.ExecuteSynchronously,
                                           _scheduler);
        }

        /// <summary>
        /// Static constructor
        /// </summary>
        static AsyncWebRequest()
        {
#if USENEW_CERTCHECK
    // experimental:
            ServicePointManager.ServerCertificateValidationCallback =
                TrustSelectedCertificatePolicy.CheckServerCertificate;  
#else
            ServicePointManager.CertificatePolicy = new TrustSelectedCertificatePolicy();
#endif
            // allow the manager to send proxy crendentials for proxies that require auth.:
            AuthenticationManager.CredentialPolicy = new ProxyCredentialsPolicy();

            // SetAllowUnsafeHeaderParsing(); now controlled by app.config 
        }

        #endregion

        /// <summary>
        /// Gets the pending queued requests.
        /// </summary>
        public int PendingRequests
        {
            get
            {
                return _queuedRequests.Count;
            }
        }

        /// <summary>
        /// Contains the url's as keys and the allowed (user interaction needed) 
        /// CertificateIssue's within an ICollection as values.
        /// </summary>
        /// <remarks>That content should be maintained completely from within
        /// the OnCertificateIssue event.</remarks>
        public static Dictionary<string, IList<CertificateIssue>> TrustedCertificateIssues
        {
            set
            {
                _trustedCertificateIssues = value;
            }
            get
            {
                return _trustedCertificateIssues;
            }
        }

        /// <summary>
        /// Used to make GET requests for multiple URLs asynchronously
        /// </summary>
        /// <param name="requests">The URLs to be fetched</param>
        /// <param name="webRequestStart">callback invoked when each GET request starts</param>
        /// <param name="webRequestComplete">callback invoked when each GET request completes</param>
        /// <param name="webRequestException">callback invoked when each GET request fails</param>
        /// <exception cref="NotSupportedException">The request scheme specified in address has not been registered.</exception>
        /// <exception cref="ArgumentNullException">The requestParameter is a null reference</exception>      
        public void QueueRequestsAsync(List<RequestParameter> requests,
                                           RequestStartCallback webRequestStart,
                                           RequestCompleteCallback webRequestComplete,
                                           RequestExceptionCallback webRequestException)
        {
            const int priority = 10;  // needed for additional requests
            
            // Parallel options object specifies scheduler and max concurrent threads
            ParallelOptions options = new ParallelOptions() { TaskScheduler = _scheduler, MaxDegreeOfParallelism = _scheduler.MaximumConcurrencyLevel };

            // Create an instance of the RequestState and perform the HTTP request for each of the request parameters           
            Parallel.ForEach(requests, options, 
                request =>
                {
                    if (request != null && ! _queuedRequests.Contains(request.RequestUri.CanonicalizedUri()))
                    {
                        _queuedRequests.Add(request.RequestUri.CanonicalizedUri(), null);
            
                        var webRequest = PrepareRequest(request);

                        var state = new RequestState(webRequest, priority, request);
                        state.WebRequestStarted += webRequestStart;
                        state.WebRequestCompleted += webRequestComplete;
                        state.WebRequestException += webRequestException;

                        PerformHttpRequest(state);
                    }
                });

        }

        /// <summary>
        /// Used to create an HTTP request.
        /// </summary>
        /// <param name="requestParameter">Could be modified for each subsequent request</param>
        internal WebRequest PrepareRequest(RequestParameter requestParameter)
        {
            if (requestParameter == null)
                throw new ArgumentNullException("requestParameter");

            // here are the exceptions caused:
            WebRequest webRequest = WebRequest.Create(requestParameter.RequestUri);

            HttpWebRequest httpRequest = webRequest as HttpWebRequest;
            FileWebRequest fileRequest = webRequest as FileWebRequest;
            NntpWebRequest nntpRequest = webRequest as NntpWebRequest;

            if (httpRequest != null)
            {
                // set extended HttpWebRequest params
                httpRequest.Timeout = Convert.ToInt32(requestParameter.Timeout.TotalMilliseconds); // default: two minutes timeout 
                httpRequest.UserAgent = FullUserAgent(requestParameter.UserAgent);
                httpRequest.Proxy = requestParameter.Proxy;
                httpRequest.AllowAutoRedirect = false;
                httpRequest.AutomaticDecompression = DecompressionMethods.GZip |
                                                     DecompressionMethods.Deflate;
                if (requestParameter.Headers != null)
                {
                    httpRequest.Headers.Add(requestParameter.Headers);
                }

                // due to the reported bug 893620 some web server fail with a server error 500
                // if we send DateTime.MinValue as IfModifiedSince. Smoe Unix derivates only know
                // about valid lowest DateTime around 1970. So in the case we use the
                // httpRequest class default setting:
                if (requestParameter.LastModified > MinValue)
                {
                    httpRequest.IfModifiedSince = requestParameter.LastModified;
                }

                /* #if DEBUG
							// further to investigate: with this setting we don't leak connections
							// (try TCPView from http://www.sysinternals.com)
							// read:
							// * http://support.microsoft.com/default.aspx?scid=kb%3Ben-us%3B819450
							// * http://cephas.net/blog/2003/10/29/the_intricacies_of_http.html
							// * http://weblogs.asp.net/jan/archive/2004/01/28/63771.aspx
			
							httpRequest.KeepAlive = false;		// to prevent open HTTP connection leak
							httpRequest.ProtocolVersion = HttpVersion.Version10;	// to prevent "Underlying connection closed" exception(s)
                #endif */

                if (httpRequest.Proxy == null)
                {
                    httpRequest.KeepAlive = false;
                    httpRequest.Proxy = WebRequest.DefaultWebProxy;
                    httpRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                }

                if (requestParameter.ETag != null)
                {
                    httpRequest.Headers.Add("If-None-Match", requestParameter.ETag);
                    httpRequest.Headers.Add("A-IM", "feed");
                }

                if (requestParameter.Credentials != null)
                {
                    httpRequest.KeepAlive = true; // required for authentication to succeed
                    httpRequest.ProtocolVersion = HttpVersion.Version11; // switch back
                    httpRequest.Credentials = requestParameter.Credentials;
                }

                if (requestParameter.ClientCertificate != null)
                {
                    httpRequest.ClientCertificates.Add(requestParameter.ClientCertificate);
                    httpRequest.Timeout *= 2;	// double the timeout (SSL && Client Certs used!)
                }

                if (requestParameter.SetCookies)
                {
                    HttpCookieManager.SetCookies(httpRequest);
                }

                if (requestParameter.Cookies != null)
                {
                    httpRequest.CookieContainer = new CookieContainer();
                    httpRequest.CookieContainer.Add(requestParameter.Cookies);
                }

                //this prevents the feed mixup issue that we've been facing. See 
                //http://www.davelemen.com/archives/2006/04/rss_bandit_feeds_mix_up.html
                //for a user complaint about the issue. 
                httpRequest.Pipelined = false;
            }
            else if (fileRequest != null)
            {
                fileRequest.Timeout = DefaultTimeout;

                if (requestParameter.Credentials != null)
                {
                    fileRequest.Credentials = requestParameter.Credentials;
                }
            }
            else if (nntpRequest != null)
            {
                // ten minutes timeout. Large timeout is needed if this is first time we are fetching news
                //TODO: move the timeout handling to the requestor
                nntpRequest.Timeout = DefaultTimeout * 5;

                if (requestParameter.Credentials != null)
                {
                    nntpRequest.Credentials = requestParameter.Credentials;
                }

                if (requestParameter.LastModified > MinValue)
                {
                    nntpRequest.IfModifiedSince = requestParameter.LastModified;
                }
            }
            else
            {
                throw new NotImplementedException("Unsupported WebRequest type: " + webRequest.GetType());
            }

            return webRequest;
        }

        /// <summary>
        /// Used to a queue an HTTP request for processing
        /// </summary>
        /// <param name="requestParameter"></param>
        /// <param name="webRequestComplete"></param>
        /// <param name="webRequestException"></param>
        /// <param name="webRequestStart"></param>
        /// <param name="priority"></param>
        /// <exception cref="NotSupportedException">The request scheme specified in address has not been registered.</exception>
        /// <exception cref="ArgumentNullException">The requestParameter is a null reference</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have permission to connect to the requested URI or a URI that the request is redirected to.</exception>
        internal RequestState QueueRequest(RequestParameter requestParameter,
                                           RequestStartCallback webRequestStart,
                                           RequestCompleteCallback webRequestComplete,
                                           RequestExceptionCallback webRequestException,
                                           int priority)
        {
            return
                DoQueueRequest(requestParameter, webRequestStart, webRequestComplete,
                             webRequestException, null, priority);
        }

        /// <summary>
        /// Used to a queue an HTTP request for processing
        /// </summary>
        /// <param name="requestParameter"></param>
        /// <param name="webRequestComplete"></param>
        /// <param name="webRequestException"></param>
        /// <param name="webRequestStart"></param>
        /// <param name="webRequestProgress"></param>
        /// <param name="priority"></param>
        /// <exception cref="NotSupportedException">The request scheme specified in address has not been registered.</exception>
        /// <exception cref="ArgumentNullException">The requestParameter is a null reference</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have permission to connect to the requested URI or a URI that the request is redirected to.</exception>
        internal RequestState QueueRequest(RequestParameter requestParameter,
                                           RequestStartCallback webRequestStart,
                                           RequestCompleteCallback webRequestComplete,
                                           RequestExceptionCallback webRequestException,
                                           RequestProgressCallback webRequestProgress,
                                           int priority)
        {
            return
                DoQueueRequest(requestParameter, webRequestStart, webRequestComplete,
                             webRequestException, webRequestProgress, priority);
        }

        /// <summary>
        /// Called for first and subsequent requests.
        /// </summary>
        /// <param name="requestParameter">Could be modified for each subsequent request</param>
        /// <param name="webRequestComplete"></param>
        /// <param name="webRequestException"></param>
        /// <param name="webRequestStart"></param>
        /// <param name="webRequestProgress"></param>
        /// <param name="priority"></param>
        private RequestState DoQueueRequest(RequestParameter requestParameter,
                                           RequestStartCallback webRequestStart,
                                           RequestCompleteCallback webRequestComplete,
                                           RequestExceptionCallback webRequestException,
                                           RequestProgressCallback webRequestProgress,
                                           int priority)
        {
            if (requestParameter == null)
                throw new ArgumentNullException("requestParameter");

            if (_queuedRequests.Contains(requestParameter.RequestUri.CanonicalizedUri()))
                return null; // httpRequest already there
            
            _queuedRequests.Add(requestParameter.RequestUri.CanonicalizedUri(), null);
            
            var webRequest = PrepareRequest(requestParameter);

            RequestState state = new RequestState(webRequest, priority, requestParameter);

            state.WebRequestStarted += webRequestStart;
            state.WebRequestCompleted += webRequestComplete;
            state.WebRequestException += webRequestException;
            state.WebRequestProgress += webRequestProgress;
            
            PerformHttpRequestAsync(state, priority);

            return state;
        }

        private void QueueRequestAgain(RequestParameter requestParameter,
                                           int priority, RequestState prevState)
        {
            if (requestParameter == null)
                throw new ArgumentNullException("requestParameter");

            if (prevState == null)
                throw new ArgumentNullException("prevState");

            var webRequest = PrepareRequest(requestParameter);

            RequestState state = prevState;

            IDisposable dispResponse = state.Response;
            if (dispResponse != null)
            {
                dispResponse.Dispose();
                state.Response = null;
            }

            if (state.ResponseStream != null)
            {
                // we don't want to get out of connections
                state.ResponseStream.Close();
            }

            if (state.Request != null)
            {
                state.Request.Credentials = null;

                // prevent NotImplementedExceptions:
                if (state.Request is HttpWebRequest)
                    state.Request.Abort();
            }
            
            
            state.Request = webRequest;
            state.RequestParams = requestParameter;

            PerformHttpRequestAsync(state, priority);

        }


        /// <summary>
        /// Makes an asynchronous HTTP request using the provided RequestState object
        /// </summary>
        /// <param name="state">The HTTP request information</param>
        /// <param name="priority">The priority of the request</param>
        private void PerformHttpRequestAsync(RequestState state, int priority)
        {
            Task t = _taskFactory.StartNew(x =>
            {
                PerformHttpRequest((RequestState)x);
            }, state, CancellationToken.None, TaskCreationOptions.PreferFairness, _scheduler);

            //increase priority of the task if it needs to be performed quickly
            if (priority > 10)
                _scheduler.Prioritize(t);
        }

        /// <summary>
        /// Performs the HTTP request (synchron).
        /// </summary>
        /// <param name="state">The state.</param>
        private void PerformHttpRequest(RequestState state)
        {
            try
            {
                // next call returns true if the real request should be cancelled 
                // (e.g. if no internet connection available)
                if (state.OnRequestStart())
                {
                    // signal this state to the worker class
                    this.RequestStartCancelled(state);
                    return;
                }
            }
            catch (Exception signalException)
            {
                Log.Error("Error during event dispatch of StartDownloadCallBack()", signalException);
            }
            
            state.StartTime = DateTime.Now;

            try
            {
                Log.Debug("calling GetResponse for " + state.Request.RequestUri);
                state.Response = state.Request.GetResponse();
            }
            catch (Exception responseException)
            {
                //For some reason the HttpWebResponse class throws an exception on 3xx responses
                WebException we = responseException as WebException;

                if ((we != null) && (we.Response != null))
                {
                    state.Response = we.Response;
                }
                else
                {
                    Log.Debug("GetResponse exception for " + state.Request.RequestUri, responseException);
                    state.OnRequestException(responseException);
                    this.FinalizeWebRequest(state);
                    return;
                }
            }

            ProcessResponse(state);

        }

        /*
        /// <summary>
        /// Performs an HTTP request using the provided request state information
        /// </summary>
        /// <param name="state">The HTTP request information</param>
        private void PerformHttpRequestEx(RequestState state)
        {
            try
            {
                // next call returns true if the real request should be cancelled 
                // (e.g. if no internet connection available)
                if (state.OnRequestStart())
                {
                    // signal this state to the worker class
                    this.RequestStartCancelled(state);
                    return;
                }
            }
            catch (Exception signalException)
            {
                Log.Error("Error during dispatch of StartDownloadCallBack()", signalException);
            }
            state.StartTime = DateTime.Now;

            try
            {
                Log.Debug("calling BeginGetResponse for " + state.Request.RequestUri);
                IAsyncResult result = state.Request.BeginGetResponse(this.ResponseCallback, state);
                ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, this.TimeoutCallback, state, state.Request.Timeout, true);
            }
            catch (Exception responseException)
            {
                Log.Debug("BeginGetResponse exception for " + state.Request.RequestUri, responseException);
                state.OnRequestException(responseException);
                this.FinalizeWebRequest(state);
            }
        }
        */
        /// <summary>
        /// To be provided
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public static string FullUserAgent(string userAgent)
        {
            return FeedSource.UserAgentString(userAgent);
        }


        /// <summary>
        /// Callback that is fired if an HTTP request times out
        /// </summary>
        /// <param name="input">the RequestState object</param>
        /// <param name="timedOut">indicates whether a time out occured</param>
        public void TimeoutCallback(object input, bool timedOut)
        {
            if (timedOut)
            {
                RequestState state = (RequestState)input;
                Log.Info("Request Timeout: " + state.RequestUri);
                //TODO: translate exception message:
                state.OnRequestException(new WebException("Request timeout", WebExceptionStatus.Timeout));
                FinalizeWebRequest(state);
            }
        }


        /// <summary>
        /// Cancels the request
        /// </summary>
        /// <param name="state"></param>
        internal void RequestStartCancelled(RequestState state)
        {
            if (state != null && !state.RequestFinalized)
            {
                Log.Info("RequestStart cancelled: " + state.RequestUri);
                state.OnRequestCompleted(state.RequestParams.ETag, state.RequestParams.LastModified,
                                         RequestResult.NotModified);
                _queuedRequests.Remove(state.InitialRequestUri.CanonicalizedUri());
                state.RequestFinalized = true;

                if (_queuedRequests.Count == 0)
                    RaiseOnAllRequestsComplete();
            }
        }

        /// <summary>
        /// Call it to cleanup any made request.
        /// </summary>
        /// <param name="state"></param>
        internal void FinalizeWebRequest(RequestState state)
        {
            if (state != null && !state.RequestFinalized)
            {
                Log.Debug("Request finalized. Request of '" + state.InitialRequestUri.CanonicalizedUri() + "' took " +
                           DateTime.Now.Subtract(state.StartTime) + " seconds");

                // ensure we close the resource so we do not get out of INet connections
                try
                {
                    if (state.ResponseStream != null)
                    {
                        state.ResponseStream.Close();
                    }

                    IDisposable dispResponse = state.Response;
                    if (dispResponse != null)
                    {
                        dispResponse.Dispose();
                        state.Response = null;
                    } // else {	// no response object
                    //	state.Request.Abort();
                    //}
                    if (state.Request != null)
                    {
                        state.Request.Credentials = null;
                        
                        // prevent NotImplementedExceptions:
                        if (state.Request is HttpWebRequest)
                            state.Request.Abort();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("FinalizeWebRequest() caused exception", ex);
                }

                _queuedRequests.Remove(state.InitialRequestUri.CanonicalizedUri());
                state.RequestFinalized = true;

                if (_queuedRequests.Count == 0)
                    RaiseOnAllRequestsComplete();
            }
        }

        /*
        /// <summary>
        /// Callback gets called if BeginGetResponse() has any result.
        /// </summary>
        /// <param name="result"></param>
        private void ResponseCallback(IAsyncResult result)
        {
            RequestState state = null;
            if (result != null)
                state = result.AsyncState as RequestState;

            if (state == null)
                return;

            try
            {
                try
                {
                    state.Response = state.Request.EndGetResponse(result);
                }
                catch (Exception exception)
                {
                    WebException we = exception as WebException;

                    if (we != null && we.Response != null)
                    {
                        state.Response = we.Response;
                    }
                    else
                    {
                        throw;
                    }
                }

                HttpWebResponse httpResponse = state.Response as HttpWebResponse;
                FileWebResponse fileResponse = state.Response as FileWebResponse;
                NntpWebResponse nntpResponse = state.Response as NntpWebResponse;

                if (httpResponse != null)
                {
                    if (httpResponse.ResponseUri != state.RequestUri)
                    {
                        Log.Debug(
                            String.Format("httpResponse.ResponseUri != state.RequestUri: \r\n'{0}'\r\n'{1}'",
                                          httpResponse.ResponseUri, state.RequestUri));
                    }

                    if (HttpStatusCode.OK == httpResponse.StatusCode ||
                        HttpExtendedStatusCode.IMUsed == (HttpExtendedStatusCode)httpResponse.StatusCode)
                    {
                        HttpCookieManager.GetCookies(httpResponse);

                        // provide last request Uri and ETag:
                        state.RequestParams.ETag = httpResponse.Headers.Get("ETag");
                        try
                        {
                            state.RequestParams.LastModified = httpResponse.LastModified;
                        }
                        catch (Exception lmEx)
                        {
                            Log.Debug("httpResponse.LastModified() parse failure: " + lmEx.Message);
                            // Build in header parser failed on provided date format
                            // Try our own parser (last chance)
                            try
                            {
                                state.RequestParams.LastModified =
                                    DateTimeExt.Parse(httpResponse.Headers.Get("Last-Modified"));
                            }
                            catch (FormatException)
                            {
                                // ignore 
                            }
                        }

                        state.ResponseStream = httpResponse.GetResponseStream();
                        state.ResponseStream.BeginRead(state.ReadBuffer, 0, RequestState.BUFFER_SIZE,
                                                       ReadCallback, state);
                        // async read started, so we are done here:
                        Log.Debug("ResponseCallback() web response OK: " + state.RequestUri);

                        return;
                    }

                    if (httpResponse.StatusCode == HttpStatusCode.NotModified)
                    {
                        HttpCookieManager.GetCookies(httpResponse);

                        string eTag = httpResponse.Headers.Get("ETag");
                        // also if it was not modified, we receive a httpResponse.LastModified with current date!
                        // so we did not store it (is is just the same as last-retrived)
                        // provide last request Uri and ETag:
                        state.OnRequestCompleted(state.InitialRequestUri, state.RequestParams.RequestUri, eTag, MinValue,
                                                 RequestResult.NotModified);
                        // cleanup:
                        FinalizeWebRequest(state);
                    }
                    else if ((httpResponse.StatusCode == HttpStatusCode.MovedPermanently)
                             || (httpResponse.StatusCode == HttpStatusCode.Moved))
                    {
                        state.RetryCount++;
                        if (state.RetryCount > RequestState.MAX_RETRIES)
                        {
                            // there is no WebExceptionStatus.UnknownError in .NET 1.0 !!!
                            throw new WebException("Repeated HTTP httpResponse: " + httpResponse.StatusCode,
                                                   null, WebExceptionStatus.RequestCanceled, httpResponse);
                        }

                        string url2 = httpResponse.Headers["Location"];
                        //Check for any cookies
                        HttpCookieManager.GetCookies(httpResponse);

                        state.MovedPermanently = true;
                        //Remove Url from queue 
                        _queuedRequests.Remove(state.InitialRequestUri.CanonicalizedUri());

                        Log.Debug("ResponseCallback() Moved: '" + state.InitialRequestUri + " to " + url2);

                        // Enqueue the request with the new Url. 
                        // We raise the queue priority a bit to get the retry request closer to the just
                        // finished one. So the user get better feedback, because the whole processing
                        // of one request (including the redirection/moved/... ) is visualized as one update
                        // action.


                        Uri req;
                        //Try absolute first
                        if (!Uri.TryCreate(url2, UriKind.Absolute, out req))
                        {
                            // Try relative
                            if (!Uri.TryCreate(httpResponse.ResponseUri, url2, out req))
                                throw new WebException(
                                    string.Format(
                                        "Original resource temporary redirected. Request new resource at '{0}{1}' failed: ",
                                        httpResponse.ResponseUri, url2));
                        }

                        RequestParameter rqp = RequestParameter.Create(req, state.RequestParams);
                        QueueRequest(rqp, null, null, null, null, state.Priority + 1, state);
                        
                    }
                    else if (IsRedirect(httpResponse.StatusCode))
                    {
                        state.RetryCount++;
                        if (state.RetryCount > RequestState.MAX_RETRIES)
                        {
                            // there is no WebExceptionStatus.UnknownError in .NET 1.0 !!!
                            throw new WebException("Repeated HTTP httpResponse: " + httpResponse.StatusCode,
                                                   null, WebExceptionStatus.RequestCanceled, httpResponse);
                        }

                        string url2 = httpResponse.Headers["Location"];
                        //Check for any cookies
                        HttpCookieManager.GetCookies(httpResponse);

                        //Remove Url from queue 
                        _queuedRequests.Remove(state.InitialRequestUri.CanonicalizedUri());

                        Log.Debug("ResponseCallback() Redirect: '" + state.InitialRequestUri + " to " + url2);
                        // Enqueue the request with the new Url. 
                        // We raise the queue priority a bit to get the retry request closer to the just
                        // finished one. So the user get better feedback, because the whole processing
                        // of one request (including the redirection/moved/... ) is visualized as one update
                        // action.

                        Uri req;
                        //Try absolute first
                        if (!Uri.TryCreate(url2, UriKind.Absolute, out req))
                        {
                            // Try relative
                            if (!Uri.TryCreate(httpResponse.ResponseUri, url2, out req))
                                throw new WebException(
                                    string.Format(
                                        "Original resource temporary redirected. Request new resource at '{0}{1}' failed: ",
                                        httpResponse.ResponseUri, url2));
                        }


                        RequestParameter rqp =
                            RequestParameter.Create(req, RebuildCredentials(state.RequestParams.Credentials, url2),
                                                    state.RequestParams);
                        QueueRequest(rqp, null, null, null, null, state.Priority + 1, state);

                    }
                    else if (IsUnauthorized(httpResponse.StatusCode))
                    {
                        if (state.RequestParams.Credentials == null)
                        {
                            // no initial credentials, try with default credentials
                            state.RetryCount++;

                            //Remove Url from queue 
                            _queuedRequests.Remove(state.InitialRequestUri.CanonicalizedUri());

                            // Enqueue the request with the new Url. 
                            // We raise the queue priority a bit to get the retry request closer to the just
                            // finished one. So the user get better feedback, because the whole processing
                            // of one request (including the redirection/moved/... ) is visualized as one update
                            // action.
                            RequestParameter rqp =
                                RequestParameter.Create(CredentialCache.DefaultCredentials, state.RequestParams);
                            QueueRequest(rqp, null, null, null, null, state.Priority + 1, state);
                            
                        }
                        else
                        {
                            // failed with provided credentials

                            if (state.RequestParams.SetCookies)
                            {
                                // one more request without cookies

                                state.RetryCount++;

                                //Remove Url from queue 
                                _queuedRequests.Remove(state.InitialRequestUri.CanonicalizedUri());

                                // Enqueue the request with the new Url. 
                                // We raise the queue priority a bit to get the retry request closer to the just
                                // finished one. So the user get better feedback, because the whole processing
                                // of one request (including the redirection/moved/... ) is visualized as one update
                                // action.
                                RequestParameter rqp = RequestParameter.Create(false, state.RequestParams);
                                QueueRequest(rqp, null, null, null, null, state.Priority + 1, state);
                                
                            }
                            else
                            {
                                throw new ResourceAuthorizationException();
                            }
                        }
                    }
                    else if (IsAccessForbidden(httpResponse.StatusCode) &&
                             state.InitialRequestUri.Scheme == "https")
                    {
                        throw new ClientCertificateRequiredException();
                    }
                    else if (httpResponse.StatusCode == HttpStatusCode.Gone)
                    {
                        throw new ResourceGoneException();
                    }
                    else
                    {
                        string statusDescription = httpResponse.StatusDescription;
                        if (String.IsNullOrEmpty(statusDescription))
                            statusDescription = httpResponse.StatusCode.ToString();

                        string htmlStatusMessage = null;
                        try
                        {
                            htmlStatusMessage = new StreamReader(httpResponse.GetResponseStream()).ReadToEnd();
                        }
                        catch { }

                        throw new WebException(String.IsNullOrEmpty(htmlStatusMessage) 
                            ? String.Format("Unexpected HTTP Response: {0} ({1})", statusDescription, httpResponse.StatusCode)
                            : htmlStatusMessage);
                    }
                }
                else if (fileResponse != null)
                {
                    string reqFile = fileResponse.ResponseUri.LocalPath;

                    if (File.Exists(reqFile))
                    {
                        DateTime lwt = File.GetLastWriteTime(reqFile);
                        state.RequestParams.ETag = lwt.ToString();
                        state.RequestParams.LastModified = lwt;
                    }

                    state.ResponseStream = fileResponse.GetResponseStream();
                    state.ResponseStream.BeginRead(state.ReadBuffer, 0, RequestState.BUFFER_SIZE,
                                                   ReadCallback, state);
                    // async read started, so we are done here:
                    Log.Debug("ResponseCallback() file response OK: " + state.RequestUri);

                    return;
                }
                else if (nntpResponse != null)
                {
                    state.RequestParams.LastModified = DateTime.Now;
                    state.ResponseStream = nntpResponse.GetResponseStream();
                    state.ResponseStream.BeginRead(state.ReadBuffer, 0, RequestState.BUFFER_SIZE,
                                                   ReadCallback, state);
                    // async read started, so we are done here:
                    Log.Debug("ResponseCallback() nntp response OK: " + state.RequestUri);

                    return;
                }
                else
                {
                    Debug.Assert(false,
                                 "ResponseCallback(): unhandled WebResponse type: " +
                                 state.Response.GetType());
                    FinalizeWebRequest(state);
                }
            }
            catch (ThreadAbortException)
            {
                FinalizeWebRequest(state);
                // ignore, just return
            }
            catch (Exception ex)
            {
                Log.Debug("ResponseCallback() exception: " + state.RequestUri + " :" + ex.Message);
                state.OnRequestException(state.InitialRequestUri, ex);
                FinalizeWebRequest(state);
            }
        }
*/

        /// <summary>
        /// WebResponse processing.
        /// </summary>
        internal void ProcessResponse(RequestState state)
        {
            try
            {
                HttpWebResponse httpResponse = state.Response as HttpWebResponse;
                FileWebResponse fileResponse = state.Response as FileWebResponse;
                NntpWebResponse nntpResponse = state.Response as NntpWebResponse;

                if (httpResponse != null)
                {
                    if (httpResponse.ResponseUri != state.RequestUri)
                    {
                        Log.Debug(
                            String.Format("httpResponse.ResponseUri != state.RequestUri: \r\n'{0}'\r\n'{1}'",
                                          httpResponse.ResponseUri, state.RequestUri));
                    }

                    if (HttpStatusCode.OK == httpResponse.StatusCode ||
                        HttpExtendedStatusCode.IMUsed == (HttpExtendedStatusCode)httpResponse.StatusCode)
                    {
                        HttpCookieManager.GetCookies(httpResponse);

                        // provide last request Uri and ETag:
                        state.RequestParams.ETag = httpResponse.Headers.Get("ETag");
                        try
                        {
                            state.RequestParams.LastModified = httpResponse.LastModified;
                        }
                        catch (Exception lmEx)
                        {
                            Log.Debug("httpResponse.LastModified() parse failure: " + lmEx.Message);
                            // Build in header parser failed on provided date format
                            // Try our own parser (last chance)
                            try
                            {
                                state.RequestParams.LastModified =
                                    DateTimeExt.Parse(httpResponse.Headers.Get("Last-Modified"));
                            }
                            catch (FormatException)
                            {
                                /* ignore */
                            }
                        }

                        state.ResponseStream = httpResponse.GetResponseStream();
                        state.ResponseStream.BeginRead(state.ReadBuffer, 0, RequestState.BUFFER_SIZE,
                                                       ReadCallback, state);
                        // async read started, so we are done here:
                        Log.Debug("ProcessResponse() web response OK: " + state.RequestUri);

                        return;
                    }

                    if (httpResponse.StatusCode == HttpStatusCode.NotModified)
                    {
                        HttpCookieManager.GetCookies(httpResponse);

                        string eTag = httpResponse.Headers.Get("ETag");
                        // also if it was not modified, we receive a httpResponse.LastModified with current date!
                        // so we did not store it (is is just the same as last-retrived)
                        // provide last request Uri and ETag:
                        state.OnRequestCompleted(state.InitialRequestUri, state.RequestParams.RequestUri, eTag, MinValue,
                                                 RequestResult.NotModified);
                        // cleanup:
                        FinalizeWebRequest(state);
                    }
                    else if ((httpResponse.StatusCode == HttpStatusCode.MovedPermanently)
                             || (httpResponse.StatusCode == HttpStatusCode.Moved))
                    {
                        state.RetryCount++;
                        if (state.RetryCount > RequestState.MAX_RETRIES)
                        {
                            // there is no WebExceptionStatus.UnknownError in .NET 1.0 !!!
                            throw new WebException("Repeated HTTP httpResponse: " + httpResponse.StatusCode,
                                                   null, WebExceptionStatus.RequestCanceled, httpResponse);
                        }

                        string url2 = httpResponse.Headers["Location"];
                        //Check for any cookies
                        HttpCookieManager.GetCookies(httpResponse);

                        state.MovedPermanently = true;
                        //Remove Url from queue 
                        _queuedRequests.Remove(state.InitialRequestUri.CanonicalizedUri());

                        Log.Debug("ProcessResponse() Moved: '" + state.InitialRequestUri + " to " + url2);

                        // Enqueue the request with the new Url. 
                        // We raise the queue priority a bit to get the retry request closer to the just
                        // finished one. So the user get better feedback, because the whole processing
                        // of one request (including the redirection/moved/... ) is visualized as one update
                        // action.


                        Uri req;
                        //Try absolute first
                        if (!Uri.TryCreate(url2, UriKind.Absolute, out req))
                        {
                            // Try relative
                            if (!Uri.TryCreate(httpResponse.ResponseUri, url2, out req))
                                throw new WebException(
                                    string.Format(
                                        "Original resource temporary redirected. Request new resource at '{0}{1}' failed: ",
                                        httpResponse.ResponseUri, url2));
                        }

                        RequestParameter rqp = RequestParameter.Create(req, state.RequestParams);
                        QueueRequestAgain(rqp, state.Priority + 1, state);

                    }
                    else if (IsRedirect(httpResponse.StatusCode))
                    {
                        state.RetryCount++;
                        if (state.RetryCount > RequestState.MAX_RETRIES)
                        {
                            // there is no WebExceptionStatus.UnknownError in .NET 1.0 !!!
                            throw new WebException("Repeated HTTP httpResponse: " + httpResponse.StatusCode,
                                                   null, WebExceptionStatus.RequestCanceled, httpResponse);
                        }

                        string url2 = httpResponse.Headers["Location"];
                        //Check for any cookies
                        HttpCookieManager.GetCookies(httpResponse);

                        //Remove Url from queue 
                        _queuedRequests.Remove(state.InitialRequestUri.CanonicalizedUri());

                        Log.Debug("ProcessResponse() Redirect: '" + state.InitialRequestUri + " to " + url2);
                        // Enqueue the request with the new Url. 
                        // We raise the queue priority a bit to get the retry request closer to the just
                        // finished one. So the user get better feedback, because the whole processing
                        // of one request (including the redirection/moved/... ) is visualized as one update
                        // action.

                        Uri req;
                        //Try absolute first
                        if (!Uri.TryCreate(url2, UriKind.Absolute, out req))
                        {
                            // Try relative
                            if (!Uri.TryCreate(httpResponse.ResponseUri, url2, out req))
                                throw new WebException(
                                    string.Format(
                                        "Original resource temporary redirected. Request new resource at '{0}{1}' failed: ",
                                        httpResponse.ResponseUri, url2));
                        }


                        RequestParameter rqp =
                            RequestParameter.Create(req, RebuildCredentials(state.RequestParams.Credentials, url2),
                                                    state.RequestParams);
                        QueueRequestAgain(rqp, state.Priority + 1, state);

                    }
                    else if (IsUnauthorized(httpResponse.StatusCode))
                    {
                        if (state.RequestParams.Credentials == null)
                        {
                            // no initial credentials, try with default credentials
                            state.RetryCount++;

                            //Remove Url from queue 
                            _queuedRequests.Remove(state.InitialRequestUri.CanonicalizedUri());

                            // Enqueue the request with the new Url. 
                            // We raise the queue priority a bit to get the retry request closer to the just
                            // finished one. So the user get better feedback, because the whole processing
                            // of one request (including the redirection/moved/... ) is visualized as one update
                            // action.
                            RequestParameter rqp =
                                RequestParameter.Create(CredentialCache.DefaultCredentials, state.RequestParams);
                            QueueRequestAgain(rqp, state.Priority + 1, state);

                        }
                        else
                        {
                            // failed with provided credentials

                            if (state.RequestParams.SetCookies)
                            {
                                // one more request without cookies

                                state.RetryCount++;

                                //Remove Url from queue 
                                _queuedRequests.Remove(state.InitialRequestUri.CanonicalizedUri());

                                // Enqueue the request with the new Url. 
                                // We raise the queue priority a bit to get the retry request closer to the just
                                // finished one. So the user get better feedback, because the whole processing
                                // of one request (including the redirection/moved/... ) is visualized as one update
                                // action.
                                RequestParameter rqp = RequestParameter.Create(false, state.RequestParams);
                                QueueRequestAgain(rqp, state.Priority + 1, state);

                            }
                            else
                            {
                                throw new ResourceAuthorizationException();
                            }
                        }
                    }
                    else if (IsAccessForbidden(httpResponse.StatusCode) &&
                             state.InitialRequestUri.Scheme == "https")
                    {
                        throw new ClientCertificateRequiredException();
                    }
                    else if (httpResponse.StatusCode == HttpStatusCode.Gone)
                    {
                        throw new ResourceGoneException();
                    }
                    else
                    {
                        string statusDescription = httpResponse.StatusDescription;
                        if (String.IsNullOrEmpty(statusDescription))
                            statusDescription = httpResponse.StatusCode.ToString();

                        string htmlStatusMessage = null;
                        try
                        {
                            htmlStatusMessage = new StreamReader(httpResponse.GetResponseStream()).ReadToEnd();
                        }
                        catch { }

                        throw new WebException(String.IsNullOrEmpty(htmlStatusMessage)
                            ? String.Format("Unexpected HTTP Response: {0} ({1})", statusDescription, httpResponse.StatusCode)
                            : htmlStatusMessage);
                    }
                }
                else if (fileResponse != null)
                {
                    string reqFile = fileResponse.ResponseUri.LocalPath;

                    if (File.Exists(reqFile))
                    {
                        DateTime lwt = File.GetLastWriteTime(reqFile);
                        state.RequestParams.ETag = lwt.ToString();
                        state.RequestParams.LastModified = lwt;
                    }

                    state.ResponseStream = fileResponse.GetResponseStream();
                    state.ResponseStream.BeginRead(state.ReadBuffer, 0, RequestState.BUFFER_SIZE,
                                                   ReadCallback, state);
                    // async read started, so we are done here:
                    Log.Debug("ProcessResponse() file response OK: " + state.RequestUri);

                    return;
                }
                else if (nntpResponse != null)
                {
                    state.RequestParams.LastModified = DateTime.Now;
                    state.ResponseStream = nntpResponse.GetResponseStream();
                    state.ResponseStream.BeginRead(state.ReadBuffer, 0, RequestState.BUFFER_SIZE,
                                                   ReadCallback, state);
                    // async read started, so we are done here:
                    Log.Debug("ProcessResponse() nntp response OK: " + state.RequestUri);

                    return;
                }
                else
                {
                    Debug.Assert(false,
                                 "ProcessResponse(): unhandled WebResponse type: " +
                                 state.Response.GetType());
                    FinalizeWebRequest(state);
                }
            }
            catch (ThreadAbortException)
            {
                FinalizeWebRequest(state);
                // ignore, just return
            }
            catch (Exception ex)
            {
                Log.Debug("ProcessResponse() exception: " + state.RequestUri + " :" + ex.Message);
                state.OnRequestException(state.InitialRequestUri, ex);
                FinalizeWebRequest(state);
            }
        }


        private static ICredentials RebuildCredentials(ICredentials credentials, string redirectUrl)
        {
            CredentialCache cc = credentials as CredentialCache;

            if (cc != null)
            {
                IEnumerator iterate = cc.GetEnumerator();
                while (iterate.MoveNext())
                {
                    NetworkCredential c = iterate.Current as NetworkCredential;
                    if (c != null)
                    {
                        // we just take the first one to recreate 
                        string domainUser = c.Domain;
                        if (!string.IsNullOrEmpty(domainUser))
                            domainUser = domainUser + @"\";
                        domainUser = String.Concat(domainUser, c.UserName);
                        return FeedSource.CreateCredentialsFrom(redirectUrl, domainUser, c.Password);
                    }
                }
            }
            // give up/forward original credentials:
            return credentials;
        }

        /// <summary>
        /// Callback gets called (recursively) on subsequent response stream read requests
        /// </summary>
        /// <param name="result"></param>
        private void ReadCallback(IAsyncResult result)
        {
            RequestState state = null;
            if (result != null)
                state = result.AsyncState as RequestState;

            if (state == null)
                return;

            try
            {
                Stream responseStream = state.ResponseStream;
                int read = responseStream.EndRead(result);

                // fix at least one of the leaks in CLR 1.1 (and 1.0?)
                // see also http://dturini.blogspot.com/2004/06/on-past-few-days-im-dealing-with-some.html
                // and: http://support.microsoft.com/?kbid=831138
                if (Common.ClrVersion.Major < 2 && result.AsyncWaitHandle != null)
                    result.AsyncWaitHandle.Close();

                if (read > 0)
                {
                    state.BytesTransferred += read;
                    state.RequestData.Write(state.ReadBuffer, 0, read); // write buffer to mem stream, queue next read:
                    responseStream.BeginRead(state.ReadBuffer, 0, RequestState.BUFFER_SIZE,
                                             ReadCallback, state);

                    if (((state.BytesTransferred / RequestState.BUFFER_SIZE) % 10) == 0)
                    {
                        state.OnRequestProgress(state.InitialRequestUri, state.BytesTransferred);
                    }

                    // continue read:
                    return;
                }

                // completed (stream yet deflated/unzipped, just reset pos.)
                state.ResponseStream = state.RequestData;
                state.ResponseStream.Seek(0, SeekOrigin.Begin);

                state.OnRequestCompleted(state.InitialRequestUri, state.RequestParams.RequestUri,
                                         state.RequestParams.ETag, state.RequestParams.LastModified,
                                         RequestResult.OK);
                // usual cleanup:
                responseStream.Close();
                state.RequestData.Close();
            }
            catch (WebException e)
            {
                Log.Error("ReadCallBack WebException raised. Status: " + e.Status, e);
                state.OnRequestException(state.RequestParams.RequestUri, e);
            }
            catch (Exception e)
            {
                Log.Error("ReadCallBack Exception raised", e);
                state.OnRequestException(state.RequestParams.RequestUri, e);
            }

            FinalizeWebRequest(state);
        }

        /// <summary>
        /// Helper to copy a non-seekable stream (like from a HttpResponse) 
        /// to a seekable memory stream. 
        /// </summary>
        /// <param name="input">Input stream. Does not gets closed herein!</param>
        /// <returns>Seekable stream</returns>
        private static Stream MakeSeekableStream(Stream input)
        {
            MemoryStream output = new MemoryStream();
            int size = RequestState.BUFFER_SIZE; // 4K read buffer
            byte[] writeData = new byte[size];
            while (true)
            {
                size = input.Read(writeData, 0, size);
                if (size > 0)
                {
                    output.Write(writeData, 0, size);
                }
                else
                {
                    break;
                }
            }
            output.Seek(0, SeekOrigin.Begin);
            return output;
        }

        /// <summary>
        /// Helper method checks if a status code is a redirect or not
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns>True if the status code is a redirect</returns>
        public static bool IsRedirect(HttpStatusCode statusCode)
        {
            if ((statusCode == HttpStatusCode.Ambiguous)
                || (statusCode == HttpStatusCode.Found)
                || (statusCode == HttpStatusCode.MultipleChoices)
                || (statusCode == HttpStatusCode.Redirect)
                || (statusCode == HttpStatusCode.RedirectKeepVerb)
                || (statusCode == HttpStatusCode.RedirectMethod)
                || (statusCode == HttpStatusCode.TemporaryRedirect)
                || (statusCode == HttpStatusCode.SeeOther))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Helper method checks if a status code is a unauthorized or not
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns>True if the status code is unauthorized</returns>
        public static bool IsUnauthorized(HttpStatusCode statusCode)
        {
            if (statusCode == HttpStatusCode.Unauthorized)
                return true;
            return false;
        }

        /// <summary>
        /// Helper method determines whether the response requires a client certificate.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <returns>
        /// 	<c>true</c> if a client certificate is required for the specified response; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAccessForbidden(HttpStatusCode statusCode)
        {
            if (statusCode == HttpStatusCode.Forbidden)
                return true;

            return false;
        }

        /// <summary>
        /// Can be called synchronized to get a HttpWebResponse.
        /// </summary>
        /// <param name="method">The HTTP method being used</param>
        /// <param name="address">Url to request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="userAgent"></param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="ifModifiedSince">Header date</param>
        /// <param name="eTag">Header tag</param>
        /// <param name="timeout">Request timeout. E.g. 60 * 1000, means one minute timeout. 
        /// If zero or less than zero, the default timeout of one minute will be used</param>
        /// <param name="cookie">HTTP cookie to send along with the request</param>
        /// <param name="body">The body of the request (if it is POST request)</param>
        /// <param name="additonalHeaders">These are additional headers that are being specified to the Web request</param>
        /// <returns>WebResponse</returns>
        public static WebResponse GetSyncResponse(HttpMethod method, string address, ICredentials credentials, string userAgent,
                                                  IWebProxy proxy, DateTime ifModifiedSince, string eTag, int timeout, Cookie cookie, string body, WebHeaderCollection additonalHeaders)
        {
            try
            {
                WebRequest webRequest = WebRequest.Create(address);

                HttpWebRequest httpRequest = webRequest as HttpWebRequest;
                FileWebRequest fileRequest = webRequest as FileWebRequest;

                if (httpRequest != null)
                {
                    httpRequest.Timeout = (timeout <= 0 ? DefaultTimeout : timeout);
                    //two minute timeout, if lower than zero
                    httpRequest.UserAgent = userAgent ?? FullUserAgent(userAgent);
                    httpRequest.Proxy = proxy;
                    httpRequest.AllowAutoRedirect = false;
                    httpRequest.IfModifiedSince = ifModifiedSince;
                    //httpRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
                    httpRequest.AutomaticDecompression = DecompressionMethods.GZip |
                                                         DecompressionMethods.Deflate;
                    httpRequest.Method = method.ToString();

                    if (additonalHeaders != null)
                    {
                        httpRequest.Headers.Add(additonalHeaders);
                    }

                    if (cookie != null)
                    {
                        httpRequest.CookieContainer = new CookieContainer();
                        httpRequest.CookieContainer.Add(cookie);
                    }

                    if (eTag != null)
                    {
                        httpRequest.Headers.Add("If-None-Match", eTag);
                        httpRequest.Headers.Add("A-IM", "feed");
                    }

                    if (credentials != null)
                    {
                        httpRequest.Credentials = credentials;
                    }

                    if (method != HttpMethod.GET && !StringHelper.EmptyTrimOrNull(body))
                    {
                        UTF8Encoding encoding = new UTF8Encoding();
                        byte[] data = encoding.GetBytes(body);
                        httpRequest.ContentType = (body.StartsWith("<") ? "application/xml" : "application/x-www-form-urlencoded");
                        httpRequest.ContentLength = data.Length;
                        Stream newStream = httpRequest.GetRequestStream();
                        newStream.Write(data, 0, data.Length);
                        newStream.Close();
                    }
                }
                else if (fileRequest != null)
                {
                    fileRequest.Timeout = (timeout <= 0 ? DefaultTimeout : timeout);
                    if (credentials != null)
                    {
                        fileRequest.Credentials = credentials;
                    }
                }
                else
                {
                    Debug.Assert(false,
                                 "GetSyncResponse(): unhandled WebRequest type: " + webRequest.GetType());
                }

                return webRequest.GetResponse();
            }
            catch (Exception e)
            {
                //For some reason the HttpWebResponse class throws an exception on 3xx responses

                WebException we = e as WebException;

                if ((we != null) && (we.Response != null))
                {
                    return we.Response;
                }
                throw;
            } //end try/catch
        }

        #region GetSyncResponseHeadersOnly()

        /// <summary>
        /// Can be called syncronized to get a HttpWebResponse (Headers only!).
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeout">Request timeout. E.g. 60 * 1000, means one minute timeout. 
        /// If zero or less than zero, the default timeout of one minute will be used</param>
        /// <returns>WebResponse</returns>
        public static WebResponse GetSyncResponseHeadersOnly(string address, IWebProxy proxy, int timeout)
        {
            return GetSyncResponseHeadersOnly(address, proxy, timeout, null);
        }

        /// <summary>
        /// Can be called syncronized to get a HttpWebResponse (Headers only!).
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeout">Request timeout. E.g. 60 * 1000, means one minute timeout. 
        /// If zero or less than zero, the default timeout of one minute will be used</param>
        /// <param name="credentials">ICredentials</param>
        /// <returns>WebResponse</returns>
        public static WebResponse GetSyncResponseHeadersOnly(string address, IWebProxy proxy, int timeout,
                                                             ICredentials credentials)
        {
            try
            {
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(address);

                httpRequest.Timeout = (timeout <= 0 ? DefaultTimeout : timeout);
                //one minute timeout, if lower than zero
                if (proxy != null)
                    httpRequest.Proxy = proxy;
                if (credentials != null)
                    httpRequest.Credentials = credentials;
                httpRequest.Method = "HEAD";

                return httpRequest.GetResponse();
            }
            catch (Exception e)
            {
                //For some reason the HttpWebResponse class throws an exception on 3xx responses

                WebException we = e as WebException;

                if ((we != null) && (we.Response != null))
                {
                    return we.Response;
                }
                throw;
            } //end try/catch
        }

        #endregion

        #region GetSyncResponseStream()


        /// <summary>
        /// Can be called syncronized to get a Http Web Response.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="body">The body of the request</param>
        /// <param name="headers">Additional headers.</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="proxy">Proxy to use</param>
        /// <returns></returns>
        public static HttpWebResponse PostSyncResponse(string address, string body, WebHeaderCollection headers, ICredentials credentials, IWebProxy proxy)
        {

            DateTime ifModifiedSince = MinValue;
            return
                GetSyncResponse(HttpMethod.POST, address, credentials, null /* userAgent */, proxy, ifModifiedSince,
                                      null /* eTag */, DefaultTimeout, null /* cookie */, body, headers) as HttpWebResponse;
        }

        /// <summary>
        /// Can be called syncronized to send a POST Http request.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="body">The body of the request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="additionalHeaders">The additional headers.</param>
        /// <returns></returns>
        public static HttpWebResponse PostSyncResponse(string address, string body, ICredentials credentials, IWebProxy proxy, WebHeaderCollection additionalHeaders)
        {

            DateTime ifModifiedSince = MinValue;
            return
                GetSyncResponse(HttpMethod.POST, address, credentials, null /* userAgent */, proxy, ifModifiedSince,
                                      null /* eTag */, DefaultTimeout, null /* cookie */, body, additionalHeaders) as HttpWebResponse;
        }

        /// <summary>
        /// Can be called syncronized to send a PUT Http request.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="body">The body of the request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="additionalHeaders">The additional headers.</param>
        /// <returns></returns>
        public static HttpWebResponse PutSyncResponse(string address, string body, ICredentials credentials, IWebProxy proxy, WebHeaderCollection additionalHeaders)
        {

            DateTime ifModifiedSince = MinValue;
            return
                GetSyncResponse(HttpMethod.PUT, address, credentials, null /* userAgent */, proxy, ifModifiedSince,
                                      null /* eTag */, DefaultTimeout, null /* cookie */, body, additionalHeaders) as HttpWebResponse;
        }


        /// <summary>
        /// Can be called syncronized to send a DELETE Http request
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="body">The body of the request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="additionalHeaders">The additional headers.</param>
        /// <returns></returns>
        public static HttpWebResponse DeleteSyncResponse(string address, string body, ICredentials credentials, IWebProxy proxy, WebHeaderCollection additionalHeaders)
        {

            DateTime ifModifiedSince = MinValue;
            return
                GetSyncResponse(HttpMethod.DELETE, address, credentials, null /* userAgent */, proxy, ifModifiedSince,
                                      null /* eTag */, DefaultTimeout, null /* cookie */, body, additionalHeaders) as HttpWebResponse;
        }

        /// <summary>
        /// Can be called syncronized to get a Http Web ResponseStream.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="body">The body of the request</param>
        /// <param name="cookie">The cookie to send with the request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="proxy">Proxy to use</param>
        public static Stream PostSyncResponseStream(string address, string body, Cookie cookie, ICredentials credentials, IWebProxy proxy)
        {
            string newAddress, eTag = null;
            RequestResult result;
            DateTime ifModifiedSince = MinValue;
            return
                GetSyncResponseStream(HttpMethod.POST, address, out newAddress, credentials, FullUserAgent(null), proxy, ref ifModifiedSince,
                                      ref eTag, DefaultTimeout, out result, cookie, body, null /* additonalHeaders */);
        }

        /// <summary>
        /// Can be called syncronized to get a Http Web ResponseStream.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="proxy">Proxy to use</param>
        public static Stream GetSyncResponseStream(string address, ICredentials credentials, IWebProxy proxy)
        {
            return GetSyncResponseStream(address, credentials, FullUserAgent(null), proxy, DefaultTimeout);
        }

        /// <summary>
        /// Can be called syncronized to get a Http Web ResponseStream.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeout">Timeout in msecs</param>
        public static Stream GetSyncResponseStream(string address, ICredentials credentials, IWebProxy proxy,
                                                   int timeout)
        {
            return GetSyncResponseStream(address, credentials, FullUserAgent(null), proxy, timeout);
        }

        /// <summary>
        /// Can be called syncronized to get a Http Web ResponseStream.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="userAgent"></param>
        /// <param name="proxy">Proxy to use</param>
        public static Stream GetSyncResponseStream(string address, ICredentials credentials, string userAgent,
                                                   IWebProxy proxy)
        {
            return GetSyncResponseStream(address, credentials, userAgent, proxy, DefaultTimeout);
        }

        /// <summary>
        /// Can be called syncronized to get a Http Web ResponseStream.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="userAgent"></param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeout">Timeout in msecs</param>
        public static Stream GetSyncResponseStream(string address, ICredentials credentials, string userAgent,
                                                   IWebProxy proxy, int timeout)
        {
            string newAddress, eTag = null;
            RequestResult result;
            DateTime ifModifiedSince = MinValue;
            return
                GetSyncResponseStream(HttpMethod.GET, address, out newAddress, credentials, userAgent, proxy, ref ifModifiedSince,
                                      ref eTag, timeout, out result, null /* cookie */, null /* body */, null /* additonalHeaders */ );
        }


        /// <summary>
        /// Can be called syncronized to get a Http Web ResponseStream.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="additionalHeaders">The additional headers.</param>
        /// <returns></returns>
        public static Stream GetSyncResponseStream(string address, ICredentials credentials,
                                                   IWebProxy proxy, WebHeaderCollection additionalHeaders)
        {
            string newAddress, eTag = null;
            RequestResult result;
            DateTime ifModifiedSince = MinValue;
            return
                GetSyncResponseStream(HttpMethod.GET, address, out newAddress, credentials, FullUserAgent(null), proxy, ref ifModifiedSince,
                                      ref eTag, DefaultTimeout, out result, null /* cookie */, null /* body */, additionalHeaders);
        }


        /// <summary>
        /// Can be called syncronized to get a Http Web ResponseStream.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="cookie">The HTTP cookie to send along with the request</param>
        public static Stream GetSyncResponseStream(string address, ICredentials credentials,
                                                   IWebProxy proxy, Cookie cookie)
        {
            string newAddress, eTag = null;
            RequestResult result;
            DateTime ifModifiedSince = MinValue;
            return
                GetSyncResponseStream(HttpMethod.GET, address, out newAddress, credentials, FullUserAgent(null), proxy, ref ifModifiedSince,
                                      ref eTag, DefaultTimeout, out result, cookie, null /* body */, null /* additonalHeaders */);
        }

        /// <summary>
        /// Can be called syncronized to get a Http Web ResponseStream.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="newAddress">New Url, if redirected</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="userAgent"></param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeout">Timeout in msecs</param>
        public static Stream GetSyncResponseStream(string address, out string newAddress, ICredentials credentials,
                                                   string userAgent, IWebProxy proxy, int timeout)
        {
            string eTag = null;
            RequestResult result;
            DateTime ifModifiedSince = MinValue;
            return
                GetSyncResponseStream(HttpMethod.GET, address, out newAddress, credentials, userAgent, proxy, ref ifModifiedSince,
                                      ref eTag, timeout, out result, null /* cookie */, null /* body */, null /* additonalHeaders */);
        }

        /// <summary>
        /// Can be called syncronized to get a Http Web ResponseStream.
        /// </summary>
        /// <param name="method">The HTTP request method</param>
        /// <param name="address">Url to request</param>
        /// <param name="newAddress">out string. return a new url, if the original requested is permanent moved</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="userAgent"></param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="ifModifiedSince">Header date</param>
        /// <param name="eTag">Header tag</param>
        /// <param name="timeout">Request timeout. E.g. 60 * 1000, means one minute timeout.
        /// If zero or less than zero, the default timeout of one minute will be used</param>
        /// <param name="responseResult">out. Result of the request</param>
        /// <param name="cookie">The cookie associated with the request</param>
        /// <param name="body">The body of the HTTP request (if it is a POST)</param>
        /// <param name="additonalHeaders">These are additional headers that are being specified to the Web request</param>
        /// <returns>Stream</returns>
        public static Stream GetSyncResponseStream(HttpMethod method, string address, out string newAddress, ICredentials credentials,
                                                   string userAgent,
                                                   IWebProxy proxy, ref DateTime ifModifiedSince, ref string eTag,
                                                   int timeout, out RequestResult responseResult, Cookie cookie, string body, WebHeaderCollection additonalHeaders)
        {
            bool useDefaultCred = false;
            int requestRetryCount = 0;
            const int MaxRetries = 25;

            newAddress = null;

        send_request:

            string requestUri = address;
            if (useDefaultCred)
                credentials = CredentialCache.DefaultCredentials;

            WebResponse wr =
                GetSyncResponse(method, address, credentials, userAgent, proxy, ifModifiedSince, eTag, timeout, cookie, body, additonalHeaders);

            HttpWebResponse response = wr as HttpWebResponse;
            FileWebResponse fileresponse = wr as FileWebResponse;

            if (response != null)
            {
                if (HttpStatusCode.OK == response.StatusCode ||
                    HttpExtendedStatusCode.IMUsed == (HttpExtendedStatusCode)response.StatusCode)
                {
                    responseResult = RequestResult.OK;
                    // stream will be disposed on response.Close():
                    Stream ret = MakeSeekableStream(response.GetResponseStream());//GetDeflatedResponse(response);
                    response.Close();
                    return ret;
                }
                if ((response.StatusCode == HttpStatusCode.MovedPermanently)
                    || (response.StatusCode == HttpStatusCode.Moved))
                {
                    newAddress = HtmlHelper.ConvertToAbsoluteUrl(response.Headers["Location"], address, false);
                    address = newAddress;
                    response.Close();

                    if (requestRetryCount < MaxRetries)
                    {
                        requestRetryCount++;
                        goto send_request;
                    }
                }
                else if (IsUnauthorized(response.StatusCode))
                {
                    //try with default credentials

                    useDefaultCred = true;
                    response.Close();

                    if (requestRetryCount < MaxRetries)
                    {
                        requestRetryCount++;
                        goto send_request;
                    }
                }
                else if (IsRedirect(response.StatusCode))
                {
                    address = HtmlHelper.ConvertToAbsoluteUrl(response.Headers["Location"], address, false);
                    response.Close();

                    if (requestRetryCount < MaxRetries)
                    {
                        requestRetryCount++;
                        goto send_request;
                    }
                }
                else if (IsAccessForbidden(response.StatusCode) &&
                        requestUri.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ClientCertificateRequiredException();
                }
                else if (response.StatusCode == HttpStatusCode.Gone)
                {
                    throw new ResourceGoneException();
                }
                else
                {
                    string statusCode = response.StatusDescription;
                    if (String.IsNullOrEmpty(statusCode))
                        statusCode = response.StatusCode.ToString();
                    response.Close();
                    throw new WebException(String.Format("Request of '{0}' gets unexpected HTTP response: {1}", requestUri, statusCode));
                }

                // unauthorized more than MaxRetries
                if (IsUnauthorized(response.StatusCode))
                {
                    response.Close();
                    throw new ResourceAuthorizationException();
                }

                //we got a moved, redirect more than MaxRetries
                string returnCode = response.StatusDescription;
                if (String.IsNullOrEmpty(returnCode))
                    returnCode = response.StatusCode.ToString();
                response.Close();
                throw new WebException(String.Format("Request of '{0}' gets repeated HTTP response: {1}", requestUri, returnCode));
            }

            if (fileresponse != null)
            {
                responseResult = RequestResult.OK;
                // stream will be disposed on response.Close():
                Stream ret = MakeSeekableStream(fileresponse.GetResponseStream()); //GetDeflatedResponse(fileresponse);
                fileresponse.Close();
                return ret;
            }
            throw new ApplicationException("no handler for WebResponse. Address: " + requestUri);
        }

        #endregion

        private void RaiseOnAllRequestsComplete()
        {
            var handler = OnAllRequestsComplete;
            if (handler != null)
            {
                try
                {
                    handler();
                }
                catch (Exception ex)
                {
                    Log.Error("OnAllRequestsComplete() event impl. caused an error", ex);
                }
            }
        }

        internal static void RaiseOnCertificateIssue(object sender, CertificateIssueCancelEventArgs e)
        {
            string url = e.WebRequest.RequestUri.CanonicalizedUri();
            ICollection trusted = null;

            if (_trustedCertificateIssues != null)
            {
                lock (_trustedCertificateIssues)
                {
                    if (_trustedCertificateIssues.ContainsKey(url))
                        trusted = (ICollection)_trustedCertificateIssues[url];
                }
            }

            if (trusted != null && trusted.Count > 0)
            {
                if (trusted.Cast<CertificateIssue>().Any(trustedIssue => trustedIssue == e.CertificateIssue))
                {
                    e.Cancel = false; // is an yet accepted certificate isse
                    return;
                }
            }

            var handler = OnCertificateIssue;
            if (handler != null)
            {
                try
                {
                    handler(sender, e);
                }
                catch (Exception ex)
                {
                    Log.Error("OnCertificateIssue() event impl. caused an error", ex);
                }
            }
        }
    }

}

