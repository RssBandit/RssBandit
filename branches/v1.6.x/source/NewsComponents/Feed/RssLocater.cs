#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml; 
using System.Collections; 
using System.IO; 
using System.Net; 
using System.Text;

using RssBandit.Common;
using NewsComponents.Net;
using NewsComponents.Utils;

namespace NewsComponents.Feed {

	/// <summary>
	/// Feed Location Methods
	/// </summary>
	public enum FeedLocationMethod{
		/// <summary>
		/// RSS location algorithm described by Mark Pilgrim
		/// </summary>
		AutoDiscoverUrl, 
		/// <summary>
		/// Syndic8 service request
		/// </summary>
		Syndic8Search,
	}

	//TODO: Rework the class to create SimpleHyperLink objects
	// instead of the simple url strings. 
	// This will require to extend the regex's by also matching the
	// link text, if appropriate.

	/// <summary>
	/// This class implements the RSS location algorithm described by Mark Pilgrim at 
	/// http://diveintomark.org/archives/2002/08/15/ultraliberal_rss_locator.html
	/// </summary>
	public class RssLocater
	{	
		// logging/tracing:
		private static readonly log4net.ILog _log = RssBandit.Common.Logging.Log.GetLogger(typeof(RssLocater));

		
		private string userAgent = NewsHandler.DefaultUserAgent;
		
		private ICredentials credentials = null; 
		/// <summary>
		/// Gets or sets the credentials.
		/// </summary>
		/// <value>The credentials.</value>
		public ICredentials Credentials{
			set{ credentials = value;}
			get { return credentials;}		
		}
		
		/// <summary>
		/// Proxy server information used for connections when fetching feeds. 
		/// </summary>
        private IWebProxy proxy = WebRequest.DefaultWebProxy;
		/// <summary>
		/// Proxy server information used for connections when fetching feeds. 
		/// </summary>
		public IWebProxy Proxy{
			set{ proxy = value;}
			get { return proxy;}		
		}

		/// <summary>
		/// Indicates whether the application is offline or not. 
		/// </summary>
		private bool offline = false; 

		/// <summary>
		/// Indicates whether the application is offline or not. 
		/// </summary>
		public bool Offline
		{
			set { offline = value; }
			get { return offline; }
		}

		/// <summary>
		/// Detect, if the url contains the 'feed:' uri scheme. If so, it just remove it
		/// to prepare a valid web url.
		/// </summary>
		/// <param name="webUrl">Url to mangle</param>
		/// <returns>Mangled Url</returns>
		public static string UrlFromFeedProtocolUrl(string webUrl) 
		{
			if (webUrl == null)
				return String.Empty;

			string retUrl = webUrl;
			
			if (retUrl == null)
				return String.Empty;

			if (retUrl.ToLower(CultureInfo.InvariantCulture).StartsWith("feed:"))
				retUrl = retUrl.Substring(5);

			if (retUrl.StartsWith("//"))
				retUrl = retUrl.Substring(2);

			try 
			{
				new Uri(retUrl);
				// valid Url here
			} 
			catch 
			{
				// format exception
				if (!retUrl.ToLower(CultureInfo.InvariantCulture).StartsWith("http")) 
				{
					if (retUrl.StartsWith("/"))
						retUrl = "http://" + retUrl.Substring(1);
					else
						retUrl = "http://" + retUrl;
				}
			}
			return retUrl;
		}

