using System;
using NewsComponents.Feed;
using NewsComponents.Utils;
using NUnit.Framework;
using RssBandit.UnitTests;

namespace NewsComponents.UnitTests
{
	/// <summary>
	/// NUnit test fixture containing unit tests of the RssItem class.
	/// </summary>
	[TestFixture]
	public class RssItemTests : BaseTestFixture
	{
		/// <summary>
		/// Test DateTimeExt class for various dateTime formats
		/// </summary>
		[Test]
		public void DateTimeExtEnsureFormats() 
		{
			DateTime dtUTCTime = new DateTime(2004, 6, 5, 5, 20, 9,0);
			
			// DateTimeExt.Parse returns the UTC time
		    DateTime dt = DateTimeExt.ParseRfc2822DateTime("2004-06-05T04:20:09-01:00");
            Assert.AreEqual(dtUTCTime, dt, "Invalid date returned by .Parse().");

			dt = DateTimeExt.ParseRfc2822DateTime("2004-06-05T05:20:09+00:00");
            Assert.AreEqual(dtUTCTime, dt, "Invalid date returned by .Parse().");

			// returns the GMT time 
			DateTime dtx = DateTime.Parse("2004-06-05T06:20:09+01:00").ToUniversalTime();
			Assert.AreEqual(dtUTCTime , dtx, "Invalid date returned by .ToDateTime().");

			dtx = DateTime.Parse("2004-06-05T05:20:09+00:00").ToUniversalTime();
			Assert.AreEqual(dtUTCTime , dtx, "Invalid date returned by .ToDateTime().");
		}

		/// <summary>
		/// A simple test that demonstrates various ways to create an 
		/// instance of an RssItem.  If this test fails, then we're in 
		/// deep trouble. Yes, it's almost too simple to even worry about, 
		/// but let's do this just for kicks.
		/// </summary>
		[Test]
		public void CreateNewsItemEnsureProperties()
		{
			DateTime fetchDate = DateTime.Now;

			NewsItem item = new NewsItem(null, "TestTitle", "TestLink", "TestDescription", fetchDate, "TestSubject");
			Assert.IsNull( item.Feed,"Feed should be null.");
			Assert.AreEqual("TestTitle", item.Title, "Strange. The title is wrong.");
            //Assert.AreEqual("TestDescription", item., "Weird. The description is wrong." );
            Assert.AreEqual(fetchDate, item.Date,"Curious. The date is wrong.");
            Assert.AreEqual("TestSubject", item.Subject, "Odd. The subject is wrong.");
		}

		/// <summary>
		/// This test gets a little more complicated. It makes sure that 
		/// CDATA sections within the description are stripped and that 
		/// other replacements occur properly.
		/// </summary>
		[Test]
		public void CDATAStrippingAndEntityEscaping()
		{
			NewsItem item = new NewsItem(null, "", "link", "<![CDATA[This is stripped]]>", DateTime.Now, "");
			Assert.AreEqual( "This is stripped", item.Content,"Bad strip job.");

			item = new NewsItem(null, "", "link", "<![CDATA[]]>How about now?", DateTime.Now, "");
			Assert.AreEqual("How about now?", item.Content, "Bad strip job.");

			item = new NewsItem(null, "", "link", "<![CDATA[]]>", DateTime.Now, "");
			Assert.AreEqual(null, item.Content, "Should be empty.");

			item = new NewsItem(null, "", "link", "<?xml:namespace xmlns=blah>", DateTime.Now, "");
			Assert.AreEqual("<?xml:namespace xmlns=blah>", item.Content, "Bad escaping! Bad!");
			
			item = new NewsItem(null, "", "link", "<?XML:NAMESPACE xmlns=blah>", DateTime.Now, "");
			Assert.AreEqual("<?XML:NAMESPACE xmlns=blah>", item.Content, "Bad escaping! Bad!" );
		}

		/// <summary>
		/// Makes sure that the ToString() method correctly writes 
		/// out the XML.
		/// </summary>
		[Test]
		public void ToStringProducesCorrectXml()
		{
			DateTime fetchDate = DateTimeExt.ParseRfc2822DateTime("Fri, 04 Apr 2003 10:41:37 GMT");
			NewsItem item = new NewsItem(null, "TestTitle", "TestLink", "TestDescription", fetchDate, "TestSubject");
			
			item.FeedDetails = new FeedInfo("", null, null, "FeedTitle", "FeedLink", "FeedDescription");
			Assert.AreEqual("FeedTitle", item.FeedDetails.Title, "The oh so creative title is wrong!");

			Assert.AreEqual(UnpackResource("Expected.RssItemTests.TestToString.xml"), item.ToString(), "The XML was not as we expected.");

			// we can test here only GMT universal time (we are world wide organized :-) 
			//Assert.AreEqual(UnpackResource("Expected.RssItemTests.TestToString.NoGMT.xml"), item.ToString(NewsItemSerializationFormat.RssItem, false),"No GMT wrong.");
			Assert.AreEqual(UnpackResource("Expected.RssItemTests.TestToString.NotStandalone.xml"), item.ToString(NewsItemSerializationFormat.RssFeed, true), "Not Standalone wrong.");
		}

		[SetUp]
		public void SetUp()
		{
			UnpackResourceDirectory("Expected");
		}

		[TearDown]
		public void TearDown()
		{
			DeleteDirectory(UNPACK_DESTINATION);
		}
	}
}
