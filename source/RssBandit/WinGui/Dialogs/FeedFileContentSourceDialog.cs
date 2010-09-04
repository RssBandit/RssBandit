#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Genghis.Windows.Forms;
using NewsComponents.Net;
using RssBandit.Common;
using RssBandit.Common.Logging;
using RssBandit.Resources;

namespace RssBandit.WinGui.Dialogs
{
	internal class FeedFileContentSourceDialog : DialogBase
	{
		private readonly string feedUrl;
		private readonly ICredentials feedCredentials;
		private readonly IWebProxy proxy;
		
		private RichTextBox txtSource;
		private System.Timers.Timer timer;
		private System.ComponentModel.IContainer components=null;

		public FeedFileContentSourceDialog(IWebProxy proxy, ICredentials feedCredentials, string sourceUrl, string title)
		{
			this.feedUrl = sourceUrl;
			this.feedCredentials = feedCredentials;
			this.proxy = proxy;
			
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			base.Text = title;
			this.txtSource.Text = SR.GUIStatusLoadingChildItems;
			
			horizontalEdge.Visible = false;
			btnSubmit.Visible = false;
			btnCancel.Visible = false;
			this.KeyPreview = true;
			
		}

		private Stream GetFeedSourceStream() {
			return AsyncWebRequest.GetSyncResponseStream(this.feedUrl, this.feedCredentials, RssBanditApplication.UserAgent, this.proxy);			
		}
		
		private void LoadAndFormatFeedSource() {
			try {
				using (new CursorChanger(Cursors.WaitCursor)) {
					using (StreamReader reader = new StreamReader(GetFeedSourceStream())) {
						XmlDocument doc = new XmlDocument();
						doc.Load(reader);
						StringBuilder sb = new StringBuilder(); 
						XmlTextWriter writer = new XmlTextWriter(new StringWriter(sb)); 
						writer.Formatting = Formatting.Indented; 
						writer.Indentation = 4;
						writer.IndentChar = ' ';
						doc.Save(writer);
						this.txtSource.Text = sb.ToString();
						this.txtSource.ScrollBars = RichTextBoxScrollBars.ForcedBoth;
					}
				}
			} catch (Exception ex) {
				this.txtSource.Text = String.Format(SR.ExceptionGeneral,ex.Message);
			} 
		}
		
		/// <summary>
		/// Convert Font to a serializable string
		/// </summary>
		/// <param name="font"></param>
		/// <returns></returns>
		static string FontToString(Font font) {
			FontConverter oFontConv = new FontConverter();
			return oFontConv.ConvertToString(null,CultureInfo.InvariantCulture,font);
		}
		/// <summary>
		/// Transform string name to font.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		static Font StringToFont(string name, Font defaultValue) {
			if (string.IsNullOrEmpty(name))
				return defaultValue;
			try {
					FontConverter oFontConv = new FontConverter();
					return oFontConv.ConvertFromString(null, CultureInfo.InvariantCulture, name) as Font;
			} catch {
				return defaultValue;
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

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.txtSource = new System.Windows.Forms.RichTextBox();
			this.timer = new System.Timers.Timer();
			((System.ComponentModel.ISupportInitialize)(this.timer)).BeginInit();
			this.SuspendLayout();
			// 
			// btnCancel
			// 
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Visible = false;
			// 
			// horizontalEdge
			// 
			this.horizontalEdge.Name = "horizontalEdge";
			this.horizontalEdge.Visible = false;
			// 
			// btnSubmit
			// 
			this.btnSubmit.Name = "btnSubmit";
			this.btnSubmit.Visible = false;
			// 
			// windowSerializer
			// 
			this.windowSerializer.LoadStateEvent += new RssBandit.WinGui.Controls.WindowSerializer.WindowSerializerDelegate(this.OnLoadState);
			this.windowSerializer.SaveStateEvent += new RssBandit.WinGui.Controls.WindowSerializer.WindowSerializerDelegate(this.OnSaveState);
			// 
			// txtSource
			// 
			this.txtSource.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtSource.HideSelection = false;
			this.txtSource.Location = new System.Drawing.Point(0, 0);
			this.txtSource.Name = "txtSource";
			this.txtSource.ReadOnly = true;
			this.txtSource.ShowSelectionMargin = true;
			this.txtSource.Size = new System.Drawing.Size(387, 251);
			this.txtSource.TabIndex = 102;
			this.txtSource.Text = "";
			this.txtSource.WordWrap = false;
			this.txtSource.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnTxtMouseDown);
			this.txtSource.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnTxtKeyPress);
			this.txtSource.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.OnLinkClicked);
			// 
			// timer
			// 
			this.timer.SynchronizingObject = this;
			this.timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimerElapsed);
			// 
			// FeedSourceDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.ClientSize = new System.Drawing.Size(387, 251);
			this.Controls.Add(this.txtSource);
			this.KeyPreview = true;
			this.MaximizeBox = true;
			this.Name = "FeedSourceDialog";
			this.ShowInTaskbar = true;
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.Controls.SetChildIndex(this.txtSource, 0);
			this.Controls.SetChildIndex(this.btnSubmit, 0);
			this.Controls.SetChildIndex(this.btnCancel, 0);
			this.Controls.SetChildIndex(this.horizontalEdge, 0);
			((System.ComponentModel.ISupportInitialize)(this.timer)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e) {
			this.timer.Enabled = false;
			LoadAndFormatFeedSource();
		}

		private void OnFormLoad(object sender, EventArgs e) {
			this.timer.Enabled = true;
		}

		private void OnLinkClicked(object sender, LinkClickedEventArgs e) {
				
			try {
				if (!string.IsNullOrEmpty(e.LinkText)) {
					Uri uri = new Uri(e.LinkText);
					Process.Start(uri.CanonicalizedUri());
				}
					
			} catch (Exception ex) {
				DefaultLog.Error("Cannot navigate to url '" + e.LinkText + "'", ex);
			}
		}

		private void OnTxtMouseDown(object sender, MouseEventArgs e) {
			if (MouseButtons == MouseButtons.Left) {
				try {
					if (ModifierKeys == Keys.Alt) {
						using (FontDialog fntDialog = new FontDialog() ) {
							fntDialog.Font = this.txtSource.Font;
							if (DialogResult.OK == fntDialog.ShowDialog(this)) {
								this.txtSource.Font = fntDialog.Font;
							}
						}
					} else
					if (ModifierKeys == Keys.Control) {
						this.txtSource.ZoomFactor += 0.25f;
					} else if (ModifierKeys == (Keys.Control | Keys.Shift)) {
						this.txtSource.ZoomFactor -= 0.25f;
					}
				} catch (ArgumentException) {
				}
			}
		}

		private void OnLoadState(object sender, Genghis.Preferences preferences) {
			this.txtSource.Font = StringToFont(preferences.GetString("fnt", null), this.txtSource.Font);
			this.txtSource.ZoomFactor = preferences.GetSingle("zoom", 1.0f);
		}

		private void OnSaveState(object sender, Genghis.Preferences preferences) {
			preferences.SetProperty("fnt", FontToString(this.txtSource.Font));
			preferences.SetProperty("zoom", this.txtSource.ZoomFactor);
		}

		private void OnTxtKeyPress(object sender, KeyPressEventArgs e) {
#if TRACE_WIN_MESSAGES	
			Debug.WriteLine("OnTxtKeyPress(" + e.KeyChar +")");
#endif
			if (ModifierKeys == Keys.Control && (e.KeyChar == 'a' || e.KeyChar == 'A')) {
				this.txtSource.SelectAll();
				e.Handled = true;
			}
		}
		
	}
}

