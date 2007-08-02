#region CVS Version Header
/*
 * $Id: EntertainmentDialog.cs,v 1.7 2005/04/08 15:00:20 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/04/08 15:00:20 $
 * $Revision: 1.7 $
 */
#endregion

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;

namespace RssBandit.WinGui.Forms
{
	/// <summary>
	/// EntertainmentDialog used by EntertainmentThreadHandlerBase
	/// inherited classes.
	/// </summary>
	public class EntertainmentDialog : System.Windows.Forms.Form
	{
		private TimeSpan timeout = TimeSpan.Zero;	// no timeout
		private bool operationTimeout = false;
		private int timeCounter;
        private AutoResetEvent waitHandle;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label messageLabel;
        private System.ComponentModel.IContainer components;

		private EntertainmentDialog() {
			InitializeComponent();
		}
		
		public EntertainmentDialog( AutoResetEvent waitHandle ):
			this(waitHandle, TimeSpan.Zero) {
        }

		public EntertainmentDialog( AutoResetEvent waitHandle, TimeSpan timeout ):this() {
			this.waitHandle = waitHandle;
			this.timeout = timeout;
			this.timeCounter = 0;
			if (timeout != TimeSpan.Zero)
				this.timeCounter = (int)(timeout.TotalMilliseconds / this.timer.Interval);
			this.DialogResult = DialogResult.OK;
		}

        public string Message {
					get { return messageLabel.Text;		}
					set { messageLabel.Text = value;  }
        }

		public bool OperationTimeout {
			get { return this.operationTimeout;		}
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
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(EntertainmentDialog));
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.messageLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// progressBar
			// 
			this.progressBar.AccessibleDescription = resources.GetString("progressBar.AccessibleDescription");
			this.progressBar.AccessibleName = resources.GetString("progressBar.AccessibleName");
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("progressBar.Anchor")));
			this.progressBar.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("progressBar.BackgroundImage")));
			this.progressBar.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("progressBar.Dock")));
			this.progressBar.Enabled = ((bool)(resources.GetObject("progressBar.Enabled")));
			this.progressBar.Font = ((System.Drawing.Font)(resources.GetObject("progressBar.Font")));
			this.progressBar.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("progressBar.ImeMode")));
			this.progressBar.Location = ((System.Drawing.Point)(resources.GetObject("progressBar.Location")));
			this.progressBar.Name = "progressBar";
			this.progressBar.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("progressBar.RightToLeft")));
			this.progressBar.Size = ((System.Drawing.Size)(resources.GetObject("progressBar.Size")));
			this.progressBar.Step = 2;
			this.progressBar.TabIndex = ((int)(resources.GetObject("progressBar.TabIndex")));
			this.progressBar.Text = resources.GetString("progressBar.Text");
			this.progressBar.Visible = ((bool)(resources.GetObject("progressBar.Visible")));
			// 
			// timer
			// 
			this.timer.Enabled = true;
			this.timer.Tick += new System.EventHandler(this.timer_Tick);
			// 
			// messageLabel
			// 
			this.messageLabel.AccessibleDescription = resources.GetString("messageLabel.AccessibleDescription");
			this.messageLabel.AccessibleName = resources.GetString("messageLabel.AccessibleName");
			this.messageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("messageLabel.Anchor")));
			this.messageLabel.AutoSize = ((bool)(resources.GetObject("messageLabel.AutoSize")));
			this.messageLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("messageLabel.Dock")));
			this.messageLabel.Enabled = ((bool)(resources.GetObject("messageLabel.Enabled")));
			this.messageLabel.Font = ((System.Drawing.Font)(resources.GetObject("messageLabel.Font")));
			this.messageLabel.Image = ((System.Drawing.Image)(resources.GetObject("messageLabel.Image")));
			this.messageLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("messageLabel.ImageAlign")));
			this.messageLabel.ImageIndex = ((int)(resources.GetObject("messageLabel.ImageIndex")));
			this.messageLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("messageLabel.ImeMode")));
			this.messageLabel.Location = ((System.Drawing.Point)(resources.GetObject("messageLabel.Location")));
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("messageLabel.RightToLeft")));
			this.messageLabel.Size = ((System.Drawing.Size)(resources.GetObject("messageLabel.Size")));
			this.messageLabel.TabIndex = ((int)(resources.GetObject("messageLabel.TabIndex")));
			this.messageLabel.Text = resources.GetString("messageLabel.Text");
			this.messageLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("messageLabel.TextAlign")));
			this.messageLabel.Visible = ((bool)(resources.GetObject("messageLabel.Visible")));
			// 
			// EntertainmentDialog
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.messageLabel);
			this.Controls.Add(this.progressBar);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "EntertainmentDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Runs every 100 msecs
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void timer_Tick(object sender, System.EventArgs e)
        {

            if ( waitHandle.WaitOne(0,false) )	// gets signal
            {
				this.DialogResult = DialogResult.OK;
                Close();
				return;
            }

			if (this.timeout != TimeSpan.Zero) {
				this.timeCounter--;
				if (this.timeCounter <= 0) {
					waitHandle.Set();		// signal done (timeout)
					this.operationTimeout = true;
					this.DialogResult = DialogResult.Abort;
					Close();
					return;
				}
			}
            
			// update progress bar info
			if ( progressBar.Value + progressBar.Step >= progressBar.Maximum ) {
				progressBar.Value = 0;
			} else {
				progressBar.Increment(progressBar.Step);
			}
        }
	}
}
