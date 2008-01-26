using System;
using NewsComponents.Utils;
using NUnit.Framework;

namespace RssBandit.UnitTests.Utils
{
	/// <summary>
	/// Summary description for StringHelperTests.
	/// </summary>
	[TestFixture]
	public class StringHelperTests
	{
		/// <summary>
		/// Tests the ShortenByEllipsis method.
		/// </summary>
		[Test]
		public void TestGetFirstWords()
		{
			Assert.AreEqual("One two three", StringHelper.GetFirstWords("One two" + Environment.NewLine + "three Four Five", 3));
			Assert.AreEqual("One two three", StringHelper.GetFirstWords("One two     " + Environment.NewLine + "three Four Five", 3));
			Assert.AreEqual("One two three", StringHelper.GetFirstWords("One two     " + Environment.NewLine + "\tthree\tFour   Five", 3));
		}
	}
}
