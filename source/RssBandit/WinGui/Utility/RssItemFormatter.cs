#region CVS Version Header
/*
 * $Id: RssItemFormatter.cs,v 1.34 2007/06/07 02:04:18 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2007/06/07 02:04:18 $
 * $Revision: 1.34 $
 */
#endregion

using System;
using System.Threading;
using System.Xml;
using System.Xml.XPath; 
using System.Xml.Xsl;
using System.IO;
using System.Windows.Forms;
using System.Collections.Specialized;

using NewsComponents;
using NewsComponents.Feed;
using RssBandit.Exceptions;
using RssBandit.Resources;

namespace RssBandit.WinGui.Utility
{
	/// <summary>
	/// Summary description for NewsItemFormatter.
	/// </summary>
	public class NewsItemFormatter {

		public event FeedExceptionEventArgs.EventHandler TransformError;
		public event ExceptionEventArgs.EventHandler StylesheetError;
		public event ExceptionEventArgs.EventHandler StylesheetValidationError;

		static private string _defaultTmpl = null;

		static private string _searchTmpl  = null; 

		public const string SearchTemplateId = ":*<>?"; 

		//the table of XSLT transforms
		private ListDictionary stylesheetTable = new ListDictionary(); 

		static NewsItemFormatter() {
			//_defaultTmpl = Resource.Manager["XSLT_DefaultTemplate"];
			using (Stream xsltStream = Resource.GetStream("Resources.DefaultTemplate.xslt")) {
				_defaultTmpl = new StreamReader(xsltStream).ReadToEnd();	
			}

			using (Stream xsltStream = Resource.GetStream("Resources.SearchResultsTemplate.xslt")) {
				_searchTmpl = new StreamReader(xsltStream).ReadToEnd();	
			}
		}

		public NewsItemFormatter():this(String.Empty, _defaultTmpl) {
			this.AddXslStyleSheet(SearchTemplateId, _searchTmpl); 
		}
		public NewsItemFormatter(string xslStyleSheetName, string xslStyleSheet) {		
			this.AddXslStyleSheet(xslStyleSheetName, xslStyleSheet);
		}
		

		/// <summary>
		/// Tests whether a particular stylesheet is contained within the item formatter
		/// </summary>
		/// <param name="name">The name of the stylesheet</param>
		/// <returns>Tests whether the </returns>
		public bool ContainsXslStyleSheet(string name){
		 return this.stylesheetTable.Contains(name); 
		}

