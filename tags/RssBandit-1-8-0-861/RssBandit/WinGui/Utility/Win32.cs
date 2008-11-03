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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Interop.ThumbCache;
using Microsoft.Win32;
using NewsComponents.Utils;
using RssBandit.Resources;
using Application=System.Windows.Forms.Application;
using Logger = RssBandit.Common.Logging;

namespace RssBandit
{
	/// <summary>
	/// Common used Win32 Interop decl.
	/// </summary>
	[System.Security.SuppressUnmanagedCodeSecurity]
	internal sealed class Win32 {

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
		internal struct POINT {
			public int x;
			public int y;
		}
		/// <summary>
		/// The RECT structure defines the coordinates of the upper-left 
		/// and lower-right corners of a rectangle
		/// </summary>
		[Serializable, StructLayout(LayoutKind.Sequential)]
		internal struct RECT {
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
		internal struct API_STARTUPINFO {
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
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };


		[StructLayout(LayoutKind.Sequential)]
		internal class HDITEM {
			public int     mask; 
			public int     cxy; 
			public IntPtr  pszText = IntPtr.Zero; 
			public IntPtr  hbm = IntPtr.Zero; 
			public int     cchTextMax; 
			public int     fmt;
            public IntPtr lParam = IntPtr.Zero; 
			public int     iImage;
			public int     iOrder;
		};

		/// <summary>
		/// Receives dynamic-link library (DLL)-specific version information. 
		/// It is used with the DllGetVersion function
		/// </summary>
		[Serializable(), 
		StructLayout(LayoutKind.Sequential)]
		internal struct DLLVERSIONINFO {
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

		[StructLayout(LayoutKind.Sequential)]  
		private struct OSVERSIONINFOEX {
			public int dwOSVersionInfoSize;  
			public int dwMajorVersion;  
			public int dwMinorVersion;  
			public int dwBuildNumber;  
			public int dwPlatformId;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
			public string szCSDVersion;
			public UInt16 wServicePackMajor;  
			public UInt16 wServicePackMinor;  
			public UInt16 wSuiteMask;
			public byte wProductType;  
			public byte wReserved;
		}
		#endregion

		#region interop decl.
		[DllImport("User32.dll")] public static extern
            IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam); 
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
			int GetWindowText(IntPtr hWnd, StringBuilder title, int size);
		[DllImport("user32.dll")] public static extern 
			int EnumWindows(EnumWindowsProc ewp, IntPtr lParam); 
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
		public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
		
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

		[ DllImport( "kernel32.dll", SetLastError=true )]
		private static extern bool GetVersionEx(ref OSVERSIONINFOEX osvi );

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath,
                                    uint dwFileAttributes,
                                    ref SHFILEINFO psfi,
                                    uint cbSizeFileInfo,
                                    ShellFileInfoFlags uFlags);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);
           
        [DllImport("gdi32.dll")]
      private static extern bool DeleteObject(IntPtr handle);

        [DllImport("shell32.dll", SetLastError=true)]
        private static extern bool SHILCreateFromPath([MarshalAs(UnmanagedType.LPWStr)] string path, out IntPtr ppidl, ref IShellItem rgflnOut);
        /*
        HRESULT SHILCreateFromPath(LPCWSTR pszPath,
    PIDLIST_ABSOLUTE* ppidl,
    DWORD* rgflnOut
);*/
        /*
         HRESULT SHCreateShellItem(          PCIDLIST_ABSOLUTE pidlParent,
    IShellFolder *psfParent,
    PCUITEMID_CHILD pidl,
    IShellItem **ppsi
);*/

