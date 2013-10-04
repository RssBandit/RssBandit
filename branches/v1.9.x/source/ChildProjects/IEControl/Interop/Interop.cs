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
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices.ComTypes;


namespace IEControl {
	/// <summary>
	/// Implements various utility methods for COM interoperability.
	/// </summary>
	[System.Security.SuppressUnmanagedCodeSecurityAttribute()]
	public static class Interop {

		#region Enums
		/// <summary>
		/// NewWindow3 dwFlags.
		/// </summary>
		/// <remarks>
		/// See http://msdn.microsoft.com/en-us/library/bb762518(VS.85).aspx
		/// or in the latest Windows SDK shobjidl.h
		/// </remarks>
		[Flags]
		public enum NWMF
		{
			/// <summary>
			/// The page is unloading. This flag is set in response to the onbeforeunload and onunload events. Some pages load pop-up windows when you leave them, not when you enter. This flag is used to identify those situations
			/// </summary>
			NWMF_UNLOADING = 0x0001,
			/// <summary>
			/// The call to INewWindowManager::EvaluateNewWindow is the result of a user-initiated action (a mouse click or key press). Use this flag in conjunction with the NWMF_FIRST_USERINITED flag to determine whether the call is a direct or indirect result of the user-initiated action.
			/// </summary>
			NWMF_USERINITED = 0x0002,
			/// <summary>
			/// When NWMF_USERINITED is present, this flag indicates that the call to INewWindowManager::EvaluateNewWindow is the first query that results from this user-initiated action. Always use this flag in conjunction with NWMF_USERINITED.
			/// </summary>
			NWMF_FIRST = 0x0004,
			/// <summary>
			/// The override key (ALT) was pressed. The override key is used to bypass the pop-up manager—allowing all pop-up windows to display—and must be held down at the time that INewWindowManager::EvaluateNewWindow is called. 
			/// </summary>
			/// <remarks>
			/// When INewWindowManager::EvaluateNewWindow is implemented for a WebBrowser control host, the implementer can choose to ignore the override key.
			/// </remarks>
			NWMF_OVERRIDEKEY = 0x0008,
			/// <summary>
			/// The new window attempting to load is the result of a call to the showHelp method. Help is sometimes displayed in a separate window, and this flag is valuable in those cases
			/// </summary>
			NWMF_SHOWHELP = 0x0010,
			/// <summary>
			/// The new window is a dialog box that displays HTML content.
			/// </summary>
			NWMF_HTMLDIALOG = 0x0020,
			/// <summary>
			/// 
			/// </summary>
			NWMF_FROMDIALOGCHILD    = 0x40,
			/// <summary>
			/// 
			/// </summary>
			NWMF_USERREQUESTED      = 0x80,
			/// <summary>
			/// 
			/// </summary>
			NWMF_USERALLOWED        = 0x100,
			/// <summary>
			/// 
			/// </summary>
			NWMF_FORCEWINDOW = 0x10000,
			/// <summary>
			/// 
			/// </summary>
			NWMF_FORCETAB = 0x20000,
			/// <summary>
			/// 
			/// </summary>
			NWMF_SUGGESTWINDOW = 0x40000,
			/// <summary>
			/// 
			/// </summary>
			NWMF_SUGGESTTAB = 0x80000,
			/// <summary>
			/// 
			/// </summary>
			NWMF_INACTIVETAB = 0x100000
		} 

		#endregion

		#region public properties
		#endregion

		#region public methods
		/// <summary>
		/// Gets an assembly used to interop with the objects defined in the type
		/// library.
		/// </summary>
		/// <param name="typeLibraryName">
		///		Name of the type library such as SHDocVw.dll
		/// </param>
		/// <returns>
		///		Returns the assembly if found/created otherwise null.
		/// </returns>
		public static Assembly GetAssemblyForTypeLib( string typeLibraryName ) {
			object typeLib;
			NativeMethods.LoadTypeLibEx( typeLibraryName, RegKind.RegKind_None, out typeLib ); 
      
			if( typeLib == null )
				return null;
         
			TypeLibConverter converter = new TypeLibConverter();
			ConversionEventHandler eventHandler = new ConversionEventHandler();
			AssemblyBuilder asm = converter.ConvertTypeLibToAssembly( typeLib, "Interop." + typeLibraryName, 0, eventHandler, null, null, null, null );   

			return asm;
		}

		/// <summary>
		/// Gets the state of the async key.
		/// </summary>
		/// <param name="vKey">The v key.</param>
		/// <returns></returns>
		public static short GetAsyncKeyState(int vKey)
		{
			return NativeMethods.GetAsyncKeyState(vKey);
		}

        public static int SignedHIWORD(int n)
        {
            return (short)((n >> 0x10) & 0xffff);
        }

        public static int SignedHIWORD(IntPtr n)
        {
            return SignedHIWORD((int)((long)n));
        }

		#endregion

		#region interop decl.

	    public const int
	        DOCHOSTUIDBLCLICK_DEFAULT = 0x0,
	        DOCHOSTUIDBLCLICK_SHOWCODE = 0x2,
	        DOCHOSTUIDBLCLICK_SHOWPROPERTIES = 0x1,