		/// <summary>
		/// Examines the provided webUrl for wellknown local listeners
		/// </summary>
		/// <param name="webUrl">Url string to examine</param>
		/// <returns>ArrayList with the found Urls (strings), or an empty ArrayList</returns>
		/// <remarks>
		/// Examine the link:
		/// we try to find out if someone provides a link to import
		/// a feed or feedlist to Userland, AmphetaDesk, Awasu etc. (Local HTTP Listeners)
		/// A good reference about all these is:
		/// http://xml.mfd-consult.dk/syn-sub/?rss=http://www22.brinkster.com/rendelmann/db/net.rss.xml
		/// (with my blog feed as an example ;-).
		/// These links are of the form (Userland):
		/// <code>http://127.0.0.1:5335/system/pages/subscriptions?url=&lt;FEED_URL&gt;</code>
		/// or (Amphetadesk, see also <a href="http://www.disobey.com/amphetadesk/website_integration.html">Website integration</a>):
		/// <code> 
		/// http://127.0.0.1:8888/index.html?add_url=&lt;FEED_URL>
		/// http://127.0.0.1:8888/index.html?add_urls=&lt;FEED_URL1>,&lt;FEED_URL2>,...
		/// http://127.0.0.1:8888/index.html?add_url=&lt;OPML_URL>
		/// </code>
		/// </remarks>
		public static ArrayList UrlsFromWellknownListener(string webUrl) 
		{
			Uri url = null;
			ArrayList feedurls = new ArrayList();

			try {
				url = new Uri(webUrl);
			} catch {
				// catch invalid url formats
				return feedurls;
			}

			// first look for localhost
			if (url.IsLoopback) {

				string urlQuery = System.Web.HttpUtility.UrlDecode(url.Query);
				string urlQueryLowerCase = urlQuery.ToLower(CultureInfo.InvariantCulture);

				if (url.Port == 8888) 
				{ // AmphetaDesk, Wildgrape Newsdesk
					string urlpart = urlQuery.Substring(urlQuery.IndexOf("=")+1);
					//&amp;go is optionally placed at the end of the href
					//			as a workaround.
					urlpart = urlpart.Replace("&amp;go", "");
					urlpart = urlpart.Replace("&go", "");
					
					if (urlQueryLowerCase.StartsWith("?add_urls")) 
					{	
						feedurls.AddRange(urlpart.Split(new char[]{','}));
					} 
					else if (urlQueryLowerCase.StartsWith("?add_url")) 
					{
						feedurls.Add(urlpart);
					}
				} 
				else if (url.Port == 5335 && urlQueryLowerCase.StartsWith("?url=")) {	
					// Userland, SharpReader
					string   urlpart  = urlQuery.Substring(urlQuery.IndexOf("=")+1);
					feedurls.Add(urlpart);
				}
				else if (url.Port == 8666 && urlQueryLowerCase.StartsWith("?rss=")) {	
					// BottomFeeder: http://127.0.0.1:8666/btf?rss=http://www.intertwingly.net/blog/index.rss2
					string   urlpart  = urlQuery.Substring(urlQuery.IndexOf("=")+1);
					feedurls.Add(urlpart);
				}
				else if (url.Port == 8900 && urlQueryLowerCase.StartsWith("?url=")) {	
					// HeadLine Viewer: http://127.0.0.1:8900/add_provider?url=http://www22.brinkster.com/rendelmann/db/net.rss.xml
					string   urlpart  = urlQuery.Substring(urlQuery.IndexOf("=")+1);
					feedurls.Add(urlpart);
				}
				else if (url.Port == 7810 && urlQueryLowerCase.StartsWith("?action=")) {	
					// nntp//rss: http://127.0.0.1:7810/?action=addform&URL=http://www22.brinkster.com/rendelmann/db/net.rss.xml
					string   urlpart  = urlQuery.Substring(urlQuery.IndexOf("&")+1);
					urlpart  = urlpart.Substring(urlpart.IndexOf("=")+1);
					feedurls.Add(urlpart);
				}
				else if (url.Port == 2604 && urlQueryLowerCase.StartsWith("?url=")) {	
					// Awasu: http://127.0.0.1:2604/subscribe?url=http://www22.brinkster.com/rendelmann/db/net.rss.xml
					string   urlpart  = urlQuery.Substring(urlQuery.IndexOf("=")+1);
					feedurls.Add(urlpart);
				}

			}
			
			return feedurls;
		}

		private   string rpc_start = "<methodCall><methodName>"; 

		private string rpc_middle  = "</methodName><params><param><value>"; 

		private  string rpc_end  = "</value></param></params></methodCall>"; 

		private  string rpc_end2  = "</value></param><param><value><string>feedid</string></value></param><param><value><int>100</int></value></param></params></methodCall>"; 



		/// <summary>
		/// Creates a new <see cref="RssLocater"/> instance.
		/// </summary>
		private RssLocater(){;} 
		
		/// <summary>
		/// Creates a new <see cref="RssLocater"/> instance.
		/// </summary>
		/// <param name="p">P.</param>
		/// <param name="userAgent"></param>
		public RssLocater(IWebProxy p, string userAgent):
			this(p, userAgent, null) {
		}
		
