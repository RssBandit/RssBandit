<xsl:stylesheet version='1.0' 
xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
xmlns:content = 'http://purl.org/rss/1.0/modules/content/'
xmlns:xhtml='http://www.w3.org/1999/xhtml'
xmlns:slash='http://purl.org/rss/1.0/modules/slash/' 
xmlns:dc='http://purl.org/dc/elements/1.1/' 
xmlns:fd='http://www.bradsoft.com/feeddemon/xmlns/1.0/'
xmlns:bndt='http://www.25hoursaday.com/2003/RSSBandit/feeds/'
xmlns:localized='urn:localization-extension'
xmlns:wfw='http://wellformedweb.org/CommentAPI/'
xmlns:gr='http://www.google.com/reader/'
xmlns:ng='http://newsgator.com/schema/extensions'
exclude-result-prefixes='wfw content slash dc fd bndt localized gr ng'>

<!-- 
	Two variables, that are setup to reflect:
	* AppStartupPath: the current path to the executable RssBandit.exe (C:\Program Files\RssBandit)
	* AppUserDataPath: the path to the Application Data folder 
	(usually C:\Documents and Settings\<username>\Application Data\RssBandit)
-->
<xsl:param name='AppStartupPath'/>
<xsl:param name='AppUserDataPath'/>
<!-- 
	* Paging related:
-->
<xsl:param name='LimitNewsItemsPerPage' />
<xsl:param name='CurrentPageNumber' />
<xsl:param name='LastPageNumber' />
<!-- 
	* while scroll, mark items read:
-->
<xsl:param name="MarkItemsAsReadWhenViewed" />

<xsl:output method='html' indent='yes' /> 

<!-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ -->
<!-- match channel group newspaper -->
<!-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ -->
<xsl:template match="newspaper[@type='group']">
<html><head><title><xsl:value-of select='title'/></title>
<!-- <base href="{//channel/link}" /> -->
<xsl:call-template name="embedded_style" />
</head>
<body id="body">
 <xsl:attribute name="onscroll">
  <xsl:if test="$MarkItemsAsReadWhenViewed != false">
  handleScroll()
  </xsl:if>
 </xsl:attribute>

<table height="100%" width="100%">	
<tr>
<td class="PostFrame" height="100%" width="100%" valign="top">	
<xsl:if test="$LimitNewsItemsPerPage and ($LastPageNumber &gt; 1)">
<div class="PageNavigation">
<xsl:choose>
	  <xsl:when test='$CurrentPageNumber &gt; 1'>
	    <a href="fdaction:?action=previouspage&amp;pagetype=category" class="img">  	 
	   <img src="$IMAGEDIR$leftarrow.gif" border="0"/>
	   </a><a href="fdaction:?action=previouspage&amp;pagetype=category">  	 	  
	   <xsl:value-of select='localized:PreviousPageText()' />
	   </a>
	   </xsl:when>
	  <xsl:otherwise>
	  <img src="$IMAGEDIR$leftarrow.gif" /><xsl:value-of select='localized:PreviousPageText()' />	  
	  </xsl:otherwise>
</xsl:choose>&#160;<xsl:choose>
	  <xsl:when test='$LastPageNumber &gt; $CurrentPageNumber'>
	   <a href="fdaction:?action=nextpage&amp;pagetype=category">  	 	  
	     <xsl:value-of select='localized:NextPageText()' />
	   </a>
	   <a href="fdaction:?action=nextpage&amp;pagetype=category" class="img">  	 
	   <img src="$IMAGEDIR$rightarrow.gif" border="0"/>
	   </a>	    
	  </xsl:when>
	  <xsl:otherwise>
	   <xsl:value-of select='localized:NextPageText()' /><img src="$IMAGEDIR$rightarrow.gif" />	   
	  </xsl:otherwise>
</xsl:choose>
	 |&#160;<xsl:value-of select='localized:DisplayingPageText()' />&#160;<xsl:value-of select='$CurrentPageNumber' />&#160;<xsl:value-of select='localized:PageOfText()' />&#160;<xsl:value-of select='$LastPageNumber' />
	  	  
    </div>
    </xsl:if>	  
<xsl:for-each select="//channel">

  <xsl:if test="current()/image">
<div class="PostTitle" style="FLOAT: right"> 
  <a href="{current()/image/link}" title="{current()/image/title}"><img src="{current()/image/url}"  alt="{current()/image/title}" border="0">
  <xsl:if test="current()/image/width!=''"><xsl:attribute name="width"><xsl:value-of select="current()/image/width"/></xsl:attribute></xsl:if>
  <xsl:if test="current()/image/height!=''"><xsl:attribute name="height"><xsl:value-of select="current()/image/height"/></xsl:attribute></xsl:if>
  </img></a>
</div>
  </xsl:if>
  <div class="PostTitle">			
<a href='{current()/link}'>
  <xsl:value-of  disable-output-escaping='yes' select='current()/title'/>
