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
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using log4net;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Net;
using NewsComponents.Utils;
using RssBandit;
using RssBandit.Common.Logging;
using RssBandit.Resources;
using RssBandit.Common;
using Logger = RssBandit.Common.Logging;

namespace RssBandit.SpecialFeeds
{

	#region FlaggedItemsFeed

	/// <summary>
	/// The local feed for flagged items
	/// </summary>
	internal class FlaggedItemsFeed:LocalFeedsFeed
	{
		private const string MigrationKey = "FlaggedItemsFeed.migrated.to.1.7";
		// as long the FlagStatus of NewsItem's wasn't persisted all the time, 
		// we have to re-init the feed item's FlagStatus from the flagged items collection:
		private const string SelfHealingFlagStatusKey = "RunSelfHealing.FlagStatus";

		private readonly bool runSelfHealingFlagStatus;

		/// <summary>
		/// Initializes a new instance of the <see cref="FlaggedItemsFeed"/> class.
		/// </summary>
		/// <param name="migratedItemsOwner">The migrated items owner.</param>
		public FlaggedItemsFeed(FeedSourceEntry migratedItemsOwner): 
			this(migratedItemsOwner, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FlaggedItemsFeed"/> class.
		/// </summary>
		/// <param name="migratedItemsOwner">The migrated items owner.</param>
		/// <param name="reader">The reader.</param>
		public FlaggedItemsFeed(FeedSourceEntry migratedItemsOwner, XmlReader reader) :
			base(migratedItemsOwner, RssBanditApplication.GetFlagItemsFileName(),
			     SR.FeedNodeFlaggedFeedsCaption,
			     SR.FeedNodeFlaggedFeedsDesc, false)
		{
			runSelfHealingFlagStatus = (bool)RssBanditApplication.PersistedSettings.GetProperty(
			                                 	SelfHealingFlagStatusKey, typeof(bool), true);

			// set this to indicate required migration:
			migrationRequired = runSelfHealingFlagStatus || (bool)RssBanditApplication.PersistedSettings.GetProperty(
			                                                      	MigrationKey, typeof(bool), true);

			if (reader == null)
				reader = this.GetDefaultReader();

			using (reader)
				LoadItems(reader, migratedItemsOwner);
		}

		/// <summary>
		/// Overridden to migrate an item.
		/// Base implementation just return the <paramref name="newsItem"/>.
		/// </summary>
		/// <param name="newsItem">The news item.</param>
		/// <param name="migratedItemsOwner">The migrated items owner.</param>
		/// <returns></returns>
		protected override NewsItem MigrateItem(NewsItem newsItem, FeedSourceEntry migratedItemsOwner)
		{
			if (migratedItemsOwner != null)
			{
				if (runSelfHealingFlagStatus)
				{
					if (newsItem.FlagStatus == Flagged.None)
					{
						// correction: older Bandit versions are not able to store flagStatus
						newsItem.FlagStatus = Flagged.FollowUp;
						Modified = true;
					}
				}

				XmlQualifiedName key = AdditionalElements.GetQualifiedName(
					OptionalItemElement.OldVersion.FeedUrlElementName, OptionalItemElement.OldVersion.Namespace);
				string feedUrl = AdditionalElements.GetElementValue(key, newsItem);
				if (!String.IsNullOrEmpty(feedUrl))
				{
					if (runSelfHealingFlagStatus && migratedItemsOwner.Source.IsSubscribed(feedUrl))
					{
						//check if feed exists 
						IList<INewsItem> itemsForFeed = migratedItemsOwner.Source.GetItemsForFeed(feedUrl, false);

						//find this item 
						int itemIndex = itemsForFeed.IndexOf(newsItem);

						if (itemIndex != -1)
						{
							//check if item still exists 
							INewsItem item = itemsForFeed[itemIndex];
							if (item.FlagStatus != newsItem.FlagStatus)
							{
								// correction: older Bandit versions are not able to store flagStatus
								item.FlagStatus = newsItem.FlagStatus;
								Modified = true;
								//FeedWasModified(feedUrl, NewsFeedProperty.FeedItemFlag); // self-healing
							}
						}
					}

					AdditionalElements.RemoveElement(key, newsItem);
					OptionalItemElement.AddOrReplaceOriginalFeedReference(newsItem, feedUrl, migratedItemsOwner.ID);
					Modified = true;
				}
			}

			return base.MigrateItem(newsItem, migratedItemsOwner);
		}

		/// <summary>
		/// Called when items are loaded.
		/// </summary>
		/// <param name="loadSucceeds">if set to <c>true</c> [load succeeds].</param>
		protected override void OnItemsLoaded(bool loadSucceeds)
		{
			if (migrationRequired && loadSucceeds)
				RssBanditApplication.PersistedSettings.SetProperty(MigrationKey, false);
		}
	}

