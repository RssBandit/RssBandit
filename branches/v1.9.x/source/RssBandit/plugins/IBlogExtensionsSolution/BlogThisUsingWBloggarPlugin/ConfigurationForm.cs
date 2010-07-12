using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Haack.Rss.BlogExtensions
{
	/// <summary>
	/// Summary description for ConfigurationForm.
	/// </summary>
	public class ConfigurationForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton radLinkOnly;
		private System.Windows.Forms.RadioButton radFullText;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.RadioButton radLinkWithAuthor;
		private System.Windows.Forms.GroupBox groupBox2;
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

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
		/// Gets the blog type.
		/// </summary>
		/// <value></value>
		internal BlogThisUsingWbloggarPlugin.BlogThisType BlogType
		{
			get
			{
				if(this.radFullText.Checked)
					return BlogThisUsingWbloggarPlugin.BlogThisType.Full;
				if(this.radLinkOnly.Checked)
					return BlogThisUsingWbloggarPlugin.BlogThisType.LinkOnly;
				if(this.radLinkWithAuthor.Checked)
					return BlogThisUsingWbloggarPlugin.BlogThisType.LinkWithAuthor;

				//Set this as the default.
				return BlogThisUsingWbloggarPlugin.BlogThisType.LinkOnly;
			}

			set
			{
				switch(value)
				{
					case BlogThisUsingWbloggarPlugin.BlogThisType.Full:
						this.radFullText.Checked = true;
						break;
					case BlogThisUsingWbloggarPlugin.BlogThisType.LinkOnly:
						this.radLinkOnly.Checked = true;
						break;
				}
			}
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.radFullText = new System.Windows.Forms.RadioButton();
			this.radLinkOnly = new System.Windows.Forms.RadioButton();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.radLinkWithAuthor = new System.Windows.Forms.RadioButton();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label1.Location = new System.Drawing.Point(8, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(280, 32);
			this.label1.TabIndex = 0;
			this.label1.Text = "What information do you want to send to w.bloggar when using Blog This With w.blo" +
				"ggar?";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.groupBox2);
			this.groupBox1.Controls.Add(this.btnCancel);
			this.groupBox1.Controls.Add(this.btnOK);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox1.Location = new System.Drawing.Point(0, 0);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(292, 266);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Blog This Settings";
			// 
			// radFullText
			// 
			this.radFullText.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radFullText.Location = new System.Drawing.Point(40, 48);
			this.radFullText.Name = "radFullText";
			this.radFullText.TabIndex = 2;
			this.radFullText.Text = "Full Text Of Post";
			// 
			// radLinkOnly
			// 
			this.radLinkOnly.Checked = true;
			this.radLinkOnly.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radLinkOnly.Location = new System.Drawing.Point(40, 16);
			this.radLinkOnly.Name = "radLinkOnly";
			this.radLinkOnly.Size = new System.Drawing.Size(112, 24);
			this.radLinkOnly.TabIndex = 1;
			this.radLinkOnly.TabStop = true;
			this.radLinkOnly.Text = "Link To Post Only";
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnOK.Location = new System.Drawing.Point(136, 240);
			this.btnOK.Name = "btnOK";
			this.btnOK.TabIndex = 3;
			this.btnOK.Text = "OK";
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnCancel.Location = new System.Drawing.Point(216, 240);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 4;
			this.btnCancel.Text = "Cancel";
			// 
			// radLinkWithAuthor
			// 
			this.radLinkWithAuthor.Location = new System.Drawing.Point(40, 80);
			this.radLinkWithAuthor.Name = "radLinkWithAuthor";
			this.radLinkWithAuthor.Size = new System.Drawing.Size(168, 24);
			this.radLinkWithAuthor.TabIndex = 5;
			this.radLinkWithAuthor.Text = "Link With Author";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.radLinkWithAuthor);
			this.groupBox2.Controls.Add(this.radLinkOnly);
			this.groupBox2.Controls.Add(this.radFullText);
			this.groupBox2.Location = new System.Drawing.Point(8, 56);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(272, 168);
			this.groupBox2.TabIndex = 6;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Options";
			// 
			// ConfigurationForm
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Controls.Add(this.groupBox1);
			this.Name = "ConfigurationForm";
			this.Text = "ConfigurationForm";
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
