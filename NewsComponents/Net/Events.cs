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

    #region TaskEventArgs class

    /// <summary>
    /// Used to notify information about an UpdaterTask.
    /// </summary>
    public class TaskEventArgs : EventArgs
    {
        private readonly DownloadTask currentDownloadTask;

        /// <summary>
        /// Constructor for a TaskEventArgs.
        /// </summary>
        /// <param name="task">The DownloadTask for initialization.</param>
        public TaskEventArgs(DownloadTask task)
        {
            currentDownloadTask = task;
        }

        /// <summary>
        /// Returns the updater task.
        /// </summary>
        public DownloadTask Task
        {
            get { return currentDownloadTask; }
        }
    }

    #endregion

    #region BaseDownloadProgressEventArgs class

    /// <summary>
    /// Base class used to provide information about the download progress.
    /// </summary>
    public class BaseDownloadProgressEventArgs : EventArgs
    {
        /// <summary>
        /// The total bystes that will be transfered.
        /// </summary>
        private readonly long bytesTotal;

        /// <summary>
        /// The amount of bytes that have been transfered.
        /// </summary>
        private readonly long bytesTransferred;

        /// <summary>
        /// The total files that will be transfered.
        /// </summary>
        private readonly int filesTotal;

        /// <summary>
        /// The files that have been transfered.
        /// </summary>
        private readonly int filesTransferred;

        /// <summary>
        /// Indicates whether the operation was canceled.
        /// </summary>
        private bool cancel;

        /// <summary>
        /// Constructor for the BaseDownloadProgressEventArgs.
        /// </summary>
        /// <param name="bytesTotal">The total bytes to be transferred.</param>
        /// <param name="bytesTransferred">Number of bytes that have been transferred.</param>
        /// <param name="filesTotal">The total number of files to be transferred.</param>
        /// <param name="filesTransferred">Number of files that have been transferred.</param>
        public BaseDownloadProgressEventArgs(long bytesTotal, long bytesTransferred, int filesTotal,
                                             int filesTransferred)
        {
            this.bytesTotal = bytesTotal;
            this.bytesTransferred = bytesTransferred;
            this.filesTotal = filesTotal;
            this.filesTransferred = filesTransferred;
        }

        /// <summary>
        /// The total bytes to be transferred.
        /// </summary>
        public long BytesTotal
        {
            get { return bytesTotal; }
        }

        /// <summary>
        /// Number of bytes that have been transferred.
        /// </summary>
        public long BytesTransferred
        {
            get { return bytesTransferred; }
        }

        /// <summary>
        /// The total number of files to be transferred.
        /// </summary>
        public int FilesTotal
        {
            get { return filesTotal; }
        }

        /// <summary>
        /// Number of files that have been transferred.
        /// </summary>
        public int FilesTransferred
        {
            get { return filesTransferred; }
        }

        /// <summary>
        /// Indicates whether the operation was cancelled or not.
        /// </summary>
        public bool Cancel
        {
            get { return cancel; }
            set { cancel = cancel || value; }
        }
    }

    #endregion

    #region DownloadTaskProgressEventArgs class

    /// <summary>
    /// Used to notify events about download progess.
    /// </summary>
    public class DownloadTaskProgressEventArgs : BaseDownloadProgressEventArgs
    {
        /// <summary>
        /// The updater task.
        /// </summary>
        private readonly DownloadTask task;

        /// <summary>
        /// Constructor for the DownloadTaskProgressEventArgs.
        /// </summary>
        /// <param name="bytesTotal">The total bytes to be transferred.</param>
        /// <param name="bytesTransferred">Number of bytes that have been transferred.</param>
        /// <param name="filesTotal">The total number of files to be transferred.</param>
        /// <param name="filesTransferred">Number of files that have been transferred.</param>
        /// <param name="task">The <see cref="DownloadTask"/> instance.</param>
        public DownloadTaskProgressEventArgs(long bytesTotal, long bytesTransferred, int filesTotal,
                                             int filesTransferred,
                                             DownloadTask task)
            : base(bytesTotal, bytesTransferred, filesTotal, filesTransferred)
        {
            this.task = task;
        }

        /// <summary>
        /// Returns the DownloadTask.
        /// </summary>
        public DownloadTask Task
        {
            get { return task; }
        }
    }

    #endregion

    #region DownloadTaskErrorEventArgs class

    /// <summary>
    /// Used to provide information about download errors.
    /// </summary>
    public class DownloadTaskErrorEventArgs : TaskEventArgs
    {
        /// <summary>
        /// The exception received.
        /// </summary>
        private readonly Exception exception;

        /// <summary>
        /// Constructor for the DownloadTaskErrorEventArgs.
        /// </summary>
        /// <param name="task">The <see cref="DownloadTask"/> instance.</param>
        /// <param name="exception">The exception information.</param>
        public DownloadTaskErrorEventArgs(DownloadTask task, Exception exception) : base(task)
        {
            this.exception = exception;
        }

        /// <summary>
        /// The exception received.
        /// </summary>
        public Exception Exception
        {
            get { return exception; }
        }
    }

    #endregion

    #region EventArgs classes

    /// <summary>
    /// Used to provide event information about the manifest.
    /// </summary>
    public class DownloadItemEventArgs : EventArgs
    {
        /// <summary>
        /// The manifest reference.
        /// </summary>
        private readonly DownloadItem manifestInEventsArgs;

        /// <summary>
        /// Constructor for a ManifestEventArgs.
        /// </summary>
        /// <param name="manifest">The <see cref="DownloadItem"/> reference.</param>
        public DownloadItemEventArgs(DownloadItem manifest)
        {
            manifestInEventsArgs = manifest;
        }

        /// <summary>
        /// Returns the manifest reference.
        /// </summary>
        public DownloadItem DownloadItem
        {
            get { return manifestInEventsArgs; }
        }
    }

    /// <summary>
    /// Used to provide information about manifest download errors.
    /// </summary>
    public class DownloadItemErrorEventArgs : DownloadItemEventArgs
    {
        /// <summary>
        /// The exception detected.
        /// </summary>
        private readonly Exception exceptionContainedInManifestErrorEventArgs;

        /// <summary>
        /// Constructor for the ManifestErrorEventArgs.
        /// </summary>
        /// <param name="manifest">The <see cref="DownloadItem"/> reference.</param>
        /// <param name="exception">The exception information.</param>
        public DownloadItemErrorEventArgs(DownloadItem manifest, Exception exception) : base(manifest)
        {
            exceptionContainedInManifestErrorEventArgs = exception;
        }

        /// <summary>
        /// The thrown exception.
        /// </summary>
        public Exception Exception
        {
            get { return exceptionContainedInManifestErrorEventArgs; }
        }
    }

    /// <summary>
    /// Used to provide information about the download progress.
    /// </summary>
    public class DownloadProgressEventArgs : BaseDownloadProgressEventArgs
    {
        /// <summary>
        /// The manifest instance.
        /// </summary>
        private readonly DownloadItem downloadItem;

        /// <summary>
        /// Constructor for the DownloadProgressEventArgs.
        /// </summary>
        /// <param name="bytesTotal">The total bytes to be transferred.</param>
        /// <param name="bytesTransferred">Number of bytes that have been transferred.</param>
        /// <param name="filesTotal">The total number of files to be transferred.</param>
        /// <param name="filesTransferred">Number of files that have been transferred.</param>
        /// <param name="item">The <see cref="DownloadItem"/> instance.</param>
        public DownloadProgressEventArgs(long bytesTotal, long bytesTransferred, int filesTotal, int filesTransferred,
                                         DownloadItem item)
            : base(bytesTotal, bytesTransferred, filesTotal, filesTransferred)
        {
            downloadItem = item;
        }

        /// <summary>
        /// The DownloadItem instance.
        /// </summary>
        public DownloadItem DownloadItem
        {
            get { return downloadItem; }
        }
    }

    /// <summary>
    /// Used to provide information about download started.
    /// </summary>
    public class DownloadStartedEventArgs : DownloadItemEventArgs
    {
        /// <summary>
        /// Indicates whether the download has been canceled.
        /// </summary>
        private bool cancel;

        /// <summary>
        /// Constructor for DownloadStartedEventArgs.
        /// </summary>
        /// <param name="item">The <see cref="DownloadItem"/> instance.</param>
        public DownloadStartedEventArgs(DownloadItem item) : base(item)
        {
            cancel = false;
        }

        /// <summary>
        /// Indicates whether the download has been canceled.
        /// </summary>
        public bool Cancel
        {
            get { return cancel; }
            set { cancel = cancel || value; }
        }
    }

    #endregion
}