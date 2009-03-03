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
using System.Globalization;
using System.Windows.Forms;
using NewsComponents;
using NewsComponents.Feed;
using RssBandit.Resources;
using UserIdentity = RssBandit.Core.Storage.Serialization.UserIdentity;

namespace RssBandit.WinGui.Dialogs
{
	/// <summary>
	/// Summary description for PostReplyForm.
	/// </summary>
	internal partial class PostReplyForm : Form
	{
		public event PostReplyEventHandler PostReply;

		/// <summary>
		/// Someone may not like to have the translated/localized "Re: " prefixes
		/// </summary>
		private static readonly bool useEnglishReplyPrefix = RssBanditApplication.ReadAppSettingsEntry("UseEnglishReplyPrefix", false);

		private readonly string NoInfo;
		private INewsItem replyToItem;
		private INewsFeed postToFeed;
		private readonly IdentityNewsServerManager identityManager;

		private Label label4;
		private Button button1;
		internal TextBox richTextBox1;
		private Button btnCancel;
		private Button btnManageIdentities;
		internal ComboBox cboUserIdentityForComments;
		private TextBox txtTitle;
		private Label label2;
		private GroupBox groupBox1;
		private GroupBox grpReplyItems;
		private CheckBox chkBeautify;
		private Label label1;
		private TextBox txtSentInfos;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		
		public PostReplyForm(string defaultIdentity, IdentityNewsServerManager identityManager):
			this() 
		{
			this.NoInfo = SR.PostReplySentIdentityInfoTextEmptyValue;
			this.identityManager = identityManager;
			this.RefreshUserIdentities(defaultIdentity);
			this.OnIdentitySelectionChangeCommitted(this, EventArgs.Empty);
			this.richTextBox1.Text = this.UserSignature;

			this.txtTitle.TextChanged += this.OnTxtTitleTextChanged;
			this.richTextBox1.TextChanged += this.OnTxtRichTextTextChanged;
			this.Activated += PostReplyForm_Activated;
		}

