#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System.Collections;
using System.Diagnostics;
using System.Security;
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
		private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(UACManager));

		static Hashtable _actions = new Hashtable(5);
		
		
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
							rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
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
							rk = Win32.WindowsRegistry.ClassesRootKey(false).OpenSubKey(@"feed", true);
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

		
		private UACManager(){}
	}
}

#region CVS Version Log
/*
 * $Log: UACManager.cs,v $
 * Revision 1.2  2007/07/01 17:59:54  t_rendelmann
 * feature: support for portable application mode (running Bandit from a stick)
 *
 * Revision 1.1  2007/02/13 16:21:23  t_rendelmann
 * security/UAC required changes
 *
 */
#endregion