</a>
  </div>
  <!-- <div class="PostInfos">
  <b><xsl:value-of disable-output-escaping='yes' select="current()/category"/></b>
  </div> -->
  <xsl:variable name="outerposition" select="position()" />
  <div class="PostContent">
   <xsl:for-each select='current()//item'>

	<div class="PostItemContent" id="{concat('item',string(position()), 'in' , string($outerposition))}" >
	<xsl:call-template name="process_item_read_flag_states">
	  <xsl:with-param name="current_position" select="concat(string(position()), 'in' , string($outerposition))" />
	  <xsl:with-param name="current_item" select="." />
	</xsl:call-template>
		
  <div class="PostInfos">	
	<a href='{current()/link}'>
	  <b><xsl:value-of disable-output-escaping='yes' select="current()/title"/></b>
	</a>
  </div>
  <xsl:choose>
	<xsl:when test='current()/xhtml:body'>
	  <xsl:copy-of select='current()/xhtml:body'/>
	</xsl:when>
	<xsl:when test='current()/content:encoded'>
	  <xsl:value-of  disable-output-escaping='yes' select='current()/content:encoded'/>
	</xsl:when>
	<xsl:otherwise>
	  <xsl:value-of disable-output-escaping='yes' select='current()/description'/>
	</xsl:otherwise>
  </xsl:choose>
  
    <xsl:if test='current()/enclosure'>
	<p>
	<a class='img'>
	 
	 <xsl:attribute name="href"> 
	  <xsl:value-of  disable-output-escaping='yes' select='current()/enclosure/@url'/>
	 </xsl:attribute>
	 
	<img border="0" src="$IMAGEDIR$play.gif" height="16" width="16" >
	 <xsl:attribute name="alt"> 
	  <xsl:value-of  disable-output-escaping='yes' select='current()/enclosure/@url'/>
	 </xsl:attribute>
	</img>
	</a>
	<xsl:text disable-output-escaping='yes'>&amp;nbsp;</xsl:text>
	<xsl:text disable-output-escaping='yes'>&lt;a href='</xsl:text><xsl:value-of  disable-output-escaping='yes' select='current()/enclosure/@url'/><xsl:text disable-output-escaping='yes'>'&gt;</xsl:text>
	<xsl:value-of disable-output-escaping='yes' select='localized:ItemEnclosureText()' /> (<xsl:value-of select='current()/enclosure/@type'/>, <xsl:choose><xsl:when test='count(current()/enclosure/@length)=0 or current()/enclosure/@length &lt;= 0'>?</xsl:when><xsl:when test='current()/enclosure/@duration'><xsl:value-of select='current()/enclosure/@duration' /></xsl:when><xsl:otherwise><xsl:value-of select='current()/enclosure/@length'/> Bytes</xsl:otherwise></xsl:choose>)
	<xsl:text disable-output-escaping='yes'>&lt;/a></xsl:text>				
	</p>
	</xsl:if>	
  
  <div class="PostSignature">			
	<br />
	<xsl:if test="current()/dc:creator and current()/dc:creator!=''">
	  <xsl:value-of disable-output-escaping='yes' select='localized:ItemPublisherText()' />:
	  <xsl:value-of disable-output-escaping='yes' select='current()/dc:creator'/>
	  <xsl:text disable-output-escaping='yes'>&amp;nbsp;</xsl:text>
	  </xsl:if><xsl:if test="current()/author and current()/author!=''">
	  <xsl:value-of disable-output-escaping='yes' select='localized:ItemAuthorText()' />  <xsl:value-of disable-output-escaping='yes' select='current()/author'/>
	  <xsl:text disable-output-escaping='yes'>&amp;nbsp;</xsl:text>
	  </xsl:if><xsl:value-of disable-output-escaping='yes' select='localized:ItemDateText()' />
	  <xsl:text disable-output-escaping='yes'>&amp;nbsp;</xsl:text>
	  <xsl:value-of select='current()/pubDate'/>			
	  
	</div>
	</div>
  </xsl:for-each>
</div>
  
</xsl:for-each>
<xsl:if test="$LimitNewsItemsPerPage and ($LastPageNumber &gt; 1)">
<div class="PageNavigation">
<xsl:choose>
	  <xsl:when test='$CurrentPageNumber &gt; 1'>
	    <a href="fdaction:?action=previouspage&amp;pagetype=category" class="img">  	 
	   <img src="$IMAGEDIR$leftarrow.gif" border="0"/>
	   </a><a href="fdaction:?action=previouspage&amp;pagetype=category">  	 	  
	   <xsl:value-of select='localized:PreviousPageText()' />
	   </a>
	   </xsl:when>
	  <xsl:otherwise>
	  <img src="$IMAGEDIR$leftarrow.gif" /><xsl:value-of select='localized:PreviousPageText()' />	  
	  </xsl:otherwise>
</xsl:choose>&#160;<xsl:choose>
	  <xsl:when test='$LastPageNumber &gt; $CurrentPageNumber'>
	   <a href="fdaction:?action=nextpage&amp;pagetype=category">  	 	  
	     <xsl:value-of select='localized:NextPageText()' />
	   </a>
	   <a href="fdaction:?action=nextpage&amp;pagetype=category" class="img">  	 
	   <img src="$IMAGEDIR$rightarrow.gif" border="0"/>
	   </a>	    
	  </xsl:when>
	  <xsl:otherwise>
	   <xsl:value-of select='localized:NextPageText()' /><img src="$IMAGEDIR$rightarrow.gif" />	   
	  </xsl:otherwise>
