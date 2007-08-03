<xsl:stylesheet version='1.0' 
xmlns:xsl='http://www.w3.org/1999/XSL/Transform'
xmlns:content = 'http://purl.org/rss/1.0/modules/content/'
xmlns:xhtml='http://www.w3.org/1999/xhtml'
xmlns:slash='http://purl.org/rss/1.0/modules/slash/' 
xmlns:dc='http://purl.org/dc/elements/1.1/' 
xmlns:fd='http://www.bradsoft.com/feeddemon/xmlns/1.0/'
xmlns:bndt='http://www.25hoursaday.com/2003/RSSBandit/feeds/'
xmlns:localized='urn:localization-extension'
xmlns:atom='http://www.w3.org/2005/Atom'
exclude-result-prefixes='content slash dc fd bndt localized atom'>

<xsl:output method='xml' indent='yes' /> 

<!-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ -->
<!-- match channel group newspaper -->
<!-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ -->
<xsl:template match="newspaper[@type='group']">
<html><head><title><xsl:value-of select='title'/></title>
<!-- <base href="{//channel/link}" /> -->
<xsl:call-template name="embedded_style" />
</head>
<body id="body">
<table height="100%" width="100%">			  
<xsl:for-each select="//channel">
<xsl:sort select="current()/title" />
<tr>
<td class="PostFrame" height="100%" width="100%" valign="top">
  <xsl:if test="current()/image">
<div class="FeedTitle" style="FLOAT: right"> 
  <a href="{current()/image/link}" title="{current()/image/title}"><img src="{current()/image/url}"  alt="{current()/image/title}" border="0">
  <xsl:if test="current()/image/width!=''"><xsl:attribute name="width"><xsl:value-of select="current()/image/width"/></xsl:attribute></xsl:if>
  <xsl:if test="current()/image/height!=''"><xsl:attribute name="height"><xsl:value-of select="current()/image/height"/></xsl:attribute></xsl:if>
  </img></a>
</div>
  </xsl:if>
  <div class="FeedTitle">			
<a href='{current()/link}' style='text-decoration: none;'>
  <xsl:value-of  disable-output-escaping='yes' select='current()/title'/>
</a>
  </div>
 
  <xsl:variable name="outerposition" select="position()" />
  <div class="PostContent">
   <xsl:for-each select='current()//item'>

	<div class="PostItemContent" id="{concat('item',string($outerposition), 'in' , string(position()))}" >
	
		
  <div class="PostInfos">	
  <xsl:call-template name="process_item_read_flag_states">
	  <xsl:with-param name="current_position" select="concat(string($outerposition), 'in' , string(position()))" />
	  <xsl:with-param name="current_item" select="." />
	</xsl:call-template>
	<a href='{current()/link}'>
	  <b><xsl:value-of disable-output-escaping='yes' select="current()/title"/></b>
	</a>		
  </div>
 
 <div class="PostBody">
   <xsl:value-of disable-output-escaping='yes' select='current()/atom:summary'/> <b>...</b>
 </div>	
	
   <div class="PostLink">
     <xsl:value-of select="substring-after(string(current()/link),'://')" /> -   	
   <nobr>
   <a href="{concat('http://www.technorati.com/search/', substring-after(string(current()/link),'://'))}" class="fl">
     <xsl:value-of disable-output-escaping='yes' select='localized:RelatedLinksText()' />	 
   </a>
   </nobr>
    </div>
  
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
	  <xsl:if test='current()/enclosure'>
	<br />
	<xsl:text disable-output-escaping='yes'>&amp;nbsp;</xsl:text>
	<xsl:text disable-output-escaping='yes'>&lt;a href='</xsl:text><xsl:value-of  disable-output-escaping='yes' select='current()/enclosure/@url'/><xsl:text disable-output-escaping='yes'>'&gt;</xsl:text>
	<xsl:value-of disable-output-escaping='yes' select='localized:ItemEnclosureText()' /> (<xsl:value-of select='current()/enclosure/@type'/>, <xsl:choose><xsl:when test='count(current()/enclosure/@length)=0 or current()/enclosure/@length &lt;= 0'>?</xsl:when><xsl:otherwise><xsl:value-of select='current()/enclosure/@length'/></xsl:otherwise></xsl:choose> Bytes)
	<xsl:text disable-output-escaping='yes'>&lt;/a></xsl:text>				
	  </xsl:if>			
	  
	</div>
	</div>	
  </xsl:for-each>
