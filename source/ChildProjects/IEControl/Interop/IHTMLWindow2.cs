#region Copyright
/*
 * Copyright (c) 2007 by Dare Obasanjo 
 * Based on code found at http://www.codeproject.com/cs/miscctrl/csEXWB.asp?msg=2134983 
 */ 
#endregion

using System;
using System.Runtime.InteropServices;

namespace IEControl {

	      		

	/// <summary>
	/// Summary description for IHTMLWindow2.
	/// </summary>
	[ComVisible(true), ComImport]
	[TypeLibType((short)4160)] //TypeLibTypeFlags.FDispatchable
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIDispatch)]
	[Guid("332c4427-26cb-11d0-b483-00c04fd90119")]
	public interface IHTMLWindow2 {
		[DispId(HTMLDispIDs.DISPID_IHTMLFRAMESCOLLECTION2_ITEM)]
		object item([In] object pvarIndex);

		[DispId(HTMLDispIDs.DISPID_IHTMLFRAMESCOLLECTION2_LENGTH)]
		int length { get;}

		//[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_FRAMES)]
		//IHTMLFramesCollection2 frames { [return: MarshalAs(UnmanagedType.Interface)] get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_DEFAULTSTATUS)]
		string defaultStatus { set;  [return: MarshalAs(UnmanagedType.BStr)] get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_STATUS)]
		string status { set;  [return: MarshalAs(UnmanagedType.BStr)] get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_SETTIMEOUT)]
		int setTimeout([In] string expression, [In] int msec, [In] ref object language);

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_CLEARTIMEOUT)]
		void clearTimeout([In] int timerID);

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_ALERT)]
		void alert([In, MarshalAs(UnmanagedType.BStr)] string message); //default value ""

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_CONFIRM)]
		bool confirm([In, MarshalAs(UnmanagedType.BStr)] string message);

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_PROMPT)]
			//default for message = ""
			//default for defstr = "undefined"
		object prompt([In, MarshalAs(UnmanagedType.BStr)] string message, 
			[In, MarshalAs(UnmanagedType.BStr)] string defstr, 
			[In] ref object textdata);

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_IMAGE)]
		object Image { get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_LOCATION)]
		object location { get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_HISTORY)]
		object history { get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_CLOSE)]
		void close();

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_OPENER)]
		object opener { set;get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_NAVIGATOR)]
		object navigator { get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_NAME)]
		string name { set; get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_PARENT)]
		IHTMLWindow2 parent { [return: MarshalAs(UnmanagedType.Interface)] get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_OPEN)]
		IHTMLWindow2 open([In] string url, [In] string name, [In] string features, [In, MarshalAs(UnmanagedType.VariantBool)] bool replace);

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_SELF)]
		IHTMLWindow2 self { [return: MarshalAs(UnmanagedType.Interface)] get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_TOP)]
		IHTMLWindow2 top { [return: MarshalAs(UnmanagedType.Interface)] get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_WINDOW)]
		IHTMLWindow2 window { [return: MarshalAs(UnmanagedType.Interface)] get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_NAVIGATE)]
		void navigate([In, MarshalAs(UnmanagedType.BStr)] string url);

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_ONFOCUS)]
		object onfocus { set;get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_ONBLUR)]
		object onblur { set;get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_ONLOAD)]
		object onload { set;get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_ONBEFOREUNLOAD)]
		object onbeforeunload { set;get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_ONUNLOAD)]
		object onunload { set;get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_ONHELP)]
		object onhelp { set; get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_ONERROR)]
		object onerror { set; get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_ONRESIZE)]
		object onresize { set; get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_ONSCROLL)]
		object onscroll { set; get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_DOCUMENT)]
		IHTMLDocument2 document { [return: MarshalAs(UnmanagedType.Interface)] get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_EVENT)]
		IHTMLEventObj eventobj { [return: MarshalAs(UnmanagedType.Interface)] get;} //event

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2__NEWENUM)]
		object _newEnum { [return: MarshalAs(UnmanagedType.IUnknown)] get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_SHOWMODALDIALOG)]
		object showModalDialog([In, MarshalAs(UnmanagedType.BStr)] string dialog,
			[In] ref object varArgIn, [In] ref object varOptions);

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_SHOWHELP)]
		void showHelp([In, MarshalAs(UnmanagedType.BStr)] string helpURL, 
			[In] object helpArg,
			[In, MarshalAs(UnmanagedType.BStr)] string features);

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_SCREEN)]
		object screen { get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_OPTION)]
		object Option { get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_FOCUS)]
		void focus();

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_CLOSED)]
		bool closed { get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_BLUR)]
		void blur();

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_SCROLL)]
		void scroll([In] int x, [In] int y);

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_CLIENTINFORMATION)]
		object clientInformation { get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_SETINTERVAL)]
		int setInterval([In] string expression, [In] int msec, [In] ref object language);

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_CLEARINTERVAL)]
		void clearInterval([In] int timerID);

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_OFFSCREENBUFFERING)]
		object offscreenBuffering { set; get;}

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_EXECSCRIPT)]
		object execScript([In] string code, [In] string language); //default language JScript

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_TOSTRING)]
		string toString();

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_SCROLLBY)]
		void scrollBy([In] int x, [In] int y);

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_SCROLLTO)]
		void scrollTo([In] int x, [In] int y);

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_MOVETO)]
		void moveTo([In] int x, [In] int y);

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_MOVEBY)]
		void moveBy([In] int x, [In] int y);

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_RESIZETO)]
		void resizeTo([In] int x, [In] int y);

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_RESIZEBY)]
		void resizeBy([In] int x, [In] int y);

		[DispId(HTMLDispIDs.DISPID_IHTMLWINDOW2_EXTERNAL)]
		object external { [return: MarshalAs(UnmanagedType.IDispatch)] get;}
	
	}
}
