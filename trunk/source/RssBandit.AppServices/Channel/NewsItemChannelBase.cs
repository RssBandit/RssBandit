#region CVS Version Header
/*
 * $Id: NewsItemChannelBase.cs,v 1.1 2005/05/08 17:03:39 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/05/08 17:03:39 $
 * $Revision: 1.1 $
 */
#endregion

using System;

namespace NewsComponents
{
	/// <summary>
	/// Provides a base implementation of a NewsChannel.
	/// NewsItem processors MUST inherit this class!
	/// </summary>
	public class NewsItemChannelBase: INewsChannel {

		protected string p_channelName;
		protected int p_channelPriority;

		public NewsItemChannelBase():
			this("http://www.rssbandit.org/channels/newsitemchannel", 50){
			}

		public NewsItemChannelBase(string channelName, int channelPriority)
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