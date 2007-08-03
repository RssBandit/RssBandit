<?xml version="1.0" encoding="UTF-8" ?>
<!-- Begin subscriptions.xml to feedlist.xml XSLT -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  
 xmlns="http://www.25hoursaday.com/2003/RSSBandit/feeds/">

  <xsl:output method="xml" indent="yes" />

<xsl:template match="*">
  <xsl:if test="not(local-name()='deleted-stories') and not(local-name()='listview-layout') and not(local-name()='stylesheet') and not(local-name()='favicon') and not(local-name()='enclosure-folder') and not(local-name()='download-enclosures')">    
    <xsl:element name="{local-name()}" namespace="http://www.25hoursaday.com/2003/RSSBandit/feeds/">
	<xsl:apply-templates select="child::node()|@*"/>  
    </xsl:element>
  </xsl:if>
</xsl:template>

  <xsl:template match="@*">
     <xsl:if test="not(local-name()='deleted-stories') and not(local-name()='listview-layout') and not(local-name()='stylesheet') and not(local-name()='favicon') and not(local-name()='enclosure-folder') and not(local-name()='download-enclosures') and not(local-name()='refresh-rate')">    
       <xsl:copy />
     </xsl:if>
  </xsl:template>

  <xsl:template match="text()">
    <xsl:copy />
  </xsl:template>
   
</xsl:stylesheet>
<!-- End  subscriptions.xml to feedlist.xml XSLT -->
  