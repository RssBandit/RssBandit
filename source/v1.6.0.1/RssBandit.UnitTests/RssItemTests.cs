using System;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Utils;
using NUnit.Framework;

namespace RssBandit.UnitTests
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
			DateTime dtLocalTime = TimeZone.CurrentTimeZone.ToLocalTime(dtUTCTime);
			
			if (!TimeZone.CurrentTimeZone.IsDaylightSavingTime(dtLocalTime))		
			{
					// we need to pull this back an hour for our friends in Arizona..
					dtLocalTime.AddHours(-1);
			}
				
			// DateTimeExt.Parse returns the local time
			DateTime dt = DateTimeExt.Parse("2004-06-05T04:20:09-01:00");
			Assert.AreEqual(dtLocalTime , dt, "Invalid date returned by .Parse().");

			dt = DateTimeExt.Parse("2004-06-05T05:20:09+00:00");
			Assert.AreEqual(dtLocalTime , dt, "Invalid date returned by .Parse().");

			//DateTimeExt.ToDateTime returns the GMT time 
			DateTime dtx = DateTimeExt.ToDateTime("2004-06-05T06:20:09+01:00");
			Assert.AreEqual(dtUTCTime , dtx, "Invalid date returned by .ToDateTime().");

			dtx = DateTimeExt.ToDateTime("2004-06-05T05:20:09+00:00");
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
			Assertion.AssertNull("Feed should be null.", item.Feed);
			Assertion.AssertEquals("Strange. The title is wrong.", "TestTitle", item.Title);
			//Assertion.AssertEquals("Weird. The description is wrong.", "TestDescription", item.);
			Assertion.AssertEquals("Curious. The date is wrong.", fetchDate, item.Date);
			Assertion.AssertEquals("Odd. The subject is wrong.", "TestSubject", item.Subject);
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
			Assertion.AssertEquals("Bad strip job.", "This is stripped", item.Content);

			item = new NewsItem(null, "", "link", "<![CDATA[]]>How about now?", DateTime.Now, "");
			Assertion.AssertEquals("Bad strip job.", "How about now?", item.Content);

			item = new NewsItem(null, "", "link", "<![CDATA[]]>", DateTime.Now, "");
			Assertion.AssertEquals("Should be empty.", null, item.Content);

			item = new NewsItem(null, "", "link", "<?xml:namespace xmlns=blah>", DateTime.Now, "");
			Assertion.AssertEquals("Bad escaping! Bad!", "<?xml:namespace xmlns=blah>", item.Content);
			
			item = new NewsItem(null, "", "link", "<?XML:NAMESPACE xmlns=blah>", DateTime.Now, "");
			Assertion.AssertEquals("Bad escaping! Bad!", "<?XML:NAMESPACE xmlns=blah>", item.Content);
		}

		/// <summary>
		/// Makes sure that the ToString() method correctly writes 
		/// out the XML.
		/// </summary>
		[Test]
		public void ToStringProducesCorrectXml()
		{
			DateTime fetchDate = DateTimeExt.Parse("Fri, 04 Apr 2003 10:41:37 GMT");
			NewsItem item = new NewsItem(null, "TestTitle", "TestLink", "TestDescription", fetchDate, "TestSubject");
			
			item.FeedDetails = new FeedInfo("", null, "FeedTitle", "FeedLink", "FeedDescription");
			Assert.AreEqual("FeedTitle", item.FeedDetails.Title, "The oh so creative title is wrong!");

			Assertion.AssertEquals("Capt'n! The XML was not as we expected.", UnpackResource("Expected.RssItemTests.TestToString.xml"), item.ToString());

			// we can test here only GMT universal time (we are world wide organized :-) 
			Assertion.AssertEquals("No GMT wrong.", UnpackResource("Expected.RssItemTests.TestToString.NoGMT.xml"), item.ToString(NewsItemSerializationFormat.RssItem, true));
			Assertion.AssertEquals("Not Standalone wrong.", UnpackResource("Expected.RssItemTests.TestToString.NotStandalone.xml"), item.ToString(NewsItemSerializationFormat.RssFeed, true));
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
