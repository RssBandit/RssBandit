using System;
using System.Runtime.InteropServices;

namespace IEControl{

	/// <summary>
	/// Summary description for IHTMLEventObj.
	/// </summary>
	[ComVisible(true), ComImport()]
	[TypeLibType((short)4160)] //TypeLibTypeFlags.FDispatchable
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIDispatch)]
	[Guid("3050f32d-98b5-11cf-bb82-00aa00bdce0b")]
	public interface IHTMLEventObj {
		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_SRCELEMENT)]
		IHTMLElement SrcElement { [return: MarshalAs(UnmanagedType.Interface)] get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_ALTKEY)]
		bool AltKey { get;}
		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_CTRLKEY)]
		bool CtrlKey { get;}
		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_SHIFTKEY)]
		bool ShiftKey { get;}
		//VARIANT
		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_RETURNVALUE)]
		Object ReturnValue { set; get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_CANCELBUBBLE)]
		bool CancelBubble { set; [return: MarshalAs(UnmanagedType.VariantBool)] get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_FROMELEMENT)]
		IHTMLElement FromElement { [return: MarshalAs(UnmanagedType.Interface)] get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_TOELEMENT)]
		IHTMLElement ToElement { [return: MarshalAs(UnmanagedType.Interface)] get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_KEYCODE)]
		int keyCode { set; get;}
		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_BUTTON)]
		int Button { get;}
		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_TYPE)]
		string EventType { [return: MarshalAs(UnmanagedType.BStr)] get;}
		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_QUALIFIER)]
		string Qualifier { [return: MarshalAs(UnmanagedType.BStr)] get;}
		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_REASON)]
		int Reason { get;}
		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_X)]
		int X { get;}
		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_Y)]
		int Y { get;}
		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_CLIENTX)]
		int ClientX { get;}
		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_CLIENTY)]
		int ClientY { get;}
		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_OFFSETX)]
		int OffsetX { get;}
		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_OFFSETY)]
		int OffsetY { get;}
		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_SCREENX)]
		int ScreenX { get;}
		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_SCREENY)]
		int ScreenY { get;}
		[DispId(HTMLDispIDs.DISPID_IHTMLEVENTOBJ_SRCFILTER)]
		object SrcFilter { [return: MarshalAs(UnmanagedType.IDispatch)] get;}
	}
}
