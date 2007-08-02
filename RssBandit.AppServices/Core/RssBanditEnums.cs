using System;

namespace RssBandit
{

	/// <summary>
	/// Internet connection states
	/// </summary>
 	[Flags]
	public enum INetState {
		Invalid = 0,
		DisConnected = 1,
		Connected = 2,
		Offline = 4, 
		Online = 8
	}

	#region Preferences 
	/// <summary>
	/// What action will be taken to minimize the app to the system tray
	/// </summary>
	public enum HideToTray:int {
		None = 0,
		OnMinimize,
		OnClose
	}

	/// <summary>
	/// Auto-update interval settings
	/// </summary>
	public enum AutoUpdateMode:int {
		Manually = 0,
		OnApplicationStart,
		OnceIn14Days,
		OnceAMonth
	}

	/// <summary>
	/// Supported remote storage locations
	/// </summary>
	public enum RemoteStorageProtocolType:int {
		UNC = 0,
		FTP,
		dasBlog,	
		dasBlog_1_3,
		WebDAV
	}

	/// <summary>
	/// Embedded Web-Browser behavior on new window requests
	/// </summary>
	public enum BrowserBehaviorOnNewWindow:int {
		OpenNewTab = 0,
		OpenDefaultBrowser,
		OpenWithCustomExecutable
	}

	/// <summary>
	/// Options to control the display of toast notify windows
	/// </summary>
	public enum DisplayFeedAlertWindow:int {
		None = 0,
		AsConfiguredPerFeed,
		AsConfiguredPerCategory,
		All
	}

	#endregion
}
