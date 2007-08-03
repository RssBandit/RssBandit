#region CVS Version Header
/*
 * $Id: FeedChannel.cs,v 1.1 2005/03/04 16:42:35 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/03/04 16:42:35 $
 * $Revision: 1.1 $
 */
#endregion

using System;

namespace NewsComponents {
	/// <summary>
	/// Provides a base implementation of a NewsChannel.
	/// Feed processor classes must inherit this class!
	/// </summary>
	public class FeedChannel: INewsChannel {

		protected string p_channelName;
		protected int p_channelPriority;

		public FeedChannel():
			this("http://www.rssbandit.org/channels/feedchannel", 50){
		}

		public FeedChannel(string channelName, int channelPriority) {
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
			get { return ChannelProcessingType.Feed;}
		}

		#endregion

		/// <summary>
		/// Base implementation does nothing then return the non-modified item.
		/// </summary>
		/// <param name="item">IFeedDetails</param>
		/// <returns></returns>
		public virtual IFeedDetails Process(IFeedDetails item) {
			return item;
		}

	}
}