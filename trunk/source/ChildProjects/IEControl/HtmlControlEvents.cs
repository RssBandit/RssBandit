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
using System.ComponentModel;

namespace IEControl
{
	///<summary>
	///Summary of BrowserPrivacyImpactedStateChangeEventHandler
	///</summary>
	public delegate void BrowserPrivacyImpactedStateChangeEventHandler(object sender, BrowserPrivacyImpactedStateChangeEvent e);
    
	///<summary>
	///Summary of BrowserPrivacyImpactedStateChangeEvent
	///</summary>
	public class BrowserPrivacyImpactedStateChangeEvent: EventArgs {
        
		///<summary>
		///</summary>
		public bool bImpacted;
        
		///<summary>
		///Summary of BrowserPrivacyImpactedStateChangeEvent.
		///</summary>
		///<param name="bImpacted"></param>
		public BrowserPrivacyImpactedStateChangeEvent(bool bImpacted) {
			this.bImpacted = bImpacted;
		}
	}
    
	///<summary>
	///Summary of BrowserUpdatePageStatusEventHandler
	///</summary>

	public delegate void BrowserUpdatePageStatusEventHandler(object sender, BrowserUpdatePageStatusEvent e);
    
	///<summary>
	///Summary of BrowserUpdatePageStatusEvent
	///</summary>

	public class BrowserUpdatePageStatusEvent: EventArgs {
        
		///<summary>
		///</summary>
		public object pDisp;
        
		///<summary>
		///</summary>
		public object nPage;
        
		///<summary>
		///</summary>
		public object fDone;
        
		///<summary>
		///Summary of BrowserUpdatePageStatusEvent.
		///</summary>
		///<param name="pDisp"></param>
		///<param name="nPage"></param>
		///<param name="fDone"></param>
		public BrowserUpdatePageStatusEvent(object pDisp, object nPage, object fDone) {
			this.pDisp = pDisp;
			this.nPage = nPage;
			this.fDone = fDone;
		}
	}
    
	///<summary>
	///Summary of BrowserPrintTemplateTeardownEventHandler
	///</summary>

	public delegate void BrowserPrintTemplateTeardownEventHandler(object sender, BrowserPrintTemplateTeardownEvent e);
    
	///<summary>
	///Summary of BrowserPrintTemplateTeardownEvent
	///</summary>

	public class BrowserPrintTemplateTeardownEvent: EventArgs {
        
		///<summary>
		///</summary>
		public object pDisp;
        
		///<summary>
		///Summary of BrowserPrintTemplateTeardownEvent.
		///</summary>
		///<param name="pDisp"></param>
		public BrowserPrintTemplateTeardownEvent(object pDisp) {
			this.pDisp = pDisp;
		}
	}
    
	///<summary>
	///Summary of BrowserPrintTemplateInstantiationEventHandler
	///</summary>

	public delegate void BrowserPrintTemplateInstantiationEventHandler(object sender, BrowserPrintTemplateInstantiationEvent e);
    
	///<summary>
	///Summary of BrowserPrintTemplateInstantiationEvent
	///</summary>

	public class BrowserPrintTemplateInstantiationEvent: EventArgs {
        
		///<summary>
		///</summary>
		public object pDisp;
        
		///<summary>
		///Summary of BrowserPrintTemplateInstantiationEvent.
		///</summary>
		///<param name="pDisp"></param>
		public BrowserPrintTemplateInstantiationEvent(object pDisp) {
			this.pDisp = pDisp;
		}
	}
    
	///<summary>
	///Summary of BrowserNavigateErrorEventHandler
	///</summary>

	public delegate void BrowserNavigateErrorEventHandler(object sender, BrowserNavigateErrorEvent e);
    
	///<summary>
	///Summary of BrowserNavigateErrorEvent
	///</summary>

	public class BrowserNavigateErrorEvent:CancelEventArgs {
        
		///<summary>
		///</summary>
		public object pDisp;
        
		///<summary>
		///</summary>
		public object uRL;
        
		///<summary>
		///</summary>
		public object frame;
        
		///<summary>
		///</summary>
		public object statusCode;
        
