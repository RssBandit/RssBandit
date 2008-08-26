using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RssBandit.WinGui;

namespace RssBandit.Core.Storage
{
	/// <summary>
	/// Defines used entity names
	/// </summary>
	internal enum DataEntityName
	{
		/// <summary>
		/// 
		/// </summary>
		None,
		/// <summary>
		/// 
		/// </summary>
		ColumnLayoutDefinitions,
	}

	#region IoC resolvable marker data interfaces

	/// <summary>
	/// Cache Data Service interface.
	/// Inherits <see cref="IDisposable"/> interface to cleanup any used database
	/// connection or other resource(s)
	/// </summary>
	internal interface IUserCacheDataService : IClientDataService
	{
		// marker interface to get a configuration aware data service 
		// writing to/reading from user cache folder
	}

	/// <summary>
	/// User Data Storage services.
	/// Inherits IDisposable interface to cleanup any used database
	/// connection or other resource(s)
	/// </summary>
	internal interface IUserDataService : IClientDataService
	{
		// marker interface to get a configuration aware data service 
		// writing to/reading from personal user folder		
	}

	/// <summary>
	/// Roaming Data Storage services.
	/// Inherits IDisposable interface to cleanup any used database
	/// connection or other resource(s)
	/// </summary>
	internal interface IUserRoamingDataService : IClientDataService
	{
		// marker interface to get a configuration aware data service 
		// writing to/reading from user roaming folder		
	}

	#endregion

	/// <summary>
	/// Real client data storage services.
	/// Inherits IDisposable interface to cleanup any used database
	/// connection or other resource(s)
	/// </summary>
	internal interface IClientDataService : IDisposable
	{
		// interface to store data into the windows user roaming data path
		#region item column display Layout

		/// <summary>
		/// Loads the column layouts.
		/// </summary>
		/// <returns></returns>
		FeedColumnLayoutCollection LoadColumnLayouts();

		/// <summary>
		/// Saves the column layouts.
		/// </summary>
		/// <param name="layouts">The layouts.</param>
		void SaveColumnLayouts(FeedColumnLayoutCollection layouts);

		#endregion

		/// <summary>
		/// Gets the used user data file names.
		/// </summary>
		/// <returns></returns>
		string[] GetUserDataFileNames();

		/// <summary>
		/// Sets the content for a data file.
		/// </summary>
		/// <param name="dataFileName">Name of the data file.</param>
		/// <param name="content">The content.</param>
		/// <returns></returns>
		DataEntityName SetContentForDataFile(string dataFileName, Stream content);
	}
}
