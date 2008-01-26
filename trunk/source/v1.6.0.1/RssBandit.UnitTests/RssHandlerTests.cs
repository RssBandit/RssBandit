using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Security.Permissions;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Storage;
using NUnit.Framework;

namespace RssBandit.UnitTests
{
	/// <summary>
	/// NUnit Test Fixture containing tests of the RssParser class.
	/// </summary>
	[TestFixture]
	public class RssParserTests : BaseTestFixture
	{
		const string CACHE_DIR = UNPACK_DESTINATION + @"\Cache";
		const string FEEDS_DIR = UNPACK_DESTINATION;
		const string APP_NAME = "RssBanditUnitTests";
		const string BASE_URL = "http://127.0.0.1:8081/RssParserTestFiles/";

		/// <summary>
		/// Tests loading a feed list.
		/// </summary>
		[Test]
		public void TestLoadFeeds()
		{
			RssParser handler = new RssParser(APP_NAME);
			handler.LoadFeedlist(BASE_URL + "FeedList03Feeds.xml", null);
			Assertion.Assert("Feeds should be valid!", handler.FeedsListOK);
			Assertion.AssertEquals("FeedList03Feeds.xml should contain 3 feeds. Hence the name.", 3, handler.FeedsTable.Count);
			
			//Assert the titles of the three feeds.
			Assertion.AssertEquals("MSDN: Visual C#", handler.FeedsTable["http://msdn.microsoft.com/vcsharp/rss.xml"].title);
			Assertion.AssertEquals("ASP.NET Forums: Architecture", handler.FeedsTable["http://www.asp.net/Forums/rss.aspx?forumid=16"].title);
			Assertion.AssertEquals("Slashdot", handler.FeedsTable["http://slashdot.org/slashdot.rss"].title);
		}

		/// <summary>
		/// Tests loading a non-existent feed list.  
		/// This should throw a 404 exception
		/// </summary>
		[Test, ExpectedException(typeof(WebException))]
		public void TestLoadNonExistentFeed()
		{
			RssParser handler = new RssParser(APP_NAME);
			handler.LoadFeedlist(BASE_URL + "ThisFeedDoesNotExist.xml", null);
		}

		/// <summary>
		/// This tests that importing the smallest feed that caused an 
		/// error similar to "The key sequence 'Blogs' in Keyref fails to refer to 
		/// some key. An error occurred at , (1, 226)." no longer causes a problem.
		/// </summary>
		[Test]
		public void TestImportFeedThatCausedKeyrefError()
		{
			RssParser handler = new RssParser(APP_NAME);
			using(Stream feedStream = base.GetResourceStream("WebRoot.RssParserTestFiles.KeyRefBugTest.xml"))
			{
				handler.ImportFeedlist(feedStream);
				feedStream.Close();
			}
		}

		/// <summary>
		/// Tests the ImportFeedlist method by loading a feed list and then 
		/// importing another list.
		/// </summary>
		[Test]
		public void TestImportFeedlist()
		{
			RssParser handler = new RssParser(APP_NAME);
			handler.LoadFeedlist(BASE_URL + "FeedList03Feeds.xml", null);
			Assertion.Assert("Feeds should be valid!", handler.FeedsListOK);
			Assertion.AssertEquals("FeedList03Feeds.xml should contain 3 feeds. Hence the name.", 3, handler.FeedsTable.Count);

			//Now import a feed list.
			using(FileStream stream = File.OpenRead(Path.Combine(WEBROOT_PATH, @"RssParserTestFiles\FeedList04Feeds.xml")))
			{
				handler.ImportFeedlist(stream);
				stream.Close();
			}
			Assertion.AssertEquals("3 + 4 = 7.  7 Feeds expected.", 7, handler.FeedsTable.Count);
	
			Assertion.AssertEquals("MSDN: Visual C#", handler.FeedsTable["http://msdn.microsoft.com/vcsharp/rss.xml"].title);
			Assertion.AssertEquals("Development", handler.FeedsTable["http://msdn.microsoft.com/vcsharp/rss.xml"].category);
			Assertion.AssertEquals("ASP.NET Forums: Architecture", handler.FeedsTable["http://www.asp.net/Forums/rss.aspx?forumid=16"].title);
			Assertion.AssertEquals("Forums", handler.FeedsTable["http://www.asp.net/Forums/rss.aspx?forumid=16"].category);
			Assertion.AssertEquals("Slashdot", handler.FeedsTable["http://slashdot.org/slashdot.rss"].title);
			Assertion.AssertEquals("News Technology", handler.FeedsTable["http://slashdot.org/slashdot.rss"].category);
			Assertion.AssertEquals("Torsten's .NET Blog", handler.FeedsTable["http://www.rendelmann.info/blog/SyndicationService.asmx/GetRss"].title);
			Assertion.AssertEquals("Blogs", handler.FeedsTable["http://www.rendelmann.info/blog/SyndicationService.asmx/GetRss"].category);
			Assertion.AssertEquals("you've been HAACKED", handler.FeedsTable["http://haacked.com/Rss.aspx"].title);
			Assertion.AssertEquals("Blogs", handler.FeedsTable["http://haacked.com/Rss.aspx"].category);
			Assertion.AssertEquals("kuro5hin.org", handler.FeedsTable["http://www.kuro5hin.org/backend.rdf"].title);
			Assertion.AssertEquals("News Technology", handler.FeedsTable["http://www.kuro5hin.org/backend.rdf"].category);
			Assertion.AssertEquals("Dare Obasanjo aka Carnage4Life", handler.FeedsTable["http://www.25hoursaday.com/weblog/SyndicationService.asmx/GetRss"].title);
			Assertion.AssertEquals("Blogs Microsoft", handler.FeedsTable["http://www.25hoursaday.com/weblog/SyndicationService.asmx/GetRss"].category);
		}

