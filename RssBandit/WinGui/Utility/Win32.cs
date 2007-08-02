#region CVS Version Header
/*
 * $Id: Win32.cs,v 1.21 2005/04/23 09:39:06 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/04/23 09:39:06 $
 * $Revision: 1.21 $
 */
#endregion

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;

using Logger = RssBandit.Common.Logging;

namespace RssBandit
{
	/// <summary>
	/// Common used Win32 Interop decl.
	/// </summary>
	public sealed class Win32 {

		#region enum/consts
		public enum ShowWindowStyles : short {
			SW_HIDE				= 0,
			SW_SHOWNORMAL	= 1,
			SW_NORMAL           = 1,
			SW_SHOWMINIMIZED = 2,
			SW_SHOWMAXIMIZED = 3,
			SW_MAXIMIZE         = 3,
			SW_SHOWNOACTIVATE  = 4,
			SW_SHOW				= 5,
			SW_MINIMIZE			= 6,
			SW_SHOWMINNOACTIVE  = 7,
			SW_SHOWNA          = 8,
			SW_RESTORE          = 9,
			SW_SHOWDEFAULT  = 10,
			SW_FORCEMINIMIZE = 11,
			SW_MAX					= 11
		}

		public enum Message {
			WM_NULL                   = 0x0000,
			WM_CREATE                 = 0x0001,
			WM_DESTROY                = 0x0002,
			WM_MOVE                   = 0x0003,
			WM_SIZE                   = 0x0005,
			WM_ACTIVATE               = 0x0006,
			WM_SETFOCUS               = 0x0007,
			WM_KILLFOCUS              = 0x0008,
			WM_ENABLE                 = 0x000A,
			WM_SETREDRAW              = 0x000B,
			WM_SETTEXT                = 0x000C,
			WM_GETTEXT                = 0x000D,
			WM_GETTEXTLENGTH          = 0x000E,
			WM_PAINT                  = 0x000F,
			WM_CLOSE                  = 0x0010,
			WM_QUERYENDSESSION        = 0x0011,
			WM_QUIT                   = 0x0012,
			WM_QUERYOPEN              = 0x0013,
			WM_ERASEBKGND             = 0x0014,
			WM_SYSCOLORCHANGE         = 0x0015,
			WM_ENDSESSION             = 0x0016,
			WM_SHOWWINDOW             = 0x0018,
			WM_WININICHANGE           = 0x001A,
			WM_SETTINGCHANGE          = 0x001A,
			WM_DEVMODECHANGE          = 0x001B,
			WM_ACTIVATEAPP            = 0x001C,
			WM_FONTCHANGE             = 0x001D,
			WM_TIMECHANGE             = 0x001E,
			WM_CANCELMODE             = 0x001F,
			WM_SETCURSOR              = 0x0020,
			WM_MOUSEACTIVATE          = 0x0021,
			WM_CHILDACTIVATE          = 0x0022,
			WM_QUEUESYNC              = 0x0023,
			WM_GETMINMAXINFO          = 0x0024,
			WM_PAINTICON              = 0x0026,
			WM_ICONERASEBKGND         = 0x0027,
			WM_NEXTDLGCTL             = 0x0028,
			WM_SPOOLERSTATUS          = 0x002A,
			WM_DRAWITEM               = 0x002B,
			WM_MEASUREITEM            = 0x002C,
			WM_DELETEITEM             = 0x002D,
			WM_VKEYTOITEM             = 0x002E,
			WM_CHARTOITEM             = 0x002F,
			WM_SETFONT                = 0x0030,
			WM_GETFONT                = 0x0031,
			WM_SETHOTKEY              = 0x0032,
			WM_GETHOTKEY              = 0x0033,
			WM_QUERYDRAGICON          = 0x0037,
			WM_COMPAREITEM            = 0x0039,
			WM_GETOBJECT              = 0x003D,
			WM_COMPACTING             = 0x0041,
			WM_COMMNOTIFY             = 0x0044 ,
			WM_WINDOWPOSCHANGING      = 0x0046,
			WM_WINDOWPOSCHANGED       = 0x0047,
			WM_POWER                  = 0x0048,
			WM_COPYDATA               = 0x004A,
			WM_CANCELJOURNAL          = 0x004B,
			WM_NOTIFY                 = 0x004E,
			WM_INPUTLANGCHANGEREQUEST = 0x0050,
			WM_INPUTLANGCHANGE        = 0x0051,
			WM_TCARD                  = 0x0052,
			WM_HELP                   = 0x0053,
			WM_USERCHANGED            = 0x0054,
			WM_NOTIFYFORMAT           = 0x0055,
			WM_CONTEXTMENU            = 0x007B,
			WM_STYLECHANGING          = 0x007C,
			WM_STYLECHANGED           = 0x007D,
			WM_DISPLAYCHANGE          = 0x007E,
			WM_GETICON                = 0x007F,
			WM_SETICON                = 0x0080,
			WM_NCCREATE               = 0x0081,
			WM_NCDESTROY              = 0x0082,
			WM_NCCALCSIZE             = 0x0083,
			WM_NCHITTEST              = 0x0084,
			WM_NCPAINT                = 0x0085,
			WM_NCACTIVATE             = 0x0086,
			WM_GETDLGCODE             = 0x0087,
			WM_SYNCPAINT              = 0x0088,
			WM_NCMOUSEMOVE            = 0x00A0,
			WM_NCLBUTTONDOWN          = 0x00A1,
			WM_NCLBUTTONUP            = 0x00A2,
			WM_NCLBUTTONDBLCLK        = 0x00A3,
			WM_NCRBUTTONDOWN          = 0x00A4,
			WM_NCRBUTTONUP            = 0x00A5,
			WM_NCRBUTTONDBLCLK        = 0x00A6,
			WM_NCMBUTTONDOWN          = 0x00A7,
			WM_NCMBUTTONUP            = 0x00A8,
			WM_NCMBUTTONDBLCLK        = 0x00A9,
			WM_NCXBUTTONDOWN          = 0x00AB,
			WM_NCXBUTTONUP            = 0x00AC,
			WM_KEYDOWN                = 0x0100,
			WM_KEYUP                  = 0x0101,
			WM_CHAR                   = 0x0102,
			WM_DEADCHAR               = 0x0103,
			WM_SYSKEYDOWN             = 0x0104,
			WM_SYSKEYUP               = 0x0105,
			WM_SYSCHAR                = 0x0106,
			WM_SYSDEADCHAR            = 0x0107,
			WM_KEYLAST                = 0x0108,
			WM_IME_STARTCOMPOSITION   = 0x010D,
			WM_IME_ENDCOMPOSITION     = 0x010E,
			WM_IME_COMPOSITION        = 0x010F,
			WM_IME_KEYLAST            = 0x010F,
			WM_INITDIALOG             = 0x0110,
			WM_COMMAND                = 0x0111,
			WM_SYSCOMMAND             = 0x0112,
			WM_TIMER                  = 0x0113,
			WM_HSCROLL                = 0x0114,
			WM_VSCROLL                = 0x0115,
			WM_INITMENU               = 0x0116,
			WM_INITMENUPOPUP          = 0x0117,
			WM_MENUSELECT             = 0x011F,
			WM_MENUCHAR               = 0x0120,
			WM_ENTERIDLE              = 0x0121,
			WM_MENURBUTTONUP          = 0x0122,
			WM_MENUDRAG               = 0x0123,
			WM_MENUGETOBJECT          = 0x0124,
			WM_UNINITMENUPOPUP        = 0x0125,
			WM_MENUCOMMAND            = 0x0126,
			WM_CTLCOLORMSGBOX         = 0x0132,
			WM_CTLCOLOREDIT           = 0x0133,
			WM_CTLCOLORLISTBOX        = 0x0134,
			WM_CTLCOLORBTN            = 0x0135,
			WM_CTLCOLORDLG            = 0x0136,
			WM_CTLCOLORSCROLLBAR      = 0x0137,
			WM_CTLCOLORSTATIC         = 0x0138,
			WM_MOUSEMOVE              = 0x0200,
			WM_LBUTTONDOWN            = 0x0201,
			WM_LBUTTONUP              = 0x0202,
			WM_LBUTTONDBLCLK          = 0x0203,
			WM_RBUTTONDOWN            = 0x0204,
			WM_RBUTTONUP              = 0x0205,
			WM_RBUTTONDBLCLK          = 0x0206,
			WM_MBUTTONDOWN            = 0x0207,
			WM_MBUTTONUP              = 0x0208,
			WM_MBUTTONDBLCLK          = 0x0209,
			WM_MOUSEWHEEL             = 0x020A,
			WM_XBUTTONDOWN            = 0x020B,
			WM_XBUTTONUP              = 0x020C,
			WM_XBUTTONDBLCLK          = 0x020D,
			WM_PARENTNOTIFY           = 0x0210,
			WM_ENTERMENULOOP          = 0x0211,
			WM_EXITMENULOOP           = 0x0212,
			WM_NEXTMENU               = 0x0213,
			WM_SIZING                 = 0x0214,
			WM_CAPTURECHANGED         = 0x0215,
			WM_MOVING                 = 0x0216,
			WM_DEVICECHANGE           = 0x0219,
			WM_MDICREATE              = 0x0220,
			WM_MDIDESTROY             = 0x0221,
			WM_MDIACTIVATE            = 0x0222,
			WM_MDIRESTORE             = 0x0223,
			WM_MDINEXT                = 0x0224,
			WM_MDIMAXIMIZE            = 0x0225,
			WM_MDITILE                = 0x0226,
			WM_MDICASCADE             = 0x0227,
			WM_MDIICONARRANGE         = 0x0228,
			WM_MDIGETACTIVE           = 0x0229,
			WM_MDISETMENU             = 0x0230,
			WM_ENTERSIZEMOVE          = 0x0231,
			WM_EXITSIZEMOVE           = 0x0232,
			WM_DROPFILES              = 0x0233,
			WM_MDIREFRESHMENU         = 0x0234,
			WM_IME_SETCONTEXT         = 0x0281,
			WM_IME_NOTIFY             = 0x0282,
			WM_IME_CONTROL            = 0x0283,
			WM_IME_COMPOSITIONFULL    = 0x0284,
			WM_IME_SELECT             = 0x0285,
			WM_IME_CHAR               = 0x0286,
			WM_IME_REQUEST            = 0x0288,
			WM_IME_KEYDOWN            = 0x0290,
			WM_IME_KEYUP              = 0x0291,
			WM_MOUSEHOVER             = 0x02A1,
			WM_MOUSELEAVE             = 0x02A3,
			WM_CUT                    = 0x0300,
			WM_COPY                   = 0x0301,
			WM_PASTE                  = 0x0302,
			WM_CLEAR                  = 0x0303,
			WM_UNDO                   = 0x0304,
			WM_RENDERFORMAT           = 0x0305,
			WM_RENDERALLFORMATS       = 0x0306,
			WM_DESTROYCLIPBOARD       = 0x0307,
			WM_DRAWCLIPBOARD          = 0x0308,
			WM_PAINTCLIPBOARD         = 0x0309,
			WM_VSCROLLCLIPBOARD       = 0x030A,
			WM_SIZECLIPBOARD          = 0x030B,
			WM_ASKCBFORMATNAME        = 0x030C,
			WM_CHANGECBCHAIN          = 0x030D,
			WM_HSCROLLCLIPBOARD       = 0x030E,
			WM_QUERYNEWPALETTE        = 0x030F,
			WM_PALETTEISCHANGING      = 0x0310,
			WM_PALETTECHANGED         = 0x0311,
			WM_HOTKEY                 = 0x0312,
			WM_PRINT                  = 0x0317,
			WM_PRINTCLIENT            = 0x0318,
			WM_HANDHELDFIRST          = 0x0358,
			WM_HANDHELDLAST           = 0x035F,
			WM_AFXFIRST               = 0x0360,
			WM_AFXLAST                = 0x037F,
			WM_PENWINFIRST            = 0x0380,
			WM_PENWINLAST             = 0x038F,
			WM_APP                    = 0x8000,
			WM_USER                   = 0x0400
		}

