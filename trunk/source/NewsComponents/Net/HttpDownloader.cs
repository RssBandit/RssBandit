#region CVS Version Header
/*
 * $Id: HttpDownloader.cs,v 1.3 2007/06/10 18:41:26 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2007/06/10 18:41:26 $
 * $Revision: 1.3 $
 */
#endregion

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;
using NewsComponents.Utils;



namespace NewsComponents.Net
{
	/// <summary>
	/// This downloader uses HTTP to download files.
	/// </summary>
	public sealed class HttpDownloader: IDownloader, IDisposable {

		#region private members

		private static readonly log4net.ILog Logger = RssBandit.Common.Logging.Log.GetLogger(typeof(HttpDownloader));

		/// <summary>
		/// An IDownloader is only associated with a single DownloadTask. 
		/// </summary>
		private DownloadTask currentTask = null; 


		/// <summary>
		/// The current state of the download task
		/// </summary>
		private RequestState state = null;

		#endregion


		#region Constructors

		public HttpDownloader() {
			//
			// TODO: Add constructor logic here
			//
		}

		#endregion

		#region IDownloader implementation

		#region Downloader events

		/// <summary>
		/// Notifies the application of the download progress. 
		/// </summary>
		public event DownloadTaskProgressEventHandler DownloadProgress;


		/// <summary>
		/// Notifies the application when the download is complete.
		/// </summary>
		public event DownloadTaskCompletedEventHandler DownloadCompleted;

		/// <summary>
		/// Notifies the application when there is a download error. 
		/// </summary>
		public event DownloadTaskErrorEventHandler DownloadError;


		/// <summary>
		/// Notifies the application that the download has started. 
		/// </summary>
		public event DownloadTaskStartedEventHandler DownloadStarted;

		/// <summary>
		/// Helper method to fire the event.
		/// </summary>
		/// <param name="e">The event information.</param>
		private void OnDownloadStarted( TaskEventArgs e ) {
			if ( DownloadStarted != null ) {
				DownloadStarted( this, e );
			}
		}

		/// <summary>
		/// Helper method to fire the event.
		/// </summary>
		/// <param name="e">The event information.</param>
		private void OnDownloadProgress( DownloadTaskProgressEventArgs e ) {
			if ( DownloadProgress != null ) {
				DownloadProgress( this, e );
			}
		}

		/// <summary>
		/// Helper method to fire the event.
		/// </summary>
		/// <param name="e">The event information.</param>
		private void OnDownloadCompleted( TaskEventArgs e ) {
			if ( DownloadCompleted != null ) {
				DownloadCompleted( this, e );
			}
		}

		/// <summary>
		/// Helper method to fire the event.
		/// </summary>
		/// <param name="e">The event information.</param>
		private void OnDownloadError( DownloadTaskErrorEventArgs e ) {
			if ( DownloadError != null ) {
				DownloadError( this, e );
			}
		}

		#endregion


		/// <summary>
		/// Synchronous download method implementation.
		/// </summary>
		/// <param name="task">The DownloadTask to process.</param>
		/// <param name="maxWaitTime">The maximum wait time.</param>
		public void Download(DownloadTask task, TimeSpan maxWaitTime) {	
			this.currentTask = task;

			WebResponse response = AsyncWebRequest.GetSyncResponse(task.DownloadItem.Enclosure.Url, 
																task.DownloadItem.Credentials, 
																NewsHandler.UserAgentString(String.Empty),
																task.DownloadItem.Proxy,
																DateTime.MinValue, 
																null, 
																Convert.ToInt32(maxWaitTime.TotalSeconds));

			this.OnRequestComplete(new Uri(task.DownloadItem.Enclosure.Url), response.GetResponseStream(), null, null, DateTime.MinValue, RequestResult.OK, 0); 
		}

		
		/// <summary>
		/// Asynchronous download method implementation.
		/// </summary>
		/// <param name="task">The DownloadTask to process.</param>
		public void BeginDownload(DownloadTask task) {

			this.currentTask = task; 

			Uri reqUri = new Uri(task.DownloadItem.Enclosure.Url);
			int priority = 10;

			RequestParameter reqParam = RequestParameter.Create(reqUri, NewsHandler.UserAgentString(String.Empty), 
				task.DownloadItem.Proxy, task.DownloadItem.Credentials, 
				DateTime.MinValue, null);
			// global cookie handling:
			reqParam.SetCookies = NewsHandler.SetCookies;

				
			this.state = BackgroundDownloadManager.AsyncWebRequest.QueueRequest(reqParam, 
				null /* new RequestQueuedCallback(this.OnRequestQueued) */, 
				new RequestStartCallback(this.OnRequestStart), 
				new RequestCompleteCallback(this.OnRequestComplete), 
				new RequestExceptionCallback(this.OnRequestException), 
				new RequestProgressCallback(this.OnRequestProgress),
				priority);
		
		}


