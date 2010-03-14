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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

using Interop.SHDocVw;

namespace IEControl
{
	/// <summary>
	/// ActiveX Wrapper for the web browser control.
	/// </summary>
	[
	ClsidAttribute("{8856f961-340a-11d0-a96b-00c04fd705a2}"),
	DesignTimeVisible(true),
	DefaultProperty("Name"),
	ToolboxItem(true),
	ToolboxBitmap( typeof( HtmlControl ), "Resources.IEControl.Toolbitmap16.bmp" ),
	CLSCompliant(false),
	ComVisible(false)
	]
	public class HtmlControl : AxHost , IMessageFilter
    {
        
		///<summary>
		///</summary>
		public static Assembly SHDocVwAssembly;

		/// <summary>
		/// Gets the current IE Version found on the system
		/// </summary>
		public static Version CurrentIEVersion = new Version(1,0);
		
		#region private members
		internal IWebBrowser2 ocx ;
		private WebBrowser_V1 ocx_v1;
		private AxWebBrowserEventMulticaster eventMulticaster;
		private ConnectionPointCookie cookie;
		private ShellUIHelper shellHelper;
		private DocHostUIHandler uiHandler;
		private Interop.IWebBrowser2Application iwb2app;
		private bool ie7BuildInZoomEnabled;
        private int currentOpticalZoomFactor = 100;
        private const int zoomStep = 5;

		string url = String.Empty;
		string html = String.Empty;
		string body = String.Empty;

		[Flags]
		private enum ControlBehaviorFlags { 
			activate = 0x01,
			allowInPlaceNavigation = 0x02,
			border3d = 0x04, 
			flatScrollBars = 0x08,
			scriptEnabled = 0x10,
			activeXEnabled = 0x20,
			javaEnabled = 0x40,
			bgSoundEnabled = 0x80,
			imagesDownloadEnabled = 0x100,
			videoEnabled = 0x200,
			scrollBarsEnabled = 0x400,
			silentModeEnabled = 0x800,
			clientPullEnabled =  0x1000,
			behaviorsExecuteEnabled =  0x2000,
			frameDownloadEnabled =  0x4000
		}

		/// <summary>
		/// Store all the various booleans now as a flag enum, to save a bit memory
		/// </summary>
		private ControlBehaviorFlags cbFlags;

		/// <summary>
		/// true, if we did not attach DocHostUIHandler (causes exception(s) on Win98/Me :(
		/// </summary>
		private static readonly bool lowSecurity;

		/// <summary>
		/// holds the external script reference.
		/// </summary>
		private object scriptObject;
		#endregion

		#region public events

        /// <summary>
        /// Occurs when optical zoom factor changed.
        /// </summary>
	    public event EventHandler OpticalZoomFactorChanged;
		///<summary>
		///</summary>
		public event BrowserPrivacyImpactedStateChangeEventHandler PrivacyImpactedStateChange;
        
		///<summary>
		///</summary>
		public event BrowserUpdatePageStatusEventHandler UpdatePageStatus;
        
		///<summary>
		///</summary>
		public event BrowserPrintTemplateTeardownEventHandler PrintTemplateTeardown;
        
		///<summary>
		///</summary>
		public event BrowserPrintTemplateInstantiationEventHandler PrintTemplateInstantiation;
        
		///<summary>
		///</summary>
		public event BrowserNavigateErrorEventHandler NavigateError;
        
		///<summary>
		///</summary>
		public event BrowserFileDownloadEventHandler FileDownload;
        
		///<summary>
		///</summary>
		public event BrowserSetSecureLockIconEventHandler SetSecureLockIcon;
        
		///<summary>
		///</summary>
		public event BrowserClientToHostWindowEventHandler ClientToHostWindow;
        
		///<summary>
		///</summary>
		public event BrowserWindowClosingEventHandler WindowClosing;
        
		///<summary>
		///</summary>
		public event BrowserWindowSetHeightEventHandler WindowSetHeight;
        
		///<summary>
		///</summary>
		public event BrowserWindowSetWidthEventHandler WindowSetWidth;
        
		///<summary>
		///</summary>
		public event BrowserWindowSetTopEventHandler WindowSetTop;
        
		///<summary>
		///</summary>
		public event BrowserWindowSetLeftEventHandler WindowSetLeft;
        
		///<summary>
		///</summary>
		public event BrowserWindowSetResizableEventHandler WindowSetResizable;
        
		#region unwanted events
		//		///<summary>
		//		///</summary>
		//		public event BrowserOnTheaterModeEventHandler OnTheaterMode;
		//        
		//		///<summary>
		//		///</summary>
		//		public event BrowserOnFullScreenEventHandler OnFullScreen;
		//        
		//		///<summary>
		//		///</summary>
		//		public event BrowserOnStatusBarEventHandler OnStatusBar;
		//        
		//		///<summary>
		//		///</summary>
		//		public event BrowserOnMenuBarEventHandler OnMenuBar;
		//        
		//		///<summary>
		//		///</summary>
		//		public event BrowserOnToolBarEventHandler OnToolBar;
		#endregion

		///<summary>
		///</summary>
		public event BrowserOnVisibleEventHandler OnVisible;
        
		///<summary>
		///</summary>
		public event System.EventHandler OnQuit;
        
		///<summary>
		///</summary>
		public event BrowserDocumentCompleteEventHandler DocumentComplete;
        
		///<summary>
		///</summary>
		public event BrowserNavigateComplete2EventHandler NavigateComplete;
        
		///<summary>
		///</summary>
		public event BrowserNewWindow2EventHandler NewWindow2;

		/// <summary>
		/// </summary>
		public event EventHandler<BrowserNewWindow3Event> NewWindow3;
        
		///<summary>
		///</summary>
		public event BrowserNewWindowEventHandler NewWindow;

		///<summary>
		///</summary>
		public event BrowserBeforeNavigate2EventHandler BeforeNavigate;
        
		///<summary>
		///</summary>
		public event BrowserPropertyChangeEventHandler PropertyChanged;
        
		///<summary>
		///</summary>
		public event BrowserTitleChangeEventHandler TitleChanged;
        
		///<summary>
		///</summary>
		public event EventHandler DownloadCompleted;
        
		///<summary>
		///</summary>
		public event EventHandler DownloadBegin;
        
		///<summary>
		///</summary>
		public event BrowserCommandStateChangeEventHandler CommandStateChanged;
        
		///<summary>
		///</summary>
		public event BrowserProgressChangeEventHandler ProgressChanged;
        
		///<summary>
		///</summary>
		public event BrowserStatusTextChangeEventHandler StatusTextChanged;
        
		/// <summary>
		/// </summary>
		public event BrowserTranslateUrlEventHandler TranslateUrl;

		/// <summary>
		/// </summary>
		public event BrowserContextMenuCancelEventHandler ShowContextMenu;
		
		/// <summary>
		/// </summary>
		public event KeyEventHandler TranslateAccelerator;

		#endregion

		///<summary>
		///Summary of HtmlControl.
		///</summary>
		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		public HtmlControl()
			: base("8856f961-340a-11d0-a96b-00c04fd705a2")
		{
			this.cbFlags = ControlBehaviorFlags.scrollBarsEnabled |
				ControlBehaviorFlags.imagesDownloadEnabled |
				ControlBehaviorFlags.scriptEnabled  |
				ControlBehaviorFlags.javaEnabled  |
				ControlBehaviorFlags.flatScrollBars |
				ControlBehaviorFlags.imagesDownloadEnabled |
				ControlBehaviorFlags.behaviorsExecuteEnabled |
				ControlBehaviorFlags.frameDownloadEnabled |
				ControlBehaviorFlags.clientPullEnabled;

			HandleCreated += SelfHandleCreated;
			HandleDestroyed += SelfHandleDestroyed;
			NavigateComplete += SelfNavigateComplete;

		}
    
		///<summary>
		///Summary of HtmlControl.
		///</summary>
		static HtmlControl() {
			try {
				lowSecurity = (Environment.OSVersion.Platform != PlatformID.Win32NT);
				CurrentIEVersion = OSHelper.GetInternetExplorerVersion();
				_UseCurrentDll();
				return;
			}
			catch{}

			try {
				SHDocVwAssembly = Interop.GetAssemblyForTypeLib( "SHDocVw.DLL" );
				AppDomain.CurrentDomain.AssemblyResolve += RedirectSHDocAssembly;
			}
			catch{}
		}

		///<summary>
		///Summary of _UseCurrentDll.
		///</summary>
		private static void _UseCurrentDll() {
			SHDocVwAssembly = typeof( IWebBrowser2 ).Assembly;
		}

		///<summary>
		///Summary of RedirectSHDocAssembly.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		///<returns></returns>	
		private static Assembly RedirectSHDocAssembly( object sender, ResolveEventArgs e ) {
			if( e.Name != null && e.Name.StartsWith( "Interop.SHDocVw", StringComparison.OrdinalIgnoreCase ) )
				return SHDocVwAssembly;
			return null;
		}

