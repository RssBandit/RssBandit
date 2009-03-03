using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AmphetaRatePlugin
{
	/// <summary>
	/// Summary description for ConfigurationForm.
	/// </summary>
	public class ConfigurationForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnGenerate;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.TextBox txtID;
		private System.Windows.Forms.Label lblDescription;
		private System.Windows.Forms.Label lblID;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.LinkLabel lnkSubscribeURL;
		private System.Windows.Forms.Label lblSubscribeUrl;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ConfigurationForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.lblSubscribeUrl.Visible = false;
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

		/// <summary>
		/// Gets/Sets the AmphetaRate ID.
		/// </summary>
		public string AmphetaRateID
		{
			get
			{
				return this.txtID.Text;
			}
			set
			{
				this.txtID.Text = value;
				if(value.Length > 0)
				{
					this.txtID_TextChanged(null, null);
				}
			}
		}

		bool IsValidID(string id)
		{
			return Regex.IsMatch(id, "^[a-z]{3}[0-9]{3}$", RegexOptions.IgnoreCase);
		}

		protected override void OnLoad(EventArgs e)
		{
			
			base.OnLoad (e);
		}


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnGenerate = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.txtID = new System.Windows.Forms.TextBox();
			this.lblID = new System.Windows.Forms.Label();
			this.lblDescription = new System.Windows.Forms.Label();
			this.lnkSubscribeURL = new System.Windows.Forms.LinkLabel();
			this.lblSubscribeUrl = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnGenerate
			// 
			this.btnGenerate.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnGenerate.Location = new System.Drawing.Point(272, 56);
			this.btnGenerate.Name = "btnGenerate";
			this.btnGenerate.Size = new System.Drawing.Size(72, 23);
			this.btnGenerate.TabIndex = 0;
			this.btnGenerate.Text = "Generate ID";
			this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
			// 
			// btnOK
			// 
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnOK.Location = new System.Drawing.Point(192, 176);
			this.btnOK.Name = "btnOK";
			this.btnOK.TabIndex = 1;
			this.btnOK.Text = "OK";
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnCancel.Location = new System.Drawing.Point(272, 176);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Cancel";
			// 
			// txtID
			// 
			this.txtID.Location = new System.Drawing.Point(104, 56);
			this.txtID.Name = "txtID";
			this.txtID.Size = new System.Drawing.Size(160, 20);
			this.txtID.TabIndex = 3;
			this.txtID.Text = "";
			this.txtID.TextChanged += new System.EventHandler(this.txtID_TextChanged);
			// 
			// lblID
			// 
			this.lblID.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblID.Location = new System.Drawing.Point(8, 56);
			this.lblID.Name = "lblID";
			this.lblID.Size = new System.Drawing.Size(88, 23);
			this.lblID.TabIndex = 4;
			this.lblID.Text = "AmphetaRate ID:";
			// 
			// lblDescription
			// 
			this.lblDescription.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblDescription.Location = new System.Drawing.Point(8, 8);
			this.lblDescription.Name = "lblDescription";
			this.lblDescription.Size = new System.Drawing.Size(336, 40);
			this.lblDescription.TabIndex = 5;
			// 
			// lnkSubscribeURL
			// 
			this.lnkSubscribeURL.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lnkSubscribeURL.Location = new System.Drawing.Point(8, 112);
			this.lnkSubscribeURL.Name = "lnkSubscribeURL";
			this.lnkSubscribeURL.Size = new System.Drawing.Size(336, 40);
			this.lnkSubscribeURL.TabIndex = 6;
			this.lnkSubscribeURL.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkSubscribeURL_LinkClicked);
			// 
			// lblSubscribeUrl
			// 
			this.lblSubscribeUrl.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblSubscribeUrl.Location = new System.Drawing.Point(8, 88);
			this.lblSubscribeUrl.Name = "lblSubscribeUrl";
			this.lblSubscribeUrl.Size = new System.Drawing.Size(208, 23);
			this.lblSubscribeUrl.TabIndex = 7;
			this.lblSubscribeUrl.Text = "Subscribe To Personalized Feed:";
			// 
			// ConfigurationForm
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(352, 206);
			this.Controls.Add(this.lblSubscribeUrl);
			this.Controls.Add(this.lnkSubscribeURL);
			this.Controls.Add(this.lblDescription);
			this.Controls.Add(this.lblID);
			this.Controls.Add(this.txtID);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.btnGenerate);
			this.Name = "ConfigurationForm";
			this.Text = "AmphetaRate Configuration";
			this.ResumeLayout(false);

		}
		#endregion

		private void btnGenerate_Click(object sender, System.EventArgs e)
		{
			string url = "http://amphetarate.sourceforge.net/dinka-create-uid.php";
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
			request.Method = "GET";
			
			using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			{
				using(StreamReader reader = new StreamReader(response.GetResponseStream()))
				{
					this.AmphetaRateID = reader.ReadToEnd();
				}
			}
		}

		private void btnOK_Click(object sender, System.EventArgs e)
		{
			if(!IsValidID(this.txtID.Text))
			{
				MessageBox.Show(this, "Sorry, that is an invalid AmphetaRate id.  The ID should have six characters. The first three are letters and the last three are numbers.");
				this.DialogResult = DialogResult.None;
			}
		}

		private void txtID_TextChanged(object sender, EventArgs e)
		{
			if(IsValidID(this.txtID.Text))
			{
				this.lblSubscribeUrl.Visible = true;
				this.lnkSubscribeURL.Text = "http://amphetarate.sourceforge.net/dinka-get-rss.php?uid=" + this.txtID.Text;
				this.lblDescription.Text = "Your AmphetaRateID is displayed in the text box below. " 
					+ "If it is incorrect, feel free to correct it by typing in the correct ID or " 
					+ "click the 'Generate' button to generate a new ID.";
			}
			else
			{
				this.lblDescription.Text = "You do not have an AmphetaRateID configured yet. " 
					+ "If you already have an ID, please enter it in the text box below. " 
					+ "Otherwise, click the 'Generate' button to generate a new ID.";
			}	
		}

		private void lnkSubscribeURL_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if(IsValidID(this.txtID.Text))
			{
				object banditApp = this.Owner.GetType().InvokeMember("GuiOwner", BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty, null, this.Owner, null);
				if(banditApp != null)
				{
					bool result = (bool)banditApp.GetType().InvokeMember("CmdNewFeed", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, banditApp, new object[] {null, this.lnkSubscribeURL.Text, "Personalized AmphetaRate Feed"});
				}
			}
		}
	}
}
