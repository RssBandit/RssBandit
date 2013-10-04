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
using Infragistics.Win;
using Infragistics.Win.Misc;
using NewsComponents;
using NewsComponents.Net;
using NewsComponents.Feed;
using NewsComponents.Utils;
using RssBandit.Resources;
using RssBandit.WinGui.Controls;
using Logger = RssBandit.Common.Logging;

namespace RssBandit.WinGui
{
	#region public delegates
	/// <summary>
	/// Delegate called on a NewsItem link click.
	/// </summary>
	public delegate void ItemActivateCallback(INewsItem item);
	/// <summary>
	/// Delegate called on a Feed Properties link click.
	/// </summary>
	public delegate void DisplayFeedPropertiesCallback(INewsFeed f);
	/// <summary>
	/// Delegate called on a Feed link click.
	/// </summary>
	public delegate void FeedActivateCallback(INewsFeed f);
	/// <summary>
	/// dDelegate called on downloaded enclosure link click
	/// </summary>
	public delegate void EnclosureActivateCallback(DownloadItem enclosure);
	#endregion

	/// <summary>
	/// Manages Toast Notification Windows.
	/// </summary>
	public class ToastNotifier
	{
		#region private variables

		private const int maxItemTextWith = 45;
		
		private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(ToastNotifier));

	    private readonly UltraDesktopAlert _alertWindow;
		private readonly ItemActivateCallback _itemActivateCallback;
        private readonly DisplayFeedPropertiesCallback _displayFeedPropertiesCallback;
        private readonly FeedActivateCallback _feedActivateCallback;
        private readonly EnclosureActivateCallback _enclosureActivateCallback;

		private readonly Random _keyGen = new Random();
		
		#endregion

		#region ctor()'s
		
        public ToastNotifier(
			UltraDesktopAlert desktopAlert,
            ItemActivateCallback onItemActivateCallback, 
			DisplayFeedPropertiesCallback onFeedPropertiesDialog,
			FeedActivateCallback onFeedActivateCallback, 
			EnclosureActivateCallback onEnclosureActivateCallback) 
        {
			if (desktopAlert == null)
				throw new ArgumentNullException("desktopAlert");
		    
			_alertWindow = desktopAlert;
			_alertWindow.Style = DesktopAlertStyle.Office2007;
			_alertWindow.DesktopAlertLinkClicked += OnDesktopAlertLinkClicked;
			
			this._itemActivateCallback = onItemActivateCallback;
			this._displayFeedPropertiesCallback = onFeedPropertiesDialog;
			this._feedActivateCallback = onFeedActivateCallback;
			this._enclosureActivateCallback = onEnclosureActivateCallback;
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

			if (_alertWindow != null && unreadCount > dispItemCount)
			{
				UltraDesktopAlertShowWindowInfo windowInfo = new UltraDesktopAlertShowWindowInfo();
				windowInfo.Image = node.ImageResolved;
				windowInfo.Data = firstItem;	
				
				_alertWindow.TreatCaptionAsLink = DefaultableBoolean.False;
				_alertWindow.TreatTextAsLink = DefaultableBoolean.False;
				_alertWindow.TreatFooterTextAsLink = DefaultableBoolean.False;
				
				var link = windowInfo.Key = "#F{0}".FormatWith(_keyGen.Next());
				
				windowInfo.Caption = "<a href=\"{2}\"><font face=\"Tahoma\" size=\"+1\"><b>{0} ({1})</b></font></a>"
					.FormatWith(node.Text, unreadCount, link);
				windowInfo.Text = String.Format("<a href=\"{2}\"><font face=\"Tahoma\">{0}<br/><span style=\"font-style:italic_x003B_\">{1}</span></font></a>", 
					SR.GUIStatusFeedJustReceivedItemsMessage.FormatWith( unreadCount - dispItemCount),
					StringHelper.ShortenByEllipsis(firstItem.Title, maxItemTextWith), link);
				windowInfo.FooterText = "<font face=\"Tahoma\" size=\"-1\"><a href=\"{2}\" title=\"{1}\">{0}</a></font>"
					.FormatWith(SR.MenuShowFeedPropertiesCaption, SR.MenuShowFeedPropertiesDesc, link);
				
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

			if (_alertWindow != null && unreadCount > dispItemCount)
			{
				UltraDesktopAlertShowWindowInfo windowInfo = new UltraDesktopAlertShowWindowInfo();
				windowInfo.Image = Properties.Resources.download_enclosure_32;
				windowInfo.Data = firstItem;	

				_alertWindow.TreatCaptionAsLink = DefaultableBoolean.False;
				_alertWindow.TreatTextAsLink = DefaultableBoolean.False;
				_alertWindow.TreatFooterTextAsLink = DefaultableBoolean.False;
				
				var link = windowInfo.Key = "#D{0}".FormatWith(_keyGen.Next());

				windowInfo.Caption = "<a href=\"{1}\"><font face=\"Tahoma\" size=\"+1\"><b>{0}</b></font></a><br/>&nbsp;"
					.FormatWith(feed.title, link);
				windowInfo.Text = String.Format("<a href=\"{2}\" title=\"{3}\"><font face=\"Tahoma\">{0}<br/>{1}</font></a>",
					SR.GUIStatusEnclosureJustReceivedItemsMessage,
					StringHelper.ShortenByEllipsis(firstItem.File.LocalName, maxItemTextWith), link, 
					String.IsNullOrEmpty(firstItem.Enclosure.Description) ? firstItem.File.LocalName : firstItem.Enclosure.Description);
				windowInfo.FooterText = "<font face=\"Tahoma\" size=\"-1\"><a href=\"{2}\" title=\"{1}\">{0}</a></font>"
					.FormatWith(SR.MenuShowFeedPropertiesCaption, SR.MenuShowFeedPropertiesDesc, link);

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

			//var key = e.LinkClickedArgs.LinkRef;
			//object alertItem = _alertWindow.GetWindowInfo(key).Data;

			var newsItem = alertItem as INewsItem;
			var dwldItem = alertItem as DownloadItem;

			switch (e.LinkType)
			{
				case DesktopAlertLinkType.Footer:
					// navigate to feed options dialog
					if (_displayFeedPropertiesCallback != null && (newsItem != null || dwldItem != null))
					{
						try { _displayFeedPropertiesCallback(newsItem != null ? newsItem.Feed : dwldItem.OwnerFeed); }
						catch { }
					}
					break;

				case DesktopAlertLinkType.Caption:
					// navigate to feed
					if (_feedActivateCallback != null && (newsItem != null || dwldItem != null))
					{
						try { _feedActivateCallback(newsItem != null ? newsItem.Feed : dwldItem.OwnerFeed); }
						catch { }
					}
					break;

				case DesktopAlertLinkType.Text:
					// navigate to feed item
					if (_itemActivateCallback != null && newsItem != null)
					{
						try { _itemActivateCallback(newsItem); }
						catch { }
					}
					else if (_enclosureActivateCallback != null && dwldItem != null)
					{
						try { _enclosureActivateCallback(dwldItem); }
						catch { }
					}
					break;
			}

		}
		
		#endregion
	}
}
