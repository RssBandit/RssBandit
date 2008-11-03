<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" /> 
	<xsl:template match="/">
		<title><xsl:value-of select="/rss/channel/item/title" /></title>
		<xsl:choose><xsl:when test="/rss/channel/managingEditor"><xsl:value-of select="/rss/channel/managingEditor" /> writes about </xsl:when><xsl:otherwise>Found this interesting post with the title </xsl:otherwise></xsl:choose>"<xsl:apply-templates select="//item" />" on <a href="{/rss/channel/link}"><xsl:choose><xsl:when test="/rss/channel/title"><xsl:value-of select="/rss/channel/title" /></xsl:when><xsl:otherwise>this blog</xsl:otherwise></xsl:choose></a>.
	</xsl:template>

	<xsl:template match="/rss/channel/item"><xsl:choose><xsl:when test="link"><a href="{link}"><xsl:value-of select="title" /></a></xsl:when><xsl:otherwise><xsl:value-of select="title" /></xsl:otherwise></xsl:choose></xsl:template>
</xsl:stylesheet>