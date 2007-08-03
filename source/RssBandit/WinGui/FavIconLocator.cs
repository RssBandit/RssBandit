#region CVS Version Header
/*
 * $Id: FavIconLocator.cs,v 1.4 2005/03/05 19:52:13 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/03/05 19:52:13 $
 * $Revision: 1.4 $
 */
#endregion

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Forms;

using NewsComponents.Feed;
using NewsComponents.Net;
using RssBandit.WinGui.Utility;

namespace RssBandit.WinGui {
	
	public class FeedImagesManager {

		private string _cachePath;
		private bool _loaded;

		private Hashtable _faviconInfo;		// list of FavIconDescriptor's. Key: feeds homepage Url
		private Hashtable _feedImageInfo;	// list of FavIconDescriptor's. Key: feed link

		public FeedImagesManager(string cachePath) {
			_cachePath = cachePath;
			_loaded = false;
			_faviconInfo = new Hashtable();
			_feedImageInfo = new Hashtable();
		}

		/// <summary>
		/// Init structures from cache
		/// </summary>
		public void Load() {
			_loaded = true;
		}

		/// <summary>
		/// Write structures to cache
		/// </summary>
		public void Save() {}

		/// <summary>
		/// Gets the favicon Image. If the feed does not have one or it was not yet
		/// fetched from the web, null is returned.
		/// </summary>
		/// <param name="f">feedsFeed</param>
		/// <returns>Image, or null</returns>
		public Image GetFavIcon(feedsFeed f) {
			return null;
		}

		/// <summary>
		/// Returns the feed image url as provided within the feed XML.
		/// If there is no such element, null is returned. In case the feed image was
		/// yet retrived from the web and cached locally, a tweaked local file Url will
		/// be returned instead of the original Url.
		/// </summary>
		/// <param name="f">feedsFeed</param>
		/// <returns>string</returns>
		public string GetFeedImageUrl(feedsFeed f) {
			return null;
		}

		
		/// <summary>
		/// Used to keep the internal structures in sync. with the feed list.
		/// Images from feeds that are not found anymore in the feeds collection, are deleted.
		/// New feeds, that do not have any infos about any image requests, will be added
		/// to an internal queue for later retrive/request the image(s).
		/// </summary>
		/// <param name="feeds"></param>
		public void Synchronize(FeedsCollection feeds) {
			if (!_loaded)
				this.Load();

			if (_loaded)		// issue while load
				return;
		}
	}
	
	/// <summary>
	/// Yet under construction: FavIconHandler.
	/// </summary>
	public class FavIconLocator
	{

		public static FavIconDescriptor RefreshInfo(FavIconDescriptor descriptor, IWebProxy proxy) {
			return RefreshInfo(descriptor, proxy, null);
		}
		public static FavIconDescriptor RefreshInfo(FavIconDescriptor descriptor, IWebProxy proxy, ICredentials credentials) {
			//return RefreshInfo(descriptor, proxy, credentials);
		}

		public static FavIconDescriptor RequestIcon(string baseUri, IWebProxy proxy) {
			return RequestIcon(baseUri, proxy, null);
		}

		public static FavIconDescriptor RequestIcon(string baseUri, IWebProxy proxy, ICredentials credentials) {
			if (baseUri == null)
				throw new ArgumentNullException("baseUri");

			if (proxy == null)
				proxy = GlobalProxySelection.GetEmptyWebProxy();

			FavIconDescriptor ico = null;

			try {
				using (Stream stream = AsyncWebRequest.GetSyncResponseStream(baseUri, credentials, "RssBandit", proxy)) {
					string htmlContent = string.Empty;
					using(StreamReader reader = new StreamReader(stream)) {
						htmlContent = reader.ReadToEnd();
						ico = RequestIcon(baseUri, htmlContent, proxy, credentials);
					}
				}
			} catch {
				// no default HTML page to examine for a FavIcon entry.
				// we do not stop here: try to get a direct download by requesting favicon.ico directly
			}

			string realIconUrl = null;

			if (ico == null || ico == FavIconDescriptor.Empty) {
				try {
					Uri b = new Uri(new Uri(baseUri), "favicon.ico");
					realIconUrl = b.ToString();
				} catch (UriFormatException) {}
			}

			if (realIconUrl == null)
				return FavIconDescriptor.Empty;

			// now get the icon stream
			using (Stream stream = AsyncWebRequest.GetSyncResponseStream(realIconUrl, credentials, "RssBandit", proxy)) {
				Image img = CheckAndScaleImageFromStream(stream);
				if (img != null)
					return new FavIconDescriptor(realIconUrl, baseUri, DateTime.Now, img);
			}

			return FavIconDescriptor.Empty;
		}
			
