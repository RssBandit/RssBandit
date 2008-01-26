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


namespace IEControl
{
	/// <summary>
	/// Helper class to support the Internet Feature Controls
	/// introduced by Windows XP SP2 for enhanced browser security.
	/// </summary>
	/// <remarks>
	/// For more details, have a look at these articles:
	/// http://www.microsoft.com/technet/prodtechnol/winxppro/maintain/sp2brows.mspx
	/// http://msdn.microsoft.com/workshop/security/szone/overview/sec_featurecontrols.asp
	/// </remarks>
	internal sealed class InternetFeature
	{
		private static PrivateInterop pi = new PrivateInterop();
		
		/// <summary>
		/// Determines whether the specified feature is enabled.
		/// </summary>
		/// <param name="feature">The feature.</param>
		/// <param name="flags">The flags.</param>
		/// <returns>
		/// 	<c>true</c> if the specified feature is enabled; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsEnabled(InternetFeatureList feature, GetFeatureFlag flags) {
			if (!OSHelper.IsOSAtLeastWindowsXPSP2 && !OSHelper.IsIE6) 
				return false;
			
			return pi.isEnabled(feature, flags);
		}

		/// <summary>
		/// Sets the specified feature enabled or disabled.
		/// </summary>
		/// <param name="feature">The feature.</param>
		/// <param name="flags">The flags.</param>
		/// <param name="enable">if set to <c>true</c> [enable].</param>
		public static void SetEnabled(InternetFeatureList feature, SetFeatureFlag flags, bool enable) {
			if (OSHelper.IsOSAtLeastWindowsXPSP2 && OSHelper.IsIE6) {
				pi.setEnabled(feature, flags, enable);
			}
		}

		private InternetFeature() {}

		#region Interop calles (keep separately, to prevent JIT compile failes in public functions!)
		sealed class PrivateInterop {
			
			internal bool isEnabled(InternetFeatureList feature, GetFeatureFlag flags) {
				int HRESULT = CoInternetIsFeatureEnabled(feature, (int)flags);
				//If the HRESULT is 0 or positive (a success code), the method 
				//returns without creating or throwing an exception:
				Marshal.ThrowExceptionForHR(HRESULT);
				return (HRESULT == Interop.S_OK);
			}
			internal void setEnabled(InternetFeatureList feature, SetFeatureFlag flags, bool enable) {
				int HRESULT = CoInternetSetFeatureEnabled(feature, (int)flags, enable);
				//If the HRESULT is 0 or positive (a success code), the method 
				//returns without creating or throwing an exception:
				Marshal.ThrowExceptionForHR(HRESULT);
			}
			
			#region related Interop declarations
			// See: http://msdn.microsoft.com/workshop/security/szone/reference/functions/cointernetisfeatureenabled.asp
			//HRESULT CoInternetIsFeatureEnabled(INTERNETFEATURELIST FeatureEntry,DWORD dwFlags);
			[DllImport("urlmon.dll")]
			[PreserveSig]
			[return:MarshalAs(UnmanagedType.Error)]
			private static extern int CoInternetIsFeatureEnabled(
				InternetFeatureList FeatureEntry,
				[MarshalAs(UnmanagedType.U4)] int dwFlags);

			// See: http://msdn.microsoft.com/workshop/security/szone/reference/functions/cointernetsetfeatureenabled.asp
			//HRESULT CoInternetSetFeatureEnabled(INTERNETFEATURELIST FeatureEntry,DWORD dwFlags,BOOL fEnable);
			[DllImport("urlmon.dll")]
			[PreserveSig]
			[return:MarshalAs(UnmanagedType.Error)]
			private static extern int CoInternetSetFeatureEnabled(
				InternetFeatureList FeatureEntry,
				[MarshalAs(UnmanagedType.U4)] int dwFlags,
				bool fEnable);

			#endregion
		}
		#endregion

		
	}

	/// <summary>
	/// Contains the feature controls for Microsoft Internet Explorer.
	/// See also: http://msdn.microsoft.com/workshop/security/szone/reference/enums/internetfeaturelist.asp
	/// </summary>
	public enum InternetFeatureList: int
	{
		FEATURE_OBJECT_CACHING = 0,
		FEATURE_ZONE_ELEVATION = 1,
		FEATURE_MIME_HANDLING = 2,
		FEATURE_MIME_SNIFFING = 3,
		FEATURE_WINDOW_RESTRICTIONS = 4,
		FEATURE_WEBOC_POPUPMANAGEMENT = 5,
		FEATURE_BEHAVIORS = 6,
		FEATURE_DISABLE_MK_PROTOCOL = 7,
		FEATURE_LOCALMACHINE_LOCKDOWN = 8,
		FEATURE_SECURITYBAND = 9,
		FEATURE_RESTRICT_ACTIVEXINSTALL = 10,
		FEATURE_VALIDATE_NAVIGATE_URL = 11,
		FEATURE_RESTRICT_FILEDOWNLOAD = 12,
		FEATURE_ADDON_MANAGEMENT = 13,
		FEATURE_PROTOCOL_LOCKDOWN = 14,
		FEATURE_HTTP_USERNAME_PASSWORD_DISABLE = 15,
		FEATURE_SAFE_BINDTOOBJECT = 16,
		FEATURE_UNC_SAVEDFILECHECK = 17,
		FEATURE_GET_URL_DOM_FILEPATH_UNENCODED = 18,
		FEATURE_TABBED_BROWSING = 19,	//IE7
		FEATURE_SSLUX = 20,	//IE7
		FEATURE_DISABLE_NAVIGATION_SOUNDS = 21,	//IE7
		FEATURE_DISABLE_LEGACY_COMPRESSION = 22,	//IE7
		FEATURE_FORCE_ADDR_AND_STATUS = 23,	//IE7
		FEATURE_XMLHTTP = 24,	//IE7
		FEATURE_DISABLE_TELNET_PROTOCOL = 25,
		FEATURE_FEEDS = 26,
		FEATURE_BLOCK_INPUT_PROMPTS = 27,	//IE7
		FEATURE_ENTRY_COUNT = 28
	}

	/// <summary>
	/// Specifies where to set the feature control's value.
	/// </summary>
	[Flags]
	public enum SetFeatureFlag: int
	{
		SET_FEATURE_ON_THREAD = 0x00000001,
		SET_FEATURE_ON_PROCESS = 0x00000002,
		SET_FEATURE_IN_REGISTRY = 0x00000004,
		SET_FEATURE_ON_THREAD_LOCALMACHINE = 0x00000008,
		SET_FEATURE_ON_THREAD_INTRANET = 0x00000010,
		SET_FEATURE_ON_THREAD_TRUSTED = 0x00000020,
		SET_FEATURE_ON_THREAD_INTERNET = 0x00000040,
		SET_FEATURE_ON_THREAD_RESTRICTED = 0x00000080,
	}
	
	/// <summary>
	/// Specifies from where to get the feature control's value.
	/// </summary>
	[Flags]
	public enum GetFeatureFlag: int {
		GET_FEATURE_FROM_THREAD = 0x00000001,
		GET_FEATURE_FROM_PROCESS = 0x00000002,
		GET_FEATURE_FROM_REGISTRY = 0x00000004,
		GET_FEATURE_FROM_THREAD_LOCALMACHINE = 0x00000008,
		GET_FEATURE_FROM_THREAD_INTRANET = 0x00000010,
		GET_FEATURE_FROM_THREAD_TRUSTED = 0x00000020,
		GET_FEATURE_FROM_THREAD_INTERNET = 0x00000040,
		GET_FEATURE_FROM_THREAD_RESTRICTED = 0x00000080,
	}
}
