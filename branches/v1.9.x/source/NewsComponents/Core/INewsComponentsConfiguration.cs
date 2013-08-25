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
using System.ComponentModel;
using System.IO;
using log4net;
using Microsoft.Win32;

using NewsComponents.Utils;
using Logger = RssBandit.Common.Logging;

namespace NewsComponents
{
	#region INewsComponentsConfiguration interface

	/// <summary>
	/// INewsComponentsConfiguration provides the relevant configuration
	/// information required to run FeedSource.
	/// </summary>
	public interface INewsComponentsConfiguration : INotifyPropertyChanged
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
		/// Gets the application version. E.g. used to assemble the user agent string.
		/// </summary>
		/// <value>The application version.</value>
		Version ApplicationVersion { get; }
		
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
		/// Gets the search index behavior.
		/// </summary>
		/// <remarks>Required</remarks>
		/// <value>The search index behavior.</value>
		SearchIndexBehavior SearchIndexBehavior { get; }

		/// <summary>
		/// Gets the refresh rate in millisecs.
		/// </summary>
		/// <value>The refresh rate.</value>
		int RefreshRate { get; }

		/// <summary>
		/// Gets a value that control if enclosures should be downloaded
		/// </summary>
		/// <value><c>true</c> if [download enclosures]; otherwise, <c>false</c>.</value>
		bool DownloadEnclosures { get; }
		
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
		/// <summary/>
		protected string appID;
		/// <summary/>
		protected Version appVersion = new Version(1, 0);
		/// <summary/>
		protected string applicationDataPath;
		/// <summary/>
		protected string applicationLocalDataPath;
		/// <summary/>
		protected string applicationDownloadPath;
		/// <summary/>
		protected SearchIndexBehavior searchBehavior = SearchIndexBehavior.Default;
		/// <summary/>
		protected IPersistedSettings settings;
		
		/// <summary/>
		protected int p_refreshRate = -1;
		private bool downloadEnclosures;
		
		#region INewsComponentsConfiguration Members

		/// <summary>
		/// Gets/Sets the application ID. This will be used e.g. to build
		/// the relative path below the user's appdata (UserApplicationDataPath)
		/// or local appdata (UserLocalApplicationDataPath) paths.
		/// </summary>
		/// <value>The application ID.</value>
		public virtual string ApplicationID {
			get { return appID; }
			set { 
				appID = value;
				this.OnPropertyChanged("ApplicationID");
			}
		}

		/// <summary>
		/// Gets the application version. E.g. used to assemble the user agent string.
		/// </summary>
		/// <value>The application version.</value>
		public virtual Version ApplicationVersion
		{
			get { return appVersion; }
			set {
				appVersion = value;
				this.OnPropertyChanged("ApplicationVersion");
			}
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
				this.OnPropertyChanged("UserApplicationDataPath");
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
				this.OnPropertyChanged("UserLocalApplicationDataPath");
			}
		}

		/// <summary>
		/// Gets the downloaded files data path. Usually a folder
		/// located below user's Documents.
		/// </summary>
		/// <value>The downloaded files data path.</value>
		public virtual string DownloadedFilesDataPath {
			get { return applicationDownloadPath; }
			set {
				applicationDownloadPath = value;
				this.OnPropertyChanged("DownloadedFilesDataPath");
			}
		}

		/// <summary>
		/// Gets a persisted settings implementation.
		/// </summary>
		/// <value>The persisted settings.</value>
		public virtual IPersistedSettings PersistedSettings {
			get { return settings; } 
			set {
				settings = value;
				this.OnPropertyChanged("PersistedSettings");
			}
		}

		/// <summary>
		/// Gets the search index behavior.
		/// </summary>
		/// <value>The search index behavior.</value>
		public virtual SearchIndexBehavior SearchIndexBehavior {
			get { return searchBehavior; }
			set {
				searchBehavior = value;
				this.OnPropertyChanged("SearchIndexBehavior");
			}
		}

		/// <summary>
		/// Gets the refresh rate in millisecs.
		/// </summary>
		/// <value>The refresh rate.</value>
		public virtual int RefreshRate {
			get {
				if (p_refreshRate >= 0)
					return p_refreshRate;
				return FeedSource.DefaultRefreshRate;
			}
			set {
				p_refreshRate = value;
				this.OnPropertyChanged("RefreshRate");
			}
		}

		/// <summary>
		/// Gets a value that control if enclosures should be downloaded
		/// </summary>
		/// <value><c>true</c> if [download enclosures]; otherwise, <c>false</c>.</value>
		public virtual bool DownloadEnclosures {
			get { return downloadEnclosures; }
			set { 
				downloadEnclosures = value;
				this.OnPropertyChanged("DownloadEnclosures");
			}
		}

		#endregion
		
		/// <summary>
		/// Creates the default configuration.
		/// </summary>
		/// <returns></returns>
		private static INewsComponentsConfiguration CreateDefaultConfiguration() 
		{
			NewsComponentsConfiguration cfg = new NewsComponentsConfiguration();
			cfg.ApplicationID = defaultApplicationID;
			
			cfg.SearchIndexBehavior = SearchIndexBehavior.Default;
			cfg.UserApplicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), cfg.ApplicationID);
			cfg.UserLocalApplicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), cfg.ApplicationID);

			string mydocs = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), cfg.ApplicationID);
			cfg.DownloadedFilesDataPath = Path.Combine(mydocs, "My Downloaded Files");

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

		#region INotifyPropertyChanged Members

		///<summary>
		///Occurs when a property value changes.
		///</summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Fired whenever a property is changed. 
		/// </summary>
		/// <param name="propertyName"></param>
		protected virtual void OnPropertyChanged(string propertyName)
		{
			OnPropertyChanged(DataBindingHelper.GetPropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Notifies listeners that a property has changed. 
		/// </summary>
		/// <param name="e">Details on the property change event</param>
		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (null != PropertyChanged)
			{
				PropertyChanged(this, e);
			}
		}

		#endregion
	}
	#endregion
}

