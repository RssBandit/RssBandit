using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace RssBandit.WinGui.Utility
{
	
	/// <summary>
	/// Mouse Wheel Support.
	/// </summary>
	public class WheelSupport: NativeWindow, IMessageFilter
	{

		/// <summary>
		///  Used in OnGetChildControl event
		/// </summary>
		public delegate Control OnGetChildControlHandler(Control control);
		/// <summary>
		/// Should return the child control of the provided parent control.
		/// </summary>
		/// <remarks>Use it to handle special parent/child relationships on third party controls,
		/// like document manager or toolbars</remarks>
		public event OnGetChildControlHandler OnGetChildControl;

		private Form parent;

		private WheelSupport()	{}
		/// <summary>
		/// Call it before InitComponents() within the parent form.
		/// It listens to the HandleCreated/Destroyed events!
		/// </summary>
		/// <param name="f"></param>
		public WheelSupport(Form f) {
			this.parent = f;
			this.parent.HandleCreated += new EventHandler(this.OnParentHandleCreated);
			this.parent.HandleDestroyed += new EventHandler(this.OnParentHandleDestroyed);
		}

		private void OnParentHandleCreated(object sender, EventArgs e) {
			Application.AddMessageFilter(this);
		}

		private void OnParentHandleDestroyed(object sender, EventArgs e) {
			Application.RemoveMessageFilter(this);
		}

		// IMessageFilter impl.
		public virtual bool PreFilterMessage(ref Message m) {
			
			// Listen for operating system messages
			switch (m.Msg){
				case WM_MOUSEWHEEL:
					
					// get position (better debug support than calling Control.MousePosition in GetTopmostChild):
					Point screenPoint = new Point(m.LParam.ToInt32());
					// redirect the wheel message to the topmost child control
					Control child = GetTopmostChild(parent, screenPoint);
					
					if (child != null) {
						
						if (m.HWnd == child.Handle && child.Focused)
							return false;	// control is focused, so it should handle the wheel itself
	
						// thanks to http://sourceforge.net/users/kevindente/:
						if (child is IEControl.HtmlControl)	{
							return ScrollHtmlControl(child as IEControl.HtmlControl, m);
						}

						if (m.HWnd != child.Handle) {	// no recursion, please. Redirect message...
							PostMessage(child.Handle, WM_MOUSEWHEEL, m.WParam, m.LParam);
							return true;
						}

						return false;
					}
					break;       
			}
			return false;
		}
		
		public Control GetTopmostChild(Control ctrl, Point mousePosition) {
			if (this.OnGetChildControl != null) {
				Control childControl = this.OnGetChildControl(ctrl);
				if (childControl != null)
					ctrl = childControl;
			}

			if (ctrl.Controls.Count > 0) {
				Point p = ctrl.PointToClient(mousePosition);
				Control child = ctrl.GetChildAtPoint(p);
				if (child != null) {
					return GetTopmostChild(child, mousePosition);
				} else {
					return ctrl;
				}
			} else {
				return ctrl;
			}
		}
		
		/// <summary>
		/// Mouse wheel scrolling on IEControl support.
		/// Thanks to // thanks to http://sourceforge.net/users/kevindente/:
		/// </summary>
		/// <param name="control"></param>
		/// <param name="m"></param>
		/// <returns></returns>
		private bool ScrollHtmlControl(IEControl.HtmlControl control, Message m) {
			IntPtr hwnd;

			IEControl.Interop.IOleWindow oleWindow = null;
			try {
				oleWindow = control.Document2 as IEControl.Interop.IOleWindow;
			} catch (Exception){}

			if (oleWindow == null)
				return false;

			oleWindow.GetWindow(out hwnd);

			if (m.HWnd == hwnd) { 
			 	// avoid recursion
				return false;
			}
	
			PostMessage(hwnd, WM_MOUSEWHEEL, m.WParam, m.LParam);

			return true;	
		}

		#region Win32 interop

		[DllImport("user32.dll")] private static extern
			bool PostMessage( IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam );

		private const int WM_MOUSEWHEEL = 0x20A;

		#endregion

	}

}
