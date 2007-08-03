#region CVS Version Header
/*
 * $Id: DownloadItem.cs,v 1.3 2006/12/19 04:39:52 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2006/12/19 04:39:52 $
 * $Revision: 1.3 $
 */
#endregion

using NewsComponents.Feed;
using System;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace NewsComponents.Net
{
	/// <summary>
	/// This class mantains all the information needed to describe
	/// a downloadable item.
	/// </summary>
	[Serializable]
	public class DownloadItem : ISerializable {
		#region Private fields
		

		/// <summary>
		/// Information needed to download the file such as credentials, proxy information, etc
		/// </summary>
		private IDownloadInfoProvider downloadInfo; 


		/// <summary>
		/// This represents the local file and also contains information about where it was downloaded from. 
		/// </summary>
		private DownloadFile file; 

		/// <summary>
		/// The enclosure that is being downloaded. 
		/// </summary>
		private Enclosure enclosure; 

		/// <summary>
		/// The ID for a DownloadItem owner, such as a feed.
		/// </summary>
		private string ownerFeedId;

		/// <summary>
		/// Additional ID of a DownloadItem owner, such as a feed item.
		/// </summary>
		private string ownerItemId;

		/// <summary>
		/// The download item id.
		/// </summary>
		private Guid downloadItemId = Guid.Empty;		
	
		/// <summary>
		/// The feed this object is associated with. 
		/// </summary>
		private feedsFeed ownerFeed;
		
		#endregion

		#region Public Constructors
		

		/// <summary>
		/// Creates a DownloadItem using the owner ID of the creating instance.
		/// </summary>
		/// <param name="ownerFeedId">The download owner ID.</param>
		/// <param name="ownerItemId">The download item ID</param>
		/// <param name="enclosure">Information about the item to download</param>
		/// <param name="downloadInfo">Information needed to download the files that is independent of the file</param>
		public DownloadItem( string ownerFeedId, string ownerItemId, Enclosure enclosure, IDownloadInfoProvider downloadInfo ) {
			this.ownerFeedId = ownerFeedId;
			this.ownerItemId = ownerItemId;
			this.enclosure = enclosure; 
			this.file      = new DownloadFile(enclosure); 
			this.downloadInfo = downloadInfo; 
		}

		#endregion

		#region DownloadItem Members

		/// <summary>
		/// The DownloadItem ID.
		/// </summary>
		public Guid ItemId {
			get {
				if ( downloadItemId == Guid.Empty ) {
					downloadItemId = Guid.NewGuid();
				}
				return downloadItemId;
			}
		} 

		/// <summary>
		/// The owner of the download item.
		/// </summary>
		public string OwnerFeedId {
			get {
				return this.ownerFeedId;
			}
		}

	
		/// <summary>
		/// The feed that this item belongs to. 
		/// </summary>
		public feedsFeed OwnerFeed{
			get {
				return this.ownerFeed;
			}
			set{ this.ownerFeed = value;}
		}

		/// <summary>
		/// The owner item of the download item.
		/// </summary>
		public string OwnerItemId {
			get {
				return this.ownerItemId;
			}
		}
		
		/// <summary>
		/// The target folder to place the downloaded file
		/// </summary>
		public string TargetFolder{
		get{
			return this.downloadInfo.GetTargetFolder(this);
			}
		}


		/// <summary>
		/// The enclosure being downloaded. 
		/// </summary>
		public Enclosure Enclosure{
			get{ 
				return this.enclosure; 
			}
		}

		
		/// <summary>
		/// This represents the local file and also contains information about where it was downloaded from. 
		/// </summary>
		public DownloadFile File{

			get{
				return this.file;
			}
		}

		/// <summary>
		/// The credentials needed to download the file.
		/// </summary>
		public ICredentials Credentials {
			get {
				return this.downloadInfo.GetCredentials(this); 
			}
		}
		/// <summary>
		/// The proxy information
		/// </summary>
		public IWebProxy Proxy{
			get {
				return this.downloadInfo.Proxy;
			}
		}		
		

		#endregion

		#region ISerializable Members

		/// <summary>
		/// Constructor used by the serialization infrastructure.
		/// </summary>
		/// <param name="info">The serialization information.</param>
		/// <param name="context">The serialization context.</param>
		[System.Security.Permissions.SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		protected DownloadItem(SerializationInfo info, StreamingContext context) {
			this.downloadItemId = (Guid)info.GetValue("_id", typeof(Guid));
			this.ownerItemId = info.GetString("_itemId");
			this.ownerFeedId = info.GetString("_ownerId");			
			this.enclosure   = new Enclosure(info.GetString("_mimetype"), info.GetInt64("_length"), info.GetString("_url"), info.GetString("_description")); 			
			this.file        = new DownloadFile(enclosure);
		}

		/// <summary>
		/// Method used by the serialization infrastructure.
		/// </summary>
		/// <param name="info">The serialization information.</param>
		/// <param name="context">The serialization context.</param>
		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter=true)]
		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue("_id", this.downloadItemId); 
			info.AddValue( "_itemId", this.OwnerItemId );
			info.AddValue("_ownerId", this.OwnerFeedId );
			info.AddValue("_url", this.enclosure.Url); 
			info.AddValue("_mimetype", this.enclosure.MimeType); 
			info.AddValue("_length", this.enclosure.Length); 
			info.AddValue("_description", this.enclosure.Description); 
		}

		#endregion

		#region Public methods 


		/// <summary>
		/// Initializes the IDownloadInfoProvider for this object. This is needed if this DownloadItem is deserialized from disk. 
		/// </summary>
		/// <param name="downloadInfo"></param>
		public void Init(IDownloadInfoProvider downloadInfo){
			this.downloadInfo = downloadInfo; 
		}

		#endregion 
	}

	

}
