using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using RssBandit.Utility.Keyboard;

namespace ShortcutsEditor
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class MainForm : Form
	{
		ShortcutHandler _handler = new ShortcutHandler();
		private GroupBox grpSettingFile;
		private TextBox txtSettingsPath;
		private Button btnBrowse;
		private OpenFileDialog openFileDialog;
		private GroupBox groupBox1;
		private GroupBox groupBox2;
		private Label label1;
		private ComboBox cmbMenuCommands;
		private ComboBox cmbMenuShortcutValues;
		private Button btnSet;
		private Label label2;
		private Label label3;
		private ComboBox cmbFilterCommands;
		private Label label4;
		private Label label5;
		private Label lblCurrentMenuShortcut;
		private Label lblCurrentKeyboardCombo;
		private Label label7;
		private System.Windows.Forms.Button btnAddShortcut;
		private System.Windows.Forms.Button btnRemove;
		private System.Windows.Forms.ListBox lstKeyCombos;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.Button btnUseDefault;
		private System.Windows.Forms.Button btnTest;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Container components = null;

		public MainForm()
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
		/// Gets the shortcut manager.
		/// </summary>
		/// <value></value>
		public ShortcutHandler ShortcutManager
		{
			get
			{
				return _handler;
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
			this.btnBrowse = new System.Windows.Forms.Button();
			this.grpSettingFile = new System.Windows.Forms.GroupBox();
			this.btnUseDefault = new System.Windows.Forms.Button();
			this.txtSettingsPath = new System.Windows.Forms.TextBox();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.lblCurrentMenuShortcut = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.btnSet = new System.Windows.Forms.Button();
			this.cmbMenuShortcutValues = new System.Windows.Forms.ComboBox();
			this.cmbMenuCommands = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.btnTest = new System.Windows.Forms.Button();
			this.btnSave = new System.Windows.Forms.Button();
			this.label6 = new System.Windows.Forms.Label();
			this.btnRemove = new System.Windows.Forms.Button();
			this.lstKeyCombos = new System.Windows.Forms.ListBox();
			this.btnAddShortcut = new System.Windows.Forms.Button();
			this.lblCurrentKeyboardCombo = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.cmbFilterCommands = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.grpSettingFile.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnBrowse
			// 
			this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnBrowse.Location = new System.Drawing.Point(288, 16);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.TabIndex = 1;
			this.btnBrowse.Text = "Browse";
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// grpSettingFile
			// 
			this.grpSettingFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.grpSettingFile.Controls.Add(this.btnUseDefault);
			this.grpSettingFile.Controls.Add(this.txtSettingsPath);
			this.grpSettingFile.Controls.Add(this.btnBrowse);
			this.grpSettingFile.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.grpSettingFile.Location = new System.Drawing.Point(8, 8);
			this.grpSettingFile.Name = "grpSettingFile";
			this.grpSettingFile.Size = new System.Drawing.Size(448, 48);
			this.grpSettingFile.TabIndex = 2;
			this.grpSettingFile.TabStop = false;
			this.grpSettingFile.Text = "Settings File";
			// 
			// btnUseDefault
			// 
			this.btnUseDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnUseDefault.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnUseDefault.Location = new System.Drawing.Point(368, 16);
			this.btnUseDefault.Name = "btnUseDefault";
			this.btnUseDefault.TabIndex = 2;
			this.btnUseDefault.Text = "Create";
			this.btnUseDefault.Click += new System.EventHandler(this.btnCreate_Click);
			// 
			// txtSettingsPath
			// 
			this.txtSettingsPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtSettingsPath.Location = new System.Drawing.Point(8, 16);
			this.txtSettingsPath.Name = "txtSettingsPath";
			this.txtSettingsPath.Size = new System.Drawing.Size(272, 20);
			this.txtSettingsPath.TabIndex = 1;
			this.txtSettingsPath.Text = "";
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.lblCurrentMenuShortcut);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.btnSet);
			this.groupBox1.Controls.Add(this.cmbMenuShortcutValues);
			this.groupBox1.Controls.Add(this.cmbMenuCommands);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 64);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(448, 96);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Menu Shortcuts";
			// 
			// lblCurrentMenuShortcut
			// 
			this.lblCurrentMenuShortcut.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblCurrentMenuShortcut.Location = new System.Drawing.Point(72, 64);
			this.lblCurrentMenuShortcut.Name = "lblCurrentMenuShortcut";
			this.lblCurrentMenuShortcut.Size = new System.Drawing.Size(352, 23);
			this.lblCurrentMenuShortcut.TabIndex = 8;
			// 
			// label5
			// 
			this.label5.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label5.Location = new System.Drawing.Point(8, 64);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(48, 23);
			this.label5.TabIndex = 7;
			this.label5.Text = "Current:";
			// 
			// label2
			// 
			this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label2.Location = new System.Drawing.Point(188, 24);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(44, 16);
			this.label2.TabIndex = 6;
			this.label2.Text = "Shortcut";
			// 
			// btnSet
			// 
			this.btnSet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSet.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnSet.Location = new System.Drawing.Point(368, 24);
			this.btnSet.Name = "btnSet";
			this.btnSet.TabIndex = 5;
			this.btnSet.Text = "Set";
			this.btnSet.Click += new System.EventHandler(this.btnSet_Click);
			// 
			// cmbMenuShortcutValues
			// 
			this.cmbMenuShortcutValues.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.cmbMenuShortcutValues.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbMenuShortcutValues.Location = new System.Drawing.Point(240, 24);
			this.cmbMenuShortcutValues.Name = "cmbMenuShortcutValues";
			this.cmbMenuShortcutValues.Size = new System.Drawing.Size(120, 21);
			this.cmbMenuShortcutValues.TabIndex = 4;
			// 
			// cmbMenuCommands
			// 
			this.cmbMenuCommands.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbMenuCommands.Location = new System.Drawing.Point(72, 24);
			this.cmbMenuCommands.Name = "cmbMenuCommands";
			this.cmbMenuCommands.Size = new System.Drawing.Size(104, 21);
			this.cmbMenuCommands.TabIndex = 3;
			this.cmbMenuCommands.SelectedIndexChanged += new System.EventHandler(this.cmbMenuCommands_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label1.Location = new System.Drawing.Point(8, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Command";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.btnTest);
			this.groupBox2.Controls.Add(this.btnSave);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.btnRemove);
			this.groupBox2.Controls.Add(this.lstKeyCombos);
			this.groupBox2.Controls.Add(this.btnAddShortcut);
			this.groupBox2.Controls.Add(this.lblCurrentKeyboardCombo);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.cmbFilterCommands);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(8, 168);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(448, 272);
			this.groupBox2.TabIndex = 4;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Filter Shortcuts";
			// 
			// btnTest
			// 
			this.btnTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnTest.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnTest.Location = new System.Drawing.Point(368, 88);
			this.btnTest.Name = "btnTest";
			this.btnTest.TabIndex = 20;
			this.btnTest.Text = "Test";
			this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
			// 
			// btnSave
			// 
			this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnSave.Location = new System.Drawing.Point(368, 240);
			this.btnSave.Name = "btnSave";
			this.btnSave.TabIndex = 19;
			this.btnSave.Text = "Save";
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// label6
			// 
			this.label6.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label6.Location = new System.Drawing.Point(8, 56);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(200, 88);
			this.label6.TabIndex = 18;
			this.label6.Text = "Some Filter shortcuts might have more than one keyboard combination, though most " +
				"only have one.";
			// 
			// btnRemove
			// 
			this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRemove.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnRemove.Location = new System.Drawing.Point(368, 56);
			this.btnRemove.Name = "btnRemove";
			this.btnRemove.TabIndex = 17;
			this.btnRemove.Text = "Remove";
			this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
			// 
			// lstKeyCombos
			// 
			this.lstKeyCombos.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lstKeyCombos.Location = new System.Drawing.Point(248, 24);
			this.lstKeyCombos.Name = "lstKeyCombos";
			this.lstKeyCombos.Size = new System.Drawing.Size(112, 121);
			this.lstKeyCombos.TabIndex = 16;
			// 
			// btnAddShortcut
			// 
			this.btnAddShortcut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAddShortcut.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnAddShortcut.Location = new System.Drawing.Point(368, 24);
			this.btnAddShortcut.Name = "btnAddShortcut";
			this.btnAddShortcut.TabIndex = 15;
			this.btnAddShortcut.Text = "Add";
			this.btnAddShortcut.Click += new System.EventHandler(this.btnAddShortcut_Click);
			// 
			// lblCurrentKeyboardCombo
			// 
			this.lblCurrentKeyboardCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblCurrentKeyboardCombo.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblCurrentKeyboardCombo.Location = new System.Drawing.Point(72, 160);
			this.lblCurrentKeyboardCombo.Name = "lblCurrentKeyboardCombo";
			this.lblCurrentKeyboardCombo.Size = new System.Drawing.Size(368, 64);
			this.lblCurrentKeyboardCombo.TabIndex = 13;
			// 
			// label7
			// 
			this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left)));
			this.label7.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label7.Location = new System.Drawing.Point(8, 160);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(48, 64);
			this.label7.TabIndex = 12;
			this.label7.Text = "Current:";
			// 
			// label3
			// 
			this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label3.Location = new System.Drawing.Point(216, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(32, 16);
			this.label3.TabIndex = 9;
			this.label3.Text = "Keys";
			// 
			// cmbFilterCommands
			// 
			this.cmbFilterCommands.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbFilterCommands.Location = new System.Drawing.Point(64, 24);
			this.cmbFilterCommands.Name = "cmbFilterCommands";
			this.cmbFilterCommands.Size = new System.Drawing.Size(152, 21);
			this.cmbFilterCommands.TabIndex = 8;
			this.cmbFilterCommands.SelectedIndexChanged += new System.EventHandler(this.cmbFilterCommands_SelectedIndexChanged);
			// 
			// label4
			// 
			this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label4.Location = new System.Drawing.Point(8, 24);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(56, 16);
			this.label4.TabIndex = 7;
			this.label4.Text = "Command";
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(464, 446);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.grpSettingFile);
			this.Name = "MainForm";
			this.Text = "Rss Bandit Shortcuts Editor";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.grpSettingFile.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.EnableVisualStyles();
			Application.DoEvents();
			Application.Run(new MainForm());
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			openFileDialog.CheckFileExists = true;
			openFileDialog.Filter = "Shortcut Settings File (*.xml)|*.xml";
			DialogResult result = openFileDialog.ShowDialog(this);
			if(result == DialogResult.OK)
			{
				this.txtSettingsPath.Text = openFileDialog.FileName;
				LoadHandler();
			}
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			PresearchForSettingsFile();
		}

		/// <summary>
		/// Looks for existing Settings File.
		/// </summary>
		void PresearchForSettingsFile()
		{
			SearchAppDataPath(DefaultSettingsPath);
		}

		string DefaultSettingsPath
		{
			get
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"RssBandit\ShortcutSettings.xml");;
			}
		}

		private bool SearchAppDataPath(string settingsPath)
		{
			if(File.Exists(settingsPath))
			{
				this.txtSettingsPath.Text = settingsPath;
				this.openFileDialog.FileName = settingsPath;
				LoadHandler();
				return true;
			}
			return false;
		}

		void LoadHandler()
		{
			_handler.Load(this.txtSettingsPath.Text);
			this.cmbMenuCommands.DataSource = _handler.AvailableMenuCommands;
			this.cmbMenuShortcutValues.DataSource = Enum.GetNames(typeof(Shortcut));
			cmbMenuCommands_SelectedIndexChanged(null, null);
			//cmbFilterCommands_SelectedIndexChanged(null, null);

			this.cmbFilterCommands.DataSource = _handler.AvailableKeyComboCommands;
		}

		private void cmbMenuCommands_SelectedIndexChanged(object sender, EventArgs e)
		{
			if(_handler.IsDefined(SelectedMenuCommand))
			{
				this.cmbMenuShortcutValues.SelectedItem = _handler.GetShortcut(SelectedMenuCommand).ToString();
			}
			else
			{
				this.cmbMenuShortcutValues.SelectedIndex = 0;
			}
			this.lblCurrentMenuShortcut.Text = "\"" + SelectedMenuCommand + "\" is set to \"" + this.SelectedMenuShortcut + "\"";
		}

		private void btnSet_Click(object sender, EventArgs e)
		{
			this._handler.SetShortcut(SelectedMenuCommand, SelectedMenuShortcut);
		}

		private void cmbFilterCommands_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.lstKeyCombos.DataSource = SelectedKeyCombinations;
			DisplayCurrentFilterText();
		}

		void DisplayCurrentFilterText()
		{
			if(SelectedKeyCombinations.Length > 1)
			{
				this.lblCurrentKeyboardCombo.Text = "\"" + SelectedFilterCommand + "\" is set to the following keyboard combinations:" + Environment.NewLine;
				foreach(Keys combo in this.SelectedKeyCombinations)
				{
					this.lblCurrentKeyboardCombo.Text += "\"" + combo.ToString() + "\",";					
				}
				if(this.lblCurrentKeyboardCombo.Text.Length > 0)
					this.lblCurrentKeyboardCombo.Text = this.lblCurrentKeyboardCombo.Text.Substring(0, this.lblCurrentKeyboardCombo.Text.Length - 1);
			}
			else
			{
				try
				{
					this.lblCurrentKeyboardCombo.Text = "\"" + SelectedFilterCommand + "\" is set to the keyboard combination \"" + SelectedKeyCombinations[0] + "\"";
				}
				catch
				{
					this.lblCurrentKeyboardCombo.Text = "\"" + SelectedFilterCommand + "\" does not have a keyboard combination shortcut.";
				}
			}
		}

		private void btnAddShortcut_Click(object sender, System.EventArgs e)
		{
			using(ShortcutEntryForm shortcutForm = new ShortcutEntryForm())
			{
				if(shortcutForm.ShowDialog(this) == DialogResult.OK)
				{
					_handler.AddKeyboardCombination(this.SelectedFilterCommand, shortcutForm.KeyCombination);
					//TODO: This is a kluge till I improve the data binding experience.
					cmbFilterCommands_SelectedIndexChanged(null, null);
				}			
			}
		}

		private void btnRemove_Click(object sender, System.EventArgs e)
		{
			if(lstKeyCombos.SelectedIndex > -1)
			{
				_handler.RemoveKeyCombination(this.SelectedFilterCommand, lstKeyCombos.SelectedIndex);
				//TODO: This is a kluge till I improve the data binding experience.
				cmbFilterCommands_SelectedIndexChanged(null, null);
			}
		}

		private void btnSave_Click(object sender, System.EventArgs e)
		{
			saveFileDialog.FileName = this.txtSettingsPath.Text;
			saveFileDialog.Filter = "Shortcut Settings File (*.xml)|*.xml";
			if(saveFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				_handler.Write(saveFileDialog.FileName);
				this.txtSettingsPath.Text = saveFileDialog.FileName;
				this.openFileDialog.FileName = saveFileDialog.FileName;
			}
		}

		private void btnCreate_Click(object sender, System.EventArgs e)
		{
			this.saveFileDialog.FileName = this.DefaultSettingsPath;
			this.saveFileDialog.Title = "Save copy of default settings";
			this.saveFileDialog.Filter = "Shortcut Settings File (*.xml)|*.xml";
			if(saveFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				using(Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ShortcutsEditor.Resources.RssBanditSettings.xml"))
				{
					_handler.Load(stream);
				}
				_handler.Write(saveFileDialog.FileName);
				this.openFileDialog.FileName = saveFileDialog.FileName;
				this.txtSettingsPath.Text = saveFileDialog.FileName;
				LoadHandler();
			}
 
		}

		private void btnTest_Click(object sender, System.EventArgs e)
		{
			using(TestingForm testingForm = new TestingForm())
			{
				testingForm.ShortcutManager = this.ShortcutManager;
				testingForm.ShowDialog(this);
			}
		}

		/// <summary>
		/// Gets the selected menu command.
		/// </summary>
		/// <value></value>
		public string SelectedMenuCommand
		{
			get
			{
				return (string)cmbMenuCommands.SelectedValue;
			}
		}

		/// <summary>
		/// Gets the selected filter command.
		/// </summary>
		/// <value></value>
		public string SelectedFilterCommand
		{
			get
			{
				return (string)cmbFilterCommands.SelectedValue;
			}
		}


		/// <summary>
		/// Gets the selected menu shortcut.
		/// </summary>
		/// <value></value>
		public Shortcut SelectedMenuShortcut
		{
			get
			{
				return (Shortcut)Enum.Parse(typeof(Shortcut), this.cmbMenuShortcutValues.SelectedItem.ToString());
			}
		}

		/// <summary>
		/// Gets the selected key combinations.
		/// </summary>
		/// <value></value>
		public Keys[] SelectedKeyCombinations
		{
			get
			{
				return this._handler.GetKeyCombinations(SelectedFilterCommand);
			}
		}
	}
}
