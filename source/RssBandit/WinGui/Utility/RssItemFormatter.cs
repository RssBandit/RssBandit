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
using System.Threading;
using System.Xml;
using System.Xml.XPath; 
using System.Xml.Xsl;
using System.IO;
using System.Windows.Forms;

using NewsComponents;
using NewsComponents.Feed;
using RssBandit.Common.Logging;
using RssBandit.Exceptions;
using RssBandit.Resources;

namespace RssBandit.WinGui.Utility
{
	/// <summary>
	/// NewsItemFormatter manages stylesheets and news item 
	/// transformation/formatting to HTML.
	/// </summary>
	public class NewsItemFormatter {

		public event EventHandler<FeedExceptionEventArgs> TransformError;
		public event EventHandler<ExceptionEventArgs> StylesheetError;
		public event EventHandler<ExceptionEventArgs> StylesheetValidationError;

		static private readonly string _defaultTmpl;

		static private readonly string _searchTmpl;      

		public const string SearchTemplateId = ":*<>?"; 

		//the item of the table of XSLT stylesheets
        struct StylesheetDescriptor
        {
            /// <summary>
            /// The XSL Compiled Transform object
            /// </summary>
            public XslCompiledTransform CompiledTransform;
            /// <summary>
            /// The full path to the stylesheet used.
            /// Can e used to resolve relative images, etc.
            /// </summary>
            public string Location;
        }

        private readonly IDictionary<string, StylesheetDescriptor> stylesheetTable = new Dictionary<string, StylesheetDescriptor>(17); 

		static NewsItemFormatter()
		{
			_defaultTmpl = Properties.Resources.DefaultTemplate_xslt;
			_searchTmpl = Properties.Resources.SearchResultsTemplate_xslt;
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
		 return this.stylesheetTable.ContainsKey(name); 
		}

		/// <summary>
		/// Set/Get the currently used XSLT stylesheet to be used to format
		/// an NewsItem for the detail display.
		/// </summary>
		/// <exception cref="XmlException"></exception>
		/// <exception cref="XsltException"></exception>
		public void AddXslStyleSheet(string name, string stylesheet){

            XslCompiledTransform transform = new XslCompiledTransform();

			try {
					
				if(name == null){
					name = String.Empty;
				}

				if (string.IsNullOrEmpty(stylesheet)) {
					stylesheet = DefaultNewsItemTemplate;
				}	
					
				//FeedDemon styles use an $IMAGEDIR$ magic variable which we should account for
				//stylesheet = stylesheet.Replace("$IMAGEDIR$", Path.Combine(Application.StartupPath,@"templates\images\")); 
				//Uri toImages;
				//Uri.TryCreate(Path.Combine(Application.StartupPath, @"templates\images\"), UriKind.Absolute, out toImages);
				//stylesheet = stylesheet.Replace("$IMAGEDIR$", toImages.AbsoluteUri); 
				stylesheet = stylesheet.Replace("$IMAGEDIR$", "file:///templates/images/"); 


                transform.Load(new XmlTextReader(new StringReader(stylesheet))); 	
				
				if(this.stylesheetTable.ContainsKey(name)){
					this.stylesheetTable.Remove(name); 
				}

				this.stylesheetTable.Add(name, new StylesheetDescriptor {Location = name, CompiledTransform  = transform}); 
				
			}catch (XsltCompileException e)
			{
				this.OnStylesheetError(this, new ExceptionEventArgs(e, SR.ExceptionNewsItemFormatterStylesheetCompile));
			}
			catch (XsltException e)
			{
				this.OnStylesheetError(this, new ExceptionEventArgs(e, SR.ExceptionNewsItemFormatterInvalidStylesheet));
			}
			catch (XmlException e)
			{
				this.OnStylesheetError(this, new ExceptionEventArgs(e, SR.ExceptionNewsItemFormatterStylesheetMessage));
			}
			catch (Exception e)
			{
				Log.Error("AddXslStyleSheet() caused unexpected error", e);
			}
		}


		/// <summary>
		/// Transform a NewsItem, FeedInfo or FeedInfoList using the specified stylesheet
		/// </summary>
		/// <param name="stylesheet">The stylesheet to use for transformation</param>
		/// <param name="transformTarget">The object to transform</param>
		/// <param name="xslArgs"></param>
		/// <returns>The results of the transformation</returns>
		public virtual string ToHtml(string stylesheet, object transformTarget, XsltArgumentList xslArgs) 
		{

			string link = String.Empty, content = String.Empty;
			
			if (transformTarget == null) 
				return "<html><head><title>empty</title></head><body></body></html>";

			// use a streamed output to get the "disable-output-escaping" 
			// working:
			StringWriter swr = new StringWriter();

			try	{
				XPathDocument doc; 
				
				if(transformTarget is INewsItem){
					INewsItem item = (INewsItem) transformTarget;
					link = item.FeedLink;
					content = item.Content;
					doc = new XPathDocument(new XmlTextReader(new StringReader(item.ToString(NewsItemSerializationFormat.NewsPaper, false))), XmlSpace.Preserve);
                }else if (transformTarget is IFeedDetails){
					IFeedDetails feed = (IFeedDetails) transformTarget;
					link = feed.Link;
					doc = new XPathDocument(new XmlTextReader(new StringReader(feed.ToString(NewsItemSerializationFormat.NewsPaper, false))), XmlSpace.Preserve);				
				}else if(transformTarget is FeedInfoList){
					FeedInfoList feeds = (FeedInfoList) transformTarget;
					doc = new XPathDocument(new XmlTextReader(new StringReader(feeds.ToString())), XmlSpace.Preserve);				
				}else{
					throw new ArgumentException("transformTarget"); 
				}

				XslCompiledTransform transform; 
				
				if(this.stylesheetTable.ContainsKey(stylesheet)){
                    transform = this.stylesheetTable[stylesheet].CompiledTransform;
				}else{
                    transform = this.stylesheetTable[String.Empty].CompiledTransform;
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
				foreach (EventHandler<FeedExceptionEventArgs> eh in TransformError.GetInvocationList())
					eh.BeginInvoke(sender, e, null, null);
		}

		protected void OnStylesheetError(object sender, ExceptionEventArgs e) {
			if (StylesheetError != null)
				foreach (EventHandler<ExceptionEventArgs> eh in StylesheetError.GetInvocationList())
					eh.BeginInvoke(this, e, null, null);
		}

		protected void OnStylesheetValidationError(object sender, ExceptionEventArgs e) {
			if (StylesheetValidationError != null)
				foreach (EventHandler<ExceptionEventArgs> eh in StylesheetValidationError.GetInvocationList())
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

            /// <summary>
            /// Gets the localized text to indicate toggle of Google Reader shared state.
            /// </summary>
            /// <returns></returns>
            public string ToggleShareStateText()
            {
                return SR.XsltDefaultTemplate_ToggleShareState;
            }

            /// <summary>
            /// Gets the localized text to indicate toggle of NewsGator Online clipped state.
            /// </summary>
            /// <returns></returns>
            public string ToggleClipStateText()
            {
                return SR.XsltDefaultTemplate_ToggleClipState;
            }
		}

		#endregion
	}

	
}
