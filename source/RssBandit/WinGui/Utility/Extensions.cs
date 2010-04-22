#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

#region System.Windows extensions

namespace System.Windows
{
    static class Extensions
    {
        /// <summary>
        /// Help to simply serialize as a bounds rectangle.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns>A ';' separated string: "X;Y;Width;Height".</returns>
        public static string GetBounds(this Rect rect)
        {
            return string.Format("{0};{1};{2};{3}", rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <summary>
        /// Gets the bounds from a string.
        /// </summary>
        /// <param name="boundsString">The bounds string.</param>
        /// <returns></returns>
        public static Rect GetBounds(this string boundsString)
        {
            string[] ba = boundsString.Split(new[] { ';' });
            Rect r = Rect.Empty;
            if (ba.GetLength(0) == 4)
            {
                try
                {
                    r = new Rect(Double.Parse(ba[0]), Double.Parse(ba[1]), Double.Parse(ba[2]), Double.Parse(ba[3]));
                }
                catch
                {
                }
            }
            return r;
        }

    }
}

#endregion

#region System.Drawing extensions

namespace System.Drawing
{
    static class Extensions
    {
        /// <summary>
        /// Help to simply serialize as a bounds rectangle.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns>A ';' separated string: "X;Y;Width;Height".</returns>
        public static string GetBounds(this Rectangle rect)
        {
            return string.Format("{0};{1};{2};{3}", rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static Rectangle GetBounds(this string boundsString)
        {
            string[] ba = boundsString.Split(new[] { ';' });
            Rectangle r = Rectangle.Empty;
            if (ba.GetLength(0) == 4)
            {
                try
                {
                    r = new Rectangle(Int32.Parse(ba[0]), Int32.Parse(ba[1]), Int32.Parse(ba[2]), Int32.Parse(ba[3]));
                }
                catch
                {
                }
            }
            return r;
        }
    }
}

#endregion
