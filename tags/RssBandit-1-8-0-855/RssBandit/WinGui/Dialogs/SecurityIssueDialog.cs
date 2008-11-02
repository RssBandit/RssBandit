#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System.Windows.Forms;

namespace RssBandit.WinGui.Dialogs
{
	/// <summary>
	/// Security Issue Dialog. Currently used to display X509 certificate issues.
	/// </summary>
	internal partial class SecurityIssueDialog : System.Windows.Forms.Form
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

		
		private void buttonYes_Click(object sender, System.EventArgs e) {
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
