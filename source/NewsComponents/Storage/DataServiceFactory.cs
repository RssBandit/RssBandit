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

namespace NewsComponents.Storage
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
	/// FileStorageDataService only, configured using INewsComponentsConfiguration
	/// provided folder settings.
	/// </summary>
	internal static class DataServiceFactory
	{
		/// <summary>
		/// Gets the required per storage domain data service.
		/// </summary>
		/// <param name="domain">The domain.</param>
		/// <param name="configuration">The configuration.</param>
		/// <returns></returns>
		public static object GetService(StorageDomain domain, INewsComponentsConfiguration configuration)
		{
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            IInitDataService service = new FileStorageDataService();
			if (domain == StorageDomain.UserCacheData)
			{
				service.Initialize(Path.Combine(
						configuration.UserLocalApplicationDataPath,
						"Cache"));
			} else
			
			if (domain == StorageDomain.UserData)
			{
				service.Initialize(
					configuration.UserApplicationDataPath);
			} else 
			
			if (domain == StorageDomain.UserRoamingData)
			{
				service.Initialize(
					configuration.UserLocalApplicationDataPath);
			} else
			
				Debug.Assert(false, "No data service for StorageDomain: " + domain);
			
			return service;
		}
	}
}
