#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.IO; 
using System.Text; 
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Collections;

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

		
		/// <summary>
		/// Holds the search engines.
		/// </summary>
		private SearchEngines _engines = null;
		
		///<summary>
		///Internal flag used to track whether the XML in the 
		///searches config validated against the schema. 
		///</summary>
		private bool  validationErrorOccured = false; 

		///<summary>
		///Internal flag used to track whether the XML in the 
		///searches config validated against the schema. 
		///</summary>
		private bool  enginesLoaded = false; 

		/// <summary>
		/// The schema for the search engines list format
		/// </summary>
		private XmlSchema searchConfigSchema = null; 

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
					_engines.Engines = new ArrayList();
				
				return _engines.NewTabRequired;
			}
			set 
			{
				if(_engines == null)
					_engines = new SearchEngines(); 

				if(_engines.Engines== null)
					_engines.Engines = new ArrayList();
				
				_engines.NewTabRequired = value;
			}
		}

		public ArrayList Engines		
		{
			get 
			{ 
				if(_engines == null)
					_engines = new SearchEngines(); 

				if(_engines.Engines== null)
					_engines.Engines = new ArrayList();
				
				return _engines.Engines;
			}			
		}

		///<summary>Loads the schema for search Engines into an XmlSchema object. 
		///<seealso cref="SearchConfigSchema"/></summary>		
		private void LoadSearchConfigSchema()	{
			using (Stream stream = Resource.Manager.GetStream("Resources.SearchEnginesConfig.xsd")) {
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
			XmlValidatingReader vr = new XmlValidatingReader(new XmlTextReader(configUrl)); 
			vr.Schemas.Add(searchConfigSchema); 
			vr.ValidationType = ValidationType.Schema; 

			//specify validation event handler passed by caller and the one we use 
			//internally to track state 
			vr.ValidationEventHandler += veh;
			vr.ValidationEventHandler += new ValidationEventHandler(LoaderValidationCallback);
			validationErrorOccured = false; 
			enginesLoaded = false;

			doc.Load(vr); 
			vr.Close(); 

			if(!validationErrorOccured)
			{

				//convert XML to objects 
				XmlNodeReader reader = new XmlNodeReader(doc);		
				XmlSerializer serializer = new XmlSerializer(typeof(SearchEngines));
				SearchEngines mySearchEngines = (SearchEngines)serializer.Deserialize(reader); 
				reader.Close(); 

				_engines = mySearchEngines; 
				enginesLoaded = true;

				if (this.RepairedPhrasePlaceholders()) {
					using (Stream stream = RssComponents.FileUtils.OpenForWrite(configUrl)) {
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
			if(args.Severity == XmlSeverityType.Error)	
				validationErrorOccured = true; 							

		}


		/// <summary>
		/// Loads the search engines list from the given URL. 
		/// </summary>
		/// <param name="configUrl">The Stream of the engines config</param>
		/// <exception cref="Exception">Exception thrown on file access errors</exception>
		public void SaveEngines(Stream configUrl)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(SearchEngines));


			if(_engines != null)
			{

				if(_engines.Engines == null)
				{
					_engines.Engines = new ArrayList(); 
				}

			}//if(_engines != null) 
			else
			{
				_engines = new SearchEngines(); 
			}

			TextWriter writer = new StreamWriter(configUrl);
			serializer.Serialize(writer, _engines);
			writer.Close();

		}

		/// <summary>
		/// Generates a config file with default search engine(s).
		/// </summary>
		public void GenerateDefaultEngines()
		{
			this.Clear();

			SearchEngine s1 = new SearchEngine();
			s1.Title = "Google";
			s1.SearchLink = @"http://www.google.com/search?sourceid=navclient&ie=UTF-8&oe=UTF-8&q={0}";
			s1.Description = "Search the web with Google...";			
			s1.ImageName = "google.bmp";
			s1.IsActive = true;
			
			_engines.Engines.Add(s1);

			s1 = new SearchEngine();
			s1.Title = "Feedster";
			s1.SearchLink = @"http://www.feedster.com/search.php?hl=en&ie=ISO-8859-1&q={0}&btnG=Search&sort=date&type=rss";
			s1.Description = "Search RSS with Feedster...";			
			s1.ImageName = "feedster.bmp";
			s1.IsActive = true;
			s1.ReturnRssResult = true; 
			
			_engines.Engines.Add(s1);

			s1 = new SearchEngine();
			s1.Title = "msn.com";
			s1.SearchLink = @"http://search.msn.com/results.aspx?q={0}&FORM=SMCRT&x=32&y=15";
			s1.Description = "Search the web with msn.com...";			
			s1.ImageName = "msn.bmp";
			s1.IsActive = true;
			
			
			_engines.Engines.Add(s1);

		}

		/// <summary>
		/// Reset the engines.
		/// </summary>
		public void Clear() {
			_engines = new SearchEngines();
			_engines.Engines = new ArrayList();
			_engines.NewTabRequired = true;
			validationErrorOccured = false; 
			enginesLoaded = true;
		}
	}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.25hoursaday.com/2003/RSSBandit/searchConfiguration/")]
	[System.Xml.Serialization.XmlRootAttribute("searchConfiguration", Namespace="http://www.25hoursaday.com/2003/RSSBandit/searchConfiguration/", IsNullable=false)]
	public class SearchEngines
	{
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("engine", Type = typeof(SearchEngine), IsNullable = false)]
		public ArrayList Engines;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("open-newtab", DataType="boolean")]
		public bool NewTabRequired;
	}

	/// <remarks/>
	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.25hoursaday.com/2003/RSSBandit/searchConfiguration/")]
	public class SearchEngine: ICloneable
	{
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("title")]
		public string Title;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("search-link", DataType="anyURI")]
		public string SearchLink;
    
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("description")]
		public string Description;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("image-name")]
		public string ImageName;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("active", DataType="boolean") ]
		public bool IsActive;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("rss-resultset", DataType="boolean" ), System.ComponentModel.DefaultValue(false) ]
		public bool ReturnRssResult;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute("merge-with-local-resultset", DataType="boolean" ), System.ComponentModel.DefaultValue(false) ]
		public bool MergeRssResult;

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

		public override string ToString() {
			if (this.Title != null) {
				if (this.Description != null)
					return String.Format("{0} ({1})", this.Title, this.Description);
				else
					return this.Title;
			} else
				return Resource.Manager["RES_GeneralNewItemText"];
		}


		#endregion
	}
}
