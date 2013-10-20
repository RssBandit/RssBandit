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
using System.Collections.Generic;
using System.Linq;
using Infragistics.Win.Misc;
using JetBrains.Annotations;
using NewsComponents;
using NewsComponents.Net;
using NewsComponents.Feed;
using NewsComponents.Utils;
using RssBandit.AppServices;
using RssBandit.Resources;
using RssBandit.WinGui.Controls;
using Logger = RssBandit.Common.Logging;

namespace RssBandit.WinGui
{
	public enum NotifierAction
	{
		ActivateItem,
		ActivateFeed,
		ShowFeedProperties,
	}

	public class NotifierActionEventArgs : EventArgs
	{
		public NotifierAction Action;
		public INewsItem NewsItem;
		public INewsFeed NewsFeed;
		public DownloadItem DownloadItem;

		public NotifierActionEventArgs(NotifierAction action)
		{
			this.Action = action;
		}
	}

	/// <summary>
	/// Manages Toast Notification Windows.
	/// </summary>
	public class ToastNotifier
	{
		public event EventHandler<NotifierActionEventArgs> NotificationAction;

		#region private variables

		private const int maxItemTextWith = 45;
		private const string NewsItemAlertWindowKey = "#NIAWK";
		private const string DownloadItemAlertWindowKey = "#DIAWK";

		private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(ToastNotifier));

	    private readonly UltraDesktopAlert _alertWindow;
		private readonly IUserPreferences _preferences;
		
		#endregion

		#region ctor()'s
		
        public ToastNotifier(
			[NotNull]IUserPreferences preferences,
			[NotNull]UltraDesktopAlert desktopAlert) 
        {
			if (preferences == null)
				throw new ArgumentNullException("preferences");
			if (desktopAlert == null)
				throw new ArgumentNullException("desktopAlert");

			_preferences = preferences;
			_alertWindow = desktopAlert;
			_alertWindow.Style = DesktopAlertStyle.Office2007;
			_alertWindow.DesktopAlertLinkClicked += OnDesktopAlertLinkClicked;
		}

		#endregion

		#region public members
		
        /// <summary>
		/// Called to show the small toast alert window on new items received.
		/// </summary>
		/// <param name="node">Feed node where items received</param>
		/// <param name="dispItemCount">unread items count to display</param>
		/// <param name="items">list of the newest NewsItem's received. We assume,
		/// they are sorted with the newest items first!</param>
		/// <remarks>
		/// The parameter <c>dispItemCount</c> controls, if and how many item links
		/// are displayed in the window. This means; if 0 (zero) or lower than zero, nothing
		/// happens (no window). If one or more is specified, it displayes up to three items
		/// in the window. This way you can control, if there was allready e.g. 3 new items on the
		/// feed, and just only one new was received, that the window display only a link
		/// to that one newest item by specify 1 (one) as the parameter.
		/// </remarks>
        public void Alert(TreeFeedsNodeBase node, int dispItemCount, IList<INewsItem> items) 
		{
			if (node == null || dispItemCount < 0 || items == null || items.Count == 0)
				return;

			int unreadCount = items.Aggregate(0, (i, item) =>
			{
				if (item.BeenRead)
					return i;
				return i+1;
			});

	        var firstItem = items[0];

			if (_alertWindow != null && unreadCount > dispItemCount && !_alertWindow.IsOpen(NewsItemAlertWindowKey))
			{
				UltraDesktopAlertShowWindowInfo windowInfo = new UltraDesktopAlertShowWindowInfo();
				windowInfo.Key = NewsItemAlertWindowKey;
				windowInfo.PinButtonVisible = true;
				windowInfo.Image = node.ImageResolved;
				windowInfo.Data = firstItem;	
				
				windowInfo.Caption = "<font face=\"Tahoma\">{0} ({1})</font>"
					.FormatWith(node.Text, unreadCount - dispItemCount);
				windowInfo.Text = String.Format("<hr NoShade=\"true\" size=\"1px\"/><font face=\"Tahoma\"><span style=\"font-style:italic_x003B_\">{0}</span></font>", 
					StringHelper.ShortenByEllipsis(firstItem.Title, maxItemTextWith));
				windowInfo.FooterText = "<font face=\"Tahoma\">{0}</font>"
					.FormatWith(SR.MenuShowFeedPropertiesCaption);

				if (_preferences.AllowAppEventSounds)
					windowInfo.Sound = Resource.ApplicationSound.GetSoundStream(Resource.ApplicationSound.NewItemsReceived);
				
				_alertWindow.Show(windowInfo);
			}
        }

