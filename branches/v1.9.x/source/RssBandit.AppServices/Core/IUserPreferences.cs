using System;
using System.ComponentModel;
using System.Drawing;

namespace RssBandit.AppServices
{
	/// <summary>
	/// User Preferences service
	/// </summary>
	public interface IUserPreferences : INotifyPropertyChanged
	{

		/// <summary>
		/// Gets the user identity used to post feed comments.
		/// </summary>
		string UserIdentityForComments { get; }

		/// <summary>
		/// Get a value that control if feeds should be refreshed from the original
		/// source on startup of the application.
		/// </summary>
		bool FeedRefreshOnStartup { get; }

		/// <summary>
		/// Gets a value to control if the application have to use a proxy to
		/// request feeds.
		/// </summary>
		bool UseProxy { get; }
		
		/// <summary>
		/// If <see cref="UseProxy">UseProxy</see> is set to true, this option is used
		/// to force a takeover the proxy settings from and installed Internet Explorer (if true).
		/// (Including automatic proxy configuration).
		/// </summary>
		bool UseIEProxySettings { get; }

		/// <summary>
		/// Gets the value if the used proxy should bypass requests
		/// for local (intranet) servers.
		/// </summary>
		bool BypassProxyOnLocal { get; }

		/// <summary>
		/// Get a list of servers/web addresses to bypass by the used proxy.
		/// </summary>
		string[] ProxyBypassList { get; }

		/// <summary>
		/// Get a value indicating if the proxy have to use 
		/// custom credentials (proxy needs authentication).
		/// </summary>
		bool ProxyCustomCredentials { get; }

		/// <summary>
		/// Get the proxy address.
		/// </summary>
		string ProxyAddress { get; }

		/// <summary>
		/// Get the proxy port number.
		/// </summary>
		int ProxyPort { get; }

		/// <summary>
		/// Get the proxy custom credential user name.
		/// </summary>
		string ProxyUser { get; }

		/// <summary>
		/// Get the proxy custom credential user password.
		/// </summary>
		string ProxyPassword { get; }

		/// <summary>
		/// Get the global news item formatter stylesheet 
		/// (filename exluding path name)
		/// </summary>
		string NewsItemStylesheetFile { get; }

		/// <summary>
		/// /Get a value to control if the first opened web browser Tab should
		/// be reused or not.
		/// </summary>
		bool ReuseFirstBrowserTab { get; }

		/// <summary>
		/// Get a value to control if unread items should be marked as read
		/// while leaving the feed through UI navigation (to another feed/category)
		/// </summary>
		bool MarkItemsReadOnExit { get; }

		/// <summary>
		/// Get a value to control if an news item without a description
		/// should display the (web page) content of the link target instead (if true).
		/// </summary>
		bool NewsItemOpenLinkInDetailWindow { get; }

		/// <summary>
		/// Get the user action <see cref="HideToTray">HideToTray</see> 
		/// when the application should minimize to the
		/// system tray area.
		/// </summary>
		HideToTray HideToTrayAction { get; }

		/// <summary>
		/// Get the frequency defined in <see cref="AutoUpdateMode">AutoUpdateMode</see>
		/// the application should check for available updates (internet access required).
		/// </summary>
		AutoUpdateMode AutoUpdateFrequency { get; }

		/// <summary>
		/// Get the DateTime of the last check for updates. 
		/// <see cref="AutoUpdateFrequency">AutoUpdateFrequency</see>
		/// </summary>
		[Obsolete("Please use the property DateTime ICoreApplication:LastAutoUpdateCheck instead!", true)]
		DateTime LastAutoUpdateCheck { get; }

		/// <summary>
		/// Get the TimeSpan for the global maximum news item age.
		/// We use TimeSpan.MinValue for the unlimited item age.
		/// </summary>
		TimeSpan MaxItemAge { get; }

		/// <summary>
		/// Normal font used to render items (listview) 
		/// and feeds (tree view)
		/// </summary>
		Font NormalFont { get; }

		/// <summary>
		/// Normal font color used to render items (listview) 
		/// and feeds (tree view)
		/// </summary>
		Color NormalFontColor { get; }

		/// <summary>
		/// Font used to highlight items (listview) 
		/// and feeds (tree view)
		/// </summary>
		Font UnreadFont { get; }
			
