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
using System.Threading;
using System.Windows.Forms;
using RssBandit.Resources;

namespace RssBandit.WinGui.Forms
{
	/// <summary>
	/// Summary description for SplashScreen.
	/// </summary>
	partial class SplashScreen : Form
	{
		private Label lblStatusInfo;
		private Label lblVersionInfo;
		private Label labelSlogan;

		private string statusInfo;
		private string versionInfo;

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
		}

		public void SetInfos(string status, string version)
		{
			statusInfo = status;
			versionInfo = version;
			ApplyChanges();
		}

		public string StatusInfo
		{
			set { statusInfo = value; ApplyChanges(); }
			get { return statusInfo; }
		}

		public string VersionInfo
		{
			set { versionInfo = value; ApplyChanges(); }
			get { return versionInfo; }
		}

		private void ApplyChanges()
		{
			try
			{
				if (InvokeRequired)
				{
					Invoke(new MethodInvoker(this.ApplyChanges));
					return;
				}

				if (this.lblStatusInfo.Text != statusInfo)
					this.lblStatusInfo.Text = statusInfo;
				
				if (this.lblVersionInfo.Text != versionInfo)
					this.lblVersionInfo.Text = versionInfo;
			}
			catch
			{
				//	do something here...
			}
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
				    components = null;
				}
			}
			base.Dispose( disposing );
		}

		private void OnFormClick(object sender, EventArgs e)
		{
			this.Close();
		}

	}

	/// <summary>
	/// The Splash screen controller class
	/// </summary>
	public class Splash 
    {
		static SplashScreen MySplashForm;
		
		//	public Method to show the SplashForm
		static public void Show(string status, string version) 
        {
            // (initially not signalled)
            using (ManualResetEvent splashThreadStartedSignal = new ManualResetEvent(false))
            {
                ThreadPool.QueueUserWorkItem(
                    delegate
                        {
                            MySplashForm = new SplashScreen();
							MySplashForm.SetInfos(status, version);
                            splashThreadStartedSignal.Set();

                            Application.Run(MySplashForm); 
                        });

                // wait until splash thread signals it was started and displayed:
                splashThreadStartedSignal.WaitOne();
                
            } // using() free resources
        }

		//	public Method to hide the SplashForm
		static public void Close() 
        {
			if (MySplashForm == null) return;

			try
			{
				if (MySplashForm.IsHandleCreated)
					MySplashForm.Invoke(new MethodInvoker(MySplashForm.Close));
			}
			catch (Exception) {
			}
			MySplashForm = null;
		}

		//	public Method to set or get the loading Status
		static public string Status 
        {
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
