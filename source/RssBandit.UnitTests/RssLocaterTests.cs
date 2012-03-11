using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using NewsComponents.Feed;
using NUnit.Framework;

namespace RssBandit.UnitTests
{
	/// <summary>
	/// NUnit test fixture containing unit tests of the RssLocater class.
	/// </summary>
	/// <remarks>
	/// IDEAS for some tests:
	///		- Simulate network issues (may require some custom mods to Cassini or just writing HttpModules)
	///		- USE CassiniEX instead of Cassini http://www.systemex.net/CassiniEx/
	/// </remarks>
	[TestFixture]
	public class RssLocaterTests : CassiniHelperTestFixture
	{
		const string BASE_URL = "http://127.0.0.1:8081/RssLocaterTestFiles/";

		/// <summary>
		/// Let's see if Auto Discovery finds ATOM feeds.
		/// </summary>
		/// <remarks>
		/// Remove the Ignore attribute after we've added ATOM support.
		/// </remarks>
		[Test]
		public void GetRssAutoDiscoveryLinksFindsAtom10Links()
		{
			RssLocater locater = new RssLocater(null, null);
			var feeds = locater.GetRssAutoDiscoveryLinks(BASE_URL + "PageWithAtomLinks.htm");
			
			Assert.AreEqual(1, feeds.Count, "Hmm, the page only has one feed and I couldn't find it..");
			Assert.AreEqual(BASE_URL + "SampleATOM0.3Feed.xml", feeds[0], "ATOM was too small to discover.");
		}
		
		/// <summary>
		/// Creates an RssLocater and scans an html page (hosted locally) 
		/// for links to Rss feeds.  Makes sure it can find the 
		/// feeds we planted in the page.
		/// </summary>
		[Test]
		public void GetRssAutoDiscoveryLinksFindsRssLinks()
		{
            RssLocater locater = new RssLocater(null, null);
			var feeds = locater.GetRssAutoDiscoveryLinks(BASE_URL + "GetRssAutoDiscoveryLinks.html");
			
			Assert.AreEqual(4, feeds.Count, "Obviously I could not count as I expected 4 feeds.");

			Assert.AreEqual(BASE_URL + "SampleRss0.91Feed.xml", (string)feeds[0]);
			Assert.AreEqual(BASE_URL + "SampleRss0.92Feed.rss", (string)feeds[1]);
			Assert.AreEqual(BASE_URL + "SampleRss1.0Feed.rss", (string)feeds[2]);
			Assertion.AssertEquals("Missing version 2.0. Stuck in the past.", BASE_URL + "SampleRss2.0Feed.xml", (string)feeds[3]);

			feeds = locater.GetRssFeedsForUrl(BASE_URL + "AutoDiscovery1.htm", true);
			Assert.AreEqual(feeds.Count, 2);
		}

		/// <summary>
		/// Tests finding Rss feeds using the feed: protocol.
		/// </summary>
		/// <remarks>http://www.brindys.com/winrss/feedformat.html</remarks>
		[Test]
		public void GetRssFeedsForUrlFindsFeedProtocolUrls()
		{
            RssLocater locater = new RssLocater(null, null);
			var feeds = locater.GetRssFeedsForUrl(BASE_URL + "feedProtocol.htm", true);
			Assert.AreEqual(3, feeds.Count);

			Assert.AreEqual(BASE_URL + "SampleRss0.91Feed.xml", feeds[0].ToString());
			Assert.AreEqual(BASE_URL + "SampleRss0.92Feed.rss", feeds[1].ToString());
			Assert.AreEqual(BASE_URL + "SampleRss2.0Feed.xml", feeds[2].ToString());
		}

		/// <summary>
		/// Tests finding Rss feeds using well know local host listeners.
		/// </summary>
		[Test]
		public void GetRssFeedsForUrlFindsLinksToWellKnownListeners()
		{
            RssLocater locater = new RssLocater(null, null);
			var feeds = locater.GetRssFeedsForUrl(BASE_URL + "localListeners.htm", true);
			Assert.AreEqual(8, feeds.Count);

			Assert.AreEqual(BASE_URL + "SampleRss0.91Feed.xml", feeds[0].ToString());
			Assert.AreEqual(BASE_URL + "SampleRss0.92Feed.rss", feeds[1].ToString());
			Assert.AreEqual(BASE_URL + "SampleRss1.0Feed.rss", feeds[2].ToString());
			Assert.AreEqual(BASE_URL + "SampleRss2.0Feed.xml", feeds[3].ToString());
			Assert.AreEqual(BASE_URL + "SampleFeed001Rss2.0.rss", feeds[4].ToString());
			Assert.AreEqual(BASE_URL + "SampleFeed002Rss2.0.rss", feeds[5].ToString());
			Assert.AreEqual(BASE_URL + "SampleFeed003Rss2.0.rss", feeds[6].ToString());
			Assert.AreEqual(BASE_URL + "SampleFeed004Rss2.0.rss", feeds[7].ToString());
		}