	#endregion

	#region WatchedItemsFeed

	/// <summary>
	/// The local feed for watched items
	/// </summary>
	internal class WatchedItemsFeed : LocalFeedsFeed
	{
		private const string MigrationKey = "WatchedItemsFeed.migrated.to.1.7";
		
		/// <summary>
		/// Initializes a new instance of the <see cref="WatchedItemsFeed"/> class.
		/// </summary>
		/// <param name="migratedItemsOwner">The migrated items owner.</param>
		public WatchedItemsFeed(FeedSourceEntry migratedItemsOwner) :
			base(migratedItemsOwner, RssBanditApplication.GetWatchedItemsFileName(),
			     SR.FeedNodeWatchedItemsCaption,
			     SR.FeedNodeWatchedItemsDesc, false)
		{
			// set this to indicate required migration:
			migrationRequired = (bool)RssBanditApplication.PersistedSettings.GetProperty(
			                          	MigrationKey, typeof(bool), true);
			
			using (XmlReader reader = this.GetDefaultReader())
				LoadItems(reader, migratedItemsOwner);
		}

		/// <summary>
		/// Overridden to migrate an item.
		/// Base implementation just return the <paramref name="newsItem"/>.
		/// </summary>
		/// <param name="newsItem">The news item.</param>
		/// <param name="migratedItemsOwner">The migrated items owner.</param>
		/// <returns></returns>
		protected override NewsItem MigrateItem(NewsItem newsItem, FeedSourceEntry migratedItemsOwner)
		{
			if (migratedItemsOwner != null)
			{
				XmlQualifiedName key =
					AdditionalElements.GetQualifiedName(
						OptionalItemElement.OldVersion.FeedUrlElementName, OptionalItemElement.OldVersion.Namespace);
				string feedUrl = AdditionalElements.GetElementValue(key, newsItem);
				if (!String.IsNullOrEmpty(feedUrl))
				{
					AdditionalElements.RemoveElement(key, newsItem);
					OptionalItemElement.AddOrReplaceOriginalFeedReference(newsItem, feedUrl, migratedItemsOwner.ID);
					Modified = true;
				}
			}
			return base.MigrateItem(newsItem, migratedItemsOwner);
		}

		/// <summary>
		/// Called when items are loaded.
		/// </summary>
		/// <param name="loadSucceeds">if set to <c>true</c> [load succeeds].</param>
		protected override void OnItemsLoaded(bool loadSucceeds)
		{
			if (migrationRequired && loadSucceeds)
				RssBanditApplication.PersistedSettings.SetProperty(MigrationKey, false);
		}
	}

	#endregion

	#region SentItemsFeed

	/// <summary>
	/// The local feed for sent items
	/// </summary>
	internal class SentItemsFeed : LocalFeedsFeed
	{
		private const string MigrationKey = "SentItemsFeed.migrated.to.1.7";
		