		private const int GWL_STYLE = -16;
		public const int TVS_INFOTIP = 0x0800;

		#endregion

		#region structs/classes

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT {
			public int x;
			public int y;
		}
		/// <summary>
		/// The RECT structure defines the coordinates of the upper-left 
		/// and lower-right corners of a rectangle
		/// </summary>
		[Serializable(), StructLayout(LayoutKind.Sequential)]
			public struct RECT {
			/// <summary>Specifies the x-coordinate of the upper-left corner of the rectangle</summary>
			public int left;
			/// <summary>Specifies the y-coordinate of the upper-left corner of the rectangle</summary>
			public int top;
			/// <summary>Specifies the x-coordinate of the lower-right corner of the rectangle</summary>
			public int right;
			/// <summary>Specifies the y-coordinate of the lower-right corner of the rectangle</summary>
			public int bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
			public struct API_STARTUPINFO {
			public int cb;

			/*
			 * The reason they can't declare those 3 fields as strings is that the 
			 * Interop marshaler, after having converted the string to a managed string, 
			 * will release the native data using CoTaskMemFree. 
			 * This is clearly not the right thing to do in this case so they need to declare 
			 * the fields as IntPtrs and then manually marshal them to strings via the 
			 * Marshal.PtrToStringUni() API. 
			 * 
			 * Wrong:
				[MarshalAs(UnmanagedType.LPWStr)] public string lpReserved;
				[MarshalAs(UnmanagedType.LPWStr)] public string lpDesktop;
				[MarshalAs(UnmanagedType.LPWStr)] public string lpTitle;
			 *	
			 * Working:
			 */
			public IntPtr lpReserved; 
			public IntPtr lpDesktop; 
			public IntPtr lpTitle; 

