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
using IEControl;
using Interop.ThumbCache;
using Microsoft.Win32;
using NewsComponents.Utils;
using RssBandit.Common.Logging;
using RssBandit.Resources;
using Application=System.Windows.Forms.Application;
using Logger = RssBandit.Common.Logging;

namespace RssBandit
{
	/// <summary>
	/// Common used Win32 Interop decl.
	/// </summary>
	[System.Security.SuppressUnmanagedCodeSecurity]
	internal sealed class Win32 
    {

		#region enum/consts

        /// <summary>
        /// wProductType consts. 
        /// Any additional information about the system. 
        /// This can be one of the following values.
        /// See http://msdn.microsoft.com/en-us/library/ms724833(VS.85).aspx
        /// </summary>
	    public const int VER_NT_WORKSTATION = 1;
        public const int VER_NT_DOMAIN_CONTROLLER = 2;
        public const int VER_NT_SERVER = 3;

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
        public enum WindowsSuite
        {
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
            /// <summary>
            /// Windows Storage Server 2003 R2 or Windows Storage Server 2003is installed
            /// </summary>
            VER_SUITE_STORAGE_SERVER = 0x00002000,
            /// <summary>
            /// Windows Server 2003, Compute Cluster Edition is installed.
            /// </summary>
            VER_SUITE_COMPUTE_SERVER = 0x00004000,
            /// <summary>
            /// Windows Home Server is installed.
            /// </summary>
            VER_SUITE_WH_SERVER = 0x00008000,
        }

        /// <summary>
        /// See also: http://msdn.microsoft.com/en-us/library/ms724358(VS.85).aspx
        /// </summary>
        internal static class WindowsProductType
        {
            #region consts

            private const uint PRODUCT_UNLICENSED = 0xABCDABCD;
            private const uint PRODUCT_BUSINESS =0x00000006;
            private const uint PRODUCT_BUSINESS_N = 0x00000010;
            private const uint PRODUCT_CLUSTER_SERVER = 0x00000012;
            private const uint PRODUCT_DATACENTER_SERVER = 0x00000008;
            private const uint PRODUCT_DATACENTER_SERVER_CORE = 0x0000000C;
            private const uint PRODUCT_DATACENTER_SERVER_CORE_V = 0x00000027;
            private const uint PRODUCT_DATACENTER_SERVER_V = 0x00000025;
            private const uint PRODUCT_ENTERPRISE = 0x00000004;
            private const uint PRODUCT_ENTERPRISE_E = 0x00000046;
            private const uint PRODUCT_ENTERPRISE_N = 0x0000001B;
            private const uint PRODUCT_ENTERPRISE_SERVER = 0x0000000A;
            private const uint PRODUCT_ENTERPRISE_SERVER_CORE = 0x0000000E;
            private const uint PRODUCT_ENTERPRISE_SERVER_CORE_V = 0x00000029;
            private const uint PRODUCT_ENTERPRISE_SERVER_IA64 = 0x0000000F;
            private const uint PRODUCT_ENTERPRISE_SERVER_V = 0x00000026;
            private const uint PRODUCT_HOME_BASIC = 0x00000002;
            private const uint PRODUCT_HOME_BASIC_E = 0x00000043;
            private const uint PRODUCT_HOME_BASIC_N = 0x00000005;
            private const uint PRODUCT_HOME_PREMIUM = 0x00000003;
            private const uint PRODUCT_HOME_PREMIUM_E = 0x00000044;
            private const uint PRODUCT_HOME_PREMIUM_N = 0x0000001A;
            private const uint PRODUCT_HYPERV = 0x0000002A;
            private const uint PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT = 0x0000001E;
            private const uint PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING = 0x00000020;
            private const uint PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY = 0x0000001F;
            private const uint PRODUCT_PROFESSIONAL = 0x00000030;
            private const uint PRODUCT_PROFESSIONAL_E = 0x00000045;
            private const uint PRODUCT_PROFESSIONAL_N = 0x00000031;
            private const uint PRODUCT_SERVER_FOR_SMALLBUSINESS = 0x00000018;
            private const uint PRODUCT_SERVER_FOR_SMALLBUSINESS_V = 0x00000023;
            private const uint PRODUCT_SERVER_FOUNDATION = 0x00000021;
            private const uint PRODUCT_SMALLBUSINESS_SERVER = 0x00000009;
            private const uint PRODUCT_STANDARD_SERVER = 0x00000007;
            private const uint PRODUCT_STANDARD_SERVER_CORE = 0x0000000D;
            private const uint PRODUCT_STANDARD_SERVER_CORE_V = 0x00000028;
            private const uint PRODUCT_STANDARD_SERVER_V = 0x00000024;
            private const uint PRODUCT_STARTER = 0x0000000B;
            private const uint PRODUCT_STARTER_E = 0x00000042;
            private const uint PRODUCT_STARTER_N = 0x0000002F;
            private const uint PRODUCT_STORAGE_ENTERPRISE_SERVER = 0x00000017;
            private const uint PRODUCT_STORAGE_EXPRESS_SERVER = 0x00000014;
            private const uint PRODUCT_STORAGE_STANDARD_SERVER = 0x00000015;
            private const uint PRODUCT_STORAGE_WORKGROUP_SERVER = 0x00000016;
            private const uint PRODUCT_UNDEFINED = 0x00000000;
            private const uint PRODUCT_ULTIMATE = 0x00000001;
            private const uint PRODUCT_ULTIMATE_E = 0x00000047;
            private const uint PRODUCT_ULTIMATE_N = 0x0000001C;
            private const uint PRODUCT_WEB_SERVER = 0x00000011;
            private const uint PRODUCT_WEB_SERVER_CORE = 0x0000001D;

            #endregion

