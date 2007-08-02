#region CVS Version Header
/*
 * $Id: IChannelProcessor.cs,v 1.1 2005/05/08 17:03:39 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/05/08 17:03:39 $
 * $Revision: 1.1 $
 */
#endregion

using System;

namespace NewsComponents
{
	/// <summary>
	/// Base plugin interface for News Channel Processing: IChannelProcessor.
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
