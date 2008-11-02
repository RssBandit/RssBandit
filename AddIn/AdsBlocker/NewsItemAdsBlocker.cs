#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using NewsComponents;

namespace ChannelServices.AdsBlocker.AddIn
{
	/// <summary>
	/// NewsItemAdsBlocker. 
	/// Sample for an NewsComponents.ChannelService implementation
	/// </summary>
	public class NewsItemAdsBlocker: IChannelProcessor
	{
	    #region IChannelProcessor Members

		public INewsChannel[] GetChannels() {
			return new INewsChannel[] {new AdsBlockerChannel()};
		}

		#endregion
	}
}
