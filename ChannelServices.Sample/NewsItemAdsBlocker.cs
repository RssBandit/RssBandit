using System;
using NewsComponents;

namespace ChannelServices.AdsBlocker
{
	/// <summary>
	/// NewsItemAdsBlocker. 
	/// Sample for an NewsComponents.ChannelService implementation
	/// </summary>
	public class NewsItemAdsBlocker: IChannelProcessor
	{
		public NewsItemAdsBlocker(){}
		
		#region IChannelProcessor Members

		public INewsChannel[] GetChannels() {
			return new INewsChannel[] {new AdsBlockerChannel()};
		}

		#endregion
	}
}
