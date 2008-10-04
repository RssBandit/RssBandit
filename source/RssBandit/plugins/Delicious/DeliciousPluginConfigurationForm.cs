using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

namespace BlogExtension.Delicious 
{
	public class DeliciousPluginConfigurationForm : System.Windows.Forms.Form
	{
		public string TemplateText;

		private System.Windows.Forms.Button _btOK;
		private System.Windows.Forms.Button _btCancel;
		private System.Windows.Forms.GroupBox groupBox1;
		internal System.Windows.Forms.TextBox textPwd;
		private System.Windows.Forms.Label label7;
		internal System.Windows.Forms.TextBox textUser;
		private System.Windows.Forms.Label label6;
		internal System.Windows.Forms.TextBox textUri;
		private System.Windows.Forms.Label label2;
		private System.ComponentModel.Container components = null;


		public DeliciousPluginConfigurationForm(string username, string password, string apiUrl)
		{
			InitializeComponent();
			InitializeComponentTranslation();
			textUri.Text  = apiUrl; 
			textUser.Text = username; 
			textPwd.Text  = password; 
		
		}

		private void InitializeComponentTranslation() {
			this._btOK.Text = Resource.Manager["RES_DeliciousFormOk"];
			this._btCancel.Text = Resource.Manager["RES_DeliciousFormCancel"];
			this.groupBox1.Text = Resource.Manager["RES_DeliciousFormAuthentication"];
			this.label7.Text = Resource.Manager["RES_DeliciousFormPassword"];
			this.label6.Text = Resource.Manager["RES_DeliciousFormUsername"];
			this.label2.Text = Resource.Manager["RES_DeliciousFormApiUrl"];
			this.Text = Resource.Manager["RES_DeliciousFormConfiguration"];
		}
		

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
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
			this._btOK = new System.Windows.Forms.Button();
			this._btCancel = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.textPwd = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.textUser = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.textUri = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// _btOK
			// 
			this._btOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._btOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._btOK.Location = new System.Drawing.Point(240, 147);
			this._btOK.Name = "_btOK";
			this._btOK.Size = new System.Drawing.Size(75, 23);
			this._btOK.TabIndex = 5;
			this._btOK.Text = "OK";
			// 
			// _btCancel
			// 
			this._btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._btCancel.Location = new System.Drawing.Point(328, 147);
			this._btCancel.Name = "_btCancel";
			this._btCancel.Size = new System.Drawing.Size(75, 23);
			this._btCancel.TabIndex = 4;
			this._btCancel.Text = "Cancel";
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.textPwd);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.textUser);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(4, 48);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(406, 80);
			this.groupBox1.TabIndex = 36;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Authentication";
			// 
			// textPwd
			// 
			this.textPwd.AllowDrop = true;
			this.textPwd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textPwd.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.textPwd.Location = new System.Drawing.Point(128, 50);
			this.textPwd.Name = "textPwd";
			this.textPwd.PasswordChar = '*';
			this.textPwd.Size = new System.Drawing.Size(221, 20);
			this.textPwd.TabIndex = 36;
			// 
			// label7
			// 
			this.label7.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label7.Location = new System.Drawing.Point(25, 55);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(95, 16);
			this.label7.TabIndex = 35;
			this.label7.Text = "&Password";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textUser
			// 
			this.textUser.AllowDrop = true;
			this.textUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textUser.Location = new System.Drawing.Point(128, 26);
			this.textUser.Name = "textUser";
			this.textUser.Size = new System.Drawing.Size(221, 20);
			this.textUser.TabIndex = 34;
			// 
			// label6
			// 
			this.label6.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label6.Location = new System.Drawing.Point(25, 30);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(95, 16);
			this.label6.TabIndex = 33;
			this.label6.Text = "User&name";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textUri
			// 
			this.textUri.AllowDrop = true;
			this.textUri.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textUri.Location = new System.Drawing.Point(132, 14);
			this.textUri.Name = "textUri";
			this.textUri.Size = new System.Drawing.Size(276, 20);
			this.textUri.TabIndex = 38;
			// 
			// label2
			// 
			this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label2.Location = new System.Drawing.Point(8, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(116, 32);
			this.label2.TabIndex = 37;
			this.label2.Text = "&del.icio.us API URL";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// DeliciousPluginConfigurationForm
			// 
			this.AcceptButton = this._btOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this._btCancel;
			this.ClientSize = new System.Drawing.Size(416, 181);
			this.ControlBox = false;
			this.Controls.Add(this.textUri);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this._btOK);
			this.Controls.Add(this._btCancel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(424, 215);
			this.Name = "DeliciousPluginConfigurationForm";
			this.Text = "Del.icio.us Plugin Configuration...";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		
		
	}
}
