#region CVS Version Header
/*
 * $Id: NewsHandler.cs,v 1.76 2005/06/13 13:45:26 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2005/06/13 13:45:26 $
 * $Revision: 1.76 $
 */
#endregion

// activates the new features branches for the Nightcrawler release
// also may need some references set!
#undef NIGHTCRAWLER

#region framework usings
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml; 
using System.Xml.Schema;
using System.IO; 
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Xsl;
using System.Net;
using System.Xml.XPath;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Resources;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.Security.Cryptography;
#endregion

#region project usings
using NewsComponents.Collections;
using NewsComponents.Feed;
using NewsComponents.News;
using NewsComponents.Net;
using NewsComponents.Search;
using NewsComponents.RelationCosmos;
using NewsComponents.Storage;
using NewsComponents.Utils;

using msfeeds.feeds;
#endregion


namespace NewsComponents {

	/// <summary>
	/// Supported Feedlist Formats (import/export).
	/// </summary>
	public enum FeedListFormat{
		/// <summary>
		/// Open Content Syndication. See http://internetalchemy.org/ocs/
		/// </summary>
		OCS,
		/// <summary>
		/// Outline Processor Markup Language, see http://opml.scripting.com/spec
		/// </summary>
		OPML, 
		/// <summary>
		/// Native NewsHandler format
		/// </summary>
		NewsHandler,
		/// <summary>
		/// Native reduced/light NewsHandler format
		/// </summary>
		NewsHandlerLite,
	}


	/// <summary>
	/// Class for managing News feeds. This class is NOT thread-safe.
	/// </summary>
	public class NewsHandler {

		#region ctor's
		/// <summary>
		/// Initialize the userAgent template
		/// </summary>
		static NewsHandler()	{	
			
			StringBuilder sb = new StringBuilder(200);
			sb.Append("{0}");	// userAgent filled in later
			sb.Append(" (.NET CLR ");
			sb.Append(Environment.Version);
			sb.Append("; ");
			sb.Append(Environment.OSVersion.ToString().Replace("Microsoft Windows ", "Win"));
			sb.Append("; http://www.rssbandit.org");
			sb.Append(")");

			userAgentTemplate = sb.ToString();
		}

		/// <summary>
		/// Initializes class. 
		/// </summary>
		public NewsHandler(): this(null, null){;}


		/// <summary>		
		/// </summary>
		/// <param name="cm">The object that manages the on-disk cache of feeds for the 
		/// application. </param>
		public NewsHandler(CacheManager cm): this(null, cm) {;}

		/// <summary>
		/// Constructor initializes class.
		/// </summary>
		/// <param name="applicationName">The Application Name or ID that uses the component. This will be used to 
		/// initialize the user path to store the feeds file and cached items.</param>
		public NewsHandler(string applicationName): this(applicationName, null) {;}


		/// <summary>
		/// Constructor initializes class.
		/// </summary>
		/// <param name="applicationName">The Application Name or ID that uses the component. This will be used to 
		/// initialize the user path to store the feeds file and cached items.</param>
		/// <param name="cm">The object that manages the on-disk cache of feeds for the 
		/// application. </param>
		public NewsHandler(string applicationName, CacheManager cm){
      
			this.LoadFeedlistSchema();   
						
			if(applicationName != null){
				this.applicationName = applicationName;
			}

			this.cacheHandler    = cm; 

			if(this.cacheHandler == null){
				this.cacheHandler = new FileCacheManager(Path.Combine(GetUserPath(applicationName), "Cache"));  
			}

			this.rssParser = new RssParser(this);
			//this.rssParser = new RssParser(this.applicationName, this.cacheHandler);

			AsyncWebRequest.OnAllRequestsComplete += new AsyncWebRequest.RequestAllCompleteCallback(this.OnAllRequestsComplete);

		}

		#endregion

		/// <summary>
		/// Manages the cache. 
		/// </summary>
		private CacheManager cacheHandler; 

		/// <summary>
		/// Manages the FeedType.Rss 
		/// </summary>
		private RssParser rssParser;

		/// <summary>
		/// Provide access to the RssParser for Rss specific tasks
		/// </summary>
		internal RssParser RssParser {
			get { return this.rssParser; }
		}

#if NIGHTCRAWLER 
	
		/// <summary>
		/// Register INewsChannel processing services provided by external plugins
		/// </summary>
		public void RegisterNewsChannel (INewsChannel channel)
		{
			// We use an instance method to register services.
			// So we are able to change later the internal processing to a non-static
			// class/instance if required.
			NewsChannelServices.RegisterNewsChannel(channel);
		}
#endif

		/// <summary>
		/// Gets a empty item list.
		/// </summary>
		public static readonly ArrayList EmptyItemList = new ArrayList(0);

		// logging/tracing:
		private static readonly log4net.ILog _log = RssBandit.Common.Logging.Log.GetLogger(typeof(NewsHandler));

		/// <summary>
		/// Manage the InfoItem relations
		/// </summary>
		private static RelationCosmos.RelationCosmos relationCosmos = new RelationCosmos.RelationCosmos();

		/// <summary>
		/// Proxy server information used for connections when fetching feeds. 
		/// </summary>
		private IWebProxy proxy = GlobalProxySelection.GetEmptyWebProxy(); 

		/// <summary>
		/// Proxy server information used for connections when fetching feeds. 
		/// </summary>
		public IWebProxy Proxy{
			set{ 
				proxy = value; 
				RssParser.GlobalProxy = value;
			}
			get { return proxy;}		
		}
		

		/// <summary>
		/// Indicates whether the application is offline or not. 
		/// </summary>
		private bool offline = false; 

		/// <summary>
		/// Indicates whether the application is offline or not. 
		/// </summary>
		public bool Offline{
			set { 
				offline = value; 
				RssParser.Offline = value;
			}
			get { return offline; }
		}

    
		/// <summary>
		/// Internal flag used after loading feed list to indicate that a category attribute of a feed is not 
		/// listed as one of the category elements. 
		/// </summary>
		private static bool categoryMismatch = false; 

		private static bool traceMode = false; 
		
		/// <summary>
		/// Boolean flag indicates whether errors should be written to a logfile 
		///	using Trace.Write(); 
		/// </summary>
		public static bool TraceMode{
			set {traceMode = value; }
			get {return traceMode; }
		}

		private static bool unconditionalCommentRss = false;
		/// <summary>
		/// Boolean flag indicates whether the commentCount should be considered
		/// for NewsItem.HasExternalRelations() tests.
		///	 Default is false and will test both the CommentRssUrl as a non-empty string
		///	 and commentCount > 0 (zero)
		/// </summary>
		public static bool UnconditionalCommentRss{
			set {unconditionalCommentRss = value; }
			get {return unconditionalCommentRss; }
		}

		#region Feed Credentials handling
		public static ICredentials CreateCredentialsFrom(feedsFeed f) {
			ICredentials c = null;
			if (!StringHelper.EmptyOrNull(f.authUser)) {
				string u = null, p = null;
				GetFeedCredentials(f, ref u, ref p);
				NetworkCredential credentials = null;
				string[] aDomainUser = u.Split(new char[]{'\\'});
				if (aDomainUser.GetLength(0) > 1)	// Domain specified: e.g. Domain\UserName
					credentials = new NetworkCredential(aDomainUser[1], p, aDomainUser[0]);
				else
					credentials = new NetworkCredential(aDomainUser[0], p);
				try{
					Uri feedUri = new Uri(f.link);
					CredentialCache cc = new CredentialCache(); 					cc.Add(feedUri, "Basic", credentials); 					cc.Add(feedUri, "Digest", credentials); 					cc.Add(feedUri, "NTLM", credentials); 					c = cc;
				} catch (UriFormatException){
					c = credentials;
				}
			}
			return c;
		}
		/// <summary>
		/// Create and return a ICredentials object with the provided informations.
		/// </summary>
		/// <param name="domainUser">username and optional a domain: DOMAIN\user</param>
		/// <param name="password">the pwd</param>
		/// <returns>ICredentials</returns>
		public static ICredentials CreateCredentialsFrom(string domainUser, string password) {
			ICredentials c = null;
			if (domainUser != null) {
				  
				NetworkCredential credentials = null;
				string[] aDomainUser = domainUser.Split(new char[]{'\\'});
				if (aDomainUser.GetLength(0) > 1)	// Domain specified: e.g. Domain\UserName
					credentials = new NetworkCredential(aDomainUser[1], password, aDomainUser[0]);
				else
					credentials = new NetworkCredential(aDomainUser[0], password);

				c = credentials;
			}
			return c;
		}

		/// <summary>
		/// Set the authorization credentials for a feed.
		/// </summary>
		/// <param name="f">feedsFeed to be modified</param>
		/// <param name="user">username, identifier</param>
		/// <param name="pwd">password</param>
		public static void SetFeedCredentials(feedsFeed f, string user, string pwd) {
			if (f == null) return;
			f.authPassword = CryptHelper.EncryptB(pwd);
			f.authUser = user;
		}

		/// <summary>
		/// Get the authorization credentials for a feed.
		/// </summary>
		/// <param name="f">feedsFeed, where the credentials are taken from</param>
		/// <param name="user">String return parameter containing the username</param>
		/// <param name="pwd">String return parameter, containing the password</param>
		public static void GetFeedCredentials(feedsFeed f, ref string user, ref string pwd) {
			if (f == null) return;
			pwd = CryptHelper.Decrypt(f.authPassword);
			user = f.authUser;
		}


		/// <summary>
		/// Return ICredentials of a feed. 
		/// </summary>
		/// <param name="feedUrl">url of the feed</param>
		/// <returns>null in the case the feed does not have credentials</returns>
		public ICredentials GetFeedCredentials(string feedUrl) {
			if (feedUrl != null && FeedsTable.Contains(feedUrl))
				return GetFeedCredentials(FeedsTable[feedUrl]);
			return null;
		}

		/// <summary>
		/// Return ICredentials of a feed. 
		/// </summary>
		/// <param name="f">feedsFeed</param>
		/// <returns>null in the case the feed does not have credentials</returns>
		public static ICredentials GetFeedCredentials(feedsFeed f) {
			ICredentials c = null;
			if (f != null && f.authUser != null) {
				return CreateCredentialsFrom(f);
//				string u = null, p = null;
//				GetFeedCredentials(f, ref u, ref p);
//				c = CreateCredentialsFrom(u, p);
			}
			return c;
		}

		#endregion

		#region NntpServerDefinition Credentials handling

		/// <summary>
		/// Set the authorization credentials for a Nntp Server.
		/// </summary>
		/// <param name="sd">NntpServerDefinition to be modified</param>
		/// <param name="user">username, identifier</param>
		/// <param name="pwd">password</param>
		public static void SetNntpServerCredentials(NntpServerDefinition sd, string user, string pwd) {
			if (sd == null) return;
			sd.AuthPassword = CryptHelper.EncryptB(pwd);
			sd.AuthUser = user;
		}

		/// <summary>
		/// Get the authorization credentials for a feed.
		/// </summary>
		/// <param name="sd">NntpServerDefinition, where the credentials are taken from</param>
		/// <param name="user">String return parameter containing the username</param>
		/// <param name="pwd">String return parameter, containing the password</param>
		public static void GetNntpServerCredentials(NntpServerDefinition sd, ref string user, ref string pwd) {
			if (sd == null) return;
			pwd = (sd.AuthPassword != null ? CryptHelper.Decrypt(sd.AuthPassword): null);
			user = sd.AuthUser;
		}


		/// <summary>
		/// Return ICredentials of a nntp server. 
		/// </summary>
		/// <param name="serverAccountName">account name of the server</param>
		/// <returns>null in the case the server does not have credentials</returns>
		public ICredentials GetNntpServerCredentials(string serverAccountName) {
			if (serverAccountName != null && nntpServers.Contains(serverAccountName))
				return GetFeedCredentials((NntpServerDefinition)nntpServers[serverAccountName]);
			return null;
		}

		/// <summary>
		/// Return ICredentials of a feed. 
		/// </summary>
		/// <param name="sd">NntpServerDefinition</param>
		/// <returns>null in the case the nntp server does not have credentials</returns>
		public static ICredentials GetFeedCredentials(NntpServerDefinition sd) {
			ICredentials c = null;
			if (sd.AuthUser != null) {
				string u = null, p = null;
				GetNntpServerCredentials(sd, ref u, ref p);
				c = CreateCredentialsFrom(u, p);
			}
			return c;
		}

		#endregion

		/// <summary>
		/// Gets the refresh rate for a particular feed
		/// </summary>
		public static void GetRefreshRate(){
			//TODO
		}

		/// <summary>
		/// Returns the user path used to store the current feed and cached items.
		/// </summary>
		/// <param name="appname">The application name that uses the component.</param>
		/// <returns></returns>
		public static string GetUserPath(string appname) {
			string s = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appname);
			if(!Directory.Exists(s)) Directory.CreateDirectory(s);
			if (!Directory.Exists(Path.Combine(s,"Cache"))) Directory.CreateDirectory(Path.Combine(s,"Cache"));
			return s;
		}
		
		/// <summary>
		/// Maximum item age. Default value is 1 month.
		/// </summary>
		private TimeSpan maxitemage = new TimeSpan(30, 0, 0, 0);
		
		/// <summary>
		/// Gets or sets the maximum amount of time an item should be kept in the 
		/// cache. This value is used for all feeds unless one is specified on 
		/// the particular feed or its category
		/// </summary>
		public  TimeSpan MaxItemAge { 
			get{ return this.maxitemage;}  
			
			[MethodImpl(MethodImplOptions.Synchronized)]
			set{ this.maxitemage = value; } 
		}

		/// <summary>
		/// The stylesheet for displaying feeds.
		/// </summary>
		private string stylesheet;
		
		/// <summary>
		/// Gets or sets the stylesheet for displaying feeds
		/// </summary>
		public  string Stylesheet { 
			get{ return this.stylesheet;}  
			
			set{ this.stylesheet = value; } 
		}

		/// <summary>
		/// The folder for downloading enclosures.
		/// </summary>
		private string enclosurefolder;
		
		/// <summary>
		/// Gets or sets the folder for downloading enclosures
		/// </summary>
		public  string EnclosureFolder { 
			get{ return this.enclosurefolder;}  
			
			set{ this.enclosurefolder = value; } 
		}


		/// <summary>
		/// Indicates whether items in the feed should be marked as read on exiting
		/// the feed in the UI.
		/// </summary>
		private bool markitemsreadonexit;

		/// <summary>
		/// Gets or sets whether items in the feed should be marked as read on exiting
		/// the feed in the UI
		/// </summary>
		public bool MarkItemsReadOnExit{
			get{ return this.markitemsreadonexit;}  
			
			set{ this.markitemsreadonexit = value; } 		
		}

		/// <summary>
		/// Indicates whether enclosures should be downloaded in the background.
		/// </summary>
		private bool downloadenclosures;
		
		/// <summary>
		/// Gets or sets whether enclosures should be downloaded in the background
		/// </summary>
		public  bool DownloadEnclosures { 
			get{ return this.downloadenclosures;}  
			
			set{ this.downloadenclosures = value; } 
		}

		/// <summary>
		/// Indicates which properties of a NewsItem should be made columns in the RSS Bandit listview
		/// </summary>
		private string listviewlayout;
		
		/// <summary>
		/// Gets or sets wwhich properties of a NewsItem should be made columns in the RSS Bandit listview
		/// </summary>
		public  string FeedColumnLayout { 
			get{ return this.listviewlayout;}  
			
			set{ this.listviewlayout = value; } 
		}

		#region HTTP UserAgent 
		/// <summary>
		/// Our default short HTTP user agent string
		/// </summary>
		private const string defaultUserAgent = "NewsHandler 1.1"; 

		/// <summary>
		/// A template string to assamble a unified user agent string.
		/// </summary>
		private static string userAgentTemplate;

		/// <summary>
		/// global long HTTP user agent string
		/// </summary>
		private static string globalLongUserAgent;

		/// <summary>
		/// Build a full user agent string incl. OS and .NET version 
		/// from the provided userAgent
		/// </summary>
		/// <param name="userAgent">string</param>
		/// <returns>The long HTTP user agent string</returns>
		public static string UserAgentString(string userAgent) {
			if (StringHelper.EmptyOrNull(userAgent))
				return GlobalUserAgentString;
			return String.Format(userAgentTemplate, userAgent); 
		}
		/// <summary>
		/// Returns a global long HTTP user agent string build from the
		/// instance setting. 
		/// To be used by sub-components that do not have a instance variable 
		/// of the NewsHandler.
		/// </summary>
		public static string GlobalUserAgentString {
			get { 
				if (null == globalLongUserAgent)
					globalLongUserAgent = UserAgentString(defaultUserAgent);
				return globalLongUserAgent; 
			}
		}

		/// <summary>
		/// The short HTTP user agent string used when requesting feeds
		/// and the property was not set via 
		/// </summary>
		private string useragent = defaultUserAgent; 

		/// <summary>
		/// The short HTTP user agent string used when requesting feeds. 
		/// </summary>
		public string UserAgent{ 
			get { return useragent;		} 
			set { 
				useragent = value; 	
				globalLongUserAgent = UserAgentString(useragent);
			}		
		}

		/// <summary>
		/// The long HTTP user agent string used when requesting feeds. 
		/// </summary>
		public string FullUserAgent{ 
			get { return UserAgentString(this.UserAgent);	} 
		}
		
		#endregion

		/// <summary>
		/// FeedsCollection representing subscribed feeds list
		/// </summary>
		private FeedsCollection _feedsTable = new FeedsCollection();  

		/// <summary>
		/// Represents the list of available categories for feeds. 
		/// </summary>
		private CategoriesCollection categories = new CategoriesCollection(); 

		/// <summary>
		/// Represents the list of available feed column layouts for feeds. 
		/// </summary>
		private FeedColumnLayoutCollection layouts = new FeedColumnLayoutCollection(); 


		/// <summary>
		/// Hashtable representing downloaded feed items
		/// </summary>
		private Hashtable itemsTable = new Hashtable();  

		/// <summary>
		/// Collection contains NntpServerDefinition objects.
		/// Keys are the account name(s) - friendly names for the news server def.:
		/// NntpServerDefinition.Name's
		/// </summary>
		private ListDictionary nntpServers = new ListDictionary();
		/// <summary>
		/// Collection contains UserIdentity objects.
		/// Keys are the UserIdentity.Name's
		/// </summary>
		private ListDictionary identities = new ListDictionary();