			public int dwX;
			public int dwY;
			public int dwXSize;
			public int dwYSize;
			public int dwXCountChars;
			public int dwYCountChars;
			public int dwFillAttribute;
			public int dwFlags;
			public short wShowWindow;
			public short cbReserved2;
			public IntPtr lpReserved2;
			public IntPtr hStdInput;
			public IntPtr hStdOutput;
			public IntPtr hStdError;
		}

		[StructLayout(LayoutKind.Sequential)]
			public class HDITEM {
			public int     mask; 
			public int     cxy = 0; 
			public	IntPtr pszText; 
			public IntPtr  hbm; 
			public int     cchTextMax; 
			public int     fmt; 
			public int     lParam = 0; 
			public int     iImage = 0;
			public int     iOrder = 0;
		};

		/// <summary>
		/// Receives dynamic-link library (DLL)-specific version information. 
		/// It is used with the DllGetVersion function
		/// </summary>
		[Serializable(), 
			StructLayout(LayoutKind.Sequential)]
			public struct DLLVERSIONINFO {
			/// <summary>
			/// Size of the structure, in bytes. This member must be filled 
			/// in before calling the function
			/// </summary>
			public int cbSize;

			/// <summary>
			/// Major version of the DLL. If the DLL's version is 4.0.950, 
			/// this value will be 4
			/// </summary>
			public int dwMajorVersion;

			/// <summary>
			/// Minor version of the DLL. If the DLL's version is 4.0.950, 
			/// this value will be 0
			/// </summary>
			public int dwMinorVersion;

			/// <summary>
			/// Build number of the DLL. If the DLL's version is 4.0.950, 
			/// this value will be 950
			/// </summary>
			public int dwBuildNumber;

			/// <summary>
			/// Identifies the platform for which the DLL was built
			/// </summary>
			public int dwPlatformID;
		}

		#endregion