</div>
  </td>
</tr>
</xsl:for-each>
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
		<xsl:if test='current()/enclosure'>
		  <br />
		  <xsl:text disable-output-escaping='yes'>&amp;nbsp;</xsl:text>
		  <xsl:text disable-output-escaping='yes'>&lt;a href='</xsl:text><xsl:value-of  disable-output-escaping='yes' select='current()/enclosure/@url'/><xsl:text disable-output-escaping='yes'>'&gt;</xsl:text>
		  <xsl:value-of disable-output-escaping='yes' select='localized:ItemEnclosureText()' /> (<xsl:value-of select='current()/enclosure/@type'/>, <xsl:choose><xsl:when test='count(current()/enclosure/@length)=0 or current()/enclosure/@length &lt;= 0'>?</xsl:when><xsl:otherwise><xsl:value-of select='current()/enclosure/@length'/></xsl:otherwise></xsl:choose> Bytes)
		  <xsl:text disable-output-escaping='yes'>&lt;/a></xsl:text>				
		</xsl:if>			
		
	  </div>
	  </div>
	</xsl:for-each>
	  </div>
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
		<b><xsl:value-of disable-output-escaping='yes' select="//item/category"/></b>
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
		<xsl:if test='//item/enclosure'>
		  <br />
		  <xsl:text disable-output-escaping='yes'>&amp;nbsp;</xsl:text>
		  <xsl:text disable-output-escaping='yes'>&lt;a href='</xsl:text><xsl:value-of  disable-output-escaping='yes' select='//item/enclosure/@url'/><xsl:text disable-output-escaping='yes'>'&gt;</xsl:text>
		  <xsl:value-of disable-output-escaping='yes' select='localized:ItemEnclosureText()' /> (<xsl:value-of select='//item/enclosure/@type'/>, <xsl:choose><xsl:when test='count(//item/enclosure/@length)=0 or //item/enclosure/@length &lt;= 0'>?</xsl:when><xsl:otherwise><xsl:value-of select='//item/enclosure/@length'/></xsl:otherwise></xsl:choose> Bytes)
		  <xsl:text disable-output-escaping='yes'>&lt;/a></xsl:text>				
		</xsl:if>			
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
	<area shape="rect" coords="0,0,16,16" href="{concat('fdaction:?action=toggleflag&amp;postid=', $itemID)}" />
  </map>
  <map name="{concat('readstate', string($current_position))}">
	<area shape="rect" coords="0,0,16,16" href="{concat('fdaction:?action=toggleread&amp;postid=', $itemID)}" />
  </map>

  <xsl:choose>
    <xsl:when test="$current_item/fd:state[@read='1']">
	  <img alt="Mark read or unread" border="0" usemap="{concat('#readstate', string($current_position))}" class="icon" src="$IMAGEDIR$read.gif" onclick="swapImage(this)" /> 
	</xsl:when>
	<xsl:when test="$current_item/fd:state[@read='0']">
	  <img alt="Mark read or unread" border="0" usemap="{concat('#readstate', string($current_position))}" class="icon" src="$IMAGEDIR$unread.gif" onclick="swapImage(this)" /> 
	</xsl:when>                               
  </xsl:choose>
                       
  <xsl:choose>                                  
	<xsl:when test="$current_item/fd:state[@flagged='0']">
	  <img alt="{localized:ToggleFlagStateText()}" border="0" usemap="{concat('#flagstate', string($current_position))}" class="icon" src="$IMAGEDIR$flag.clear.gif" onclick="swapImage(this)" /> 
	</xsl:when>
	<xsl:when test="$current_item/bndt:flag-status[. = 'Review']">
	  <img alt="{localized:ToggleFlagStateText()}" border="0" usemap="{concat('#flagstate', string($current_position))}" class="icon" src="$IMAGEDIR$flag.yellow.gif" onclick="swapImage(this)" /> 
	</xsl:when>
	<xsl:when test="$current_item/bndt:flag-status[.='Read']">
	  <img alt="{localized:ToggleFlagStateText()}" border="0" usemap="{concat('#flagstate', string($current_position))}" class="icon" src="$IMAGEDIR$flag.green.gif" onclick="swapImage(this)" /> 
	</xsl:when>
	<xsl:when test="$current_item/bndt:flag-status[.='Forward']">
	  <img alt="{localized:ToggleFlagStateText()}" border="0" usemap="{concat('#flagstate', string($current_position))}" class="icon" src="$IMAGEDIR$flag.blue.gif" onclick="swapImage(this)" /> 
	</xsl:when>
	<xsl:when test="$current_item/bndt:flag-status[.='FollowUp']">
	  <img alt="{localized:ToggleFlagStateText()}" border="0" usemap="{concat('#flagstate', string($current_position))}" class="icon" src="$IMAGEDIR$flag.red.gif" onclick="swapImage(this)" /> 
	</xsl:when>
	<xsl:when test="$current_item/bndt:flag-status[.='Reply']">
	  <img alt="{localized:ToggleFlagStateText()}" border="0" usemap="{concat('#flagstate', string($current_position))}" class="icon" src="$IMAGEDIR$flag.purple.gif" onclick="swapImage(this)" /> 
	</xsl:when>
	<xsl:when test="$current_item/bndt:flag-status[.='Complete']">
	  <img alt="{localized:ToggleFlagStateText()}" border="0" usemap="{concat('#flagstate', string($current_position))}" class="icon" src="$IMAGEDIR$flag.complete.gif" onclick="swapImage(this)" /> 
	</xsl:when>				
  </xsl:choose>     
