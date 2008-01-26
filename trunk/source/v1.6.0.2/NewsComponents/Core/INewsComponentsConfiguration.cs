#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.IO;
using log4net;
using Microsoft.Win32;

using NewsComponents.Storage;
using Logger = RssBandit.Common.Logging;

namespace NewsComponents
{
	#region INewsComponentsConfiguration interface

	/// <summary>
	/// INewsComponentsConfiguration provides the relevant configuration
	/// information required to run NewsHandler.
	/// </summary>
	public interface INewsComponentsConfiguration
	{
		
		/// <summary>
		/// Gets the application ID. This will be used e.g. to build
		/// the relative path below the user's appdata (UserApplicationDataPath)
		/// or local appdata (UserLocalApplicationDataPath) paths.
		/// </summary>
		/// <remarks>Required</remarks>
		/// <value>The application ID.</value>
		string ApplicationID { get; }
		
		/// <summary>
		/// Gets the windows user application data path (roaming one).
		/// </summary>
		/// <remarks>Required</remarks>
		/// <value>The windows user application path.</value>
		string UserApplicationDataPath { get; }
		
		/// <summary>
		/// Gets the windows user local application data path (non-roaming).
		/// </summary>
		/// <remarks>Required</remarks>
		/// <value>The windows user local application path.</value>
		string UserLocalApplicationDataPath { get; }
		
		/// <summary>
		/// Gets the downloaded files data path. Usually a folder 
		/// located below user's Documents.
		/// </summary>
		/// <remarks>Optional. Set to null to prevent initializing the 
		/// BackgroundDownloadManager.</remarks>
		/// <value>The downloaded files data path.</value>
		string DownloadedFilesDataPath { get; }
		
		/// <summary>
		/// Gets a persisted settings implementation.
		/// </summary>
		/// <remarks>Required</remarks>
		/// <value>The persisted settings.</value>
		IPersistedSettings PersistedSettings { get; }
		
		/// <summary>
		/// Gets the cache manager.
		/// </summary>
		/// <remarks>Required</remarks>
		/// <value>The cache manager.</value>
		CacheManager CacheManager { get; }

		/// <summary>
		/// Gets the search index behavior.
		/// </summary>
		/// <remarks>Required</remarks>
		/// <value>The search index behavior.</value>
		SearchIndexBehavior SearchIndexBehavior { get; }
		
	}

	#endregion
	
	#region IPersistedSettings interface

	/// <summary>
	/// Defines the interface to a permanent/persisted settings storage impl.
	/// Permanent/persisted means: the settings must be available for multiple
	/// (sequential) running application sessions.
	/// </summary>
	public interface IPersistedSettings
	{
		/// <summary>
		/// Gets the property value.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="returnType">Type of the return.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		object GetProperty(string name, Type returnType, object defaultValue);
		
		/// <summary>
		/// Sets the property value.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		void SetProperty(string name, object value);
	}

	#endregion

	#region SearchIndexBehavior enum

	/// <summary>
	/// Defines the search indexing storage behavior options
	/// </summary>
	public enum SearchIndexBehavior
	{
		/// <summary>
		/// No index will be built
		/// </summary>
		NoIndexing = 0,
		/// <summary>
		/// Index is directory based and build relative to
		/// the user's local application data folder
		/// </summary>
		LocalAppDataDirectoryBased = 1,
		/// <summary>
		/// Index is directory based and build relative to
		/// the user's application data folder
		/// </summary>
		AppDataDirectoryBased = 2,
		/// <summary>
		/// Index is directory based and build relative to
		/// the user's temporary folder
		/// </summary>
		TempDirectoryBased = 3,

#if DEBUG		
// TR: currently not supported to release, causing errors!		
		
		/// <summary>
		/// Index is built in memory 
		/// </summary>
		RAMDirectoryBased = 4,
#endif	
		
		/// <summary>
		/// Defindes the default indexing behavior: LocalAppDataDirectoryBased
		/// </summary>
		Default = LocalAppDataDirectoryBased
	}

	#endregion

	#region NewsComponentsConfiguration default impl. class
	/// <summary>
	/// Provides a default implementation of INewsComponentsConfiguration
	/// </summary>
	public class NewsComponentsConfiguration: INewsComponentsConfiguration 
	{
		/// <summary>
		/// Gets the default configuration instance
		/// </summary>
		public static INewsComponentsConfiguration Default = CreateDefaultConfiguration();
		
		const string defaultApplicationID = "NewsComponents";
		
		protected string appID = null;
		protected string applicationDataPath = null;
		protected string applicationLocalDataPath = null;
		protected string applicationDownloadPath = null;
		protected SearchIndexBehavior searchBehavior = NewsComponents.SearchIndexBehavior.Default;
		protected IPersistedSettings settings = null;
		protected CacheManager p_cacheManager = null;

		#region INewsComponentsConfiguration Members

