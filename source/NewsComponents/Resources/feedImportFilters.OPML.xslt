<?xml version="1.0" encoding="UTF-8" ?>
<!-- Begin OPML XSLT -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns="http://www.25hoursaday.com/2004/RSSBandit/feeds/" xmlns:bndt="http://www.25hoursaday.com/2004/RSSBandit/feeds/" xmlns:ng="http://newsgator.com/schema/opml" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes" />
  <xsl:variable name="feeds">
    <xsl:for-each select="/opml/body//outline">
      <xsl:if test="count(child::*)=0">
        <feed>
          <xsl:if test="parent::*[name()='outline'] and ancestor::*[@title or @text]">
            <xsl:attribute name="category">
              <xsl:for-each select="ancestor::*[name()='outline']">
                <xsl:if test="@title or @text">
                  <xsl:choose>
                    <xsl:when test="@text">
                      <xsl:value-of select="@text" />
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="@title" />
                    </xsl:otherwise>
                  </xsl:choose>
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
              <xsl:when test="@text">
                <xsl:value-of select="@text" />
              </xsl:when>
              <xsl:otherwise>
                <link>No title for RSS feed provided in imported OPML</link>
              </xsl:otherwise>
            </xsl:choose>
          </title>
          <xsl:choose>
            <xsl:when test="@xmlUrl and (string-length(@xmlUrl) > 0)">
              <link>
                <xsl:value-of select="@xmlUrl" />
              </link>
            </xsl:when>
            <xsl:when test="@xmlurl and (string-length(@xmlurl) > 0)">

              <link>
                <xsl:value-of select="@xmlurl" />
              </link>
            </xsl:when>
            <xsl:otherwise>
              <link>http://www.example.com/no-url-for-rss-feed-provided-in-imported-opml</link>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:for-each select="attribute::*[namespace-uri()='http://newsgator.com/schema/opml']">
            <xsl:element name="{local-name()}" namespace="{namespace-uri()}">
              <xsl:value-of select="." />
            </xsl:element>
          </xsl:for-each>
        </feed>
      </xsl:if>
    </xsl:for-each>
  </xsl:variable>
  <xsl:variable name="categories">
    <!-- 
    <xsl:for-each select="msxsl:node-set($feeds)//@category">
      <xsl:sort select="string(.)" />
      <category>    
	<xsl:value-of select="." />     
      </category>
    </xsl:for-each>
-->
    <xsl:for-each select="/opml/body//outline">
      <xsl:if test="count(child::*)!=0 or (boolean(./@xmlUrl)=false and boolean(./@xmlurl)=false)">
        <category>
          <xsl:if test="@ng:id">
            <xsl:attribute name="folderId" namespace="http://newsgator.com/schema/opml">
              <xsl:value-of select="string(@ng:id)" />
            </xsl:attribute>
          </xsl:if>
          <xsl:for-each select="ancestor-or-self::*[name()='outline']">
            <xsl:if test="@title or @text">
              <xsl:choose>
                <xsl:when test="@text">
                  <xsl:value-of select="@text" />
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="@title" />
                </xsl:otherwise>
              </xsl:choose>
              <xsl:if test="last() != position()">\</xsl:if>
            </xsl:if>
          </xsl:for-each>
        </category>
      </xsl:if>
    </xsl:for-each>
  </xsl:variable>
  <xsl:template match="/">
    <feeds>
      <xsl:copy-of select="$feeds" />
      <xsl:if test="count(msxsl:node-set($categories)) &gt; 0">
        <categories>
          <xsl:copy-of select="$categories" />
        </categories>
      </xsl:if>
    </feeds>
  </xsl:template>
</xsl:stylesheet>
<!-- End OPML XSLT -->