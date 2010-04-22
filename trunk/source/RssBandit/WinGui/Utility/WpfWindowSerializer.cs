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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Genghis;
using NewsComponents.Utils;

namespace RssBandit.WinGui.Utility
{
    #region WindowStates enum

    [Flags]
    public enum WindowStates
    {
        Location = 1,
        Size = 2,
        WindowState = 4,
        All = Location | Size | WindowState
    }

    #endregion

    #region WpfWindowSerializer class

    public class WpfWindowSerializer
    {
        public static void Register(Window window, WindowStates states)
        {
            new WpfWindowSerializer(window, states);
        }

        private readonly string windowName;

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfWindowSerializer"/> class.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="states">The states to save and restore.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="window"/> is null</exception>
        private WpfWindowSerializer(Window window, WindowStates states)
        {
            window.ExceptionIfNull("window");
            
            windowName = String.Format("{0}.{1}", GetType().FullName, window.Name);
            
            // read state from preferences
            Preferences prefReader = GetPreferences();
            
            Rect defaultBounds = new Rect(Double.IsNaN(window.Left) ? 20.0: window.Left, Double.IsNaN(window.Top) ? 20.0: window.Top, window.Width, window.Height);
            Rect restoreBounds = defaultBounds;
            
            try { restoreBounds = prefReader.GetString("restoreBounds", defaultBounds.GetBounds()).GetBounds(); } 
            catch { /* format changed, or user fiddled around... */ }

            // adjust location to available screens:
            restoreBounds.Location = AdjustLocationToAvailableScreens(restoreBounds.Location);
            
            // adjust size to fit:
            if (restoreBounds.Height > SystemParameters.VirtualScreenHeight)
                restoreBounds.Height = SystemParameters.VirtualScreenHeight;
            if (restoreBounds.Width > SystemParameters.VirtualScreenWidth)
                restoreBounds.Width = SystemParameters.VirtualScreenWidth;

            window.Closing += OnWindowClosing;

            if ((states & WindowStates.Location) == WindowStates.Location)
            {
                window.Left = restoreBounds.Left;
                window.Top = restoreBounds.Top;
            }

            if ((states & WindowStates.Size) == WindowStates.Size)
            {
                window.Width = restoreBounds.Width;
                window.Height = restoreBounds.Height;
            }
            
            if ((states & WindowStates.WindowState) == WindowStates.WindowState)
            {
                // do also restore:
                window.WindowState = (WindowState)prefReader.GetInt32("windowState", (int)window.WindowState);
            }
        }

        void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Window window = sender as Window;
            if (window != null)
            {
                window.Closing -= OnWindowClosing;

                Preferences prefWriter = GetPreferences();
                // Write always restore bounds value and windowstate:
                prefWriter.SetProperty("restoreBounds", window.RestoreBounds.GetBounds());
                prefWriter.SetProperty("windowState", (int)window.WindowState);
                prefWriter.Close();
            }

        }

        private Preferences GetPreferences()
        {
            return Preferences.GetUserNode(GetType()).GetSubnode(windowName);
        }

        private static Point AdjustLocationToAvailableScreens(Point location)
        {
            // transform the Windows.Point into a drawing point:
            System.Drawing.Point p = new System.Drawing.Point(Convert.ToInt32(location.X), Convert.ToInt32(location.Y));
            if (Screen.AllScreens.Any(s => s.WorkingArea.Contains(p)))
            {
                return location;
            }
            // not found, return default at primary screen:
            return new Point(
                Screen.PrimaryScreen.WorkingArea.Location.X, 
                Screen.PrimaryScreen.WorkingArea.Location.Y);
        }

    }

    #endregion
}
