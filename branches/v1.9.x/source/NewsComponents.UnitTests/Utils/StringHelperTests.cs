using System;
using NewsComponents.Utils;
using Xunit;

namespace NewsComponents.UnitTests.Utils
{
	/// <summary>
	/// Summary description for StringHelperTests.
	/// </summary>
	public class StringHelperTests
	{
		/// <summary>
		/// Tests the ShortenByEllipsis method.
		/// </summary>
		[Fact]
		public void TestGetFirstWords()
		{
			Assert.Equal("One two three", StringHelper.GetFirstWords("One two" + Environment.NewLine + "three Four Five", 3));
			Assert.Equal("One two three", StringHelper.GetFirstWords("One two     " + Environment.NewLine + "three Four Five", 3));
			Assert.Equal("One two three", StringHelper.GetFirstWords("One two     " + Environment.NewLine + "\tthree\tFour   Five", 3));
		}
	}
}
