#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

namespace RssBandit.AppServices
{
	/// <summary>
	/// Feed sources interface
	/// </summary>
	public interface IFeedSources
	{
		/// <summary>
		/// Determines whether the feed source specified by name is already used (true), otherwise false.
		/// </summary>
		/// <param name="name">The feed source name.</param>
		/// <returns></returns>
		bool Contains(string name);
	}
}
