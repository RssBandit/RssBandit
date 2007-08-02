using System;
using System.Drawing;

namespace RssBandit.AppServices
{
	/// <summary>
	/// User Preferences service
	/// </summary>
	public interface IUserPreferences: IPropertyChange
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
		DateTime LastAutoUpdateCheck { get; }

		/// <summary>
		/// Get the TimeSpan for the global maximum news item age.
		/// We use TimeSpan.MinValue for the unlimited item age.
		/// </summary>
		TimeSpan MaxItemAge { get; }

		/// <summary>
		/// Normal font used to render items (listview) 
		/// and feeds (treeview)
		/// </summary>
		Font NormalFont { get; }

		/// <summary>
		/// Normal font color used to render items (listview) 
		/// and feeds (treeview)
		/// </summary>
		Color NormalFontColor { get; }

		/// <summary>
		/// Font used to highlight items (listview) 
		/// and feeds (treeview)
		/// </summary>
		Font HighlightFont { get; }
			
		/// <summary>
		/// Color used to highlight items (listview) 
		/// and feeds (treeview)
		/// </summary>
		Color HighlightFontColor { get; }

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
		Font RefererFont { get; }

		/// <summary>
		/// Color used to render items that refer back to the users 
		/// default identity (listview) 
		/// </summary>
		Color RefererFontColor { get; }

		/// <summary>
		/// Font used to render items that display an error message (listview) 
		/// </summary>
		Font ErrorFont { get; }

		/// <summary>
		/// Color used to render items that display an error message (listview) 
		/// </summary>
		Color ErrorFontColor { get; }


		//TODO: much more to add here (RemoteStorage, etc.)

	}
}
