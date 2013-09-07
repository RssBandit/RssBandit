#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System.Drawing;

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
	/// <summary>
	/// TrayStateManager class manages the application
	/// states to be visualized within the system tray icon.
	/// </summary>
	public class TrayStateManager
	{
		private ApplicationTrayState _currentState;
		private readonly NotifyIconAnimation _notifyIcon;

		private TrayStateManager()
		{
			_currentState = ApplicationTrayState.NormalIdle;
		}

		public TrayStateManager(NotifyIconAnimation notifyIconAnimation) :
			this()
		{
			_notifyIcon = notifyIconAnimation;

			_notifyIcon.AddState(
				new NotifyIconState(ApplicationTrayState.NormalIdle.ToString(),
					SR.GUIStatusIdle, Properties.Resources.AppTray)
			);


			_notifyIcon.AddState(
				new NotifyIconState(ApplicationTrayState.NewUnreadFeedsReceived.ToString(),
					SR.GUIStatusNewFeedItemsReceived,
					Resource.LoadBitmapStrip("Resources.AniImages.png", new Size(16, 16) /*, new Point(0,0) */), 2)
				);

			_notifyIcon.AddState(
				new NotifyIconState(ApplicationTrayState.BusyRefreshFeeds.ToString(),
					SR.GUIStatusBusyRefresh, Properties.Resources.AppBusy)
				);

			_notifyIcon.AddState(
				new NotifyIconState(ApplicationTrayState.NewUnreadFeeds.ToString(),
					SR.GUIStatusUnreadFeedItemsAvailable, Properties.Resources.UnreadFeedItems)
				);

			_notifyIcon.AnimationFinished += this.OnAnimationFinished;
			_notifyIcon.Visible = true;

		}

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
		/// Called from the NotifyIconAnimation if a animation is done (non-endless).
		/// This we use to switch to the next visual app state.
		/// </summary>
		/// <param name="sender">My NotifyIconAnimation</param>
		/// <param name="animation">The current NotifyIconState of type Animation.</param>
		private void OnAnimationFinished(object sender, NotifyIconState animation)
		{
			if (animation.Key.Equals(ApplicationTrayState.NewUnreadFeedsReceived.ToString()))
				this.SetState(ApplicationTrayState.NewUnreadFeeds);
		}

	}
}