		#region interop decl.
		[DllImport("User32.dll")] public static extern 
			bool SendMessage(IntPtr hWnd, int msg, int wParam, int lParam); 
		[DllImport("user32", EntryPoint="SendMessage")] public static extern 
			IntPtr SendMessage2(IntPtr Handle, int msg, IntPtr wParam, HDITEM lParam);
		[DllImport("user32", EntryPoint="SendMessage")] public static extern 
			IntPtr SendMessage3(IntPtr Handle, int msg, IntPtr wParam, IntPtr lParam);
		[DllImport("User32.dll", SetLastError=true)] public static extern 
			uint SetForegroundWindow (IntPtr hwnd);
		[DllImport("User32.dll", SetLastError=true)] public static extern 
			uint SetForegroundWindow (HandleRef hwnd);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]	static public extern 
			bool ShowWindow(IntPtr hWnd, ShowWindowStyles State);
		[DllImport("user32.dll")] public static extern 
			bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
		[DllImport("user32.dll")] public static extern 
			bool IsIconic(IntPtr hWnd);
		[DllImport("user32.dll", SetLastError=true)] public static extern 
			IntPtr FindWindow(string lpClassName, string lpWindowName);
		[DllImport("user32.dll")] public static extern 
			IntPtr GetWindowThreadProcessId(IntPtr hWnd, ref IntPtr ProcessId);
		[DllImport("user32.dll", SetLastError=true)] public static extern 
			int GetWindowText(IntPtr hWnd, System.Text.StringBuilder title, int size);
		[DllImport("user32.dll")] public static extern 
			int EnumWindows(EnumWindowsProc ewp, int lParam); 
		[DllImport("User32", CharSet=CharSet.Auto)] public static extern
			int GetWindowLong(IntPtr hWnd, int Index);
		[DllImport("User32", CharSet=CharSet.Auto)] public static extern
			int SetWindowLong(IntPtr hWnd, int Index, int Value);

		[DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)] public static extern 
			bool GetCursorPos(out POINT pt);
		[DllImport("user32.dll")] public static extern 
			bool TrackPopupMenuEx(HandleRef hmenu, uint fuFlags, int x, int y, HandleRef hwnd, IntPtr lptpm);
		[DllImport("user32.dll")] public static extern 
			bool PostMessage(HandleRef hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		//delegate used for EnumWindows() callback function
		public delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);
		
		[DllImport("Kernel32.dll", CharSet=CharSet.Auto)] public static extern 
			void GetStartupInfo(ref API_STARTUPINFO info);

		/// <summary>
		/// Implemented by many of the Microsoft® Windows® Shell dynamic-link libraries 
		/// (DLLs) to allow applications to obtain DLL-specific version information
		/// </summary>
		/// <param name="pdvi">Pointer to a DLLVERSIONINFO structure that receives the 
		/// version information. The cbSize member must be filled in before calling 
		/// the function</param>
		/// <returns>Returns NOERROR if successful, or an OLE-defined error value otherwise</returns>
		[DllImport("Comctl32.dll")] 
		public static extern int DllGetVersion(ref DLLVERSIONINFO pdvi);

		#endregion

		#region Registry stuff
		
		/// <summary>
		/// Wrap the windows registry access needed for Bandit
		/// </summary>
		public class Registry {
			
			private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(Registry));
			private static string BanditSettings = @"Software\RssBandit\Settings";

			/// <summary>
			/// Set/Get the Port number to be used by the Single Instance Activator.
			/// </summary>
			public static int InstanceActivatorPort {
				get {
					try {
						int retval = 0;
						RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(BanditSettings, false);
						string val = ((key == null) ? null : (key.GetValue("InstanceActivatorPort") as string));
						if (val != null && val.Trim().Length > 0) {
							try {
								int iConfPort = Int32.Parse(val);
								retval = iConfPort;
							} catch {}
						}
						return retval;
					} catch (Exception) {
						return 0;
					}
				}
				set {
					try {
						int newPort = value;
						RegistryKey keySettings = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(BanditSettings, true);
						if (keySettings == null) {
							keySettings = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(BanditSettings);
						}
						keySettings.SetValue("InstanceActivatorPort", newPort.ToString());
					} catch (Exception) {}
				}
			}
			/// <summary>
			/// Set/Get the current "feed:" Url protocol handler. 
			/// Provide the complete executable file path name.
			/// </summary>
			/// <exception cref="Exception">On set, if there are no rights to write the value</exception>
			public static string CurrentFeedProtocolHandler {
				get {
					try {
						//RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"feed\shell\open\command", false);
						RegistryKey key = ClassesRootKey().OpenSubKey(@"feed\shell\open\command", false);
						string val = ((key == null) ? null : (key.GetValue(null) as string));
						return val;
					} catch (Exception) {
						return null;
					}
				}
				set {
					string appExePath = value;
					RegistryKey keyFeed = ClassesRootKey().OpenSubKey(@"feed", true);
					if (keyFeed == null) {
						keyFeed = ClassesRootKey(true).CreateSubKey(@"feed");
					}
					keyFeed.SetValue(null, "URL:feed protocol");
					keyFeed.SetValue("URL Protocol", "");
					RegistryKey keyIcon = keyFeed.OpenSubKey("DefaultIcon", true);
					if (keyIcon == null) {
						keyIcon = keyFeed.CreateSubKey("DefaultIcon");
					}
					keyIcon.SetValue(null, appExePath+",0");
					RegistryKey keyFeedSub = keyFeed.OpenSubKey(@"shell\open\command", true);
					if (keyFeedSub == null) {
						keyFeedSub = keyFeed.CreateSubKey(@"shell\open\command");
					}
					keyFeedSub.SetValue(null, String.Concat(appExePath, " ", "\"",  "%1", "\""));
				}
			}
			