</xsl:choose>
	 |&#160;<xsl:value-of select='localized:DisplayingPageText()' />&#160;<xsl:value-of select='$CurrentPageNumber' />&#160;<xsl:value-of select='localized:PageOfText()' />&#160;<xsl:value-of select='$LastPageNumber' />
	  	  
    </div>
    </xsl:if>	  
</td>
</tr>
</table>
</body></html>
</xsl:template>

<!-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ -->
<!-- match channel newspaper -->
<!-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ -->
<xsl:template match="newspaper[@type='channel']">
<html><head><title><xsl:value-of select='//channel/title'/></title>
<base href="{//channel/link}" />
<xsl:call-template name="embedded_style" />
</head>
<body id="body">
 <xsl:attribute name="onscroll">
  <xsl:if test="$MarkItemsAsReadWhenViewed != false">
  handleScroll()
  </xsl:if>
 </xsl:attribute>
 
  <table height="100%" width="100%">
<tr>
  <td class="PostFrame" height="100%" width="100%" valign="top">
	<xsl:if test="//channel/image">
	  <div class="PostTitle" style="FLOAT: right">
	<a href="{//channel/image/link}" title="{//channel/image/title}"><img src="{//channel/image/url}"  alt="{//channel/image/title}" border="0">
	<xsl:if test="//channel/image/width!=''"><xsl:attribute name="width"><xsl:value-of select="//channel/image/width"/></xsl:attribute></xsl:if>
	<xsl:if test="//channel/image/height!=''"><xsl:attribute name="height"><xsl:value-of select="//channel/image/height"/></xsl:attribute></xsl:if>
	</img></a>
	  </div>
	</xsl:if>
	<div class="PostTitle">			
	  <a href='{//channel/link}'>
	<xsl:value-of  disable-output-escaping='yes' select='//channel/title'/>
	  </a>
	</div>
	
<xsl:if test="$LimitNewsItemsPerPage and ($LastPageNumber &gt; 1)">
<div class="PageNavigation">
<xsl:choose>
	  <xsl:when test='$CurrentPageNumber &gt; 1'>
	    <a href="fdaction:?action=previouspage&amp;pagetype=feed" class="img">  	 
	   <img src="$IMAGEDIR$leftarrow.gif" border="0"/>
	   </a><a href="fdaction:?action=previouspage&amp;pagetype=feed">  	 	  
	   <xsl:value-of select='localized:PreviousPageText()' />
	   </a>
	   </xsl:when>
	  <xsl:otherwise>
	  <img src="$IMAGEDIR$leftarrow.gif" /><xsl:value-of select='localized:PreviousPageText()' />	  
	  </xsl:otherwise>
</xsl:choose>&#160;<xsl:choose>
	  <xsl:when test='$LastPageNumber &gt; $CurrentPageNumber'>
	   <a href="fdaction:?action=nextpage&amp;pagetype=feed">  	 	  
	     <xsl:value-of select='localized:NextPageText()' />
	   </a><a href="fdaction:?action=nextpage&amp;pagetype=feed" class="img">  	 
	   <img src="$IMAGEDIR$rightarrow.gif" border="0"/>
	   </a>	    
	  </xsl:when>
	  <xsl:otherwise>
	   <xsl:value-of select='localized:NextPageText()' /><img src="$IMAGEDIR$rightarrow.gif" />	   
	  </xsl:otherwise>
