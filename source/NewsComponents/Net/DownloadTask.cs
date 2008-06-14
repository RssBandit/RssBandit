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
using System.Runtime.Serialization;
using System.Security.Permissions;
using NewsComponents.Core;
using NewsComponents.Utils;

namespace NewsComponents.Net
{
    /// <summary>
    /// Holds the state information about an download in progress.
    /// </summary>
    [Serializable]
    public class DownloadTask : BindableObject, ISerializable
    {
        #region Private fields

        /// <summary>
        /// The task Id.
        /// </summary>
        private readonly Guid id;

        private readonly DateTime _createDate;

        /// <summary>
        /// The status of the download task.
        /// </summary>
        private DownloadTaskState state = DownloadTaskState.None;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instance of the updater task using the specified item.
        /// </summary>
        /// <param name="item">The item corresponding to an updater task.</param>
        /// <param name="info">IDownloadInfo</param>
        public DownloadTask(DownloadItem item, IDownloadInfoProvider info)
        {
            id = Guid.NewGuid();
            _createDate = DateTime.Now;

            Init(item, info);
        }

        #endregion

        #region DownloadTask Members


        /// <summary>
        /// The base folder where files will be downloaded.
        /// </summary>
        public string DownloadFilesBase { get; private set; }

        /// <summary>
        /// Initializes an existing DownloadTask with the specified item.
        /// </summary>
        /// <param name="item">The item corresponding to an updater task.</param>
        /// <param name="info">IDownloadInfoProvider</param>
        public void Init(DownloadItem item, IDownloadInfoProvider info)
        {
            DownloadItem = item;
            if (DownloadItem != null && info != null)
            {
                DownloadFilesBase = info.InitialDownloadLocation;
                DownloadItem.Init(info);
            }
        }

        /// <summary>
        /// An unique identifier for the DownloadTask used internally
        /// </summary>
        public Guid TaskId
        {
            get { return id; }
        }

        private Guid? _jobId;

        /// <summary>
        /// An external Id that can be used for tracking
        /// </summary>
        public Guid? JobId
        {
            get { return _jobId; }
            set
            {
                _jobId = value;
                RaisePropertyChanged("JobId");
            }
        }

        /// <summary>
        /// The current state of the DownloadTask.
        /// <see cref="DownloadTaskState"/>
        /// </summary>
        public DownloadTaskState State
        {
            get { return state; }
            set
            {
                state = value;
                RaisePropertyChanged("State");
            }
        }

        public DateTime CreatedDate
        {
            get
            {
                return _createDate;
            }
        }

        public string FileName
        {
            get
            {
                return Path.Combine(DownloadFilesBase, DownloadItem.File.LocalName);
            }
        }

        private long _fileSize;

        public long FileSize
        {
            get { return _fileSize; }
            internal set
            {
                _fileSize = value;
                RaisePropertyChanged("FileSize");

                CalculatePercentComplete();
            }
        }

        private long _transferredSize;

        public long TransferredSize
        {
            get { return _transferredSize; }
            internal set
            {
                _transferredSize = value;
                RaisePropertyChanged("TransferredSize");

                CalculatePercentComplete();
            }
        }

        public double PercentComplete
        {
            get;
            private set;
        }

        private void CalculatePercentComplete()
        {
            if(FileSize > 0)
            {
                PercentComplete = (TransferredSize / (double)FileSize) * 100;
            }
            else
            {
                PercentComplete = 0;
            }

            RaisePropertyChanged("PercentComplete");
        }

        public bool CanCancelResume
        {
            get
            {
                return _downloader != null;
            }
            
        }

        public void Cancel()
        {
            if(CanCancelResume)
                Downloader.CancelDownload(this);
        }

        /// <summary>
        /// The item corresponding to the current DownloadTask.
        /// </summary>
        public DownloadItem DownloadItem { get; private set; }

        private IDownloader _downloader;

        /// <summary>
        /// The IDownloader instance responsible for downloading this task
        /// </summary>
        internal IDownloader Downloader
        {
            get { return _downloader; }
            set
            {
                _downloader = value;
                RaisePropertyChanged("CanCancelResume");
                RaisePropertyChanged("Downloader");
            }
        }

        #endregion

        #region ISerializable Members

        /// <summary>
        /// Constructor to support serialization required for storing the task.
        /// </summary>
        /// <param name="info">The serialization information.</param>
        /// <param name="context">The serialization context.</param>
        [SecurityPermission(SecurityAction.LinkDemand)]
        protected DownloadTask(SerializationInfo info, StreamingContext context)
        {
            var reader = new SerializationInfoReader(info, context);
            DownloadItem = (DownloadItem) reader.GetValue("_manifest", typeof (DownloadItem), null);
            state = (DownloadTaskState) reader.GetValue("_state", typeof (DownloadTaskState), DownloadTaskState.None);
            id = (Guid) reader.GetValue("_id", typeof (Guid), Guid.Empty);
            JobId = (Guid?) reader.GetValue("_jobId", typeof (Guid?), null);
            TransferredSize = (long) reader.GetValue("_transferSize", typeof(long), 0);
            FileSize = (long)reader.GetValue("_fileSize", typeof(long), 0);
            _createDate = TimeZoneInfo.ConvertTime((DateTime)reader.GetValue("_createDate", typeof(DateTime), DateTime.Now), TimeZoneInfo.Local);

            if (reader.Contains("_downloadFilesBase"))
                DownloadFilesBase = reader.GetString("_downloadFilesBase", null);
            else /* there used to be a typo in the field name */
                DownloadFilesBase = reader.GetString("_donwnloadFilesBase", null);
        }

        /// <summary>
        /// Method used by the seralization mechanism to retrieve the serialized information.
        /// </summary>
        /// <param name="info">The serialization information.</param>
        /// <param name="context">The serialization context.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_manifest", DownloadItem);
            info.AddValue("_state", state);
            info.AddValue("_id", id);
            info.AddValue("_downloadFilesBase", DownloadFilesBase);
            info.AddValue("_jobId", JobId);
            info.AddValue("_transferSize", TransferredSize);
            info.AddValue("_fileSize", FileSize);
            info.AddValue("_createDate", TimeZoneInfo.ConvertTimeToUtc(_createDate));
        }

        #endregion
    }
}