			#region Internet Explorer Menu Extension handling

			public enum IEMenuExtension {
				DefaultFeedAggregator,
				Bandit
			}

			/// <summary>
			/// For more infos read:
			/// http://msdn.microsoft.com/library/default.asp?url=/workshop/browser/ext/tutorials/context.asp
			/// </summary>
			public static bool IsInternetExplorerExtensionRegistered(IEMenuExtension extension) {

				string scriptName = GetIEExtensionScriptName(extension);
				string keyName = FindIEExtensionKey(extension);
				
				if (keyName != null) {
					if (!File.Exists(Path.Combine(RssBanditApplication.GetUserPath(), scriptName))) {
						try {	// delete bad entries (pointing to a missing file)
							Microsoft.Win32.Registry.CurrentUser.DeleteSubKey(String.Format(@"Software\Microsoft\Internet Explorer\MenuExt\{0}", keyName), false);
							keyName = null;
						} catch (Exception) {
							return true;
						}
					}
				}

				return (keyName != null);
			}

			public static void RegisterInternetExplorerExtension(IEMenuExtension extension) {
				WriteIEExtensionScript(extension);
				WriteIEExtensionRegistryEntry(extension);
			}

			public static void UnRegisterInternetExplorerExtension(IEMenuExtension extension) {
				DeleteIEExtensionRegistryEntry(extension);
				DeleteIEExtensionScript(extension);
			}

			private static string FindIEExtensionKey(IEMenuExtension extension) {
				
				string scriptName = GetIEExtensionScriptName(extension);
				try {
					RegistryKey menuBase = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\MenuExt", false);
					foreach (string skey in menuBase.GetSubKeyNames()) {
						RegistryKey subMenu = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(String.Format(@"Software\Microsoft\Internet Explorer\MenuExt\{0}", skey), false);
						// we test for the default entry in the above Reg.Hive:
						string defVal = ((subMenu == null) ? null : (subMenu.GetValue(null) as string));
						if (defVal != null && defVal.EndsWith(scriptName)) {
							return skey;
						}
					}

					return null;

				} catch (Exception ex) {
					_log.Error("Registry:FindIEExtensionKey() cause exception", ex);
					return null;
				}
			}

			private static void WriteIEExtensionRegistryEntry(IEMenuExtension extension) {
				
				string scriptName = GetIEExtensionScriptName(extension);
				string caption = null;
				if (extension == IEMenuExtension.DefaultFeedAggregator) {
					caption = Resource.Manager["RES_InternetExplorerMenuExtDefaultCaption"];
				} else {
					caption = Resource.Manager["RES_InternetExplorerMenuExtBanditCaption"];
				}

				try {
					
					RegistryKey menuBase = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\MenuExt", true);
					RegistryKey subMenu = menuBase.OpenSubKey(caption, true);
					if (subMenu == null) {
						subMenu = menuBase.CreateSubKey(caption);
					}
					subMenu.SetValue(null, Path.Combine(RssBanditApplication.GetUserPath(), scriptName));
					subMenu.SetValue("contexts", InternetExplorerExtensionsContexts);

				} catch (Exception ex) {
					_log.Error("Registry:WriteIEExtensionRegistryEntry() cause exception", ex);
				}
			}

			private static void WriteIEExtensionScript(IEMenuExtension extension) {
				
				string scriptName = GetIEExtensionScriptName(extension);

				try {
					
					string scriptContent = null;

					using (Stream resStream = Resource.Manager.GetStream("Resources."+scriptName)) {
						StreamReader reader = new StreamReader(resStream);
						scriptContent = reader.ReadToEnd();
					}
					
					if (scriptContent != null) {
						if (extension == IEMenuExtension.Bandit) {	// set the path to the exe within script
							scriptContent = String.Format(scriptContent, Application.ExecutablePath);
						}

						using (Stream outStream = NewsComponents.Utils.FileHelper.OpenForWrite(Path.Combine(RssBanditApplication.GetUserPath(), scriptName))) {
							StreamWriter writer = new StreamWriter(outStream);
							writer.Write(scriptContent);
							writer.Flush();
						}
					}

				} catch (Exception ex) {
					_log.Error("Registry:WriteIEExtensionScript("+scriptName+") cause exception",ex);
					throw;
				}
			}
			
			private static void DeleteIEExtensionRegistryEntry(IEMenuExtension extension) {
				string keyName = FindIEExtensionKey(extension);
				if (keyName != null) {
					Microsoft.Win32.Registry.CurrentUser.DeleteSubKey(String.Format(@"Software\Microsoft\Internet Explorer\MenuExt\{0}", keyName), false);
				}
			}

			private static void DeleteIEExtensionScript(IEMenuExtension extension) {
				string scriptName = GetIEExtensionScriptName(extension);
				string scriptPath = Path.Combine(RssBanditApplication.GetUserPath(), scriptName);
				if (File.Exists(scriptPath)) {
					NewsComponents.Utils.FileHelper.Delete(scriptPath);
				}
			}

