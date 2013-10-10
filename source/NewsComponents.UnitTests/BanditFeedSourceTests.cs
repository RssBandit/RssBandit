using System;
using System.IO;
using System.Linq;
using NewsComponents.Feed;
using NUnit.Framework;
using RssBandit.UnitTests;

namespace NewsComponents.UnitTests
{
	/// <summary>
    /// Tests BanditFeedSource
	/// </summary>
	[TestFixture]
	public class BanditFeedSourceTests : CassiniHelperTestFixture
	{
		internal const string BASE_URL = "http://127.0.0.1:8081/NewsHandlerTestFiles/";
		readonly string _cacheDirectory = Path.Combine(Path.GetTempPath(), "RssBanditUnitTestCache");

        /// <summary>
        /// Tests loading a non-existent feed list.  
        /// This should throw a 404 exception
        /// </summary>
        [Test, ExpectedException(typeof(System.Net.WebException))]
        public void LoadNonExistentEnsureExceptionThrown()
        {
            var handler = new BanditFeedSource(ConfigurationWithoutSearchIndexer, new SubscriptionLocation(BASE_URL + "ThisFeedDoesNotExist.xml"));
            handler.LoadFeedlist(); 
        }

        /// <summary>
        /// Loads a feedlist containing three feeds and makes 
        /// sure they look good (title).
        /// </summary>
        [Test]
        public void LoadFeedListWithThreeFeeds()
        {
            var handler = CreateNewsHandlerWithFeedList();

            Assert.IsTrue(handler.IsSubscribed("http://msdn.microsoft.com/vcsharp/rss.xml"));
            Assert.IsTrue(handler.IsSubscribed("http://www.asp.net/Forums/rss.aspx?forumid=16"));
            Assert.IsTrue(handler.IsSubscribed("http://slashdot.org/slashdot.rss"));
        }

        /// <summary>
        /// Loads a feedlist containing three feeds and makes 
        /// sure they look good (title).
        /// </summary>
        [Test]
        public void LoadFeedListWithThreeFeedsCheckTitle()
        {
            var handler = CreateNewsHandlerWithFeedList();

            AssertFeedTitle("MSDN: Visual C#", "http://msdn.microsoft.com/vcsharp/rss.xml", handler);
            AssertFeedTitle("ASP.NET Forums: Architecture", "http://www.asp.net/Forums/rss.aspx?forumid=16", handler);
            AssertFeedTitle("Slashdot", "http://slashdot.org/slashdot.rss", handler);
        }

	    private INewsComponentsConfiguration ConfigurationWithoutSearchIndexer
	    {
	        get {
                var cfg = NewsComponentsConfiguration.Default as NewsComponentsConfiguration;
                cfg.SearchIndexBehavior = SearchIndexBehavior.NoIndexing;
	            return cfg;
	        }
	    }

        private INewsComponentsConfiguration ConfigurationWithoutSearchIndexerAndUnitTestCache
        {
            get
            {
                var cfg = NewsComponentsConfiguration.Default as NewsComponentsConfiguration;
                cfg.SearchIndexBehavior = SearchIndexBehavior.NoIndexing;
                cfg.UserLocalApplicationDataPath = _cacheDirectory;
                return cfg;
            }
        }

        private void AssertFeedTitle(string expectedTitle, string feedUrl, FeedSource handler)
        {
            Assert.AreEqual(expectedTitle, handler.GetFeeds()[feedUrl].title);
        }

        private void AssertFeedCategory(string expectedCategory, string feedUrl, FeedSource handler)
        {
            Assert.AreEqual(expectedCategory, handler.GetFeeds()[feedUrl].category);
        }

