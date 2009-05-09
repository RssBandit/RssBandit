<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl fb localized bndt"
    xmlns:fb="http://api.facebook.com/1.0/"
    xmlns:bndt='http://www.25hoursaday.com/2003/RSSBandit/feeds/'
    xmlns:localized='urn:localization-extension'
>
    <xsl:output method="xml" indent="yes"/>


  <xsl:param name="UserID" />
  <xsl:param name="FeedTitle" />

  <msxsl:script language="C#" implements-prefix="bndt">
    <![CDATA[
 public static string ConvertFromUnixTimestamp(double timestamp)  
{    
    DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
    return origin.AddSeconds(timestamp).ToString("o");
}

public static string GetRelativeTime(double timestamp){
    DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
    DateTime date = origin.AddSeconds(timestamp);
    
    TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - date.Ticks);
    double delta = ts.TotalSeconds;
    
const int SECOND = 1;
const int MINUTE = 60 * SECOND;
const int HOUR = 60 * MINUTE;
const int DAY = 24 * HOUR;
const int MONTH = 30 * DAY;

if (delta < 1 * MINUTE)
{
  return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";
}
if (delta < 2 * MINUTE)
{
  return "a minute ago";
}
if (delta < 55 * MINUTE)
{
  return ts.Minutes + " minutes ago";
}
if (delta < 90 * MINUTE)
{
  return "an hour ago";
}
if (delta < 24 * HOUR)
{
  return ts.Hours + " hours ago";
}
if (delta < 48 * HOUR)
{
  return "yesterday";
}
if (delta < 30 * DAY)
{
  return ts.Days + " days ago";
}
if (delta < 12 * MONTH)
{
  int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
  return months <= 1 ? "one month ago" : months + " months ago";
}
else
{
  int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
  return years <= 1 ? "one year ago" : years + " years ago";
}

}
  ]]>
  </msxsl:script>

  <msxsl:script language="C#" implements-prefix="localized">
    <![CDATA[
 public static string GetSharingMessage(string type)  
{    
    string message = String.Empty; 
    switch(type){
     case "link":
       message = "shared a link"; 
       break; 
     case "image":
       message = "shared a photo"; 
       break; 
     case "flash":
     case "video":
       message = "shared a video"; 
       break; 
     case "mp3": 
       message = "shared a song"; 
       break; 
     default: 
       break; 
    }
    return message; 
}

public static string GetVideoHoverText(){
  return "Click to play video"; 
}

  ]]>
  </msxsl:script>

  <xsl:template match="fb:posts">
      <feed xmlns="http://www.w3.org/2005/Atom">
        <id>http://www.facebook.com/profile.php?id=<xsl:value-of select="$UserID"/>
      </id>
        <title>
          <xsl:value-of select="$FeedTitle"/>
        </title>
        <link>
          <xsl:attribute name="href">http://www.facebook.com/profile.php?id=<xsl:value-of select="$UserID"/></xsl:attribute>
        </link> 
        <icon>http://www.facebook.com/favicon.ico</icon>
        <logo>http://creative.ak.facebook.com/ads3/creative/pressroom/jpg/b_1234209334_facebook_logo.jpg</logo>

        <xsl:for-each select="/fb:stream_get_response/fb:posts/fb:stream_post" >
          <entry>
            <xsl:variable name="userid" select="string(fb:actor_id)" />
            <id>
              <xsl:value-of select="fb:post_id"/>              
            </id>
            <title>
             <xsl:value-of select="//fb:profile[fb:id = $userid]/fb:name"/>: 
              <xsl:choose>
                <xsl:when test="fb:attachment/fb:media">
                  <xsl:value-of select="localized:GetSharingMessage(string(fb:attachment/fb:media//fb:type))"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="fb:message"/>
                </xsl:otherwise>
              </xsl:choose>
            </title>
            <link>
              <xsl:attribute name="href">
                <xsl:value-of select="fb:permalink"/>
              </xsl:attribute>
            </link>
            <published>             
              <xsl:value-of select="bndt:ConvertFromUnixTimestamp(number(fb:created_time))"/>
            </published>
            <updated>
              <xsl:value-of select="bndt:ConvertFromUnixTimestamp(number(fb:updated_time))"/>
            </updated>
            <author>
              <name>
                <xsl:value-of select="//fb:profile[fb:id = $userid]/fb:name"/>
              </name>
              <uri>
                <xsl:value-of select="//fb:profile[fb:id = $userid]/fb:url"/>
              </uri>
            </author>
            <content type="xhtml">
              <div xmlns="http://www.w3.org/1999/xhtml">
                <div class="UIIntentionalStory_Content">
                  <a class="UIIntentionalStory_Pic">
                    <xsl:attribute name="href">
                      <xsl:value-of select="//fb:profile[fb:id = $userid]/fb:url"/>
                    </xsl:attribute>
                    <span class="UIRoundedImage UIRoundedImage_WHITE UIRoundedImage_LARGE">
                      <img class="UIRoundedImage_Image">
                        <xsl:attribute name="src">
                          <xsl:value-of select="//fb:profile[fb:id = $userid]/fb:pic_square"/>
                        </xsl:attribute>
                        <xsl:attribute name="alt">
                          <xsl:value-of select="//fb:profile[fb:id = $userid]/fb:name"/>
                        </xsl:attribute>
                      </img>
                    </span>
                  </a>
                  <div class="UIIntentionalStory_Body">
                    <div class="UIIntentionalStory_Header">
                      <h3 class="UIIntentionalStory_Message">
                        <span class="UIIntentionalStory_Names">
                          <a>
                            <xsl:attribute name ="href">
                              <xsl:value-of select="//fb:profile[fb:id = $userid]/fb:url"/>
                            </xsl:attribute>
                            <xsl:value-of select="//fb:profile[fb:id = $userid]/fb:name"/>
                          </a>
                        </span>
                        <xsl:value-of select="fb:message"/>
                      </h3>
                    </div>
                    <xsl:if test="fb:attachment/fb:media">
                      <div class="UIStoryAttachment">
                        <div class="UIStoryAttachment_Media UIStoryAttachment_MediaSingle">
                          <div class="UIMediaItem">
                            <xsl:choose>
                              <xsl:when test="fb:attachment/fb:media/fb:stream_media/fb:video">
                                <div class="UIMediaItem_video">
                                  <a class="video_extra_anchor">
                                    <xsl:attribute name="title">
                                      <xsl:value-of select="localized:GetVideoHoverText()"/>
                                    </xsl:attribute>
                                    <xsl:attribute name="href">
                                      <xsl:value-of select="fb:attachment/fb:href"/>
                                    </xsl:attribute>
                                    <div class="video_thumb">
                                      <span class="play">
                                        <img style="height: 120px; width: 160px;">
                                          <xsl:attribute name="src">
                                            <xsl:value-of select="fb:attachment/fb:media/fb:stream_media/fb:src"/>
                                          </xsl:attribute>
                                        </img>
                                      </span>
                                    </div>
                                  </a>                                                                       
                                </div>                                
                              </xsl:when>
                              <xsl:when test="fb:attachment/fb:media/fb:stream_media/fb:src">
                                <a>
                                  <xsl:attribute name="href">
                                    <xsl:value-of select="fb:attachment/fb:media/fb:stream_media/fb:href"/>
                                  </xsl:attribute>
                                  <div class="UIMediaItem_Wrapper">
                                    <img>
                                      <xsl:attribute name="src">
                                        <xsl:value-of select="fb:attachment/fb:media/fb:stream_media/fb:src"/>
                                      </xsl:attribute>
                                    </img>
                                  </div>
                                </a>
                              </xsl:when>                             
                            </xsl:choose>
                          </div>                          
                        </div>
                        <div class="UIStoryAttachment_Title">
                          <a>
                            <xsl:attribute name="href">
                              <xsl:value-of select="fb:attachment/fb:href"/>
                            </xsl:attribute>
                            <xsl:value-of select="fb:attachment/fb:name"/>
                          </a>
                        </div>
                        <div class="UIStoryAttachment_Copy">
                          <xsl:value-of select="fb:attachment/fb:description"/>
                        </div>
                        <div class="UIStoryAttachment_Table">
                          <xsl:for-each select="fb:attachment/fb:properties/fb:stream_property">
                            <div>
                              <div class="UIStoryAttachment_Label">
                                <xsl:value-of select="current()/fb:name"/>
                              </div>
                              <div class="UIStoryAttachment_Value">
                                <xsl:value-of select="current()/fb:text"/>
                              </div>
                            </div>
                          </xsl:for-each>
                        </div>
                      </div>
                    </xsl:if>
                    <div class="UIIntentionalStory_Info UIIntentionalStory_AttachmentInfo">
                      <div class="UIIntentionalStory_InfoText UIIntentionalStory_InfoTextIndented">
                        <span class="UIIntentionalStory_Time">
                          <a>
                            <xsl:attribute name="href">
                              <xsl:value-of select="fb:permalink"/>
                            </xsl:attribute>
                            <xsl:value-of select="bndt:GetRelativeTime(number(fb:created_time))"/>
                          </a>
                        </span>
                      </div>
                    </div>
                  </div>                 
                </div>
              </div>
            </content>
            <xsl:if test="fb:comments/fb:can_post = 1">
              <fb:can-comment>true</fb:can-comment>
            </xsl:if>
            <xsl:if test="fb:comments/fb:count &gt; 0" >
              <xsl:copy-of select="fb:comments" />
            </xsl:if>
            <xsl:if test="fb:likes/fb:count &gt; 0" >
              <xsl:copy-of select="fb:likes" />
            </xsl:if>
          </entry>          
        </xsl:for-each>
        
      </feed>
    </xsl:template>

  <xsl:template match="fb:profiles | fb:albums" ></xsl:template>
   
</xsl:stylesheet>