		#region delegates/events/argument classes
		/// <summary>
		/// The callback used within the BeforeDownloadFeedStarted event.
		/// </summary>
		public delegate void DownloadFeedStartedCallback(object sender, DownloadFeedCancelEventArgs e);
		/// <summary>
		/// The event that will be invoked on clients to notify them that 
		/// when a feed starts to be downloaded (AsyncWebRequest). 
		/// </summary>
		public event DownloadFeedStartedCallback BeforeDownloadFeedStarted = null;

		/// <summary>
		/// BeforeDownloadFeedStarted event argument class.
		/// </summary>
		[ComVisible(false)]
		public class DownloadFeedCancelEventArgs: System.ComponentModel.CancelEventArgs {
			/// <summary>
			/// Class initializer.
			/// </summary>
			/// <param name="feed">feed Uri</param>
			/// <param name="cancel">bool, set to true, if you want to cancel further processing</param>
			public DownloadFeedCancelEventArgs(Uri feed, bool cancel):base(cancel) {
				this.feedUri = feed;
			}
			private Uri feedUri;
			/// <summary>
			/// The related feed Uri.
			/// </summary>
			public Uri FeedUri { get { return feedUri; } }
		}
	  
		/// <summary>
		/// Callback delegate used on event OnUpdatedFeed.
		/// </summary>
		public delegate void UpdatedFeedCallback(object sender, UpdatedFeedEventArgs e);
		/// <summary>
		/// Event called on every updated feed.
		/// </summary>
		public event UpdatedFeedCallback OnUpdatedFeed = null;

		/// <summary>
		/// OnUpdatedFeed event argument class.
		/// </summary>
		public class UpdatedFeedEventArgs: EventArgs {
			/// <summary>
			/// Called on every updated feed.
			/// </summary>
			/// <param name="requestUri">Original requested Uri of the feed</param>
			/// <param name="newUri">The (maybe) new feed location. This could be set on a redirect or other mechanism.
			/// If the location was not changed, this parameter is left null</param>
			/// <param name="feedItems">ArrayList with all the newly received feed items</param>
			/// <param name="result">If result is <c>NotModified</c>, the conditional GET succeeds and no items are returned.</param>
			public UpdatedFeedEventArgs(Uri requestUri, Uri newUri, ArrayList feedItems, RequestResult result) {
				this.requestUri = requestUri;
				this.newUri = newUri;
				this.items = feedItems;
				this.result = result;
			}
			private Uri requestUri, newUri;
			/// <summary>
			/// Uri of the feed, that was updated
			/// </summary>
			public Uri UpdatedFeedUri { get { return requestUri; } }	// should return Clone() ?
			/// <summary>
			/// Uri of the feed, if it was moved on the Web to a new location.
			/// </summary>
			public Uri NewFeedUri { get { return newUri; } }				// should return Clone() ?
			private ArrayList items;
			/// <summary>
			/// ArrayList of NewsItem objects.
			/// </summary>
			public ArrayList FeedItems { get { return items; } }
			private RequestResult result;
			/// <summary>
			/// RequestResult: OK or NotModified
			/// </summary>
			public RequestResult UpdateState { get { return result; } }
		}

		/// <summary>
		/// Callback delegate used for event OnUpdateFeedException
		/// </summary>
		public delegate void UpdateFeedExceptionCallback(object sender, UpdateFeedExceptionEventArgs e);
		/// <summary>
		/// Event called, if the WebRequest fails with any exception.
		/// </summary>
		public event UpdateFeedExceptionCallback OnUpdateFeedException = null;

		/// <summary>
		/// Event argument class used in OnUpdateFeedException.
		/// </summary>
		public class UpdateFeedExceptionEventArgs: EventArgs {
			/// <summary>
			/// Initializer
			/// </summary>
			/// <param name="requestUri">feed Uri, that was requested</param>
			/// <param name="e">Exception caused by the request</param>
			public UpdateFeedExceptionEventArgs(Uri requestUri, Exception e) {
				this.requestUri = requestUri;
				this.exception = e;
			}
			private Uri requestUri;
			/// <summary>
			/// feed Uri.
			/// </summary>
			public Uri UpdatedFeedUri { get { return requestUri; } }	// should return Clone() ?
			private Exception exception;
			/// <summary>
			/// caused exception
			/// </summary>
			public Exception ExceptionThrown { get { return exception; } }
		}

		/// <summary>
		/// UpdateFeedsStarted event argument class. Multiple feeds update.
		/// </summary>
		public class UpdateFeedsEventArgs: EventArgs {
			/// <summary>
			/// Initializer
			/// </summary>
			/// <param name="forced">true, if it was a forced (manually initiated) request</param>
			public UpdateFeedsEventArgs(bool forced) {
				this.forced = forced;
			}
			private bool forced;
			/// <summary>
			/// True, if it was a manually forced request
			/// </summary>
			public bool ForcedRefresh { get { return forced; } }	
		}

		/// <summary>
		/// UpdateFeedStarted event argument class. Single feed update.
		/// </summary>
		public class UpdateFeedEventArgs: UpdateFeedsEventArgs {
			/// <summary>
			/// Initializer
			/// </summary>
			/// <param name="feed">feed Uri</param>
			/// <param name="forced">true, if it was a forced (manually initiated) request</param>
			public UpdateFeedEventArgs(Uri feed, bool forced):base(forced) {
				this.feedUri = feed;
			}
			private Uri feedUri;
			/// <summary>
			/// Feed Uri.
			/// </summary>
			public Uri FeedUri { get { return feedUri; } }
		}

		/// <summary>
		/// Delegate used for UpdateFeedsStarted event.
		/// </summary>
		public delegate void UpdateFeedsStartedHandler(object sender, UpdateFeedsEventArgs e);
		/// <summary>
		/// Called if RefreshFeeds() was initiated (all feeds).
		/// </summary>
		public event UpdateFeedsStartedHandler UpdateFeedsStarted = null;

		/// <summary>
		/// Delegate used for UpdateFeedStarted event.
		/// </summary>
		public delegate void UpdateFeedStartedHandler(object sender, UpdateFeedEventArgs e);
		/// <summary>
		/// Called as each individual feed start to refresh
		/// </summary>
		public event UpdateFeedStartedHandler UpdateFeedStarted = null;

		/// <summary>
		/// Called if all async. requests are done.
		/// </summary>
		public event System.EventHandler OnAllAsyncRequestsCompleted = null;

		//Search	impl. 

		/// <summary>Signature for <see cref="NewsItemSearchResult">NewsItemSearchResult</see>  event</summary>
		public delegate void NewsItemSearchResultEventHandler(object sender, NewsItemSearchResultEventArgs e); 
		/// <summary>Signature for <see cref="FeedSearchResult">FeedSearchResult</see>  event</summary>
		public delegate void FeedSearchResultEventHandler(object sender, FeedSearchResultEventArgs e); 
		/// <summary>Signature for <see cref="SearchFinished">SearchFinished</see>  event</summary>
		public delegate void SearchFinishedEventHandler(object sender, SearchFinishedEventArgs e);

		/// <summary>Called if NewsItems are found, that match the search criteria(s)</summary>
		public event NewsItemSearchResultEventHandler NewsItemSearchResult; 
		/// <summary>Called if feedsFeed(s) are found, that match the search criteria(s)</summary>
		public event FeedSearchResultEventHandler FeedSearchResult; 
		/// <summary>Called on a search finished</summary>
		public event SearchFinishedEventHandler SearchFinished;

		/// <summary>
		/// Contains the search result, if feedsFeed's are found. Used on FeedSearchResult event.
		/// </summary>
		[ComVisible(false)]
		public class FeedSearchResultEventArgs : System.ComponentModel.CancelEventArgs {
			/// <summary>
			/// Initializer
			/// </summary>
			/// <param name="f">feedsFeed</param>
			/// <param name="tag">object, used by the caller only</param>
			/// <param name="cancel">true, if the search request should be cancelled</param>
			public FeedSearchResultEventArgs (
				feedsFeed f, object tag, bool cancel):base(cancel) {
				this.Feed = f; this.Tag = tag;
			}
			/// <summary>
			/// feedsFeed.
			/// </summary>
			public feedsFeed Feed;
			/// <summary>
			/// Object used by the caller only
			/// </summary>
			public object Tag;
		}

		/// <summary>
		/// Contains the search result, if NewsItem's are found. Used on NewsItemSearchResult event.
		/// </summary>
		[ComVisible(false)]
		public class NewsItemSearchResultEventArgs: System.ComponentModel.CancelEventArgs {
			/// <summary>
			/// Initializer
			/// </summary>
			/// <param name="items">ArrayList of NewsItems</param>
			/// <param name="tag">Object used by caller</param>
			/// <param name="cancel"></param>
			public NewsItemSearchResultEventArgs(
				ArrayList items, object tag, bool cancel):base(cancel) {
				this.NewsItems = items;
				this.Tag = tag;
			}
			/// <summary>
			/// NewsItem list
			/// </summary>
			public ArrayList NewsItems;
			/// <summary>
			/// Object used by caller
			/// </summary>
			public object Tag;
		}

		/// <summary>
		/// Provide informations about a finished search. Used on SearchFinished event.
		/// </summary>
		public class SearchFinishedEventArgs : EventArgs {
			/// <summary>
			/// Initializer
			/// </summary>
			/// <param name="tag">Object used by caller</param>
			/// <param name="matchingFeedsCount">integer stores the count of matching feeds</param>
			/// <param name="matchingItemsCount">integer stores the count of matching NewsItem's (over all feeds)</param>
			public SearchFinishedEventArgs (
				object tag, FeedInfoList matchingFeeds, int matchingFeedsCount, int matchingItemsCount):base() {
				this.MatchingFeedsCount= matchingFeedsCount;
				this.MatchingItemsCount= matchingItemsCount;
				this.MatchingFeeds = matchingFeeds;
				this.Tag = tag;				
			}
			/// <summary></summary>
			public int MatchingFeedsCount;
			/// <summary></summary>
			public int MatchingItemsCount;
			/// <summary></summary>
			public object Tag;
			/// <summary></summary>
			public FeedInfoList MatchingFeeds;
		}

		#endregion

		private const int maxItemsPerSearchResult = 10;


		private ArrayList SearchNewsItemsHelper(ArrayList prevMatchItems, SearchCriteriaCollection criteria, FeedDetailsInternal fi, FeedInfo fiMatchedItems,  ref int itemmatches, ref int feedmatches, object tag){
		  
			ArrayList matchItems = new ArrayList(maxItemsPerSearchResult);
			matchItems.AddRange(prevMatchItems); 
			bool cancel = false; 
			bool feedmatch = false; 
		  
			foreach(NewsItem item in fi.ItemsList){
				if(criteria.Match(item)){
					//_log.Info("MATCH FOUND: " + item.Title);  
					feedmatch = true; 
					matchItems.Add(item); 
					fiMatchedItems.ItemsList.Add(item);
					itemmatches++;
					if ((itemmatches % 50) == 0) { //Caller return results On the last feed we found results 
						cancel = RaiseNewsItemSearchResultEvent(matchItems, tag);
						matchItems.Clear();
					}
					if (cancel) throw new InvalidOperationException("SEARCH CANCELLED");
				}
			}//foreach(NewsItem...)

			if(feedmatch) feedmatches++; 

			return matchItems; 
		}

		/// <summary>
		/// Search for NewsItems, that match a provided criteria collection within a optional search scope.
		/// </summary>
		/// <param name="criteria">SearchCriteriaCollection containing the defined search criteria</param>
		/// <param name="scope">Search scope: an array of feedsFeed</param>
		/// <param name="tag">optional object to be used by the caller to identify this search</param>
		public void SearchNewsItems(SearchCriteriaCollection criteria, feedsFeed[] scope, object tag) {
			// if scope is an empty array: search all, else search only in spec. feeds
			int feedmatches = 0;
			int itemmatches = 0;
			int feedcounter = 0;

			ArrayList unreturnedMatchItems = new ArrayList(); 
			FeedDetailsInternal[] feedInfos;
			FeedInfoList fiList = new FeedInfoList(String.Empty); 			
		  
			try{

				if(scope.Length == 0){
					// we search a copy of the current content to prevent the lock(itemsTable)
					// while we do the more time consuming search ops. New received items are
					// automatically recognized to be searched as they are float into the system.
					lock(itemsTable) { 
						feedInfos = new FeedDetailsInternal[itemsTable.Count];
						itemsTable.Values.CopyTo(feedInfos, 0);
					}
					foreach(FeedDetailsInternal fi in feedInfos){
		
						FeedInfo fiClone = (FeedInfo) fi.Clone();
						fiClone.ItemsList.Clear(); 

						unreturnedMatchItems = SearchNewsItemsHelper(unreturnedMatchItems, criteria, fi, fiClone, ref itemmatches, ref feedmatches, tag); 				  
						feedcounter++;

						if ((feedcounter % 5) == 0) {	// to shorten search if user want to cancel. Above modulo will only stop if it founds at least 100 matches...
							bool cancel = RaiseNewsItemSearchResultEvent(unreturnedMatchItems, tag);
							unreturnedMatchItems.Clear();
							if (cancel) 
								break;
						}

						if(fiClone.ItemsList.Count != 0){
							fiList.Add(fiClone); 
						}

					}//foreach(FeedInfo...)

				}else{
		  
					lock(itemsTable) { 
						feedInfos = new FeedDetailsInternal[scope.Length];
						for (int i = 0; i < scope.Length; i++){
							feedInfos[i] = (FeedDetailsInternal) itemsTable[scope[i].link];
						}
					}
		  
					foreach(FeedDetailsInternal fi in feedInfos){
						if(fi != null){
							
							FeedInfo fiClone = (FeedInfo) fi.Clone();
							fiClone.ItemsList.Clear();

							unreturnedMatchItems = SearchNewsItemsHelper(unreturnedMatchItems, criteria, fi, fiClone, ref itemmatches, ref feedmatches, tag); 
							feedcounter++;

							if ((feedcounter % 5) == 0) {	// to shorten search if user want to cancel. Above modulo will only stop if it founds at least 100 matches...
								bool cancel = RaiseNewsItemSearchResultEvent(unreturnedMatchItems, tag);
								unreturnedMatchItems.Clear();
								if (cancel) 
									break;
							}

							if(fiClone.ItemsList.Count != 0){
								fiList.Add(fiClone); 
							}

						}
					}

				}
			
				if(unreturnedMatchItems.Count > 0){
					RaiseNewsItemSearchResultEvent(unreturnedMatchItems, tag);
				}

			}catch(InvalidOperationException ioe){// New feeds added to FeedsTable from another thread  
				_log.Error("SearchNewsItems()", ioe);
			} 
	
			RaiseSearchFinishedEvent(tag, fiList, feedmatches, itemmatches); 
		}

		/// <summary>
		/// Initiate a remote (web) search using the engine incl. search expression specified
		/// by searchFeedUrl. We assume, the specified Url will return a RSS feed.
		/// This can be used e.g. to get a RSS search result from feedster.
		/// </summary>
		/// <param name="searchFeedUrl">Complete Url of the search engine incl. search expression</param>
		/// <param name="tag">optional, can be used by the caller</param>
		public void SearchRemoteFeed(string searchFeedUrl, object tag) {
			int feedmatches = 0;
			int itemmatches = 0;

			ArrayList unreturnedMatchItems = this.GetItemsForFeed(searchFeedUrl); 
			RaiseNewsItemSearchResultEvent(unreturnedMatchItems, tag);
			feedmatches = 1;
			itemmatches = unreturnedMatchItems.Count;
			FeedInfo fi = new FeedInfo(String.Empty, unreturnedMatchItems, String.Empty, String.Empty, String.Empty, new Hashtable()); 			
			FeedInfoList fil = new FeedInfoList(String.Empty); 
			fil.Add(fi);
			RaiseSearchFinishedEvent(tag, fil, feedmatches, itemmatches); 

		}

		/// <summary>
		/// [To be provided]
		/// </summary>
		/// <param name="criteria"></param>
		/// <param name="scope"></param>
		/// <param name="tag"></param>
		public void SearchFeeds(SearchCriteriaCollection criteria, feedsFeed[] scope, object tag) {
			// if scope is an empty array: search all, else search only in spec. feeds
			// pseudo code:
			/* int matches = 0;
			foreach (feedsFeed f in _feedsTable) {
				if (criteria.Match(f)) {
					matches++;
					if (RaiseFeedSearchResultEvent(f, tag))
					  break;
				}
			}
			RaiseSearchFinishedEvent(tag, matches, 0); */

			throw new NotSupportedException(); 
		}
	  
		private bool RaiseNewsItemSearchResultEvent(ArrayList matchItems, object tag) {
			try {
				if (NewsItemSearchResult != null) {
					NewsItemSearchResultEventArgs ea = new NewsItemSearchResultEventArgs(matchItems, tag, false);
					NewsItemSearchResult(this, ea);
					return ea.Cancel;
				}
			} catch {}
			return false;
		}
		private bool RaiseFeedSearchResultEvent(feedsFeed f, object tag) {
			try {
				if (FeedSearchResult != null) {
					FeedSearchResultEventArgs ea = new FeedSearchResultEventArgs(f, tag, false);
					FeedSearchResult(this, ea);
					return ea.Cancel;
				}
			} catch {}
			return false;
		}
		private void RaiseSearchFinishedEvent(object tag, FeedInfoList matchingFeeds, int matchingFeedsCount, int matchingItemsCount) {
			try {
				if (SearchFinished != null) {
					SearchFinished(this, new SearchFinishedEventArgs(tag, matchingFeeds, matchingFeedsCount, matchingItemsCount ));
				}
			} catch (Exception e) { 
				_log.Error("SearchFinished() event code raises exception",e);
			}
		}


		/// <summary>
		/// The Application Name or ID that uses the component. This will be used to 
		/// initialize the user path to store the feeds file and cached items.
		/// </summary>
		private string applicationName = "NewsComponents";


		/// <summary>
		/// Accesses the list of user specified layouts (currently listview only) 
		/// </summary>
		public FeedColumnLayoutCollection ColumnLayouts{ 
		
			get { 
				
				if(layouts== null){				
					layouts = new FeedColumnLayoutCollection();
				}							
				
				return layouts;
			}			
		
		}


