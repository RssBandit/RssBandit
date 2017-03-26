#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Reflection; 
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using log4net;
using NewsComponents.Utils;
using RssBandit.Common.Logging;


namespace RssBandit
{
	/// <summary>
	/// Helper class used to manage application Resources
	/// </summary>
	internal static class Resource {
		
		#region News Item Images/Colors (index store)

		#region Item Flag background colors
		public sealed class ItemFlagBackground
		{
			public static Color Red { get { return Color.FromArgb(207, 93, 96); } }
			public static Color Purple { get { return Color.FromArgb(165, 88, 124); } }
			public static Color Blue { get { return Color.FromArgb(92, 131, 180); } }
			public static Color Yellow { get { return Color.FromArgb(255, 193, 96); } }
			public static Color Green { get { return Color.FromArgb(139, 180, 103); } }
			public static Color Orange { get { return Color.FromArgb(255, 140, 90); } }
		}
		#endregion
		
		#region News Item Core Image Indexes
		/// <summary>
		/// News Item Core Image Indexes. 
		/// It is important that the Read state preceeds the Unread state by one!
		/// </summary>
		public sealed class NewsItemImage {
			public const int DefaultRead = 0;	
			public const int DefaultUnread = 1;	
			public const int OutgoingRead = 2;	
			public const int OutgoingUnread = 3;	
			public const int IncomingRead = 4;	
			public const int IncomingUnread = 5;	
			public const int CommentRead = 6;	
			public const int CommentUnread = 7;	
			public const int ReplySentRead = 8;	
			public const int ReplySentUnread = 9;	
		}
		#endregion

		#region Item Flag Image Indexes
		public sealed class FlagImage
		{
			public const int Red = 14;	
			public const int Purple = 15;	
			public const int Blue = 16;	
			public const int Yellow = 17;	
			public const int Green = 18;	
			public const int Orange = 19;	
			public const int Clear = 20;	
			public const int Complete = 21;	
		}
		#endregion

		#region Other related (index store)
		/// <summary>
		/// The used listview image index offsets
		/// (centralized here to enable easier icon modifications).
		/// </summary>
		public sealed class NewsItemRelatedImage {
			public const int Attachment = 22;
			public const int Failure = 23;
		}
		#endregion

		#endregion

		#region Tool Image Indexes
		/// <summary>
		/// Main Toolitems Image Indexes. 
		/// </summary>
		public sealed class ToolItemImage {
			public const int RefreshAll = 0;	
			public const int NewSubscription = 1;	
			public const int Delete = 2;	
			public const int NextUnreadItem = 3;	
			public const int NextUnreadSubscription = 4;	
			public const int PostReply = 5;	
			public const int OptionsDialog = 6;	
			public const int MarkAsRead = 7;	
			public const int ToggleSubscriptions = 13;	
			public const int ToggleRssSearch = 14;	
			public const int Search = 15;	
			public const int NewPost = 16;
			public const int ItemDetailViewAtRight = 17;	
			public const int ItemDetailViewAtBottom = 18;	
			public const int ItemDetailViewAtLeft = 19;	
			public const int ItemDetailViewAtTop = 20;	
			public const int ItemDetailViewWithoutList = 21;	

			public const int NewNntpSubscription = 22;	
			public const int NewDiscoveredSubscription = 23;	
			
			/// <summary>
			/// This offset is used to address BrowserItemImage's
			/// within AllToolsImages.bmp strip!
			/// </summary>
			/// <example>
			/// int openFolderIndex = 
			///   ToolItemImage.BrowserItemImageOffset + 
			///   BrowserItemImage.OpenFolder;
			/// </example>
			public const int BrowserItemImageOffset = 30;
		}
		#endregion

		#region Browser Image Indexes
		/// <summary>
		/// Browser Toolitems Image Indexes. 
		/// </summary>
		public sealed class BrowserItemImage {
			public const int OpenNewTab = 0;	

