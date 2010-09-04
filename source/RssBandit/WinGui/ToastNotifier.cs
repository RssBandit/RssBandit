using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Infragistics.Win.Misc;
using NewsComponents;
using NewsComponents.Net;
using NewsComponents.Feed;
using RssBandit.WinGui.Forms;
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
	public class ToastNotifier: IDisposable
	{
		#region private variables
		private const int TOASTWINDOW_HEIGHT = 145;
		private const int TOASTWINDOW_OFFSET = 2;
		private static readonly log4net.ILog _log = Logger.DefaultLog.GetLogger(typeof(RssBanditApplication));

	    private UltraDesktopAlert alertWindow;
		private readonly ItemActivateCallback _itemActivateCallback;
        private readonly DisplayFeedPropertiesCallback _displayFeedPropertiesCallback;
        private readonly FeedActivateCallback _feedActivateCallback;
        private readonly EnclosureActivateCallback _enclosureActivateCallback;

		private int _usedToastWindowLocations;
		private object SyncRoot = new object();
		private bool _disposing;
		#endregion

		#region ctor()'s
		
        public ToastNotifier(UltraDesktopAlert alert,
            ItemActivateCallback onItemActivateCallback, 
			DisplayFeedPropertiesCallback onFeedPropertiesDialog,
			FeedActivateCallback onFeedActivateCallback, 
			EnclosureActivateCallback onEnclosureActivateCallback) 
        {
		    alertWindow = alert;
			this._usedToastWindowLocations = 0;
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
		/// <param name="feedName">Feedname to be displayed</param>
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
        public void Alert(string feedName, int dispItemCount, IList<INewsItem> items) {
            this.Alert(feedName, dispItemCount, (IList)items); 
        }

        /// <summary>
        /// Called to show the small toast alert window on new items received.
        /// </summary>
        /// <param name="feedName">Feedname to be displayed</param>
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
        public void Alert(string feedName, int dispItemCount, IList<DownloadItem> items) {
            this.Alert(feedName, dispItemCount, (IList)items); 
        }

		/// <summary>
		/// Called to show the small toast alert window on new items received.
		/// </summary>
		/// <param name="feedName">Feedname to be displayed</param>
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
        private void Alert(string feedName, int dispItemCount, IList items)
		{
			if (dispItemCount < 0 || items == null || items.Count == 0)
				return;

			if (_disposing)
				return;

            // should be used in Phoenix, needs more UI cleanup here
            if (alertWindow != null)
            {
                UltraDesktopAlertShowWindowInfo windowInfo = new UltraDesktopAlertShowWindowInfo();
                windowInfo.Caption = feedName;
                windowInfo.Text = "Display " + dispItemCount + " items of " + items.Count;
                windowInfo.Data = items;
                alertWindow.Show(windowInfo);
                return; // if (toastObject is INewsItem) NewsItemToastNotify...
            }
          
			//lock (_toastWindows) {
				ToastNotify theWindow = this.GetToastWindow(items[0]);
				if (theWindow != null) {
					try {
						if (theWindow.ItemsToDisplay(feedName, dispItemCount, items) &&
							!theWindow.Disposing)
						{
							if (this._usedToastWindowLocations == 1)
							{
								// play sound only for first toast window
								if (theWindow is NewsItemToastNotify)
									Win32.PlaySound(Resource.ApplicationSound.NewItemsReceived);
								else if (theWindow is EnclosureToastNotify)
									Win32.PlaySound(Resource.ApplicationSound.NewAttachmentDownloaded);
							}
			
							//_openWindowsCount++;
							theWindow.Animate();
							// will be auto-disposed...
						} 
						else 
						{	// detach event and mark window location as free:
							this.OnToastAnimatingDone(theWindow, EventArgs.Empty);
							// not displayed, so dispose manually:
							theWindow.Close();
							theWindow.Dispose();
						}
					} catch(Exception e) {
						_log.Fatal("ToastNotify.Alert() caused an error", e); 
					}
				}
			//}
		}
		
		
		#endregion

		#region private members
		
		private ToastNotify GetToastWindow(object toastObject) {
			int windowIndex = GetFreeToastWindowOffset();
			if (windowIndex < 0)
				return null;


			// because we can display stacked toaster's, we have to re-calc the new position
			// do it every time, maybe the display resolution was changed as we run the app
			Rectangle rPrimeScreen = Screen.PrimaryScreen.WorkingArea;
			int newX = (rPrimeScreen.Height - TOASTWINDOW_OFFSET) - (windowIndex * TOASTWINDOW_HEIGHT);
			
			if (newX < (rPrimeScreen.Top + TOASTWINDOW_HEIGHT)) {
				return null;	// do not display toast flooding screen area (all open/used)
			}

			ToastNotify tnNew;
			if (toastObject is INewsItem)
			{
				tnNew = new NewsItemToastNotify(_itemActivateCallback, _displayFeedPropertiesCallback, _feedActivateCallback);
			}
			else
			{
				tnNew = new EnclosureToastNotify(_enclosureActivateCallback, _displayFeedPropertiesCallback, _feedActivateCallback);
			}
			tnNew.AutoDispose = true;
			tnNew.Tag = windowIndex;
			tnNew.AnimatingDone += this.OnToastAnimatingDone;
			
			return tnNew;
		}


		private void OnToastAnimatingDone(object sender, EventArgs e) {
			ToastNotify n = sender as ToastNotify;
			// detach event:
			if (n != null) {
				n.AnimatingDone -= this.OnToastAnimatingDone;
				MarkToastWindowOffsetFree((int)n.Tag);
			}
		}

		/// <summary>
		/// Gets the free toast window offset and mark it as in-use.
		/// </summary>
		/// <returns></returns>
		private int GetFreeToastWindowOffset() {
			int max = (Screen.PrimaryScreen.WorkingArea.Height - TOASTWINDOW_OFFSET) / TOASTWINDOW_HEIGHT;
			lock (this.SyncRoot) {
				for (int i = 0; i < max; i++)
					if (0 == (this._usedToastWindowLocations & (1 << i))) {
						this._usedToastWindowLocations |= (1 << i);
						return i;
					}
			}
			// nothing free:
			return -1;
		}

		/// <summary>
		/// Marks the toast window offset free (to be re-used by a new window).
		/// </summary>
		/// <param name="index">The index.</param>
		private void MarkToastWindowOffsetFree(int index)
		{
			lock (this.SyncRoot)
			{
				this._usedToastWindowLocations &= ~(1 << index);
			}
		}
		#endregion

		#region IDisposable Members

		public void Dispose() {
			_disposing = true;
			//lock (_toastWindows) {
			//    foreach(ToastNotify f in _toastWindows) {                    
			//        f.RequestClose();
			//    }
			//}
		}

		#endregion

	}
}
