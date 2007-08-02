#region CVS Version Header
/*
 * $Id: Resource.cs,v 1.5 2005/03/08 16:56:08 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/03/08 16:56:08 $
 * $Revision: 1.5 $
 */
#endregion

using System;
using System.Resources;
using System.Reflection; 
using System.IO;
using System.Drawing;
using System.Windows.Forms;

using System.Diagnostics;

namespace RssBandit
{
	/// <summary>
	/// Helper class used to manage application Resources
	/// </summary>
	internal sealed class Resource {
		#region Static part
		private const string ResourceFileName = ".Resources.RSSBanditText";

		static Resource InternalResource = new Resource();
		/// <summary>
		/// Gets a resource manager for the assembly resource file
		/// </summary>
		public static Resource Manager {
			[DebuggerStepThrough()]
			get	{	return InternalResource; }
		}
		#endregion
		
		#region Instance part 
		ResourceManager rm = null;

		/// <summary>
		/// Constructor
		/// </summary>
		public Resource() {
			rm = new ResourceManager(this.GetType().Namespace + ResourceFileName, Assembly.GetExecutingAssembly());
		}

		/// <summary>
		/// Gets the message with the specified key from the assembly resource file
		/// </summary>
		public string this [ string key ]	{
			[DebuggerStepThrough()]
			get	{
				return rm.GetString( key, System.Globalization.CultureInfo.CurrentUICulture );
			}
		}

		/// <summary>
		/// Gets the formatted message with the specified key and format arguments
		/// from the assembly resource file
		/// </summary>
		public string this [ string key, params object[] formatArgs ]	{
			[DebuggerStepThrough()]
			get	{
				return String.Format( System.Globalization.CultureInfo.CurrentUICulture, this[key], formatArgs );  
			}
		}
		/// <summary>
		/// Gets a resource stream with the messages used by the Bandit classes
		/// </summary>
		/// <param name="name">resource key</param>
		/// <returns>a resource stream</returns>
		public Stream GetStream( string name ){
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(this.GetType().Namespace + "." + name); 
		}

		/// <summary>
		/// Formats a message stored in the Data assembly resource file.
		/// </summary>
		/// <param name="key">resource key</param>
		/// <param name="format">format arguments</param>
		/// <returns>a formated string</returns>
		public string FormatMessage( string key, params object[] formatArgs )	{
			return String.Format( System.Globalization.CultureInfo.CurrentUICulture, this[key], formatArgs );  
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
		public Cursor LoadCursor(string cursorName) {
			return new Cursor(this.GetStream(cursorName));
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
		public Icon LoadIcon(string iconName) {
/*			
 			// This loop helps to identify a resource name (e.g. an icon "a.ico" within 
			// a subfolder "Resources" has a name like this: MyNameSapce.Resources.a.ico
			System.Reflection.Assembly thisExe = Assembly.GetExecutingAssembly();
			string[] resources = thisExe.GetManifestResourceNames();
			
			foreach (string resource in resources) {
				Trace.WriteLine(resource , "ManifestResourceName");
			}
*/
			return new Icon(this.GetStream(iconName));
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
		public Icon LoadIcon(string iconName, Size iconSize) {
			return new Icon(this.LoadIcon(iconName), iconSize);
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
		public Bitmap LoadBitmap(string imageName) {
			return this.LoadBitmap(imageName, false, Point.Empty);
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
		public Bitmap LoadBitmap(string imageName, Point transparentPixel) {
			return this.LoadBitmap(imageName, true, transparentPixel);
		}
		
		// workhorse:
		private Bitmap LoadBitmap(string imageName, bool makeTransparent, Point transparentPixel) {
			Bitmap bmp = new Bitmap(this.GetStream(imageName));
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
		public ImageList LoadBitmapStrip(string imageName, Size imageSize) {
			return this.LoadBitmapStrip(imageName, imageSize, false, Point.Empty);
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
		public ImageList LoadBitmapStrip(string imageName, Size imageSize, Point transparentPixel) {
			return this.LoadBitmapStrip(imageName, imageSize, true, transparentPixel);
		}

		// workhorse:
		private ImageList LoadBitmapStrip(string imageName, Size imageSize, bool makeTransparent, Point transparentPixel) {
			Bitmap bmp = this.LoadBitmap(imageName, makeTransparent, transparentPixel);
			ImageList img = new ImageList();
			img.ImageSize = imageSize;
			img.Images.AddStrip(bmp);
			return img;
		}

		#endregion
	}
}
