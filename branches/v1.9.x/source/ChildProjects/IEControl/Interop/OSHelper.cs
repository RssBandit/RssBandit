#region Copyright
/*
Copyright (c) 2004-2006 by Torsten Rendelmann

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */
#endregion

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using Microsoft.Win32;

namespace IEControl
{
	#region public enums
	/// <summary>
	/// Windows Suite enum mask
	/// </summary>
	/// <remarks>
	/// Relying on version information is not the best way to test for a feature. 
	/// Instead, refer to the documentation for the feature of interest. For more 
	/// information on common techniques for feature detection, see Operating System 
	/// Version.
	/// If you must require a particular operating system, be sure to use it as a minimum 
	/// supported version, rather than design the test for the one operating system. 
	/// This way, your detection code will continue to work on future versions of Windows.
	/// Note that you should not solely rely upon the VER_SUITE_SMALLBUSINESS flag 
	/// to determine whether Small Business Server has been installed on the system, 
	/// as both this flag and the VER_SUITE_SMALLBUSINESS_RESTRICTED flag are set 
	/// when this product suite is installed. If you upgrade this installation to 
	/// Windows Server, Standard Edition, the VER_SUITE_SMALLBUSINESS_RESTRICTED flag 
	/// will be unset — however, the VER_SUITE_SMALLBUSINESS flag will remain set. 
	/// In this case, this indicates that Small Business Server was once installed on 
	/// this system. If this installation is further upgraded to Windows Server, 
	/// Enterprise Edition, the VER_SUITE_SMALLBUSINESS key will remain set.
	/// To determine whether a Win32-based application is running on WOW64, 
	/// call the Win32 API IsWow64Process function.
	/// </remarks>
	[Flags]
	internal enum WindowsSuite {
		/// <summary>
		/// Windows NT Server
		/// </summary>
		VER_SERVER_NT = unchecked((int)0x80000000),
		/// <summary>
		/// Windows NT Workstation
		/// </summary>
		VER_WORKSTATION_NT = 0x40000000,
		/// <summary>
		/// Microsoft Small Business Server was once installed on the system, 
		/// but may have been upgraded to another version of Windows. 
		/// Refer to the Remarks section for more information about this bit flag.
		/// </summary>
		VER_SUITE_SMALLBUSINESS = 0x00000001,
		/// <summary>
		/// Windows Server 2003, Enterprise Edition, Windows 2000 Advanced Server, 
		/// or Windows NT 4.0 Enterprise Edition, is installed. 
		/// Refer to the Remarks section for more information about this bit flag.
		/// </summary>
		VER_SUITE_ENTERPRISE = 0x00000002,
		/// <summary>
		/// Microsoft BackOffice components are installed.
		/// </summary>
		VER_SUITE_BACKOFFICE = 0x00000004,
		/// <summary>
		/// ?
		/// </summary>
		VER_SUITE_COMMUNICATIONS = 0x00000008,
		/// <summary>
		/// Terminal Services is installed.
		/// </summary>
		VER_SUITE_TERMINAL = 0x00000010,
		/// <summary>
		/// Microsoft Small Business Server is installed with the restrictive client license in force. 
		/// Refer to the Remarks section for more information about this bit flag.
		/// </summary>
		VER_SUITE_SMALLBUSINESS_RESTRICTED = 0x00000020,
		/// <summary>
		/// ?
		/// </summary>
		VER_SUITE_EMBEDDEDNT = 0x00000040,
		/// <summary>
		/// Windows Server 2003, Datacenter Edition or Windows 2000 Datacenter Server is installed.
		/// </summary>
		VER_SUITE_DATACENTER = 0x00000080,
		/// <summary>
		/// Terminal Services is installed, but only one interactive session is supported.
		/// </summary>
		VER_SUITE_SINGLEUSERTS = 0x00000100,
		/// <summary>
		/// Windows XP Home Edition is installed.
		/// </summary>
		VER_SUITE_PERSONAL = 0x00000200,
		/// <summary>
		/// Windows Server 2003, Web Edition is installed.
		/// </summary>
		VER_SUITE_BLADE = 0x00000400,
		/// <summary>
		/// ?
		/// </summary>
		VER_SUITE_EMBEDDED_RESTRICTED = 0x00000800,
	}
	#endregion