		/// <summary>
		/// Accesses the list of user specified categories used for organizing 
		/// feeds. 
		/// </summary>
		public CategoriesCollection Categories{ 
		
			get { 
				
				if(categories== null){				
					categories = new CategoriesCollection();
				}							
				
				return categories;
			}			
		
		}

		/// <summary>
		/// Accesses the table of RSS feed objects. 
		/// </summary>
		/// <exception cref="InvalidOperationException">If some error occurs on converting 
		/// XML feed list to feed table</exception>

		public FeedsCollection FeedsTable{
		
			//		[MethodImpl(MethodImplOptions.Synchronized)]
			get{
				if(!validationErrorOccured){ 
					return _feedsTable; 										
				}else {
					return null;
				}
			}
		
		}

		/// <summary>
		/// Accesses the list of NntpServerDefinition objects 
		/// Keys are the account name(s) - friendly names for the news server def.:
		/// NewsServerDefinition.Name's
		/// </summary>
		public IDictionary NntpServers { 
		
			[DebuggerStepThrough()]
			get { 
				
				if(this.nntpServers== null){				
					this.nntpServers = new ListDictionary();
				}							
				
				return this.nntpServers;
			}			
		
		}

		/// <summary>
		/// Accesses the list of UserIdentity objects.
		/// Keys are the UserIdentity.Name's
		/// </summary>
		public IDictionary UserIdentity { 
		
			[DebuggerStepThrough()]
			get { 
				
				if(this.identities== null){				
					this.identities = new ListDictionary();
				}							
				
				return this.identities;
			}			
		
		}

		/// <summary>
		/// How often feeds are refreshed by default if no specific rate specified by the feed. 
		/// The value is specified in milliseconds. 
		/// </summary>
		/// <remarks>By default this value is set to one hour. </remarks>
		private int refreshrate = 60 * 60 * 1000; 

		/// <summary>
		///  How often feeds are refreshed by default if no specific rate specified by the feed. 
		/// </summary>
		/// <remarks>If set to a negative value then the old value remains. Setting the 
		/// value to zero means feeds are no longer updated.</remarks>
		public int RefreshRate {
		
			set {
				if(value >= 0){
					this.refreshrate = value; 
				}
			}

			get { return  refreshrate; } 

		}

		///<summary>
		///Internal flag used to track whether the XML in the feed list validated against the schema. 
		///</summary>
		private static bool  validationErrorOccured = false; 

		/// <summary>
		/// The schema for the RSS feed list format
		/// </summary>
		private XmlSchema feedsSchema = null; 

		/// <summary>
		/// Boolean flag indicates whether the feeds list was loaded 
		/// successfully during the last call to LoadFeedlist()
		/// </summary>
		public bool FeedsListOK{		
			get { return !validationErrorOccured; }
		}
		
		///<summary>Loads the schema for a feedlist into an XmlSchema object. 
		///<seealso cref="feedsSchema"/></summary>		
		private void LoadFeedlistSchema(){
		
			using(Stream xsdStream = Resource.Manager.GetStream("Resources.feedListSchema.xsd")){
				feedsSchema = XmlSchema.Read(xsdStream, null); 
			}
			
		}


		/// <summary>
		/// Loads the RSS feedlist from the given URL and validates it against the schema. 
		/// </summary>
		/// <param name="feedListUrl">The URL of the feedlist</param>
		/// <param name="veh">The event handler that should be invoked on the client if validation errors occur</param>
		/// <exception cref="XmlException">XmlException thrown if XML is not well-formed</exception>
		public void LoadFeedlist(string feedListUrl, ValidationEventHandler veh){
			/* LoadFeedlist(AsyncWebRequest.GetSyncResponseStream(feedListUrl, null, this.UserAgent, this.Proxy), veh); */ 

			IFeeds fs = new FeedsClass();
			IFeedFolder root = fs.Subscriptions;

			if(root.Feeds.Count > 0){				
				foreach (IFeed f in root.Feeds){
					this.AddFeed(f, null, null); 		
				}
			}

			if(root.Subfolders.Count > 0){
				foreach(IFeedFolder f in root.Subfolders){
					this.AddFolder(f, null, null, null); 	
				}
			}
						
		}

		/// <summary>
		/// Grabs the common feedlist and populates the input collections
		/// </summary>
		/// <param name="f"></param>
		/// <param name="c"></param>
		public void GetCurrentCommonFeedList(FeedsCollection feedList, CategoriesCollection catList){
		
			IFeeds fs = new FeedsClass();
			IFeedFolder root = fs.Subscriptions;

			if(root.Feeds.Count > 0){				
				foreach (IFeed f in root.Feeds){
					this.AddFeed(f, null, feedList); 		
				}
			}

			if(root.Subfolders.Count > 0){
				foreach(IFeedFolder f in root.Subfolders){
					this.AddFolder(f, null, feedList, catList); 	
				}
			}
			

		}

		/// <summary>
		/// Adds an IFeed instance to the subscription list
		/// </summary>
		/// <param name="feed">The feed to add</param>
		/// <param name="category">it's category</param>
		public void AddFeed(IFeed feed, string category, FeedsCollection feedList){

			feedList = (feedList == null ? _feedsTable : feedList); 

			feedsFeed f = new feedsFeed(); 
			f.link   = feed.Url; 
			f.title  = feed.Name;
			f.category = category; 

			if(feedList.Contains(f.link) == false){
				feedList.Add(f.link, f); 							 
			}		
		}

		/// <summary>
		/// Adds an IFeed instance to the common feed list
		/// </summary>
		/// <param name="feed">The feed to add</param>
		/// <param name="category">it's category</param>
		public void AddFeed2CommonFeedList(feedsFeed feed, string category){

			IFeeds fs = new FeedsClass();
			IFeedFolder folder = fs.Subscriptions;

			if(category != null){
				string[] categoryPath = category.Split(new char[]{'\\'}); 

				foreach(string c in categoryPath){
					folder = folder.GetSubfolder(c); 
				}
			}

			folder.CreateFeed(feed.title, feed.link); 
		}

		/// Adds an IFeed instance to the common feed list
		/// </summary>
		/// <param name="feed">The feed to add</param>
		/// <param name="category">it's category</param>
		public void DeleteFeedFromCommonFeedList(feedsFeed feed, string category){

			IFeeds fs = new FeedsClass();
			IFeedFolder folder = fs.Subscriptions;

			if(category != null){
				string[] categoryPath = category.Split(new char[]{'\\'}); 

				foreach(string c in categoryPath){
					folder = folder.GetSubfolder(c); 
				}
			}

			folder.GetFeed(feed.title).Delete(); 
		}

		/// <summary>
		/// Add a folder to the feedlist 
		/// </summary>
		/// <param name="folder"></param>
		/// <param name="category">The path to the folder</param>
		public void AddFolder(IFeedFolder folder, string path, FeedsCollection feedList, CategoriesCollection catList){

			feedList = (feedList == null ? _feedsTable : feedList); 
			catList = (catList == null ? this.categories : catList); 

			string category = (path == null ? folder.Name : path + "\\" + folder.Name);

			if(folder.Feeds.Count > 0){				
				
				foreach (IFeed f in folder.Feeds){
					this.AddFeed(f, category, feedList); 		
				}

				if(!catList.ContainsKey(category)){					
					catList.Add(category); 
				}
			}

			if(folder.Subfolders.Count > 0){
				foreach(IFeedFolder f in folder.Subfolders){
					this.AddFolder(f, category, feedList, catList); 	
				}
			}
		}




		/// <summary>
		/// Loads the RSS feedlist from the given URL and validates it against the schema. 
		/// </summary>
		/// <param name="xmlStream">The XML Stream of a feedlist to load</param>
		/// <param name="veh">The event handler that should be invoked on the client if validation errors occur</param>
		/// <exception cref="XmlException">XmlException thrown if XML is not well-formed</exception>
		public void LoadFeedlist(Stream xmlStream, ValidationEventHandler veh){

			XmlDocument doc = new XmlDocument(); 
			XmlParserContext context = new XmlParserContext(null, new RssBanditXmlNamespaceResolver(), null, XmlSpace.None);
			XmlValidatingReader vr = new RssBanditXmlValidatingReader(xmlStream, XmlNodeType.Document, context);
			vr.Schemas.Add(feedsSchema); 
			vr.ValidationType = ValidationType.Schema;
	  

			//specify validation event handler passed by caller and the one we use 
			//internally to track state 
			vr.ValidationEventHandler += veh;
			vr.ValidationEventHandler += new ValidationEventHandler(ValidationCallbackOne);
			validationErrorOccured = false; 

			doc.Load(vr); 
			vr.Close(); 

			if(!validationErrorOccured){

				//convert XML to objects 
				XmlNodeReader reader = new XmlNodeReader(doc);		
				XmlSerializer serializer = new XmlSerializer(typeof(NewsComponents.Feed.feeds));
				feeds myFeeds = (NewsComponents.Feed.feeds)serializer.Deserialize(reader); 
				reader.Close(); 				
				
				//copy feeds over if we are importing a new feed  
				
				if(myFeeds.feed != null){
					foreach(feedsFeed f in myFeeds.feed){						
						if(_feedsTable.Contains(f.link) == false){
							_feedsTable.Add(f.link, f); 							 
						}						
					}
				}

				//copy over category info if we are importing a new feed
				if(myFeeds.categories != null){
					foreach(category cat in myFeeds.categories){
						string cat_trimmed = cat.Value.Trim();
						if(!this.categories.ContainsKey(cat_trimmed)){
							cat.Value = cat_trimmed;
							this.categories.Add(cat_trimmed, cat); 
						}
					}
				}		
		
				//This happens if for some reason the category of a feed didn't end up 
				//in the categories collection during the last save of the feedlist. 
				if(categoryMismatch && (myFeeds.feed != null)){									

					foreach(feedsFeed f in myFeeds.feed){	
						if(f.category != null){								
							string cat_trimmed = f.category = f.category.Trim();								
								
							if(!this.categories.ContainsKey(cat_trimmed)){									
								this.categories.Add(cat_trimmed); 
							}
						}
					}					
					
					categoryMismatch = false; 
				}

				//copy over layout info if we are importing a new feed
				if(myFeeds.listviewLayouts != null){
					foreach(listviewLayout layout in myFeeds.listviewLayouts){
						string layout_trimmed = layout.ID.Trim();
						if(!this.layouts.ContainsKey(layout_trimmed)){
							this.layouts.Add(layout_trimmed,  layout.FeedColumnLayout); 
						}
					}
				}

				//copy nntp-server defs. over if we are importing  
				if(myFeeds.nntpservers != null){
					foreach(NntpServerDefinition sd in myFeeds.nntpservers){						
						if(nntpServers.Contains(sd.Name) == false){
							nntpServers.Add(sd.Name, sd); 							 
						}						
					}
				}
				
				//copy user-identities over if we are importing  
				if(myFeeds.identities != null){
					foreach(UserIdentity ui in myFeeds.identities){						
						if(identities.Contains(ui.Name) == false){
							identities.Add(ui.Name, ui); 							 
						}						
					}
				}

				//if refresh rate in imported feed then use that
				if( myFeeds.refreshrateSpecified){
					this.refreshrate = myFeeds.refreshrate; 					
				}

				//if stylesheet specified in imported feed then use that
				if(!StringHelper.EmptyOrNull(myFeeds.stylesheet)){
					this.stylesheet = myFeeds.stylesheet; 					
				}

				//if download enclosures specified in imported feed then use that
				if(myFeeds.downloadenclosuresSpecified){
					this.downloadenclosures = myFeeds.downloadenclosures; 					
				}

				//if marking items as read on exit specified in imported feed then use that
				if(myFeeds.markitemsreadonexitSpecified){
					this.markitemsreadonexit = myFeeds.markitemsreadonexit; 					
				}

				//if enclosure folder specified in imported feed then use that
				if(!StringHelper.EmptyOrNull(myFeeds.enclosurefolder)){
					this.enclosurefolder = myFeeds.enclosurefolder; 					
				}

				//if listview layout specified in imported feed then use that
				if(!StringHelper.EmptyOrNull(myFeeds.listviewlayout)){
					this.listviewlayout = myFeeds.listviewlayout;
				}

				//if max item age in imported feed then use that
				try{

					if(!StringHelper.EmptyOrNull(myFeeds.maxitemage)){
						this.maxitemage = XmlConvert.ToTimeSpan(myFeeds.maxitemage); 					
					}

				}catch(FormatException fe){
					Trace.WriteLine("Error occured while parsing maximum item age from feed list: " + fe.ToString(), "RssParser"); 	
				}

			}
		}
		    


		/// <summary>
		/// Specifies that a feed should be ignored when RefreshFeeds() is called by 
		/// setting its refresh rate to zero. The feed can still be refreshed manually by 
		/// calling GetItemsForFeed(). 
		/// </summary>
		/// <remarks>If no feed with that URL exists then nothing is done.</remarks>
		/// <param name="feedUrl">The URL of the feed to ignore. </param>
		public void DisableFeed(string feedUrl){

			if(!FeedsTable.ContainsKey(feedUrl)){
				return; 
			}
		
			feedsFeed f = FeedsTable[feedUrl];
			f.refreshrate = 0; 
			f.refreshrateSpecified = true; 
		
		}


		/// <summary>
		/// Removes all information related to a feed from the NewsHandler. 
		/// </summary>
		/// <remarks>If the item doesn't exist in the NewsHandler then nothing is done</remarks>
		/// <param name="item">the item to delete</param>
		public void DeleteItem(NewsItem item){

			if(item.Feed != null && !StringHelper.EmptyOrNull( item.Feed.link )){
				
				/* 
				 * There is no attempt to load feed from disk because it is 
				 * assumed that for this to be called the feed was already loaded
				 * since we have an item from the feed */
				
				FeedInfo fi = itemsTable[item.Feed.link] as FeedInfo;
				
				if(fi != null){
					lock(fi.itemsList){
						item.Feed.deletedstories.Add(item.Id); 				
						fi.itemsList.Remove(item); 
					}
				}//if(fi != null)
			}//if(item.Feed != null) 
		
		}

		/// <summary>
		/// Deletes all the items in a feed
		/// </summary>
		/// <param name="feed">the feed</param>
		public void DeleteAllItemsInFeed(feedsFeed feed){


			if (feed != null && !StringHelper.EmptyOrNull( feed.link ) && FeedsTable.ContainsKey(feed.link)) {
			  
				FeedInfo fi = itemsTable[feed.link] as FeedInfo; 

				//load feed from disk 
				if(fi == null){
					fi = (FeedInfo) this.GetFeed(feed); 
				}

				if(fi != null){
					lock(fi.itemsList){
						foreach(NewsItem item in fi.itemsList){
							feed.deletedstories.Add(item.Id); 
						}
						fi.itemsList.Clear(); 
					}					
				}//if(fi != null)		
			
			}//if (feed != null && !StringHelper.EmptyOrNull( feed.link ) && FeedsTable.ContainsKey(feed.link)) {
					
		}

		/// <summary>
		/// Deletes all items in a feed
		/// </summary>
		/// <param name="feedUrl">the URL of the feed</param>
		public void DeleteAllItemsInFeed(string feedUrl){

			if(FeedsTable.ContainsKey(feedUrl)){
				this.DeleteAllItemsInFeed(FeedsTable[feedUrl]);			
			}
		
		}

		/// <summary>
		/// Undeletes a deleted item
		/// </summary>
		/// <remarks>if the parent feed has been deleted then this does nothing</remarks>
		/// <param name="item">the utem to restore</param>
		public void RestoreDeletedItem(NewsItem item){
		
			if(item.Feed != null && !StringHelper.EmptyOrNull( item.Feed.link ) && FeedsTable.ContainsKey(item.Feed.link)){
				
				FeedInfo fi = itemsTable[item.Feed.link] as FeedInfo;

				//load feed from disk 
				if(fi == null){
					fi = (FeedInfo) this.GetFeed(item.Feed); 
				}
				
				if(fi != null){
					lock(fi.itemsList){
						item.Feed.deletedstories.Remove(item.Id); 				
						fi.itemsList.Add(item); 
					}
				}//if(fi != null)
			}//if(item.Feed != null) 
		}

		/// <summary>
		/// Undeletes all the deleted items in the list
		/// </summary>
		/// <remarks>if the parent feed has been deleted then this does nothing</remarks>
		/// <param name="deletedItems">the list of items to restore</param>
		public void RestoreDeletedItem(ArrayList deletedItems){

			foreach(NewsItem item in deletedItems){
				this.RestoreDeletedItem(item); 
			}

		}

		/// <summary>
		/// Removes all information related to a feed from the NewsHandler.   
		/// </summary>
		/// <remarks>If no feed with that URL exists then nothing is done.</remarks>
		/// <param name="feedUrl">The URL of the feed to delete. </param>
		/// <exception cref="ApplicationException">If an error occured while 
		/// attempting to delete the cached feed. Examine the InnerException property 
		/// for details</exception>
		public void DeleteFeed(string feedUrl){

			if(!FeedsTable.ContainsKey(feedUrl)){
				return; 
			}
		
			feedsFeed f = FeedsTable[feedUrl];
			FeedsTable.Remove(feedUrl); 

			if(itemsTable.Contains(feedUrl)){
				itemsTable.Remove(feedUrl); 
			}

			try{ 
				this.cacheHandler.RemoveFeed(f); 
				
			}catch(Exception e){
				throw new ApplicationException(e.Message, e); 
			}
		
		}

		/// <summary>
		/// Saves the feed list to the specified stream. The feed is written in 
		/// the RSS Bandit feed file format as described in feeds.xsd
		/// </summary>
		/// <param name="feedStream">The stream to save the feed list to</param>
		public void SaveFeedList(Stream feedStream){
			this.SaveFeedList(feedStream, FeedListFormat.NewsHandler); 
		}


