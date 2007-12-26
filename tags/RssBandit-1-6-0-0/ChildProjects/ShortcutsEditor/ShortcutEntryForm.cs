using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace ShortcutsEditor
{
	/// <summary>
	/// Summary description for ShortcutEntryForm.
	/// </summary>
	public class ShortcutEntryForm : Form, IMessageFilter
	{
		public const int WM_KEYDOWN       = 0x100;
		public const int WM_KEYUP         = 0x101;
		public const int WM_SYSKEYDOWN = 0x104;
		public const int WM_SYSKEYUP = 0x105;
		public const int WM_LEFTMOUSEDOWN = 0x201;
		public const int WM_LEFTMOUSEUP   = 0x202;
		public const int WM_LEFTMOUSEDBL  = 0x203;

		private Label label1;
		private TextBox txtKeyCombination;
		private Button btnOk;
		private Button btnCancel;
		private System.Windows.Forms.Button btnReset;
		private Hashtable _pressedKeys = new Hashtable();
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Container components = null;

		public ShortcutEntryForm()
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
		/// Gets the key combination.
		/// </summary>
		/// <value></value>
		public Keys KeyCombination
		{
			get
			{
				if(txtKeyCombination.Text.Length == 0)
					return Keys.None;
				return (Keys)Enum.Parse(typeof(Keys), txtKeyCombination.Text);
			}
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
			this.txtKeyCombination = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnReset = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// txtKeyCombination
			// 
			this.txtKeyCombination.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtKeyCombination.Location = new System.Drawing.Point(152, 8);
			this.txtKeyCombination.Name = "txtKeyCombination";
			this.txtKeyCombination.Size = new System.Drawing.Size(208, 20);
			this.txtKeyCombination.TabIndex = 0;
			this.txtKeyCombination.Text = "";
			// 
			// label1
			// 
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label1.Location = new System.Drawing.Point(0, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(152, 40);
			this.label1.TabIndex = 1;
			this.label1.Text = "Click in the textbox and then press the actual keys you wish to use.";
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnOk.Location = new System.Drawing.Point(200, 56);
			this.btnOk.Name = "btnOk";
			this.btnOk.TabIndex = 2;
			this.btnOk.Text = "Ok";
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnCancel.Location = new System.Drawing.Point(280, 56);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			// 
			// btnReset
			// 
			this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnReset.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnReset.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnReset.Location = new System.Drawing.Point(8, 56);
			this.btnReset.Name = "btnReset";
			this.btnReset.TabIndex = 4;
			this.btnReset.Text = "Reset";
			this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
			// 
			// ShortcutEntryForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(368, 86);
			this.ControlBox = false;
			this.Controls.Add(this.btnReset);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtKeyCombination);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ShortcutEntryForm";
			this.ShowInTaskbar = false;
			this.Text = "Shortcut Entry Form";
			this.Activated += new System.EventHandler(this.OnFormActivated);
			this.Deactivate += new System.EventHandler(this.OnFormDeactivated);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Implements IMessageFilter.
		/// </summary>
		/// <param name="m">M.</param>
		/// <returns></returns>
		public bool PreFilterMessage(ref Message m)
		{
			if(this.txtKeyCombination.Focused && (m.Msg == WM_KEYDOWN || m.Msg == WM_SYSKEYDOWN))
			{
				Keys keys = ((Keys)(int)m.WParam & Keys.KeyCode);

				string keyName = keys.ToString();
				
				//KLUGE: Till I learn better. I want to allow the user to 
				//		define a shortcut combination one key at a time.  
				//		Therefore I need to convert "Menu" to "Alt" and 
				//		ControlKey into Key.
				if(keyName == "Menu")
					keyName = "Alt";

				if(keyName.Length > 3 && keyName.IndexOf("Key") == keyName.Length - 3)
				{
					keyName = keyName.Substring(0, keyName.Length - 3);
				}

				if(_pressedKeys.ContainsKey(keyName))
					return true;
				else
					_pressedKeys.Add(keyName, null);

				if(this.txtKeyCombination.Text.Length > 0)
					this.txtKeyCombination.AppendText(",");
				
				this.txtKeyCombination.AppendText(keyName);
			
				return true;
			}
			return false;
		}

		private void OnFormActivated(object sender, System.EventArgs e)
		{
			_pressedKeys.Clear();
			Application.AddMessageFilter(this);
		}

		private void OnFormDeactivated(object sender, System.EventArgs e)
		{
			_pressedKeys.Clear();
			Application.RemoveMessageFilter(this);
		}

		private void btnReset_Click(object sender, System.EventArgs e)
		{
			_pressedKeys.Clear();
			this.txtKeyCombination.Text = string.Empty;
		}
	}
}