		/// <summary>
		/// Color used to highlight items (listview) 
		/// and feeds (tree view)
		/// </summary>
		Color UnreadFontColor { get; }

		/// <summary>
		/// Font used to render flagged items (listview) 
		/// </summary>
		Font FlagFont { get; }
		
		/// <summary>
		/// Color used to render flagged items (listview) 
		/// </summary>
		Color FlagFontColor { get; }
		
		/// <summary>
		/// Font used to render items that refer back to the users 
		/// default identity (listview) 
		/// </summary>
		Font ReferrerFont { get; }

		/// <summary>
		/// Color used to render items that refer back to the users 
		/// default identity (listview) 
		/// </summary>
		Color ReferrerFontColor { get; }

		/// <summary>
		/// Font used to render items that display an error message (listview) 
		/// </summary>
		Font ErrorFont { get; }

		/// <summary>
		/// Color used to render items that display an error message (listview) 
		/// </summary>
		Color ErrorFontColor { get; }

		/// <summary>
		/// Font used to render items that received new comments (watched) 
		/// </summary>
		Font NewCommentsFont { get;	}
		

		/// <summary>
		/// Color used to render items that received new comments (watched) 
		/// </summary>
		Color NewCommentsFontColor  { get;	}
		
		/// <summary>
		/// Sets/Get the value indicating if we have to use a remote storage
		/// for sync. states.
		/// </summary>
		bool UseRemoteStorage { get; }
		
		/// <summary>
		/// Sets/Get the user name that may be required to access
		/// the remote storage location.
		/// </summary>
		string RemoteStorageUserName { get; }

		/// <summary>
		/// Sets/Get the password that may be required to access the remote
		/// storage location.
		/// </summary>
		string RemoteStoragePassword { get; }

		/// <summary>
		/// Sets/Get the type of remote storage to use. <see cref="RemoteStorageProtocolType"/>
		/// </summary>
		RemoteStorageProtocolType RemoteStorageProtocol { get; }

		/// <summary>
		/// Sets/Get the remote storage location. Can vary dep. on
		/// the location type (ftp, share,...)
		/// </summary>
		string RemoteStorageLocation { get; }

		/// <summary>
		/// Sets/Get the behavior how to handle requests to open new
		/// window(s) while browsing
		/// </summary>
		BrowserBehaviorOnNewWindow BrowserOnNewWindow { get; }

		/// <summary>
		/// Gets/Set the executable application to start if
		/// browser requires to open a new window.
		/// </summary>
		string BrowserCustomExecOnNewWindow  { get; }

		/// <summary>
		/// Sets/Get if Javascript should be allowed to execute
		/// </summary>
		bool BrowserJavascriptAllowed { get; }

		/// <summary>
		/// Sets/Get if Java should be allowed to execute
		/// </summary>
		bool BrowserJavaAllowed { get; }

		/// <summary>
		/// Sets/Get if ActiveX controls should be allowed to execute
		/// </summary>
		bool BrowserActiveXAllowed { get; }

		/// <summary>
		/// Sets/Get if background sounds are allowed to be played
		/// </summary>
		bool BrowserBGSoundAllowed { get; }

		/// <summary>
		/// Sets/Get if video can be played
		/// </summary>
		bool BrowserVideoAllowed { get; }

		/// <summary>
		/// Sets/Get if images should be loaded
		/// </summary>
		bool BrowserImagesAllowed { get; }

		/// <summary>
		/// Sets/Get the DisplayFeedAlertWindow enumeration value
		/// </summary>
		DisplayFeedAlertWindow ShowAlertWindow { get; }

		/// <summary>
		/// Sets/Get if the system tray balloon tip should be displayed
		/// if new news items are received.
		/// </summary>
		bool ShowNewItemsReceivedBalloon { get; }

		/// <summary>
		/// Sets/Get if we build the relation cosmos (interlinkage of news items).
		/// </summary>
		bool BuildRelationCosmos { get; }

		/// <summary>
		/// Gets a value indicating whether to allow application
		/// event sounds.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [allow app event sounds]; otherwise, <c>false</c>.
		/// </value>
		bool AllowAppEventSounds { get; }
	}
}
