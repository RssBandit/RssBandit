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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace NewsComponents.Net
{
    /// <summary>
    /// Indicates which HTTP method is being used when making a synchronous request
    /// </summary>
    public enum HttpMethod
    {
        /// <summary>
        /// HTTP Delete method
        /// </summary>
        Delete,
        /// <summary>
        /// HTTP Get method
        /// </summary>
        Get,
        /// <summary>
        /// HTTP Post method
        /// </summary>
        Post,
        /// <summary>
        /// HTTP Put method
        /// </summary>
        Put
    }

    /// <summary>
    /// Base class for our web/file requests. Provide functions and consts that are in common
    /// </summary>
    public abstract class WebRequestBase
    {
        #region consts

        /// <summary>
        /// We use our own default MinValue for web requests to
        /// prevent first chance exceptions (InvalidRangeException on
        /// assigning to Request.IfModifiedSince). This value is expected
        /// in local Time, so we don't use DateTime.MinValue! It goes out
        /// of range if converted to universal time (e.g. if we have GMT +xy)
        /// </summary>
        protected static readonly DateTime MinValue = new DateTime(1981, 1, 1);

        /// <summary>
        /// Gets the default requests timeout: 2 minutes.
        /// </summary>
        internal const int DefaultTimeout = 2 * 60 * 1000;

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

        #endregion

        #region ctor's

        /// <summary>
        /// Static constructor to init common / shared global framework managers in use.
        /// </summary>
        static WebRequestBase()
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

        #region common protected members

        /// <summary>
        /// Build a full user agent string (template + <paramref name="userAgent"/> or get the global default one.
        /// </summary>
        /// <param name="userAgent">Can be null</param>
        /// <returns></returns>
        protected static string FullUserAgent(string userAgent)
        {
            return FeedSource.UserAgentString(userAgent);
        }

        /// <summary>
        /// Helper method checks if a status code is a redirect or not
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns>True if the status code is a redirect</returns>
        protected static bool IsRedirect(HttpStatusCode statusCode)
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
        protected static bool IsUnauthorized(HttpStatusCode statusCode)
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
        protected static bool IsAccessForbidden(HttpStatusCode statusCode)
        {
            if (statusCode == HttpStatusCode.Forbidden)
                return true;

            return false;
        }

        /// <summary>
        /// Helper to copy a non-seekable stream (like from a HttpResponse) 
        /// to a seekable memory stream. 
        /// </summary>
        /// <param name="input">Input stream. Does not gets closed herein!</param>
        /// <returns>Seekable stream</returns>
        protected static Stream MakeSeekableStream(Stream input)
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

        #endregion
    }
}