		///<summary>
		///Summary of BrowserNavigateErrorEvent.
		///</summary>
		///<param name="pDisp"></param>
		///<param name="uRL"></param>
		///<param name="frame"></param>
		///<param name="statusCode"></param>
		///<param name="cancel"></param>
		public BrowserNavigateErrorEvent(object pDisp, object uRL, object frame, object statusCode, bool cancel):base(cancel) {
			this.pDisp = pDisp;
			this.uRL = uRL;
			this.frame = frame;
			this.statusCode = statusCode;
			base.Cancel = cancel;
		}
	}
    
	///<summary>
	///Summary of BrowserFileDownloadEventHandler
	///</summary>

	public delegate void BrowserFileDownloadEventHandler(object sender, BrowserFileDownloadEvent e);
    
	///<summary>
	///Summary of BrowserFileDownloadEvent
	///</summary>

	public class BrowserFileDownloadEvent:CancelEventArgs {
        
		///<summary>
		///Summary of BrowserFileDownloadEvent.
		///</summary>
		///<param name="cancel"></param>
		public BrowserFileDownloadEvent(bool cancel):base(cancel) {}
	}
    
	///<summary>
	///Summary of BrowserSetSecureLockIconEventHandler
	///</summary>

	public delegate void BrowserSetSecureLockIconEventHandler(object sender, BrowserSetSecureLockIconEvent e);
    
	///<summary>
	///Summary of BrowserSetSecureLockIconEvent
	///</summary>

	public class BrowserSetSecureLockIconEvent: EventArgs {
        
		///<summary>
		///</summary>
		public SHDocVw.SecureLockIconConstants secureLockIcon;
        
		///<summary>
		///Summary of BrowserSetSecureLockIconEvent.
		///</summary>
		///<param name="secureLockIcon"></param>
		public BrowserSetSecureLockIconEvent(int secureLockIcon) {
			this.secureLockIcon = (SHDocVw.SecureLockIconConstants)secureLockIcon;
		}
	}
    
	///<summary>
	///Summary of BrowserClientToHostWindowEventHandler
	///</summary>

	public delegate void BrowserClientToHostWindowEventHandler(object sender, BrowserClientToHostWindowEvent e);
    
	///<summary>
	///Summary of BrowserClientToHostWindowEvent
	///</summary>

	public class BrowserClientToHostWindowEvent: EventArgs {
        
		///<summary>
		///</summary>
		public int cX;
        
		///<summary>
		///</summary>
		public int cY;
        
		///<summary>
		///Summary of BrowserClientToHostWindowEvent.
		///</summary>
		///<param name="cX"></param>
		///<param name="cY"></param>
		public BrowserClientToHostWindowEvent(int cX, int cY) {
			this.cX = cX;
			this.cY = cY;
		}
	}
    
	///<summary>
	///Summary of BrowserWindowClosingEventHandler
	///</summary>

	public delegate void BrowserWindowClosingEventHandler(object sender, BrowserWindowClosingEvent e);
    
	///<summary>
	///Summary of BrowserWindowClosingEvent
	///</summary>

	public class BrowserWindowClosingEvent:CancelEventArgs {
        
		///<summary>
		///</summary>
		public bool isChildWindow;
        
		///<summary>
		///Summary of BrowserWindowClosingEvent.
		///</summary>
		///<param name="isChildWindow"></param>
		///<param name="cancel"></param>
		public BrowserWindowClosingEvent(bool isChildWindow, bool cancel):base(cancel) {
			this.isChildWindow = isChildWindow;
		}
	}
    
	///<summary>
	///Summary of BrowserWindowSetHeightEventHandler
	///</summary>

	public delegate void BrowserWindowSetHeightEventHandler(object sender, BrowserWindowSetHeightEvent e);
    
	///<summary>
	///Summary of BrowserWindowSetHeightEvent
	///</summary>

	public class BrowserWindowSetHeightEvent: EventArgs {
        
		///<summary>
		///</summary>
		public int height;
        
		///<summary>
		///Summary of BrowserWindowSetHeightEvent.
		///</summary>
		///<param name="height"></param>
		public BrowserWindowSetHeightEvent(int height) {
			this.height = height;
		}
	}
    
	///<summary>
	///Summary of BrowserWindowSetWidthEventHandler
	///</summary>

