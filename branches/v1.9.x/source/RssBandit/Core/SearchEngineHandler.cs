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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Collections.Generic;
using JetBrains.Annotations;
using NewsComponents;
using NewsComponents.Utils;
using RssBandit.Xml;
using AppExceptions = Microsoft.ApplicationBlocks.ExceptionManagement;
using Logger = RssBandit.Common.Logging;

namespace RssBandit.WebSearch
{
	/// <summary>
	/// Summary description for SearchEngineHandler.
	/// </summary>
	public class SearchEngineHandler
	{
		public SearchEngineHandler()
		{
			this.LoadSearchConfigSchema(); 
		}

		private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(SearchEngineHandler));

		/// <summary>
		/// Holds the search engines.
		/// </summary>
		private SearchEngines _engines;
		
		///<summary>
		///Internal flag used to track whether the XML in the 
		///searches config validated against the schema. 
		///</summary>
		private bool  validationErrorOccured; 

		///<summary>
		///Internal flag used to track whether the XML in the 
		///searches config validated against the schema. 
		///</summary>
		private bool  enginesLoaded; 

		/// <summary>
		/// The schema for the search engines list format
		/// </summary>
		private XmlSchema searchConfigSchema; 

		/// <summary>
		/// Boolean flag indicates whether the search engines list was loaded 
		/// successfully during the last call to LoadEngines()
		/// </summary>
		public bool EnginesOK
		{		
			get { return !validationErrorOccured; }
		}
		
		/// <summary>
		/// Boolean flag indicates whether the search engines list was loaded 
		/// during the last call to LoadEngines()
		/// </summary>
		public bool EnginesLoaded {		
			get { return enginesLoaded; }
		}

		public bool NewTabRequired
		{
			get 
			{
				if(_engines == null)
					_engines = new SearchEngines(); 

				if(_engines.Engines== null)
					_engines.Engines = new List<SearchEngine>();
				
				return _engines.NewTabRequired;
			}
			set 
			{
				if(_engines == null)
					_engines = new SearchEngines(); 

				if(_engines.Engines== null)
					_engines.Engines = new List<SearchEngine>();
				
				_engines.NewTabRequired = value;
			}
		}

		public List<SearchEngine> Engines		
		{
			get 
			{ 
				if(_engines == null)
					_engines = new SearchEngines(); 

				if(_engines.Engines== null)
                    _engines.Engines = new List<SearchEngine>();
				
				return _engines.Engines;
			}			
		}

		///<summary>
		/// Loads the schema for search Engines into an XmlSchema object. 
		///</summary>		
		private void LoadSearchConfigSchema()	{
			using (Stream stream = Resource.GetStream("Resources.SearchEnginesConfig.xsd")) {
				searchConfigSchema = XmlSchema.Read(stream, null); 
			}
		}