</xsl:choose>
	 |&#160;<xsl:value-of select='localized:DisplayingPageText()' />&#160;<xsl:value-of select='$CurrentPageNumber' />&#160;<xsl:value-of select='localized:PageOfText()' />&#160;<xsl:value-of select='$LastPageNumber' />
	  	  
    </div>
    </xsl:if>
       
	<!-- <div class="PostInfos">
	<b><xsl:value-of disable-output-escaping='yes' select="current()/category"/></b>
	</div> -->
	<div class="PostContent">
	  <xsl:for-each select='//item'>
		<div class="PostItemContent" id="{concat('item', string(position()))}" >
		<xsl:call-template name="process_item_read_flag_states">
		  <xsl:with-param name="current_position" select="position()" />
		  <xsl:with-param name="current_item" select="." />
		</xsl:call-template>
		
	<div class="PostInfos">	
	  <a href='{current()/link}'>
		<b><xsl:value-of disable-output-escaping='yes' select="current()/title"/></b>
	  </a>
	</div>
	<xsl:choose>
	  <xsl:when test='current()/xhtml:body'>
		<xsl:copy-of select='current()/xhtml:body'/>
	  </xsl:when>
	  <xsl:when test='current()/content:encoded'>
		<xsl:value-of  disable-output-escaping='yes' select='current()/content:encoded'/>
	  </xsl:when>
	  <xsl:otherwise>
		<xsl:value-of disable-output-escaping='yes' select='current()/description'/>
	  </xsl:otherwise>
	</xsl:choose>
	
	    <xsl:if test='current()/enclosure'>
	<p>
	<a class='img'>
	 
	 <xsl:attribute name="href"> 
	  <xsl:value-of  disable-output-escaping='yes' select='current()/enclosure/@url'/>
	 </xsl:attribute>
	 
	<img border="0" src="$IMAGEDIR$play.gif" height="16" width="16" >
	 <xsl:attribute name="alt"> 
	  <xsl:value-of  disable-output-escaping='yes' select='current()/enclosure/@url'/>
	 </xsl:attribute>
	</img>
	</a>
	<xsl:text disable-output-escaping='yes'>&amp;nbsp;</xsl:text>
	<xsl:text disable-output-escaping='yes'>&lt;a href='</xsl:text><xsl:value-of  disable-output-escaping='yes' select='current()/enclosure/@url'/><xsl:text disable-output-escaping='yes'>'&gt;</xsl:text>
	<xsl:value-of disable-output-escaping='yes' select='localized:ItemEnclosureText()' /> (<xsl:value-of select='current()/enclosure/@type'/>, <xsl:choose><xsl:when test='count(current()/enclosure/@length)=0 or current()/enclosure/@length &lt;= 0'>?</xsl:when><xsl:when test='current()/enclosure/@duration'><xsl:value-of select='current()/enclosure/@duration' /></xsl:when><xsl:otherwise><xsl:value-of select='current()/enclosure/@length'/> Bytes</xsl:otherwise></xsl:choose>)
	<xsl:text disable-output-escaping='yes'>&lt;/a></xsl:text>				
	</p>
	</xsl:if>
	
	<div class="PostSignature">			
	  <br />
	  <xsl:if test="current()/dc:creator and current()/dc:creator!=''">
		<xsl:value-of disable-output-escaping='yes' select='localized:ItemPublisherText()' />:
		<xsl:value-of disable-output-escaping='yes' select='current()/dc:creator'/>
		<xsl:text disable-output-escaping='yes'>&amp;nbsp;</xsl:text>
		</xsl:if><xsl:if test="current()/author and current()/author!=''">
		<xsl:value-of disable-output-escaping='yes' select='localized:ItemAuthorText()' />  <xsl:value-of disable-output-escaping='yes' select='current()/author'/>
		<xsl:text disable-output-escaping='yes'>&amp;nbsp;</xsl:text>
		</xsl:if><xsl:value-of disable-output-escaping='yes' select='localized:ItemDateText()' />
		<xsl:text disable-output-escaping='yes'>&amp;nbsp;</xsl:text>
		<xsl:value-of select='current()/pubDate'/>
					
		
	  </div>
	  </div>
	</xsl:for-each>
	  </div>	  
 
<xsl:if test="$LimitNewsItemsPerPage and ($LastPageNumber &gt; 1)">
<div class="PageNavigation">
<xsl:choose>
	  <xsl:when test='$CurrentPageNumber &gt; 1'>
	    <a href="fdaction:?action=previouspage&amp;pagetype=feed" class="img">  	 
	   <img src="$IMAGEDIR$leftarrow.gif" border="0"/>
	   </a><a href="fdaction:?action=previouspage&amp;pagetype=feed">  	 	  
	   <xsl:value-of select='localized:PreviousPageText()' />
	   </a>
	   </xsl:when>
	  <xsl:otherwise>
	  <img src="$IMAGEDIR$leftarrow.gif" /><xsl:value-of select='localized:PreviousPageText()' />	  
	  </xsl:otherwise>
</xsl:choose>&#160;<xsl:choose>
	  <xsl:when test='$LastPageNumber &gt; $CurrentPageNumber'>
	   <a href="fdaction:?action=nextpage&amp;pagetype=feed">  	 	  
	     <xsl:value-of select='localized:NextPageText()' />
	   </a><a href="fdaction:?action=nextpage&amp;pagetype=feed" class="img">  	 
	   <img src="$IMAGEDIR$rightarrow.gif" border="0"/>
	   </a>	    
	  </xsl:when>
	  <xsl:otherwise>
	   <xsl:value-of select='localized:NextPageText()' /><img src="$IMAGEDIR$rightarrow.gif" />	   
	  </xsl:otherwise>
</xsl:choose>
	 |&#160;<xsl:value-of select='localized:DisplayingPageText()' />&#160;<xsl:value-of select='$CurrentPageNumber' />&#160;<xsl:value-of select='localized:PageOfText()' />&#160;<xsl:value-of select='$LastPageNumber' />
	  	  
    </div>
    </xsl:if>
	</td>
  </tr>
</table>
</body></html>
  </xsl:template>


