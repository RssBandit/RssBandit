// AniForm.cs: Contributed by Mike Marshall [mmarshall@mn.rr.com]]
// An MSN Messenger-style popup form class for animated message display
// An ani-form may hold any Windows Forms Component or Control
#region Copyright © 2002-2004 The Genghis Group
/*
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from the
 * use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not claim
 * that you wrote the original software. If you use this software in a product,
 * an acknowledgment in the product documentation is required, as shown here:
 *
 * Portions Copyright © 2002-2004 The Genghis Group (http://www.genghisgroup.com/).
 *
 * 2. No substantial portion of the source code of this library may be redistributed
 * without the express written permission of the copyright holders, where
 * "substantial" is defined as enough code to be recognizably from this library.
*/
#endregion
#region Features
/*
 * - Bottom-To-Top and Left-To-Right animation
 * - "Sticks" when form is activated
 * - Optional Gradient Background
 * - Optional fixed or raised border
 * - AniForms are "stackable"
*/
#endregion
#region Limitations
/*
 * - Needs Top-To-Bottom, Right-To-Left and Diagonal animation
 * - some flickering occurs when Raised border is selected
 * - Need to add support for "wrapping" when stacked forms would
 *   leave the visible display region.  Right now any forms that
 *   would stack out of sight are merely skipped.
*/
#endregion
#region History
/*
 * 01/02/04 (mjmarsh)
 *        - Stacking functionality still put too much responsibility on the client app.
 *          So 12/5 changes have been rolled back and now stacking is automatically
 *          done based on the form's StackMode property. Options are no stacking,
 *          always stack on top, or place in first available position in the stack.
 *          The stack itself is static, so all forms within the current process
 *          (or maybe AppDomain?) will use the same stack if they are configured to
 *          do so.
 * 
 *          Also some minor cleanup:
 *             - Removed additional constructor and overload of Animate due to change
 *               in stacking behavior
 *             - Removed all subscribtions to the form's own events and implemented
 *               the corresponding virtual overrides.  This is the prescribed
 *               way to handle you own events according to the docs
 *             - made changes to prefer ShowWindow(hwnd, SW_HIDE) instead of
 *               Visible = false.  This fixed some boundary cases where form
 *               would not show immediately after it had been hidden
 *             - Added OnExpanded/Expanded and removed expended()/contracted() to
 *               comform to event firing/handling conventions
 * 
 * 12/05/03 (mjmarsh)
 *        - Forms are now stackable by either 1) specifying the previous form
 *          in the stack in the overload of the constructor that takes an AniForm
 *          instance or 2) passing the previous form in the stack to the overload
 *          of Animate() that takes an AniForm
 * 
 * 12/01/03 (mjmarsh)
 *        - Fixed OnPaint to avoid painting into rectangles w <= 0 area.  This was
 *          causing high memory consumption from within Terminal Services sessions
 *        - added OnAnimatingDone/AnimatingDone to notify others
 *          when window is done expanding/contracting
 * 
 * 09/08/03 (jcmag)
 *			- added MouseDown, MouseMove and MouseUp event handlers to be able
 * to move Aniform using the mouse.
 * 
 * 09/05/03 (jcmag)
 *			- added a Persistent property to always show the form (until the user
 * close it explicitly)  
 *			- increase the Speed legal values
 *			- added "expanded" and "contracted" methods which are called when the
 * respective animation is completed 
 * 
 * 04/15/03 - Initial Version - based on MSN Messenger Popup Window
 * */
#endregion

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RssBandit;

namespace Genghis.Windows.Forms
{

    /// <summary>
    /// AniForm is a MSNMessenger-style animated popup windows that
    /// can have any windows forms control placed on it.  The class'
    /// properties define animation direction, animation speed, delay
    /// and a number of other properties that are used to render and 
    /// animate the form at run-time.
    /// </summary>
    public class AniForm : System.Windows.Forms.Form
    {
        #region PInvoke methods and constants
        private const Int32 GWL_STYLE = (-16);
        private const int WS_BORDER = 0x00800000;
        private const Int32 WS_CAPTION = 0x00C00000;
        private const Int32 WS_EX_APPWINDOW = 0x00040000;
        private const Int32 GWL_EXSTYLE = (-20);
        private const Int32 SW_SHOWNOACTIVATE = 4;
        private const Int32 SW_HIDE = 0;

        // SetWindowPos Flags
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 SWP_NOZORDER = 0x0004;
        private const UInt32 SWP_NOREDRAW = 0x0008;
        private const UInt32 SWP_NOACTIVATE = 0x0010;
        private const UInt32 SWP_FRAMECHANGED = 0x0020;  /* The frame changed: send WM_NCCALCSIZE */
        private const UInt32 SWP_SHOWWINDOW = 0x0040;
        private const UInt32 SWP_HIDEWINDOW = 0x0080;
        private const UInt32 SWP_NOCOPYBITS = 0x0100;
        private const UInt32 SWP_NOOWNERZORDER = 0x0200;  /* Don't do owner Z ordering */
        private const UInt32 SWP_NOSENDCHANGING = 0x0400;  /* Don't send WM_WINDOWPOSCHANGING */

