#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Infragistics.Win.UltraWinTree;
using RssBandit.WinGui.Controls;

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

		private readonly Form _parent;

		/// <summary>
		/// Add support for wheel scrolling on non-focused UI widgets 
		/// to the form f.
		/// </summary>
		/// <param name="f"></param>
		public WheelSupport(Form f) {
			this._parent = f;
			this._parent.Activated += this.OnParentActivated;
			this._parent.Deactivate += this.OnParentDeactivate;
		}

		private void OnParentActivated(object sender, EventArgs e) {
			Application.AddMessageFilter(this);
		}

		private void OnParentDeactivate(object sender, EventArgs e) {
			Application.RemoveMessageFilter(this);
		}

		// IMessageFilter impl.
		public virtual bool PreFilterMessage(ref Message m) {
			
			// Listen for operating system messages
			switch (m.Msg){
				case NativeMethods.WM_MOUSEWHEEL:
					
					// don't handle all (e.g. Ctrl-MouseWheel: zoom feature in IE)
					if (Control.ModifierKeys != Keys.None)
						return false;
					
					// get position (better debug support than calling Control.MousePosition in GetTopmostVisibleChild):
					Point screenPoint = new Point(m.LParam.ToInt32());
					// redirect the wheel message to the topmost child control
					Control child = GetTopmostVisibleChild(_parent, screenPoint);
					
					if (child != null) {
						
						if (m.HWnd == child.Handle && child.Focused)
							return false;	// control is focused, so it should handle the wheel itself
	
						// thanks to http://sourceforge.net/users/kevindente/:
						//if (child is IEControl.HtmlControl)	{
						//	return ScrollHtmlControl(child as IEControl.HtmlControl, m);
						//}

						if (child is UltraTree) {
							//HACK: because of no managed support, but works...
							UltraTree tree = child as UltraTree;
							// get the wheel delta:
							int delta = SignedHIWORD(m.WParam);
							return TreeHelper.InvokeDoVerticalScroll(tree, delta);
						}

						if (m.HWnd != child.Handle) {	// no recursion, please. Redirect message...
							NativeMethods.PostMessage(child.Handle, NativeMethods.WM_MOUSEWHEEL, m.WParam, m.LParam);
							return true;
						}

						return false;
					}
					break;       
			}
			return false;
		}
		
		/// <summary>
		/// Gets the topmost visible child.
		/// </summary>
		/// <param name="control">The control to start.</param>
		/// <param name="mousePosition">The mouse position.</param>
		/// <returns>Control</returns>
		public Control GetTopmostVisibleChild(Control control, Point mousePosition) {
			if (this.OnGetChildControl != null) {
				Control childControl = this.OnGetChildControl(control);
				if (childControl != null)
					control = childControl;
			}

			if (control.Controls.Count > 0) {
				Point p = control.PointToClient(mousePosition);
				Control child = control.GetChildAtPoint(p, GetChildAtPointSkip.Disabled | 
					GetChildAtPointSkip.Invisible | GetChildAtPointSkip.Transparent);
				if (child != null) {
					return GetTopmostVisibleChild(child, mousePosition);
				}
				return control;
			}
			return control;
		}
		
		///// <summary>
		///// Mouse wheel scrolling on IEControl support.
		///// Thanks to // thanks to http://sourceforge.net/users/kevindente/:
		///// </summary>
		///// <param name="control"></param>
		///// <param name="m"></param>
		///// <returns></returns>
		//private bool ScrollHtmlControl(IEControl.HtmlControl control, Message m) {
		//	IntPtr hwnd;

		//	IEControl.Interop.IOleWindow oleWindow = null;
		//	try {
		//		oleWindow = control.Document2 as IEControl.Interop.IOleWindow;
		//	} catch {}

		//	if (oleWindow == null)
		//		return false;

		//	oleWindow.GetWindow(out hwnd);

		//	if (m.HWnd == hwnd) { 
		//	 	// avoid recursion
		//		return false;
		//	}

		//	NativeMethods.PostMessage(hwnd, NativeMethods.WM_MOUSEWHEEL, m.WParam, m.LParam);

		//	return true;	
		//}

		#region Win32 interop/helpers

		static int SignedHIWORD(int n) {
			return (short) ((n >> 0x10) & 0xffff);
		}
		static int SignedHIWORD(IntPtr n) {
			return SignedHIWORD((int) ((long) n));
		}

		private static class NativeMethods
		{
			[DllImport("user32.dll")]
			static extern
			internal bool PostMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

			internal const int WM_MOUSEWHEEL = 0x20A;
		}


		#endregion

	}

}
