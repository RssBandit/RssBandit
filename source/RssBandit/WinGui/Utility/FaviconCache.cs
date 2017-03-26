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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using log4net;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Utils;
using RssBandit.Common.Logging;

namespace RssBandit.WinGui.Utility
{
	internal static class FaviconCache
	{
		#region ivars

		//Stores Image object for favicons so we can reuse the same object if used by
		//multiple feeds. 
		private static readonly Dictionary<string, Image> _favicons = new Dictionary<string, Image>();

		// logging/tracing:
		private static readonly ILog _log = Log.GetLogger(typeof (FaviconCache));

		#endregion

		public static Image GetImage(FeedSource source, INewsFeed feed)
		{
			if (feed == null || source == null)
				return null;

			if (!source.FeedHasFavicon(feed))
				return null;

			var id = feed.favicon;
			Uri faviconUri;
			
			if (Uri.TryCreate(feed.link, UriKind.Absolute, out faviconUri))
				id = "{0}/{1}".FormatWith(faviconUri.Authority, feed.favicon);

            Image image = null;
			

			try
			{
			    if (_favicons.TryGetValue(id, out image))
			    {
                    // Ensure they're valid
                    var w = image.Width;
                    var h = image.Height;

			        return image;
			    }

                byte[] imageData = source.GetFaviconForFeed(feed);
				
				if (ImageDataAreResized(ref imageData, feed.favicon, out image))
				{
					// save resized image data to permanent store:
					source.SetFaviconForFeed(feed, feed.favicon, imageData);

				    // Ensure they're valid
				    var w = image.Width;
				    var h = image.Height;
                }
			}
			catch(Exception ex)
			{
				_log.Error("Failed in GetImage({0}); id = {1}".FormatWith(feed.link, id), ex);
			}
			
			lock (_favicons)
			{
				if (!_favicons.ContainsKey(id) && image != null)
					_favicons.Add(id, image);
			}

			return image;
		}

		#region private

		// check for size 16x16
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
		private static bool ImageDataAreResized(ref byte[] imageData, string imageName, out Image favicon)
		{
			favicon = null;
			bool changed;

			if (imageData == null || String.IsNullOrEmpty(imageName))
				return false;

			var extension = Path.GetExtension(imageName);

			using (Stream s = new MemoryStream(imageData))
			{
				if (".ico".EqualsOrdinalIgnoreCase(extension))
				{
					try
					{
						// looks like an ICO:
						using (var ico = new Icon(s, new Size(16, 16)))
						{
							//HACK: this is a workaround to the AccessViolationException caused
							// on call .ToBitmap(), if the ico.Width is != ico.Height (CLR 2.0)
							// XP and below can't handle non-square icons
							if (ico.Width != ico.Height)
								return false;

							favicon = ResizeFavicon(ico.ToBitmap());
						}
					}
					catch
					{
						// may happens, if we just downloaded a new icon from Web, that is not a real ICO (e.g. .png)
					}
				}

				if (favicon != null)
					return true;

				// for debug:
				WriteDataToTemp(imageData, imageName);

				// fallback to init an icon from other image file formats:

				// we call Image.FromStream with validateImageData set to true to
				// prevent UI control rendering issues later on. Better we fail here
				// (and ignore the image) than breaking the UI/rendering:
				favicon = ResizeFavicon(Image.FromStream(s, true, true), out changed);
			}

			if (changed)
			{
				using (MemoryStream saveStream = new MemoryStream())
				{
					var codecInfo = GetImageCodecInfo(extension);
					if (codecInfo != null)
					{
						var codecParam = new EncoderParameter(Encoder.Quality, 60L);
						var codecParams = new EncoderParameters(1);
						codecParams.Param = new[] {codecParam};

						favicon.Save(saveStream, codecInfo, codecParams);
					}
					else
					{
						// fails on .jpg, for whatever reason
						favicon.Save(saveStream, favicon.RawFormat);
					}

					imageData = saveStream.ToArray();
				}

				return true;
			}


			return false;
		}

		/// <summary>
		/// Resizes the image to 16x16 so it can be used as a favicon in the treeview
		/// </summary>
		/// <param name="toResize">Image to resize.</param>
		/// <returns></returns>
		private static Image ResizeFavicon(Image toResize)
		{
			bool resized;
			return ResizeFavicon(toResize, out resized);
		}

		/// <summary>
		/// Resizes the image to 16x16 so it can be used as a favicon in the treeview
		/// </summary>
		/// <param name="toResize">To resize.</param>
		/// <param name="resized">if set to <c>true</c> the returned image was resized/changed.</param>
		/// <returns></returns>
		private static Image ResizeFavicon(Image toResize, out bool resized)
		{
			resized = false;
			if ((toResize.Height == 16) && (toResize.Width == 16))
			{
				return toResize;
			}

			resized = true;

			// Fix for the exception: "A Graphics object cannot be created from an image that has an indexed pixel format"
			// See also: http://thedotnet.com/nntp/308566/showpost.aspx
			// We can specify optional pixel format, but defaults to 3bbppArgb:
			var result = new Bitmap(16, 16);
			result.SetResolution(toResize.HorizontalResolution, toResize.VerticalResolution);
			using (Graphics g = Graphics.FromImage(result))
			{
				g.SmoothingMode = SmoothingMode.HighQuality;
				g.CompositingQuality = CompositingQuality.HighQuality;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;

				g.DrawImage(toResize, 0, 0, 16, 16);
			}
			toResize.Dispose();
			return result;
		}

		private static ImageCodecInfo GetImageCodecInfo(string extension)
		{
			if (String.IsNullOrEmpty(extension))
				return null;

			extension = extension.Replace(".", "");
			if (extension.EqualsOrdinalIgnoreCase("jpg"))
				extension = "jpeg";

			return ImageCodecInfo.GetImageEncoders()
				.FirstOrDefault(e => extension.EqualsOrdinalIgnoreCase(e.FormatDescription));
		}

		[Conditional("DEBUG")]
		private static void WriteDataToTemp(byte[] imageData, string imageName)
		{
			var path = Path.Combine(Path.GetTempPath(), "FavIconDebug");
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			using (var f = FileHelper.OpenForWrite(Path.Combine(path, imageName)))
			{
				f.Write(imageData, 0, imageData.Length);
			}
		}

		#endregion
	}
}