		/// <summary>
		/// Initializes a new instance of the <see cref="SentItemsFeed"/> class.
		/// </summary>
		/// <param name="migratedItemsOwner">The migrated items owner.</param>
		public SentItemsFeed(FeedSourceEntry migratedItemsOwner) :
			this(migratedItemsOwner, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SentItemsFeed"/> class.
		/// </summary>
		/// <param name="migratedItemsOwner">The migrated items owner.</param>
		/// <param name="reader">The reader.</param>
		public SentItemsFeed(FeedSourceEntry migratedItemsOwner, XmlReader reader) :
			base(migratedItemsOwner, RssBanditApplication.GetSentItemsFileName(),
			     SR.FeedNodeSentItemsCaption,
			     SR.FeedNodeSentItemsDesc, false)
		{
			// set this to indicate required migration:
			migrationRequired = (bool)RssBanditApplication.PersistedSettings.GetProperty(
			                          	MigrationKey, typeof(bool), true);
			
			if (reader == null)
				reader = this.GetDefaultReader();
			using (reader)
				LoadItems(reader, migratedItemsOwner);
		}

		/// <summary>
		/// Overridden to migrate an item.
		/// Base implementation just return the <paramref name="newsItem"/>.
		/// </summary>
		/// <param name="newsItem">The news item.</param>
		/// <param name="migratedItemsOwner">The migrated items owner.</param>
		/// <returns></returns>
		protected override NewsItem MigrateItem(NewsItem newsItem, FeedSourceEntry migratedItemsOwner)
		{
			if (migratedItemsOwner != null)
			{
				XmlQualifiedName key =
					AdditionalElements.GetQualifiedName(
						OptionalItemElement.OldVersion.FeedUrlElementName, OptionalItemElement.OldVersion.Namespace);
				string feedUrl = AdditionalElements.GetElementValue(key, newsItem);
				if (!String.IsNullOrEmpty(feedUrl))
				{
					AdditionalElements.RemoveElement(key, newsItem);
					OptionalItemElement.AddOrReplaceOriginalFeedReference(newsItem, feedUrl, migratedItemsOwner.ID);
					Modified = true;
				}
			}
			return base.MigrateItem(newsItem, migratedItemsOwner);
		}

		/// <summary>
		/// Called when items are loaded.
		/// </summary>
		/// <param name="loadSucceeds">if set to <c>true</c> [load succeeds].</param>
		protected override void OnItemsLoaded(bool loadSucceeds)
		{
			if (migrationRequired && loadSucceeds)
				RssBanditApplication.PersistedSettings.SetProperty(MigrationKey, false);
		}
	}

	#endregion

	#region DeletedItemsFeed

	/// <summary>
	/// The local feed for deleted items
	/// </summary>
	internal class DeletedItemsFeed : LocalFeedsFeed
	{
		private const string MigrationKey = "DeletedItemsFeed.migrated.to.1.7";
		
		/// <summary>
		/// Initializes a new instance of the <see cref="DeletedItemsFeed"/> class.
		/// </summary>
		/// <param name="migratedItemsOwner">The migrated items owner.</param>
		public DeletedItemsFeed(FeedSourceEntry migratedItemsOwner) :
			base(migratedItemsOwner, RssBanditApplication.GetDeletedItemsFileName(),
			     SR.FeedNodeDeletedItemsCaption,
			     SR.FeedNodeDeletedItemsDesc, false)
		{
			// set this to indicate required migration:
			migrationRequired = (bool)RssBanditApplication.PersistedSettings.GetProperty(
			                          	MigrationKey, typeof(bool), true);

			using (XmlReader reader = this.GetDefaultReader())
				LoadItems(reader, migratedItemsOwner);
		}

		/// <summary>
		/// Overridden to migrate an item.
		/// Base implementation just return the <paramref name="newsItem"/>.
		/// </summary>
		/// <param name="newsItem">The news item.</param>
		/// <param name="migratedItemsOwner">The migrated items owner.</param>
		/// <returns></returns>
		protected override NewsItem MigrateItem(NewsItem newsItem, FeedSourceEntry migratedItemsOwner)
		{
			if (migratedItemsOwner != null)
			{
				XmlQualifiedName key =
					AdditionalElements.GetQualifiedName(
						OptionalItemElement.OldVersion.ContainerUrlElementName, OptionalItemElement.OldVersion.Namespace);
				string feedUrl = AdditionalElements.GetElementValue(key, newsItem);
				if (!String.IsNullOrEmpty(feedUrl))
				{
					AdditionalElements.RemoveElement(key, newsItem);
					OptionalItemElement.AddOrReplaceOriginalFeedReference(newsItem, feedUrl, migratedItemsOwner.ID);
					Modified = true;
				}
			}
			return base.MigrateItem(newsItem, migratedItemsOwner);
		}

		/// <summary>
		/// Called when items are loaded.
		/// </summary>
		/// <param name="loadSucceeds">if set to <c>true</c> [load succeeds].</param>
		protected override void OnItemsLoaded(bool loadSucceeds)
		{
			if (migrationRequired && loadSucceeds)
				RssBanditApplication.PersistedSettings.SetProperty(MigrationKey, false);
		}
	}

