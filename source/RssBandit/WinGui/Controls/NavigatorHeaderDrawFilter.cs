#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System.Drawing;
using System.Drawing.Drawing2D;
using Infragistics.Win;

namespace RssBandit.WinGui.Controls
{
	/// <summary>
	/// Helps to draw the small chevron.
	/// </summary>
	public class NavigatorHeaderDrawFilter: IUIElementDrawFilter
	{
		private Image image;

		public NavigatorHeaderDrawFilter(Image image)
		{
			this.image = image;
		}

		#region IUIElementDrawFilter

		DrawPhase IUIElementDrawFilter.GetPhasesToFilter(ref UIElementDrawParams drawParams) {
			if (this.image != null)
				return drawParams.Element is EditorWithTextDisplayTextUIElement ? DrawPhase.AfterDrawForeground : DrawPhase.None;
			return DrawPhase.None;
		}

		bool IUIElementDrawFilter.DrawElement(DrawPhase drawPhase, ref UIElementDrawParams drawParams) {
			
			if (drawParams.Element is EditorWithTextDisplayTextUIElement &&
				this.image != null) {
				
				int offset = drawParams.Element.Rect.Y + (drawParams.Element.Rect.Height - this.image.Height) / 2;
				Point p = new Point(drawParams.Element.Rect.X + drawParams.Element.Rect.Width - offset - this.image.Width, offset);
				Rectangle r = new Rectangle(p, new Size(this.image.Width, this.image.Height));
				drawParams.Graphics.CompositingMode = CompositingMode.SourceOver;
				drawParams.Graphics.CompositingQuality = CompositingQuality.HighQuality;
				drawParams.Graphics.DrawImage(this.image, r, new Rectangle(new Point(0,0), this.image.Size), GraphicsUnit.Pixel);
				
			}
			return false;
		}

		#endregion
	}
}