		/// <summary>
		/// Helper method used for constructing OPML file. It traverses down the tree on the 
		/// path defined by 'category' starting with 'startNode'. 
		/// </summary>
		/// <param name="startNode">Node to start with</param>
		/// <param name="category">A category path, e.g. 'Category1\SubCategory1'.</param>
		/// <returns>The leaf category node.</returns>
		/// <remarks>If one category in the path is not found, it will be created.</remarks>
		private XmlElement CreateCategoryHive(XmlElement startNode, string category)	{

			

			if (category == null || category.Length == 0 || startNode == null) return startNode;

			string[] catHives = category.Split(new char[]{'\\'});
			XmlElement n = null;
			bool wasNew = false;

			foreach (string catHive in catHives){

				if (!wasNew){ 
					string xpath = "child::outline[@title=" + buildXPathString(catHive) + " and (count(@*)= 1)]";				 
					n = (XmlElement) startNode.SelectSingleNode(xpath); 
				}else{
					n = null;
				}

				if (n == null) {
					
				 
					n = startNode.OwnerDocument.CreateElement("outline"); 
					n.SetAttribute("title", catHive); 
					startNode.AppendChild(n);
					wasNew = true;	// shorten search
				}

				startNode = n;

			}//foreach
			
			return startNode;
		}


		/// <summary>
		/// Helper function that gets the listview layout with the specified ID from the
		/// Arraylist
		/// </summary>
		/// <param name="id"></param>
		/// <param name="layouts"></param>
		/// <returns></returns>
		private static listviewLayout FindLayout(string id, ArrayList layouts){
		
			foreach(listviewLayout layout in layouts){
				if(id.Equals(layout.ID))
					return layout; 			
			}
			return null; 
		}

		/// <summary>
		/// Helper function breaks up a string containing quote characters into 
		///	a series of XPath concat() calls. 
		/// </summary>
		/// <param name="input">input string</param>
		/// <returns>broken up string</returns>
		private static string buildXPathString (string input) {
			string[] components = input.Split(new char[] { '\''});
			string result = "";
			result += "concat(''";
			for (int i = 0; i < components.Length; i++) {
				result += ", '" + components[i] + "'";
				if (i < components.Length - 1) {
					result += ", \"'\"";
				}
			}
			result += ")";
			Console.WriteLine(result);
			return result;
		}

		/// <summary>
		/// Saves the whole feed list incl. empty categories to the specified stream
		/// </summary>
		/// <param name="feedStream">The feedStream to save the feed list to</param>
		/// <param name="format">The format to save the stream as. </param>
		/// <exception cref="InvalidOperationException">If anything wrong goes on with XmlSerializer</exception>
		/// <exception cref="ArgumentNullException">If feedStream is null</exception>
		public void SaveFeedList(Stream feedStream, FeedListFormat format){
			this.SaveFeedList(feedStream, format, this._feedsTable, true);
		}

		/// <summary>
		/// Saves the provided feed list to the specified stream
		/// </summary>
		/// <param name="feedStream">The feedStream to save the feed list to</param>
		/// <param name="format">The format to save the stream as. </param>
		/// <param name="feeds">FeedsCollection containing the feeds to save. 
		/// Can contain a subset of the owned feeds collection</param>
		/// <param name="includeEmptyCategories">Set to true, if categories without a contained feed should be included</param>
		/// <exception cref="InvalidOperationException">If anything wrong goes on with XmlSerializer</exception>
		/// <exception cref="ArgumentNullException">If feedStream is null</exception>
		public void SaveFeedList(Stream feedStream, FeedListFormat format, FeedsCollection feeds, bool includeEmptyCategories){

			if (feedStream == null)
				throw new ArgumentNullException("feedStream");

			if(format.Equals(FeedListFormat.OPML)){

				XmlDocument opmlDoc = new XmlDocument(); 
				opmlDoc.LoadXml("<opml version='1.0'><head /><body /></opml>"); 

				Hashtable categoryTable = new Hashtable(categories.Count); 
				//CategoriesCollection categoryList = (CategoriesCollection)categories.Clone();
			
				foreach(feedsFeed f in feeds.Values) {

					XmlElement outline = opmlDoc.CreateElement("outline"); 
					outline.SetAttribute("title", f.title); 
					outline.SetAttribute("xmlUrl", f.link); 
			  
					FeedInfo fi  = (FeedInfo) itemsTable[f.link];
			  
					if(fi != null){
						outline.SetAttribute("htmlUrl", fi.Link); 
						outline.SetAttribute("description", fi.Description); 
					}


					string category = (f.category == null ? String.Empty: f.category);
				
					XmlElement catnode;
					if (categoryTable.ContainsKey(category))
						catnode = (XmlElement)categoryTable[category];
					else {
						catnode = CreateCategoryHive((XmlElement) opmlDoc.DocumentElement.ChildNodes[1], category);
						categoryTable.Add(category, catnode); 
					}

					catnode.AppendChild(outline);			 			
				}

				if (includeEmptyCategories) {
					//add categories, we don't already have
					foreach(string category in this.categories.Keys) {
						CreateCategoryHive((XmlElement) opmlDoc.DocumentElement.ChildNodes[1], category);
					}
				}

				XmlTextWriter opmlWriter = new XmlTextWriter(feedStream,System.Text.Encoding.UTF8); 
				opmlWriter.Formatting    = Formatting.Indented; 
				opmlDoc.Save(opmlWriter); 

			}else if(format.Equals(FeedListFormat.NewsHandler)|| format.Equals(FeedListFormat.NewsHandlerLite)){ 

				XmlSerializer serializer              = new XmlSerializer(typeof(NewsComponents.Feed.feeds));
				feeds feedlist = new feeds(); 

				if(feeds != null){
					
					feedlist.refreshrate = this.refreshrate;
					feedlist.refreshrateSpecified = true; 

					feedlist.downloadenclosures = this.downloadenclosures;
					feedlist.downloadenclosuresSpecified = true; 

					feedlist.maxitemage = XmlConvert.ToString(this.maxitemage);
					feedlist.listviewlayout = this.listviewlayout;
					feedlist.stylesheet = this.stylesheet;
					feedlist.enclosurefolder = this.enclosurefolder;
					feedlist.markitemsreadonexit = this.markitemsreadonexit;
					feedlist.markitemsreadonexitSpecified = true; 
				
					foreach(feedsFeed f in feeds.Values){
						feedlist.feed.Add(f); 

						if(itemsTable.Contains(f.link)){
									
							ArrayList items = ((FeedInfo)itemsTable[f.link]).itemsList;
							 
							// Taken out because it meant that when we sync we lose information
							// about stuff we've read from other instances of RSS Bandit synced from 
							// if its cache is older than this one. 
							/* f.storiesrecentlyviewed.Clear(); */
							 

							if(!format.Equals(FeedListFormat.NewsHandlerLite)){
								foreach(NewsItem ri in items){
									if(ri.BeenRead && !f.storiesrecentlyviewed.Contains(ri.Id)){ //THIS MAY BE SLOW
										f.storiesrecentlyviewed.Add(ri.Id); 	 
									}
								}
							}//foreach
						
						}//if
					}//foreach

				}//if(feeds != null) 


				ArrayList c =  new ArrayList(this.categories.Count); 
				/* sometimes we get nulls in the arraylist, remove them */
				for(int i=0; i < this.categories.Count; i++){
					CategoryEntry s = this.categories[i]; 
					if(s.Value.Value == null){
						this.categories.RemoveAt(i); 
						i--;			
					} else {
						c.Add(s.Value);
					}
				}

				//we don't want to write out empty <categories /> into the schema. 				
				if((c== null) || (c.Count == 0)){
					feedlist.categories = null; 
				}else{
					feedlist.categories = c; 
				}
				

				c =  new ArrayList(this.layouts.Count); 
				/* sometimes we get nulls in the arraylist, remove them */
				for(int i=0; i < this.layouts.Count; i++){
					FeedColumnLayoutEntry s = this.layouts[i]; 
					if(s.Value == null){
						this.layouts.RemoveAt(i); 
						i--;			
					} else {
						c.Add(new listviewLayout(s.Key, s.Value));
					}
				}

				//we don't want to write out empty <listview-layouts /> into the schema. 				
				if((c== null) || (c.Count == 0)){
					feedlist.listviewLayouts = null; 
				}else{
					feedlist.listviewLayouts = c; 
				}

				c =  new ArrayList(this.nntpServers.Values); 

				//we don't want to write out empty <nntp-servers /> into the schema. 				
				if((c== null) || (c.Count == 0)){
					feedlist.nntpservers = null; 
				}else{
					feedlist.nntpservers = c; 
				}

				c =  new ArrayList(this.identities.Values); 

				//we don't want to write out empty <user-identities /> into the schema. 				
				if((c== null) || (c.Count == 0)){
					feedlist.identities = null; 
				}else{
					feedlist.identities = c; 
				}


				TextWriter writer = new StreamWriter(feedStream);
				serializer.Serialize(writer, feedlist);
				//writer.Close(); DON'T CLOSE STREAM
								
			}
		}


		/// <summary>
		/// Used to clear the information about when last the feed was downloaded. This allows
		/// us to refetch the feed without sending If-Modified-Since or If-None-Match header
		/// information and thus force a download. 
		/// </summary>
		/// <param name="f">The feed to mark for download</param>
		public void MarkForDownload(feedsFeed f){
			f.etag = null; 
			f.lastretrievedSpecified = false; 
			f.lastretrieved = DateTime.MinValue;		
			f.lastmodified = DateTime.MinValue;
		}
		

		/// <summary>
		/// Used to clear the information about when last the feeds downloaded. This allows
		/// us to refetch the feed without sending If-Modified-Since or If-None-Match header
		/// information and thus force a download. 
		/// </summary>		
		public void MarkForDownload(){
			if(this.FeedsListOK){
				foreach(feedsFeed f in this.FeedsTable.Values){
					this.MarkForDownload(f);
				}
			}
		}

		/// <summary>
		/// Removes all the RSS items cached in-memory and on-disk for all feeds. 
		/// </summary>
		public void ClearItemsCache(){
			this.itemsTable.Clear(); 
			this.cacheHandler.ClearCache(); 
		}		


		/// <summary>
		/// Marks all items stored in the internal cache of RSS items as read.
		/// </summary>
		public void MarkAllCachedItemsAsRead(){
		
			foreach(feedsFeed f in this.FeedsTable.Values) {
				this.MarkAllCachedItemsAsRead(f); 			
			}

		}


		/// <summary>
		/// Marks all items stored in the internal cache of RSS items as read
		/// for a particular category.
		/// </summary>
		/// <param name="category">The category the feeds belong to</param>
		public void MarkAllCachedCategoryItemsAsRead(string category){
		
			if(FeedsListOK){

				if(this.categories.ContainsKey(category)) {

					foreach(feedsFeed f in this.FeedsTable.Values) {
					
						if((f.category!= null) && f.category.Equals(category)) {
							this.MarkAllCachedItemsAsRead(f); 			
						}
					}
				}
				else if (category == null /* the default category */) {
					foreach(feedsFeed f in this.FeedsTable.Values) {
					
						if (f.category== null) {
							this.MarkAllCachedItemsAsRead(f); 			
						}
					}
				}

			}//if(FeedsListOK)
		}
	
		/// <summary>
		/// Marks all items stored in the internal cache of RSS items as read
		/// for a particular feed.
		/// </summary>
		/// <param name="feedUrl">The URL of the RSS feed</param>
		public void MarkAllCachedItemsAsRead(string feedUrl){
		
			if (!StringHelper.EmptyOrNull( feedUrl )) {
			
				feedsFeed feed = this.FeedsTable[feedUrl] as feedsFeed;
				if (feed != null){
					this.MarkAllCachedItemsAsRead(feed);
				}
			}
		}

		/// <summary>
		/// Marks all items stored in the internal cache of RSS items as read
		/// for a particular feed.
		/// </summary>
		/// <param name="feed">The RSS feed</param>
		public void MarkAllCachedItemsAsRead(feedsFeed feed){
		
			if (feed != null && !StringHelper.EmptyOrNull( feed.link )) {
			  
				FeedInfo fi = itemsTable[feed.link] as FeedInfo; 

				if(fi != null){
					foreach(NewsItem ri in fi.itemsList){
						ri.BeenRead = true; 
					}
				}

				feed.containsNewMessages = false;
			}
		}

		/// <summary>
		/// Do apply any internal work needed after some feed or feed item properties 
		/// or content was changed outside.
		/// </summary>
		/// <param name="feedUrl">The feed to update</param>
		/// <exception cref="ArgumentNullException">If feedUrl is null or empty</exception>
		public void ApplyFeedModifications(string feedUrl) {
		  
			if (feedUrl == null || feedUrl.Length == 0)
				throw new ArgumentNullException("feedUrl");

			FeedDetailsInternal fi = null;
			feedsFeed f = null;
			if(itemsTable.Contains(feedUrl)){
				fi = (FeedDetailsInternal) itemsTable[feedUrl]; 			
			}
			if(this.FeedsTable.Contains(feedUrl)){
				f = this.FeedsTable[feedUrl]; 
			}
			if (fi != null && f != null) {
				try {
					f.cacheurl = this.SaveFeed(f);
				} catch (Exception ex){
					Trace.WriteLineIf(TraceMode, "ApplyFeedModifications() cause exception while saving feed '"+ feedUrl +" 'to cache:"+ex.Message);
				}
			}

		}


		/// <summary>
		/// Tests whether a particular propery value is set
		/// </summary>
		/// <param name="value">the value to test</param>
		/// <param name="owner">the object which the property comes from</param>
		/// <returns>true if it is set and false otherwise</returns>
		private static bool IsPropertyValueSet(object value, string propertyName, object owner){
		
			//TODO: Make this code more efficient

			if(value == null){
			
				return false; 
			
			}else if(value is string){		
	
				bool isSet = !StringHelper.EmptyOrNull((string) value); 

				if(propertyName.Equals("maxitemage") && isSet){
					isSet = !value.Equals(XmlConvert.ToString(TimeSpan.MaxValue));
				}

				return isSet; 
			}else{
			
				return (bool) owner.GetType().GetField(propertyName + "Specified").GetValue(owner);  
			}
		}
		

		/// <summary>
		/// Gets the value of a feed's property. This does not inherit the properties of parent
		/// categories. 
		/// </summary>
		/// <param name="feedUrl">the feed URL</param>
		/// <param name="propertyName">the name of the property</param>		
		/// <returns>the value of the property</returns>
		private object GetFeedProperty(string feedUrl, string propertyName){
		
			return this.GetFeedProperty(feedUrl, propertyName, false);
		}

		/// <summary>
		/// Gets the value of a feed's property
		/// </summary>
		/// <param name="feedUrl">the feed URL</param>
		/// <param name="propertyName">the name of the property</param>
		/// <param name="inheritCategory">indicates whether the settings from the parent category should be inherited or not</param>
		/// <returns>the value of the property</returns>
		private object GetFeedProperty(string feedUrl, string propertyName, bool inheritCategory){
		
			//TODO: Make this code more efficient

			object value = this.GetType().GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this); 			