        /// <summary>
        /// Called to show the small toast alert window on new items received.
        /// </summary>
        /// <param name="feed">Feed to be displayed</param>
        /// <param name="dispItemCount">unread items count to display</param>
        /// <param name="items">list of the newest DownloadItem's received. We assume,
        /// they are sorted with the newest items first!</param>
        /// <remarks>
        /// The parameter <c>dispItemCount</c> controls, if and how many item links
        /// are displayed in the window. This means; if 0 (zero) or lower than zero, nothing
        /// happens (no window). If one or more is specified, it displayes up to three items
        /// in the window. This way you can control, if there was allready e.g. 3 new items on the
        /// feed, and just only one new was received, that the window display only a link
        /// to that one newest item by specify 1 (one) as the parameter.
        /// </remarks>
        public void Alert(INewsFeed feed, int dispItemCount, IList<DownloadItem> items) 
		{
			if (feed == null || dispItemCount < 0 || items == null || items.Count == 0)
				return;

			int unreadCount = items.Count;

			var firstItem = items[0];

			if (_alertWindow != null && unreadCount > dispItemCount && !_alertWindow.IsOpen(DownloadItemAlertWindowKey))
			{
				UltraDesktopAlertShowWindowInfo windowInfo = new UltraDesktopAlertShowWindowInfo();
				windowInfo.Key = DownloadItemAlertWindowKey;
				windowInfo.Image = Properties.Resources.download_enclosure_32;
				windowInfo.Data = firstItem;
				windowInfo.PinButtonVisible = true;

				windowInfo.Caption = "<font face=\"Tahoma\" size=\"+2\"><b>{0}</b></font><br/>&nbsp;"
					.FormatWith(feed.title);
				windowInfo.Text = String.Format("<font face=\"Tahoma\">{0}<br/>{1}<br/>{2}</font>",
					SR.GUIStatusEnclosureJustReceivedItemsMessage,
					StringHelper.ShortenByEllipsis(firstItem.File.LocalName, maxItemTextWith),  
					String.IsNullOrEmpty(firstItem.Enclosure.Description) ? "" : firstItem.Enclosure.Description);
				windowInfo.FooterText = "<font face=\"Tahoma\" size=\"-1\">{0}</font>"
					.FormatWith(SR.MenuShowFeedPropertiesCaption);

				if (_preferences.AllowAppEventSounds)
					windowInfo.Sound = Resource.ApplicationSound.GetSoundStream(Resource.ApplicationSound.NewAttachmentDownloaded);
				
				_alertWindow.Show(windowInfo);
			}
        }

		
		#endregion
		
		#region private members

		private void OnDesktopAlertLinkClicked(object sender, DesktopAlertLinkClickedEventArgs e)
		{
			e.LinkClickedArgs.OpenLink = false;
			object alertItem = null;

			if (e.WindowInfo != null)
				alertItem = e.WindowInfo.Data;

			var newsItem = alertItem as INewsItem;
			var dwldItem = alertItem as DownloadItem;

			switch (e.LinkType)
			{
				case DesktopAlertLinkType.Footer:
					// navigate to feed options dialog
					if (newsItem != null || dwldItem != null)
					{
						try
						{
							RaiseNotificationAction(new NotifierActionEventArgs(NotifierAction.ShowFeedProperties) 
								{ NewsFeed = newsItem != null ? newsItem.Feed : dwldItem.OwnerFeed });
						}
						catch { }
					}
					break;

				case DesktopAlertLinkType.Caption:
					// navigate to feed
					if (newsItem != null || dwldItem != null)
					{
						try
						{
							RaiseNotificationAction(new NotifierActionEventArgs(NotifierAction.ActivateFeed) 
								{ NewsFeed = newsItem != null ? newsItem.Feed : dwldItem.OwnerFeed });
						}
						catch { }
					}
					break;

				case DesktopAlertLinkType.Text:
					// navigate to feed item
					if (newsItem != null)
					{
						try
						{
							RaiseNotificationAction(new NotifierActionEventArgs(NotifierAction.ActivateItem) 
								{ NewsItem = newsItem });
						}
						catch { }
					}
					else if (dwldItem != null)
					{
						try
						{
							RaiseNotificationAction(new NotifierActionEventArgs(NotifierAction.ActivateItem) 
								{ DownloadItem = dwldItem });
						}
						catch { }
					}
					break;
			}

		}

		private void RaiseNotificationAction(NotifierActionEventArgs e)
		{
			var handler = NotificationAction;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		#endregion
	}
}
