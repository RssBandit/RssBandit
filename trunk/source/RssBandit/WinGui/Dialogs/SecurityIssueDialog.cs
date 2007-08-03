using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace RssBandit.WinGui.Dialogs
{
	/// <summary>
	/// Summary description for SecurityIssueDialog.
	/// </summary>
	public class SecurityIssueDialog : System.Windows.Forms.Form
	{

		private System.Windows.Forms.Button buttonYes;
		private System.Windows.Forms.Button buttonNo;
		private System.Windows.Forms.Label labelIssueCaption;
		private System.Windows.Forms.Label labelProceedMessage;
		private System.Windows.Forms.Label labelIssueDescription;
		private System.Windows.Forms.Label horizontalEdge;
		internal System.Windows.Forms.Button CustomCommand;
		private System.Windows.Forms.Label labelCaptionImage;
		private System.Windows.Forms.Label labelAttentionImage;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SecurityIssueDialog(string issueCaption, string issueDescription)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			labelIssueCaption.Text = issueCaption;
			if (issueDescription != null) {
				labelIssueDescription.Text = issueDescription;
			} else {
				labelIssueDescription.Visible = labelAttentionImage.Visible = false;
			}
			CustomCommand.Visible = false;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SecurityIssueDialog));
			this.labelIssueCaption = new System.Windows.Forms.Label();
			this.buttonYes = new System.Windows.Forms.Button();
			this.labelCaptionImage = new System.Windows.Forms.Label();
			this.buttonNo = new System.Windows.Forms.Button();
			this.labelProceedMessage = new System.Windows.Forms.Label();
			this.labelAttentionImage = new System.Windows.Forms.Label();
			this.labelIssueDescription = new System.Windows.Forms.Label();
			this.horizontalEdge = new System.Windows.Forms.Label();
			this.CustomCommand = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// labelIssueCaption
			// 
			this.labelIssueCaption.AccessibleDescription = resources.GetString("labelIssueCaption.AccessibleDescription");
			this.labelIssueCaption.AccessibleName = resources.GetString("labelIssueCaption.AccessibleName");
			this.labelIssueCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelIssueCaption.Anchor")));
			this.labelIssueCaption.AutoSize = ((bool)(resources.GetObject("labelIssueCaption.AutoSize")));
			this.labelIssueCaption.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelIssueCaption.Dock")));
			this.labelIssueCaption.Enabled = ((bool)(resources.GetObject("labelIssueCaption.Enabled")));
			this.labelIssueCaption.Font = ((System.Drawing.Font)(resources.GetObject("labelIssueCaption.Font")));
			this.labelIssueCaption.Image = ((System.Drawing.Image)(resources.GetObject("labelIssueCaption.Image")));
			this.labelIssueCaption.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelIssueCaption.ImageAlign")));
			this.labelIssueCaption.ImageIndex = ((int)(resources.GetObject("labelIssueCaption.ImageIndex")));
			this.labelIssueCaption.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelIssueCaption.ImeMode")));
			this.labelIssueCaption.Location = ((System.Drawing.Point)(resources.GetObject("labelIssueCaption.Location")));
			this.labelIssueCaption.Name = "labelIssueCaption";
			this.labelIssueCaption.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelIssueCaption.RightToLeft")));
			this.labelIssueCaption.Size = ((System.Drawing.Size)(resources.GetObject("labelIssueCaption.Size")));
			this.labelIssueCaption.TabIndex = ((int)(resources.GetObject("labelIssueCaption.TabIndex")));
			this.labelIssueCaption.Text = resources.GetString("labelIssueCaption.Text");
			this.labelIssueCaption.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelIssueCaption.TextAlign")));
			this.labelIssueCaption.Visible = ((bool)(resources.GetObject("labelIssueCaption.Visible")));
			// 
			// buttonYes
			// 
			this.buttonYes.AccessibleDescription = resources.GetString("buttonYes.AccessibleDescription");
			this.buttonYes.AccessibleName = resources.GetString("buttonYes.AccessibleName");
			this.buttonYes.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("buttonYes.Anchor")));
			this.buttonYes.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonYes.BackgroundImage")));
			this.buttonYes.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("buttonYes.Dock")));
			this.buttonYes.Enabled = ((bool)(resources.GetObject("buttonYes.Enabled")));
			this.buttonYes.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("buttonYes.FlatStyle")));
			this.buttonYes.Font = ((System.Drawing.Font)(resources.GetObject("buttonYes.Font")));
			this.buttonYes.Image = ((System.Drawing.Image)(resources.GetObject("buttonYes.Image")));
			this.buttonYes.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("buttonYes.ImageAlign")));
			this.buttonYes.ImageIndex = ((int)(resources.GetObject("buttonYes.ImageIndex")));
			this.buttonYes.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("buttonYes.ImeMode")));
			this.buttonYes.Location = ((System.Drawing.Point)(resources.GetObject("buttonYes.Location")));
			this.buttonYes.Name = "buttonYes";
			this.buttonYes.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("buttonYes.RightToLeft")));
			this.buttonYes.Size = ((System.Drawing.Size)(resources.GetObject("buttonYes.Size")));
			this.buttonYes.TabIndex = ((int)(resources.GetObject("buttonYes.TabIndex")));
			this.buttonYes.Text = resources.GetString("buttonYes.Text");
			this.buttonYes.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("buttonYes.TextAlign")));
			this.buttonYes.Visible = ((bool)(resources.GetObject("buttonYes.Visible")));
			this.buttonYes.Click += new System.EventHandler(this.buttonYes_Click);
			// 
			// labelCaptionImage
			// 
			this.labelCaptionImage.AccessibleDescription = resources.GetString("labelCaptionImage.AccessibleDescription");
			this.labelCaptionImage.AccessibleName = resources.GetString("labelCaptionImage.AccessibleName");
			this.labelCaptionImage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelCaptionImage.Anchor")));
			this.labelCaptionImage.AutoSize = ((bool)(resources.GetObject("labelCaptionImage.AutoSize")));
			this.labelCaptionImage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelCaptionImage.Dock")));
			this.labelCaptionImage.Enabled = ((bool)(resources.GetObject("labelCaptionImage.Enabled")));
			this.labelCaptionImage.Font = ((System.Drawing.Font)(resources.GetObject("labelCaptionImage.Font")));
			this.labelCaptionImage.Image = ((System.Drawing.Image)(resources.GetObject("labelCaptionImage.Image")));
			this.labelCaptionImage.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelCaptionImage.ImageAlign")));
			this.labelCaptionImage.ImageIndex = ((int)(resources.GetObject("labelCaptionImage.ImageIndex")));
			this.labelCaptionImage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelCaptionImage.ImeMode")));
			this.labelCaptionImage.Location = ((System.Drawing.Point)(resources.GetObject("labelCaptionImage.Location")));
			this.labelCaptionImage.Name = "labelCaptionImage";
			this.labelCaptionImage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelCaptionImage.RightToLeft")));
			this.labelCaptionImage.Size = ((System.Drawing.Size)(resources.GetObject("labelCaptionImage.Size")));
			this.labelCaptionImage.TabIndex = ((int)(resources.GetObject("labelCaptionImage.TabIndex")));
			this.labelCaptionImage.Text = resources.GetString("labelCaptionImage.Text");
			this.labelCaptionImage.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelCaptionImage.TextAlign")));
			this.labelCaptionImage.Visible = ((bool)(resources.GetObject("labelCaptionImage.Visible")));
			// 
			// buttonNo
			// 
			this.buttonNo.AccessibleDescription = resources.GetString("buttonNo.AccessibleDescription");
			this.buttonNo.AccessibleName = resources.GetString("buttonNo.AccessibleName");
			this.buttonNo.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("buttonNo.Anchor")));
			this.buttonNo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonNo.BackgroundImage")));
			this.buttonNo.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonNo.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("buttonNo.Dock")));
			this.buttonNo.Enabled = ((bool)(resources.GetObject("buttonNo.Enabled")));
			this.buttonNo.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("buttonNo.FlatStyle")));
			this.buttonNo.Font = ((System.Drawing.Font)(resources.GetObject("buttonNo.Font")));
			this.buttonNo.Image = ((System.Drawing.Image)(resources.GetObject("buttonNo.Image")));
			this.buttonNo.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("buttonNo.ImageAlign")));
			this.buttonNo.ImageIndex = ((int)(resources.GetObject("buttonNo.ImageIndex")));
			this.buttonNo.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("buttonNo.ImeMode")));
			this.buttonNo.Location = ((System.Drawing.Point)(resources.GetObject("buttonNo.Location")));
			this.buttonNo.Name = "buttonNo";
			this.buttonNo.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("buttonNo.RightToLeft")));
			this.buttonNo.Size = ((System.Drawing.Size)(resources.GetObject("buttonNo.Size")));
			this.buttonNo.TabIndex = ((int)(resources.GetObject("buttonNo.TabIndex")));
			this.buttonNo.Text = resources.GetString("buttonNo.Text");
			this.buttonNo.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("buttonNo.TextAlign")));
			this.buttonNo.Visible = ((bool)(resources.GetObject("buttonNo.Visible")));
			// 
			// labelProceedMessage
			// 
			this.labelProceedMessage.AccessibleDescription = resources.GetString("labelProceedMessage.AccessibleDescription");
			this.labelProceedMessage.AccessibleName = resources.GetString("labelProceedMessage.AccessibleName");
			this.labelProceedMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelProceedMessage.Anchor")));
			this.labelProceedMessage.AutoSize = ((bool)(resources.GetObject("labelProceedMessage.AutoSize")));
			this.labelProceedMessage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelProceedMessage.Dock")));
			this.labelProceedMessage.Enabled = ((bool)(resources.GetObject("labelProceedMessage.Enabled")));
			this.labelProceedMessage.Font = ((System.Drawing.Font)(resources.GetObject("labelProceedMessage.Font")));
			this.labelProceedMessage.Image = ((System.Drawing.Image)(resources.GetObject("labelProceedMessage.Image")));
			this.labelProceedMessage.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelProceedMessage.ImageAlign")));
			this.labelProceedMessage.ImageIndex = ((int)(resources.GetObject("labelProceedMessage.ImageIndex")));
			this.labelProceedMessage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelProceedMessage.ImeMode")));
			this.labelProceedMessage.Location = ((System.Drawing.Point)(resources.GetObject("labelProceedMessage.Location")));
			this.labelProceedMessage.Name = "labelProceedMessage";
			this.labelProceedMessage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelProceedMessage.RightToLeft")));
			this.labelProceedMessage.Size = ((System.Drawing.Size)(resources.GetObject("labelProceedMessage.Size")));
			this.labelProceedMessage.TabIndex = ((int)(resources.GetObject("labelProceedMessage.TabIndex")));
			this.labelProceedMessage.Text = resources.GetString("labelProceedMessage.Text");
			this.labelProceedMessage.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelProceedMessage.TextAlign")));
			this.labelProceedMessage.Visible = ((bool)(resources.GetObject("labelProceedMessage.Visible")));
			// 
			// labelAttentionImage
			// 
			this.labelAttentionImage.AccessibleDescription = resources.GetString("labelAttentionImage.AccessibleDescription");
			this.labelAttentionImage.AccessibleName = resources.GetString("labelAttentionImage.AccessibleName");
			this.labelAttentionImage.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelAttentionImage.Anchor")));
			this.labelAttentionImage.AutoSize = ((bool)(resources.GetObject("labelAttentionImage.AutoSize")));
			this.labelAttentionImage.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelAttentionImage.Dock")));
			this.labelAttentionImage.Enabled = ((bool)(resources.GetObject("labelAttentionImage.Enabled")));
			this.labelAttentionImage.Font = ((System.Drawing.Font)(resources.GetObject("labelAttentionImage.Font")));
			this.labelAttentionImage.Image = ((System.Drawing.Image)(resources.GetObject("labelAttentionImage.Image")));
			this.labelAttentionImage.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelAttentionImage.ImageAlign")));
			this.labelAttentionImage.ImageIndex = ((int)(resources.GetObject("labelAttentionImage.ImageIndex")));
			this.labelAttentionImage.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelAttentionImage.ImeMode")));
			this.labelAttentionImage.Location = ((System.Drawing.Point)(resources.GetObject("labelAttentionImage.Location")));
			this.labelAttentionImage.Name = "labelAttentionImage";
			this.labelAttentionImage.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelAttentionImage.RightToLeft")));
			this.labelAttentionImage.Size = ((System.Drawing.Size)(resources.GetObject("labelAttentionImage.Size")));
			this.labelAttentionImage.TabIndex = ((int)(resources.GetObject("labelAttentionImage.TabIndex")));
			this.labelAttentionImage.Text = resources.GetString("labelAttentionImage.Text");
			this.labelAttentionImage.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelAttentionImage.TextAlign")));
			this.labelAttentionImage.Visible = ((bool)(resources.GetObject("labelAttentionImage.Visible")));
			// 
			// labelIssueDescription
			// 
			this.labelIssueDescription.AccessibleDescription = resources.GetString("labelIssueDescription.AccessibleDescription");
			this.labelIssueDescription.AccessibleName = resources.GetString("labelIssueDescription.AccessibleName");
			this.labelIssueDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("labelIssueDescription.Anchor")));
			this.labelIssueDescription.AutoSize = ((bool)(resources.GetObject("labelIssueDescription.AutoSize")));
			this.labelIssueDescription.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("labelIssueDescription.Dock")));
			this.labelIssueDescription.Enabled = ((bool)(resources.GetObject("labelIssueDescription.Enabled")));
			this.labelIssueDescription.Font = ((System.Drawing.Font)(resources.GetObject("labelIssueDescription.Font")));
			this.labelIssueDescription.Image = ((System.Drawing.Image)(resources.GetObject("labelIssueDescription.Image")));
			this.labelIssueDescription.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelIssueDescription.ImageAlign")));
			this.labelIssueDescription.ImageIndex = ((int)(resources.GetObject("labelIssueDescription.ImageIndex")));
			this.labelIssueDescription.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("labelIssueDescription.ImeMode")));
			this.labelIssueDescription.Location = ((System.Drawing.Point)(resources.GetObject("labelIssueDescription.Location")));
			this.labelIssueDescription.Name = "labelIssueDescription";
			this.labelIssueDescription.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("labelIssueDescription.RightToLeft")));
			this.labelIssueDescription.Size = ((System.Drawing.Size)(resources.GetObject("labelIssueDescription.Size")));
			this.labelIssueDescription.TabIndex = ((int)(resources.GetObject("labelIssueDescription.TabIndex")));
			this.labelIssueDescription.Text = resources.GetString("labelIssueDescription.Text");
			this.labelIssueDescription.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("labelIssueDescription.TextAlign")));
			this.labelIssueDescription.Visible = ((bool)(resources.GetObject("labelIssueDescription.Visible")));
			// 
			// horizontalEdge
			// 
			this.horizontalEdge.AccessibleDescription = resources.GetString("horizontalEdge.AccessibleDescription");
			this.horizontalEdge.AccessibleName = resources.GetString("horizontalEdge.AccessibleName");
			this.horizontalEdge.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("horizontalEdge.Anchor")));
			this.horizontalEdge.AutoSize = ((bool)(resources.GetObject("horizontalEdge.AutoSize")));
			this.horizontalEdge.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.horizontalEdge.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("horizontalEdge.Dock")));
			this.horizontalEdge.Enabled = ((bool)(resources.GetObject("horizontalEdge.Enabled")));
			this.horizontalEdge.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.horizontalEdge.Font = ((System.Drawing.Font)(resources.GetObject("horizontalEdge.Font")));
			this.horizontalEdge.Image = ((System.Drawing.Image)(resources.GetObject("horizontalEdge.Image")));
			this.horizontalEdge.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("horizontalEdge.ImageAlign")));
			this.horizontalEdge.ImageIndex = ((int)(resources.GetObject("horizontalEdge.ImageIndex")));
			this.horizontalEdge.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("horizontalEdge.ImeMode")));
			this.horizontalEdge.Location = ((System.Drawing.Point)(resources.GetObject("horizontalEdge.Location")));
			this.horizontalEdge.Name = "horizontalEdge";
			this.horizontalEdge.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("horizontalEdge.RightToLeft")));
			this.horizontalEdge.Size = ((System.Drawing.Size)(resources.GetObject("horizontalEdge.Size")));
			this.horizontalEdge.TabIndex = ((int)(resources.GetObject("horizontalEdge.TabIndex")));
			this.horizontalEdge.Text = resources.GetString("horizontalEdge.Text");
			this.horizontalEdge.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("horizontalEdge.TextAlign")));
			this.horizontalEdge.Visible = ((bool)(resources.GetObject("horizontalEdge.Visible")));
			// 
			// CustomCommand
			// 
			this.CustomCommand.AccessibleDescription = resources.GetString("CustomCommand.AccessibleDescription");
			this.CustomCommand.AccessibleName = resources.GetString("CustomCommand.AccessibleName");
			this.CustomCommand.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CustomCommand.Anchor")));
			this.CustomCommand.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CustomCommand.BackgroundImage")));
			this.CustomCommand.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CustomCommand.Dock")));
			this.CustomCommand.Enabled = ((bool)(resources.GetObject("CustomCommand.Enabled")));
			this.CustomCommand.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CustomCommand.FlatStyle")));
			this.CustomCommand.Font = ((System.Drawing.Font)(resources.GetObject("CustomCommand.Font")));
			this.CustomCommand.Image = ((System.Drawing.Image)(resources.GetObject("CustomCommand.Image")));
			this.CustomCommand.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CustomCommand.ImageAlign")));
			this.CustomCommand.ImageIndex = ((int)(resources.GetObject("CustomCommand.ImageIndex")));
			this.CustomCommand.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CustomCommand.ImeMode")));
			this.CustomCommand.Location = ((System.Drawing.Point)(resources.GetObject("CustomCommand.Location")));
			this.CustomCommand.Name = "CustomCommand";
			this.CustomCommand.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CustomCommand.RightToLeft")));
			this.CustomCommand.Size = ((System.Drawing.Size)(resources.GetObject("CustomCommand.Size")));
			this.CustomCommand.TabIndex = ((int)(resources.GetObject("CustomCommand.TabIndex")));
			this.CustomCommand.Text = resources.GetString("CustomCommand.Text");
			this.CustomCommand.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CustomCommand.TextAlign")));
			this.CustomCommand.Visible = ((bool)(resources.GetObject("CustomCommand.Visible")));
			// 
			// SecurityIssueDialog
			// 
			this.AcceptButton = this.buttonNo;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.buttonNo;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.CustomCommand);
			this.Controls.Add(this.horizontalEdge);
			this.Controls.Add(this.labelAttentionImage);
			this.Controls.Add(this.labelIssueDescription);
			this.Controls.Add(this.labelProceedMessage);
			this.Controls.Add(this.buttonNo);
			this.Controls.Add(this.labelCaptionImage);
			this.Controls.Add(this.buttonYes);
			this.Controls.Add(this.labelIssueCaption);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "SecurityIssueDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.ResumeLayout(false);

		}
		#endregion

		private void buttonYes_Click(object sender, System.EventArgs e) {
			this.DialogResult = DialogResult.OK;
			this.Hide();
		}
	}
}
