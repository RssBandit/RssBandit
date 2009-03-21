#region Version Header

/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
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
		public static void PaintOutlook2003Header(Graphics graphics, RectangleF r)
		{
			// render all to an image:
			using (Bitmap bmp = new Bitmap((int)r.Width, (int)r.Height))
			using (Graphics g = Graphics.FromImage(bmp)) 
			{
				RectangleF bounds = new RectangleF(0, 0, r.Width, r.Height);
				if ((bounds.Width > 0) && (bounds.Height > 0)) 
                {
					// draw background gradient
					using (LinearGradientBrush brush = new LinearGradientBrush(
							   new PointF(bounds.X, bounds.Y), new PointF(bounds.X, bounds.Bottom-1), 
							   Office2003Colors.OutlookNavPaneGroupHeaderGradientLight, 
							   Office2003Colors.OutlookNavPaneGroupHeaderGradientDark)) 
					{
						g.FillRectangle(brush, bounds);
					}
					// draw top border
					using (Pen pen = new Pen(Office2003Colors.MainMenuBarGradientDark)) {
						g.DrawLine(pen, bounds.X, bounds.Y-1, bounds.Right, bounds.Y - 1);
					}
					// draw bottom border
					using (Pen pen = new Pen(Office2003Colors.OutlookNavPaneBorder)) {
						g.DrawLine(pen, bounds.X, bounds.Bottom - 1, bounds.Right, bounds.Bottom - 1);
					}
					
					// now take over the image drawn:
                    graphics.DrawImage(bmp, r);
				}
			}
		}
	}
}


























