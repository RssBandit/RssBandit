using System.Windows.Forms;
using System.Drawing;
using IEControl;
using RssBandit.Resources;
namespace RssBandit.WinGui.Dialogs
{
    partial class FacebookConnectDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FacebookConnectDialog));
			this.browserFB = new IEControl.HtmlControl();
			((System.ComponentModel.ISupportInitialize)(this.browserFB)).BeginInit();
			this.SuspendLayout();
			// 
			// browserFB
			// 
			this.browserFB.Dock = System.Windows.Forms.DockStyle.Fill;
			this.browserFB.Enabled = true;
			this.browserFB.Location = new System.Drawing.Point(0, 0);
			this.browserFB.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.browserFB.MinimumSize = new System.Drawing.Size(27, 25);
			this.browserFB.Name = "browserFB";
			this.browserFB.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("browserFB.OcxState")));
			this.browserFB.OpticalZoomFactor = 100;
			this.browserFB.Size = new System.Drawing.Size(749, 603);
			this.browserFB.TabIndex = 0;
			// 
			// FacebookConnectDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(749, 603);
			this.Controls.Add(this.browserFB);
			this.IsMdiContainer = true;
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FacebookConnectDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Facebook Login";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FacebookConnectDialog_FormClosed);
			((System.ComponentModel.ISupportInitialize)(this.browserFB)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion
    }
}