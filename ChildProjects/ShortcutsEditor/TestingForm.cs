using System;
using System.ComponentModel;
using System.Windows.Forms;
using RssBandit.Utility.Keyboard;

namespace ShortcutsEditor
{
	/// <summary>
	/// Summary description for TestingForm.
	/// </summary>
	public class TestingForm : Form, IMessageFilter
	{
		ShortcutHandler _handler = null;
		private Label label1;
		private TextBox txtCommand;
		private Label label2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Container components = null;

		/// <summary>
		/// Creates a new <see cref="TestingForm"/> instance.
		/// </summary>
		public TestingForm()
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
		/// Gets or sets the shortcut manager.
		/// </summary>
		/// <value></value>
		public ShortcutHandler ShortcutManager
		{
			get { return _handler; }
			set { _handler = value; }
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
			this.label1 = new System.Windows.Forms.Label();
			this.txtCommand = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(272, 48);
			this.label1.TabIndex = 0;
			this.label1.Text = "Go ahead and press a shortcut Key Combination and see what command it corresponds" +
				" to displayed in the TextBox.";
			// 
			// txtCommand
			// 
			this.txtCommand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtCommand.Location = new System.Drawing.Point(56, 72);
			this.txtCommand.Name = "txtCommand";
			this.txtCommand.Size = new System.Drawing.Size(232, 20);
			this.txtCommand.TabIndex = 1;
			this.txtCommand.Text = "";
			// 
			// label2
			// 
			this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label2.Location = new System.Drawing.Point(0, 72);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(56, 23);
			this.label2.TabIndex = 2;
			this.label2.Text = "Command:";
			// 
			// TestingForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 110);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtCommand);
			this.Controls.Add(this.label1);
			this.Name = "TestingForm";
			this.Text = "TestingForm";
			this.Activated += new System.EventHandler(this.OnFormActivated);
			this.Deactivate += new System.EventHandler(this.OnFormDeactivated);
			this.ResumeLayout(false);

		}
		#endregion

		private void OnFormActivated(object sender, System.EventArgs e)
		{
			Application.AddMessageFilter(this);
		}

		private void OnFormDeactivated(object sender, System.EventArgs e)
		{
			Application.RemoveMessageFilter(this);
		}

		public bool PreFilterMessage(ref Message m)
		{
			if(m.Msg == ShortcutEntryForm.WM_KEYDOWN && _handler != null)
			{
				Keys keys = (Keys)(int)m.WParam | Control.ModifierKeys;
				//Keys keys = ((Keys)m.WParam.ToInt32());
				Console.WriteLine(keys.ToString());
				
				//This is going to be slow. Oh well.
				foreach(string command in _handler.AvailableKeyComboCommands)
				{
					if(_handler.IsCommandInvoked(command, m.WParam))
					{
						this.txtCommand.Text = command;
					}
				}
				return true;
			}
			return false;
		}
	}
}
