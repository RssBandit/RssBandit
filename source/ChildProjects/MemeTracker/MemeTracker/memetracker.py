import time, sys, re, System, System.IO
from System import *
from System.IO import *
import clr
clr.AddReference("System.Xml")
from System.Xml import *

cache_location = r"C:\Documents and Settings\dareo\Local Settings\Application Data\RssBandit\Cache"
href_regex     = r"<a\s+([^>]*\s*)?href\s*=\s*(?:""(?<1>[/\a-z0-9_][^""]*)""|'(?<1>[/\a-z0-9_][^']*)'|(?<1>[/\a-z0-9_]\S*))(\s[^>]*)?>(?<2>.*?)</a>"
all_links = {}
(popular_in_unread, popular_in_past_week) = range(2)
mode = popular_in_past_week

class RssItem:
    """Represents an RSS item"""
    def __init__(self, permalink, title, date, read_status, outgoing_links):
        self.outgoing_links = outgoing_links
        self.permalink      = permalink
        self.title          = title
        self.date           = date
        self.read_status    = read_status

def MakeRssItem(xmlnode):
    pass 
    

if __name__ == "__main__":
    if len(sys.argv) > 1: #get directory of RSS feeds
        cache_location = sys.argv[1]
    if len(sys.argv) > 2: # mode = 0 means use only unread items, mode = 1 means use all items from past week 
        mode           = int(argv[2]) and popular_in_past_week or popular_in_unread

    print "Processing items from %s seeking items that are %s" % (cache_location,
                                                                  mode and "popular in unread items"
                                                                  or  "popular in items from the past week")

    di = DirectoryInfo(cache_location)
    for fi in di.GetFiles("*.xml"):      
        doc = XmlDocument()
        doc.Load(Path.Combine(cache_location, fi.Name))
        # for each item in feed
        #  1. Get permalink & title
        #  2. Get outgoing links & link titles
        #  3. Get read status and date
        #  4. Get feed name
        # print "%s has %s <item> nodes" % (fi.Name, doc.SelectNodes("//item").Count)
    