		/// <summary>
		/// Tests the ImportFeedlist method by loading a feed list (FeedList03Feeds.xml) 
		/// and then importing a category from another list (FeedList04Feeds.xml).
		/// </summary>
		[Test]
		public void TestImportFeedlistCategory()
		{
			RssParser handler = new RssParser(APP_NAME);
			handler.LoadFeedlist(BASE_URL + "FeedList03Feeds.xml", null);
			Assertion.Assert("Feeds should be valid!", handler.FeedsListOK);
			Assertion.AssertEquals("FeedList03Feeds.xml should contain 3 feeds. Hence the name.", 3, handler.FeedsTable.Count);

			//Now import a feed list.
			using(FileStream stream = File.OpenRead(Path.Combine(WEBROOT_PATH, @"RssParserTestFiles\FeedList04Feeds.xml")))
			{
				handler.ImportFeedlist(stream, "News Technology");
				stream.Close();
			}
			Assertion.AssertEquals("3 + 4 = 7.  7 Feeds expected.", 7, handler.FeedsTable.Count);
	
			Assertion.AssertEquals("MSDN: Visual C#", handler.FeedsTable["http://msdn.microsoft.com/vcsharp/rss.xml"].title);
			Assertion.AssertEquals("Development", handler.FeedsTable["http://msdn.microsoft.com/vcsharp/rss.xml"].category);
			Assertion.AssertEquals("ASP.NET Forums: Architecture", handler.FeedsTable["http://www.asp.net/Forums/rss.aspx?forumid=16"].title);
			Assertion.AssertEquals("Forums", handler.FeedsTable["http://www.asp.net/Forums/rss.aspx?forumid=16"].category);
			Assertion.AssertEquals("Slashdot", handler.FeedsTable["http://slashdot.org/slashdot.rss"].title);
			Assertion.AssertEquals("News Technology", handler.FeedsTable["http://slashdot.org/slashdot.rss"].category);
			Assertion.AssertEquals("Torsten's .NET Blog", handler.FeedsTable["http://www.rendelmann.info/blog/SyndicationService.asmx/GetRss"].title);
			Assertion.AssertEquals(@"News Technology\Blogs", handler.FeedsTable["http://www.rendelmann.info/blog/SyndicationService.asmx/GetRss"].title);
			Assertion.AssertEquals("you've been HAACKED", handler.FeedsTable["http://haacked.com/Rss.aspx"].title);
			Assertion.AssertEquals(@"News Technology\Blogs", handler.FeedsTable["http://haacked.com/Rss.aspx"].category);
			Assertion.AssertEquals("kuro5hin.org", handler.FeedsTable["http://www.kuro5hin.org/backend.rdf"].title);
			Assertion.AssertEquals(@"News Technology\News Technology", handler.FeedsTable["http://www.kuro5hin.org/backend.rdf"].category);
			Assertion.AssertEquals("Dare Obasanjo aka Carnage4Life", handler.FeedsTable["http://www.25hoursaday.com/weblog/SyndicationService.asmx/GetRss"].title);
			Assertion.AssertEquals(@"News Technology\Blogs Microsoft", handler.FeedsTable["http://www.25hoursaday.com/weblog/SyndicationService.asmx/GetRss"].category);
		}