	public delegate void BrowserWindowSetWidthEventHandler(object sender, BrowserWindowSetWidthEvent e);
    
	///<summary>
	///Summary of BrowserWindowSetWidthEvent
	///</summary>

	public class BrowserWindowSetWidthEvent: EventArgs {
        
		///<summary>
		///</summary>
		public int width;
        
		///<summary>
		///Summary of BrowserWindowSetWidthEvent.
		///</summary>
		///<param name="width"></param>
		public BrowserWindowSetWidthEvent(int width) {
			this.width = width;
		}
	}
    
	///<summary>
	///Summary of BrowserWindowSetTopEventHandler
	///</summary>

	public delegate void BrowserWindowSetTopEventHandler(object sender, BrowserWindowSetTopEvent e);
    
	///<summary>
	///Summary of BrowserWindowSetTopEvent
	///</summary>

	public class BrowserWindowSetTopEvent: EventArgs {
        
		///<summary>
		///</summary>
		public int top;
        
		///<summary>
		///Summary of BrowserWindowSetTopEvent.
		///</summary>
		///<param name="top"></param>
		public BrowserWindowSetTopEvent(int top) {
			this.top = top;
		}
	}
    
	///<summary>
	///Summary of BrowserWindowSetLeftEventHandler
	///</summary>

	public delegate void BrowserWindowSetLeftEventHandler(object sender, BrowserWindowSetLeftEvent e);
    
	///<summary>
	///Summary of BrowserWindowSetLeftEvent
	///</summary>

	public class BrowserWindowSetLeftEvent: EventArgs {
        
		///<summary>
		///</summary>
		public int left;
        
		///<summary>
		///Summary of BrowserWindowSetLeftEvent.
		///</summary>
		///<param name="left"></param>
		public BrowserWindowSetLeftEvent(int left) {
			this.left = left;
		}
	}
    
	///<summary>
	///Summary of BrowserWindowSetResizableEventHandler
	///</summary>

	public delegate void BrowserWindowSetResizableEventHandler(object sender, BrowserWindowSetResizableEvent e);
    
	///<summary>
	///Summary of BrowserWindowSetResizableEvent
	///</summary>

	public class BrowserWindowSetResizableEvent: EventArgs {
        
		///<summary>
		///</summary>
		public bool resizable;
        
		///<summary>
		///Summary of BrowserWindowSetResizableEvent.
		///</summary>
		///<param name="resizable"></param>
		public BrowserWindowSetResizableEvent(bool resizable) {
			this.resizable = resizable;
		}
	}
    
	///<summary>
	///Summary of BrowserOnTheaterModeEventHandler
	///</summary>

	public delegate void BrowserOnTheaterModeEventHandler(object sender, BrowserOnTheaterModeEvent e);
    
	///<summary>
	///Summary of BrowserOnTheaterModeEvent
	///</summary>

	public class BrowserOnTheaterModeEvent: EventArgs {
        
		///<summary>
		///</summary>
		public bool theaterMode;
        
		///<summary>
		///Summary of BrowserOnTheaterModeEvent.
		///</summary>
		///<param name="theaterMode"></param>
		public BrowserOnTheaterModeEvent(bool theaterMode) {
			this.theaterMode = theaterMode;
		}
	}
    
	///<summary>
	///Summary of BrowserOnFullScreenEventHandler
	///</summary>

	public delegate void BrowserOnFullScreenEventHandler(object sender, BrowserOnFullScreenEvent e);
    
	///<summary>
	///Summary of BrowserOnFullScreenEvent
	///</summary>

	public class BrowserOnFullScreenEvent: EventArgs {
        
		///<summary>
		///</summary>
		public bool fullScreen;
        
		///<summary>
		///Summary of BrowserOnFullScreenEvent.
		///</summary>
		///<param name="fullScreen"></param>
		public BrowserOnFullScreenEvent(bool fullScreen) {
			this.fullScreen = fullScreen;
		}
	}
    
	///<summary>
	///Summary of BrowserOnStatusBarEventHandler
	///</summary>

	public delegate void BrowserOnStatusBarEventHandler(object sender, BrowserOnStatusBarEvent e);
    
	///<summary>
	///Summary of BrowserOnStatusBarEvent
	///</summary>

	public class BrowserOnStatusBarEvent: EventArgs {
        
