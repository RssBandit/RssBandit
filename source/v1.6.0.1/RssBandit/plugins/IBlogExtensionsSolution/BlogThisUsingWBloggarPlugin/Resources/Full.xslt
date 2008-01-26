<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" /> 
	<xsl:variable name="feed-title" select="/rss/channel/title" />
	
	<xsl:template match="/">
		<xsl:apply-templates select="//item" />
	</xsl:template>

	<xsl:template match="/rss/channel/item">
		<title>RE: <xsl:value-of select="title" /></title>
		<xsl:choose>
			<xsl:when test="description">
				<blockquote>
					<xsl:value-of disable-output-escaping="yes" select="description" />
				</blockquote>
			</xsl:when>
			<xsl:otherwise xmlns:xhtml="http://www.w3.org/1999/xhtml">
				<blockquote>
					<xsl:copy-of select="xhtml:body" />
				</blockquote>
			</xsl:otherwise> 
		</xsl:choose> 
		<i>[Via <xsl:choose>
			<xsl:when test="link"><a href="{link}"><xsl:value-of select="$feed-title" /></a></xsl:when>
			<xsl:otherwise><xsl:value-of select="$feed-title" /></xsl:otherwise> 
		</xsl:choose>]</i>
	</xsl:template> 
</xsl:stylesheet>