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
	/// The Service dependency inversion container class
	/// </summary>
	public class IoC
	{
		private static IServiceDependencyResolver s_Inner;

		/// <summary>
		/// Initializes the instance with the specified resolver.
		/// </summary>
		/// <param name="resolver">The resolver.</param>
		public static void Initialize(IServiceDependencyResolver resolver)
		{
			s_Inner = resolver;
		}

		/// <summary>
		/// Resolves the concrete service instance implementing the requested interface.
		/// </summary>
		/// <typeparam name="T">The service interface</typeparam>
		/// <returns></returns>
		public static T Resolve<T>()
		{
			if (s_Inner != null) 
				return s_Inner.Resolve<T>();
			return default(T);
		}

		/// <summary>
		/// Resolves all concrete service instances implementing the requested interface.
		/// </summary>
		/// <typeparam name="T">The service interface</typeparam>
		/// <returns></returns>
		public static IEnumerable<T> ResolveAll<T>()
		{
			if (s_Inner != null) 
				return s_Inner.ResolveAll<T>();
			return new List<T>(0);
		}
	}
}
