#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;

namespace NewsComponents.Net
{
	/// <summary>
	/// The enumeration of DownloadTask states.
	/// </summary>
	public enum DownloadTaskState {
		/// <summary>
		/// No state specified.
		/// </summary>
		None,

		/// <summary>
		/// Downloader is starting the download process.
		/// </summary>
		Downloading,

		/// <summary>
		/// Downloader has completed the download.
		/// </summary>
		Downloaded,

		/// <summary>
		/// A download error occurred.
		/// </summary>
		DownloadError,

		/// <summary>
		/// A new download was cancelled.
		/// </summary>
		Cancelled,

        /// <summary>
        /// Waiting to download
        /// </summary>
        Pending
	}
}
