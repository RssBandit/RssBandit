#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.ComponentModel;
using System.Windows.Forms;
using RssBandit.Resources;
using RssBandit.WinGui.Controls;

namespace RssBandit.WinGui
{
	/// <summary>
	/// Application Tray States.
	/// </summary>
	public enum ApplicationTrayState
	{
		NormalIdle,
		BusyRefreshFeeds,
		NewUnreadFeedsReceived,
		NewUnreadFeeds
	}

	public enum BalloonIcon
	{
		None = 0x0,			// NIIF_NONE
		Error = 0x3,		// NIIF_ERROR
		Info = 0x1,			// NIIF_INFO
		Warning = 0x2		// NIIF_WARNING
	}

	/// <summary>
	/// TrayStateManager class manages the application
	/// states to be visualized within the system tray icon.
	/// </summary>
	public sealed class TrayStateManager: IDisposable
	{
		public event EventHandler<EventArgs> TrayIconClicked;
		public event EventHandler<EventArgs> TrayBalloonClickClicked;
		public event EventHandler<EventArgs> TrayBalloonTimeout;

		private ApplicationTrayState _currentState;
		private NotifyIconAnimation _notifyIcon;

		#region ctor's

		private TrayStateManager()
		{
			_currentState = ApplicationTrayState.NormalIdle;
		}

		public TrayStateManager(IContainer components) :
			this()
		{
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }
            
			_notifyIcon = new NotifyIconAnimation(components);

			_notifyIcon.Click += OnTrayIconClick;
			_notifyIcon.BalloonClick += OnTrayBalloonClick;
			_notifyIcon.BalloonTimeout += OnTrayBalloonTimeoutClose;

			_notifyIcon.AddState(
				new NotifyIconState(ApplicationTrayState.NormalIdle.ToString(),
					SR.GUIStatusIdle, Properties.Resources.AppTray)
				);

			_notifyIcon.AddState(
				new NotifyIconState(ApplicationTrayState.BusyRefreshFeeds.ToString(),
					SR.GUIStatusBusyRefresh, Properties.Resources.AppBusy)
				);

			_notifyIcon.AddState(
				new NotifyIconState(ApplicationTrayState.NewUnreadFeeds.ToString(),
					SR.GUIStatusUnreadFeedItemsAvailable, Properties.Resources.UnreadFeedItems)
				);

			_notifyIcon.Visible = true;
		}

		#endregion

		/// <summary>
		/// Use it to set/switch the 
		/// </summary>
		public ApplicationTrayState CurrentState
		{
			get { return _currentState; }
		}

		public void SetState(ApplicationTrayState state)
		{
			_currentState = state;
			_notifyIcon.SetState(_currentState.ToString());
		}

		/// <summary>
		/// Overloaded. Display a balloon window on top of the tray icon.
		/// </summary>
		/// <param name="icon">Balloon window icon</param>
		/// <param name="text">Text to display</param>
		/// <param name="title">Title to display</param>
		public void ShowBalloon(BalloonIcon icon, string text, string title)
		{
			ShowBalloon(icon, text, title, 15000);
		}

		/// <summary>
		/// Overloaded. Display a balloon window on top of the tray icon.
		/// </summary>
		/// <param name="icon">Balloon window icon</param>
		/// <param name="text">Text to display</param>
		/// <param name="title">Title to display</param>
		/// <param name="timeout">Time in msecs that the balloon window should be displayed</param>
		public void ShowBalloon(BalloonIcon icon, string text, string title, int timeout)
		{
			_notifyIcon.ShowBalloon((NotifyIconAnimation.EBalloonIcon)icon, text, title, timeout);
		}

		public ContextMenuStrip IconContextMenu
		{
			get { return _notifyIcon.ContextMenu; }
			set { _notifyIcon.ContextMenu = value; }
		}

		public bool IconVisible
		{
			get { return _notifyIcon.Visible; }
			set { _notifyIcon.Visible = value; }
		}


		#region private 
		
		private void OnTrayIconClick(object sender, EventArgs e)
		{
			var handler = TrayIconClicked;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}

		private void OnTrayBalloonClick(object sender, EventArgs e)
		{
			var handler = TrayBalloonClickClicked;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}

		private void OnTrayBalloonTimeoutClose(object sender, EventArgs e)
		{
			var handler = TrayBalloonTimeout;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}

		#endregion

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_notifyIcon != null)
					_notifyIcon.Dispose();

				_notifyIcon = null;
			}
		}
	}
}