			// not all images are currently used
			public const int OpenFolder = 1;	
			public const int Save = 2;	
			public const int Cut = 3;	
			public const int Copy = 4;	
			public const int Paste = 5;	
			public const int Delete = 6;	
			public const int Checked = 7;		//?
			public const int Undo = 8;	
			public const int Redo = 9;	
			public const int Zoom = 10;	
			public const int Print = 11;	
			public const int Search = 12;	
			public const int SearchContinue = 13;	
			public const int PointedHelp = 14;	
			public const int ZoomIn = 15;	
			public const int ZoomOut = 16;	
			public const int GoBack = 17;	
			public const int GoForward = 18;	
			//...
			public const int CancelNavigation = 21;	
			public const int Refresh = 22;	
			public const int Home = 23;
			//...
			public const int Mail = 34;	
			//...
			public const int SearchWeb = 37;	
			public const int DoNavigate = 38;	
			public const int OpenInExternalBrowser = 39;	
		}
		#endregion

		#region SubscriptionTreeImage (index store)
		/// <summary>
		/// The used subscription tree image index offsets
		/// (centralized here to enable easier icon modifications).
		/// </summary>
		public sealed class SubscriptionTreeImage
		{
			// general root nodes
			public const int AllSubscriptions = 0;
			public const int AllSubscriptionsExpanded = 1;
			public const int AllFinderFolders = 21;
			public const int AllFinderFoldersExpanded = 22;
			public const int AllSmartFolders = 19;
			public const int AllSmartFoldersExpanded = 20;

			// category node images
			public const int SubscriptionsCategory = 2;
			public const int SubscriptionsCategoryExpanded = 3;
			public const int FinderCategory = 2;
			public const int FinderCategoryExpanded = 3;

			// feed node images
			public const int Feed = 4;
			public const int FeedSelected = 4;
			public const int FeedDisabled = 5;
			public const int FeedDisabledSelected = 5;
			public const int FeedFailure = 6;
			public const int FeedFailureSelected = 6;
			public const int FeedSecured = 7;
			public const int FeedSecuredSelected = 7;
			public const int FeedUpdating = 8;
			public const int FeedUpdatingSelected = 8;

			// nntp node images
			public const int Nntp = 23;
			public const int NntpSelected = 23;
			public const int NntpDisabled = 24;
			public const int NntpDisabledSelected = 24;
			public const int NntpFailure = 25;
			public const int NntpFailureSelected = 25;
			public const int NntpSecured = 26;
			public const int NntpSecuredSelected = 26;
			public const int NntpUpdating = 27;
			public const int NntpUpdatingSelected = 27;

			// feed/news failures
			public const int Exceptions = 9;
			public const int ExceptionsSelected = 9;
			
			public const int SentItems = 11;
			public const int SentItemsSelected = 11;

			public const int WatchedItems = 28;
			public const int WatchedItemsSelected = 29;


			// flag nodes
			public const int RedFlag = 12;
			public const int RedFlagSelected = 12;
			public const int BlueFlag = 13;
			public const int BlueFlagSelected = 13;
			public const int GreenFlag = 14;
			public const int GreenFlagSelected = 14;
			public const int YellowFlag = 15;
			public const int YellowFlagSelected = 15;
			public const int ReplyFlag = 16;
			public const int ReplyFlagSelected = 16;

			// waste basket/deleted items
			public const int WasteBasketEmpty = 17;
			public const int WasteBasketEmptySelected = 17;
			public const int WasteBasketFull = 18;
			public const int WasteBasketFullSelected = 18;

			// search folder
			public const int SearchFolder = 10;
			public const int SearchFolderSelected = 10;
		}
		#endregion

		#region Toolbar keys
		/// <summary>
		/// Defines toolbar keys
		/// </summary>
		internal sealed class Toolbar
		{
			public static string WebTools = "tbWebBrowser";
			public static string MenuBar = "tbMainMenu";
			public static string MainTools = "tbMainAppBar";
			public static string SearchTools = "tbSearchBar";
		}
		#endregion
		
