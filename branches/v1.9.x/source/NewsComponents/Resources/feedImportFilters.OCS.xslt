<?xml version="1.0" encoding="UTF-8" ?>
<!-- Begin OCS XSLT -->
<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns='http://www.25hoursaday.com/2004/RSSBandit/feeds/' xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#' xmlns:dc='http://purl.org/metadata/dublin_core#' exclude-result-prefixes='dc rdf'>
	<xsl:output method='xml' indent='yes' />
	<xsl:template match='/'>
		<feeds>
			<xsl:for-each select='/rdf:RDF/rdf:description/rdf:description'>
				<feed>
					<title>
						<xsl:choose>
							<xsl:when test='dc:title'>
								<xsl:value-of select='dc:title' />
							</xsl:when>										
							<xsl:otherwise>
							<link>No title for RSS feed provided in imported OPML</link>
							</xsl:otherwise>
						</xsl:choose> 
					</title>
					<link>
					<xsl:choose>
							<xsl:when test='rdf:description/@about'>
								<xsl:value-of select='rdf:description/@about' />
							</xsl:when>										
							<xsl:otherwise>
							<link>No URL for RSS feed provided in imported OPML</link>
							</xsl:otherwise>
						</xsl:choose>
					</link>
				</feed>
			</xsl:for-each>
		</feeds>
	</xsl:template>
</xsl:stylesheet>
<!-- End OCS XSLT -->
  