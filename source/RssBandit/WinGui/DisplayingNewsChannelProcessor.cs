#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
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