		#region Navigator Group keys
		/// <summary>
		/// Defines Navigator Group keys
		/// </summary>
		internal sealed class NavigatorGroup {
			public static string Subscriptions = "groupFeedsTree";
			public static string RssSearch = "groupFeedsSearch";
		}
		#endregion
		
		#region Application Sounds
		
		public class ApplicationSound 
		{
			private const string appSndPrefix = "RSSBANDIT_";
			private static readonly ILog _log = Log.GetLogger(typeof(ApplicationSound));
		
			public const string FeedDiscovered = appSndPrefix + "FeedDiscovered";
			public const string NewItemsReceived = appSndPrefix + "ItemsReceived";
			public const string NewAttachmentDownloaded = appSndPrefix + "AttachmentDownloaded";

			public static string GetSoundFilePath(string applicationSound)
			{
				try
				{
					string soundFile = null;
					// just to ensure only the predefined sounds are played:
					switch (applicationSound)
					{
						case FeedDiscovered:
							soundFile = Path.Combine(Application.StartupPath, @"Media\Feed Discovered.wav");
							break;
						case NewItemsReceived:
							soundFile = Path.Combine(Application.StartupPath, @"Media\New Feed Items Received.wav");
							break;
						case NewAttachmentDownloaded:
							soundFile = Path.Combine(Application.StartupPath, @"Media\New Attachment Downloaded.wav");
							break;
					}

					if (File.Exists(soundFile))
					{
						return soundFile;
					}

				}
				catch (Exception ex)
				{
					_log.Error("Error in GetSoundFilePath('" + applicationSound + "'): " +ex.Message, ex);
				}

				return null;
			}

			public static Stream GetSoundStream(string applicationSound)
			{
				try
				{
					string soundFile = GetSoundFilePath(applicationSound);
					
					if (!String.IsNullOrEmpty(soundFile) && File.Exists(soundFile))
					{
						return FileHelper.OpenForRead(soundFile);
					}

				}
				catch (Exception ex)
				{
					_log.Error("Error in GetSoundStream('" + applicationSound + "'): " + ex.Message, ex);
				}

				return null;
			}
		}
		
		#endregion
		
		#region OutgoingLinks 

		internal static class OutgoingLinks
		{
			internal static class Default
			{

				public const string FeedValidationUrlBase = "http://www.feedvalidator.org/check?url=";
				public const string FeedLinkCosmosUrlBase = "http://www.technorati.com/cosmos/links.html?rank=links&url=";
			
				// now use the new tracker at SF, filtering bugs by status "open":
				public const string BugReportUrl = "https://github.com/RssBandit/RssBandit/issues";
				// now pointing to the new site structure:
				public const string WebHelpUrl = "http://docs.rssbandit.org/v1.8/";
				public const string ProjectNewsUrl = "http://rssbandit.org/";
				public const string ProjectBlogUrl = "http://rssbandit.org/blog/";
				public const string UserForumUrl = "http://forum.rssbandit.org/";
				public const string ProjectDonationUrl = "http://rssbandit.org/donate/";
				public const string ProjectDownloadUrl = "http://rssbandit.org/rss-bandit-download/";
			}

			// they can be overridden by app.config settings:
			public static string FeedValidationUrlBase = Default.FeedValidationUrlBase;
			public static string FeedLinkCosmosUrlBase = Default.FeedLinkCosmosUrlBase;
			public static string BugReportUrl = Default.BugReportUrl;
			public static string WebHelpUrl = Default.WebHelpUrl;
			public static string ProjectNewsUrl = Default.ProjectNewsUrl;
			public static string ProjectBlogUrl = Default.ProjectBlogUrl;
			public static string UserForumUrl = Default.UserForumUrl;
			public static string ProjectDonationUrl = Default.ProjectDonationUrl;
			public static string ProjectDownloadUrl = Default.ProjectDownloadUrl;

