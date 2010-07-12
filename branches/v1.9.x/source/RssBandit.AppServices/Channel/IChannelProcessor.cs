#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
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
