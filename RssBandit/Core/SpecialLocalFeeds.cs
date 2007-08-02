#region CVS Version Header
/*
 * $Id: SpecialLocalFeeds.cs,v 1.37 2005/03/08 16:56:08 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/03/08 16:56:08 $
 * $Revision: 1.37 $
 */
#endregion

using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Configuration;
using System.Text;

using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Net;
using NewsComponents.Utils;
using RssBandit;

using Logger = RssBandit.Common.Logging;

namespace RssBandit.SpecialFeeds
{

	#region specialized local feedsFeed
		
	/// <summary>
	/// Special local feeds
	/// </summary>
	public class LocalFeedsFeed:feedsFeed {
		
		protected static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(LocalFeedsFeed));

		protected ArrayList itemsList = new ArrayList();
		private string filePath = null;
		private FeedInfo feedInfo = null;
		private string description = null;
		private bool modified = false;

		public LocalFeedsFeed() {
			base.refreshrate = 0;		
			base.refreshrateSpecified = true;
			this.feedInfo = new FeedInfo(null, null);
		}
			

		/// <summary>
		/// Initializer.
		/// </summary>
		/// <param name="feedUrl"></param>
		/// <param name="feedTitle"></param>
		/// <param name="feedDescription"></param>
		/// <exception cref="UriFormatException">If feedUrl could not be formatted as a Uri</exception>
		public LocalFeedsFeed(string feedUrl, string feedTitle, string feedDescription):	
			this(feedUrl, feedTitle, feedDescription, true) {
		}
		
		/// <summary>
		/// Initializer.
		/// </summary>
		/// <param name="feedUrl"></param>
		/// <param name="feedTitle"></param>
		/// <param name="feedDescription"></param>
		/// <param name="loadItems">bool</param>
		/// <exception cref="UriFormatException">If feedUrl could not be formatted as a Uri</exception>
		public LocalFeedsFeed(string feedUrl, string feedTitle, string feedDescription, bool loadItems):this() {
			description = feedDescription;
			filePath = feedUrl;
			try {
				Uri feedUri = new Uri(feedUrl);
				base.link = feedUri.ToString();
			} catch {
				base.link = feedUrl;
			}
			base.title = feedTitle;
			
			this.feedInfo = new FeedInfo(filePath, itemsList, feedTitle, base.link, feedDescription);
			
			if (loadItems)
				LoadItems(this.GetDefaultReader());
		}

		/// <summary>
		/// Initializer.
		/// </summary>
		/// <param name="feedUrl">local file path</param>
		/// <param name="feedTitle"></param>
		/// <param name="feedDescription"></param>
		/// <param name="reader"></param>
		/// <exception cref="UriFormatException">If feedUrl could not be formatted as a Uri</exception>
		public LocalFeedsFeed(string feedUrl, string feedTitle, string feedDescription, XmlReader reader):
			this(feedUrl, feedTitle, feedDescription, false) {
			LoadItems(reader);
		}

		public ArrayList Items {
			get { return itemsList;  }
			set { itemsList = value; }
		}

		public void Add(LocalFeedsFeed lff){
			foreach(NewsItem item in lff.Items){
				if(!this.itemsList.Contains(item)){
					this.Add(item); 
				}
			}		
		}

		public void Add(NewsItem item) {
			if (item == null)
				return;
			
			item.FeedDetails = this.feedInfo;
			this.itemsList.Add(item);
			this.modified = true;
		}

		public void Remove(NewsItem item) {
			if (item != null && this.itemsList.Contains(item)) {
				this.itemsList.Remove(item);
				this.modified = true;
			}
		}

		public string Location {
			get { return this.filePath;	}
			set { this.filePath = value;	}
		}
					
		public string Url {
			get { return base.link;  }
		}

		public bool Modified {
			get { return this.modified;  }
			set { this.modified = value; }
		}

		public string Description {
			get { return this.feedInfo.Description;  }
		}