        private const int HWND_TOPMOST = (-1);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hParent);

        [DllImport("user32.dll")]
        private static extern Int32 SetWindowLong(IntPtr hWnd, Int32 Offset, Int32 newLong);

        [DllImport("user32.dll")]
        private static extern Int32 GetWindowLong(IntPtr hWnd, Int32 Offset);

        [DllImport("user32.dll")]
        private static extern Int32 ShowWindow(IntPtr hWnd, Int32 dwFlags);

        [DllImport("user32.dll")]
        private static extern Int32 SetWindowPos(IntPtr hWnd, IntPtr hWndAfter, Int32 x, Int32 y, Int32 cx, Int32 cy, UInt32 uFlags);
        #endregion

        #region Private Fields
        private bool m_bPersistent;
        private AnimateDirection m_direction;
        private int m_iSpeed;
        private Color m_colorStart;
        private Color m_colorEnd;
        private BackgroundMode m_bgMode;
        private int m_iDelay;
        private FormPlacement m_placement;
        private int m_iGradientSize;
        private bool m_bAnimating;
        private bool m_bCloseRequested;
        private bool m_bSavedBounds;
        private AutoResetEvent m_eventClosed;
        private ManualResetEvent m_eventNotifyClosed;
        private int m_iInterval;
        private int m_iDelta;
        private int m_iCalcSpeed;
        private int m_iAdjSpeed;
        private int m_iLastDelta;
        private Rectangle m_oldBounds;
        private Point m_startLocation;
        private bool m_bActivated;
        private BorderStyle m_borderStyle;
        private int m_iBorderWidth;
        private Point mouseOffset;
        private bool isMouseDown = false;
        private WndMover m_wndMover = null;
        private AniForm m_baseForm = null;
        private StackMode m_stackMode = StackMode.None;
        private Point m_origLocation;
        private bool m_bAutoDispose;
        
        ThreadStart tsAnimate;
        Thread thAnimate;
        // This delegate enables asynchronous calls for setting
        // the text property on a TextBox control.
        delegate void CloseWindowDelegate();

        #endregion

        #region Private Static Fields
        private static StackArray s_currentForms = new StackArray();
        #endregion

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        #region Constructors
        public AniForm()
        {
            CommonConstruction();
        }

        private void CommonConstruction()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            m_eventClosed = new AutoResetEvent(false);
            m_eventNotifyClosed = new ManualResetEvent(false);
            m_direction = AnimateDirection.BottomToTop;
            m_iSpeed = 40;
            m_colorStart = Color.FromArgb(255, 168, 168, 255);
            m_colorEnd = SystemColors.Window;
            m_bgMode = BackgroundMode.GradientVertical;
            m_iDelay = 5000;
            m_bCloseRequested = false;
            m_iInterval = 10;
            InitCenterLocation();
            m_startLocation = this.Location;
            m_bSavedBounds = false;
            m_bActivated = false;
            m_borderStyle = BorderStyle.None;
            m_iBorderWidth = 0;
            m_wndMover = new WndMover(this.DoMove);
            
