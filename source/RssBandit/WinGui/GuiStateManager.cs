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
using System.ComponentModel;
using RssBandit.AppServices;

namespace RssBandit.WinGui
{
	/// <summary>
	/// The various busy states for the feed source.
	/// </summary>
	public enum FeedSourceBusyState {
		Idle,
		RefreshOne,
		RefreshOneDone,
		RefreshCategory,
		RefreshAllAuto,
		RefreshAllForced,
		RefreshAllDone
	}

	/// <summary>
	/// Class used to manage the current state of the user interface.  
	/// For example, this indicates whether the application is currently 
	/// idle, refreshing one feed, refreshing all etc...  Also 
	/// handles connectivity state.
	/// </summary>
	public class GuiStateManager
	{
		#region FeedSource state handling
		//public delegate void NewsHandlerBeforeStateMoveHandler(FeedSourceBusyState oldState, FeedSourceBusyState newState, ref bool cancel);
		public event EventHandler<NewsHandlerBeforeStateMoveCancelEventArgs> NewsHandlerBeforeStateMove;
		//public delegate void NewsHandlerStateMovedHandler(FeedSourceBusyState oldState, FeedSourceBusyState newState);
		public event EventHandler<NewsHandlerStateMovedEventArgs> NewsHandlerStateMoved;

		/// <summary>
		/// Used to indicate that the application has 
		/// transitioned form one state to another.
		/// </summary>
		/// <param name="newState"></param>
		public void MoveNewsHandlerStateTo(FeedSourceBusyState newState) 
		{
			FeedSourceBusyState oldState = this._sourceBusyState;
			bool shouldCancel = false;
			
			if (newState == oldState)	// not really a new state
				return;

			if (NewsHandlerBeforeStateMove != null)
			{
				try
				{
					NewsHandlerBeforeStateMoveCancelEventArgs args = new NewsHandlerBeforeStateMoveCancelEventArgs(oldState, newState, false);
					NewsHandlerBeforeStateMove(this, args);
					shouldCancel = args.Cancel;
				}
				catch { }
			}
			if (shouldCancel)
				return;

			this._sourceBusyState = newState;
			if (NewsHandlerStateMoved != null)
			{
				try
				{
					NewsHandlerStateMoved(this, new NewsHandlerStateMovedEventArgs( oldState, newState));
				}
				catch { }
			}
		}

		private FeedSourceBusyState _sourceBusyState = FeedSourceBusyState.Idle;
		
		/// <summary>
		/// Gets the state of the news handler.
		/// </summary>
		/// <value></value>
		public FeedSourceBusyState FeedSourceBusyState
		{
			get { return _sourceBusyState; }
		}
		
		#endregion

		#region internet connection state handling
		public event EventHandler<InternetConnectionStateChangeEventArgs> InternetConnectionStateMoved;

		/// <summary>
		/// Moves the internet connection state to the specified new state.
		/// </summary>
		/// <param name="newState">State of the new.</param>
		public void MoveInternetConnectionStateTo(INetState newState) {
			INetState oldState = internetConnectionState;
			internetConnectionState = newState;
			if (InternetConnectionStateMoved != null) {
				try {
					InternetConnectionStateMoved(this, new InternetConnectionStateChangeEventArgs(oldState, newState));
				} catch {}
			}
		}

		/// <summary>
		/// Gets a value indicating whether internet access is allowed (i.e. 
		/// the user is connected and online).
		/// </summary>
		/// <value>
		/// 	<c>true</c> if internet access allowed; otherwise, <c>false</c>.
		/// </value>
		public bool InternetAccessAllowed {
			get { 
				if ((internetConnectionState & INetState.Connected) > 0 && (internetConnectionState & INetState.Online) > 0) 
					return true; 
				return false;
			}
		}
		
		/// <summary>
		/// Gets a value indicating whether the internet connection is offline.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if internet connection is offline; otherwise, <c>false</c>.
		/// </value>
		public bool InternetConnectionOffline {
			get { 
				if ((internetConnectionState & INetState.Offline) > 0) 
					return true; 
				return false;
			}
		}
		
		/// <summary>
		/// Gets the state of the internet connection.
		/// </summary>
		/// <value></value>
		public INetState InternetConnectionState {
			get { return internetConnectionState; 	}
		}
		private INetState internetConnectionState = INetState.Invalid;
		#endregion

	}

	#region eventArgs classes

	public class NewsHandlerStateMovedEventArgs : EventArgs
	{
		public FeedSourceBusyState OldState { get; private set; }
		public FeedSourceBusyState NewState { get; private set; }

		public NewsHandlerStateMovedEventArgs(FeedSourceBusyState oldState, FeedSourceBusyState newState)
		{
			this.OldState = oldState;
			this.NewState = newState;
		}
	}

	public class NewsHandlerBeforeStateMoveCancelEventArgs : CancelEventArgs
	{
		public FeedSourceBusyState OldState { get; private set; }
		public FeedSourceBusyState NewState { get; private set; }

		public NewsHandlerBeforeStateMoveCancelEventArgs(FeedSourceBusyState oldState, FeedSourceBusyState newState, bool cancel)
			: base(cancel)
		{
			this.OldState = oldState;
			this.NewState = newState;
		}
	}

	#endregion
}