		public PostReplyForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		static PostReplyForm() {
			useEnglishReplyPrefix = RssBanditApplication.ReadAppSettingsEntry("UseEnglishReplyPrefix", false);
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


		public void RefreshUserIdentities(string defaultIdentity) {

			this.cboUserIdentityForComments.Items.Clear();
			foreach (UserIdentity ui in this.identityManager.Identities.Values)
			{
				this.cboUserIdentityForComments.Items.Add(ui.Name);
			} 
			if (defaultIdentity != null && this.identityManager.Identities.ContainsKey(defaultIdentity))
			{
				this.cboUserIdentityForComments.Text = defaultIdentity;
			} else {
				if (this.cboUserIdentityForComments.Items.Count > 0)
					this.cboUserIdentityForComments.SelectedIndex = 0;
			}
		}

		/// <summary>
		/// Gets or sets the reply to NewsItem.
		/// </summary>
		/// <value>The reply to item.</value>
		public INewsItem ReplyToItem {
			get { return this.replyToItem; }
			set {
				this.replyToItem = value;
				this.postToFeed = null;
				if (this.replyToItem != null && !this.IsDisposed) {
					this.Text = String.Format(SR.PostReplyFormCaption,this.replyToItem.Feed.title);
					this.txtTitle.Text = GetPrefixedTitle(this.ReplyToItem.Title);
				}
			}
		}

		/// <summary>
		/// Gets or sets the NewsFeed to post to.
		/// </summary>
		/// <value>The NewsFeed to post to.</value>
		public INewsFeed PostToFeed {
			get { return this.postToFeed; }
			set {
				this.postToFeed = value;
				this.replyToItem = null;
				if (this.postToFeed != null && !this.IsDisposed) {
					this.Text = String.Format(SR.NewPostToFeedFormCaption,this.postToFeed.title);
					this.txtTitle.Text = String.Empty;
				}
			}
		}

		public string SelectedIdentityName {
			get { return (this.cboUserIdentityForComments.Text ?? String.Empty); }
		}

		public UserIdentity SelectedIdentity {
			get
			{
				if (this.identityManager.Identities.ContainsKey(SelectedIdentityName)) {
					return this.identityManager.Identities[SelectedIdentityName];
				}
				return IdentityNewsServerManager.AnonymousIdentity;
			}
		}

		public string UserSignature{
			get{ return this.SelectedIdentity.Signature; }
		}

		public string UserName { 
			get { return this.SelectedIdentity.RealName; }
		}
		public string UserMailAddress { 
			get { return this.SelectedIdentity.MailAddress; }
		}
		public string UserReferrerUrl { 
			get { return this.SelectedIdentity.ReferrerUrl; }
		}

		public string PostTitle { 
			get { return (string.IsNullOrEmpty(this.txtTitle.Text) ? String.Format(SR.PostReplyTitlePrefix,this.ReplyToItem.Title):  this.txtTitle.Text); }
		}

		/// <summary>
		/// Gets the prefixed title.
		/// </summary>
		/// <param name="feedItemTitle">The feed item title.</param>
		/// <returns></returns>
		private static string GetPrefixedTitle(string feedItemTitle) {
			if (feedItemTitle == null)
				feedItemTitle = String.Empty;

			CultureInfo cSave = RssBanditApplication.SharedUICulture;
			if (useEnglishReplyPrefix) 
				RssBanditApplication.SharedUICulture = new CultureInfo("en-US");
			
			string prefixed = feedItemTitle;

			if ( ! feedItemTitle.StartsWith(String.Format(SR.PostReplyTitlePrefix,String.Empty)))
				prefixed = String.Format(SR.PostReplyTitlePrefix,feedItemTitle);
			
			if (useEnglishReplyPrefix) 
				RssBanditApplication.SharedUICulture = cSave;

			return prefixed;
		}

		private void DoPostReplyClick(object sender, EventArgs e) {
			this.Hide();
			try {
				OnPostReply();
			} catch {}
		}

		protected void OnPostReply() {
			if (PostReply != null) {
				if (this.PostToFeed == null) {
					PostReply(this, new PostReplyEventArgs(this.ReplyToItem, this.PostTitle, 
					                                       this.UserName, this.UserReferrerUrl, this.UserMailAddress, 
					                                       this.richTextBox1.Text, this.chkBeautify.Checked ));
				} else {
					PostReply(this, new PostReplyEventArgs(this.PostToFeed, this.PostTitle, 
					                                       this.UserName, this.UserReferrerUrl, this.UserMailAddress, 
					                                       this.richTextBox1.Text, this.chkBeautify.Checked ));	
				}
			}
		}

		private void ValidateInput() {
			this.button1.Enabled = (txtTitle.Text.Length > 0 && richTextBox1.Text.Length > 0);
		}

		private void btnCancel_Click(object sender, EventArgs e) {
			this.richTextBox1.Text = String.Empty;
			this.Hide();
		}

		private void OnIdentitySelectionChangeCommitted(object sender, EventArgs e) {
			// fill sent informations textbox
			this.txtSentInfos.Text = String.Format(SR.PostReplySentIdentityInfoText,
				string.IsNullOrEmpty(this.UserName) ? NoInfo : this.UserName , 
				string.IsNullOrEmpty(this.UserMailAddress) ? NoInfo : this.UserMailAddress, 
				string.IsNullOrEmpty(this.UserReferrerUrl) ? NoInfo : this.UserReferrerUrl); 		
		}

		private void btnManageIdentities_Click(object sender, EventArgs e) {
			this.identityManager.ShowIdentityDialog(this);
			this.RefreshUserIdentities(this.SelectedIdentityName);
			this.OnIdentitySelectionChangeCommitted(this, EventArgs.Empty);
		}

		private void OnFormLoad(object sender, EventArgs e) {
			this.richTextBox1.Select(0,0);
		}

		private void OnTxtTitleTextChanged(object sender, EventArgs e) {
			ValidateInput();
		}
		private void OnTxtRichTextTextChanged(object sender, EventArgs e) {
			ValidateInput();
		}

		private void PostReplyForm_Activated(object sender, EventArgs e) {
			ValidateInput();
			if (txtTitle.Text.Length == 0)
				txtTitle.Focus();
			else
				this.richTextBox1.Focus();
		}
	}

	/// <summary>
	/// Callback to initiate the post action
	/// </summary>
	public delegate void PostReplyEventHandler(object sender, PostReplyEventArgs e);

	/// <summary>
	/// Event arguments container for post reply action
	/// </summary>
	public class PostReplyEventArgs:EventArgs {
		public string FromName; 
		public string FromUrl; 
		public string FromEMail; 
		public string Comment; 	
		public bool Beautify;
		public string Title; 	
		public INewsItem ReplyToItem;
		public INewsFeed PostToFeed;

		/// <summary>
		/// Designated initializer
		/// </summary>
		/// <param name="replyToItem"></param>
		/// <param name="title"></param>
		/// <param name="name"></param>
		/// <param name="url"></param>
		/// <param name="email"></param>
		/// <param name="comment"></param>
		/// <param name="beautify"></param>
		public PostReplyEventArgs(INewsItem replyToItem, string title, string name, string url, string email, string comment, bool beautify) {
			this.ReplyToItem = replyToItem;
			this.FromName = name;
			this.FromUrl = url;
			this.FromEMail = email;
			this.Comment = comment;
			this.Title = title;
			this.Beautify = beautify;
		}
		public PostReplyEventArgs(INewsFeed postToFeed, string title, string name, string url, string email, string comment, bool beautify) {
			this.PostToFeed = postToFeed;
			this.FromName = name;
			this.FromUrl = url;
			this.FromEMail = email;
			this.Comment = comment;
			this.Title = title;
			this.Beautify = beautify;
		}
	}
}