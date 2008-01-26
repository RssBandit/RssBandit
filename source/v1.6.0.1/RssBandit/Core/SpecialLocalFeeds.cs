#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

#region CVS Version Log
/*
 * $Log: SpecialLocalFeeds.cs,v $
 * Revision 1.46  2007/06/15 11:02:01  t_rendelmann
 * new: enabled user failure actions within feed error reports (validate, navigate to, delete subscription)
 *
 * Revision 1.45  2007/02/11 15:58:53  carnage4life
 * 1.) Added proper handling for when a podcast download exceeds the size limit on the podcast folder
 *
 * Revision 1.44  2006/12/08 17:00:22  t_rendelmann
 * fixed: flag a item with no content did not show content anymore in the flagged item view (linked) (was regression because of the dynamic load of item content from cache);
 * fixed: "View Outlook Reading Pane" was not working correctly (regression from the toolbars migration);
 *
 * Revision 1.43  2006/11/01 16:03:53  t_rendelmann
 * small optimizations
 *
 * Revision 1.42  2006/08/18 19:10:57  t_rendelmann
 * added an "id" XML attribute to the feedsFeed. We need it to make the feed items (feeditem.id + feed.id) unique to enable progressive indexing (lucene)
 *
 */
#endregion

using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Net;
using NewsComponents.Utils;
using RssBandit;
using RssBandit.Resources;
using Logger = RssBandit.Common.Logging;

namespace RssBandit.SpecialFeeds
{

	#region specialized local feedsFeed
		
	/// <summary>
	/// Special local feeds
	/// </summary>
	public class LocalFeedsFeed:feedsFeed {
		
		private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(LocalFeedsFeed));

        
		private string filePath = null;
		protected FeedInfo feedInfo = null;
		private string description = null;
		private bool modified = false;

		public LocalFeedsFeed() {
			base.refreshrate = 0;		
			base.refreshrateSpecified = true;
			this.feedInfo = new FeedInfo(null, null, null);
			// required, to make items with no content work:
			base.cacheurl = "non.cached.feed";
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
				base.link = feedUri.AbsoluteUri;
			} catch {
				base.link = feedUrl;
			}
			base.title = feedTitle;
			
			this.feedInfo = new FeedInfo(null, filePath, new List<NewsItem>(), feedTitle, base.link, feedDescription);
			
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

		public List<NewsItem> Items {
			get { return this.feedInfo.ItemsList;  }
			set { this.feedInfo.ItemsList = new List<NewsItem>(value); }
		}

		public void Add(LocalFeedsFeed lff){
			foreach(NewsItem item in lff.Items){
				if(!this.feedInfo.ItemsList.Contains(item)){
					this.Add(item); 
				}
			}		
		}

		public void Add(NewsItem item) {
			if (item == null)
				return;
			
			item.FeedDetails = this.feedInfo;
			this.feedInfo.ItemsList.Add(item);
			this.modified = true;
		}

		public void Remove(NewsItem item) {
			if (item != null)
			{
				int index = this.feedInfo.ItemsList.IndexOf(item);
				if (index >= 0)
				{
					this.feedInfo.ItemsList.RemoveAt(index);
					this.modified = true;
				}
			}
		}


