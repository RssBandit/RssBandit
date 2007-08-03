#region CVS Version Header
/*
 * $Id: AskForDefaultAggregator.cs,v 1.3 2006/10/20 10:10:15 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2006/10/20 10:10:15 $
 * $Revision: 1.3 $
 */
#endregion

using System.Windows.Forms;

namespace RssBandit.WinGui.Dialogs
{
	/// <summary>
	/// Summary description for AskForDefaultAggregator.
	/// </summary>
	public class AskForDefaultAggregator : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button buttonYes;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button buttonNo;
		internal System.Windows.Forms.CheckBox checkBoxDoNotAskAnymore;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AskForDefaultAggregator()
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AskForDefaultAggregator));
			this.label1 = new System.Windows.Forms.Label();
			this.buttonYes = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.buttonNo = new System.Windows.Forms.Button();
			this.checkBoxDoNotAskAnymore = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
			this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
			this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
			this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
			this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
			this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
			this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
			this.label1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.ImageAlign")));
			this.label1.ImageIndex = ((int)(resources.GetObject("label1.ImageIndex")));
			this.label1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label1.ImeMode")));
			this.label1.Location = ((System.Drawing.Point)(resources.GetObject("label1.Location")));
			this.label1.Name = "label1";
			this.label1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label1.RightToLeft")));
			this.label1.Size = ((System.Drawing.Size)(resources.GetObject("label1.Size")));
			this.label1.TabIndex = ((int)(resources.GetObject("label1.TabIndex")));
			this.label1.Text = resources.GetString("label1.Text");
			this.label1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.TextAlign")));
			this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
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
			// label2
			// 
			this.label2.AccessibleDescription = resources.GetString("label2.AccessibleDescription");
			this.label2.AccessibleName = resources.GetString("label2.AccessibleName");
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label2.Anchor")));
			this.label2.AutoSize = ((bool)(resources.GetObject("label2.AutoSize")));
			this.label2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label2.Dock")));
			this.label2.Enabled = ((bool)(resources.GetObject("label2.Enabled")));
			this.label2.Font = ((System.Drawing.Font)(resources.GetObject("label2.Font")));
			this.label2.Image = ((System.Drawing.Image)(resources.GetObject("label2.Image")));
			this.label2.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label2.ImageAlign")));
			this.label2.ImageIndex = ((int)(resources.GetObject("label2.ImageIndex")));
			this.label2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label2.ImeMode")));
			this.label2.Location = ((System.Drawing.Point)(resources.GetObject("label2.Location")));
			this.label2.Name = "label2";
			this.label2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label2.RightToLeft")));
			this.label2.Size = ((System.Drawing.Size)(resources.GetObject("label2.Size")));
			this.label2.TabIndex = ((int)(resources.GetObject("label2.TabIndex")));
			this.label2.Text = resources.GetString("label2.Text");
			this.label2.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label2.TextAlign")));
			this.label2.Visible = ((bool)(resources.GetObject("label2.Visible")));
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
			// checkBoxDoNotAskAnymore
			// 
			this.checkBoxDoNotAskAnymore.AccessibleDescription = resources.GetString("checkBoxDoNotAskAnymore.AccessibleDescription");
			this.checkBoxDoNotAskAnymore.AccessibleName = resources.GetString("checkBoxDoNotAskAnymore.AccessibleName");
			this.checkBoxDoNotAskAnymore.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("checkBoxDoNotAskAnymore.Anchor")));
			this.checkBoxDoNotAskAnymore.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("checkBoxDoNotAskAnymore.Appearance")));
			this.checkBoxDoNotAskAnymore.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("checkBoxDoNotAskAnymore.BackgroundImage")));
			this.checkBoxDoNotAskAnymore.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxDoNotAskAnymore.CheckAlign")));
			this.checkBoxDoNotAskAnymore.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("checkBoxDoNotAskAnymore.Dock")));
			this.checkBoxDoNotAskAnymore.Enabled = ((bool)(resources.GetObject("checkBoxDoNotAskAnymore.Enabled")));
			this.checkBoxDoNotAskAnymore.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("checkBoxDoNotAskAnymore.FlatStyle")));
			this.checkBoxDoNotAskAnymore.Font = ((System.Drawing.Font)(resources.GetObject("checkBoxDoNotAskAnymore.Font")));
			this.checkBoxDoNotAskAnymore.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxDoNotAskAnymore.Image")));
			this.checkBoxDoNotAskAnymore.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxDoNotAskAnymore.ImageAlign")));
			this.checkBoxDoNotAskAnymore.ImageIndex = ((int)(resources.GetObject("checkBoxDoNotAskAnymore.ImageIndex")));
			this.checkBoxDoNotAskAnymore.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("checkBoxDoNotAskAnymore.ImeMode")));
			this.checkBoxDoNotAskAnymore.Location = ((System.Drawing.Point)(resources.GetObject("checkBoxDoNotAskAnymore.Location")));
			this.checkBoxDoNotAskAnymore.Name = "checkBoxDoNotAskAnymore";
			this.checkBoxDoNotAskAnymore.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("checkBoxDoNotAskAnymore.RightToLeft")));
			this.checkBoxDoNotAskAnymore.Size = ((System.Drawing.Size)(resources.GetObject("checkBoxDoNotAskAnymore.Size")));
			this.checkBoxDoNotAskAnymore.TabIndex = ((int)(resources.GetObject("checkBoxDoNotAskAnymore.TabIndex")));
			this.checkBoxDoNotAskAnymore.Text = resources.GetString("checkBoxDoNotAskAnymore.Text");
			this.checkBoxDoNotAskAnymore.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("checkBoxDoNotAskAnymore.TextAlign")));
			this.checkBoxDoNotAskAnymore.Visible = ((bool)(resources.GetObject("checkBoxDoNotAskAnymore.Visible")));
			// 
			// AskForDefaultAggregator
			// 
			this.AcceptButton = this.buttonYes;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.buttonNo;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.checkBoxDoNotAskAnymore);
			this.Controls.Add(this.buttonNo);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.buttonYes);
			this.Controls.Add(this.label1);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "AskForDefaultAggregator";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.ResumeLayout(false);

		}
		#endregion

		private void buttonYes_Click(object sender, System.EventArgs e) {
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