			private static readonly string[] applicationUpdateServiceUrls =
				new[] { "http://updateService.rssbandit.org/UpdateService.asmx" };

			/// <summary>
			/// Gets the update service URL.
			/// </summary>
			/// <value>The update service URL.</value>
			public static string UpdateServiceUrl
			{
				get
				{
					int idx = DateTime.Now.Second % applicationUpdateServiceUrls.Length;
					return applicationUpdateServiceUrls[idx];
				}
			}
		}

		#endregion

		#region public methods
		
		/// <summary>
		/// Gets a resource stream with the messages used by the Bandit classes
		/// </summary>
		/// <param name="name">resource key</param>
		/// <returns>a resource stream</returns>
		public static Stream GetStream( string name ){
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Resource).Namespace + "." + name); 
		}

		/// <summary>
		/// Loads an Cursor resource from the executing assembly Manifest stream.
		/// </summary>
		/// <param name="cursorName">The name of the cursor resource</param>
		/// <returns>Cursor instance.</returns>
		/// <remarks>Remember, that resource names within a project subfolder 
		/// needs to be prefixed with subfolder name like this: "MySubfolder.MyResource.cur".
		/// The Resource class uses the Namespace of itself to prefix the provided name.
		/// </remarks>
		public static Cursor LoadCursor(string cursorName) {
			return new Cursor(GetStream(cursorName));
		}
		
		/// <summary>
		/// Loads an Icon resource from the executing assembly Manifest stream.
		/// </summary>
		/// <param name="iconName">The name of the icon resource</param>
		/// <returns>Icon instance.</returns>
		/// <remarks>Remember, that resource names within a project subfolder 
		/// needs to be prefixed with subfolder name like this: "MySubfolder.MyResource.ico".
		/// The Resource class uses the Namespace of itself to prefix the provided name.
		/// </remarks>
		public static Icon LoadIcon(string iconName) {
/*			
 			// This loop helps to identify a resource name (e.g. an icon "a.ico" within 
			// a subfolder "Resources" has a name like this: MyNameSapce.Resources.a.ico
			System.Reflection.Assembly thisExe = Assembly.GetExecutingAssembly();
			string[] resources = thisExe.GetManifestResourceNames();
			
			foreach (string resource in resources) {
				Trace.WriteLine(resource , "ManifestResourceName");
			}
*/
			return new Icon(GetStream(iconName));
		}

		/// <summary>
		/// Loads an Icon resource with a specific size 
		/// from the executing assembly Manifest stream.
		/// </summary>
		/// <param name="iconName">The name of the icon resource</param>
		/// <param name="iconSize">The size of the icon to load</param>
		/// <returns>Icon instance.</returns>
		/// <remarks>Remember, that resource names within a project subfolder 
		/// needs to be prefixed with subfolder name like this: "MySubfolder.MyResource.ico".
		/// The Resource class uses the Namespace of itself to prefix the provided name.
		/// </remarks>
		public static Icon LoadIcon(string iconName, Size iconSize) {
			return new Icon(LoadIcon(iconName), iconSize);
		}

		/// <summary>
		/// Loads an Bitmap resource 
		/// from the executing assembly Manifest stream.
		/// </summary>
		/// <param name="imageName">Name of the bitmap resource</param>
		/// <returns>Bitmap instance.</returns>
		/// <remarks>Remember, that resource names within a project subfolder 
		/// needs to be prefixed with subfolder name like this: "MySubfolder.MyResource.gif".
		/// The Resource class uses the Namespace of itself to prefix the provided name.
		/// </remarks>
		public static Bitmap LoadBitmap(string imageName) {
			return LoadBitmap(imageName, false, Point.Empty);
		}
		/// <summary>
		/// Loads an Bitmap resource 
		/// from the executing assembly Manifest stream and makes it transparent.
		/// </summary>
		/// <param name="imageName">Name of the bitmap resource</param>
		/// <param name="transparentPixel">A pixel, that marks a color at the position
		/// to be used as transparent color.</param>
		/// <returns>Bitmap instance.</returns>
		/// <remarks>Remember, that resource names within a project subfolder 
		/// needs to be prefixed with subfolder name like this: "MySubfolder.MyResource.bmp".
		/// The Resource class uses the Namespace of itself to prefix the provided name.
		/// </remarks>
		public static Bitmap LoadBitmap(string imageName, Point transparentPixel) {
			return LoadBitmap(imageName, true, transparentPixel);
		}
		
		// workhorse:
		private static Bitmap LoadBitmap(string imageName, bool makeTransparent, Point transparentPixel) {
			Bitmap bmp = new Bitmap(GetStream(imageName));
			if (makeTransparent) {
				Color c = bmp.GetPixel(transparentPixel.X, transparentPixel.Y);
				bmp.MakeTransparent(c);
			}
			return bmp;
		}


		/// <summary>
		/// Loads an BitmapStrip resource 
		/// from the executing assembly Manifest stream.
		/// </summary>
		/// <param name="imageName">Name of the BitmapStrip resource</param>
		/// <param name="imageSize">Size of one image</param>
		/// <returns>ImageList instance, that contains the images from the strip</returns>
		/// <remarks>Remember, that resource names within a project subfolder 
		/// needs to be prefixed with subfolder name like this: "MySubfolder.MyStripResource.bmp".
		/// The Resource class uses the Namespace of itself to prefix the provided name.
		/// </remarks>
		public static ImageList LoadBitmapStrip(string imageName, Size imageSize) {
			return LoadBitmapStrip(imageName, imageSize, false, Point.Empty);
		}

		/// <summary>
		/// Loads an BitmapStrip resource 
		/// from the executing assembly Manifest stream and makes it transparent.
		/// </summary>
		/// <param name="imageName">Name of the BitmapStrip resource</param>
		/// <param name="imageSize">Size of one image</param>
		/// <param name="transparentPixel">A pixel, that marks a color at the position
		/// to be used as transparent color.</param>
		/// <returns>ImageList instance, that contains the images from the strip</returns>
		/// <remarks>Remember, that resource names within a project subfolder 
		/// needs to be prefixed with subfolder name like this: "MySubfolder.MyStripResource.bmp".
		/// The Resource class uses the Namespace of itself to prefix the provided name.
		/// </remarks>
		public static ImageList LoadBitmapStrip(string imageName, Size imageSize, Point transparentPixel) {
			return LoadBitmapStrip(imageName, imageSize, true, transparentPixel);
		}

		// workhorse:
		private static ImageList LoadBitmapStrip(string imageName, Size imageSize, bool makeTransparent, Point transparentPixel) {
			Bitmap bmp = LoadBitmap(imageName, makeTransparent, transparentPixel);
            ImageList img = new ImageList();
            img.ColorDepth = ColorDepth.Depth32Bit;
			img.ImageSize = imageSize;
			img.Images.AddStrip(bmp);
			return img;
		}

	    /// <summary>
	    /// Loads an BitmapStrip resource 
	    /// from the executing assembly Manifest stream.
	    /// </summary>
	    /// <param name="image">The image</param>
	    /// <param name="imageSize">Size of one image</param>
	    /// <returns>ImageList instance, that contains the images from the strip</returns>
	    /// <remarks>Remember, that resource names within a project subfolder 
	    /// needs to be prefixed with subfolder name like this: "MySubfolder.MyStripResource.bmp".
	    /// The Resource class uses the Namespace of itself to prefix the provided name.
	    /// </remarks>
	    public static ImageList LoadBitmapStrip(Image image, Size imageSize)
	    {
	        ImageList img = new ImageList();
	        img.ColorDepth = ColorDepth.Depth32Bit;
	        img.ImageSize = imageSize;
	        img.Images.AddStrip(image);
	        return img;
        }

        #endregion
    }
}