            internal static string Get(uint type)
            {
                switch (type)
                {
                    case PRODUCT_UNDEFINED: return "Unknown product";
                    case PRODUCT_UNLICENSED: return "Not licensed";
                    case PRODUCT_BUSINESS: return "Business Edition";
                    case PRODUCT_BUSINESS_N: return "Business Edition N";
                    case PRODUCT_CLUSTER_SERVER: return "HPC Edition";
                    case PRODUCT_DATACENTER_SERVER: return "Server Datacenter Edition (full installation)";
                    case PRODUCT_DATACENTER_SERVER_CORE: return "Server Datacenter Edition (core installation)";
                    case PRODUCT_DATACENTER_SERVER_CORE_V: return "Server Datacenter Edition without Hyper-V (core installation)";
                    case PRODUCT_DATACENTER_SERVER_V: return "Server Datacenter Edition without Hyper-V (full installation)";
                    case PRODUCT_ENTERPRISE: return "Enterprise Edition";
                    case PRODUCT_ENTERPRISE_E: return "Enterprise Edition E";
                    case PRODUCT_ENTERPRISE_N: return "Enterprise Edition N";
                    case PRODUCT_ENTERPRISE_SERVER: return "Server Enterprise Edition (full installation)";
                    case PRODUCT_ENTERPRISE_SERVER_CORE: return "Server Enterprise Edition (core installation)";
                    case PRODUCT_ENTERPRISE_SERVER_CORE_V: return "Server Enterprise Edition without Hyper-V (core installation)";
                    case PRODUCT_ENTERPRISE_SERVER_IA64: return "Server Enterprise Edition for Itanium-based Systems";
                    case PRODUCT_ENTERPRISE_SERVER_V: return "Server Enterprise Edition without Hyper-V (full installation)";
                    case PRODUCT_HOME_BASIC: return "Home Basic Edition";
                    case PRODUCT_HOME_BASIC_E: return "Home Basic Edition E";
                    case PRODUCT_HOME_BASIC_N: return "Home Basic Edition N";
                    case PRODUCT_HOME_PREMIUM: return "Home Premium Edition";
                    case PRODUCT_HOME_PREMIUM_E: return "Home Premium Edition E";
                    case PRODUCT_HOME_PREMIUM_N: return "Home Premium Edition N";
                    case PRODUCT_HYPERV: return "Microsoft Hyper-V Server Edition";
                    case PRODUCT_MEDIUMBUSINESS_SERVER_MANAGEMENT: return "Windows Essential Business Server Management Server";
                    case PRODUCT_MEDIUMBUSINESS_SERVER_MESSAGING: return "Windows Essential Business Server Messaging Server";
                    case PRODUCT_MEDIUMBUSINESS_SERVER_SECURITY: return "Windows Essential Business Server Security Server";
                    case PRODUCT_PROFESSIONAL: return "Professional Edition";
                    case PRODUCT_PROFESSIONAL_E: return "Professional Edition E";
                    case PRODUCT_PROFESSIONAL_N: return "Professional Edition N";
                    case PRODUCT_SERVER_FOR_SMALLBUSINESS: return "Windows Server 2008 for Windows Essential Server Solutions";
                    case PRODUCT_SERVER_FOR_SMALLBUSINESS_V: return "Windows Server 2008 without Hyper-V for Windows Essential Server Solutions";
                    case PRODUCT_SERVER_FOUNDATION: return "Server Foundation Edition";
                    case PRODUCT_SMALLBUSINESS_SERVER: return "Windows Small Business Server";
                    case PRODUCT_STANDARD_SERVER: return "Server Standard Edition (full installation)";
                    case PRODUCT_STANDARD_SERVER_CORE: return "Server Standard Edition (core installation)";
                    case PRODUCT_STANDARD_SERVER_CORE_V: return "Server Standard Edition without Hyper-V (core installation)";
                    case PRODUCT_STANDARD_SERVER_V: return "Server Standard Edition without Hyper-V (full installation)";
                    case PRODUCT_STARTER: return "Starter Edition";
                    case PRODUCT_STARTER_E: return "Starter Edition E";
                    case PRODUCT_STARTER_N: return "Starter Edition N";
                    case PRODUCT_STORAGE_ENTERPRISE_SERVER: return "Storage Server Enterprise Edition";
                    case PRODUCT_STORAGE_EXPRESS_SERVER: return "Storage Server Express Edition";
                    case PRODUCT_STORAGE_STANDARD_SERVER: return "Storage Server Standard Edition";
                    case PRODUCT_STORAGE_WORKGROUP_SERVER: return "Storage Server Workgroup Edition";
                    case PRODUCT_ULTIMATE: return "Ultimate Edition";
                    case PRODUCT_ULTIMATE_E: return "Ultimate Edition E";
                    case PRODUCT_ULTIMATE_N: return "Ultimate Edition N";
                    case PRODUCT_WEB_SERVER: return "Web Server Edition (full installation)";
                    case PRODUCT_WEB_SERVER_CORE: return "Web Server Edition (core installation)";
                    default: return String.Empty;
                }
            }
        }

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

        /// <summary>
        /// From winerror.h.
        /// </summary>
        public const int ERROR_SUCCESS = 0;

        /// <summary>
        /// From winerror.h.
        /// </summary>
        public const int ERROR_FILE_NOT_FOUND = 2;

        /// <summary>
        /// From winerror.h.
        /// </summary>
        public const int ERROR_ACCESS_DENIED = 5;

        /// <summary>
        /// From winerror.h.
        /// </summary>
        public const int ERROR_INSUFFICIENT_BUFFER = 122;

        /// <summary>
        /// From winerror.h.
        /// </summary>
        public const int ERROR_NO_MORE_ITEMS = 259;

      

		#endregion

		#region structs/classes

        [StructLayout(LayoutKind.Sequential)]
        internal struct FILETIME
        {
            public UInt32 dwLowDateTime;
            public UInt32 dwHighDateTime;
        }

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

        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_INFO
        {
            public ushort processorArchitecture;
            ushort reserved;
            public uint pageSize;
            public IntPtr minimumApplicationAddress;
            public IntPtr maximumApplicationAddress;
            public IntPtr activeProcessorMask;
            public uint numberOfProcessors;
            public uint processorType;
            public uint allocationGranularity;
            public ushort processorLevel;
            public ushort processorRevision;
        }

        internal const ushort PROCESSOR_ARCHITECTURE_AMD64 = 9;
        internal const ushort PROCESSOR_ARCHITECTURE_IA64 = 6;
        internal const ushort PROCESSOR_ARCHITECTURE_INTEL = 0;
        internal const ushort PROCESSOR_ARCHITECTURE_UNKNOWN = 0xffff;