        private FeedSource CreateNewsHandlerWithFeedList()
        {
            var handler = new BanditFeedSource(ConfigurationWithoutSearchIndexer, new SubscriptionLocation(WEBROOT_PATH + @"\NewsHandlerTestFiles\FeedList03Feeds.xml"));
            handler.LoadFeedlist(); 

            //handler.LoadFeedlist(new FileStream(WEBROOT_PATH + @"\NewsHandlerTestFiles\FeedList03Feeds.xml", FileMode.Open), null);
            Assert.IsTrue(handler.FeedsListOK, "Feeds should be valid!");
            //Assert.AreEqual(3, handler.Feeds.Count, "FeedList03Feeds.xml should contain 3 feeds. Hence the name");

            return handler;
        }

        /// <summary>
        /// Tests the ImportFeedlist method by loading a feed list and then 
        /// importing another list.
        /// </summary>
        [Test]
        public void ImportTwoFeedlists()
        {
            var handler = CreateNewsHandlerWithFeedList();

            //Now import a feed list.
            using (FileStream stream = File.OpenRead(Path.Combine(WEBROOT_PATH, @"NewsHandlerTestFiles\FeedList04Feeds.xml")))
            {
                handler.ImportFeedlist(stream);
                stream.Close();
            }
			Assert.AreEqual(7, handler.GetFeeds().Count, "3 + 4 = 7.  7 Feeds expected.");
			
            AssertFeedTitle("MSDN: Visual C#", "http://msdn.microsoft.com/vcsharp/rss.xml", handler);
            AssertFeedCategory("Development", "http://msdn.microsoft.com/vcsharp/rss.xml", handler);
            AssertFeedTitle("ASP.NET Forums: Architecture", "http://www.asp.net/Forums/rss.aspx?forumid=16", handler);
            AssertFeedCategory("Forums", "http://www.asp.net/Forums/rss.aspx?forumid=16", handler);
            AssertFeedTitle("Slashdot", "http://slashdot.org/slashdot.rss", handler);
            AssertFeedCategory("News Technology", "http://slashdot.org/slashdot.rss", handler);
            AssertFeedTitle("Torsten's .NET Blog", "http://www.rendelmann.info/blog/SyndicationService.asmx/GetRss", handler);
            AssertFeedCategory("Blogs", "http://www.rendelmann.info/blog/SyndicationService.asmx/GetRss", handler);
            AssertFeedTitle("you've been HAACKED", "http://haacked.com/Rss.aspx", handler);
            AssertFeedCategory("Blogs", "http://haacked.com/Rss.aspx", handler);
            AssertFeedTitle("kuro5hin.org", "http://www.kuro5hin.org/backend.rdf", handler);
            AssertFeedCategory("News Technology", "http://www.kuro5hin.org/backend.rdf", handler);
            AssertFeedTitle("Dare Obasanjo aka Carnage4Life", "http://www.25hoursaday.com/weblog/SyndicationService.asmx/GetRss", handler);
            AssertFeedCategory("Blogs Microsoft", "http://www.25hoursaday.com/weblog/SyndicationService.asmx/GetRss", handler);
        }

