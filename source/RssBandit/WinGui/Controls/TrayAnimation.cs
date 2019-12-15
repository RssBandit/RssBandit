#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Collections;
using System.Reflection;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace RssBandit.WinGui.Controls
{

	/// <summary>
	/// Used to manage the animation -> static icon transition(s)
	/// </summary>
	public struct NotifyIconState
	{

		private readonly Icon _icon;
		private readonly Icon[] _aniList;
		private int _aniLoops;
		private readonly string _stateId;
		private string _stateTip;

		/// <summary>
		/// Icon state Initializer.
		/// </summary>
		/// <param name="stateId"></param>
		/// <param name="stateTip"></param>
		/// <param name="icon"></param>
		public NotifyIconState(string stateId, string stateTip, Icon icon)
		{
			_stateId = stateId;
			_stateTip = stateTip;
			_icon = icon;
			_aniLoops = 0;
			_aniList = null;
		}

		internal bool IsAnimation { get { return (_aniList != null && _aniList.GetLength(0) > 0); } }
		internal bool IsIcon { get { return (_icon != null); } }

		public string Key { get { return _stateId; } }
		public string Text { get { return _stateTip; } set { _stateTip = value; } }
		public Icon Icon { get { return _icon; } }
		public int AniLoops { get { return _aniLoops; } set { _aniLoops = value; } }
		public Icon IconAt(int index) { return _aniList[index]; }
		public int IconCount() { return (_aniList != null ? _aniList.GetLength(0) : 0); }
	}


	public class NotifyIconAnimation : Component
	{
		#region NativeMethods class

		private static class NativeMethods
		{
			internal static readonly Int32 WM_TASKBARCREATED = RegisterWindowMessage("TaskbarCreated");

			internal const int MF_POPUP = 0x10;
			private const int WM_USER = 0x400;
			internal const int WM_NOTIFYICONCALLBACK = WM_USER + 1024;
			internal const int WM_MOUSEMOVE = 0x200;
			internal const int WM_COMMAND = 0x111;
			internal const int WM_LBUTTONDOWN = 0x201;
			internal const int WM_LBUTTONUP = 0x202;
			internal const int WM_LBUTTONDBLCLK = 0x203;
			internal const int WM_RBUTTONDOWN = 0x204;
			internal const int WM_RBUTTONUP = 0x205;
			internal const int WM_RBUTTONDBLCLK = 0x206;
			internal const int WM_MBUTTONDOWN = 0x207;
			internal const int WM_MBUTTONUP = 0x208;
			internal const int WM_MBUTTONDBLCLK = 0x209;
			internal const int NIN_BALLOONSHOW = 0x402;
			internal const int NIN_BALLOONHIDE = 0x403;
			internal const int NIN_BALLOONTIMEOUT = 0x404;
			internal const int NIN_BALLOONUSERCLICK = 0x405;

			[DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
			private static extern Int32 RegisterWindowMessage(string lpString);

			/// <summary>
			/// See also http://www.pinvoke.net/default.aspx/Structures.NOTIFYICONDATA
			/// </summary>
			[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
			internal struct NOTIFYICONDATA
			{
				/// <summary>
				/// Size of this structure, in bytes. 
				/// </summary>
				public int cbSize;

				/// <summary>
				/// Handle to the window that receives notification messages associated with an icon in the 
				/// taskbar status area. The Shell uses hWnd and uID to identify which icon to operate on 
				/// when Shell_NotifyIcon is invoked. 
				/// </summary>
				public IntPtr hWnd;

				/// <summary>
				/// Application-defined identifier of the taskbar icon. The Shell uses hWnd and uID to identify 
				/// which icon to operate on when Shell_NotifyIcon is invoked. You can have multiple icons 
				/// associated with a single hWnd by assigning each a different uID. 
				/// </summary>
				public int uID;

				/// <summary>
				/// Flags that indicate which of the other members contain valid data. This member can be 
				/// a combination of the NIF_XXX constants.
				/// </summary>
				public int uFlags;

				/// <summary>
				/// Application-defined message identifier. The system uses this identifier to send 
				/// notifications to the window identified in hWnd. 
				/// </summary>
				public int uCallbackMessage;

				/// <summary>
				/// Handle to the icon to be added, modified, or deleted. 
				/// </summary>
				public IntPtr hIcon;

				/// <summary>
				/// String with the text for a standard ToolTip. It can have a maximum of 64 characters including 
				/// the terminating NULL. For Version 5.0 and later, szTip can have a maximum of 
				/// 128 characters, including the terminating NULL.
				/// </summary>
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
				public string szTip;

				/// <summary>
				/// State of the icon. 
				/// </summary>
				public int dwState;

				/// <summary>
				/// A value that specifies which bits of the state member are retrieved or modified. 
				/// For example, setting this member to NIS_HIDDEN causes only the item's hidden state to be retrieved. 
				/// </summary>
				public int dwStateMask;

				/// <summary>
				/// String with the text for a balloon ToolTip. It can have a maximum of 255 characters. 
				/// To remove the ToolTip, set the NIF_INFO flag in uFlags and set szInfo to an empty string. 
				/// </summary>
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
				public string szInfo;

				/// <summary>
				/// NOTE: This field is also used for the Timeout value. Specifies whether the Shell notify 
				/// icon interface should use Windows 95 or Windows 2000 
				/// behavior. For more information on the differences in these two behaviors, see 
				/// Shell_NotifyIcon. This member is only employed when using Shell_NotifyIcon to send an 
				/// NIM_VERSION message. 
				/// </summary>
				public int uVersion;

				/// <summary>
				/// String containing a title for a balloon ToolTip. This title appears in boldface 
				/// above the text. It can have a maximum of 63 characters. 
				/// </summary>
				[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
				public string szInfoTitle;

				/// <summary>
				/// Adds an icon to a balloon ToolTip. It is placed to the left of the title. If the 
				/// szTitleInfo member is zero-length, the icon is not shown. See 
				/// RMUtils.WinAPI.Structs.BalloonIconStyle for more
				/// information.
				/// </summary>
				public int dwInfoFlags;

				/* only XP valid:
				public Guid guidItem;
				*/
			}

			//		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
			//		internal struct NOTIFYICONDATA
			//		{
			//			internal int cbSize;
			//			internal IntPtr hWnd;
			//			internal uint uID;
			//			internal uint uFlags;
			//			internal uint uCallbackMessage;
			//			internal IntPtr hIcon;
			//			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)]
			//			internal string szTip;
			//			internal int dwState;
			//			internal int dwStateMask;
			//			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)]
			//			internal string szInfo;
			//			internal uint uVersion;
			//			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=64)]
			//			internal string szInfoTitle;
			//			internal int dwInfoFlags;
			//			/* only XP valid:
			//			internal Guid guidItem;
			//			*/
			//		}

			[DllImport("shell32.dll", CharSet = CharSet.Auto)]
			internal static extern bool Shell_NotifyIcon(int dwMessage, [In] ref NOTIFYICONDATA lpdata);

			internal const Int32 NIF_MESSAGE = 0x1;
			internal const Int32 NIF_ICON = 0x2;
			internal const Int32 NIF_STATE = 0x8;
			internal const Int32 NIF_INFO = 0x10;
			internal const Int32 NIF_TIP = 0x4;
			internal const Int32 NIM_ADD = 0x0;
			internal const Int32 NIM_MODIFY = 0x1;
			internal const Int32 NIM_DELETE = 0x2;
			internal const Int32 NIM_SETVERSION = 0x4;
			internal const Int32 NOTIFYICON_VERSION = 5;


		}

		#endregion

		internal enum EBalloonIcon
		{
			None = 0x0,			// NIIF_NONE
			Error = 0x3,		// NIIF_ERROR
			Info = 0x1,			// NIIF_INFO
			Warning = 0x2		// NIIF_WARNING
		}

		/// <summary>
		/// Handles the window native events
		/// </summary>
		private sealed class MessageHandler : NativeWindow, IDisposable
		{
			public event MouseEventHandler ClickNotify;
			public event MouseEventHandler DoubleClickNotify;
			public event MouseEventHandler MouseDownNotify;
			public event MouseEventHandler MouseUpNotify;
			public event MouseEventHandler MouseMoveNotify;

			public event EventHandler TaskbarReload;
			public event EventHandler BalloonShow;
			public event EventHandler BalloonHide;
			public event EventHandler BalloonTimeout;
			public event EventHandler BalloonClick;

			private void OnClickNotify(MouseEventArgs e) { if (ClickNotify != null) ClickNotify(this, e); }
			private void OnDoubleClickNotify(MouseEventArgs e) { if (DoubleClickNotify != null) DoubleClickNotify(this, e); }
			private void OnMouseUp(MouseEventArgs e) { if (MouseUpNotify != null) MouseUpNotify(this, e); }
			private void OnMouseDown(MouseEventArgs e) { if (MouseDownNotify != null) MouseDownNotify(this, e); }
			private void OnMouseMove(MouseEventArgs e) { if (MouseMoveNotify != null) MouseMoveNotify(this, e); }

			private void OnBallonClick(EventArgs e) { if (BalloonClick != null) BalloonClick(this, e); }
			private void OnBalloonShow(EventArgs e) { if (BalloonShow != null) BalloonShow(this, e); }
			private void OnBalloonHide(EventArgs e) { if (BalloonHide != null) BalloonHide(this, e); }
			private void OnBalloonTimeout(EventArgs e) { if (BalloonTimeout != null) BalloonTimeout(this, e); }
			private void OnTaskbarReload(EventArgs e) { if (TaskbarReload != null) TaskbarReload(this, e); }

			internal delegate void PerformContextMenuClickHandler(int menuId);
			internal event PerformContextMenuClickHandler PerformContextMenuClick;

			private void OnPerformContextMenuClick(int menuId)
			{
				if (PerformContextMenuClick != null)
					PerformContextMenuClick(menuId);
			}

			public MessageHandler()
			{
				base.CreateHandle(new CreateParams());
			}

			protected override void WndProc(ref Message m)
			{

				switch (m.Msg)
				{

					case NativeMethods.WM_NOTIFYICONCALLBACK:

						switch ((int)m.LParam)
						{
							case NativeMethods.WM_LBUTTONDBLCLK:
								OnDoubleClickNotify(new MouseEventArgs(MouseButtons.Left, 2, 0, 0, 0));
								break;
							case NativeMethods.WM_RBUTTONDBLCLK:
								OnDoubleClickNotify(new MouseEventArgs(MouseButtons.Right, 2, 0, 0, 0));
								break;
							case NativeMethods.WM_MBUTTONDBLCLK:
								OnDoubleClickNotify(new MouseEventArgs(MouseButtons.Middle, 2, 0, 0, 0));
								break;
							case NativeMethods.WM_LBUTTONDOWN:
								OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
								break;
							case NativeMethods.WM_RBUTTONDOWN:
								OnMouseDown(new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0));
								break;
							case NativeMethods.WM_MBUTTONDOWN:
								OnMouseDown(new MouseEventArgs(MouseButtons.Middle, 1, 0, 0, 0));
								break;
							case NativeMethods.WM_MOUSEMOVE:
								OnMouseMove(new MouseEventArgs(Control.MouseButtons, 0, 0, 0, 0));
								break;
							case NativeMethods.WM_LBUTTONUP:
								OnMouseUp(new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0));
								OnClickNotify(new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0));
								break;
							case NativeMethods.WM_RBUTTONUP:
								OnMouseUp(new MouseEventArgs(MouseButtons.Right, 0, 0, 0, 0));
								OnClickNotify(new MouseEventArgs(MouseButtons.Right, 0, 0, 0, 0));
								break;
							case NativeMethods.WM_MBUTTONUP:
								OnMouseUp(new MouseEventArgs(MouseButtons.Middle, 0, 0, 0, 0));
								OnClickNotify(new MouseEventArgs(MouseButtons.Middle, 0, 0, 0, 0));
								break;
							case NativeMethods.NIN_BALLOONSHOW:
								OnBalloonShow(new EventArgs());
								break;
							case NativeMethods.NIN_BALLOONHIDE:
								OnBalloonHide(new EventArgs());
								break;
							case NativeMethods.NIN_BALLOONTIMEOUT:
								OnBalloonTimeout(new EventArgs());
								break;
							case NativeMethods.NIN_BALLOONUSERCLICK:
								OnBallonClick(new EventArgs());
								break;

							default:
								//Trace.WriteLine("unhandled tray message.LParam:"+m.LParam.ToInt32());
								break;
						}
						break;

					case NativeMethods.WM_COMMAND:
						if (IntPtr.Zero == m.LParam)
						{
							int item = LOWORD(m.WParam);
							int flags = HIWORD(m.WParam);

							if ((flags & NativeMethods.MF_POPUP) == 0)
							{
								OnPerformContextMenuClick(item);
							}
						}
						break;

					default:
						if (m.Msg == NativeMethods.WM_TASKBARCREATED)
							OnTaskbarReload(new EventArgs());
						break;
				}
				base.WndProc(ref m);
			}

			private static int HIWORD(IntPtr x)
			{
				return (unchecked((int)(long)x) >> 16) & 0xffff;
			}

			private static int LOWORD(IntPtr x)
			{
				return unchecked((int)(long)x) & 0xffff;
			}

			#region IDisposable Members

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			#endregion

			private void Dispose(bool disposing)
			{
				Win32.NativeMethods.PostMessage(new HandleRef(this, Handle), (uint)Win32.NativeMethods.Message.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
				DestroyHandle();
			}

			~MessageHandler()
			{
				Dispose(false);
			}

		}


		[DescriptionAttribute("Occurs when the user clicks the icon in the status area."),
		CategoryAttribute("Action")]
		public event EventHandler Click;

		[DescriptionAttribute("Occurs when the user double-clicks the icon in the status notification area of the taskbar."),
		CategoryAttribute("Action")]
		public event EventHandler DoubleClick;

		[DescriptionAttribute("Occurs when the user presses the mouse button while the pointer is over the icon in the status notification area of the taskbar."),
		CategoryAttribute("Mouse")]
		public event MouseEventHandler MouseDown;

		[DescriptionAttribute("Occurs when the user releases the mouse button while the pointer is over the icon in the status notification area of the taskbar."),
		CategoryAttribute("Mouse")]
		public event MouseEventHandler MouseUp;

		[DescriptionAttribute("Occurs when the user moves the mouse while the pointer is over the icon in the status notification area of the taskbar."),
		CategoryAttribute("Mouse")]
		public event MouseEventHandler MouseMove;

		[DescriptionAttribute("Occurs when the balloon is shown (balloons are queued)."),
		CategoryAttribute("Behavior")]
		public event EventHandler BalloonShow;

		[DescriptionAttribute("Occurs when the balloon disappears—for example, when the icon is deleted. This message is not sent if the balloon is dismissed because of a timeout or a mouse click."),
		CategoryAttribute("Behavior")]
		public event EventHandler BalloonHide;

		[DescriptionAttribute("Occurs when the balloon is dismissed because of a timeout."),
		CategoryAttribute("Behavior")]
		public event EventHandler BalloonTimeout;

		[DescriptionAttribute("Occurs when the balloon is dismissed because of a mouse click."),
		CategoryAttribute("Action")]
		public event EventHandler BalloonClick;

		// own events/delegates
		public delegate void AnimationFinishedDelegate(object sender, NotifyIconState animation);

		[DescriptionAttribute("Occurs when a icon animation finishes to enable the change to another icon."),
		CategoryAttribute("Action")]
		public event AnimationFinishedDelegate AnimationFinished;



		#region Interop Declarations


		#endregion

		private NativeMethods.NOTIFYICONDATA _NID;
		private readonly MessageHandler _messages = new MessageHandler();

		private bool _visibleBeforeBalloon;
		private bool _visible;
		private bool _versionSet;
		private ContextMenuStrip _contextMenu;

		// provide the NotifyIcon events
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Container components;

		// the collection of 'NotifyIconState'
		private Hashtable _iconStates;
		private NotifyIconState _currentState;

		// Track whether Dispose has been called.
		private bool _disposed;

		#region Constructors/Destructor
		public NotifyIconAnimation()
		{

			// Required for Windows.Forms Class Composition Designer support

			InitializeComponent();

			Initialize();
		}
		public NotifyIconAnimation(IContainer container)
		{

			// Required for Windows.Forms Class Composition Designer support

			container.Add(this);
			InitializeComponent();

			Initialize();
		}

		public NotifyIconAnimation(NotifyIconState notifyIconState)
			: this()
		{
			_currentState = this.AddState(notifyIconState);
		}
		public NotifyIconAnimation(IContainer container, NotifyIconState notifyIconState)
			: this(container)
		{
			_currentState = this.AddState(notifyIconState);
		}

		private void Initialize()
		{
			// init struct
			_NID = new NativeMethods.NOTIFYICONDATA();
			_NID.hWnd = _messages.Handle;

			_NID.szTip = String.Empty;
			_NID.szInfo = String.Empty;
			_NID.szInfoTitle = String.Empty;
			_NID.cbSize = Marshal.SizeOf(typeof(NativeMethods.NOTIFYICONDATA));
			_NID.dwState = 0;
			_NID.dwStateMask = 0;
			_NID.uFlags = NativeMethods.NIF_ICON | NativeMethods.NIF_TIP | NativeMethods.NIF_MESSAGE;
			_NID.uCallbackMessage = NativeMethods.WM_NOTIFYICONCALLBACK;
			_NID.uVersion = NativeMethods.NOTIFYICON_VERSION;
			_NID.uID = 1;

			_iconStates = new Hashtable();
			InitEventHandler();
		}

		/// <summary>
		/// Only a helper to do not waste up the code with too many #if...#else.
		/// Wraps calls to Shell_NotifyIcon() and use the member var _NID.
		/// </summary>
		/// <param name="dwMessage">Notify message to send</param>
		/// <returns></returns>
		private bool CallShellNotify(int dwMessage)
		{
			return NativeMethods.Shell_NotifyIcon(dwMessage, ref _NID);
		}

		private void InitEventHandler()
		{
			_messages.ClickNotify += this.OnClickDownCast;
			_messages.DoubleClickNotify += this.OnDoubleClickDownCast;
			_messages.MouseDownNotify += this.OnMouseDown;
			_messages.MouseMoveNotify += this.OnMouseMove;
			_messages.MouseUpNotify += this.OnMouseUp;
			_messages.BalloonShow += this.OnBalloonShow;
			_messages.BalloonHide += this.OnBalloonHide;
			_messages.BalloonClick += this.OnBalloonClick;
			_messages.BalloonTimeout += this.OnBalloonTimeout;
			_messages.TaskbarReload += this.OnTaskbarReload;
			_messages.PerformContextMenuClick += OnPerformContextMenuClick;
		}
		
		protected override void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if (!this._disposed)
			{

				// If disposing equals true, dispose all managed 
				// and unmanaged resources.
				if (disposing)
				{

					// Dispose managed resources.
					Visible = false;
					_iconStates.Clear();

					if (_messages != null)
						_messages.Dispose();

					if (components != null) 
						components.Dispose();
				}

				// Release unmanaged resources. If disposing is false, 
				// only the following code is executed.

				// ...

				// Note that this is not thread safe.
				// Another thread could start disposing the object
				// after the managed resources are disposed,
				// but before the disposed flag is set to true.
			}
			_disposed = true;

			base.Dispose(disposing);
		}
		#endregion

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion

		#region private _NID struct accessors
		private void SetIcon()
		{
			this.SetIcon(_currentState.Icon);
		}
		private void SetIcon(Icon icon)
		{
			_NID.uFlags = _NID.uFlags | NativeMethods.NIF_ICON;
			_NID.hIcon = icon.Handle;
			if (Visible)
				CallShellNotify(NativeMethods.NIM_MODIFY);
		}
		private void SetText()
		{
			_NID.szTip = _currentState.Text;
			if (Visible)
			{
				_NID.uFlags = _NID.uFlags | NativeMethods.NIF_TIP;
				CallShellNotify(NativeMethods.NIM_MODIFY);
			}
		}
		#endregion

		[Description("The pop-up menu to show when the user right-clicks the icon."),
		CategoryAttribute("Behavior"),
		DefaultValueAttribute("")]
		public ContextMenuStrip ContextMenu
		{
			get { return _contextMenu; }
			set { _contextMenu = value; }
		}

		[Description("Determines whether the control is visible or hidden."),
		CategoryAttribute("Behavior"),
		DefaultValueAttribute(false)]
		public bool Visible
		{
			get { return _visible; }
			set
			{
				_visible = value;
				if (!DesignMode)
				{
					if (_visible)
					{
						CallShellNotify(NativeMethods.NIM_ADD);
						if (!_versionSet)
						{
							if (Environment.OSVersion.Version.Major >= 5)
								CallShellNotify(NativeMethods.NIM_SETVERSION);
							_versionSet = true;
						}

					}
					else
						CallShellNotify(NativeMethods.NIM_DELETE);
				}

			}
		}

		/// <summary>
		/// Overloaded. Display a balloon window on top of the tray icon.
		/// </summary>
		/// <param name="icon">Balloon window icon</param>
		/// <param name="text">Text to display</param>
		/// <param name="title">Title to display</param>
		internal void ShowBalloon(EBalloonIcon icon, string text, string title)
		{
			this.ShowBalloon(icon, text, title, 15000);
		}
		/// <summary>
		/// Overloaded. Display a balloon window on top of the tray icon.
		/// </summary>
		/// <param name="icon">Balloon window icon</param>
		/// <param name="text">Text to display</param>
		/// <param name="title">Title to display</param>
		/// <param name="timeout">Time in msecs that the balloon window should be displayed</param>
		internal void ShowBalloon(EBalloonIcon icon, string text, string title, int timeout)
		{
			int _old = _NID.uFlags;

			_visibleBeforeBalloon = _visible;
			_NID.uFlags |= NativeMethods.NIF_INFO;
			_NID.uVersion = timeout;
			// 10.Oct. 2003, TorstenR, Next line fixes the bug "Did not receive the balloon window messages" !!!
			_NID.hWnd = _messages.Handle;

			_NID.szInfo = text;
			_NID.szInfoTitle = title;
			_NID.dwInfoFlags = (int)icon;
			if (!Visible)
			{
				Visible = true;
			}
			else
			{
				CallShellNotify(NativeMethods.NIM_MODIFY);
			}
			_NID.uVersion = NativeMethods.NOTIFYICON_VERSION;
			_NID.uFlags = _old;
		}

		public NotifyIconState AddState(NotifyIconState newState)
		{
			_iconStates.Add(newState.Key, newState);
			return newState;
		}

		public void RemoveState(string stateKey)
		{
			_iconStates.Remove(stateKey);
			if (_currentState.Key.Equals(stateKey))
				SetState(_iconStates.Count != 0 ? ((NotifyIconState)_iconStates[0]).Key : "");
		}

		/// <summary>
		/// Set the new state. If it is an animation, it starts immediatly
		/// and iterates the animation as defined within the NotifyIconState.AniLoops
		/// property. If it was set to -1, this will be an endless animation.
		/// </summary>
		/// <param name="stateKey"></param>
		/// <returns></returns>
		public NotifyIconState SetState(string stateKey)
		{
			// set the new state
			if (_iconStates.ContainsKey(stateKey))
				_currentState = (NotifyIconState)_iconStates[stateKey];
			else
				_currentState = new NotifyIconState();

			// refresh current display state
			if (_currentState.IsIcon) SetIcon();

			SetText();
			
			return _currentState;
		}

		protected virtual void OnClickDownCast(object o, MouseEventArgs e) { if (Click != null) Click(this, e); }
		protected virtual void OnDoubleClickDownCast(object o, MouseEventArgs e) { if (DoubleClick != null) DoubleClick(this, e); }

		protected virtual void OnClick(object o, EventArgs e) { if (Click != null) Click(this, e); }
		protected virtual void OnDoubleClick(object o, EventArgs e) { if (DoubleClick != null) DoubleClick(this, e); }
		protected virtual void OnMouseUp(object o, MouseEventArgs e)
		{
			if (MouseUp != null) MouseUp(this, e);
			if (e.Button == MouseButtons.Right && _contextMenu != null)
			{
				Win32.NativeMethods.POINT p = new Win32.NativeMethods.POINT();
				Win32.NativeMethods.GetCursorPos(out p);

				// See Q135788 in the Microsoft Knowledge Base for more info.
				Win32.NativeMethods.SetForegroundWindow(new HandleRef(_messages, _messages.Handle));
				// we cannot call _contextMenu.Show(control, point): we don't have a control reference
				Type ctxType = _contextMenu.GetType();
				try
				{
					ctxType.InvokeMember("OnPopup", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, _contextMenu, new object[] { EventArgs.Empty });
					Win32.NativeMethods.TrackPopupMenuEx(new HandleRef(ContextMenu, ContextMenu.Handle), 0, p.x, p.y, new HandleRef(_messages, _messages.Handle), IntPtr.Zero);
					Win32.NativeMethods.PostMessage(new HandleRef(_messages, _messages.Handle), (uint)Win32.NativeMethods.Message.WM_NULL, IntPtr.Zero, IntPtr.Zero);
					// clicks handled in this.OnPerformContextMenuClick()...
				}
				catch { }
			}
		}

		protected virtual void OnMouseDown(object s, MouseEventArgs e) { if (MouseDown != null) MouseDown(this, e); }
		protected virtual void OnMouseMove(object s, MouseEventArgs e) { if (MouseMove != null) MouseMove(this, e); }

		private void OnTaskbarReload(object sender, EventArgs e)
		{
			if (Visible) Visible = true;
		}

		//private void OnTaskbarReload(EventArgs e) { if (Visible) Visible = true; }

		protected virtual void OnBalloonShow(object s, EventArgs e) { if (BalloonShow != null) BalloonShow(this, e); }
		protected virtual void OnBalloonHide(object s, EventArgs e) { if (BalloonHide != null) BalloonHide(this, e); }
		protected virtual void OnBalloonClick(object s, EventArgs e)
		{
			if (!_visibleBeforeBalloon) Visible = false;
			if (BalloonClick != null) BalloonClick(this, e);
		}
		protected virtual void OnBalloonTimeout(object s, EventArgs e)
		{
			if (!_visibleBeforeBalloon) Visible = false;
			if (BalloonTimeout != null) BalloonTimeout(this, e);
		}

		protected virtual void OnAnimationFinished(NotifyIconState animation) { if (AnimationFinished != null) AnimationFinished(this, animation); }

		private void OnPerformContextMenuClick(int menuID)
		{
			foreach (ToolStripMenuItem menuItem in _contextMenu.Items)
			{
				if (PerformContextMenuClick(menuID, menuItem))
					break;
			}
		}

		private bool PerformContextMenuClick(int menuID, ToolStripMenuItem item)
		{
			if (item == null)
				return false;

			int id = 0; Type itemType = item.GetType();
			try
			{
				id = (int)itemType.InvokeMember("MenuID", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance, null, item, new object[] { });
			}
			catch { }

			if (menuID == id)
			{
				item.PerformClick();
				return true;
			}

			// check for child menu items:
			foreach (ToolStripMenuItem menuItem in item.DropDownItems)
			{
				if (PerformContextMenuClick(menuID, menuItem))
					return true;
			}

			return false;
		}

	}
}
