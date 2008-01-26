#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

#region CVS Version Log
/*
 * $Log: TreeFeedsNodeGroupHeaderPainter.cs,v $
 * Revision 1.2  2006/09/22 16:34:19  t_rendelmann
 * added CVS header and change history
 *
 */
#endregion

using System.Drawing;
using System.Drawing.Drawing2D;
using Infragistics.Win;

namespace RssBandit.WinGui
{
	public class TreeFeedsNodeGroupHeaderPainter
	{
		/// <summary>
		/// Paints a node background as a Outlook 2003 header.
		/// </summary>
		/// <param name="graphics">The graphics.</param>
		/// <param name="r">The rectangle.</param>
		public static void PaintOutlook2003Header(Graphics graphics, Rectangle r)
		{
			// render all to an image:
			Bitmap bmp = new Bitmap(r.Width, r.Height);
			using (Graphics g = Graphics.FromImage(bmp)) 
			{
				Rectangle bounds = new Rectangle(0, 0, r.Width, r.Height);
				if ((bounds.Width > 0) && (bounds.Height > 0)) {
					// draw background gradient
					using (LinearGradientBrush brush = new LinearGradientBrush(
							   new Point(bounds.X, bounds.Y - 1), new Point(bounds.X, bounds.Bottom), 
							   Office2003Colors.OutlookNavPaneGroupHeaderGradientLight, 
							   Office2003Colors.OutlookNavPaneGroupHeaderGradientDark)) 
					{
						g.FillRectangle(brush, bounds);
					}
					// draw top border
					using (Pen pen = new Pen(Office2003Colors.MainMenuBarGradientDark)) {
						g.DrawLine(pen, bounds.X, bounds.Y, bounds.Right - 1, bounds.Y);
					}
					// draw bottom border
					using (Pen pen = new Pen(Office2003Colors.OutlookNavPaneBorder)) {
						g.DrawLine(pen, bounds.X, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
					}
					// text is drawn by the Control itself, so we don't need it here:
	//				using (SolidBrush brush = new SolidBrush(Office2003Colors.OutlookNavPaneGroupHeaderForecolor)) {
	//					g.DrawString(text, font, brush, (RectangleF) bounds, StringFormat.GenericDefault);
	//				}

					// now take over the image drawn:
					graphics.DrawImage(bmp, r.Left, r.Top);
				}
			}
		}
	}
}


























