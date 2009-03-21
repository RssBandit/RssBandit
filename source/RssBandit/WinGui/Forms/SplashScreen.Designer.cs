

namespace RssBandit.WinGui.Forms
{
	partial class SplashScreen
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashScreen));
			this.labelSlogan = new System.Windows.Forms.Label();
			this.lblStatusInfo = new System.Windows.Forms.Label();
			this.lblVersionInfo = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// labelSlogan
			// 
			this.labelSlogan.BackColor = System.Drawing.Color.Transparent;
			this.labelSlogan.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.labelSlogan.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.labelSlogan.ForeColor = System.Drawing.SystemColors.WindowText;
			this.labelSlogan.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.labelSlogan.Location = new System.Drawing.Point(0, 232);
			this.labelSlogan.Name = "labelSlogan";
			this.labelSlogan.Size = new System.Drawing.Size(376, 27);
			this.labelSlogan.TabIndex = 0;
			this.labelSlogan.Text = "Your desktop news aggregator";
			this.labelSlogan.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.labelSlogan.UseMnemonic = false;
			// 
			// lblStatusInfo
			// 
			this.lblStatusInfo.BackColor = System.Drawing.Color.Transparent;
			this.lblStatusInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.lblStatusInfo.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblStatusInfo.ForeColor = System.Drawing.SystemColors.WindowText;
			this.lblStatusInfo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.lblStatusInfo.Location = new System.Drawing.Point(0, 259);
			this.lblStatusInfo.Name = "lblStatusInfo";
			this.lblStatusInfo.Size = new System.Drawing.Size(376, 27);
			this.lblStatusInfo.TabIndex = 1;
			this.lblStatusInfo.Text = "Initializing...";
			this.lblStatusInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblStatusInfo.UseMnemonic = false;
			// 
			// lblVersionInfo
			// 
			this.lblVersionInfo.BackColor = System.Drawing.Color.Transparent;
			this.lblVersionInfo.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblVersionInfo.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblVersionInfo.ForeColor = System.Drawing.SystemColors.WindowText;
			this.lblVersionInfo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.lblVersionInfo.Location = new System.Drawing.Point(0, 0);
			this.lblVersionInfo.Name = "lblVersionInfo";
			this.lblVersionInfo.Size = new System.Drawing.Size(376, 27);
			this.lblVersionInfo.TabIndex = 2;
			this.lblVersionInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblVersionInfo.UseMnemonic = false;
			// 
			// SplashScreen
			// 
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.ClientSize = new System.Drawing.Size(376, 286);
			this.Controls.Add(this.lblVersionInfo);
			this.Controls.Add(this.labelSlogan);
			this.Controls.Add(this.lblStatusInfo);
			this.DoubleBuffered = true;
			this.Font = new System.Drawing.Font("Tahoma", 8.25F);
			this.ForeColor = System.Drawing.SystemColors.WindowText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "SplashScreen";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Welcome!";
			this.TransparencyKey = System.Drawing.Color.Magenta;
			this.Click += new System.EventHandler(this.OnFormClick);
			this.ResumeLayout(false);

		}
		#endregion

		

	}
}
