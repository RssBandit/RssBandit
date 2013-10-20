using System;
using System.ComponentModel;

namespace RssBandit.AppServices
{
	#region IPropertyChange
	/// <summary>
	/// Support property change notification
	/// </summary>
	public interface IPropertyChange {
		/// <summary>
		/// Gets fired on a property change
		/// </summary>
		event PropertyChangedEventHandler PropertyChanged;
	}
	#endregion

	#region IInternetService
	/// <summary>
	/// IInternetService.
	/// </summary>
	public interface IInternetService
	{
		/// <summary>
		/// Attach to receive InternetConnectionStateChange Events.
		/// </summary>
		event InternetConnectionStateChangeHandler InternetConnectionStateChange;

		/// <summary>
		/// Gets the general info (bool) if direct internet access 
		/// is currently possible and allowed.
		/// </summary>
		bool InternetAccessAllowed { get; }

		/// <summary>
		/// Gets the general info (bool) if the application is currently 
		/// running in Offline Mode.
		/// </summary>
		bool InternetConnectionOffline { get; }
		
		/// <summary>
		/// Gets the current INetState.
		/// </summary>
		InternetState InternetConnectionState { get; }
	}
	#endregion
}
