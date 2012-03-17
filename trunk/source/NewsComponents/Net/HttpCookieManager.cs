using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using NewsComponents.Utils;
using RssBandit.Common;
using RssBandit.Common.Logging;
using log4net;

namespace NewsComponents.Net
{
    /// <summary>
    /// Used to manage cookies
    /// </summary>
    /// <remarks>see http://www.rendelmann.info/blog/CommentView.aspx?guid=bd99bcd5-7088-4d46-801e-c0fe622dc2e5</remarks>
    internal class HttpCookieManager
    {
        private static readonly ILog _log = DefaultLog.GetLogger(typeof(HttpCookieManager));

        /// <summary>
        /// Retrieves the cookie(s) from windows system and assign them to the request, 
        /// if available.
        /// </summary>
        /// <param name="request">HttpWebRequest</param>
        public static void SetCookies(HttpWebRequest request)
        {
            CookieContainer c = GetCookieContainerUri(request.RequestUri);
            if (c.Count > 0)
                request.CookieContainer = c;
        }

        /// <summary>
        /// Gets newly received cookie(s) and make them persistent within windows system.
        /// </summary>
        /// <param name="response">HttpWebResponse</param>
        public static void GetCookies(HttpWebResponse response)
        {
            if (response.Headers["Set-Cookie"] != null)
            {
                /* 
				 * It seems this may log users out of certain sites, 
				 * see http://www.rssbandit.org/forum/topic.asp?whichpage=1&TOPIC_ID=2080&#4080
				 *	- InternetSetCookie(response.ResponseUri.CanonicalizedUri(), null, response.Headers["Set-Cookie"]);
				 */
            }
        }

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InternetGetCookie(
            string lpszUrl, string lpszCookieName, StringBuilder lpCookieData, ref int lpdwSize);

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InternetSetCookie(
            string lpszUrl, string lpszCookieName, string lpszCookieData);

        private static CookieContainer GetCookieContainerUri(Uri url)
        {
            CookieContainer container = new CookieContainer();
            string cookieHeaders = RetrieveIECookiesForUrl(url.CanonicalizedUri());
            if (cookieHeaders.Length > 0)
            {
                try
                {
                    container.SetCookies(url, cookieHeaders);
                }
                catch (CookieException ce)
                {
                    //we might get an error on malformed cookies
                    _log.Error(
                        String.Format("GetCookieContainerUri() exception parsing '{0}' for url '{1}'", cookieHeaders,
                                      url.CanonicalizedUri()), ce);
                }
            }
            return container;
        }


        private static string RetrieveIECookiesForUrl(string url)
        {
            StringBuilder cookieHeader = new StringBuilder(new String(' ', 256), 256);
            int datasize = cookieHeader.Length;
            if (!InternetGetCookie(url, null, cookieHeader, ref datasize))
            {
                if (datasize < 0)
                    return String.Empty;
                cookieHeader = new StringBuilder(datasize); // resize with new datasize
                InternetGetCookie(url, null, cookieHeader, ref datasize);
            }
            return FixupIECookies(cookieHeader);
        }

        /// <summary>
        /// Fixups the cookies IE may return. 
        /// If there is a semicolon, we replace it with a comma because 
        /// .NET requires comma as a cookie separators.        
        /// If there is a comma AND a semicolon, we escape the comma
        /// first, then replace the semicolon with a comma 
        /// </summary>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private static string FixupIECookies(StringBuilder b)
        {
            string s = b.ToString();
            if (s.IndexOf(",") >= 0 || s.IndexOf(";") >= 0)
            {
                s = s.Replace(",", escapedComma).Replace(";", ",");
            }
            return s;
        }

        private static readonly string escapedComma = HtmlHelper.UrlEncode(",");
    }
}