		/// <summary>
		/// Set/Get the currently used XSLT stylesheet to be used to format
		/// an NewsItem for the detail display.
		/// </summary>
		/// <exception cref="XmlException"></exception>
		/// <exception cref="XsltException"></exception>
		public void AddXslStyleSheet(string name, string stylesheet){
			
			XslTransform transform = new XslTransform();

			try {
					
				if(name == null){
					name = String.Empty;
				}

				if (stylesheet == null || stylesheet.Length == 0) {
					stylesheet = DefaultNewsItemTemplate;
				}	
					
				//FeedDemon styles use an $IMAGEDIR$ magic variable which we should account for
				stylesheet = stylesheet.Replace("$IMAGEDIR$", Path.Combine(Application.StartupPath,@"templates\images\")); 
						
				transform.Load(new XmlTextReader(new StringReader(stylesheet))); 	
				
				if(this.stylesheetTable.Contains(name)){
					this.stylesheetTable.Remove(name); 
				}

				this.stylesheetTable.Add(name, transform); 
				
			}catch (XsltCompileException e){
				stylesheet = DefaultNewsItemTemplate;
				//transform.Load(new XmlTextReader(new StringReader(stylesheet))); 
				this.OnStylesheetError(this, new ExceptionEventArgs(e, SR.ExceptionNewsItemFormatterStylesheetCompile));
				if(!(e.InnerException is ThreadAbortException)){				
					RssBanditApplication.PublishException(new BanditApplicationException("Error in AddXslStyleSheet()", e)); 				
				}
			}catch (XmlException e){
				stylesheet = DefaultNewsItemTemplate;
				//transform.Load(new XmlTextReader(new StringReader(stylesheet))); 				
				this.OnStylesheetError(this, new ExceptionEventArgs(e, SR.ExceptionNewsItemFormatterStylesheetCompile));
				if(!(e.InnerException is ThreadAbortException)){
					RssBanditApplication.PublishException(new BanditApplicationException("Error in AddXslStyleSheet()", e)); 					
				}
			}
		}


		/// <summary>
		/// Transform a NewsItem, FeedInfo or FeedInfoList using the specified stylesheet
		/// </summary>
		/// <param name="stylesheet">The stylesheet to use for transformation</param>
		/// <param name="transformTarget">The object to transform</param>
		/// <param name="xslArgs"></param>
		/// <returns>The results of the transformation</returns>
		public virtual string ToHtml(string stylesheet, object transformTarget, XsltArgumentList xslArgs) {

			NewsItemSerializationFormat format = NewsItemSerializationFormat.NewsPaper;
			string link = String.Empty, content = String.Empty;
			
			if (transformTarget == null) 
				return "<html><head><title>empty</title></head><body></body></html>";

			// use a streamed output to get the "disable-output-escaping" 
			// working:
			StringWriter swr = new StringWriter();

			try	{
				XPathDocument doc = null; 
				
				if(transformTarget is NewsItem){
					NewsItem item = (NewsItem) transformTarget;
					link = item.FeedLink;
					content = item.Content;
					doc = new XPathDocument(new XmlTextReader(new StringReader(item.ToString(format, false))), XmlSpace.Preserve);				
				}else if(transformTarget is FeedInfo){
					FeedInfo feed = (FeedInfo) transformTarget;
					link = feed.Link;
					doc = new XPathDocument(new XmlTextReader(new StringReader(feed.ToString(format, false))), XmlSpace.Preserve);				
				}else if(transformTarget is FeedInfoList){
					FeedInfoList feeds = (FeedInfoList) transformTarget;
					doc = new XPathDocument(new XmlTextReader(new StringReader(feeds.ToString())), XmlSpace.Preserve);				
				}else{
					throw new ArgumentException("transformTarget"); 
				}

				XslTransform transform = null; 
				
				if(this.stylesheetTable.Contains(stylesheet)){
					transform = (XslTransform) this.stylesheetTable[stylesheet];
				}else{
					transform = (XslTransform) this.stylesheetTable[String.Empty];
				}	
				
				// support simple localizations (some common predefined strings to display):
				xslArgs.AddExtensionObject("urn:localization-extension", new LocalizerExtensionObject());
				transform.Transform(doc, xslArgs, swr);

			} catch (ThreadAbortException) {
				// ignored
			}	catch (Exception e)	{
				this.OnTransformationError(this, new FeedExceptionEventArgs(e, link, SR.ExceptionNewsItemTransformation));
				return content;	// try to display unformatted simple text
			}
		
			return swr.ToString();
		}
		

		/* THIS METHOD DOESN'T WORK CORRECTLY
		public virtual XmlDocument ToXml(NewsItem item) {
			bool standalone = false;

			XmlDocument doc = new XmlDocument();
			if (item == null) {
				doc.LoadXml("<html><head><title>empty</title></head><body></body></html>");			
				return doc;
			}

			try	{
				 
				if (DesignMode){ // for template tests
					StreamWriter sw = new StreamWriter(Path.Combine(Path.GetTempPath(), @"NewsItem.xml")); 
					sw.Write(item.ToString(standalone)); 
					sw.Close(); 
				}
			}	catch {} 

			try	{
				StringBuilder sb = new StringBuilder(); 				
				_transform.Transform(item.CreateNavigator(standalone), null, new StringWriter(sb));
				doc.LoadXml(sb.ToString());
			}	catch (Exception e)	{
				this.OnTransformationError(this, new FeedExceptionEventArgs(e, item.Feed.link, SR.ExceptionNewsItemTransformation"]));
				// try to display unformatted simple text
				doc.LoadXml("<html><head><title>" + item.Title + "</title></head><body><![CDATA[" + item.Description + "]]></body></html>");			
			}
		
			return doc;
		}*/


		public static string DefaultNewsItemTemplate	{
			get {	return _defaultTmpl; }
		}

		protected void OnTransformationError(object sender, FeedExceptionEventArgs e) {
			if (TransformError != null)
				foreach (FeedExceptionEventArgs.EventHandler eh in TransformError.GetInvocationList())
					eh.BeginInvoke(sender, e, null, null);
		}

		protected void OnStylesheetError(object sender, ExceptionEventArgs e) {
			if (StylesheetError != null)
				foreach (ExceptionEventArgs.EventHandler eh in StylesheetError.GetInvocationList())
					eh.BeginInvoke(this, e, null, null);
		}

		protected void OnStylesheetValidationError(object sender, ExceptionEventArgs e) {
			if (StylesheetValidationError != null)
				foreach (ExceptionEventArgs.EventHandler eh in StylesheetValidationError.GetInvocationList())
					eh.BeginInvoke(this, e, null, null);
		}

		#region LocalizerExtensionObject class

		/// <summary>
		/// Xslt Transformation extension to provide localized strings 
		/// to Xslt templates
		/// </summary>
		public class LocalizerExtensionObject
		{
			
			/// <summary>
			/// Gets the localized related links text.
			/// </summary>
			/// <returns></returns>
			public string RelatedLinksText(){
				return SR.XsltDefaultTemplate_RelatedLinks;
			}

			/// <summary>
			/// Returns the localized previous text.
			/// </summary>
			/// <returns></returns>
			public string PreviousPageText(){
				return SR.XsltDefaultTemplate_Previous;
			}

			/// <summary>
			/// Returns the localized next text.
			/// </summary>
			/// <returns></returns>
			public string NextPageText(){
				return SR.XsltDefaultTemplate_Next;
			}

			/// <summary>
			/// Returns the localized displaying page text.
			/// </summary>
			/// <returns></returns>
			public string DisplayingPageText(){
				return SR.XsltDefaultTemplate_Displaying_page;
			}

			/// <summary>
			/// Returns the localized of text.
			/// </summary>
			/// <returns></returns>
			public string PageOfText(){
				return SR.XsltDefaultTemplate_of;
			}

			/// <summary>
			/// Gets the localized item publisher text.
			/// </summary>
			/// <returns></returns>
			public string ItemPublisherText() {
				return SR.XsltDefaultTemplate_ItemPublisher;	
			}

			/// <summary>
			/// Gets the localized item author text.
			/// </summary>
			/// <returns></returns>
			public string ItemAuthorText() {
				return SR.XsltDefaultTemplate_ItemAuthor;
			}

			/// <summary>
			/// Gets the localized item date text.
			/// </summary>
			/// <returns></returns>
			public string ItemDateText() {
				return SR.XsltDefaultTemplate_ItemDate;
			}

			/// <summary>
			/// Gets the localized item enclosure text.
			/// </summary>
			/// <returns></returns>
			public string ItemEnclosureText() {
				return SR.XsltDefaultTemplate_ItemEnclosure;
			}
			/// <summary>
			/// Gets the localized text to indicate toggle of flag states.
			/// </summary>
			/// <returns></returns>
			public string ToggleFlagStateText() {
				return SR.XsltDefaultTemplate_ToggleFlagState;
			}

			/// <summary>
			/// Gets the localized text to indicate toggle of read state.
			/// </summary>
			/// <returns></returns>
			public string ToggleReadStateText() {
				return SR.XsltDefaultTemplate_ToggleReadState;
			}

			/// <summary>
			/// Gets the localized text to indicate toggle of watched state.
			/// </summary>
			/// <returns></returns>
			public string ToggleWatchStateText() {
				return SR.XsltDefaultTemplate_ToggleWatchState;
			}
		}

		#endregion
	}

	
}
