#region CVS Version Header

/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */

#endregion

using System;
using System.IO;
using System.Net;
using log4net;
using NewsComponents.Utils;
using RssBandit.Common.Logging;

namespace NewsComponents.Net
{
    /// <summary>
    /// This downloader uses HTTP to download files.
    /// </summary>
    public sealed class HttpDownloader : IDownloader, IDisposable
    {
        #region private members

        private static readonly ILog Logger = DefaultLog.GetLogger(typeof (HttpDownloader));

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

        #endregion

        #region IDownloader implementation

        #region Downloader events

        /// <summary>
        /// Notifies about the download progress for the update.
        /// </summary>
        public event EventHandler<DownloadTaskProgressEventArgs> DownloadProgress;

        /// <summary>
        /// Notifies that the downloading for an DownloadTask has started.
        /// </summary>
        public event EventHandler<TaskEventArgs> DownloadStarted;

        /// <summary>
        /// Notifies that the downloading for an DownloadTask has finished.
        /// </summary>
        public event EventHandler<TaskEventArgs> DownloadCompleted;

        /// <summary>
        /// Notifies that an error ocurred while downloading the files for an DownloadTask.
        /// </summary>
        public event EventHandler<DownloadTaskErrorEventArgs> DownloadError;

        /// <summary>
        /// Helper method to fire the event.
        /// </summary>
        /// <param name="e">The event information.</param>
        private void OnDownloadStarted(TaskEventArgs e)
        {
            if (DownloadStarted != null)
            {
                DownloadStarted(this, e);
            }
        }

        /// <summary>
        /// Helper method to fire the event.
        /// </summary>
        /// <param name="e">The event information.</param>
        private void OnDownloadProgress(DownloadTaskProgressEventArgs e)
        {
            if (DownloadProgress != null)
            {
                DownloadProgress(this, e);
            }
        }

        /// <summary>
        /// Helper method to fire the event.
        /// </summary>
        /// <param name="e">The event information.</param>
        private void OnDownloadCompleted(TaskEventArgs e)
        {
            if (DownloadCompleted != null)
            {
                DownloadCompleted(this, e);
            }
        }

        /// <summary>
        /// Helper method to fire the event.
        /// </summary>
        /// <param name="e">The event information.</param>
        private void OnDownloadError(DownloadTaskErrorEventArgs e)
        {
            if (DownloadError != null)
            {
                DownloadError(this, e);
            }
        }

        #endregion

        /// <summary>
        /// Synchronous download method implementation.
        /// </summary>
        /// <param name="task">The DownloadTask to process.</param>
        /// <param name="maxWaitTime">The maximum wait time.</param>
        public void Download(DownloadTask task, TimeSpan maxWaitTime)
        {
            currentTask = task;

            WebResponse response = AsyncWebRequest.GetSyncResponse(HttpMethod.GET, task.DownloadItem.Enclosure.Url,
                                                                   task.DownloadItem.Credentials,
                                                                   FeedSource.UserAgentString(String.Empty),
                                                                   task.DownloadItem.Proxy,
                                                                   DateTime.MinValue,
                                                                   null /* eTag */,
                                                                   Convert.ToInt32(maxWaitTime.TotalSeconds), 
                                                                   null /* cookie */, null /* body */, null /* newsGatorAPIToken */);

            OnRequestComplete(new Uri(task.DownloadItem.Enclosure.Url), response.GetResponseStream(), response, null, null,
                              DateTime.MinValue, RequestResult.OK, 0);
        }


        /// <summary>
        /// Asynchronous download method implementation.
        /// </summary>
        /// <param name="task">The DownloadTask to process.</param>
        public void BeginDownload(DownloadTask task)
        {
            currentTask = task;

            Uri reqUri = new Uri(task.DownloadItem.Enclosure.Url);
            int priority = 10;

            RequestParameter reqParam = RequestParameter.Create(reqUri, FeedSource.UserAgentString(String.Empty),
                                                                task.DownloadItem.Proxy, task.DownloadItem.Credentials,
                                                                DateTime.MinValue, null);
            // global cookie handling:
            reqParam.SetCookies = FeedSource.SetCookies;


            state = BackgroundDownloadManager.AsyncWebRequest.QueueRequest(reqParam,
                                                                           OnRequestStart,
                                                                           OnRequestComplete,
                                                                           OnRequestException,
                                                                           new RequestProgressCallback(OnRequestProgress),
                                                                           priority);
        }


        /// <summary>
        /// Terminates or cancels an unfinished asynchronous download.
        /// </summary>
        /// <param name="task">The associated <see cref="DownloadTask"/> that holds a reference to the manifest to process</param>
        /// <returns>Returns true if the task was cancelled.</returns>
        public bool CancelDownload(DownloadTask task)
        {
            currentTask = task;
            Uri requestUri = new Uri(task.DownloadItem.Enclosure.Url);

            if (state != null && state.InitialRequestUri.Equals(requestUri))
            {
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
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// used by externally visible overload.
        /// </summary>
        /// <param name="isDisposing">whether or not to clean up managed + unmanaged/large (true) or just unmanaged(false)</param>
        private void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (currentTask.State == DownloadTaskState.Downloading || currentTask.State == DownloadTaskState.Pending)
                {
                    try
                    {
                        CancelDownload(currentTask);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.Message, e);
                    }
                }
            }
        }

        /// <summary>
        /// Destructor/Finalizer
        /// </summary>
        ~HttpDownloader()
        {
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
        public void OnRequestStart(Uri requestUri, ref bool cancel)
        {
            OnDownloadStarted(new TaskEventArgs(currentTask));
        }

        /// <summary>
        /// Called, if the web request caused an exception, that is not yet handled by the class itself.
        /// </summary>
        public void OnRequestException(Uri requestUri, Exception e, int priority)
        {
            OnDownloadError(new DownloadTaskErrorEventArgs(currentTask, e));
        }


        /// <summary>
        /// Called on every queued request, when the real fetch is finished.
        /// </summary>
        public void OnRequestComplete(Uri requestUri, Stream responseStream, WebResponse response, Uri newUri, string eTag, DateTime lastModified,
                                      RequestResult result, int priority)
        {
            string fileLocation = Path.Combine(currentTask.DownloadFilesBase, currentTask.DownloadItem.File.LocalName);

            //write file to disk from memory stream
            FileHelper.WriteStreamWithRename(fileLocation, responseStream);
            responseStream.Close();

            OnDownloadCompleted(new TaskEventArgs(currentTask));
        }

        /// <summary>
        /// Called infrequently as bytes are transferred for the file. 
        /// </summary>
        public void OnRequestProgress(Uri requestUri, long bytesTransferred)
        {
            long size;

            if (currentTask.DownloadItem.File.FileSize > currentTask.DownloadItem.Enclosure.Length)
            {
                size = currentTask.DownloadItem.File.FileSize;
            }
            else
            {
                size = currentTask.DownloadItem.Enclosure.Length;
            }

            OnDownloadProgress(new DownloadTaskProgressEventArgs(size, bytesTransferred, 1, 0, currentTask));
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