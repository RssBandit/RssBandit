#region CVS Version Header
/*
 * $Id: ImportFilter.cs,v 1.4 2005/01/07 00:54:20 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2005/01/07 00:54:20 $
 * $Revision: 1.4 $
 */
#endregion

using System;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Resources;
using System.IO;

namespace NewsComponents.Utils {

	/// <summary>
	/// Supported feed formats to import
	/// </summary>
	public enum ImportFeedFormat
	{
		/// <summary>
		/// Unknown or not supported
		/// </summary>
		Unknown,
		/// <summary>
		/// Native RSS Bandit import format
		/// </summary>
		Bandit,
		/// <summary>
		/// Open Content Syndication. See http://internetalchemy.org/ocs/
		/// </summary>
		OCS,
		/// <summary>
		/// Outline Processor Markup Language, see http://opml.scripting.com/spec
		/// </summary>
		OPML,
		/// <summary>
		/// Synchronization of Information Aggregators using Markup (SIAM),
		/// see http://www.25hoursaday.com/draft-obasanjo-siam-01.html
		/// </summary>
		SIAM
	}

	/// <summary>
	/// The ImportFilter class provides methods to assist in importing Rss Feed lists from other formats.
	/// </summary>
	public class ImportFilter
	{
		#region Private Variables
		private XmlDocument _feedList;
		private ImportFeedFormat _feedFormat;
		#endregion

		#region Filter Name Definitions
		
		// these values correspond to the names used as the name of the Xslt in the resource file
		private static string FILTER_FORMAT_OPML	= "OPML";
		private static string FILTER_FORMAT_OCS		= "OCS";
		private static string FILTER_FORMAT_SIAM	= "SIAM";

		#endregion

		#region Constructors
		/// <summary>
		/// This ctor instantiates a new ImportFilter without initializing the feed list.
		/// Sets this.Format = ImportFeedFormat.Unknown
		/// </summary>
		public ImportFilter()
		{
			this._feedList = null;
			this._feedFormat = ImportFeedFormat.Unknown;
		}
		/// <summary>
		/// Instantiates a new ImportFilter from an XmlDocument containing the
		/// feed list
		/// </summary>
		/// <param name="FeedList">System.Xml.XmlDocument containing the Xml of the feed list to process</param>
		public ImportFilter(XmlDocument FeedList)
		{
			this._feedList = FeedList;
			this._feedFormat = this.DetectFormat();
		}
		/// <summary>
		/// Instantiates a new ImportFilter from a string containing Xml text
		/// of the feed list
		/// </summary>
		/// <param name="FeedList">System.String containing Xml text of the feed list to process</param>
		public ImportFilter(string FeedList)
		{
			this._feedList = new XmlDocument();
			this._feedList.LoadXml(FeedList);
			this._feedFormat = this.DetectFormat();
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Get/Set the feedlist as a XmlDocument.
		/// </summary>
		public XmlDocument FeedList
		{
			get{return this._feedList;}
			set
			{
				this._feedList = value;
				// detect the format of the new feed
				this._feedFormat = this.DetectFormat();
			}
		}

		/// <summary>
		/// State variable representing the format of the feed.  This is valued
		/// upon instantiation using the parameterized constructors.
		/// It is also valued after the GetImportXsl() method is called or when the 
		/// value of FeedList is changed.
		/// </summary>
		public ImportFeedFormat Format
		{
			get{return (ImportFeedFormat)this._feedFormat;}
			set{this._feedFormat = value;}
		}

		#endregion

		#region Methods
		/// <summary>
		/// Get the filter (XslTransform) to use for import.
		/// </summary>
		/// <returns>System.Xml.Xsl.XslTransform containing the transform that will convert the input to the native feedlist format</returns>
		public XslTransform GetImportXsl()
		{
			string _formatName = String.Empty;

			if(this._feedFormat == ImportFeedFormat.Unknown)
				this._feedFormat = this.DetectFormat();
			
			switch((ImportFeedFormat)this._feedFormat)
			{
				case ImportFeedFormat.OPML:
					_formatName = FILTER_FORMAT_OPML;
					break;
				case ImportFeedFormat.OCS:
					_formatName = FILTER_FORMAT_OCS;
					break;
				case ImportFeedFormat.SIAM:
					_formatName = FILTER_FORMAT_SIAM;
					break;
				case ImportFeedFormat.Bandit:
				case ImportFeedFormat.Unknown:
				default:
					return null;
			}

			// Open the resource and build our XslTransform

			using (Stream _xsltStream = Resource.Manager.GetStream(String.Format("Resources.feedImportFilters.{0}.xslt", _formatName))) {
				XslTransform _xslt = new XslTransform();
				_xslt.Load(new XmlTextReader(_xsltStream));
				return _xslt;			
			}
//
//			// TODO: Find a better way to get the Assembly short name (ex: NewsComponents)
//			string assemblyName = this.GetType().Assembly.FullName;
//			string resName = assemblyName.Substring(0,assemblyName.IndexOf(",")) + ".feedImportFilters";
//			ResourceManager _importFilterRM = new ResourceManager(resName, this.GetType().Assembly);
//			_importFilterRM.IgnoreCase = true;
//			
//			// we can't load a string directly into an XslTransform, so we'll
//			// need to do a little conversion
//			string _xslText = _importFilterRM.GetString(_formatName);
//			System.IO.StringReader _sr = new System.IO.StringReader(_xslText);			
//			XslTransform _xslt = new XslTransform();
//			_xslt.Load(new XmlTextReader(_sr));
//
//			return _xslt;			
			
		}

		/// <summary>
		/// Detects the format of the feed list.
		/// </summary>
		/// <returns>System.String indicating the format.  If the format can not be determined, then this value
		/// will be ImportFeedFormat.Unknown</returns>
		public ImportFeedFormat DetectFormat()
		{
			#region TO DO - Future Enhancements/ideas
			/* TODO: This needs to spawn a thread for each format we're looking for.
			 * the thread will then use a callback to indicate that it has completed a positive match
			 * on the format that it's responsible for.  Then we'll spin-down the other
			 * threads and continue on.
			 * 
			 * Detection of a feed list format should eventually be done using either a string
			 * or an object implementing IXPathNavigable.  It will need to use a regular expression
			 * for matching the format.
			 * 
			 * This will require that we create an IImportFilter interface so we can have pluggable
			 * import formats.  Each assembly used for an ImportFilter will need to have it's own built-in XSLT
			 * (either by using a subfolder, internal static string, or .resx file).
			 * 
			 * This introduces an interesting problem, since resources are managed outside of this class library.
			 */
			#endregion
			
			// Bandit Feed detection
			if((this._feedList.DocumentElement.NamespaceURI == "http://www.25hoursaday.com/2003/RSSBandit/feeds/")
			   || (this._feedList.DocumentElement.NamespaceURI == "http://www.25hoursaday.com/2004/RSSBandit/feeds/")){
				return ImportFeedFormat.Bandit;
			}else if(this._feedList.DocumentElement.LocalName.Equals("opml")){// OPML Detection
				return ImportFeedFormat.OPML;
			}else if(this._feedList.DocumentElement.NamespaceURI == "http://groups.yahoo.com/information_aggregators/2004/01/siam/"){
				return ImportFeedFormat.SIAM;
			}else{ // Unknown or OCS

				foreach(XmlAttribute attr in this._feedList.DocumentElement.Attributes){
					if(attr.Value.Equals("http://InternetAlchemy.org/ocs/directory#"))
						return ImportFeedFormat.OCS;
				}  

				return ImportFeedFormat.Unknown;	
			}
		}

		#endregion

	}
}
