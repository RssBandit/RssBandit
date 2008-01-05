using System;
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
using RssBandit.Resources;
using RssBandit.WinGui.Forms;

namespace RssBandit
{
    internal partial class RssBanditApplication
    {
        #region static class routines

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
                lock (typeof (RssBanditApplication))
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
                lock (typeof (RssBanditApplication))
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
                    try
                    {
                        appVersion = Assembly.GetEntryAssembly().GetName().Version;
                    }
                    catch
                    {
                        appVersion = new Version(Application.ProductVersion);
                    }
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
                Version verInfo = Version;
                string versionStr = String.Format("{0}.{1}.{2}.{3}",
                                                  verInfo.Major, verInfo.Minor,
                                                  verInfo.Build, verInfo.Revision);

                if (!string.IsNullOrEmpty(versionPostfix))
                    return String.Format("{0} {1}", versionStr, versionPostfix);
                return versionStr;
            }
        }

        /// <summary>
        /// Gets the version (short format, incl. version postfix).
        /// </summary>
        /// <value>The version</value>
        public static string VersionShort
        {
            get
            {
                Version verInfo = Version;
                string versionStr = String.Format("{0}.{1}",
                                                  verInfo.Major, verInfo.Minor);

                if (!string.IsNullOrEmpty(versionPostfix))
                    return String.Format("{0} {1}", versionStr, versionPostfix);
                return versionStr;
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
        /// Gets the update service URL.
        /// </summary>
        /// <value>The update service URL.</value>
        public static string UpdateServiceUrl
        {
            get
            {
                int idx = DateTime.Now.Second%applicationUpdateServiceUrls.Length;
                return applicationUpdateServiceUrls[idx];
            }
        }

        /// <summary>
        /// Gets the feed validation URL base.
        /// </summary>
        /// <value>The feed validation URL base.</value>
        public static string FeedValidationUrlBase
        {
            get
            {
                return validationUrlBase;
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
                    }
                    else
                    {
                        try
                        {
                            // once
                            appDataFolderPath =
                                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Name);
                            if (!Directory.Exists(appDataFolderPath))
                                Directory.CreateDirectory(appDataFolderPath);
                        }
                        catch (SecurityException secEx)
                        {
                            MessageBox.Show(
                                "Cannot query for Environment.SpecialFolder.ApplicationData:\n" + secEx.Message,
                                "Security violation");
                            Application.Exit();
                        }
                    }

                    // expand a relative path to be relative to the executable:
                    if (!Path.IsPathRooted(appDataFolderPath))
                        appDataFolderPath =
                            Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), appDataFolderPath);
                    if (-1 == Path.GetPathRoot(appDataFolderPath).IndexOf(":"))
                        // we have to cut the leading slash off (Path.Combine don't like it):
                        appDataFolderPath =
                            Path.Combine(Path.GetPathRoot(Application.ExecutablePath), appDataFolderPath.Substring(1));

#if ALT_CONFIG_PATH
    // Keep debug path separate
                    appDataFolderPath = Path.Combine(appDataFolderPath, "Debug");
