#region CVS Version Header
/*
 * $Id: RequestParameter.cs,v 1.5 2006/12/19 04:39:52 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2006/12/19 04:39:52 $
 * $Revision: 1.5 $
 */
#endregion

using System;
using System.IO;
using System.Net;

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
	public delegate void RequestCompleteCallback(Uri requestUri, Stream response, Uri newUri, string eTag, DateTime lastModified, RequestResult result, int priority);
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
	public class RequestParameter {

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
			this(address, userAgent, proxy, credentials, ifModifiedSince, eTag, true) {
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
		public RequestParameter(Uri address, string userAgent, 
			IWebProxy proxy, ICredentials credentials, 
			DateTime ifModifiedSince, string eTag, bool setCookies) {
			
			this.requestUri = address;
			this.userAgent = userAgent;
			this.proxy = proxy;
			this.credentials = credentials;
			this.lastModified = ifModifiedSince;
			this.eTag = eTag;
			this.setCookies = setCookies;
		}
		#endregion

		#region static creator routines
		/// <summary>
		/// To be provided
		/// </summary>
		/// <param name="address"></param>
		/// <param name="userAgent"></param>
		/// <param name="proxy"></param>
		/// <param name="credentials"></param>
		/// <param name="ifModifiedSince"></param>
		/// <param name="eTag"></param>
		/// <returns></returns>
		/// <exception cref="UriFormatException">The URI specified in requestUriString is not a valid URI.</exception>
		public static RequestParameter Create(string address, string userAgent, 
			IWebProxy proxy, ICredentials credentials, DateTime ifModifiedSince, string eTag) {

			return new RequestParameter(new Uri(address), userAgent, proxy, credentials, ifModifiedSince, eTag );
		}

		/// <summary>
		/// To be provided
		/// </summary>
		/// <param name="address"></param>
		/// <param name="userAgent"></param>
		/// <param name="proxy"></param>
		/// <param name="credentials"></param>
		/// <param name="ifModifiedSince"></param>
		/// <param name="eTag"></param>
		/// <param name="setCookies"></param>
		/// <returns></returns>
		/// <exception cref="UriFormatException">The URI specified in requestUriString is not a valid URI.</exception>
		public static RequestParameter Create(string address, string userAgent, 
			IWebProxy proxy, ICredentials credentials, DateTime ifModifiedSince, string eTag, bool setCookies) {

			return new RequestParameter(new Uri(address), userAgent, proxy, credentials, ifModifiedSince, eTag, setCookies );
		}

		/// <summary>
		/// To be provided
		/// </summary>
		/// <param name="address"></param>
		/// <param name="userAgent"></param>
		/// <param name="proxy"></param>
		/// <param name="credentials"></param>
		/// <param name="ifModifiedSince"></param>
		/// <param name="eTag"></param>
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
		public static RequestParameter Create(string address, RequestParameter p) {
			return new RequestParameter(new Uri(address), p.UserAgent, p.Proxy, p.Credentials, p.LastModified, p.ETag, p.SetCookies );
		}
		/// <summary>
		/// Creates a new RequestParameter instance.
		/// </summary>
		/// <param name="credentials"></param>
		/// <param name="p"></param>
		/// <returns></returns>
		public static RequestParameter Create(ICredentials credentials, RequestParameter p) {
			return new RequestParameter(p.RequestUri, p.UserAgent, p.Proxy, credentials, p.LastModified, p.ETag, p.SetCookies );
		}
		/// <summary>
		/// Creates a new RequestParameter instance.
		/// </summary>
		/// <param name="setCookies"></param>
		/// <param name="p"></param>
		/// <returns></returns>
		public static RequestParameter Create(bool setCookies, RequestParameter p) {
			return new RequestParameter(p.RequestUri, p.UserAgent, p.Proxy, p.Credentials, p.LastModified, p.ETag, setCookies );
		}

		/// <summary>
		/// Creates a new RequestParameter instance.
		/// </summary>
		/// <param name="address">request url</param>
		/// <param name="credentials">The credentials.</param>
		/// <param name="p">The RequestParameter.</param>
		/// <returns></returns>
		public static RequestParameter Create(string address,ICredentials credentials, RequestParameter p) {
			return new RequestParameter(new Uri(address), p.UserAgent, p.Proxy, credentials, p.LastModified, p.ETag, p.SetCookies );
		}

		#endregion

		#region request parameter properties
		private Uri requestUri;
		/// <summary>
		/// Gets the Request Uri.
		/// </summary>
		public Uri RequestUri {	get { return this.requestUri; } }

		private string userAgent;
		/// <summary>
		/// Gets the Request user agent string
		/// </summary>
		public string UserAgent { get { return this.userAgent; }	}

		private IWebProxy proxy;
		/// <summary>
		/// Gets the proxy.
		/// </summary>
		public IWebProxy Proxy { get { return this.proxy; }	}

		private ICredentials credentials;
		/// <summary>
		/// Gets the credentials.
		/// </summary>
		public ICredentials Credentials { get { return this.credentials; }	}

		private DateTime lastModified;
		/// <summary>
		/// Gets the "last modified since" date
		/// </summary>
		public DateTime LastModified { 
			get { return this.lastModified; }	
			set { this.lastModified = value; }	
		}

		private string eTag;
		/// <summary>
		/// Gets ETag header info
		/// </summary>
		public string ETag { 
			get { return this.eTag; }	
			set { this.eTag = value; }	
		}

		private bool setCookies;
		/// <summary>
		/// Gets/Set if cookies should be set on request (taken from IE)
		/// </summary>
		public bool SetCookies { 
			get { return this.setCookies; }	
			set { this.setCookies = value; }	
		}
		#endregion
	}
}
