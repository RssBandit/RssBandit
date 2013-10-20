

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
			this.labelSlogan = new System.Windows.Forms.Label();
			this.lblStatusInfo = new System.Windows.Forms.Label();
			this.lblVersionInfo = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// labelSlogan
			// 
			this.labelSlogan.BackColor = System.Drawing.Color.Transparent;
			this.labelSlogan.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.labelSlogan.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelSlogan.ForeColor = System.Drawing.Color.Gainsboro;
			this.labelSlogan.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.labelSlogan.Location = new System.Drawing.Point(0, 262);
			this.labelSlogan.Name = "labelSlogan";
			this.labelSlogan.Padding = new System.Windows.Forms.Padding(10, 3, 10, 1);
			this.labelSlogan.Size = new System.Drawing.Size(550, 32);
			this.labelSlogan.TabIndex = 0;
			this.labelSlogan.Text = "Your desktop news aggregator";
			this.labelSlogan.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labelSlogan.UseMnemonic = false;
			// 
			// lblStatusInfo
			// 
			this.lblStatusInfo.BackColor = System.Drawing.Color.Transparent;
			this.lblStatusInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.lblStatusInfo.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
			this.lblStatusInfo.ForeColor = System.Drawing.Color.Gainsboro;
			this.lblStatusInfo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.lblStatusInfo.Location = new System.Drawing.Point(0, 294);
			this.lblStatusInfo.Name = "lblStatusInfo";
			this.lblStatusInfo.Padding = new System.Windows.Forms.Padding(5, 1, 10, 5);
			this.lblStatusInfo.Size = new System.Drawing.Size(550, 30);
			this.lblStatusInfo.TabIndex = 1;
			this.lblStatusInfo.Text = "Initializing...";
			this.lblStatusInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lblStatusInfo.UseMnemonic = false;
			// 
			// lblVersionInfo
			// 
			this.lblVersionInfo.BackColor = System.Drawing.Color.Transparent;
			this.lblVersionInfo.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblVersionInfo.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold);
			this.lblVersionInfo.ForeColor = System.Drawing.Color.Gainsboro;
			this.lblVersionInfo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.lblVersionInfo.Location = new System.Drawing.Point(0, 0);
			this.lblVersionInfo.Name = "lblVersionInfo";
			this.lblVersionInfo.Padding = new System.Windows.Forms.Padding(10, 10, 5, 5);
			this.lblVersionInfo.Size = new System.Drawing.Size(550, 39);
			this.lblVersionInfo.TabIndex = 2;
			this.lblVersionInfo.Text = "Version";
			this.lblVersionInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblVersionInfo.UseMnemonic = false;
			// 
			// SplashScreen
			// 
			this.BackColor = System.Drawing.Color.Black;
			this.BackgroundImage = global::RssBandit.Properties.Resources.splash2;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.ClientSize = new System.Drawing.Size(550, 324);
			this.Controls.Add(this.lblVersionInfo);
			this.Controls.Add(this.labelSlogan);
			this.Controls.Add(this.lblStatusInfo);
			this.DoubleBuffered = true;
			this.Font = new System.Drawing.Font("Tahoma", 8.25F);
			this.ForeColor = System.Drawing.SystemColors.WindowText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "SplashScreen";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Welcome!";
			this.TransparencyKey = System.Drawing.Color.Magenta;
			this.Click += new System.EventHandler(this.OnFormClick);
			this.ResumeLayout(false);

		}
		#endregion

		

	}
}