</xsl:template>


<!-- ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ -->
<xsl:template name="embedded_style">
<style type="text/css">
	body,td,div,.p,a{font-family:arial,sans-serif}
	
	div.FeedTitle{
	background:#e5ecf9;
	color:#000;
	border-top:1px solid #36c;
	font-weight: bold;
	text-decoration: none;
	}
	
	.fl:link{color:#77c}
	
	a:link,.w,a.w:link,.w a:link,.q:visited,.q:link,.q:active,.q{color:#00c}
	a:visited,.fl:visited{color:#551a8b}
	a:active,.fl:active{color:red}

	a:hover { 	
	text-decoration: none;
	border-bottom: 1px dotted;
	}
		
	div,td{color:#000}
	
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
	
	div.PostLink{
	color:green;
	font-size: smaller;
	}
	
	div.PostBody{
		font-family: verdana, arial, sans-serif;
		font-weight: normal;
		text-transform: none;
		font-size: smaller;
	}
	
	div.PostInfos { 
	color: #808080;
	font-family: verdana, arial, sans-serif;	font-size: x-small;
	font-weight: normal;
	text-transform: none;
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

 function swapImage(img){

  var oldSrc = img.src; 
  var folderEnd = oldSrc.lastIndexOf('/') + 1; 


  if(img.useMap.indexOf('#readstate')==0){
     if(oldSrc.indexOf('unread.gif')== -1){
      img.src = oldSrc.substr(0, folderEnd) + 'unread.gif'; 
     }else{
      img.src = oldSrc.substr(0, folderEnd) + 'read.gif'; 
     }
  }else if(img.useMap.indexOf('#flagstate')==0){
     if(oldSrc.indexOf('flag.clear.gif')== -1){
      img.src = oldSrc.substr(0, folderEnd) + 'flag.clear.gif'; 
     }else{
      img.src = oldSrc.substr(0, folderEnd) + 'flag.red.gif'; 
     }
  }

 }


 
</script>
</xsl:template>

</xsl:stylesheet>