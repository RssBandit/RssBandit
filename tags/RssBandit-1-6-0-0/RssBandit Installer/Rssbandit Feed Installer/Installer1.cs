using System;
using System.Collections.Specialized;
using System.IO; 
using System.Xml;
using System.Collections;
using System.ComponentModel;
using Microsoft.Win32;

namespace RssBandit.Installer
{
	/// <summary>
	/// Summary description for Installer1.
	/// </summary>
	[RunInstaller(true)]
	public class Installer1 : System.Configuration.Install.Installer
	{
		

		/// <summary>
		/// The default feed list installed if this is first time RSS Bandit has been
		/// installed on the machine
		/// </summary>
		const string defaultFeedList = 
@"<feeds refresh-rate='3600000' xmlns='http://www.25hoursaday.com/2004/RSSBandit/feeds/'>
   <feed category='RSS Bandit'>
 	<title>RSS Bandit News</title>
	<link>http://sourceforge.net/export/rss2_projnews.php?group_id=96589</link>
   </feed>  
   <feed category='RSS Bandit'>
 	<title>Torsten's RSS Bandit Blog</title>
	<link>http://www.rendelmann.info/blog/SyndicationService.asmx/GetRssCategory?categoryName=RSS%20Bandit</link>
    <refresh-rate>86400000</refresh-rate>
   </feed>
   <feed category='RSS Bandit'>
 	<title>Dare's RSS Bandit Blog</title>
	<link>http://www.25hoursaday.com/weblog/SyndicationService.asmx/GetRssCategory?categoryName=RSS%20Bandit</link>
    <refresh-rate>86400000</refresh-rate> 
   </feed>
   <feed category='News'>
 	<title>BBC News</title>
	<link>http://news.bbc.co.uk/rss/newsonline_world_edition/front_page/rss091.xml</link>
   </feed>
   <feed category='News'>
 	<title>Yahoo News</title>
	<link>http://rss.news.yahoo.com/rss/topstories</link>
   </feed>
   <feed category='Entertainment'>
 	<title>Rolling Stone</title>
	<link>http://www.rollingstone.com/rssxml/music_news.xml</link>
   </feed>
   <feed category='Entertainment'>
 	<title>Yahoo News [Entertainment]</title>
	<link>http://rss.news.yahoo.com/rss/entertainment</link>
   </feed>  
   <feed category='Politics'>
 	<title>1115.org</title>
	<link>http://feeds.feedburner.com/1115org</link>
   </feed>
   <feed category='Politics'>
 	<title>Yahoo News [Politics]</title>
	<link>http://rss.news.yahoo.com/rss/politics</link>
   </feed>
   <feed category='News\Technology'>
	<title>Microsoft Watch</title>
	<link>http://rssnewsapps.ziffdavis.com/msw.xml</link>
   </feed>
   <feed category='News\Technology'>
	<title>Slashdot</title>
	<link>http://slashdot.org/slashdot.rss</link>
    <listview-layout>0ecbbe6fd000496e868f93f8932cc570</listview-layout>
	<stylesheet>slashdot</stylesheet>
   </feed>
   <feed category='News\Technology'>
	<title>Wired News</title>
	<link>http://www.wired.com/news_drop/netcenter/netcenter.rdf</link>
   </feed>
   <feed category='Blogs'>
     <title>Dave Barry</title>
     <link>http://weblog.herald.com/column/davebarry/index.xml</link>
   </feed>
    <feed category='Blogs'>
     <title>Robert Scoble</title>
     <link>http://radio.weblogs.com/0001011/rss.xml</link>
   </feed> 
    <feed category='Blogs'>
     <title>Jason Kottke</title>
     <link>http://www.kottke.org/index.xml</link>
   </feed>
   <feed category='Comics'>
     <title>Dilbert</title>
     <link>http://dwlt.net/tapestry/dilbert.rdf</link>
   </feed>
    <feed category='Comics'>
     <title>Snoopy</title>
     <link>http://dwlt.net/tapestry/peanuts.rdf</link>
   </feed>
   <categories>    
    <category>Entertainment</category>
    <category>Politics</category>
    <category>News</category>
    <category>News\Technology</category>
    <category>RSS Bandit</category>
	<category>Blogs</category>
    <category>Comics</category>
   </categories>
   <listview-layouts>
    <listview-layout ID='54b7a2c3d0b74fcea19e84e0cae0e745'>
	 <FeedColumnLayout>
	    <LayoutType>GlobalFeedLayout</LayoutType>
	    <SortByColumn>Date</SortByColumn>
		<SortOrder>Descending</SortOrder>
		<ColumnList>
		 <string>Title</string>
		 <string>Subject</string>
		 <string>Date</string>
		 <string>CommentCount</string>
		</ColumnList>
		<ColumnWidthList>
		 <int>343</int>
		 <int>113</int>
		 <int>135</int>
		 <int>63</int>
		</ColumnWidthList>
	</FeedColumnLayout>
   </listview-layout>
   <listview-layout ID='9bc6fd4047384a19a030b315ffba5723'>
    <FeedColumnLayout>
		<LayoutType>GlobalCategoryLayout</LayoutType>
		<SortByColumn>Date</SortByColumn>
		<SortOrder>Descending</SortOrder>
		<ColumnList>
		 <string>Title</string>
		 <string>Subject</string>
		 <string>Date</string>
		 <string>FeedTitle</string>
		</ColumnList>
		<ColumnWidthList>
		 <int>250</int>
		 <int>120</int>
		 <int>100</int>
		 <int>100</int>
		</ColumnWidthList>
	  </FeedColumnLayout>
	</listview-layout>
    <listview-layout ID='0ecbbe6fd000496e868f93f8932cc570'>
	 <FeedColumnLayout>
	  <LayoutType>IndividualLayout</LayoutType>
	    <SortByColumn>Date</SortByColumn>
	    <SortOrder>Descending</SortOrder>
		<ColumnList>
		 <string>Title</string>
		 <string>Author</string>
		 <string>Subject</string>
		 <string>Date</string>
		 <string>CommentCount</string>
		</ColumnList>
		<ColumnWidthList>
		 <int>313</int>
		 <int>94</int>
		 <int>78</int>
		 <int>109</int>
		 <int>120</int>
	   </ColumnWidthList>
	  </FeedColumnLayout>
     </listview-layout>
    </listview-layouts>
   </feeds>
";

