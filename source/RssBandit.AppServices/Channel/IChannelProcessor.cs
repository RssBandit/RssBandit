#region CVS Version Header
/*
 * $Id: IChannelProcessor.cs,v 1.2 2005/10/08 18:21:00 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/10/08 18:21:00 $
 * $Revision: 1.2 $
 */
#endregion

using System;

namespace NewsComponents
{
	/// <summary>
	/// Base interface for News Channel Processing: IChannelProcessor.
	/// </summary>
	public interface IChannelProcessor
	{
		/// <summary>
		/// Provides the list of available news processing channels.
		/// </summary>
		/// <returns></returns>
		INewsChannel[] GetChannels();
	}


}
