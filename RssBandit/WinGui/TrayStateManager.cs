#region CVS Version Header
/*
 * $Id: TrayStateManager.cs,v 1.6 2004/04/20 12:01:50 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2004/04/20 12:01:50 $
 * $Revision: 1.6 $
 */
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;

using RssBandit.WinGui.Controls;

namespace RssBandit.WinGui
{
	/// <summary>
	/// Summary description for ApplicationTrayState.
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
		private NotifyIconAnimation _notifyIcon;

		private TrayStateManager(){
			_currentState = ApplicationTrayState.NormalIdle;
		}
		
		public TrayStateManager(NotifyIconAnimation notifyIconAnimation , ImageList il ):this()	{
			_notifyIcon = notifyIconAnimation;

			string postFix = String.Empty;
			// XP and higher are able to render the tray icon/animation with more colors than 16:
			if (Win32.IsOSAtLeastWindowsXP) {
				postFix = "XP";
			}

			_notifyIcon.AddState(
				new NotifyIconState(ApplicationTrayState.NormalIdle.ToString(),
					Resource.Manager["RES_GUIStatusIdle"], 
					Resource.Manager.LoadIcon(String.Format("Resources.AppTray{0}.ico", postFix))
				)
			);
			
			_notifyIcon.AddState(
				new NotifyIconState(ApplicationTrayState.NewUnreadFeedsReceived.ToString(),
					Resource.Manager["RES_GUIStatusNewFeedItemsReceived"], 
					(il == null ? Resource.Manager.LoadBitmapStrip(String.Format("Resources.AniImages{0}.bmp", postFix), new Size(16,16),  new Point(0,0)) : il) , 3)
			);
			
			_notifyIcon.AddState(
				new NotifyIconState(ApplicationTrayState.BusyRefreshFeeds.ToString(),
					Resource.Manager["RES_GUIStatusBusyRefresh"], 
					Resource.Manager.LoadIcon(String.Format("Resources.AppBusy{0}.ico", postFix))
				)
			);

			_notifyIcon.AddState(
				new NotifyIconState(ApplicationTrayState.NewUnreadFeeds.ToString(),
					Resource.Manager["RES_GUIStatusUnreadFeedItemsAvailable"], 
					Resource.Manager.LoadIcon(String.Format("Resources.UnreadFeedItems{0}.ico", postFix))
				)
			);

			_notifyIcon.AnimationFinished += new NotifyIconAnimation.AnimationFinishedDelegate(this.OnAnimationFinished);
			_notifyIcon.Visible = true;
			
		}

		/// <summary>
		/// Use it to set/switch the 
		/// </summary>
		public ApplicationTrayState CurrentState {
			get { return _currentState;  }
		}

		public void SetState(ApplicationTrayState state){ 
			_currentState = state; 
			_notifyIcon.SetState(_currentState.ToString());
		}

		/// <summary>
		/// Called from the NotifyIconAnimation if a animation is done (non-endless).
		/// This we use to switch to the next visual app state.
		/// </summary>
		/// <param name="sender">My NotifyIconAnimation</param>
		/// <param name="animation">The current NotifyIconState of type Animation.</param>
		private void OnAnimationFinished(object sender, NotifyIconState animation)	{
			if (animation.Key.Equals(ApplicationTrayState.NewUnreadFeedsReceived.ToString()))
				this.SetState(ApplicationTrayState.NewUnreadFeeds);
		}

	}
}