		/// <summary>
		/// Creates a new <see cref="RssLocater"/> instance.
		/// </summary>
		/// <param name="p">WebProxy.</param>
		/// <param name="userAgent">The user agent.</param>
		/// <param name="credentials">The credentials.</param>
		public RssLocater(IWebProxy p, string userAgent, ICredentials credentials){
			this.Proxy = p; 
			if (!string.IsNullOrEmpty(userAgent))
				this.userAgent = userAgent;
			this.Credentials = credentials;
		}

		private Stream GetWebPage(string url)
		{
			return AsyncWebRequest.GetSyncResponseStream(url, this.Credentials, this.userAgent, this.Proxy);			
		}
	
		/// <summary>
		/// Examines the contents of the specified URL looking for 
		/// potential RSS feeds.
		/// </summary>
		/// <param name="url">URL.</param>
		/// <param name="content">Content.</param>
		/// <param name="deepSearch">Deep search.</param>
		/// <returns></returns>
		public  ArrayList GetRssFeedsForUrlContent(string url, string content, bool deepSearch)
		{
			ArrayList list = null;
		
			if(!url.ToLower().StartsWith("http")){
				url = "http://" + url; 
			}

			list = GetRssFeedsFromXml(content, deepSearch, url);
			return list; 
		}

		private ArrayList GetRssFeedsFromXml(string htmlContent, bool deepSearch, string url)
		{
			ArrayList list = new ArrayList();
			list.AddRange(GetRssAutoDiscoveryLinks(htmlContent, url)); 
	
			if(list.Count == 0){ //no RSS autodiscovery links , examine "feed:" link or "localhost" listeners
				list.AddRange(GetLinksFromWellknownLocalListenersOrProtocol(htmlContent));

				if (list.Count == 0) {
					list.AddRange(GetLinksToInternalXmlFiles(htmlContent, url, FeedUrlSearchType.Extension)); 

					if(list.Count == 0){ //no links ending in .xml, .rdf or .rss on same server are RSS feeds 
						list.AddRange(GetLinksToInternalXmlFiles(htmlContent, url, FeedUrlSearchType.Anywhere)); 

						if(list.Count == 0){ //no links with xml, rdf, or rss in the url on same server 
							list.AddRange(GetLinksToExternalXmlFiles(htmlContent, url, FeedUrlSearchType.Extension)); 
		    
							if(list.Count == 0)
							{
								list.AddRange(GetLinksToExternalXmlFiles(htmlContent, url, FeedUrlSearchType.Anywhere));
							
								if(list.Count == 0 && deepSearch)
								{ // no links to external RSS feeds, try Syndic8
		    					
									try
									{ 
										list.AddRange(GetFeedsFromSyndic8(url)); 
									}
									catch (WebException){;}
								}
							}
						}
					}			      
				}
			}
			return list;
		}

		/// <summary>
		/// Gets the RSS feeds for URL.
		/// </summary>
		/// <param name="url">URL.</param>
		/// <param name="throwExceptions">if set to <c>true</c> [throw exceptions].</param>
		/// <returns></returns>
		/// <remarks>
		/// If the URL is not available or down, returns an empty ArrayList.
		/// </remarks>
		public ArrayList GetRssFeedsForUrl(string url, bool throwExceptions)
		{
			ArrayList list = null;
		
			if(!url.ToLower().StartsWith("http")){
				url = "http://" + url; 
			}

			try
			{
				if (LooksLikeRssFeed(url))
					return new ArrayList(new string[]{url});
				list = GetRssFeedsFromXml(GetHtmlContent(url), false, url);
			}
			catch(WebException)
			{
				list = new ArrayList();
				if (throwExceptions)
					throw;
			}

			return list; 
		}

		private string GetHtmlContent(string url)
		{
			string htmlContent = string.Empty;
			using(StreamReader reader = new StreamReader(this.GetWebPage(url)))
			{
				htmlContent = reader.ReadToEnd();
			}
			return htmlContent;
		}


		/// <summary>
		/// Gets the RSS auto discovery links from the specified URL.
		/// </summary>
		/// <param name="url">URL.</param>
		/// <returns></returns>
		public  ArrayList GetRssAutoDiscoveryLinks(string url)
		{
			return GetRssAutoDiscoveryLinks(GetHtmlContent(url), url);
		}