		/// <summary>
		/// Looks for links to ATOM and Rss Feeds.
		/// </summary>
		[Test]
		public void GetRssFeedsForUrlFindsAtomAndRssFeeds()
		{
            RssLocater locater = new RssLocater(null, null);
			var feeds = locater.GetRssFeedsForUrl(BASE_URL + "GetRssFeedsForUrl.html", true);
			
			Assert.AreEqual(4, feeds.Count);

			Assert.AreEqual(BASE_URL + "SampleRss0.91Feed.xml", feeds[0].ToString());
			Assert.AreEqual(BASE_URL + "SampleRss0.92Feed.rss", feeds[1].ToString());
			Assert.AreEqual(BASE_URL + "SampleRss1.0Feed.rss", feeds[2].ToString());
			Assert.AreEqual(BASE_URL + "SampleRss2.0Feed.xml", feeds[3].ToString());
			
			feeds = locater.GetRssFeedsForUrl(BASE_URL + "LinksToExternalFeed.htm", true);
			Assertion.AssertEquals(1, feeds.Count);
		}

		/// <summary>
		/// Tests the exception case where a file is not found when calling 
		/// the method GetRssAutoDiscoveryLinks().
		/// In this scenario, a WebException is thrown.
		/// </summary>
		[Test, ExpectedException(typeof(WebException))]
		public void GetRssAutoDiscoveryLinksThrowsWebExceptionIfFileNotFound()
		{
            RssLocater locater = new RssLocater(null, null);
			locater.GetRssAutoDiscoveryLinks(BASE_URL + "FileNotFound.html");
		}

		/// <summary>
		/// Tests that the method GetRssFeedsForUrl() does not 
		/// throw an exception when a 404 response is encountered.
		/// </summary>
		[Test]
		public void GetRssFeedsForUrlDoesNotThrowExceptionWhen404Encountered()
		{
            RssLocater locater = new RssLocater(null, null);
			var feeds = locater.GetRssFeedsForUrl(BASE_URL +"FileNotFound.html", false);
			Assert.AreEqual(0, feeds.Count);
		}

		/// <summary>
		/// Make sure that we don't find phantom feeds where there are none. 
		/// This is done for completeness.
		/// </summary>
		[Test]
		public void GetRssFeedsForUrlDoesNotFindPhantomFeeds()
		{
            RssLocater locater = new RssLocater(null, null);
			var feeds = locater.GetRssFeedsForUrl(BASE_URL + "NoFeeds.html", true);
			Assert.AreEqual(0, feeds.Count, "Impossibly, we found a feed where there are none.");
		}

		/// <summary>
		/// Make sure that we don't auto-discover phantom feeds where there are none. 
		/// This is done for completeness.
		/// </summary>
		[Test]
		public void GetRssAutoDiscoveryLinksDoesNotFindPhantomFeeds()
		{
            RssLocater locater = new RssLocater(null, null);
			var feeds = locater.GetRssAutoDiscoveryLinks(BASE_URL + "NoFeeds.html");
			Assert.AreEqual(0, feeds.Count, "Impossibly, we auto-discovered a feed where there are none.");
		}

		

		/// <summary>
		/// Setups the test fixture by starting unpacking 
		/// embedded resources and starting the web server.
		/// </summary>
		[TestFixtureSetUp]
		protected override void SetUp()
		{
			Console.WriteLine("SetupTestFixture");
			DeleteDirectory(UNPACK_DESTINATION);
			UnpackResourceDirectory("WebRoot.RssLocaterTestFiles");
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


    /// <summary>
    /// RSS Locator tests without web server
    /// </summary>
    [TestFixture]
    public class RssLocaterWithoutWebServerTests
    {
        private const string BASE_URL = "http://127.0.0.1:8081/RssLocaterTestFiles/";

        /// <summary>
        /// If the web server is down, GetRssFeedsForUrl should not throw an 
        /// exception. but GetRssAutoDiscoveryLinks should be quite exceptional!
        /// </summary>
        [Test]
        public void GetRssFeedsForUrlDoesNotThrowExceptionWhenWebServerDown()
        {
            RssLocater locater = new RssLocater(null, null);
            IList<string> feeds = locater.GetRssFeedsForUrl(BASE_URL + "NoFeeds.html", false);
            Assert.AreEqual(0, feeds.Count, "Impossibly we found feeds where there are none.");
        }

        /// <summary>
        /// If the web server is down, GetRssAutoDiscoveryLinks should throw 
        /// an exception!
        /// </summary>
        [Test, ExpectedException(typeof(SocketException))]
        public void GetRssAutoDiscoveryLinksThrowsExceptionIfWebServerIsDown()
        {
            RssLocater locater = new RssLocater(null, null);
            locater.GetRssAutoDiscoveryLinks(BASE_URL + "NoFeeds.html");
        }
    }
}
