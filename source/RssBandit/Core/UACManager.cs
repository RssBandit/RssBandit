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
using System.Diagnostics;
using System.Drawing;
using System.Security;
using System.Security.AccessControl;
using Microsoft.Win32;
using Logger = RssBandit.Common.Logging;

namespace RssBandit
{
	internal enum ElevationRequiredAction 
	{
		RunBanditAsWindowsUserLogon,
		MakeDefaultAggregator,
	}

	/// <summary>
	/// User Account Control Manager.
	/// Gets info about critical actions , e.g. registry access require 
	/// higher OS privilegs or user rights.
	/// </summary>
	internal class UACManager
	{
		private static readonly log4net.ILog _log = Logger.DefaultLog.GetLogger(typeof(UACManager));

		static readonly Dictionary<ElevationRequiredAction, string> _actions = new Dictionary<ElevationRequiredAction, string>(3);
		
		
		/// <summary>
		/// Checks, if the action is denied for the current windows user.
		/// </summary>
		/// <param name="action">The action.</param>
		/// <returns>True, in case the action cannot be performed successfully
		/// without user elevation.</returns>
		internal static bool Denied(ElevationRequiredAction action) 
		{
			// check only once:
			if (_actions.ContainsKey(action))
				return _actions[action] != null;
			
			if (RssBanditApplication.PortableApplicationMode) {
				switch (action) {
					case ElevationRequiredAction.RunBanditAsWindowsUserLogon:
						_actions.Add(action, "Not applicable in portable mode.");
						return true;
					case ElevationRequiredAction.MakeDefaultAggregator:
						_actions.Add(action, "Not applicable in portable mode.");
						return true;
					default:
						Debug.Assert(false, "Unhandled ElevationRequiredAction: " + action);
						break;
				}
			} 
			else 
			{
				RegistryKey rk = null;
				switch (action) {
					case ElevationRequiredAction.RunBanditAsWindowsUserLogon:
						try {
							// this test will succeed on Vista, because of the silent redirect of write requests
							// to hives we can write. But it should work on older OS versions with defined restrictions.
							rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\\Microsoft\\Windows\\CurrentVersion\\Run", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.CreateSubKey);
							rk.Close();
							_actions.Add(action, null);
						
						} catch (SecurityException secEx) {
							_log.WarnFormat("ElevationRequiredAction: {0}, {1}", action, secEx);
							_actions.Add(action, secEx.Message);
							return true;
						}
						break;
				
					case ElevationRequiredAction.MakeDefaultAggregator:
						try {
							// this test will succeed on Vista, because of the silent redirect of write requests
							// to hives we can write. But it should work on older OS versions with defined restrictions.
							rk = Win32.WindowsRegistry.ClassesRootKey(false).OpenSubKey(@"feed", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.CreateSubKey);
							rk.Close();
							_actions.Add(action, null);
						
						} catch (SecurityException secEx) {
							_log.WarnFormat("ElevationRequiredAction: {0}, {1}", action, secEx);
							_actions.Add(action, secEx.Message);
							return true;
						}
						break;
					
					default:
						Debug.Assert(false, "Unhandled ElevationRequiredAction: " + action);
						break;
				}
			}
			
			return false;
		}

        /* Requires .NET 2.0 SP1 
		/// <summary>
		/// Gets the shield icon.
		/// </summary>
		/// <value>The shield icon.</value>
		public static Icon ShieldIcon {
			get { return SystemIcons.Shield;  }
		} */ 

		private UACManager(){}
	}
}