		/// <summary>
		/// Makes sure that when importing a feed list that contains a 
		/// feed that already exists, we do not add it.
		/// </summary>
		[Test]
		public void TestImportFeedListWithDuplicate()
		{
			//Start by loading 3 feeds.
			RssParser handler = new RssParser(APP_NAME);
			handler.LoadFeedlist(BASE_URL + "FeedList03Feeds.xml", null);
			Assertion.Assert("Feeds should be valid!", handler.FeedsListOK);
			Assertion.AssertEquals("FeedList03Feeds.xml should contain 3 feeds. Hence the name.", 3, handler.FeedsTable.Count);

			// Now import a feed list that contains 1 new feed 
			// and one that already exists. 
			using(FileStream stream = File.OpenRead(Path.Combine(WEBROOT_PATH, @"RssParserTestFiles\FeedListWithDuplicateFrom03.xml")))
			{
				handler.ImportFeedlist(stream);
				stream.Close();
			}

			Assertion.AssertEquals("3 + 1 = 4.  4 Feeds expected because one is a duplicate!", 4, handler.FeedsTable.Count);

			Assertion.AssertEquals("MSDN: Visual C#", handler.FeedsTable["http://msdn.microsoft.com/vcsharp/rss.xml"].title);
			Assertion.AssertEquals("Development", handler.FeedsTable["http://msdn.microsoft.com/vcsharp/rss.xml"].category);
			Assertion.AssertEquals("ASP.NET Forums: Architecture", handler.FeedsTable["http://www.asp.net/Forums/rss.aspx?forumid=16"].title);
			Assertion.AssertEquals("Forums", handler.FeedsTable["http://www.asp.net/Forums/rss.aspx?forumid=16"].category);
			Assertion.AssertEquals("Slashdot", handler.FeedsTable["http://slashdot.org/slashdot.rss"].title);
			Assertion.AssertEquals("News Technology", handler.FeedsTable["http://slashdot.org/slashdot.rss"].category);
			Assertion.AssertEquals("Channel 9", handler.FeedsTable["http://channel9.msdn.com/rss.aspx"].title);
			Assertion.AssertEquals("Microsoft", handler.FeedsTable["http://channel9.msdn.com/rss.aspx"].category);
		}

		/// <summary>
		/// Runs some tests on marking items as recently viewed and 
		/// then making sure they appear in the Recently Viewed count.
		/// </summary>
		[Test, FileIOPermission(SecurityAction.Demand)]
		public void TestRecentlyViewed()
		{
			//Load feed list.
			CacheManager cache = new FileCacheManager(CACHE_DIR, TimeSpan.MaxValue);
			RssParser handler = new RssParser(APP_NAME, cache);
			handler.LoadFeedlist(BASE_URL + "LocalTestFeedList.xml", null);
			Assertion.Assert("Feeds should be valid!", handler.FeedsListOK);

			//Grab a feed.
			feedsFeed feed = handler.FeedsTable["http://localhost/RssHandlerTestFiles/LocalTestFeed.xml"];
			
			//TODO: FeedInfo feedInfo = handler.GetFeedInfo(feed.link);
			//Assertion.AssertNotNull(feedInfo);
			//cache.SaveFeed(feedInfo);
			
			Assertion.AssertEquals("Rss Bandit Unit Test Feed", feed.title);
			Assertion.AssertEquals(1, feed.storiesrecentlyviewed.Count);
			
			//Grab feed items
			ArrayList items = handler.GetItemsForFeed(feed);
			Assertion.AssertEquals(2, items.Count);
			
			//The first one should have been read. the second not.
			NewsItem item = (NewsItem)items[0];
			Assertion.Assert(item.BeenRead);
			item = (NewsItem)items[1];
			Assertion.Assert(!item.BeenRead);

			//So let's read the second item.
			handler.MarkAllCachedItemsAsRead(feed);
			handler.ApplyFeedModifications(feed.link);

			//Let's save this guy.
			using(FileStream newFeedStream = File.OpenWrite(Path.Combine(WEBROOT_PATH, @"RssParserTestFiles\LocalTestFeedList_NEW.xml")))
			{
				//TODO: Ask Dare about RssParser.SaveFeedList()
				//handler.SaveFeedList(newFeedStream, RssFeedListFormat.RssParser);
				newFeedStream.Close();
				Assertion.Assert(File.Exists(Path.Combine(WEBROOT_PATH, @"RssParserTestFiles\LocalTestFeedList_NEW.xml")));
			}

			//Let's reload and see what happens.
			handler = new RssParser(APP_NAME, cache);
			handler.LoadFeedlist(BASE_URL + "LocalTestFeedList_NEW.xml", null);
			feed = handler.FeedsTable["http://localhost/RssHandlerTestFiles/LocalTestFeed.xml"];
			Assertion.AssertEquals("Should be two now.", 2, feed.storiesrecentlyviewed.Count);
		}

		/// <summary>
		/// Runs a test of the comment posting function.
		/// </summary>
		[Test]
		public void TestPostCommentViaCommentAPI()
		{
			/*
			RssParser handler = new RssParser(APP_NAME);
			handler.LoadFeedlist(BASE_URL + "FeedList03Feeds.xml", null);
			handler.PostCommentViaCommentAPI("", null);
			*/
		}

		[SetUp]
		public void SetUp()
		{
			DeleteDirectory(UNPACK_DESTINATION);
			UnpackResourceDirectory("WebRoot.RssParserTestFiles");
			UnpackResourceDirectory("Cache");
			UnpackResourceDirectory("Expected");
			UnpackResourceDirectory("Settings");
			
			StartWebServer();
		}
		
		[TearDown]
		public void TearDown()
		{
			StopWebServer();
			
			DeleteDirectory(UNPACK_DESTINATION);
		}
	}
}