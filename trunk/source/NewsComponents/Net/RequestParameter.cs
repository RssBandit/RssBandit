#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace NewsComponents.Net
{
	#region delegates
	/// <summary>
	/// Called on every web request, that was queued.
	/// </summary>
	public delegate void RequestQueuedCallback(Uri requestUri, int priority);
	/// <summary>
	/// Called on every queued web request, that is now about to really make the request to 
	/// destination server.
	/// </summary>
	public delegate void RequestStartCallback(Uri requestUri, ref bool cancel);
	/// <summary>
	/// Called, if the web request caused an exception, that is not yet handled by the class itself.
	/// </summary>
	public delegate void RequestExceptionCallback(Uri requestUri, Exception e, int priority);
	/// <summary>
	/// Called on every queued request, when the real fetch is finished.
	/// </summary>
	public delegate void RequestCompleteCallback(Uri requestUri, Stream responseStream, WebResponse response, Uri newUri, string eTag, DateTime lastModified, RequestResult result, int priority);
	/// <summary>
	/// Called infrequently as bytes are transferred for the file. 
	/// </summary>
	public delegate void RequestProgressCallback(Uri requestUri, long bytesTransferred); 
	#endregion

	/// <summary>
	/// State of a successfully made web request.
	/// </summary>
	public enum RequestResult {
		/// <summary>
		/// Request returns a new response stream.
		/// </summary>
		OK,
		/// <summary>
		/// Web request results in a not modfied response.
		/// </summary>
		NotModified
	}

	/// <summary>
	/// Class is a container for a all the needed web request parameter.
	/// </summary>
	public class RequestParameter 
    {

		#region ctor's
		/// <summary>
		/// Constructor initialize a RequestParameter instance. 
		/// </summary>
		/// <param name="address">Requested Web Url</param>
		/// <param name="userAgent">User Agent string the request should send</param>
		/// <param name="proxy">IWebProxy instance, if a proxy should be used</param>
		/// <param name="credentials">ICredentials instance, if credentials have to be used</param>
		/// <param name="ifModifiedSince">Advanced Request Header info</param>
		/// <param name="eTag">Advanced Request Header info</param>
		/// <remarks><see cref="SetCookies">SetCookies</see> is true by default</remarks>
		public RequestParameter(Uri address, string userAgent, 
			IWebProxy proxy, ICredentials credentials, DateTime ifModifiedSince, string eTag):
			this(address, userAgent, proxy, credentials, ifModifiedSince, eTag, true, TimeSpan.FromMilliseconds(AsyncWebRequest.DefaultTimeout)) 
        {
		}

        /// <summary>
        /// Constructor initialize a RequestParameter instance.
        /// </summary>
        /// <param name="address">Requested Web Url</param>
        /// <param name="userAgent">User Agent string the request should send</param>
        /// <param name="proxy">IWebProxy instance, if a proxy should be used</param>
        /// <param name="credentials">ICredentials instance, if credentials have to be used</param>
        /// <param name="ifModifiedSince">Advanced Request Header info</param>
        /// <param name="eTag">Advanced Request Header info</param>
        /// <param name="setCookies">Set cookies on request</param>
        /// <param name="timeout">The timeout.</param>
		public RequestParameter(Uri address, string userAgent, 
			IWebProxy proxy, ICredentials credentials, 
			DateTime ifModifiedSince, string eTag, bool setCookies, TimeSpan timeout) 
        {
			this._requestUri = address;
			this._userAgent = userAgent;
			this._proxy = proxy;
			this._credentials = credentials;
			this.LastModified = ifModifiedSince;
			this.ETag = eTag;
			this.SetCookies = setCookies;
            this.Timeout = timeout;
        }
		#endregion

		#region static creator routines

        /// <summary>
        /// Creates the request parameter with the specified address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="userAgent">The user agent.</param>
        /// <param name="proxy">The proxy.</param>
        /// <param name="credentials">The credentials.</param>
        /// <param name="ifModifiedSince">If modified since.</param>
        /// <param name="eTag">The e tag.</param>
        /// <returns></returns>
		public static RequestParameter Create (Uri address, string userAgent, 
			IWebProxy proxy, ICredentials credentials, DateTime ifModifiedSince, string eTag) {

			return new RequestParameter(address, userAgent, proxy, credentials, ifModifiedSince, eTag );
		}

		/// <summary>
		/// Creates a new RequestParameter instance.
		/// </summary>
		/// <param name="address"></param>
		/// <param name="p"></param>
		/// <returns></returns>
		public static RequestParameter Create(Uri address, RequestParameter p) {
			return new RequestParameter(address, p.UserAgent, p.Proxy, p.Credentials, p.LastModified, p.ETag, p.SetCookies, p.Timeout );
		}
		/// <summary>
		/// Creates a new RequestParameter instance.
		/// </summary>
		/// <param name="credentials"></param>
		/// <param name="p"></param>
		/// <returns></returns>
		public static RequestParameter Create(ICredentials credentials, RequestParameter p) {
			return new RequestParameter(p.RequestUri, p.UserAgent, p.Proxy, credentials, p.LastModified, p.ETag, p.SetCookies, p.Timeout );
		}
		/// <summary>
		/// Creates a new RequestParameter instance.
		/// </summary>
		/// <param name="setCookies"></param>
		/// <param name="p"></param>
		/// <returns></returns>
		public static RequestParameter Create(bool setCookies, RequestParameter p) {
			return new RequestParameter(p.RequestUri, p.UserAgent, p.Proxy, p.Credentials, p.LastModified, p.ETag, setCookies, p.Timeout );
		}

		/// <summary>
		/// Creates a new RequestParameter instance.
		/// </summary>
		/// <param name="address">request url</param>
		/// <param name="credentials">The credentials.</param>
		/// <param name="p">The RequestParameter.</param>
		/// <returns></returns>
		public static RequestParameter Create(Uri address,ICredentials credentials, RequestParameter p) {
			return new RequestParameter(address, p.UserAgent, p.Proxy, credentials, p.LastModified, p.ETag, p.SetCookies, p.Timeout );
		}

		#endregion

		#region request parameter properties
		private readonly Uri _requestUri;
		/// <summary>
		/// Gets the Request Uri.
		/// </summary>
		public Uri RequestUri {	get { return this._requestUri; } }

		private readonly string _userAgent;
		/// <summary>
		/// Gets the Request user agent string
		/// </summary>
		public string UserAgent { get { return this._userAgent; }	}

		private readonly IWebProxy _proxy;
		/// <summary>
		/// Gets the proxy.
		/// </summary>
		public IWebProxy Proxy { get { return this._proxy; }	}

		private readonly ICredentials _credentials;
		/// <summary>
		/// Gets the credentials.
		/// </summary>
		public ICredentials Credentials { get { return this._credentials; }	}

	    /// <summary>
	    /// Gets the "last modified since" date
	    /// </summary>
	    public DateTime LastModified { get; set; }

	    /// <summary>
	    /// Gets ETag header info
	    /// </summary>
	    public string ETag { get; set; }

	    /// <summary>
	    /// Gets/Set if cookies should be set on request (taken from IE)
	    /// </summary>
	    public bool SetCookies { get; set; }

	    /// <summary>
        /// Cookies to be used when sending the request
        /// </summary>
        public CookieCollection Cookies { get; set; }

        /// <summary>
        /// Additional HTTP headers that should be sent. 
        /// </summary>
        public WebHeaderCollection Headers { get; set; }

		/// <summary>
		/// Gets or sets the client certificate.
		/// </summary>
		/// <value>The client certificate.</value>
		public X509Certificate2 ClientCertificate { get; set; }

        /// <summary>
        /// Gets or sets the timeout of this request
        /// </summary>
        public TimeSpan Timeout { get; set; }

		#endregion
	}
}
