#region Copyright
/*
Copyright (c) 2004-2006 by Torsten Rendelmann

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */
#endregion

using System;
using System.Windows.Forms;

using System.Diagnostics;
using System.Runtime.InteropServices;
using IEControl;

namespace Test
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{

		private const string htmText = "<html><head><title>Test</title><script language='javascript'>function Button1_onclick() { external.MyCustomMethod('MyCustomMethod(Hello) called');	}</script></head>"+
			"<body>"+"The quick <B>brown</B> fox jumps over <a target='_blank' href=\"http://www.rssbandit.org/\">www.rssbandit.org/</a>, to .NET.<br />"+
			"Click <a href=\"javascript:Button1_onclick()\">here</a> to call a custom method in the main application.<br />"+
			"Click <a href=\"javascript:window.close()\">here</a> to close this window."+
			"</body></html>";
		private System.Windows.Forms.TextBox textUrl;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button buttonPrint;
		private System.Windows.Forms.Panel htmlControlContainer;
		private IEControl.HtmlControl htmlControl1;
		private System.Windows.Forms.Button buttonFavorites;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.StatusBarPanel statusBarPanel1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button btnSetHTMLText;

		private System.ComponentModel.Container components = null;

		public Form1() {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			
			//htmlControl1.EnhanceBrowserSecurityForProcess();
			htmlControl1.FrameDownloadEnabled = true;
			htmlControl1.FlatScrollBars = true;
			bool useX = false;
			
			if (useX) 
			{
				HtmlControl.SetInternetFeatureEnabled(
					InternetFeatureList.FEATURE_RESTRICT_ACTIVEXINSTALL,
					SetFeatureFlag.SET_FEATURE_ON_PROCESS, true);
				htmlControl1.ActiveXEnabled = true;
			}
			else 
			{
				htmlControl1.ActiveXEnabled = true;
				HtmlControl.SetInternetFeatureEnabled(
					InternetFeatureList.FEATURE_SECURITYBAND,
					SetFeatureFlag.SET_FEATURE_ON_PROCESS, true);
				HtmlControl.SetInternetFeatureEnabled(
					InternetFeatureList.FEATURE_RESTRICT_ACTIVEXINSTALL,
					SetFeatureFlag.SET_FEATURE_ON_PROCESS, false);
				HtmlControl.SetInternetFeatureEnabled(
					InternetFeatureList.FEATURE_RESTRICT_FILEDOWNLOAD,
					SetFeatureFlag.SET_FEATURE_ON_PROCESS, false);
				
				
				
			}
			
			htmlControl1.ImagesDownloadEnabled = true;
			htmlControl1.SilentModeEnabled = true;
			htmlControl1.Clear();
			htmlControl1.ScriptObject = new HTMLBrowserExternalCallImplementation();
			htmlControl1.Html = htmText;
			
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Form1));
			this.textUrl = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.buttonPrint = new System.Windows.Forms.Button();
			this.htmlControlContainer = new System.Windows.Forms.Panel();
			this.htmlControl1 = new IEControl.HtmlControl();
			this.buttonFavorites = new System.Windows.Forms.Button();
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.statusBarPanel1 = new System.Windows.Forms.StatusBarPanel();
			this.button2 = new System.Windows.Forms.Button();
			this.btnSetHTMLText = new System.Windows.Forms.Button();
			this.htmlControlContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.htmlControl1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanel1)).BeginInit();
			this.SuspendLayout();
			// 
			// textUrl
			// 
			this.textUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.textUrl.Location = new System.Drawing.Point(16, 18);
			this.textUrl.Name = "textUrl";
			this.textUrl.Size = new System.Drawing.Size(445, 20);
			this.textUrl.TabIndex = 0;
			this.textUrl.Text = "http://www.adobe.com/";
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.ImageIndex = 0;
			this.button1.Location = new System.Drawing.Point(470, 15);
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
			this.buttonPrint.Location = new System.Drawing.Point(470, 51);
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
			this.htmlControlContainer.Size = new System.Drawing.Size(446, 259);
			this.htmlControlContainer.TabIndex = 4;
			// 
			// htmlControl1
			// 
			this.htmlControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.htmlControl1.ContainingControl = this;
			this.htmlControl1.Enabled = true;
			this.htmlControl1.Location = new System.Drawing.Point(9, 11);
			this.htmlControl1.Name = "htmlControl1";
			this.htmlControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("htmlControl1.OcxState")));
			this.htmlControl1.Size = new System.Drawing.Size(426, 239);
			this.htmlControl1.TabIndex = 3;
			this.htmlControl1.NewWindow += new IEControl.BrowserNewWindowEventHandler(this.htmlControl1_NewWindow);
			this.htmlControl1.WindowClosing += new IEControl.BrowserWindowClosingEventHandler(this.htmlWindowClosing);
			this.htmlControl1.CommandStateChanged += new IEControl.BrowserCommandStateChangeEventHandler(this.htmlCommandStateChanged);
			this.htmlControl1.OnQuit += new System.EventHandler(this.htmlQuit);
			this.htmlControl1.NavigateComplete += new IEControl.BrowserNavigateComplete2EventHandler(this.htmlNavigateComplete);
			this.htmlControl1.StatusTextChanged += new IEControl.BrowserStatusTextChangeEventHandler(this.htmlStatusTextChanged);
			this.htmlControl1.DocumentComplete += new IEControl.BrowserDocumentCompleteEventHandler(this.htmlDocumentComplete);
			this.htmlControl1.BeforeNavigate += new IEControl.BrowserBeforeNavigate2EventHandler(this.htmlBeforeNavigate);
			// 
			// buttonFavorites
			// 
			this.buttonFavorites.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonFavorites.ImageIndex = 6;
			this.buttonFavorites.Location = new System.Drawing.Point(473, 150);
			this.buttonFavorites.Name = "buttonFavorites";
			this.buttonFavorites.Size = new System.Drawing.Size(40, 25);
			this.buttonFavorites.TabIndex = 5;
			this.buttonFavorites.Text = "Fav";
			this.buttonFavorites.Click += new System.EventHandler(this.buttonFavorites_Click);
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 310);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																						  this.statusBarPanel1});
			this.statusBar1.ShowPanels = true;
			this.statusBar1.Size = new System.Drawing.Size(523, 21);
			this.statusBar1.TabIndex = 6;
			this.statusBar1.PanelClick += new System.Windows.Forms.StatusBarPanelClickEventHandler(this.statusBar1_PanelClick);
			// 
			// statusBarPanel1
			// 
			this.statusBarPanel1.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this.statusBarPanel1.Width = 507;
			// 
			// button2
			// 
			this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button2.ImageIndex = 5;
			this.button2.Location = new System.Drawing.Point(471, 87);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(40, 25);
			this.button2.TabIndex = 7;
			this.button2.Text = "Print";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// btnSetHTMLText
			// 
			this.btnSetHTMLText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSetHTMLText.Location = new System.Drawing.Point(475, 187);
			this.btnSetHTMLText.Name = "btnSetHTMLText";
			this.btnSetHTMLText.Size = new System.Drawing.Size(38, 36);
			this.btnSetHTMLText.TabIndex = 8;
			this.btnSetHTMLText.Text = "Set HTM";
			this.btnSetHTMLText.Click += new System.EventHandler(this.btnSetHTMLText_Click);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(523, 331);
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
			((System.ComponentModel.ISupportInitialize)(this.htmlControl1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanel1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void button1_Click(object sender, System.EventArgs e) {
			htmlControl1.Navigate(textUrl.Text);
		}

		private void buttonPrint_Click(object sender, System.EventArgs e) {
			htmlControl1.ShowDialogPrintPreview();
		}

		private void htmlBeforeNavigate(object sender, BrowserBeforeNavigate2Event e) {
			Trace.WriteLine("htmlBeforeNavigate(): "+ e.url);
			bool test = e.pDisp == ((HtmlControl)sender).GetOcx();
			Trace.WriteLine("htmlBeforeNavigate(): isRootPage: "+ ( e.IsRootPage.ToString()));
			if (!e.IsRootPage) {
				e.Cancel = true;	// cancel all the subsequent iframe/javascript etc. requests
			}
		}

		private void buttonFavorites_Click(object sender, System.EventArgs e) {
			string title = "new entry";
			string url = textUrl.Text;
			htmlControl1.ShowDialogAddFavorite(url, title);
			Trace.WriteLine("buttonFavorites(): " + (title != null ? title: "null"));
		}

		private void htmlNavigateComplete(object sender, BrowserNavigateComplete2Event e) {
			string url = e.url;
			Trace.WriteLine("htmlNavigateComplete(): "+ e.url);
			bool rootPage = e.IsRootPage;
			if (rootPage) {
				if (url != "about:blank" )
					textUrl.Text= url;
				htmlControl1.Focus();
			}
		}

		private void htmlDocumentComplete(object sender, BrowserDocumentCompleteEvent e) {
			string url = e.url;
			Trace.WriteLine("htmlDocumentComplete(): "+ url);
			bool complete = e.IsRootPage;
			if (complete) {
				if (url != "about:blank" )
					textUrl.Text= url;
				htmlControl1.Focus();
			}
		}

		private void htmlWindowClosing(object sender, BrowserWindowClosingEvent e) {
			if (MessageBox.Show("You clicked a link or javascript try to close the window." + Environment.NewLine + "Do you want to proceed?", "htmlWindowClosing Event", MessageBoxButtons.YesNo) == DialogResult.No)
				e.Cancel = true;
		}

		private void htmlQuit(object sender, System.EventArgs e) {
			MessageBox.Show("Exiting application...", "OnQuit Event", MessageBoxButtons.OK);
			this.Close();
		}

		private void statusBar1_PanelClick(object sender, System.Windows.Forms.StatusBarPanelClickEventArgs e) {
			this.htmlControl1.Html = @"<html><head><title>Hällo World from SetHtmlText()</title></head><body><p>Hällo, World</p></body></html>";
			this.htmlControl1.Navigate(null);
		}

		private void htmlCommandStateChanged(object sender, BrowserCommandStateChangeEvent e) {
			Trace.WriteLine("htmlCommandStateChanged(): "+ e.command.ToString() + ":" + e.enable.ToString());
		}

		private void htmlStatusTextChanged(object sender, BrowserStatusTextChangeEvent e) {
			this.statusBar1.Panels[0].Text = e.text;
		}

		private void htmlControl1_NewWindow(object sender, BrowserNewWindowEvent e) {
			string url = e.url;
			Trace.WriteLine("htmlDocumentNewWindow(): "+ url);
			if (MessageBox.Show("You clicked '"+e.url+"', that try to open a new window." + Environment.NewLine + "Do you want to proceed?", "htmlNewWindow Event", MessageBoxButtons.YesNo) == DialogResult.No)
				e.Cancel = true;
		}

		private void button2_Click(object sender, System.EventArgs e) {
			htmlControl1.ShowDialogPrint();
		}

		private void btnSetHTMLText_Click(object sender, System.EventArgs e) {
			for (int i=0; i < 50; i++) {
				htmlControl1.Html = htmText;
				this.htmlControl1.Navigate(null);
				if (i % 2 == 0) Application.DoEvents();
			}
		}
	}
	
	/// <summary>
	/// Demonstrate the usage/impl. of a external object.
	/// Important is the flag COMVisible(true) to get it work.
	/// Javascript can now call external.MyCustomMethod("Hello");
	/// </summary>
	[ComVisible(true)]
	public class HTMLBrowserExternalCallImplementation {
		public void MyCustomMethod([MarshalAs(UnmanagedType.BStr)] string theCaption) {
			// just use a MessageBox
			string msg = "This messageBox was shown by a call from script on an HTML page, such as \n\n" +
				"<script language=\"javascript\">\n" + 
				"function Button1_onclick()\n" + 
				"{\n" +
				"  external.MyCustomMethod();\n" + 
				"}\n" + 
				"</script>\n\n" +
				"You can also pass parameters to the custom method.\n" +
				"The parameter passed in this demo was the string '" + theCaption + "'.";

			string caption = "Custom callback using javscript external.Function()";
			MessageBox.Show(msg, caption);
		}		
	}
}
