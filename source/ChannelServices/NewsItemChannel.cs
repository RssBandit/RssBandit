#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;

namespace NewsComponents
{
	/// <summary>
	/// Provides a base implementation of a NewsChannel.
	/// NewsItem processors MUST inherit this class!
	/// </summary>
	public class NewsItemChannel: INewsChannel {

		protected string p_channelName;
		protected int p_channelPriority;

		public NewsItemChannel():
			this("http://www.rssbandit.org/channels/newsitemchannel", 50){
			}

		public NewsItemChannel(string channelName, int channelPriority)
		{
			p_channelName = channelName;
			p_channelPriority = channelPriority;
		}

		#region INewsChannel Members

		public virtual string ChannelName {
			get { return p_channelName; }
		}

		public virtual int ChannelPriority {
			get {return p_channelPriority; }
		}

		public NewsComponents.ChannelProcessingType ChannelProcessingType {
			get { return ChannelProcessingType.NewsItem;}
		}

		#endregion

		/// <summary>
		/// Base implementation does nothing then return the non-modified item.
		/// </summary>
		/// <param name="item">INewsItem</param>
		/// <returns></returns>
		public virtual INewsItem Process(INewsItem item)
		{
			return item;
		}

	}
}