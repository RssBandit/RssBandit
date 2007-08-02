using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

using NewsComponents;
using NewsComponents.Feed;
using RssBandit.WinGui.Forms;

namespace RssBandit.WinGui
{
	#region public delegates
	/// <summary>
	/// Delegate called on a NewsItem link click.
	/// </summary>
	public delegate void ItemActivateCallback(NewsItem item);
	/// <summary>
	/// Delegate called on a Feed Properties link click.
	/// </summary>
	public delegate void DisplayFeedPropertiesCallback(feedsFeed f);
	/// <summary>
	/// Delegate called on a Feed link click.
	/// </summary>
	public delegate void FeedActivateCallback(feedsFeed f);
	#endregion

	/// <summary>
	/// Manages Toast Notification Windows.
	/// </summary>
	public class ToastNotifier: IDisposable
	{
		#region private variables
		private const int TOASTWINDOW_HEIGHT = 145;
		private const int TOASTWINDOW_OFFSET = 2;

		private ItemActivateCallback				_itemActivateCallback;
		private DisplayFeedPropertiesCallback	_displayFeedPropertiesCallback;
		private FeedActivateCallback				_feedActivateCallback;

		private ArrayList	_toastWindows;
		private bool			_disposing = false;
		private int			_openWindowsCount = 0;
		#endregion

		#region ctor()'s
		public ToastNotifier():this(null, null, null) {}
		public ToastNotifier(ItemActivateCallback onItemActivateCallback, 
			DisplayFeedPropertiesCallback onFeedPropertiesDialog,
			FeedActivateCallback onFeedActivateCallback) {
			this._toastWindows = new ArrayList(5);
			this._itemActivateCallback = onItemActivateCallback;
			this._displayFeedPropertiesCallback = onFeedPropertiesDialog;
			this._feedActivateCallback = onFeedActivateCallback;
		}
		#endregion

		#region public members
		
		/// <summary>
		/// Called to show the small toast alert window on new items received.
		/// </summary>
		/// <param name="feedName">Feedname to be displayed</param>
		/// <param name="dispItemCount">unread items count to display</param>
		/// <param name="items">ArrayList of the new NewsItem's received. We assume,
		/// they are sorted with the newest items first!</param>
		/// <remarks>
		/// The parameter <c>dispItemCount</c> controls, if and how many item links
		/// are displayed in the window. This means; if 0 (zero) or lower than zero, nothing
		/// happens (no window). If one or more is specified, it displayes up to three items
		/// in the window. This way you can control, if there was allready e.g. 3 new items on the
		/// feed, and just only one new was received, that the window display only a link
		/// to that one newest item by specify 1 (one) as the parameter.
		/// </remarks>
		public void  Alert(string feedName, int dispItemCount, ArrayList items)
		{
			if (dispItemCount < 0 || items == null || items.Count == 0)
				return;

			if (_disposing)
				return;

			ArrayList myItems = new ArrayList(items);
			myItems.Sort(NewsComponents.Utils.RssHelper.GetComparer(true,NewsComponents.Utils.NewsItemSortField.Date));

			lock (_toastWindows) {
				ToastNotify theWindow = this.GetToastWindow();
				if (theWindow != null) {
					try {
						if (theWindow.ItemsToDisplay(feedName, dispItemCount, myItems)) {
							if (!theWindow.Disposing) {
								_openWindowsCount++;
								theWindow.Animate();
							}
						}
					} catch {/* catch all */ }
				}
			}
		}
		
		
		#endregion

		#region private members
		
		private ToastNotify GetToastWindow() {
			
			ToastNotify tn = null;
			int windowIndex = 0;
			
			foreach(ToastNotify tnCurrent in _toastWindows) {                    
				if (!tnCurrent.Disposing && !tnCurrent.Animating) {
					tn = tnCurrent;
					break;
				}
				windowIndex++;
			}

			// because we can display stacked toaster's, we have to re-calc the new position
			// do it every time, maybe the display resolution was changed as we run the app
			Rectangle rPrimeScreen = Screen.PrimaryScreen.WorkingArea;
			int newX = (rPrimeScreen.Height - TOASTWINDOW_OFFSET) - (windowIndex * TOASTWINDOW_HEIGHT);
			
			if (newX < (rPrimeScreen.Top + TOASTWINDOW_HEIGHT)) {
				tn = null;	// do not display toast flooding screen area (all open/used)
			} else {
				if (tn == null) {
					ToastNotify tnNew = new ToastNotify(_itemActivateCallback, _displayFeedPropertiesCallback, _feedActivateCallback);
					tnNew.AnimatingDone += new EventHandler(this.OnToastAnimatingDone);
					_toastWindows.Add(tnNew);
					tn = tnNew;
				}
				tn.StartLocation = new Point(rPrimeScreen.Width - tn.Width - TOASTWINDOW_OFFSET, newX);
			}
			return tn;
		}

		private void CleanupForms() {
			lock (_toastWindows) {
				if (_toastWindows.Count > 0) {
					foreach(ToastNotify f in _toastWindows) {                    
						f.Close();                                                            
						f.Dispose();
					}
				}
				_toastWindows.Clear();
			}
		}

		private void OnToastAnimatingDone(object sender, EventArgs e) {
			if (_openWindowsCount > 0) {
				if (--_openWindowsCount == 0 && _disposing) {
					this.CleanupForms();
				}
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose() {
			_disposing = true;
			lock (_toastWindows) {
				foreach(ToastNotify f in _toastWindows) {                    
					f.RequestClose();
				}
			}
		}

		#endregion

	}
}
