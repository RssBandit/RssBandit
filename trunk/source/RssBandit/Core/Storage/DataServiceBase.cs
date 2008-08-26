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
using System.IO;
using RssBandit.WinGui;

namespace RssBandit.Core.Storage
{
	internal abstract class DataServiceBase : IInitDataService,
		IUserCacheDataService, IUserDataService, IUserRoamingDataService
	{
		#region Implementation of IDisposable

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region Implementation of IInitDataService

		/// <summary>
		/// The init data
		/// </summary>
		protected string initializationData;

		/// <summary>
		/// Initializes the data service with the specified initialization data.
		/// </summary>
		/// <param name="initData">The initialization data.
		/// Can be a connection string, or a file path;
		/// depending on the implementation of the data service</param>
		public virtual void Initialize(string initData)
		{
			initializationData = initData;
		}

		#endregion

		#region Implementation of IUserDataService

		public abstract FeedColumnLayoutCollection LoadColumnLayouts();
		public abstract void SaveColumnLayouts(FeedColumnLayoutCollection layouts);
		public abstract string[] GetUserDataFileNames();
		public abstract DataEntityName SetContentForDataFile(string dataFileName, Stream content);

		#endregion
	}
}
