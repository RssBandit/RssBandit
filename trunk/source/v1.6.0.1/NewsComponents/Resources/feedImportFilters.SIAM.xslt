<?xml version="1.0" encoding="UTF-8" ?>
<!-- Begin SIAM XSLT -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns="http://www.25hoursaday.com/2004/RSSBandit/feeds/" xmlns:bndt="http://www.25hoursaday.com/2004/RSSBandit/feeds/" xmlns:siam="http://groups.yahoo.com/information_aggregators/2004/01/siam/" exclude-result-prefixes="msxsl siam">
  <xsl:output method="xml" indent="yes" />
  <xsl:variable name="feeds">
    <xsl:for-each select="/siam:feeds//siam:feed">
      <feed>
        <xsl:if test="parent::*[local-name()='category']">
          <xsl:attribute name="category">
            <xsl:for-each select="ancestor::*[local-name()='category']">
              <xsl:if test="@title">
                <xsl:value-of select="@title" />
                <xsl:if test="last() != position()">\</xsl:if>
              </xsl:if>
            </xsl:for-each>
          </xsl:attribute>
        </xsl:if>
        <title>
          <xsl:choose>
            <xsl:when test="@title">
              <xsl:value-of select="@title" />
            </xsl:when>
            <xsl:otherwise>
              <link>No title for RSS feed provided in imported SIAM</link>
            </xsl:otherwise>
          </xsl:choose>
        </title>

        <xsl:choose>
          <xsl:when test="@xmlUrl and (string-length(@xmlUrl) &gt; 0)">
            <link>
              <xsl:value-of select="@xmlUrl" />
            </link>
          </xsl:when>
          <xsl:when test="@xmlurl and (string-length(@xmlurl) &gt; 0)">
            <link>
              <xsl:value-of select="@xmlurl" />
            </link>
          </xsl:when>
          <xsl:otherwise>
            <link>http://www.example.com/no-url-for-rss-feed-provided-in-imported-siam</link>
          </xsl:otherwise>
        </xsl:choose>

        <xsl:if test="siam:items">
          <stories-recently-viewed>
	    <xsl:for-each select="siam:items/siam:item">
	      <xsl:if test="@link and not(contains(@status,'unread'))">
	      <story><xsl:value-of select="@link" /></story>
	      </xsl:if>
	    </xsl:for-each>
	  </stories-recently-viewed>
        </xsl:if>

      </feed>
    </xsl:for-each>
  </xsl:variable>
  <xsl:variable name="categories">
    <xsl:for-each select="msxsl:node-set($feeds)//@category">
      <xsl:sort select="string(.)" />
      <category>
        <xsl:value-of select="." />
      </category>
    </xsl:for-each>
  </xsl:variable>
  <xsl:template match="/">
    <feeds>
      <xsl:copy-of select="$feeds" />
      <xsl:if test="count(msxsl:node-set($categories)/bndt:category) &gt; 0">
        <categories>
          <xsl:for-each select="msxsl:node-set($categories)/bndt:category">
            <xsl:choose>
              <xsl:when test="position() = 1">
                <xsl:copy-of select="." />
              </xsl:when>
              <xsl:when test="not(string(preceding::bndt:category[1]) = string(.))">
                <xsl:copy-of select="." />
              </xsl:when>
            </xsl:choose>
          </xsl:for-each>
        </categories>
      </xsl:if>
    </feeds>
  </xsl:template>
</xsl:stylesheet>
<!-- End SIAM XSLT -->
  