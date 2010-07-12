#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

namespace RssBandit
{
	/// <summary>
	/// Implemented by concrete service/interface dependency resolve instances
	/// to all programmatic add/remove of service implementations.
	/// </summary>
	public interface IServiceDependencyContainer
	{
		/// <summary>
		/// Registers the specified object as a service implementation for the concrete
		/// interface specified in the type parameter T.
		/// </summary>
		/// <typeparam name="T">The service interface</typeparam>
		/// <param name="obj">The implementor.</param>
		void Register<T>(object obj);

		/// <summary>
		/// Unregisters the specified object as a service implementation for the concrete
		/// interface specified in the type parameter T. If <paramref name="obj"/> is not null,
		/// it checks for the registered service implementor if the references are equal
		/// and unregister only in case this test gets true. If the parameter is null,
		/// the implementor will get unregistered no matter if it was the only one.
		/// </summary>
		/// <typeparam name="T">The service interface</typeparam>
		/// <param name="obj">The implementor (optional, can be null).</param>
		void Unregister<T>(object obj);

	}
}
