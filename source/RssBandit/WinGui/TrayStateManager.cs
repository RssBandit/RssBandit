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
using System.Windows.Forms;

using RssBandit.Resources;
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
					SR.GUIStatusIdle, 
					Resource.LoadIcon(String.Format("Resources.AppTray{0}.ico", postFix))
				)
			);
			
			if (Win32.IsOSAtLeastWindowsXP) {
				_notifyIcon.AddState(
					new NotifyIconState(ApplicationTrayState.NewUnreadFeedsReceived.ToString(),
					SR.GUIStatusNewFeedItemsReceived, 
					(il == null ? Resource.LoadBitmapStrip(String.Format("Resources.AniImages{0}.png", postFix), new Size(16,16) /*, new Point(0,0) */) : il) , 3)
					);
			} else {
				_notifyIcon.AddState(
					new NotifyIconState(ApplicationTrayState.NewUnreadFeedsReceived.ToString(),
					SR.GUIStatusNewFeedItemsReceived, 
					Resource.LoadIcon(String.Format("Resources.UnreadFeedItems{0}.ico", postFix))
					)
				);
			}
			
			_notifyIcon.AddState(
				new NotifyIconState(ApplicationTrayState.BusyRefreshFeeds.ToString(),
				SR.GUIStatusBusyRefresh, 
				Resource.LoadIcon(String.Format("Resources.AppBusy{0}.ico", postFix))
				)
			);

			_notifyIcon.AddState(
				new NotifyIconState(ApplicationTrayState.NewUnreadFeeds.ToString(),
					SR.GUIStatusUnreadFeedItemsAvailable, 
					Resource.LoadIcon(String.Format("Resources.UnreadFeedItems{0}.ico", postFix))
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