<!-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ -->
<!-- match single news item -->
<!-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ -->
<xsl:template match="newspaper[@type='newsitem']">
<html><head><title><xsl:value-of select='//item/title'/></title>
<base href="{//item/link}" />
<xsl:call-template name="embedded_style" />
</head>
  <body>
  <div class="PostItemContent" id="item1">
	<table height="100%" width="100%" >
	  <tr>
	<td class="PostFrame" height="100%" width="100%" valign="top">
	  <xsl:if test="//channel/image">
		<div class="PostTitle" style="FLOAT: right">
		  <a href="{//channel/image/link}" title="{//channel/image/title}"><img src="{//channel/image/url}"  alt="{//channel/image/title}" border="0">
		  <xsl:if test="//channel/image/width!=''"><xsl:attribute name="width"><xsl:value-of select="//channel/image/width"/></xsl:attribute></xsl:if>
		  <xsl:if test="//channel/image/height!=''"><xsl:attribute name="height"><xsl:value-of select="//channel/image/height"/></xsl:attribute></xsl:if>
		  </img></a>
		</div>
	  </xsl:if>
	  <div class="PostTitle">

         
        <xsl:variable name="itemID">
            <xsl:choose>
             <xsl:when test="//item/guid">
		<xsl:value-of select="//item/guid" />
	     </xsl:when>
	     <xsl:when test="//item/link">
		<xsl:value-of select="//item/link" />
	     </xsl:when>
	    </xsl:choose>
	  </xsl:variable>
	  
		<xsl:call-template name="process_item_read_flag_states">
		  <xsl:with-param name="current_position" select="position()" />
		  <xsl:with-param name="current_item" select="//item" />
		</xsl:call-template>
		
		<a href='{//item/link}'>
		  <xsl:value-of  disable-output-escaping='yes' select='//item/title'/>
		</a>
	  </div>
	  <div class="PostInfos">
		<b><xsl:value-of disable-output-escaping='yes' select="//item/category"/>&#32;
  </b>
	  </div>
	  <div class="PostContent">
		<xsl:choose>
		  <xsl:when test='//item/xhtml:body'>
		<xsl:copy-of select='//item/xhtml:body'/>
		  </xsl:when>
		  <xsl:when test='//item/content:encoded'>
		<xsl:value-of  disable-output-escaping='yes' select='//item/content:encoded'/>
		  </xsl:when>
		  <xsl:otherwise>
		<xsl:value-of disable-output-escaping='yes' select='//item/description'/>
		  </xsl:otherwise>
		</xsl:choose>
		
   <xsl:if test='//item/enclosure'>
	<p>
	<a class='img'>
	 
	 <xsl:attribute name="href"> 
	  <xsl:value-of  disable-output-escaping='yes' select='//item/enclosure/@url'/>
	 </xsl:attribute>
	 
	<img border="0" src="$IMAGEDIR$play.gif" height="16" width="16" >
	 <xsl:attribute name="alt"> 
	  <xsl:value-of  disable-output-escaping='yes' select='//item/enclosure/@url'/>
	 </xsl:attribute>
	</img>
	</a>
	<xsl:text disable-output-escaping='yes'>&amp;nbsp;</xsl:text>
	<xsl:text disable-output-escaping='yes'>&lt;a href='</xsl:text><xsl:value-of  disable-output-escaping='yes' select='//item/enclosure/@url'/><xsl:text disable-output-escaping='yes'>'&gt;</xsl:text>
	<xsl:value-of disable-output-escaping='yes' select='localized:ItemEnclosureText()' /> (<xsl:value-of select='//item/enclosure/@type'/>, <xsl:choose><xsl:when test='count(//item/enclosure/@length)=0 or //item/enclosure/@length &lt;= 0'>?</xsl:when><xsl:when test='//item/enclosure/@duration'><xsl:value-of select='//item/enclosure/@duration' /></xsl:when><xsl:otherwise><xsl:value-of select='//item/enclosure/@length'/> Bytes</xsl:otherwise></xsl:choose>)
	<xsl:text disable-output-escaping='yes'>&lt;/a></xsl:text>				
	</p>
	</xsl:if>
	
	  </div>
	  <div class="PostSignature">
		<a href='{//channel/link}' title='{//channel/description}'><xsl:value-of disable-output-escaping='yes'  select='//channel/title'/></a>
		<br />
		<xsl:if test="//item/dc:creator and //item/dc:creator!=''">
		<xsl:value-of disable-output-escaping='yes' select='localized:ItemPublisherText()' />:
		  <xsl:value-of disable-output-escaping='yes' select='//item/dc:creator'/>
		  <xsl:text disable-output-escaping='yes'>&amp;nbsp;</xsl:text>
		  </xsl:if><xsl:if test="//item/author and //item/author!=''">
		  <xsl:value-of disable-output-escaping='yes' select='//item/author'/>
		  <xsl:text disable-output-escaping='yes'>&amp;nbsp;</xsl:text>
		</xsl:if>
		<xsl:value-of select='//item/pubDate'/>
			
	  </div>
	</td>
	  </tr>
	</table>
	</div>
	</body></html>
  </xsl:template>

