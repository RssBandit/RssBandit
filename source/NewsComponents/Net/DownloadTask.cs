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
        private readonly Guid _id;

        private readonly DateTime _createDate;

        private string _fileName;

        /// <summary>
        /// The status of the download task.
        /// </summary>
        private DownloadTaskState _state = DownloadTaskState.Pending;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instance of the updater task using the specified item.
        /// </summary>
        /// <param name="item">The item corresponding to an updater task.</param>
        /// <param name="info">IDownloadInfo</param>
        public DownloadTask(DownloadItem item, IDownloadInfoProvider info)
        {
            _id = Guid.NewGuid();
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

                // Set a default name
                if(FileName == null)
                    FileName = Path.Combine(DownloadItem.TargetFolder, DownloadItem.File.LocalName);
            }
        }

        /// <summary>
        /// An unique identifier for the DownloadTask used internally
        /// </summary>
        public Guid TaskId
        {
            get { return _id; }
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
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The current state of the DownloadTask.
        /// <see cref="DownloadTaskState"/>
        /// </summary>
        public DownloadTaskState State
        {
            get { return _state; }
            set
            {
                _state = value;
                RaisePropertyChanged();
            }
        }

        private string _errorText;
		/// <summary>
		/// Gets or sets the error text.
		/// </summary>
		/// <value>The error text.</value>
        public string ErrorText
        {
            get { return _errorText; }

            set
            {
                _errorText = value;
                RaisePropertyChanged();
            }
        }

		/// <summary>
		/// Gets the created date.
		/// </summary>
		/// <value>The created date.</value>
        public DateTime CreatedDate
        {
            get
            {
                return _createDate;
            }
        }

		/// <summary>
		/// Gets or sets the name of the file.
		/// </summary>
		/// <value>The name of the file.</value>
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
                RaisePropertyChanged();
            }
        }

        private long _fileSize;

		/// <summary>
		/// Gets the size of the file.
		/// </summary>
		/// <value>The size of the file.</value>
        public long FileSize
        {
            get { return _fileSize; }
            internal set
            {
                _fileSize = value;
                RaisePropertyChanged();

                CalculatePercentComplete();
            }
        }

        private long _transferredSize;

		/// <summary>
		/// Gets the size of the transferred.
		/// </summary>
		/// <value>The size of the transferred.</value>
        public long TransferredSize
        {
            get { return _transferredSize; }
            internal set
            {
                _transferredSize = value;
                RaisePropertyChanged();

                CalculatePercentComplete();
            }
        }

		/// <summary>
		/// Gets the percent complete.
		/// </summary>
		/// <value>The percent complete.</value>
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

            RaisePropertyChanged();
        }

	    private int _downloadErrorResumeCount;
		/// <summary>
		/// Gets or sets the download error resume count.
		/// </summary>
		/// <value>The download error resume count.</value>
	    public int DownloadErrorResumeCount
	    {
		    get { return _downloadErrorResumeCount; }
		    set
		    {
			    _downloadErrorResumeCount = value;
			    RaisePropertyChanged();
		    }
	    }

		/// <summary>
		/// Gets a value indicating whether this instance can cancel resume.
		/// </summary>
		/// <value><c>true</c> if this instance can cancel resume; otherwise, <c>false</c>.</value>
        public bool CanCancelResume
        {
            get
            {
                return _downloader != null;
            }
            
        }

		/// <summary>
		/// Cancels this downloading instance.
		/// </summary>
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
                RaisePropertyChanged(nameof(CanCancelResume));
                RaisePropertyChanged();
            }
        }


        private bool _supportsBITS; 

        /// <summary>
        /// Indicates whether the downloader for this task supports BITS or not. 
        /// </summary>
        public bool SupportsBITS {
            get { return _supportsBITS;  } 
            set { _supportsBITS = value; }
        }

        #endregion

        #region ISerializable Members

        /// <summary>
        /// Constructor to support serialization required for storing the task.
        /// </summary>
        /// <param name="info">The serialization information.</param>
        /// <param name="context">The serialization context.</param>
        protected DownloadTask(SerializationInfo info, StreamingContext context)
        {
            var reader = new SerializationInfoReader(info, context);
            DownloadItem =  reader.Get("_manifest", (DownloadItem)null);
            _state = reader.Get("_state", DownloadTaskState.None);
            _id = reader.Get("_id",  Guid.Empty);
            JobId = reader.Get("_jobId",(Guid?) null);
            TransferredSize = reader.Get("_transferSize", 0);
            FileSize = reader.Get("_fileSize", 0);
            _createDate = TimeZoneInfo.ConvertTime(reader.Get("_createDate", DateTime.Now), TimeZoneInfo.Local);
            _fileName = reader.Get("_fileName", (string)null);
            _errorText = reader.Get("_errorText", (string) null);
            _supportsBITS = reader.Get("_supportsBITS", false); 

            if (reader.Contains("_downloadFilesBase"))
                DownloadFilesBase = reader.Get("_downloadFilesBase", (string)null);
            else /* there used to be a typo in the field name */
                DownloadFilesBase = reader.Get("_donwnloadFilesBase", (string)null);
	        if (reader.Contains("_downloadErrorResumeCount"))
		        _downloadErrorResumeCount = reader.Get("_downloadErrorResumeCount", 0);
        }

        /// <summary>
        /// Method used by the serialization mechanism to retrieve the serialized information.
        /// </summary>
        /// <param name="info">The serialization information.</param>
        /// <param name="context">The serialization context.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_manifest", DownloadItem);
            info.AddValue("_state", _state.ToString());
            info.AddValue("_id", _id);
            info.AddValue("_downloadFilesBase", DownloadFilesBase);
            info.AddValue("_jobId", JobId);
            info.AddValue("_transferSize", TransferredSize);
            info.AddValue("_fileSize", FileSize);
            info.AddValue("_createDate", TimeZoneInfo.ConvertTimeToUtc(_createDate));
            info.AddValue("_fileName", _fileName);
            info.AddValue("_errorText", _errorText);
            info.AddValue("_supportsBITS", _supportsBITS);
			info.AddValue("_downloadErrorResumeCount", DownloadErrorResumeCount);
        }

        #endregion
    }
}