		/// <summary>
		/// Loads the search engines list from the given URL. 
		/// </summary>
		/// <param name="configUrl">The URL of the engines config</param>
		/// <param name="veh">The event handler that should be invoked on the client if validation errors occur</param>
		/// <exception cref="XmlException">XmlException thrown if XML is not well-formed</exception>
		public void LoadEngines(string configUrl, ValidationEventHandler veh)	{

			XmlDocument doc = new XmlDocument();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas.Add(searchConfigSchema);
            //specify validation event handler passed by caller and the one we use 
            //internally to track state 
			if (veh != null)
				settings.ValidationEventHandler += veh;
            settings.ValidationEventHandler += LoaderValidationCallback;
            validationErrorOccured = false;
            enginesLoaded = false;

            XmlReader vr = XmlReader.Create(new XmlTextReader(configUrl), settings);   

			doc.Load(vr); 
			vr.Close(); 

			if(!validationErrorOccured)
			{

				//convert XML to objects 
				XmlNodeReader reader = new XmlNodeReader(doc);		
				XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(SearchEngines));
				SearchEngines mySearchEngines = (SearchEngines)serializer.Deserialize(reader); 
				reader.Close(); 

				_engines = mySearchEngines; 
				enginesLoaded = true;

				if (this.RepairedPhrasePlaceholders()) {
					using (Stream stream = NewsComponents.Utils.FileHelper.OpenForWrite(configUrl)) {
						this.SaveEngines(stream);
					}
				}
			}
		}

		/// <summary>
		/// Because of the localiaziation issue with the hardcoded "[PHRASE]" string
		/// we have to repair (replace) teh old definitions by the new one: "{0}"
		/// </summary>
		private bool RepairedPhrasePlaceholders() {
			bool anyFound = false;
			if (EnginesOK) {
				foreach (SearchEngine se in Engines) {
					if (se.SearchLink.IndexOf("[PHRASE]") >= 0) {
						se.SearchLink = se.SearchLink.Replace("[PHRASE]", "{0}");
						anyFound = true;
					}
				}
			}
			return anyFound;
		}

		/// <summary>
		/// Handles errors that occur during schema validation of search engines list
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void LoaderValidationCallback(object sender,
			ValidationEventArgs args) 
		{

			if(args.Severity == XmlSeverityType.Warning) {
				_log.Info(@"searches\config.xml validation warning: " + args.Message);
			}
			else if(args.Severity == XmlSeverityType.Error) {

				validationErrorOccured = true; 
				
				_log.Error(@"searches\config.xml validation error: " + args.Message);
				AppExceptions.ExceptionManager.Publish(args.Exception);
				
			}

		}


		/// <summary>
		/// Loads the search engines list from the given URL. 
		/// </summary>
		/// <param name="configStream">The Stream of the engines config</param>
		/// <exception cref="Exception">Exception thrown on file access errors</exception>
		public void SaveEngines(Stream configStream)
		{
			XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(SearchEngines));


			if(_engines != null)
			{

				if(_engines.Engines == null)
				{
					_engines.Engines = new List<SearchEngine>(); 
				}

			}//if(_engines != null) 
			else
			{
				_engines = new SearchEngines(); 
			}

			TextWriter writer = new StreamWriter(configStream);
			serializer.Serialize(writer, _engines);
			writer.Close();

		}

		/// <summary>
		/// Generates a config file with default search engine(s).
		/// </summary>
		public void GenerateDefaultEngines([NotNull]string configUrl)
		{
			configUrl.ExceptionIfNullOrEmpty("configUrl");

			this.Clear();

			var searchesPath = Path.GetDirectoryName(configUrl);
			using (var stream = FileHelper.OpenForWrite(configUrl))
			{
				TextWriter writer = new StreamWriter(stream);
				writer.Write(Properties.Resources.web_searches_config);
			}

			using (var stream = FileHelper.OpenForWrite(Path.Combine(searchesPath, "google.ico")))
			{
				Properties.Resources.google.Save(stream);
			}
			using (var stream = FileHelper.OpenForWrite(Path.Combine(searchesPath, "bing.ico")))
			{
				Properties.Resources.bing.Save(stream);
			}
			using (var stream = FileHelper.OpenForWrite(Path.Combine(searchesPath, "yahoo.ico")))
			{
				Properties.Resources.yahoo.Save(stream);
			}

			this.LoadEngines(configUrl, null);
		}

		/// <summary>
		/// Reset the engines.
		/// </summary>
		public void Clear() {
			_engines = new SearchEngines();
			_engines.Engines = new List<SearchEngine>();
			_engines.NewTabRequired = true;
			validationErrorOccured = false; 
			enginesLoaded = true;
		}
	}

	/// <remarks/>
	[XmlTypeAttribute(Namespace=RssBanditNamespace.SearchConfiguration)]
	[XmlRootAttribute("searchConfiguration", Namespace=RssBanditNamespace.SearchConfiguration, IsNullable=false)]
	public class SearchEngines
	{
		/// <remarks/>
		[XmlElementAttribute("engine", Type = typeof(SearchEngine), IsNullable = false)]
		public List<SearchEngine> Engines;

		/// <remarks/>
		[XmlAttributeAttribute("open-newtab", DataType="boolean")]
		public bool NewTabRequired;
	}

	/// <remarks/>
	[XmlTypeAttribute(Namespace=RssBanditNamespace.SearchConfiguration)]
	public class SearchEngine: ISearchEngine, ICloneable
	{
		private string title = String.Empty;
		/// <remarks/>
		[XmlElementAttribute("title")] 
		public string Title {
			get { return title; }
			set { title = value; }
		}

		private string searchLink = String.Empty;
		/// <remarks/>
		[XmlElementAttribute("search-link", DataType="anyURI")] 
		public string SearchLink {
			get { return searchLink; }
			set { searchLink = value; }
		}

		private string description = String.Empty;
		/// <remarks/>
		[XmlElementAttribute("description")] 
		public string Description {
			get { return description; }
			set { description = value; }
		}

		private string imageName = String.Empty;
		/// <remarks/>
		[XmlElementAttribute("image-name")] 
		public string ImageName {
			get { return imageName; }
			set { imageName = value; }
		}

		private bool isActive;
		/// <remarks/>
		[XmlAttributeAttribute("active", DataType="boolean") ] 
		public bool IsActive {
			get { return isActive; }
			set { isActive = value; }
		}

		private bool returnRssResult;
		/// <remarks/>
		[XmlAttributeAttribute("rss-resultset", DataType="boolean" ), System.ComponentModel.DefaultValue(false) ] 
		public bool ReturnRssResult {
			get { return returnRssResult; }
			set { returnRssResult = value; }
		}

		private bool mrergeRssResult;
		/// <remarks/>
		[XmlAttributeAttribute("merge-with-local-resultset", DataType="boolean" ), System.ComponentModel.DefaultValue(false) ] 
		public bool MergeRssResult {
			get { return mrergeRssResult; }
			set { mrergeRssResult = value; }
		}

		#region ICloneable Members

		public object Clone() {
			SearchEngine se = new SearchEngine();
			se.Title = this.Title;
			se.SearchLink = this.SearchLink;
			se.Description = this.Description;
			se.ImageName = this.ImageName;
			se.IsActive = this.IsActive;
			se.ReturnRssResult = this.ReturnRssResult;
			se.MergeRssResult = this.MergeRssResult;
			return se;
		}

		public override string ToString()
		{
			if (this.Title != null)
			{
				if (this.Description != null)
					return String.Format("{0} ({1})", this.Title, this.Description);
				return this.Title;
			}
			return Resources.SR.GeneralNewItemText;
		}

		#endregion
	}
}
