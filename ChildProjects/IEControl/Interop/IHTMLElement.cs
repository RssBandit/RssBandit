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

namespace IEControl {
	
	/// <summary>
	/// mshtml.IHTMLElement interface
	/// </summary>
	[
	ComVisible(true), 
	Guid(@"3050F1FF-98B5-11CF-BB82-00AA00BDCE0B"), 
	InterfaceType(ComInterfaceType.InterfaceIsDual)
	]
	public interface IHTMLElement {
		/// <summary>
		/// </summary>
		/// <param name="strAttributeName"></param>
		/// <param name="AttributeValue"></param>
		/// <param name="lFlags"></param>
		void SetAttribute(string strAttributeName, object AttributeValue, int lFlags); 
        
		/// <summary>
		/// </summary>
		/// <param name="strAttributeName"></param>
		/// <param name="lFlags"></param>
		/// <param name="pvars"></param>
		void GetAttribute(string strAttributeName, int lFlags, object[] pvars); 
        
		/// <summary>
		/// </summary>
		/// <param name="strAttributeName"></param>
		/// <param name="lFlags"></param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)] 
		bool RemoveAttribute(string strAttributeName, int lFlags); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetClassName(string p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
		string GetClassName(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetId(string p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
		string GetId(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
		string GetTagName(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
		IHTMLElement GetParentElement(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
		object GetStyle(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOnhelp(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOnhelp(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOnclick(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOnclick(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOndblclick(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOndblclick(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOnkeydown(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOnkeydown(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOnkeyup(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOnkeyup(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOnkeypress(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOnkeypress(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOnmouseout(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOnmouseout(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOnmouseover(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOnmouseover(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOnmousemove(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOnmousemove(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOnmousedown(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOnmousedown(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOnmouseup(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOnmouseup(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
		object GetDocument(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetTitle(string p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
		string GetTitle(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetLanguage(string p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
		string GetLanguage(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOnselectstart(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOnselectstart(); 
        
		/// <summary>
		/// </summary>
		/// <param name="varargStart"></param>
		void ScrollIntoView(object varargStart); 
        
		/// <summary>
		/// </summary>
		/// <param name="pChild"></param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)] 
		bool Contains(IHTMLElement pChild); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.I4)] 
		int GetSourceIndex(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetRecordNumber(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetLang(string p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
		string GetLang(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.I4)] 
		int GetOffsetLeft(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.I4)] 
		int GetOffsetTop(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.I4)] 
		int GetOffsetWidth(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.I4)] 
		int GetOffsetHeight(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
		IHTMLElement GetOffsetParent(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetInnerHTML(string p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
		string GetInnerHTML(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetInnerText(string p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
		string GetInnerText(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOuterHTML(string p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
		string GetOuterHTML(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOuterText(string p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
		string GetOuterText(); 
        
		/// <summary>
		/// </summary>
		/// <param name="where"></param>
		/// <param name="html"></param>
		void InsertAdjacentHTML(string where, string html); 
        
		/// <summary>
		/// </summary>
		/// <param name="where"></param>
		/// <param name="text"></param>
		void InsertAdjacentText(string where, string text); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
		IHTMLElement GetParentTextEdit(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)] 
		bool GetIsTextEdit(); 
        
		/// <summary>
		/// </summary>
		void Click(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
		object GetFilters(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOndragstart(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOndragstart(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
		string toString(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOnbeforeupdate(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOnbeforeupdate(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOnafterupdate(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOnafterupdate(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOnerrorupdate(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOnerrorupdate(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOnrowexit(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOnrowexit(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOnrowenter(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOnrowenter(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOndatasetchanged(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOndatasetchanged(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOndataavailable(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOndataavailable(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOndatasetcomplete(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOndatasetcomplete(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetOnfilterchange(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object GetOnfilterchange(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
		object GetChildren(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
		object GetAll(); 
	} 

	/// <summary>
	/// mshtml.IHTMLElement2 interface
	/// </summary>
	[
		ComVisible(true), 
		Guid(@"3050F434-98B5-11CF-BB82-00AA00BDCE0B"), 
		InterfaceType(ComInterfaceType.InterfaceIsDual)
	]
	public interface IHTMLElement2: IHTMLElement {
		
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
		string scopeName();

		/// <summary>
		/// </summary>
		/// <param name="containerCapture"></param>
		void setCapture(bool containerCapture);

		/// <summary>
		/// </summary>
		void releaseCapture();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOnlosecapture(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOnlosecapture();

		/// <summary>
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
		string componentFromPoint(int x,int y);

		/// <summary>
		/// </summary>
		/// <param name="component"></param>
		void doScroll(object component); //maybe better UnmanagedType.BStr?

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOnscroll(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOnscroll();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOndrag(object v);
		
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOndrag();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOndragend(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOndragend();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOndragenter(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOndragenter();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOndragover(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOndragover();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOndragleave(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOndragleave();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOndrop(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOndrop();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOnbeforecut(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOnbeforecut();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOncut(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOncut();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOnbeforecopy(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOnbeforecopy();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOncopy(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOncopy();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOnbeforepaste(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOnbeforepaste();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOnpaste(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOnpaste();

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
		object currentStyle();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOnpropertychange(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOnpropertychange();

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
		object getClientRects();

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
		object getBoundingClientRect();

		/// <summary>
		/// </summary>
		/// <param name="propname"></param>
		/// <param name="expression"></param>
		/// <param name="language"></param>
		void setExpression(string propname, string expression, string language);
		
		/// <summary>
		/// </summary>
		/// <param name="propname"></param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getExpression(string propname);

		/// <summary>
		/// </summary>
		/// <param name="propname"></param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)] 
		bool removeExpression(string propname);

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setTabIndex(Int16 v);
		/// <summary>
		/// </summary>
		/// <returns></returns>
		Int16 getTabIndex();
		
		/// <summary>
		/// </summary>
		void focus();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setAccessKey(string v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
		string getAccessKey();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOnblur(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOnblur();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOnfocus(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOnfocus();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOnresize(object v);

		/// <summary>
		/// </summary>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOnresize();

		/// <summary>
		/// </summary>
		void blur();

		/// <summary>
		/// </summary>
		/// <param name="pUnk"></param>
		void addFilter(object pUnk);

		/// <summary>
		/// </summary>
		/// <param name="pUnk"></param>
		void  removeFilter(object pUnk);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		int clientHeight();
		/// <summary>
		/// </summary>
		/// <returns></returns>
		int clientWidth();
		/// <summary>
		/// </summary>
		/// <returns></returns>
		int clientTop();
		/// <summary>
		/// </summary>
		/// <returns></returns>
		int clientLeft();

		/// <summary>
		/// </summary>
		/// <param name="dhtmlEvent"></param>
		/// <param name="pDisp"></param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)] 
		bool attachEvent(string dhtmlEvent,object pDisp);

		/// <summary>
		/// </summary>
		/// <param name="dhtmlEvent"></param>
		/// <param name="pDisp"></param>
		void detachEvent(string dhtmlEvent,object pDisp);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object readyState();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOnreadystatechange(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOnreadystatechange();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOnrowsdelete(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOnrowsdelete();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOnrowsinserted(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOnrowsinserted();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setOncellchange(object v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
		object getOncellchange();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setDir(string v);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
		string getDir();

		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
		object createControlRange();

		/// <summary>
		/// </summary>
		/// <returns></returns>
		int scrollHeight();
		/// <summary>
		/// </summary>
		/// <returns></returns>
		int scrollWidth();
		
		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setScrollTop(int v);
		/// <summary>
		/// </summary>
		/// <returns></returns>
		int getScrollTop();

		/// <summary>
		/// </summary>
		/// <param name="v"></param>
		void setScrollLeft(int v);
		/// <summary>
		/// </summary>
		/// <returns></returns>
		int getScrollLeft();

		/// <summary>
		/// </summary>
		void clearAttributes();

	/*
	 *  under construction:
	 * 
		[id(DISPID_IHTMLELEMENT2_MERGEATTRIBUTES)] HRESULT mergeAttributes([in] IHTMLElement* mergeThis);
		[propput, id(DISPID_IHTMLELEMENT2_ONCONTEXTMENU), displaybind, bindable] HRESULT oncontextmenu(object v);
		[propget, id(DISPID_IHTMLELEMENT2_ONCONTEXTMENU), displaybind, bindable] HRESULT oncontextmenu();
		[id(DISPID_IHTMLELEMENT2_INSERTADJACENTELEMENT)] HRESULT insertAdjacentElement([in] BSTR where,[in] IHTMLElement* insertedElement,[retval, out] IHTMLElement** inserted);
		[id(DISPID_IHTMLELEMENT2_APPLYELEMENT)] HRESULT applyElement([in] IHTMLElement* apply,[in] BSTR where,[retval, out] IHTMLElement** applied);
		[id(DISPID_IHTMLELEMENT2_GETADJACENTTEXT)] HRESULT getAdjacentText([in] BSTR where,[retval, out] BSTR* text);
		[id(DISPID_IHTMLELEMENT2_REPLACEADJACENTTEXT)] HRESULT replaceAdjacentText([in] BSTR where,[in] BSTR newText,[retval, out] BSTR* oldText);
		[propget, id(DISPID_IHTMLELEMENT2_CANHAVECHILDREN)] HRESULT canHaveChildren([retval, out] VARIANT_BOOL * p);
		[id(DISPID_IHTMLELEMENT2_ADDBEHAVIOR)] HRESULT addBehavior([in] BSTR bstrUrl,[optional, in] VARIANT* pvarFactory,[retval, out] long* pCookie);
		[id(DISPID_IHTMLELEMENT2_REMOVEBEHAVIOR)] HRESULT removeBehavior([in] long cookie,[retval, out] VARIANT_BOOL* pfResult);
		[propget, id(DISPID_IHTMLELEMENT2_RUNTIMESTYLE), nonbrowsable] HRESULT runtimeStyle([retval, out] IHTMLStyle* * p);
		[propget, id(DISPID_IHTMLELEMENT2_BEHAVIORURNS)] HRESULT behaviorUrns([retval, out] IDispatch* * p);
		[propput, id(DISPID_IHTMLELEMENT2_TAGURN)] HRESULT tagUrn([in] BSTR v);
		[propget, id(DISPID_IHTMLELEMENT2_TAGURN)] HRESULT tagUrn([retval, out] BSTR * p);
		[propput, id(DISPID_IHTMLELEMENT2_ONBEFOREEDITFOCUS), displaybind, bindable] HRESULT onbeforeeditfocus(object v);
		[propget, id(DISPID_IHTMLELEMENT2_ONBEFOREEDITFOCUS), displaybind, bindable] HRESULT onbeforeeditfocus();
		[propget, id(DISPID_IHTMLELEMENT2_READYSTATEVALUE), hidden, restricted] HRESULT readyStateValue([retval, out] long * p);
		[id(DISPID_IHTMLELEMENT2_GETELEMENTSBYTAGNAME)] HRESULT getElementsByTagName([in] BSTR v,[retval, out] IHTMLElementCollection** pelColl);
		*/
	} 
}

