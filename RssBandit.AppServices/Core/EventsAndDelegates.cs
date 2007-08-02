using System;

using NewsComponents;
using RssBandit;

namespace RssBandit.AppServices
{
	#region InternetConnectionState
	/// <summary>
	/// Signature to receive InternetConnectionStateChange Events.
	/// </summary>
	public delegate void InternetConnectionStateChangeHandler(object sender, InternetConnectionStateChangeEventArgs e);
 
	/// <summary>
	/// Contains the current and the new state of the internet connection.
	/// </summary>
	public class InternetConnectionStateChangeEventArgs: EventArgs {
		/// <summary>Initializer.</summary>
		/// <param name="currentState">INetState</param>
		/// <param name="newState">INetState</param>
		public InternetConnectionStateChangeEventArgs(INetState currentState, INetState newState) {
			this._currentState = currentState;
			this._newState = newState;
		}

		/// <summary>
		/// Gets the current internet connection state (flags).
		/// </summary>
		public INetState CurrentState {
			get { return _currentState; }
		}
		private readonly INetState _currentState;

		/// <summary>
		/// Gets the new internet connection state (flags).
		/// </summary>
		public INetState NewState {
			get { return _newState; }
		}
		private readonly INetState _newState;
	}
	#endregion
}