<!-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ -->
<!-- process one item and output flag and read state images -->
<!-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ -->
<xsl:template name="process_item_read_flag_states">
  <xsl:param name ="current_position" />
  <xsl:param name ="current_item" />
  
  <xsl:variable name="itemID">
    <xsl:choose>
      <xsl:when test="$current_item/guid">
		<xsl:value-of select="$current_item/guid" />
	  </xsl:when>
	  <xsl:when test="$current_item/link">
		<xsl:value-of select="$current_item/link" />
	  </xsl:when>
	</xsl:choose>
  </xsl:variable>
  
   <map name="{concat('flagstate', string($current_position))}">
	<area alt="{localized:ToggleFlagStateText()}" shape="rect" coords="0,0,16,16" href="{concat('fdaction:?action=toggleflag&amp;postid=', $itemID)}" />
  </map>
  <map name="{concat('readstate', string($current_position))}">
	<area alt="{localized:ToggleReadStateText()}" shape="rect" coords="0,0,16,16" href="{concat('fdaction:?action=toggleread&amp;postid=', $itemID)}" />
  </map>
  <xsl:if test="count($current_item/slash:comments | $current_item/wfw:commentRss) &gt; 0">
	<map name="{concat('watchstate', string($current_position))}">
	<area alt="{localized:ToggleWatchStateText()}" shape="rect" coords="0,0,20,16" href="{concat('fdaction:?action=togglewatch&amp;postid=', $itemID)}" />
  </map>	
  </xsl:if>
  <xsl:if test="count($current_item/gr:broadcast) &gt; 0">
    <map name="{concat('sharestate', string($current_position))}">
      <area alt="{localized:ToggleShareStateText()}" shape="rect" coords="0,0,16,16" href="{concat('fdaction:?action=toggleshare&amp;postid=', $itemID)}" />
    </map>
  </xsl:if>
  <xsl:if test="count($current_item/ng:clipped) &gt; 0">
    <map name="{concat('clipstate', string($current_position))}">
      <area alt="{localized:ToggleClipStateText()}" shape="rect" coords="0,0,16,16" href="{concat('fdaction:?action=toggleclip&amp;postid=', $itemID)}" />
    </map>
  </xsl:if>

  <xsl:choose>
    <xsl:when test="$current_item/fd:state[@read='1']">
	  <img  border="0" usemap="{concat('#readstate', string($current_position))}" class="icon" src="$IMAGEDIR$read.gif" onclick="swapImage(this, true)" /> 
	</xsl:when>
	<xsl:when test="$current_item/fd:state[@read='0']">
	  <img  border="0" usemap="{concat('#readstate', string($current_position))}" class="icon" src="$IMAGEDIR$unread.gif" onclick="swapImage(this, true)" /> 
	</xsl:when>                               
  </xsl:choose>
                       
  <xsl:choose>                                  
	<xsl:when test="$current_item/fd:state[@flagged='0']">
	  <img  border="0" usemap="{concat('#flagstate', string($current_position))}" class="icon" src="$IMAGEDIR$flag.clear.gif" onclick="swapImage(this, true)" /> 
	</xsl:when>
	<xsl:when test="$current_item/bndt:flag-status[. = 'Review']">
	  <img  border="0" usemap="{concat('#flagstate', string($current_position))}" class="icon" src="$IMAGEDIR$flag.yellow.gif" onclick="swapImage(this, true)" /> 
	</xsl:when>
	<xsl:when test="$current_item/bndt:flag-status[.='Read']">
	  <img  border="0" usemap="{concat('#flagstate', string($current_position))}" class="icon" src="$IMAGEDIR$flag.green.gif" onclick="swapImage(this, true)" /> 
	</xsl:when>
	<xsl:when test="$current_item/bndt:flag-status[.='Forward']">
	  <img  border="0" usemap="{concat('#flagstate', string($current_position))}" class="icon" src="$IMAGEDIR$flag.blue.gif" onclick="swapImage(this, true)" /> 
	</xsl:when>
	<xsl:when test="$current_item/bndt:flag-status[.='FollowUp']">
	  <img  border="0" usemap="{concat('#flagstate', string($current_position))}" class="icon" src="$IMAGEDIR$flag.red.gif" onclick="swapImage(this, true)" /> 
	</xsl:when>
	<xsl:when test="$current_item/bndt:flag-status[.='Reply']">
	  <img  border="0" usemap="{concat('#flagstate', string($current_position))}" class="icon" src="$IMAGEDIR$flag.purple.gif" onclick="swapImage(this, true)" /> 
	</xsl:when>
	<xsl:when test="$current_item/bndt:flag-status[.='Complete']">
	  <img  border="0" usemap="{concat('#flagstate', string($current_position))}" class="icon" src="$IMAGEDIR$flag.complete.gif" onclick="swapImage(this, true)" /> 
	</xsl:when>				
  </xsl:choose>     
  
  <xsl:if test="count($current_item/slash:comments | $current_item/wfw:commentRss) &gt; 0">   
	<xsl:choose>
    <xsl:when test="$current_item/bndt:watch-comments[.='1']">
	  <img  border="0" usemap="{concat('#watchstate', string($current_position))}" class="icon" src="$IMAGEDIR$been.watched.gif" onclick="swapImage(this, true)" /> 
	</xsl:when>
	<xsl:otherwise>
	  <img  border="0" usemap="{concat('#watchstate', string($current_position))}" class="icon" src="$IMAGEDIR$not.watched.gif" onclick="swapImage(this, true)" /> 
	</xsl:otherwise>                               
  </xsl:choose>	
  </xsl:if>

  <xsl:if test="count($current_item/gr:broadcast) &gt; 0">
    <xsl:choose>
      <xsl:when test="$current_item/gr:broadcast[.='1']">
        <img  border="0" usemap="{concat('#sharestate', string($current_position))}" class="icon" src="$IMAGEDIR$shared.png" onclick="swapImage(this, true)" />
      </xsl:when>
      <xsl:otherwise>
        <img  border="0" usemap="{concat('#sharestate', string($current_position))}" class="icon" src="$IMAGEDIR$unshared.gif" onclick="swapImage(this, true)" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:if>

  <xsl:if test="count($current_item/ng:clipped) &gt; 0">
    <xsl:choose>
      <xsl:when test="$current_item/ng:clipped[.='True']">
        <img  border="0" usemap="{concat('#clipstate', string($current_position))}" class="icon" src="$IMAGEDIR$newsbin.gif" onclick="swapImage(this, true)" />
      </xsl:when>
      <xsl:otherwise>
        <img  border="0" usemap="{concat('#clipstate', string($current_position))}" class="icon" src="$IMAGEDIR$clip.gif" onclick="swapImage(this, true)" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:if>

