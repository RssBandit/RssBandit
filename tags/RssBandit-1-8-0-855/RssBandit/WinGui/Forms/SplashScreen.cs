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
using System.Threading;
using System.Windows.Forms;
using RssBandit.Resources;

namespace RssBandit.WinGui.Forms
{
	/// <summary>
	/// Summary description for SplashScreen.
	/// </summary>
	public class SplashScreen : Form
	{
		private string statusInfo = String.Empty;
		private string versionInfo = String.Empty;

		private RectangleF rectStatus, rectVersion = RectangleF.Empty; 
		private readonly Font statusFont = new Font("Tahoma", 8, FontStyle.Regular);
		private Label labelSlogan;
		private readonly Font versionFont = new Font("Tahoma", 8, FontStyle.Bold);
		
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
			base.Text = SR.MainForm_DetailHeaderCaption_AtStartup;
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashScreen));
			this.labelSlogan = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// labelSlogan
			// 
			this.labelSlogan.BackColor = System.Drawing.Color.Transparent;
			this.labelSlogan.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelSlogan.ForeColor = System.Drawing.SystemColors.WindowText;
			this.labelSlogan.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.labelSlogan.Location = new System.Drawing.Point(0, 240);
			this.labelSlogan.Name = "labelSlogan";
			this.labelSlogan.Size = new System.Drawing.Size(365, 27);
			this.labelSlogan.TabIndex = 0;
			this.labelSlogan.Text = "Your desktop news aggregator";
			this.labelSlogan.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.labelSlogan.UseMnemonic = false;
			// 
			// SplashScreen
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = new System.Drawing.Size(365, 270);
			this.Controls.Add(this.labelSlogan);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F);
			this.ForeColor = System.Drawing.SystemColors.WindowText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "SplashScreen";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Welcome!";
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
		static SplashScreen MySplashForm;
		static Thread MySplashThread;

		//	internally used as a thread function - showing the form and
		//	starting the messageloop for it
		static void ShowThread() {
            MySplashForm = new SplashScreen();
			Application.Run(MySplashForm);
		}

		//	public Method to show the SplashForm
		static public void Show() {
			if (MySplashThread != null)
				return;

			MySplashThread = new Thread(ShowThread);
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
