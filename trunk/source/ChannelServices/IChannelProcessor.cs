#region CVS Version Header
/*
 * $Id: IChannelProcessor.cs,v 1.1 2005/03/04 16:42:35 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/03/04 16:42:35 $
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
