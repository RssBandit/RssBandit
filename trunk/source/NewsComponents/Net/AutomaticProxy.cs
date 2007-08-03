#region CVS Version Header
/*
 * $Id: AutomaticProxy.cs,v 1.8 2006/11/03 14:29:55 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2006/11/03 14:29:55 $
 * $Revision: 1.8 $
 */
#endregion

using System;
using System.Globalization;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace NewsComponents.Net
{
	/// <summary>
	/// AutomaticProxy: class handles auto-configuration of the web proxy.
	/// </summary>
	public class AutomaticProxy: IWebProxy
	{

		#region private variables
		private static bool canUseWinHTTP = false;

		private DateTime lastIESettingsRefresh = DateTime.MaxValue;				// no refresh
		private TimeSpan refreshIESettingsInterval = new TimeSpan(0, 4, 0);	// interval: 15 minutes

		private bool autodiscovery = false;
		private Uri configScriptUri = null;
		private IWebProxy webProxy = null;
		private static readonly log4net.ILog _log = RssBandit.Common.Logging.Log.GetLogger(typeof(AutomaticProxy));
		#endregion

		#region ctor's
		static AutomaticProxy(){
			try {
				canUseWinHTTP = CheckPlatform();
			} catch { /* can fail, if the DLL isn't there*/ }
		}

		public AutomaticProxy():this(false, null, null ){}
		public AutomaticProxy(bool autoDetectSettings):
			this(autoDetectSettings, null, null){
		}
		public AutomaticProxy(bool autoDetectSettings, Uri configurationScriptUri):
			this(autoDetectSettings, configurationScriptUri, null ) {
		}
		public AutomaticProxy(bool autoDetectSettings, string configurationScriptUrl):
			this(autoDetectSettings, AutomaticProxy.CreateProxyUri(configurationScriptUrl), null ) {
		}
		public AutomaticProxy(bool autoDetectSettings, Uri configurationScriptUri, IWebProxy fallbackProxy):base() {
			
			autodiscovery = autoDetectSettings;
			configScriptUri = configurationScriptUri;

			webProxy = fallbackProxy;
			if (webProxy == null)
				webProxy = GlobalProxySelection.GetEmptyWebProxy();

			CheckAndRefreshInfos();
		}
		#endregion

		#region IWebProxy Members

		/// <summary>
		/// Returns the URI of a proxy.
		/// </summary>
		/// <param name="destination">A <see cref="T:System.Uri"/> specifying the requested Internet resource.</param>
		/// <returns>
		/// A <see cref="T:System.Uri"/>
		/// containing the URI of the proxy used to contact
		/// <paramref name="destination"/>.
		/// </returns>
		public Uri GetProxy(Uri destination) {
			
			CheckAndRefreshInfos();

			if (false == canUseWinHTTP)
				return webProxy.GetProxy(destination);

			if (false == autodiscovery && configScriptUri == null) 
				return webProxy.GetProxy(destination);

			return GetAutoProxyForUrl(destination);
		}

		/// <summary>
		/// The credentials to submit to the proxy server for authentication.
		/// </summary>
		/// <value></value>
		public ICredentials Credentials {
			get {
				return webProxy.Credentials;
			}
			set {
				webProxy.Credentials = value;
			}
		}

		/// <summary>
		/// Indicates that the proxy should not be used for the specified host.
		/// </summary>
		/// <param name="host">The <see cref="T:System.Uri"/> of the host to check for proxy use.</param>
		/// <returns>
		/// 	<see langword="true "/>if the proxy server should not be used for <paramref name="host"/>;
		/// otherwise, <see langword="false"/>.
		/// </returns>
		public bool IsBypassed(Uri host) {
			 
			 CheckAndRefreshInfos();

			 if (false == autodiscovery && configScriptUri == null) 
				 return webProxy.IsBypassed(host);
			 
			 // when return true, we will not be ask for the Uri via GetProxy(destination):
			 return false;
		}

		#endregion

		#region properties
		/// <summary>
		/// Gets/Set a boolean to control the ability to automatically detect 
		/// the proxy-server settings or automatic configuration settings.
		/// </summary>
		public bool AutoDetectSettings
		{
			get { return autodiscovery; }
			set {autodiscovery = value; }
		}

		/// <summary>
		/// Gets/Set the proxy configuration script address Url.
		/// </summary>
		public Uri ConfigScriptAddress
		{
			get{ return configScriptUri;	} 
			set{ configScriptUri = value;	}
		}
	
		/// <summary>
		/// Returns true, if Proxy Auto-Config functionality is available
		/// </summary>
		public static bool AutoConfigurationPossible {
			get { return canUseWinHTTP; }	
		}
		#endregion

		#region methods
		/// <summary>
		/// Extends the WebProxy.GetDefaultProxy() method by adding also
		/// the dynamic proxy settings, if possible.
		/// </summary>
		/// <returns>AutomaticProxy</returns>
		public static AutomaticProxy GetProxyFromIESettings()
		{
			AutomaticProxy autoProxy = null;
			WebProxy ieProxy = null;

			try {
				ieProxy = WebProxy.GetDefaultProxy();
				if (ieProxy != null) {
					// fixing IE Proxy Auth. issue:
					ieProxy.Credentials = CredentialCache.DefaultCredentials;
				}

			} catch (Exception ex){
				// happens on systems with IE release below 5.5
				_log.Error("WebProxy.GetDefaultProxy() caused exception: "+ ex.Message);
			}

			// we prefer to have the settings including the dynamic options:
			if (ieProxy != null && canUseWinHTTP){	
				
				// ensure, we have a impersonating app:
				try {
					AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
					Thread.CurrentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
				} catch (Exception ex){
					_log.Error("Cannot set thread principal to current windows user: " + ex.Message);
				}
				Interop.WINHTTP_CURRENT_USER_IE_PROXY_CONFIG config = new Interop.WINHTTP_CURRENT_USER_IE_PROXY_CONFIG();
				if (Interop.WinHttpGetIEProxyConfigForCurrentUser(ref config)){
					
					bool autodetect = config.fAutoDetect;
					string scriptUrl = null;
					
					if (config.lpszAutoConfigUrl != IntPtr.Zero) {
						scriptUrl = Marshal.PtrToStringUni(config.lpszAutoConfigUrl);
						Marshal.FreeHGlobal(config.lpszAutoConfigUrl);
					}
					if (config.lpszProxy != IntPtr.Zero) {	// this we should already have (for debug only here)
						string proxyUrl = Marshal.PtrToStringUni(config.lpszProxy);
						Marshal.FreeHGlobal(config.lpszProxy);
					}
					if (config.lpszProxyBypass != IntPtr.Zero) {	// this we should already have (for debug only here)
						string byPass = Marshal.PtrToStringUni(config.lpszProxyBypass);
						Marshal.FreeHGlobal(config.lpszProxyBypass);
					}

					Uri scriptUri = CreateProxyUri(scriptUrl);

					autoProxy = new AutomaticProxy(autodetect, scriptUri, ieProxy);
					autoProxy.lastIESettingsRefresh = DateTime.Now;

				} else {

					int failureCode = Marshal.GetLastWin32Error();
					if (failureCode == Interop.ERROR_FILE_NOT_FOUND)
						_log.Error("WinHttpGetIEProxyConfigForCurrentUser() failed: Cannot find IE settings.");
					if (failureCode == Interop.ERROR_WINHTTP_INTERNAL_ERROR)
						_log.Error("WinHttpGetIEProxyConfigForCurrentUser() failed: An internal error has occurred");
					if (failureCode == Interop.ERROR_NOT_ENOUGH_MEMORY)
						_log.Error("WinHttpGetIEProxyConfigForCurrentUser() failed: Not enough memory was available to complete the requested operation");

					autoProxy = new AutomaticProxy(false, null, ieProxy);
					autoProxy.lastIESettingsRefresh = DateTime.Now;
				}
			}

			return autoProxy;
		}
		#endregion

		#region private stuff

		private static Uri CreateProxyUri(string address) {
			if (address == null) {
				return null;
			}
			if (address.IndexOf("://") == -1) {
				address = "http://" + address;
			}
			return new Uri(address);
		}

		private static bool CheckPlatform()
		{
			try{
				return Interop.WinHttpCheckPlatform();
			} catch{
				return false;
			}
		}

		/// <summary>
		/// Because we cannot "listen" to registry key changes for now,
		/// we poll in regular intervals for IE proxy config changes.
		/// </summary>
		private void CheckAndRefreshInfos(){
			
			if (lastIESettingsRefresh != DateTime.MaxValue) {	
				// initialized by a call to GetProxyFromIESettings()
				TimeSpan timePast = DateTime.Now.Subtract(lastIESettingsRefresh); 
				if (timePast > refreshIESettingsInterval) {
					try {
						AutomaticProxy p = GetProxyFromIESettings();
						if (p != null) {
							p.Credentials = this.Credentials;	// forward credentials
							this.AutoDetectSettings = p.AutoDetectSettings;
							this.ConfigScriptAddress = p.ConfigScriptAddress;
							this.webProxy = p.webProxy;		// also get credentials back
						}
					} catch (Exception) {
					}

					lastIESettingsRefresh = DateTime.Now;
				}
			}

		}

		private Uri GetAutoProxyForUrl(Uri destination) {
			
			StringBuilder error = new StringBuilder();	// for exception message texts
			IntPtr hSession = IntPtr.Zero;

			Interop.WINHTTP_AUTOPROXY_OPTIONS autoProxyOptions = new Interop.WINHTTP_AUTOPROXY_OPTIONS();
			Interop.WINHTTP_PROXY_INFO proxyInfo = new Interop.WINHTTP_PROXY_INFO();

			string proxy = null;
			Uri toReturn = destination;

			hSession = Interop.WinHttpOpen("auto proxy agent/1.0", 
				Interop.WINHTTP_ACCESS_TYPE_DEFAULT_PROXY,
				Interop.WINHTTP_NO_PROXY_NAME, 
				Interop.WINHTTP_NO_PROXY_BYPASS,
				0);

			// Exit if the WinHttpOpen function fails.
			if (hSession == IntPtr.Zero) {
				int failureCode = Marshal.GetLastWin32Error();
				if (failureCode == Interop.ERROR_WINHTTP_SHUTDOWN)
					_log.Error("Failed to open session handle: The WinHTTP function support is being shut down or unloaded.");
				if (failureCode == Interop.ERROR_WINHTTP_INTERNAL_ERROR)
					_log.Error("Failed to open session handle: An internal error has occurred");
				if (failureCode == Interop.ERROR_NOT_ENOUGH_MEMORY)
					_log.Error("Failed to open session handle: Not enough memory was available to complete the requested operation");
				return toReturn;
			}

			if (configScriptUri != null){
				// The proxy auto-configuration URL is already known.
				// Therefore, auto-detection is not required.
				autoProxyOptions.dwFlags = Interop.WINHTTP_AUTOPROXY_CONFIG_URL;

				// Set the proxy auto configuration URL.
				autoProxyOptions.lpszAutoConfigUrl = configScriptUri.AbsoluteUri;
			
			} else {
				// Use auto-detection because you do not know a PAC URL.
				autoProxyOptions.dwFlags = Interop.WINHTTP_AUTOPROXY_AUTO_DETECT;
				// Use both Dynamic Host Configuration Protocol (DHCP)
				// and Domain Name System (DNS) based auto-detection.
				autoProxyOptions.dwAutoDetectFlags = Interop.WINHTTP_AUTO_DETECT_TYPE_DHCP
					| Interop.WINHTTP_AUTO_DETECT_TYPE_DNS_A;

			}


			// If obtaining the PAC script requires NTLM/Negotiate
			// authentication, "true" will automatically supply the domain credentials
			// of the client.
			// For reduce of perf. impact we try first with false. See also
			// http://msdn.microsoft.com/library/en-us/winhttp/http/autoproxy_cache.asp?frame=true 
			autoProxyOptions.fAutoLoginIfChallenged = false;
			
	retry_get_proxy:

			// Call the WinHttpGetProxyForUrl function with our target URL.
			if(Interop.WinHttpGetProxyForUrl( hSession, destination.AbsoluteUri, ref autoProxyOptions, ref proxyInfo)) {

				switch (proxyInfo.dwAccessType) {
					case Interop.WINHTTP_ACCESS_TYPE_DEFAULT_PROXY:
						_log.Info("Call WinHttpGetProxyForUrl() gets WINHTTP_ACCESS_TYPE_DEFAULT_PROXY");
						break;
					case Interop.WINHTTP_ACCESS_TYPE_NO_PROXY:
						_log.Info("Call WinHttpGetProxyForUrl() gets WINHTTP_ACCESS_TYPE_NO_PROXY");
						break;
					case Interop.WINHTTP_ACCESS_TYPE_NAMED_PROXY:
						_log.Info("Call WinHttpGetProxyForUrl() gets WINHTTP_ACCESS_TYPE_NAMED_PROXY");
						break;
				}
				
				if (proxyInfo.lpszProxy != IntPtr.Zero) {
					proxy = Marshal.PtrToStringUni(proxyInfo.lpszProxy);
					Marshal.FreeHGlobal(proxyInfo.lpszProxy);
				}
				
				// Clean the WINHTTP_PROXY_INFO structure.
				if (proxyInfo.lpszProxyBypass != IntPtr.Zero) {
					string bypass = Marshal.PtrToStringUni(proxyInfo.lpszProxyBypass);
					Marshal.FreeHGlobal(proxyInfo.lpszProxyBypass);
				}

				if (proxy != null){
					
					// we can get more than one proxy, delimited by ";"
					string[] proxies = proxy.Split(new char[]{';'});
					string workingProxy = MakeHTTPUrl(proxies[0]);
					if (proxies.Length > 1) {
						// if more than one, we check for availability
						for (int i=0; i < proxies.Length; i++) {
							string url = MakeHTTPUrl(proxies[i]);
							if (IsProxyAlive(url)) {
								workingProxy = url;
								break;
							}
						}
					}
				
					try {
						toReturn = new Uri(workingProxy);
					} catch (UriFormatException ex) {
						_log.Error("UriFormatException on received proxy: '" + proxies[0] + "':" + ex.Message);
					}
				
				} else {
					toReturn = this.webProxy.GetProxy(destination);
				}

			} else {
				int failureCode = Marshal.GetLastWin32Error();
				if (failureCode == Interop.ERROR_WINHTTP_AUTODETECTION_FAILED)
					_log.Error("Failed to call WinHttpGetProxyForUrl(): ERROR_WINHTTP_AUTODETECTION_FAILED");
				else if (failureCode == Interop.ERROR_WINHTTP_BAD_AUTO_PROXY_SCRIPT)
					_log.Error("Failed to call WinHttpGetProxyForUrl(): ERROR_WINHTTP_BAD_AUTO_PROXY_SCRIPT");
				else if (failureCode == Interop.ERROR_WINHTTP_INCORRECT_HANDLE_TYPE)
					_log.Error("Failed to call WinHttpGetProxyForUrl(): ERROR_WINHTTP_INCORRECT_HANDLE_TYPE");
				else if (failureCode == Interop.ERROR_WINHTTP_INVALID_URL)
					_log.Error("Failed to call WinHttpGetProxyForUrl(): ERROR_WINHTTP_INVALID_URL");
				else if (failureCode == Interop.ERROR_WINHTTP_LOGIN_FAILURE) {
					_log.Error("Failed to call WinHttpGetProxyForUrl(): ERROR_WINHTTP_LOGIN_FAILURE");
					if (false == autoProxyOptions.fAutoLoginIfChallenged) {
						autoProxyOptions.fAutoLoginIfChallenged = true;
						goto retry_get_proxy;	// try again with auto-credentials
					}
				} else if (failureCode == Interop.ERROR_WINHTTP_UNABLE_TO_DOWNLOAD_SCRIPT)
					_log.Error("Failed to call WinHttpGetProxyForUrl(): ERROR_WINHTTP_UNABLE_TO_DOWNLOAD_SCRIPT");
				else if (failureCode == Interop.ERROR_WINHTTP_UNRECOGNIZED_SCHEME)
					_log.Error("Failed to call WinHttpGetProxyForUrl(): ERROR_WINHTTP_UNRECOGNIZED_SCHEME");
				else
					_log.Error("Failed to call WinHttpGetProxyForUrl(): " + failureCode.ToString());
				// throw new Exception(...);
			}

			if (hSession != IntPtr.Zero) {
				// close handle
				if (false == Interop.WinHttpCloseHandle(hSession)) {
					int failureCode = Marshal.GetLastWin32Error();
					if (failureCode == Interop.ERROR_WINHTTP_SHUTDOWN)
						_log.Error("Failed to close session handle: The WinHTTP function support is being shut down or unloaded.");
					if (failureCode == Interop.ERROR_WINHTTP_INTERNAL_ERROR)
						_log.Error("Failed to close session handle: An internal error has occurred");
					if (failureCode == Interop.ERROR_NOT_ENOUGH_MEMORY)
						_log.Error("Failed to close session handle: Not enough memory was available to complete the requested operation");
				}
			}

			return toReturn;
		}

		private string MakeHTTPUrl(string url)
		{
			if (!url.ToLower(CultureInfo.InvariantCulture).StartsWith("http")) {
				if (url.StartsWith("/"))
					url = "http://" + url.Substring(1);
				else
					url = "http://" + url;
			}
		
			return url;
		}

		private bool IsProxyAlive(string proxyUrl) {
			try {
				if (Interop.InternetCheckConnection(proxyUrl, 0, 0)) 
					return true;
				else {
					int failureCode = Marshal.GetLastWin32Error();
					_log.Error("IsProxyAlive('"+proxyUrl+"') failed with errorCode: " + failureCode);
				}
			} catch (Exception ex) {
				_log.Error("IsProxyAlive('"+proxyUrl+"') failed with exception: " + ex.Message);
			}
			_log.Error("ApiCheckConnection() returns false");
			return false;

		}
		#endregion
	}


	class Interop {

		#region public consts
		
		#region options manifests for WinHttp{Query|Set}Option
		public const int WINHTTP_FIRST_OPTION = WINHTTP_OPTION_CALLBACK;

		public const int WINHTTP_OPTION_CALLBACK = 1;
		public const int WINHTTP_OPTION_RESOLVE_TIMEOUT=2;
		public const int WINHTTP_OPTION_CONNECT_TIMEOUT=3;
		public const int WINHTTP_OPTION_CONNECT_RETRIES=4;
		public const int WINHTTP_OPTION_SEND_TIMEOUT=5;
		public const int WINHTTP_OPTION_RECEIVE_TIMEOUT=6;
		public const int WINHTTP_OPTION_RECEIVE_RESPONSE_TIMEOUT=7;
		public const int WINHTTP_OPTION_HANDLE_TYPE=9;
		public const int WINHTTP_OPTION_READ_BUFFER_SIZE=12;
		public const int WINHTTP_OPTION_WRITE_BUFFER_SIZE=13;
		public const int WINHTTP_OPTION_PARENT_HANDLE=21;
		public const int WINHTTP_OPTION_EXTENDED_ERROR=24;
		public const int WINHTTP_OPTION_SECURITY_FLAGS=31;
		public const int WINHTTP_OPTION_SECURITY_CERTIFICATE_STRUCT=32;
		public const int WINHTTP_OPTION_URL=34;
		public const int WINHTTP_OPTION_SECURITY_KEY_BITNESS=36;
		public const int WINHTTP_OPTION_PROXY=38;

		public const int WINHTTP_OPTION_USER_AGENT=41;
		public const int WINHTTP_OPTION_CONTEXT_VALUE=45;
		public const int WINHTTP_OPTION_CLIENT_CERT_CONTEXT=47;
		public const int WINHTTP_OPTION_REQUEST_PRIORITY=58;
		public const int WINHTTP_OPTION_HTTP_VERSION=59;
		public const int WINHTTP_OPTION_DISABLE_FEATURE=63;

		public const int WINHTTP_OPTION_CODEPAGE=68;
		public const int WINHTTP_OPTION_MAX_CONNS_PER_SERVER=73;
		public const int WINHTTP_OPTION_MAX_CONNS_PER_1_0_SERVER=74;
		public const int WINHTTP_OPTION_AUTOLOGON_POLICY=77;
		public const int WINHTTP_OPTION_SERVER_CERT_CONTEXT=78;
		public const int WINHTTP_OPTION_ENABLE_FEATURE=79;
		public const int WINHTTP_OPTION_WORKER_THREAD_COUNT=80;
		public const int WINHTTP_OPTION_PASSPORT_COBRANDING_TEXT=81;
		public const int WINHTTP_OPTION_PASSPORT_COBRANDING_URL=82;
		public const int WINHTTP_OPTION_CONFIGURE_PASSPORT_AUTH=83;
		public const int WINHTTP_OPTION_SECURE_PROTOCOLS=84;
		public const int WINHTTP_OPTION_ENABLETRACING=85;
		public const int WINHTTP_OPTION_PASSPORT_SIGN_OUT=86;
		public const int WINHTTP_OPTION_PASSPORT_RETURN_URL=87;
		public const int WINHTTP_OPTION_REDIRECT_POLICY=88;
		public const int WINHTTP_OPTION_MAX_HTTP_AUTOMATIC_REDIRECTS=89;
		public const int WINHTTP_OPTION_MAX_HTTP_STATUS_CONTINUE=90;
		public const int WINHTTP_OPTION_MAX_RESPONSE_HEADER_SIZE=91;
		public const int WINHTTP_OPTION_MAX_RESPONSE_DRAIN_SIZE=92;

		public const int WINHTTP_LAST_OPTION =WINHTTP_OPTION_MAX_RESPONSE_DRAIN_SIZE;
		
		#endregion

		#region other options
		public const int WINHTTP_OPTION_USERNAME=0x1000;
		public const int WINHTTP_OPTION_PASSWORD=0x1001;
		public const int WINHTTP_OPTION_PROXY_USERNAME=0x1002;
		public const int WINHTTP_OPTION_PROXY_PASSWORD=0x1003;


		// manifest value for WINHTTP_OPTION_MAX_CONNS_PER_SERVER and WINHTTP_OPTION_MAX_CONNS_PER_1_0_SERVER
		public const uint WINHTTP_CONNS_PER_SERVER_UNLIMITED=0xFFFFFFFF;


		// values for WINHTTP_OPTION_AUTOLOGON_POLICY
		public const int WINHTTP_AUTOLOGON_SECURITY_LEVEL_MEDIUM=0;
		public const int WINHTTP_AUTOLOGON_SECURITY_LEVEL_LOW=1;
		public const int WINHTTP_AUTOLOGON_SECURITY_LEVEL_HIGH=2;

		public const int WINHTTP_AUTOLOGON_SECURITY_LEVEL_DEFAULT=WINHTTP_AUTOLOGON_SECURITY_LEVEL_MEDIUM;

		// values for WINHTTP_OPTION_REDIRECT_POLICY
		public const int WINHTTP_OPTION_REDIRECT_POLICY_NEVER=0;
		public const int WINHTTP_OPTION_REDIRECT_POLICY_DISALLOW_HTTPS_TO_HTTP=1;
		public const int WINHTTP_OPTION_REDIRECT_POLICY_ALWAYS=2;

		public const int WINHTTP_OPTION_REDIRECT_POLICY_LAST=WINHTTP_OPTION_REDIRECT_POLICY_ALWAYS;
		public const int WINHTTP_OPTION_REDIRECT_POLICY_DEFAULT=WINHTTP_OPTION_REDIRECT_POLICY_DISALLOW_HTTPS_TO_HTTP;

		public const int WINHTTP_DISABLE_PASSPORT_AUTH=0x00000000;
		public const int WINHTTP_ENABLE_PASSPORT_AUTH=0x10000000;
		public const int WINHTTP_DISABLE_PASSPORT_KEYRING=0x20000000;
		public const int WINHTTP_ENABLE_PASSPORT_KEYRING =0x40000000	;


		// values for WINHTTP_OPTION_DISABLE_FEATURE
		public const int WINHTTP_DISABLE_COOKIES=0x00000001;
		public const int WINHTTP_DISABLE_REDIRECTS=0x00000002;
		public const int WINHTTP_DISABLE_AUTHENTICATION=0x00000004;
		public const int WINHTTP_DISABLE_KEEP_ALIVE=0x00000008;

		// values for WINHTTP_OPTION_ENABLE_FEATURE
		public const int WINHTTP_ENABLE_SSL_REVOCATION=0x00000001;
		public const int WINHTTP_ENABLE_SSL_REVERT_IMPERSONATION = 0x00000002;
		#endregion

		#region general
		//
		// winhttp handle types
		//
		public const int WINHTTP_HANDLE_TYPE_SESSION=1;
		public const int WINHTTP_HANDLE_TYPE_CONNECT=2;
		public const int WINHTTP_HANDLE_TYPE_REQUEST=3;

		//
		// values for auth schemes
		//
		public const int WINHTTP_AUTH_SCHEME_BASIC=0x00000001;
		public const int WINHTTP_AUTH_SCHEME_NTLM=0x00000002;
		public const int WINHTTP_AUTH_SCHEME_PASSPORT=0x00000004;
		public const int WINHTTP_AUTH_SCHEME_DIGEST=0x00000008;
		public const int WINHTTP_AUTH_SCHEME_NEGOTIATE =0x00000010;
    
		// WinHttp supported Authentication Targets

		public const int WINHTTP_AUTH_TARGET_SERVER=0x00000000;
		public const int WINHTTP_AUTH_TARGET_PROXY =0x00000001;

		//
		// values for WINHTTP_OPTION_SECURITY_FLAGS
		//

		// query only
		public const int SECURITY_FLAG_SECURE=0x00000001 ; // can query only
		public const int SECURITY_FLAG_STRENGTH_WEAK=0x10000000;
		public const int SECURITY_FLAG_STRENGTH_MEDIUM=0x40000000;
		public const int SECURITY_FLAG_STRENGTH_STRONG=0x20000000;

		// Secure connection error status flags
		public const int WINHTTP_CALLBACK_STATUS_FLAG_CERT_REV_FAILED=0x00000001;
		public const int WINHTTP_CALLBACK_STATUS_FLAG_INVALID_CERT=0x00000002;
		public const int WINHTTP_CALLBACK_STATUS_FLAG_CERT_REVOKED=0x00000004;
		public const int WINHTTP_CALLBACK_STATUS_FLAG_INVALID_CA=0x00000008;
		public const int WINHTTP_CALLBACK_STATUS_FLAG_CERT_CN_INVALID=0x00000010;
		public const int WINHTTP_CALLBACK_STATUS_FLAG_CERT_DATE_INVALID=0x00000020;
		public const int WINHTTP_CALLBACK_STATUS_FLAG_CERT_WRONG_USAGE=0x00000040;
		public const uint WINHTTP_CALLBACK_STATUS_FLAG_SECURITY_CHANNEL_ERROR=0x80000000;


		public const int WINHTTP_FLAG_SECURE_PROTOCOL_SSL2=0x00000008;
		public const int WINHTTP_FLAG_SECURE_PROTOCOL_SSL3=0x00000020;
		public const int WINHTTP_FLAG_SECURE_PROTOCOL_TLS1=0x00000080;
		public const int WINHTTP_FLAG_SECURE_PROTOCOL_ALL=
			(WINHTTP_FLAG_SECURE_PROTOCOL_SSL2 | 
			WINHTTP_FLAG_SECURE_PROTOCOL_SSL3 | 
			WINHTTP_FLAG_SECURE_PROTOCOL_TLS1);

		// WinHttpOpen dwAccessType values (also for WINHTTP_PROXY_INFO::dwAccessType)
		public const int WINHTTP_ACCESS_TYPE_DEFAULT_PROXY=0;
		public const int WINHTTP_ACCESS_TYPE_NO_PROXY=1;
		public const int WINHTTP_ACCESS_TYPE_NAMED_PROXY=3;

		// WinHttpOpen prettifiers for optional parameters
		public const string WINHTTP_NO_PROXY_NAME  = null;
		public const string WINHTTP_NO_PROXY_BYPASS = null;

		public const int WINHTTP_AUTOPROXY_AUTO_DETECT=0x00000001;
		public const int WINHTTP_AUTOPROXY_CONFIG_URL=0x00000002;
		public const int WINHTTP_AUTOPROXY_RUN_INPROCESS=0x00010000;
		public const int WINHTTP_AUTOPROXY_RUN_OUTPROCESS_ONLY=0x00020000;
		//
		// Flags for dwAutoDetectFlags 
		//
		public const int WINHTTP_AUTO_DETECT_TYPE_DHCP=0x00000001;
		public const int WINHTTP_AUTO_DETECT_TYPE_DNS_A=0x00000002;

		#endregion

		#region status manifests for WinHttp status callback
		public const int WINHTTP_CALLBACK_STATUS_RESOLVING_NAME=0x00000001;
		public const int WINHTTP_CALLBACK_STATUS_NAME_RESOLVED=0x00000002;
		public const int WINHTTP_CALLBACK_STATUS_CONNECTING_TO_SERVER=0x00000004;
		public const int WINHTTP_CALLBACK_STATUS_CONNECTED_TO_SERVER=0x00000008;
		public const int WINHTTP_CALLBACK_STATUS_SENDING_REQUEST=0x00000010;
		public const int WINHTTP_CALLBACK_STATUS_REQUEST_SENT=0x00000020;
		public const int WINHTTP_CALLBACK_STATUS_RECEIVING_RESPONSE=0x00000040;
		public const int WINHTTP_CALLBACK_STATUS_RESPONSE_RECEIVED=0x00000080;
		public const int WINHTTP_CALLBACK_STATUS_CLOSING_CONNECTION=0x00000100;
		public const int WINHTTP_CALLBACK_STATUS_CONNECTION_CLOSED=0x00000200;
		public const int WINHTTP_CALLBACK_STATUS_HANDLE_CREATED=0x00000400;
		public const int WINHTTP_CALLBACK_STATUS_HANDLE_CLOSING=0x00000800;
		public const int WINHTTP_CALLBACK_STATUS_DETECTING_PROXY=0x00001000;
		public const int WINHTTP_CALLBACK_STATUS_REDIRECT=0x00004000;
		public const int WINHTTP_CALLBACK_STATUS_INTERMEDIATE_RESPONSE=0x00008000;
		public const int WINHTTP_CALLBACK_STATUS_SECURE_FAILURE=0x00010000;
		public const int WINHTTP_CALLBACK_STATUS_HEADERS_AVAILABLE=0x00020000;
		public const int WINHTTP_CALLBACK_STATUS_DATA_AVAILABLE=0x00040000;
		public const int WINHTTP_CALLBACK_STATUS_READ_COMPLETE=0x00080000;
		public const int WINHTTP_CALLBACK_STATUS_WRITE_COMPLETE=0x00100000;
		public const int WINHTTP_CALLBACK_STATUS_REQUEST_ERROR=0x00200000;
		public const int WINHTTP_CALLBACK_STATUS_SENDREQUEST_COMPLETE=0x00400000;

		// API Enums for WINHTTP_CALLBACK_STATUS_REQUEST_ERROR:
		public const int API_RECEIVE_RESPONSE=1;
		public const int API_QUERY_DATA_AVAILABLE =2;
		public const int API_READ_DATA =3;
		public const int API_WRITE_DATA =4;
		public const int API_SEND_REQUEST =5;

		public const int WINHTTP_CALLBACK_FLAG_RESOLVE_NAME=(WINHTTP_CALLBACK_STATUS_RESOLVING_NAME | WINHTTP_CALLBACK_STATUS_NAME_RESOLVED);
		public const int WINHTTP_CALLBACK_FLAG_CONNECT_TO_SERVER=(WINHTTP_CALLBACK_STATUS_CONNECTING_TO_SERVER | WINHTTP_CALLBACK_STATUS_CONNECTED_TO_SERVER);
		public const int WINHTTP_CALLBACK_FLAG_SEND_REQUEST=(WINHTTP_CALLBACK_STATUS_SENDING_REQUEST | WINHTTP_CALLBACK_STATUS_REQUEST_SENT);
		public const int WINHTTP_CALLBACK_FLAG_RECEIVE_RESPONSE=(WINHTTP_CALLBACK_STATUS_RECEIVING_RESPONSE | WINHTTP_CALLBACK_STATUS_RESPONSE_RECEIVED);
		public const int WINHTTP_CALLBACK_FLAG_CLOSE_CONNECTION=(WINHTTP_CALLBACK_STATUS_CLOSING_CONNECTION | WINHTTP_CALLBACK_STATUS_CONNECTION_CLOSED);
		public const int WINHTTP_CALLBACK_FLAG_HANDLES=(WINHTTP_CALLBACK_STATUS_HANDLE_CREATED | WINHTTP_CALLBACK_STATUS_HANDLE_CLOSING);
		public const int WINHTTP_CALLBACK_FLAG_DETECTING_PROXY=WINHTTP_CALLBACK_STATUS_DETECTING_PROXY;
		public const int WINHTTP_CALLBACK_FLAG_REDIRECT=WINHTTP_CALLBACK_STATUS_REDIRECT;
		public const int WINHTTP_CALLBACK_FLAG_INTERMEDIATE_RESPONSE=WINHTTP_CALLBACK_STATUS_INTERMEDIATE_RESPONSE;
		public const int WINHTTP_CALLBACK_FLAG_SECURE_FAILURE=WINHTTP_CALLBACK_STATUS_SECURE_FAILURE;
		public const int WINHTTP_CALLBACK_FLAG_SENDREQUEST_COMPLETE=WINHTTP_CALLBACK_STATUS_SENDREQUEST_COMPLETE;
		public const int WINHTTP_CALLBACK_FLAG_HEADERS_AVAILABLE=WINHTTP_CALLBACK_STATUS_HEADERS_AVAILABLE;
		public const int WINHTTP_CALLBACK_FLAG_DATA_AVAILABLE=WINHTTP_CALLBACK_STATUS_DATA_AVAILABLE;
		public const int WINHTTP_CALLBACK_FLAG_READ_COMPLETE=WINHTTP_CALLBACK_STATUS_READ_COMPLETE;
		public const int WINHTTP_CALLBACK_FLAG_WRITE_COMPLETE=WINHTTP_CALLBACK_STATUS_WRITE_COMPLETE;
		public const int WINHTTP_CALLBACK_FLAG_REQUEST_ERROR=WINHTTP_CALLBACK_STATUS_REQUEST_ERROR;


		public const int WINHTTP_CALLBACK_FLAG_ALL_COMPLETIONS=(WINHTTP_CALLBACK_STATUS_SENDREQUEST_COMPLETE   
			| WINHTTP_CALLBACK_STATUS_HEADERS_AVAILABLE     
			| WINHTTP_CALLBACK_STATUS_DATA_AVAILABLE        
			| WINHTTP_CALLBACK_STATUS_READ_COMPLETE        
			| WINHTTP_CALLBACK_STATUS_WRITE_COMPLETE      
			| WINHTTP_CALLBACK_STATUS_REQUEST_ERROR);
		public const uint WINHTTP_CALLBACK_FLAG_ALL_NOTIFICATIONS=  0xffffffff;

		public delegate void WINHTTP_STATUS_CALLBACK(IntPtr hSession, IntPtr dwContext,
			int dwInternetStatus, IntPtr lpvStatusInformation, int dwStatusInformationLength);
		//
		// if the following value is returned by WinHttpSetStatusCallback, then
		// probably an invalid (non-code) address was supplied for the callback
		//
		//public const uint WINHTTP_INVALID_STATUS_CALLBACK= ((WINHTTP_STATUS_CALLBACK)(-1L));

		#endregion

		#region WinHttpQueryHeaders info levels
		//
		// WinHttpQueryHeaders info levels. Generally, there is one info level
		// for each potential RFC822/HTTP/MIME header that an HTTP server
		// may send as part of a request response.
		//
		// The WINHTTP_QUERY_RAW_HEADERS info level is provided for clients
		// that choose to perform their own header parsing.
		//

		public const int WINHTTP_QUERY_MIME_VERSION=0;
		public const int WINHTTP_QUERY_CONTENT_TYPE=1;
		public const int WINHTTP_QUERY_CONTENT_TRANSFER_ENCODING=2;
		public const int WINHTTP_QUERY_CONTENT_ID=3;
		public const int WINHTTP_QUERY_CONTENT_DESCRIPTION=4;
		public const int WINHTTP_QUERY_CONTENT_LENGTH=5;
		public const int WINHTTP_QUERY_CONTENT_LANGUAGE=6;
		public const int WINHTTP_QUERY_ALLOW=7;
		public const int WINHTTP_QUERY_PUBLIC=8;
		public const int WINHTTP_QUERY_DATE=9;
		public const int WINHTTP_QUERY_EXPIRES=10;
		public const int WINHTTP_QUERY_LAST_MODIFIED=11;
		public const int WINHTTP_QUERY_MESSAGE_ID=12;
		public const int WINHTTP_QUERY_URI=13;
		public const int WINHTTP_QUERY_DERIVED_FROM=14;
		public const int WINHTTP_QUERY_COST=15;
		public const int WINHTTP_QUERY_LINK=16;
		public const int WINHTTP_QUERY_PRAGMA=17;
		public const int WINHTTP_QUERY_VERSION=18 ; // special: part of status line
		public const int WINHTTP_QUERY_STATUS_CODE=19;  // special: part of status line
		public const int WINHTTP_QUERY_STATUS_TEXT=20;  // special: part of status line
		public const int WINHTTP_QUERY_RAW_HEADERS=21;  // special: all headers as ASCIIZ
		public const int WINHTTP_QUERY_RAW_HEADERS_CRLF=22;  // special: all headers
		public const int WINHTTP_QUERY_CONNECTION=23;
		public const int WINHTTP_QUERY_ACCEPT=24;
		public const int WINHTTP_QUERY_ACCEPT_CHARSET=25;
		public const int WINHTTP_QUERY_ACCEPT_ENCODING=26;
		public const int WINHTTP_QUERY_ACCEPT_LANGUAGE=27;
		public const int WINHTTP_QUERY_AUTHORIZATION=28;
		public const int WINHTTP_QUERY_CONTENT_ENCODING=29;
		public const int WINHTTP_QUERY_FORWARDED=30;
		public const int WINHTTP_QUERY_FROM=31;
		public const int WINHTTP_QUERY_IF_MODIFIED_SINCE=32;
		public const int WINHTTP_QUERY_LOCATION=33;
		public const int WINHTTP_QUERY_ORIG_URI=34;
		public const int WINHTTP_QUERY_REFERER=35;
		public const int WINHTTP_QUERY_RETRY_AFTER=36;
		public const int WINHTTP_QUERY_SERVER=37;
		public const int WINHTTP_QUERY_TITLE=38;
		public const int WINHTTP_QUERY_USER_AGENT=39;
		public const int WINHTTP_QUERY_WWW_AUTHENTICATE=40;
		public const int WINHTTP_QUERY_PROXY_AUTHENTICATE=41;
		public const int WINHTTP_QUERY_ACCEPT_RANGES=42;
		public const int WINHTTP_QUERY_SET_COOKIE=43;
		public const int WINHTTP_QUERY_COOKIE=44;
		public const int WINHTTP_QUERY_REQUEST_METHOD=45;  // special: GET/POST etc.
		public const int WINHTTP_QUERY_REFRESH=46;
		public const int WINHTTP_QUERY_CONTENT_DISPOSITION=47;

		//
		// HTTP 1.1 defined headers
		//

		public const int WINHTTP_QUERY_AGE=48;
		public const int WINHTTP_QUERY_CACHE_CONTROL=49;
		public const int WINHTTP_QUERY_CONTENT_BASE=50;
		public const int WINHTTP_QUERY_CONTENT_LOCATION=51;
		public const int WINHTTP_QUERY_CONTENT_MD5=52;
		public const int WINHTTP_QUERY_CONTENT_RANGE=53;
		public const int WINHTTP_QUERY_ETAG=54;
		public const int WINHTTP_QUERY_HOST=55;
		public const int WINHTTP_QUERY_IF_MATCH=56;
		public const int WINHTTP_QUERY_IF_NONE_MATCH=57;
		public const int WINHTTP_QUERY_IF_RANGE=58;
		public const int WINHTTP_QUERY_IF_UNMODIFIED_SINCE=59;
		public const int WINHTTP_QUERY_MAX_FORWARDS=60;
		public const int WINHTTP_QUERY_PROXY_AUTHORIZATION=61;
		public const int WINHTTP_QUERY_RANGE=62;
		public const int WINHTTP_QUERY_TRANSFER_ENCODING=63;
		public const int WINHTTP_QUERY_UPGRADE=64;
		public const int WINHTTP_QUERY_VARY=65;
		public const int WINHTTP_QUERY_VIA=66;
		public const int WINHTTP_QUERY_WARNING=67;
		public const int WINHTTP_QUERY_EXPECT=68;
		public const int WINHTTP_QUERY_PROXY_CONNECTION=69;
		public const int WINHTTP_QUERY_UNLESS_MODIFIED_SINCE=70;

		public const int WINHTTP_QUERY_PROXY_SUPPORT=75;
		public const int WINHTTP_QUERY_AUTHENTICATION_INFO=76;
		public const int WINHTTP_QUERY_PASSPORT_URLS=77;
		public const int WINHTTP_QUERY_PASSPORT_CONFIG=78;

		public const int WINHTTP_QUERY_MAX=78;

		//
		// WINHTTP_QUERY_CUSTOM - if this special value is supplied as the dwInfoLevel
		// parameter of WinHttpQueryHeaders() then the lpBuffer parameter contains the name
		// of the header we are to query
		//
		public const int WINHTTP_QUERY_CUSTOM=65535;

		//
		// WINHTTP_QUERY_FLAG_REQUEST_HEADERS - if this bit is set in the dwInfoLevel
		// parameter of WinHttpQueryHeaders() then the request headers will be queried for the
		// request information
		//
		public const uint WINHTTP_QUERY_FLAG_REQUEST_HEADERS=0x80000000;

		//
		// WINHTTP_QUERY_FLAG_SYSTEMTIME - if this bit is set in the dwInfoLevel parameter
		// of WinHttpQueryHeaders() AND the header being queried contains date information,
		// e.g. the "Expires:" header then lpBuffer will contain a SYSTEMTIME structure
		// containing the date and time information converted from the header string
		//
		public const int WINHTTP_QUERY_FLAG_SYSTEMTIME=0x40000000;

		//
		// WINHTTP_QUERY_FLAG_NUMBER - if this bit is set in the dwInfoLevel parameter of
		// HttpQueryHeader(), then the value of the header will be converted to a number
		// before being returned to the caller, if applicable
		//
		public const int WINHTTP_QUERY_FLAG_NUMBER=0x20000000;

		#endregion

		#region HTTP Response Status Codes

		public const int HTTP_STATUS_CONTINUE=100; // OK to continue with request
		public const int HTTP_STATUS_SWITCH_PROTOCOLS=101; // server has switched protocols in upgrade header

		public const int HTTP_STATUS_OK=200; // request completed
		public const int HTTP_STATUS_CREATED=201; // object created, reason = new URI
		public const int HTTP_STATUS_ACCEPTED=202; // async completion (TBS)
		public const int HTTP_STATUS_PARTIAL=203; // partial completion
		public const int HTTP_STATUS_NO_CONTENT=204; // no info to return
		public const int HTTP_STATUS_RESET_CONTENT=205; // request completed, but clear form
		public const int HTTP_STATUS_PARTIAL_CONTENT=206; // partial GET fulfilled
		public const int HTTP_STATUS_WEBDAV_MULTI_STATUS=207; // WebDAV Multi-Status

		public const int HTTP_STATUS_AMBIGUOUS=300; // server couldn't decide what to return
		public const int HTTP_STATUS_MOVED=301; // object permanently moved
		public const int HTTP_STATUS_REDIRECT=302; // object temporarily moved
		public const int HTTP_STATUS_REDIRECT_METHOD=303; // redirection w/ new access method
		public const int HTTP_STATUS_NOT_MODIFIED=304; // if-modified-since was not modified
		public const int HTTP_STATUS_USE_PROXY=305; // redirection to proxy, location header specifies proxy to use
		public const int HTTP_STATUS_REDIRECT_KEEP_VERB=307; // HTTP/1.1: keep same verb

		public const int HTTP_STATUS_BAD_REQUEST=400; // invalid syntax
		public const int HTTP_STATUS_DENIED=401; // access denied
		public const int HTTP_STATUS_PAYMENT_REQ=402; // payment required
		public const int HTTP_STATUS_FORBIDDEN=403; // request forbidden
		public const int HTTP_STATUS_NOT_FOUND=404; // object not found
		public const int HTTP_STATUS_BAD_METHOD=405; // method is not allowed
		public const int HTTP_STATUS_NONE_ACCEPTABLE=406; // no response acceptable to client found
		public const int HTTP_STATUS_PROXY_AUTH_REQ=407; // proxy authentication required
		public const int HTTP_STATUS_REQUEST_TIMEOUT=408; // server timed out waiting for request
		public const int HTTP_STATUS_CONFLICT=409; // user should resubmit with more info
		public const int HTTP_STATUS_GONE=410; // the resource is no longer available
		public const int HTTP_STATUS_LENGTH_REQUIRED=411; // the server refused to accept request w/o a length
		public const int HTTP_STATUS_PRECOND_FAILED=412; // precondition given in request failed
		public const int HTTP_STATUS_REQUEST_TOO_LARGE=413; // request entity was too large
		public const int HTTP_STATUS_URI_TOO_LONG=414; // request URI too long
		public const int HTTP_STATUS_UNSUPPORTED_MEDIA=415; // unsupported media type
		public const int HTTP_STATUS_RETRY_WITH=449; // retry after doing the appropriate action.

		public const int HTTP_STATUS_SERVER_ERROR=500; // internal server error
		public const int HTTP_STATUS_NOT_SUPPORTED=501; // required not supported
		public const int HTTP_STATUS_BAD_GATEWAY=502; // error response received from gateway
		public const int HTTP_STATUS_SERVICE_UNAVAIL=503; // temporarily overloaded
		public const int HTTP_STATUS_GATEWAY_TIMEOUT=504; // timed out waiting for gateway
		public const int HTTP_STATUS_VERSION_NOT_SUP=505; // HTTP version not supported

		public const int HTTP_STATUS_FIRST=HTTP_STATUS_CONTINUE;
		public const int HTTP_STATUS_LAST=HTTP_STATUS_VERSION_NOT_SUP;

		#endregion

		#region error codes

		public const int ERROR_NOT_ENOUGH_MEMORY = 8;	// windows error code
		public const int ERROR_FILE_NOT_FOUND = 2;
		//
		// WinHttp API error returns
		//
		public const int WINHTTP_ERROR_BASE = 12000;

		public const int ERROR_WINHTTP_OUT_OF_HANDLES		= WINHTTP_ERROR_BASE + 1;
		public const int ERROR_WINHTTP_TIMEOUT					= WINHTTP_ERROR_BASE + 2;
		public const int ERROR_WINHTTP_INTERNAL_ERROR			= WINHTTP_ERROR_BASE + 4;
		public const int ERROR_WINHTTP_INVALID_URL				= WINHTTP_ERROR_BASE + 5;
		public const int ERROR_WINHTTP_UNRECOGNIZED_SCHEME	= WINHTTP_ERROR_BASE + 6;
		public const int ERROR_WINHTTP_NAME_NOT_RESOLVED	= WINHTTP_ERROR_BASE + 7;
		public const int ERROR_WINHTTP_INVALID_OPTION			= WINHTTP_ERROR_BASE + 9;
		public const int ERROR_WINHTTP_OPTION_NOT_SETTABLE	= WINHTTP_ERROR_BASE + 11;
		public const int ERROR_WINHTTP_SHUTDOWN				= WINHTTP_ERROR_BASE + 12;

		public const int ERROR_WINHTTP_LOGIN_FAILURE			= WINHTTP_ERROR_BASE + 15;
		public const int ERROR_WINHTTP_OPERATION_CANCELLED	= WINHTTP_ERROR_BASE + 17;
		public const int ERROR_WINHTTP_INCORRECT_HANDLE_TYPE	= WINHTTP_ERROR_BASE + 18;
		public const int ERROR_WINHTTP_INCORRECT_HANDLE_STATE	= WINHTTP_ERROR_BASE + 19;
		public const int ERROR_WINHTTP_CANNOT_CONNECT		= WINHTTP_ERROR_BASE + 29;
		public const int ERROR_WINHTTP_CONNECTION_ERROR	= WINHTTP_ERROR_BASE + 30;
		public const int ERROR_WINHTTP_RESEND_REQUEST		= WINHTTP_ERROR_BASE + 32;

		public const int ERROR_WINHTTP_CLIENT_AUTH_CERT_NEEDED  = WINHTTP_ERROR_BASE + 44;

		//
		// WinHttpRequest Component errors
		//
		public const int ERROR_WINHTTP_CANNOT_CALL_BEFORE_OPEN	= WINHTTP_ERROR_BASE + 100;
		public const int ERROR_WINHTTP_CANNOT_CALL_BEFORE_SEND	= WINHTTP_ERROR_BASE + 101;
		public const int ERROR_WINHTTP_CANNOT_CALL_AFTER_SEND	= WINHTTP_ERROR_BASE + 102;
		public const int ERROR_WINHTTP_CANNOT_CALL_AFTER_OPEN	= WINHTTP_ERROR_BASE + 103;

		//
		// HTTP API errors
		//
		public const int ERROR_WINHTTP_HEADER_NOT_FOUND			= WINHTTP_ERROR_BASE + 150;
		public const int ERROR_WINHTTP_INVALID_SERVER_RESPONSE	= WINHTTP_ERROR_BASE + 152;
		public const int ERROR_WINHTTP_INVALID_QUERY_REQUEST		= WINHTTP_ERROR_BASE + 154;
		public const int ERROR_WINHTTP_HEADER_ALREADY_EXISTS		= WINHTTP_ERROR_BASE + 155;
		public const int ERROR_WINHTTP_REDIRECT_FAILED				= WINHTTP_ERROR_BASE + 156;

		//
		// additional WinHttp API error codes
		//
		public const int ERROR_WINHTTP_AUTO_PROXY_SERVICE_ERROR	= WINHTTP_ERROR_BASE + 178;
		public const int ERROR_WINHTTP_BAD_AUTO_PROXY_SCRIPT	= WINHTTP_ERROR_BASE + 166;
		public const int ERROR_WINHTTP_UNABLE_TO_DOWNLOAD_SCRIPT = WINHTTP_ERROR_BASE + 167;

		public const int ERROR_WINHTTP_NOT_INITIALIZED				= WINHTTP_ERROR_BASE + 172;
		public const int ERROR_WINHTTP_SECURE_FAILURE					= WINHTTP_ERROR_BASE + 175;

		//
		// Certificate security errors. These are raised only by the WinHttpRequest
		// component. The WinHTTP Win32 API will return ERROR_WINHTTP_SECURE_FAILE and
		// provide additional information via the WINHTTP_CALLBACK_STATUS_SECURE_FAILURE
		// callback notification.
		//
		public const int ERROR_WINHTTP_SECURE_CERT_DATE_INVALID	= WINHTTP_ERROR_BASE + 37;
		public const int ERROR_WINHTTP_SECURE_CERT_CN_INVALID	= WINHTTP_ERROR_BASE + 38;
		public const int ERROR_WINHTTP_SECURE_INVALID_CA			= WINHTTP_ERROR_BASE + 45;
		public const int ERROR_WINHTTP_SECURE_CERT_REV_FAILED	= WINHTTP_ERROR_BASE + 57;
		public const int ERROR_WINHTTP_SECURE_CHANNEL_ERROR		= WINHTTP_ERROR_BASE + 157;
		public const int ERROR_WINHTTP_SECURE_INVALID_CERT			= WINHTTP_ERROR_BASE + 169;
		public const int ERROR_WINHTTP_SECURE_CERT_REVOKED		= WINHTTP_ERROR_BASE + 170;
		public const int ERROR_WINHTTP_SECURE_CERT_WRONG_USAGE	= WINHTTP_ERROR_BASE + 179;

		public const int ERROR_WINHTTP_AUTODETECTION_FAILED		= WINHTTP_ERROR_BASE + 180;
		public const int ERROR_WINHTTP_HEADER_COUNT_EXCEEDED	= WINHTTP_ERROR_BASE + 181;
		public const int ERROR_WINHTTP_HEADER_SIZE_OVERFLOW		= WINHTTP_ERROR_BASE + 182;
		public const int ERROR_WINHTTP_CHUNKED_ENCODING_HEADER_SIZE_OVERFLOW = WINHTTP_ERROR_BASE + 183;
		public const int ERROR_WINHTTP_RESPONSE_DRAIN_OVERFLOW	= WINHTTP_ERROR_BASE + 184;
		#endregion
		
		#endregion

		#region structs
		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		public struct WINHTTP_AUTOPROXY_OPTIONS {
			[MarshalAs(UnmanagedType.U4)]
			public int dwFlags;
			[MarshalAs(UnmanagedType.U4)]
			public int dwAutoDetectFlags;
			public string lpszAutoConfigUrl;
			public IntPtr lpvReserved;
			[MarshalAs(UnmanagedType.U4)]
			public int dwReserved;
			public bool fAutoLoginIfChallenged;
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		public struct WINHTTP_PROXY_INFO {
			[MarshalAs(UnmanagedType.U4)]
			public int dwAccessType;
			public IntPtr lpszProxy;
			public IntPtr lpszProxyBypass;
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		public struct WINHTTP_CURRENT_USER_IE_PROXY_CONFIG {
			public bool fAutoDetect;  
			public IntPtr lpszAutoConfigUrl;  
			public IntPtr lpszProxy;  
			public IntPtr lpszProxyBypass;
		} 
		#endregion

		#region functions

		/// <summary>
		/// The WinHttpCheckPlatform function determines whether the 
		/// current platform is supported by this version of 
		/// Microsoft Windows HTTP Services (WinHTTP).
		/// </summary>
		/// <returns>
		/// Returns TRUE if the platform is supported by 
		/// Microsoft Windows HTTP Services (WinHTTP), or FALSE otherwise.
		/// </returns>
		[DllImport("winhttp.dll", SetLastError=true, CharSet=CharSet.Unicode)]
		public static extern bool WinHttpCheckPlatform();

		[DllImport("winhttp.dll", SetLastError=true, CharSet=CharSet.Unicode)]
		public static extern IntPtr WinHttpOpen(
			string pwszUserAgent,
			int dwAccessType,
			string pwszProxyName,
			string pwszProxyBypass,
			int dwFlags);

		[DllImport("winhttp.dll", SetLastError=true, CharSet=CharSet.Unicode)]
		public static extern bool WinHttpGetProxyForUrl(
			IntPtr hSession,
			string lpcwszUrl,
			ref WINHTTP_AUTOPROXY_OPTIONS pAutoProxyOptions,
			ref WINHTTP_PROXY_INFO pProxyInfo
			);

		/// <summary>
		/// The WinHttpGetIEProxyConfigForCurrentUser function obtains the 
		/// Internet Explorer proxy configuration for the current user.
		/// </summary>
		/// <param name="pProxyConfig">WINHTTP_CURRENT_USER_IE_PROXY_CONFIG structure</param>
		/// <returns>Returns TRUE if successful, or FALSE otherwise. 
		/// For extended error information, call Marshal.GetLastWin32Error().</returns>
		[DllImport("winhttp.dll", SetLastError=true, CharSet=CharSet.Unicode)]
		public static extern bool WinHttpGetIEProxyConfigForCurrentUser(
			ref WINHTTP_CURRENT_USER_IE_PROXY_CONFIG pProxyConfig
			);

		[DllImport("winhttp.dll", SetLastError=true, CharSet=CharSet.Unicode)]
		public static extern bool WinHttpCloseHandle(IntPtr hHandle);

		[DllImport("wininet.dll", SetLastError=true)]
		public extern static bool InternetCheckConnection(string url, int flags, int reserved);
		// the only possible flag for InternetCheckConnection()
		public const int FLAG_ICC_FORCE_CONNECTION = 0x01;

		#endregion
	}
}

#region CVS Version Log
/*
 * $Log: AutomaticProxy.cs,v $
 * Revision 1.8  2006/11/03 14:29:55  t_rendelmann
 * fixed: we did not used the Uri.AbsoluteUri property (but Uri.ToString()(
 *
 */
#endregion