		///<summary>
		///</summary>
		public bool statusBar;
        
		///<summary>
		///Summary of BrowserOnStatusBarEvent.
		///</summary>
		///<param name="statusBar"></param>
		public BrowserOnStatusBarEvent(bool statusBar) {
			this.statusBar = statusBar;
		}
	}
    
	///<summary>
	///Summary of BrowserOnMenuBarEventHandler
	///</summary>

	public delegate void BrowserOnMenuBarEventHandler(object sender, BrowserOnMenuBarEvent e);
    
	///<summary>
	///Summary of BrowserOnMenuBarEvent
	///</summary>

	public class BrowserOnMenuBarEvent: EventArgs {
        
		///<summary>
		///</summary>
		public bool menuBar;
        
		///<summary>
		///Summary of BrowserOnMenuBarEvent.
		///</summary>
		///<param name="menuBar"></param>
		public BrowserOnMenuBarEvent(bool menuBar) {
			this.menuBar = menuBar;
		}
	}
    
	///<summary>
	///Summary of BrowserOnToolBarEventHandler
	///</summary>

	public delegate void BrowserOnToolBarEventHandler(object sender, BrowserOnToolBarEvent e);
    
	///<summary>
	///Summary of BrowserOnToolBarEvent
	///</summary>

	public class BrowserOnToolBarEvent: EventArgs {
        
		///<summary>
		///</summary>
		public bool toolBar;
        
		///<summary>
		///Summary of BrowserOnToolBarEvent.
		///</summary>
		///<param name="toolBar"></param>
		public BrowserOnToolBarEvent(bool toolBar) {
			this.toolBar = toolBar;
		}
	}
    
	///<summary>
	///Summary of BrowserOnVisibleEventHandler
	///</summary>

	public delegate void BrowserOnVisibleEventHandler(object sender, BrowserOnVisibleEvent e);
    
	///<summary>
	///Summary of BrowserOnVisibleEvent
	///</summary>

	public class BrowserOnVisibleEvent: EventArgs {
        
		///<summary>
		///</summary>
		public bool visible;
        
		///<summary>
		///Summary of BrowserOnVisibleEvent.
		///</summary>
		///<param name="visible"></param>
		public BrowserOnVisibleEvent(bool visible) {
			this.visible = visible;
		}
	}
    
	///<summary>
	///Summary of BrowserDocumentCompleteEventHandler
	///</summary>

	public delegate void BrowserDocumentCompleteEventHandler(object sender, BrowserDocumentCompleteEvent e);
    
	///<summary>
	///Summary of BrowserDocumentCompleteEvent
	///</summary>

	public class BrowserDocumentCompleteEvent: EventArgs {
        
		/// <summary>
		/// True, if the navigation completed the root document.
		/// </summary>
		public bool IsRootPage;
		///<summary>
		///</summary>
		public object pDisp;
        
		///<summary>
		///</summary>
		public string url;
        
		///<summary>
		///Summary of BrowserDocumentCompleteEvent.
		///</summary>
		///<param name="pDisp"></param>
		///<param name="uRL"></param>
		///<param name="isRootPage"></param>
		public BrowserDocumentCompleteEvent(object pDisp, object uRL, bool isRootPage) {
			this.pDisp = pDisp;
			this.url = (String)uRL;
			this.IsRootPage = isRootPage;
		}
	}
    
	///<summary>
	///Summary of BrowserNavigateComplete2EventHandler
	///</summary>

	public delegate void BrowserNavigateComplete2EventHandler(object sender, BrowserNavigateComplete2Event e);
    
	///<summary>
	///Summary of BrowserNavigateComplete2Event
	///</summary>

	public class BrowserNavigateComplete2Event: EventArgs {
        
		/// <summary>
		/// True, if the navigation completed the root document.
		/// </summary>
		public bool IsRootPage;

		///<summary>
		///</summary>
		public object pDisp;
        
		///<summary>
		///</summary>
		public string url;
        
		///<summary>
		///Summary of BrowserNavigateComplete2Event.
		///</summary>
		///<param name="pDisp"></param>
		///<param name="uRL"></param>
		public BrowserNavigateComplete2Event(object pDisp, object uRL, bool isRootPage) {
			this.pDisp = pDisp;
			this.url = (String)uRL;
			this.IsRootPage = isRootPage;
		}
	}
    
