#region Version Info Header
/*
 * $Id: SyncWebRequest.cs 1098 2012-03-24 10:15:52Z t_rendelmann $
 * $HeadURL: https://svn.code.sf.net/p/rssbandit/code/trunk/source/NewsComponents/Net/SyncWebRequest.cs $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2012-03-24 06:15:52 -0400 (Sat, 24 Mar 2012) $
 * $Revision: 1098 $
 */
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NewsComponents.Utils;

namespace NewsComponents.Net
{
    public class SyncWebRequest : WebRequestBase
    {
        #region GetResponse

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
        public static WebResponse GetResponse(HttpMethod method, string address, ICredentials credentials,
                                              string userAgent,
                                              IWebProxy proxy, DateTime ifModifiedSince, string eTag, int timeout,
                                              Cookie cookie, string body, WebHeaderCollection additonalHeaders)
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
                    httpRequest.Method = method.ToString().ToUpperInvariant();

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

                    if (method != HttpMethod.Get && !string.IsNullOrWhiteSpace(body))
                    {
                        UTF8Encoding encoding = new UTF8Encoding();
                        byte[] data = encoding.GetBytes(body);
                        httpRequest.ContentType = (body.StartsWith("<")
                                                       ? "application/xml"
                                                       : "application/x-www-form-urlencoded");
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

        #endregion

        #region GetResponseHeadersOnly

        /// <summary>
        /// Can be called syncronized to get a HttpWebResponse (Headers only!).
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeout">Request timeout. E.g. 60 * 1000, means one minute timeout. 
        /// If zero or less than zero, the default timeout of one minute will be used</param>
        /// <returns>WebResponse</returns>
        public static WebResponse GetResponseHeadersOnly(string address, IWebProxy proxy, int timeout)
        {
            return GetResponseHeadersOnly(address, proxy, timeout, null);
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
        public static WebResponse GetResponseHeadersOnly(string address, IWebProxy proxy, int timeout,
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

        #region PostResponse

        /// <summary>
        /// Can be called syncronized to get a Http Web Response.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="body">The body of the request</param>
        /// <param name="headers">Additional headers.</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="proxy">Proxy to use</param>
        /// <returns></returns>
        public static HttpWebResponse PostResponse(string address, string body, WebHeaderCollection headers,
                                                   ICredentials credentials, IWebProxy proxy)
        {

            DateTime ifModifiedSince = MinValue;
            return
                GetResponse(HttpMethod.Post, address, credentials, null /* userAgent */, proxy, ifModifiedSince,
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
        public static HttpWebResponse PostResponse(string address, string body, ICredentials credentials,
                                                   IWebProxy proxy, WebHeaderCollection additionalHeaders)
        {

            DateTime ifModifiedSince = MinValue;
            return
                GetResponse(HttpMethod.Post, address, credentials, null /* userAgent */, proxy, ifModifiedSince,
                            null /* eTag */, DefaultTimeout, null /* cookie */, body, additionalHeaders) as
                HttpWebResponse;
        }

        #endregion

        #region PutResponse

        /// <summary>
        /// Can be called syncronized to send a PUT Http request.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="body">The body of the request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="additionalHeaders">The additional headers.</param>
        /// <returns></returns>
        public static HttpWebResponse PutResponse(string address, string body, ICredentials credentials, IWebProxy proxy,
                                                  WebHeaderCollection additionalHeaders)
        {

            DateTime ifModifiedSince = MinValue;
            return
                GetResponse(HttpMethod.Put, address, credentials, null /* userAgent */, proxy, ifModifiedSince,
                            null /* eTag */, DefaultTimeout, null /* cookie */, body, additionalHeaders) as
                HttpWebResponse;
        }

        #endregion
        
        #region DeleteResponse

        /// <summary>
        /// Can be called syncronized to send a DELETE Http request
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="body">The body of the request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="additionalHeaders">The additional headers.</param>
        /// <returns></returns>
        public static HttpWebResponse DeleteResponse(string address, string body, ICredentials credentials,
                                                     IWebProxy proxy, WebHeaderCollection additionalHeaders)
        {

            DateTime ifModifiedSince = MinValue;
            return
                GetResponse(HttpMethod.Delete, address, credentials, null /* userAgent */, proxy, ifModifiedSince,
                            null /* eTag */, DefaultTimeout, null /* cookie */, body, additionalHeaders) as
                HttpWebResponse;
        }

        #endregion

        #region PostResponseStream

        /// <summary>
        /// Can be called syncronized to get a Http Web ResponseStream.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="body">The body of the request</param>
        /// <param name="cookie">The cookie to send with the request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="proxy">Proxy to use</param>
        public static Stream PostResponseStream(string address, string body, Cookie cookie, ICredentials credentials,
                                                IWebProxy proxy)
        {
            string newAddress, eTag = null;
            RequestResult result;
            DateTime ifModifiedSince = MinValue;
            return
                GetResponseStream(HttpMethod.Post, address, out newAddress, credentials, FullUserAgent(null), proxy,
                                  ref ifModifiedSince,
                                  ref eTag, DefaultTimeout, out result, cookie, body, null /* additonalHeaders */);
        }

        #endregion

        #region GetResponseStream

        /// <summary>
        /// Can be called syncronized to get a Http Web ResponseStream.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="proxy">Proxy to use</param>
        public static Stream GetResponseStream(string address, ICredentials credentials, IWebProxy proxy)
        {
            return GetResponseStream(address, credentials, FullUserAgent(null), proxy, DefaultTimeout);
        }

        /// <summary>
        /// Can be called syncronized to get a Http Web ResponseStream.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeout">Timeout in msecs</param>
        public static Stream GetResponseStream(string address, ICredentials credentials, IWebProxy proxy,
                                               int timeout)
        {
            return GetResponseStream(address, credentials, FullUserAgent(null), proxy, timeout);
        }

        /// <summary>
        /// Can be called syncronized to get a Http Web ResponseStream.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="userAgent"></param>
        /// <param name="proxy">Proxy to use</param>
        public static Stream GetResponseStream(string address, ICredentials credentials, string userAgent,
                                               IWebProxy proxy)
        {
            return GetResponseStream(address, credentials, userAgent, proxy, DefaultTimeout);
        }

        /// <summary>
        /// Can be called syncronized to get a Http Web ResponseStream.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="userAgent"></param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="timeout">Timeout in msecs</param>
        public static Stream GetResponseStream(string address, ICredentials credentials, string userAgent,
                                               IWebProxy proxy, int timeout)
        {
            string newAddress, eTag = null;
            RequestResult result;
            DateTime ifModifiedSince = MinValue;
            return
                GetResponseStream(HttpMethod.Get, address, out newAddress, credentials, userAgent, proxy,
                                  ref ifModifiedSince,
                                  ref eTag, timeout, out result, null /* cookie */, null /* body */, null
                    /* additonalHeaders */);
        }


        /// <summary>
        /// Can be called syncronized to get a Http Web ResponseStream.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="additionalHeaders">The additional headers.</param>
        /// <returns></returns>
        public static Stream GetResponseStream(string address, ICredentials credentials,
                                               IWebProxy proxy, WebHeaderCollection additionalHeaders)
        {
            string newAddress, eTag = null;
            RequestResult result;
            DateTime ifModifiedSince = MinValue;
            return
                GetResponseStream(HttpMethod.Get, address, out newAddress, credentials, FullUserAgent(null), proxy,
                                  ref ifModifiedSince,
                                  ref eTag, DefaultTimeout, out result, null /* cookie */, null /* body */,
                                  additionalHeaders);
        }


        /// <summary>
        /// Can be called syncronized to get a Http Web ResponseStream.
        /// </summary>
        /// <param name="address">Url to request</param>
        /// <param name="credentials">Url credentials</param>
        /// <param name="proxy">Proxy to use</param>
        /// <param name="cookie">The HTTP cookie to send along with the request</param>
        public static Stream GetResponseStream(string address, ICredentials credentials,
                                               IWebProxy proxy, Cookie cookie)
        {
            string newAddress, eTag = null;
            RequestResult result;
            DateTime ifModifiedSince = MinValue;
            return
                GetResponseStream(HttpMethod.Get, address, out newAddress, credentials, FullUserAgent(null), proxy,
                                  ref ifModifiedSince,
                                  ref eTag, DefaultTimeout, out result, cookie, null /* body */, null
                    /* additonalHeaders */);
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
        public static Stream GetResponseStream(string address, out string newAddress, ICredentials credentials,
                                               string userAgent, IWebProxy proxy, int timeout)
        {
            string eTag = null;
            RequestResult result;
            DateTime ifModifiedSince = MinValue;
            return
                GetResponseStream(HttpMethod.Get, address, out newAddress, credentials, userAgent, proxy,
                                  ref ifModifiedSince,
                                  ref eTag, timeout, out result, null /* cookie */, null /* body */, null
                    /* additonalHeaders */);
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
        public static Stream GetResponseStream(HttpMethod method, string address, out string newAddress,
                                               ICredentials credentials,
                                               string userAgent,
                                               IWebProxy proxy, ref DateTime ifModifiedSince, ref string eTag,
                                               int timeout, out RequestResult responseResult, Cookie cookie, string body,
                                               WebHeaderCollection additonalHeaders)
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
                GetResponse(method, address, credentials, userAgent, proxy, ifModifiedSince, eTag, timeout, cookie, body,
                            additonalHeaders);

            HttpWebResponse response = wr as HttpWebResponse;
            FileWebResponse fileresponse = wr as FileWebResponse;

            if (response != null)
            {
                if (HttpStatusCode.OK == response.StatusCode ||
                    HttpExtendedStatusCode.IMUsed == (HttpExtendedStatusCode) response.StatusCode)
                {
                    responseResult = RequestResult.OK;
                    // stream will be disposed on response.Close():
                    Stream ret = MakeSeekableStream(response.GetResponseStream()); //GetDeflatedResponse(response);
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
                    throw new WebException(String.Format("Request of '{0}' gets unexpected HTTP response: {1}",
                                                         requestUri, statusCode));
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
                throw new WebException(String.Format("Request of '{0}' gets repeated HTTP response: {1}", requestUri,
                                                     returnCode));
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

    }
}
