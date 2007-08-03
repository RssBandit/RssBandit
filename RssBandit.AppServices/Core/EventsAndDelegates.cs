using System;

using RssBandit;

namespace RssBandit.AppServices
{

	#region Feed deletion

	/// <summary>
	/// Signature to receive Feed Deletion events
	/// </summary>
	public delegate void FeedDeletedHandler(object sender, FeedDeletedEventArgs e);
	
	/// <summary>
	/// Event arguments class to inform about feed deletion
	/// </summary>
	public class FeedDeletedEventArgs: FeedEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FeedDeletedEventArgs"/> class.
		/// </summary>
		/// <param name="feedUrl">The feed Url.</param>
		public FeedDeletedEventArgs(string feedUrl):
			base(feedUrl) {
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="FeedDeletedEventArgs"/> class.
		/// </summary>
		/// <param name="feedUrl">The feed URL.</param>
		/// <param name="feedTitle">The feed title.</param>
		public FeedDeletedEventArgs(string feedUrl, string feedTitle):
			base(feedUrl, feedTitle) {
		}
	}

	/// <summary>
	/// Event arguments class to inform about a feed event
	/// </summary>
	public class FeedEventArgs: EventArgs {
		
		/// <summary>
		/// Initializes a new instance of the <see cref="FeedEventArgs"/> class.
		/// </summary>
		/// <param name="feedUrl">The feed URL.</param>
		public FeedEventArgs(string feedUrl):
			this(feedUrl, String.Empty) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedEventArgs"/> class.
		/// </summary>
		/// <param name="feedUrl">The feed url.</param>
		/// <param name="feedTitle">The title of the feed</param>
		public FeedEventArgs(string feedUrl, string feedTitle):base() {
			this._feedUrl = feedUrl;
			this._feedTitle = feedTitle;
		}
		/// <summary>
		/// Gets the feed Url.
		/// </summary>
		public string FeedUrl {
			get { return _feedUrl; }
		}

		private readonly string _feedUrl;

		/// <summary>
		/// Gets the feed caption.
		/// </summary>
		public string FeedTitle {
			get { return _feedTitle; }
		}
		private readonly string _feedTitle;

	}
	#endregion

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
