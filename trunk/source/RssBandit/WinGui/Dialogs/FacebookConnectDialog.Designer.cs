using System.Windows.Forms;
using System.Drawing;
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
            this.browserFB = new WebBrowser();
            base.SuspendLayout();
            this.browserFB.Dock = DockStyle.Fill;
            this.browserFB.Location = new Point(0, 0);
            this.browserFB.MinimumSize = new Size(20, 20);
            this.browserFB.Name = "browserFB";
            this.browserFB.ScrollBarsEnabled = false;
            this.browserFB.Size = new Size(700, 490);
            this.browserFB.TabIndex = 0;
            this.browserFB.Navigated += new WebBrowserNavigatedEventHandler(this.FacebookConnectDialog_Navigated);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(700, 490);
            base.Controls.Add(this.browserFB);
            base.FormBorderStyle = FormBorderStyle.Sizable;
            base.IsMdiContainer = true;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "FacebookConnect";
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = SR.FacebookConnectFormTitle;
            base.FormClosed += new FormClosedEventHandler(this.FacebookConnectDialog_FormClosed);
            base.ResumeLayout(false);
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;            
        }

        #endregion
    }
}