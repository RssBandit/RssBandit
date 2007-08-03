using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

namespace BlogExtension.Delicious 
{
	public class DeliciousPostForm : System.Windows.Forms.Form
	{
		public string TemplateText;

		internal System.Windows.Forms.TextBox textUri;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button btnPost;
		private System.Windows.Forms.Button btnCancel;
		internal System.Windows.Forms.TextBox textDescription;
		internal System.Windows.Forms.TextBox textTags;
		private System.ComponentModel.Container components = null;


		public DeliciousPostForm(string url, string description)
		{
			InitializeComponent();
			InitializeComponentTranslation();
			textUri.Text = url; 
			textDescription.Text = description;
		}

		private void InitializeComponentTranslation() {
			this.btnPost.Text = Resource.Manager["RES_DeliciousFormPost"];
			this.btnCancel.Text = Resource.Manager["RES_DeliciousFormCancel"];
			this.label2.Text = Resource.Manager["RES_DeliciousFormUrl"];
			this.label1.Text = Resource.Manager["RES_DeliciousFormDescription"];
			this.label3.Text = Resource.Manager["RES_DeliciousFormTags"];
			this.Text = Resource.Manager["RES_MenuDeliciousCaption"];
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
			this.btnPost = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.textUri = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textDescription = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.textTags = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// btnPost
			// 
			this.btnPost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnPost.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnPost.Location = new System.Drawing.Point(240, 123);
			this.btnPost.Name = "btnPost";
			this.btnPost.TabIndex = 5;
			this.btnPost.Text = "Post";
			this.btnPost.Click += new System.EventHandler(this._btOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(328, 123);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 4;
			this.btnCancel.Text = "Cancel";
			// 
			// textUri
			// 
			this.textUri.AllowDrop = true;
			this.textUri.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.textUri.Location = new System.Drawing.Point(88, 8);
			this.textUri.Name = "textUri";
			this.textUri.Size = new System.Drawing.Size(320, 20);
			this.textUri.TabIndex = 38;
			this.textUri.Text = "";
			// 
			// label2
			// 
			this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label2.Location = new System.Drawing.Point(8, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 32);
			this.label2.TabIndex = 37;
			this.label2.Text = "&URL";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDescription
			// 
			this.textDescription.AllowDrop = true;
			this.textDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.textDescription.Location = new System.Drawing.Point(88, 48);
			this.textDescription.Name = "textDescription";
			this.textDescription.Size = new System.Drawing.Size(320, 20);
			this.textDescription.TabIndex = 40;
			this.textDescription.Text = "";
			// 
			// label1
			// 
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label1.Location = new System.Drawing.Point(16, 48);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 32);
			this.label1.TabIndex = 39;
			this.label1.Text = "&Description";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label3.Location = new System.Drawing.Point(8, 88);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(72, 32);
			this.label3.TabIndex = 41;
			this.label3.Text = "&Tags";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textTags
			// 
			this.textTags.Location = new System.Drawing.Point(88, 88);
			this.textTags.Name = "textTags";
			this.textTags.Size = new System.Drawing.Size(320, 20);
			this.textTags.TabIndex = 42;
			this.textTags.Text = "";
			// 
			// DeliciousPostForm
			// 
			this.AcceptButton = this.btnPost;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(416, 157);
			this.ControlBox = false;
			this.Controls.Add(this.textTags);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textDescription);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textUri);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btnPost);
			this.Controls.Add(this.btnCancel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DeliciousPostForm";
			this.Text = "Post to del.icio.us";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DeliciousPostForm_Closing);
			this.ResumeLayout(false);

		}
		#endregion

		

		private void DeliciousPostForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{		
	
			if(this.DialogResult == DialogResult.Retry){
				e.Cancel = true; 
			}
		}

		private void _btOK_Click(object sender, System.EventArgs e)
		{					
			
			if((textDescription.Text.Length == 0) || 
				(textTags.Text.Length == 0) ||
				(textUri.Text.Length == 0)){
				
				MessageBox.Show(this, Resource.Manager["RES_DeliciousIncompleteSubmissionMessage"], Resource.Manager["RES_DeliciousIncompleteSubmissionTitle"],  MessageBoxButtons.OK, MessageBoxIcon.Error);
				this.DialogResult = DialogResult.Retry; 
			}		
			
		}
		
	}
}