		protected void LoadItems(XmlReader reader){
			if (itemsList.Count > 0)
				itemsList.Clear(); 

			if (reader != null) {
				try{				
					XmlDocument doc = new XmlDocument(); 
					doc.Load(reader); 
					foreach(XmlElement elem in doc.SelectNodes("//item")){				
						NewsItem item = RssParser.MakeRssItem(this,  new XmlNodeReader(elem)); 
						item.BeenRead = true;
						item.FeedDetails = this.feedInfo;
						itemsList.Add(item); 
					}
				}catch(Exception e){
					ExceptionManager.GetInstance().Add(RssBanditApplication.CreateLocalFeedRequestException(e, this, this.feedInfo));
				}			
			}
		}

		protected XmlReader GetDefaultReader(){
			
			if (File.Exists(this.filePath)) {
				return new XmlTextReader(this.filePath);
			}

			return null;
		}

		/// <summary>
		/// Writes this object as an RSS 2.0 feed to the specified writer
		/// </summary>
		/// <param name="writer"></param>
		public void WriteTo(XmlWriter writer){
			this.feedInfo.WriteTo(writer);
		}
		
		public void Save() {

			XmlTextWriter writer = null;
			try {
				writer = new XmlTextWriter(new StreamWriter( this.filePath ));
				this.WriteTo(writer); 
				writer.Flush(); 
				writer.Close();
				Modified = false;
			} catch (Exception e) { 
				_log.Error("LocalFeedsFeed.Save()", e);
			}
			finally { if (writer!=null) writer.Close(); }
		}

	}

	#endregion

	/// <summary>
	/// Threadsafe ExceptionManager to handle and report
	/// Feed errors and other errors that needs to be published 
	/// to a user. Singleton is implemented 
	/// (see also http://www.yoda.arachsys.com/csharp/beforefieldinit.html).
	/// </summary>
	public sealed class ExceptionManager:LocalFeedsFeed
	{

		private ExceptionManager() {	}
		public ExceptionManager(string feedUrl, string feedTitle, string feedDescription):base(feedUrl, feedTitle, feedDescription, false){ 
			try {
				base.Save();	// re-create a new file with no items
			} catch (Exception ex) {
				Common.Logging.Log.Fatal("ExceptionManager.Save() failed", ex);
			}
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
		public void Add(System.Exception e) {
			FeedException fe = new FeedException(this, e);
			base.itemsList.Add(fe.NewsItemInstance);
			try {
				base.Save();
			} catch (Exception ex) {
				Common.Logging.Log.Fatal("ExceptionManager.Save() failed", ex);
			}
		}

		public new ArrayList Items {
			get { return base.itemsList; }
		}

		/// <summary>
		/// Returns a instance of ExceptionManager.
		/// </summary>
		/// <returns>Instance of the ExceptionManageer class</returns>
		public static ExceptionManager GetInstance() {
			return InstanceHelper.instance;
		}

		/// <summary>
		/// Private instance helper class to impl. Singleton
		/// </summary>
		private class InstanceHelper {
			// Explicit static constructor to tell C# compiler
			// not to mark type as beforefieldinit
			static InstanceHelper() {;}
			internal static readonly ExceptionManager instance = new ExceptionManager(				
				RssBanditApplication.GetFeedErrorFileName(), 
				Resource.Manager["RES_FeedNodeFeedExceptionsCaption"], 
				Resource.Manager["RES_FeedNodeFeedExceptionsDesc"]);
		}

		public class FeedException {
			
			private static int idCounter = 0;
			private static string validationUrlBase = null;			// used to provide a validate Url

			private NewsItem _delegateTo = null;	// cannot inherit, so we use the old containment

			// used to build up a informational error description
			private string _ID;
			private string _feedCategory = String.Empty;
			private string _feedTitle = String.Empty;
			private string _resourceUrl = String.Empty;
			private string _resourceUIText = String.Empty;
			private string _publisherHomepage = String.Empty;
			private string _publisher = String.Empty;
			private string _techContact = String.Empty;
			private string _generator = String.Empty;
			private string _fullErrorInfoFile = String.Empty;

			static FeedException() {
				// read app.config If a key was not found, take it from the resources
				validationUrlBase = ConfigurationSettings.AppSettings["validationUrlBase"];
				if (validationUrlBase == null || validationUrlBase.Length == 0)
					validationUrlBase = Resource.Manager["URL_FeedValidationBase"];
			}
			public FeedException(ExceptionManager ownerFeed, Exception e) {

				idCounter++;
				this._ID = String.Concat("#", idCounter.ToString());

				if (e is FeedRequestException)
					this.InitWith(ownerFeed, (FeedRequestException)e);
				else if (e is XmlException)
					this.InitWith(ownerFeed, (XmlException)e);
				else if (e is System.Net.WebException)
					this.InitWith(ownerFeed, (System.Net.WebException)e);
				else if (e is System.Net.ProtocolViolationException)
					this.InitWith(ownerFeed, (System.Net.ProtocolViolationException)e);
				else
					this.InitWith(ownerFeed, e);
			}

			private void InitWith(ExceptionManager ownerFeed, FeedRequestException e) {
				if (e.Feed != null) {
					this._feedCategory = e.Feed.category;
					this._feedTitle = e.Feed.title;
					this._resourceUrl = e.Feed.link;
				}

				this._resourceUIText = e.FullTitle;
				this._publisherHomepage = e.PublisherHomepage;
				this._techContact = e.TechnicalContact;
				this._publisher = e.Publisher;
				this._generator = e.Generator;
				
				if (e.InnerException is XmlException)
					this.InitWith(ownerFeed, (XmlException)e.InnerException);
				else if (e.InnerException is System.Net.WebException)
					this.InitWith(ownerFeed, (System.Net.WebException)e.InnerException);
				else if (e.InnerException is System.Net.ProtocolViolationException)
					this.InitWith(ownerFeed, (System.Net.ProtocolViolationException)e.InnerException);
				else
					this.InitWith(ownerFeed, e.InnerException);
			}

			private void InitWith(ExceptionManager ownerFeed, XmlException e) {
				this.InitWith(ownerFeed, e, Resource.Manager["RES_XmlExceptionCategory"]);
			}

			private void InitWith(ExceptionManager ownerFeed, System.Net.WebException e) {
				this.InitWith(ownerFeed, e, Resource.Manager["RES_WebExceptionCategory"]);
			}

			private void InitWith(ExceptionManager ownerFeed, System.Net.ProtocolViolationException e) {
				this.InitWith(ownerFeed, e, Resource.Manager["RES_ProtocolViolationExceptionCategory"]);
			}

			private void InitWith(ExceptionManager ownerFeed, Exception e) {
				this.InitWith(ownerFeed, e, Resource.Manager["RES_ExceptionCategory"]);
			}

			private void InitWith(ExceptionManager ownerFeed, Exception e, string categoryName) {
				FeedInfo fi = new FeedInfo(String.Empty, ownerFeed.Items , ownerFeed.title, ownerFeed.link, ownerFeed.Description );
				// to prevent confusing about daylight saving time and to work similar to RssComponts, that save item DateTime's
				// as GMT, we convert DateTime to universal to be conform
				DateTime exDT = new DateTime(DateTime.Now.Ticks).ToUniversalTime();
				bool enableValidation = (e is XmlException);

				string link = this.BuildBaseLink(e, enableValidation); 
				_delegateTo = new NewsItem(ownerFeed, this._feedTitle, link,
					this.BuildBaseDesc(e, enableValidation), 
					exDT, categoryName,
					ContentType.Xhtml, new Hashtable(), link, null );

				_delegateTo.FeedDetails = fi;
				_delegateTo.BeenRead = false;
			}

			public string ID { get {return _ID; }	}
			public NewsItem NewsItemInstance { 
				get { 
					NewsItem ri = (NewsItem)_delegateTo.Clone(); 
					ri.FeedDetails = _delegateTo.FeedDetails;	// not cloned!
					return ri;
				} 
			}



			/// <summary>
			/// Used to test whether a string contains any character that is illegal in XML 1.0. 
			/// Specifically it checks for the ASCII control characters except for tab, carriage return 
			/// and newline which are the only ones allowed in XML. 
			/// </summary>
			/// <param name="errorMessage">the string to test</param>
			/// <returns>true if the string contains a character that is illegal in XML</returns>
			private bool ContainsInvalidXmlCharacter(string errorMessage){
			
				foreach(char c in errorMessage){				
					if(Char.IsControl(c) && !c.Equals('\t') && !c.Equals('\r') && !c.Equals('\n')){
						return true; 
					}
				}

				return false; 

			}

			private string BuildBaseDesc(Exception e, bool provideXMLValidationUrl) {
				StringBuilder s = new StringBuilder();
				XmlTextWriter writer = new XmlTextWriter(new StringWriter(s)); 
				writer.Formatting = Formatting.Indented; 

				string msg = e.Message; 

				if((e is XmlException) && ContainsInvalidXmlCharacter(e.Message)){
					msg = e.Message.Substring(5);
				}

				if (msg.IndexOf("<") >= 0) {
					msg = System.Web.HttpUtility.HtmlEncode(msg);
				}

				writer.WriteRaw(Resource.Manager["RES_RefreshFeedExceptionReportStringPart", this._resourceUIText, msg]);
				if (provideXMLValidationUrl && this._resourceUrl.StartsWith("http")) {
					writer.WriteRaw("<br />");
					writer.WriteStartElement("a");
					writer.WriteAttributeString("href", validationUrlBase+this._resourceUrl);
					writer.WriteRaw(Resource.Manager["RES_RefreshFeedExceptionReportValidationPart"]);
					writer.WriteEndElement();	// </a>
				}

				if (this._publisher.Length > 0 || this._techContact.Length > 0 || this._publisherHomepage.Length > 0 ) {
					writer.WriteStartElement("p");
					writer.WriteString(Resource.Manager["RES_RefreshFeedExceptionReportContactInfo"]);
					writer.WriteStartElement("ul");
					if (this._publisher.Length > 0) {
						writer.WriteStartElement("li");
						writer.WriteString(Resource.Manager["RES_RefreshFeedExceptionReportManagingEditor"]+": ");
						if (StringHelper.IsEMailAddress(this._publisher)) {
							string mail = StringHelper.GetEMailAddress(this._publisher);
							writer.WriteStartElement("a");
							writer.WriteAttributeString("href", "mailto:"+mail);
							writer.WriteString(mail);
							writer.WriteEndElement();	// </a>
						} else {
							writer.WriteString(this._publisher);
						}
						writer.WriteEndElement();	// </li>
					}
					if (this._techContact.Length > 0) {
						writer.WriteStartElement("li");
						writer.WriteString(Resource.Manager["RES_RefreshFeedExceptionReportWebMaster"]+": ");
						if (StringHelper.IsEMailAddress(this._techContact)) {
							string mail = StringHelper.GetEMailAddress(this._techContact);
							writer.WriteStartElement("a");
							writer.WriteAttributeString("href", "mailto:"+mail);
							writer.WriteString(mail);
							writer.WriteEndElement();	// </a>
						} else {
							writer.WriteString(this._techContact);
						}
						writer.WriteEndElement();	// </li>
					}
					if (this._publisherHomepage.Length > 0) {
						writer.WriteStartElement("li");
						writer.WriteString(Resource.Manager["RES_RefreshFeedExceptionReportHomePage"]+": ");
						writer.WriteStartElement("a");
						writer.WriteAttributeString("href", this._publisherHomepage);
						writer.WriteString(this._publisherHomepage);
						writer.WriteEndElement();	// </a>
						writer.WriteEndElement();	// </li>
					}
					writer.WriteEndElement();	// </ul>
					if (this._generator .Length > 0) {
						writer.WriteString(Resource.Manager["RES_RefreshFeedExceptionGeneratorStringPart", this._generator]);
					}
					writer.WriteEndElement();	// </p>
				}

				return s.ToString();
			}


			private string BuildBaseLink(Exception e, bool provideXMLValidationUrl) {
				string sLink = String.Empty;
				//TODO: may be, we can later use the e.HelpLink also...
				// setup "Read On..." link
				if (this._fullErrorInfoFile.Length == 0) {
					if (this._resourceUrl.StartsWith("http")) {
						sLink = (provideXMLValidationUrl ? validationUrlBase : String.Empty) + this._resourceUrl;
					} else {
						sLink = this._resourceUrl;
					}
				} else {
					sLink = this._fullErrorInfoFile;	
				}

				return sLink;
			}

		}

	}


}