	#endregion

	#region UnreadItemsFeed

	/// <summary>
	/// The local (virtual, non-persisted) feed for unread items
	/// </summary>
	internal class UnreadItemsFeed : LocalFeedsFeed
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UnreadItemsFeed"/> class.
		/// </summary>
		public UnreadItemsFeed(FeedSourceEntry migratedItemsOwner) :
			base(migratedItemsOwner, "virtualfeed://rssbandit.org/local/unreaditems",
			     SR.FeedNodeUnreadItemsCaption,
			     SR.FeedNodeUnreadItemsDesc,
			     false)
		{
		}
	}

	#endregion

	#region ExceptionManager (LocalFeedsFeed)

	/// <summary>
	/// Thread safe ExceptionManager to handle and report
	/// Feed errors and other errors that needs to be published 
	/// to a user. Singleton is implemented 
	/// (see also http://www.yoda.arachsys.com/csharp/beforefieldinit.html).
	/// </summary>
	internal sealed class ExceptionManager : LocalFeedsFeed
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="ExceptionManager"/> class.
		/// </summary>
		/// <param name="feedUrl">The feed URL.</param>
		/// <param name="feedTitle">The feed title.</param>
		/// <param name="feedDescription">The feed description.</param>
		public ExceptionManager(string feedUrl, string feedTitle, string feedDescription)
			: base(null, feedUrl, feedTitle, feedDescription, false)
		{
			try
			{
				Save();	// re-create a new file with no items
			}
			catch (Exception ex)
			{
				Log.Fatal("ExceptionManager.Save() failed", ex);
			}
		}

		/// <summary>
		/// Adds the specified exception.
		/// </summary>
		/// <param name="e">The exception.</param>
		/// <param name="entry">The entry.</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Add(Exception e, FeedSourceEntry entry)
		{
			FeedException fe = new FeedException(this, e, entry);
			Add(fe.NewsItemInstance);
			try
			{
				Save();
			}
			catch (Exception ex)
			{
				Log.Fatal("ExceptionManager.Save() failed", ex);
			}
		}

		/// <summary>
		/// Removes the entries for the specified feed URL.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="feedUrl">The feed URL.</param>
		public void RemoveFeed(FeedSourceEntry entry, string feedUrl)
		{

			if (string.IsNullOrEmpty(feedUrl) || feedInfo.ItemsList.Count == 0)
				return;

			Stack removeAtIndex = new Stack();
			for (int i = 0; i < feedInfo.ItemsList.Count; i++)
			{
				INewsItem n = feedInfo.ItemsList[i];
				if (n != null)
				{
					int sourceID;
					string feedUrlRef = OptionalItemElement.GetOriginalFeedReference(n, out sourceID);
					if (!String.IsNullOrEmpty(feedUrlRef) && sourceID == entry.ID)
						removeAtIndex.Push(i);

					//XmlElement xe = RssHelper.GetOptionalElement(n, AdditionalElements.OriginalFeedOfErrorItem);
					//if (xe != null && xe.InnerText == feedUrl)
					//{
					//    removeAtIndex.Push(i);
					//    break;
					//}

				}
			}

			while (removeAtIndex.Count > 0)
				feedInfo.ItemsList.RemoveAt((int)removeAtIndex.Pop());
		}

		/// <summary>
		/// Gets the items.
		/// </summary>
		/// <value>The items.</value>
		public new IList<INewsItem> Items
		{
			get { return feedInfo.ItemsList; }
		}

		/// <summary>
		/// Returns a instance of ExceptionManager.
		/// </summary>
		/// <returns>Instance of the ExceptionManageer class</returns>
		public static ExceptionManager GetInstance()
		{
			return InstanceHelper.instance;
		}

		/// <summary>
		/// Private instance helper class to impl. Singleton
		/// </summary>
		private class InstanceHelper
		{
			// Explicit static constructor to tell C# compiler
			// not to mark type as beforefieldinit
			static InstanceHelper() { ;}
			internal static readonly ExceptionManager instance = new ExceptionManager(
				RssBanditApplication.GetFeedErrorFileName(),
				SR.FeedNodeFeedExceptionsCaption,
				SR.FeedNodeFeedExceptionsDesc);
		}

