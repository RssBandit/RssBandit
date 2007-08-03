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

using System.Runtime.InteropServices;

namespace IEControl
{
    /// <summary>
    /// mshtml.IHTMLDocument2 interface
    /// </summary>
	[
        InterfaceType(ComInterfaceType.InterfaceIsDual), 
        ComVisible(true), 
        Guid(@"332C4425-26CB-11D0-B483-00C04FD90119")
    ]
    public interface IHTMLDocument2 
    {
        /// <summary>
        /// </summary>
        /// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object GetScript(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object GetAll(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        IHTMLElement2 GetBody(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object GetActiveElement(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object GetImages(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object GetApplets(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object GetLinks(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object GetForms(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object GetAnchors(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetTitle([In, MarshalAs(UnmanagedType.BStr)] string p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string GetTitle(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object GetScripts(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetDesignMode(string p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string GetDesignMode(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object GetSelection(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string GetReadyState(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object GetFrames(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object GetEmbeds(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object GetPlugins(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetAlinkColor(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
        object GetAlinkColor(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetBgColor(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
        object GetBgColor(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetFgColor(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
        object GetFgColor(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetLinkColor(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
        object GetLinkColor(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetVlinkColor(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
        object GetVlinkColor(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string GetReferrer(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object GetLocation(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string GetLastModified(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetURL(string p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string GetURL(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetDomain(string p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string GetDomain(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetCookie(string p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string GetCookie(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetExpando(bool p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)] 
        bool GetExpando(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetCharset(string p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string GetCharset(); 
        
		/// <summary>
		/// </summary>
		/// <param name="p"></param>
		void SetDefaultCharset(string p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string GetDefaultCharset(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string GetMimeType(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string GetFileSize(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string GetFileCreatedDate(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string GetFileModifiedDate(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string GetFileUpdatedDate(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string GetSecurity(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string GetProtocol(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string GetNameProp(); 
        
		/// <summary>
		/// Write complete html doc content including markup. 
		/// </summary>
		/// <param name="psarray">object[] containing the string(s)</param>
		/// <example>
		/// <code>
		///   IHTMLDocument2 document = control.GetDocument();
		///   if (document != null) {
		///      document.Open("", null, null, null);
		///      object[] a = new object[]{"<html><body>Hello world</body></html>"};
		///      document.DummyWrite(a);
		///      document.Close();
		///   }
		/// </code>
		/// </example>
		void Write(
			[In, MarshalAs(UnmanagedType.SafeArray)]
			object[] psarray);

		/// <summary>
		/// Write complete html doc content including markup. 
		/// </summary>
		/// <param name="psarray"></param>
		void Writeln(
			[In, MarshalAs(UnmanagedType.SafeArray)]
			object[] psarray);
        
        /// <summary>
        /// </summary>
        /// <param name="URL"></param>
        /// <param name="name"></param>
        /// <param name="features"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object Open(string URL, object name, object features, object replace); 
        
        /// <summary>
        /// </summary>
		void Close(); 
        
		/// <summary>
		/// </summary>
		void Clear(); 
        
        /// <summary>
        /// </summary>
        /// <param name="cmdID"></param>
        /// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)] 
        bool QueryCommandSupported(string cmdID); 
        
		/// <summary>
		/// </summary>
		/// <param name="cmdID"></param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)] 
        bool QueryCommandEnabled(string cmdID); 
        
		/// <summary>
		/// </summary>
		/// <param name="cmdID"></param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)] 
        bool QueryCommandState(string cmdID); 
        
		/// <summary>
		/// </summary>
		/// <param name="cmdID"></param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)] 
        bool QueryCommandIndeterm(string cmdID); 
        
		/// <summary>
		/// </summary>
		/// <param name="cmdID"></param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string QueryCommandText(string cmdID); 
        
		/// <summary>
		/// </summary>
		/// <param name="cmdID"></param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
        object QueryCommandValue(string cmdID); 
        
        /// <summary>
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="showUI"></param>
        /// <param name="value"></param>
        /// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)] 
        bool ExecCommand(string cmdID, bool showUI, object value); 
        
        /// <summary>
        /// </summary>
        /// <param name="cmdID"></param>
        /// <returns></returns>
		[return: MarshalAs(UnmanagedType.Bool)] 
        bool ExecCommandShowHelp(string cmdID); 
        
        /// <summary>
        /// </summary>
        /// <param name="eTag"></param>
        /// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object CreateElement(string eTag); 
        
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
		void SetOnkeyup(object p); 
        
        /// <summary>
        /// </summary>
        /// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
        object GetOnkeyup(); 
        
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
		void SetOnkeypress(object p); 
        
        /// <summary>
        /// </summary>
        /// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
        object GetOnkeypress(); 
        
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
		void SetOnmousemove(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
        object GetOnmousemove(); 
        
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
		void SetOnreadystatechange(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
        object GetOnreadystatechange(); 
        
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
		void SetOndragstart(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
        object GetOndragstart(); 
        
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
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object ElementFromPoint(int x, int y); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object GetParentWindow(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] 
        object GetStyleSheets(); 
        
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
		void SetOnerrorupdate(object p); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Struct)] 
        object GetOnerrorupdate(); 
        
		/// <summary>
		/// </summary>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.BStr)] 
        string toString(); 
        
		/// <summary>
		/// </summary>
		/// <param name="bstrHref"></param>
		/// <param name="lIndex"></param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)]
		object CreateStyleSheet(
			[In, MarshalAs(UnmanagedType.BStr)] string bstrHref,
			[In, MarshalAs(UnmanagedType.I4)]	int lIndex);
	} 
} 