</xsl:template>


<!-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ -->
<xsl:template name="embedded_style">
<style type="text/css">
	body { background-color: #808080; color: black; font-family: verdana, arial, sans-serif; 
	margin: 5px; padding 0px;}
	
	a, a:visited, a:active { 
	color: #355EA0;
	text-decoration: none;
	border-bottom: 1px dotted #355EA0;
	}
	
	a.img { border-bottom: 0px hidden } 
	
	a:hover { 
	color: #FF6600;
	text-decoration: none;
	border-bottom: 1px dotted #FF6600;
	}
	
	div {
	/* font-family: "trebuchet ms", "lucida grande", verdana, arial, sans-serif;	*/
	font-family: verdana, arial, sans-serif;
	}
	
	td.PostFrame { background-color: white; border: 1px solid black; padding: 10px; }
	
	div.PostTitle { 
	/* font-family: "trebuchet ms", "lucida grande", verdana, arial, sans-serif;	*/
	font-family: verdana, arial, sans-serif;
	font-size: medium;
	}
	
	.PostTitle a, .PostTitle a:active, .PostTitle a:visited {
	border-width: 0px;
	color: #FF6600;
	font-weight: bold;
	text-decoration: none;
	}
	
	.PostTitle a:hover {
	border-width: 0px;
	text-decoration: underline;
	}
	
	div.PageNavigation {
	font-size: x-small;
	border-top: 1px dotted #CBCBCB;
	margin: 10px 0px 10px 0px;
	padding: 2px 0px 2px 0px;
	}
	
	div.PostInfos { 
	color: #808080;
	font-family: verdana, arial, sans-serif;	font-size: x-small;
	font-weight: normal;
	text-transform: none;
	}
	
	div.PostContent {
	font-size: x-small;
	border-top: 1px dotted #CBCBCB;	
	}
 
 	div.PostItemContent {
	font-size: x-small;
	margin: 10px 0px 10px 0px;
	padding: 10px 0px 10px 0px;
	border-bottom: 1px dotted #CBCBCB;
	}
	
	div.PostSignature { 
	text-align: right;
	font-family: verdana, arial, sans-serif;	
	font-size: xx-small; 
	font-style: italic;
	}
	
	.PostSignature a { font-size: x-small; }
</style>
<script> 

 var currentItem   = null; 
 var currentIndex  = 0; 
 var prevScrollPos = 0; 

 var FadeInterval = 200;
 var StartFadeAt = 7;

 var FadeSteps = new Array();
	FadeSteps[1] = "ff";
	FadeSteps[2] = "ee";
	FadeSteps[3] = "dd";
	FadeSteps[4] = "cc";
	FadeSteps[5] = "bb";
	FadeSteps[6] = "aa";
	FadeSteps[7] = "99"; 

 function swapImage(img, userClicked){ 

  if(userClicked){
   img.className="keepunread";
  }else if(img.className == "keepunread"){
	return; 
  } 

  var oldSrc = img.src; 
  var folderEnd = oldSrc.lastIndexOf('/') + 1; 


  if(img.useMap.indexOf('#readstate')==0){
     if(oldSrc.indexOf('unread.gif')== -1){
      img.src = oldSrc.substr(0, folderEnd) + 'unread.gif'; 
     }else{
      img.src = oldSrc.substr(0, folderEnd) + 'read.gif'; 
     }
  }else if(img.useMap.indexOf('#watchstate')==0){
     if(oldSrc.indexOf('been.watched.gif')== -1){
      img.src = oldSrc.substr(0, folderEnd) + 'been.watched.gif'; 
     }else{
      img.src = oldSrc.substr(0, folderEnd) + 'not.watched.gif'; 
     }
  }else if(img.useMap.indexOf('#flagstate')==0){
     if(oldSrc.indexOf('flag.clear.gif')== -1){
      img.src = oldSrc.substr(0, folderEnd) + 'flag.clear.gif'; 
     }else{
      img.src = oldSrc.substr(0, folderEnd) + 'flag.red.gif'; 
     }
  }else if(img.useMap.indexOf('#sharestate')==0){
     if(oldSrc.indexOf('shared.png')== -1){
      img.src = oldSrc.substr(0, folderEnd) + 'shared.png'; 
     }else{
      img.src = oldSrc.substr(0, folderEnd) + 'unshared.gif'; 
     }
  }else if(img.useMap.indexOf('#clipstate')==0){
     if(oldSrc.indexOf('clip.gif')== -1){
      img.src = oldSrc.substr(0, folderEnd) + 'clip.gif'; 
     }else{
      img.src = oldSrc.substr(0, folderEnd) + 'newsbin.gif'; 
     }
  }

 }

