#region CVS Version Header
/*
 * $Id: NavigatorHeaderHelper.cs,v 1.2 2006/11/30 17:11:34 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2006/11/30 17:11:34 $
 * $Revision: 1.2 $
 */
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using Infragistics.Win;
using Infragistics.Win.UltraWinExplorerBar;
using RssBandit.WinGui.Controls;

namespace RssBandit.WinGui.Controls
{
	/// <summary>
	/// NavigatorHeaderHelper interacts with the provided 
	/// UltraExplorerBar and draws the image provided in the top
	/// right corner of the Group Header Area.
	/// Further it catches the mouse over and click to that image
	/// and raises the ImageClick event.
	/// </summary>
	public class NavigatorHeaderHelper
	{
		/// <summary>
		/// Raised, if an image is assigned and the mouse click happens over them.
		/// </summary>
		public event EventHandler ImageClick;

		#region private ivars

		private UltraExplorerBar navigator;
		private Cursor _saveCursorImageArea;

		#endregion

		#region ctor's

		/// <summary>
		/// Initializes a new instance of the <see cref="NavigatorHeaderHelper"/> class.
		/// </summary>
		/// <param name="navigator">The UltraExplorerBar</param>
		/// <param name="image">The image to draw.</param>
		public NavigatorHeaderHelper(UltraExplorerBar navigator, Image image)
		{
			this.navigator = navigator;
			this.navigator.DrawFilter = new NavigatorHeaderDrawFilter(image);
			this.navigator.MouseMove += new MouseEventHandler(this.OnMouseMove);
			this.navigator.Click += new EventHandler(this.OnClick);
		}

		#endregion

		#region protected members

		/// <summary>
		/// Called when the image was clicked and raises the ImageClick event (if set).
		/// </summary>
		protected void OnImageClick() {
			if (ImageClick != null)
				ImageClick(this, EventArgs.Empty);
		}

		#endregion

		#region Events

		private void OnMouseMove(object sender, MouseEventArgs args) 
		{
			if (IsMouseInsideImageArea()) {
				if (_saveCursorImageArea == null) {
					_saveCursorImageArea = this.navigator.Cursor;
					this.navigator.Cursor = Cursors.Hand;
				}
			} else {
				if (_saveCursorImageArea != null) {
					this.navigator.Cursor = _saveCursorImageArea;
					_saveCursorImageArea = null;
				}
			}
		}

		private void OnClick(object sender, EventArgs args) 
		{
			if (IsMouseInsideImageArea()) {
				OnImageClick();
			}
		}

		private bool IsMouseInsideImageArea() {
			Point mousePosition = this.navigator.PointToClient(Control.MousePosition);
			UIElement e = this.navigator.UIElement.ElementFromPoint(mousePosition);
			if (e is EditorWithTextDisplayTextUIElement) {
				int offset = e.Rect.Y + (e.Rect.Height - 16) / 2;
				Point p = new Point(e.Rect.X + e.Rect.Width - offset - 16, offset);
				Rectangle r = new Rectangle(p, new Size(16, 16));
				if (r.Contains(mousePosition)) {
					return true;
				}
			}
			return false;
		}
		#endregion
	}
}