		public class FeedException
		{

			private static int idCounter;

			private NewsItem _delegateTo;	// cannot inherit, so we use the old containment

			// used to build up a informational error description
			private readonly string _ID;
			private string _feedCategory = String.Empty;
			private string _feedTitle = String.Empty;
			private string _resourceUrl = String.Empty;
			private string _resourceUIText = String.Empty;
			private string _publisherHomepage = String.Empty;
			private string _publisher = String.Empty;
			private string _techContact = String.Empty;
			private string _generator = String.Empty;
			private readonly string _fullErrorInfoFile = String.Empty;
			private readonly string _feedSourceName = String.Empty;
			private readonly int _feedSourceID = 0;


			/// <summary>
			/// Initializes a new instance of the <see cref="FeedException"/> class.
			/// </summary>
			/// <param name="ownerFeed">The owner feed.</param>
			/// <param name="e">The e.</param>
			/// <param name="entry">The entry.</param>
			public FeedException(ExceptionManager ownerFeed, Exception e, FeedSourceEntry entry)
			{

				idCounter++;
				this._ID = String.Concat("#", idCounter.ToString());

				if (entry != null)
				{
					_feedSourceName = entry.Name;
					_feedSourceID = entry.ID;
				}

				if (e is FeedRequestException)
					this.InitWith(ownerFeed, (FeedRequestException)e);
				else if (e is XmlException)
					this.InitWith(ownerFeed, (XmlException)e);
				else if (e is WebException)
					this.InitWith(ownerFeed, (WebException)e);
				else if (e is ProtocolViolationException)
					this.InitWith(ownerFeed, (ProtocolViolationException)e);
				else
					this.InitWith(ownerFeed, e);
			}