	        // see:
            // http://msdn.microsoft.com/en-us/library/aa753277(VS.85).aspx
	        DOCHOSTUIFLAG_DIALOG = 0x00000001,
	        DOCHOSTUIFLAG_DISABLE_HELP_MENU = 0x00000002,
	        DOCHOSTUIFLAG_NO3DBORDER = 0x00000004,
	        DOCHOSTUIFLAG_SCROLL_NO = 0x00000008,
	        DOCHOSTUIFLAG_DISABLE_SCRIPT_INACTIVE = 0x00000010,
	        DOCHOSTUIFLAG_OPENNEWWIN = 0x00000020,
	        DOCHOSTUIFLAG_DISABLE_OFFSCREEN = 0x00000040,
	        DOCHOSTUIFLAG_FLAT_SCROLLBAR = 0x00000080,
	        DOCHOSTUIFLAG_DIV_BLOCKDEFAULT = 0x00000100,
	        DOCHOSTUIFLAG_ACTIVATE_CLIENTHIT_ONLY = 0x00000200,
	        DOCHOSTUIFLAG_OVERRIDEBEHAVIORFACTORY = 0x00000400,
	        DOCHOSTUIFLAG_CODEPAGELINKEDFONTS = 0x00000800,
	        DOCHOSTUIFLAG_URL_ENCODING_DISABLE_UTF8 = 0x00001000,
	        DOCHOSTUIFLAG_URL_ENCODING_ENABLE_UTF8 = 0x00002000,
	        DOCHOSTUIFLAG_ENABLE_FORMS_AUTOCOMPLETE = 0x00004000,
	        DOCHOSTUIFLAG_ENABLE_INPLACE_NAVIGATION = 0x00010000,
	        DOCHOSTUIFLAG_IME_ENABLE_RECONVERSION = 0x00020000,
	        DOCHOSTUIFLAG_THEME = 0x00040000,
	        DOCHOSTUIFLAG_NOTHEME = 0x00080000,
	        DOCHOSTUIFLAG_NOPICS = 0x00100000,
	        DOCHOSTUIFLAG_NO3DOUTERBORDER = 0x00200000,
            DOCHOSTUIFLAG_DISABLE_EDIT_NS_FIXUP = 0x00400000,
            DOCHOSTUIFLAG_LOCAL_MACHINE_ACCESS_CHECK = 0x00800000,
            DOCHOSTUIFLAG_DISABLE_UNTRUSTEDPROTOCOL =  0x01000000,
            DOCHOSTUIFLAG_HOST_NAVIGATES = 0x02000000,
            DOCHOSTUIFLAG_ENABLE_REDIRECT_NOTIFICATION = 0x04000000,
            DOCHOSTUIFLAG_USE_WINDOWLESS_SELECTCONTROL = 0x08000000,
            DOCHOSTUIFLAG_USE_WINDOWED_SELECTCONTROL = 0x10000000,
            DOCHOSTUIFLAG_ENABLE_ACTIVEX_INACTIVATE_MODE = 0x20000000,

	        // IE8 use this to opt-in for DPI aware behavior.
            // See http://msdn.microsoft.com/en-us/library/cc849094(VS.85).aspx
            DOCHOSTUIFLAG_DPI_AWARE = 0x40000000,  

			//DISPID_AMBIENT_DLCONTROL constants
			DLCTL_DLIMAGES  =  0x00000010,
			DLCTL_VIDEOS =  0x00000020,
			DLCTL_BGSOUNDS    = 0x00000040,
			DLCTL_NO_SCRIPTS   = 0x00000080,
			DLCTL_NO_JAVA     = 0x00000100,
			DLCTL_NO_RUNACTIVEXCTLS   = 0x00000200,
			DLCTL_NO_DLACTIVEXCTLS   = 0x00000400,
			DLCTL_DOWNLOADONLY  = 0x00000800,
			DLCTL_NO_FRAMEDOWNLOAD  = 0x00001000,
			DLCTL_RESYNCHRONIZE   = 0x00002000,
			DLCTL_PRAGMA_NO_CACHE    = 0x00004000,
			DLCTL_NO_BEHAVIORS  = 0x00008000,
			DLCTL_NO_METACHARSET   = 0x00010000,
			DLCTL_URL_ENCODING_DISABLE_UTF8  = 0x00020000,
			DLCTL_URL_ENCODING_ENABLE_UTF8  = 0x00040000,
			DLCTL_NOFRAMES     = 0x00080000,
			DLCTL_FORCEOFFLINE      = 0x10000000,
			DLCTL_NO_CLIENTPULL    = 0x20000000,
			DLCTL_SILENT  = 0x40000000,
			DLCTL_OFFLINEIFNOTCONNECTED     = unchecked((int) 0x80000000),
			DLCTL_OFFLINE    = unchecked((int) 0x80000000),

			
			E_NOTIMPL = unchecked((int)0x80004001),

			S_FALSE = 1,
			S_OK = 0,
			
			OLEIVERB_UIACTIVATE = -4,

			/*
			 * From the IE 8 (beta) zoom v2 docs at http://code.msdn.microsoft.com/ie8whitepapers/Release/ProjectReleases.aspx?ReleaseId=563
			 * Zoom APIs 
			 * 
			 * OLECMDID_ZOOM – This corresponds to the View menu's Text Size command. 
			 * Only the text is scaled by using this command. 
			 * Sites that have fixed text size are not changed in anyway. 
			 * This command is used for primarily three purposes: 
			 *   querying the current zoom value, displaying the zoom dialog box, 
			 *   and setting a zoom value. 
			 *   
			 * OLECMDID_GETZOOMRANGE – This command returns two values (LOWORD, HIWORD) 
			 * that represent the minimum and maximum of the zoom value range. 
			 * This command is used when the user activates zoom UI. 
			 * 
			 * OLECMDID_OPTICAL_ZOOM – Internet Explorer 7 and later. 
			 * Sets the zoom factor of the browser. 
			 * Takes a VT_I4 parameter in the range of 10 to 1000 (percent). 
			 * 
			 * OLECMDID_OPTICAL_GETZOOMRANGE – Internet Explorer 7 and later. 
			 * Retrieves the minimum and maximum browser zoom factor limits. 
			 * Returns a VT_I4 parameter, where the LOWORD is the minimum zoom factor, 
			 * and the HIWORD is the maximum.
			 */
			OLECMDID_OPTICAL_ZOOM = 63,
            OLECMDID_OPTICAL_GETZOOMRANGE   = 64,