			if(_feedsTable.ContainsKey(feedUrl)){

				feedsFeed f = this.FeedsTable[feedUrl]; 
				object f_value = f.GetType().GetField(propertyName).GetValue(f);

				if(IsPropertyValueSet(f_value, propertyName, f)){			
				
					if(propertyName.Equals("maxitemage")){
						f_value = XmlConvert.ToTimeSpan((string)f_value);
					}
												   
					value = f_value; 

				}else if(inheritCategory && !StringHelper.EmptyOrNull(f.category)){
				
					category c = this.Categories.GetByKey(f.category);
					
					while(c != null){

						object c_value = c.GetType().GetField(propertyName).GetValue(c);
					
						if(IsPropertyValueSet(c_value, propertyName, c)){	

							if(propertyName.Equals("maxitemage")){
								c_value = XmlConvert.ToTimeSpan((string)c_value);
							}												   
							value = c_value; 							
							break; 
						}else{
							c = c.parent; 
						}
					}//while
				}//else if(!StringHelper.EmptyOrNull(f.category))

			}//if(_feedsTable.ContainsKey(feedUrl)){

			
			return value; 
		}

		/// <summary>
		/// Sets the value of a feed property.
		/// </summary>
		/// <param name="feedUrl"></param>
		/// <param name="propertyName"></param>
		/// <param name="value"></param>
		private void SetFeedProperty(string feedUrl, string propertyName, object value){

			//TODO: Make this code more efficient

			if(_feedsTable.ContainsKey(feedUrl)){
				feedsFeed f = this.FeedsTable[feedUrl]; 
				
				if(value is TimeSpan){
					value = XmlConvert.ToString((TimeSpan)value);
				}				
				f.GetType().GetField(propertyName).SetValue(f, value);
 				
				if((value != null) && !(value is string)){
					f.GetType().GetField(propertyName + "Specified").SetValue(f, true); 
				}
			}	
		
		}
		
		/// <summary>
		///  Sets the maximum amount of time an item should be kept in the 
		/// cache for a particular feed. This overrides the value of the 
		/// maxItemAge property. 
		/// </summary>
		/// <remarks>If the feed URL is not found in the FeedsTable then nothing happens</remarks>
		/// <param name="feedUrl">The feed</param>
		/// <param name="age">The maximum amount of time items should be kept for the 
		/// specified feed.</param>
		public  void SetMaxItemAge(string feedUrl, TimeSpan age){
			
			this.SetFeedProperty(feedUrl, "maxitemage", age); 								
		} 

		/// <summary>
		/// Gets the maximum amount of time an item is kept in the 
		/// cache for a particular feed. 
		/// </summary>
		/// <param name="feedUrl">The feed identifier</param>
		/// <exception cref="FormatException">if an error occurs while converting the max item age value to a TimeSpan</exception>
		public TimeSpan GetMaxItemAge(string feedUrl){			
			
			return (TimeSpan) this.GetFeedProperty(feedUrl, "maxitemage", true); 		
		} 
		

		/// <summary>
		/// Sets the refresh rate for a feed
		/// </summary>
		/// <param name="feedUrl">the URL of the feed</param>
		/// <param name="refreshRate">the new refresh rate</param>
		public void SetRefreshRate(string feedUrl, int refreshRate){
		
			this.SetFeedProperty(feedUrl, "refreshrate", refreshRate); 
		}

		/// <summary>
		/// Gets the refresh rate for a feed
		/// </summary>
		/// <param name="feedUrl">the URL of the feed</param>
		/// <returns>the refresh rate</returns>
		public int GetRefreshRate(string feedUrl){
		
			return (int) this.GetFeedProperty(feedUrl, "refreshrate", true); 
		}

		/// <summary>
		/// Sets the stylesheet for a feed
		/// </summary>
		/// <param name="feedUrl">the URL of the feed</param>
		/// <param name="stylesheet">the new stylesheet</param>
		public void SetStyleSheet(string feedUrl, string stylesheet){
		
			this.SetFeedProperty(feedUrl, "stylesheet", stylesheet); 
		}

		/// <summary>
		/// Gets the stylesheet for a feed
		/// </summary>
		/// <param name="feedUrl">the URL of the feed</param>
		/// <returns>the stylesheet</returns>
		public string GetStyleSheet(string feedUrl){
		
			return (string) this.GetFeedProperty(feedUrl, "stylesheet"); 
		}


		/// <summary>
		/// Sets the enclosure folder for a feed
		/// </summary>
		/// <param name="feedUrl">the URL of the feed</param>
		/// <param name="enclosurefolder">the new enclosure folder </param>
		public void SetEnclosureFolder(string feedUrl, string enclosurefolder){
		
			this.SetFeedProperty(feedUrl, "enclosurefolder", enclosurefolder); 
		}

		/// <summary>
		/// Gets the enclosure folder for a feed
		/// </summary>
		/// <param name="feedUrl">the URL of the feed</param>
		/// <returns>the enclosure folder</returns>
		public string GetEnclosureFolder(string feedUrl){
		
			return (string) this.GetFeedProperty(feedUrl, "enclosurefolder"); 
		}


		/// <summary>
		/// Sets the listview layout for a feed
		/// </summary>
		/// <param name="feedUrl">the URL of the feed</param>
		/// <param name="listviewlayout">the new listview layout </param>
		public void SetFeedColumnLayout(string feedUrl, string listviewlayout){
		
			this.SetFeedProperty(feedUrl, "listviewlayout", listviewlayout); 
		}

		/// <summary>
		/// Gets the listview layout for a feed
		/// </summary>
		/// <param name="feedUrl">the URL of the feed</param>
		/// <returns>the listview layout</returns>
		public string GetFeedColumnLayout(string feedUrl){
		
			return (string) this.GetFeedProperty(feedUrl, "listviewlayout"); 
		}


		/// <summary>
		/// Sets whether to mark items as read on exiting the feed in the UI
		/// </summary>
		/// <param name="feedUrl">the URL of the feed</param>
		/// <param name="markitemsreadonexit">the new value for markitemsreadonexit</param>
		public void SetMarkItemsReadOnExit(string feedUrl, bool markitemsreadonexit){
		
			this.SetFeedProperty(feedUrl, "markitemsreadonexit", markitemsreadonexit); 
		}

		/// <summary>
		/// Gets whether to mark items as read on exiting the feed in the UI
		/// </summary>
		/// <param name="feedUrl">the URL of the feed</param>
		/// <returns>whether to mark items as read on exit</returns>
		public bool GetMarkItemsReadOnExit(string feedUrl){
		
			return (bool) this.GetFeedProperty(feedUrl, "markitemsreadonexit"); 
		}

		/// <summary>
		/// Sets whether to download enclosures for this feed
		/// </summary>
		/// <param name="feedUrl">the URL of the feed</param>
		/// <param name="downloadenclosures">the new value for downloadenclosures</param>
		public void SetDownloadEnclosures(string feedUrl, bool downloadenclosures){
		
			this.SetFeedProperty(feedUrl, "downloadenclosures", downloadenclosures); 
		}

		/// <summary>
		/// Gets whether to download enclosures for this feed
		/// </summary>
		/// <param name="feedUrl">the URL of the feed</param>
		/// <returns>hether to download enclosures for this feed</returns>
		public bool GetDownloadEnclosures(string feedUrl){
		
			return (bool) this.GetFeedProperty(feedUrl, "downloadenclosures"); 
		}

		/// <summary>
		/// Gets the value of a category's property
		/// </summary>
		/// <param name="category">the category name</param>
		/// <param name="propertyName">the name of the property</param>
		/// <returns>the value of the property</returns>
		private object GetCategoryProperty(string category, string propertyName){
		
			//TODO: Make this code more efficient

			object value = this.GetType().GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this); 			

				 if(!StringHelper.EmptyOrNull(category)){
				
					category c = this.Categories.GetByKey(category);
					
					while(c != null){

						object c_value = c.GetType().GetField(propertyName).GetValue(c);
					
						if(IsPropertyValueSet(c_value, propertyName, c)){	

							if(propertyName.Equals("maxitemage")){
								c_value = XmlConvert.ToTimeSpan((string)c_value);
							}												   
							value = c_value; 							
							break; 
						}else{
							c = c.parent; 
						}
					}//while
				}//if(!StringHelper.EmptyOrNull(category))

			

			
			return value; 
		}

		/// <summary>
		/// Sets the value of a category's property.
		/// </summary>
		/// <param name="category">the category's name</param>
		/// <param name="propertyName">the name of the property</param>
		/// <param name="value">the new value</param>
		private void SetCategoryProperty(string category, string propertyName, object value){

			//TODO: Make this code more efficient

			if(!StringHelper.EmptyOrNull(category)){

				//category c = this.Categories.GetByKey(category);
				
				foreach(category c in this.Categories.Values){
									
				   	//if(c!= null){			

					if(c.Value.Equals(category) || c.Value.StartsWith(category + "\\")){
				
						if(value is TimeSpan){
							value = XmlConvert.ToString((TimeSpan)value);
						}
			
						c.GetType().GetField(propertyName).SetValue(c, value);
 				
						if((value != null) && !(value is string)){
							c.GetType().GetField(propertyName + "Specified").SetValue(c, true); 
						}

						break;
					}//if(c!= null) 
				}//foreach
		
			}//	if(!StringHelper.EmptyOrNull(category)){
		}


		/// <summary>
		///  Sets the maximum amount of time an item should be kept in the 
		/// cache for a particular category. This overrides the value of the 
		/// maxItemAge property. 
		/// </summary>
		/// <remarks>If the feed URL is not found in the FeedsTable then nothing happens</remarks>
		/// <param name="category">The feed</param>
		/// <param name="age">The maximum amount of time items should be kept for the 
		/// specified feed.</param>
		public  void SetCategoryMaxItemAge(string category, TimeSpan age){
			
			this.SetCategoryProperty(category, "maxitemage", age); 								
		} 

		/// <summary>
		/// Gets the maximum amount of time an item is kept in the 
		/// cache for a particular feed. 
		/// </summary>
		/// <param name="category">The name of the category</param>
		/// <exception cref="FormatException">if an error occurs while converting the max item age value to a TimeSpan</exception>
		public TimeSpan GetCategoryMaxItemAge(string category){			
			
			return (TimeSpan) this.GetCategoryProperty(category, "maxitemage"); 		
		} 
		

		/// <summary>
		/// Sets the refresh rate for a category
		/// </summary>
		/// <param name="category">the name of the category</param>
		/// <param name="refreshRate">the new refresh rate</param>
		public void SetCategoryRefreshRate(string category, int refreshRate){
		
			this.SetCategoryProperty(category, "refreshrate", refreshRate); 
		}

		/// <summary>
		/// Gets the refresh rate for a category
		/// </summary>
		/// <param name="category">the name of the category</param>
		/// <returns>the refresh rate</returns>
		public int GetCategoryRefreshRate(string category){
		
			return (int) this.GetCategoryProperty(category, "refreshrate"); 
		}

		/// <summary>
		/// Sets the stylesheet for a category
		/// </summary>
		/// <param name="category">the name of the category</param>
		/// <param name="stylesheet">the new stylesheet</param>
		public void SetCategoryStyleSheet(string category, string stylesheet){
		
			this.SetCategoryProperty(category, "stylesheet", stylesheet); 
		}

		/// <summary>
		/// Gets the stylesheet for a category
		/// </summary>
		/// <param name="category">the name of the category</param>
		/// <returns>the stylesheet</returns>
		public string GetCategoryStyleSheet(string category){
		
			return (string) this.GetCategoryProperty(category, "stylesheet"); 
		}


		/// <summary>
		/// Sets the enclosure folder for a category
		/// </summary>
		/// <param name="category">the name of the category</param>
		/// <param name="enclosurefolder">the new enclosure folder </param>
		public void SetCategoryEnclosureFolder(string category, string enclosurefolder){
		
			this.SetCategoryProperty(category, "enclosurefolder", enclosurefolder); 
		}

		/// <summary>
		/// Gets the enclosure folder for a category
		/// </summary>
		/// <param name="category">the name of the category</param>
		/// <returns>the enclosure folder</returns>
		public string GetCategoryEnclosureFolder(string category){
		
			return (string) this.GetCategoryProperty(category, "enclosurefolder"); 
		}


		/// <summary>
		/// Sets the listview layout for a category
		/// </summary>
		/// <param name="category">the name of the category</param>
		/// <param name="listviewlayout">the new listview layout </param>
		public void SetCategoryFeedColumnLayout(string category, string listviewlayout){
		
			this.SetCategoryProperty(category, "listviewlayout", listviewlayout); 
		}

		/// <summary>
		/// Gets the listview layout for a category
		/// </summary>
		/// <param name="category">the name of the category</param>
		/// <returns>the listview layout</returns>
		public string GetCategoryFeedColumnLayout(string category){
		
			return (string) this.GetCategoryProperty(category, "listviewlayout"); 
		}


		/// <summary>
		/// Sets whether to mark items as read on exiting the feed in the UI
		/// </summary>
		/// <param name="category">the name of the category</param>
		/// <param name="markitemsreadonexit">the new value for markitemsreadonexit</param>
		public void SetCategoryMarkItemsReadOnExit(string category, bool markitemsreadonexit){
		
			this.SetCategoryProperty(category, "markitemsreadonexit", markitemsreadonexit); 
		}

		/// <summary>
		/// Gets whether to mark items as read on exiting the feed in the UI
		/// </summary>
		/// <param name="category">the name of the category</param>
		/// <returns>whether to mark items as read on exit</returns>
		public bool GetCategoryMarkItemsReadOnExit(string category){
		
			return (bool) this.GetCategoryProperty(category, "markitemsreadonexit"); 
		}

		/// <summary>
		/// Sets whether to download enclosures for this category
		/// </summary>
		/// <param name="category">the name of the category</param>
		/// <param name="downloadenclosures">the new value for downloadenclosures</param>
		public void SetCategoryDownloadEnclosures(string category, bool downloadenclosures){
		
			this.SetCategoryProperty(category, "downloadenclosures", downloadenclosures); 
		}

		/// <summary>
		/// Gets whether to download enclosures for this category
		/// </summary>
		/// <param name="category">the name of the category</param>
		/// <returns>the refresh rate</returns>
		public bool GetCategoryDownloadEnclosures(string category){
		
			return (bool) this.GetCategoryProperty(category, "downloadenclosures"); 
		}

		/// <summary>
		/// Returns the FeedDetails of a feed.
		/// </summary>
		/// <param name="feedUrl">string feed's Url</param>
		/// <returns>FeedInfo or null, if feed was removed or parameter is invalid</returns>
		public IFeedDetails GetFeedInfo(string feedUrl) {
			return this.GetFeedInfo(feedUrl, null);
		}

		/// <summary>
		/// Returns the FeedDetails of a feed.
		/// </summary>
		/// <param name="feedUrl">string feed's Url</param>
		/// <param name="credentials">ICredentials, optional. Can be null</param>
		/// <returns>FeedInfo or null, if feed was removed or parameter is invalid</returns>
		public IFeedDetails GetFeedInfo(string feedUrl, ICredentials credentials) {
			
			if (StringHelper.EmptyOrNull(feedUrl))
				return null;

			IFeedDetails fd = null;

			if(!itemsTable.ContainsKey(feedUrl)){
				feedsFeed theFeed = FeedsTable[feedUrl] as feedsFeed;
			  
				if (theFeed == null) {//external feed?

					using (Stream mem = AsyncWebRequest.GetSyncResponseStream(feedUrl, credentials, this.UserAgent, this.Proxy)) {
						feedsFeed f = new feedsFeed();
						f.link = feedUrl;
						if (RssParser.CanProcessUrl(feedUrl)) {
							fd = RssParser.GetItemsForFeed(f, mem, false); 
						}
						//TODO: NntpHandler.CanProcessUrl()
					}
					return fd;
				}

				fd = this.GetFeed(theFeed); 					 
				lock(itemsTable){	
					//if feed was in cache but not in itemsTable we load it into itemsTable
					if(!itemsTable.ContainsKey(feedUrl) && (fd!= null)){
						itemsTable.Add(feedUrl, fd); 
					}
				}
			} else {
				fd = (IFeedDetails)itemsTable[feedUrl];
			}

			return fd;
		}
	
		/// <summary>
		/// Reads the RSS feed from the stream then caches and returns the feed items
		/// in an array list.
		/// </summary>
		/// <remarks>If the feedUrl is currently not stored in this object's internal table
		///	then it is added/</remarks>
		/// <param name="feedUrl">The URL of the feed to download</param>
		/// <param name="feedStream">A stream containing an RSS feed.</param>
		/// <param name="id">A unique identifier for an RSS feed. This typically is the ETag returned if
		/// the feed was fetched via HTTP.</param>
		/// <exception cref="ApplicationException">If the RSS feed is not
		/// version 0.91, 1.0 or 2.0</exception>
		/// <exception cref="XmlException">If an error occured parsing the
		/// RSS feed</exception>
		/// <returns>An arraylist of RSS items (i.e. instances of the NewsItem class)</returns>
		//	[MethodImpl(MethodImplOptions.Synchronized)]
		private ArrayList GetItemsForFeed(string feedUrl, Stream  feedStream, string id){
			return this.GetItemsForFeed(feedUrl,feedStream,id,false);
		}

	  

		/// <summary>
		/// Reads the RSS feed from the feedsFeed link then caches and returns the feed items 
		/// in an array list.
		/// </summary>
		/// <param name="f">Information about the feed. This information is updated based
		/// on the results of processing the feed. </param>
		/// <returns>An arraylist of News items (i.e. instances of the NewsItem class)</returns>
		/// <exception cref="ApplicationException">If the RSS feed is not 
		/// version 0.91, 1.0 or 2.0</exception>
		/// <exception cref="XmlException">If an error occured parsing the 
		/// RSS feed</exception>	
		public ArrayList GetItemsForFeed(feedsFeed f){
			//REM gets called from Bandit (retrive comment feeds)
			ArrayList returnList = EmptyItemList;

			if (this.offline)
				return returnList;

			ICredentials c = null;
			if (!StringHelper.EmptyOrNull(f.authUser)) {
				c = CreateCredentialsFrom(f);
			}


			using (Stream mem = AsyncWebRequest.GetSyncResponseStream(f.link, c, this.UserAgent, this.Proxy)) {
				if (RssParser.CanProcessUrl(f.link)) {
					returnList = RssParser.GetItemsForFeed(f, mem, false).itemsList; 
				}
			}

			return returnList;

		}

		/// <summary>
		/// Reads the RSS feed from the feedsFeed link then caches and returns the feed items 
		/// in an array list.
		/// </summary>
		/// <param name="feedUrl">The feed Url.</param>
		/// <returns>An arraylist of RSS items (i.e. instances of the NewsItem class)</returns>
		/// <exception cref="ApplicationException">If the RSS feed is not 
		/// version 0.91, 1.0 or 2.0</exception>
		/// <exception cref="XmlException">If an error occured parsing the 
		/// RSS feed</exception>	
		public  ArrayList GetItemsForFeed(string feedUrl){

			feedsFeed f = new feedsFeed();
			f.link = feedUrl;
			return this.GetItemsForFeed(f); 
		}

		/// <summary>
		/// Reads the feed from the stream then caches and returns the feed items 
		/// in an array list.
		/// </summary>
		/// <remarks>If the feedUrl is currently not stored in this object's internal table 
		///	then it is added/</remarks>		
		/// <param name="f">Information about the feed. This information is updated based
		/// on the results of processing the feed. </param>
		/// <param name="feedReader">A reader containing an feed.</param>				
		/// <param name="cachedStream">Flag states update last retrieved date on feed only 
		/// if the item was not cached. Indicates whether the lastretrieved date is updated
		/// on the feedsFeed object passed in. </param>
		/// <returns>A FeedDetails object which represents the feed</returns>
		/// <exception cref="ApplicationException">If the feed cannot be processed</exception>
		/// <exception cref="XmlException">If an error occured parsing the feed</exception>	
		public static IFeedDetails GetItemsForFeed(feedsFeed f, XmlReader feedReader, bool cachedStream) {
			//REM gets called from Bandit (AutoDiscoverFeedsThreadandler)
			if (f != null && f.link != null) 
				return null;

			if (RssParser.CanProcessUrl(f.link)) {
				return RssParser.GetItemsForFeed(f, feedReader, cachedStream); 																	
			}

			//TODO: NntpHandler.CanProcessUrl())
			throw new ApplicationException(Resource.Manager["RES_ExceptionNoProcessingHandlerMessage", f.link]);

		}

		/// <summary>
		/// Reads a feed from the stream then caches and returns the feed items 
		/// in an array list.
		/// </summary>
		/// <remarks>If the feedUrl is currently not stored in this object's internal table 
		///	then it is added/</remarks>		
		/// <param name="f">Information about the feed. This information is updated based
		/// on the results of processing the feed. </param>
		/// <param name="feedStream">A stream containing an feed.</param>				
		/// <param name="cachedStream">Flag states update last retrieved date on feed only 
		/// if the item was not cached. Indicates whether the lastretrieved date is updated
		/// on the feedsFeed object passed in. </param>
		/// <returns>A FeedDetails object which represents the feed</returns>
		/// <exception cref="ApplicationException">If the feed cannot be processed</exception>
		/// <exception cref="XmlException">If an error occured parsing the RSS feed</exception>	
		//	[MethodImpl(MethodImplOptions.Synchronized)]
		public static IFeedDetails GetItemsForFeed(feedsFeed f, Stream feedStream, bool cachedStream) {

			if (f != null && f.link != null) 
				return null;

			if (RssParser.CanProcessUrl(f.link)) {
				return RssParser.GetItemsForFeed(f, feedStream, cachedStream); 																	
			}

			//TODO: NntpHandler.CanProcessUrl())
			throw new ApplicationException(Resource.Manager["RES_ExceptionNoProcessingHandlerMessage", f.link]);
		}


		/// <summary>
		/// Reads the RSS feed from the stream then caches and returns the feed items 
		/// in an array list.
		/// </summary>
		/// <remarks>If the feedUrl is currently not stored in this object's internal table 
		///	then it is added/</remarks>
		/// <param name="feedUrl">The URL of the feed to download</param>
		/// <param name="feedStream">A stream containing an RSS feed.</param>
		/// <param name="id">A unique identifier for an RSS feed. This typically is the ETag returned if 
		/// the feed was fetched via HTTP.</param>
		/// <param name="cachedStream">Flag states update last retrieved date on feed only 
		/// if the item was not cached.</param>
		/// <exception cref="ApplicationException">If the RSS feed is not 
		/// version 0.91, 1.0 or 2.0</exception>
		/// <exception cref="XmlException">If an error occured parsing the 
		/// RSS feed</exception>	
		/// <returns>An arraylist of RSS items (i.e. instances of the NewsItem class)</returns>
		//	[MethodImpl(MethodImplOptions.Synchronized)]
		private ArrayList GetItemsForFeed(string  feedUrl, Stream  feedStream, string id, bool cachedStream) {

			feedsFeed f = FeedsTable[feedUrl];
			FeedInfo fi = RssParser.GetItemsForFeed(f, feedStream, cachedStream); 

			if(f.link == null){ f.containsNewMessages = false; }

			//add feed and related info to items table			
			lock(itemsTable) {
				if(itemsTable.ContainsKey(feedUrl)){
					itemsTable.Remove(feedUrl); 
				}

				itemsTable.Add(feedUrl, fi); 
			}

			if(id != null){
				f.etag = id; 
			}
						
			//return (ArrayList) fi.itemsList.Clone(); 			
			return fi.itemsList; 			
		}
	

		/// <summary>
		/// Retrieves the RSS feed for a particular subscription then converts 
		/// the blog posts or articles to an arraylist of items. 
		/// </summary>
		/// <param name="feedUrl">The URL of the feed to download</param>
		/// <param name="force_download">Flag indicates whether cached feed items 
		/// can be returned or whether the application must fetch resources from 
		/// the web</param>
		/// <exception cref="ApplicationException">If the RSS feed is not 
		/// version 0.91, 1.0 or 2.0</exception>
		/// <exception cref="XmlException">If an error occured parsing the 
		/// RSS feed</exception>
		/// <exception cref="WebException">If an error occurs while attempting to download from the URL</exception>
		/// <exception cref="UriFormatException">If an error occurs while attempting to format the URL as an Uri</exception>
		/// <returns>An arraylist of News items (i.e. instances of the NewsItem class)</returns>		
		//	[MethodImpl(MethodImplOptions.Synchronized)]
		public ArrayList GetItemsForFeed(string feedUrl, bool force_download){
			//REM gets called from Bandit
			string url2Access = feedUrl; 

			if(((!force_download)|| this.offline) && itemsTable.Contains(feedUrl)){
				return ((FeedDetailsInternal)itemsTable[feedUrl]).ItemsList;
			}
			
			//We need a reference to the feed so we can see if a cached object exists
			feedsFeed theFeed = null;
			if (FeedsTable.Contains(feedUrl)) 
				theFeed = FeedsTable[feedUrl] as feedsFeed;

			if (theFeed == null)	// not anymore in feedTable
				return EmptyItemList;

			try{ 
				if (theFeed != null) {
													
					if( ((!force_download) || this.offline) && (!itemsTable.ContainsKey(feedUrl)) && ((theFeed.cacheurl != null) && (theFeed.cacheurl.Length > 0) && (this.cacheHandler.FeedExists(theFeed)))) {						
						bool getFromCache = false;
						lock(itemsTable) {
							getFromCache= !itemsTable.ContainsKey(feedUrl);
						}
						if (getFromCache) {	// do not call from within a lock:
							IFeedDetails fi = this.GetFeed(theFeed);
							if (fi != null) {
								lock(itemsTable) {
									if (!itemsTable.ContainsKey(feedUrl))
										itemsTable.Add(feedUrl, fi);  
								}
							}
						}

						return ((FeedDetailsInternal)itemsTable[feedUrl]).ItemsList;
					}
				}

			}catch(Exception ex){
				Trace.WriteLine("Error retrieving feed {" +  feedUrl + "} from cache: " + ex, "Newshandler"); 
			}


			if(this.offline){ //we are in offline mode and don't have the feed cached. 
				return EmptyItemList; 
			}

			try {
				new Uri(url2Access);
			}				
			catch(UriFormatException ufex) {
				Trace.WriteLine("Uri format exception {" + feedUrl + ":" + ufex.ToString(), "RSS BANDIT");
				throw;
			}
		

			this.AsyncGetItemsForFeed(feedUrl, true, true); 
			return EmptyItemList; //we just return this for now, the async call will return real results 
			
		}	
							
	

	
		/// <summary>
		/// Returns the number of pending async. requests in the queue.
		/// </summary>
		/// <returns></returns>
		public int AsyncRequestsPending() {
			return AsyncWebRequest.PendingRequests;
		}

	  
		/// <summary>
		/// Retrieves items from local cache. 
		/// </summary>
		/// <param name="feedUrl"></param>
		/// <returns>A ArrayList of NewsItem objects</returns>
		public ArrayList GetCachedItemsForFeed(string feedUrl) {
		
			lock(itemsTable) {
				if ( itemsTable.Contains(feedUrl)) {
					return ((FeedInfo)itemsTable[feedUrl]).itemsList;
				}
			}
			
			//We need a reference to the feed so we can see if a cached object exists
			feedsFeed theFeed = FeedsTable[feedUrl] as feedsFeed;

			try{ 
				
				if (theFeed != null) {
													
					if( (theFeed.cacheurl != null) && (theFeed.cacheurl.Trim().Length > 0) &&
						(this.cacheHandler.FeedExists(theFeed))  ) {	
						bool getFromCache = false;
						lock(itemsTable) {
							getFromCache = !itemsTable.Contains(feedUrl);
						}
						if (getFromCache) {
							IFeedDetails fi = this.GetFeed(theFeed);
							if (fi != null) {
								lock(itemsTable) {
									if (!itemsTable.Contains(feedUrl)) 
										itemsTable.Add(feedUrl, fi);  
								}
							}
						}
						return ((FeedDetailsInternal)itemsTable[feedUrl]).ItemsList;
					}
				}
			}catch(FileNotFoundException){ // may be deleted in the middle of Test for Exists and GetFeed()
				// ignore
			}catch(XmlException xe){ //cached file is not well-formed so we remove it from cache. 	
				xe = xe;	// tweak compiler warnings, keep for debug info
				this.cacheHandler.RemoveFeed(theFeed); 		  
			}catch(Exception ex){
				Trace.WriteLineIf(TraceMode, "Error retrieving feed {" +  feedUrl + "} from cache: " + ex, "RSS COMPONENTS"); 
				if (!theFeed.causedException) {        
					theFeed.causedException = true;   
					RaiseOnUpdateFeedException(new Uri(feedUrl), new Exception("Error retrieving feed {" +  feedUrl + "} from cache: " + ex.Message, ex));
				} 

			}

			return EmptyItemList;

		}

		/// <summary>
		/// Retrieves the RSS feed for a particular subscription then converts 
		/// the blog posts or articles to an arraylist of items. The http requests are async calls.
		/// </summary>
		/// <param name="feedUrl">The URL of the feed to download</param>
		/// <param name="force_download">Flag indicates whether cached feed items 
		/// can be returned or whether the application must fetch resources from 
		/// the web</param>
		/// <exception cref="ApplicationException">If the RSS feed is not 
		/// version 0.91, 1.0 or 2.0</exception>
		/// <exception cref="XmlException">If an error occured parsing the 
		/// RSS feed</exception>
		/// <exception cref="ArgumentNullException">If feedUrl is a null reference</exception>
		/// <exception cref="UriFormatException">If an error occurs while attempting to format the URL as an Uri</exception>
		/// <returns>true, if the request really was queued up</returns>
		/// <remarks>Result arraylist is returned by OnUpdatedFeed event within UpdatedFeedEventArgs</remarks>		
		//	[MethodImpl(MethodImplOptions.Synchronized)]
		public bool AsyncGetItemsForFeed(string feedUrl, bool force_download){
			return this.AsyncGetItemsForFeed(feedUrl, force_download, false);
		}
		/// <summary>
		/// Retrieves the RSS feed for a particular subscription then converts 
		/// the blog posts or articles to an arraylist of items. The http requests are async calls.
		/// </summary>
		/// <param name="feedUrl">The URL of the feed to download</param>
		/// <param name="force_download">Flag indicates whether cached feed items 
		/// can be returned or whether the application must fetch resources from 
		/// the web</param>
		/// <param name="manual">Flag indicates whether the call was initiated by user (true), or
		/// by automatic refresh timer (false)</param>
		/// <exception cref="ApplicationException">If the RSS feed is not version 0.91, 1.0 or 2.0</exception>
		/// <exception cref="XmlException">If an error occured parsing the RSS feed</exception>
		/// <exception cref="ArgumentNullException">If feedUrl is a null reference</exception>
		/// <exception cref="UriFormatException">If an error occurs while attempting to format the URL as an Uri</exception>
		/// <returns>true, if the request really was queued up</returns>
		/// <remarks>Result arraylist is returned by OnUpdatedFeed event within UpdatedFeedEventArgs</remarks>		
		//	[MethodImpl(MethodImplOptions.Synchronized)]
		public bool AsyncGetItemsForFeed(string feedUrl, bool force_download, bool manual){
			
			if (feedUrl == null || feedUrl.Trim().Length == 0)
				throw new ArgumentNullException("feedUrl");

			Uri reqUri = new Uri(feedUrl);

			RaiseOnUpdateFeedStarted(reqUri, force_download);

			try{

				if ((!force_download) || this.offline) {
					ArrayList items = GetCachedItemsForFeed(feedUrl);
					RaiseOnUpdatedFeed(reqUri, null,  items, RequestResult.NotModified);
					return false;
				}

			}catch(XmlException xe){ //cache file is corrupt
				Trace.WriteLineIf(TraceMode,"Unexpected error retrieving cached feed {" + feedUrl + ":" + xe.ToString(), "RssParser");
			}

			//We need a reference to the feed so we can see if a cached object exists
			feedsFeed theFeed = null;
			if (FeedsTable.ContainsKey(feedUrl)) 
				theFeed = FeedsTable[feedUrl] as feedsFeed;

			if (theFeed == null)
				return false;

			string etag = null; 
			DateTime lastModified = DateTime.MinValue; 
			bool requestQueued = false;

			try { 	     
	
				//DateTime lastRetrieved = DateTime.MinValue; 
				lastModified = DateTime.MinValue;

				if(itemsTable.Contains(feedUrl)){
					etag = theFeed.etag; 
					lastModified = theFeed.lastmodified;

					//if(theFeed.lastretrievedSpecified){
					//	  lastRetrieved = theFeed.lastretrieved; 
					//}
				}

				ICredentials c = null;
				if (!StringHelper.EmptyOrNull(theFeed.authUser)) {
				  
					c = CreateCredentialsFrom(theFeed);
//					string u = null, p = null;
//					GetFeedCredentials(theFeed, ref u, ref p);
//					c = CreateCredentialsFrom(u, p);
				}

				int priority = 10;
				if (force_download)
					priority += 100;
				if (manual) 
					priority += 1000;

				RequestParameter reqParam = RequestParameter.Create(reqUri, this.UserAgent, this.Proxy, c, lastModified, etag);
				AsyncWebRequest.QueueRequest(reqParam, 
					new RequestQueuedCallback(this.OnRequestQueued), 
					new RequestStartCallback(this.OnRequestStart), 
					new RequestCompleteCallback(this.OnRequestComplete), 
					new RequestExceptionCallback(this.OnRequestException), priority);
				
				requestQueued = true;
				
			} catch (Exception e) { 
					
				Trace.WriteLineIf(TraceMode, "Unexpected error on QueueRequest(), processing feed {" + feedUrl + ":" + e.ToString(), "RssParser");												
				RaiseOnUpdateFeedException(reqUri, e);
			}			
			
			return requestQueued;
		}

		#region GetFailureContext()
		/// <summary>
		/// Populates a hashtable with additional feed infos 
		/// we need to provide useful error infos to a user.
		/// It is only fully populated, if we have it allready read from cache.
		/// </summary>
		/// <remarks>
		/// Currently we populate the following keys:
		/// * TECH_CONTACT	(opt.; mail address from: 'webMaster' (RSS) or 'errorReportsTo' (Atom) )
		/// * PUBLISHER			(opt.; mail address from: 'managingEditor' (RSS)
		/// * PUBLISHER_HOMEPAGE	(opt.; additional info link)
		/// * GENERATOR			(opt.; generator software)
		/// * FULL_TITLE			(allways there; category and title as it is used in the UI)
		/// * FAILURE_OBJECT 	(allways there; feedsFeed | nntpFeed)
		/// </remarks>
		/// <param name="feedUri">Uri</param>
		/// <returns>Hashtable</returns>
		public Hashtable GetFailureContext(Uri feedUri) {
			if (feedUri == null)
				return new Hashtable();
			return this.GetFailureContext(FeedsTable[feedUri] as feedsFeed);
		}

		/// <summary>
		/// Overloaded.
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		public Hashtable GetFailureContext(feedsFeed f) {
			
			if (f == null) {	// how about nntpFeeds? They are within the FeedsTable (with different class type)?
				return new Hashtable();
			}

			FeedInfo fi = null;
			lock(itemsTable) {
				if (itemsTable.ContainsKey(f.link)) {
					fi = itemsTable[f.link] as FeedInfo;
				}
			}

			return NewsHandler.GetFailureContext(f, fi);
		}
		
		/// <summary>
		/// Overloaded.
		/// </summary>
		/// <param name="f"></param>
		/// <param name="fi"></param>
		/// <returns></returns>
		public static Hashtable GetFailureContext(feedsFeed f, IFeedDetails fi) {
			
			Hashtable context = new Hashtable();
			
			if (f == null) {	
				return context;
			}

			context.Add("FULL_TITLE", (f.category != null ? f.category: String.Empty) + "\\" +  f.title);
			context.Add("FAILURE_OBJECT", f);

			if (fi == null) 
				return context;

			context.Add("PUBLISHER_HOMEPAGE", fi.Link);
			
			XmlElement xe = RssHelper.GetOptionalElement(fi.OptionalElements, "managingEditor", String.Empty);
			if (xe != null)
				context.Add("PUBLISHER", xe.InnerText);

			xe = RssHelper.GetOptionalElement(fi.OptionalElements, "webMaster",  String.Empty);
			if (xe != null){					
				context.Add("TECH_CONTACT", xe.InnerText);
			}else{
				xe = RssHelper.GetOptionalElement(fi.OptionalElements, "errorReportsTo", "http://webns.net/mvcb/");
				if (xe != null && xe.Attributes["resource", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"] != null)											
					context.Add("TECH_CONTACT", xe.Attributes["resource", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"].InnerText);
			}

			xe = RssHelper.GetOptionalElement(fi.OptionalElements, "generator", String.Empty);
			if (xe != null)
				context.Add("GENERATOR", xe.InnerText);

			return context;
		}
		#endregion

		private void OnRequestQueued(Uri requestUri, int priority) {
			//Trace.WriteLineIf(TraceMode, "Queued: '"+ requestUri.ToString() + "', with Priority '" + priority.ToString()+ "'...", "AsyncRequest");
		}

		private void OnRequestStart(Uri requestUri, ref bool cancel) {
			Trace.WriteLineIf(TraceMode, "Start download: '"+ requestUri.ToString() + "'...", "AsyncRequest");
			this.RaiseOnDownloadFeedStarted(requestUri, ref cancel);
			if (!cancel)
				cancel = this.Offline;
		}

		private void OnRequestException(Uri requestUri, Exception e) {
		  
			Trace.WriteLine("Exception fetching " + requestUri.ToString()); 

			if(this.FeedsTable.Contains(requestUri)){
				Trace.WriteLine(requestUri.ToString() + " in FeedsTable"); 
				feedsFeed f = FeedsTable[requestUri]; 
				// now we set this within causedException prop.
				//f.lastretrieved = DateTime.Now; 
				//f.lastretrievedSpecified = true; 
				f.causedException = true;
			}

			Trace.WriteLineIf(TraceMode, "Exception: '"+ requestUri.ToString() + "' was: "+e.ToString(), "AsyncRequest");
			RaiseOnUpdateFeedException(requestUri, e);
		}

		private void OnRequestComplete(Uri requestUri, Stream response, Uri newUri, string eTag, DateTime lastModified, RequestResult result) {
			Trace.WriteLineIf(TraceMode, "Complete: '"+ requestUri.ToString() + "'.", "AsyncRequest");

			ArrayList itemsForFeed = new ArrayList(); 
		  
			//grab items from feed, then save stream to cache. 
			try{

				//We need a reference to the feed so we can see if a cached object exists
				feedsFeed theFeed = FeedsTable[requestUri] as feedsFeed;

				if (theFeed == null) {
					Trace.WriteLine("(A) HASHTABLE ATTENTION! FeedsTable[requestUri] as feedsFeed returns null for: "+requestUri.ToString());
					return;
				}

				string feedUrl = theFeed.link;

				if (newUri != null) {	// Uri changed/moved permanently

					FeedsTable.Remove(feedUrl); 
					theFeed.link = newUri.ToString(); 
					FeedsTable.Add(theFeed.link, theFeed); 
						
					lock(itemsTable) {
						if(itemsTable.Contains(feedUrl)){
							object FI = itemsTable[feedUrl];
							itemsTable.Remove(feedUrl); 
							itemsTable.Remove(theFeed.link); //remove any old cached versions of redirected link
							itemsTable.Add(theFeed.link, FI); 
						}
					}

					feedUrl = theFeed.link;

				}	// newUri

				if (result == RequestResult.OK) {
					//Update our recently read stories. This is very necessary for 
					//dynamically generated feeds which always return 200(OK) even if unchanged							
						
					FeedDetailsInternal fi = null; 

					if((requestUri.Scheme == "nntp") || (requestUri.Scheme == "news")){
						fi = NntpParser.GetItemsForNewsGroup(theFeed, response, false);							 					
					}else{
						fi = RssParser.GetItemsForFeed(theFeed, response, false);							 
					}
					
					FeedDetailsInternal fiFromCache = null; 
				   
					// Sometimes we may not have loaded feed from cache. So ensure it is 
					// loaded into memory if cached. We don't lock here because loading from
					// disk is too long a time to hold a lock.  
					try{
					
						if(!itemsTable.ContainsKey(feedUrl)){
							fiFromCache = this.GetFeed(theFeed); 					 
						}
					}catch(Exception){ 
						/* the cache file may be corrupt or an IO exception 
						 * not much we can do so just ignore it 
						 */					
					}

					//Merge items list from cached copy of feed with this newly fetched feed. 
					//Thus if a feed removes old entries (such as a news site with daily updates) we 
					//don't lose them in the aggregator. 
					lock(itemsTable){	//TODO: resolve time consuming lock to hold only a short time!!!
				 		
						//if feed was in cache but not in itemsTable we load it into itemsTable
						if(!itemsTable.ContainsKey(feedUrl) && (fiFromCache!= null)){
							itemsTable.Add(feedUrl, fiFromCache); 
						}

						if(itemsTable.ContainsKey(feedUrl)){	
								
							FeedDetailsInternal fi2    = (FeedDetailsInternal) itemsTable[feedUrl];
							
							if (RssParser.CanProcessUrl(feedUrl)) {
								fi.ItemsList = (ArrayList)MergeAndPurgeItems(fi2.ItemsList, fi.ItemsList, theFeed.deletedstories);								
							} else  {
								//TODO: impl. NntpHandler.MergeAndPurgeItems();
							}
						
							//fi.MaxItemAge = fi2.MaxItemAge;						 
									
							itemsTable.Remove(feedUrl); 
						}//if(itemsTable.ContainsKey(feedUrl)){

						itemsTable.Add(feedUrl, fi); 
					}//lock(itemsTable)					    

					//if(eTag != null){	// why we did not store the null?
					theFeed.etag = eTag; 
					//}

					if (lastModified > theFeed.lastmodified) {
						theFeed.lastmodified = lastModified;
					}
			
					theFeed.lastretrieved = new DateTime(DateTime.Now.Ticks); 
					theFeed.lastretrievedSpecified = true; 

								
					theFeed.cacheurl = this.SaveFeed(theFeed); 							
				  
					theFeed.causedException = false;
					itemsForFeed = fi.ItemsList; 

					/* Make sure read stories are accurately calculated */ 
					theFeed.containsNewMessages = false; 
					theFeed.storiesrecentlyviewed.Clear();
	 
					foreach(NewsItem ri in itemsForFeed){
						if(ri.BeenRead){
							theFeed.storiesrecentlyviewed.Add(ri.Id); 
						}
					}
								

					if(itemsForFeed.Count > theFeed.storiesrecentlyviewed.Count){
						theFeed.containsNewMessages = true; 
					} 

				} else if (result == RequestResult.NotModified) {

					// expected behavior: response == null, if not modified !!!
					theFeed.lastretrieved = new DateTime(DateTime.Now.Ticks); 
					theFeed.lastretrievedSpecified = true; 
					theFeed.causedException = false;

					FeedDetailsInternal feedInfo = (FeedDetailsInternal)itemsTable[feedUrl];
					if (feedInfo != null)
						itemsForFeed = feedInfo.ItemsList;
					else 
						itemsForFeed = EmptyItemList;
			  
				}

				RaiseOnUpdatedFeed(requestUri, newUri, itemsForFeed, result);

			}catch(Exception e){
			  
				if(this.FeedsTable.Contains(requestUri)){
					Trace.WriteLine("OnRequestComplete('" + requestUri.ToString() + "') Exception: " + e.Message); 
					feedsFeed f = FeedsTable[requestUri]; 
					// now we set this within causedException prop.:
					//f.lastretrieved = DateTime.Now; 
					//f.lastretrievedSpecified = true; 
					f.causedException = true;
				}

				//Trace.WriteLine("Unexpected error caching feed {" + feedUrl + ":" + e.ToString(), "RSS COMPONENTS");								
				RaiseOnUpdateFeedException(requestUri, e);

			} finally {
				if (response != null)
					response.Close();
			}

		}

		private void OnAllRequestsComplete() {
			RaiseOnAllAsyncRequestsCompleted();
		}

		private void RaiseOnDownloadFeedStarted(Uri requestUri, ref bool cancel) {
			if (BeforeDownloadFeedStarted != null) {
				try {
					DownloadFeedCancelEventArgs ea = new DownloadFeedCancelEventArgs(requestUri, cancel);
					BeforeDownloadFeedStarted(this, ea);
					cancel = ea.Cancel;
				} catch { /* ignore ex. thrown by callback */  }
			}
		}

		private void RaiseOnUpdatedFeed(Uri requestUri, Uri newUri, ArrayList items, RequestResult result) {
			if (OnUpdatedFeed != null) {
				try {
					OnUpdatedFeed(this, new UpdatedFeedEventArgs(requestUri, newUri, items, result));
				} catch { /* ignore ex. thrown by callback */  }
			}
		}

		private void RaiseOnUpdateFeedException(Uri requestUri, Exception e) {
			if (OnUpdateFeedException != null) {
				try {
					if (requestUri != null &&  (requestUri.Scheme == "http" || requestUri.Scheme == "https" || requestUri.Scheme == "file" ))
						e = new FeedRequestException(e.Message, e, this.GetFailureContext(requestUri)); 
					OnUpdateFeedException(this, new UpdateFeedExceptionEventArgs(requestUri, e));
				} catch { /* ignore ex. thrown by callback */  }
			}
		}

		private void RaiseOnAllAsyncRequestsCompleted() {
			if (OnAllAsyncRequestsCompleted != null) {
				try {
					OnAllAsyncRequestsCompleted(this, new EventArgs());
				} catch {/* ignore ex. thrown by callback */ }
			}
		}

		private void RaiseOnUpdateFeedsStarted(bool forced) {
			if ( UpdateFeedsStarted != null) {
				try {
					UpdateFeedsStarted(this, new UpdateFeedsEventArgs(forced));
				} catch {/* ignore ex. thrown by callback */ }
			}
		}
	 
		private void RaiseOnUpdateFeedStarted(Uri feedUri, bool forced) {
			if ( UpdateFeedStarted != null) {
				try {
					UpdateFeedStarted(this, new UpdateFeedEventArgs(feedUri, forced));
				} catch {/* ignore ex. thrown by callback */ }
			}
		}

		/// <summary>
		/// Downloads every feed that has either never been downloaded before or 
		/// whose elapsed time since last download indicates a fresh attempt should be made. 
		/// </summary>
		/// <param name="force_download">A flag that indicates whether download attempts should be made 
		/// or whether the cache can be used.</param>
		/// <remarks>This method uses the cache friendly If-None-Match and If-modified-Since
		/// HTTP headers when downloading feeds.</remarks>	
		public void RefreshFeeds(bool force_download){		

			if(this.FeedsListOK == false){ //we don't have a feed list
				return; 
			}

			bool anyRequestQueued = false;

			try{ 
			
				RaiseOnUpdateFeedsStarted(force_download);

				// The "CopyTo()" construct prevents against InvalidOpExceptions/ArgumentOutOfRange
				// exceptions and keep the loop alive if FeedsTable gets modified from other thread(s)
				string[] keys;
			
				lock (FeedsTable.SyncRoot) {
					keys = new string[FeedsTable.Count];
					if (FeedsTable.Count > 0)
						FeedsTable.Keys.CopyTo(keys, 0);	
				}

				//foreach(string sKey in FeedsTable.Keys){
				//  feedsFeed current = FeedsTable[sKey];	

				for(int i = 0, len = keys.Length; i < len; i++){

					feedsFeed current = FeedsTable[keys[i]];	
				
					if (current == null)	// may have been redirected/removed meanwhile
						continue;

					try{ 

						// new: giving up after three unsuccessfull requests
						if (!force_download && current.causedExceptionCount >= 3) {
							continue;
						}

						if(current.refreshrateSpecified && (current.refreshrate == 0)){
							continue; 	    
						}

						if(itemsTable.ContainsKey(current.link)){ //check if feed downloaded in the past
					
							//check if enough time has elapsed as to require a download attempt
							if((!force_download) && current.lastretrievedSpecified){
		
								double timeSinceLastDownload = DateTime.Now.Subtract(current.lastretrieved).TotalMilliseconds;
								int refreshRate           =  current.refreshrateSpecified ?  current.refreshrate : this.RefreshRate; 
		
								if(timeSinceLastDownload < refreshRate){
									continue; //no need to download 
								}	 																		
							}//if(current.lastretrievedSpecified...) 

	      
							if (this.AsyncGetItemsForFeed(current.link, true, false))
								anyRequestQueued = true;
	    
						}else{	
							
							// not yet loaded, so not loaded from cache, new subscribed or imported
							if (current.lastretrievedSpecified && StringHelper.EmptyOrNull(current.cacheurl))  {	
								// imported may have lastretrievedSpecified set to reduce the initial payload
								double timeSinceLastDownload = DateTime.Now.Subtract(current.lastretrieved).TotalMilliseconds;
								int refreshRate           =  current.refreshrateSpecified ?  current.refreshrate : this.RefreshRate; 
		
								if(timeSinceLastDownload < refreshRate){
									continue; //no need to download 
								} 
							}
							
							if (this.AsyncGetItemsForFeed(current.link, force_download, false))
								anyRequestQueued = true;
							
						}

						Thread.Sleep(15);	// force a context switches

					}catch(Exception e){ 

						Trace.WriteLine("Unexpected error processing feed {" + keys[i] + ":" + e.ToString(), "RSS BANDIT"); 
	  
					}	  									

				}//for(i)

			}catch(InvalidOperationException ioe){// New feeds added to FeedsTable from another thread  
							
				Trace.WriteLine(ioe.ToString(), "RSS Bandit");										
			} 							
			finally {
				if (offline || !anyRequestQueued)
					RaiseOnAllAsyncRequestsCompleted();
			}
		}

		/// <summary>
		/// Downloads every feed that has either never been downloaded before or 
		/// whose elapsed time since last download indicates a fresh attempt should be made. 
		/// </summary>
		/// <param name="category">Refresh all feeds, that are part of the category</param>
		/// <param name="force_download">A flag that indicates whether download attempts should be made 
		/// or whether the cache can be used.</param>
		/// <remarks>This method uses the cache friendly If-None-Match and If-modified-Since
		/// HTTP headers when downloading feeds.</remarks>	
		public void RefreshFeeds(string category, bool force_download){		

			if(this.FeedsListOK == false){ //we don't have a feed list
				return; 
			}

			bool anyRequestQueued = false;

			try{ 
			
				RaiseOnUpdateFeedsStarted(force_download);

				// The "CopyTo()" construct prevents against InvalidOpExceptions/ArgumentOutOfRange
				// exceptions and keep the loop alive if FeedsTable gets modified from other thread(s)
				string[] keys;
			
				lock (FeedsTable.SyncRoot) {
					keys = new string[FeedsTable.Count];
					if (FeedsTable.Count > 0)
						FeedsTable.Keys.CopyTo(keys, 0);	
				}

				//foreach(string sKey in FeedsTable.Keys){
				//  feedsFeed current = FeedsTable[sKey];	

				for(int i = 0, len = keys.Length; i < len; i++){

					feedsFeed current = FeedsTable[keys[i]];	
				
					if (current == null)	// may have been redirected/removed meanwhile
						continue;

					try{ 

						// new: giving up after three unsuccessfull requests
						if (!force_download && current.causedExceptionCount >= 3) {
							continue;
						}

						if(current.refreshrateSpecified && (current.refreshrate == 0)){
							continue; 	    
						}

						if(itemsTable.ContainsKey(current.link)){ //check if feed downloaded in the past
					
							//check if enough time has elapsed as to require a download attempt
							if((!force_download) && current.lastretrievedSpecified){
		
								double timeSinceLastDownload = DateTime.Now.Subtract(current.lastretrieved).TotalMilliseconds;
								int refreshRate           =  current.refreshrateSpecified ?  current.refreshrate : this.RefreshRate; 
		
								if(timeSinceLastDownload < refreshRate){
									continue; //no need to download 
								}	 																		
							}//if(current.lastretrievedSpecified...) 

	      
							if (current.category != null && current.category.StartsWith(category)) {
								if (this.AsyncGetItemsForFeed(current.link, true, false))
									anyRequestQueued = true;
							}
	    
						}else{

							if (current.category != null && current.category.StartsWith(category)) {
								if (this.AsyncGetItemsForFeed(current.link, force_download, false))
									anyRequestQueued = true;
							}
						}

						Thread.Sleep(15);	// force a context switches

					}catch(Exception e){ 

						Trace.WriteLine("Unexpected error processing feed {" + current.link + ":" + e.ToString(), "RSS BANDIT"); 
	  
					}	  									

				}//for(i)

			}catch(InvalidOperationException ioe){// New feeds added to FeedsTable from another thread  
							
				Trace.WriteLine(ioe.ToString(), "RSS Bandit");										
			} 							
			finally {
				if (offline || !anyRequestQueued)
					RaiseOnAllAsyncRequestsCompleted();
			}
		}
	  
		/// <summary>
		/// Converts the input XML document from OCS, OPML or SIAM to the RSS Bandit feed list 
		/// format. 
		/// </summary>
		/// <param name="doc">The input feed list</param>
		/// <returns>The converted feed list</returns>
		/// <exception cref="ApplicationException">if the feed list format is unknown</exception>
		private XmlDocument ConvertFeedList(XmlDocument doc){
					
			ImportFilter importFilter = new ImportFilter(doc);

			XslTransform transform = importFilter.GetImportXsl();

			if(transform != null) {
				// We have a format other than Bandit
				// Apply the import filter (transform)
				XmlDocument temp = new XmlDocument(); 
				temp.Load(transform.Transform(doc, null)); 
				doc = temp;
			}
			else {
				// see if we have a Bandit format
				if(importFilter.Format == ImportFeedFormat.Bandit) {
					// load and validate the Bandit feed file
					//validate document 
					XmlParserContext context = new XmlParserContext(null, new RssBanditXmlNamespaceResolver(), null, XmlSpace.None);
					XmlValidatingReader vr = new RssBanditXmlValidatingReader(doc.OuterXml, XmlNodeType.Document, context);
					vr.Schemas.Add(feedsSchema); 
					vr.ValidationType = ValidationType.Schema; 	
					vr.ValidationEventHandler += new ValidationEventHandler(ValidationCallbackOne);		
					doc.Load(vr); 
					vr.Close();
				}
				else {
					// We have an unknown format
					throw new ApplicationException("Unknown Feed Format.",null);
				}
			}
		
			return doc; 
		}




		/// <summary>
		/// Replaces the existing list of feeds used by the application with the list of 
		/// feeds in the specified XML document. The file must be an RSS Bandit feed list
		/// or a SIAM file. 
		/// </summary>
		/// <param name="feedlist">The list of feeds</param>
		/// <exception cref="ApplicationException">If the file is not a SIAM, OPML or RSS bandit feedlist</exception>		
		public void ReplaceFeedlist(Stream feedlist){
			
			CategoriesCollection categories = new CategoriesCollection(); 
			FeedsCollection syncedfeeds = new FeedsCollection();

			GetCurrentCommonFeedList(syncedfeeds, categories); 

			/* update feeds table */ 
			this._feedsTable = syncedfeeds; 
			/* update category information */
			this.categories = categories; 
		}



		/// <summary>
		/// Replaces or imports the existing list of feeds used by the application with the list of 
		/// feeds in the specified XML document. The file must be an RSS Bandit feed list
		/// or a SIAM file. 
		/// </summary>
		/// <param name="feedlist">The list of feeds</param>
		/// <param name="category">The category to import the feeds into</param>
		/// <param name="replace">Indicates whether the feedlist should be replaced or not</param>
		/// <exception cref="ApplicationException">If the file is not a SIAM, OPML or RSS bandit feedlist</exception>		
		public void ImportFeedlist(Stream feedlist, string category, bool replace){
		
			/* TODO: Sync category settings */ 

			CategoriesCollection categories = new CategoriesCollection(); 
			FeedColumnLayoutCollection layouts = new FeedColumnLayoutCollection(); 

			FeedsCollection syncedfeeds = new FeedsCollection();			
			XmlDocument doc = new XmlDocument(); 			
			doc.Load(feedlist); 

			//convert feed list to RSS Bandit format
			doc = ConvertFeedList(doc); 

			//load up 
			XmlNodeReader reader = new XmlNodeReader(doc);		
			XmlSerializer serializer  = new XmlSerializer(typeof(feeds));
			feeds myFeeds = (feeds)serializer.Deserialize(reader); 
			reader.Close(); 

			// InitialHTTPLastModifiedSettings used to reduce the initial payload
			// for the first request of imported feeds.
			// HTTP endpoints considering also/only the ETag header will influence 
			// if a 200 OK is returned onrequest or not.
			// HTTP endpoints not considering the Last Modified header will not be affected.
			DateTime[] dta = RssHelper.InitialLastRetrievedSettings(myFeeds.feed.Count, this.RefreshRate);
			int dtaCount = dta.Length, count = 0;

			while(myFeeds.feed.Count != 0){
				
				feedsFeed f1 = (feedsFeed) myFeeds.feed[0]; 

				if(replace && _feedsTable.ContainsKey(f1.link)){					

					//copy category information over
					feedsFeed f2 = _feedsTable[f1.link]; 
					f2.category = f1.category; 

					if((f2.category != null) && !categories.ContainsKey(f2.category)){
						categories.Add(f2.category); 
					}

					//copy listview layout information over
					if((f1.listviewlayout != null) && !layouts.ContainsKey(f1.listviewlayout)){
						listviewLayout layout = FindLayout(f1.listviewlayout, myFeeds.listviewLayouts);

						if(layout != null)
							layouts.Add(f1.listviewlayout, layout.FeedColumnLayout); 
						else
							f1.listviewlayout = null; 
					}
					f2.listviewlayout = (f1.listviewlayout != null ? f1.listviewlayout: f2.listviewlayout);


					//copy title information over 
					f2.title   = f1.title; 

					if(f2.link.IndexOf("blogs.msdn.com")!= -1){
						f2.title = f2.title; 
					}

					//copy various settings over			
					f2.markitemsreadonexitSpecified = f1.markitemsreadonexitSpecified;
					if(f1.markitemsreadonexitSpecified){
						f2.markitemsreadonexit = f1.markitemsreadonexit;
					}

					f2.stylesheet = (f1.stylesheet != null ? f1.stylesheet: f2.stylesheet);
					f2.maxitemage = (f1.maxitemage != null ? f1.maxitemage: f2.maxitemage);
					f2.alertEnabledSpecified = f1.alertEnabledSpecified;
					f2.alertEnabled = (f1.alertEnabledSpecified ? f1.alertEnabled : f2.alertEnabled);
					f2.refreshrateSpecified = f1.refreshrateSpecified;
					f2.refreshrate = (f1.refreshrateSpecified ? f1.refreshrate : f2.refreshrate);

					//DISCUSS
					//f2.downloadenclosures ?

					// save to sync.: key is generated the same on every machine, IV seems to have no influence 
					f2.authPassword = f1.authPassword; 
					f2.authUser     = f1.authUser; 

					//copy over deleted stories
					foreach(string story in f1.deletedstories){
						if(!f2.deletedstories.Contains(story)){
							f2.deletedstories.Add(story); 
						}		
					}//foreach

					//copy over read stories
					foreach(string story in f1.storiesrecentlyviewed){
						if(!f2.storiesrecentlyviewed.Contains(story)){
							f2.storiesrecentlyviewed.Add(story); 
						}		
					}//foreach					

					if(itemsTable.ContainsKey(f2.link)){
						ArrayList items = ((FeedInfo)itemsTable[f2.link]).itemsList;

						foreach(NewsItem item in items){
							if(f2.storiesrecentlyviewed.Contains(item.Id)){
								item.BeenRead = true; 
							}
						}					
					}

					syncedfeeds.Add(f2.link, f2); 
				

				}else{ 

					if(replace){
						if((f1.category != null) && !categories.ContainsKey(f1.category)){
							categories.Add(f1.category); 
						}

						if((f1.listviewlayout != null) && !layouts.ContainsKey(f1.listviewlayout)){
							listviewLayout layout = FindLayout(f1.listviewlayout, myFeeds.listviewLayouts);

							if(layout != null)
								layouts.Add(f1.listviewlayout, layout.FeedColumnLayout); 
							else
								f1.listviewlayout = null; 
						}
						
						if(!syncedfeeds.ContainsKey(f1.link)){
							syncedfeeds.Add(f1.link, f1); 			
						}		

					}else{						

						if(category.Length > 0){
							f1.category = (f1.category == null ? category : category + '\\' + f1.category);
						}
						//f1.category = (category  == String.Empty ? f1.category : category + '\\' + f1.category); 
						if(!_feedsTable.ContainsKey(f1.link)){
							f1.lastretrievedSpecified = true;
							f1.lastretrieved = dta[count % dtaCount];
							_feedsTable.Add(f1.link, f1); 	
						}					
					}
				}
			
				myFeeds.feed.RemoveAt(0); 
				count++;
			}


			ListDictionary serverList = new ListDictionary(); 
			ListDictionary identityList = new ListDictionary(); 
			
			/* copy over user identity information */
			foreach(UserIdentity identity in myFeeds.identities){

				if(replace){
					identityList.Add(identity.Name, identity); 
				} else if(!this.identities.Contains(identity.Name)){								
					this.identities.Add(identity.Name, identity); 
				}
			}//foreach
			

			/* copy over newsgroup information */
			foreach(NntpServerDefinition server in myFeeds.nntpservers){
				if(replace){
					serverList.Add(server.Name, server); 
				} else if(!this.identities.Contains(server.Name)){								
					this.nntpServers.Add(server.Name, server); 
				}
			}

			// copy over layout information 
			foreach(listviewLayout layout in myFeeds.listviewLayouts){
				if(replace){
					if(layout.FeedColumnLayout.LayoutType == LayoutType.GlobalFeedLayout ||
					   layout.FeedColumnLayout.LayoutType == LayoutType.GlobalCategoryLayout ||
					   layout.FeedColumnLayout.LayoutType == LayoutType.SearchFolderLayout ||
					   layout.FeedColumnLayout.LayoutType == LayoutType.SpecialFeedsLayout)
					layouts.Add(layout.ID, layout.FeedColumnLayout); 

				} else if(!this.layouts.ContainsKey(layout.ID)){ //don't replace layouts on import
					if(layout.FeedColumnLayout.LayoutType != LayoutType.GlobalFeedLayout ||
						layout.FeedColumnLayout.LayoutType != LayoutType.GlobalCategoryLayout ||
						layout.FeedColumnLayout.LayoutType != LayoutType.SearchFolderLayout ||
						layout.FeedColumnLayout.LayoutType != LayoutType.SpecialFeedsLayout)		
					this.layouts.Add(layout.ID, layout.FeedColumnLayout); 
				}
			}


			if(replace){
				
				/* update feeds table */ 
				this._feedsTable = syncedfeeds; 
				/* update category information */
				this.categories = categories; 
				/* update identities */ 
				this.identities = identityList; 
				/* update servers */
				this.nntpServers = serverList; 
				/* update layouts */
				this.layouts = layouts;
			
			}else{
			
				if(myFeeds.categories.Count == 0){ //no new subcategories
					if(category.Length > 0 && this.categories.ContainsKey(category) == false){
						this.categories.Add(category); 
					}	  
				}else {

					foreach(category cat in myFeeds.categories){
						string cat2 = (category.Length == 0 ? cat.Value : category + '\\' + cat.Value); 
				
						if(this.categories.ContainsKey(cat2) == false){
							this.categories.Add(cat2); 
						}
					}
				}
			}

			//if original feed list was invalid then reset error indication	
			if(validationErrorOccured){
				validationErrorOccured = false; 
			}
			
		}


		/// <summary>
		/// Merges the list of feeds in the specified XML document with that currently 
		/// used by the application. The file can either be an RSS Bandit feed list or an 
		/// OPML file. 
		/// </summary>
		/// <param name="feedlist">The list of feeds</param>
		/// <exception cref="ApplicationException">If the file is neither an OPML file or RSS bandit feedlist</exception>		
		public void ImportFeedlist(Stream feedlist){
			this.ImportFeedlist(feedlist, String.Empty, false); 
		}
	  
    

		/// <summary>
		/// Merges the list of feeds in the specified XML document with that currently 
		/// used by the application. The file can either be an RSS Bandit feed list or an 
		/// OPML file. 
		/// </summary>
		/// <param name="feedlist">The list of feeds</param>
		/// <param name="category">The category to import the feeds into</param>
		/// <exception cref="ApplicationException">If the file is neither an OPML file or RSS bandit feedlist</exception>		
		public void ImportFeedlist(Stream feedlist, string category){
		
			try{ 
				this.ImportFeedlist(feedlist, category, false); 

				/* XmlDocument doc = new XmlDocument(); 
				//XmlDocument fl = new XmlDocument(); 
				doc.Load(feedlist); 

				//convert feed list to RSS Bandit format
				doc = ConvertFeedList(doc); 

				//load up 
				XmlNodeReader reader = new XmlNodeReader(doc);		
				XmlSerializer serializer  = new XmlSerializer(typeof(feeds));
				feeds myFeeds = (feeds)serializer.Deserialize(reader); 
				reader.Close(); 

				if(_feedsTable == null){	
					_feedsTable = new FeedsCollection(); 
				}
		 
			 

				foreach(feedsFeed f in myFeeds.feed){
		
					//if the same feed seen twice, ignore second occurence 
					if(_feedsTable.ContainsKey(f.link) == false){
						if(category != String.Empty){
							f.category = (f.category == null ? category : category + '\\' + f.category);
						}
						//f.category = (category  == String.Empty ? f.category : category + '\\' + f.category); 
						_feedsTable.Add(f.link, f); 
					}
				}		
	
				if(myFeeds.categories.Count == 0){ //no new subcategories
					if(category != String.Empty && this.categories.ContainsKey(category) == false){
						this.categories.Add(category); 
					}	  
				}else {

					foreach(string cat in myFeeds.categories){
						string cat2 = (category == String.Empty ? cat : category + '\\' + cat); 
				
						if(this.categories.ContainsKey(cat2) == false){
							this.categories.Add(cat2); 
						}
					}
				}
				//if original feed list was invalid then reset error indication	
				if(validationErrorOccured){
					validationErrorOccured = false; 
				}*/				

			}catch(Exception e){
				throw new ApplicationException(e.Message, e); 
			}

		}

		/// <summary>
		/// Handles errors that occur during schema validation of RSS feed list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public static void ValidationCallbackOne(object sender,
			ValidationEventArgs args) {
			if(args.Severity == XmlSeverityType.Error){
				Trace.WriteLine(args.Message);
			
				/* In some cases we corrupt feedlist.xml by not putting all referenced
				 * categories in <category> elements. This is not a fatal error. 
				 */
				XmlSchemaException xse = args.Exception;
				if(xse != null){
					Type xseType            = xse.GetType(); 
					FieldInfo resFieldInfo  = xseType.GetField("res", BindingFlags.NonPublic | BindingFlags.Instance);             

					string errorType = (string) resFieldInfo.GetValue(xse); 

					if(!errorType.Equals("Sch_UnresolvedKeyref")){								
						validationErrorOccured = true; 		
					} else{
						categoryMismatch = true; 
					}
				}//if(xse != null) 
			}//if(args.Severity...)	

		}


		/// <summary>
		/// Saves a particular RSS feed.
		/// </summary>
		/// <remarks>This method should be thread-safe</remarks>
		/// <param name="feed">The the feed to save. This is an identifier
		/// and not used to actually fetch the feed from the WWW.</param>
		/// <returns>An identifier for the saved feed. </returns>		
		private string SaveFeed(feedsFeed feed){

			TimeSpan maxItemAge    = this.GetMaxItemAge(feed.link); 
			FeedDetailsInternal fi = (FeedDetailsInternal) this.itemsTable[feed.link];
			ArrayList items        = fi.ItemsList;

				/* remove items that have expired according to users cache requirements */ 
				if(maxItemAge != TimeSpan.MinValue){ /* check if feed set to never delete items */ 

					lock(items){
			
						for(int i = 0, count = items.Count ; i < count ; i++){
							NewsItem item = (NewsItem) items[i]; 
				
							if(feed.deletedstories.Contains(item.Id) || ((DateTime.Now - item.Date) >= maxItemAge)){
						
								items.Remove(item); 
								NewsHandler.RelationCosmosRemove(item);
								count--; 
								i--; 
							}//if
						}//for
					}//lock
				}//if(maxItemAge != TimeSpan.MinValue)						


			return this.cacheHandler.SaveFeed(fi); 
		}

		/// <summary>
		/// Returns an RSS feed. 
		/// </summary>
		/// <param name="feed">The feed whose FeedInfo is required.</param>
		/// <returns>The requested feed or null if it doesn't exist</returns>
		private FeedDetailsInternal GetFeed(feedsFeed feed){
			
			FeedDetailsInternal fi = this.cacheHandler.GetFeed(feed); 

			if(fi != null){
				
				/* remove items that have expired according to users cache requirements */ 
				TimeSpan maxItemAge = this.GetMaxItemAge(feed.link);
			  
				int readItems = 0; 

				ArrayList items = fi.ItemsList;
				lock(items){
			    
					/* check if feed set to never delete items */ 
					bool keepAll = (maxItemAge == TimeSpan.MinValue) && (feed.deletedstories.Count == 0); 

					//since we are going to use this value for calculation we should change it 
					//from TimeSpan.MinValue which is used to indicate 'keep indefinitely' to TimeSpan.MaxValue
					maxItemAge = (maxItemAge == TimeSpan.MinValue ? TimeSpan.MaxValue: maxItemAge);

					for(int i = 0, count = items.Count ; i < count ; i++){
						NewsItem item = (NewsItem) items[i]; 
			      
						if((!keepAll) && ((DateTime.Now - item.Date) >= maxItemAge) || feed.deletedstories.Contains(item.Id)){
				
							items.Remove(item); 
							NewsHandler.RelationCosmosRemove(item);
							i--; 
							count--; 
			      
						}else if(item.BeenRead){							
							readItems++;
						}
			    
					}			  
				}

				if(readItems == items.Count){
					feed.containsNewMessages = false; 
				}else{
					feed.containsNewMessages = true; 
				}	
			
			}//if(fi != null)

			return fi; 
		}

		/// <summary>
		/// Merge and purge items.
		/// </summary>
		/// <param name="oldItems">IList with the old items</param>
		/// <param name="newItems">IList with the new items</param>
		/// <param name="deletedItems">IList with the IDs of deleted items</param>
		/// <returns>IList merge/purge result</returns>
		public static IList MergeAndPurgeItems(IList oldItems, IList newItems, IList deletedItems) {
			lock(oldItems){

				foreach(NewsItem newitem in newItems){
					int index = oldItems.IndexOf(newitem);
					if(index == -1) {
						if(!deletedItems.Contains(newitem.Id)) {
							oldItems.Add(newitem);   
						}
					}else{
						NewsItem olditem = (NewsItem) oldItems[index];										    
						newitem.BeenRead   = olditem.BeenRead;
						newitem.SetDate(olditem.Date); //so the date is from when it was first fetched
						newitem.FlagStatus = olditem.FlagStatus; 
						oldItems.RemoveAt(index);
						oldItems.Add(newitem);
					}
				}
								
				/*
					foreach(NewsItem olditem in oldItems){
						int index = fi.itemsList.IndexOf(olditem);		// linear (!) search, calling Object.Equals() on NewsItem's

						if(index == -1){
							fi.itemsList.Add(olditem); 
						}else{
							NewsItem item = (NewsItem) fi.itemsList[index];										    
							item.BeenRead   = olditem.BeenRead;
							item.SetDate(olditem.Date); //so the date is from when it was first fetched
							item.FlagStatus = olditem.FlagStatus; 
						}
									
					} */
			}

			return oldItems;

		}

		#region RelationCosmos management
		/// <summary>
		/// </summary>
		/// <param name="item"></param>
		/// <param name="excludeItemsList"></param>
		/// <returns></returns>
		public ICollection GetItemsWithIncomingLinks(NewsItem item, IList excludeItemsList){
			return relationCosmos.GetIncoming(item, excludeItemsList);
		}
		
		/// <summary>
		/// </summary>
		/// <param name="item"></param>
		/// <param name="excludeItemsList"></param>
		/// <returns></returns>
		public ICollection GetItemsFromOutGoingLinks(NewsItem item, IList excludeItemsList){
			return relationCosmos.GetOutgoing(item, excludeItemsList);
		}
	 
		/// <summary>
		/// </summary>
		/// <param name="item"></param>
		/// <param name="excludeItemsList"></param>
		/// <returns></returns>
		public bool HasItemAnyRelations(NewsItem item, IList excludeItemsList) {
			return relationCosmos.HasIncomingOrOutgoing(item, excludeItemsList);
		}

		/// <summary>
		/// Internal used accessor
		/// </summary>
		/// <param name="relation"></param>
		internal static void RelationCosmosAdd (RelationBase relation) {
			relationCosmos.Add(relation);
		}
		internal static void RelationCosmosAddRange (IList relations) {
			relationCosmos.AddRange(relations);
		}
		internal static void RelationCosmosRemove (RelationBase relation) {
			relationCosmos.Remove(relation);
		}
		internal static void RelationCosmosRemoveRange (IList relations) {
			relationCosmos.RemoveRange(relations);
		}
		#endregion

		/// <summary>
		/// Posts a comment in reply to an item using either NNTP or the CommentAPI 
		/// </summary>
		/// <param name="url">The URL to post the comment to</param>
		/// <param name="item2post">An RSS item that will be posted to the website</param>
		/// <param name="inReply2item">An RSS item that is the post parent</param>		
		/// <exception cref="WebException">If an error occurs when the POSTing the 
		/// comment</exception>
		public void PostComment(string url, NewsItem item2post, NewsItem inReply2item){			  
			
			if(inReply2item.CommentStyle == SupportedCommentStyle.CommentAPI){
				this.RssParser.PostCommentViaCommentAPI(url, item2post, inReply2item, GetFeedCredentials(inReply2item.Feed));
			}else if(inReply2item.CommentStyle == SupportedCommentStyle.NNTP){
				NntpParser.PostCommentViaNntp(item2post, inReply2item, GetFeedCredentials(inReply2item.Feed));
			}

		}
		

	}


	/// <summary>
	/// Interface represents extended information about a particular feed
	/// (internal use only)
	/// </summary>
	internal interface FeedDetailsInternal: IFeedDetails {
		ArrayList ItemsList { get; set; }
		string FeedLocation {get; set; }
		void WriteTo(XmlWriter writer);
	}
  
	/// <summary>
	/// Get informations about the size of an object or item
	/// </summary>
	public interface ISizeInfo {
		int GetSize();
		string GetSizeDetails();
	}

	#region RssBanditXmlNamespaceResolver 

	/// <summary>
	/// Helper class used for treating v1.2.* RSS Bandit feedlist.xml files as RSS Bandit v1.3.* 
	/// subscriptions.xml files
	/// </summary>
	internal class RssBanditXmlNamespaceResolver : XmlNamespaceManager {

		public RssBanditXmlNamespaceResolver(): base(new NameTable()){}

		public override void AddNamespace(string prefix, string uri) {   
			if ( uri == "http://www.25hoursaday.com/2003/RSSBandit/feeds/" ) {	
				uri = "http://www.25hoursaday.com/2004/RSSBandit/feeds/";	
			}      			
			base.AddNamespace(prefix, uri);      
		}

	}

	#endregion

	#region RssBanditXmlValidatingReader 

	/// <summary>
	/// Helper class used for treating v1.2.* RSS Bandit feedlist.xml files as RSS Bandit v1.3.* 
	/// subscriptions.xml files
	/// </summary>
	internal class RssBanditXmlValidatingReader: XmlValidatingReader{	

		public RssBanditXmlValidatingReader(Stream s, XmlNodeType nodeType, XmlParserContext context): base(s, nodeType, context){}
		public RssBanditXmlValidatingReader(string s, XmlNodeType nodeType, XmlParserContext context): base(s, nodeType, context){}

		public override string Value{
			get {
				if((this.NodeType == XmlNodeType.Attribute) && 
					(base.Value == "http://www.25hoursaday.com/2003/RSSBandit/feeds/")){
					return "http://www.25hoursaday.com/2004/RSSBandit/feeds/"; 
				}else{
					return base.Value; 
				}
			}
		}
	}//class 

	#endregion 

}