		/// <summary>
		/// Set/Get the current "feed:" Url protocol handler. 
		/// Provide the complete executable file path name.
		/// </summary>
		public  string CurrentFeedProtocolHandler {
			get {
				try {
					RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"feed\shell\open\command", false);
					string val = ((key == null) ? String.Empty : (key.GetValue(null) as string));
					return val;
				} catch (Exception ex) {
					System.Diagnostics.Trace.WriteLine("Registry:CurrentFeedProtocolHandler (get) cause exception: "+ex.Message);
					return String.Empty;
				}
						
			}
		}

		private string targetDir = null;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Installer1()
		{
			// This call is required by the Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		/// <summary>
		/// Performs Installation procedure. 
		/// </summary>
		/// <param name="stateSaver"></param>
		public override void Install(IDictionary stateSaver) {
			base.Install (stateSaver);
			try 
			{
				if(this.Context!=null) 
				{
					StringDictionary parameters = Context.Parameters;
					string[] keys =new string[parameters.Count];
					parameters.Keys.CopyTo(keys,0); 

					// Set the StateServer collection values
					for(int intKeys=0;intKeys<keys.Length;intKeys++)
					{
						if(keys[intKeys].Equals("target")) 
						{
					 		targetDir = parameters[keys[intKeys]].ToString();
							// if we need it during uninstallation, uncomment this:
							//stateSaver.Add("target", targetDir);
						}
					}
				}
			} catch (Exception) {}

			try 
			{
				try{
					this.InstallDefaultFeedList(); 	
				}catch(Exception e){ /* Not fatal enough to halt installation process */				
					this.Context.LogMessage("InstallDefaultFeedList() caused exception: " + e.Message);
				}
				try {
					this.InstallDefaultSearchFolders(); 
				}catch(Exception e){ /* Not fatal enough to halt installation process */				
					this.Context.LogMessage("InstallDefaultSearchFolders() caused exception: " + e.Message);
				}
				try {
					this.InstallRssSearch(); 
				}catch(Exception e){ /* Not fatal enough to halt installation process */				
					this.Context.LogMessage("InstallRssSearch() caused exception: " + e.Message);
				}
			}catch(Exception){ /* Not fatal enough to halt installation process */				
			}
	}

		/// <summary>
		/// Performs Uninstallation procedure
		/// </summary>
		/// <param name="savedState"></param>
		public override void Uninstall(IDictionary savedState) {
			base.Uninstall (savedState);
			try {
				try{
					this.UninstallFeedUriHandler(); 					
				}catch(Exception e){ /* Not fatal enough to halt uninstallation process */
					this.Context.LogMessage("UninstallFeedUriHandler() caused exception: " + e.Message);			}
			}catch(Exception){ /* Not fatal enough to halt uninstallation process */
			}
		}

		
		/// <summary>
		/// Ensure that RSS Bandit is no longer the handler for the feed:// URI scheme. 
		/// </summary>
		private void UninstallFeedUriHandler(){
		
			if(this.CurrentFeedProtocolHandler.ToLower().IndexOf("rssbandit")!= -1){
				Registry.ClassesRoot.DeleteSubKey(@"feed\shell\open\command");
			}
		}


		/// <summary>
		/// Adds a default feed list if this is the first time RSS Bandit installed on the machine. 
		/// </summary>
		private void InstallDefaultFeedList(){
		
			string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RssBandit");
			string feedlist      = Path.Combine(appDataFolder, "feedlist.xml"); 
			string subslist      = Path.Combine(appDataFolder, "subscriptions.xml"); 


			if(!Directory.Exists(appDataFolder)){
				Directory.CreateDirectory(appDataFolder); 
			}

			if(!File.Exists(feedlist) && !File.Exists(subslist)){				
				StreamWriter sw    = new StreamWriter(subslist); 
				sw.WriteLine(defaultFeedList); 
				sw.Close(); 
			}		
		}

		/// <summary>
		/// If this is overriding a previous install then update Feedster and MSN search to return results as RSS. 
		/// </summary>
		private void InstallRssSearch(){
			string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RssBandit");
			string searchFolder  = Path.Combine(appDataFolder, "searches"); 
			string searches      = Path.Combine(searchFolder, "config.xml"); 

			XmlDocument doc      = new XmlDocument(); 

			if(File.Exists(searches)){				
				doc.Load(searches); 
			
				XmlNamespaceManager mgr   = new XmlNamespaceManager(doc.NameTable); 
				mgr.AddNamespace("s", "http://www.25hoursaday.com/2003/RSSBandit/searchConfiguration/"); 
				XmlElement feedsterSearch =  (XmlElement) doc.SelectSingleNode("/s:searchConfiguration/s:engine[s:title='Feedster']", mgr);
			
				if(feedsterSearch != null){
					feedsterSearch.SetAttribute("rss-resultset", "true"); 
					XmlElement searchlink = (XmlElement) feedsterSearch.SelectSingleNode("./s:search-link", mgr); 
					if(searchlink != null){
						searchlink.InnerXml = "http://www.feedster.com/search.php?hl=en&amp;ie=ISO-8859-1&amp;q=[PHRASE]&amp;btnG=Search&amp;type=rss&amp;sort=date";
					}

					XmlElement imagename = (XmlElement)feedsterSearch.SelectSingleNode("./s:image-name", mgr); 
					if(imagename == null){
						imagename = doc.CreateElement(String.Empty, "image-name", "http://www.25hoursaday.com/2003/RSSBandit/searchConfiguration/");
						imagename.InnerText = "feedster.gif"; 
						feedsterSearch.AppendChild(imagename); 						
					}
				}


				XmlElement msnSearch =  (XmlElement) doc.SelectSingleNode("/s:searchConfiguration/s:engine[s:title='msn.com']", mgr);
			
				if(msnSearch != null){
					msnSearch.SetAttribute("rss-resultset", "true"); 
					XmlElement searchlink = (XmlElement) msnSearch.SelectSingleNode("./s:search-link", mgr); 
					if(searchlink != null){
						searchlink.InnerXml = "http://search.msn.com/results.aspx?q={0}&amp;FORM=SMCRT&amp;x=32&amp;y=15&amp;format=rss"; 
					}

					XmlElement imagename = (XmlElement)msnSearch.SelectSingleNode("./s:image-name", mgr); 
					if(imagename == null){
						imagename = doc.CreateElement(String.Empty, "image-name", "http://www.25hoursaday.com/2003/RSSBandit/searchConfiguration/");
						imagename.InnerText = "msn.ico"; 
						msnSearch.AppendChild(imagename); 						
					}
				}


				XmlElement googleSearch =  (XmlElement) doc.SelectSingleNode("/s:searchConfiguration/s:engine[s:title='Google']", mgr);
			
				if(googleSearch != null){
					
					XmlElement imagename = (XmlElement)googleSearch.SelectSingleNode("./s:image-name", mgr); 
					if(imagename == null){
						imagename = doc.CreateElement(String.Empty, "image-name", "http://www.25hoursaday.com/2003/RSSBandit/searchConfiguration/");
						imagename.InnerText = "google.bmp"; 
						googleSearch.AppendChild(imagename); 						
					}
				}

				doc.Save(searches); 
			}

		}
		
		/// <summary>
		/// Adds default search folders like the 'Unread' items search folder
		/// </summary>
		private void InstallDefaultSearchFolders(){
		
			string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RssBandit");
			string settings      = Path.Combine(appDataFolder, ".settings.xml"); 
			string searchfolders = Path.Combine(appDataFolder, "searchfolders.xml"); 
			XmlDocument doc      = new XmlDocument(); 

			if(File.Exists(searchfolders)){ //there should be an 'Unread Items' there
				return; 
			} else if(File.Exists(settings)){				
				doc.Load(settings); 
			}else{
				doc.LoadXml("<settings />");
			}
			 
			XmlElement finderProp = (XmlElement) doc.SelectSingleNode("/settings/property[@name='findernodes']"); 
			bool finderFound = true; 

			if(finderProp==null){
				finderProp = doc.CreateElement("property"); 
				finderProp.SetAttribute("name", "findernodes");
				finderFound = false; 
			}			
      
			if(!finderFound){
				finderProp.InnerText  = @"<FinderSearchNodes xmlns:xsd='http://www.w3.org/2001/XMLSchema' 
										xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'> 
										<RssFinderNodes> <RssFinder> <FullPath>Unread Items</FullPath> <SearchCriterias> 
										<ISearchCriteria xsi:type='SearchCriteriaProperty'> <BeenRead>false</BeenRead>
										 <Flags>None</Flags> <WhatKind>Unread</WhatKind> </ISearchCriteria> </SearchCriterias>
										 <category-scopes /> <feedurl-scopes /> <DoHighlight>true</DoHighlight> </RssFinder> 
										</RssFinderNodes> </FinderSearchNodes>";
				doc.DocumentElement.AppendChild(finderProp); 			
			}else{
				XmlDocument finderSearchNodes = new XmlDocument(); 
				finderSearchNodes.LoadXml(finderProp.InnerText); 
				
				XmlDocument unreadItems = new XmlDocument(); 
				unreadItems.LoadXml(@"<RssFinder xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'> <FullPath>Unread Items</FullPath> <SearchCriterias> 
										<ISearchCriteria xsi:type='SearchCriteriaProperty'> <BeenRead>false</BeenRead>
										 <Flags>None</Flags> <WhatKind>Unread</WhatKind> </ISearchCriteria> </SearchCriterias>
										 <category-scopes /> <feedurl-scopes /> <DoHighlight>true</DoHighlight> </RssFinder>");
					
				XmlElement elem = (XmlElement) finderSearchNodes.SelectSingleNode("/FinderSearchNodes/RssFinderNodes/RssFinder[FullPath='Unread']");
				if(elem != null){
					elem.ParentNode.ReplaceChild(elem.OwnerDocument.ImportNode(unreadItems.DocumentElement, true), elem); 
				}else{
					finderSearchNodes.DocumentElement.FirstChild.AppendChild(finderSearchNodes.OwnerDocument.ImportNode(unreadItems.DocumentElement, true)); 
				}
				
				finderProp.InnerText = finderSearchNodes.OuterXml; 
			}
				doc.Save(settings); 
      
		}

		

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion
	}
}