        internal delegate void GetNativeSystemInfoInvoker(ref SYSTEM_INFO lpSystemInfo);
        // see http://msdn.microsoft.com/en-us/library/ms724358(VS.85).aspx
        internal delegate bool GetProductInfoInvoker(int dwOSMajorVersion,
            int dwOSMinorVersion,int dwSpMajorVersion,int dwSpMinorVersion,
            out uint pdwReturnedProductType);
		
        /// <summary>
		/// Flags used with the Windows API (User32.dll):GetSystemMetrics(SystemMetric smIndex)
		///   
		/// This Enum and declaration signature was written by Gabriel T. Sharp
		/// ai_productions@verizon.net or osirisgothra@hotmail.com
		/// Obtained on pinvoke.net, please contribute your code to support the wiki!
		/// </summary>
		internal enum SystemMetric
		{
			/// <summary>
			///  Width of the screen of the primary display monitor, in pixels. This is the same values obtained by calling GetDeviceCaps as follows: GetDeviceCaps( hdcPrimaryMonitor, HORZRES).
			/// </summary>
			SM_CXSCREEN = 0,
			/// <summary>
			/// Height of the screen of the primary display monitor, in pixels. This is the same values obtained by calling GetDeviceCaps as follows: GetDeviceCaps( hdcPrimaryMonitor, VERTRES).
			/// </summary>
			SM_CYSCREEN = 1,
			/// <summary>
			/// Width of a horizontal scroll bar, in pixels.
			/// </summary>
			SM_CYVSCROLL = 2,
			/// <summary>
			/// Height of a horizontal scroll bar, in pixels.
			/// </summary>
			SM_CXVSCROLL = 3,
			/// <summary>
			/// Height of a caption area, in pixels.
			/// </summary>
			SM_CYCAPTION = 4,
			/// <summary>
			/// Width of a window border, in pixels. This is equivalent to the SM_CXEDGE value for windows with the 3-D look. 
			/// </summary>
			SM_CXBORDER = 5,
			/// <summary>
			/// Height of a window border, in pixels. This is equivalent to the SM_CYEDGE value for windows with the 3-D look. 
			/// </summary>
			SM_CYBORDER = 6,
			/// <summary>
			/// Thickness of the frame around the perimeter of a window that has a caption but is not sizable, in pixels. SM_CXFIXEDFRAME is the height of the horizontal border and SM_CYFIXEDFRAME is the width of the vertical border. 
			/// </summary>
			SM_CXDLGFRAME = 7,
			/// <summary>
			/// Thickness of the frame around the perimeter of a window that has a caption but is not sizable, in pixels. SM_CXFIXEDFRAME is the height of the horizontal border and SM_CYFIXEDFRAME is the width of the vertical border. 
			/// </summary>
			SM_CYDLGFRAME = 8,
			/// <summary>
			/// Height of the thumb box in a vertical scroll bar, in pixels
			/// </summary>
			SM_CYVTHUMB = 9,
			/// <summary>
			/// Width of the thumb box in a horizontal scroll bar, in pixels.
			/// </summary>
			SM_CXHTHUMB = 10,
			/// <summary>
			/// Default width of an icon, in pixels. The LoadIcon function can load only icons with the dimensions specified by SM_CXICON and SM_CYICON
			/// </summary>
			SM_CXICON = 11,
			/// <summary>
			/// Default height of an icon, in pixels. The LoadIcon function can load only icons with the dimensions SM_CXICON and SM_CYICON.
			/// </summary>
			SM_CYICON = 12,
			/// <summary>
			/// Width of a cursor, in pixels. The system cannot create cursors of other sizes.
			/// </summary>
			SM_CXCURSOR = 13,
			/// <summary>
			/// Height of a cursor, in pixels. The system cannot create cursors of other sizes.
			/// </summary>
			SM_CYCURSOR = 14,
			/// <summary>
			/// Height of a single-line menu bar, in pixels.
			/// </summary>
			SM_CYMENU = 15,
			/// <summary>
			/// Width of the client area for a full-screen window on the primary display monitor, in pixels. To get the coordinates of the portion of the screen not obscured by the system taskbar or by application desktop toolbars, call the SystemParametersInfo function with the SPI_GETWORKAREA value.
			/// </summary>
			SM_CXFULLSCREEN = 16,
			/// <summary>
			/// Height of the client area for a full-screen window on the primary display monitor, in pixels. To get the coordinates of the portion of the screen not obscured by the system taskbar or by application desktop toolbars, call the SystemParametersInfo function with the SPI_GETWORKAREA value.
			/// </summary>
			SM_CYFULLSCREEN = 17,
			/// <summary>
			/// For double byte character set versions of the system, this is the height of the Kanji window at the bottom of the screen, in pixels
			/// </summary>
			SM_CYKANJIWINDOW = 18,
			/// <summary>
			/// Nonzero if a mouse with a wheel is installed; zero otherwise
			/// </summary>
			SM_MOUSEWHEELPRESENT = 75,
			/// <summary>
			/// Height of the arrow bitmap on a vertical scroll bar, in pixels.
			/// </summary>
			SM_CYHSCROLL = 20,
			/// <summary>
			/// Width of the arrow bitmap on a horizontal scroll bar, in pixels.
			/// </summary>
			SM_CXHSCROLL = 21,
			/// <summary>
			/// Nonzero if the debug version of User.exe is installed; zero otherwise.
			/// </summary>
			SM_DEBUG = 22,
			/// <summary>
			/// Nonzero if the left and right mouse buttons are reversed; zero otherwise.
			/// </summary>
			SM_SWAPBUTTON = 23,
			/// <summary>
			/// Reserved for future use
			/// </summary>
			SM_RESERVED1 = 24,
			/// <summary>
			/// Reserved for future use
			/// </summary>
			SM_RESERVED2 = 25,
			/// <summary>
			/// Reserved for future use
			/// </summary>
			SM_RESERVED3 = 26,
			/// <summary>
			/// Reserved for future use
			/// </summary>
			SM_RESERVED4 = 27,
			/// <summary>
			/// Minimum width of a window, in pixels.
			/// </summary>
			SM_CXMIN = 28,
			/// <summary>
			/// Minimum height of a window, in pixels.
			/// </summary>
			SM_CYMIN = 29,
			/// <summary>
			/// Width of a button in a window's caption or title bar, in pixels.
			/// </summary>
			SM_CXSIZE = 30,
			/// <summary>
			/// Height of a button in a window's caption or title bar, in pixels.
			/// </summary>
			SM_CYSIZE = 31,
			/// <summary>
			/// Thickness of the sizing border around the perimeter of a window that can be resized, in pixels. SM_CXSIZEFRAME is the width of the horizontal border, and SM_CYSIZEFRAME is the height of the vertical border. 
			/// </summary>
			SM_CXFRAME = 32,
			/// <summary>
			/// Thickness of the sizing border around the perimeter of a window that can be resized, in pixels. SM_CXSIZEFRAME is the width of the horizontal border, and SM_CYSIZEFRAME is the height of the vertical border. 
			/// </summary>
			SM_CYFRAME = 33,
			/// <summary>
			/// Minimum tracking width of a window, in pixels. The user cannot drag the window frame to a size smaller than these dimensions. A window can override this value by processing the WM_GETMINMAXINFO message.
			/// </summary>
			SM_CXMINTRACK = 34,
			/// <summary>
			/// Minimum tracking height of a window, in pixels. The user cannot drag the window frame to a size smaller than these dimensions. A window can override this value by processing the WM_GETMINMAXINFO message
			/// </summary>
			SM_CYMINTRACK = 35,
			/// <summary>
			/// Width of the rectangle around the location of a first click in a double-click sequence, in pixels. The second click must occur within the rectangle defined by SM_CXDOUBLECLK and SM_CYDOUBLECLK for the system to consider the two clicks a double-click
			/// </summary>
			SM_CXDOUBLECLK = 36,
			/// <summary>
			/// Height of the rectangle around the location of a first click in a double-click sequence, in pixels. The second click must occur within the rectangle defined by SM_CXDOUBLECLK and SM_CYDOUBLECLK for the system to consider the two clicks a double-click. (The two clicks must also occur within a specified time.) 
			/// </summary>
			SM_CYDOUBLECLK = 37,
			/// <summary>
			/// Width of a grid cell for items in large icon view, in pixels. Each item fits into a rectangle of size SM_CXICONSPACING by SM_CYICONSPACING when arranged. This value is always greater than or equal to SM_CXICON
			/// </summary>
			SM_CXICONSPACING = 38,
			/// <summary>
			/// Height of a grid cell for items in large icon view, in pixels. Each item fits into a rectangle of size SM_CXICONSPACING by SM_CYICONSPACING when arranged. This value is always greater than or equal to SM_CYICON.
			/// </summary>
			SM_CYICONSPACING = 39,
			/// <summary>
			/// Nonzero if drop-down menus are right-aligned with the corresponding menu-bar item; zero if the menus are left-aligned.
			/// </summary>
			SM_MENUDROPALIGNMENT = 40,
			/// <summary>
			/// Nonzero if the Microsoft Windows for Pen computing extensions are installed; zero otherwise.
			/// </summary>
			SM_PENWINDOWS = 41,
			/// <summary>
			/// Nonzero if User32.dll supports DBCS; zero otherwise. (WinMe/95/98): Unicode
			/// </summary>
			SM_DBCSENABLED = 42,
			/// <summary>
			/// Number of buttons on mouse, or zero if no mouse is installed.
			/// </summary>
			SM_CMOUSEBUTTONS = 43,
			/// <summary>
			/// Identical Values Changed After Windows NT 4.0  
			/// </summary>
			SM_CXFIXEDFRAME = SM_CXDLGFRAME,
			/// <summary>
			/// Identical Values Changed After Windows NT 4.0
			/// </summary>
			SM_CYFIXEDFRAME = SM_CYDLGFRAME,
			/// <summary>
			/// Identical Values Changed After Windows NT 4.0
			/// </summary>
			SM_CXSIZEFRAME = SM_CXFRAME,
			/// <summary>
			/// Identical Values Changed After Windows NT 4.0
			/// </summary>
			SM_CYSIZEFRAME = SM_CYFRAME,
			/// <summary>
			/// Nonzero if security is present; zero otherwise.
			/// </summary>
			SM_SECURE = 44,
			/// <summary>
			/// Width of a 3-D border, in pixels. This is the 3-D counterpart of SM_CXBORDER
			/// </summary>
			SM_CXEDGE = 45,
			/// <summary>
			/// Height of a 3-D border, in pixels. This is the 3-D counterpart of SM_CYBORDER
			/// </summary>
			SM_CYEDGE = 46,
			/// <summary>
			/// Width of a grid cell for a minimized window, in pixels. Each minimized window fits into a rectangle this size when arranged. This value is always greater than or equal to SM_CXMINIMIZED.
			/// </summary>
			SM_CXMINSPACING = 47,
			/// <summary>
			/// Height of a grid cell for a minimized window, in pixels. Each minimized window fits into a rectangle this size when arranged. This value is always greater than or equal to SM_CYMINIMIZED.
			/// </summary>
			SM_CYMINSPACING = 48,
			/// <summary>
			/// Recommended width of a small icon, in pixels. Small icons typically appear in window captions and in small icon view
			/// </summary>
			SM_CXSMICON = 49,
			/// <summary>
			/// Recommended height of a small icon, in pixels. Small icons typically appear in window captions and in small icon view.
			/// </summary>
			SM_CYSMICON = 50,
			/// <summary>
			/// Height of a small caption, in pixels
			/// </summary>
			SM_CYSMCAPTION = 51,
			/// <summary>
			/// Width of small caption buttons, in pixels.
			/// </summary>
			SM_CXSMSIZE = 52,
			/// <summary>
			/// Height of small caption buttons, in pixels.
			/// </summary>
			SM_CYSMSIZE = 53,
			/// <summary>
			/// Width of menu bar buttons, such as the child window close button used in the multiple document interface, in pixels.
			/// </summary>
			SM_CXMENUSIZE = 54,
			/// <summary>
			/// Height of menu bar buttons, such as the child window close button used in the multiple document interface, in pixels.
			/// </summary>
			SM_CYMENUSIZE = 55,
			/// <summary>
			/// Flags specifying how the system arranged minimized windows
			/// </summary>
			SM_ARRANGE = 56,
			/// <summary>
			/// Width of a minimized window, in pixels.
			/// </summary>
			SM_CXMINIMIZED = 57,
			/// <summary>
			/// Height of a minimized window, in pixels.
			/// </summary>
			SM_CYMINIMIZED = 58,
			/// <summary>
			/// Default maximum width of a window that has a caption and sizing borders, in pixels. This metric refers to the entire desktop. The user cannot drag the window frame to a size larger than these dimensions. A window can override this value by processing the WM_GETMINMAXINFO message.
			/// </summary>
			SM_CXMAXTRACK = 59,
			/// <summary>
			/// Default maximum height of a window that has a caption and sizing borders, in pixels. This metric refers to the entire desktop. The user cannot drag the window frame to a size larger than these dimensions. A window can override this value by processing the WM_GETMINMAXINFO message.
			/// </summary>
			SM_CYMAXTRACK = 60,
			/// <summary>
			/// Default width, in pixels, of a maximized top-level window on the primary display monitor.
			/// </summary>
			SM_CXMAXIMIZED = 61,
			/// <summary>
			/// Default height, in pixels, of a maximized top-level window on the primary display monitor.
			/// </summary>
			SM_CYMAXIMIZED = 62,
			/// <summary>
			/// Least significant bit is set if a network is present; otherwise, it is cleared. The other bits are reserved for future use
			/// </summary>
			SM_NETWORK = 63,
			/// <summary>
			/// Value that specifies how the system was started: 0-normal, 1-failsafe, 2-failsafe /w net
			/// </summary>
			SM_CLEANBOOT = 67,
			/// <summary>
			/// Width of a rectangle centered on a drag point to allow for limited movement of the mouse pointer before a drag operation begins, in pixels. 
			/// </summary>
			SM_CXDRAG = 68,
			/// <summary>
			/// Height of a rectangle centered on a drag point to allow for limited movement of the mouse pointer before a drag operation begins. This value is in pixels. It allows the user to click and release the mouse button easily without unintentionally starting a drag operation.
			/// </summary>
			SM_CYDRAG = 69,
			/// <summary>
			/// Nonzero if the user requires an application to present information visually in situations where it would otherwise present the information only in audible form; zero otherwise. 
			/// </summary>
			SM_SHOWSOUNDS = 70,
			/// <summary>
			/// Width of the default menu check-mark bitmap, in pixels.
			/// </summary>
			SM_CXMENUCHECK = 71,
			/// <summary>
			/// Height of the default menu check-mark bitmap, in pixels.
			/// </summary>
			SM_CYMENUCHECK = 72,
			/// <summary>
			/// Nonzero if the computer has a low-end (slow) processor; zero otherwise
			/// </summary>
			SM_SLOWMACHINE = 73,
			/// <summary>
			/// Nonzero if the system is enabled for Hebrew and Arabic languages, zero if not.
			/// </summary>
			SM_MIDEASTENABLED = 74,
			/// <summary>
			/// Nonzero if a mouse is installed; zero otherwise. This value is rarely zero, because of support for virtual mice and because some systems detect the presence of the port instead of the presence of a mouse.
			/// </summary>
			SM_MOUSEPRESENT = 19,
			/// <summary>
			/// Windows 2000 (v5.0+) Coordinate of the top of the virtual screen
			/// </summary>
			SM_XVIRTUALSCREEN = 76,
			/// <summary>
			/// Windows 2000 (v5.0+) Coordinate of the left of the virtual screen
			/// </summary>
			SM_YVIRTUALSCREEN = 77,
			/// <summary>
			/// Windows 2000 (v5.0+) Width of the virtual screen
			/// </summary>
			SM_CXVIRTUALSCREEN = 78,
			/// <summary>
			/// Windows 2000 (v5.0+) Height of the virtual screen
			/// </summary>
			SM_CYVIRTUALSCREEN = 79,
			/// <summary>
			/// Number of display monitors on the desktop
			/// </summary>
			SM_CMONITORS = 80,
			/// <summary>
			/// Windows XP (v5.1+) Nonzero if all the display monitors have the same color format, zero otherwise. Note that two displays can have the same bit depth, but different color formats. For example, the red, green, and blue pixels can be encoded with different numbers of bits, or those bits can be located in different places in a pixel's color value. 
			/// </summary>
			SM_SAMEDISPLAYFORMAT = 81,
			/// <summary>
			/// Windows XP (v5.1+) Nonzero if Input Method Manager/Input Method Editor features are enabled; zero otherwise
			/// </summary>
			SM_IMMENABLED = 82,
			/// <summary>
			/// Windows XP (v5.1+) Width of the left and right edges of the focus rectangle drawn by DrawFocusRect. This value is in pixels. 
			/// </summary>
			SM_CXFOCUSBORDER = 83,
			/// <summary>
			/// Windows XP (v5.1+) Height of the top and bottom edges of the focus rectangle drawn by DrawFocusRect. This value is in pixels. 
			/// </summary>
			SM_CYFOCUSBORDER = 84,
			/// <summary>
			/// Nonzero if the current operating system is the Windows XP Tablet PC edition, zero if not.
			/// </summary>
			SM_TABLETPC = 86,
			/// <summary>
			/// Nonzero if the current operating system is the Windows XP, Media Center Edition, zero if not.
			/// </summary>
			SM_MEDIACENTER = 87,
			/// <summary>
			/// Metrics Other
			/// </summary>
			SM_CMETRICS_OTHER = 76,
			/// <summary>
			/// Metrics Windows 2000
			/// </summary>
			SM_CMETRICS_2000 = 83,
			/// <summary>
			/// Metrics Windows NT
			/// </summary>
			SM_CMETRICS_NT = 88,
            /// <summary>
            /// The build number if the system is Windows Server 2003 R2; otherwise, 0.
            /// </summary>
            SM_SERVERR2 = 89,
			/// <summary>
			/// Windows XP (v5.1+) This system metric is used in a Terminal Services environment. If the calling process is associated with a Terminal Services client session, the return value is nonzero. If the calling process is associated with the Terminal Server console session, the return value is zero. The console session is not necessarily the physical console - see WTSGetActiveConsoleSessionId for more information. 
			/// </summary>
			SM_REMOTESESSION = 0x1000,
			/// <summary>
			/// Windows XP (v5.1+) Nonzero if the current session is shutting down; zero otherwise
			/// </summary>
			SM_SHUTTINGDOWN = 0x2000,
			/// <summary>
			/// Windows XP (v5.1+) This system metric is used in a Terminal Services environment. Its value is nonzero if the current session is remotely controlled; zero otherwise
			/// </summary>
			SM_REMOTECONTROL = 0x2001,
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

