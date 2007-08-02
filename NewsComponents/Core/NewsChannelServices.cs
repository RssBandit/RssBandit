#region CVS Version Header
/*
 * $Id: NewsChannelServices.cs,v 1.1 2005/03/06 20:04:21 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/03/06 20:04:21 $
 * $Revision: 1.1 $
 */
#endregion

using System;
using System.Collections;

namespace NewsComponents
{
#if NIGHTCRAWLER 

	/// <summary>
	/// NewsChannelServices class is to be used to register INewsChannels.
	/// </summary>
	public class NewsChannelServices
	{
		static SortedList _newsItemChannels = new SortedList(new ChannelComparer());
		static SortedList _feedChannels = new SortedList(new ChannelComparer());
		
		private NewsChannelServices(){}

		public static void RegisterNewsChannel(INewsChannel channel)
		{
			if (channel == null)
				throw new ArgumentNullException("channel");
			if (channel.ChannelProcessingType == ChannelProcessingType.NewsItem)
				_newsItemChannels.Add(channel.ChannelName, channel);
			else if (channel.ChannelProcessingType == ChannelProcessingType.Feed)
				_feedChannels.Add(channel.ChannelName, channel);
			else
				throw new NotSupportedException("The channel processing type is not yet supported.");

		}

		internal static INewsItem ProcessItem(INewsItem item)
		{
			if (_newsItemChannels.Count == 0)
				return item;

			foreach (NewsItemChannel sink in _newsItemChannels.Values){
				item = sink.Process(item);
			}
			return item;
		}
		internal static IFeedDetails ProcessItem(IFeedDetails item) {
			if (_feedChannels.Count == 0)
				return item;

			foreach (FeedChannel sink in _feedChannels.Values){
				item = sink.Process(item);
			}
			return item;
		}

		class ChannelComparer: IComparer {

			#region IComparer Members

			public int Compare(object x, object y) {
				INewsChannel lhsX = x as INewsChannel;
				INewsChannel rhsY = y as INewsChannel;

				if(lhsX == null || rhsY == null)    // We only know how to sort INewsChannel, so return equal
					return 0;

				if (Object.ReferenceEquals(lhsX, rhsY))
					return 0;

				if (lhsX.ChannelPriority == rhsY.ChannelPriority)
					return 0;
				return (lhsX.ChannelPriority < rhsY.ChannelPriority ? -1: 1);
			}

			#endregion

		}

	}
#endif
}