function deselect(target){

  if(target == null) return; 

 target.style.backgroundColor = "#ffffff";	
 target.style.backgroundColor = "transparent";		

} 

// This is the recursive function call that actually performs the fade
function highlightCurrentItem(colorId) {

	if(currentItem == null) return; 

    if (colorId <xsl:text disable-output-escaping='yes'>&gt;=</xsl:text> 2) {
		currentItem.style.backgroundColor = "#ffff" + FadeSteps[colorId];		      
		
		colorId--; 
		
        // Wait a little bit and fade another shade
        setTimeout("highlightCurrentItem("+colorId+")", FadeInterval);
	}
}

function markItemRead(target){
 
  var images = target.getElementsByTagName("img"); 
  var img    = images[0]; 
    
  if((img.className != "keepunread") <xsl:text disable-output-escaping='yes'>&amp;&amp;</xsl:text>(img.src.indexOf('unread.gif')!= -1)){
   swapImage(img, false);
   
   var oldHref = target.childNodes[1].childNodes[0].href;
   target.childNodes[1].childNodes[0].href = target.childNodes[1].childNodes[0].href.replace("fdaction:?action=toggleread", "fdaction:?action=markread"); 
   target.childNodes[1].childNodes[0].click(); 
   target.childNodes[1].childNodes[0].href = oldHref; 
  }

}

function getItemDivs(){

	var allDivs = document.getElementsByTagName("DIV");
	var itemDivs = new Array(); 
	
	for(var i = 0; i <xsl:text disable-output-escaping='yes'>&lt;</xsl:text> allDivs.length; i++){
		if(allDivs[i].id.indexOf("item")==0){ 
		  itemDivs.push(allDivs[i]);
		}
	}	
	return itemDivs	
}

  function handleScroll(){
  
  //try{  

  // Detect currently visible feed item 
  var items  = getItemDivs();  
  var previousItem = currentItem; 
  var previousIndex = currentIndex; 
  var movedDown     =  (prevScrollPos  <xsl:text disable-output-escaping='yes'>&lt;</xsl:text> body.scrollTop);
  var itemSelected  = false;  

  /* locate and highlight currently visible item, then deselect previous item */  
  for(var i = 0; i <xsl:text disable-output-escaping='yes'>&lt;</xsl:text> items.length; i++){  
    var item = items[i];
   
   //is current item the topmost one? 
   if((item.offsetTop <xsl:text disable-output-escaping='yes'>&lt;=</xsl:text> body.scrollTop) <xsl:text disable-output-escaping='yes'>&amp;&amp;</xsl:text> ((item.offsetTop + item.offsetHeight + 10 /* padding */) <xsl:text disable-output-escaping='yes'>&gt;=</xsl:text> body.scrollTop)){
        
        currentItem  = item;  
        currentIndex = i; 
         
       //if there is no next item or item is only one on screen then it is to be highlighted
       if( ((i + 1) ==  items.length) ||        
           ((item.offsetTop + item.offsetHeight) <xsl:text disable-output-escaping='yes'>&gt;=</xsl:text> (body.scrollTop + body.offsetHeight))){           
          itemSelected = true;           
          break;           
       }             
		
       var nextItem = items[i + 1];  
       
       //is the next item 100% visible or does it take up a third or more of the page? 
       // if so we should highlight it       
       if( (nextItem.offsetTop + nextItem.offsetHeight) <xsl:text disable-output-escaping='yes'>&lt;=</xsl:text> (body.scrollTop + body.offsetHeight) 
          ||  
           (((body.scrollTop - nextItem.offsetTop) / (body.offsetHeight * 1.0)) <xsl:text disable-output-escaping='yes'>&gt;</xsl:text> 0.34) 
          ) { 
       
        currentItem  = nextItem; 
        currentIndex = i + 1; 
       } 
       
       itemSelected = true;        
       break; 
   }/* if */
  
  }/* for */   
  
  if(!itemSelected){
   currentItem = items[0]; 
   currentIndex = 0; 
  }
  
  if( previousItem == null || currentItem.id != previousItem.id){
    highlightCurrentItem(StartFadeAt); 
    deselect(previousItem); 
  }
  
  /* For each item between current and previous, If unread we mark it read then call out to RSS Bandit by calling click() */
  if(movedDown){
  
   for(var j = previousIndex; j <xsl:text disable-output-escaping='yes'>&lt;=</xsl:text> currentIndex; j++){
	markItemRead(items[j]);
   }
   
  }
 
 //}catch(err){}
  
 prevScrollPos = body.scrollTop;  
    
    
}

 
</script>
<script>
window.onerror = handleError;

function handleError(msg, file_loc, line_no) {
   return true;
}
</script>
</xsl:template>

</xsl:stylesheet>