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
	public partial class Form1 : Form
	{

		private const string htmText = "<html><head><title>Test</title><script language='javascript'>function Button1_onclick() { external.MyCustomMethod('MyCustomMethod(Hello) called');	}</script></head>"+
			"<body>"+"The quick <B>brown</B> fox jumps over <a target='_blank' href=\"http://www.rssbandit.org/\">www.rssbandit.org/</a>, to .NET.<br />"+
			"Click <a href=\"javascript:Button1_onclick()\">here</a> to call a custom method in the main application.<br />"+
			"Click <a href=\"javascript:window.close()\">here</a> to close this window."+
			"</body></html>";
		private TextBox textUrl;
		private Button button1;
		private Button buttonPrint;
		private Panel htmlControlContainer;
		private HtmlControl htmlControl1;
		private Button buttonFavorites;
		private StatusBar statusBar1;
		private StatusBarPanel statusBarPanel1;
		private Button button2;
		private Button btnSetHTMLText;

		private System.ComponentModel.Container components;

		public Form1() {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			
			//htmlControl1.EnhanceBrowserSecurityForProcess();
			htmlControl1.FrameDownloadEnabled = true;
			htmlControl1.FlatScrollBars = true;
			bool useX = false;
			
			if (useX) {
				HtmlControl.SetInternetFeatureEnabled(
					InternetFeatureList.FEATURE_RESTRICT_ACTIVEXINSTALL,
					SetFeatureFlag.SET_FEATURE_ON_PROCESS, true);
				htmlControl1.ActiveXEnabled = true;
			}
			else {
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
			htmlControl1.SilentModeEnabled = false;
			htmlControl1.Clear();
			htmlControl1.ScriptObject = new HTMLBrowserExternalCallImplementation();
			htmlControl1.Html = htmText;

			chkAllowActiveX.Checked = htmlControl1.ActiveXEnabled;

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


		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void button1_Click(object sender, EventArgs e) {
			htmlControl1.Navigate(textUrl.Text);
		}

		private void buttonPrint_Click(object sender, EventArgs e) {
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

		private void buttonFavorites_Click(object sender, EventArgs e) {
			string title = "new entry";
			string url = textUrl.Text;
			htmlControl1.ShowDialogAddFavorite(url, title);
			Trace.WriteLine("buttonFavorites(): called");
		}

		private void WindowLoad(IHTMLEventObj e){
			
			Trace.WriteLine("onload(): " + e.Reason);		
		}

		private void WindowError(string description, string url, int line) {	
			Console.WriteLine("{0} on line {1} while processing {2}", description, url, line);
			IHTMLWindow2 window = (IHTMLWindow2) htmlControl1.Document2.GetParentWindow();			
			IHTMLEventObj eventObj = window.eventobj;
			//eventObj.CancelBubble = true; 
			eventObj.ReturnValue = true;
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

			HTMLWindowEvents2_Event window = (HTMLWindowEvents2_Event) htmlControl1.Document2.GetParentWindow();
			window.onerror += this.WindowError;					
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

			HTMLWindowEvents2_Event window = (HTMLWindowEvents2_Event) htmlControl1.Document2.GetParentWindow();
			window.onload  += this.WindowLoad;					
			window.onerror += this.WindowError;			
		}

		private void htmlWindowClosing(object sender, BrowserWindowClosingEvent e) {
			if (MessageBox.Show("You clicked a link or javascript try to close the window." + Environment.NewLine + "Do you want to proceed?", "htmlWindowClosing Event", MessageBoxButtons.YesNo) == DialogResult.No)
				e.Cancel = true;
		}

		private void htmlQuit(object sender, EventArgs e) {
			MessageBox.Show("Exiting application...", "OnQuit Event", MessageBoxButtons.OK);
			this.Close();
		}

		private void statusBar1_PanelClick(object sender, StatusBarPanelClickEventArgs e) {
			this.htmlControl1.Html = @"<html><head><title>Hällo World from SetHtmlText()</title></head><body><p>Hällo, World</p></body></html>";
			this.htmlControl1.Navigate(null);
		}

		private void htmlCommandStateChanged(object sender, BrowserCommandStateChangeEvent e) {
			//Trace.WriteLine("htmlCommandStateChanged(): "+ e.command.ToString() + ":" + e.enable.ToString());
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
		private void htmlControl1_NewWindow2(object sender, BrowserNewWindow2Event e)
		{
			object url = e.ppDisp;
			Trace.WriteLine("htmlDocumentNewWindow2(): " + url);
			if (MessageBox.Show("You clicked '" + url + "', that try to open a new window/tab." + Environment.NewLine + "Do you want to proceed?", "htmlNewWindow2 Event", MessageBoxButtons.YesNo) == DialogResult.No)
				e.Cancel = true;
		}
		void htmlControl1_NewWindow3(object sender, BrowserNewWindow3Event e)
		{
			string url = e.bstrUrl;
			string what = "window";
			if (IEControl.Interop.NWMF.NWMF_FORCETAB == (e.dwFlags & IEControl.Interop.NWMF.NWMF_FORCETAB))
				what = "tab";
			
			Trace.WriteLine("htmlDocumentNewWindow3(): " + url);
			if (MessageBox.Show("You clicked '" + url + "', that try to open a new " + what + Environment.NewLine + e.dwFlags.ToString() + Environment.NewLine + "Do you want to proceed?", "htmlNewWindow3 Event", MessageBoxButtons.YesNo) == DialogResult.No)
				e.Cancel = true;
		}

		private void button2_Click(object sender, EventArgs e) {
			htmlControl1.ShowDialogPrint();
		}

		private void btnSetHTMLText_Click(object sender, EventArgs e) {
			for (int i=0; i < 50; i++) {
				htmlControl1.Html = htmText;
				this.htmlControl1.Navigate(null);
				if (i % 2 == 0) Application.DoEvents();
			}
		}

		private void chkAllowTabs_CheckedChanged(object sender, EventArgs e)
		{
			HtmlControl.SetInternetFeatureEnabled(
				InternetFeatureList.FEATURE_TABBED_BROWSING,
				SetFeatureFlag.SET_FEATURE_ON_PROCESS, chkAllowTabs.Checked);
		}

		private void chkAllowActiveX_CheckedChanged(object sender, EventArgs e)
		{
			htmlControl1.ActiveXEnabled = chkAllowActiveX.Checked;
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
