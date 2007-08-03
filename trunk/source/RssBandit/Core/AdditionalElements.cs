#region CVS Version Header
/*
 * $Id: AdditionalElements.cs,v 1.5 2006/12/14 18:52:20 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2006/12/14 18:52:20 $
 * $Revision: 1.5 $
 */
#endregion

using System.Xml;

namespace RssBandit
{
	/// <summary>
	/// AdditionalFeedElements contains the RSS Bandit owned
	/// additional feed XML elements/attributes and namespaces
	/// used to annotate, keep relations between feeds etc.
	/// </summary>
	internal sealed class AdditionalFeedElements
	{
		#region Const defs.

		/// <summary>
		/// Gets the common namespace used for RSS Bandit extensions
		/// </summary>
		//TODO: check: do we should use the v2004/vCurrent namespace instead?
		public const string Namespace = NewsComponents.NamespaceCore.Feeds_v2003;
		
		/// <summary>
		/// Gets the former common namespace extension prefix
		/// </summary>
		public const string OldElementPrefix = "bandit";

		/// <summary>
		/// Gets the common namespace extension prefix
		/// </summary>
		public const string ElementPrefix = "rssbandit";

		/// <summary>
		/// Gets the element name used to store flag state of new items
		/// </summary>
		public const string FlaggedElementName = "feed-url";

		/// <summary>
		/// Gets the element name used to keep the feed url of deleted news items 
		/// </summary>
		public const string DeletedElementName = "container-url";

		/// <summary>
		/// Gets the element name used to keep the feed url of failed feeds 
		/// (messages stored as news items) 
		/// </summary>
		public const string ErrorElementName = "failed-url";
		#endregion

		private static volatile XmlQualifiedName _originalFeedOfFlaggedItem = null;
		private static volatile XmlQualifiedName _originalFeedOfDeletedItem = null;
		private static volatile XmlQualifiedName _originalFeedOfErrorItem = null;
		private static volatile XmlQualifiedName _originalFeedOfWatchedItem = null;

		private static object creationLock = new object();

		#region public properties

		/// <summary>
		/// Gets the <see cref="XmlQualifiedName"/> used to reference 
		/// the original feed of flagged items.
		/// </summary>
		/// <value>XmlQualifiedName</value>
		public static XmlQualifiedName OriginalFeedOfFlaggedItem {
			get {
				if (_originalFeedOfFlaggedItem == null) {
					lock(creationLock) {
						_originalFeedOfFlaggedItem = new XmlQualifiedName(FlaggedElementName, Namespace);
					}
				}
				return _originalFeedOfFlaggedItem;
			}
		}  

		/// <summary>
		/// Gets the <see cref="XmlQualifiedName"/> used to reference 
		/// the original feed of deleted items.
		/// </summary>
		/// <value>XmlQualifiedName</value>
		public static XmlQualifiedName OriginalFeedOfDeletedItem {
			get {
				if (_originalFeedOfDeletedItem == null) {
					lock(creationLock) {
						_originalFeedOfDeletedItem = new XmlQualifiedName(DeletedElementName, Namespace); 
					}
				}
				return _originalFeedOfDeletedItem;
			}
		} 


		/// <summary>
		/// Gets the <see cref="XmlQualifiedName"/> used to reference 
		/// the original feed of watched items.
		/// </summary>
		/// <value>XmlQualifiedName</value>
		public static XmlQualifiedName OriginalFeedOfWatchedItem {
			get {
				if (_originalFeedOfWatchedItem == null) {
					lock(creationLock) {
						_originalFeedOfWatchedItem = new XmlQualifiedName(FlaggedElementName, Namespace); 
					}
				}
				return _originalFeedOfWatchedItem;
			}
		} 

		/// <summary>
		/// Gets the <see cref="XmlQualifiedName"/> used to reference 
		/// the original feed of deleted items.
		/// </summary>
		/// <value>XmlQualifiedName</value>
		public static XmlQualifiedName OriginalFeedOfErrorItem {
			get {
				if (_originalFeedOfErrorItem == null) {
					lock(creationLock) {
						_originalFeedOfErrorItem = new XmlQualifiedName(ErrorElementName, Namespace); 
					}
				}
				return _originalFeedOfErrorItem;
			}
		}

		#endregion

		private AdditionalFeedElements() {}
	}
}

#region CVS Version Log
/*
 * $Log: AdditionalElements.cs,v $
 * Revision 1.5  2006/12/14 18:52:20  carnage4life
 * Removed redundant 'Subscribe in defautl aggregator' option added to IE right-click menu
 *
 * Revision 1.4  2006/12/07 23:18:54  carnage4life
 * Fixed issue where Feed Title column in Smart Folders does not show the original feed
 *
 * Revision 1.3  2006/10/05 08:01:55  t_rendelmann
 * refactored: use string constants for our XML namespaces
 *
 */
#endregion
