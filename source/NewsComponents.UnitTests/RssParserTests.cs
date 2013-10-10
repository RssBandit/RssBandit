using System;
using NewsComponents.Feed;
using NewsComponents.Utils;
using NUnit.Framework;

namespace NewsComponents.UnitTests
{
	[TestFixture]
	public class RssParserTests
	{
		const string TEST_BASE_URL = "http://127.0.0.1:8081/RssParserTests/";

		[Test]
		public void GetItemsForFeedWithRssRdfv091()
		{
			using (var stream = Resources.Resource.GetResourceAsStream("TestFeeds.rss_rdf_0_91.xml"))
			{
				NewsFeed f = new NewsFeed();
				f.link = TEST_BASE_URL + "rss_rdf_0_91.xml";
				var feedInfo = RssParser.GetItemsForFeed(f, stream, false, true);
				Assert.IsNotNull(feedInfo);
				Assert.IsNotNull(feedInfo.ItemsList);
				Assert.AreEqual(40, feedInfo.ItemsList.Count);
				Assert.AreEqual(new DateTime(2013, 9, 12, 12, 0, 0, DateTimeKind.Utc), feedInfo.ItemsList[39].Date);

				string requestUrl = null;
				var xElem = RssHelper.GetOptionalElement(feedInfo.OptionalElements, "image");
				if (xElem != null)
				{
					var urlNode = xElem.SelectSingleNode("url");
					if (urlNode != null)
						requestUrl = urlNode.InnerText;
				}

				Assert.AreEqual("http://www.golem.de/staticrl/images/golem-rss.png", requestUrl);
			}
		}

		[Test]
		public void GetItemsForFeedReturnsCorrectTitle()
		{
			using (var stream = Resources.Resource.GetResourceAsStream("TestFeeds.FeedWithWeirdLineBreaks.xml"))
			{
				NewsFeed f = new NewsFeed();
				f.link = TEST_BASE_URL + "FeedWithWeirdLineBreaks.xml";
				var feedInfo = RssParser.GetItemsForFeed(f, stream, false, true);
				Assert.IsNotNull(feedInfo);

				var feedItems = feedInfo.ItemsList;
				Assert.IsNotNull(feedItems, "null is not expected");
				Assert.AreEqual(2, feedItems.Count, "2 items expected");
				var item = feedItems[0];
				var item2 = feedItems[1];
				Assert.AreEqual("This is item 1 of a really cool(...)", item.Title, "The title was not what we expected.");
				Assert.AreEqual("Testing a second item", item2.Title);
			}
		}

		[Test]
		public void GetItemsForFeedWithRssv20NoDates()
		{
			using (var stream = Resources.Resource.GetResourceAsStream("TestFeeds.rss_2_0_no_dates.xml"))
			{
				// ensure the self-assigned item dates are all older than this:
				DateTime utcNow = DateTime.UtcNow + TimeSpan.FromMinutes(1);
				NewsFeed f = new NewsFeed();
				f.link = TEST_BASE_URL + "rss_2_0_no_dates.xml";
				var feedInfo = RssParser.GetItemsForFeed(f, stream, false, true);
				Assert.IsNotNull(feedInfo);
				Assert.IsNotNull(feedInfo.ItemsList);
				Assert.AreEqual(10, feedInfo.ItemsList.Count);

				INewsItem recent = null;
				foreach (var item in feedInfo.ItemsList)
				{
					// items ordered newest first:
					Assert.IsTrue(item.Date < utcNow);
					if (recent != null)
						Assert.IsTrue(item.Date < recent.Date);
					recent = item;
				}

				string optElement = null;
				var xElem = RssHelper.GetOptionalElement(feedInfo.OptionalElements, "ttl");
				if (xElem != null)
					optElement = xElem.InnerText;
				Assert.AreEqual("60", optElement);

				optElement = null;
				xElem = RssHelper.GetOptionalElement(feedInfo.OptionalElements, "managingEditor");
				if (xElem != null)
					optElement = xElem.InnerText;
				Assert.AreEqual("scot_petersen@ziffdavis.com", optElement);

				optElement = null;
				xElem = RssHelper.GetOptionalElement(feedInfo.OptionalElements, "copyright");
				if (xElem != null)
					optElement = xElem.InnerText;
				Assert.AreEqual("Copyright 2008 Ziff Davis Media Inc. All Rights Reserved.", optElement);
			}
		}
	}

	

    
}
