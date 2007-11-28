import time, sys, re, System, System.IO, System.Globalization
from System import *
from System.IO import *
from System.Globalization import DateTimeStyles
import clr
clr.AddReference("System.Xml")
from System.Xml import *

#################################################################
#
# USAGE: ipy memetracker.py <directory-of-rss-feeds> <mode> 
# mode = 0 show most popular links in unread items
# mode = 1 show most popular links from items from the past week
#################################################################

all_links = {}
one_week =  TimeSpan(7,0,0,0)

cache_location = r"C:\Documents and Settings\dareo\Local Settings\Application Data\RssBandit\Cache"
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
    # obtain href value and link text pairs
    outgoing   = desc_node and regex.findall(desc_node.InnerText) or []
    outgoing_links = {}
    #ensure we only collect unique href values from entry by replacing list returned by regex with dictionary
    if len(outgoing) > 0:
        for url, linktext in outgoing:
            outgoing_links[url] = linktext
    return RssItem(permalink, title, date, read, outgoing_links)    
    

if __name__ == "__main__":
    if len(sys.argv) > 1: #get directory of RSS feeds
        cache_location = sys.argv[1]
    if len(sys.argv) > 2: # mode = 0 means use only unread items, mode = 1 means use all items from past week 
        mode           = int(argv[2]) and popular_in_past_week or popular_in_unread

    print "Processing items from %s seeking items that are %s" % (cache_location,
                                                                  mode and "popular in items from the past week"
                                                                  or "popular in unread items" )
    #decide what filter function to use depending on mode
    filterFunc = mode and (lambda x : (DateTime.Now - x.date) < one_week) or (lambda x : x.read == 0)
    #in mode = 0 each entry linking to an item counts as a vote, in mode = 1 value of vote depends on item age
    voteFunc   = mode and (lambda x: 1.0 - (DateTime.Now.Ticks - x.date.Ticks) * 1.0 / one_week.Ticks) or (lambda x: 1.0)

    di = DirectoryInfo(cache_location)
    for fi in di.GetFiles("*.xml"):      
        doc = XmlDocument()
        doc.Load(Path.Combine(cache_location, fi.Name))
        # for each item in feed        
        #  1. Get permalink, title, read status and date
        #  2. Get list of outgoing links + link title pairs
        #  3. Convert above to RssItem object
        items = [ MakeRssItem(node) for node in doc.SelectNodes("//item")]
        # print "Processing %d items from %s" % (len(items), fi.Name)
        feedTitle = doc.SelectSingleNode("/rss/channel/title").InnerText

        #BUGBUG: We should canonicalize URLs
        # apply filter to pick candidate items, then calculate vote for each outgoing url
        for item in filter(filterFunc, items):
            vote = [voteFunc(item), item, feedTitle]
            #add a vote for the permalink by the author
            if all_links.get(item.permalink) == None:
                 all_links[item.permalink] = []
            all_links.get(item.permalink).append(vote)
            #add a vote for each of the URLs
            for url in item.outgoing_links.Keys:
                if all_links.get(url) == None:
                    all_links[url] = []
                all_links.get(url).append(vote)

       # tally the votes, only 1 vote counts per feed
    weighted_links = []
    for link, votes in all_links.items():
        site = {}
        for weight, item, feedTitle in votes:                
            site[feedTitle] = min(site.get(feedTitle,1), weight) 
        weighted_links.append((sum(site.values()), link))
    weighted_links.sort()
    weighted_links.reverse()

    # output the results, choose link text from first item we saw story linked from
    print "<ol>"    
    #BUGBUG: We should use <title> from the HTML page
    for weight, link in weighted_links[:10]:
        link_text = (all_links.get(link)[0])[1].outgoing_links.get(link)
        print "<li><a href='%s'>%s</a> (%s)" % (link, link_text, weight)
        print "<p>Seen on:"
        print "<ul>"
        for weight, item, feedTitle in all_links.get(link):
            print "<li>%s: <a href='%s'>%s</a></li>" % (feedTitle, item.permalink, item.title)
        print "</ul></p></li>"  
    print "</ol>"
                
