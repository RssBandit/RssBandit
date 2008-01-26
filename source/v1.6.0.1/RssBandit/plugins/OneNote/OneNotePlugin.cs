using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using Syndication.Extensibility;
using Microsoft.Office.OneNote;

namespace BlogExtension.OneNote
{
	/// <summary>
	/// Summary description for OneNotePlugin.
	/// </summary>
	public class OneNotePlugin: IBlogExtension
	{

		string configFile;	
		OneNotePluginConfig configInfo; 
		XmlSerializer serializer; 
		
		public OneNotePlugin()
		{
			//setup path to config file
			string assemblyUri = this.GetType().Assembly.CodeBase;
			string assemblyPath = new Uri(assemblyUri).LocalPath;
			string assemblyDir = Path.GetDirectoryName(assemblyPath);
			configFile = Path.Combine(assemblyDir, "OneNotePlugin.config.xml");	

			//setup XML serializer for config file
			serializer = new XmlSerializer(typeof(OneNotePluginConfig));		
		}

		#region IBlogExtension Members

		public void BlogItem(IXPathNavigable rssFragment, bool edited) {
			LoadConfig(); 				
			DoExportItem(rssFragment.CreateNavigator());
		}

		public bool HasEditingGUI {
			get {
				// TODO:  Add OneNotePlugin.HasEditingGUI getter implementation
				return false;
			}
		}

		public string DisplayName {
			get {
				return Resource.Manager["RES_MenuSendToOneNoteCaption"];
			}
		}

		public void Configure(System.Windows.Forms.IWin32Window parent) {
			// TODO:  Add OneNotePlugin.Configure visual implementation
			LoadConfig(); 				
		}

		public bool HasConfiguration {
			get {
				// TODO:  Add OneNotePlugin.HasConfiguration getter implementation
				return false;
			}
		}

		#endregion

		private void DoExportItem(XPathNavigator nav) {
			// Create a new page in the section "General", with the           
			// title from the item:
			string title = StripAndDecode(nav.Evaluate("string(//item/title/text())").ToString());
			Page p = new Page(configInfo.Page, title);           
			p.Date = DateTime.Parse(nav.Evaluate("string(//item/pubDate/text())").ToString());
			// Create a new Outline, and add some content to it:          
			OutlineObject outline = new OutlineObject();      
			outline.AddContent(new HtmlContent(String.Format(configInfo.ItemLinkTemplate, nav.Evaluate("string(//item/link/text())").ToString())));
			outline.AddContent(new HtmlContent(String.Format(configInfo.ItemContentTemplate, nav.Evaluate("string(//item/description/text())").ToString())));
			// Add the outline to our page:          
			p.AddObject(outline);           
			// Commit the changes to OneNote, and set the actively viewed page:          
			p.Commit();          
			p.NavigateTo(); 

		}

		/// <summary>
		/// Strip tags and expand HTML entities in s 
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		private string StripAndDecode(string s) {
			string t = HtmlHelper.StripAnyTags(s);
			if (t.IndexOf("&") >= 0 && t.IndexOf(";") >= 0) {
				t = HtmlHelper.HtmlDecode(t);
			}
			return t; 
		}

		private void LoadConfig(){
			
			if(configInfo == null){

				if(File.Exists(configFile)){

					XmlTextReader reader = new XmlTextReader(configFile);
					configInfo = (OneNotePluginConfig) serializer.Deserialize(reader); 					
					reader.Close();
			
				}else{
					configInfo = new OneNotePluginConfig(); 
					SaveConfig();
				}
			}
		}

		private void SaveConfig(){
			if(configInfo != null){
				using (Stream f = new FileStream(configFile, FileMode.Create, FileAccess.Write, FileShare.None, 4*1024)) {
					try {
						serializer.Serialize(f, configInfo); 					
					} catch (Exception ex) {
						Trace.WriteLine("Failed to save '" + configFile + "': " + ex.Message);
					}
				}	
			}		
		}
	}

	/// <summary>
	/// OneNote Plugin configuration
	/// </summary>
	[System.Xml.Serialization.XmlRootAttribute("onenote-plugin", Namespace="", IsNullable=false)]
	public class OneNotePluginConfig {
		
		/// <summary>
		/// Constructs a default config
		/// </summary>
		public OneNotePluginConfig() {
			this.Page = "General.one";	// default
			this.ItemLinkTemplate = "Original Web Location: <a href=\"{0}\">click here</a>";
			this.ItemContentTemplate = "{0}";
		}

		/// <summary>
		/// The target OneNote page to export to
		/// </summary>
		[System.Xml.Serialization.XmlElementAttribute("target-note-page")]
		public string Page;
    
		/// <summary>
		/// The item link template (can be HTML)
		/// </summary>
		[System.Xml.Serialization.XmlElementAttribute("item-link-template")]
		public string ItemLinkTemplate;

		/// <summary>
		/// The item link template (can be HTML)
		/// </summary>
		[System.Xml.Serialization.XmlElementAttribute("item-content-template")]
		public string ItemContentTemplate;
	}
}
