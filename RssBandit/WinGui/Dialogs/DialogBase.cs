#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using RssBandit.Resources;
using RssBandit.WinGui.Controls;

namespace RssBandit.WinGui.Dialogs
{
	/// <summary>
	/// DialogBase can be inherited from to get the right
	/// icon, OK/Cancel, Line separator, helpProvider, errorProvider,
	/// toolTip and resizing behavior.
	/// </summary>
	public class DialogBase : System.Windows.Forms.Form
	{
        protected System.Windows.Forms.Button btnCancel;
        protected Line horizontalEdge;
		protected System.Windows.Forms.Button btnSubmit;
		protected WindowSerializer windowSerializer;
		protected System.Windows.Forms.ToolTip toolTip;
		protected System.Windows.Forms.ErrorProvider errorProvider;
		protected System.Windows.Forms.HelpProvider helpProvider;
		
		private System.ComponentModel.IContainer components;

		/// <summary>
		/// Initializes a new instance of the <see cref="DialogBase"/> class.
		/// </summary>
		public DialogBase():base()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		#region Dispose / Translate / Designer code

		/// <summary>
		/// Initializes the component translations of the base dialog
		/// and calles then InitializeComponentTranslation().
		/// </summary>
		protected  void ApplyComponentTranslations() {
			InitializeComponentTranslation();
		}
		
		/// <summary>
		/// Place to Initializes the child component translation. Must be impl.
		/// to translate the control captions. 
		/// </summary>
		protected virtual void InitializeComponentTranslation() {
			// translate the default button captions
			this.btnSubmit.Text = SR.DialogBase_SubmitButtonCaption;
			this.toolTip.SetToolTip(this.btnSubmit, SR.DialogBase_SubmitButtonCaptionTip);
			this.btnCancel.Text = SR.DialogBase_CancelButtonCaption;
			this.toolTip.SetToolTip(this.btnCancel, SR.DialogBase_CancelButtonCaptionTip);
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(DialogBase));
			this.btnSubmit = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.horizontalEdge = new RssBandit.WinGui.Controls.Line();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.windowSerializer = new RssBandit.WinGui.Controls.WindowSerializer();
			this.errorProvider = new System.Windows.Forms.ErrorProvider();
			this.helpProvider = new System.Windows.Forms.HelpProvider();
			this.SuspendLayout();
			// 
			// btnSubmit
			// 
			this.btnSubmit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSubmit.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnSubmit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnSubmit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.btnSubmit.Location = new System.Drawing.Point(185, 218);
			this.btnSubmit.Name = "btnSubmit";
			this.btnSubmit.Size = new System.Drawing.Size(90, 25);
			this.btnSubmit.TabIndex = 100;
			this.btnSubmit.Text = "OK";
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.CausesValidation = false;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.btnCancel.Location = new System.Drawing.Point(285, 218);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(90, 25);
			this.btnCancel.TabIndex = 101;
			this.btnCancel.Text = "Cancel";
			// 
			// horizontalEdge
			// 
			this.horizontalEdge.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.horizontalEdge.Beveled = true;
			this.horizontalEdge.Highlight = System.Drawing.SystemColors.ControlLightLight;
			this.horizontalEdge.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.horizontalEdge.Location = new System.Drawing.Point(-1, 206);
			this.horizontalEdge.Name = "horizontalEdge";
			this.horizontalEdge.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.horizontalEdge.Shadow = System.Drawing.SystemColors.ControlDark;
			this.horizontalEdge.Size = new System.Drawing.Size(397, 2);
			this.horizontalEdge.TabIndex = 7;
			this.horizontalEdge.TabStop = false;
			// 
			// windowSerializer
			// 
			this.windowSerializer.Form = this;
			this.windowSerializer.SaveNoWindowState = true;
			// 
			// errorProvider
			// 
			this.errorProvider.ContainerControl = this;
			// 
			// helpProvider
			// 
			this.helpProvider.HelpNamespace = "BanditHelp.chm";
			// 
			// DialogBase
			// 
			this.AcceptButton = this.btnSubmit;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(387, 251);
			this.Controls.Add(this.horizontalEdge);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnSubmit);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DialogBase";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.ResumeLayout(false);

		}
		#endregion

		#endregion

	}
}

#region CVS Version Log
/*
 * $Log: DialogBase.cs,v $
 * Revision 1.1  2006/11/11 14:42:45  t_rendelmann
 * added: DialogBase base Form to be able to inherit simple OK/Cancel dialogs;
 * added new PodcastOptionsDialog (inherits DialogBase)
 *
 */
#endregion
