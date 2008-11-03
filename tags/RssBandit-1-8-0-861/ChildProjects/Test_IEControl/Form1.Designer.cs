
namespace Test
{
	partial class Form1
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.textUrl = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.buttonPrint = new System.Windows.Forms.Button();
			this.htmlControlContainer = new System.Windows.Forms.Panel();
			this.buttonFavorites = new System.Windows.Forms.Button();
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.statusBarPanel1 = new System.Windows.Forms.StatusBarPanel();
			this.button2 = new System.Windows.Forms.Button();
			this.btnSetHTMLText = new System.Windows.Forms.Button();
			this.chkAllowTabs = new System.Windows.Forms.CheckBox();
			this.htmlControl1 = new IEControl.HtmlControl();
			this.chkAllowActiveX = new System.Windows.Forms.CheckBox();
			this.htmlControlContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanel1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.htmlControl1)).BeginInit();
			this.SuspendLayout();
			// 
			// textUrl
			// 
			this.textUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textUrl.Location = new System.Drawing.Point(16, 18);
			this.textUrl.Name = "textUrl";
			this.textUrl.Size = new System.Drawing.Size(498, 20);
			this.textUrl.TabIndex = 0;
			this.textUrl.Text = "http://www.adobe.com/";
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.ImageIndex = 0;
			this.button1.Location = new System.Drawing.Point(523, 15);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(40, 25);
			this.button1.TabIndex = 1;
			this.button1.Text = "Go";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// buttonPrint
			// 
			this.buttonPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonPrint.ImageIndex = 5;
			this.buttonPrint.Location = new System.Drawing.Point(523, 51);
			this.buttonPrint.Name = "buttonPrint";
			this.buttonPrint.Size = new System.Drawing.Size(40, 25);
			this.buttonPrint.TabIndex = 3;
			this.buttonPrint.Text = "PPV";
			this.buttonPrint.Click += new System.EventHandler(this.buttonPrint_Click);
			// 
			// htmlControlContainer
			// 
			this.htmlControlContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.htmlControlContainer.BackColor = System.Drawing.SystemColors.Desktop;
			this.htmlControlContainer.Controls.Add(this.htmlControl1);
			this.htmlControlContainer.Location = new System.Drawing.Point(16, 49);
			this.htmlControlContainer.Name = "htmlControlContainer";
			this.htmlControlContainer.Size = new System.Drawing.Size(499, 293);
			this.htmlControlContainer.TabIndex = 4;
			// 
			// buttonFavorites
			// 
			this.buttonFavorites.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonFavorites.ImageIndex = 6;
			this.buttonFavorites.Location = new System.Drawing.Point(526, 150);
			this.buttonFavorites.Name = "buttonFavorites";
			this.buttonFavorites.Size = new System.Drawing.Size(40, 25);
			this.buttonFavorites.TabIndex = 5;
			this.buttonFavorites.Text = "Fav";
			this.buttonFavorites.Click += new System.EventHandler(this.buttonFavorites_Click);
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 344);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.statusBarPanel1});
			this.statusBar1.ShowPanels = true;
			this.statusBar1.Size = new System.Drawing.Size(576, 21);
			this.statusBar1.TabIndex = 6;
			this.statusBar1.PanelClick += new System.Windows.Forms.StatusBarPanelClickEventHandler(this.statusBar1_PanelClick);
			// 
			// statusBarPanel1
			// 
			this.statusBarPanel1.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this.statusBarPanel1.Name = "statusBarPanel1";
			this.statusBarPanel1.Width = 559;
			// 
			// button2
			// 
			this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button2.ImageIndex = 5;
			this.button2.Location = new System.Drawing.Point(524, 87);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(40, 25);
			this.button2.TabIndex = 7;
			this.button2.Text = "Print";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// btnSetHTMLText
			// 
			this.btnSetHTMLText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSetHTMLText.Location = new System.Drawing.Point(528, 187);
			this.btnSetHTMLText.Name = "btnSetHTMLText";
			this.btnSetHTMLText.Size = new System.Drawing.Size(38, 36);
			this.btnSetHTMLText.TabIndex = 8;
			this.btnSetHTMLText.Text = "Set HTM";
			this.btnSetHTMLText.Click += new System.EventHandler(this.btnSetHTMLText_Click);
			// 
			// chkAllowTabs
			// 
			this.chkAllowTabs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.chkAllowTabs.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkAllowTabs.Location = new System.Drawing.Point(529, 239);
			this.chkAllowTabs.Name = "chkAllowTabs";
			this.chkAllowTabs.Size = new System.Drawing.Size(47, 18);
			this.chkAllowTabs.TabIndex = 9;
			this.chkAllowTabs.Text = "Tabs";
			this.chkAllowTabs.UseVisualStyleBackColor = true;
			this.chkAllowTabs.CheckedChanged += new System.EventHandler(this.chkAllowTabs_CheckedChanged);
			// 
			// htmlControl1
			// 
			this.htmlControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.htmlControl1.Enabled = true;
			this.htmlControl1.Location = new System.Drawing.Point(9, 11);
			this.htmlControl1.Name = "htmlControl1";
			this.htmlControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("htmlControl1.OcxState")));
			this.htmlControl1.Size = new System.Drawing.Size(479, 273);
			this.htmlControl1.TabIndex = 3;
			this.htmlControl1.NavigateComplete += new IEControl.BrowserNavigateComplete2EventHandler(this.htmlNavigateComplete);
			this.htmlControl1.CommandStateChanged += new IEControl.BrowserCommandStateChangeEventHandler(this.htmlCommandStateChanged);
			this.htmlControl1.OnQuit += new System.EventHandler(this.htmlQuit);
			this.htmlControl1.DocumentComplete += new IEControl.BrowserDocumentCompleteEventHandler(this.htmlDocumentComplete);
			this.htmlControl1.StatusTextChanged += new IEControl.BrowserStatusTextChangeEventHandler(this.htmlStatusTextChanged);
			this.htmlControl1.WindowClosing += new IEControl.BrowserWindowClosingEventHandler(this.htmlWindowClosing);
			this.htmlControl1.BeforeNavigate += new IEControl.BrowserBeforeNavigate2EventHandler(this.htmlBeforeNavigate);
			this.htmlControl1.NewWindow2 += new IEControl.BrowserNewWindow2EventHandler(this.htmlControl1_NewWindow2);
			this.htmlControl1.NewWindow3 += new System.EventHandler<IEControl.BrowserNewWindow3Event>(this.htmlControl1_NewWindow3);
			this.htmlControl1.NewWindow += new IEControl.BrowserNewWindowEventHandler(this.htmlControl1_NewWindow);
			// 
			// chkAllowActiveX
			// 
			this.chkAllowActiveX.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.chkAllowActiveX.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkAllowActiveX.Location = new System.Drawing.Point(529, 263);
			this.chkAllowActiveX.Name = "chkAllowActiveX";
			this.chkAllowActiveX.Size = new System.Drawing.Size(47, 18);
			this.chkAllowActiveX.TabIndex = 10;
			this.chkAllowActiveX.Text = "ActX";
			this.chkAllowActiveX.UseVisualStyleBackColor = true;
			this.chkAllowActiveX.CheckedChanged += new System.EventHandler(this.chkAllowActiveX_CheckedChanged);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(576, 365);
			this.Controls.Add(this.chkAllowActiveX);
			this.Controls.Add(this.chkAllowTabs);
			this.Controls.Add(this.btnSetHTMLText);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.statusBar1);
			this.Controls.Add(this.buttonFavorites);
			this.Controls.Add(this.htmlControlContainer);
			this.Controls.Add(this.buttonPrint);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.textUrl);
			this.Name = "Form1";
			this.Text = "Test IE Control";
			this.htmlControlContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanel1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.htmlControl1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox chkAllowTabs;
		private System.Windows.Forms.CheckBox chkAllowActiveX;

	}
}
