import time, sys, re, System, System.IO, System.Globalization
from System import *
from System.IO import *
from System.Globalization import DateTimeStyles
import clr
clr.AddReference("System.Xml")
from System.Xml import *


one_week =  TimeSpan(7,0,0,0)

cache_location = r"C:\Documents and Settings\dareo\Application Data\RssBandit\Cache.Old"
href_regex     = r"<a[\s]+[^>]*?href[\s]?=[\s\"\']+(.*?)[\"\']+.*?>([^<]+|.*?)?<\/a>"
regex          = re.compile(href_regex)

(popular_in_unread, popular_in_past_week) = range(2)
mode = popular_in_past_week


class RssItem:
    """Represents an RSS item"""
    def __init__(self, permalink, title, date, read, outgoing_links):
        self.outgoing_links = outgoing_links
        self.permalink      = permalink
        self.title          = title
        self.date           = date
        self.read           = read

def MakeRssItem(itemnode):
    link_node  = itemnode.SelectSingleNode("link")
    permalink  = link_node and link_node.InnerText or ''
    title_node = itemnode.SelectSingleNode("title")
    title      = link_node and title_node.InnerText or ''
    date_node  = itemnode.SelectSingleNode("pubDate")
    date       = date_node and DateTime.Parse(date_node.InnerText, None, DateTimeStyles.AdjustToUniversal) or DateTime.Now  
    read_node  = itemnode.SelectSingleNode("//@*[local-name() = 'read']")
    read       = read_node and int(read_node.Value) or 0
    desc_node  = itemnode.SelectSingleNode("description")
    outgoing_links = desc_node and regex.findall(desc_node.InnerText) or []    
    return RssItem(permalink, title, date, read, outgoing_links)
    pass 
    

if __name__ == "__main__":
    if len(sys.argv) > 1: #get directory of RSS feeds
        cache_location = sys.argv[1]
    if len(sys.argv) > 2: # mode = 0 means use only unread items, mode = 1 means use all items from past week 
        mode           = int(argv[2]) and popular_in_past_week or popular_in_unread

    print "Processing items from %s seeking items that are %s" % (cache_location,
                                                                  mode and "popular in items from the past week"
                                                                  or "popular in unread items" )
    #decide what filter function to use depending on mode
    filterFunc = mode and (lambda x : x.read == 0) or (lambda x : (DateTime.Now - x.date) < one_week)

    di = DirectoryInfo(cache_location)
    for fi in di.GetFiles("*.xml"):      
        doc = XmlDocument()
        doc.Load(Path.Combine(cache_location, fi.Name))
        # for each item in feed
        #  1. Get permalink & title
        #  2. Get outgoing links & link titles
        #  3. Get read status and date
        #  4. Get feed name
        items = [ MakeRssItem(node) for node in doc.SelectNodes("//item")]
        for i in items:
            print "%s has the following outgoing links: %s" % (i.permalink, i.outgoing_links)
#        print "%s has %s <item> nodes" % (fi.Name, doc.SelectNodes("//item").Count)
    