	/// <summary>
	/// </summary>
	public delegate void BrowserNewWindowEventHandler(object sender, BrowserNewWindowEvent e);

	/// <summary>
	/// </summary>
	public class BrowserNewWindowEvent : CancelEventArgs {
		string _url;
		/// <summary>
		/// Initialize a new instance of BrowserNewWindowEvent
		/// </summary>
		/// <param name="url"></param>
		/// <param name="cancel"></param>
		public BrowserNewWindowEvent(string url, bool cancel)	: base(cancel)	{ this._url = url;	}
		/// <summary>
		/// Get the url that should be opened in a new window
		/// </summary>
		public string url { get { return this._url;	} 	}
	} 

	///<summary>
	///Summary of BrowserNewWindow2EventHandler
	///</summary>
	public delegate void BrowserNewWindow2EventHandler(object sender, BrowserNewWindow2Event e);
    
	///<summary>
	///Summary of BrowserNewWindow2Event
	///</summary>
	public class BrowserNewWindow2Event:CancelEventArgs {
        
		///<summary>
		///</summary>
		public object ppDisp;
        
		///<summary>
		///Summary of BrowserNewWindow2Event.
		///</summary>
		///<param name="ppDisp"></param>
		///<param name="cancel"></param>
		public BrowserNewWindow2Event(object ppDisp, bool cancel):base(cancel) {
			this.ppDisp = ppDisp;
		}
	}
    
	///<summary>
	///Summary of BrowserBeforeNavigate2EventHandler
	///</summary>

	public delegate void BrowserBeforeNavigate2EventHandler(object sender, BrowserBeforeNavigate2Event e);
    
	///<summary>
	///Summary of BrowserBeforeNavigate2Event
	///</summary>
	public class BrowserBeforeNavigate2Event: CancelEventArgs {
        
		/// <summary>
		/// True, if the navigation completed the root document.
		/// </summary>
		public bool IsRootPage;

		///<summary>
		///</summary>
		public object pDisp;
        
		///<summary>
		///</summary>
		public string url;
        
		///<summary>
		///</summary>
		public object flags;
        
		///<summary>
		///</summary>
		public object targetFrameName;
        
		///<summary>
		///</summary>
		public object postData;
        
		///<summary>
		///</summary>
		public object headers;
        
		///<summary>
		///Summary of BrowserBeforeNavigate2Event.
		///</summary>
		///<param name="pDisp"></param>
		///<param name="uRL"></param>
		///<param name="flags"></param>
		///<param name="targetFrameName"></param>
		///<param name="postData"></param>
		///<param name="headers"></param>
		///<param name="cancel"></param>
		public BrowserBeforeNavigate2Event(object pDisp, object uRL, object flags, object targetFrameName, object postData, object headers, bool cancel, bool isRootPage):base(cancel) {
			this.pDisp = pDisp;
			this.url = (String)uRL;
			this.flags = flags;
			this.targetFrameName = targetFrameName;
			this.postData = postData;
			this.headers = headers;
			this.IsRootPage = isRootPage;
		}
	}
    
	///<summary>
	///Summary of BrowserPropertyChangeEventHandler
	///</summary>

	public delegate void BrowserPropertyChangeEventHandler(object sender, BrowserPropertyChangeEvent e);
    
	///<summary>
	///Summary of BrowserPropertyChangeEvent
	///</summary>
	public class BrowserPropertyChangeEvent: EventArgs {
        
		///<summary>
		///</summary>
		public string szProperty;
        
		///<summary>
		///Summary of BrowserPropertyChangeEvent.
		///</summary>
		///<param name="szProperty"></param>
		public BrowserPropertyChangeEvent(string szProperty) {
			this.szProperty = szProperty;
		}
	}
    
	///<summary>
	///Summary of BrowserTitleChangeEventHandler
	///</summary>
	public delegate void BrowserTitleChangeEventHandler(object sender, BrowserTitleChangeEvent e);
    
	///<summary>
	///Summary of BrowserTitleChangeEvent
	///</summary>
	public class BrowserTitleChangeEvent: EventArgs {
        
		///<summary>
		///</summary>
		public string text;
        
