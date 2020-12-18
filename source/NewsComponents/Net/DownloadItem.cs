#region CVS Version Header

/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */

#endregion

using System;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Permissions;
using NewsComponents.Feed;

namespace NewsComponents.Net
{
    /// <summary>
    /// This class mantains all the information needed to describe
    /// a downloadable item.
    /// </summary>
    [Serializable]
    public sealed class DownloadItem : ISerializable
    {
        #region Private fields

        /// <summary>
        /// Information needed to download the file such as credentials, proxy information, etc
        /// </summary>
        private IDownloadInfoProvider downloadInfo;


        /// <summary>
        /// This represents the local file and also contains information about where it was downloaded from. 
        /// </summary>
        private readonly DownloadFile file;

        /// <summary>
        /// The enclosure that is being downloaded. 
        /// </summary>
        private readonly Enclosure enclosure;

        /// <summary>
        /// The ID for a DownloadItem owner, such as a feed.
        /// </summary>
        private readonly string ownerFeedId;

        /// <summary>
        /// Additional ID of a DownloadItem owner, such as a feed item.
        /// </summary>
        private readonly string ownerItemId;

        /// <summary>
        /// The download item id.
        /// </summary>
        private Guid downloadItemId = Guid.Empty;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a DownloadItem using the owner ID of the creating instance.
        /// </summary>
        /// <param name="ownerFeedId">The download owner ID.</param>
        /// <param name="ownerItemId">The download item ID</param>
        /// <param name="enclosure">Information about the item to download</param>
        /// <param name="downloadInfo">Information needed to download the files that is independent of the file</param>
        public DownloadItem(string ownerFeedId, string ownerItemId, Enclosure enclosure,
                            IDownloadInfoProvider downloadInfo)
        {
            this.ownerFeedId = ownerFeedId;
            this.ownerItemId = ownerItemId;
            this.enclosure = enclosure;
            file = new DownloadFile(enclosure);
            this.downloadInfo = downloadInfo;
        }

        #endregion

        #region DownloadItem Members

        /// <summary>
        /// The DownloadItem ID.
        /// </summary>
        public Guid ItemId
        {
            get
            {
                if (downloadItemId == Guid.Empty)
                {
                    downloadItemId = Guid.NewGuid();
                }
                return downloadItemId;
            }
        }

        /// <summary>
        /// The owner of the download item.
        /// </summary>
        public string OwnerFeedId
        {
            get { return ownerFeedId; }
        }


        /// <summary>
        /// The feed that this item belongs to. 
        /// </summary>
        public INewsFeed OwnerFeed { get; set; }

        /// <summary>
        /// The owner item of the download item.
        /// </summary>
        public string OwnerItemId
        {
            get { return ownerItemId; }
        }

        /// <summary>
        /// The target folder to place the downloaded file
        /// </summary>
        public string TargetFolder
        {
            get 
            {
                if (downloadInfo != null)
                    return downloadInfo.GetTargetFolder(this);
                else
                    return FeedSource.EnclosureFolder;
            }
        }


        /// <summary>
        /// The enclosure being downloaded. 
        /// </summary>
        public Enclosure Enclosure
        {
            get { return enclosure; }
        }


        /// <summary>
        /// This represents the local file and also contains information about where it was downloaded from. 
        /// </summary>
        public DownloadFile File
        {
            get { return file; }
        }

        /// <summary>
        /// The credentials needed to download the file.
        /// </summary>
        public ICredentials Credentials
        {
            get { return downloadInfo.GetCredentials(this); }
        }

        /// <summary>
        /// The proxy information
        /// </summary>
        public IWebProxy Proxy
        {
            get { return downloadInfo.Proxy; }
        }

        #endregion

        #region ISerializable Members

        /// <summary>
        /// Constructor used by the serialization infrastructure.
        /// </summary>
        /// <param name="info">The serialization information.</param>
        /// <param name="context">The serialization context.</param>
        private DownloadItem(SerializationInfo info, StreamingContext context)
        {
            downloadItemId = (Guid) info.GetValue("_id", typeof (Guid));
            ownerItemId = info.GetString("_itemId");
            ownerFeedId = info.GetString("_ownerId");
            enclosure = new Enclosure(info.GetString("_mimetype"), info.GetInt64("_length"), info.GetString("_url"),
                                      info.GetString("_description"));
            file = new DownloadFile(enclosure);

        }

        /// <summary>
        /// Method used by the serialization infrastructure.
        /// </summary>
        /// <param name="info">The serialization information.</param>
        /// <param name="context">The serialization context.</param>
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_id", downloadItemId);
            info.AddValue("_itemId", OwnerItemId);
            info.AddValue("_ownerId", OwnerFeedId);
            info.AddValue("_url", enclosure.Url);
            info.AddValue("_mimetype", enclosure.MimeType);
            info.AddValue("_length", enclosure.Length);
            info.AddValue("_description", enclosure.Description);

        }

        #endregion

        #region Public methods 

        /// <summary>
        /// Initializes the IDownloadInfoProvider for this object. This is needed if this DownloadItem is deserialized from disk. 
        /// </summary>
        /// <param name="downloadInfo"></param>
        public void Init(IDownloadInfoProvider downloadInfo)
        {
            this.downloadInfo = downloadInfo;
        }

        #endregion
    }
}
