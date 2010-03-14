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
using NewsComponents;

namespace RssBandit.WinGui.Dialogs
{
	public partial class FeedSourceProperties : DialogBase
	{
		#region ctor's

		public FeedSourceProperties()
		{
			InitializeComponent();
			ApplyComponentTranslations();
		}

		public FeedSourceProperties(FeedSourceEntry entry) : this()
		{
			txtFeedSourceName.Text = entry.Name;
			grpCredentials.Enabled = entry.Source.SubscriptionLocation.CredentialsSupported 
                                       && entry.SourceType != FeedSourceType.Facebook;
			if (grpCredentials.Enabled)
			{
				if (String.IsNullOrEmpty(entry.Source.SubscriptionLocation.Credentials.Domain))
					txtUsername.Text = entry.Source.SubscriptionLocation.Credentials.UserName;
				else
					txtUsername.Text = entry.Source.SubscriptionLocation.Credentials.Domain + @"\" + 
					                   entry.Source.SubscriptionLocation.Credentials.UserName;
				txtPassword.Text = entry.Source.SubscriptionLocation.Credentials.Password;
			}
		}

		#endregion

		protected override void InitializeComponentTranslation()
		{
			base.InitializeComponentTranslation();

			Text = DR.FeedSourceProperties_Title;
			lblFeedSourceName.Text = DR.FeedSourceProperties_labelFeedSourceName_Text;
			grpCredentials.Text = DR.FeedSourceProperties_groupCredentials_Text;
			lblUsername.Text = DR.FeedSourceProperties_labelUsername_Text;
			lblPassword.Text = DR.FeedSourceProperties_labelPassword_Text;
		}

		public string FeedSoureName
		{
			get { return txtFeedSourceName.Text; }
		}
		public string Username
		{
			get { return txtUsername.Text; }
		}
		public string Password
		{
			get { return txtPassword.Text; }
		}
	}
}