		public void Remove(string commentFeedUrl){
			if(!string.IsNullOrEmpty(commentFeedUrl)){
			
				for(int i = 0; i < this.feedInfo.ItemsList.Count; i++){
					NewsItem ni = feedInfo.ItemsList[i] as NewsItem; 
					if(ni.CommentRssUrl.Equals(commentFeedUrl)){
						this.feedInfo.ItemsList.RemoveAt(i); 
						break; 
					}
				}
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
			if (feedInfo.ItemsList.Count > 0)
				feedInfo.ItemsList.Clear(); 

			if (reader != null) {
				try{				
					XmlDocument doc = new XmlDocument(); 
					doc.Load(reader); 
					foreach(XmlElement elem in doc.SelectNodes("//item")){				
						NewsItem item = RssParser.MakeRssItem(this,  new XmlNodeReader(elem)); 
						item.BeenRead = true;
						item.FeedDetails = this.feedInfo;
						feedInfo.ItemsList.Add(item); 
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
			base.Add(fe.NewsItemInstance);
			try {
				base.Save();
			} catch (Exception ex) {
				Common.Logging.Log.Fatal("ExceptionManager.Save() failed", ex);
			}
		}

		/// <summary>
		/// Removes the entries for the specified feed URL.
		/// </summary>
		/// <param name="feedUrl">The feed URL.</param>
		public void RemoveFeed(string feedUrl) {
			
			if (string.IsNullOrEmpty(feedUrl) || base.feedInfo.ItemsList.Count == 0)
				return;
			
			Stack removeAtIndex = new Stack();
			for (int i = 0; i < base.feedInfo.ItemsList.Count; i++) {
				NewsItem n = base.feedInfo.ItemsList[i] as NewsItem;
				if (n != null) {
					XmlElement xe = RssHelper.GetOptionalElement(n, AdditionalFeedElements.OriginalFeedOfErrorItem);
					if (xe != null && xe.InnerText == feedUrl) {
						removeAtIndex.Push(i);
						break;
					}
					
				}
			}

			while (removeAtIndex.Count > 0)
				base.feedInfo.ItemsList.RemoveAt((int)removeAtIndex.Pop());
		}

		public new IList<NewsItem> Items {
			get { return base.feedInfo.ItemsList; }
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
				SR.FeedNodeFeedExceptionsCaption, 
				SR.FeedNodeFeedExceptionsDesc);
		}

		public class FeedException {
			
			private static int idCounter = 0;

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
				this.InitWith(ownerFeed, e, SR.XmlExceptionCategory);
			}

			private void InitWith(ExceptionManager ownerFeed, System.Net.WebException e) {
				this.InitWith(ownerFeed, e, SR.WebExceptionCategory);
			}

			private void InitWith(ExceptionManager ownerFeed, System.Net.ProtocolViolationException e) {
				this.InitWith(ownerFeed, e, SR.ProtocolViolationExceptionCategory);
			}

			private void InitWith(ExceptionManager ownerFeed, Exception e) {
				this.InitWith(ownerFeed, e, SR.ExceptionCategory);
			}

			private void InitWith(ExceptionManager ownerFeed, Exception e, string categoryName) {
				FeedInfo fi = new FeedInfo(null, String.Empty, ownerFeed.Items , ownerFeed.title, ownerFeed.link, ownerFeed.Description );
				// to prevent confusing about daylight saving time and to work similar to RssComponts, that save item DateTime's
				// as GMT, we convert DateTime to universal to be conform
				DateTime exDT = new DateTime(DateTime.Now.Ticks).ToUniversalTime();
				bool enableValidation = (e is XmlException);

				string link = this.BuildBaseLink(e, enableValidation); 
				_delegateTo = new NewsItem(ownerFeed, this._feedTitle, link,
					this.BuildBaseDesc(e, enableValidation), 
					exDT, categoryName,
					ContentType.Xhtml, CreateAdditionalElements(this._resourceUrl), link, null );

				_delegateTo.FeedDetails = fi;
				_delegateTo.BeenRead = false;
			}

			private Hashtable CreateAdditionalElements(string errorCausingFeedUrl) {
				Hashtable r = new Hashtable();
				// add a optional element to remember the original feed container (for later ref)
				if (null != errorCausingFeedUrl) {
					XmlElement originalFeed = RssHelper.CreateXmlElement(
						AdditionalFeedElements.ElementPrefix, 
						AdditionalFeedElements.OriginalFeedOfErrorItem.Name, 
						AdditionalFeedElements.OriginalFeedOfErrorItem.Namespace, 
						errorCausingFeedUrl); 
					
					r.Add(AdditionalFeedElements.OriginalFeedOfErrorItem, 
						originalFeed.OuterXml);
				}
				return r;
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

				writer.WriteStartElement("p");
				writer.WriteRaw(SR.RefreshFeedExceptionReportStringPart(this._resourceUIText, msg));
				writer.WriteEndElement();	// </p>
				
				if (this._publisher.Length > 0 || this._techContact.Length > 0 || this._publisherHomepage.Length > 0 ) 
				{
					writer.WriteStartElement("p");
					writer.WriteString(SR.RefreshFeedExceptionReportContactInfo);
					writer.WriteStartElement("ul");
					if (this._publisher.Length > 0) {
						writer.WriteStartElement("li");
						writer.WriteString(SR.RefreshFeedExceptionReportManagingEditor + ": ");
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
						writer.WriteString(SR.RefreshFeedExceptionReportWebMaster + ": ");
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
						writer.WriteString(SR.RefreshFeedExceptionReportHomePage + ": ");
						writer.WriteStartElement("a");
						writer.WriteAttributeString("href", this._publisherHomepage);
						writer.WriteString(this._publisherHomepage);
						writer.WriteEndElement();	// </a>
						writer.WriteEndElement();	// </li>
					}
					writer.WriteEndElement();	// </ul>
					if (this._generator .Length > 0) {
						writer.WriteString(SR.RefreshFeedExceptionGeneratorStringPart(this._generator));
					}
					writer.WriteEndElement();	// </p>
					
				}//if (this._publisher.Length > 0 || this._techContact.Length > 0 || this._publisherHomepage.Length > 0 )
				
				// render actions section:
				writer.WriteStartElement("p");
				writer.WriteString(SR.RefreshFeedExceptionUserActionIntroText);
				// List:
				writer.WriteStartElement("ul");
				// Validate entry (optional):
				if (provideXMLValidationUrl && this._resourceUrl.StartsWith("http")) {
					writer.WriteStartElement("li");
					writer.WriteStartElement("a");
					writer.WriteAttributeString("href", RssBanditApplication.FeedValidationUrlBase+this._resourceUrl);
					writer.WriteRaw(SR.RefreshFeedExceptionReportValidationPart);
					writer.WriteEndElement();	// </a>
					writer.WriteEndElement();	// </li>
				}
				// Show/Navigate to feed subscription:
				writer.WriteStartElement("li");
				writer.WriteStartElement("a");
				// fdaction:?action=navigatetofeed&feedid=id-of-feed
				writer.WriteAttributeString("href", String.Format("fdaction:?action=navigatetofeed&feedid={0}", HtmlHelper.UrlEncode(this._resourceUrl)));
				writer.WriteRaw(SR.RefreshFeedExceptionUserActionNavigateToSubscription);
				writer.WriteEndElement();	// </a>
				writer.WriteEndElement();	// </li>
				// Remove feed subscription:
				writer.WriteStartElement("li");
				writer.WriteStartElement("a");
				// fdaction:?action=unsubscribefeed&feedid=id-of-feed
				writer.WriteAttributeString("href", String.Format("fdaction:?action=unsubscribefeed&feedid={0}", HtmlHelper.UrlEncode(this._resourceUrl)));
				writer.WriteRaw(SR.RefreshFeedExceptionUserActionDeleteSubscription);
				writer.WriteEndElement();	// </a>
				writer.WriteEndElement();	// </li>
					
				writer.WriteEndElement();	// </ul>
				writer.WriteEndElement();	// </p>
				
				return s.ToString();
			}


			private string BuildBaseLink(Exception e, bool provideXMLValidationUrl) {
				string sLink = String.Empty;
				if (e.HelpLink != null) {
					//TODO: may be, we can later use the e.HelpLink
					// to setup "Read On..." link
				}
				if (this._fullErrorInfoFile.Length == 0) {
					if (this._resourceUrl.StartsWith("http")) {
						sLink = (provideXMLValidationUrl ? RssBanditApplication.FeedValidationUrlBase : String.Empty) + this._resourceUrl;
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
