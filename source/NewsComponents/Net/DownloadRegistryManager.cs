#region CVS Version Header
/*
 * $Id: DownloadRegistryManager.cs,v 1.5 2007/06/14 00:59:40 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2007/06/14 00:59:40 $
 * $Revision: 1.5 $
 */
#endregion

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using NewsComponents.Utils;

namespace NewsComponents.Net
{
	/// <summary>
	/// Manages the list of download tasks under progress, from its initial registration when they
	/// are submitted to download, to their final state when they are successfully downloaded
	/// or cancelled.
	/// </summary>
	[Serializable]
	internal sealed class DownloadRegistryManager
	{
		#region Private fields

		/// <summary>
		/// The singleton instance stored.
		/// </summary>
		private static readonly DownloadRegistryManager instance = new DownloadRegistryManager();

		private static readonly log4net.ILog Logger = RssBandit.Common.Logging.Log.GetLogger(typeof(DownloadRegistryManager));

		/// <summary>
		/// Root folder name for the registry.
		/// </summary>
		private const string root = "download.registry";

		/// <summary>
		/// Helper class for directory information.
		/// </summary>
		private DirectoryInfo rootDirInfo;

		/// <summary>
		/// The in memory registry storage.
		/// </summary>
		private Hashtable registry = new Hashtable( 10 );

		/// <summary>
		/// Indicates if the list of tasks is loaded
		/// </summary>
		private bool loaded = false;

		#endregion

		#region Singleton implementation

		/// <summary>
		/// Singleton instance.
		/// </summary>
		public static DownloadRegistryManager Current {
			get { return instance; }
		}