		public static FavIconDescriptor RequestIcon(string baseUri, string htmlContent, IWebProxy proxy, ICredentials credentials) {
			if (htmlContent == null || htmlContent.Length == 0)
				return FavIconDescriptor.Empty;

			if (proxy == null)
				proxy = WebProxy.GetDefaultProxy();

			string realIconUrl = null;

			//<link rel="shortcut icon" href="url to an .ico"> 
			MatchCollection matches = autoDiscoverRegex.Matches(htmlContent);
			foreach(Match match in matches) {
				if (String.Compare(match.Groups["attName"].Value, "rel", true) == 0 ) {
					string url = match.Groups["href"].Value;
					realIconUrl = ConvertToAbsoluteUrl(url, baseUri);
					break;
				}
			}

			if (realIconUrl == null)
				return FavIconDescriptor.Empty;

			// now get the icon stream
			using (Stream stream = AsyncWebRequest.GetSyncResponseStream(realIconUrl, credentials, "RssBandit", proxy)) {
				Image img = CheckAndScaleImageFromStream(stream);
				if (img != null)
					return new FavIconDescriptor(realIconUrl, baseUri, DateTime.Now, img);
			}

			return FavIconDescriptor.Empty;
		}
		
		private static Image CheckAndScaleImageFromStream(Stream byteStream) {
			Image img = Image.FromStream(byteStream);
			if (img.Height > 1 && img.Width > 1) {
				Bitmap b = (img.Height > 16 && img.Width > 16) ? new Bitmap(img, 16, 16) : new Bitmap(img);
				return b;
			}
			return null;
		}

		private static string ConvertToAbsoluteUrl(string url, string baseurl) {
			try { 
				Uri uri = new Uri(url); 
				return uri.ToString(); 
			}
			catch(UriFormatException) {
				Uri baseUri= new Uri(baseurl);
				return (new Uri(baseUri,url).ToString()); 
			}
		}

		private static System.Drawing.Imaging.ImageCodecInfo GetCodecInfo(string mimeType) {

			System.Drawing.Imaging.ImageCodecInfo[] ici = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
			int idx = 0;
			for (int ii=0; ii<ici.Length; ii++) {
				if (ici[ii].MimeType == mimeType) {
					idx = ii;
					break;
				}
			}
			return ici[idx];
		}
		private FavIconLocator(){}

		private static Regex autoDiscoverRegex = new Regex(autoDiscoverRegexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

		#region Regex Patterns
		const string iconExtensionsPattern = "(ico)";
		const string hrefRegexPattern = @"(\s+href\s*=\s*(?:""(?<href>.*?)""|'(?<href>.*?)'|(?<href>[^'"">\s]+)))";
		const string hrefRegexIconExtensionPattern = @"(\s+href\s*=\s*(?:""(?<href>.*?\." + iconExtensionsPattern + @")""|'(?<href>.*?\." + iconExtensionsPattern + @")'|(?<href>[^'"">\s]+\." + iconExtensionsPattern + ")))";
		const string attributeRegexPattern = @"(\s+(?<attName>\w+)\s*=\s*(?:""(?<attVal>.*?)""|'(?<attVal>.*?)'|(?<attVal>[^'"">\s]+))?)";
		const string autoDiscoverRegexPattern = "<link(" + attributeRegexPattern + @"+|\s*)" + hrefRegexPattern + "(" + attributeRegexPattern + @"+|\s*)\s*/?>";
		#endregion	

	}

	[Serializable]
	public class FavIconDescriptor {
		
		private static FavIconDescriptor _empty;

		public string BaseUrl;
		public string IconUrl;
		public DateTime LastRetrieved;
		public Image Icon;

		public FavIconDescriptor(string iconUrl, string baseUrl, DateTime lastRetrieved, Image icon) {
			this.IconUrl = iconUrl;
			this.BaseUrl = baseUrl;
			this.LastRetrieved = lastRetrieved;
			this.Icon = icon;
		}

		public static FavIconDescriptor Empty {
			get {
				if (_empty == null)
					_empty = new FavIconDescriptor(String.Empty, String.Empty, DateTime.MinValue, null);
				return _empty;
			}
		}
	}
}
