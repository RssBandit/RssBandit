namespace RssBandit.WinGui.Dialogs
{
	partial class FeedSourceProperties
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
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
			this.grpCredentials = new System.Windows.Forms.GroupBox();
			this.txtUsername = new System.Windows.Forms.TextBox();
			this.lblUsername = new System.Windows.Forms.Label();
			this.txtFeedSourceName = new System.Windows.Forms.TextBox();
			this.lblFeedSourceName = new System.Windows.Forms.Label();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.lblPassword = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
			this.grpCredentials.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(208, 251);
			// 
			// horizontalEdge
			// 
			this.horizontalEdge.Location = new System.Drawing.Point(-1, 239);
			this.horizontalEdge.Size = new System.Drawing.Size(320, 2);
			// 
			// btnSubmit
			// 
			this.btnSubmit.Location = new System.Drawing.Point(108, 251);
			// 
			// grpCredentials
			// 
			this.grpCredentials.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.grpCredentials.Controls.Add(this.txtPassword);
			this.grpCredentials.Controls.Add(this.lblPassword);
			this.grpCredentials.Controls.Add(this.txtUsername);
			this.grpCredentials.Controls.Add(this.lblUsername);
			this.grpCredentials.Location = new System.Drawing.Point(12, 89);
			this.grpCredentials.Name = "grpCredentials";
			this.grpCredentials.Size = new System.Drawing.Size(285, 133);
			this.grpCredentials.TabIndex = 102;
			this.grpCredentials.TabStop = false;
			this.grpCredentials.Text = "Credentials";
			// 
			// txtUsername
			// 
			this.txtUsername.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtUsername.Location = new System.Drawing.Point(8, 46);
			this.txtUsername.Name = "txtUsername";
			this.txtUsername.Size = new System.Drawing.Size(263, 21);
			this.txtUsername.TabIndex = 105;
			// 
			// lblUsername
			// 
			this.lblUsername.AutoSize = true;
			this.lblUsername.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblUsername.Location = new System.Drawing.Point(9, 27);
			this.lblUsername.Name = "lblUsername";
			this.lblUsername.Size = new System.Drawing.Size(59, 13);
			this.lblUsername.TabIndex = 106;
			this.lblUsername.Text = "Username:";
			// 
			// txtFeedSourceName
			// 
			this.txtFeedSourceName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtFeedSourceName.Location = new System.Drawing.Point(20, 47);
			this.txtFeedSourceName.Name = "txtFeedSourceName";
			this.txtFeedSourceName.Size = new System.Drawing.Size(263, 21);
			this.txtFeedSourceName.TabIndex = 103;
			// 
			// lblFeedSourceName
			// 
			this.lblFeedSourceName.AutoSize = true;
			this.lblFeedSourceName.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblFeedSourceName.Location = new System.Drawing.Point(21, 28);
			this.lblFeedSourceName.Name = "lblFeedSourceName";
			this.lblFeedSourceName.Size = new System.Drawing.Size(101, 13);
			this.lblFeedSourceName.TabIndex = 104;
			this.lblFeedSourceName.Text = "Feed Source Name:";
			// 
			// txtPassword
			// 
			this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtPassword.Location = new System.Drawing.Point(8, 100);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.Size = new System.Drawing.Size(263, 21);
			this.txtPassword.TabIndex = 107;
			this.txtPassword.UseSystemPasswordChar = true;
			// 
			// lblPassword
			// 
			this.lblPassword.AutoSize = true;
			this.lblPassword.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblPassword.Location = new System.Drawing.Point(9, 81);
			this.lblPassword.Name = "lblPassword";
			this.lblPassword.Size = new System.Drawing.Size(57, 13);
			this.lblPassword.TabIndex = 108;
			this.lblPassword.Text = "Password:";
			// 
			// FeedSourceProperties
			// 
			this.ClientSize = new System.Drawing.Size(310, 284);
			this.Controls.Add(this.grpCredentials);
			this.Controls.Add(this.txtFeedSourceName);
			this.Controls.Add(this.lblFeedSourceName);
			this.MinimumSize = new System.Drawing.Size(318, 318);
			this.Name = "FeedSourceProperties";
			this.Text = "Feed Source Properties";
			this.Controls.SetChildIndex(this.lblFeedSourceName, 0);
			this.Controls.SetChildIndex(this.txtFeedSourceName, 0);
			this.Controls.SetChildIndex(this.grpCredentials, 0);
			this.Controls.SetChildIndex(this.btnSubmit, 0);
			this.Controls.SetChildIndex(this.btnCancel, 0);
			this.Controls.SetChildIndex(this.horizontalEdge, 0);
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
			this.grpCredentials.ResumeLayout(false);
			this.grpCredentials.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox grpCredentials;
		private System.Windows.Forms.TextBox txtUsername;
		private System.Windows.Forms.Label lblUsername;
		private System.Windows.Forms.TextBox txtFeedSourceName;
		private System.Windows.Forms.Label lblFeedSourceName;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.Label lblPassword;
	}
}
