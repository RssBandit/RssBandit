using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RssBandit.WinGui.Utility
{
    static class ImageExtensions
    {
        public static Image GetImageStretchedDpi(this Image imageIn, double ratio)
        {
            Debug.Assert(imageIn != null);

            if (ratio == 1.0)
                return imageIn;


            var newWidth = (int)(imageIn.Width * ratio);
            var newHeight = (int)(imageIn.Height * ratio);
            var newBitmap = new Bitmap(newWidth, newHeight);

            using (var g = Graphics.FromImage(newBitmap))
            {
                // According to this blog post http://blogs.msdn.com/b/visualstudio/archive/2014/03/19/improving-high-dpi-support-for-visual-studio-2013.aspx
                // NearestNeighbor is more adapted for 200% and 200%+ DPI
                var interpolationMode = InterpolationMode.HighQualityBicubic;
                if (ratio >= 2.0f)
                {
                    interpolationMode = InterpolationMode.NearestNeighbor;
                }
                g.InterpolationMode = interpolationMode;
                g.DrawImage(imageIn, new Rectangle(0, 0, newWidth, newHeight));
            }

            imageIn.Dispose();
            return newBitmap;
        }
    }
}