		void SelfHandleCreated(object s, EventArgs e) {

			HandleCreated -= SelfHandleCreated;
			if (DesignMode)
				return;

			if (url == null)		url = String.Empty;
			if (html == null)	html = String.Empty;
			if (body == null)	body = String.Empty;

			if (body == String.Empty && html == String.Empty)
				url = "about:blank";

			// attach to the Browser.V1 new window event
			ocx_v1 = ocx as WebBrowser_V1;
			if (ocx_v1 != null) {
				ocx_v1.NewWindow += this.RaiseOnNewBrowserWindow;
			}

			if (!lowSecurity) {
				// need to SetClientSite() to enable set properties in GetHostInfo() and setFlags():
				Interop.IOleObject oleObj = ocx as Interop.IOleObject;
				if (oleObj != null) {
					uiHandler = new DocHostUIHandler(this);
					// next line causes problems on Win98 :(
					oleObj.SetClientSite(uiHandler);
				}
			}
			
			Interop.IOleControl oleCtrl = ocx as Interop.IOleControl;
			if (oleCtrl != null)
				oleCtrl.OnAmbientPropertyChange(HTMLDispIDs.DISPID_AMBIENT_DLCONTROL);

            System.Windows.Forms.Application.AddMessageFilter(this);
		}

		

		// this can be called multiple times in the lifetime of the control!!!
		void SelfHandleDestroyed(object s, EventArgs e) 
        {
            System.Windows.Forms.Application.RemoveMessageFilter(this);
			if (ocx_v1 != null) {
				ocx_v1.NewWindow -=  this.RaiseOnNewBrowserWindow;
			}
			ocx_v1 = null;
			uiHandler = null;
		}

		/// <summary>
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SelfNavigateComplete(object sender, BrowserNavigateComplete2Event e) {
			CheckAndActivate();

			if (this.html != String.Empty) {
				this.SetHtmlText(html);
				RaiseOnDocumentComplete(this, new BrowserDocumentCompleteEvent(e.pDisp, e.url, e.IsRootPage ));
				this.html = String.Empty;
			}

			if (this.body != String.Empty) {
				SetBodyText(body);
				RaiseOnDocumentComplete(this, new BrowserDocumentCompleteEvent(e.pDisp, e.url, e.IsRootPage));
				this.body = String.Empty;
			}

			if (!ie7BuildInZoomEnabled && CurrentIEVersion.Major >= 7)
			{
				// re-enable build-in zoom.
				// see also http://msdn.microsoft.com/en-us/library/aa770056(VS.85).aspx
				object inval = 100;
				object outval = 0;
				ExecWB((OLECMDID)Interop.OLECMDID_OPTICAL_ZOOM, OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER, ref inval, ref outval);
				ie7BuildInZoomEnabled = true;
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		protected override void Dispose(bool disposing)
		{
			if( disposing ) {
				// free own resources
				if (shellHelper != null) {
					try {
						// this can maybe also cause exceptions: see http://support.microsoft.com/?kbid=327106
						Marshal.ReleaseComObject(shellHelper);
					} catch {}
					shellHelper = null;
				}
			}
			base.Dispose (disposing);
		}

      
		/// <summary>
		/// Workaround/BugBug: handle OnQuit/Zoom in IE8
		/// </summary>
		/// <param name="m"></param>
		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		protected override void WndProc(ref Message m)
		{
            switch (m.Msg)
            {
                
                case Interop.WM_CLOSE:
                {
                    if (this.RaiseOnQuit(this, EventArgs.Empty))
                        return;
                    break;
                }

			}

            base.WndProc(ref m);
		}

        /// <summary>
        /// Gets or sets the optical zoom factor.
        /// </summary>
        /// <value>The optical zoom factor.</value>
	    public int OpticalZoomFactor
	    {
            get { return this.currentOpticalZoomFactor; }
            set
            {
                if (value >= 10 && value <= 1000)
                {
                    currentOpticalZoomFactor = value;
                    ApplyOpticalZoom(currentOpticalZoomFactor);
                    if (OpticalZoomFactorChanged != null)
                        OpticalZoomFactorChanged(this, EventArgs.Empty);
                }
                else
                    throw new InvalidOperationException(String.Format(
                        "Invalid zoomfactor: {0}. Allowed values are between 10 and 1000", value));
            }
	    }

        /// <summary>
        /// Sets the optical zoom factor (10...1000).
        /// </summary>
        /// <param name="factor">The factor.</param>
        private void ApplyOpticalZoom(int factor)
        {
            if (CurrentIEVersion.Major >= 7)
            {
                object inval = factor;
                object outval = 0;
                ExecWB((OLECMDID)Interop.OLECMDID_OPTICAL_ZOOM, OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER, ref inval, ref outval);
            }
        }

		/// <summary>
		/// Just clear the browser window.
		/// </summary>
		public void Clear() {
			this.html = String.Empty;
			this.body = String.Empty;
			this.Navigate("about:blank");
		}

		/// <summary>
		/// Set/Get HTML.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Html {
			get { return this.html;	}
			set { 
				this.html = value ?? String.Empty;
				this.body = String.Empty;
			}
		}

		/// <summary>
		/// Set/Get the body part of the current HTML
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Body {
			get { return this.body;	}
			set { 
				this.body = value ?? String.Empty;
				this.html  = String.Empty;
			}
		}

		/// <summary>
		/// HtmlControl show activate the content after navigation
		/// </summary>
		public void Activate() {
			this.SetFlag(ControlBehaviorFlags.activate, true);
		}

		/// <summary>
		/// Set/Get a bool to allow in place navigation
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AllowInPlaceNavigation {
			get {	return this.IsFlagSet(ControlBehaviorFlags.allowInPlaceNavigation); }
			set
			{
				this.SetFlag(ControlBehaviorFlags.allowInPlaceNavigation, value);
				SynchAmbientPropertyChange();
			}
		}

		/// <summary>
		/// Set/Get a bool to cotnrol the visual style of the boder
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Border3d {
			get {	return this.IsFlagSet(ControlBehaviorFlags.border3d); }
			set
			{
				this.SetFlag(ControlBehaviorFlags.border3d, value);
				SynchAmbientPropertyChange();
			}
		}

		/// <summary>
		/// Set/Get a bool to control the style of the scrollbars
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool FlatScrollBars {
			get {	return this.IsFlagSet(ControlBehaviorFlags.flatScrollBars); }
			set
			{
				this.SetFlag(ControlBehaviorFlags.flatScrollBars, value);
				SynchAmbientPropertyChange();
			}
		}

		/// <summary>
		/// Set/Get a bool to allow exec. of JavaScript
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ScriptEnabled {
			get {	return this.IsFlagSet(ControlBehaviorFlags.scriptEnabled); }
			set
			{
				this.SetFlag(ControlBehaviorFlags.scriptEnabled, value);
				SynchAmbientPropertyChange();
			}
		}

		/// <summary>
		/// Set/Get a bool to allow to excecute ActiveX controls
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ActiveXEnabled {
			get {	return this.IsFlagSet(ControlBehaviorFlags.activeXEnabled); }
			set
			{
				this.SetFlag(ControlBehaviorFlags.activeXEnabled, value);
				SynchAmbientPropertyChange();
			}
		}
	
		/// <summary>
		/// Set/Get a bool to control if the browsing component will display any user interface
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool SilentModeEnabled {
			get {	return this.IsFlagSet(ControlBehaviorFlags.silentModeEnabled); }
			set {	
				this.SetFlag(ControlBehaviorFlags.silentModeEnabled, value);
				SynchAmbientPropertyChange();
			}
		}

		/// <summary>
		/// Set/Get a bool to allow Java applets
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool JavaEnabled {
			get {	return this.IsFlagSet(ControlBehaviorFlags.javaEnabled); }
			set
			{
				this.SetFlag(ControlBehaviorFlags.javaEnabled, value);
				SynchAmbientPropertyChange();
			}
		}

		/// <summary>
		/// Set/Get a bool to control the playback of background sound
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool BackroundSoundEnabled {
			get {	return this.IsFlagSet(ControlBehaviorFlags.bgSoundEnabled); }
			set
			{
				this.SetFlag(ControlBehaviorFlags.bgSoundEnabled, value);
				SynchAmbientPropertyChange();
			}
		}

		/// <summary>
		/// Set/Get a bool to control the display of any images
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ImagesDownloadEnabled {
			get {	return this.IsFlagSet(ControlBehaviorFlags.imagesDownloadEnabled); }
			set
			{
				this.SetFlag(ControlBehaviorFlags.imagesDownloadEnabled, value);
				SynchAmbientPropertyChange();
			}
		}

		/// <summary>
		/// Set/Get a bool to control the display of embedded video plugins
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool VideoEnabled {
			get {	return this.IsFlagSet(ControlBehaviorFlags.videoEnabled); }
			set
			{
				this.SetFlag(ControlBehaviorFlags.videoEnabled, value);
				SynchAmbientPropertyChange();
			}
		}

		/// <summary>
		/// Set/Get a bool that control the scrollbar dislpay
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ScrollBarsEnabled {
			get {	return this.IsFlagSet(ControlBehaviorFlags.scrollBarsEnabled); }
			set
			{
				this.SetFlag(ControlBehaviorFlags.scrollBarsEnabled, value);
				SynchAmbientPropertyChange();
			}
		}

		/// <summary>
		/// If false, the browsing component will not perform any client pull operations.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ClientPullEnabled {
			get {	return this.IsFlagSet(ControlBehaviorFlags.clientPullEnabled); }
			set
			{
				this.SetFlag(ControlBehaviorFlags.clientPullEnabled, value);
				SynchAmbientPropertyChange();
			}
		}

