#region CVS Version Header
/*
 * $Id: INewsChannel.cs,v 1.1 2005/03/04 16:42:35 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/03/04 16:42:35 $
 * $Revision: 1.1 $
 */
#endregion

using System;

namespace NewsComponents
{
	public interface INewsChannel
	{
		/// <summary>
		/// Gets a channel name. This have to be a unique one, so you may use a Url/Uri scheme.
		/// </summary>
		string ChannelName { get ;}
		/// <summary>
		/// Gets the channel priority. Used/Required to determine the order of processing
		/// if multiple channels are registered for one type of processing.
		/// </summary>
		int ChannelPriority { get ;}
		/// <summary>
		/// Type of processing the channel is used for.
		/// </summary>
		ChannelProcessingType ChannelProcessingType { get; }
	}
}