		///<summary>
		///Summary of BrowserTitleChangeEvent.
		///</summary>
		///<param name="text"></param>
		public BrowserTitleChangeEvent(string text) {
			this.text = text;
		}
	}
    
	///<summary>
	///Summary of BrowserCommandStateChangeEventHandler
	///</summary>
	public delegate void BrowserCommandStateChangeEventHandler(object sender, BrowserCommandStateChangeEvent e);
    
	///<summary>
	///Summary of BrowserCommandStateChangeEvent
	///</summary>
	public class BrowserCommandStateChangeEvent: EventArgs {
        
		///<summary>
		///</summary>
		public SHDocVw.CommandStateChangeConstants command;
        
		///<summary>
		///</summary>
		public bool enable;
        
		///<summary>
		///Summary of BrowserCommandStateChangeEvent.
		///</summary>
		///<param name="command"></param>
		///<param name="enable"></param>
		public BrowserCommandStateChangeEvent(int command, bool enable) {
			this.command = (SHDocVw.CommandStateChangeConstants)command;
			this.enable = enable;
		}
	}
    
	///<summary>
	///Summary of BrowserProgressChangeEventHandler
	///</summary>
	public delegate void BrowserProgressChangeEventHandler(object sender, BrowserProgressChangeEvent e);
    
	///<summary>
	///Summary of BrowserProgressChangeEvent
	///</summary>
	public class BrowserProgressChangeEvent: EventArgs {
        
		///<summary>
		///</summary>
		public int progress;
        
		///<summary>
		///</summary>
		public int progressMax;
        
		///<summary>
		///Summary of BrowserProgressChangeEvent.
		///</summary>
		///<param name="progress"></param>
		///<param name="progressMax"></param>
		public BrowserProgressChangeEvent(int progress, int progressMax) {
			this.progress = progress;
			this.progressMax = progressMax;
		}
	}
	

	///<summary>
	///Summary of BrowserStatusTextChangeEventHandler
	///</summary>
	public delegate void BrowserStatusTextChangeEventHandler(object sender, BrowserStatusTextChangeEvent e);
    
	///<summary>
	///Summary of BrowserStatusTextChangeEvent
	///</summary>
	public class BrowserStatusTextChangeEvent: EventArgs {
        
		///<summary>
		///</summary>
		public string text;
        
		///<summary>
		///Summary of BrowserStatusTextChangeEvent.
		///</summary>
		///<param name="text"></param>
		public BrowserStatusTextChangeEvent(string text) {
			this.text = text;
		}
	}

	#region ContextMenu event
	/// <summary>
	/// </summary>
	public delegate void BrowserContextMenuCancelEventHandler(object sender, BrowserContextMenuCancelEventArgs e);

	/// <summary>
	/// </summary>
	public class BrowserContextMenuCancelEventArgs : CancelEventArgs {
		Point  location;
		/// <summary>
		/// Init a new instance of BrowserContextMenuCancelEventArgs
		/// </summary>
		/// <param name="loaction"></param>
		/// <param name="cancel"></param>
		public BrowserContextMenuCancelEventArgs(Point loaction, bool cancel)	: base(cancel)	{ this.location = location;	}
		/// <summary>
		/// Get the position of the cursor to be used to display the context menu
		/// </summary>
		public Point Location { get { return this.location;	} 	}
	} 
	#endregion

	/// <summary>
	/// </summary>
	public delegate void BrowserTranslateUrlEventHandler(object sender, BrowserTranslateUrlEventArgs e);

	/// <summary>
	/// </summary>
	public class BrowserTranslateUrlEventArgs: EventArgs {
		string url;
		string translatedUrl;

		/// <summary>
		/// Init a new instance of BrowserTranslateUrlEventArgs
		/// </summary>
		/// <param name="url"></param>
		public BrowserTranslateUrlEventArgs(string url)	{	this.url = this.translatedUrl = url; /* assume to be the same */	}
		/// <summary>
		/// Url to translate
		/// </summary>
		public string Url { get { return this.url;	} 	}
		/// <summary>
		/// Translated Url
		/// </summary>
		public string TranslatedUrl { get { return this.translatedUrl; } set { this.translatedUrl = value; } 	}
	}


}