			private static string GetIEExtensionScriptName(IEMenuExtension extension) {
				return (extension == IEMenuExtension.DefaultFeedAggregator ? InternetExplorerExtensionsExecFeedScript : InternetExplorerExtensionsExecBanditScript);
			}

			private const string InternetExplorerExtensionsExecFeedScript = "iecontext_subscribefeed.htm";
			private const string InternetExplorerExtensionsExecBanditScript = "iecontext_subscribebandit.htm";
			/// <summary>
			/// Context Value is integer build from flag(s):
			///		Default 0x1 
			///		Images 0x2 
			///		Controls 0x4 
			///		Tables 0x8 
			///		Text selection 0x10 
			///		Anchor 0x20 
			/// </summary>
			private const int InternetExplorerExtensionsContexts = 0x22;

			#endregion

			private static RegistryKey ClassesRootKey() {
				return ClassesRootKey(false);
			}
			
			private static RegistryKey ClassesRootKey(bool writable) {
					if (Win32.IsOSAtLeastWindows2000)
					return Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Classes", writable);
				else
					return Microsoft.Win32.Registry.ClassesRoot;
			}

			private Registry(){}
		}

		#endregion
		
		private Win32() {}

		private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(Win32));
		private static int _paintFrozen = 0;

		#region General

		/// <summary>
		/// Returns true, if the OS is at least Windows 2000 (or higher), else false.
		/// </summary>
		public static bool IsOSAtLeastWindows2000 {
			get { 
				if (Environment.OSVersion.Platform != PlatformID.Win32NT)
					return false;
				return (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 5 );
			}
		}

		/// <summary>
		/// Returns true, if the OS is at least Windows XP (or higher), else false.
		/// </summary>
		public static bool IsOSAtLeastWindowsXP {
			get { 
				return (Environment.OSVersion.Platform == PlatformID.Win32NT && (Environment.OSVersion.Version.Major > 5 || (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1)));
			}
		}

		/// <summary>
		/// API Wrapper to retrive the startup process window state.
		/// </summary>
		/// <returns>FormWindowState</returns>
		public static System.Windows.Forms.FormWindowState GetStartupWindowState() {
			API_STARTUPINFO sti = new API_STARTUPINFO();
			try {
				sti.cb = Marshal.SizeOf(typeof(API_STARTUPINFO));
				GetStartupInfo(ref sti);
				//System.Diagnostics.Process.GetCurrentProcess().StartInfo
				//System.Windows.Forms.MessageBox.Show("sti.wShowWindow is: "+sti.wShowWindow.ToString());

				if (sti.wShowWindow == (short)ShowWindowStyles.SW_MINIMIZE || sti.wShowWindow == (short)ShowWindowStyles.SW_SHOWMINIMIZED ||
					sti.wShowWindow == (short)ShowWindowStyles.SW_SHOWMINNOACTIVE || sti.wShowWindow == (short)ShowWindowStyles.SW_FORCEMINIMIZE)
					return System.Windows.Forms.FormWindowState.Minimized;
				else if (sti.wShowWindow == (short)ShowWindowStyles.SW_MAXIMIZE || sti.wShowWindow == (short)ShowWindowStyles.SW_SHOWMAXIMIZED ||
					sti.wShowWindow == (short)ShowWindowStyles.SW_MAX)
					return System.Windows.Forms.FormWindowState.Maximized;
				
			} catch (Exception e) {
				_log.Error("GetStartupWindowState() caused exception", e);
			}
			return System.Windows.Forms.FormWindowState.Normal;
		}

		/// <summary>
		/// Helper routine to modify the window style of any native windows widget
		/// </summary>
		/// <param name="hWnd">widget handle</param>
		/// <param name="styleToRemove">Style to be removed. Should be 0 (zero), if
		/// you only want to add a style.</param>
		/// <param name="styleToAdd">Style to add. Should be 0 (zero), if
		/// you only want to remove a style.</param>
		public static void ModifyWindowStyle(IntPtr hWnd, int styleToRemove, int styleToAdd) {
			int style = GetWindowLong(hWnd, GWL_STYLE);
			style &= ~styleToRemove;
			style |= styleToAdd;
			SetWindowLong(hWnd, GWL_STYLE, style);
		}

			
		/// <summary>
		/// </summary>
		/// <param name="ctrl"></param>
		/// <param name="freeze"></param>
		public static void FreezePainting (Control ctrl, bool freeze) { 
		
			if (freeze && ctrl != null && ctrl.IsHandleCreated && ctrl.Visible) { 
				if (0 == _paintFrozen++) { 
					SendMessage(ctrl.Handle, (int) Message.WM_SETREDRAW, 0, 0); 
				} 
			} 
			if (!freeze) { 
				if (_paintFrozen == 0) { 
					return; 
				} 
  
				if (0 == --_paintFrozen && ctrl != null) { 
					SendMessage(ctrl.Handle, (int)Message.WM_SETREDRAW, 1, 0); 
					ctrl.Invalidate(true); 
				} 
			} 
		}

		public static int HIWORD(IntPtr x) {
			return (unchecked((int)(long)x) >> 16) & 0xffff;
		}
    
		public static int LOWORD(IntPtr x) {
			return unchecked((int)(long)x) & 0xffff;
		}

		#endregion

	}

	#region UxTheme
	
	/*
	* Copyright © 2004-2005, Mathew Hall
	* All rights reserved.
	*
	* Redistribution and use in source and binary forms, with or without modification, 
	* are permitted provided that the following conditions are met:
	*
	*    - Redistributions of source code must retain the above copyright notice, 
	*      this list of conditions and the following disclaimer.
	* 
	*    - Redistributions in binary form must reproduce the above copyright notice, 
	*      this list of conditions and the following disclaimer in the documentation 
	*      and/or other materials provided with the distribution.
	*
	* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
	* ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
	* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
	* IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
	* INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
	* NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, 
	* OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
	* WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
	* ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY 
	* OF SUCH DAMAGE.
	*/

	/// <summary>
	/// A class that wraps Windows XPs UxTheme.dll
	/// </summary>
	public sealed class UxTheme {
		/// <summary>
		/// Private constructor
		/// </summary>
		private UxTheme() {
			
		}


		/// <summary>
		/// Reports whether the current application's user interface 
		/// displays using visual styles
		/// </summary>
		public static bool AppThemed {
			get {
				bool themed = false;
			
				OperatingSystem os = System.Environment.OSVersion;

				// check if the OS id XP or higher
				// fix:	Win2k3 now recognised
				//      Russkie (codeprj@webcontrol.net.au)
				if (os.Platform == PlatformID.Win32NT && ((os.Version.Major == 5 && os.Version.Minor >= 1) || os.Version.Major > 5)) {
					themed = IsAppThemed();
				}

				return themed;
			}
		}


		/// <summary>
		/// Retrieves the name of the current visual style
		/// </summary>
		public static String ThemeName {
			get {
				StringBuilder themeName = new StringBuilder(256);

				GetCurrentThemeName(themeName, 256, null, 0, null, 0);

				return themeName.ToString();
			}
		}


		/// <summary>
		/// Retrieves the color scheme name of the current visual style
		/// </summary>
		public static String ColorName {
			get {
				StringBuilder themeName = new StringBuilder(256);
				StringBuilder colorName = new StringBuilder(256);

				GetCurrentThemeName(themeName, 256, colorName, 256, null, 0);

				return colorName.ToString();
			}
		}


		#region Win32 Methods

		/// <summary>
		/// Opens the theme data for a window and its associated class
		/// </summary>
		/// <param name="hwnd">Handle of the window for which theme data 
		/// is required</param>
		/// <param name="pszClassList">Pointer to a string that contains 
		/// a semicolon-separated list of classes</param>
		/// <returns>OpenThemeData tries to match each class, one at a 
		/// time, to a class data section in the active theme. If a match 
		/// is found, an associated HTHEME handle is returned. If no match 
		/// is found NULL is returned</returns>
		[DllImport("UxTheme.dll")]
		public static extern IntPtr OpenThemeData(IntPtr hwnd, [MarshalAs(UnmanagedType.LPTStr)] string pszClassList);


		/// <summary>
		/// Closes the theme data handle
		/// </summary>
		/// <param name="hTheme">Handle to a window's specified theme data. 
		/// Use OpenThemeData to create an HTHEME</param>
		/// <returns>Returns S_OK if successful, or an error value otherwise</returns>
		[DllImport("UxTheme.dll")]
		public static extern int CloseThemeData(IntPtr hTheme);


		/// <summary>
		/// Draws the background image defined by the visual style for the 
		/// specified control part
		/// </summary>
		/// <param name="hTheme">Handle to a window's specified theme data. 
		/// Use OpenThemeData to create an HTHEME</param>
		/// <param name="hdc">Handle to a device context (HDC) used for 
		/// drawing the theme-defined background image</param>
		/// <param name="iPartId">Value of type int that specifies the part 
		/// to draw</param>
		/// <param name="iStateId">Value of type int that specifies the state 
		/// of the part to draw</param>
		/// <param name="pRect">Pointer to a RECT structure that contains the 
		/// rectangle, in logical coordinates, in which the background image 
		/// is drawn</param>
		/// <param name="pClipRect">Pointer to a RECT structure that contains 
		/// a clipping rectangle. This parameter may be set to NULL</param>
		/// <returns>Returns S_OK if successful, or an error value otherwise</returns>
		[DllImport("UxTheme.dll")]
		public static extern int DrawThemeBackground(IntPtr hTheme, IntPtr hdc, int iPartId, int iStateId, ref Win32.RECT pRect, ref Win32.RECT pClipRect);


		/// <summary>
		/// Tests if a visual style for the current application is active
		/// </summary>
		/// <returns>TRUE if a visual style is enabled, and windows with 
		/// visual styles applied should call OpenThemeData to start using 
		/// theme drawing services, FALSE otherwise</returns>
		[DllImport("UxTheme.dll")]
		public static extern bool IsThemeActive();


		/// <summary>
		/// Reports whether the current application's user interface 
		/// displays using visual styles
		/// </summary>
		/// <returns>TRUE if the application has a visual style applied,
		/// FALSE otherwise</returns>
		[DllImport("UxTheme.dll")]
		public static extern bool IsAppThemed();


		/// <summary>
		/// Retrieves the name of the current visual style, and optionally retrieves the 
		/// color scheme name and size name
		/// </summary>
		/// <param name="pszThemeFileName">Pointer to a string that receives the theme 
		/// path and file name</param>
		/// <param name="dwMaxNameChars">Value of type int that contains the maximum 
		/// number of characters allowed in the theme file name</param>
		/// <param name="pszColorBuff">Pointer to a string that receives the color scheme 
		/// name. This parameter may be set to NULL</param>
		/// <param name="cchMaxColorChars">Value of type int that contains the maximum 
		/// number of characters allowed in the color scheme name</param>
		/// <param name="pszSizeBuff">Pointer to a string that receives the size name. 
		/// This parameter may be set to NULL</param>
		/// <param name="cchMaxSizeChars">Value of type int that contains the maximum 
		/// number of characters allowed in the size name</param>
		/// <returns>Returns S_OK if successful, otherwise an error code</returns>
		[DllImport("UxTheme.dll", ExactSpelling=true, CharSet=CharSet.Unicode)]
		internal static extern int GetCurrentThemeName(System.Text.StringBuilder pszThemeFileName, int dwMaxNameChars, System.Text.StringBuilder pszColorBuff, int cchMaxColorChars, System.Text.StringBuilder pszSizeBuff, int cchMaxSizeChars);


		/// <summary>
		/// Draws the part of a parent control that is covered by a 
		/// partially-transparent or alpha-blended child control
		/// </summary>
		/// <param name="hwnd">Handle of the child control</param>
		/// <param name="hdc">Handle to the child control's device context </param>
		/// <param name="prc">Pointer to a RECT structure that defines the 
		/// area to be drawn. The rectangle is in the child window's coordinates. 
		/// This parameter may be set to NULL</param>
		/// <returns>Returns S_OK if successful, or an error value otherwise</returns>
		[DllImport("UxTheme.dll")]
		public static extern int DrawThemeParentBackground(IntPtr hwnd, IntPtr hdc, ref Win32.RECT prc);
	
		#endregion
	


		#region WindowClasses

		/// <summary>
		/// Window class IDs used by UxTheme.dll to draw controls
		/// </summary>
		public class WindowClasses {
			/// <summary>
			/// TextBox class
			/// </summary>
			public static readonly string Edit = "EDIT";
			
			/// <summary>
			/// ListView class
			/// </summary>
			public static readonly string ListView = "LISTVIEW";
			
			/// <summary>
			/// TreeView class
			/// </summary>
			public static readonly string TreeView = "TREEVIEW";
		}

		#endregion



		#region Parts

		/// <summary>
		/// Window parts IDs used by UxTheme.dll to draw controls
		/// </summary>
		public class Parts {
			#region Edit

			/// <summary>
			/// TextBox parts
			/// </summary>
			public enum Edit {
				/// <summary>
				/// TextBox
				/// </summary>
				EditText = 1
			}

			#endregion


			#region ListView

			/// <summary>
			/// ListView parts
			/// </summary>
			public enum ListView {
				/// <summary>
				/// ListView
				/// </summary>
				ListItem = 1
			}

			#endregion


			#region TreeView

			/// <summary>
			/// TreeView parts
			/// </summary>
			public enum TreeView {
				/// <summary>
				/// TreeView
				/// </summary>
				TreeItem = 1
			}

			#endregion
		}

		#endregion



		#region PartStates

		/// <summary>
		/// Window part state IDs used by UxTheme.dll to draw controls
		/// </summary>
		public class PartStates {
			#region EditParts

			/// <summary>
			/// TextBox part states
			/// </summary>
			public enum EditText {
				/// <summary>
				/// The TextBox is in its normal state
				/// </summary>
				Normal = 1,
				
				/// <summary>
				/// The mouse is over the TextBox
				/// </summary>
				Hot = 2,
				
				/// <summary>
				/// The TextBox is selected
				/// </summary>
				Selected = 3,
				
				/// <summary>
				/// The TextBox is disabled
				/// </summary>
				Disabled = 4,
				
				/// <summary>
				/// The TextBox currently has focus
				/// </summary>
				Focused = 5,
				
				/// <summary>
				/// The TextBox is readonly
				/// </summary>
				Readonly = 6
			}

			#endregion


			#region ListViewParts

			/// <summary>
			/// ListView part states
			/// </summary>
			public enum ListItem {
				/// <summary>
				/// The ListView is in its normal state
				/// </summary>
				Normal = 1,
				
				/// <summary>
				/// The mouse is over the ListView
				/// </summary>
				Hot = 2,
				
				/// <summary>
				/// The ListView is selected
				/// </summary>
				Selected = 3,
				
				/// <summary>
				/// The ListView is disabled
				/// </summary>
				Disabled = 4,
				
				/// <summary>
				/// The ListView is selected but currently does not have focus
				/// </summary>
				SelectedNotFocused = 5
			}

			#endregion


			#region TreeViewParts

			/// <summary>
			/// TreeView part states
			/// </summary>
			public enum TreeItem {
				/// <summary>
				/// The TreeView is in its normal state
				/// </summary>
				Normal = 1,
				
				/// <summary>
				/// The mouse is over the TreeView
				/// </summary>
				Hot = 2,
				
				/// <summary>
				/// The TreeView is selected
				/// </summary>
				Selected = 3,
				
				/// <summary>
				/// The TreeView is disabled
				/// </summary>
				Disabled = 4,
				
				/// <summary>
				/// The TreeView is selected but currently does not have focus
				/// </summary>
				SelectedNotFocused = 5
			}

			#endregion
		}

		#endregion
	}

	#endregion

}