		public ArrayList GetRssAutoDiscoveryLinks(string htmlContent, string baseUri)
		{
			Regex autoDiscoverRegex = new Regex(autoDiscoverRegexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
			
			ArrayList list = new ArrayList();
	    
			// Note that right now we only check that 
			// type="application/rss+xml" and href="urlToRssFile"

			//<link rel="alternate" type="application/rss+xml" title="RSS" href="url/to/rss/file">
			//<link rel=alternate type=application/rss+xml title=RSS href=url/to/rss/file>
			//<link rel='alternate type=application/rss+xml' title='RSS' href='url/to/rss/file'>
			MatchCollection matches = autoDiscoverRegex.Matches(htmlContent);
			foreach(Match match in matches)
			{
				if((match.Value.ToLower().IndexOf("application/atom+xml") > 0)
					|| (match.Value.ToLower().IndexOf("application/rss+xml") > 0))
				{
					string url = match.Groups["href"].Value;
					url = ConvertToAbsoluteUrl(url, baseUri);
					if(LooksLikeRssFeed(url))
					{
						if(!list.Contains(url))
							list.Add(url);
					}
				}
			}
			return list; 
		}

		private  ArrayList GetLinksFromWellknownLocalListenersOrProtocol(string htmlContent)
		{
			ArrayList list = new ArrayList(); 
	    
			//Matches any href with a value starting with feed: or feed://
			Regex regexfeedProtocolHrefRegex = new Regex(hrefFeedProtocolPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
			MatchCollection matches = regexfeedProtocolHrefRegex.Matches(htmlContent);
			foreach(Match match in matches)
			{
				// The href group only contains the portion AFTER feed: or 
				// feed://
				string url = UrlFromFeedProtocolUrl(match.Groups["href"].Value);
				if (url.Length > 0 && !list.Contains(url))
					list.Add(url); 
			}

			if (list.Count > 0)
				return list; 

			Regex hrefListenersRegex = new Regex(hrefListenersPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
			matches = hrefListenersRegex.Matches(htmlContent);
			foreach(Match match in matches)
			{
				list.AddRange(UrlsFromWellknownListener(match.Groups["href"].Value)); 
			}
			return list; 
		}

		private  ArrayList GetLinksToInternalXmlFiles(string htmlContent, string baseUri, FeedUrlSearchType searchType)
		{
			ArrayList list = new ArrayList(); 

			MatchCollection matches = null;

			if(searchType == FeedUrlSearchType.Extension)
			{
				Regex feedExtensionLinkRegex = new Regex(hrefRegexFeedExtensionPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
				matches = feedExtensionLinkRegex.Matches(htmlContent);
			}
			else
			{
				Regex feedUrlLinkRegex = new Regex(hrefRegexFeedUrlPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
				matches = feedUrlLinkRegex.Matches(htmlContent);
			}
	    
			foreach(Match match in matches)
			{
				string url = ConvertToAbsoluteUrl(match.Groups["href"].Value, baseUri);
				if(OnSameServer(baseUri, url) && LooksLikeRssFeed(url))
				{
					if (!list.Contains(url))
						list.Add(url); 
				}
			}
			return list; 
		}

		private  ArrayList GetLinksToExternalXmlFiles(string htmlContent, string baseUri, FeedUrlSearchType searchType)
		{
			ArrayList list = new ArrayList(); 

			MatchCollection matches = null;
			if(searchType == FeedUrlSearchType.Extension)
			{
				Regex feedExtensionLinkRegex = new Regex(hrefRegexFeedExtensionPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
				matches = feedExtensionLinkRegex.Matches(htmlContent);
			}
			else
			{
				Regex feedUrlLinkRegex = new Regex(hrefRegexFeedUrlPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
				matches = feedUrlLinkRegex.Matches(htmlContent);
			}
			foreach(Match match in matches)
			{
				string url = ConvertToAbsoluteUrl(match.Groups["href"].Value, baseUri);
				if((!OnSameServer(baseUri, url)) && LooksLikeRssFeed(url))
				{
					if (!list.Contains(url))
						list.Add(url); 
				}
			}
			return list; 
		}


//		private ICollection MakeHyperLinkArrayFrom(string[] urls) {
//			if (urls == null || urls.Length == 0)
//				return new SimpleHyperLink[]{};
//			SimpleHyperLink[] a = new SimpleHyperLink[urls.Length];
//			for (int i=0; i<urls.Length; i++)
//				a[i] = new SimpleHyperLink(urls[i]);
//			return a;
//		}

		private  bool OnSameServer(string url1, string url2){
	    
			Uri uri1 = new Uri(url1); 
			Uri uri2 = new Uri(url2); 

			if(uri1.Host.Equals(uri2.Host)){
				return true; 
			}else{ 
				return false; 
			}

		}

		private  string ConvertToAbsoluteUrl(string url, string baseurl)
		{
			try
			{ 
				Uri uri = new Uri(url); 
				return uri.CanonicalizedUri(); 
			}
			catch(UriFormatException)
			{
				try{
					Uri baseUri= new Uri(baseurl);
					return (new Uri(baseUri,url).CanonicalizedUri()); 
				}catch(UriFormatException){ /* This is a last resort so we don't bork processing chain*/
					return "http://www.example.com";
				}
				/* 
					string fullurl = String.Empty; 

				  if(baseurl.EndsWith("/")){
					fullurl = url.StartsWith("/") ? baseurl + url.Substring(1) : baseurl + url; 
				  }else if(url.StartsWith("/")){
					fullurl =  baseurl + url;
				  }else{
					fullurl = baseurl + "/" + url; 
				  }

				  return new Uri(fullurl).ToString(); */
			}

		}

	  
		private  bool LooksLikeRssFeed(string url){

			XmlTextReader reader = null;

			try{ 
	  
				reader = new XmlTextReader(this.GetWebPage(url));
				reader.XmlResolver = null; 
				reader.MoveToContent();

				if((reader.LocalName == "rss") || (reader.LocalName == "RDF") || (reader.LocalName == "feed") ){
		
					return true; 
				}else{
					return false; 
				}

			}catch(Exception){ 
				return false; 
			}finally{
	      
				if(reader != null){
					reader.Close(); 
				}
	      
			}

	    
		}

	  
		private HttpWebResponse SendRequestToSyndic8(string functionname, string param){

			string rpc_message = rpc_start + functionname + rpc_middle + param + (functionname.StartsWith("syndic8.FindF") ? rpc_end2 : rpc_end);  			
			Encoding enc = Encoding.UTF8, unicode = Encoding.Unicode;
			byte[] encBytes = Encoding.Convert(unicode, enc, unicode.GetBytes(rpc_message)); //enc.GetBytes(rpc_message); enough ???
		
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create("http://www.syndic8.com/xmlrpc.php");
			request.Timeout          = 1 * 60 * 1000; //one minute timeout 
			request.Credentials = CredentialCache.DefaultCredentials; //???
			request.UserAgent = NewsHandler.GlobalUserAgentString;
			request.Method = "POST";
			request.ContentType = "text/xml";
			request.Proxy            = this.Proxy;
			request.Headers.Add("charset", "UTF-8");	// see http://asg.web.cmu.edu/rfc/rfc2616.html and http://www.iana.org/assignments/character-sets
			request.ContentLength = encBytes.Length; 

			StreamWriter myWriter = null; 
			try{ 
				myWriter = new StreamWriter(request.GetRequestStream());
				myWriter.Write(rpc_message); 
			} catch(Exception e){
	      
				throw new WebException(e.Message, e); 
			}finally{
				if(myWriter != null){
					myWriter.Close(); 	
				}
			}

			 
			return (HttpWebResponse) request.GetResponse(); 

		}

		private string GetResponseString(HttpWebResponse response){

			StringBuilder sb = new StringBuilder(); 
			StringWriter writeStream = null; 
			StreamReader readStream = null; 
	    
			writeStream = new StringWriter(sb); 
	      
			//Retrieve input stream from response and specify encoding 
			Stream receiveStream     = response.GetResponseStream();
			Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
	    
			// Pipe the stream to a higher level stream reader with the required encoding format. 
			readStream = new StreamReader( receiveStream, encode );	      
			Char[] read = new Char[256]; 	      						
			int count = readStream.Read( read, 0, 256 );
	    
	      
			while (count > 0) {
	      
				// Dumps the 256 characters on a string and displays the string to the console.
				writeStream.Write(read, 0, count);
				count = readStream.Read(read, 0, 256);
	      
			}   

			return sb.ToString(); 

		}



		public  Hashtable GetFeedsFromSyndic8(string searchTerm, FeedLocationMethod locationMethod){

			string rpc_method_name = null; 
		  
			switch(locationMethod){
				case FeedLocationMethod.AutoDiscoverUrl:
					rpc_method_name = "syndic8.FindSites";
					break; 

				case FeedLocationMethod.Syndic8Search:
					rpc_method_name = "syndic8.FindFeeds";
					break; 

				default:
					rpc_method_name = "syndic8.FindSites";
					break;
			}

			HttpWebResponse response = SendRequestToSyndic8(rpc_method_name,
				"<DataURL>" + searchTerm + "</DataURL>"); 
			string syndic8response   = GetResponseString(response); 
			_log.Debug(syndic8response); 

			XmlDocument doc = new XmlDocument(); 
			doc.LoadXml(syndic8response); 

			//Build Request array. 
			StringBuilder requestArray = new StringBuilder("<array><data>"); 


			foreach(XmlNode node in doc.SelectNodes("//value/int")){
				requestArray.Append("<value><FeedID>"); 
				requestArray.Append(node.InnerText); 
				requestArray.Append("</FeedID></value>"); 

			}
	 
			requestArray.Append("</data></array>"); 

			response = SendRequestToSyndic8("syndic8.GetFeedInfo", requestArray.ToString()); 
			syndic8response   = GetResponseString(response); 
			
			Hashtable list =   new Hashtable(); 

			try { 

				_log.Debug(syndic8response); 
				doc.LoadXml(syndic8response);			

				foreach(XmlNode node in doc.SelectNodes("//member[name = 'feedid']")){
					XmlNode dataurl = node.ParentNode.SelectSingleNode("member[name = 'dataurl']/value/string");
					if (dataurl != null && dataurl.InnerText.Trim().Length > 0 && !list.ContainsKey(dataurl.InnerText)) {
						XmlNode sitename = node.ParentNode.SelectSingleNode("member[name = 'sitename']/value/string");
						XmlNode desc = node.ParentNode.SelectSingleNode("member[name = 'description']/value/string");
						XmlNode siteurl = node.ParentNode.SelectSingleNode("member[name = 'siteurl']/value/string");
						list.Add(dataurl.InnerText, new string[]{sitename.InnerText, desc.InnerText, siteurl.InnerText, dataurl.InnerText}); 	      
					}
				}
			}catch(XmlException){
				/* Syndic8 sent us malformed XML */
			}

			return list; 
		}	 	 


		public  ArrayList GetFeedsFromSyndic8(string url){
			Hashtable ht = this.GetFeedsFromSyndic8(url, FeedLocationMethod.AutoDiscoverUrl);
			return new ArrayList(ht.Keys); 
		}

		/// <summary>
		/// Enum used to determine where to search for feed strings
		/// </summary>
		enum FeedUrlSearchType
		{
			Extension
			,Anywhere
		}
	  
		#region Regex Patterns
		const string feedExtensionsPattern = "(xml|rdf|rss)";
		const string hrefRegexPattern = @"(\s+href\s*=\s*(?:""(?<href>[^""]*?)""|'(?<href>[^']*?)'|(?<href>[^'""<>\s]+)))";
		const string hrefRegexFeedExtensionPattern = @"(\s+href\s*=\s*(?:""(?<href>[^""]*?\." + feedExtensionsPattern + @")""|'(?<href>[^']*?\." + feedExtensionsPattern + @")'|(?<href>[^'""<>\s]+\." + feedExtensionsPattern + ")))";
		const string hrefRegexFeedUrlPattern = @"(\s+href\s*=\s*(?:""(?<href>[^""]*?" + feedExtensionsPattern + @"[^""]+)""|'(?<href>[^']*?" + feedExtensionsPattern + @"[^']+)'|(?<href>[^'""<>\s]*" + feedExtensionsPattern + @"[^'""<>\s]+)))";
		const string hrefFeedProtocolPattern = @"(\s+href\s*=\s*(?:""feed:(//)?(?<href>[^""]*?)""|'feed:(//)?(?<href>[^']*?)'|feed:(//)?(?<href>[^'""<>\s]+)))";
		const string hrefListenersPattern = @"(\s+href\s*=\s*(?:""(?<href>http://(127.0.0.1|localhost):[^""]*?)""|'(?<href>http://(127.0.0.1|localhost):[^']*?)'|(?<href>http://(127.0.0.1|localhost):[^'""<>\s]+)))";
		const string attributeRegexPattern = @"(\s+(?<attName>\w+)\s*=\s*(?:""(?<attVal>[^""]*?)""|'(?<attVal>[^']*?)'|(?<attVal>[^'""<>\s]+))?)";
		const string autoDiscoverRegexPattern = "<link(" + attributeRegexPattern + @"+|\s*)" + hrefRegexPattern + "(" + attributeRegexPattern + @"+|\s*)\s*/?>";
		#endregion	
	}
}
