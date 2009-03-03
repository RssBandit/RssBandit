#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System.Collections.Generic;


namespace RssBandit
{
	/// <summary>
	/// Implemented by concrete service/interface dependency resolve instances
	/// </summary>
	public interface IServiceDependencyResolver
	{
		/// <summary>
		/// Resolves the concrete service instance implementing the requested interface.
		/// </summary>
		/// <typeparam name="T">The service interface</typeparam>
		/// <returns></returns>
		T Resolve<T>();
		/// <summary>
		/// Resolves all concrete service instances implementing the requested interface.
		/// </summary>
		/// <typeparam name="T">The service interface</typeparam>
		/// <returns></returns>
		IEnumerable<T> ResolveAll<T>();
	}
}
