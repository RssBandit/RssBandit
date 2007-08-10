#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using RssBandit.WinGui.Utility;

namespace RssBandit.WinGui.Forms {
	/// <summary>
	/// Summary description for AutoDiscoverFeedsDialog.
	/// </summary>
	public class AutoDiscoverFeedsDialog : System.Windows.Forms.Form {

		private UrlCompletionExtender urlExtender;
		
		private System.Windows.Forms.Button btnFindFeed;
        private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TabControl tabFindByWhat;
		private System.Windows.Forms.TabPage tabPageFindByUrl;
		private System.Windows.Forms.TabPage tabPageFindByKeyword;
		private System.Windows.Forms.TextBox textBoxUrl;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxKeywords;
		private System.Windows.Forms.Label label2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AutoDiscoverFeedsDialog() {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			urlExtender = new UrlCompletionExtender(this);
			urlExtender.Add(this.textBoxUrl);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if(components != null) {
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
		private void InitializeComponent() {
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AutoDiscoverFeedsDialog));
			this.btnFindFeed = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.tabFindByWhat = new System.Windows.Forms.TabControl();
			this.tabPageFindByUrl = new System.Windows.Forms.TabPage();
			this.textBoxUrl = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.tabPageFindByKeyword = new System.Windows.Forms.TabPage();
			this.textBoxKeywords = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.tabFindByWhat.SuspendLayout();
			this.tabPageFindByUrl.SuspendLayout();
			this.tabPageFindByKeyword.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnFindFeed
			// 
			this.btnFindFeed.AccessibleDescription = resources.GetString("btnFindFeed.AccessibleDescription");
			this.btnFindFeed.AccessibleName = resources.GetString("btnFindFeed.AccessibleName");
			this.btnFindFeed.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnFindFeed.Anchor")));
			this.btnFindFeed.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnFindFeed.BackgroundImage")));
			this.btnFindFeed.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnFindFeed.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnFindFeed.Dock")));
			this.btnFindFeed.Enabled = ((bool)(resources.GetObject("btnFindFeed.Enabled")));
			this.btnFindFeed.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnFindFeed.FlatStyle")));
			this.btnFindFeed.Font = ((System.Drawing.Font)(resources.GetObject("btnFindFeed.Font")));
			this.btnFindFeed.Image = ((System.Drawing.Image)(resources.GetObject("btnFindFeed.Image")));
			this.btnFindFeed.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnFindFeed.ImageAlign")));
			this.btnFindFeed.ImageIndex = ((int)(resources.GetObject("btnFindFeed.ImageIndex")));
			this.btnFindFeed.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnFindFeed.ImeMode")));
			this.btnFindFeed.Location = ((System.Drawing.Point)(resources.GetObject("btnFindFeed.Location")));
			this.btnFindFeed.Name = "btnFindFeed";
			this.btnFindFeed.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnFindFeed.RightToLeft")));
			this.btnFindFeed.Size = ((System.Drawing.Size)(resources.GetObject("btnFindFeed.Size")));
			this.btnFindFeed.TabIndex = ((int)(resources.GetObject("btnFindFeed.TabIndex")));
			this.btnFindFeed.Text = resources.GetString("btnFindFeed.Text");
			this.btnFindFeed.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnFindFeed.TextAlign")));
			this.btnFindFeed.Visible = ((bool)(resources.GetObject("btnFindFeed.Visible")));
			// 
			// btnCancel
			// 
			this.btnCancel.AccessibleDescription = resources.GetString("btnCancel.AccessibleDescription");
			this.btnCancel.AccessibleName = resources.GetString("btnCancel.AccessibleName");
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnCancel.Anchor")));
			this.btnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCancel.BackgroundImage")));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnCancel.Dock")));
			this.btnCancel.Enabled = ((bool)(resources.GetObject("btnCancel.Enabled")));
			this.btnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnCancel.FlatStyle")));
			this.btnCancel.Font = ((System.Drawing.Font)(resources.GetObject("btnCancel.Font")));
			this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
			this.btnCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.ImageAlign")));
			this.btnCancel.ImageIndex = ((int)(resources.GetObject("btnCancel.ImageIndex")));
			this.btnCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnCancel.ImeMode")));
			this.btnCancel.Location = ((System.Drawing.Point)(resources.GetObject("btnCancel.Location")));
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnCancel.RightToLeft")));
			this.btnCancel.Size = ((System.Drawing.Size)(resources.GetObject("btnCancel.Size")));
			this.btnCancel.TabIndex = ((int)(resources.GetObject("btnCancel.TabIndex")));
			this.btnCancel.Text = resources.GetString("btnCancel.Text");
			this.btnCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.TextAlign")));
			this.btnCancel.Visible = ((bool)(resources.GetObject("btnCancel.Visible")));
			// 
			// tabFindByWhat
			// 
			this.tabFindByWhat.AccessibleDescription = resources.GetString("tabFindByWhat.AccessibleDescription");
			this.tabFindByWhat.AccessibleName = resources.GetString("tabFindByWhat.AccessibleName");
			this.tabFindByWhat.Alignment = ((System.Windows.Forms.TabAlignment)(resources.GetObject("tabFindByWhat.Alignment")));
			this.tabFindByWhat.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabFindByWhat.Anchor")));
			this.tabFindByWhat.Appearance = ((System.Windows.Forms.TabAppearance)(resources.GetObject("tabFindByWhat.Appearance")));
			this.tabFindByWhat.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabFindByWhat.BackgroundImage")));
			this.tabFindByWhat.Controls.Add(this.tabPageFindByUrl);
			this.tabFindByWhat.Controls.Add(this.tabPageFindByKeyword);
			this.tabFindByWhat.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabFindByWhat.Dock")));
			this.tabFindByWhat.Enabled = ((bool)(resources.GetObject("tabFindByWhat.Enabled")));
			this.tabFindByWhat.Font = ((System.Drawing.Font)(resources.GetObject("tabFindByWhat.Font")));
			this.tabFindByWhat.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabFindByWhat.ImeMode")));
			this.tabFindByWhat.ItemSize = ((System.Drawing.Size)(resources.GetObject("tabFindByWhat.ItemSize")));
			this.tabFindByWhat.Location = ((System.Drawing.Point)(resources.GetObject("tabFindByWhat.Location")));
			this.tabFindByWhat.Name = "tabFindByWhat";
			this.tabFindByWhat.Padding = ((System.Drawing.Point)(resources.GetObject("tabFindByWhat.Padding")));
			this.tabFindByWhat.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabFindByWhat.RightToLeft")));
			this.tabFindByWhat.SelectedIndex = 0;
			this.tabFindByWhat.ShowToolTips = ((bool)(resources.GetObject("tabFindByWhat.ShowToolTips")));
			this.tabFindByWhat.Size = ((System.Drawing.Size)(resources.GetObject("tabFindByWhat.Size")));
			this.tabFindByWhat.TabIndex = ((int)(resources.GetObject("tabFindByWhat.TabIndex")));
			this.tabFindByWhat.Text = resources.GetString("tabFindByWhat.Text");
			this.tabFindByWhat.Visible = ((bool)(resources.GetObject("tabFindByWhat.Visible")));
			// 
			// tabPageFindByUrl
			// 
			this.tabPageFindByUrl.AccessibleDescription = resources.GetString("tabPageFindByUrl.AccessibleDescription");
			this.tabPageFindByUrl.AccessibleName = resources.GetString("tabPageFindByUrl.AccessibleName");
			this.tabPageFindByUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabPageFindByUrl.Anchor")));
			this.tabPageFindByUrl.AutoScroll = ((bool)(resources.GetObject("tabPageFindByUrl.AutoScroll")));
			this.tabPageFindByUrl.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabPageFindByUrl.AutoScrollMargin")));
			this.tabPageFindByUrl.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabPageFindByUrl.AutoScrollMinSize")));
			this.tabPageFindByUrl.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabPageFindByUrl.BackgroundImage")));
			this.tabPageFindByUrl.Controls.Add(this.textBoxUrl);
			this.tabPageFindByUrl.Controls.Add(this.label1);
			this.tabPageFindByUrl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabPageFindByUrl.Dock")));
			this.tabPageFindByUrl.Enabled = ((bool)(resources.GetObject("tabPageFindByUrl.Enabled")));
			this.tabPageFindByUrl.Font = ((System.Drawing.Font)(resources.GetObject("tabPageFindByUrl.Font")));
			this.tabPageFindByUrl.ImageIndex = ((int)(resources.GetObject("tabPageFindByUrl.ImageIndex")));
			this.tabPageFindByUrl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabPageFindByUrl.ImeMode")));
			this.tabPageFindByUrl.Location = ((System.Drawing.Point)(resources.GetObject("tabPageFindByUrl.Location")));
			this.tabPageFindByUrl.Name = "tabPageFindByUrl";
			this.tabPageFindByUrl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabPageFindByUrl.RightToLeft")));
			this.tabPageFindByUrl.Size = ((System.Drawing.Size)(resources.GetObject("tabPageFindByUrl.Size")));
			this.tabPageFindByUrl.TabIndex = ((int)(resources.GetObject("tabPageFindByUrl.TabIndex")));
			this.tabPageFindByUrl.Text = resources.GetString("tabPageFindByUrl.Text");
			this.tabPageFindByUrl.ToolTipText = resources.GetString("tabPageFindByUrl.ToolTipText");
			this.tabPageFindByUrl.Visible = ((bool)(resources.GetObject("tabPageFindByUrl.Visible")));
			// 
			// textBoxUrl
			// 
			this.textBoxUrl.AccessibleDescription = resources.GetString("textBoxUrl.AccessibleDescription");
			this.textBoxUrl.AccessibleName = resources.GetString("textBoxUrl.AccessibleName");
			this.textBoxUrl.AllowDrop = true;
			this.textBoxUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textBoxUrl.Anchor")));
			this.textBoxUrl.AutoSize = ((bool)(resources.GetObject("textBoxUrl.AutoSize")));
			this.textBoxUrl.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textBoxUrl.BackgroundImage")));
			this.textBoxUrl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textBoxUrl.Dock")));
			this.textBoxUrl.Enabled = ((bool)(resources.GetObject("textBoxUrl.Enabled")));
			this.textBoxUrl.Font = ((System.Drawing.Font)(resources.GetObject("textBoxUrl.Font")));
			this.textBoxUrl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textBoxUrl.ImeMode")));
			this.textBoxUrl.Location = ((System.Drawing.Point)(resources.GetObject("textBoxUrl.Location")));
			this.textBoxUrl.MaxLength = ((int)(resources.GetObject("textBoxUrl.MaxLength")));
			this.textBoxUrl.Multiline = ((bool)(resources.GetObject("textBoxUrl.Multiline")));
			this.textBoxUrl.Name = "textBoxUrl";
			this.textBoxUrl.PasswordChar = ((char)(resources.GetObject("textBoxUrl.PasswordChar")));
			this.textBoxUrl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textBoxUrl.RightToLeft")));
			this.textBoxUrl.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textBoxUrl.ScrollBars")));
			this.textBoxUrl.Size = ((System.Drawing.Size)(resources.GetObject("textBoxUrl.Size")));
			this.textBoxUrl.TabIndex = ((int)(resources.GetObject("textBoxUrl.TabIndex")));
			this.textBoxUrl.Text = resources.GetString("textBoxUrl.Text");
			this.textBoxUrl.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textBoxUrl.TextAlign")));
			this.textBoxUrl.Visible = ((bool)(resources.GetObject("textBoxUrl.Visible")));
			this.textBoxUrl.WordWrap = ((bool)(resources.GetObject("textBoxUrl.WordWrap")));
			// 
			// label1
			// 
			this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
			this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
			this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
			this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
			this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
			this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
			this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
			this.label1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.ImageAlign")));
			this.label1.ImageIndex = ((int)(resources.GetObject("label1.ImageIndex")));
			this.label1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label1.ImeMode")));
			this.label1.Location = ((System.Drawing.Point)(resources.GetObject("label1.Location")));
			this.label1.Name = "label1";
			this.label1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label1.RightToLeft")));
			this.label1.Size = ((System.Drawing.Size)(resources.GetObject("label1.Size")));
			this.label1.TabIndex = ((int)(resources.GetObject("label1.TabIndex")));
			this.label1.Text = resources.GetString("label1.Text");
			this.label1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.TextAlign")));
			this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
			// 
			// tabPageFindByKeyword
			// 
			this.tabPageFindByKeyword.AccessibleDescription = resources.GetString("tabPageFindByKeyword.AccessibleDescription");
			this.tabPageFindByKeyword.AccessibleName = resources.GetString("tabPageFindByKeyword.AccessibleName");
			this.tabPageFindByKeyword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabPageFindByKeyword.Anchor")));
			this.tabPageFindByKeyword.AutoScroll = ((bool)(resources.GetObject("tabPageFindByKeyword.AutoScroll")));
			this.tabPageFindByKeyword.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabPageFindByKeyword.AutoScrollMargin")));
			this.tabPageFindByKeyword.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabPageFindByKeyword.AutoScrollMinSize")));
			this.tabPageFindByKeyword.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabPageFindByKeyword.BackgroundImage")));
			this.tabPageFindByKeyword.Controls.Add(this.textBoxKeywords);
			this.tabPageFindByKeyword.Controls.Add(this.label2);
			this.tabPageFindByKeyword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabPageFindByKeyword.Dock")));
			this.tabPageFindByKeyword.Enabled = ((bool)(resources.GetObject("tabPageFindByKeyword.Enabled")));
			this.tabPageFindByKeyword.Font = ((System.Drawing.Font)(resources.GetObject("tabPageFindByKeyword.Font")));
			this.tabPageFindByKeyword.ImageIndex = ((int)(resources.GetObject("tabPageFindByKeyword.ImageIndex")));
			this.tabPageFindByKeyword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabPageFindByKeyword.ImeMode")));
			this.tabPageFindByKeyword.Location = ((System.Drawing.Point)(resources.GetObject("tabPageFindByKeyword.Location")));
			this.tabPageFindByKeyword.Name = "tabPageFindByKeyword";
			this.tabPageFindByKeyword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabPageFindByKeyword.RightToLeft")));
			this.tabPageFindByKeyword.Size = ((System.Drawing.Size)(resources.GetObject("tabPageFindByKeyword.Size")));
			this.tabPageFindByKeyword.TabIndex = ((int)(resources.GetObject("tabPageFindByKeyword.TabIndex")));
			this.tabPageFindByKeyword.Text = resources.GetString("tabPageFindByKeyword.Text");
			this.tabPageFindByKeyword.ToolTipText = resources.GetString("tabPageFindByKeyword.ToolTipText");
			this.tabPageFindByKeyword.Visible = ((bool)(resources.GetObject("tabPageFindByKeyword.Visible")));
			// 
			// textBoxKeywords
			// 
			this.textBoxKeywords.AccessibleDescription = resources.GetString("textBoxKeywords.AccessibleDescription");
			this.textBoxKeywords.AccessibleName = resources.GetString("textBoxKeywords.AccessibleName");
			this.textBoxKeywords.AllowDrop = true;
			this.textBoxKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("textBoxKeywords.Anchor")));
			this.textBoxKeywords.AutoSize = ((bool)(resources.GetObject("textBoxKeywords.AutoSize")));
			this.textBoxKeywords.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("textBoxKeywords.BackgroundImage")));
			this.textBoxKeywords.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("textBoxKeywords.Dock")));
			this.textBoxKeywords.Enabled = ((bool)(resources.GetObject("textBoxKeywords.Enabled")));
			this.textBoxKeywords.Font = ((System.Drawing.Font)(resources.GetObject("textBoxKeywords.Font")));
			this.textBoxKeywords.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("textBoxKeywords.ImeMode")));
			this.textBoxKeywords.Location = ((System.Drawing.Point)(resources.GetObject("textBoxKeywords.Location")));
			this.textBoxKeywords.MaxLength = ((int)(resources.GetObject("textBoxKeywords.MaxLength")));
			this.textBoxKeywords.Multiline = ((bool)(resources.GetObject("textBoxKeywords.Multiline")));
			this.textBoxKeywords.Name = "textBoxKeywords";
			this.textBoxKeywords.PasswordChar = ((char)(resources.GetObject("textBoxKeywords.PasswordChar")));
			this.textBoxKeywords.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("textBoxKeywords.RightToLeft")));
			this.textBoxKeywords.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("textBoxKeywords.ScrollBars")));
			this.textBoxKeywords.Size = ((System.Drawing.Size)(resources.GetObject("textBoxKeywords.Size")));
			this.textBoxKeywords.TabIndex = ((int)(resources.GetObject("textBoxKeywords.TabIndex")));
			this.textBoxKeywords.Text = resources.GetString("textBoxKeywords.Text");
			this.textBoxKeywords.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("textBoxKeywords.TextAlign")));
			this.textBoxKeywords.Visible = ((bool)(resources.GetObject("textBoxKeywords.Visible")));
			this.textBoxKeywords.WordWrap = ((bool)(resources.GetObject("textBoxKeywords.WordWrap")));
			// 
			// label2
			// 
			this.label2.AccessibleDescription = resources.GetString("label2.AccessibleDescription");
			this.label2.AccessibleName = resources.GetString("label2.AccessibleName");
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label2.Anchor")));
			this.label2.AutoSize = ((bool)(resources.GetObject("label2.AutoSize")));
			this.label2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label2.Dock")));
			this.label2.Enabled = ((bool)(resources.GetObject("label2.Enabled")));
			this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.label2.Font = ((System.Drawing.Font)(resources.GetObject("label2.Font")));
			this.label2.Image = ((System.Drawing.Image)(resources.GetObject("label2.Image")));
			this.label2.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label2.ImageAlign")));
			this.label2.ImageIndex = ((int)(resources.GetObject("label2.ImageIndex")));
			this.label2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label2.ImeMode")));
			this.label2.Location = ((System.Drawing.Point)(resources.GetObject("label2.Location")));
			this.label2.Name = "label2";
			this.label2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label2.RightToLeft")));
			this.label2.Size = ((System.Drawing.Size)(resources.GetObject("label2.Size")));
			this.label2.TabIndex = ((int)(resources.GetObject("label2.TabIndex")));
			this.label2.Text = resources.GetString("label2.Text");
			this.label2.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label2.TextAlign")));
			this.label2.Visible = ((bool)(resources.GetObject("label2.Visible")));
			// 
			// AutoDiscoverFeedsDialog
			// 
			this.AcceptButton = this.btnFindFeed;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.btnCancel;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.tabFindByWhat);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnFindFeed);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "AutoDiscoverFeedsDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.OnFormLoad);
			this.tabFindByWhat.ResumeLayout(false);
			this.tabPageFindByUrl.ResumeLayout(false);
			this.tabPageFindByKeyword.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion	

		public string WebpageUrl { 
			get { return textBoxUrl.Text;  } 
			set { 
				textBoxUrl.Text = value; 
				this.IsKeywordSearch = (textBoxUrl.Text.Length == 0);
			} 
		}
		public string Keywords { 
			get { return textBoxKeywords.Text;  } 
			set { 
				textBoxKeywords.Text = value; 
				this.IsKeywordSearch = (textBoxKeywords.Text.Length > 0);
			} 
		}
		public bool IsKeywordSearch { 
			get { return tabFindByWhat.SelectedTab == tabPageFindByKeyword;  } 
			set { 
				if (value) {
					tabFindByWhat.SelectedTab = tabPageFindByKeyword;
				} else {
					tabFindByWhat.SelectedTab = tabPageFindByUrl;
				}
			} 
		}

		private void OnFormLoad(object sender, System.EventArgs e) {
			SendKeys.Send("+{TAB}");
		}

	}
}
