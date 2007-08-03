using System; 
using System.Xml;
using System.IO; 
using System.Xml.XPath; 
using System.Xml.Xsl;
using Syndication.Extensibility;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;

namespace BlogExtension.BlogThis.WBloggar {


       public class BlogThisUsingWbloggarPlugin:IBlogExtension {
	
	 public static string styleSheet = @"<xsl:stylesheet version='1.0' 
  xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>

  <xsl:output method='html' /> 
  <xsl:variable name='feed-title' select='/rss/channel/title' />

  <xsl:template match='/'>
    <xsl:apply-templates select='//item' />
  </xsl:template>

  <xsl:template match='/rss/channel/item'>
    <title>RE: <xsl:value-of select='title' /></title>
    <xsl:choose>
      <xsl:when test='description'>
	<blockquote>
	  <xsl:value-of disable-output-escaping='yes' select='description' />
	</blockquote>
      </xsl:when>
      <xsl:otherwise  xmlns:xhtml='http://www.w3.org/1999/xhtml'>
	<blockquote>
	  <xsl:copy-of select='xhtml:body' />
	  </blockquote>
	</xsl:otherwise> 
    </xsl:choose> 
    <i>[Via <xsl:choose><xsl:when test='link'><a href='{link}'><xsl:value-of select='$feed-title' /></a></xsl:when>
    <xsl:otherwise><xsl:value-of select='$feed-title' /></xsl:otherwise> 
    </xsl:choose>]</i>		 
  </xsl:template> 
</xsl:stylesheet>"; 

         public bool HasConfiguration { get {return false; } }
	 public bool HasEditingGUI{ get {return true; } }


	 public void Configure(IWin32Window parent){
	   /* yeah, right */
	 }


	 public string DisplayName { get { return Resource.Manager["RES_MenuWBloggarCaption"]; } }

 
	 public void BlogItem(System.Xml.XPath.IXPathNavigable rssFragment, bool edited) {

	   /* check to see if w::bloggar installed */ 
	   RegistryKey rkey;
	   rkey = Registry.CurrentUser.OpenSubKey(@"Software\VB and VBA Program Settings\Bloggar");

	   if(rkey == null){
	     throw new ApplicationException(Resource.Manager["RES_ExceptionWBloggarNotFound"]); 
	   }
	   
	   string wbloggarPath = ((string) rkey.GetValue("InstallPath")); 
	   
	   XslTransform transform = new XslTransform();
	   transform.Load(new XmlTextReader(new StringReader(styleSheet))); 

	   string tempfile = Path.GetTempFileName(); 
	   transform.Transform(rssFragment, null, new StreamWriter(tempfile)); 	   
	   	   
	   Process.Start(wbloggarPath + @"\wbloggar.exe", tempfile); 
	 }	 	 
	 
	 
	 public static void Main(string[] args){
	   BlogThisUsingWbloggarPlugin plugin = new BlogThisUsingWbloggarPlugin(); 
	   plugin.BlogItem(new XPathDocument("rssitem2.xml"), false); 
	 }

       }

}
