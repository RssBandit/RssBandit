#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Collections;
using System.Reflection;
using System.Threading;

namespace NewsComponents
{

	/// <summary>
	/// NewsChannelServices class is to be used to (un)register INewsChannels.
	/// </summary>
	public class NewsChannelServices
	{
		private static readonly log4net.ILog Log = RssBandit.Common.Logging.DefaultLog.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly SortedList _newsItemChannels = new SortedList(new ChannelComparer());
		private readonly SortedList _feedChannels = new SortedList(new ChannelComparer());
		private readonly ReaderWriterLock _newsItemChannelsLock = new ReaderWriterLock();
		private readonly ReaderWriterLock _feedChannelsLock = new ReaderWriterLock();

		/// <summary>
		/// Registers the news channel.
		/// </summary>
		/// <param name="channel">The channel.</param>
		public void RegisterNewsChannel(INewsChannel channel)
		{
			if (channel == null)
				throw new ArgumentNullException("channel");
			if (channel.ChannelProcessingType == ChannelProcessingType.NewsItem) {
				try {
					_newsItemChannelsLock.AcquireWriterLock(0);
					try {
						_newsItemChannels.Add(channel, channel);
					} finally {
						_newsItemChannelsLock.ReleaseWriterLock();
					}
				} catch (ApplicationException) {
					// lock timeout
				}
			} else if (channel.ChannelProcessingType == ChannelProcessingType.Feed) {
				try {
					_feedChannelsLock.AcquireWriterLock(0);
					try {
					_feedChannels.Add(channel, channel);
					} finally {
						_feedChannelsLock.ReleaseWriterLock();
					}
				} catch (ApplicationException) {
					// lock timeout
				}
			} else
				throw new NotSupportedException("The channel processing type is not yet supported.");

		}

		/// <summary>
		/// Unregisters the news channel.
		/// </summary>
		/// <param name="channel">The channel.</param>
		public void UnregisterNewsChannel(INewsChannel channel) {
			if (channel == null)
				throw new ArgumentNullException("channel");
			if (channel.ChannelProcessingType == ChannelProcessingType.NewsItem) {
				try {
					_newsItemChannelsLock.AcquireWriterLock(0);
					try {
					if (_newsItemChannels.ContainsKey(channel))
						_newsItemChannels.Remove(channel);
					} finally {
						_newsItemChannelsLock.ReleaseWriterLock();
					}
				} catch (ApplicationException) {
					// lock timeout
				}
			} else if (channel.ChannelProcessingType == ChannelProcessingType.Feed) {
				try {
					_feedChannelsLock.AcquireWriterLock(0);
					try {
					if (_feedChannels.ContainsKey(channel))
						_feedChannels.Remove(channel);
					} finally {
						_feedChannelsLock.ReleaseWriterLock();
					}
				} catch (ApplicationException) {
					// lock timeout
				}
			} else
				throw new NotSupportedException("The channel processing type is not yet supported.");

		}

		/// <summary>
		/// Processes the item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public INewsItem ProcessItem(INewsItem item)
		{
			try {
				_newsItemChannelsLock.AcquireReaderLock(0);
				try {
					if (_newsItemChannels.Count == 0)
						return item;
			
					// process in prio order:
					foreach (NewsItemChannelBase sink in _newsItemChannels.Values){
						try {
							item = sink.Process(item);
						} catch (Exception ex) {
							Log.Error("NewsChannelServices.ProcessItem() sink '"+(sink != null ? sink.ChannelName : "[ChannelName]?") +"' failed to process INewsItem", ex);
						}
					}

				} finally {
					_newsItemChannelsLock.ReleaseReaderLock();	
				}
			} catch (ApplicationException) {
				// lock timeout	
			} catch (Exception ex) {
				Log.Error("Failed to process INewsItem.", ex);
			}
			return item;
		}

		/// <summary>
		/// Processes the item.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public IFeedDetails ProcessItem(IFeedDetails item) {
			try {
				_feedChannelsLock.AcquireReaderLock(0);
				try {
					if (_feedChannels.Count == 0)
						return item;

					// process in prio order:
					foreach (FeedChannelBase sink in _feedChannels.Values){
						item = sink.Process(item);
					}
				} finally {
					_feedChannelsLock.ReleaseReaderLock();	
				}
			} catch (ApplicationException) {
				// lock timeout	
			} catch (Exception ex) {
				Log.Error("Failed to process IFeedDetails.", ex);
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
					return String.Compare(lhsX.ChannelName, rhsY.ChannelName);
				
				return (lhsX.ChannelPriority < rhsY.ChannelPriority ? -1: 1);
			}

			#endregion

		}

	}

}
