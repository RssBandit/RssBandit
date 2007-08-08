#region CVS Version Header
/*
 * $Id: SplashScreen.cs,v 1.9 2005/11/24 16:39:34 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/11/24 16:39:34 $
 * $Revision: 1.9 $
 */
#endregion

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace RssBandit.WinGui.Forms
{
	/// <summary>
	/// Summary description for SplashScreen.
	/// </summary>
	public class SplashScreen : System.Windows.Forms.Form
	{
		private string statusInfo = String.Empty;
		private string versionInfo = String.Empty;

		private RectangleF rectStatus, rectVersion = RectangleF.Empty; 
		private Font statusFont = new Font("Tahoma", 8, FontStyle.Regular);
		private System.Windows.Forms.Label labelSlogan;
		private Font versionFont = new Font("Tahoma", 8, FontStyle.Bold);
		
		public string StatusInfo {
			set {	statusInfo = value; ApplyChanges();	}
			get {	return statusInfo;	}
		}

		public string VersionInfo {
			set {	versionInfo = value; ApplyChanges();	}
			get {	return versionInfo;	}
		}

		private void ApplyChanges() {
			try {
				if (this.InvokeRequired) {
					this.Invoke(new MethodInvoker(this.Invalidate));
					return;
				}

				this.Invalidate();
			}
			catch {
				//	do something here...
			}
		}

		
		
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SplashScreen()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			InitDrawingRectangles();

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SplashScreen));
			this.labelSlogan = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// labelSlogan
			// 
			this.labelSlogan.AccessibleDescription = resources.GetString("labelSlogan.AccessibleDescription");
			this.labelSlogan.AccessibleName = resources.GetString("labelSlogan.AccessibleName");
			this.labelSlogan.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelSlogan.Anchor")));
			this.labelSlogan.AutoSize = ((bool)(resources.GetObject("labelSlogan.AutoSize")));
			this.labelSlogan.BackColor = System.Drawing.Color.Transparent;
			this.labelSlogan.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelSlogan.Dock")));
			this.labelSlogan.Enabled = ((bool)(resources.GetObject("labelSlogan.Enabled")));
			this.labelSlogan.Font = ((System.Drawing.Font)(resources.GetObject("labelSlogan.Font")));
			this.labelSlogan.ForeColor = System.Drawing.SystemColors.WindowText;
			this.labelSlogan.Image = ((System.Drawing.Image)(resources.GetObject("labelSlogan.Image")));
			this.labelSlogan.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelSlogan.ImageAlign")));
			this.labelSlogan.ImageIndex = ((int)(resources.GetObject("labelSlogan.ImageIndex")));
			this.labelSlogan.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelSlogan.ImeMode")));
			this.labelSlogan.Location = ((System.Drawing.Point)(resources.GetObject("labelSlogan.Location")));
			this.labelSlogan.Name = "labelSlogan";
			this.labelSlogan.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelSlogan.RightToLeft")));
			this.labelSlogan.Size = ((System.Drawing.Size)(resources.GetObject("labelSlogan.Size")));
			this.labelSlogan.TabIndex = ((int)(resources.GetObject("labelSlogan.TabIndex")));
			this.labelSlogan.Text = resources.GetString("labelSlogan.Text");
			this.labelSlogan.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelSlogan.TextAlign")));
			this.labelSlogan.UseMnemonic = false;
			this.labelSlogan.Visible = ((bool)(resources.GetObject("labelSlogan.Visible")));
			// 
			// SplashScreen
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.labelSlogan);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.ForeColor = System.Drawing.SystemColors.WindowText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "SplashScreen";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.TransparencyKey = System.Drawing.Color.Magenta;
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.OnFormPaint);
			this.ResumeLayout(false);

		}
		#endregion

		private void OnFormPaint(object sender, System.Windows.Forms.PaintEventArgs e) {
			Graphics g = e.Graphics;

			StringFormat f = new StringFormat();
			
			if (Win32.IsOSAtLeastWindowsXP) {
				g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			} else {
				g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
			}

			f.Alignment = StringAlignment.Far;
			f.LineAlignment = StringAlignment.Near;
			g.DrawString(versionInfo, versionFont, new SolidBrush(this.ForeColor), rectVersion , f);

			f.Alignment = StringAlignment.Near;
			f.LineAlignment = StringAlignment.Near;
			g.DrawString(statusInfo, statusFont, new SolidBrush(this.ForeColor), rectStatus , f);
		}

		private void InitDrawingRectangles() {
			int boxWidth = this.ClientSize.Width-20;
			// 0,0 is the top left corner of the window:
			rectVersion = new RectangleF(new Point(10, 190),
				new Size(boxWidth, Convert.ToInt32(versionFont.Size) + 20));		// one line
			rectStatus = new RectangleF(new Point(10, 10),
				new Size(boxWidth, Convert.ToInt32(statusFont.Size * 2) + 25));	// two lines
		}

	}

	public class Splash {
		static SplashScreen MySplashForm = null;
		static Thread MySplashThread = null;

		//	internally used as a thread function - showing the form and
		//	starting the messageloop for it
		static void ShowThread() {
            Application.EnableVisualStyles();
			MySplashForm = new SplashScreen();
			Application.Run(MySplashForm);
		}

		//	public Method to show the SplashForm
		static public void Show() {
			if (MySplashThread != null)
				return;

			MySplashThread = new Thread(new ThreadStart(Splash.ShowThread));
			// take over the culture settings from main/default thread 
			// (if not, Splash will not care about a change of the culture in the main thread)
			MySplashThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			MySplashThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			MySplashThread.IsBackground = true;
			MySplashThread.SetApartmentState(ApartmentState.STA);
			MySplashThread.Start();
			while (MySplashForm == null) Thread.Sleep(new TimeSpan(100));
		}

		//	public Method to hide the SplashForm
		static public void Close() {
			if (MySplashThread == null) return;
			if (MySplashForm == null) return;

			try {
				MySplashForm.Invoke(new MethodInvoker(MySplashForm.Close));
			}
			catch (Exception) {
			}
			MySplashThread = null;
			MySplashForm = null;
		}

		//	public Method to set or get the loading Status
		static public string Status {
			set {
				if (MySplashForm == null) {
					return;
				}

				MySplashForm.StatusInfo = value;
			}
			get {
				if (MySplashForm == null) {
					throw new InvalidOperationException("Splash Form not on screen");
				}
				return MySplashForm.StatusInfo;
			}
		}

		//	public Method to set or get the loading Status
		static public string Version {
			set {
				if (MySplashForm == null) {
					return;
				}

				MySplashForm.VersionInfo = value;
			}
			get {
				if (MySplashForm == null) {
					throw new InvalidOperationException("Splash Form not on screen");
				}
				return MySplashForm.VersionInfo;
			}
		}
	}

}
