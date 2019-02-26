using System;
using System.Collections.Generic;
using NewsComponents.Utils;
using Xunit;

namespace NewsComponents.UnitTests.Utils
{
	/// <summary>
	/// Summary description for StringHelperTests.
	/// </summary>
	public class HtmlHelperTests
	{
		#region test consts

		private readonly List<string> htmlFragments = new List<string>
		{
			@"<p>text<a href=""http://www.live.com"">live.com</a> for the term <a href=""http://search.live.com/results.aspx?q=%2Blivewhacking"" title=""Search livewhacking"">livewhacking</a> returns exactly one result.<img src=""http://blogs.msdn.com/aggbug.aspx?PostID=7227433"" width='1' height='1'></p>",
			@"<p>text<a title=""Hi Live"" href=""http://www.live.com"">live.com</a> for the term <a href=""http://search.live.com/results.aspx?q=%2Blivewhacking"">livewhacking</a> returns exactly one result.<img src=""http://blogs.msdn.com/aggbug.aspx?PostID=7227433"" width='1' height='1'></p>",
			@"<p xmlns=""http://www.w3.org/1999/xhtml"">
You will ask: why?
</p><p xmlns=""http://www.w3.org/1999/xhtml"">
I mean all the People that wrote an E-Mail and sent it to my old js/hidden E-Mail
address at <a href=""http://www.rendelmann.info/blog/ct.ashx?id=e21b2bcb-7d7d-4225-82c4-e7dcd0dac8d4&amp;url=http%3a%2f%2fwww.rendelmann.info%2fstatic%2fimpressum.htm"">about
page</a>: contact[at]rendelmann...! I just realized today this address was not monitored
by any of my POP E-Mail Clients. So the mails stay there at the Providers storage
and I never get any notice about - I'm so sorry!
</p><p xmlns=""http://www.w3.org/1999/xhtml"">
Now I did not like to answer each sender directly (mails aged from 8 to 9 years),
instead I write this post and say thanks and so sorry for their (unanswered) questions
(David Johnston, Didier Barbas, R. Rohrmoser, Andrew Connell, Ben Hollis, Zeljko Svedic
(GemBox Software), Zemp Dominik, UPAMUELLERS, Duncan Garratt), feature requests for <a href=""http://www.rendelmann.info/blog/ct.ashx?id=e21b2bcb-7d7d-4225-82c4-e7dcd0dac8d4&amp;url=http%3a%2f%2fwww.rssbandit.org"">RSS
Bandit</a> (Nicolas ANTOINE), or problem reports (Arthur Salomon, Christine Rice
(Volt), Daniel McPherson, zess/zessy) and other related notes (Alp Uckan, Prof. Dr.
Schuetz - TU Chemnitz)!
</p><img width=""0"" height=""0"" src=""http://www.rendelmann.info/blog/aggbug.ashx?id=e21b2bcb-7d7d-4225-82c4-e7dcd0dac8d4"" xmlns=""http://www.w3.org/1999/xhtml"" />"
		};
		#endregion

		/// <summary>
		/// Tests the ShortenByEllipsis method.
		/// </summary>
		[Fact]
		public void RetrieveTitledLinks()
		{
			var links = HtmlHelper.RetrieveTitledLinks(htmlFragments[0]);
			Assert.NotNull(links);
			Assert.Equal(2, links.Count);

			// Note: Urls should be lower case:
			Assert.Equal("http://www.live.com", links[0].Url);
			Assert.Equal("live.com", links[0].Title);
			Assert.Equal("http://search.live.com/results.aspx?q=%2blivewhacking", links[1].Url);
			Assert.Equal("Search livewhacking", links[1].Title);

			links = HtmlHelper.RetrieveTitledLinks(htmlFragments[1]);
			Assert.Equal("http://www.live.com", links[0].Url);
			Assert.Equal("Hi Live", links[0].Title);
			Assert.Equal("http://search.live.com/results.aspx?q=%2blivewhacking", links[1].Url);
			Assert.Equal("livewhacking", links[1].Title);

			links = HtmlHelper.RetrieveTitledLinks(htmlFragments[2]);
			Assert.Equal(2, links.Count);
		}
	}
}