		/// <summary>
		/// Terminates or cancels an unfinished asynchronous download.
		/// </summary>
		/// <param name="task">The associated <see cref="DownloadTask"/> that holds a reference to the manifest to process</param>
		/// <returns>Returns true if the task was cancelled.</returns>
		public bool CancelDownload(DownloadTask task){
		
			currentTask = task; 
			Uri requestUri = new Uri(task.DownloadItem.Enclosure.Url); 

			if(state.InitialRequestUri.Equals(requestUri)){			
				BackgroundDownloadManager.AsyncWebRequest.FinalizeWebRequest(state);				
			}

			return true;
		}

		#endregion

		#region IDisposable implementation
		
		//  take care of IDisposable too   
		/// <summary>
		/// Allows graceful cleanup of hard resources
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this); 
		}


		/// <summary>
		/// used by externally visible overload.
		/// </summary>
		/// <param name="isDisposing">whether or not to clean up managed + unmanaged/large (true) or just unmanaged(false)</param>
		private void Dispose(bool isDisposing) {	
			if(this.currentTask.State == DownloadTaskState.Downloading){
				try{
					this.CancelDownload(this.currentTask); 
				}catch(Exception e){
					Logger.Error(e.Message, e); 
				}
			}
		}

		/// <summary>
		/// Destructor/Finalizer
		/// </summary>
		~HttpDownloader() {
			// Simply call Dispose(false).
			Dispose(false);
		}

		#endregion

		#region Event Handling Methods 

		/// <summary>
		/// Notification that the download of the file has started. 
		/// </summary>
		/// <param name="requestUri"></param>
		/// <param name="cancel"></param>
		public void OnRequestStart(Uri requestUri, ref bool cancel){
			this.OnDownloadStarted(new TaskEventArgs(this.currentTask)); 
		}

		/// <summary>
		/// Called, if the web request caused an exception, that is not yet handled by the class itself.
		/// </summary>
		public void OnRequestException(Uri requestUri, Exception e, int priority){
			this.OnDownloadError(new DownloadTaskErrorEventArgs(this.currentTask, e));
		}


		/// <summary>
		/// Called on every queued request, when the real fetch is finished.
		/// </summary>
		public void OnRequestComplete(Uri requestUri, Stream response, Uri newUri, string eTag, DateTime lastModified, RequestResult result, int priority){
			
			string fileLocation = Path.Combine(this.currentTask.DownloadFilesBase, this.currentTask.DownloadItem.File.LocalName);
			
			//write file to disk from memory stream
			FileHelper.WriteStreamWithRename(fileLocation, response); 
			response.Close(); 
			
			this.OnDownloadCompleted(new TaskEventArgs(this.currentTask)); 
		}

		/// <summary>
		/// Called infrequently as bytes are transferred for the file. 
		/// </summary>
		public void OnRequestProgress(Uri requestUri, long bytesTransferred){
			
			long size = 0;
				
			if(currentTask.DownloadItem.File.FileSize > currentTask.DownloadItem.Enclosure.Length){
				size = currentTask.DownloadItem.File.FileSize;
			}else{
				size = currentTask.DownloadItem.Enclosure.Length;
			}

			this.OnDownloadProgress(new DownloadTaskProgressEventArgs(size, bytesTransferred, 1, 0, this.currentTask));
		}


		#endregion
	}
}

#region CVS Version Log
/*
 * $Log: HttpDownloader.cs,v $
 * Revision 1.3  2007/06/10 18:41:26  carnage4life
 * Fixed issues with HttpDownloader.Finalize() causing NullReferenceExceptions
 *
 * Revision 1.2  2006/12/19 17:00:39  t_rendelmann
 * added: CVS log sections
 *
 */
#endregion