#endif
                    if (!Directory.Exists(appDataFolderPath))
                        Directory.CreateDirectory(appDataFolderPath);
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
                        s =
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Name);
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

        /// <summary>
        /// Returns true, if a Yes/No dialog should be displayed on startup (asking for
        /// to make Bandit the default "feed:" scheme protocol handler)
        /// </summary>
        public static bool ShouldAskForDefaultAggregator
        {
            get
            {
                return (bool) guiSettings.GetProperty("AskForMakeDefaultAggregator", true);
            }
            set
            {
                guiSettings.SetProperty("AskForMakeDefaultAggregator", value);
            }
        }

        public static string GetUserPath()
        {
            return ApplicationDataFolderFromEnv;
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


        public static string GetEnclosuresPath()
        {
            string mydocs = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string s = Path.Combine(mydocs, "RSS Bandit\\My Downloaded Files");
            return s;
        }


        public static string GetPlugInPath()
        {
            string s = Path.Combine(Application.StartupPath, "plugins");
            if (!Directory.Exists(s)) return null;
            return s;
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
                            _log.Error("GetFeedFileCachePath(): " + SR.CacheFolderInvalid_CannotBeMoved(s));
                            Splash.Close();
                            MessageBox.Show(
                                SR.CacheFolderInvalid_CannotBeMoved(s),
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
                                MessageBox.Show(
                                    SR.CacheFolderInvalid_CannotBeMovedException(s, ex.Message),
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
        /// Gets the name of the settings file.
        /// </summary>
        /// <returns></returns>
        public static string GetSettingsFileName()
        {
            string clr = String.Empty;
            if (NewsComponents.Utils.Common.ClrVersion.Major > 1)
                clr = NewsComponents.Utils.Common.ClrVersion.Major.ToString();
            return Path.Combine(GetUserPath(), ".settings" + clr + ".xml");
        }

        /// <summary>
        /// Gets the name of the feed list file.
        /// </summary>
        /// <returns></returns>
        public static string GetFeedListFileName()
        {
            return Path.Combine(GetUserPath(), "subscriptions.xml");
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
        /// Gets the old name of the feed list file.
        /// </summary>
        /// <returns></returns>
        public static string GetOldFeedListFileName()
        {
            return Path.Combine(GetUserPath(), "feedlist.xml");
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
        /// Handles errors that occur during schema validation of RSS feed list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public static void CommentFeedListValidationCallback(object sender,
                                                             ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
            {
                _log.Info(GetCommentsFeedListFileName() + " validation warning: " + args.Message);
            }
            else if (args.Severity == XmlSeverityType.Error)
            {
                _log.Error(GetCommentsFeedListFileName() + " validation error: " + args.Message);
            }
        }

        /// <summary>
        /// Handles errors that occur during schema validation of RSS feed list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public static void FeedListValidationCallback(object sender,
                                                      ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
            {
                _log.Info(GetFeedListFileName() + " validation warning: " + args.Message);
            }
            else if (args.Severity == XmlSeverityType.Error)
            {
                validationErrorOccured = true;

                _log.Error(GetFeedListFileName() + " validation error: " + args.Message);
                ExceptionManager.Publish(args.Exception);
            }
        }

        /// <summary>
        /// Method install Bandit as the "feed:" url scheme handler
        /// </summary>
        public static void MakeDefaultAggregator()
        {
            string appPath = Application.ExecutablePath;
            try
            {
                Win32.Registry.CurrentFeedProtocolHandler = appPath;
                // on success, ask the next startup time, if we are not anymore the default handler:
                ShouldAskForDefaultAggregator = true;
            }
            catch (Exception ex)
            {
                _log.Debug("Unable to set CurrentFeedProtocolHandler", ex);
                throw;
            }

            CheckAndRegisterIEMenuExtensions();
        }

        /// <summary>
        /// Checks and register IE Menu Extensions.
        /// Ensures that there is a 'Subscribe in RSS Bandit' menu option. Also if we 
        /// are the default aggregator, we remove the option to subscribe in the default 
        /// aggregator.			
        /// </summary>
        public static void CheckAndRegisterIEMenuExtensions()
        {
            try
            {
                //if we are the default aggregator then remove that menu option since it is redundant
                if (Win32.Registry.IsInternetExplorerExtensionRegistered(Win32.IEMenuExtension.DefaultFeedAggregator))
                    Win32.Registry.UnRegisterInternetExplorerExtension(Win32.IEMenuExtension.DefaultFeedAggregator);

                if (!Win32.Registry.IsInternetExplorerExtensionRegistered(Win32.IEMenuExtension.Bandit))
                    Win32.Registry.RegisterInternetExplorerExtension(Win32.IEMenuExtension.Bandit);
            }
            catch (Exception ex)
            {
                _log.Debug("CheckAndRegisterIEMenuExtensions(): Unable to modify InternetExplorerExtension", ex);
            }
        }

        /// <summary>
        /// Method test the running application, if it is registered as the
        /// default "feed:" protocol scheme handler.
        /// </summary>
        /// <returns>true, if registered, else false</returns>
        public static bool IsDefaultAggregator()
        {
            return IsDefaultAggregator(Application.ExecutablePath);
        }

        /// <summary>
        /// Method test the provided appPath (incl. .exe name!), if it is registered as the
        /// default "feed:" protocol scheme handler.
        /// </summary>
        /// <param name="appPath">Full path name incl. executable name</param>
        /// <returns>true, if registered, else false</returns>
        public static bool IsDefaultAggregator(string appPath)
        {
            bool isDefault = false;
            try
            {
                string currentHandler = Win32.Registry.CurrentFeedProtocolHandler;
                if (string.IsNullOrEmpty(currentHandler))
                {
                    // we just take over the control, if it is not yet set
                    MakeDefaultAggregator();
                    isDefault = true;
                }
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
        /// <param name="f">feedsFeed</param>
        /// <param name="fi">IFeedDetails</param>
        /// <returns></returns>
        public static FeedRequestException CreateLocalFeedRequestException(Exception e, feedsFeed f, IFeedDetails fi)
        {
            return new FeedRequestException(e.Message, e, NewsHandler.GetFailureContext(f, fi));
        }

        #endregion
    }
}