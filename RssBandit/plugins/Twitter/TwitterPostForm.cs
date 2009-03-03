using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

namespace BlogExtension.Twitter 
{
	public class TwitterPostForm : System.Windows.Forms.Form
	{
		public string TemplateText;

        internal System.Windows.Forms.TextBox textPost;
        private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnPost;
        private System.Windows.Forms.Button btnCancel;
		private System.ComponentModel.Container components = null;
        private int wordCount = 140; 


		public TwitterPostForm(string body)
		{
			InitializeComponent();
			InitializeComponentTranslation();
			textPost.Text = body;
            label1.Text = (wordCount - body.Length).ToString();
		}

		private void InitializeComponentTranslation() {
			this.btnPost.Text = Resource.Manager["RES_DeliciousFormPost"];
			this.btnCancel.Text = Resource.Manager["RES_DeliciousFormCancel"];
			this.label1.Text = Resource.Manager["RES_TwitterFormCount"];
			this.Text = Resource.Manager["RES_MenuTwitterCaption"];
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
            this.textPost = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnPost
            // 
            this.btnPost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPost.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnPost.Location = new System.Drawing.Point(240, 123);
            this.btnPost.Name = "btnPost";
            this.btnPost.Size = new System.Drawing.Size(75, 23);
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
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            // 
            // textPost
            // 
            this.textPost.AllowDrop = true;
            this.textPost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textPost.Location = new System.Drawing.Point(12, 8);
            this.textPost.Multiline = true;
            this.textPost.Name = "textPost";
            this.textPost.Size = new System.Drawing.Size(391, 83);
            this.textPost.TabIndex = 38;
            this.textPost.TextChanged += new System.EventHandler(this.textPost_TextChanged);
            // 
            // label1
            // 
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(297, 94);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 26);
            this.label1.TabIndex = 39;
            this.label1.Text = "140";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TwitterPostForm
            // 
            this.AcceptButton = this.btnPost;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(416, 157);
            this.ControlBox = false;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textPost);
            this.Controls.Add(this.btnPost);
            this.Controls.Add(this.btnCancel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TwitterPostForm";
            this.Text = "Post to Twitter";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.TwitterPostForm_Closing);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		

		private void TwitterPostForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{		
	
			if(this.DialogResult == DialogResult.Retry){
				e.Cancel = true; 
			}
		}

		private void _btOK_Click(object sender, System.EventArgs e)
		{					
			
			if(textPost.Text.Length == 0){
				
				MessageBox.Show(this, Resource.Manager["RES_TwitterIncompleteSubmissionMessage"], Resource.Manager["RES_DeliciousIncompleteSubmissionTitle"],  MessageBoxButtons.OK, MessageBoxIcon.Error);
				this.DialogResult = DialogResult.Retry; 
			}		
			
		}

        private void textPost_TextChanged(object sender, EventArgs e) {
            this.label1.Text = (wordCount - this.textPost.Text.Length).ToString();
        }
		
	}
}
