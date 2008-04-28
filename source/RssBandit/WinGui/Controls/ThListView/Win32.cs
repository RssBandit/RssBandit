#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.ThListView
{
	/// <summary>
	/// Win32.
	/// </summary>
	internal static class Win32
	{
		
		#region ComInterop stuff

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT {
			public int x;
			public int y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SIZE {
			public int cx;
			public int cy;
		}

		/// <summary>
		/// First message codes for sub message groups
		/// </summary>
		public enum BaseCodes : int {
			LVM_FIRST = 0x1000,			// Listview
			TV_FIRST  = 0x1100,			// Treeview
			HDM_FIRST = 0x1200,			// Header messages
			TCM_FIRST = 0x1300,			// tab control
			PGM_FIRST = 0x1400,			// Pager control messages
			ECM_FIRST = 0x1500,
			BCM_FIRST = 0x1600,
			CBM_FIRST = 0x1700,
			CCM_FIRST = 0x2000,			// Common control shared messages
			NM_FIRST  = 0x0000,    
			LVN_FIRST = ( NM_FIRST - 100 ),     
			HDN_FIRST = ( NM_FIRST - 300 ),
		}

		/// <summary>
		/// Windows message codes WM_
		/// </summary>
		public enum W32_WM : int {
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
			WM_CTLCOLOR               = 0x0019,
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
			WM_USER                   = 0x0400,
			WM_REFLECT                = WM_USER + 0x1c00
		}


		/// <summary>
		/// Notify message codes NM_
		/// </summary>
		public enum W32_NM : int {
			NM_FIRST           = BaseCodes.NM_FIRST,
			NM_OUTOFMEMORY     = ( NM_FIRST - 1 ),
			NM_CLICK           = ( NM_FIRST - 2 ),
			NM_DBLCLK          = ( NM_FIRST - 3 ),
			NM_RETURN          = ( NM_FIRST - 4 ),
			NM_RCLICK          = ( NM_FIRST - 5 ),
			NM_RDBLCLK         = ( NM_FIRST - 6 ),
			NM_SETFOCUS        = ( NM_FIRST - 7 ),
			NM_KILLFOCUS       = ( NM_FIRST - 8 ),
			NM_CUSTOMDRAW      = ( NM_FIRST - 12 ),
			NM_HOVER           = ( NM_FIRST - 13 ),
			NM_NCHITTEST       = ( NM_FIRST - 14 ),
			NM_KEYDOWN         = ( NM_FIRST - 15 ),
			NM_RELEASEDCAPTURE = ( NM_FIRST - 16 ),
			NM_SETCURSOR       = ( NM_FIRST - 17 ),
			NM_CHAR            = ( NM_FIRST - 18 ),
			NM_TOOLTIPSCREATED = ( NM_FIRST - 19 ),
			NM_LDOWN           = ( NM_FIRST - 20 ),
			NM_RDOWN           = ( NM_FIRST - 21 ),
			NM_THEMECHANGED    = ( NM_FIRST - 22 )
		}


		/// <summary>
		/// Reflected message codes OCM_
		/// </summary>
		public enum W32_OCM : int {
			OCM__BASE				      = W32_WM.WM_REFLECT,
			OCM_COMMAND				    = ( OCM__BASE + W32_WM.WM_COMMAND ),
			OCM_CTLCOLORBTN			  = ( OCM__BASE + W32_WM.WM_CTLCOLORBTN ),
			OCM_CTLCOLOREDIT		  = ( OCM__BASE + W32_WM.WM_CTLCOLOREDIT ),
			OCM_CTLCOLORDLG			  = ( OCM__BASE + W32_WM.WM_CTLCOLORDLG ),
			OCM_CTLCOLORLISTBOX		= ( OCM__BASE + W32_WM.WM_CTLCOLORLISTBOX ),
			OCM_CTLCOLORMSGBOX		= ( OCM__BASE + W32_WM.WM_CTLCOLORMSGBOX ),
			OCM_CTLCOLORSCROLLBAR = ( OCM__BASE + W32_WM.WM_CTLCOLORSCROLLBAR ),
			OCM_CTLCOLORSTATIC		= ( OCM__BASE + W32_WM.WM_CTLCOLORSTATIC ),
			OCM_CTLCOLOR		    	= ( OCM__BASE + W32_WM.WM_CTLCOLOR ),
			OCM_DRAWITEM			    = ( OCM__BASE + W32_WM.WM_DRAWITEM ),
			OCM_MEASUREITEM			  = ( OCM__BASE + W32_WM.WM_MEASUREITEM ),
			OCM_DELETEITEM        = ( OCM__BASE + W32_WM.WM_DELETEITEM ),
			OCM_VKEYTOITEM        = ( OCM__BASE + W32_WM.WM_VKEYTOITEM ),
			OCM_CHARTOITEM        = ( OCM__BASE + W32_WM.WM_CHARTOITEM ),
			OCM_COMPAREITEM       = ( OCM__BASE + W32_WM.WM_COMPAREITEM ),
			OCM_HSCROLL           = ( OCM__BASE + W32_WM.WM_HSCROLL ),
			OCM_VSCROLL           = ( OCM__BASE + W32_WM.WM_VSCROLL ),
			OCM_PARENTNOTIFY      = ( OCM__BASE + W32_WM.WM_PARENTNOTIFY ),
			OCM_NOTIFY            = ( OCM__BASE + W32_WM.WM_NOTIFY )
		}

		/// <summary>
		/// ListView message codes LVM_
		/// </summary>
		public enum W32_LVM : int {
			LVM_FIRST                    = BaseCodes.LVM_FIRST,
			LVM_GETBKCOLOR               = ( LVM_FIRST + 0 ),
			LVM_SETBKCOLOR               = ( LVM_FIRST + 1 ),
			LVM_GETIMAGELIST             = ( LVM_FIRST + 2 ),
			LVM_SETIMAGELIST             = ( LVM_FIRST + 3 ),
			LVM_GETITEMCOUNT             = ( LVM_FIRST + 4 ),
			LVM_GETITEMA                 = ( LVM_FIRST + 5 ),
			LVM_GETITEMW                 = ( LVM_FIRST + 75 ),
			LVM_SETITEMA                 = ( LVM_FIRST + 6 ),
			LVM_SETITEMW                 = ( LVM_FIRST + 76 ),
			LVM_INSERTITEMA              = ( LVM_FIRST + 7 ),
			LVM_INSERTITEMW              = ( LVM_FIRST + 77 ),
			LVM_DELETEITEM               = ( LVM_FIRST + 8 ),
			LVM_DELETEALLITEMS           = ( LVM_FIRST + 9 ),
			LVM_GETCALLBACKMASK          = ( LVM_FIRST + 10 ),
			LVM_SETCALLBACKMASK          = ( LVM_FIRST + 11 ),
			LVM_GETNEXTITEM              = ( LVM_FIRST + 12 ),
			LVM_FINDITEMA                = ( LVM_FIRST + 13 ),
			LVM_FINDITEMW                = ( LVM_FIRST + 83 ),
			LVM_GETITEMRECT              = ( LVM_FIRST + 14 ),
			LVM_SETITEMPOSITION          = ( LVM_FIRST + 15 ),
			LVM_GETITEMPOSITION          = ( LVM_FIRST + 16 ),
			LVM_GETSTRINGWIDTHA          = ( LVM_FIRST + 17 ),
			LVM_GETSTRINGWIDTHW          = ( LVM_FIRST + 87 ),
			LVM_HITTEST                  = ( LVM_FIRST + 18 ),
			LVM_ENSUREVISIBLE            = ( LVM_FIRST + 19 ),
			LVM_SCROLL                   = ( LVM_FIRST + 20 ),
			LVM_REDRAWITEMS              = ( LVM_FIRST + 21 ),
			LVM_ARRANGE                  = ( LVM_FIRST + 22 ),
			LVM_EDITLABELA               = ( LVM_FIRST + 23 ),
			LVM_EDITLABELW               = ( LVM_FIRST + 118 ),
			LVM_GETEDITCONTROL           = ( LVM_FIRST + 24 ),
			LVM_GETCOLUMNA               = ( LVM_FIRST + 25 ),
			LVM_GETCOLUMNW               = ( LVM_FIRST + 95 ),
			LVM_SETCOLUMNA               = ( LVM_FIRST + 26 ),
			LVM_SETCOLUMNW               = ( LVM_FIRST + 96 ),
			LVM_INSERTCOLUMNA            = ( LVM_FIRST + 27 ),
			LVM_INSERTCOLUMNW            = ( LVM_FIRST + 97 ),
			LVM_DELETECOLUMN             = ( LVM_FIRST + 28 ),
			LVM_GETCOLUMNWIDTH           = ( LVM_FIRST + 29 ),
			LVM_SETCOLUMNWIDTH           = ( LVM_FIRST + 30 ),
			LVM_GETHEADER                = ( LVM_FIRST + 31 ),
			LVM_CREATEDRAGIMAGE          = ( LVM_FIRST + 33 ),
			LVM_GETVIEWRECT              = ( LVM_FIRST + 34 ),
			LVM_GETTEXTCOLOR             = ( LVM_FIRST + 35 ),
			LVM_SETTEXTCOLOR             = ( LVM_FIRST + 36 ),
			LVM_GETTEXTBKCOLOR           = ( LVM_FIRST + 37 ),
			LVM_SETTEXTBKCOLOR           = ( LVM_FIRST + 38 ),
			LVM_GETTOPINDEX              = ( LVM_FIRST + 39 ),
			LVM_GETCOUNTPERPAGE          = ( LVM_FIRST + 40 ),
			LVM_GETORIGIN                = ( LVM_FIRST + 41 ),
			LVM_UPDATE                   = ( LVM_FIRST + 42 ),
			LVM_SETITEMSTATE             = ( LVM_FIRST + 43 ),
			LVM_GETITEMSTATE             = ( LVM_FIRST + 44 ),
			LVM_GETITEMTEXTA             = ( LVM_FIRST + 45 ),
			LVM_GETITEMTEXTW             = ( LVM_FIRST + 115 ),
			LVM_SETITEMTEXTA             = ( LVM_FIRST + 46 ),
			LVM_SETITEMTEXTW             = ( LVM_FIRST + 116 ),
			LVM_SETITEMCOUNT             = ( LVM_FIRST + 47 ),
			LVM_SORTITEMS                = ( LVM_FIRST + 48 ),
			LVM_SETITEMPOSITION32        = ( LVM_FIRST + 49 ),
			LVM_GETSELECTEDCOUNT         = ( LVM_FIRST + 50 ),
			LVM_GETITEMSPACING           = ( LVM_FIRST + 51 ),
			LVM_GETISEARCHSTRINGA        = ( LVM_FIRST + 52 ),
			LVM_GETISEARCHSTRINGW        = ( LVM_FIRST + 117 ),
			LVM_SETICONSPACING           = ( LVM_FIRST + 53 ),
			LVM_SETEXTENDEDLISTVIEWSTYLE = ( LVM_FIRST + 54 ),
			LVM_GETEXTENDEDLISTVIEWSTYLE = ( LVM_FIRST + 55 ),
			LVM_GETSUBITEMRECT           = ( LVM_FIRST + 56 ),
			LVM_SUBITEMHITTEST           = ( LVM_FIRST + 57 ),
			LVM_SETCOLUMNORDERARRAY      = ( LVM_FIRST + 58 ),
			LVM_GETCOLUMNORDERARRAY      = ( LVM_FIRST + 59 ),
			LVM_SETHOTITEM               = ( LVM_FIRST + 60 ),
			LVM_GETHOTITEM               = ( LVM_FIRST + 61 ),
			LVM_SETHOTCURSOR             = ( LVM_FIRST + 62 ),
			LVM_GETHOTCURSOR             = ( LVM_FIRST + 63 ),
			LVM_APPROXIMATEVIEWRECT      = ( LVM_FIRST + 64 ),
			LVM_SETWORKAREAS             = ( LVM_FIRST + 65 ),
			LVM_GETWORKAREAS             = ( LVM_FIRST + 70 ),
			LVM_GETNUMBEROFWORKAREAS     = ( LVM_FIRST + 73 ),
			LVM_GETSELECTIONMARK         = ( LVM_FIRST + 66 ),
			LVM_SETSELECTIONMARK         = ( LVM_FIRST + 67 ),
			LVM_SETHOVERTIME             = ( LVM_FIRST + 71 ),
			LVM_GETHOVERTIME             = ( LVM_FIRST + 72 ),
			LVM_SETTOOLTIPS              = ( LVM_FIRST + 74 ),
			LVM_GETTOOLTIPS              = ( LVM_FIRST + 78 ),
			LVM_SORTITEMSEX              = ( LVM_FIRST + 81 ),
			LVM_SETBKIMAGEA              = ( LVM_FIRST + 68 ),
			LVM_SETBKIMAGEW              = ( LVM_FIRST + 138 ),
			LVM_GETBKIMAGEA              = ( LVM_FIRST + 69 ),
			LVM_GETBKIMAGEW              = ( LVM_FIRST + 139 ),
			LVM_SETSELECTEDCOLUMN        = ( LVM_FIRST + 140 ),
			LVM_SETTILEWIDTH             = ( LVM_FIRST + 141 ),
			LVM_SETVIEW                  = ( LVM_FIRST + 142 ),
			LVM_GETVIEW                  = ( LVM_FIRST + 143 ),
			LVM_INSERTGROUP              = ( LVM_FIRST + 145 ),
			LVM_SETGROUPINFO             = ( LVM_FIRST + 147 ),
			LVM_GETGROUPINFO             = ( LVM_FIRST + 149 ),
			LVM_REMOVEGROUP              = ( LVM_FIRST + 150 ),
			LVM_MOVEGROUP                = ( LVM_FIRST + 151 ),
			LVM_MOVEITEMTOGROUP          = ( LVM_FIRST + 154 ),
			LVM_SETGROUPMETRICS          = ( LVM_FIRST + 155 ),
			LVM_GETGROUPMETRICS          = ( LVM_FIRST + 156 ),
			LVM_ENABLEGROUPVIEW          = ( LVM_FIRST + 157 ),
			LVM_SORTGROUPS               = ( LVM_FIRST + 158 ),
			LVM_INSERTGROUPSORTED        = ( LVM_FIRST + 159 ),
			LVM_REMOVEALLGROUPS          = ( LVM_FIRST + 160 ),
			LVM_HASGROUP                 = ( LVM_FIRST + 161 ),
			LVM_SETTILEVIEWINFO          = ( LVM_FIRST + 162 ),
			LVM_GETTILEVIEWINFO          = ( LVM_FIRST + 163 ),
			LVM_SETTILEINFO              = ( LVM_FIRST + 164 ),
			LVM_GETTILEINFO              = ( LVM_FIRST + 165 ),
			LVM_SETINSERTMARK            = ( LVM_FIRST + 166 ),
			LVM_GETINSERTMARK            = ( LVM_FIRST + 167 ),
			LVM_INSERTMARKHITTEST        = ( LVM_FIRST + 168 ),
			LVM_GETINSERTMARKRECT        = ( LVM_FIRST + 169 ),
			LVM_SETINSERTMARKCOLOR       = ( LVM_FIRST + 170 ),
			LVM_GETINSERTMARKCOLOR       = ( LVM_FIRST + 171 ),
			LVM_SETINFOTIP               = ( LVM_FIRST + 173 ),
			LVM_GETSELECTEDCOLUMN        = ( LVM_FIRST + 174 ),
			LVM_ISGROUPVIEWENABLED       = ( LVM_FIRST + 175 ),
			LVM_GETOUTLINECOLOR          = ( LVM_FIRST + 176 ),
			LVM_SETOUTLINECOLOR          = ( LVM_FIRST + 177 ),
			LVM_CANCELEDITLABEL          = ( LVM_FIRST + 179 ),
			LVM_MAPINDEXTOID             = ( LVM_FIRST + 180 ),
			LVM_MAPIDTOINDEX             = ( LVM_FIRST + 181 )                                
		}

		#region HeaderItem masks
        [Flags]
		public enum HeaderItemMask {
			HDI_WIDTH               = 0x0001,
			HDI_HEIGHT              = HDI_WIDTH,
			HDI_TEXT                = 0x0002,
			HDI_FORMAT              = 0x0004,
			HDI_LPARAM              = 0x0008,
			HDI_BITMAP              = 0x0010,
			HDI_IMAGE               = 0x0020,
			HDI_DI_SETITEM          = 0x0040,
			HDI_ORDER               = 0x0080,
			HDI_FILTER			= 0x0100	// 0x0501
		}
		#endregion

		#region HeaderItem flags
		public enum HeaderItemFlags {
			HDF_LEFT			= 0x0000,
			HDF_RIGHT			= 0x0001,
			HDF_CENTER			= 0x0002,
			HDF_JUSTIFYMASK		= 0x0003,
			HDF_RTLREADING		= 0x0004,
			HDF_OWNERDRAW		= 0x8000,
			HDF_STRING			= 0x4000,
			HDF_BITMAP			= 0x2000,
			HDF_BITMAP_ON_RIGHT = 0x1000,
			HDF_IMAGE			= 0x0800,
			HDF_SORTUP			= 0x0400,		// 0x0600
			HDF_SORTDOWN		= 0x0200,		// 0x0600
			HDF_NOJUSTIFY       = 0xFFFC,		// !HDF_JUSTIFYMASK
		}
		#endregion

		#region Header Control Messages
		public enum HeaderControlMessages : int {
			HDM_FIRST        =  0x1200,
			HDM_GETITEMRECT  = (HDM_FIRST + 7),
			HDM_HITTEST      = (HDM_FIRST + 6),
			HDM_SETIMAGELIST = (HDM_FIRST + 8),
			HDM_GETITEM     = (HDM_FIRST + 11),
			HDM_SETITEM     = (HDM_FIRST + 12),
			HDM_ORDERTOINDEX = (HDM_FIRST + 15)
		}
		#endregion

		#region Header Control Notifications
		public enum HeaderControlNotifications {
			HDN_FIRST       = (0-300),
			HDN_BEGINTRACKW = (HDN_FIRST-26),
			HDN_ENDTRACKW   = (HDN_FIRST-27),
			HDN_ITEMCLICKW  = (HDN_FIRST-22),
		}
		#endregion

		#region Header Control HitTest Flags
        [Flags]
		public enum HeaderControlHitTestFlags : uint {
			HHT_NOWHERE             = 0x0001,
			HHT_ONHEADER            = 0x0002,
			HHT_ONDIVIDER           = 0x0004,
			HHT_ONDIVOPEN           = 0x0008,
			HHT_ABOVE               = 0x0100,
			HHT_BELOW               = 0x0200,
			HHT_TORIGHT             = 0x0400,
			HHT_TOLEFT              = 0x0800
		}
		#endregion

		#region List View sub item portion
		public enum SubItemPortion {
			LVIR_BOUNDS = 0,
			LVIR_ICON   = 1,
			LVIR_LABEL  = 2
		}
		#endregion

		/// <summary>
		/// ListView notification message codes LVN_ 
		/// </summary>
		public enum W32_LVN : int {
			LVN_FIRST           = BaseCodes.LVN_FIRST,
			LVN_ITEMCHANGING    = ( LVN_FIRST-0 ),
			LVN_ITEMCHANGED     = ( LVN_FIRST-1 ),
			LVN_INSERTITEM      = ( LVN_FIRST-2 ),
			LVN_DELETEITEM      = ( LVN_FIRST-3 ),
			LVN_DELETEALLITEMS  = ( LVN_FIRST-4 ),
			LVN_BEGINLABELEDITA = ( LVN_FIRST-5 ),
			LVN_ENDLABELEDITA   = ( LVN_FIRST-6 ),
			LVN_COLUMNCLICK     = ( LVN_FIRST-8 ),
			LVN_BEGINDRAG       = ( LVN_FIRST-9 ),
			LVN_BEGINRDRAG      = ( LVN_FIRST-11 ),
			LVN_ODCACHEHINT     = ( LVN_FIRST-13 ),
			LVN_ITEMACTIVATE    = ( LVN_FIRST-14 ),
			LVN_ODSTATECHANGED  = ( LVN_FIRST-15 ),
			LVN_HOTTRACK        = ( LVN_FIRST-21 ),
			LVN_GETDISPINFOA    = ( LVN_FIRST-50 ),
			LVN_SETDISPINFOA    = ( LVN_FIRST-51 ),
			LVN_ODFINDITEMA     = ( LVN_FIRST-52 ),
			LVN_KEYDOWN         = ( LVN_FIRST-55 ),
			LVN_MARQUEEBEGIN    = ( LVN_FIRST-56 ),
			LVN_GETINFOTIPA     = ( LVN_FIRST-57 ),
			LVN_GETINFOTIPW     = ( LVN_FIRST-58 ),
			LVN_BEGINLABELEDITW = ( LVN_FIRST-75 ),
			LVN_ENDLABELEDITW   = ( LVN_FIRST-76 ),
			LVN_GETDISPINFOW    = ( LVN_FIRST-77 ),
			LVN_SETDISPINFOW    = ( LVN_FIRST-78 ),
			LVN_ODFINDITEMW     = ( LVN_FIRST-79 ),
			LVN_BEGINSCROLL     = ( LVN_FIRST-80 ),          
			LVN_ENDSCROLL       = ( LVN_FIRST-81 )
		}

		
		/// <summary>
		/// ListView extended style flags
		/// </summary>
		[Flags]
		public enum LVS_EX {
			LVS_EX_GRIDLINES        =0x00000001,
			LVS_EX_SUBITEMIMAGES    =0x00000002,
			LVS_EX_CHECKBOXES       =0x00000004,
			LVS_EX_TRACKSELECT      =0x00000008,
			LVS_EX_HEADERDRAGDROP   =0x00000010,
			LVS_EX_FULLROWSELECT    =0x00000020, 
			LVS_EX_ONECLICKACTIVATE =0x00000040,
			LVS_EX_TWOCLICKACTIVATE =0x00000080,
			LVS_EX_FLATSB           =0x00000100,
			LVS_EX_REGIONAL         =0x00000200,
			LVS_EX_INFOTIP          =0x00000400,
			LVS_EX_UNDERLINEHOT     =0x00000800,
			LVS_EX_UNDERLINECOLD    =0x00001000,
			LVS_EX_MULTIWORKAREAS   =0x00002000,
			LVS_EX_LABELTIP         =0x00004000,
			LVS_EX_BORDERSELECT     =0x00008000, 
			LVS_EX_DOUBLEBUFFER     =0x00010000,
			LVS_EX_HIDELABELS       =0x00020000,
			LVS_EX_SINGLEROW        =0x00040000,
			LVS_EX_SNAPTOGRID       =0x00080000,  
			LVS_EX_SIMPLESELECT     =0x00100000  
		}

        [Flags]
		public enum ListViewItemFlags {
			LVIF_TEXT               = 0x0001,
			LVIF_IMAGE              = 0x0002,
			LVIF_PARAM              = 0x0004,
			LVIF_STATE              = 0x0008,
			LVIF_INDENT             = 0x0010,
			LVIF_GROUPID           = 0x0100,
			LVIF_COLUMNS          = 0x0200,
			LVIF_NORECOMPUTE    = 0x0800,
			LVIF_DI_SETITEM      = 0x1000,
		}

		#region Constants for LVCOLUMN.fmt
		public const int LVCFMT_BITMAP_ON_RIGHT = 4096;
		public const int LVCFMT_CENTER = 2;
		public const int LVCFMT_COL_HAS_IMAGES = 32768;
		public const int LVCFMT_IMAGE = 2048;
		public const int LVCFMT_JUSTIFYMASK = 3;
		public const int LVCFMT_LEFT = 0;
		public const int  LVCFMT_RIGHT = 1;
		#endregion

		#region Constants for LVGROUP.mask
		public const int LVGF_ALIGN = 8;
		public const int LVGF_FOOTER = 2;
		public const int LVGF_GROUPID = 16;
		public const int LVGF_HEADER = 1;
		public const int LVGF_NONE = 0;
		public const int LVGF_STATE = 4;
		#endregion

		#region Constants for LVGROUP.uAlign
		public const int LVGA_FOOTER_CENTER = 16;
		public const int LVGA_FOOTER_LEFT = 8;
		public const int LVGA_FOOTER_RIGHT = 23;
		public const int LVGA_HEADER_CENTER = 2;
		public const int LVGA_HEADER_LEFT = 1;
		public const int LVGA_HEADER_RIGHT = 4;
		#endregion

		#region Constants for LVGROUP.state
		public const int LVGS_COLLAPSED = 1;
		public const int LVGS_HIDDEN = 2;
		public const int LVGS_NORMAL = 0;
		#endregion

		#region Consts for BACKGROUND image mods.
		public const int LVBKIF_STYLE_NORMAL = 0;
		public const int LVBKIF_SOURCE_URL = 2;
		public const int LVBKIF_STYLE_TILE = 16;
		public const int LVBKIF_FLAG_TILEOFFSET = 0x00000100 ;
		#endregion

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
		public struct LVITEM {
			public	ListViewItemFlags mask;
			public	int iItem;
			public	int iSubItem;
			public	uint state;
			public	uint stateMask;

           // [MarshalAs(UnmanagedType.LPTStr)]
            public 	IntPtr  pszText;
			public	int cchTextMax;
			public	int iImage;
			public	IntPtr lParam;
			public	int iIndent;
			public int iGroupId; 
			public int cColumns; 
			public IntPtr puColumns; 
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
		public class HDITEM {
			public int     mask; 
			public int     cxy; 
			public	IntPtr pszText; 
			public IntPtr hbm; 
			public int     cchTextMax; 
			public int     fmt;
            public IntPtr  lParam; 
			public int     iImage;
			public int     iOrder;
			public HDITEM() {
				mask = cxy = cchTextMax = fmt = iImage = iOrder = 0;
				pszText = hbm = lParam = IntPtr.Zero;
			}
		};

		/// <summary>
		/// LV Item hit test results
		/// </summary>
		public enum ListViewHitTestFlags: int {
			LVHT_NOWHERE = 0x1,
			LVHT_ONITEMICON = 0x2,
			LVHT_ONITEMLABEL = 0x4,
			LVHT_ONITEMSTATEICON = 0x8,
			LVHT_ONITEM = (LVHT_ONITEMICON | LVHT_ONITEMLABEL | LVHT_ONITEMSTATEICON)
		}

		[StructLayout(LayoutKind.Sequential)] 
			public struct LVHITTESTINFO {
			public POINT pt; 
			public uint  flags; 
			public int   iItem; 
			public int   iSubItem;
		}
		
		[StructLayout(LayoutKind.Sequential)] 
			public struct LVSETINFOTIP {
			public int cbSize;
			public uint dwFlags;
			public string pszText;
			public int iItem;
			public int iSubItem;
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto, Pack=1)]
			public class LVCOLUMN {
			public int mask = 0; 
			public int fmt = 0; 
			public int cx = 0; 
			public	IntPtr pszText = IntPtr.Zero; 
			public int cchTextMax = 0; 
			public int iSubItem = 0; 
			public int iImage = 0; 
			public int iOrder = 0; 
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
		public struct LVGROUP{
			public int cbSize; 
			public int mask; 
			public	IntPtr pszHeader; 
			public int cchHeader; 
			public IntPtr pszFooter;
			public int cchFooter; 
			public int iGroupId; 
			public int stateMask; 
			public int state; 
			public int uAlign; 
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct LVTILEVIEWINFO{
			public int cbSize;
			public int dwMask; 
			public int dwFlags; 
			public SIZE sizeTile; 
			public int cLines;
			public SIZE rcLabelMargin;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct LVTILEINFO {
			public int cbSize; 
			public int iItem; 
			public int cColumns;
            public IntPtr puColumns;
		}
		[StructLayout(LayoutKind.Sequential)]
			public struct LVBKIMAGE{
			public int ulFlags;
			public int hbm;			// Not used according to MSDN
			public IntPtr pszImage;
			public int cchImageMax;
			public int xOffsetPercent;
			public int yOffsetPercent;
		}

		#endregion

		#region Windows API

		[System.Security.SuppressUnmanagedCodeSecurity]
		internal sealed class API {
			
			#region Post/SendMessage overloads
			[DllImport("user32.dll")] public static extern bool 
				PostMessage( IntPtr hWnd, int wMsg, int wParam, IntPtr lParam );
			[DllImport("user32.dll")] public static extern
                IntPtr SendMessage(IntPtr hWnd, W32_LVM wMsg, int wParam, IntPtr lParam);
			[DllImport("user32.dll")] public static extern
                IntPtr SendMessage(IntPtr hWnd, W32_LVM wMsg, int wParam, ref IntPtr lParam);
            // Int[] is okay as the lparam here since pointers are marshalled platform specific sizes anyway
			[DllImport("user32.dll")] public static extern
                IntPtr SendMessage(IntPtr hWnd, W32_LVM wMsg, int wParam, int[] lParam);
			[DllImport("user32.dll")] public static extern
                IntPtr SendMessage(IntPtr hWnd, W32_LVM wMsg, int wParam, ref LVITEM lParam);
			[DllImport("user32.dll")] public static extern
                IntPtr SendMessage(IntPtr hWnd, W32_LVM wMsg, int wParam, ref LVHITTESTINFO lParam);
			[DllImport("user32.dll")] public static extern
                IntPtr SendMessage(IntPtr hWnd, W32_LVM wMsg, int wParam, LVSETINFOTIP lParam);
			[DllImport("user32.dll")] public static extern
                IntPtr SendMessage(IntPtr hWnd, HeaderControlMessages wMsg, IntPtr wParam, IntPtr lParam);
			[DllImport("user32.dll")] public static extern
                IntPtr SendMessage(IntPtr hWnd, HeaderControlMessages wMsg, IntPtr wParam, HDITEM lParam);
			[DllImport("user32.dll")] public static extern 
				IntPtr SendMessage(IntPtr hWnd, W32_LVM wMsg, IntPtr wParam, IntPtr lParam);
			[DllImport("User32.dll")] public static extern
                IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, LVCOLUMN lParam);
			[DllImport("User32.dll")]	public static extern
                IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref LVTILEINFO lParam);
			[DllImport("User32.dll")] public static extern
                IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref LVTILEVIEWINFO lParam);
			[DllImport("User32.dll")] public static extern
                IntPtr SendMessage(IntPtr hWnd, W32_LVM msg, int wParam, ref LVGROUP lParam);
			[DllImport("User32.dll")]	public static extern
                IntPtr SendMessage(IntPtr hWnd, W32_LVM msg, int wParam, ref LVBKIMAGE lParam);
			#endregion

			#region ListView helper methods
			public static int AddItemToGroup(IntPtr listHandle, int index, int groupID ){ 
				LVITEM apiItem;
                IntPtr ptrRetVal;

				try{
					if( listHandle == IntPtr.Zero){ return 0; }

					apiItem = new LVITEM();
					apiItem.mask = ListViewItemFlags.LVIF_GROUPID;
					apiItem.iItem = index;
					apiItem.iGroupId = groupID;

					ptrRetVal = SendMessage(listHandle, W32_LVM.LVM_SETITEMA, 0, ref apiItem);

					return ptrRetVal.ToInt32();
				}
				catch(Exception ex) {
					throw new System.Exception("An exception in API.AddItemToGroup occured: " + ex.Message);
				}
			}


			public static int AddListViewGroup(IntPtr listHandle, string text, int index){ 
				LVGROUP apiGroup;
                IntPtr ptrRetVal;

				try{
					if(listHandle == IntPtr.Zero){ return -1; }
					if (text == null) text = String.Empty;

					apiGroup = new LVGROUP();
					apiGroup.mask = LVGF_GROUPID | LVGF_HEADER | LVGF_STATE;
					apiGroup.pszHeader = Marshal.StringToCoTaskMemUni(text);
					apiGroup.cchHeader = text.Length;
					apiGroup.iGroupId = index;
					apiGroup.stateMask = LVGS_NORMAL;
					apiGroup.state = LVGS_NORMAL;
					apiGroup.cbSize = Marshal.SizeOf(typeof(LVGROUP));

					ptrRetVal = SendMessage(listHandle, W32_LVM.LVM_INSERTGROUP, -1, ref apiGroup);
					return ptrRetVal.ToInt32();
				}
				catch(Exception ex){
					throw new System.Exception("An exception in API.AddListViewGroup occured: " + ex.Message);
				}

			}


			public static int RemoveListViewGroup(IntPtr listHandle, int index){ 
				
				try {
					if(listHandle == IntPtr.Zero){ return -1; }

					IntPtr param = IntPtr.Zero;
                    IntPtr ptrRetVal = SendMessage(listHandle, W32_LVM.LVM_REMOVEGROUP, index, ref param);

					return ptrRetVal.ToInt32();
				}
				catch(Exception ex) {
					throw new System.Exception("An exception in API.RemoveListViewGroup occured: " + ex.Message);
				}
			}


			public static void ClearListViewGroup(IntPtr listHandle){

				try{
					if(listHandle == IntPtr.Zero){ return; }

					IntPtr param = IntPtr.Zero;
					SendMessage(listHandle, W32_LVM.LVM_REMOVEALLGROUPS, 0, ref param);
				}catch(Exception ex) {
					throw new System.Exception("An exception in API.ClearListViewGroup occured: " + ex.Message);
				}
			}


			public static void RedrawItems(ListView lst, bool update){
				
				try{
					if(lst != null){ return; }

					IntPtr param = new IntPtr(lst.Items.Count - 1);
					SendMessage(lst.Handle, W32_LVM.LVM_REDRAWITEMS, 0, ref param);

					if (update) { UpdateItems(lst); }
	
					//lst.Refresh();
				}
				catch(Exception ex){
					throw new System.Exception("An exception in API.RedrawItems occured: " + ex.Message);
				}
			}


			public static void UpdateItems(ListView lst){
				
				try{
					if( lst != null){ return; }

					for(int i = 0; i < lst.Items.Count - 1; i++){
						IntPtr param = IntPtr.Zero;
						SendMessage(lst.Handle, W32_LVM.LVM_UPDATE, i, ref param);
					}
				}
				catch(Exception ex){
					throw new System.Exception("An exception in API.UpdateItems occured: " + ex.Message);
				}
			}


			public static void SetListViewImage(IntPtr listHandle, string imagePath, ImagePosition position){

				if(listHandle == IntPtr.Zero){ return; }

				int x = 0, y = 0;
				GetImageLocation(position, out x, out y);

				try{
					LVBKIMAGE apiItem = new LVBKIMAGE();
					apiItem.pszImage = Marshal.StringToCoTaskMemUni(imagePath);
					apiItem.cchImageMax = imagePath.Length;
					apiItem.ulFlags = LVBKIF_SOURCE_URL | LVBKIF_STYLE_NORMAL;
					apiItem.xOffsetPercent = x;
					apiItem.yOffsetPercent = y;
			
					// Set the background color of the ListView to 0XFFFFFFFF (-1) so it will be transparent
					IntPtr clear = new IntPtr(-1);
					SendMessage(listHandle, W32_LVM.LVM_SETTEXTBKCOLOR, 0, ref clear);

					SendMessage(listHandle, W32_LVM.LVM_SETBKIMAGEW, 0, ref apiItem);
				}
				catch(Exception ex){
					throw new System.Exception("An exception in API.SetListViewImage occured: " + ex.Message);
				}

			}

			public static void SetListViewImage(IntPtr listHandle, string imagePath, int xTileOffsetPercent, int yTileOffsetPercent){
				if(listHandle == IntPtr.Zero){ return; }

				try{
					LVBKIMAGE apiItem = new LVBKIMAGE();
					apiItem.pszImage = Marshal.StringToCoTaskMemUni(imagePath);
					apiItem.cchImageMax = imagePath.Length;
					apiItem.ulFlags = LVBKIF_SOURCE_URL | LVBKIF_STYLE_TILE;
					apiItem.xOffsetPercent = xTileOffsetPercent;
					apiItem.yOffsetPercent = yTileOffsetPercent;
			
					// Set the background color of the ListView to 0XFFFFFFFF (-1) so it will be transparent
					IntPtr clear = new IntPtr(-1);
					SendMessage(listHandle, W32_LVM.LVM_SETTEXTBKCOLOR, 0, ref clear);

					SendMessage(listHandle, W32_LVM.LVM_SETBKIMAGEW, 0, ref apiItem);
				}
				catch(Exception ex){
					throw new System.Exception("An exception in API.SetListViewImage occured: " + ex.Message);
				}

			}


			private static void GetImageLocation(ImagePosition Position, out int XOffset, out int YOffset){
				switch(Position) {
					case ImagePosition.TopCenter:
						XOffset = 50;
						YOffset = 0;
						break;
					case ImagePosition.TopRight:
						XOffset = 100;
						YOffset = 0;
						break;
					case ImagePosition.CenterLeft:
						XOffset = 0;
						YOffset = 50;
						break;
					case ImagePosition.CenterRight:
						XOffset = 100;
						YOffset = 50;
						break;
					case ImagePosition.AbsoluteMiddle:
						XOffset = YOffset = 50;
						break;
					case ImagePosition.BottomLeft:
						XOffset = 0;
						YOffset = 100;
						break;
					case ImagePosition.BottomCenter:
						XOffset = 50;
						YOffset = 100;
						break;
					case ImagePosition.BottomRight:
						XOffset = 100;
						YOffset = 100;
						break;
					default:	// ImagePosition.TopLeft:
						XOffset = YOffset = 0;
						break;
				}
			}

            public static int[] GetColumnOrderArray(IntPtr listHandle, int columnCount)
            {
				int[] lParam	= new int[columnCount];
                IntPtr result = SendMessage(listHandle, W32_LVM.LVM_GETCOLUMNORDERARRAY, columnCount, lParam);
				if (result == IntPtr.Zero) {
					// something wrong
				}
				return lParam;
			}

			public static void SetColumnOrderArray(IntPtr listHandle, int[] columnArray) {
				int columnCount	= columnArray.GetLength(0);
                IntPtr result = SendMessage(listHandle, W32_LVM.LVM_SETCOLUMNORDERARRAY, columnCount, columnArray);
				if (result == IntPtr.Zero) {
					// something wrong
				}
			}
			#endregion

			private API(){	}
		}
		#endregion
	}
}
