using System;
using System.Collections;
using NewsComponents;
using NewsComponents.Feed;
using NUnit.Framework;
using Rhino.Mocks;

namespace RssBandit.UnitTests
{
	/// <summary>
    /// Tests the RssParser
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
            var feedSourceStub = MockRepository.GenerateStub<FeedSource>();
            RssParser parser = new RssParser(feedSourceStub);
            var feedItems = parser.GetItemsForFeed(BASE_URL + "FeedWithWeirdLineBreaks.xml");

            Assert.IsNotNull(feedItems, "null is not expected");
            Assert.AreEqual(2, feedItems.Count, "2 items expected");
            var item = feedItems[0];
            var item2 = feedItems[1];
            Assert.AreEqual("This is item 1 of a really cool(...)", item.Title, "The title was not what we expected.");
            Assert.AreEqual("Testing a second item", item2.Title);
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
			UnpackResourceDirectory("WebRoot.NewsHandlerTestFiles");
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