		/// <summary>
		/// If false, the browsing component will not execute any behaviors.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool BehaviorsExecuteEnabled {
			get {	return this.IsFlagSet(ControlBehaviorFlags.behaviorsExecuteEnabled); }
			set
			{
				this.SetFlag(ControlBehaviorFlags.behaviorsExecuteEnabled, value);
				SynchAmbientPropertyChange();
			}
		}

		/// <summary>
		/// If false, the browsing component will not download frames 
		/// but will download and parse the frameset page. 
		/// The browsing component will also ignore the frameset, 
		/// and will render no frame tags.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool FrameDownloadEnabled {
			get {	return this.IsFlagSet(ControlBehaviorFlags.frameDownloadEnabled); }
			set
			{
				this.SetFlag(ControlBehaviorFlags.frameDownloadEnabled, value);
				SynchAmbientPropertyChange();
			}
		}

		/// <summary>
		/// Set/Get the script object to be used from within JavaScript via 
		/// <c>window.getExternal</c> call
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object ScriptObject {
			get { return this.scriptObject; }
			set { this.scriptObject = value;}
		}

		/// <summary>
		/// Get a bool that informs about low security browser state.
		/// Win98/Me does not support all we need and causes exceptions.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public static bool LowSecurity {
			get {	return lowSecurity;	}
		}

		/// <summary>
		/// Gets the application object (as of IWebBrowser2.Application).
		/// </summary>
		/// <value>The application.</value>
		/// <remarks>See http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=12559&SiteID=1</remarks>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object Application
		{
			get { return iwb2app.Application; }
		}

		/// <summary>
		/// Locks down the browser security for the current process.
		/// It uses the new features introduced by Windows XP SP2 to
		/// enhance browser security. See also
		/// http://www.microsoft.com/technet/prodtechnol/winxppro/maintain/sp2brows.mspx
		/// </summary>
		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		public void EnhanceBrowserSecurityForProcess()
		{
			SetFeatureFlag where = SetFeatureFlag.SET_FEATURE_ON_PROCESS;
			GetFeatureFlag from = GetFeatureFlag.GET_FEATURE_FROM_PROCESS;
			
			// we set/enable the most enhanced security features mentioned at
			// http://msdn.microsoft.com/workshop/security/szone/overview/sec_featurecontrols.asp
			try {
				if (!InternetFeature.IsEnabled(InternetFeatureList.FEATURE_OBJECT_CACHING, from))
					InternetFeature.SetEnabled(InternetFeatureList.FEATURE_OBJECT_CACHING, where, true);
				/*
				 * As long we run in My Computer Zone (we just set HTML Content),
				 * we cannot use the following feature - or a user is not able to navigate
				 * to a trusted site!
				if (!InternetFeature.IsEnabled(InternetFeatureList.FEATURE_ZONE_ELEVATION, from))
					InternetFeature.SetEnabled(InternetFeatureList.FEATURE_ZONE_ELEVATION, where, true);
				*/
				if (!InternetFeature.IsEnabled(InternetFeatureList.FEATURE_MIME_HANDLING, from))
					InternetFeature.SetEnabled(InternetFeatureList.FEATURE_MIME_HANDLING, where, true);
				if (!InternetFeature.IsEnabled(InternetFeatureList.FEATURE_MIME_SNIFFING, from))
					InternetFeature.SetEnabled(InternetFeatureList.FEATURE_MIME_SNIFFING, where, true);
				if (!InternetFeature.IsEnabled(InternetFeatureList.FEATURE_WINDOW_RESTRICTIONS, from))
					InternetFeature.SetEnabled(InternetFeatureList.FEATURE_WINDOW_RESTRICTIONS, where, true);
				if (!InternetFeature.IsEnabled(InternetFeatureList.FEATURE_WEBOC_POPUPMANAGEMENT, from))
					InternetFeature.SetEnabled(InternetFeatureList.FEATURE_WEBOC_POPUPMANAGEMENT, where, true);
				if (!InternetFeature.IsEnabled(InternetFeatureList.FEATURE_ADDON_MANAGEMENT, from))
					InternetFeature.SetEnabled(InternetFeatureList.FEATURE_ADDON_MANAGEMENT, where, true);

				if (!InternetFeature.IsEnabled(InternetFeatureList.FEATURE_SECURITYBAND, from))
					InternetFeature.SetEnabled(InternetFeatureList.FEATURE_SECURITYBAND, where, true);
			
				// next requires FEATURE_SECURITYBAND is enabled to work/notify the user:
				if (!InternetFeature.IsEnabled(InternetFeatureList.FEATURE_RESTRICT_FILEDOWNLOAD, from))
					InternetFeature.SetEnabled(InternetFeatureList.FEATURE_RESTRICT_FILEDOWNLOAD, where, true);
				if (!InternetFeature.IsEnabled(InternetFeatureList.FEATURE_RESTRICT_ACTIVEXINSTALL, from))
					InternetFeature.SetEnabled(InternetFeatureList.FEATURE_RESTRICT_ACTIVEXINSTALL, where, true);
			} catch (EntryPointNotFoundException) {
				System.Diagnostics.Debug.WriteLine("InternetFeature() not available");
			} catch (COMException come){
				System.Diagnostics.Debug.WriteLine(String.Format("InternetFeature() failed with error ({0}):{1}", come.ErrorCode, come.Message));
			}
		}
		
		/// <summary>
		/// Determines whether the specified internet security feature is enabled.
		/// </summary>
		/// <param name="feature">The feature.</param>
		/// <param name="flags">The flags.</param>
		/// <returns>
		/// 	<c>true</c> if the specified feature is enabled; otherwise, <c>false</c>.
		/// </returns>
		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		public static bool IsInternetFeatureEnabled(InternetFeatureList feature, GetFeatureFlag flags)
		{
			try {
				return InternetFeature.IsEnabled(feature, flags);
			} catch (EntryPointNotFoundException) {
				System.Diagnostics.Debug.WriteLine("InternetFeature() not available");
			}
			return false;
		}

		/// <summary>
		/// Sets the specified internet security feature enabled or disabled.
		/// </summary>
		/// <param name="feature">The feature.</param>
		/// <param name="flags">The flags.</param>
		/// <param name="enable">if set to <c>true</c> [enable].</param>
		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		public static void SetInternetFeatureEnabled(InternetFeatureList feature, SetFeatureFlag flags, bool enable)
		{
			try {
				InternetFeature.SetEnabled(feature, flags, enable);
			} catch (EntryPointNotFoundException) {
				System.Diagnostics.Debug.WriteLine("InternetFeature() not available");
			}
		}
		/// <summary>
		/// Enables the parent to test, if there is already a listener attached to the
		/// BeforeNavigate event. Without that, the parent isn't able to test, because
		/// delegates are only allowed to have += and -= operators.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AnyBeforeNavigateEventListener {
			get { return (BeforeNavigate != null);}
		}

		#region unwanted 1
		//		///<summary>
		//		///Gets a value of object.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(200)]
		//        public virtual object Application {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("Application", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.Application;
		//            }
		//        }
        
		//		///<summary>
		//		///Gets a value of object.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(201)]
		//        public virtual object CtlParent {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("CtlParent", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.Parent;
		//            }
		//        }
        
		//		///<summary>
		//		///Gets a value of object.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(202)]
		//        public virtual object CtlContainer {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("CtlContainer", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.Container;
		//            }
		//        }
        
		#endregion

		///<summary>
		///Gets a value of object.
		///</summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[DispIdAttribute(203)]
		public virtual object Document {
			get
			{
				return (this.ocx == null) ? null : this.ocx.Document;
			}
		}

		///<summary>
		///Gets a value of object.
		///</summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public virtual IHTMLDocument2 Document2 {
			get { 
				if ((this.ocx == null)) {
					return null;
				}
				return this.ocx.Document as IHTMLDocument2;
			}
		}

		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		protected void SetHtmlText(string text)
		{

			if (text == null) 
				text = String.Empty;

			if (!IsHandleCreated) 
				return;

			if (ocx != null) {
				
				IHTMLDocument2 document = ocx.Document as IHTMLDocument2;
				if (document != null) {

					CheckAndActivate();
					// this way we can provide the FULL HTML incl. <head><style> etc.
					try {
						document.Open("", null, null, null);
						object[] a = new object[]{text};
						document.Write(a);
						document.Close();
						//_document = document;
						// Without the "Refresh" command, the browser control doesn't
						// react to links containing the # character in the hyperlink.
						document.ExecCommand("Refresh", false, null);
					} catch {}
				}
			}
		}

		// called from SelfNavigateComplete()
		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		protected void SetBodyText(string text)
		{
			
			if (text == null) 
				text = String.Empty;

			if (!IsHandleCreated) 
				return;

			if (ocx != null) {
				IHTMLDocument2 document = ocx.Document as IHTMLDocument2;
				if (document != null) {
					//_document = document;
					IHTMLElement e = document.GetBody();
					if (e != null) {
						CheckAndActivate();
						e.SetInnerHTML(text);
						return;
					}
				}
			}
		}
		/// <summary>
		/// Returns the MSHTML document's inner HTML content
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public string DocumentInnerHTML {
			get { 
				string content = String.Empty;
				if (ocx == null)
					return content; 

				IHTMLDocument3 document3 = ocx.Document as IHTMLDocument3;
				if (document3 == null || document3.documentElement() == null)
					return content; 

				content = document3.documentElement().GetInnerHTML();
				return content;
			}
		}

		/// <summary>
		/// Returns the MSHTML document's outer HTML content
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public string DocumentOuterHTML {
			get { 
				string content = String.Empty;
				if (ocx == null)
					return content; 

				IHTMLDocument3 document3 = ocx.Document as IHTMLDocument3;
				if (document3 == null || document3.documentElement() == null)
					return content; 

				content = document3.documentElement().GetOuterHTML();
				return content;
			}
		}

		/// <summary>
		/// Returns the MSHTML document's inner Text content
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public string DocumentInnerText {
			get { 
				string content = String.Empty;
				if (ocx == null)
					return content; 

				IHTMLDocument3 document3 = ocx.Document as IHTMLDocument3;
				if (document3 == null || document3.documentElement() == null)
					return content; 

				content = document3.documentElement().GetInnerText();
				return content;
			}
		}
		/// <summary>
		/// Returns the MSHTML document's outer Text content
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public string DocumentOuterText {
			get { 
				string content = String.Empty;
				if (ocx == null)
					return content; 

				IHTMLDocument3 document3 = ocx.Document as IHTMLDocument3;
				if (document3 == null || document3.documentElement() == null)
					return content; 

				content = document3.documentElement().GetOuterText();
				return content;
			}
		}

		#region unwanted 2
		//		///<summary>
		//		///Gets a value of bool.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(204)]
		//        public virtual bool TopLevelContainer {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("TopLevelContainer", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.TopLevelContainer;
		//            }
		//        }
        
		//		///<summary>
		//		///Gets a value of string.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(205)]
		//        public virtual string Type {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("Type", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.Type;
		//            }
		//        }
		//		///<summary>
		//		///Gets or sets a value of int.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(206)]
		//        public virtual int CtlLeft {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("CtlLeft", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.Left;
		//            }
		//            set {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("CtlLeft", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertySet);
		//                }
		//                this.ocx.Left = value;
		//            }
		//        }
		//        
		//		///<summary>
		//		///Gets or sets a value of int.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(207)]
		//        public virtual int CtlTop {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("CtlTop", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.Top;
		//            }
		//            set {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("CtlTop", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertySet);
		//                }
		//                this.ocx.Top = value;
		//            }
		//        }
		//        
		//		///<summary>
		//		///Gets or sets a value of int.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(208)]
		//        public virtual int CtlWidth {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("CtlWidth", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.Width;
		//            }
		//            set {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("CtlWidth", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertySet);
		//                }
		//                this.ocx.Width = value;
		//            }
		//        }
		//        
		//		///<summary>
		//		///Gets or sets a value of int.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(209)]
		//        public virtual int CtlHeight {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("CtlHeight", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.Height;
		//            }
		//            set {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("CtlHeight", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertySet);
		//                }
		//                this.ocx.Height = value;
		//            }
		//        }
        
		//		///<summary>
		//		///Gets a value of string.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(210)]
		//        public virtual string LocationName {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("LocationName", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.LocationName;
		//            }
		//        }
		#endregion
        
		///<summary>
		///Gets a value of string.
		///</summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[DispIdAttribute(211)]
		public virtual string LocationURL {
			get
			{
				return (this.ocx == null) ? null : this.ocx.LocationURL;
			}
		}
        
		///<summary>
		///Gets a value of bool.
		///</summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[DispIdAttribute(212)]
		public virtual bool Busy {
			get
			{
				return (this.ocx != null) && this.ocx.Busy;
			}
		}
        
		//		///<summary>
		//		///Gets a value of string.
		//		///</summary>
		//        [System.ComponentModel.Browsable(true)]
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(0)]
		//        public new virtual string Name {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("Name", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.Name;
		//            }
		//        }
        
		///<summary>
		///Gets a value of int.
		///</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DispIdAttribute(-515)]
		[ComAliasNameAttribute("System.Int32")]
		public virtual int HWND {
			get
			{
				return (this.ocx == null) ? 0 : (this.ocx.HWND);
			}
		}
        
		#region unwanted 3
		//		///<summary>
		//		///Gets a value of string.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(400)]
		//        public virtual string FullName {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("FullName", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.FullName;
		//            }
		//        }
		//        
		//		///<summary>
		//		///Gets a value of string.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(401)]
		//        public virtual string Path {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("Path", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.Path;
		//            }
		//        }
		//        
		//		///<summary>
		//		///Gets or sets a value of bool.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(402)]
		//        public virtual bool CtlVisible {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("CtlVisible", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.Visible;
		//            }
		//            set {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("CtlVisible", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertySet);
		//                }
		//                this.ocx.Visible = value;
		//            }
		//        }
        
		//		///<summary>
		//		///Gets or sets a value of bool.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(403)]
		//        public virtual bool StatusBar {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("StatusBar", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.StatusBar;
		//            }
		//            set {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("StatusBar", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertySet);
		//                }
		//                this.ocx.StatusBar = value;
		//            }
		//        }
		//        
		//		///<summary>
		//		///Gets or sets a value of string.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(404)]
		//        public virtual string StatusText {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("StatusText", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.StatusText;
		//            }
		//            set {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("StatusText", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertySet);
		//                }
		//                this.ocx.StatusText = value;
		//            }
		//        }
		//        
		//		///<summary>
		//		///Gets or sets a value of int.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(405)]
		//        public virtual int ToolBar {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("ToolBar", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.ToolBar;
		//            }
		//            set {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("ToolBar", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertySet);
		//                }
		//                this.ocx.ToolBar = value;
		//            }
		//        }
		//        
		//		///<summary>
		//		///Gets or sets a value of bool.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(406)]
		//        public virtual bool MenuBar {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("MenuBar", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.MenuBar;
		//            }
		//            set {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("MenuBar", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertySet);
		//                }
		//                this.ocx.MenuBar = value;
		//            }
		//        }
		//        
		//		///<summary>
		//		///Gets or sets a value of bool.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(407)]
		//        public virtual bool FullScreen {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("FullScreen", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.FullScreen;
		//            }
		//            set {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("FullScreen", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertySet);
		//                }
		//                this.ocx.FullScreen = value;
		//            }
		//        }
		
		#endregion

		///<summary>
		///Gets a value of SHDocVw.tagREADYSTATE.
		///</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DispIdAttribute(-525)]
		[Bindable(BindableSupport.Yes)]
		public virtual tagREADYSTATE ReadyState {
			get
			{
				return (this.ocx == null) ? tagREADYSTATE.READYSTATE_UNINITIALIZED : this.ocx.ReadyState;
			}
		}

		#region unwanted 4        
		//		///<summary>
		//		///Gets or sets a value of bool.
		//		///</summary>
		//		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//		[Browsable(false)]
		//		[System.Runtime.InteropServices.DispIdAttribute(550)]
		//		public virtual bool Offline {
		//			get {
		//				if ((this.ocx == null)) {
		//					throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("Offline", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//				}
		//				return this.ocx.Offline;
		//			}
		//			set {
		//				if ((this.ocx == null)) {
		//					throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("Offline", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertySet);
		//				}
		//				this.ocx.Offline = value;
		//			}
		//		}
		//		///<summary>
		//		///Gets or sets a value of bool.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(551)]
		//        public virtual bool Silent {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("Silent", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.Silent;
		//            }
		//            set {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("Silent", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertySet);
		//                }
		//                this.ocx.Silent = value;
		//            }
		//        }
		//        
		//		///<summary>
		//		///Gets or sets a value of bool.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(552)]
		//        public virtual bool RegisterAsBrowser {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("RegisterAsBrowser", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.RegisterAsBrowser;
		//            }
		//            set {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("RegisterAsBrowser", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertySet);
		//                }
		//                this.ocx.RegisterAsBrowser = value;
		//            }
		//        }
        
		//		///<summary>
		//		///Gets or sets a value of bool.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(553)]
		//        public virtual bool RegisterAsDropTarget {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("RegisterAsDropTarget", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.RegisterAsDropTarget;
		//            }
		//            set {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("RegisterAsDropTarget", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertySet);
		//                }
		//                this.ocx.RegisterAsDropTarget = value;
		//            }
		//        }
		//        
		//		///<summary>
		//		///Gets or sets a value of bool.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(554)]
		//        public virtual bool TheaterMode {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("TheaterMode", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.TheaterMode;
		//            }
		//            set {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("TheaterMode", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertySet);
		//                }
		//                this.ocx.TheaterMode = value;
		//            }
		//        }
		//        
		//		///<summary>
		//		///Gets or sets a value of bool.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(555)]
		//        public virtual bool AddressBar {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("AddressBar", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.AddressBar;
		//            }
		//            set {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("AddressBar", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertySet);
		//                }
		//                this.ocx.AddressBar = value;
		//            }
		//        }
		//        
		//		///<summary>
		//		///Gets or sets a value of bool.
		//		///</summary>
		//        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		//        [System.Runtime.InteropServices.DispIdAttribute(556)]
		//        public virtual bool Resizable {
		//            get {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("Resizable", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertyGet);
		//                }
		//                return this.ocx.Resizable;
		//            }
		//            set {
		//                if ((this.ocx == null)) {
		//                    throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("Resizable", System.Windows.Forms.AxHost.ActiveXInvokeKind.PropertySet);
		//                }
		//                this.ocx.Resizable = value;
		//            }
		//        }
		//		///<summary>
		//		///Summary of ShowBrowserBar.
		//		///</summary>
		//		///<param name="pvaClsid"></param>
		//		///<param name="pvarShow"></param>
		//		///<param name="pvarSize"></param>
		//        public virtual void ShowBrowserBar(ref object pvaClsid, [System.Runtime.InteropServices.Optional()] ref object pvarShow, [System.Runtime.InteropServices.Optional()] ref object pvarSize) {
		//            if ((this.ocx == null)) {
		//                throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("ShowBrowserBar", System.Windows.Forms.AxHost.ActiveXInvokeKind.MethodInvoke);
		//            }
		//            this.ocx.ShowBrowserBar(ref pvaClsid, ref pvarShow, ref pvarSize);
		//        }
		#endregion
        
		///<summary>
		///Summary of ExecWB.
		///</summary>
		///<param name="cmdID"></param>
		///<param name="cmdexecopt"></param>
		///<param name="pvaIn"></param>
		///<param name="pvaOut"></param>
		public virtual void ExecWB(OLECMDID cmdID, OLECMDEXECOPT cmdexecopt, [Optional] ref object pvaIn, [Optional] ref object pvaOut) {
			if ((this.ocx == null)) {
				return;
			}
			this.ocx.ExecWB(cmdID, cmdexecopt, ref pvaIn, ref pvaOut);
		}
        
		///<summary>
		///Summary of QueryStatusWB.
		///</summary>
		///<param name="cmdID"></param>
		///<returns></returns>	
		public virtual OLECMDF QueryStatusWB(OLECMDID cmdID) {
			if ((this.ocx == null)) {
				throw new InvalidActiveXStateException("QueryStatusWB", ActiveXInvokeKind.MethodInvoke);
			}
			return this.ocx.QueryStatusWB(cmdID);
		}
        
        
		///<summary>
		///Summary of GetProperty.
		///</summary>
		///<param name="property"></param>
		///<returns></returns>	
		public virtual object GetProperty(string property)
		{
			return (this.ocx == null) ? null : this.ocx.GetProperty(property);
		}

		///<summary>
		///Summary of PutProperty.
		///</summary>
		///<param name="property"></param>
		///<param name="vtValue"></param>
		public virtual void PutProperty(string property, object vtValue) {
			if ((this.ocx == null)) {
				return;
			}
			this.ocx.PutProperty(property, vtValue);
		}
		        
		///<summary>
		///Summary of ClientToWindow.
		///</summary>
		///<param name="pcx"></param>
		///<param name="pcy"></param>
		public virtual void ClientToWindow(ref int pcx, ref int pcy) {
			if ((this.ocx == null)) {
				return;
			}
			this.ocx.ClientToWindow(ref pcx, ref pcy);
		}
		//        
		//		///<summary>
		//		///Summary of Quit.
		//		///</summary>
		//        public virtual void Quit() {
		//            if ((this.ocx == null)) {
		//                throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("Quit", System.Windows.Forms.AxHost.ActiveXInvokeKind.MethodInvoke);
		//            }
		//            this.ocx.Quit();
		//        }
        
		///<summary>
		///Summary of Stop.
		///</summary>
		public virtual void Stop() {
			if ((this.ocx == null)) {
				return;
			}
			this.ocx.Stop();
		}
        
		///<summary>
		///Summary of Refresh2.
		///</summary>
		///<param name="level"></param>
		public virtual void Refresh2([Optional] ref object level) {
			if ((this.ocx == null)) {
				return;
			}
			this.ocx.Refresh2(ref level);
		}
        
		//		///<summary>
		//		///Summary of CtlRefresh.
		//		///</summary>
		//        public virtual void CtlRefresh() {
		//            if ((this.ocx == null)) {
		//                throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("CtlRefresh", System.Windows.Forms.AxHost.ActiveXInvokeKind.MethodInvoke);
		//            }
		//            this.ocx.Refresh();
		//        }
		//        
		
		///<summary>
		///Preferred method to Navigate to a url.
		///</summary>
		///<param name="location"></param>
		public virtual void Navigate( string location ) {
			
			if (location == null || location.Length == 0)
				location = "about:blank";

			object flags		= null;
			object targetFrame	= null;
			object postData	= null;
			object headers		= null;

			//Navigate2( ref url, ref flags, ref targetFrame, ref postData, ref headers );
			Navigate( location, ref flags, ref targetFrame, ref postData, ref headers );
		}

		///<summary>
		///Summary of Navigate.
		///</summary>
		///<param name="uRL"></param>
		///<param name="flags"></param>
		///<param name="targetFrameName"></param>
		///<param name="postData"></param>
		///<param name="headers"></param>
		public virtual void Navigate(string uRL, [Optional] ref object flags, [Optional] ref object targetFrameName, [Optional] ref object postData, [Optional] ref object headers) {
			if ((this.ocx == null)) {
				return;
			}
			try {
				this.ocx.Navigate(uRL, ref flags, ref targetFrameName, ref postData, ref headers);
			} catch {}
		}

		///<summary>
		///Summary of Navigate2.
		///</summary>
		///<param name="uRL"></param>
		///<param name="flags"></param>
		///<param name="targetFrameName"></param>
		///<param name="postData"></param>
		///<param name="headers"></param>
		public virtual void Navigate2(ref object uRL, [Optional] ref object flags, [Optional] ref object targetFrameName, [Optional] ref object postData, [Optional] ref object headers) {
			if ((this.ocx == null)) {
				return;
			}
			try {
				this.ocx.Navigate2(ref uRL, ref flags, ref targetFrameName, ref postData, ref headers);
			} catch {}
		}

		//        
		//		///<summary>
		//		///Summary of GoSearch.
		//		///</summary>
		//        public virtual void GoSearch() {
		//            if ((this.ocx == null)) {
		//                throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("GoSearch", System.Windows.Forms.AxHost.ActiveXInvokeKind.MethodInvoke);
		//            }
		//            this.ocx.GoSearch();
		//        }
		//        
		//		///<summary>
		//		///Summary of GoHome.
		//		///</summary>
		//        public virtual void GoHome() {
		//            if ((this.ocx == null)) {
		//                throw new System.Windows.Forms.AxHost.InvalidActiveXStateException("GoHome", System.Windows.Forms.AxHost.ActiveXInvokeKind.MethodInvoke);
		//            }
		//            this.ocx.GoHome();
		//        }
		//        
		///<summary>
		///Summary of GoForward.
		///</summary>
		public virtual void GoForward() {
			if ((this.ocx == null)) {
				return;
			}
			this.ocx.GoForward();
		}
        
		///<summary>
		///Summary of GoBack.
		///</summary>
		public virtual void GoBack() {
			if ((this.ocx == null)) {
				return;
			}
			this.ocx.GoBack();
		}
		
		#region shell helper functions
		/// <summary>
		/// Display the AddFavorite dialog
		/// </summary>
		/// <exception cref="UriFormatException">If favUrl is a ivalid favUrl</exception>
		/// <exception cref="ArgumentNullException">If any argument is null</exception>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public void ShowDialogAddFavorite(string favUrl, string title) {
			if (favUrl == null)
				throw new ArgumentNullException("favUrl");
			if (title == null)
				throw new ArgumentNullException("title");
			
			if (favUrl.IndexOf("://", StringComparison.Ordinal) == -1)
				favUrl = "http://" + favUrl;

			Uri uri = new Uri(favUrl);
			object oTitle = title;

			ShellUIHelperClass uih = new ShellUIHelperClass();
			try {
				uih.AddFavorite(uri.AbsoluteUri, ref oTitle);
			} catch (Exception ex) {
				System.Diagnostics.Trace.WriteLine("IEControl::ShellUIHelperClass.AddFavorite() exception - " + ex.Message);
			} 

			
		}
		/// <summary>
		/// Display the Print Preview dialog
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public void ShowDialogPrintPreview() {
			try {
				object o = null;
				this.ExecWB(OLECMDID.OLECMDID_PRINTPREVIEW,
					OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, ref o, ref o);
			} catch (Exception ex) {
				System.Diagnostics.Trace.WriteLine("IEControl::ExecWB(OLECMDID_PRINTPREVIEW) exception - " + ex.Message);
			} 
		}
		
		/// <summary>
		/// Display the Printer dialog
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public void ShowDialogPrint() {
			try {
				object o = null;
				this.ExecWB(OLECMDID.OLECMDID_PRINT,
					OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, ref o, ref o);
			} catch (Exception ex) {
				System.Diagnostics.Trace.WriteLine("IEControl::ExecWB(OLECMDID_PRINT) exception - " + ex.Message);
			} 
		}

		/// <summary>
		/// Immediatly print the document
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public void Print() {
			try {
				object o = null;
				this.ExecWB(OLECMDID.OLECMDID_PRINT,
					OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER, ref o, ref o);
			} catch (Exception ex) {
				System.Diagnostics.Trace.WriteLine("IEControl::ExecWB(OLECMDID_PRINT) exception - " + ex.Message);
			} 
		}

		#endregion

		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		public new bool Focus()
		{
			DoVerb(Interop.OLEIVERB_UIACTIVATE);
			if (ocx != null && ocx.Document != null) {
				IHTMLDocument2 doc = ocx.Document as IHTMLDocument2;
				if (doc != null) {
					IHTMLElement2 e = doc.GetBody();
					if (e != null) {
						e.focus();
						return true;
					}
				}
			}
			return false;
		}

		private bool IsFlagSet (ControlBehaviorFlags flag) {
			return ( (this.cbFlags & flag) == flag );
		}
		private void SetFlag(ControlBehaviorFlags flag, bool theValue) {
			if (theValue) {
				this.cbFlags |= flag;
			} else {
				this.cbFlags &= ~flag;
			}
		}

		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		private void CheckAndActivate()
		{
			if (this.IsFlagSet(ControlBehaviorFlags.activate)) {
				DoVerb(Interop.OLEIVERB_UIACTIVATE);
				this.SetFlag(ControlBehaviorFlags.activate, false);
			}
		}

		///<summary>
		///Summary of CreateSink.
		///</summary>
		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		protected override void CreateSink()
		{
			try {
				this.eventMulticaster = new AxWebBrowserEventMulticaster(this);
				this.cookie = new ConnectionPointCookie(this.ocx, this.eventMulticaster, typeof(DWebBrowserEvents2));
			}	catch (Exception ex) {
				System.Diagnostics.Trace.WriteLine("IEControl::CreateSink() exception - " + ex.Message);
			}
		}
        
		///<summary>
		///Summary of DetachSink.
		///</summary>
		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		protected override void DetachSink()
		{
			try {
				this.cookie.Disconnect();
			}	catch (Exception ex) {
				System.Diagnostics.Trace.WriteLine("IEControl::DetachSink() exception - " + ex.Message);
			}
		}
        
		///<summary>
		///Summary of AttachInterfaces.
		///</summary>
		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		protected override void AttachInterfaces() {
			try {
				this.ocx = ((IWebBrowser2)(GetOcx()));
				this.iwb2app = (Interop.IWebBrowser2Application) this.ocx;
			} catch (Exception ex) {
				System.Diagnostics.Trace.WriteLine("IEControl::AttachInterfaces() exception - " + ex.Message);
			}
		}

		private void SynchAmbientPropertyChange()
		{
			if (DesignMode)
				return;
			try
			{
				//Signal change of ambient properties
				//so MSHTML call our Invoke method through Dispatch
				//Otherwise refreshing the page will have no effect
				//MSHTML does not know of new flags set by us
				//QI for IOleControl
				Interop.IOleControl oleCtrl = ocx as Interop.IOleControl;
				if (oleCtrl != null)
					oleCtrl.OnAmbientPropertyChange(HTMLDispIDs.DISPID_AMBIENT_DLCONTROL);
			}
			finally
			{
			}
		}

		internal void RaiseOnNewBrowserWindow(string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Processed) {
			Processed = false;
			if (this.NewWindow != null) {
				BrowserNewWindowEvent newwindowEvent = new BrowserNewWindowEvent(URL, Processed);
				this.NewWindow(this, newwindowEvent);
				Processed = newwindowEvent.Cancel;
			}
		}


		///<summary>
		///Summary of RaiseOnPrivacyImpactedStateChange.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnPrivacyImpactedStateChange(object sender, BrowserPrivacyImpactedStateChangeEvent e) {
			if ((this.PrivacyImpactedStateChange != null)) {
				this.PrivacyImpactedStateChange(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnUpdatePageStatus.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnUpdatePageStatus(object sender, BrowserUpdatePageStatusEvent e) {
			if ((this.UpdatePageStatus != null)) {
				this.UpdatePageStatus(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnPrintTemplateTeardown.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnPrintTemplateTeardown(object sender, BrowserPrintTemplateTeardownEvent e) {
			if ((this.PrintTemplateTeardown != null)) {
				this.PrintTemplateTeardown(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnPrintTemplateInstantiation.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnPrintTemplateInstantiation(object sender, BrowserPrintTemplateInstantiationEvent e) {
			if ((this.PrintTemplateInstantiation != null)) {
				this.PrintTemplateInstantiation(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnNavigateError.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnNavigateError(object sender, BrowserNavigateErrorEvent e) {
			if ((this.NavigateError != null)) {
				this.NavigateError(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnFileDownload.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnFileDownload(object sender, BrowserFileDownloadEvent e) {
			if ((this.FileDownload != null)) {
				this.FileDownload(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnSetSecureLockIcon.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnSetSecureLockIcon(object sender, BrowserSetSecureLockIconEvent e) {
			if ((this.SetSecureLockIcon != null)) {
				this.SetSecureLockIcon(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnClientToHostWindow.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnClientToHostWindow(object sender, BrowserClientToHostWindowEvent e) {
			if ((this.ClientToHostWindow != null)) {
				this.ClientToHostWindow(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnWindowClosing.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnWindowClosing(object sender, BrowserWindowClosingEvent e) {
			if ((this.WindowClosing != null)) {
				this.WindowClosing(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnWindowSetHeight.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnWindowSetHeight(object sender, BrowserWindowSetHeightEvent e) {
			if ((this.WindowSetHeight != null)) {
				this.WindowSetHeight(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnWindowSetWidth.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnWindowSetWidth(object sender, BrowserWindowSetWidthEvent e) {
			if ((this.WindowSetWidth != null)) {
				this.WindowSetWidth(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnWindowSetTop.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnWindowSetTop(object sender, BrowserWindowSetTopEvent e) {
			if ((this.WindowSetTop != null)) {
				this.WindowSetTop(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnWindowSetLeft.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnWindowSetLeft(object sender, BrowserWindowSetLeftEvent e) {
			if ((this.WindowSetLeft != null)) {
				this.WindowSetLeft(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnWindowSetResizable.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnWindowSetResizable(object sender, BrowserWindowSetResizableEvent e) {
			if ((this.WindowSetResizable != null)) {
				this.WindowSetResizable(sender, e);
			}
		}
        
		#region unwanted/not needed internals
		//		///<summary>
		//		///Summary of RaiseOnOnTheaterMode.
		//		///</summary>
		//		///<param name="sender"></param>
		//		///<param name="e"></param>
		//		internal void RaiseOnOnTheaterMode(object sender, BrowserOnTheaterModeEvent e) {
		//			if ((this.OnTheaterMode != null)) {
		//				this.OnTheaterMode(sender, e);
		//			}
		//		}
		//        
		//		///<summary>
		//		///Summary of RaiseOnOnFullScreen.
		//		///</summary>
		//		///<param name="sender"></param>
		//		///<param name="e"></param>
		//		internal void RaiseOnOnFullScreen(object sender, BrowserOnFullScreenEvent e) {
		//			if ((this.OnFullScreen != null)) {
		//				this.OnFullScreen(sender, e);
		//			}
		//		}
		//        
		//		///<summary>
		//		///Summary of RaiseOnOnStatusBar.
		//		///</summary>
		//		///<param name="sender"></param>
		//		///<param name="e"></param>
		//		internal void RaiseOnOnStatusBar(object sender, BrowserOnStatusBarEvent e) {
		//			if ((this.OnStatusBar != null)) {
		//				this.OnStatusBar(sender, e);
		//			}
		//		}
		//        
		//		///<summary>
		//		///Summary of RaiseOnOnMenuBar.
		//		///</summary>
		//		///<param name="sender"></param>
		//		///<param name="e"></param>
		//		internal void RaiseOnOnMenuBar(object sender, BrowserOnMenuBarEvent e) {
		//			if ((this.OnMenuBar != null)) {
		//				this.OnMenuBar(sender, e);
		//			}
		//		}
		//        
		//		///<summary>
		//		///Summary of RaiseOnOnToolBar.
		//		///</summary>
		//		///<param name="sender"></param>
		//		///<param name="e"></param>
		//		internal void RaiseOnOnToolBar(object sender, BrowserOnToolBarEvent e) {
		//			if ((this.OnToolBar != null)) {
		//				this.OnToolBar(sender, e);
		//			}
		//		}
		#endregion

		///<summary>
		///Summary of RaiseOnVisible.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnVisible(object sender, BrowserOnVisibleEvent e) {
			if ((this.OnVisible != null)) {
				this.OnVisible(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnOnQuit.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal bool RaiseOnQuit(object sender, EventArgs e) {
			if ((this.OnQuit != null)) {
				this.OnQuit(sender, e);
				return true;
			}
			return false;
		}
        
		///<summary>
		///Summary of RaiseOnDocumentComplete.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnDocumentComplete(object sender, BrowserDocumentCompleteEvent e) {
			if ((this.DocumentComplete != null)) {
				this.DocumentComplete(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnNavigateComplete2.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnNavigateComplete2(object sender, BrowserNavigateComplete2Event e) {
			if ((this.NavigateComplete != null)) {
				this.NavigateComplete(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnNewWindow2.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnNewWindow2(object sender, BrowserNewWindow2Event e) {
			if ((this.NewWindow2 != null)) {
				this.NewWindow2(sender, e);
			}
		}

		///<summary>
		///Summary of RaiseOnNewWindow3.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnNewWindow3(object sender, BrowserNewWindow3Event e)
		{
			if ((this.NewWindow3 != null))
			{
				this.NewWindow3(sender, e);
			}
		}
		///<summary>
		///Summary of RaiseOnBeforeNavigate2.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnBeforeNavigate2(object sender, BrowserBeforeNavigate2Event e) {
			if ((this.BeforeNavigate != null)) {
				this.BeforeNavigate(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnPropertyChange.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnPropertyChange(object sender, BrowserPropertyChangeEvent e) {
			if ((this.PropertyChanged != null)) {
				this.PropertyChanged(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnTitleChange.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnTitleChange(object sender, BrowserTitleChangeEvent e) {
			if ((this.TitleChanged != null)) {
				this.TitleChanged(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnDownloadComplete.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnDownloadComplete(object sender, EventArgs e) {
			if ((this.DownloadCompleted != null)) {
				this.DownloadCompleted(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnDownloadBegin.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnDownloadBegin(object sender, EventArgs e) {
			if ((this.DownloadBegin != null)) {
				this.DownloadBegin(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnCommandStateChange.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnCommandStateChange(object sender, BrowserCommandStateChangeEvent e) {
			if ((this.CommandStateChanged != null)) {
				this.CommandStateChanged(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnProgressChange.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnProgressChange(object sender, BrowserProgressChangeEvent e) {
			if ((this.ProgressChanged != null)) {
				this.ProgressChanged(sender, e);
			}
		}
        
		///<summary>
		///Summary of RaiseOnStatusTextChange.
		///</summary>
		///<param name="sender"></param>
		///<param name="e"></param>
		internal void RaiseOnStatusTextChange(object sender, BrowserStatusTextChangeEvent e) {
			if ((this.StatusTextChanged != null)) {
				this.StatusTextChanged(sender, e);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="e"></param>
		internal void RaiseOnShowContextMenu(BrowserContextMenuCancelEventArgs e) {
			if (ShowContextMenu != null)
				ShowContextMenu(this, e);
		}

		/// <summary>
		/// </summary>
		/// <param name="e"></param>
		internal void RaiseOnTranslateUrl(BrowserTranslateUrlEventArgs e) {
			if (TranslateUrl != null)
				TranslateUrl(this, e);
		}

		/// <summary>
		/// </summary>
		/// <param name="e"></param>
		internal void RaiseOnTranslateAccelerator(KeyEventArgs e) {
			if (TranslateAccelerator!= null)
				TranslateAccelerator(this, e);
		}


        #region IMessageFilter Members

        
        bool IsMessageForMe(Message m)
        {
            
            IntPtr hwnd;

            Interop.IOleWindow oleWindow = null;
            try
            {
                oleWindow = this.Document2 as Interop.IOleWindow;
            }
            catch { }

            if (oleWindow == null)
                return false;

            oleWindow.GetWindow(out hwnd);

            if (m.HWnd == hwnd)
            {
                return true;
            }
            return false;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        bool IMessageFilter.PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case Interop.WM_MOUSEWHEEL:
                {
                    if (ModifierKeys != Keys.None)
                    {
                        if (ModifierKeys == Keys.Control && CurrentIEVersion.Major >= 7
                            && IsMessageForMe(m))
                        {
                            int delta = Interop.SignedHIWORD(m.WParam);
                            if (delta > 0)
                            {
                                OpticalZoomFactor += zoomStep;
                            }
                            else
                            {
                                OpticalZoomFactor -= zoomStep;
                            }
                            
                            return true;
                        }
                    }

                    break;
                }
            }
            return false;
        }

        #endregion
    }

	///<summary>
	///Summary of AxWebBrowserEventMulticaster
	///</summary>
	[CLSCompliant(false)]
	[ComVisible(false)]
	public class AxWebBrowserEventMulticaster : DWebBrowserEvents2 {
        
		private readonly HtmlControl parent;
        
		///<summary>
		///Summary of AxWebBrowserEventMulticaster.
		///</summary>
		///<param name="parent"></param>
		public AxWebBrowserEventMulticaster(HtmlControl parent) {
			this.parent = parent;
		}
        
		///<summary>
		///Summary of PrivacyImpactedStateChange.
		///</summary>
		///<param name="bImpacted"></param>
		public virtual void PrivacyImpactedStateChange(bool bImpacted) {
			BrowserPrivacyImpactedStateChangeEvent privacyimpactedstatechangeEvent = new BrowserPrivacyImpactedStateChangeEvent(bImpacted);
			this.parent.RaiseOnPrivacyImpactedStateChange(this.parent, privacyimpactedstatechangeEvent);
		}

		///<summary>
		///Summary of UpdatePageStatus.
		///</summary>
		///<param name="pDisp"></param>
		///<param name="nPage"></param>
		///<param name="fDone"></param>
		public virtual void UpdatePageStatus(object pDisp, ref object nPage, ref object fDone) {
			BrowserUpdatePageStatusEvent updatepagestatusEvent = new BrowserUpdatePageStatusEvent(pDisp, nPage, fDone);
			this.parent.RaiseOnUpdatePageStatus(this.parent, updatepagestatusEvent);
			nPage = updatepagestatusEvent.nPage;
			fDone = updatepagestatusEvent.fDone;
		}
        
		///<summary>
		///Summary of PrintTemplateTeardown.
		///</summary>
		///<param name="pDisp"></param>
		public virtual void PrintTemplateTeardown(object pDisp) {
			BrowserPrintTemplateTeardownEvent printtemplateteardownEvent = new BrowserPrintTemplateTeardownEvent(pDisp);
			this.parent.RaiseOnPrintTemplateTeardown(this.parent, printtemplateteardownEvent);
		}
        
		///<summary>
		///Summary of PrintTemplateInstantiation.
		///</summary>
		///<param name="pDisp"></param>
		public virtual void PrintTemplateInstantiation(object pDisp) {
			BrowserPrintTemplateInstantiationEvent printtemplateinstantiationEvent = new BrowserPrintTemplateInstantiationEvent(pDisp);
			this.parent.RaiseOnPrintTemplateInstantiation(this.parent, printtemplateinstantiationEvent);
		}
        
		///<summary>
		///Summary of NavigateError.
		///</summary>
		///<param name="pDisp"></param>
		///<param name="uRL"></param>
		///<param name="frame"></param>
		///<param name="statusCode"></param>
		///<param name="cancel"></param>
		public virtual void NavigateError(object pDisp, ref object uRL, ref object frame, ref object statusCode, ref bool cancel) {
			BrowserNavigateErrorEvent navigateerrorEvent = new BrowserNavigateErrorEvent(pDisp, uRL, frame, statusCode, cancel);
			this.parent.RaiseOnNavigateError(this.parent, navigateerrorEvent);
			uRL = navigateerrorEvent.uRL;
			frame = navigateerrorEvent.frame;
			statusCode = navigateerrorEvent.statusCode;
			cancel = navigateerrorEvent.Cancel;
		}
        
		///<summary>
		///Summary of FileDownload.
		///</summary>
		///<param name="cancel"></param>
		public virtual void FileDownload(ref bool cancel) {
			BrowserFileDownloadEvent filedownloadEvent = new BrowserFileDownloadEvent(cancel);
			this.parent.RaiseOnFileDownload(this.parent, filedownloadEvent);
			cancel = filedownloadEvent.Cancel;
		}
        
		///<summary>
		///Summary of SetSecureLockIcon.
		///</summary>
		///<param name="secureLockIcon"></param>
		public virtual void SetSecureLockIcon(int secureLockIcon) {
			BrowserSetSecureLockIconEvent setsecurelockiconEvent = new BrowserSetSecureLockIconEvent(secureLockIcon);
			this.parent.RaiseOnSetSecureLockIcon(this.parent, setsecurelockiconEvent);
		}
        
		///<summary>
		///Summary of ClientToHostWindow.
		///</summary>
		///<param name="cX"></param>
		///<param name="cY"></param>
		public virtual void ClientToHostWindow(ref int cX, ref int cY) {
			BrowserClientToHostWindowEvent clienttohostwindowEvent = new BrowserClientToHostWindowEvent(cX, cY);
			this.parent.RaiseOnClientToHostWindow(this.parent, clienttohostwindowEvent);
			cX = clienttohostwindowEvent.cX;
			cY = clienttohostwindowEvent.cY;
		}
        
		///<summary>
		///Summary of WindowClosing.
		///</summary>
		///<param name="isChildWindow"></param>
		///<param name="cancel"></param>
		public virtual void WindowClosing(bool isChildWindow, ref bool cancel) {
			BrowserWindowClosingEvent windowclosingEvent = new BrowserWindowClosingEvent(isChildWindow, cancel);
			this.parent.RaiseOnWindowClosing(this.parent, windowclosingEvent);
			cancel = windowclosingEvent.Cancel;
		}
        
		///<summary>
		///Summary of WindowSetHeight.
		///</summary>
		///<param name="height"></param>
		public virtual void WindowSetHeight(int height) {
			BrowserWindowSetHeightEvent windowsetheightEvent = new BrowserWindowSetHeightEvent(height);
			this.parent.RaiseOnWindowSetHeight(this.parent, windowsetheightEvent);
		}
        
		///<summary>
		///Summary of WindowSetWidth.
		///</summary>
		///<param name="width"></param>
		public virtual void WindowSetWidth(int width) {
			BrowserWindowSetWidthEvent windowsetwidthEvent = new BrowserWindowSetWidthEvent(width);
			this.parent.RaiseOnWindowSetWidth(this.parent, windowsetwidthEvent);
		}
        
		///<summary>
		///Summary of WindowSetTop.
		///</summary>
		///<param name="top"></param>
		public virtual void WindowSetTop(int top) {
			BrowserWindowSetTopEvent windowsettopEvent = new BrowserWindowSetTopEvent(top);
			this.parent.RaiseOnWindowSetTop(this.parent, windowsettopEvent);
		}
        
		///<summary>
		///Summary of WindowSetLeft.
		///</summary>
		///<param name="left"></param>
		public virtual void WindowSetLeft(int left) {
			BrowserWindowSetLeftEvent windowsetleftEvent = new BrowserWindowSetLeftEvent(left);
			this.parent.RaiseOnWindowSetLeft(this.parent, windowsetleftEvent);
		}
        
		///<summary>
		///Summary of WindowSetResizable.
		///</summary>
		///<param name="resizable"></param>
		public virtual void WindowSetResizable(bool resizable) {
			BrowserWindowSetResizableEvent windowsetresizableEvent = new BrowserWindowSetResizableEvent(resizable);
			this.parent.RaiseOnWindowSetResizable(this.parent, windowsetresizableEvent);
		}
        
		///<summary>
		///Summary of OnTheaterMode.
		///</summary>
		///<param name="theaterMode"></param>
		public virtual void OnTheaterMode(bool theaterMode) {
			//			BrowserOnTheaterModeEvent ontheatermodeEvent = new BrowserOnTheaterModeEvent(theaterMode);
			//			this.parent.RaiseOnOnTheaterMode(this.parent, ontheatermodeEvent);
		}
        
		///<summary>
		///Summary of OnFullScreen.
		///</summary>
		///<param name="fullScreen"></param>
		public virtual void OnFullScreen(bool fullScreen) {
			//			BrowserOnFullScreenEvent onfullscreenEvent = new BrowserOnFullScreenEvent(fullScreen);
			//			this.parent.RaiseOnOnFullScreen(this.parent, onfullscreenEvent);
		}
        
		///<summary>
		///Summary of OnStatusBar.
		///</summary>
		///<param name="statusBar"></param>
		public virtual void OnStatusBar(bool statusBar) {
			//			BrowserOnStatusBarEvent onstatusbarEvent = new BrowserOnStatusBarEvent(statusBar);
			//			this.parent.RaiseOnOnStatusBar(this.parent, onstatusbarEvent);
		}
        
		///<summary>
		///Summary of OnMenuBar.
		///</summary>
		///<param name="menuBar"></param>
		public virtual void OnMenuBar(bool menuBar) {
			//			BrowserOnMenuBarEvent onmenubarEvent = new BrowserOnMenuBarEvent(menuBar);
			//			this.parent.RaiseOnOnMenuBar(this.parent, onmenubarEvent);
		}
        
		///<summary>
		///Summary of OnToolBar.
		///</summary>
		///<param name="toolBar"></param>
		public virtual void OnToolBar(bool toolBar) {
			//			BrowserOnToolBarEvent ontoolbarEvent = new BrowserOnToolBarEvent(toolBar);
			//			this.parent.RaiseOnOnToolBar(this.parent, ontoolbarEvent);
		}
        
		///<summary>
		///Summary of OnVisible.
		///</summary>
		///<param name="visible"></param>
		public virtual void OnVisible(bool visible) {
			BrowserOnVisibleEvent onvisibleEvent = new BrowserOnVisibleEvent(visible);
			this.parent.RaiseOnVisible(this.parent, onvisibleEvent);
		}
        
		///<summary>
		///Summary of OnQuit.
		///</summary>
		public virtual void OnQuit() {
			if (this.parent != null)
				this.parent.RaiseOnQuit(this.parent, EventArgs.Empty);
		}
        
		///<summary>
		///Summary of DocumentComplete.
		///</summary>
		///<param name="pDisp"></param>
		///<param name="uRL"></param>
		public virtual void DocumentComplete(object pDisp, ref object uRL) {
			BrowserDocumentCompleteEvent documentcompleteEvent = new BrowserDocumentCompleteEvent(pDisp, uRL, pDisp == this.parent.ocx);
			this.parent.RaiseOnDocumentComplete(this.parent, documentcompleteEvent);
			uRL = documentcompleteEvent.url;
		}
        
		///<summary>
		///Summary of NavigateComplete2.
		///</summary>
		///<param name="pDisp"></param>
		///<param name="uRL"></param>
		public virtual void NavigateComplete2(object pDisp, ref object uRL) {
			BrowserNavigateComplete2Event navigatecomplete2Event = new BrowserNavigateComplete2Event(pDisp, uRL, pDisp == this.parent.ocx);
			this.parent.RaiseOnNavigateComplete2(this.parent, navigatecomplete2Event);
			uRL = navigatecomplete2Event.url;
		}
        
		///<summary>
		///Summary of NewWindow2.
		///</summary>
		///<param name="ppDisp"></param>
		///<param name="cancel"></param>
		public virtual void NewWindow2(ref object ppDisp, ref bool cancel) {
			BrowserNewWindow2Event newwindow2Event = new BrowserNewWindow2Event(ppDisp, cancel);
			this.parent.RaiseOnNewWindow2(this.parent, newwindow2Event);
			ppDisp = newwindow2Event.ppDisp;
			cancel = newwindow2Event.Cancel;
		}

		/// <summary>
		/// NewWindow3.
		/// </summary>
		/// <param name="ppDisp">The ppDisp.</param>
		/// <param name="cancel">if set to <c>true</c> [cancel].</param>
		/// <param name="dwFlags">The dw flags.</param>
		/// <param name="bstrUrlContext">The BSTR URL context.</param>
		/// <param name="bstrUrl">The BSTR URL.</param>
		public void NewWindow3(
			ref object ppDisp, ref bool cancel, uint dwFlags, string bstrUrlContext, string bstrUrl)
		{
			BrowserNewWindow3Event newwindow3Event = new BrowserNewWindow3Event(ppDisp, cancel, dwFlags, bstrUrlContext, bstrUrl);
			this.parent.RaiseOnNewWindow3(this.parent, newwindow3Event);
			ppDisp = newwindow3Event.ppDisp;
			cancel = newwindow3Event.Cancel;
		}

		///<summary>
		///Summary of BeforeNavigate2.
		///</summary>
		///<param name="pDisp"></param>
		///<param name="uRL"></param>
		///<param name="flags"></param>
		///<param name="targetFrameName"></param>
		///<param name="postData"></param>
		///<param name="headers"></param>
		///<param name="cancel"></param>
		public virtual void BeforeNavigate2(object pDisp, ref object uRL, ref object flags, ref object targetFrameName, ref object postData, ref object headers, ref bool cancel) {
			BrowserBeforeNavigate2Event beforenavigate2Event = new BrowserBeforeNavigate2Event(pDisp, uRL, flags, targetFrameName, postData, headers, cancel, pDisp == this.parent.ocx);
			this.parent.RaiseOnBeforeNavigate2(this.parent, beforenavigate2Event);
			uRL = beforenavigate2Event.url;
			flags = beforenavigate2Event.flags;
			targetFrameName = beforenavigate2Event.targetFrameName;
			postData = beforenavigate2Event.postData;
			headers = beforenavigate2Event.headers;
			cancel = beforenavigate2Event.Cancel;
		}
        
		///<summary>
		///Summary of PropertyChange.
		///</summary>
		///<param name="szProperty"></param>
		public virtual void PropertyChange(string szProperty) {
			BrowserPropertyChangeEvent propertychangeEvent = new BrowserPropertyChangeEvent(szProperty);
			this.parent.RaiseOnPropertyChange(this.parent, propertychangeEvent);
		}
        
		///<summary>
		///Summary of TitleChange.
		///</summary>
		///<param name="text"></param>
		public virtual void TitleChange(string text) {
			BrowserTitleChangeEvent titlechangeEvent = new BrowserTitleChangeEvent(text);
			this.parent.RaiseOnTitleChange(this.parent, titlechangeEvent);
		}
        
		///<summary>
		///Summary of DownloadComplete.
		///</summary>
		public virtual void DownloadComplete() {
			this.parent.RaiseOnDownloadComplete(this.parent, EventArgs.Empty);
		}
        
		///<summary>
		///Summary of DownloadBegin.
		///</summary>
		public virtual void DownloadBegin() {
			this.parent.RaiseOnDownloadBegin(this.parent, EventArgs.Empty);
		}
        
		///<summary>
		///Summary of CommandStateChange.
		///</summary>
		///<param name="command"></param>
		///<param name="enable"></param>
		public virtual void CommandStateChange(int command, bool enable) {
			BrowserCommandStateChangeEvent commandstatechangeEvent = new BrowserCommandStateChangeEvent(command, enable);
			this.parent.RaiseOnCommandStateChange(this.parent, commandstatechangeEvent);
		}
        
		///<summary>
		///Summary of ProgressChange.
		///</summary>
		///<param name="progress"></param>
		///<param name="progressMax"></param>
		public virtual void ProgressChange(int progress, int progressMax) {
			BrowserProgressChangeEvent progresschangeEvent = new BrowserProgressChangeEvent(progress, progressMax);
			this.parent.RaiseOnProgressChange(this.parent, progresschangeEvent);
		}
        
		///<summary>
		///Summary of StatusTextChange.
		///</summary>
		///<param name="text"></param>
		public virtual void StatusTextChange(string text) {
			BrowserStatusTextChangeEvent statustextchangeEvent = new BrowserStatusTextChangeEvent(text);
			this.parent.RaiseOnStatusTextChange(this.parent, statustextchangeEvent);
		}
	}
}