	/// <summary>
	/// Operating System Helper routines
	/// </summary>
	[SuppressUnmanagedCodeSecurity]
	internal static class OSHelper
	{
		
		// General info fields
		public static readonly bool IsAspNetServer;
		public static readonly bool IsPostWin2K;
		public static readonly bool IsWin2K;
		public static readonly bool IsWin2k3;
		public static readonly bool IsWin9x;
		public static readonly bool IsWinHttp51;
		/// <summary>
		/// Gets true, if PlatformID is != Win32Windows
		/// </summary>
		public static readonly bool IsWinNt;
		
		public static readonly Version IEVersion;

		private static readonly OperatingSystem _os;

		/// <summary>
		/// Returns true, if the OS is at least Windows 2000 (or higher), else false.
		/// </summary>
		public static bool IsOSAtLeastWindows2000 {
			get { 
				return (IsWinNt && _os.Version.Major >= 5 );
			}
		}

		/// <summary>
		/// Returns true, if the OS is at least Windows XP (or higher), else false.
		/// </summary>
		public static bool IsOSAtLeastWindowsXP {
			get { 
				return (IsWinNt && 
					(_os.Version.Major > 5 || 
					(_os.Version.Major == 5 && 
					_os.Version.Minor >= 1)));
			}
		}

		
		/// <summary>
		/// Returns true, if the OS is Windows XP SP2 and higher, else false.
		/// </summary>
		public static bool IsOSAtLeastWindowsXPSP2 {
			get
			{
				if (IsOSWindowsXP) {
					int spMajor, spMinor;
					GetWindowsServicePackInfo(out spMajor, out spMinor);
					return spMajor <= 2;
				}
				return IsOSAtLeastWindowsXP;
			}
		}

		/// <summary>
		/// Returns true, if the OS is exact Windows XP, else false.
		/// </summary>
		public static bool IsOSWindowsXP {
			get { 
				return (IsWinNt && 
					(_os.Version.Major == 5 && 
					_os.Version.Minor == 1));
			}
		}

		/// <summary>
		/// Returns true, if the OS is exact Windows Vista, else false.
		/// </summary>
		public static bool IsOSWindowsVista {
			get { 
				return (IsWinNt && 
					(_os.Version.Major == 6));
			}
		}

		/// <summary>
		/// Returns true, if the OS is at least Windows Vista (or higher), else false.
		/// </summary>
		public static bool IsOSAtLeastWindowsVista {
			get { 
				return (IsWinNt && 
					(_os.Version.Major >= 6));
			}
		}

		/// <summary>
		/// Gets a value indicating whether IE6 is available.
		/// </summary>
		/// <value><c>true</c> if IE6 is available; otherwise, <c>false</c>.</value>
		public static bool IsIE6 {
			get {
				return (IEVersion.Major >= 6);
			}
		}
		/// <summary>
		/// Gets a value indicating whether IE6 SP2 is available.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if IE6 SP2 is available; otherwise, <c>false</c>.
		/// </value>
		public static bool IsIE6SP2 {
			get {
				return (IEVersion.Major > 6 || 
					(IEVersion.Major == 6 && IEVersion.Minor == 0 && IEVersion.Build >= 2900) ||
					(IEVersion.Major == 6 && IEVersion.Minor > 0 ));
			}
		}
		
		/// <summary>
		/// Gets the windows service pack info.
		/// </summary>
		/// <param name="servicePackMajor">The service pack major.</param>
		/// <param name="servicePackMinor">The service pack minor.</param>
		public static void GetWindowsServicePackInfo(out int servicePackMajor, out int servicePackMinor) {
			OSVERSIONINFOEX ifex = new OSVERSIONINFOEX();
			ifex.dwOSVersionInfoSize = Marshal.SizeOf(ifex);
			if (!NativeMethods.GetVersionEx(ref ifex)) {
				int err = Marshal.GetLastWin32Error();
				throw new Exception(String.Format("Native function to retrieve version info failed with error '{0}'.", err));
			}
			servicePackMajor = ifex.wServicePackMajor;
			servicePackMinor = ifex.wServicePackMinor;
		}

