#region CVS Version Header
/*
 * $Id: DownloadTask.cs,v 1.3 2007/02/01 16:00:42 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2007/02/01 16:00:42 $
 * $Revision: 1.3 $
 */
#endregion

using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Security.Permissions;
using NewsComponents.Utils;

namespace NewsComponents.Net
{

	/// <summary>
	/// Holds the state information about an download in progress.
	/// </summary>
	[Serializable]
	public class DownloadTask : ISerializable {
		#region Private fields

		/// <summary>
		/// The task Id.
		/// </summary>
		private Guid id;

		/// <summary>
		/// The base folder where files will be downloaded.
		/// </summary>
		private string downloadFilesBase;

		/// <summary>
		/// The item that is being processed.
		/// </summary>
		private DownloadItem item;

		/// <summary>
		/// The task context where all the components can set information for later retrieval.
		/// </summary>
		private StateBag context;

		/// <summary>
		/// The status of the download task.
		/// </summary>
		private DownloadTaskState state = DownloadTaskState.None;

		/// <summary>
		/// An object that can be used to synchronize access to the DownloadTask.
		/// </summary>
		private object syncRoot = new object();

		/// <summary>
		/// The Downloader that is responsible for downloading this task.
		/// </summary>
		private IDownloader downloader = null;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates an instance of the updater task using the specified item.
		/// </summary>
		/// <param name="item">The item corresponding to an updater task.</param>
		/// <param name="info">IDownloadInfo</param>
		public DownloadTask( DownloadItem item, IDownloadInfoProvider info) {
			id = Guid.NewGuid();
			context = new StateBag();
			Init( item, info );
		}

		#endregion

		#region DownloadTask Members

		/// <summary>
		/// A property bag where components can set key-value pairs of information for later retrieval.
		/// </summary>
		/// <param name="key">The index key of the item to locate.</param>
		public object this[ string key ] {
			get {
				return context[ key ];
			}
			set {
				context[ key ] = value;
			}
		}

		/// <summary>
		/// Gets an object that can be used to synchronize access to the DownloadTask.
		/// </summary>
		public object SyncRoot {
			get { return syncRoot; }
		}

		/// <summary>
		/// The base folder where files will be downloaded.
		/// </summary>
		public string DownloadFilesBase {
			get {
				return downloadFilesBase;
			}
		}

		/// <summary>
		/// Initializes an existing DownloadTask with the specified item.
		/// </summary>
		/// <param name="item">The item corresponding to an updater task.</param>
		/// <param name="info">IDownloadInfoProvider</param>
		public void Init(DownloadItem item, IDownloadInfoProvider info) {
			this.item = item;
			if (this.item != null && info != null ) {
				downloadFilesBase = info.InitialDownloadLocation;
				this.item.Init(info);
			}
		}

		/// <summary>
		/// An unique identifier for the DownloadTask.
		/// </summary>
		public Guid TaskId {
			get {
				return id;
			}
		}

		/// <summary>
		/// The current state of the DownloadTask.
		/// <see cref="DownloadTaskState"/>
		/// </summary>
		public DownloadTaskState State {
			get {
				return state;
			}
			set {
				state = value;
			}
		} 

		/// <summary>
		/// The item corresponding to the current DownloadTask.
		/// </summary>
		public DownloadItem DownloadItem {
			get {
				return item;
			}
		}

		/// <summary>
		/// The IDownloader instance responsible for downloading this task
		/// </summary>
		internal IDownloader Downloader{
			get{ 
				return this.downloader;
			}

			set{
				this.downloader = value; 
			}
		}
		
		#endregion

		#region ISerializable Members

		/// <summary>
		/// Constructor to support serialization required for storing the task.
		/// </summary>
		/// <param name="info">The serialization information.</param>
		/// <param name="context">The serialization context.</param>
		[System.Security.Permissions.SecurityPermission(SecurityAction.LinkDemand)]
		protected DownloadTask(SerializationInfo info, StreamingContext context) {
			SerializationInfoReader reader = new SerializationInfoReader(info);
			item = (DownloadItem)reader.GetValue( "_manifest", typeof( DownloadItem), null );
			state = (DownloadTaskState)reader.GetValue( "_state",typeof( DownloadTaskState), DownloadTaskState.None );
			id = (Guid)reader.GetValue( "_id", typeof( Guid ), Guid.Empty );
			this.context = (StateBag)reader.GetValue( "_context", typeof( StateBag ), null );
			if (reader.Contains("_downloadFilesBase"))
				downloadFilesBase = reader.GetString( "_downloadFilesBase", null );
			else /* there used to be a typo in the field name */
				downloadFilesBase = reader.GetString( "_donwnloadFilesBase", null );
		}

		/// <summary>
		/// Method used by the seralization mechanism to retrieve the serialized information.
		/// </summary>
		/// <param name="info">The serialization information.</param>
		/// <param name="context">The serialization context.</param>
		[System.Security.Permissions.SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue( "_manifest", item );
			info.AddValue( "_state", state );
			info.AddValue( "_id", id );
			info.AddValue( "_context", this.context );
			info.AddValue( "_downloadFilesBase", downloadFilesBase );
		}

		#endregion

	}

	#region StateBag class
	/// <summary>
	/// A helper class to hold any state information needed in the DownloadTask.
	/// </summary>
	[Serializable]
	internal class StateBag : DictionaryBase {
		#region Constructor

		/// <summary>
		/// Default constructor.
		/// </summary>
		public StateBag() {
		}

		#endregion

		#region Public properties

		/// <summary>
		/// Gets or sets and object value associated with a string key.
		/// </summary>
		public object this [ string key ] {
			get {
				return Dictionary[ key ];
			}
			set {
				Dictionary[ key ] = value;
			}
		}

		#endregion
	}
	#endregion
}