        /// <summary>
        /// Tests the ImportFeedlist method by loading a feed list and then 
        /// importing a category within another list.
        /// </summary>
        [Test]
        public void ImportFeedListCategoryItemsAreImported()
        {
            var handler = CreateNewsHandlerWithFeedList();

            //Now import a feed list.
            using (FileStream stream = File.OpenRead(Path.Combine(WEBROOT_PATH, @"NewsHandlerTestFiles\FeedList04Feeds.xml")))
            {
                handler.ImportFeedlist(stream, "News Technology");
                stream.Close();
            }
			Assert.AreEqual(7, handler.GetFeeds().Count, "3 + 4 = 7.  7 Feeds expected.");

            AssertFeedTitle("MSDN: Visual C#", "http://msdn.microsoft.com/vcsharp/rss.xml", handler);
            AssertFeedCategory("Development", "http://msdn.microsoft.com/vcsharp/rss.xml", handler);
            AssertFeedTitle("ASP.NET Forums: Architecture", "http://www.asp.net/Forums/rss.aspx?forumid=16", handler);
            AssertFeedCategory("Forums", "http://www.asp.net/Forums/rss.aspx?forumid=16", handler);
            AssertFeedTitle("Slashdot", "http://slashdot.org/slashdot.rss", handler);
            AssertFeedCategory("News Technology", "http://slashdot.org/slashdot.rss", handler);
            AssertFeedTitle("Torsten's .NET Blog", "http://www.rendelmann.info/blog/SyndicationService.asmx/GetRss", handler);
            AssertFeedCategory(@"News Technology\Blogs", "http://www.rendelmann.info/blog/SyndicationService.asmx/GetRss", handler);
            AssertFeedTitle("you've been HAACKED", "http://haacked.com/Rss.aspx", handler);
            AssertFeedCategory(@"News Technology\Blogs", "http://haacked.com/Rss.aspx", handler);
            AssertFeedTitle("kuro5hin.org", "http://www.kuro5hin.org/backend.rdf", handler);
            AssertFeedCategory(@"News Technology\News Technology", "http://www.kuro5hin.org/backend.rdf", handler);
            AssertFeedTitle("Dare Obasanjo aka Carnage4Life", "http://www.25hoursaday.com/weblog/SyndicationService.asmx/GetRss", handler);
            AssertFeedCategory(@"News Technology\Blogs Microsoft", "http://www.25hoursaday.com/weblog/SyndicationService.asmx/GetRss", handler);
        }

        /// <summary>
        /// Tests that ApplyFeedModifications doesn't accept null argument.
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ApplyFeedModificationsThrowsArgumentNullException()
        {
            var handler = CreateNewsHandlerWithFeedList();
            handler.ApplyFeedModifications(null);
        }

        /// <summary>
        /// Makes sure that when importing a feed list that contains a 
        /// feed that already exists, we do not add it.
        /// </summary>
        [Test]
        public void ImportFeedListDuplicateFeedIgnored()
        {
            //Start by loading 3 feeds.
            var handler = CreateNewsHandlerWithFeedList();

            // Now import a feed list that contains 1 new feed 
            // and one that already exists. 
            using (FileStream stream = File.OpenRead(Path.Combine(WEBROOT_PATH, @"NewsHandlerTestFiles\FeedListWithDuplicateFrom03.xml")))
            {
                handler.ImportFeedlist(stream);
                stream.Close();
            }

            //Assertion.AssertEquals("3 + 1 = 4.  4 Feeds expected because one is a duplicate!", 4, handler.Feeds.Count);
			Assert.AreEqual(4, handler.GetFeeds().Count, "3 + 1 = 4.  4 Feeds expected because one is a duplicate!");

            AssertFeedTitle("MSDN: Visual C#", "http://msdn.microsoft.com/vcsharp/rss.xml", handler);
            AssertFeedCategory("Development", "http://msdn.microsoft.com/vcsharp/rss.xml", handler);
            AssertFeedTitle("ASP.NET Forums: Architecture", "http://www.asp.net/Forums/rss.aspx?forumid=16", handler);
            AssertFeedCategory("Forums", "http://www.asp.net/Forums/rss.aspx?forumid=16", handler);
            AssertFeedTitle("Slashdot", "http://slashdot.org/slashdot.rss", handler);
            AssertFeedCategory("News Technology", "http://slashdot.org/slashdot.rss", handler);
            AssertFeedTitle("Channel 9", "http://channel9.msdn.com/rss.aspx", handler);
            AssertFeedCategory("Microsoft", "http://channel9.msdn.com/rss.aspx", handler);
        }