			WM_CLOSE = 0x0010,
			WM_KEYDOWN = 0x0100,
            WM_MOUSEWHEEL = 0x20A,

			/// <summary>Left mouse button</summary>
			/// <remarks>See also http://msdn.microsoft.com/library/en-us/winui/WinUI/WindowsUserInterface/UserInput/VirtualKeyCodes.asp?frame=true</remarks>
			VK_LBUTTON = 0x01,
			/// <summary>Right mouse button</summary>
			VK_RBUTTON = 0x02,		
			/// <summary>Control-break processing</summary>
			VK_CANCEL =0x03,
			/// <summary>Middle mouse button (three-button mouse)</summary>
			VK_MBUTTON = 0x04,		
			/// <summary>Windows 2000/XP: X1 mouse button</summary>
			VK_XBUTTON1 = 0x05,
			/// <summary>Windows 2000/XP: X2 mouse button</summary>
			VK_XBUTTON2 = 0x06,
			/// <summary>BACKSPACE key</summary>
			VK_BACK =0x08,
			/// <summary>TAB key</summary>
			VK_TAB = 0x09,
			/// <summary>CLEAR key</summary>
			VK_CLEAR = 0x0C,
			/// <summary>ENTER key</summary>
			VK_RETURN = 0x0D,
			/// <summary>SHIFT key</summary>
			VK_SHIFT = 0x10,
			/// <summary>CONTROL key</summary>
			VK_CONTROL = 0x11,
			/// <summary>ALT key</summary>
			VK_MENU = 0x12,
			/// <summary>PAUSE key</summary>
			VK_PAUSE = 0x13,
			/// <summary>CAPS LOCK key</summary>
			VK_CAPITAL = 0x14;


		[ComVisible(false)]
			public sealed class OLECMDEXECOPT {
			public const int OLECMDEXECOPT_DODEFAULT = 0;
			public const int OLECMDEXECOPT_PROMPTUSER = 1;
			public const int OLECMDEXECOPT_DONTPROMPTUSER = 2;
			public const int OLECMDEXECOPT_SHOWHELP = 3;
		}

		[ComVisible(false)]
			public sealed class OLECMDF {
			public const int OLECMDF_SUPPORTED = 1;
			public const int OLECMDF_ENABLED = 2;
			public const int OLECMDF_LATCHED = 4;
			public const int OLECMDF_NINCHED = 8;
		}
		
		[ComVisible(false)]
			public sealed class READYSTATE {
			public const int READYSTATE_UNINITIALIZED = 0;
			public const int READYSTATE_LOADING = 1;
			public const int READYSTATE_LOADED = 2;
			public const int READYSTATE_INTERACTIVE = 3;
			public const int READYSTATE_COMPLETE = 4;
		} 

		[ComVisible(false), StructLayout(LayoutKind.Sequential)]
			public sealed class tagOLECMD {
			[MarshalAs(UnmanagedType.U4)] public   int cmdID;
			[MarshalAs(UnmanagedType.U4)] public   int cmdf;
		}

