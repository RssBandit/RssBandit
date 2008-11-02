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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Permissions;
using System.Windows.Forms;

namespace IEControl
{
	/// <summary>
	/// Summary description for DocHostUIHandler.
	/// More infos: http://www.codeproject.com/books/0764549146_8.asp?print=true
	/// </summary>
	/// <remarks>MUST be public, or the public int setFlags() method
	/// will not be called from hosted IE!</remarks>
	public class DocHostUIHandler : 
		Interop.IDocHostUIHandler, 
		Interop.IOleClientSite, 
		Interop.IOleContainer, 
		Interop.IOleInPlaceSite,
		Interop.IAdviseSink
		//Interop.IServiceProvider
		
	{
		private readonly HtmlControl hostControl;

		internal DocHostUIHandler(HtmlControl hostControl)
		{
			if ((hostControl == null) || (hostControl.IsHandleCreated == false)) {
				throw new ArgumentNullException("hostControl");
			}

			this.hostControl = hostControl;
		}


		[DispId(HTMLDispIDs.DISPID_AMBIENT_DLCONTROL)]
		public int setFlags() {
			//This determines which features are enabled in the control
			int flags = 0;

			if (!this.hostControl.ActiveXEnabled) {
				flags |= Interop.DLCTL_NO_DLACTIVEXCTLS	| Interop.DLCTL_NO_RUNACTIVEXCTLS;
			}

			if (this.hostControl.SilentModeEnabled) {
				flags |= Interop.DLCTL_SILENT;
			}

			if (!this.hostControl.ScriptEnabled) {
				flags |= Interop.DLCTL_NO_SCRIPTS;
			}

			if (!this.hostControl.JavaEnabled) {
				flags |= Interop.DLCTL_NO_JAVA;
			}

			if (this.hostControl.ImagesDownloadEnabled) {
				flags |= Interop.DLCTL_DLIMAGES;
			}

			if (this.hostControl.BackroundSoundEnabled) {
				flags |= Interop.DLCTL_BGSOUNDS;
			}

			if (this.hostControl.VideoEnabled) {
				flags |= Interop.DLCTL_VIDEOS;
			}

			if (!this.hostControl.ClientPullEnabled) {
				flags |= Interop.DLCTL_NO_CLIENTPULL;
			}

			if (!this.hostControl.FrameDownloadEnabled) {
				flags |= Interop.DLCTL_NO_FRAMEDOWNLOAD;
			}

			if (!this.hostControl.BehaviorsExecuteEnabled) {
				flags |= Interop.DLCTL_NO_BEHAVIORS;
			}

			return flags;
		}

		#region Implementation of IDocHostUIHandler
		/// <summary>
		/// Read http://www.codeproject.com/miscctrl/WBCArticle.asp for further details
		/// </summary>
		/// <param name="dwID"></param>
		/// <param name="pt"></param>
		/// <param name="pcmdtReserved"></param>
		/// <param name="pdispReserved"></param>
		/// <returns></returns>
		public int ShowContextMenu(int dwID, Interop.POINT pt, object pcmdtReserved, object pdispReserved) {
			int ret = Interop.S_FALSE;
			Point location = hostControl.PointToClient(new Point(pt.x, pt.y));
			BrowserContextMenuCancelEventArgs e = new BrowserContextMenuCancelEventArgs(location, false);
			try {
				hostControl.RaiseOnShowContextMenu(e);
			}
			catch {}
			finally {
				if (e.Cancel)
					ret = Interop.S_OK;
			}
			return ret;
		}

		public int GetHostInfo(Interop.DOCHOSTUIINFO info) {
			info.dwDoubleClick = Interop.DOCHOSTUIDBLCLICK_DEFAULT;
			int flags = 0;
			if (hostControl.AllowInPlaceNavigation) {
				flags |= Interop.DOCHOSTUIFLAG_ENABLE_INPLACE_NAVIGATION;
			}
			if (!hostControl.Border3d) {
				flags |= Interop.DOCHOSTUIFLAG_NO3DBORDER;
			}
			if (!hostControl.ScriptEnabled) {
				flags |= Interop.DOCHOSTUIFLAG_DISABLE_SCRIPT_INACTIVE;
			}
			if (!hostControl.ScrollBarsEnabled) {
				flags |= Interop.DOCHOSTUIFLAG_SCROLL_NO;
			}
			if (hostControl.FlatScrollBars) {
				flags |= Interop.DOCHOSTUIFLAG_FLAT_SCROLLBAR;
			}
			info.dwFlags = flags;
			return Interop.S_OK;
		}

		public int ShowUI(int dwID, Interop.IOleInPlaceActiveObject activeObject, Interop.IOleCommandTarget commandTarget, Interop.IOleInPlaceFrame frame, Interop.IOleInPlaceUIWindow doc) {
			return Interop.S_OK;
		}

		public int HideUI() {
			return Interop.S_OK;
		}

		public int UpdateUI() {
			return Interop.S_OK;
		}

		public int EnableModeless(bool fEnable) {
			return Interop.S_OK;
		}

		public int OnDocWindowActivate(bool fActivate) {
			return Interop.E_NOTIMPL;
		}

		public int OnFrameWindowActivate(bool fActivate) {
			return Interop.E_NOTIMPL;
		}

		public int ResizeBorder(Interop.COMRECT rect, Interop.IOleInPlaceUIWindow doc, bool fFrameWindow) {
			return Interop.E_NOTIMPL;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="group"></param>
		/// <param name="nCmdID"></param>
		/// <returns></returns>
		/// <remarks>
		/// Main Accelerator Keys Supported by MsHtml 
		/// Accelerator |  Description  
		/// Ctrl-N | Opens the current HTML document in a new WebBrowser window.
		/// Crtl-P | Displays a Print dialog box for printing the HTML document.
		/// Ctrl-A | Selects the entire contents of the HTML document.
		/// Crtl-F | Displays a Find dialog box for searching the HTML document.
		/// F5, Ctrl-F5 | Refreshes the currently loaded HTML document.
 		/// </remarks>
		public int TranslateAccelerator(Interop.COMMSG msg, ref Guid group, int nCmdID) {

			const int WM_KEYDOWN = 0x0100;
			const int VK_CONTROL = 0x11;
			
			if (msg.message != WM_KEYDOWN)
				return Interop.S_FALSE;				// no keydown, allow message

			Keys keyData = Keys.None;
			if (Interop.GetAsyncKeyState(VK_CONTROL) < 0)
				keyData |= Keys.Control;				// Ctrl key pressed

			int key = msg.wParam.ToInt32();
			key &= 0xFF; // get the virtual keycode
			keyData |= (Keys)key;

			KeyEventArgs kea = new KeyEventArgs(keyData);
			hostControl.RaiseOnTranslateAccelerator(kea);
			
			if (kea.Handled)
				return Interop.S_OK;
			
			// not handled, allow everything else
			return Interop.S_FALSE;
		}

		public int GetOptionKeyPath(string[] pbstrKey, int dw) {
			pbstrKey[0] = null;
			return Interop.S_OK;
		}

		public int GetDropTarget(Interop.IOleDropTarget pDropTarget, out Interop.IOleDropTarget ppDropTarget) {
			ppDropTarget = null; // no other drop target
			return Interop.S_OK;
		}

		public int GetExternal(out object ppDispatch) {
			//TODO: we should force the hostControl.ScriptObject return an object that have the IDispatch attribute attached
			ppDispatch = hostControl.ScriptObject;
			if (ppDispatch != null) {
				return Interop.S_OK;
			}
			else {
				return Interop.E_NOTIMPL;
			}
		}

		[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
		public int TranslateUrl(int dwTranslate, string strURLIn, out IntPtr pstrURLOut)
		{
			pstrURLOut = IntPtr.Zero;
			BrowserTranslateUrlEventArgs e = new BrowserTranslateUrlEventArgs(strURLIn);
			try {
				hostControl.RaiseOnTranslateUrl(e);
				// this can maybe also cause exceptions: see http://support.microsoft.com/?kbid=327106
				pstrURLOut = Marshal.StringToCoTaskMemUni(e.TranslatedUrl);
			}
			catch {}
			return Interop.S_OK;
		}

		public int FilterDataObject(object pDO, out object ppDORet) {
			ppDORet = pDO;
			return Interop.S_OK;
		}
		#endregion

		#region Implementation of IOleClientSite
		public int SaveObject() {
			return Interop.S_OK;
		}

		public int GetMoniker(int dwAssign, int dwWhichMoniker, out object ppmk) {
			ppmk = null;
			return Interop.E_NOTIMPL;
		}

		public int GetContainer(out Interop.IOleContainer ppContainer) {
			ppContainer = null;
			return Interop.S_OK;
		}

		public int ShowObject() {
			return Interop.S_OK;
		}

		public int OnShowWindow(int fShow) {
			return Interop.S_OK;
		}

		public int RequestNewObjectLayout() {
			return Interop.S_OK;
		}
		#endregion

		#region Implementation of IOleContainer
		public void ParseDisplayName(object pbc, string pszDisplayName, int[] pchEaten, object[] ppmkOut) {
			//Debug.Fail("ParseDisplayName - " + pszDisplayName);
			throw new COMException(String.Empty, Interop.E_NOTIMPL);
		}

		public void EnumObjects(int grfFlags, object[] ppenum) {
			throw new COMException(String.Empty, Interop.E_NOTIMPL);
		}

		public void LockContainer(int fLock) {
		}
		#endregion
	
		#region IOleInPlaceSite Members

		public IntPtr GetWindow() {
			return this.hostControl.Handle;
		}

		public void ContextSensitiveHelp(int fEnterMode) {
			// TODO:  Add DocHostUIHandler.ContextSensitiveHelp implementation
		}

		public int CanInPlaceActivate() {
			// TODO:  Add DocHostUIHandler.CanInPlaceActivate implementation
			return 0;
		}

		public void OnInPlaceActivate() {
			// TODO:  Add DocHostUIHandler.OnInPlaceActivate implementation
		}

		public void OnUIActivate() {
			// TODO:  Add DocHostUIHandler.OnUIActivate implementation
		}

		public void GetWindowContext(out Interop.IOleInPlaceFrame ppFrame, out Interop.IOleInPlaceUIWindow ppDoc, Interop.COMRECT lprcPosRect, Interop.COMRECT lprcClipRect, Interop.tagOIFI lpFrameInfo) {
			// TODO:  Add DocHostUIHandler.GetWindowContext implementation
			ppFrame = null;
			ppDoc = null;
		}

		public int Scroll(Interop.tagSIZE scrollExtent) {
			// TODO:  Add DocHostUIHandler.Scroll implementation
			return 0;
		}

		public void OnUIDeactivate(int fUndoable) {
			// TODO:  Add DocHostUIHandler.OnUIDeactivate implementation
		}

		public void OnInPlaceDeactivate() {
			// TODO:  Add DocHostUIHandler.OnInPlaceDeactivate implementation
		}

		public void DiscardUndoState() {
			// TODO:  Add DocHostUIHandler.DiscardUndoState implementation
		}

		public void DeactivateAndUndo() {
			// TODO:  Add DocHostUIHandler.DeactivateAndUndo implementation
		}

		public int OnPosRectChange(Interop.COMRECT lprcPosRect) {
			// TODO:  Add DocHostUIHandler.OnPosRectChange implementation
			return 0;
		}

		#endregion

		#region IAdviseSink Members

		public void OnDataChange(object pFormatetc, object pStgmed) {
			// TODO:  Add DocHostUIHandler.OnDataChange implementation
		}

		public void OnViewChange(int dwAspect, int lindex) {
			// TODO:  Add DocHostUIHandler.OnViewChange implementation
		}

		public void OnRename(IMoniker pmk) {
			// TODO:  Add DocHostUIHandler.OnRename implementation
		}

		public void OnSave() {
			// TODO:  Add DocHostUIHandler.OnSave implementation
		}

		public void OnClose() {
			// TODO:  Add DocHostUIHandler.OnClose implementation
		}

		#endregion

//		#region IServiceProvider Members
//
//		public void QueryService(ref Guid guidService, ref Guid riid, out object ppvObject) {
//			// TODO:  Add DocHostUIHandler.QueryService implementation
//			Trace.WriteLine("guidService: " + guidService.ToString() + ", riid: " + riid.ToString() ,"IServiceProvider.QueryService" );
//			ppvObject = null;
//		}
//
//		#endregion
	}
}
