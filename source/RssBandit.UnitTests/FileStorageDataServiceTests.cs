#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.IO;
using System.Threading;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Storage;
using NUnit.Framework;
using RssBandit.UnitTests;

namespace RssBandit.UnitTests
{
	/// <summary>
    /// TODO: reactivate tests with new/renamed/refactored class
	/// </summary>
	[TestFixture]
    public class FileStorageDataServiceTests : BaseTestFixture
	{
		private readonly string _cacheDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStorageDataServiceTests"/> class.
        /// </summary>
        public FileStorageDataServiceTests()
        {
            _cacheDirectory = UNPACK_DESTINATION + @"\Cache";
        }

        /// <summary>
        /// Tests that the Constructors throws argument exception if init parameter is null
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsArgumentNullExceptionIfInitializeGetNull()
        {
            new FileStorageDataService().Initialize(null);
        }

        /// <summary>
        /// Tests that the Constructors throws argument out of range exception if init parameter is empty
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorThrowsArgumentOutOfRangeExceptionIfInitializeGetEmpty()
        {
            new FileStorageDataService().Initialize("");
        }

        /// <summary>
        /// Tests that cache doesn't find phantom feeds.
        /// </summary>
        [Test]
        public void FeedExistsReturnsFalseForNonExistentFeed()
        {
            var cache = new FileStorageDataService();
            cache.Initialize(Path.GetFullPath(_cacheDirectory));
            var feed = new NewsFeed();
            feed.cacheurl = "http://localhost/NonExistent/DoesNotExist.xml";
            Assert.IsFalse(cache.FeedExists(feed), "Can't be true. This really doesn't exist.");
        }

        /// <summary>
        /// Tests that the cache can find an existing feed.
        /// </summary>
        [Test]
        public void FeedExistsFindsExistingFeed()
        {
            var cache = new FileStorageDataService();
            cache.Initialize(Path.GetFullPath(_cacheDirectory));
            var feed = new NewsFeed();
            feed.cacheurl = "172.0.0.1.8081.1214057202.df05c3d0bd8748e68f121451084e3e62.xml";
            Assert.IsTrue(cache.FeedExists(feed), "The feed's there, look harder.");
        }

        /// <summary>
        /// Tests removing items from the cache works.
        /// </summary>
        [Test]
        public void RemoveFeedDeletesCacheItem()
        {
            var cache = new FileStorageDataService();
            cache.Initialize(Path.GetFullPath(_cacheDirectory));
            var feed = new NewsFeed();
            feed.cacheurl = "172.0.0.1.8081.1214057202.df05c3d0bd8748e68f121451084e3e62.xml";
            Assert.IsTrue(cache.FeedExists(feed), "The feed's there, look harder.");

            cache.RemoveFeed(feed);
            Assert.IsFalse(cache.FeedExists(feed), "The cache is still there though you removed it.");
            Assert.IsFalse(File.Exists(_cacheDirectory + @"\172.0.0.1.8081.1214057202.df05c3d0bd8748e68f121451084e3e62.xml"), "The cache file was not removed!");
        }

        /// <summary>
        /// Clears the cache and asserts all cache files have been removed.
        /// </summary>
        [Test]
        public void ClearCacheRemovesCacheItem()
        {
            var cache = new FileStorageDataService();
            cache.Initialize(Path.GetFullPath(_cacheDirectory));
            var feed = new NewsFeed();
            feed.cacheurl = "172.0.0.1.8081.1214057202.df05c3d0bd8748e68f121451084e3e62.xml";
            Assert.IsTrue(cache.FeedExists(feed), "The feed's there, look harder.");

            cache.RemoveAllNewsItems();
            string[] files = Directory.GetFiles(_cacheDirectory);
            Assert.AreEqual(files.Length, 0, "There should be no files in the cache.");
        }

        ///// <summary>
        ///// SaveFeed can only be tested via NewsHandler.ApplyModifications method.
        ///// </summary>
        //[Test]
        //public void SaveFeedCreatesCacheFile()
        //{
        //    string cacheDirectory = string.Empty;
        //    try
        //    {
        //        cacheDirectory = NewsHandler.GetUserPath(APP_NAME);
        //        UnpackResourceDirectory("Cache", new DirectoryInfo(cacheDirectory));
        //        UnpackResourceDirectory("WebRoot.NewsHandlerTestFiles");
        //        base.SetUp();

        //        //Load feed list.
        //        FileCacheManager cache = new FileCacheManager(Path.Combine(cacheDirectory, "Cache"));

        //        NewsHandler handler = new NewsHandler(APP_NAME, cache);
        //        handler.LoadFeedlist(new FileStream(WEBROOT_PATH + @"\NewsHandlerTestFiles\LocalTestFeedList.xml", FileMode.Open), null);
        //        Assert.IsTrue(handler.FeedsListOK, "Feeds should be valid!");

        //        //Grab a feed.
        //        feedsFeed feed = handler.FeedsTable[NewsHandlerTests.BASE_URL + "LocalTestFeed.xml"];
        //        Console.WriteLine("CACHEURL: " + feed.cacheurl);
        //        FileInfo cachedFile = new FileInfo(Path.Combine(cacheDirectory, @"Cache\" + feed.cacheurl));

        //        DateTime lastWriteTime = cachedFile.LastWriteTime;

        //        Assert.IsNotNull(handler.GetFeedInfo(feed.link), "Feed info should not be null.");

        //        //Save the cache.
        //        Thread.Sleep(1000);
        //        handler.ApplyFeedModifications(feed.link);

        //        Assert.IsTrue(cache.FeedExists(feed), "The feed should have been saved to the cache");

        //        string[] files = Directory.GetFiles(Path.Combine(cacheDirectory, "Cache"));
        //        Assert.IsTrue(files.Length > 0, "There should be at least one cache file in the cache.");
        //        cachedFile = new FileInfo(Path.Combine(cacheDirectory, @"Cache\" + feed.cacheurl));
        //        Assert.IsTrue(cachedFile.LastWriteTime > lastWriteTime, "Didn't overwrite the file. Original: " + lastWriteTime + "  New: " + cachedFile.LastWriteTime);

        //    }
        //    finally
        //    {
        //        base.TearDown();
        //        if (cacheDirectory.Length > 0 && Directory.Exists(cacheDirectory))
        //            Directory.Delete(cacheDirectory, true);
        //    }
        //}

        /// <summary>
        /// Setups the test fixture by starting unpacking 
        /// embedded resources and starting the web server.
        /// </summary>
        [SetUp]
        protected new void SetUp()
        {
            DeleteDirectory(UNPACK_DESTINATION);
            UnpackResourceDirectory("Cache");
        }

        /// <summary>
        /// Stops the web server and cleans up the files.
        /// </summary>
        [TearDown]
        protected new void TearDown()
        {
            DeleteDirectory(UNPACK_DESTINATION);
            if (_cacheDirectory.Length > 0 && Directory.Exists(_cacheDirectory))
                DeleteDirectory(_cacheDirectory);
        }

	}
}