		[ComVisible(false), StructLayout(LayoutKind.Sequential)]
			public sealed class tagOleMenuGroupWidths {
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=6)]
			public int[] widths = new int[6];
		}

		[ComVisible(true), StructLayout(LayoutKind.Sequential)]
			public sealed class tagSIZEL {
			[MarshalAs(UnmanagedType.I4)]
			public int cx;
			[MarshalAs(UnmanagedType.I4)]
			public int cy;
		}

		[ComVisible(false), StructLayout(LayoutKind.Sequential)]
			public class COMMSG {
			public IntPtr   hwnd;
			public int  message;
			public IntPtr   wParam;
			public IntPtr   lParam;
			public int  time;
			// pt was a by-value POINT structure
			public int  pt_x;
			public int  pt_y;
		}

		[ComVisible(true), StructLayout(LayoutKind.Sequential)]
			public class POINT {
			public int x;
			public int y;

			public POINT() {}
			public POINT(int x, int y) {
				this.x = x;
				this.y = y;
			}
		}

		[ComVisible(true), StructLayout(LayoutKind.Sequential)]
			public class DOCHOSTUIINFO {
			[MarshalAs(UnmanagedType.U4)] public int cbSize;
			[MarshalAs(UnmanagedType.I4)] public int dwFlags;
			[MarshalAs(UnmanagedType.I4)] public int dwDoubleClick;
			[MarshalAs(UnmanagedType.I4)] public int dwReserved1;
			[MarshalAs(UnmanagedType.I4)] public int dwReserved2;
		}

		[ComVisible(true), StructLayout(LayoutKind.Sequential)]
			public class COMRECT {
			public int left;
			public int top;
			public int right;
			public int bottom;

			public COMRECT() {}

			public COMRECT(int left, int top, int right, int bottom) {
				this.left = left;
				this.top = top;
				this.right = right;
				this.bottom = bottom;
			}

			public static COMRECT FromXYWH(int x, int y, int width, int height) {
				return new COMRECT(x, y, x + width, y + height);
			}
		}


		[ComVisible(false), StructLayout(LayoutKind.Sequential)]
			public sealed class tagOIFI {
			
			[MarshalAs(UnmanagedType.U4)]
			public int cb;

			[MarshalAs(UnmanagedType.I4)]
			public int fMDIApp;

			public IntPtr hwndFrame;
			public IntPtr hAccel;

			[MarshalAs(UnmanagedType.U4)]
			public int cAccelEntries;
		}

		[ComVisible(false), StructLayout(LayoutKind.Sequential)]
			public sealed class tagOLEVERB {
			[MarshalAs(UnmanagedType.I4)]
			public int lVerb;

			/// <summary>
			/// Use Marshal.PtrToStringUni()  to access the string!
			/// </summary>
			/// <remarks>
			/// See also http://www.25hoursaday.com/weblog/CommentView.aspx?guid=2f830d35-eb2b-4f02-b4d0-bc0de075c264
			/// </remarks>
			public IntPtr lpszVerbName;
			//[MarshalAs(UnmanagedType.LPWStr)]
			//public String lpszVerbName;

			[MarshalAs(UnmanagedType.U4)]
			public int fuFlags;

			[MarshalAs(UnmanagedType.U4)]
			public int grfAttribs;

		}

		[ComVisible(false), StructLayout(LayoutKind.Sequential)]
			public sealed class tagSIZE {
			[MarshalAs(UnmanagedType.I4)]
			public int cx;

			[MarshalAs(UnmanagedType.I4)]
			public int cy;

		}

		public sealed class FORMATETC {

			[MarshalAs(UnmanagedType.I4)]
			public int cfFormat;
			public IntPtr ptd;
			[MarshalAs(UnmanagedType.I4)]
			public int dwAspect;
			[MarshalAs(UnmanagedType.I4)]
			public int lindex;
			[MarshalAs(UnmanagedType.I4)]
			public int tymed;

		}

		[ComVisible(false), StructLayout(LayoutKind.Sequential)]
			public class STGMEDIUM {

			[MarshalAs(UnmanagedType.I4)]
			public   int tymed;
			public   IntPtr unionmember;
			public   IntPtr pUnkForRelease;

		}

		[ComVisible(true), ComImport(), Guid("00000103-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IEnumFORMATETC {

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int Next(
				[In, MarshalAs(UnmanagedType.U4)]
				int celt,
				[Out]
				FORMATETC rgelt,
				[In, Out, MarshalAs(UnmanagedType.LPArray)]
				int[] pceltFetched);

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int Skip(
				[In, MarshalAs(UnmanagedType.U4)]
				int celt);

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int Reset();

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int Clone(
				[Out, MarshalAs(UnmanagedType.LPArray)]
				IEnumFORMATETC[] ppenum);
		}

		[ComVisible(true), ComImport, Guid("00000104-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IEnumOLEVERB {

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int Next(
				[MarshalAs(UnmanagedType.U4)] int celt,
				[Out]	tagOLEVERB rgelt,
				[Out, MarshalAs(UnmanagedType.LPArray)]
				int[] pceltFetched);

			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int Skip([In, MarshalAs(UnmanagedType.U4)] 	int celt);

			void Reset();
			void Clone(out IEnumOLEVERB ppenum);
		}

		[ComVisible(true), ComImport, Guid("00000118-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IOleClientSite {

			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int SaveObject();

			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int GetMoniker(
				[In, MarshalAs(UnmanagedType.U4)]          int dwAssign,
				[In, MarshalAs(UnmanagedType.U4)]          int dwWhichMoniker,
				[Out, MarshalAs(UnmanagedType.Interface)] out object ppmk);

			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int GetContainer([Out] out IOleContainer ppContainer);

			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int ShowObject();

			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int OnShowWindow([In, MarshalAs(UnmanagedType.I4)] int fShow);

			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int RequestNewObjectLayout();
		}

		#region IOleControl Interface
		
		
		//MIDL_INTERFACE("B196B288-BAB4-101A-B69C-00AA00341D07")
		//IOleControl : public IUnknown
		//{
		//public:
		//    virtual HRESULT STDMETHODCALLTYPE GetControlInfo( 
		//        /* [out] */ CONTROLINFO *pCI) = 0;
		//    virtual HRESULT STDMETHODCALLTYPE OnMnemonic( 
		//        /* [in] */ MSG *pMsg) = 0;
		//    virtual HRESULT STDMETHODCALLTYPE OnAmbientPropertyChange( 
		//        /* [in] */ DISPID dispID) = 0;
		//    virtual HRESULT STDMETHODCALLTYPE FreezeEvents( 
		//        /* [in] */ BOOL bFreeze) = 0;
		//}
		[ComImport, ComVisible(true)]
		[Guid("B196B288-BAB4-101A-B69C-00AA00341D07")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IOleControl
		{
			//struct tagCONTROLINFO
			//{
			//    ULONG cb; uint32
			//    HACCEL hAccel; intptr
			//    USHORT cAccel; ushort
			//    DWORD dwFlags; uint32
			//}
			void GetControlInfo(out object pCI);
			void OnMnemonic([In, MarshalAs(UnmanagedType.Struct)]ref tagMSG pMsg);
			void OnAmbientPropertyChange([In] int dispID);
			void FreezeEvents([In, MarshalAs(UnmanagedType.Bool)] bool bFreeze);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes"), ComVisible(true), StructLayout(LayoutKind.Sequential)]
		public struct tagMSG
		{
			public IntPtr hwnd;
			[MarshalAs(UnmanagedType.I4)]
			public int message;
			public IntPtr wParam;
			public IntPtr lParam;
			[MarshalAs(UnmanagedType.I4)]
			public int time;
			// pt was a by-value POINT structure
			[MarshalAs(UnmanagedType.I4)]
			public int pt_x;
			[MarshalAs(UnmanagedType.I4)]
			public int pt_y;
			//public tagPOINT pt;
		}
		#endregion

		[ComVisible(true), Guid("0000011B-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IOleContainer {

			void ParseDisplayName(
				[In, MarshalAs(UnmanagedType.Interface)] object pbc,
				[In, MarshalAs(UnmanagedType.BStr)]      string pszDisplayName,
				[Out, MarshalAs(UnmanagedType.LPArray)] int[] pchEaten,
				[Out, MarshalAs(UnmanagedType.LPArray)] object[] ppmkOut);

			void EnumObjects(
				[In, MarshalAs(UnmanagedType.U4)]        int grfFlags,
				[Out, MarshalAs(UnmanagedType.LPArray)] object[] ppenum);

			void LockContainer(
				[In, MarshalAs(UnmanagedType.I4)] int fLock);
		}

		[ComVisible(true), ComImport(), Guid("00000112-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IOleObject {

			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int SetClientSite(
				[In, MarshalAs(UnmanagedType.Interface)]
				IOleClientSite pClientSite);

			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int GetClientSite(out IOleClientSite site);

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int SetHostNames(
				[In, MarshalAs(UnmanagedType.LPWStr)]
				string szContainerApp,
				[In, MarshalAs(UnmanagedType.LPWStr)]
				string szContainerObj);

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int Close(
				[In, MarshalAs(UnmanagedType.I4)]
				int dwSaveOption);

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int SetMoniker(
				[In, MarshalAs(UnmanagedType.U4)]
				int dwWhichMoniker,
				[In, MarshalAs(UnmanagedType.Interface)]
				object pmk);

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int GetMoniker(
				[In, MarshalAs(UnmanagedType.U4)]
				int dwAssign,
				[In, MarshalAs(UnmanagedType.U4)]
				int dwWhichMoniker,
				out object moniker);

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int InitFromData(
				[In, MarshalAs(UnmanagedType.Interface)]
				IOleDataObject pDataObject,
				[In, MarshalAs(UnmanagedType.I4)]
				int fCreation,
				[In, MarshalAs(UnmanagedType.U4)]
				int dwReserved);

			int GetClipboardData(
				[In, MarshalAs(UnmanagedType.U4)]
				int dwReserved,
				out IOleDataObject data);

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int DoVerb(
				[In, MarshalAs(UnmanagedType.I4)]
				int iVerb,
				[In]
				IntPtr lpmsg,
				[In, MarshalAs(UnmanagedType.Interface)]
				IOleClientSite pActiveSite,
				[In, MarshalAs(UnmanagedType.I4)]
				int lindex,
				[In]
				IntPtr hwndParent,
				[In]
				COMRECT lprcPosRect);

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int EnumVerbs(out IEnumOLEVERB e);

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int OleUpdate();

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int IsUpToDate();

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int GetUserClassID(
				[In, Out]
				ref Guid pClsid);

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int GetUserType(
				[In, MarshalAs(UnmanagedType.U4)]
				int dwFormOfType,
				[Out, MarshalAs(UnmanagedType.LPWStr)]
				out string userType);

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int SetExtent(
				[In, MarshalAs(UnmanagedType.U4)]
				int dwDrawAspect,
				[In]
				tagSIZEL pSizel);

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int GetExtent(
				[In, MarshalAs(UnmanagedType.U4)]
				int dwDrawAspect,
				[Out]
				tagSIZEL pSizel);

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int Advise(
				[In, MarshalAs(UnmanagedType.Interface)]
				IAdviseSink pAdvSink,
				out int cookie);

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int Unadvise(
				[In, MarshalAs(UnmanagedType.U4)]
				int dwConnection);

			//
			//			[return: MarshalAs(UnmanagedType.I4)]
			//			[PreserveSig]
			//			int EnumAdvise(out IEnumSTATDATA e);
			//
			//			[return: MarshalAs(UnmanagedType.I4)]
			//			[PreserveSig]
			//			int GetMiscStatus(
			//				[In, MarshalAs(UnmanagedType.U4)]
			//				int dwAspect,
			//				out int misc);
			//
			//			[return: MarshalAs(UnmanagedType.I4)]
			//			[PreserveSig]
			//			int SetColorScheme(
			//				[In]
			//				tagLOGPALETTE pLogpal);

		}

		[ComImport]
			[Guid("00000114-0000-0000-C000-000000000046")]
			[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
			public interface IOleWindow {
			[PreserveSig] int GetWindow (
				out IntPtr phwnd) ;
			[PreserveSig] int ContextSensitiveHelp (
				[In, MarshalAs (UnmanagedType.Bool)] bool fEnterMode) ;
		}

		[ComVisible(true), ComImport(), Guid("00000117-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IOleInPlaceActiveObject {

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int GetWindow(out IntPtr hwnd);

			void ContextSensitiveHelp([In, MarshalAs(UnmanagedType.I4)] int fEnterMode);

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int TranslateAccelerator([In, MarshalAs(UnmanagedType.LPStruct)] COMMSG lpmsg);

			void OnFrameWindowActivate([In, MarshalAs(UnmanagedType.I4)] int fActivate);
			void OnDocWindowActivate([In, MarshalAs(UnmanagedType.I4)] int fActivate);
			void ResizeBorder([In] COMRECT prcBorder, [In] IOleInPlaceUIWindow pUIWindow,
				[In, MarshalAs(UnmanagedType.I4)] int fFrameWindow);
			void EnableModeless([In, MarshalAs(UnmanagedType.I4)] int fEnable);
		}


		[ComVisible(true), ComImport(), Guid("00000119-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IOleInPlaceSite {

			IntPtr GetWindow();
			void ContextSensitiveHelp(	[In, MarshalAs(UnmanagedType.I4)] int fEnterMode);

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int CanInPlaceActivate();

			void OnInPlaceActivate();
			void OnUIActivate();

			void GetWindowContext(
				[Out]
				out IOleInPlaceFrame ppFrame,
				[Out]
				out IOleInPlaceUIWindow ppDoc,
				[Out]
				COMRECT lprcPosRect,
				[Out]
				COMRECT lprcClipRect,
				[In, Out]
				tagOIFI lpFrameInfo);

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int Scroll(
				[In, MarshalAs(UnmanagedType.U4)]
				tagSIZE scrollExtent);

			void OnUIDeactivate(
				[In, MarshalAs(UnmanagedType.I4)]
				int fUndoable);


			void OnInPlaceDeactivate();
			void DiscardUndoState();
			void DeactivateAndUndo();

			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int OnPosRectChange(
				[In]
				COMRECT lprcPosRect);
		}

		[ComVisible(true), Guid("00000115-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IOleInPlaceUIWindow {

			//UNDONE (Field in interface) public static readonly    Guid iid;

			IntPtr GetWindow();
			void ContextSensitiveHelp(	[In, MarshalAs(UnmanagedType.I4)] int fEnterMode);
			void GetBorder([Out]	COMRECT lprectBorder);
			void RequestBorderSpace(	[In] COMRECT pborderwidths);
			void SetBorderSpace([In]	COMRECT pborderwidths);

			void SetActiveObject([In, MarshalAs(UnmanagedType.Interface)]	IOleInPlaceActiveObject pActiveObject,
				[In, MarshalAs(UnmanagedType.LPWStr)]	string pszObjName);

		}

		[ComVisible(true), ComImport(), Guid("B722BCCB-4E68-101B-A2BC-00AA00404770"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IOleCommandTarget {

			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int QueryStatus(ref Guid pguidCmdGroup,	int cCmds,	[In, Out] tagOLECMD prgCmds,
				[In, Out] int pCmdText);

			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int Exec(	ref Guid pguidCmdGroup, int nCmdID, int nCmdexecopt,
				// we need to have this an array because callers need to be able to specify NULL or VT_NULL
				[In, MarshalAs(UnmanagedType.LPArray)] object[] pvaIn,
				[Out, MarshalAs(UnmanagedType.LPArray)] object[] pvaOut);
		}

		[ComVisible(true), ComImport(), Guid("00000116-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IOleInPlaceFrame {

			IntPtr GetWindow();
			void ContextSensitiveHelp(	[In, MarshalAs(UnmanagedType.I4)] int fEnterMode);
			void GetBorder([Out] COMRECT lprectBorder);
			void RequestBorderSpace([In] COMRECT pborderwidths);
			void SetBorderSpace([In] COMRECT pborderwidths);
			void SetActiveObject([In, MarshalAs(UnmanagedType.Interface)] IOleInPlaceActiveObject pActiveObject,
				[In, MarshalAs(UnmanagedType.LPWStr)] string pszObjName);
			void InsertMenus(	[In] IntPtr hmenuShared,
				[In, Out] 	tagOleMenuGroupWidths lpMenuWidths);
			void SetMenu([In] IntPtr hmenuShared,
				[In] IntPtr holemenu,
				[In] IntPtr hwndActiveObject);
			void RemoveMenus([In] IntPtr hmenuShared);
			void SetStatusText([In, MarshalAs(UnmanagedType.BStr)] string pszStatusText);
			void EnableModeless([In, MarshalAs(UnmanagedType.I4)] int fEnable);
			
			[return: MarshalAs(UnmanagedType.I4)]
			[PreserveSig]
			int TranslateAccelerator([In, MarshalAs(UnmanagedType.LPStruct)] COMMSG lpmsg,
				[In, MarshalAs(UnmanagedType.U2)] short wID);
		}

		[ComVisible(true), ComImport(), Guid("0000010E-0000-0000-C000-000000000046"),
			InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IOleDataObject {

			int OleGetData(
				FORMATETC pFormatetc,
				[Out]
				STGMEDIUM pMedium);


			int OleGetDataHere(
				FORMATETC pFormatetc,
				[In, Out]
				STGMEDIUM pMedium);


			int OleQueryGetData(
				FORMATETC pFormatetc);


			int OleGetCanonicalFormatEtc(
				FORMATETC pformatectIn,
				[Out]
				FORMATETC pformatetcOut);


			int OleSetData(
				FORMATETC pFormatectIn,
				STGMEDIUM pmedium,

				int fRelease);

			[return: MarshalAs(UnmanagedType.Interface)]
			IEnumFORMATETC OleEnumFormatEtc(
				[In, MarshalAs(UnmanagedType.U4)]
				int dwDirection);

			int OleDAdvise(
				FORMATETC pFormatetc,
				[In, MarshalAs(UnmanagedType.U4)]
				int advf,
				[In, MarshalAs(UnmanagedType.Interface)]
				object pAdvSink,
				[Out, MarshalAs(UnmanagedType.LPArray)]
				int[] pdwConnection);

			int OleDUnadvise(
				[In, MarshalAs(UnmanagedType.U4)]
				int dwConnection);

			int OleEnumDAdvise(
				[Out, MarshalAs(UnmanagedType.LPArray)]
				object[] ppenumAdvise);
		}

		[ComVisible(true), ComImport(), Guid("00000122-0000-0000-C000-000000000046"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IOleDropTarget {

			[PreserveSig] int OleDragEnter(
				//[In, MarshalAs(UnmanagedType.Interface)]
				IntPtr pDataObj,
				[In, MarshalAs(UnmanagedType.U4)] int grfKeyState,
				[In, MarshalAs(UnmanagedType.U8)] long pt,
				[In, Out] ref int pdwEffect);

			[PreserveSig] int OleDragOver(
				[In, MarshalAs(UnmanagedType.U4)] int grfKeyState,
				[In, MarshalAs(UnmanagedType.U8)] long pt,
				[In, Out] ref int pdwEffect);

			[PreserveSig] int OleDragLeave();
			[PreserveSig] int OleDrop(
				//[In, MarshalAs(UnmanagedType.Interface)]
				IntPtr pDataObj,
				[In, MarshalAs(UnmanagedType.U4)] int grfKeyState,
				[In, MarshalAs(UnmanagedType.U8)] long pt,
				[In, Out] 	ref int pdwEffect);
		}

		/// <summary>
		/// http://msdn.microsoft.com/workshop/browser/hosting/reference/ifaces/idochostuihandler/idochostuihandler.asp
		/// </summary>
		[ComVisible(true), ComImport, Guid("BD3F23C0-D43E-11CF-893B-00AA00BDCE1A"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IDocHostUIHandler {

			[PreserveSig] int ShowContextMenu([In, MarshalAs(UnmanagedType.U4)] int dwID,
				[In] Interop.POINT pt,
				[In, MarshalAs(UnmanagedType.Interface)] object pcmdtReserved,
				[In, MarshalAs(UnmanagedType.Interface)] object pdispReserved);

			[PreserveSig] int GetHostInfo([In, Out] DOCHOSTUIINFO info);

			[PreserveSig] int ShowUI(
				[In, MarshalAs(UnmanagedType.I4)]        int dwID,
				[In, MarshalAs(UnmanagedType.Interface)] Interop.IOleInPlaceActiveObject activeObject,
				[In, MarshalAs(UnmanagedType.Interface)] Interop.IOleCommandTarget commandTarget,
				[In, MarshalAs(UnmanagedType.Interface)] Interop.IOleInPlaceFrame frame,
				[In, MarshalAs(UnmanagedType.Interface)] Interop.IOleInPlaceUIWindow doc);

			[PreserveSig] int HideUI();
			[PreserveSig] int UpdateUI();
			[PreserveSig] int EnableModeless([In, MarshalAs(UnmanagedType.Bool)] bool fEnable);
			[PreserveSig] int OnDocWindowActivate([In, MarshalAs(UnmanagedType.Bool)] bool fActivate);
			[PreserveSig] int OnFrameWindowActivate([In, MarshalAs(UnmanagedType.Bool)] bool fActivate);

			[PreserveSig] int ResizeBorder([In] Interop.COMRECT rect, [In] Interop.IOleInPlaceUIWindow doc,
				[In] bool fFrameWindow);

			[PreserveSig] int TranslateAccelerator(
				[In]                               Interop.COMMSG msg,
				[In]                               ref Guid group,
				[In, MarshalAs(UnmanagedType.I4)] int nCmdID);

			[PreserveSig] int GetOptionKeyPath(
				[Out, MarshalAs(UnmanagedType.LPArray)] String[] pbstrKey,
				[In, MarshalAs(UnmanagedType.U4)]        int dw);

			[PreserveSig] int GetDropTarget(
				[In, MarshalAs(UnmanagedType.Interface)]   Interop.IOleDropTarget pDropTarget,
				[Out, MarshalAs(UnmanagedType.Interface)] out Interop.IOleDropTarget ppDropTarget);

			[PreserveSig] int GetExternal([Out, MarshalAs(UnmanagedType.Interface)] out object ppDispatch);

			[PreserveSig]
			int TranslateUrl(
				[In, MarshalAs(UnmanagedType.U4)]       int dwTranslate,
				[In, MarshalAs(UnmanagedType.LPWStr)]   string strURLIn,
				[Out] out IntPtr pstrURLOut);

			[PreserveSig] int FilterDataObject(
				[In, MarshalAs(UnmanagedType.Interface)]   object pDO,
				[Out, MarshalAs(UnmanagedType.Interface)] out object ppDORet);
		}

		[ComVisible(true), ComImport(), Guid("3050f3f0-98b5-11cf-bb82-00aa00bdce0b"),
			InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface ICustomDoc {
			[PreserveSig] int SetUIHandler([In] Interop.IDocHostUIHandler pUIHandler);		
		}

		[ComVisible(true), ComImport(), Guid("C4D244B0-D43E-11CF-893B-00AA00BDCE1A"),
			InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IDocHostShowUI {
			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int ShowMessage(
				[In] IntPtr hwnd,[In][MarshalAs(UnmanagedType.LPWStr)] String lpStrText,
				[In][MarshalAs(UnmanagedType.LPWStr)] String lpstrCaption,
				[In][MarshalAs(UnmanagedType.U4)] Int32 dwType,[In][MarshalAs(UnmanagedType.LPWStr)] String
				lpStrHelpFile,[In][MarshalAs(UnmanagedType.U4)] Int32 dwHelpContext,
				[Out] IntPtr lpresult);
	   
			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int ShowHelp(
				[In] IntPtr hwnd,
				[In][MarshalAs(UnmanagedType.LPWStr)] String lpHelpFile,
				[In][MarshalAs(UnmanagedType.U4)] Int32 uCommand,
				[In][MarshalAs(UnmanagedType.U4)] Int32 dwData,
				[In] POINT ptMouse,
				[Out][MarshalAs(UnmanagedType.IDispatch)] Object pDispatchObjectHit
				);

		}

		[ComVisible(true), ComImport(),	Guid("B722BCC7-4E68-101B-A2BC-00AA00404770"),
			InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IOleDocumentSite {
			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int ActivateMe([In, MarshalAs(UnmanagedType.Interface)] IOleDocumentView
				pViewToActivate);
		}


		[ComVisible(true), Guid("B722BCC6-4E68-101B-A2BC-00AA00404770"),
			InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IOleDocumentView {
			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int SetInPlaceSite([In, MarshalAs(UnmanagedType.Interface)]
				IOleInPlaceSite pIPSite);
			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int	GetInPlaceSite([Out,MarshalAs(UnmanagedType.Interface)] IOleInPlaceSite ppIPSite);
			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int GetDocument([Out] Object ppunk);
			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int SetRect([In] COMRECT prcView);
			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int GetRect([Out] COMRECT prcView);
			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int SetRectComplex([In] COMRECT prcView, [In] COMRECT prcHScroll, [In] COMRECT
				prcVScroll, [In] COMRECT prcSizeBox);
			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int Show([In,MarshalAs(UnmanagedType.I4)] int fShow);
			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int UIActivate([In, MarshalAs(UnmanagedType.I4)] int fUIActivate);
			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int Open();
			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int CloseView([In, MarshalAs(UnmanagedType.I4)]int dwReserved);
			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int SaveViewState([In, MarshalAs(UnmanagedType.Interface)] IStream pstm);
			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int ApplyViewState([In, MarshalAs(UnmanagedType.Interface)] IStream
				pstm);
			[return: MarshalAs(UnmanagedType.I4)][PreserveSig]
			int Clone([In, MarshalAs(UnmanagedType.Interface)] IOleInPlaceSite
				pIPSiteNew, [Out, MarshalAs(UnmanagedType.LPArray)] IOleDocumentView[]
				ppViewNew);
		}


		[ComVisible(true), Guid("0000010f-0000-0000-C000-000000000046"),
			InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
			public interface IAdviseSink {
			void OnDataChange(
				[In] object pFormatetc,
				[In] object pStgmed
				);
			void OnViewChange(
				[In,MarshalAs(UnmanagedType.U4)] int dwAspect,
				[In,MarshalAs(UnmanagedType.I4)] int lindex
				);
			void OnRename(
				[In, MarshalAs(UnmanagedType.Interface)] IMoniker pmk 
				);
			void OnSave();
			void OnClose();
		}

		[ComImport, Guid("6d5140c1-7436-11ce-8034-00aa006009fa"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IServiceProvider {
			void QueryService( ref Guid guidService, ref Guid riid,
				[MarshalAs(UnmanagedType.Interface)] out object ppvObject);
		}

		private enum RegKind {
			RegKind_Default = 0,
			RegKind_Register = 1,
			RegKind_None = 2
		}
		
		[ComImport, Guid("D30C1661-CDAF-11D0-8A3E-00C04FC9E26E"), 
		InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
		public interface IWebBrowser2Application
		{
			object Application { [return: MarshalAs(UnmanagedType.IDispatch)][DispId(200)] get; }
		}

		#endregion

		#region private classes/interfaces
		
		private static class NativeMethods
		{
			[DllImport("User32.dll")]
			public static extern short GetAsyncKeyState(int vKey);
			
			[DllImport("oleaut32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
			public static extern void LoadTypeLibEx(String strTypeLibName, RegKind regKind,
				[MarshalAs(UnmanagedType.Interface)] out Object typeLib);

			[DllImport("ole32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
			public static extern int OleRun(
				[In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown
				);
			[DllImport("ole32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
			public static extern int OleLockRunning(
				[In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown,
				[In, MarshalAs(UnmanagedType.Bool)] bool flock,
				[In, MarshalAs(UnmanagedType.Bool)] bool fLastUnlockCloses
				);
		}

		#region ConversionEventHandler
		/// <summary>
		/// ConversionEventHandler
		/// </summary>
		internal class ConversionEventHandler : ITypeLibImporterNotifySink {
			public void ReportEvent( ImporterEventKind eventKind, int eventCode, string eventMsg ) {
				// handle warning event here...
			}
   
			public Assembly ResolveRef( object typeLib ) {
				// resolve reference here and return a correct assembly...
				return null; 
			}   
		} // End class ConversionEventHandler
		#endregion

		#region HTMLDocument
		[ComVisible(true), ComImport(), Guid("25336920-03F9-11cf-8FD0-00AA00686F13")]
			public class HTMLDocument {
			// palceholder
		}
		#endregion


		#endregion
	}
}
