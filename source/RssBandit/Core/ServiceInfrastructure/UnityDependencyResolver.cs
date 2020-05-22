using System;
using System.Collections.Generic;

using Unity;
using Unity.Lifetime;

namespace RssBandit
{
	/// <summary>
	/// Implements <see cref="IServiceDependencyResolver"/> and 
	/// <see cref="IServiceDependencyContainer"/> with help of 
	/// <see cref="IUnityContainer"/>.
	/// </summary>
	public class UnityDependencyResolver : IServiceDependencyResolver, IServiceDependencyContainer,
		IServiceProvider, IDisposable
	{
		private readonly IUnityContainer _container;

		/// <summary>
		/// Initializes a new instance of the <see cref="UnityDependencyResolver"/> class.
		/// </summary>
		public UnityDependencyResolver()
		{
			_container = new UnityContainer();
			_container.RegisterInstance(typeof(IServiceDependencyContainer), this, new ContainerControlledLifetimeManager());
			_container.RegisterInstance(typeof(IServiceDependencyResolver), this, new ContainerControlledLifetimeManager());
			_container.RegisterInstance(typeof(IServiceProvider), this, new ContainerControlledLifetimeManager());
		}

		#region IServiceDependencyResolver

		/// <summary>
		/// Resolves the concrete service instance implementing the requested interface.
		/// </summary>
		/// <typeparam name="T">The service interface</typeparam>
		/// <returns>A concrete service instance implementing the requested interface</returns>
		public T Resolve<T>()
		{
			return _container.Resolve<T>();
		}

		/// <summary>
		/// Try to resolve a concrete service instance implementing the requested interface.
		/// </summary>
		/// <typeparam name="T">The service interface</typeparam>
		/// <returns>True, in case the instance could be provided, else false</returns>
		public IEnumerable<T> ResolveAll<T>()
		{
			return _container.ResolveAll<T>();
		}

		#endregion

		#region IServiceDependencyContainer

		/// <summary>
		/// Registers the specified object as a service implementation for the concrete
		/// interface specified in the type parameter T.
		/// </summary>
		/// <typeparam name="T">The service interface</typeparam>
		/// <param name="obj">The implementor.</param>
		public void Register<T>(object obj) 
		{
			_container.RegisterInstance(typeof(T), obj);
		}

		/// <summary>
		/// Not implemented (conceptual we should use child containers instead).
		/// This feature needs some rework of the similar class 
		/// <see cref="ServiceProviderDependencyResolver"/> impl. to fit the same
		/// concept (work in progress - TR!!!)
		/// </summary>
		/// <typeparam name="T">The service interface</typeparam>
		/// <param name="obj">The implementor (optional, can be null).</param>
		public void Unregister<T>(object obj) 
		{
			// if you need such behavior, use ChildContainer and HierachicalLifeTimeManager provided by unity.
			
			//throw new NotImplementedException();
		}

		#endregion

		#region IServiceProvider

		public object GetService(Type serviceType)
		{
			return _container.Resolve(serviceType);
		}

		#endregion

		#region Implementation of IDisposable

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_container != null)
					_container.Dispose();
			}
		}
		///<summary>
		///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		///</summary>
		///<filterpriority>2</filterpriority>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
