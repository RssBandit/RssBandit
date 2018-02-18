using System;
using System.Collections;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Schema;
using Microsoft.ApplicationBlocks.ExceptionManagement;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Net;
using NewsComponents.Utils;
using RssBandit.Resources;
using RssBandit.WinGui.Forms;

namespace RssBandit
{
    internal partial class RssBanditApplication
    {
        #region static class routines

		private static readonly object SharedCultureLock = new object();
		private static readonly object SharedUiCultureLock = new object();

        /// <summary>
        /// Gets or sets the shared UI culture.
        /// </summary>
        /// <value>The shared UI culture.</value>
        public static CultureInfo SharedUICulture
        {
            get
            {
                return sharedUICulture;
            }
            set
            {
				lock (SharedUiCultureLock)
                {
                    sharedUICulture = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the shared culture.
        /// </summary>
        /// <value>The shared culture.</value>
        public static CultureInfo SharedCulture
        {
            get
            {
                return sharedCulture;
            }
            set
            {
                lock (SharedCultureLock)
                {
                    sharedCulture = value;
                }
            }
        }

        /// <summary>
        /// Gets the version (no version postfix).
        /// </summary>
        /// <value>The version.</value>
        public static Version Version
        {
            get
            {
                if (appVersion == null)
                {
                    appVersion = new Version(typeof(RssBanditApplication).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version);
                }
                return appVersion;
            }
        }

        /// <summary>
        /// Gets the version (long format, incl. version postfix).
        /// </summary>
        /// <value>The version string.</value>
        public static string VersionLong
        {
            get
            {
                if (appVersionLong == null)
                {
                    appVersionLong = typeof(RssBanditApplication).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                }
                return appVersionLong;
            }
        }
        

        /// <summary>
        /// Gets the application infos.
        /// </summary>
        /// <value>The application infos.</value>
        public static string ApplicationInfos
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0};UI:{1};", Name, Thread.CurrentThread.CurrentUICulture.Name);
                try
                {
                    sb.AppendFormat("OS:{0},", Environment.OSVersion);
                }
                catch
                {
                    sb.Append("OS:n/a,");
                }
                sb.AppendFormat("{0};", CultureInfo.InstalledUICulture.Name);
                try
                {
                    sb.AppendFormat(".NET CLR:{0};", RuntimeEnvironment.GetSystemVersion());
                }
                catch
                {
                    sb.Append(".NET CLR:n/a;");
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the app GUID. Used by update web-service.
        /// </summary>
        /// <value>The app GUID.</value>
        public static string AppGuid
        {
            get
            {
                return applicationGuid;
            }
        }

        public static string Name
        {
            get
            {
                return applicationId;
            }
        }

        public static string Caption
        {
            get
            {
                return String.Format("{0} {1}", applicationName, VersionLong);
            }
        }

        public static string CaptionOnly
        {
            get
            {
                return applicationName;
            }
        }

        public static string DefaultCategory
        {
            get
            {
                return defaultCategory;
            }
        }


        /// <summary>
        /// Gets the user agent. Used for web-access.
        /// </summary>
        /// <value>The user agent.</value>
        public static string UserAgent
        {
            get
            {
                return String.Format("{0}/{1}", applicationId, Version);
            }
        }

        /// <summary>
        /// Gets the default preferences.
        /// </summary>
        /// <value>The default preferences.</value>
        public static RssBanditPreferences DefaultPreferences
        {
            get
            {
                return defaultPrefs;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to use unconditional comment RSS.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [unconditional comment RSS]; otherwise, <c>false</c>.
        /// </value>
        public static bool UnconditionalCommentRss
        {
            get
            {
                return unconditionalCommentRss;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to use automatic color schemes.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [automatic color schemes]; otherwise, <c>false</c>.
        /// </value>
        public static bool AutomaticColorSchemes
        {
            get
            {
                return automaticColorSchemes;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [portable application mode].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [portable application mode]; otherwise, <c>false</c>.
        /// </value>
        public static bool PortableApplicationMode
        {
            get
            {
                return portableApplicationMode;
            }
        }

		/// <summary>
		/// Gets the search index behavior.
		/// </summary>
		/// <value>The search index behavior.</value>
		public static SearchIndexBehavior SearchIndexBehavior
		{
			get
			{
				return searchIndexBehavior;
			}
		}

        private static string ApplicationDataFolderFromEnv
        {
            get
            {
                if (string.IsNullOrEmpty(appDataFolderPath))
                {
                    appDataFolderPath = ConfigurationManager.AppSettings["AppDataFolder"];
                    if (!string.IsNullOrEmpty(appDataFolderPath))
                    {
                        appDataFolderPath = Environment.ExpandEnvironmentVariables(appDataFolderPath);
						
						if (!Path.IsPathRooted(appDataFolderPath))
						{
							// expand a relative path to be relative to the executable:
							appDataFolderPath = Path.Combine(
								Path.GetDirectoryName(Application.ExecutablePath), appDataFolderPath);
						}
						else
						{
							// here we can get "\folder", or also "\\server\shared\folder" or "D:\data\folder"
							// for portable app support we resolve "\folder" to AppExe root drive + folder:
							if (appDataFolderPath.StartsWith(@"\") && 
								!appDataFolderPath.StartsWith(@"\\"))
								// we have to cut the leading slash off (Path.Combine don't like it):
								appDataFolderPath =
									Path.Combine(Path.GetPathRoot(Application.ExecutablePath), 
									appDataFolderPath.Substring(1));
						}
					}
                    else
                    {
                        try
                        {
                            // once
                            appDataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Name);
                        }
                        catch (SecurityException secEx)
                        {
                            MessageBox.Show(
                                "Cannot query for Environment.SpecialFolder.ApplicationData:\n" + secEx.Message,
                                "Critical security violation");
                            Application.Exit();
                        }
                    }

#if ALT_CONFIG_PATH
    // Keep debug path separate
                    appDataFolderPath = Path.Combine(appDataFolderPath, "Debug");
#endif
					try
					{
						if (!Directory.Exists(appDataFolderPath))
							Directory.CreateDirectory(appDataFolderPath);
					}
					catch (IOException ioEx)
					{
						MessageBox.Show(String.Format(
							"Cannot access/create data directory:\r\n{0}\r\n\r\nError was: \n{1}", appDataFolderPath, ioEx.Message),
							"Critical IO error");
						Application.Exit();
					}
                }

                return appDataFolderPath;
            }
        }

        private static string ApplicationLocalDataFolderFromEnv
        {
            get
            {
                string s = ConfigurationManager.AppSettings["AppCacheFolder"];
                if (!string.IsNullOrEmpty(s))
                {
                    s = Environment.ExpandEnvironmentVariables(s);
                }
                else
                {
                    // We changed this in general to Environment.SpecialFolder.LocalApplicationData
                    // to support better roaming perf. for windows roaming profiles.
                    // but reqires a upgrade path to move existing cache content to the new location...
                    try
                    {
                        // once
                        s = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Name);
                    }
                    catch (SecurityException secEx)
                    {
                        MessageBox.Show(
                            "Cannot query for Environment.SpecialFolder.LocalApplicationData:\n" + secEx.Message,
                            "Security violation");
                        Application.Exit();
                    }
                }

                // expand a relative path to be relative to the executable:
                if (!Path.IsPathRooted(s))
                    s = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), s);
                if (-1 == Path.GetPathRoot(s).IndexOf(":"))
                    appDataFolderPath = Path.Combine(Path.GetPathRoot(Application.ExecutablePath), s.Substring(1));

#if ALT_CONFIG_PATH
    // Keep debug path separate
                s = Path.Combine(s, "Debug");
#endif
                if (!Directory.Exists(s))
                    Directory.CreateDirectory(s);

                return s;
            }
        }

    //    /// <summary>
    //    /// Returns true, if a Yes/No dialog should be displayed on startup (asking for
    //    /// to make Bandit the default "feed:" scheme protocol handler)
    //    /// </summary>
    //    public static bool ShouldAskForDefaultAggregator
    //    {
    //        get
    //        {
    //            return PersistedSettings.GetProperty("AskForMakeDefaultAggregator", true);
    //        }
    //        set
    //        {
				//PersistedSettings.SetProperty("AskForMakeDefaultAggregator", value);
    //        }
    //    }

        public static string GetUserPath()
        {
            return ApplicationDataFolderFromEnv;
        }
		
		public static string GetLocalUserPath()
		{
			return ApplicationLocalDataFolderFromEnv;
		}

		public static string GetUserPersonalPath()
		{
			string mydocs = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string s = Path.Combine(mydocs, applicationName);
			return s;
		}

        public static string GetSearchesPath()
        {
            string s = Path.Combine(ApplicationDataFolderFromEnv, "searches");
            if (!Directory.Exists(s)) Directory.CreateDirectory(s);
            return s;
        }

        public static string GetTemplatesPath()
        {
            string s = Path.Combine(Application.StartupPath, "templates");
            if (!Directory.Exists(s)) return null;
            return s;
        }


        public static string GetDefaultEnclosuresPath()
        {
			string mydocs = GetUserPersonalPath();
            string s = Path.Combine(mydocs, "My Downloaded Files");
            return s;
        }
		
		public static string GetDefaultPodcastPath()
		{
			string mydocs = GetDefaultEnclosuresPath();
			string s = Path.Combine(mydocs, "Podcasts");
			return s;
		}

        public static string GetPlugInPath()
        {
            string s = Path.Combine(Application.StartupPath, "plugins");
            if (!Directory.Exists(s)) return null;
            return s;
        }
		
		public static string GetPlugInRelativePath()
		{
			string s = Path.Combine(Application.StartupPath, "plugins");
			if (!Directory.Exists(s)) return null;
			return "plugins";
		}

		public static string GetAddInInRelativePath()
		{
			string s = Path.Combine(Application.StartupPath, "addins");
			if (!Directory.Exists(s)) return null;
			return "addins";
		}

        public static string GetSearchIndexPath()
        {
            return Path.Combine(ApplicationLocalDataFolderFromEnv, "index");
        }

        public static string GetFeedFileCachePath()
        {
            #region old behavior

            //		        // old behavior:
            //				string s = ApplicationDataFolderFromEnv;
            //				if(!Directory.Exists(s)) Directory.CreateDirectory(s);
            //				s = Path.Combine(s, @"Cache");
            //				if(!Directory.Exists(s)) 
            //					Directory.CreateDirectory(s);
            //				return s;

            #endregion

            if (appCacheFolderPath == null)
            {
                // We activated this in general to use 
                // Environment.SpecialFolder.LocalApplicationData for cache
                // to support better roaming profile performance
                string s = Path.Combine(ApplicationLocalDataFolderFromEnv, "Cache");

                if (!Directory.Exists(s))
                {
                    string old_cache = Path.Combine(GetUserPath(), "Cache");
                    // move old content:
                    if (Directory.Exists(old_cache))
                    {
                        if (s.StartsWith(old_cache))
                        {
                            _log.Error("GetFeedFileCachePath(): " + String.Format(SR.CacheFolderInvalid_CannotBeMoved,s));
                            Splash.Close();
                            MessageBox.Show(String.Format(SR.CacheFolderInvalid_CannotBeMoved, s),
                                Caption,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            s = old_cache;
                        }
                        else
                        {
                            try
                            {
                                string s_root_old = Directory.GetDirectoryRoot(old_cache);
                                string s_root = Directory.GetDirectoryRoot(s);
                                if (s_root_old == s_root)
                                {
                                    // fast move possible on the same drive:
                                    Directory.Move(old_cache, s);
                                }
                                else
                                {
                                    // slower action (source on network/oher drive):
                                    if (!Directory.Exists(s))
                                        Directory.CreateDirectory(s);
                                    // copy files:
                                    foreach (string f in Directory.GetFiles(old_cache))
                                    {
                                        File.Copy(f, Path.Combine(s, Path.GetFileName(f)), true);
                                    }
                                    // delete source(s):
                                    Directory.Delete(old_cache, true);
                                }
                            }
                            catch (Exception ex)
                            {
                                _log.Error("GetFeedFileCachePath()error while moving cache folder.", ex);
                                Splash.Close();
                                MessageBox.Show(String.Format(
                                    SR.CacheFolderInvalid_CannotBeMovedException, s, ex.Message),
                                    Caption,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                                s = old_cache;
                            }
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory(s);
                    }
                }
                appCacheFolderPath = s;
            }
            return appCacheFolderPath;
        }


        /// <summary>
        /// Gets the error log path.
        /// </summary>
        /// <returns></returns>
        public static string GetErrorLogPath()
        {
            string s = Path.Combine(ApplicationDataFolderFromEnv, "errorlog");
            if (!Directory.Exists(s)) Directory.CreateDirectory(s);
            return s;
        }

        /// <summary>
        /// Gets the name of the feed error file.
        /// </summary>
        /// <returns></returns>
        public static string GetFeedErrorFileName()
        {
            return Path.Combine(GetErrorLogPath(), "feederrors.xml");
        }

        /// <summary>
        /// Gets the name of the flag items file.
        /// </summary>
        /// <returns></returns>
        public static string GetFlagItemsFileName()
        {
            return Path.Combine(GetUserPath(), "flagitems.xml");
        }

        /// <summary>
        /// Gets the name of the watched items file.
        /// </summary>
        /// <returns></returns>
        public static string GetWatchedItemsFileName()
        {
            return Path.Combine(GetUserPath(), "watcheditems.xml");
        }

        /// <summary>
        /// Gets the name of the sent items file.
        /// </summary>
        /// <returns></returns>
        public static string GetSentItemsFileName()
        {
            return Path.Combine(GetUserPath(), "replyitems.xml");
        }

        /// <summary>
        /// Gets the name of the deleted items file.
        /// </summary>
        /// <returns></returns>
        public static string GetDeletedItemsFileName()
        {
            return Path.Combine(GetUserPath(), "deleteditems.xml");
        }

        /// <summary>
        /// Gets the name of the search folder file.
        /// </summary>
        /// <returns></returns>
        public static string GetSearchFolderFileName()
        {
            return Path.Combine(GetUserPath(), "searchfolders.xml");
        }

        /// <summary>
        /// Gets the name of the shortcut settings file.
        /// </summary>
        /// <returns></returns>
        public static string GetShortcutSettingsFileName()
        {
            return Path.Combine(GetUserPath(), "shortcutsettings.xml");
        }


        /// <summary>
        /// Gets the name of the UI settings file.
        /// </summary>
        /// <returns></returns>
        public static string GetUiSettingsFileName()
        {
            string clr = String.Empty;
            if (NewsComponents.Utils.Common.ClrVersion.Major > 1)
                clr = NewsComponents.Utils.Common.ClrVersion.Major.ToString(NumberFormatInfo.InvariantInfo);
            return Path.Combine(GetUserPath(), ".uisettings" + clr + ".xml");
        }

		public static string GetGlobalSettingsFileName()
		{
			return Path.Combine(GetUserPath(), ".global.settings.xml");
		}

		/// <summary>
		/// Gets the name of the feed sources file.
		/// </summary>
		/// <returns></returns>
		public static string GetFeedSourcesFileName()
		{
			return Path.Combine(GetUserPath(), "feedsources.xml");
		}

        
        /// <summary>
        /// Gets the name of the generated Top Stories page
        /// </summary>
        /// <returns></returns>
        public static string GetTopStoriesFileName()
        {
            return Path.Combine(GetUserPath(), "top-stories.html");
        }

        /// <summary>
        /// Gets the name of the comments feed list file.
        /// </summary>
        /// <returns></returns>
        public static string GetCommentsFeedListFileName()
        {
            return Path.Combine(GetUserPath(), "comment-subscriptions.xml");
        }

        /// <summary>
        /// Gets the name of the trusted certificate issues file.
        /// </summary>
        /// <returns></returns>
        public static string GetTrustedCertIssuesFileName()
        {
            return Path.Combine(GetUserPath(), "certificates.config.xml");
        }

        /// <summary>
        /// Gets the name of the log file.
        /// </summary>
        /// <returns></returns>
        public static string GetLogFileName()
        {
            return Path.Combine(GetUserPath(), "error.log");
        }

        /// <summary>
        /// Gets the name of the file containing the information about open browser tabs
        /// when the application was last closed 
        /// </summary>
        /// <returns></returns>
        public static string GetBrowserTabStateFileName()
        {
            return Path.Combine(GetUserPath(), ".openbrowsertabs.xml");
        }

        /// <summary>
        /// Gets the name of the subscription tree state file.
        /// </summary>
        /// <returns></returns>
        public static string GetSubscriptionTreeStateFileName()
        {
            return Path.Combine(GetUserPath(), ".treestate.xml");
        }

        /// <summary>
        /// Gets the preferences file name (old binary format).
        /// </summary>
        /// <returns></returns>
        public static string GetPreferencesFileNameOldBinary()
        {
            return Path.Combine(GetUserPath(), ".preferences");
        }

        /// <summary>
        /// Gets the name of the preferences file.
        /// </summary>
        /// <returns></returns>
        public static string GetPreferencesFileName()
        {
            return Path.Combine(GetUserPath(), ".preferences.xml");
        }

		/// <summary>
        /// Gets the path to the (add) Facebook icon used in the Windows 7 jump list
        /// </summary>
        /// <returns></returns>
        public static string GetAddFacebookIconPath()
        {
            return Path.Combine(Application.StartupPath, "Media\\addfacebook.ico");
        }

        /// <summary>
        /// Gets the path to the (add) RSS icon used in the Windows 7 jump list
        /// </summary>
        /// <returns></returns>
        public static string GetAddFeedIconPath()
        {
            return Path.Combine(Application.StartupPath, "Media\\addrssfeed.ico");
        }

		/// <summary>
		/// Gets the path to the RSS icon used in the Windows 7 jump list / recent 
		/// </summary>
		/// <returns></returns>
		public static string GetFeedIconPath()
		{
			return Path.Combine(Application.StartupPath, "Media\\rssfeed.ico");
		}


        /// <summary>
        /// Gets the path to the web page icon used in the Windows 7 jump list
        /// </summary>
        /// <returns></returns>
        public static string GetWebPageIconPath()
        {
            return Path.Combine(Application.StartupPath, "Media\\webpage.ico");
        }
        
        /// <summary>
        /// Method test the running application, if it is registered as the
        /// default "feed:" protocol scheme handler.
        /// </summary>
        /// <returns>true, if registered, else false</returns>
        public static bool IsDefaultAggregator()
        {
            string appPath = Application.ExecutablePath;
            bool isDefault = false;
            try
            {
                string currentHandler = Win32.Registry.CurrentFeedProtocolHandler;
                if (string.IsNullOrEmpty(currentHandler))
	                return false;
                
				isDefault = (String.Concat(appPath, " ", "\"", "%1", "\"").CompareTo(currentHandler) == 0);
            }
            catch (SecurityException secex)
            {
                _log.Warn("Security exception error on make default aggregator.", secex);
            }
            catch (Exception e)
            {
                _log.Error("Unexpected Error while check for default aggregator", e);
            }
            return isDefault;
        }

        /// <summary>
        /// Publish a unexpected exception to the user (simple OK dialog is displayed)
        /// </summary>
        /// <param name="ex">Exception to report</param>
        /// <returns>OK DialogResult</returns>
        public static DialogResult PublishException(Exception ex)
        {
            return PublishException(ex, false);
        }

        /// <summary>
        /// Publish a unexpected exception to the user. 
        /// Retry/Ignore/Cancel dialog is displayed, if <c>resumable</c> is true.
        /// </summary>
        /// <param name="ex">Exception to report</param>
        /// <param name="resumable">Set this to true, if the exception is resumable and react
        /// to the DialogResult returned.</param>
        /// <returns>Retry/Ignore/Cancel DialogResults</returns>
        public static DialogResult PublishException(Exception ex, bool resumable)
        {
            return ApplicationExceptionHandler.ShowExceptionDialog(ex, resumable);
        }

        /// <summary>
        /// Helper to create a wrapped Exception, that provides more error infos for a feed
        /// </summary>
        /// <param name="e">Exception</param>
        /// <param name="f">NewsFeed</param>
        /// <param name="fi">IFeedDetails</param>
        /// <returns></returns>
        internal static FeedRequestException CreateLocalFeedRequestException(Exception e, INewsFeed f, IFeedDetails fi)
        {
            return new FeedRequestException(e.Message, e, FeedSource.CreateFailureContext(f, fi));
        }

		/// <summary>
		/// Helper to create a wrapped Exception, that provides more error infos for a feed
		/// </summary>
		/// <param name="e">The exception.</param>
		/// <param name="f">The feed.</param>
		/// <param name="entry">The entry.</param>
		/// <returns></returns>
		static FeedRequestException CreateLocalFeedRequestException(Exception e, INewsFeed f, FeedSourceEntry entry)
		{
			if (entry != null)
				return new FeedRequestException(e.Message, e, entry.Source.GetFailureContext(f));
			return new FeedRequestException(e.Message, e, new Hashtable());
		}

		/// <summary>
		/// Helper to create a wrapped Exception, that provides more error infos for a feed
		/// </summary>
		/// <param name="e">Exception</param>
		/// <param name="feedUrl">feed Url</param>
		/// <param name="entry">The entry.</param>
		/// <returns></returns>
		private static FeedRequestException CreateLocalFeedRequestException(Exception e, string feedUrl, FeedSourceEntry entry)
		{
			if (!string.IsNullOrEmpty(feedUrl) && entry != null)
				return new FeedRequestException(e.Message, e, entry.Source.GetFailureContext(feedUrl));

			return new FeedRequestException(e.Message, e, new Hashtable());
		}
		/// <summary>
		/// Reads an app settings entry. Can be used to init the command line
		/// ivars with settings from a App.config or User.App.config.
		/// Preferred calls should be located in the constructor to init the
		/// ivars, so user provided command line params can override that
		/// initialization.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name">The name of the entry.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>Value read or defaultValue</returns>
		/// <exception cref="ConfigurationErrorsException">On type conversion failures</exception>
		public static T ReadAppSettingsEntry<T>(string name, T defaultValue)
		{
			try
			{
				return NewsComponents.Utils.Common.Configuration.ReadAppSettingsEntry(name, defaultValue);
			} 
			catch (Exception ex)
			{
				throw new ConfigurationErrorsException(ex.Message, ex, "RssBandit.exe.config", 0);
			}
		}

		/// <summary>
		/// Retrives the assembly informational version (from the AssemblyInformationalVersionAttribute).
		/// </summary>
		/// <param name="assembly">Assembly</param>
		/// <returns>String. It is empty if no description was found.</returns>
		public static string GetAssemblyInformationalVersion(Assembly assembly)
		{
			object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
			if (attributes.Length > 0)
			{
				string ad = ((AssemblyInformationalVersionAttribute)attributes[0]).InformationalVersion;
				if (!string.IsNullOrEmpty(ad))
					return ad;
			}
			return String.Empty;
		}

	    public static class OldVersionSupport
	    {
			/// <summary>
			/// Gets the name of the feed list file, version before we used feed sources.
			/// </summary>
			/// <returns></returns>
			public static string GetSubscriptionsFileName()
			{
				return Path.Combine(GetUserPath(), "subscriptions.xml");
			}

			/// <summary>
			/// Gets the old name of the feed list file.
			/// </summary>
			/// <returns></returns>
			public static string GetFeedListFileName()
			{
				return Path.Combine(GetUserPath(), "feedlist.xml");
			}

	    }

        #endregion
    }
}