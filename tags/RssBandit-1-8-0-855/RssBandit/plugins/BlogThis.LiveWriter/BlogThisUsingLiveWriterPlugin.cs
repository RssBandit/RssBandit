using System; 
using System.Xml;
using System.IO; 
using System.Xml.XPath; 
using System.Xml.Xsl;
using Syndication.Extensibility;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using System.Runtime.InteropServices ;


namespace BlogExtension.BlogThis.LiveWriter {


	public class BlogThisUsingLiveWriterPlugin:IBlogExtension {		

		public bool HasConfiguration { get {return true; } }
		public bool HasEditingGUI{ get {return true; } }

		private bool IsWriterInstalled{	

			get{ 
				string writerApplicationClsidKey = String.Format( "CLSID\\{0}", typeof(WindowsLiveWriterApplicationClass).GUID.ToString("B") ) ;

				using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(writerApplicationClsidKey)){	   
					if(key == null){
						return 	false;
					}
				}
				return true;
			}
		}


		public void Configure(IWin32Window parent){

			/* check to see if Windows Live Writer installed */ 
			if(!this.IsWriterInstalled){
				throw new ApplicationException(Resource.Manager["RES_ExceptionLiveWriterNotFound"]); 
			}

			/* instantiate Live Writer object */
			IWindowsLiveWriterApplication liveWriter = (IWindowsLiveWriterApplication) new WindowsLiveWriterApplicationClass();			
			liveWriter.ShowOptions("blogthis"); 
		}


		public string DisplayName { get { return Resource.Manager["RES_MenuLiveWriterCaption"]; } }

 
		public void BlogItem(System.Xml.XPath.IXPathNavigable rssFragment, bool edited) {

			/* check to see if Windows Live Writer installed */ 
			if(!this.IsWriterInstalled){
				throw new ApplicationException(Resource.Manager["RES_ExceptionLiveWriterNotFound"]); 
			}

			/* instantiate Live Writer object */
			IWindowsLiveWriterApplication liveWriter = (IWindowsLiveWriterApplication) new WindowsLiveWriterApplicationClass();			

			 string title        = rssFragment.CreateNavigator().Evaluate("string(//item/title/text())").ToString(); 
			 string description  = rssFragment.CreateNavigator().Evaluate("string(//item/description/text())").ToString();
			 string link         = rssFragment.CreateNavigator().Evaluate("string(//item/link/text())").ToString();
			 string feedName     = rssFragment.CreateNavigator().Evaluate("string(//channel/title/text())").ToString();

			liveWriter.BlogThisFeedItem(feedName, title, link, description, String.Empty, String.Empty, String.Empty, String.Empty);
		}	 	 
	 
	 	

		[ComImport]
			[Guid("8F085BC0-363D-4219-95BA-DC8A5E06D295")]
			private class WindowsLiveWriterApplicationClass {}  // implements IWindowsLiveWriterApplication

		[ComImport]
			[InterfaceType(ComInterfaceType.InterfaceIsDual)] 
			[Guid("D7E5B1EC-FEEA-476C-9CE1-BC5C3E2DB662")]
			private interface IWindowsLiveWriterApplication {
			void NewPost() ;
			void OpenPost() ;
			void BlogThisLink( string title, string url, string comment );
			void BlogThisSnippet( string title, string url, string comment, string snippetContents, bool preserveImages );
			void BlogThisImageUrl( string title, string url, string comment );
			void BlogThisImageFile( string title, string fileName, string comment );
			void BlogThisFeedItem( string feedName, string itemTitle, string itemUrl, string itemContents, string feedHomepage, string author, string authorEmail, string publishDate );
			void ShowOptions(string optionsPage) ;
		}
	}

}