		/// <summary>
		/// Default constructor disable because there is a singleton implementation.
		/// </summary>
		private DownloadRegistryManager() {
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Directs the instance to use a different base path
		/// than Temp.
		/// </summary>
		/// <param name="baseFolder">string</param>
		public void SetBaseFolder(string baseFolder) {
			string path = Path.Combine( baseFolder, root );
			if ( !Directory.Exists( path ) ) {
				rootDirInfo = Directory.CreateDirectory( path );
			}
			else {
				rootDirInfo = new DirectoryInfo( path );
			}
		}
		/// <summary>
		/// Loads all the pending tasks.
		/// </summary>
		public void Load() {
			foreach( FileInfo fi in RootDir.GetFiles() ) {
				this.LoadTask( fi.FullName );
			}
		}

		/// <summary>
		/// Updates the information of an existing stored task.
		/// </summary>
		/// <param name="task">The DownloadTask instance.</param>
		public void UpdateTask(DownloadTask task) {		
				SaveTask( task );		
		}


		/// <summary>
		/// Indicates whether a download task already exists that corresponds to the input task
		/// </summary>
		/// <param name="task">The DownloadTask instance</param>
		/// <returns>True if there is already a download task for the enclosure</returns>
		public bool TaskAlreadyExists(DownloadTask task){
			return Tasks.ContainsKey(task.DownloadItem.Enclosure.Url); 
		}

		/// <summary>
		/// Registers the task in the storage.
		/// </summary>
		/// <param name="task">The DownloadTask instance.</param>
		public void RegisterTask(DownloadTask task) {
			lock( Tasks.SyncRoot ) {
				if ( !Tasks.ContainsKey( task.DownloadItem.Enclosure.Url ) ) {
					Tasks.Add( task.DownloadItem.Enclosure.Url, task );
				}
			}
			SaveTask( task );
		}

		/// <summary>
		/// Removes the task from the storage.
		/// </summary>
		/// <param name="task">The DownloadTask.</param>
		public void UnRegisterTask(DownloadTask task) {
			lock( Tasks.SyncRoot ) {
				Tasks.Remove( task.DownloadItem.Enclosure.Url );
			}
			string fileName = Path.Combine( RootDir.FullName, task.TaskId.ToString() + ".task" );
			FileHelper.DestroyFile( fileName );
		}

		/// <summary>
		/// Return all the tasks stored in memory.
		/// </summary>
		/// <returns>An array of DownloadTask instances.</returns>
		public DownloadTask[] GetTasks() {
			DownloadTask[] result = new DownloadTask[ Tasks.Count ];
			Tasks.Values.CopyTo( result, 0 );
			return result;
		}

		/// <summary>
		/// Returns all the stored tasks by a given owner id.
		/// </summary>
		/// <param name="ownerId">The owner id.</param>
		/// <returns>An array of DownloadTask instances.</returns>
		public DownloadTask[] GetByOwnerId(string ownerId) {
			ArrayList tasks = new ArrayList();
			lock( Tasks.SyncRoot ) {
				foreach( DownloadTask task in Tasks.Values ) {
					if ( task.DownloadItem.OwnerFeedId == ownerId ) {
						tasks.Add( task );
					}
				}
			}
			return (DownloadTask[])tasks.ToArray( typeof(DownloadTask) );
		}

		/// <summary>
		/// Returns all the stored tasks by a given owner item id.
		/// </summary>
		/// <param name="ownerItemId">The owner item id.</param>
		/// <returns>An array of DownloadTask instances.</returns>
		public DownloadTask[] GetByOwnerItemId(string ownerItemId) {
			ArrayList tasks = new ArrayList();
			lock( Tasks.SyncRoot ) {
				foreach( DownloadTask task in Tasks.Values ) {
					if ( task.DownloadItem.OwnerItemId == ownerItemId ) {
						tasks.Add( task );
					}
				}
			}
			return (DownloadTask[])tasks.ToArray( typeof(DownloadTask) );
		}

		/// <summary>
		/// Return the DownloadTask for a given item id.
		/// </summary>
		/// <param name="itemId">The item id (Guid).</param>
		/// <returns>An DownloadTask instance.</returns>
		public DownloadTask GetByItemID(Guid itemId) {
			lock( Tasks.SyncRoot ) {
				foreach( DownloadTask task in Tasks.Values ) {
					if ( task.DownloadItem.ItemId == itemId ) {
						return task;
					}
				}
			}
			return null;
		}

		#endregion

		#region Private Methods

		private DirectoryInfo RootDir {
			get {
				if (rootDirInfo != null)
					return rootDirInfo;
				SetBaseFolder(Path.GetTempPath());
				return rootDirInfo;
			}
		}

		/// <summary>
		/// Load the tasks stored in a specified path.
		/// </summary>
		/// <param name="taskFilePath">The base path for the registry storage.</param>
		/// <returns>An DownloadTask instance.</returns>
		[System.Security.Permissions.SecurityPermission( SecurityAction.Demand, SerializationFormatter=true )]
		private DownloadTask LoadTask( string taskFilePath ) {
			DownloadTask task = null;
			BinaryFormatter formatter = new BinaryFormatter();
			using( FileStream stream = new FileStream( taskFilePath, FileMode.Open, FileAccess.Read, FileShare.Read) ) {
				try{
					task = (DownloadTask)formatter.Deserialize( stream );				
					lock( registry.SyncRoot ) {
						//TODO: Once we have a UI for managing enclosures we'll need to 
						//always load all tasks. 
						if( task.State != DownloadTaskState.Downloaded && !registry.ContainsKey( task.DownloadItem.Enclosure.Url ) ) {
							registry.Add( task.DownloadItem.Enclosure.Url , task );
						}else{
							stream.Close(); 
							string fileName = Path.Combine( RootDir.FullName, task.TaskId.ToString() + ".task" );
							FileHelper.DestroyFile( fileName );
						}
					}
				}catch(Exception e){ 
					Logger.Error("Error in DownloadRegistryManager.LoadTask():", e);
				}
			}
			return task;
		}

		/// <summary>
		/// Stores a task in the registry storage.
		/// </summary>
		/// <param name="task">The DownloadTask instance.</param>
		[System.Security.Permissions.SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		private void SaveTask( DownloadTask task ) {
			string filename = Path.Combine( RootDir.FullName, String.Format( CultureInfo.InvariantCulture, "{0}.task", task.TaskId.ToString() ) );
			try {
				using(Stream stream = new FileStream( filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read ) ) {
					BinaryFormatter formatter = new BinaryFormatter();
					formatter.Serialize( stream, task );
				}
			}
			catch( Exception ex) {
				File.Delete( filename );
				Logger.Error( ex );
				throw;
			}
		}


		/// <summary>
		/// Gets the list of registered tasks, ensuring the list is loaded
		/// </summary>
		private Hashtable Tasks {
			get {
				if ( !loaded ) {
					Load();
					loaded = true;
				}
				return registry;
			}
		}

		#endregion


	}
}