		/// <summary>
		/// Gets/Sets the application ID. This will be used e.g. to build
		/// the relative path below the user's appdata (UserApplicationDataPath)
		/// or local appdata (UserLocalApplicationDataPath) paths.
		/// </summary>
		/// <value>The application ID.</value>
		public virtual string ApplicationID {
			get { return appID; }
			set { appID = value;}
		}

		/// <summary>
		/// Gets the windows user application data path (roaming one).
		/// </summary>
		/// <value>The windows user application path.</value>
		public virtual string UserApplicationDataPath {
			get {
				if (applicationDataPath == null)
					applicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), this.ApplicationID);
				return applicationDataPath;
			}
			set {
				applicationDataPath = value;
			}
		}

		/// <summary>
		/// Gets the windows user local application data path (non-roaming).
		/// </summary>
		/// <value>The windows user local application path.</value>
		public virtual string UserLocalApplicationDataPath {
			get {
				if (applicationLocalDataPath == null)
					applicationLocalDataPath= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), this.ApplicationID);
				return applicationLocalDataPath;
			}
			set {
				applicationLocalDataPath = value;
			}
		}

		/// <summary>
		/// Gets the downloaded files data path. Usually a folder
		/// located below user's Documents.
		/// </summary>
		/// <value>The downloaded files data path.</value>
		public virtual string DownloadedFilesDataPath {
			get { return applicationDownloadPath; }
			set { applicationDownloadPath = value; }
		}

		/// <summary>
		/// Gets a persisted settings implementation.
		/// </summary>
		/// <value>The persisted settings.</value>
		public virtual IPersistedSettings PersistedSettings {
			get { return settings; } 
			set { settings = value;}
		}

		/// <summary>
		/// Gets the cache manager.
		/// </summary>
		/// <value>The cache manager.</value>
		public virtual CacheManager CacheManager {
			get { return p_cacheManager; }
			set { p_cacheManager = value; }
		}
		/// <summary>
		/// Gets the search index behavior.
		/// </summary>
		/// <value>The search index behavior.</value>
		public virtual SearchIndexBehavior SearchIndexBehavior {
			get { return searchBehavior; }
			set { searchBehavior = value; }
		}

		#endregion
		
		/// <summary>
		/// Creates the default configuration.
		/// </summary>
		/// <returns></returns>
		private static INewsComponentsConfiguration CreateDefaultConfiguration() {
			NewsComponentsConfiguration cfg = new NewsComponentsConfiguration();
			cfg.ApplicationID = defaultApplicationID;
			
			cfg.SearchIndexBehavior = SearchIndexBehavior.Default;
			cfg.UserApplicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), cfg.ApplicationID);
			cfg.UserLocalApplicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), cfg.ApplicationID);

			string mydocs = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), cfg.ApplicationID);
			cfg.DownloadedFilesDataPath = Path.Combine(mydocs, "My Downloaded Files");
			
			string path = Path.Combine(cfg.UserApplicationDataPath, "Cache");
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			cfg.CacheManager = new FileCacheManager(path);
			
			cfg.PersistedSettings = new SettingStore(cfg.ApplicationID);
			return cfg;
		}
		
		#region PersistedSettings impl.
		class SettingStore: IPersistedSettings 
		{
			private readonly string settingsRoot;
			private static readonly ILog _log = Logger.Log.GetLogger(typeof(SettingStore));

			
			public SettingStore(string appID) {
				this.settingsRoot = String.Format(@"Software\{0}\Settings", appID);
			}
			
			#region IPersistedSettings Members

			public object GetProperty(string name, Type returnType, object defaultValue) {
				RegistryKey key = null;
				try {
					key = Registry.CurrentUser.OpenSubKey(settingsRoot, false);
					if (key == null)
						return defaultValue;
					
					object val = key.GetValue(name, defaultValue);
					if (val != null) {
						try {
							return Convert.ChangeType(val, returnType);
						} catch {}
					}
					
					return defaultValue;
					
				} catch (Exception ex) {
					_log.Error("Failed to read value of '"+name+"' from registry hive '" + settingsRoot + "'.", ex);
					return defaultValue;
				} finally {
					if (key != null) key.Close();
				}
			}

			public void SetProperty(string name, object value) {
				try {
					RegistryKey keySettings = Registry.CurrentUser.OpenSubKey(settingsRoot, true);
					if (keySettings == null) {
						keySettings = Registry.CurrentUser.CreateSubKey(settingsRoot);
					}
					keySettings.SetValue(name, value);
					keySettings.Close();
				} catch (Exception) {}
				
			}

			#endregion
		}
		#endregion
	}
	#endregion
}

#region CVS Version Log
/*
 * $Log: INewsComponentsConfiguration.cs,v $
 * Revision 1.1  2007/07/21 12:26:21  t_rendelmann
 * added support for "portable Bandit" version
 *
 */
#endregion

