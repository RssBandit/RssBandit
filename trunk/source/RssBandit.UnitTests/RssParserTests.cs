using System;
using System.Collections;
using NewsComponents;
using NewsComponents.Feed;
using NUnit.Framework;

namespace RssBandit.UnitTests
{
	/// <summary>
	/// Summary description for RssParserTests.
	/// </summary>
	[TestFixture]
	public class RssParserTests : CassiniHelperTestFixture
	{
		const string BASE_URL = "http://127.0.0.1:8081/NewsHandlerTestFiles/";

		/// <summary>
		/// Tests obtaining the title from a RSS feed.
		/// </summary>
		[Test]
		public void GetItemsForFeedReturnsCorrectTitle()
		{
			NewsHandler handler = new NewsHandler(APP_NAME);
			RssParser parser = new RssParser(handler);
			ArrayList feedItems = null;
			feedItems = parser.GetItemsForFeed(BASE_URL + "FeedWithWeirdLineBreaks.xml");

			NewsComponents.NewsItem item = feedItems[0] as NewsComponents.NewsItem;
			NewsComponents.NewsItem item2 = feedItems[1] as NewsComponents.NewsItem;
			Assert.AreEqual("This is item 1 of a really cool(...)", item.Title, "The title was not what we expected.");
			Assert.AreEqual("Testing a second item", item2.Title);
		}

		/// <summary>
		/// Setups the test fixture by starting unpacking 
		/// embedded resources and starting the web server.
		/// </summary>
		[SetUp]
		protected override void SetUp()
		{
			DeleteDirectory(UNPACK_DESTINATION);
			UnpackResourceDirectory("WebRoot.NewsHandlerTestFiles");
			base.SetUp();
		}

		/// <summary>
		/// Stops the web server and cleans up the files.
		/// </summary>
		[TearDown]
		protected override void TearDown()
		{
			base.TearDown();
			DeleteDirectory(UNPACK_DESTINATION);
		}
	}
}