			private void InitWith(ExceptionManager ownerFeed, FeedRequestException e)
			{
				if (e.Feed != null)
				{
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
				else if (e.InnerException is WebException)
					this.InitWith(ownerFeed, (WebException)e.InnerException);
				else if (e.InnerException is ProtocolViolationException)
					this.InitWith(ownerFeed, (ProtocolViolationException)e.InnerException);
				else
					this.InitWith(ownerFeed, e.InnerException);
			}

			private void InitWith(ExceptionManager ownerFeed, XmlException e)
			{
				this.InitWith(ownerFeed, e, SR.XmlExceptionCategory);
			}

			private void InitWith(ExceptionManager ownerFeed, WebException e)
			{
				this.InitWith(ownerFeed, e, SR.WebExceptionCategory);
			}

			private void InitWith(ExceptionManager ownerFeed, ProtocolViolationException e)
			{
				this.InitWith(ownerFeed, e, SR.ProtocolViolationExceptionCategory);
			}

			private void InitWith(ExceptionManager ownerFeed, Exception e)
			{
				this.InitWith(ownerFeed, e, SR.ExceptionCategory);
			}

			private void InitWith(ExceptionManager ownerFeed, Exception e, string categoryName)
			{
				FeedInfo fi = new FeedInfo(null, String.Empty, ownerFeed.Items, ownerFeed.title, ownerFeed.link, ownerFeed.Description);
				// to prevent confusing about daylight saving time and to work similar to RssComponts, that save item DateTime's
				// as GMT, we convert DateTime to universal to be conform
				DateTime exDT = new DateTime(DateTime.Now.Ticks).ToUniversalTime();
				bool enableValidation = (e is XmlException);

				string link = this.BuildBaseLink(e, enableValidation);
				_delegateTo = new NewsItem(ownerFeed, this._feedTitle, link,
										   this.BuildBaseDesc(e, enableValidation),
										   exDT, categoryName,
										   ContentType.Xhtml, new Dictionary<XmlQualifiedName, string>(1), link, null);

				_delegateTo.FeedDetails = fi;
				_delegateTo.BeenRead = false;
				OptionalItemElement.AddOrReplaceOriginalFeedReference(
					_delegateTo, this._resourceUrl, this._feedSourceID);	
			}

			/// <summary>
			/// Gets the ID.
			/// </summary>
			/// <value>The ID.</value>
			public string ID { get { return _ID; } }
			
			/// <summary>
			/// Gets the news item instance.
			/// </summary>
			/// <value>The news item instance.</value>
			public INewsItem NewsItemInstance
			{
				get
				{
					INewsItem ri = (INewsItem)_delegateTo.Clone();
					ri.FeedDetails = _delegateTo.FeedDetails;	// not cloned!
					return ri;
				}
			}

			private string BuildBaseDesc(Exception e, bool provideXMLValidationUrl)
			{
				StringBuilder s = new StringBuilder();
				XmlTextWriter writer = new XmlTextWriter(new StringWriter(s));
				writer.Formatting = Formatting.Indented;

				string msg = e.Message;

				if ((e is XmlException) && StringHelper.ContainsInvalidXmlChars(e.Message))
				{
					msg = e.Message.Substring(5);
				}

				if (msg.IndexOf("<") >= 0)
				{
					msg = HtmlHelper.HtmlEncode(msg);
				}

				writer.WriteStartElement("p");
				writer.WriteRaw(String.Format(SR.RefreshFeedExceptionReportStringPart, this._resourceUIText, msg));
				writer.WriteEndElement();	// </p>

				if (this._publisher.Length > 0 || this._techContact.Length > 0 || this._publisherHomepage.Length > 0)
				{
					writer.WriteStartElement("p");
					writer.WriteString(SR.RefreshFeedExceptionReportContactInfo);
					writer.WriteStartElement("ul");
					if (this._publisher.Length > 0)
					{
						writer.WriteStartElement("li");
						writer.WriteString(SR.RefreshFeedExceptionReportManagingEditor + ": ");
						if (StringHelper.IsEMailAddress(this._publisher))
						{
							string mail = StringHelper.GetEMailAddress(this._publisher);
							writer.WriteStartElement("a");
							writer.WriteAttributeString("href", "mailto:" + mail);
							writer.WriteString(mail);
							writer.WriteEndElement();	// </a>
						}
						else
						{
							writer.WriteString(this._publisher);
						}
						writer.WriteEndElement();	// </li>
					}
					if (this._techContact.Length > 0)
					{
						writer.WriteStartElement("li");
						writer.WriteString(SR.RefreshFeedExceptionReportWebMaster + ": ");
						if (StringHelper.IsEMailAddress(this._techContact))
						{
							string mail = StringHelper.GetEMailAddress(this._techContact);
							writer.WriteStartElement("a");
							writer.WriteAttributeString("href", "mailto:" + mail);
							writer.WriteString(mail);
							writer.WriteEndElement();	// </a>
						}
						else
						{
							writer.WriteString(this._techContact);
						}
						writer.WriteEndElement();	// </li>
					}
					if (this._publisherHomepage.Length > 0)
					{
						writer.WriteStartElement("li");
						writer.WriteString(SR.RefreshFeedExceptionReportHomePage + ": ");
						writer.WriteStartElement("a");
						writer.WriteAttributeString("href", this._publisherHomepage);
						writer.WriteString(this._publisherHomepage);
						writer.WriteEndElement();	// </a>
						writer.WriteEndElement();	// </li>
					}
					writer.WriteEndElement();	// </ul>
					if (this._generator.Length > 0)
					{
						writer.WriteString(String.Format(SR.RefreshFeedExceptionGeneratorStringPart, this._generator));
					}
					writer.WriteEndElement();	// </p>

				}//if (this._publisher.Length > 0 || this._techContact.Length > 0 || this._publisherHomepage.Length > 0 )

				// render actions section:
				writer.WriteStartElement("p");
				writer.WriteString(SR.RefreshFeedExceptionUserActionIntroText);
				// List:
				writer.WriteStartElement("ul");
				// Validate entry (optional):
				if (provideXMLValidationUrl && this._resourceUrl.StartsWith("http"))
				{
					writer.WriteStartElement("li");
					writer.WriteStartElement("a");
					writer.WriteAttributeString("href", RssBanditApplication.FeedValidationUrlBase + this._resourceUrl);
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


			private string BuildBaseLink(Exception e, bool provideXMLValidationUrl)
			{
				string sLink = String.Empty;
				if (e.HelpLink != null)
				{
					//TODO: may be, we can later use the e.HelpLink
					// to setup "Read On..." link
				}
				if (this._fullErrorInfoFile.Length == 0)
				{
					if (this._resourceUrl.StartsWith("http"))
					{
						sLink = (provideXMLValidationUrl ? RssBanditApplication.FeedValidationUrlBase : String.Empty) + this._resourceUrl;
					}
					else
					{
						sLink = this._resourceUrl;
					}
				}
				else
				{
					sLink = this._fullErrorInfoFile;
				}

				return sLink;
			}

		}

	}

	#endregion

	#region LocalFeedsFeed (base class)

	/// <summary>
	/// Special local feeds base class
	/// </summary>
	internal class LocalFeedsFeed:NewsFeed {

		/// <summary>
		/// Logger instance
		/// </summary>
		protected static readonly ILog _log = Log.GetLogger(typeof(LocalFeedsFeed));

		/// <summary>
		/// Set this to true, to indicate a item migration is required.
		/// If true, the method <see cref="MigrateItem"/> is called for every loaded item
		/// during the load items process.
		/// </summary>
		protected bool migrationRequired;

		private string filePath;
		/// <summary>
		/// Gets/Sets the FeedInfo
		/// </summary>
		protected FeedInfo feedInfo;
		private bool modified;

		/// <summary>
		/// Initializes a new instance of the <see cref="LocalFeedsFeed"/> class.
		/// </summary>
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
		/// <param name="migratedItemsOwner">The migrated items owner.</param>
		/// <param name="feedUrl">The feed URL.</param>
		/// <param name="feedTitle">The feed title.</param>
		/// <param name="feedDescription">The feed description.</param>
		/// <exception cref="UriFormatException">If <paramref name="feedUrl"/> could not be formatted as a Uri</exception>
		public LocalFeedsFeed(FeedSourceEntry migratedItemsOwner,string feedUrl, string feedTitle, string feedDescription):
			this(migratedItemsOwner, feedUrl, feedTitle, feedDescription, true)
		{
		}

		/// <summary>
		/// Initializer.
		/// </summary>
		/// <param name="migratedItemsOwner">The migrated items owner.</param>
		/// <param name="feedUrl">The feed URL.</param>
		/// <param name="feedTitle">The feed title.</param>
		/// <param name="feedDescription">The feed description.</param>
		/// <param name="loadItems">bool</param>
		/// <exception cref="UriFormatException">If <paramref name="feedUrl"/> could not be formatted as a Uri</exception>
		public LocalFeedsFeed(FeedSourceEntry migratedItemsOwner, string feedUrl, string feedTitle, string feedDescription, bool loadItems):this() {
			filePath = feedUrl;
			try {
				Uri feedUri = new Uri(feedUrl);
				base.link = feedUri.CanonicalizedUri();
			} catch {
				base.link = feedUrl;
			}
			base.title = feedTitle;
			
			this.feedInfo = new FeedInfo(null, filePath, new List<INewsItem>(), feedTitle, base.link, feedDescription);
			
			if (loadItems)
				using (XmlReader reader = this.GetDefaultReader())
					LoadItems(reader, migratedItemsOwner);
		}

		/// <summary>
		/// Initializer.
		/// </summary>
		/// <param name="migratedItemsOwner">The migrated items owner.</param>
		/// <param name="feedUrl">local file path</param>
		/// <param name="feedTitle">The feed title.</param>
		/// <param name="feedDescription">The feed description.</param>
		/// <param name="reader">The reader.</param>
		/// <exception cref="UriFormatException">If <paramref name="feedUrl"/> could not be formatted as a Uri</exception>
		public LocalFeedsFeed(FeedSourceEntry migratedItemsOwner, string feedUrl, string feedTitle, string feedDescription, XmlReader reader) :
			this(migratedItemsOwner, feedUrl, feedTitle, feedDescription, false)
		{
			LoadItems(reader, migratedItemsOwner);
		}

		/// <summary>
		/// Gets or sets the items.
		/// </summary>
		/// <value>The items.</value>
		public List<INewsItem> Items {
			get { return this.feedInfo.ItemsList;  }
			set { this.feedInfo.ItemsList = new List<INewsItem>(value); }
		}

		/// <summary>
		/// Adds the specified LFF.
		/// </summary>
		/// <param name="lff">The LFF.</param>
		public void Add(LocalFeedsFeed lff){
			foreach(INewsItem item in lff.Items){
				if(!this.feedInfo.ItemsList.Contains(item)){
					this.Add(item); 
				}
			}		
		}

		/// <summary>
		/// Adds the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		public void Add(INewsItem item) {
			if (item == null)
				return;
			
			item.FeedDetails = this.feedInfo;
			this.feedInfo.ItemsList.Add(item);
			this.modified = true;
		}

		/// <summary>
		/// Removes the specified item.
		/// </summary>
		/// <param name="item">The item.</param>
		public void Remove(INewsItem item) {
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


		/// <summary>
		/// Removes the item(s) with the specified comment feed URL.
		/// </summary>
		/// <param name="commentFeedUrl">The comment feed URL.</param>
		public void Remove(string commentFeedUrl){
			if(!string.IsNullOrEmpty(commentFeedUrl)){
			
				for(int i = 0; i < this.feedInfo.ItemsList.Count; i++){
					INewsItem ni = feedInfo.ItemsList[i]; 
					if(!StringHelper.EmptyTrimOrNull(ni.CommentRssUrl) && ni.CommentRssUrl.Equals(commentFeedUrl)){
						this.feedInfo.ItemsList.RemoveAt(i); 
						break; 
					}
				}
			}				
		}

		/// <summary>
		/// Gets or sets the location.
		/// </summary>
		/// <value>The location.</value>
		public string Location {
			get { return this.filePath;	}
			set { this.filePath = value;	}
		}

		/// <summary>
		/// Gets the URL.
		/// </summary>
		/// <value>The URL.</value>
		public string Url {
			get { return base.link;  }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="LocalFeedsFeed"/> is modified.
		/// </summary>
		/// <value><c>true</c> if modified; otherwise, <c>false</c>.</value>
		public bool Modified {
			get { return this.modified;  }
			set { this.modified = value; }
		}

		/// <summary>
		/// Gets the description.
		/// </summary>
		/// <value>The description.</value>
		public string Description {
			get { return this.feedInfo.Description;  }
		}

		/// <summary>
		/// Loads the items.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="migratedItemsOwner">The migrated items owner.</param>
		protected void LoadItems(XmlReader reader, FeedSourceEntry migratedItemsOwner){
			if (feedInfo.ItemsList.Count > 0)
				feedInfo.ItemsList.Clear();

			bool success = false;
			if (reader != null) 
			{
				try{				
					XmlDocument doc = new XmlDocument(); 
					doc.Load(reader); 
					foreach(XmlElement elem in doc.SelectNodes("//item")){				
						NewsItem item = RssParser.MakeRssItem(this,  new XmlNodeReader(elem)); 
						
						//Question: why we do this?
						item.BeenRead = true;
						
						item.FeedDetails = this.feedInfo;
						if (migrationRequired)
							item = MigrateItem(item, migratedItemsOwner);
						
						if (item != null)
							feedInfo.ItemsList.Add(item); 
					}

					success = true;
				}catch(Exception e){
					
					ExceptionManager.GetInstance().Add(RssBanditApplication.CreateLocalFeedRequestException(e, this, this.feedInfo), migratedItemsOwner);
				}			
			}

			OnItemsLoaded(success);
		}

		/// <summary>
		/// Should be overridden to migrate the item. 
		/// Base implementation just return the <paramref name="newsItem"/>.
		/// </summary>
		/// <param name="newsItem">The news item.</param>
		/// <param name="migratedItemsOwner">The migrated items owner.</param>
		/// <returns></returns>
		protected virtual NewsItem MigrateItem(NewsItem newsItem, FeedSourceEntry migratedItemsOwner)
		{
			return newsItem;
		}

		/// <summary>
		/// Called when items are loaded.
		/// </summary>
		/// <param name="loadSucceeds">if set to <c>true</c> [load succeeds].</param>
		protected virtual void OnItemsLoaded(bool loadSucceeds)
		{
		}

		/// <summary>
		/// Gets the default reader.
		/// </summary>
		/// <returns></returns>
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

		/// <summary>
		/// Saves the feed items.
		/// </summary>
		public void Save()
		{
			try
			{
				using (XmlTextWriter writer = new XmlTextWriter(new StreamWriter(this.filePath)))
				{
					this.WriteTo(writer);
					writer.Flush();
					Modified = false;
				}
			}
			catch (Exception e)
			{
				_log.Error("LocalFeedsFeed.Save()", e);
			}
		}

	}

	#endregion

}
