#region CVS Version Header
/*
 * $Id: DownloadTaskState.cs,v 1.1 2006/10/17 15:23:26 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2006/10/17 15:23:26 $
 * $Revision: 1.1 $
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
		Cancelled
	}
}