		/// <summary>
		/// Gets the windows suite info.
		/// </summary>
		/// <returns></returns>
		public static WindowsSuite GetWindowsSuiteInfo() {
			OSVERSIONINFOEX ifex = new OSVERSIONINFOEX();
			ifex.dwOSVersionInfoSize = Marshal.SizeOf(ifex);
			if (!NativeMethods.GetVersionEx(ref ifex)) {
				int err = Marshal.GetLastWin32Error();
				throw new Exception(String.Format("Native function to retrieve version info failed with error '{0}'.", err));
			}
			return (WindowsSuite) ifex.wSuiteMask;
		}

		public static Version GetInternetExplorerVersion() {
			RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\Microsoft\\Internet Explorer", false);
			string s = null;
			if (key != null) {
				s = key.GetValue("Version") as string;
				key.Close();
			}
			if (s != null) {
				try {
					return new Version(s);
				}catch (ArgumentOutOfRangeException) {
					// A major, minor, build, or revision component is less than zero
				} catch (ArgumentException) {
					// version has fewer than two components or more than four components	
				} catch (FormatException) {
					// At least one component of version does not parse to a decimal integer.
				}
			}
			// only IE >= 4 write the Version key:
			return new Version(3, 0);
		}
		
		#region ctor's

		[EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
		static OSHelper() {
			_os = Environment.OSVersion;
			if (_os.Platform == PlatformID.Win32Windows) {
				IsWin9x = true;
			} else {
				try {
					IsAspNetServer = Thread.GetDomain().GetData(".appDomain") != null;
				}
				catch { /* all */ }

				IsWinNt = true;
				
				int spMajor, spMinor;
				GetWindowsServicePackInfo(out spMajor, out spMinor);
				
				if ((_os.Version.Major == 5) && (_os.Version.Minor == 0)) {
					IsWin2K = true;
					IsWinHttp51 = (spMajor >= 3);
				} else {
					IsPostWin2K = true;
					if ((_os.Version.Major == 5) && (_os.Version.Minor == 1)) {
						IsWinHttp51 = (spMajor >= 1);
					}
					else {
						IsWinHttp51 = true;
						IsWin2k3 = true;
					}
				}
			}
			
			IEVersion = GetInternetExplorerVersion();
		}

		#endregion

		#region private Interop decl./helper classes

		static class NativeMethods
		{
			// OSVERSIONINFO/EX are defined as a structs
			//		[ DllImport( "kernel32", SetLastError=true )]
			//		private static extern bool GetVersionEx(ref OSVERSIONINFO osvi );
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2205:UseManagedEquivalentsOfWin32Api"), DllImport("kernel32", SetLastError = true)]
			[SuppressUnmanagedCodeSecurity]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool GetVersionEx(ref OSVERSIONINFOEX osvi);
		}
		// Use this when you want to pass it by-value even though the unmanaged API
		// expects a pointer to a structure.  Being a class adds an extra level of indirection.
		//		[StructLayout(LayoutKind.Sequential)]  
		//		private struct OSVERSIONINFO {
		//			public int dwOSVersionInfoSize;  
		//			public int dwMajorVersion;  
		//			public int dwMinorVersion;  
		//			public int dwBuildNumber;  
		//			public int dwPlatformId;
		//			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
		//			public string szCSDVersion;
		//		}

		[StructLayout(LayoutKind.Sequential)]
		private struct OSVERSIONINFOEX
		{
			public int dwOSVersionInfoSize;
			public int dwMajorVersion;
			public int dwMinorVersion;
			public int dwBuildNumber;
			public int dwPlatformId;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			public string szCSDVersion;
			public UInt16 wServicePackMajor;
			public UInt16 wServicePackMinor;
			public UInt16 wSuiteMask;
			public byte wProductType;
			public byte wReserved;
		}

		#endregion
	}
}
