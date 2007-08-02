#region CVS Version Header
/*
 * $Id: RssItemFormatter.cs,v 1.20 2005/06/07 17:56:42 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/06/07 17:56:42 $
 * $Revision: 1.20 $
 */
#endregion

using System;
using System.Xml;
using System.Xml.XPath; 
using System.Xml.Xsl;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Text; 
using System.Collections.Specialized;

using NewsComponents;
using NewsComponents.Feed;
using RssBandit.Exceptions;

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

		private XsltArgumentList _xslArgs = null;
		

		//the table of XSLT transforms
		private ListDictionary stylesheetTable = new ListDictionary(); 

		static NewsItemFormatter() {
			_defaultTmpl = Resource.Manager["XSLT_DefaultTemplate"];
		}

		public NewsItemFormatter():this(String.Empty, _defaultTmpl) {}
		public NewsItemFormatter(string xslStyleSheetName, string xslStyleSheet) {		
			_xslArgs = new XsltArgumentList();
			_xslArgs.AddParam("AppStartupPath", String.Empty, Application.StartupPath);
			_xslArgs.AddParam("AppUserDataPath", String.Empty, RssBanditApplication.GetUserPath());
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
				
			} catch (XsltCompileException e){
				stylesheet = DefaultNewsItemTemplate;
				transform.Load(new XmlTextReader(new StringReader(stylesheet))); 
				this.OnStylesheetError(this, new ExceptionEventArgs(e, Resource.Manager["RES_ExceptionNewsItemFormatterStylesheetCompile"]));
				throw e;

			}	catch (XmlException e){
				stylesheet = DefaultNewsItemTemplate;
				transform.Load(new XmlTextReader(new StringReader(stylesheet))); 
				this.OnStylesheetError(this, new ExceptionEventArgs(e, Resource.Manager["RES_ExceptionNewsItemFormatterStylesheetCompile"]));
				throw e;				
			}						
		}


		/// <summary>
		/// Transform a NewsItem, FeedInfo or FeedInfoList using the specified stylesheet
		/// </summary>
		/// <param name="stylesheet">The stylesheet to use for transformation</param>
		/// <param name="transformTarget">The object to transform</param>
		/// <returns>The results of the transformation</returns>
		public virtual string ToHtml(string stylesheet, object transformTarget) {

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
					link = item.Link;
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
				
				transform.Transform(doc, _xslArgs, swr);

			}	catch (Exception e)	{
				this.OnTransformationError(this, new FeedExceptionEventArgs(e, link, Resource.Manager["RES_ExceptionNewsItemTransformation"]));
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
				this.OnTransformationError(this, new FeedExceptionEventArgs(e, item.Feed.link, Resource.Manager["RES_ExceptionNewsItemTransformation"]));
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

	}
}