        [DllImport("user32.dll")]
        internal static extern int GetSystemMetrics(SystemMetric smIndex);
			
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// only comes in an ANSI flavor, hence we help the runtime by telling 
        /// it to always use ANSI when marshalling the string parameter. 
        /// We also prevent the runtime looking for a non-existent GetProcAddressA, 
        /// because the default for C# is to set ExactSpelling to false
        /// </summary>
        /// <param name="hModule"></param>
        /// <param name="procName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true, BestFitMapping = false)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        internal static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);

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

			/// <summary>
			/// Checks the and initialize internet explorer browser emulation for the current executable.
			/// </summary>
			/// <param name="appExeName">Name of the application exe (without path, but with extension).</param>
			/// <param name="forceToInstalledIEVersion">if set to <c>true</c> the function force the emulation to the installed IE version.</param>
			void CheckAndInitInternetExplorerBrowserEmulation(string appExeName, bool forceToInstalledIEVersion);
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
			/// Checks the and initialize internet explorer browser emulation for the current executable.
			/// </summary>
			/// <param name="appExeName">Name of the application exe (without path, but with extension).</param>
			/// <param name="forceToInstalledIEVersion">if set to <c>true</c> the function force the emulation to the installed IE version.</param>
			void IRegistry.CheckAndInitInternetExplorerBrowserEmulation(string appExeName, bool forceToInstalledIEVersion)
			{
				var currentVersion = GetInternetExplorerVersion();

				// see http://support.microsoft.com/kb/969393 
				if (currentVersion >= new Version(9, 10))
				{
					InternetExplorerFeature.SetBrowserEmulation(appExeName,
						forceToInstalledIEVersion
							? InternetFeatureBrowserEmulation.IE10StandardMode
							: InternetFeatureBrowserEmulation.IE10Mode);
					return;
				}

				if (currentVersion >= new Version(9, 0))
				{
					InternetExplorerFeature.SetBrowserEmulation(appExeName,
						forceToInstalledIEVersion
							? InternetFeatureBrowserEmulation.IE9StandardMode
							: InternetFeatureBrowserEmulation.IE9Mode);
					return;
				}

				if (currentVersion >= new Version(8, 0))
				{
					InternetExplorerFeature.SetBrowserEmulation(appExeName,
						forceToInstalledIEVersion
							? InternetFeatureBrowserEmulation.IE8StandardMode
							: InternetFeatureBrowserEmulation.IE8Mode);
					return;
				}

			}

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
			private int _instanceActivatorPort;