        // This is here just because the TlbImp'd one had an incorrect
        // signature on GetSharedBitmap
        [ComImport, InterfaceType((short)1), SuppressUnmanagedCodeSecurity, Guid("091162A4-BC96-411F-AAE8-C5122CD03363"), ComConversionLoss]
        private interface ISharedBitmap1
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetSharedBitmap([Out, ComAliasName("Interop.ThumbCache.wireHBITMAP")] out IntPtr phbm);
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetSize(out tagSIZE pSize);
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void GetFormat(out uint pat);
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void InitializeBitmap([In, ComAliasName("Interop.ThumbCache.wireHBITMAP")] ref _userHBITMAP hbm, [In] uint wtsAT);
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void Detach([Out, ComAliasName("Interop.ThumbCache.wireHBITMAP")] IntPtr phbm);
        }

 

        [DllImport("shell32.dll", SetLastError=true)]
        private static extern bool SHCreateShellItem(IntPtr pidlParent, IntPtr psfParent, IntPtr ppidl, out IShellItem ppsi);

        public static BitmapSource GetShellIconForFile(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            BitmapSource src = null;
            if(IsOSAtLeastWindowsVista)
            {
                src = GetVistaIconCache(fileName);
            }

            // fall back to regular

            if (src == null)
            {

                var info = new SHFILEINFO();

                SHGetFileInfo(fileName, 0, ref info, (uint) Marshal.SizeOf(info),
                              ShellFileInfoFlags.Icon | ShellFileInfoFlags.LargeIcon |
                              ShellFileInfoFlags.UseFileAttributes);

                src = Imaging.CreateBitmapSourceFromHIcon(info.hIcon, Int32Rect.Empty,
                                                          BitmapSizeOptions.FromEmptyOptions());

                DestroyIcon(info.hIcon);

                src.Freeze();
            }

            return src;
        }

        private static BitmapSource GetVistaIconCache(string fileName)
        {
            try
            {

                //int attrs = 0;
                IShellItem ppsi = null;
                IntPtr ppidl;

                SHILCreateFromPath(fileName, out ppidl, ref ppsi);


                if (ppidl == IntPtr.Zero)
                    return null;
                
                
                SHCreateShellItem(IntPtr.Zero, IntPtr.Zero, ppidl, out ppsi);

                IThumbnailCache cache = new LocalThumbnailCacheClass();

                SharedBitmap sb;

                uint size = 100;
                uint flag = 0; //WTS_EXTRACT
                uint outFlags = 0;

                tagTHUMBNAILID ttid = new tagTHUMBNAILID();
                cache.GetThumbnail(ppsi, size, flag, out sb, ref outFlags, ref ttid);

                // according to the docs, sb might be null
                if (sb != null)
                {
                    IntPtr hbmap = IntPtr.Zero;

                    ISharedBitmap1 sb1 = (ISharedBitmap1) sb;

                    sb1.GetSharedBitmap(out hbmap);

                    BitmapSource bs = Imaging.CreateBitmapSourceFromHBitmap(hbmap, IntPtr.Zero, Int32Rect.Empty,
                                                                            BitmapSizeOptions.FromEmptyOptions());

                    DeleteObject(hbmap);

                    return bs;
                }
            }
            catch
            {
            }
            return null;
        }

	    #endregion

		#region MultiMedia
		
		/// <summary>
		/// Plays a sound.
		/// </summary>
		/// <param name="sound">Sound to play</param>
		/// <param name="hModule">Handle to the executable file that contains 
		/// the resource to be loaded. This parameter must be IntPtr.Zero unless SND_RESOURCE is specified in fdwSound.</param>
		/// <param name="fdwSound">SoundFlags.SND_* combinations</param>
		/// <returns></returns>
		[DllImport("Winmm.dll", CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool PlaySound(string sound, IntPtr hModule, SoundFlags fdwSound);
		
		/// <summary>
		/// PlaySound function (fdwSound parameter) flags
		/// </summary>
		[Flags]
		private enum SoundFlags : uint {
			SND_SYNC = 0x0000,            // play synchronously (default)
			SND_ASYNC = 0x0001,        // play asynchronously
			SND_NODEFAULT = 0x0002,        // silence (!default) if sound not found
			SND_MEMORY = 0x0004,        // pszSound points to a memory file
			SND_LOOP = 0x0008,            // loop the sound until next sndPlaySound
			SND_NOSTOP = 0x0010,        // don't stop any currently playing sound
			SND_NOWAIT = 0x00002000,        // don't wait if the driver is busy
			SND_ALIAS = 0x00010000,        // name is a registry alias
			SND_ALIAS_ID = 0x00110000,        // alias is a predefined id
			SND_FILENAME = 0x00020000,        // name is file name
			SND_RESOURCE = 0x00040004,        // name is resource name or atom
//#if(WINVER >= 0x0400)
			SND_PURGE =    0x0040,  // purge non-static events for task 
			SND_APPLICATION = 0x0080,  // look for application specific association 
//#endif
		}

        [Flags]
        private enum ShellFileInfoFlags : uint
        {
            Icon = 0x100,
            LargeIcon = 0x0,
            SmallIcon = 0x1,
            UseFileAttributes = 0x000000010,

        }
		/// <summary>
		/// Sets/Gets if application sounds are anabled.
		/// </summary>
		public static bool ApplicationSoundsAllowed = false;
		
		/// <summary>
		/// Plays a application sound.
		/// </summary>
		/// <param name="applicationSound">The application sound.</param>
		public static void PlaySound(string applicationSound) {
			if (! ApplicationSoundsAllowed)
				return;
			
			if (RssBanditApplication.PortableApplicationMode) {
				PlaySoundFromFile(applicationSound);
			} else {
				PlaySoundFromRegistry(applicationSound);
			}
		}
		
		/// <summary>
		/// Plays a application sound as configured in the registry.
		/// </summary>
		/// <param name="applicationSound">The application sound.</param>
		private static void PlaySoundFromRegistry(string applicationSound) {
			
			try {
				// just to ensure only the predefined sounds are played:
				switch (applicationSound) {
					case Resource.ApplicationSound.FeedDiscovered:
						PlaySound(applicationSound, IntPtr.Zero, SoundFlags.SND_APPLICATION |
							SoundFlags.SND_NOWAIT | SoundFlags.SND_NODEFAULT);
						break;
					case Resource.ApplicationSound.NewItemsReceived:
						PlaySound(applicationSound, IntPtr.Zero, SoundFlags.SND_APPLICATION |
							SoundFlags.SND_NOWAIT | SoundFlags.SND_NODEFAULT);
						break;
					case Resource.ApplicationSound.NewAttachmentDownloaded:
						PlaySound(applicationSound, IntPtr.Zero, SoundFlags.SND_APPLICATION |
							SoundFlags.SND_NOWAIT | SoundFlags.SND_NODEFAULT);
						break;
				}
			} catch (Exception ex) {
				int err = Marshal.GetLastWin32Error();
				if (err != 0)
					_log.Error("Error #" + err + " occured on playing sound '" + applicationSound + "'", ex);
				else
					_log.Error("Error playing sound '" + applicationSound + "'", ex);
			}
		}
		
		private static void PlaySoundFromFile(string applicationSound) {
			try {
				string soundFile = null;
				// just to ensure only the predefined sounds are played:
				switch (applicationSound) {
					case Resource.ApplicationSound.FeedDiscovered:
						soundFile = Path.Combine(Application.StartupPath, @"Media\Feed Discovered.wav");
						break;
					case Resource.ApplicationSound.NewItemsReceived:
						soundFile = Path.Combine(Application.StartupPath, @"Media\New Feed Items Received.wav");
						break;
					case Resource.ApplicationSound.NewAttachmentDownloaded:
						soundFile = Path.Combine(Application.StartupPath, @"Media\New Attachment Downloaded.wav");
						break;
				}
				
				if (File.Exists(soundFile)) {
					PlaySound(soundFile, IntPtr.Zero, SoundFlags.SND_FILENAME |
						SoundFlags.SND_NOWAIT | SoundFlags.SND_ASYNC);
				}
				
			} catch (Exception ex) {
				_log.Error("Error playing sound '" + applicationSound + "'", ex);
			}
		}
		#endregion

		#region Registry stuff

		#region IRegistry interface

		internal interface IRegistry {
			/// <summary>
			/// Set/Get the Port number to be used by the Single Instance Activator.
			/// </summary>
			int InstanceActivatorPort {get ;set ;}

			/// <summary>
			/// Set/Get the current "feed:" Url protocol handler. 
			/// Provide the complete executable file path name.
			/// </summary>
			/// <exception cref="Exception">On set, if there are no rights to write the value</exception>
			string CurrentFeedProtocolHandler {get ;set ;}

			/// <summary>
			/// Checks and init RSS bandit sounds 
			/// (configurable via Windows Sounds Control Panel).
			/// </summary>
			/// <param name="appKey">The app key (file name without extension and path).
			/// E.g. "RssBandit".</param>
			/// <remarks>
			/// See also: http://blogs.msdn.com/larryosterman/archive/2006/01/24/517183.aspx
			/// </remarks>
			void CheckAndInitSounds(string appKey);

			/// <summary>
			/// Set to true to execute Bandit if windows user login to 
			/// the system, else false.
			/// </summary>
			bool RunAtStartup { get; set ;}
		

			/// <summary>
			/// For more infos read:
			/// http://msdn.microsoft.com/library/default.asp?url=/workshop/browser/ext/tutorials/context.asp
			/// </summary>
			bool IsInternetExplorerExtensionRegistered(IEMenuExtension extension) ;

			/// <summary>
			/// Registers the internet explorer extension.
			/// </summary>
			/// <param name="extension">The extension.</param>
			void RegisterInternetExplorerExtension(IEMenuExtension extension);

			/// <summary>
			/// Uns the register internet explorer extension.
			/// </summary>
			/// <param name="extension">The extension.</param>
			void UnRegisterInternetExplorerExtension(IEMenuExtension extension) ;

			/// <summary>
			/// Gets the internet explorer version.
			/// </summary>
			/// <returns></returns>
			Version GetInternetExplorerVersion();

			/// <summary>
			/// Gets or sets a value indicating whether [current RSS bandit version executes first time after installation].
			/// </summary>
			/// <value>
			/// 	<c>true</c> if [current RSS bandit version executes first time after installation]; otherwise, <c>false</c>.
			/// </value>
			bool ThisVersionExecutesFirstTimeAfterInstallation { get; set; }
		}

		#endregion

		public enum IEMenuExtension {
			DefaultFeedAggregator,
			Bandit
		}
		
		private static IRegistry registryInstance;
		/// <summary>
		/// Gets the registry instance.
		/// </summary>
		/// <value>The registry.</value>
		public static IRegistry Registry {
			get {
				if (registryInstance != null)
					return registryInstance;
				
				if (RssBanditApplication.PortableApplicationMode)
					registryInstance = new PortableRegistry();
				else
					registryInstance = new WindowsRegistry();
				
				return registryInstance;
			}
		}

		// used in win registry:
		private static readonly string versionTagName = String.Format("version.{0}", RssBanditApplication.Version.ToString(2));
		// used in portable registry:
		private static readonly string versionTagFile = String.Format(".version.{0}", RssBanditApplication.Version.ToString(2));
			
			
		/// <summary>
		/// Wrap the windows registry access needed for Bandit
		/// </summary>
		internal class WindowsRegistry : IRegistry
		{
			
			private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(Registry));
            private readonly static string BanditSettings;
			private readonly static string BanditKey = "RssBandit";

            static WindowsRegistry()
            {
                BanditSettings = @"Software\rssbandit.org\RssBandit\Settings";

#if ALT_CONFIG_PATH
                BanditSettings = @"Software\rssbandit.org\RssBandit\Settings\Debug";
#endif
			}

			/// <summary>
			/// Gets the internet explorer version.
			/// </summary>
			/// <returns></returns>
			internal static Version GetInternetExplorerVersion() { 
				RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\\Microsoft\\Internet Explorer", false);
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
			
			#region IRegistry

			/// <summary>
			/// Set/Get the Port number to be used by the Single Instance Activator.
			/// </summary>
			int IRegistry.InstanceActivatorPort {
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
						  if (key != null) key.Close();
						  return retval;
					  } catch (Exception ex) {
						  Win32._log.Error("Cannot get InstanceActivatorPort", ex);
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
						if (keySettings != null)
						{
							keySettings.SetValue("InstanceActivatorPort", newPort.ToString());
							keySettings.Close();
						}
					} catch (Exception ex) {
						Win32._log.Error("Cannot set InstanceActivatorPort", ex);
					} 
				}
			}

			/// <summary>
			/// Set/Get the current "feed:" Url protocol handler. 
			/// Provide the complete executable file path name.
			/// </summary>
			/// <exception cref="Exception">On set, if there are no rights to write the value</exception>
			string IRegistry.CurrentFeedProtocolHandler {
				get { 
					try {
						//RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"feed\shell\open\command", false);
						RegistryKey key = ClassesRootKey().OpenSubKey(@"feed\shell\open\command", false);
						string val = ((key == null) ? null : (key.GetValue(null) as string));
						if (key != null) key.Close();
						return val;
					} catch (Exception ex) {
						Win32._log.Error("Cannot get CurrentFeedProtocolHandler", ex);
						return null;
					} 
				}
				set {
					try {
						string appExePath = value;
						// UACManager registered action: 
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
						if (keyFeed != null) keyFeed.Close();
						if (keyIcon != null) keyIcon.Close();
						if (keyFeedSub != null) keyFeedSub.Close();
					} catch (SecurityException sec) {
						Win32._log.Error("Cannot set application as CurrentFeedProtocolHandler", sec);
					}
				}
			}

			/// <summary>
			/// Checks and init RSS bandit sounds 
			/// (configurable via Windows Sounds Control Panel).
			/// </summary>
			/// <param name="appKey">The app key (file name without extension and path).
			/// E.g. "RssBandit".</param>
			/// <remarks>
			/// See also: http://blogs.msdn.com/larryosterman/archive/2006/01/24/517183.aspx
			/// </remarks>
			void IRegistry.CheckAndInitSounds(string appKey) 
			{
				const string rootKey = "AppEvents\\Schemes\\Apps";
				const string rootLabels = "AppEvents\\EventLabels";
				const string appName = "RSS Bandit";
				const string defaultKeyName = ".default";
				const string currentKeyName = ".current";
			
				RegistryKey labelRoot = null, schemeRoot = null;
				// we use the installed OS UI Language to set our sound names:
				
				CultureInfo prev = RssBanditApplication.SharedUICulture;
				RssBanditApplication.SharedUICulture = CultureInfo.InstalledUICulture;
				
				try {
					// open root keys
					schemeRoot = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(rootKey, true);
					if (schemeRoot == null)
						return;
				
					labelRoot = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(rootLabels, true);
					if (labelRoot == null)
						return;
				
					RegistryKey appSchemeRoot;
					RegistryKey sndSchemeDefinition;
					RegistryKey sndSchemeLabel;
					RegistryKey currentKey, defaultKey;
				
					if (null == (appSchemeRoot = schemeRoot.OpenSubKey(appKey, true))) {
						appSchemeRoot = schemeRoot.CreateSubKey(appKey);
						appSchemeRoot.SetValue(null, appName);	// app name as displayed in SoundConfig Control Panel
					}
					
					// new feed(s) detected sound setup:
					string currentSndKey = Resource.ApplicationSound.FeedDiscovered;
					if (null == (sndSchemeDefinition = appSchemeRoot.OpenSubKey(currentSndKey, true))) {
						sndSchemeDefinition = appSchemeRoot.CreateSubKey(currentSndKey);
					}
					if (null == (currentKey = sndSchemeDefinition.OpenSubKey(currentKeyName, true))) {
						currentKey = sndSchemeDefinition.CreateSubKey(currentKeyName);
						// prefer to use the IE7 sound, if installed:
						if (File.Exists(Environment.ExpandEnvironmentVariables(@"%WinDir%\media\Windows Feed Discovered.wav"))) 
							currentKey.SetValue(null, Environment.ExpandEnvironmentVariables(@"%WinDir%\media\Windows Feed Discovered.wav"));
						else
							currentKey.SetValue(null, Path.Combine(Application.StartupPath, @"Media\Feed Discovered.wav"));
						currentKey.Close();
					}
					if (null == (defaultKey = sndSchemeDefinition.OpenSubKey(defaultKeyName, true))) {
						defaultKey = sndSchemeDefinition.CreateSubKey(defaultKeyName);
						defaultKey.SetValue(null, Path.Combine(Application.StartupPath, @"Media\Feed Discovered.wav"));
						defaultKey.Close();
					}
					sndSchemeDefinition.Close();
				
					if (null == (sndSchemeLabel = labelRoot.OpenSubKey(currentSndKey, true))) {
						sndSchemeLabel = labelRoot.CreateSubKey(currentSndKey);
					}
					sndSchemeLabel.SetValue(null, SR.WindowsSoundControlPanelNewFeedDiscovered);	
					sndSchemeLabel.Close();
				
					// new item(s) received sound setup:
					currentSndKey = Resource.ApplicationSound.NewItemsReceived;
					if (null == (sndSchemeDefinition = appSchemeRoot.OpenSubKey(currentSndKey, true))) {
						sndSchemeDefinition = appSchemeRoot.CreateSubKey(currentSndKey);
					}
					if (null == (currentKey = sndSchemeDefinition.OpenSubKey(currentKeyName, true))) {
						currentKey = sndSchemeDefinition.CreateSubKey(currentKeyName);
						currentKey.SetValue(null, Path.Combine(Application.StartupPath, @"Media\New Feed Items Received.wav"));
						currentKey.Close();
					}
					if (null == (defaultKey = sndSchemeDefinition.OpenSubKey(defaultKeyName, true))) {
						defaultKey = sndSchemeDefinition.CreateSubKey(defaultKeyName);
						defaultKey.SetValue(null, Path.Combine(Application.StartupPath, @"Media\New Feed Items Received.wav"));
						defaultKey.Close();
					}
				
					sndSchemeDefinition.Close();
				
					if (null == (sndSchemeLabel = labelRoot.OpenSubKey(currentSndKey, true))) {
						sndSchemeLabel = labelRoot.CreateSubKey(currentSndKey);
					}
					sndSchemeLabel.SetValue(null, SR.WindowsSoundControlPanelNewItemsReceived);
					sndSchemeLabel.Close();
					
					// new attachment(s) downloaded sound setup:
					currentSndKey = Resource.ApplicationSound.NewAttachmentDownloaded;
					if (null == (sndSchemeDefinition = appSchemeRoot.OpenSubKey(currentSndKey, true))) {
						sndSchemeDefinition = appSchemeRoot.CreateSubKey(currentSndKey);
					}
					if (null == (currentKey = sndSchemeDefinition.OpenSubKey(currentKeyName, true))) {
						currentKey = sndSchemeDefinition.CreateSubKey(currentKeyName);
						currentKey.SetValue(null, Path.Combine(Application.StartupPath, @"Media\New Attachment Downloaded.wav"));
						currentKey.Close();
					}
					if (null == (defaultKey = sndSchemeDefinition.OpenSubKey(defaultKeyName, true))) {
						defaultKey = sndSchemeDefinition.CreateSubKey(defaultKeyName);
						defaultKey.SetValue(null, Path.Combine(Application.StartupPath, @"Media\New Attachment Downloaded.wav"));
						defaultKey.Close();
					}
				
					sndSchemeDefinition.Close();
				
					if (null == (sndSchemeLabel = labelRoot.OpenSubKey(currentSndKey, true))) {
						sndSchemeLabel = labelRoot.CreateSubKey(currentSndKey);
					}
					sndSchemeLabel.SetValue(null, SR.WindowsSoundControlPanelNewAttachmentDownloaded);
					sndSchemeLabel.Close();
				
					appSchemeRoot.Close();
				
				} 
				finally {
					RssBanditApplication.SharedUICulture = prev;
					if (schemeRoot != null)
						schemeRoot.Close();
					if (labelRoot != null)
						labelRoot.Close();
				}
			}

			/// <summary>
			/// Set to true to execute Bandit if windows user login to 
			/// the system, else false.
			/// </summary>
			bool IRegistry.RunAtStartup {
				get {
					try {
						RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\\Microsoft\\Windows\\CurrentVersion\\Run", false);
						string s = null;
						if (key != null) {
							s = key.GetValue(BanditKey) as string;
							key.Close();
						}
						Assembly a = Assembly.GetEntryAssembly();
						if (s != null && a != null && a.Location != null)
						{
							string location = a.Location.ToString(CultureInfo.CurrentUICulture);
							if (s == "\"" + location + "\" -t")
								return true;
						}
					} catch (SecurityException sec) {
						Win32._log.Error("Cannot set application to RunAtStartup", sec);
					}
					return false;
				}
				set {
					try {
						// UACManager registered action: 
						RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
						Assembly a = Assembly.GetEntryAssembly();
						string location = a != null && a.Location != null ? a.Location.ToString(CultureInfo.CurrentUICulture) : null;
						if (key != null && location != null)
						{
							if (value) {
								key.SetValue(BanditKey, "\"" + location + "\" -t");
							}
							else {
								string text2 = key.GetValue(BanditKey) as string;
								if (text2 != null) {
									key.DeleteValue(BanditKey);
								}
							}
							key.Close();
						}
					} catch (SecurityException sec) {
						Win32._log.Error("Cannot set application to RunAtStartup", sec);
					}
				}
			}

			/// <summary>
			/// For more infos read:
			/// http://msdn.microsoft.com/library/default.asp?url=/workshop/browser/ext/tutorials/context.asp
			/// </summary>
			bool IRegistry.IsInternetExplorerExtensionRegistered(IEMenuExtension extension) {
				string scriptName = GetIEExtensionScriptName(extension);
				string keyName = FindIEExtensionKey(extension, scriptName);
				
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

			/// <summary>
			/// Registers the Internet explorer extension.
			/// </summary>
			/// <param name="extension">The extension.</param>
			void IRegistry.RegisterInternetExplorerExtension(IEMenuExtension extension) {
				WriteIEExtensionScript(extension);
				WriteIEExtensionRegistryEntry(extension);
			}

			/// <summary>
			/// Unregister Internet explorer extension.
			/// </summary>
			/// <param name="extension">The extension.</param>
			void IRegistry.UnRegisterInternetExplorerExtension(IEMenuExtension extension) {
				DeleteIEExtensionRegistryEntry(extension);
				DeleteIEExtensionScript(extension);
			}
			
			/// <summary>
			/// Gets the Internet explorer version.
			/// </summary>
			/// <returns></returns>
			Version IRegistry.GetInternetExplorerVersion() { 
				return GetInternetExplorerVersion();
			}

			private bool? firstTimeExecution;
				
			/// <summary>
			/// Gets or sets a value indicating whether [current RSS bandit version executes first time after installation].
			/// </summary>
			/// <value>
			/// 	<c>true</c> if [current RSS bandit version executes first time after installation]; otherwise, <c>false</c>.
			/// </value>
			bool IRegistry.ThisVersionExecutesFirstTimeAfterInstallation 
			{
				get
				{
					if (firstTimeExecution == null)
					{
						firstTimeExecution = true;
						RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(BanditSettings, false);
						string val = ((key == null) ? null : (key.GetValue(versionTagName) as string));
						if (!string.IsNullOrEmpty(val))
						{
							firstTimeExecution = false;
							// currently we just relay on if the key exist:
							//try
							//{
							//    Version installedVersion = new Version(val);
							//}
							//catch { }
						}
						if (key != null) key.Close();
					}
					return firstTimeExecution.Value;
				}
				set
				{
					if (firstTimeExecution == value)
						return;

					firstTimeExecution = value;
					if (false == value)
					{
						// create reg. entry:
						try
						{
							RegistryKey keySettings = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(BanditSettings, true);
							if (keySettings == null)
								keySettings = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(BanditSettings);
							
							if (keySettings != null)
							{
								keySettings.SetValue(versionTagName, RssBanditApplication.Version.ToString());
								keySettings.Close();
							}
						}
						catch (Exception ex)
						{
							Win32._log.Error("Cannot set " + versionTagName, ex);
						} 
						
					} else
					{
						// remove reg. entry:
						try
						{
							RegistryKey keySettings = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(BanditSettings, true);
							if (keySettings == null)
								return;

							keySettings.DeleteValue(versionTagName);
							keySettings.Close();
							
						}
						catch (Exception ex)
						{
							Win32._log.Error("Cannot remove " + versionTagName, ex);
						} 
					}
					
				}
			
			}
			#endregion

			#region Internet Explorer Menu Extension handling

			private static string FindIEExtensionKey(IEMenuExtension extension) {
				return FindIEExtensionKey(extension, GetIEExtensionScriptName(extension));
			}
			
			private static string FindIEExtensionKey(IEMenuExtension extension, string scriptName) {
				if (scriptName == null)
					scriptName = GetIEExtensionScriptName(extension);
				try {
					RegistryKey menuBase = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\MenuExt", false);
					foreach (string skey in menuBase.GetSubKeyNames()) {
						RegistryKey subMenu = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(String.Format(@"Software\Microsoft\Internet Explorer\MenuExt\{0}", skey), false);
						// we test for the default entry in the above Reg.Hive:
						string defVal = ((subMenu == null) ? null : (subMenu.GetValue(null) as string));
						if (defVal != null && defVal.EndsWith(scriptName) && File.Exists(defVal)) {
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
				string caption;
				if (extension == IEMenuExtension.DefaultFeedAggregator) {
					caption = SR.InternetExplorerMenuExtDefaultCaption;
				} else {
					caption = SR.InternetExplorerMenuExtBanditCaption;
				}

				try {
					
					RegistryKey menuBase = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\MenuExt", true);
					RegistryKey subMenu = menuBase.OpenSubKey(caption, true);
					if (subMenu == null) {
						subMenu = menuBase.CreateSubKey(caption);
					}
					subMenu.SetValue(null, Path.Combine(RssBanditApplication.GetUserPath(), scriptName));
					subMenu.SetValue("contexts", InternetExplorerExtensionsContexts);
					if (menuBase != null) menuBase.Close();
					if (subMenu != null) subMenu.Close();
				} catch (Exception ex) {
					_log.Error("Registry:WriteIEExtensionRegistryEntry() cause exception", ex);
				}
			}

			private static void WriteIEExtensionScript(IEMenuExtension extension) {
				
				string scriptName = GetIEExtensionScriptName(extension);

				try {
					
					string scriptContent;

					using (Stream resStream = Resource.GetStream("Resources."+scriptName)) {
						StreamReader reader = new StreamReader(resStream);
						scriptContent = reader.ReadToEnd();
					}
					
					if (scriptContent != null) {
						if (extension == IEMenuExtension.Bandit) {	// set the path to the exe within script
							//TR: fixed FormatException, if we do this (scriptContent contained a ...{0}... placehoder):
							//scriptContent = String.Format(scriptContent, Application.ExecutablePath);
							// now using a placeholder string:
							scriptContent = scriptContent.Replace("__COMMAND_PATH_PLACEHOLDER__", Application.ExecutablePath.Replace(@"\", @"\\"));
						}

						using (Stream outStream = FileHelper.OpenForWrite(Path.Combine(RssBanditApplication.GetUserPath(), scriptName))) {
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
					FileHelper.Delete(scriptPath);
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

			#region private methods

			private static RegistryKey ClassesRootKey() {
				return ClassesRootKey(false);
			}
			
			/// <summary>
			/// Gets also called from the UACManager class
			/// </summary>
			/// <param name="writable"></param>
			/// <returns></returns>
			internal static RegistryKey ClassesRootKey(bool writable) {
				if (IsOSAtLeastWindows2000)
					return Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Classes", writable);
				else
					return Microsoft.Win32.Registry.ClassesRoot;
			}

			#endregion

		}

		/// <summary>
		/// Wrap the registry access needed for a portable Bandit running from stick
		/// </summary>
		private class PortableRegistry : IRegistry
		{
			private int instanceActivatorPort;
			
			/// <summary>
			/// Set/Get the Port number to be used by the Single Instance Activator.
			/// </summary>
			/// <remarks>
			/// We assume here, the app running from a stick have reconfigured
			/// their data and cache path(s) to be on a folder at the stick. 
			/// So it is enough to protect that path(s) usage to one single app. 
			/// instance using a file containing the port number.
			/// </remarks>
			int IRegistry.InstanceActivatorPort {
				get {
					if (instanceActivatorPort != 0)
						return instanceActivatorPort;
					
					string portFile = Path.Combine(RssBanditApplication.GetUserPath(), ".port");
					int retval = 0;
					try {
						if (!File.Exists(portFile)) 
							return retval;	
						
						using (Stream s = FileHelper.OpenForRead(portFile)) {
							TextReader reader = new StreamReader(s);
							string content = reader.ReadToEnd();
							if (StringHelper.EmptyTrimOrNull(content))
								return retval;
							retval = Int16.Parse(content);
						}
						
						instanceActivatorPort = retval;
						return instanceActivatorPort;
						
					} catch (Exception ex) {
						_log.Error("Cannot get InstanceActivatorPort from .port file", ex);
						return 0;
					} 
				}
				set {
					if (instanceActivatorPort != value) {
						string portFilePath = RssBanditApplication.GetUserPath();
						try {
							if (!Directory.Exists(portFilePath))
								Directory.CreateDirectory(portFilePath);
					
							using (Stream s  = FileHelper.OpenForWrite(Path.Combine(portFilePath, ".port"))) {
								TextWriter w = new StreamWriter(s);
								w.Write(value);
								w.Flush();
							}
						
							instanceActivatorPort = value;
						
						} catch (Exception ex) {
							Win32._log.Error("Cannot set InstanceActivatorPort in .port file", ex);
						}
					}
				}
			}

			/// <summary>
			/// Set/Get the current "feed:" Url protocol handler. 
			/// Provide the complete executable file path name.
			/// </summary>
			/// <exception cref="Exception">On set, if there are no rights to write the value</exception>
			string IRegistry.CurrentFeedProtocolHandler {
				get {
					// we fake here:
					return String.Concat(Application.ExecutablePath, " ", "\"",  "%1", "\"");
				}
				set {
					// not applicable as a portable app.
				}
			}

			/// <summary>
			/// Checks and init RSS bandit sounds 
			/// (configurable via Windows Sounds Control Panel).
			/// </summary>
			/// <param name="appKey">The app key (file name without extension and path).
			/// E.g. "RssBandit".</param>
			/// <remarks>
			/// See also: http://blogs.msdn.com/larryosterman/archive/2006/01/24/517183.aspx
			/// </remarks>
			void IRegistry.CheckAndInitSounds(string appKey) {
				// nothing to do here
				return;
			}

			/// <summary>
			/// Set to true to execute Bandit if windows user login to 
			/// the system, else false.
			/// </summary>
			bool IRegistry.RunAtStartup {
				get { return false; }
				set {  }
			}

			/// <summary>
			/// For more infos read:
			/// http://msdn.microsoft.com/library/default.asp?url=/workshop/browser/ext/tutorials/context.asp
			/// </summary>
			bool IRegistry.IsInternetExplorerExtensionRegistered(IEMenuExtension extension) {
				return true;
			}

			/// <summary>
			/// Registers the internet explorer extension.
			/// </summary>
			/// <param name="extension">The extension.</param>
			void IRegistry.RegisterInternetExplorerExtension(IEMenuExtension extension) {
				// not applicable
				return;
			}

			/// <summary>
			/// Uns the register internet explorer extension.
			/// </summary>
			/// <param name="extension">The extension.</param>
			void IRegistry.UnRegisterInternetExplorerExtension(IEMenuExtension extension) {
				// not applicable
				return;
			}

			/// <summary>
			/// Gets the internet explorer version.
			/// </summary>
			/// <returns></returns>
			Version IRegistry.GetInternetExplorerVersion() {
				return WindowsRegistry.GetInternetExplorerVersion();
			}

			private bool? firstTimeExecution;
			
			/// <summary>
			/// Gets or sets a value indicating whether [current RSS bandit version executes first time after installation].
			/// </summary>
			/// <value>
			/// 	<c>true</c> if [current RSS bandit version executes first time after installation]; otherwise, <c>false</c>.
			/// </value>
			bool IRegistry.ThisVersionExecutesFirstTimeAfterInstallation 
			{
				get
				{
					if (firstTimeExecution == null)
					{
						firstTimeExecution = true;
						string verFile = Path.Combine(RssBanditApplication.GetUserPath(), versionTagFile);
						try
						{
							firstTimeExecution = !File.Exists(verFile);
						}
						catch (Exception ex)
						{
							_log.Error(String.Format("Cannot get version info from file {0}", verFile), ex);
						}
					}
					return firstTimeExecution.Value;
				}
				set
				{
					if (firstTimeExecution == value)
						return;

					firstTimeExecution = value;
					
					if (false == value)
					{
						// create tag file:
						string verFilePath = RssBanditApplication.GetUserPath();
						string verFile = Path.Combine(verFilePath, versionTagFile);
						try
						{
							if (!Directory.Exists(verFilePath))
								Directory.CreateDirectory(verFilePath);

							using (Stream s = FileHelper.OpenForWrite(verFile))
							{
								TextWriter w = new StreamWriter(s);
								w.Write(RssBanditApplication.Version.ToString());
								w.Flush();
							}
						}
						catch (Exception ex)
						{
							_log.Error(String.Format("Cannot set version info in file {0}", verFile), ex);
						}
					} 
					else
					{
						// remove tag file
						string verFile = Path.Combine(RssBanditApplication.GetUserPath(), versionTagFile);
						try
						{
							if (File.Exists(verFile))
								File.Delete(verFile);
						}
						catch (Exception ex)
						{
							_log.Error(String.Format("Cannot remove version info file {0}", verFile), ex);
						}
					}
					
				}
			}
			
		}

		#endregion
		
		private Win32() {}

		[EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
		static Win32() 
		{
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
			
			IEVersion = Registry.GetInternetExplorerVersion();

		}

		private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(Win32));
		private static int _paintFrozen;
		private static readonly OperatingSystem _os;

		// General info fields
		internal static readonly bool IsAspNetServer;
		internal static readonly bool IsPostWin2K;
		internal static readonly bool IsWin2K;
		internal static readonly bool IsWin2k3;
		internal static readonly bool IsWin9x;
		internal static readonly bool IsWinHttp51;
		internal static readonly bool IsWinNt;
		
		internal static readonly Version IEVersion;

		#region General

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
			if (!GetVersionEx(ref ifex)) {
				int err = Marshal.GetLastWin32Error();
				throw new Exception("Requesting Windows Service Pack Information caused an windows error (Code: " + err.ToString() +").");
			}
			servicePackMajor = ifex.wServicePackMajor;
			servicePackMinor = ifex.wServicePackMinor;
		}

		/// <summary>
		/// API Wrapper to retrive the startup process window state.
		/// </summary>
		/// <returns>FormWindowState</returns>
		public static FormWindowState GetStartupWindowState() {
			API_STARTUPINFO sti = new API_STARTUPINFO();
			try {
				sti.cb = Marshal.SizeOf(typeof(API_STARTUPINFO));
				GetStartupInfo(ref sti);
				//System.Diagnostics.Process.GetCurrentProcess().StartInfo
				//System.Windows.Forms.MessageBox.Show("sti.wShowWindow is: "+sti.wShowWindow.ToString());

				if (sti.wShowWindow == (short)ShowWindowStyles.SW_MINIMIZE || sti.wShowWindow == (short)ShowWindowStyles.SW_SHOWMINIMIZED ||
				    sti.wShowWindow == (short)ShowWindowStyles.SW_SHOWMINNOACTIVE || sti.wShowWindow == (short)ShowWindowStyles.SW_FORCEMINIMIZE)
					return FormWindowState.Minimized;
				if (sti.wShowWindow == (short)ShowWindowStyles.SW_MAXIMIZE || sti.wShowWindow == (short)ShowWindowStyles.SW_SHOWMAXIMIZED ||
				    sti.wShowWindow == (short)ShowWindowStyles.SW_MAX)
					return FormWindowState.Maximized;
			} catch (Exception e) {
				_log.Error("GetStartupWindowState() caused exception", e);
			}
			return FormWindowState.Normal;
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
					SendMessage(ctrl.Handle, (int) Message.WM_SETREDRAW, 0, IntPtr.Zero); 
				} 
			} 
			if (!freeze) { 
				if (_paintFrozen == 0) { 
					return; 
				} 
  
				if (0 == --_paintFrozen && ctrl != null) { 
					SendMessage(ctrl.Handle, (int)Message.WM_SETREDRAW, 1, IntPtr.Zero); 
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
	internal sealed class UxTheme {
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
			
				OperatingSystem os = Environment.OSVersion;

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
		internal static extern int GetCurrentThemeName(StringBuilder pszThemeFileName, int dwMaxNameChars, StringBuilder pszColorBuff, int cchMaxColorChars, StringBuilder pszSizeBuff, int cchMaxSizeChars);


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
