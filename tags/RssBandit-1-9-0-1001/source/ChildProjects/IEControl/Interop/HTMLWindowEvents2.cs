#region Copyright
/*
 * Copyright (c) 2007 by Dare Obasanjo 
 * Based on code found at http://www.codeproject.com/cs/miscctrl/csEXWB.asp?msg=2134983 
 */ 
#endregion

using System;
using System.Collections;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Runtime.InteropServices;

namespace IEControl{
	/// <summary>
	/// Summary description for HTMLWindowEvents2.
	/// </summary>
	[ComVisible(true), ComImport()]
	[TypeLibType((short)4160)]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIDispatch)]
	[Guid("3050f625-98b5-11cf-bb82-00aa00bdce0b")] 
	public interface HTMLWindowEvents2 {
		[DispId(HTMLDispIDs.DISPID_HTMLWINDOWEVENTS2_ONLOAD)]
		void onload([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
		[DispId(HTMLDispIDs.DISPID_HTMLWINDOWEVENTS2_ONUNLOAD)]
		void onunload([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
		[DispId(HTMLDispIDs.DISPID_HTMLWINDOWEVENTS2_ONHELP)]
		[return: MarshalAs(UnmanagedType.VariantBool)]
		bool onhelp([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
		[DispId(HTMLDispIDs.DISPID_HTMLWINDOWEVENTS2_ONFOCUS)]
		void onfocus([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
		[DispId(HTMLDispIDs.DISPID_HTMLWINDOWEVENTS2_ONBLUR)]
		void onblur([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
		[DispId(HTMLDispIDs.DISPID_HTMLWINDOWEVENTS2_ONERROR)]
		void onerror([In, MarshalAs(UnmanagedType.BStr)] string description, [In, MarshalAs(UnmanagedType.BStr)] string url, [In] int line);
		[DispId(HTMLDispIDs.DISPID_HTMLWINDOWEVENTS2_ONRESIZE)]
		void onresize([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
		[DispId(HTMLDispIDs.DISPID_HTMLWINDOWEVENTS2_ONSCROLL)]
		void onscroll([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
		[DispId(HTMLDispIDs.DISPID_HTMLWINDOWEVENTS2_ONBEFOREUNLOAD)]
		void onbeforeunload([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
		[DispId(HTMLDispIDs.DISPID_HTMLWINDOWEVENTS2_ONBEFOREPRINT)]
		void onbeforeprint([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
		[DispId(HTMLDispIDs.DISPID_HTMLWINDOWEVENTS2_ONAFTERPRINT)] 
		void onafterprint([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
	}

	[ComVisible(false)]
	public delegate void HTMLWindowEvents2_onafterprintEventHandler([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
 
	[ComVisible(false)]
	public delegate void HTMLWindowEvents2_onbeforeprintEventHandler([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);

	[ComVisible(false)]
	public delegate void HTMLWindowEvents2_onbeforeunloadEventHandler([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);

	[ComVisible(false)]
	public delegate void HTMLWindowEvents2_onblurEventHandler([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
	
	[ComVisible(false)]
	public delegate void HTMLWindowEvents2_onerrorEventHandler([In, MarshalAs(UnmanagedType.BStr)] string description, [In, MarshalAs(UnmanagedType.BStr)] string url, [In] int line);

	[ComVisible(false)]
	public delegate void HTMLWindowEvents2_onfocusEventHandler([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);

	[ComVisible(false)]
	public delegate bool HTMLWindowEvents2_onhelpEventHandler([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);

	[ComVisible(false)]
	public delegate void HTMLWindowEvents2_onloadEventHandler([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);

	[ComVisible(false)]
	public delegate void HTMLWindowEvents2_onresizeEventHandler([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
	
	[ComVisible(false)]
	public delegate void HTMLWindowEvents2_onscrollEventHandler([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);

	[ComVisible(false)]
	public delegate void HTMLWindowEvents2_onunloadEventHandler([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);


	[ComVisible(false), ComEventInterface(typeof(HTMLWindowEvents2), typeof(HTMLWindowEvents2_EventProvider))]
	public interface HTMLWindowEvents2_Event {
		// Events
		event HTMLWindowEvents2_onafterprintEventHandler onafterprint;
		event HTMLWindowEvents2_onbeforeprintEventHandler onbeforeprint;
		event HTMLWindowEvents2_onbeforeunloadEventHandler onbeforeunload;
		event HTMLWindowEvents2_onblurEventHandler onblur;
		event HTMLWindowEvents2_onerrorEventHandler onerror;
		event HTMLWindowEvents2_onfocusEventHandler onfocus;
		event HTMLWindowEvents2_onhelpEventHandler onhelp;
		event HTMLWindowEvents2_onloadEventHandler onload;
		event HTMLWindowEvents2_onresizeEventHandler onresize;
		event HTMLWindowEvents2_onscrollEventHandler onscroll;
		event HTMLWindowEvents2_onunloadEventHandler onunload;
	}

	internal sealed class HTMLWindowEvents2_EventProvider : HTMLWindowEvents2_Event, IDisposable {
		// Fields
		private ArrayList m_aEventSinkHelpers;
		private IConnectionPoint m_ConnectionPoint;
		private readonly IConnectionPointContainer m_ConnectionPointContainer;

		// Methods
		public HTMLWindowEvents2_EventProvider(object obj1) {
			this.m_ConnectionPointContainer = (IConnectionPointContainer) obj1;
		}

		public event HTMLWindowEvents2_onafterprintEventHandler onafterprint{
			add{
				lock (this) {
					if (this.m_ConnectionPoint == null) {
						this.Init();
					}
					HTMLWindowEvents2_SinkHelper helper = new HTMLWindowEvents2_SinkHelper();
					int pdwCookie;
					this.m_ConnectionPoint.Advise(helper, out pdwCookie);
					helper.m_dwCookie = pdwCookie;
					helper.m_onafterprintDelegate = value;
					this.m_aEventSinkHelpers.Add(helper);
				}
			}

			remove {
				lock (this) {
					int count = this.m_aEventSinkHelpers.Count;
					int index = 0;
					if (0 < count) {
						do {
							HTMLWindowEvents2_SinkHelper helper = (HTMLWindowEvents2_SinkHelper) this.m_aEventSinkHelpers[index];
							if ((helper.m_onafterprintDelegate != null) && (helper.m_onafterprintDelegate.Equals(value))) {
								this.m_aEventSinkHelpers.RemoveAt(index);
								this.m_ConnectionPoint.Unadvise(helper.m_dwCookie);
								if (count <= 1) {
									this.m_ConnectionPoint = null;
									this.m_aEventSinkHelpers = null;
								}
								break;
							}
							index++;
						}
						while (index < count);
					}
				}
			}
		}

		public event HTMLWindowEvents2_onbeforeprintEventHandler onbeforeprint{

			add{
				lock (this) {
					if (this.m_ConnectionPoint == null) {
						this.Init();
					}
					HTMLWindowEvents2_SinkHelper helper = new HTMLWindowEvents2_SinkHelper();
					int pdwCookie;
					this.m_ConnectionPoint.Advise(helper, out pdwCookie);
					helper.m_dwCookie = pdwCookie;
					helper.m_onbeforeprintDelegate = value;
					this.m_aEventSinkHelpers.Add(helper);
				}
			}

			remove{
				lock (this) {
					int count = this.m_aEventSinkHelpers.Count;
					int index = 0;
					if (0 < count) {
						do {
							HTMLWindowEvents2_SinkHelper helper = (HTMLWindowEvents2_SinkHelper) this.m_aEventSinkHelpers[index];
							if ((helper.m_onbeforeprintDelegate != null) && (helper.m_onbeforeprintDelegate.Equals(value))) {
								this.m_aEventSinkHelpers.RemoveAt(index);
								this.m_ConnectionPoint.Unadvise(helper.m_dwCookie);
								if (count <= 1) {
									this.m_ConnectionPoint = null;
									this.m_aEventSinkHelpers = null;
								}
								break;
							}
							index++;
						}
						while (index < count);
					}//if
				}//lock
			}
		}

		public event HTMLWindowEvents2_onbeforeunloadEventHandler onbeforeunload{
		
			add{
				lock (this) {
					if (this.m_ConnectionPoint == null) {
						this.Init();
					}
					HTMLWindowEvents2_SinkHelper helper = new HTMLWindowEvents2_SinkHelper();
					int pdwCookie;
					this.m_ConnectionPoint.Advise(helper, out pdwCookie);
					helper.m_dwCookie = pdwCookie;
					helper.m_onbeforeunloadDelegate = value;
					this.m_aEventSinkHelpers.Add(helper);
				}
			}

			remove{
				lock (this) {
					int count = this.m_aEventSinkHelpers.Count;
					int index = 0;
					if (0 < count) {
						do {
							HTMLWindowEvents2_SinkHelper helper = (HTMLWindowEvents2_SinkHelper) this.m_aEventSinkHelpers[index];
							if ((helper.m_onbeforeunloadDelegate != null) && (helper.m_onbeforeunloadDelegate.Equals(value))) {
								this.m_aEventSinkHelpers.RemoveAt(index);
								this.m_ConnectionPoint.Unadvise(helper.m_dwCookie);
								if (count <= 1) {
									this.m_ConnectionPoint = null;
									this.m_aEventSinkHelpers = null;
								}
								break;
							}
							index++;
						}
						while (index < count);
					}
				}
			}
		}

		public event HTMLWindowEvents2_onblurEventHandler onblur{
			add{ 
				lock (this) {
					if (this.m_ConnectionPoint == null) {
						this.Init();
					}
					HTMLWindowEvents2_SinkHelper helper = new HTMLWindowEvents2_SinkHelper();
					int pdwCookie;
					this.m_ConnectionPoint.Advise(helper, out pdwCookie);
					helper.m_dwCookie = pdwCookie;
					helper.m_onblurDelegate = value;
					this.m_aEventSinkHelpers.Add(helper);
				}
			}

			remove{ 
				lock (this) {
					int count = this.m_aEventSinkHelpers.Count;
					int index = 0;
					if (0 < count) {
						do {
							HTMLWindowEvents2_SinkHelper helper = (HTMLWindowEvents2_SinkHelper) this.m_aEventSinkHelpers[index];
							if ((helper.m_onblurDelegate != null) && (helper.m_onblurDelegate.Equals(value))) {
								this.m_aEventSinkHelpers.RemoveAt(index);
								this.m_ConnectionPoint.Unadvise(helper.m_dwCookie);
								if (count <= 1) {
									this.m_ConnectionPoint = null;
									this.m_aEventSinkHelpers = null;
								}
								break;
							}
							index++;
						}
						while (index < count);
					}
				}
			}
		}
	
		public event HTMLWindowEvents2_onfocusEventHandler onfocus{
			add{
				lock (this) {
					if (this.m_ConnectionPoint == null) {
						this.Init();
					}
					HTMLWindowEvents2_SinkHelper helper = new HTMLWindowEvents2_SinkHelper();
					int pdwCookie;
					this.m_ConnectionPoint.Advise(helper, out pdwCookie);
					helper.m_dwCookie = pdwCookie;
					helper.m_onfocusDelegate = value;
					this.m_aEventSinkHelpers.Add(helper);
				}
			}

			remove{ 
				lock (this) {
					int count = this.m_aEventSinkHelpers.Count;
					int index = 0;
					if (0 < count) {
						do {
							HTMLWindowEvents2_SinkHelper helper = (HTMLWindowEvents2_SinkHelper) this.m_aEventSinkHelpers[index];
							if ((helper.m_onfocusDelegate != null) && (helper.m_onfocusDelegate.Equals(value))) {
								this.m_aEventSinkHelpers.RemoveAt(index);
								this.m_ConnectionPoint.Unadvise(helper.m_dwCookie);
								if (count <= 1) {
									this.m_ConnectionPoint = null;
									this.m_aEventSinkHelpers = null;
								}
								break;
							}
							index++;
						}
						while (index < count);
					}
				}
			}
		}

		public event HTMLWindowEvents2_onloadEventHandler onload{ 
		
			add{
				lock (this) {
					if (this.m_ConnectionPoint == null) {
						this.Init();
					}
					HTMLWindowEvents2_SinkHelper helper = new HTMLWindowEvents2_SinkHelper();
					int pdwCookie;
					this.m_ConnectionPoint.Advise(helper, out pdwCookie);
					helper.m_dwCookie = pdwCookie;
					helper.m_onloadDelegate = value;
					this.m_aEventSinkHelpers.Add(helper);
				}
			}

			remove{
				lock (this) {
					int count = this.m_aEventSinkHelpers.Count;
					int index = 0;
					if (0 < count) {
						do {
							HTMLWindowEvents2_SinkHelper helper = (HTMLWindowEvents2_SinkHelper) this.m_aEventSinkHelpers[index];
							if ((helper.m_onloadDelegate != null) && (helper.m_onloadDelegate.Equals(value))) {
								this.m_aEventSinkHelpers.RemoveAt(index);
								this.m_ConnectionPoint.Unadvise(helper.m_dwCookie);
								if (count <= 1) {
									this.m_ConnectionPoint = null;
									this.m_aEventSinkHelpers = null;
								}
								break;
							}
							index++;
						}
						while (index < count);
					}
				}
			}
		}


		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose() {
			this.Close();
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Closes this instance.
		/// </summary>
		public void Close() {
			Monitor.Enter(this);
			try {
				if (this.m_ConnectionPoint != null) {
					int count = this.m_aEventSinkHelpers.Count;
					int num2 = 0;
					if (0 < count) {
						do {
							HTMLWindowEvents2_SinkHelper helper = (HTMLWindowEvents2_SinkHelper) this.m_aEventSinkHelpers[num2];
							this.m_ConnectionPoint.Unadvise(helper.m_dwCookie);
							num2++;
						}
						while (num2 < count);
					}
				}
			}
			catch
			{
			}
			finally {
				Monitor.Exit(this);
			}
		}

		private void Init() {
			IConnectionPoint ppCP;
			byte[] b = new byte[] { 0x25, 0xf6, 80, 0x30, 0xb5, 0x98, 0xcf, 0x11, 0xbb, 130, 0, 170, 0, 0xbd, 0xce, 11 };
			Guid riid = new Guid(b);
			this.m_ConnectionPointContainer.FindConnectionPoint(ref riid, out ppCP);
			this.m_ConnectionPoint = ppCP;
			this.m_aEventSinkHelpers = new ArrayList();
		}		

	

		

		public event HTMLWindowEvents2_onerrorEventHandler onerror{

			add{
				lock (this) {
					if (this.m_ConnectionPoint == null) {
						this.Init();
					}
					HTMLWindowEvents2_SinkHelper helper = new HTMLWindowEvents2_SinkHelper();
					int pdwCookie;
					this.m_ConnectionPoint.Advise(helper, out pdwCookie);
					helper.m_dwCookie = pdwCookie;
					helper.m_onerrorDelegate = value;
					this.m_aEventSinkHelpers.Add(helper);
				}
			}

			remove{
				lock (this) {
					int count = this.m_aEventSinkHelpers.Count;
					int index = 0;
					if (0 < count) {
						do {
							HTMLWindowEvents2_SinkHelper helper = (HTMLWindowEvents2_SinkHelper) this.m_aEventSinkHelpers[index];
							if ((helper.m_onerrorDelegate != null) && (helper.m_onerrorDelegate.Equals(value))) {
								this.m_aEventSinkHelpers.RemoveAt(index);
								this.m_ConnectionPoint.Unadvise(helper.m_dwCookie);
								if (count <= 1) {
									this.m_ConnectionPoint = null;
									this.m_aEventSinkHelpers = null;
								}
								break;
							}
							index++;
						}
						while (index < count);
					}
				}
			}
		}

		public event HTMLWindowEvents2_onhelpEventHandler onhelp{
			add{
				lock (this) {
					if (this.m_ConnectionPoint == null) {
						this.Init();
					}
					HTMLWindowEvents2_SinkHelper helper = new HTMLWindowEvents2_SinkHelper();
					int pdwCookie;
					this.m_ConnectionPoint.Advise(helper, out pdwCookie);
					helper.m_dwCookie = pdwCookie;
					helper.m_onhelpDelegate = value;
					this.m_aEventSinkHelpers.Add(helper);
				}
			}

			remove{
				lock (this) {
					int count = this.m_aEventSinkHelpers.Count;
					int index = 0;
					if (0 < count) {
						do {
							HTMLWindowEvents2_SinkHelper helper = (HTMLWindowEvents2_SinkHelper) this.m_aEventSinkHelpers[index];
							if ((helper.m_onhelpDelegate != null) && (helper.m_onhelpDelegate.Equals(value))) {
								this.m_aEventSinkHelpers.RemoveAt(index);
								this.m_ConnectionPoint.Unadvise(helper.m_dwCookie);
								if (count <= 1) {
									this.m_ConnectionPoint = null;
									this.m_aEventSinkHelpers = null;
								}
								break;
							}
							index++;
						}
						while (index < count);
					}
				}
			}
		}

		public event HTMLWindowEvents2_onresizeEventHandler onresize{
			add{
				lock (this) {
					if (this.m_ConnectionPoint == null) {
						this.Init();
					}
					HTMLWindowEvents2_SinkHelper helper = new HTMLWindowEvents2_SinkHelper();
					int pdwCookie;
					this.m_ConnectionPoint.Advise(helper, out pdwCookie);
					helper.m_dwCookie = pdwCookie;
					helper.m_onresizeDelegate = value;
					this.m_aEventSinkHelpers.Add(helper);
				}
			}

			remove{
				lock (this) {
					int count = this.m_aEventSinkHelpers.Count;
					int index = 0;
					if (0 < count) {
						do {
							HTMLWindowEvents2_SinkHelper helper = (HTMLWindowEvents2_SinkHelper) this.m_aEventSinkHelpers[index];
							if ((helper.m_onresizeDelegate != null) && (helper.m_onresizeDelegate.Equals(value))) {
								this.m_aEventSinkHelpers.RemoveAt(index);
								this.m_ConnectionPoint.Unadvise(helper.m_dwCookie);
								if (count <= 1) {
									this.m_ConnectionPoint = null;
									this.m_aEventSinkHelpers = null;
								}
								break;
							}
							index++;
						}
						while (index < count);
					}
				}
			}
		}

		public event HTMLWindowEvents2_onscrollEventHandler onscroll{
			add{ 
				lock (this) {
					if (this.m_ConnectionPoint == null) {
						this.Init();
					}
					HTMLWindowEvents2_SinkHelper helper = new HTMLWindowEvents2_SinkHelper();
					int pdwCookie;
					this.m_ConnectionPoint.Advise(helper, out pdwCookie);
					helper.m_dwCookie = pdwCookie;
					helper.m_onscrollDelegate = value;
					this.m_aEventSinkHelpers.Add(helper);
				}
			}

			remove{
				lock (this) {
					int count = this.m_aEventSinkHelpers.Count;
					int index = 0;
					if (0 < count) {
						do {
							HTMLWindowEvents2_SinkHelper helper = (HTMLWindowEvents2_SinkHelper) this.m_aEventSinkHelpers[index];
							if ((helper.m_onscrollDelegate != null) && (helper.m_onscrollDelegate.Equals(value))) {
								this.m_aEventSinkHelpers.RemoveAt(index);
								this.m_ConnectionPoint.Unadvise(helper.m_dwCookie);
								if (count <= 1) {
									this.m_ConnectionPoint = null;
									this.m_aEventSinkHelpers = null;
								}
								break;
							}
							index++;
						}
						while (index < count);
					}
				}
			}
		}

		public event HTMLWindowEvents2_onunloadEventHandler onunload{
			add{
				lock (this) {
					if (this.m_ConnectionPoint == null) {
						this.Init();
					}
					HTMLWindowEvents2_SinkHelper helper = new HTMLWindowEvents2_SinkHelper();
					int pdwCookie;
					this.m_ConnectionPoint.Advise(helper, out pdwCookie);
					helper.m_dwCookie = pdwCookie;
					helper.m_onunloadDelegate = value;
					this.m_aEventSinkHelpers.Add(helper);
				}
			}

			remove{
				lock (this) {
					int count = this.m_aEventSinkHelpers.Count;
					int index = 0;
					if (0 < count) {
						do {
							HTMLWindowEvents2_SinkHelper helper = (HTMLWindowEvents2_SinkHelper) this.m_aEventSinkHelpers[index];
							if ((helper.m_onunloadDelegate != null) && (helper.m_onunloadDelegate.Equals(value))) {
								this.m_aEventSinkHelpers.RemoveAt(index);
								this.m_ConnectionPoint.Unadvise(helper.m_dwCookie);
								if (count <= 1) {
									this.m_ConnectionPoint = null;
									this.m_aEventSinkHelpers = null;
								}
								break;
							}
							index++;
						}
						while (index < count);
					}
				}
			}
		}
	}

	[ClassInterface(ClassInterfaceType.None)]
	internal sealed class HTMLWindowEvents2_SinkHelper : HTMLWindowEvents2 {
		// Fields
		public int m_dwCookie;
		public HTMLWindowEvents2_onafterprintEventHandler m_onafterprintDelegate;
		public HTMLWindowEvents2_onbeforeprintEventHandler m_onbeforeprintDelegate;
		public HTMLWindowEvents2_onbeforeunloadEventHandler m_onbeforeunloadDelegate;
		public HTMLWindowEvents2_onblurEventHandler m_onblurDelegate;
		public HTMLWindowEvents2_onerrorEventHandler m_onerrorDelegate;
		public HTMLWindowEvents2_onfocusEventHandler m_onfocusDelegate;
		public HTMLWindowEvents2_onhelpEventHandler m_onhelpDelegate;
		public HTMLWindowEvents2_onloadEventHandler m_onloadDelegate;
		public HTMLWindowEvents2_onresizeEventHandler m_onresizeDelegate;
		public HTMLWindowEvents2_onscrollEventHandler m_onscrollDelegate;
		public HTMLWindowEvents2_onunloadEventHandler m_onunloadDelegate;

		// Methods

		public  void onafterprint(IHTMLEventObj obj1) {
			if (this.m_onafterprintDelegate != null) {
				this.m_onafterprintDelegate(obj1);
			}
		}

		public  void onbeforeprint(IHTMLEventObj obj1) {
			if (this.m_onbeforeprintDelegate != null) {
				this.m_onbeforeprintDelegate(obj1);
			}
		}

		public  void onbeforeunload(IHTMLEventObj obj1) {
			if (this.m_onbeforeunloadDelegate != null) {
				this.m_onbeforeunloadDelegate(obj1);
			}
		}

		public  void onblur(IHTMLEventObj obj1) {
			if (this.m_onblurDelegate != null) {
				this.m_onblurDelegate(obj1);
			}
		}

		public  void onerror(string text1, string text2, int num1) {
			if (this.m_onerrorDelegate != null) {
				this.m_onerrorDelegate(text1, text2, num1);
			}
		}

		public  void onfocus(IHTMLEventObj obj1) {
			if (this.m_onfocusDelegate != null) {
				this.m_onfocusDelegate(obj1);
			}
		}

		public  bool onhelp(IHTMLEventObj obj1) {
			if (this.m_onhelpDelegate != null) {
				return this.m_onhelpDelegate(obj1);
			}
			return false;
		}

		public  void onload(IHTMLEventObj obj1) {
			if (this.m_onloadDelegate != null) {
				this.m_onloadDelegate(obj1);
			}
		}

		public  void onresize(IHTMLEventObj obj1) {
			if (this.m_onresizeDelegate != null) {
				this.m_onresizeDelegate(obj1);
			}
		}

		public  void onscroll(IHTMLEventObj obj1) {
			if (this.m_onscrollDelegate != null) {
				this.m_onscrollDelegate(obj1);
			}
		}

		public  void onunload(IHTMLEventObj obj1) {
			if (this.m_onunloadDelegate != null) {
				this.m_onunloadDelegate(obj1);
			}
		}
	}



}
 

