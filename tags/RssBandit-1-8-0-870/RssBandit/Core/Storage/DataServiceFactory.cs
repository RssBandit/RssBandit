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
using System.Diagnostics;
using System.IO;


namespace RssBandit.Core.Storage
{
	internal enum StorageDomain
	{
		UserData,
		UserRoamingData,
		UserCacheData,
	}

	/// <summary>
	/// Used to create data services. 
	/// can be extended later on to read the storage provider class
	/// and init parameters from a config file. For now it offers the
	/// FileStorageDataService only, configured using RssBanditApplication's
	/// global static path functions.
	/// </summary>
	internal static class DataServiceFactory
	{

		public static void Configure(IInitDataService dataService, StorageDomain domain)
		{
			if (dataService == null)
				throw new ArgumentNullException("dataService");

			if (domain == StorageDomain.UserCacheData)
			{
				dataService.Initialize(Path.Combine(
						RssBanditApplication.GetLocalUserPath(),
						"Cache"));
			}
			else

				if (domain == StorageDomain.UserData)
				{
					dataService.Initialize(
						RssBanditApplication.GetUserPersonalPath());
				}
				else

					if (domain == StorageDomain.UserRoamingData)
					{
						dataService.Initialize(
							RssBanditApplication.GetUserPath());
					}
					else

						Debug.Assert(false, "No data service for StorageDomain: " + domain);
		}

		/// <summary>
		/// Gets the required per storage domain data service.
		/// </summary>
		/// <param name="domain">The domain.</param>
		/// <returns></returns>
		public static object GetService(StorageDomain domain)
		{
			IInitDataService service = new FileStorageDataService();
			if (domain == StorageDomain.UserCacheData)
			{
				service.Initialize(Path.Combine(
						RssBanditApplication.GetLocalUserPath(),
						"Cache"));
			}
			else

				if (domain == StorageDomain.UserData)
				{
					service.Initialize(
						RssBanditApplication.GetUserPersonalPath());
				}
				else

					if (domain == StorageDomain.UserRoamingData)
					{
						service.Initialize(
							RssBanditApplication.GetUserPath());
					}
					else

						Debug.Assert(false, "No data service for StorageDomain: " + domain);

			return service;
		}
	}
}
