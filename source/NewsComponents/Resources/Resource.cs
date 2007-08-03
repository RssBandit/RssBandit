#region CVS Version Header
/*
 * $Id: Resource.cs,v 1.6 2007/02/17 14:45:52 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2007/02/17 14:45:52 $
 * $Revision: 1.6 $
 */
#endregion

using System;
using System.Globalization;
using System.Resources;
using System.Reflection; 
using System.IO;

namespace NewsComponents
{
	/// <summary>
	/// Helper class used to manage assembly Resources
	/// </summary>
	internal sealed class Resource {
		
		#region MessageKey enum

//		/// <summary>
//		/// The message keys defined in an enumeration to avoid message 
//		/// synchronization problems.
//		/// </summary>
//		public enum MessageKey {
//			
//			#region Feed Processing Messages
//			/// <summary/>
//			RES_ExceptionUnsupportedAtomVersion,	
//			/// <summary/>
//			RES_ExceptionUnknownXmlDialect,
//			/// <summary/>
//			RES_ExceptionDirectoryNotExistsMessage,
//			/// <summary/>
//			RES_ExceptionRFC2822ParseGroupsMessage,
//			/// <summary/>
//			RES_ExceptionRFC2822InvalidTimezoneFormatMessage,
//			/// <summary/>
//			RES_ExceptionNoProcessingHandlerMessage,
//			#endregion
//
//			#region Downloader/BITS Messages
//			/// <summary/>
//			UncPathUnsupported,
//			/// <summary/>
//			BitsDownloaderFailedDownload,
//			/// <summary/>
//			BitsDownloadedNotConfigured,
//			/// <summary/>
//			DownloadTimeoutExceeded,
//			/// <summary/>
//			CannotCreateDefaultDownloader,
//			/// <summary/>
//			CannotCreateDownloader,
//			/// <summary/>
//			BITSCannotConnectToJob,
//			/// <summary/>
//			CultureIdToGetComErrorStringsFormatted,
//			/// <summary />
//			RES_ExceptionEnclosureCacheLimitReached
//			#endregion
//
//		}

		#endregion

		#region Static part
		private const string ResourceFileName = ".Resources.RssComponentsText";

		static Resource InternalResource = new Resource();
		/// <summary>
		/// Gets a resource manager for the assembly resource file
		/// </summary>
		public static Resource Manager {
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

//		/// <summary>
//		/// Gets the message with the specified key from the assembly resource file
//		/// </summary>
//		public string this [ string key ]	{
//			get	{
//				return rm.GetString( key, System.Globalization.CultureInfo.CurrentUICulture );
//			}
//		}
//
//		/// <summary>
//		/// Gets the formatted message with the specified key and format arguments
//		/// from the assembly resource file
//		/// </summary>
//		public string this [ string key, params object[] formatArgs ]	{
//			get	{
//				return String.Format( System.Globalization.CultureInfo.CurrentUICulture, this[key], formatArgs );  
//			}
//		}
//		/// <summary>
//		/// Returns the localized string for a message key.
//		/// </summary>
//		public string this[ MessageKey key ] {
//			get {
//				return this[ key.ToString( CultureInfo.InvariantCulture ) ];
//			}
//		}
//
//		/// <summary>
//		/// Returns the localized message for a MessageKey key and a set of parameters.
//		/// </summary>
//		public string this[ MessageKey key, params object[] formatArgs ] {
//			get {
//				return String.Format( CultureInfo.CurrentUICulture, this[key], formatArgs );
//			}
//		}

		/// <summary>
		/// Gets a resource stream with the messages used by the Bandit classes
		/// </summary>
		/// <param name="name">resource key</param>
		/// <returns>a resource stream</returns>
		public Stream GetStream( string name ){
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(this.GetType().Namespace + "." + name); 
		}

//		/// <summary>
//		/// Formats a message stored in the Data assembly resource file.
//		/// </summary>
//		/// <param name="key">resource key</param>
//		/// <param name="formatArgs">format arguments</param>
//		/// <returns>a formated string</returns>
//		public string FormatMessage( string key, params object[] formatArgs )	{
//			return String.Format( System.Globalization.CultureInfo.CurrentUICulture, this[key], formatArgs );  
//		}

		#endregion
	}
}