            m_bAutoDispose = false;
        }

        #endregion

        #region Public Events
        /// <summary>
        /// Fires when the window stops animating (after it contracts and/or disappears)
        /// </summary>
        public event EventHandler AnimatingDone;
        public event EventHandler Expanded;
        #endregion

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // 
            // AniForm
            // 
            //this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(208, 184);
            this.ControlBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AniForm";
            this.ShowInTaskbar = false;
            this.Text = "AniForm";
        }
        #endregion

        #region Public Properties

        public bool Persistent
        {
            get
            {
                return m_bPersistent;
            }
            set
            {
                m_bPersistent = value;
            }
        }

        /// <summary>
        /// The direction of the message window expansion
        /// </summary>
        public AnimateDirection Direction
        {
            get
            {
                return m_direction;
            }
            set
            {
                m_direction = value;
            }
        }

        /// <summary>
        /// The speed with which the window expands/contracts.  Valid
        /// values are between 1 and 50
        /// </summary>
        public int Speed
        {
            get
            {
                return m_iSpeed;
            }
            set
            {
                // HACK
                // Valid speeds are 1 - 89
                if (value < 1 || value > 89)
                    throw new ArgumentOutOfRangeException("Speed");
                m_iSpeed = value;
            }
        }

        /// <summary>
        /// For gradient backgrounds, the beginning color of the gradient
        /// </summary>
        public Color StartColor
        {
            get
            {
                return m_colorStart;
            }
            set
            {
                m_colorStart = value;
            }
        }

        /// <summary>
        /// For gradient backgrounds, the ending color of the gradient
        /// </summary>
        public Color EndColor
        {
            get
            {
                return m_colorEnd;
            }
            set
            {
                m_colorEnd = value;
            }
        }

        /// <summary>
        /// Sets or retrieves the background mode of the message window.
        /// See <see cref="BackgroundMode"/> for background mode choices
        /// </summary>
        public BackgroundMode BackgroundMode
        {
            get
            {
                return m_bgMode;
            }
            set
            {
                m_bgMode = value;
                Invalidate();
            }
        }

        /// <summary>
        /// The delay (in milliseconds) between when the window expands
        /// and when it contracts
        /// </summary>
        public int Delay
        {
            get
            {
                return m_iDelay;
            }
            set
            {
                m_iDelay = value;
            }
        }

        /// <summary>
        /// Indicates whether this Form is currently animating or not
        /// </summary>
        [Browsable(false)]
        public bool Animating
        {
            get
            {
                return m_bAnimating;
            }
            set
            {
                lock (this)
                {
                    m_bAnimating = value;
                }
            }
        }

        /// <summary>
        /// Indicates whether or not someone has requested that the
        /// window close prematurely.  This property will be set to
        /// true between the time the close is requested and when the
        /// window actually disappears.
        /// </summary>
        [Browsable(false)]
        public bool CloseRequested
        {
            get
            {
                bool bReturn;

                // Heavy handed? Probably. Chances of
                // a race condition are probably very small
                lock (this)
                {
                    bReturn = m_bCloseRequested;
                }

                return bReturn;
            }
            set
            {
                lock (this)
                {
                    m_bCloseRequested = value;
                }
            }
        }

        /// <summary>
        /// Indicates where the message window will appear on the screen
        /// </summary>
        public FormPlacement Placement
        {
            get
            {
                return m_placement;
            }
            set
            {
                m_placement = value;
            }
        }

        /// <summary>
        /// If the <see cref="Placement"/> property is set to Normal, then this
        /// property indicates the initial location of the form when it
        /// is displayed
        /// </summary>
        public Point StartLocation
        {
            get
            {
                return m_startLocation;
            }
            set
            {
                m_startLocation = value;
            }
        }

        /// <summary>
        /// Indicates whether or not the form is activated
        /// </summary>
        [Browsable(false)]
        public bool IsActivated
        {
            get
            {
                bool bActivated = false;
                lock (this)
                {
                    bActivated = m_bActivated;
                }

                return bActivated;
            }
            set
            {
                lock (this)
                {
                    m_bActivated = value;
                }
            }
        }

        /// <summary>
        /// Specifies the border style for the form
        /// </summary>
        public BorderStyle BorderStyle
        {
            get
            {
                return m_borderStyle;
            }
            set
            {
                m_borderStyle = value;
                if (m_borderStyle == BorderStyle.Raised)
                {
                    m_iBorderWidth = 4;
                }
                else if (m_borderStyle == BorderStyle.FixedSingle)
                {
                    m_iBorderWidth = 1;
                }
                else
                {
                    m_iBorderWidth = 0;
                }
            }
        }


        /// <summary>
        /// Specifies the full size of the form when expanded
        /// </summary>
        public Size FullSize
        {
            get
            {
                return new Size(m_oldBounds.Width, m_oldBounds.Height);
            }
        }

        /// <summary>
        /// Specifies the stacking mode used by this form
        /// </summary>
        public StackMode StackMode
        {
            get
            {
                return m_stackMode;
            }
            set
            {
                m_stackMode = value;
            }
        }

        /// <summary>
        /// Determines if the form closes itself and calls Dispose() internally once
        /// it is done animating. 
        /// </summary>
        /// <remarks>
        /// If set to true, a given AniForm instance can only be used once, otherwise
        /// the caller has the responsibility for calling Close() and Dispose() to
        /// release all respources associated with the AniForm.
        /// </remarks>
        public bool AutoDispose
        {
            get
            {
                return m_bAutoDispose;
            }
            set
            {
                m_bAutoDispose = value;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Displays the form and begins animation
        /// </summary>
        public void Animate()
        {
            // we don't want to mess with it if we're already animating
            if (!Animating)
            {
                m_eventNotifyClosed.Reset();

                // only do these once
                if (!m_bSavedBounds)
                {
                    if (BackgroundMode == BackgroundMode.GradientVertical)
                        m_iGradientSize = this.ClientRectangle.Height;
                    else if (BackgroundMode == BackgroundMode.GradientHorizontal)
                        m_iGradientSize = this.ClientRectangle.Width;

                    m_oldBounds = this.Bounds;
                    m_bSavedBounds = true;
                    m_origLocation = StartLocation;
                }


                // snap back to the start position if we got closed
                // prematurely before                
                ResetPosition();
                Calculate();

                // Does nothing if StackMode is None
                AddToStack();

                try
                {
                    InitLocation();
                    ResetPosition();

                    Animating = true;

                    // We don't want the form to grab focus or activate              
                    SetWindowPos(this.Handle, (IntPtr)HWND_TOPMOST, 0, 0, 0, 0,
                        SWP_HIDEWINDOW | SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                    ShowWindow(this.Handle, SW_SHOWNOACTIVATE);
                    // Create a worker thread so that the animation can
                    // be interrupted if desired
                    tsAnimate = new ThreadStart(this.AniFunc);
                    thAnimate = new Thread(tsAnimate);

                    // start animating
                    thAnimate.Start();
                }
                catch (OffDisplayException ex)
                {
                    RemoveFromStack();
                    // Don't display, it is off screen anyway
                    ex.GetHashCode();
                }
            }
        }


        /// <summary>
        /// Requests that the message window be closed immediately.
        /// </summary>
        /// <remarks>
        /// There may be a short interval between calling this method and
        /// when the Window actually closes
        /// </remarks>
        public void RequestClose()
        {
            if (this.IsHandleCreated)
            {
                if (Visible)
                {
                    ShowWindow(this.Handle, SW_HIDE);
                }

                m_bActivated = false;
                // set the event to facilitate quick
                // notification

                CloseRequested = true;
                SetClosedEvent();
            }
        }

        public bool WaitForClose(int iTime)
        {
            //wait on the close event for the specified
            // period of time
            return m_eventNotifyClosed.WaitOne(iTime, false);
        }

        #endregion

        #region Virtual Methods
        protected virtual void OnAnimatingDone(EventArgs e)
        {
            if (AnimatingDone != null)
                AnimatingDone(this, e);
        }

        protected virtual void OnExpanded(EventArgs e)
        {
            if (Expanded != null)
                Expanded(this, e);
        }
        #endregion

        #region Protected Methods
        protected void Calculate()
        {
            int iInitialSize = 0;
            int iPrimaryDimension = 0;

            // Changing to zero height above does not always give you
            // an initial height/width of zero (why?), so we need to factor that
            // in
            if (Direction == AnimateDirection.LeftToRight)
            {
                iInitialSize = this.Bounds.Width + 1;
                iPrimaryDimension = m_oldBounds.Width;
            }
            else
            {
                iInitialSize = this.Bounds.Height + 1;
                iPrimaryDimension = m_oldBounds.Height;
            }

            // This is where the math gets interesting.  We are basically looking
            // To calculate n intervals of the same size so we can smoothly 
            // make the window appear.  There may be an oddball at the end
            // becuase the height of our form may not be an even multiple
            // of our speed (n)

            // Initial speed
            m_iCalcSpeed = 90 - Speed;
            m_iAdjSpeed = m_iCalcSpeed;

            //  This calculates the last movement interval in pixels
            m_iLastDelta = (iPrimaryDimension - iInitialSize) % m_iCalcSpeed;

            // This is the main movement interval which is used for every
            // step except for the last
            m_iDelta = (iPrimaryDimension - iInitialSize - m_iLastDelta) / m_iCalcSpeed;

            // Took me a while to find this out, we may need to bump the speed value
            // just a bit to accomodate smooth scrolling of the window.  If our last
            // interval is too big compared to the main interval, it looks choppy at
            // the end.  So we break the last interval up into a number of main
            // intervals plus a remainder if it is too big to begin with
            if (m_iLastDelta > m_iDelta)
            {
                // Calculate the adjusted speed to accomodate smooth scrolling
                m_iAdjSpeed = m_iCalcSpeed + ((m_iLastDelta - (m_iLastDelta % m_iDelta)) / m_iDelta);

                // The new, smaller final interval
                m_iLastDelta = m_iLastDelta % m_iDelta;
            }
        }

        protected void ThreadSafeResize(bool bExpand, int iDiff)
        {
            // This is a safe place to check to see if the
            // client app requested that the window closed.
            // If so, throw a predefined exception type
            // which will cause the animation loop to exit
            // early
            if (CloseRequested == true)
            {

                throw new CloseRequestedException();
            }
            else
            {
                // This causes the window movement to be done on
                // the main thread.  This is the advised method
                // in the .NET docs

                // if we're contracting we want to move in the
                // other direction
                if (!bExpand)
                    iDiff = 0 - iDiff;

                object[] parms = { iDiff };
				//TR: check, if the main thread exists meanwhile
				if (!this.Disposing && !this.IsDisposed)
					try { this.Invoke(m_wndMover, parms); } catch { /* all */ } 
            }
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            // Let .NET do its thing if Normal is selected
            if (BackgroundMode != BackgroundMode.Normal)
            {
                // in design mode, we only want the gradient to span the
                // visible client area, whereas at runtime we want to have
                // a fixed gradient length to give the illusion of scrolling
                if (this.DesignMode)
                {
                    if (BackgroundMode == BackgroundMode.GradientVertical)
                    {
                        m_iGradientSize = this.ClientRectangle.Height;
                    }
                    else
                    {
                        m_iGradientSize = this.ClientRectangle.Width;
                    }
                }

                if (this.ClientRectangle.Height > 0 && this.ClientRectangle.Width > 0)
                {
                    // Calculate the fill rectangle
                    Rectangle rectFill = Rectangle.Inflate(this.ClientRectangle, -(m_iBorderWidth), -(m_iBorderWidth));

                    // only paint if the rectangle has an area > 0 !
                    if ((rectFill.Width > 0) && (rectFill.Height > 0))
                    {
                        LinearGradientBrush lgb;

                        // create the proper gradient brush based on BackgroundMode
                        if (BackgroundMode == BackgroundMode.GradientVertical)
                        {
                            lgb = new LinearGradientBrush(
                                new Rectangle(this.ClientRectangle.Left, this.ClientRectangle.Top, this.ClientRectangle.Width, m_iGradientSize),
                                StartColor,
                                EndColor,
                                LinearGradientMode.Vertical);
                        }
                        else
                        {
                            lgb = new LinearGradientBrush(
                                new Rectangle(this.ClientRectangle.Left, this.ClientRectangle.Top, m_iGradientSize, this.ClientRectangle.Height),
                                StartColor,
                                EndColor,
                                LinearGradientMode.Horizontal);
                        }


                        // Paint the gradient
                        e.Graphics.FillRectangle(lgb, rectFill);

                        lgb.Dispose();
                        lgb = null;
                    }
                }
            }

            base.OnPaint(e);

            if (this.BorderStyle != BorderStyle.None)
            {
                Rectangle rectBorder = new Rectangle(this.ClientRectangle.Left, this.ClientRectangle.Top, this.ClientRectangle.Width - 1, this.ClientRectangle.Height - 1);

                if (this.BorderStyle == BorderStyle.FixedSingle)
                {
                    if (rectBorder.Width > 1 && rectBorder.Height > 1)
                    {
                        e.Graphics.DrawRectangle(new Pen(new SolidBrush(Color.Black), 1.0f), rectBorder);
                    }
                }
                else if (this.BorderStyle == BorderStyle.Raised)
                {
                    // This can lead to flickering on some machines, is there a better
                    // way to invalidate on DoMove? or a quick way to draw?
                    if (rectBorder.Width > 1 && rectBorder.Height > 1)
                    {
                        e.Graphics.DrawRectangle(new Pen(new SolidBrush(SystemColors.ControlDark), 1.0f), rectBorder);
                        rectBorder.Inflate(-1, -1);
                    }

                    if (rectBorder.Width > 1 && rectBorder.Height > 1)
                    {
                        e.Graphics.DrawRectangle(new Pen(new SolidBrush(SystemColors.ControlLight), 1.0f), rectBorder);
                        rectBorder.Inflate(-1, -1);
                    }

                    if (rectBorder.Width > 1 && rectBorder.Height > 1)
                    {
                        e.Graphics.DrawRectangle(new Pen(new SolidBrush(SystemColors.ControlDark), 1.0f), rectBorder);
                        rectBorder.Inflate(-1, -1);
                    }

                    if (rectBorder.Width > 1 && rectBorder.Height > 1)
                    {
                        e.Graphics.DrawRectangle(new Pen(new SolidBrush(SystemColors.ControlDarkDark), 1.0f), rectBorder);
                    }
                }
            }
        }
        #endregion

        #region Private Methods
        private void ResetPosition()
        {
            // Start out hidden (or as small as possible)
            if (Direction == AnimateDirection.LeftToRight)
            {
                this.SetBounds(this.Bounds.Left, this.Bounds.Top, 1, this.Bounds.Height);
            }
            else
            {
                this.SetBounds(this.Bounds.Left, this.Bounds.Top, this.Bounds.Width, 1);
            }
        }

        private void AniFunc()
        {
            try
            {
                Expand();
                // We can't just Sleep() here, the client app may
                // want to close the window during the delay period, so
                // we'll wait on the event
                if (WaitForCloseRequest(Delay))
                {
                    // this will get us out in a hurry
                    throw new CloseRequestedException();
                }

                if (!IsActivated)
                {
                    Contract();
                }
            }
            catch (CloseRequestedException)
            {
                CloseRequested = false;
            }

            // We don't want to disappear if we've been activated
            if (!IsActivated)
            {
                // hide ourselves
                EndAnimation();
            }

            // No need for people who want to close the form
            // to worry about the thread hanging out there
            CloseRequested = false;
        }

        private void DoMove(int iDelta)
        {
            Rectangle newRect;

            if (Direction == AnimateDirection.LeftToRight)
            {
                // expand left to right ...
                newRect = new Rectangle(this.Bounds.Left, this.Bounds.Top,
                    this.Bounds.Width + iDelta, this.Bounds.Height);
            }
            else
            {
                // or bottom to top
                newRect = new Rectangle(this.Bounds.Left, this.Bounds.Top - iDelta,
                    this.Bounds.Width, this.Bounds.Height + iDelta);
            }

            // resize the window accordingly
            this.SetBounds(newRect.Left, newRect.Top, newRect.Width, newRect.Height);


            // Only invalidate the region that just appeared to
            // avoid flickering
            if (Direction == AnimateDirection.LeftToRight)
            {
                this.Invalidate(this.RectangleToClient(new Rectangle(newRect.Right - iDelta - m_iBorderWidth - 2, newRect.Top, iDelta + m_iBorderWidth + 2, newRect.Height)));
            }
            else
            {
                this.Invalidate(this.RectangleToClient(new Rectangle(newRect.Left, newRect.Bottom - iDelta - m_iBorderWidth - 2, newRect.Width, iDelta + m_iBorderWidth)));
            }

        }

        private void InitLocation()
        {
        	//TR: comparing the Handle to Zero will create the window handle, if not yet done.
        	// This is a required behavior at this time!
            if (this.Handle != IntPtr.Zero)
            {
                if (m_baseForm != null)
                {
                    Point ptNew;
                    // if our base form is set than ignore positioning information
                    // and stack the forms
                    if (this.Direction == AnimateDirection.LeftToRight)
                        ptNew = new Point(m_baseForm.StartLocation.X + m_oldBounds.Width + 1, m_baseForm.StartLocation.Y);
                    else
                        ptNew = new Point(m_baseForm.StartLocation.X, m_baseForm.StartLocation.Y - m_oldBounds.Height - 1);

                    Rectangle rcScreen = Screen.PrimaryScreen.Bounds;

                    // Don't bother if you're totally off screen
                    if ((ptNew.X > rcScreen.Right) || (ptNew.Y < rcScreen.Top))
                        throw new OffDisplayException();

                    this.Location = ptNew;
                }
                else if (Placement == FormPlacement.Tray)
                {
                    InitTrayLocation();
                }
                else if (Placement == FormPlacement.Centered)
                {
                    InitCenterLocation();
                }
                else if (Placement == FormPlacement.Normal)
                {
                    this.Location = m_origLocation;
                }


                this.StartLocation = this.Location;
            }
        }

        private void InitTrayLocation()
        {
            AppBarInfo info = new AppBarInfo();
            info.GetSystemTaskBarPosition();

            Rectangle rcWorkArea = info.WorkArea;

            int x = 0, y = 0;
			//TR - fix: this.Bounds.Height returns the height 
			// including the non-client elements, such as the window 
			// caption area with the menu/minimize/close buttons!
			if (info.Edge == AppBarInfo.ScreenEdge.Left)
            {
                x = rcWorkArea.Left + 2;
                y = rcWorkArea.Bottom /* - this.Bounds.Height */ - 5;
            }
            else if (info.Edge == AppBarInfo.ScreenEdge.Bottom)
            {
                x = rcWorkArea.Right - m_oldBounds.Width - 5;
                y = rcWorkArea.Bottom /* - this.Bounds.Height */ - 1;
            }
            else if (info.Edge == AppBarInfo.ScreenEdge.Top)
            {
                x = rcWorkArea.Right - m_oldBounds.Width - 5;
                y = rcWorkArea.Top + m_oldBounds.Height + 1;
            }
            else if (info.Edge == AppBarInfo.ScreenEdge.Right)
            {
                x = rcWorkArea.Right - m_oldBounds.Width - 5;
                y = rcWorkArea.Bottom /* - this.Bounds.Height */ - 1;
            }
            SetWindowPos(this.Handle, (IntPtr)HWND_TOPMOST, x, y, 0, 0,
                SWP_HIDEWINDOW | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        private void InitCenterLocation()
        {
            AppBarInfo info = new AppBarInfo();
            Rectangle workArea = info.WorkArea;

            this.Location = new Point((workArea.Left + (workArea.Width / 2)) - (m_oldBounds.Width / 2),
                (workArea.Top + (workArea.Height / 2)) + (m_oldBounds.Height / 2));
        }

        private void Expand()
        {
            // Expand the window
            for (int i = 0; i < m_iAdjSpeed; i++)
            {
                ThreadSafeResize(true, m_iDelta);
                Thread.Sleep(m_iInterval);
            }

            // move that one last time
            ThreadSafeResize(true, m_iLastDelta);

            OnExpanded(EventArgs.Empty);
        }

        private void Contract()
        {
			if (!this.Disposing && !this.IsDisposed) {
				// Contract
				for (int i = 0; i < m_iAdjSpeed; i++) {
					if (!this.Disposing && !this.IsDisposed)
						ThreadSafeResize(false, m_iDelta);
					else
						break;
					Thread.Sleep(m_iInterval);
				}
			}
        	
			if (!this.Disposing && !this.IsDisposed)
				ThreadSafeResize(false, m_iLastDelta);

            RemoveFromStack();

			//TR: looks like, this will be done (without cross-thread exception!)
			// correctly in the EndAnimation() function. That is always called
			// AFTER a call to Contract():
			//if (!this.IsDisposed && m_bAutoDispose)
			//{
			//    Close();
			//    Dispose();
			//}
        }

        private void AddToStack()
        {
            if (m_stackMode != StackMode.None)
            {
                m_baseForm = (AniForm)s_currentForms.Push(this, m_stackMode);
            }
        }

        private void RemoveFromStack()
        {
            s_currentForms.Pop(this, StackMode);
            m_baseForm = null;
        }

        private void EndAnimation()
        {
			//TR: once again: fighting a cross-thread-call exception:
			GuiInvoker.InvokeAsync(this,
            delegate
            {
                if (Visible)
                {
                    ShowWindow(this.Handle, SW_HIDE);
                }

                Animating = false;
                OnAnimatingDone(EventArgs.Empty);

                if (!this.IsDisposed && m_bAutoDispose)
                {
                    Hide();
                    Close();
                    Dispose();
                }
            });
        }

        private void SetClosedEvent()
        {
            m_eventClosed.Set();
        }

        private void ResetClosedEvent()
        {
            m_eventClosed.Reset();
        }

        private bool WaitForCloseRequest(int iTime)
        {
            //wait on the close event for the specified
            // period of time
            if (Animating)
            {
                bool bReturn = m_eventClosed.WaitOne(iTime, false);
                return bReturn;
            }

            return false;
        }
        #endregion

        #region Protected event handlers
        protected override void OnActivated(EventArgs e)
        {
            IsActivated = true;
            base.OnActivated(e);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            if (IsActivated && !Persistent)
            {
                Contract();
                IsActivated = false;

                // hide ourselves
                EndAnimation();
            }
            base.OnDeactivate(e);
        }


        protected override void OnClosed(EventArgs e)
        {
            Animating = false;
            RemoveFromStack();
            base.OnClosed(e);
        }

        protected override void OnResize(System.EventArgs e)
        {
            // if we don't do this design mode forms don't redraw
            // correctly when using gradient mode
            if (this.DesignMode)
            {
                this.Invalidate();
            }

            base.OnResize(e);
        }

        protected override void OnLoad(System.EventArgs e)
        {
            m_oldBounds = this.Bounds;
            base.OnLoad(e);
        }

        protected override void OnVisibleChanged(System.EventArgs e)
        {
            if (this.Visible == false)
            {
                Animating = false;
                RemoveFromStack();
            }

            base.OnVisibleChanged(e);
        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            int xOffset;
            int yOffset;

            if (e.Button == MouseButtons.Left)
            {
                xOffset = -e.X;
                yOffset = -e.Y;
                mouseOffset = new Point(xOffset, yOffset);
                isMouseDown = true;
            }
            base.OnMouseDown(e);
        }


        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                Location = mousePos;
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = false;
            }
            base.OnMouseUp(e);
        }
        #endregion
    }

    public class AppBarInfo
    {
        // Appbar messages
        private const int ABM_NEW = 0x00000000;
        private const int ABM_REMOVE = 0x00000001;
        private const int ABM_QUERYPOS = 0x00000002;
        private const int ABM_SETPOS = 0x00000003;
        private const int ABM_GETSTATE = 0x00000004;
        private const int ABM_GETTASKBARPOS = 0x00000005;
        private const int ABM_ACTIVATE = 0x00000006;  // lParam == TRUE/FALSE means activate/deactivate
        private const int ABM_GETAUTOHIDEBAR = 0x00000007;
        private const int ABM_SETAUTOHIDEBAR = 0x00000008;

        // Appbar edge constants
        private const int ABE_LEFT = 0;
        private const int ABE_TOP = 1;
        private const int ABE_RIGHT = 2;
        private const int ABE_BOTTOM = 3;

        // SystemParametersInfo constants
        private const System.UInt32 SPI_GETWORKAREA = 0x0030;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public System.Int32 left;
            public System.Int32 top;
            public System.Int32 right;
            public System.Int32 bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct APPBARDATA
        {
            public System.UInt32 cbSize;
            public System.IntPtr hWnd;
            public System.UInt32 uCallbackMessage;
            public System.UInt32 uEdge;
            public RECT rc;
            public System.Int32 lParam;
        }

        [DllImport("user32.dll")]
        private static extern System.IntPtr FindWindow(String lpClassName, String lpWindowName);

        [DllImport("shell32.dll")]
        private static extern System.UInt32 SHAppBarMessage(System.UInt32 dwMessage, ref APPBARDATA data);

        [DllImport("user32.dll")]
        private static extern System.Int32 SystemParametersInfo(System.UInt32 uiAction, System.UInt32 uiParam,
            System.IntPtr pvParam, System.UInt32 fWinIni);
        private APPBARDATA m_data;

        public enum ScreenEdge
        {
            Undefined = -1,
            Left = ABE_LEFT,
            Top = ABE_TOP,
            Right = ABE_RIGHT,
            Bottom = ABE_BOTTOM
        }

        public ScreenEdge Edge
        {
            get
            {
                return (ScreenEdge)m_data.uEdge;
            }
        }

        public Rectangle WorkArea
        {
            get
            {
                Int32 bResult = 0;
                RECT rc = new RECT();
                IntPtr rawRect = System.Runtime.InteropServices.Marshal.AllocHGlobal(System.Runtime.InteropServices.Marshal.SizeOf(rc));
                bResult = SystemParametersInfo(SPI_GETWORKAREA, 0, rawRect, 0);
                rc = (RECT)System.Runtime.InteropServices.Marshal.PtrToStructure(rawRect, rc.GetType());

                if (bResult == 1)
                {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(rawRect);
                    return new Rectangle(rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top);
                }

                return new Rectangle(0, 0, 0, 0);
            }
        }

        public void GetPosition(string strClassName, string strWindowName)
        {
            m_data = new APPBARDATA();
            m_data.cbSize = (UInt32)System.Runtime.InteropServices.Marshal.SizeOf(m_data.GetType());

            IntPtr hWnd = FindWindow(strClassName, strWindowName);

            if (hWnd != IntPtr.Zero)
            {
                UInt32 uResult = SHAppBarMessage(ABM_GETTASKBARPOS, ref m_data);

                if (uResult == 1)
                {
                }
                else
                {
                    throw new Exception("Failed to communicate with the given AppBar");
                }
            }
            else
            {
                throw new Exception("Failed to find an AppBar that matched the given criteria");
            }
        }

        public void GetSystemTaskBarPosition()
        {
            GetPosition("Shell_TrayWnd", null);
        }
    }


    internal delegate void WndMover(int iDelta);

    internal class CloseRequestedException : Exception
    {
        public CloseRequestedException() : base("Close requested")
        {
        }
    }


    /// <summary>
    /// Indicates the direction in which the message window expands
    /// </summary>
    public enum AnimateDirection
    {
        /// <summary>
        /// The for, expands from left to right on the screen, and then
        /// contracts from right to left
        /// </summary>
        LeftToRight,

        /// <summary>
        /// The form expands from bottom to top on the screen, and then
        /// contracts from top to bottom
        /// </summary>
        BottomToTop
    }

    /// <summary>
    /// Enumeration which contains the options for the form background for
    /// an <see cref="AniForm"/>.
    /// </summary>
    public enum BackgroundMode
    {
        /// <summary>
        /// The form's background is painted as any normal form would be.
        /// All applicable properties will be enforced (e.g. BackgroundColor)
        /// </summary>
        Normal,

        /// <summary>
        /// The background is drawn with a horizontal gradient brush.  The
        /// gradient starts with <see cref="StartColor"/> and ends with
        /// <see cref="EndColor"/>
        /// </summary>
        GradientHorizontal,

        /// <summary>
        /// The background is drawn with a vertical gradient brush.  The
        /// gradient starts with <see cref="StartColor"/> and ends with
        /// <see cref="EndColor"/>
        /// </summary>
        GradientVertical
    }

    /// <summary>
    /// Indicates where the form should be displayed
    /// </summary>
    public enum FormPlacement
    {
        /// <summary>
        /// The form is displayed at the position specified by its
        /// <see cref="Location"/> property
        /// </summary>
        Normal,

        /// <summary>
        /// The form is displayed near the system tray
        /// </summary>
        Tray,

        /// <summary>
        /// The form is displayed in the center of the screen
        /// </summary>
        Centered
    }

    /// <summary>
    /// The styles available for the AniForm border
    /// </summary>
    public enum BorderStyle
    {
        /// <summary>
        /// No border is drawn
        /// </summary>
        None,

        /// <summary>
        /// a 1-pixel, black border is drawn
        /// </summary>
        FixedSingle,

        /// <summary>
        /// The border is drawn to give the appearance of a raised "bump" edge
        /// </summary>
        Raised
    }

    /// <summary>
    /// Defines modes for AniForm stacking behavior
    /// </summary>
    public enum StackMode
    {
        /// <summary>
        /// The form will not be stacked and will be placed according
        /// to the <see cref="Placement"/> and <see cref="StartLocation"/>
        /// properties
        /// </summary>
        None,

        /// <summary>
        /// If there are any visible forms in the stack, the current form will
        /// be placed at the top of it
        /// </summary>
        Top,

        /// <summary>
        /// The current form will be placed as low in the stack as possible
        /// </summary>
        FirstAvailable
    }

    internal class StackArray : ArrayList
    {
        private ArrayList m_syncList = null;
        private int m_iCount = 0;

        public StackArray()
        {
            m_syncList = ArrayList.Synchronized(this);
        }

        public object Peek(StackMode stackMode)
        {
            if (m_syncList.Count > 0)
                return m_syncList[m_syncList.Count - 1];
            else
                return null;
        }

        public object Push(object newObject, StackMode stackMode)
        {
            bool bInserted = false;
            object previous = null;

            if (stackMode == StackMode.None)
                return null;

            if (stackMode == StackMode.FirstAvailable)
            {
                for (int i = 0; i < m_syncList.Count; i++)
                {
                    if (m_syncList[i] == null)
                    {
                        m_syncList[i] = newObject;
                        bInserted = true;
                        break;
                    }
                    else
                    {
                        previous = m_syncList[i];
                    }
                }

                if (!bInserted)
                {
                    m_syncList.Add(newObject);
                }
            }
            else if (stackMode == StackMode.Top)
            {
                if (m_syncList.Count > 0)
                    previous = m_syncList[m_syncList.Count - 1];

                m_syncList.Add(newObject);
            }

            m_iCount++;

            return previous;
        }

        public void Pop(object targetForm, StackMode stackMode)
        {
            if (stackMode == StackMode.None)
                return;

            if (stackMode == StackMode.FirstAvailable)
            {
                for (int i = 0; i < m_syncList.Count; i++)
                {
                    if (m_syncList[i] == targetForm)
                    {
                        m_syncList[i] = null;
                        m_iCount--;
                        break;
                    }
                }
            }
            else if (stackMode == StackMode.Top)
            {
                if (m_syncList.Contains(targetForm))
                {
                    m_syncList.Remove(targetForm);
                    m_iCount--;
                }
            }

            if (m_iCount == 0)
            {
                m_syncList.Clear();
            }
        }
    }

    internal class OffDisplayException : Exception
    {
        public OffDisplayException() : base("off screen")
        {
        }
    }
    
}