        /// <summary>
        /// Runs some tests on marking items as recently viewed and 
        /// then making sure they appear in the Recently Viewed count.
        /// </summary>
        [Test]//, Ignore("New API changes makes this hard to test."), FileIOPermission(SecurityAction.Demand)]
        public void MarkAllCachedItemsAsReadPlacesItemsIntoTheRecentlyViewedCount()
        {
            //Load feed list.

            var handler = new BanditFeedSource(ConfigurationWithoutSearchIndexerAndUnitTestCache, new SubscriptionLocation(WEBROOT_PATH + @"\NewsHandlerTestFiles\LocalTestFeedList.xml"));
            handler.MaxItemAge = TimeSpan.MaxValue.Subtract(TimeSpan.FromDays(1));
            handler.LoadFeedlist(); 
            
            Assert.IsTrue(handler.FeedsListOK, "Feeds should be valid!");

            //Grab a feed.
            var feed = handler.GetFeeds()[BASE_URL + "LocalTestFeed.xml"];
            IFeedDetails feedInfo = handler.GetFeedDetails(feed.link);
            Assert.IsNotNull(feedInfo);
            Console.WriteLine("CACHEURL: " + feed.cacheurl);

            //Save the cache.
            handler.ApplyFeedModifications(feed.link);

            Assert.AreEqual(feed.title, "Rss Bandit Unit Test Feed");
            Assert.AreEqual(1, feed.storiesrecentlyviewed.Count);

            //Grab feed items
            var items = handler.GetItemsForFeed(feed.link, false);
            Assert.AreEqual(2, items.Count);

            //The first one should have been read. the second not.
            NewsItem item = (NewsItem)items[0];
            Assert.IsTrue(item.BeenRead);
            item = (NewsItem)items[1];
            Assert.IsTrue(!item.BeenRead);

            //So let's read the second item.
            handler.MarkAllCachedItemsAsRead(feed);
            handler.ApplyFeedModifications(feed.link);

            //Let's save this guy.
            using (FileStream newFeedStream = File.OpenWrite(Path.Combine(WEBROOT_PATH, @"NewsHandlerTestFiles\LocalTestFeedList_NEW.xml")))
            {
                handler.SaveFeedList(newFeedStream, FeedListFormat.NewsHandler);
                newFeedStream.Close();
                Assert.IsTrue(File.Exists(Path.Combine(WEBROOT_PATH, @"NewsHandlerTestFiles\LocalTestFeedList_NEW.xml")));
            }

            //Let's reload and see what happens.
            handler = new BanditFeedSource(ConfigurationWithoutSearchIndexerAndUnitTestCache, new SubscriptionLocation(BASE_URL + "LocalTestFeedList_NEW.xml"));
            handler.LoadFeedlist(); 
            
            feed = handler.GetFeeds().Values.FirstOrDefault();
            //Assertion.AssertEquals("Should be two now.", 2, feed.storiesrecentlyviewed.Count);
            Assert.AreEqual(2, feed.storiesrecentlyviewed.Count, "Should be two now.");
        }

        ///// <summary>
        ///// Runs a test of the comment posting function.
        ///// //TODO: Implement this.
        ///// </summary>
        //[Test]
        //public void PostCommentViaCommentAPI()
        //{
        //    /*
        //    RssParser handler = new RssParser(APP_NAME);
        //    handler.LoadFeedlist(BASE_URL + "FeedList03Feeds.xml", null);
        //    handler.PostCommentViaCommentAPI("", null);
        //    */
        //}

        /// <summary>
        /// Setups the test fixture by starting unpacking 
        /// embedded resources and starting the web server.
        /// </summary>
        [TestFixtureSetUp]
		protected override void SetUp()
        {
            //_cacheDirectory = NewsHandler.GetUserPath(APP_NAME);
            DeleteDirectory(UNPACK_DESTINATION);
            UnpackResourceDirectory("WebRoot.RssLocaterTestFiles");
            UnpackResourceDirectory("WebRoot.NewsHandlerTestFiles");
            UnpackResourceDirectory("Cache", new DirectoryInfo(_cacheDirectory));
            UnpackResourceDirectory("Expected");
            UnpackResourceDirectory("Settings");
            base.SetUp();
        }

		
        /// <summary>
        /// Stops the web server and cleans up the files.
        /// </summary>
        [TestFixtureTearDown]
        protected override void TearDown()
        {
            base.TearDown();
        }
	}
}