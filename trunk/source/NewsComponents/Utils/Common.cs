#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Runtime.InteropServices;

namespace NewsComponents.Utils
{
	/// <summary>
	/// Common static helper functions.
	/// </summary>
	public sealed class Common
	{
		/// <summary>
		/// Gets the current framework version.
		/// </summary>
		public static readonly Version ClrVersion;
		
		/// <summary>
		/// Initializes the <see cref="Common"/> class.
		/// </summary>
		static Common() {
			ClrVersion = GetFrameworkVersion();
		}

		#region private members

		private static Version GetFrameworkVersion() {
			Version fv = new Version(1,0);
			try {
				fv = new Version(RuntimeEnvironment.GetSystemVersion().Replace("v", String.Empty));
			} catch {}
			return fv;
		}
		private Common(){}

		#endregion

	}
}
#region CVS Version Log
/*
 * $Log: Common.cs,v $
 * Revision 1.1  2006/11/23 11:20:32  t_rendelmann
 * applied a fix to reduce resource leak on internet connections for CLR 1.0/1.1
 *
 */
#endregion

