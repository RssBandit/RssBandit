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
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace RssBandit
{
	/// <summary>
	/// Implements <see cref="IServiceDependencyResolver"/> and 
	/// <see cref="IServiceDependencyContainer"/> with help of 
	/// <see cref="ServiceContainer"/>.
	/// </summary>
	/// <remarks>It add itself as a <see cref="IServiceDependencyContainer"/> service instance!</remarks>
	public class ServiceProviderDependencyResolver:
		IServiceDependencyResolver, IServiceDependencyContainer, IDisposable
	{
		private readonly ServiceContainer m_Types = new ServiceContainer();

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProviderDependencyResolver"/> class.
		/// </summary>
		public ServiceProviderDependencyResolver() {
			m_Types = new ServiceContainer();
			m_Types.AddService(typeof(IServiceDependencyContainer), this);
			m_Types.AddService(typeof(IServiceProvider), m_Types);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProviderDependencyResolver"/> class.
		/// </summary>
		/// <param name="parent">The parent.</param>
		public ServiceProviderDependencyResolver(IServiceProvider parent) {
			m_Types = new ServiceContainer(parent);
			m_Types.AddService(typeof(IServiceDependencyContainer), this);
			m_Types.AddService(typeof(IServiceProvider), m_Types);
		}

		/// <summary>
		/// Resolves the concrete service instance implementing the requested interface.
		/// </summary>
		/// <typeparam name="T">The service interface</typeparam>
		/// <returns></returns>
		public T Resolve<T>()
		{
			return (T)m_Types.GetService(typeof(T));
		}

		/// <summary>
		/// Resolves all concrete service instances implementing the requested interface.
		/// </summary>
		/// <typeparam name="T">The service interface</typeparam>
		/// <returns></returns>
		public IEnumerable<T> ResolveAll<T>()
		{
			// N.B. A dictionary can only hold one instance object for each type T
			yield return (T)m_Types.GetService(typeof(T));
		}

		/// <summary>
		/// Registers the specified object as a service implementation for the concrete
		/// interface specified in the typeparam T.
		/// </summary>
		/// <typeparam name="T">The service interface</typeparam>
		/// <param name="obj">The implementor.</param>
		public void Register<T>(object obj)
		{
			m_Types.AddService(typeof(T), obj);
		}

		/// <summary>
		/// Unregisters the specified object as a service implementation for the concrete
		/// interface specified in the typeparam T. If <paramref name="obj"/> is not null,
		/// it checks for the registered service implementor if the references are equal
		/// and unregister only in case this test gets true. If the parameter is null,
		/// the implementor will get unregistered no matter if it was the only one.
		/// </summary>
		/// <typeparam name="T">The service interface</typeparam>
		/// <param name="obj">The implementor (optional, can be null).</param>
		public void Unregister<T>(object obj)
		{
			if (obj != null) {
				T registered = Resolve<T>();
				if (registered != null && ReferenceEquals(registered, obj))
					m_Types.RemoveService(typeof(T));
				return;
			}
			m_Types.RemoveService(typeof(T));
		}

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing)
			{
				if (m_Types != null)
					m_Types.Dispose();
			}
 		}
		///<summary>
		///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		///</summary>
		///<filterpriority>2</filterpriority>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