			/// <summary>
			/// Checks the and initialize internet explorer browser emulation for the current executable.
			/// </summary>
			/// <param name="appExeName">Name of the application exe (without path, but with extension).</param>
			/// <param name="forceToInstalledIEVersion">if set to <c>true</c> the function force the emulation to the installed IE version.</param>
			void IRegistry.CheckAndInitInternetExplorerBrowserEmulation(string appExeName, bool forceToInstalledIEVersion)
			{
				// no implementation: we don't like to touch the guest machine with registry changes...
			}

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
					if (_instanceActivatorPort != 0)
						return _instanceActivatorPort;
					
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
						
						_instanceActivatorPort = retval;
						return _instanceActivatorPort;
						
					} catch (Exception ex) {
						_log.Error("Cannot get InstanceActivatorPort from .port file", ex);
						return 0;
					} 
				}
				set {
					if (_instanceActivatorPort != value) {
						string portFilePath = RssBanditApplication.GetUserPath();
						try {
							if (!Directory.Exists(portFilePath))
								Directory.CreateDirectory(portFilePath);
					
							using (Stream s  = FileHelper.OpenForWrite(Path.Combine(portFilePath, ".port"))) {
								TextWriter w = new StreamWriter(s);
								w.Write(value);
								w.Flush();
							}
						
							_instanceActivatorPort = value;
						
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
            StringBuilder dispStr = new StringBuilder("Microsoft ");

			_os = Environment.OSVersion;
            _osvi = new OSVERSIONINFOEX();
            _osvi.dwOSVersionInfoSize = Marshal.SizeOf(_osvi);
            if (!GetVersionEx(ref _osvi))
            {
                int err = Marshal.GetLastWin32Error();
                _log.Error("Requesting Windows VersionInfo using GetVersionEx() caused an windows error (Code: " + err + ").");
            }

			if (_os.Platform == PlatformID.Win32Windows) 
            {
				IsWin9x = true;
                dispStr.AppendFormat("{0} ", "Windows 9x");
			} 
            else 
            {
				try { IsAspNetServer = Thread.GetDomain().GetData(".appDomain") != null; }
				catch { /* all */ }

				IsWinNt = true;
				
                // Call GetNativeSystemInfo if supported or GetSystemInfo otherwise.

                _systemInfo = new SYSTEM_INFO();
                IntPtr pGNSI = GetProcAddress(GetModuleHandle("kernel32.dll"), "GetNativeSystemInfo");
                if (IntPtr.Zero != pGNSI)
                {
                    var invoker = (GetNativeSystemInfoInvoker)Marshal.GetDelegateForFunctionPointer(pGNSI, typeof(GetNativeSystemInfoInvoker));
                    invoker(ref _systemInfo);
                }
                else
                    GetSystemInfo(ref _systemInfo);

                // The algorithm used is mostly similar to that: 
                // http://msdn.microsoft.com/en-us/library/ms724429(VS.85).aspx
                if (_os.Version.Major > 6)
                {
                    // just to get the VERY NEW WINDOWS versions:
                    dispStr.AppendFormat("Windows {0} ", _os.Version);
                }

                if (_os.Version.Major == 6)
                {
                    if (_os.Version.Minor == 0)
                    {
                        if (_osvi.wProductType == VER_NT_WORKSTATION)
                        {
                            IsVista = true; 
                            dispStr.Append("Windows Vista ");
                        }
                        else
                        {
                            IsWin2k8 = true;
                            dispStr.Append("Windows Server 2008 ");
                        }
                    }

                    if (_os.Version.Minor == 1)
                    {
                        if (_osvi.wProductType == VER_NT_WORKSTATION)
                        {
                            IsWin7 = true;
                            dispStr.Append("Windows 7 ");
                        }
                        else
                        {
                            IsWin2k8R2 = true;
                            dispStr.Append("Windows Server 2008 R2 ");
                        }
                    }

                    if (_os.Version.Minor > 1)
                    {
                        // just to get the VERY NEW WINDOWS versions:
                        dispStr.AppendFormat("Windows {0} ", _os.Version);
                    }

                    dispStr.Append(GetProductInfo());

                } // if (_os.Version.Major == 6)

                if ((_os.Version.Major == 5) && (_os.Version.Minor == 2))
                {
                    IsWin2k3 = true;
                    if (0 != GetSystemMetrics(SystemMetric.SM_SERVERR2))
                        dispStr.Append("Windows Server 2003 R2, ");
                    else if (GetWindowsSuiteInfo() == WindowsSuite.VER_SUITE_STORAGE_SERVER)
                        dispStr.Append("Windows Storage Server 2003");
                    else if (GetWindowsSuiteInfo() == WindowsSuite.VER_SUITE_WH_SERVER)
                        dispStr.Append("Windows Home Server");
                    else if (_osvi.wProductType == VER_NT_WORKSTATION &&
                             _systemInfo.processorArchitecture == PROCESSOR_ARCHITECTURE_AMD64)
                    {
                        dispStr.Append("Windows XP Professional x64 Edition");
                    }
                    else dispStr.Append("Windows Server 2003, ");

                    // Test for the server type.
                    if (_osvi.wProductType != VER_NT_WORKSTATION)
                    {
                        if (_systemInfo.processorArchitecture == PROCESSOR_ARCHITECTURE_IA64)
                        {
                            if (IsWindowsSuite(WindowsSuite.VER_SUITE_DATACENTER))
                                dispStr.Append("Datacenter Edition for Itanium-based Systems");
                            else if (IsWindowsSuite(WindowsSuite.VER_SUITE_ENTERPRISE))
                                dispStr.Append("Enterprise Edition for Itanium-based Systems");
                        }

                        else if (_systemInfo.processorArchitecture == PROCESSOR_ARCHITECTURE_AMD64)
                        {
                            if (IsWindowsSuite(WindowsSuite.VER_SUITE_DATACENTER))
                                dispStr.Append("Datacenter x64 Edition");
                            else if (IsWindowsSuite(WindowsSuite.VER_SUITE_ENTERPRISE))
                                dispStr.Append("Enterprise x64 Edition");
                            else dispStr.Append("Standard x64 Edition");
                        }

                        else
                        {
                            if (IsWindowsSuite(WindowsSuite.VER_SUITE_COMPUTE_SERVER))
                                dispStr.Append("Compute Cluster Edition");
                            else if (IsWindowsSuite(WindowsSuite.VER_SUITE_DATACENTER))
                                dispStr.Append("Datacenter Edition");
                            else if (IsWindowsSuite(WindowsSuite.VER_SUITE_ENTERPRISE))
                                dispStr.Append("Enterprise Edition");
                            else if (IsWindowsSuite(WindowsSuite.VER_SUITE_BLADE))
                                dispStr.Append("Web Edition");
                            else dispStr.Append("Standard Edition");
                        }
                    }

                } // if ((_os.Version.Major == 5) && (_os.Version.Minor == 2))

                if ((_os.Version.Major == 5) && (_os.Version.Minor == 1))
                {
                    IsWinXP = true;
                    dispStr.Append("Windows XP ");
                    if (IsWindowsSuite(WindowsSuite.VER_SUITE_PERSONAL))
                        dispStr.Append("Home Edition");
                    else dispStr.Append("Professional");

                } // if ((_os.Version.Major == 5) && (_os.Version.Minor == 1))

			    if ((_os.Version.Major == 5) && (_os.Version.Minor == 0)) 
                {
					IsWin2K = true;
                    dispStr.Append("Windows 2000 ");

                    if (_osvi.wProductType == VER_NT_WORKSTATION)
                    {
                        dispStr.Append("Professional");
                    }
                    else
                    {
                        if (IsWindowsSuite(WindowsSuite.VER_SUITE_DATACENTER))
                            dispStr.Append("Datacenter Server");
                        else if (IsWindowsSuite(WindowsSuite.VER_SUITE_ENTERPRISE))
                            dispStr.Append("Advanced Server");
                        else dispStr.Append("Server");
                    }
                } // if ((_os.Version.Major == 5) && (_os.Version.Minor == 0)) 
                
			}

            // Include service pack (if any) and build number.

            if (!String.IsNullOrEmpty(_osvi.szCSDVersion))
                dispStr.AppendFormat(" {0}", _osvi.szCSDVersion);

            dispStr.AppendFormat(" (build {0})", _osvi.dwBuildNumber);

            if (_os.Version.Major >= 6)
            {
                if (_systemInfo.processorArchitecture == PROCESSOR_ARCHITECTURE_AMD64)
                    dispStr.Append(", 64-bit");
                else if (_systemInfo.processorArchitecture == PROCESSOR_ARCHITECTURE_INTEL)
                    dispStr.Append(", 32-bit");
            }

            _osDisplayString = dispStr.ToString();
            IEVersion = Registry.GetInternetExplorerVersion();

		}

		private static readonly log4net.ILog _log = Log.GetLogger(typeof(Win32));
		private static int _paintFrozen;
		private static readonly OperatingSystem _os;

		// General info fields
		internal static readonly bool IsAspNetServer;
		internal static readonly bool IsWin2K;
		internal static readonly bool IsWin2k3;
        internal static readonly bool IsWin2k8;
	    internal static readonly bool IsWin2k8R2;
        internal static readonly bool IsWin9x;  // opposite of IsWinNt
		internal static readonly bool IsWinNt;  // opposite of IsWin9x
        internal static readonly bool IsWinXP;
        internal static readonly bool IsVista;
        internal static readonly bool IsWin7;
		
		internal static readonly Version IEVersion;
        private static readonly SYSTEM_INFO _systemInfo;
	    private static readonly OSVERSIONINFOEX _osvi;
        private static readonly string _osDisplayString;

		#region General
        /// <summary>
        /// Gets a value indicating whether the process runs with 64 bit or not.
        /// </summary>
        /// <value><c>true</c> if [is64 bit]; otherwise, <c>false</c>.</value>
        public static bool Is64Bit
        {
            get { return IntPtr.Size == 8; }
        }

        /// <summary>
        /// Gets a value indicating whether the process runs with 32 bit or not.
        /// </summary>
        /// <value><c>true</c> if [is32 bit]; otherwise, <c>false</c>.</value>
        public static bool Is32Bit
        {
            get { return IntPtr.Size == 4; }
        }

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
        /// Gets the bit size of the computer system.
        /// </summary>
        /// <value>The size of the system bit.</value>
        public static ProcessorArchitecture ProcessorArchitecture
        {
            get
            {
                // see also: http://msdn2.microsoft.com/en-us/library/ms724958.aspx
                if (_systemInfo.processorArchitecture == PROCESSOR_ARCHITECTURE_INTEL)
                    return ProcessorArchitecture.X86;
                if (_systemInfo.processorArchitecture == PROCESSOR_ARCHITECTURE_IA64)
                    return ProcessorArchitecture.IA64;
                if (_systemInfo.processorArchitecture == PROCESSOR_ARCHITECTURE_AMD64)
                    return ProcessorArchitecture.Amd64;
                if (_systemInfo.processorArchitecture == PROCESSOR_ARCHITECTURE_UNKNOWN)
                    return ProcessorArchitecture.None;
                return ProcessorArchitecture.None;
            }
        }

		/// <summary>
		/// Gets the windows service pack info.
		/// </summary>
		/// <param name="servicePackMajor">The service pack major.</param>
		/// <param name="servicePackMinor">The service pack minor.</param>
		public static void GetWindowsServicePackInfo(out int servicePackMajor, out int servicePackMinor) 
        {
			servicePackMajor = _osvi.wServicePackMajor;
			servicePackMinor = _osvi.wServicePackMinor;
		}

        /// <summary>
        /// Gets the windows suite info.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="BanditApplicationException">If there was a Win32 issue</exception>
        public static WindowsSuite GetWindowsSuiteInfo()
        {
            return (WindowsSuite)_osvi.wSuiteMask;
        }
        /// <summary>
        /// Determines whether is windows suite info set to the specified info.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <returns>
        /// 	<c>true</c> if [is windows suite] [the specified info]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsWindowsSuite(WindowsSuite info)
        {
            return ((WindowsSuite)_osvi.wSuiteMask & info) == info;
        }

        /// <summary>
        /// Format the window's suite as a string.
        /// </summary>
        /// <param name="suite">The suite.</param>
        /// <returns></returns>
        public static string WindowsSuiteToString(WindowsSuite suite)
        {
            var sb = new StringBuilder();
            if ((suite & WindowsSuite.VER_SERVER_NT) != 0)
                sb.Append("NT Server, ");
            if ((suite & WindowsSuite.VER_WORKSTATION_NT) != 0)
                sb.Append("NT WorkStation, ");
            if ((suite & WindowsSuite.VER_SUITE_SMALLBUSINESS) != 0)
                sb.Append("Small Business Server, ");
            if ((suite & WindowsSuite.VER_SUITE_SMALLBUSINESS_RESTRICTED) != 0)
                sb.Append("Small Business Server (restricted license), ");
            if ((suite & WindowsSuite.VER_SUITE_ENTERPRISE) != 0)
            {
                if (IsWin2K)
                    sb.Append("Advanced Server, ");
                else
                    sb.Append("Enterprise Edition, ");
            }
            if ((suite & WindowsSuite.VER_SUITE_BACKOFFICE) != 0)
                sb.Append("BackOffice installed, ");
            if ((suite & WindowsSuite.VER_SUITE_COMMUNICATIONS) != 0)
                sb.Append("Communications, ");
            if ((suite & WindowsSuite.VER_SUITE_TERMINAL) != 0)
                sb.Append("Terminal Services installed, ");
            if ((suite & WindowsSuite.VER_SUITE_SINGLEUSERTS) != 0)
                sb.Append("Terminal Service installed (one interactive session only), ");
            if ((suite & WindowsSuite.VER_SUITE_EMBEDDEDNT) != 0)
                sb.Append("Embedded NT, ");
            if ((suite & WindowsSuite.VER_SUITE_EMBEDDED_RESTRICTED) != 0)
                sb.Append("Embedded (restricted), ");
            if ((suite & WindowsSuite.VER_SUITE_DATACENTER) != 0)
            {
                if (IsWin2K)
                    sb.Append("Datacenter Server, ");
                else
                    sb.Append("Datacenter Edition, ");
            }
            if ((suite & WindowsSuite.VER_SUITE_PERSONAL) != 0)
                sb.Append("Home Edition, ");
            if ((suite & WindowsSuite.VER_SUITE_BLADE) != 0)
                sb.Append("Web Edition, ");

            // remove ", "
            if (sb.Length > 2)
                sb.Remove(sb.Length - 2, 2);

            return sb.ToString();
        }

        /// <summary>
        /// Gets the product info, if available (supported with Win.Major == 6), 
        /// else returns an empty string.
        /// </summary>
        /// <returns></returns>
        public static string GetProductInfo()
        {
            if (_os.Version.Major == 6)
            {
                IntPtr pGPI = GetProcAddress(GetModuleHandle("kernel32.dll"), "GetProductInfo");
                if (pGPI != IntPtr.Zero)
                {
                    var invoker = (GetProductInfoInvoker)Marshal.GetDelegateForFunctionPointer(pGPI, typeof(GetProductInfoInvoker));
                    uint dwType;
                    if (invoker(_os.Version.Major, _os.Version.Minor, 0, 0, out dwType))
                        return WindowsProductType.Get(dwType);
                }
            }
            return String.Empty;
        }

        /// <summary>
        /// Gets the OS version display string.
        /// </summary>
        /// <value>The OS version display string.</value>
        public static string OSVersionDisplayString
        {
            get { return _osDisplayString; }
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
