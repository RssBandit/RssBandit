#region CVS Version Header
/*
 * $Id: DisplayingNewsChannelProcessor.cs,v 1.2 2006/05/22 02:13:04 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2006/05/22 02:13:04 $
 * $Revision: 1.2 $
 */
#endregion

using System;
using NewsComponents;

namespace RssBandit.WinGui
{
	/// <summary>
	/// Internal build in DisplayingNewsChannelProcessor.
	/// Strip bad tags.
	/// </summary>
	public class DisplayingNewsChannelProcessor: IChannelProcessor
	{
		public DisplayingNewsChannelProcessor()	{}

		#region IChannelProcessor Members

		public INewsChannel[] GetChannels() {
			return new INewsChannel[] {new StripBadTagsChannel()};
		}

		#endregion
	}

	/// <summary>
	/// StripBadTagsChannel: implements a news channel processor to strip bad tags.
	/// </summary>
	public class StripBadTagsChannel: NewsItemChannelBase {

		public StripBadTagsChannel():
			base("http://www.rssbandit.org/displaying-channels/newsitemcontent/stripbadtags", 1000) {
		}

		public override INewsItem Process(INewsItem item) {
		/*	if (item.HasContent) {
				item.SetContent(NewsComponents.Utils.HtmlHelper.StripBadTags(item.Content), item.ContentType);
			} */ 
			return base.Process (item);
		}

	}

}
