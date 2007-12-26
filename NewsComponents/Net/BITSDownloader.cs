#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
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
	/// This downloader uses BITS technology to download files.
	/// </summary>
	public sealed class BITSDownloader : 
		IDownloader, IBackgroundCopyCallback, IDisposable {
		#region Declarations

		private static readonly log4net.ILog Logger = RssBandit.Common.Logging.Log.GetLogger(typeof(BITSDownloader));

		/// <summary>
		/// This is used to wait for some time between checking the download status to avoid CPU consumtion.
		/// </summary>
		private const int TimeToWaitDuringSynchronousDownload = 200; // milliseconds

		/// <summary>
		/// Maximum time to wait for a pregress event.
		/// </summary>
		private const int BitsNoProgressTimeout = 5; //seconds

		/// <summary>
		/// The delay between reties.
		/// </summary>
		private const int BitsMinimumRetryDelay = 0; //immediate retry

		/// <summary>
		/// Constant for the COM error when an error is requested and no error have been raised.
		/// </summary>
		private const int ExceptionCodeNotAnError = -2145386481;

		/// <summary>
		/// The culture Id.
		/// </summary>
		private readonly int CultureIdForGettingComErrorMessages = CultureInfo.CurrentUICulture.LCID;

		/// <summary>
		/// Keeps all the pending downloader jobs.
		/// </summary>
		private HybridDictionary bitsDownloaderJobs = new HybridDictionary();

		/// <summary>
		/// The error message.
		/// </summary>
		private string cumulativeErrorMessage = String.Empty;

		/// <summary>
		/// The key to the job Id to be stored in the task state
		/// </summary>
		private const string TASK_JOBID_KEY = "jobId";

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor.
		/// </summary>
		public BITSDownloader() {
		}

		#endregion

		#region IDownloader Members

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
		[FileIOPermission( SecurityAction.Demand )]
		public void Download(DownloadTask task, TimeSpan maxWaitTime) {
			IBackgroundCopyManager backGroundCopyManager = null;
			IBackgroundCopyJob backgroundCopyJob = null;
			Guid jobID = Guid.Empty;

			try {
				//  create the manager
				backGroundCopyManager = (IBackgroundCopyManager) new BackgroundCopyManager();

				// If the job is already finished, just return
				if ( CheckForResumeAndProceed( backGroundCopyManager, task, out backgroundCopyJob ) ) {
					return;
				}

				if ( backgroundCopyJob == null ) {
					//  use utility function to create manager, job, get back jobid etc.; uses 'out' semantics
					CreateCopyJob( 
						backGroundCopyManager, 
						out backgroundCopyJob, 
						ref jobID, 
						task.DownloadItem.OwnerItemId, 
						task.DownloadItem.Enclosure.Description, task );

					// Save the jobId in the task
					task[TASK_JOBID_KEY] = jobID;

					// Prepare the job to download the manifest files
					PrepareJob( backgroundCopyJob, task );
				}

				WaitForDownload( task,  backgroundCopyJob, maxWaitTime );

			}
			catch( Exception e ) {
				//  if exception, clean up job
				OnJobError( task, backgroundCopyJob, null, e );
			}
			finally {
				if( null != backgroundCopyJob ) {
					Marshal.ReleaseComObject( backgroundCopyJob );
				}
				if( null !=  backGroundCopyManager ) {
					Marshal.ReleaseComObject( backGroundCopyManager );
				}
			}		
		}

		/// <summary>
		/// Asynchronous download method implementation.
		/// </summary>
		/// <param name="task">The DownloadTask to process.</param>
		[FileIOPermission( SecurityAction.Demand )]
		public void BeginDownload(DownloadTask task) {
			IBackgroundCopyManager backGroundCopyManager = null;
			IBackgroundCopyJob backgroundCopyJob = null;
			Guid jobID = Guid.Empty;

			try {
				//  create the manager
				backGroundCopyManager = (IBackgroundCopyManager) new BackgroundCopyManager();

				// If the job is already finished, just return
				if ( CheckForResumeAndProceed( backGroundCopyManager, task, out backgroundCopyJob ) ) {
					return;
				}

				if ( backgroundCopyJob != null ) {
					// if CheckForResumeAndProceed connected to an ongoing BITS job
					// wire up our notify interface to forward events to the client

					backgroundCopyJob.SetNotifyInterface( this );
			
					backgroundCopyJob.SetNotifyFlags( (uint)( 
						BG_JOB_NOTIFICATION_TYPE.BG_NOTIFY_JOB_ERROR | 
						BG_JOB_NOTIFICATION_TYPE.BG_NOTIFY_JOB_MODIFICATION | 
						BG_JOB_NOTIFICATION_TYPE.BG_NOTIFY_JOB_TRANSFERRED )
						);
				}
				else {
					//  use utility function to create the job. 
					CreateCopyJob( 
						backGroundCopyManager, 
						out backgroundCopyJob, 
						ref jobID, 
						task.DownloadItem.OwnerItemId,
						task.DownloadItem.Enclosure.Description, task );

					// Save the jobId in the task
					task[TASK_JOBID_KEY] = jobID;

					// Prepare the job to download the manifest files
					PrepareJob( backgroundCopyJob, task );

					// Set the notify interface to get BITS events
					backgroundCopyJob.SetNotifyInterface( this );

					backgroundCopyJob.SetNotifyFlags( (uint)( 
						BG_JOB_NOTIFICATION_TYPE.BG_NOTIFY_JOB_ERROR | 
						BG_JOB_NOTIFICATION_TYPE.BG_NOTIFY_JOB_MODIFICATION | 
						BG_JOB_NOTIFICATION_TYPE.BG_NOTIFY_JOB_TRANSFERRED )
						);

					// Fire our download start event
					OnDownloadStarted( new TaskEventArgs( task ) );

					// Initiate the BITS Job
					backgroundCopyJob.Resume();
				}
			}
			catch( Exception e ) {
				//  if exception, clean up job
				OnJobError( task, backgroundCopyJob, null, e );
			}
			finally {
				if( null != backgroundCopyJob ) {
					Marshal.ReleaseComObject( backgroundCopyJob );
				}
				if( null !=  backGroundCopyManager ) {
					Marshal.ReleaseComObject( backGroundCopyManager );
				}
			}	
		}

		/// <summary>
		/// Cancels an asynhronous download operation.
		/// </summary>
		/// <param name="task">The <see cref="DownloadTask"/> for the operation.</param>
		/// <returns>Indicates whether the operation was canceled.</returns>
		public bool CancelDownload(DownloadTask task) {
			IBackgroundCopyManager copyManager = null;
			IBackgroundCopyJob pJob = null;

			if ( task[TASK_JOBID_KEY] != null ) {
				try {
					Guid jobID = (Guid)task[ TASK_JOBID_KEY ];
					copyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
					copyManager.GetJob( ref jobID, out pJob );

					if ( pJob != null ) {
						pJob.Cancel();
					}
				}catch(COMException){
					/* we may come up empty when trying to get the job */ 
				}finally {
					if ( copyManager != null ) {
						Marshal.ReleaseComObject( copyManager );
					}

					if ( pJob != null ) {
						Marshal.ReleaseComObject( pJob );
					}
				}
			}
			return true;
		}
		
		#endregion

		#region Private helper methods

		/// <summary>
		/// Verifies if the task has a download job assigned, meaning this is a retry.
		/// If a transferred job is detected, the job is completed and the event
		/// OnDownloadCompleted is raised.
		/// </summary>
		/// <param name="copyManager">The BITS background copy manager to use</param>
		/// <param name="task">The DownloadTask to get the data from</param>
		/// <param name="copyJob">If an in progress BITS job is found for this task, this job is returned on this parameter</param>
		/// <returns>A Boolean value indicating whether the job is completed or not.
		/// A True value means that the job has been completed by BITS while a False value
		/// means that the job doesn't exists or can be resumed.
		/// </returns>
		private bool CheckForResumeAndProceed( IBackgroundCopyManager copyManager, DownloadTask task, out IBackgroundCopyJob copyJob ) {
			copyJob = null;
			if ( task[TASK_JOBID_KEY] != null ) {
				Guid jobId = (Guid)task[TASK_JOBID_KEY];
				BG_JOB_STATE jobState;

				try {
					copyManager.GetJob( ref jobId, out copyJob );
					if ( copyJob != null ) {
						copyJob.GetState( out jobState );
						if ( jobState == BG_JOB_STATE.BG_JOB_STATE_TRANSFERRED ) {
							OnJobTransferred( task, copyJob );
							return true;
						}
					}
				}
				catch(Exception ex) {
					Logger.Error( new DownloaderException( String.Format("The BITSDownloader cannot connect to the job '{0}' for the task '{1}' so a new BITS job will be created.", jobId, task.TaskId ), ex ));
				}
			}
			return false;
		}

		/// <summary>
		/// Locate the DownloadTask associated with the given background job.
		/// </summary>
		/// <param name="pJob">The job reference.</param>
		/// <returns>The DownloadTask for that job.</returns>
		private DownloadTask FindTask(IBackgroundCopyJob pJob) {
			Guid jobID = Guid.Empty;
			pJob.GetId( out jobID );

			foreach( DownloadTask task in BackgroundDownloadManager.GetTasks() ) {
				if ( Guid.Equals( task[ TASK_JOBID_KEY ], jobID ) ) {
					return task;
				}
			}

			return null;
		}

		/// <summary>
		/// Waits for the download to complete, for the synchronous usage of the downloader.
		/// </summary>
		/// <param name="backgroundCopyJob">The job instance reference.</param>
		/// <param name="maxWaitTime">The maximum wait time.</param>
		/// <param name="task">The updater task instance.</param>
		private void WaitForDownload( DownloadTask task, IBackgroundCopyJob backgroundCopyJob, TimeSpan maxWaitTime ) {
			Guid jobID = Guid.Empty;
			
			bool isCompleted = false;
			bool isSuccessful = false;
			BG_JOB_STATE state;

			try {
				backgroundCopyJob.GetId( out jobID );

				//  set endtime to current tickcount + allowable # milliseconds to wait for job
				double endTime = Environment.TickCount + maxWaitTime.TotalMilliseconds;

				while ( !isCompleted ) {
					backgroundCopyJob.GetState( out state );
					switch( state ) {
						case BG_JOB_STATE.BG_JOB_STATE_SUSPENDED: {
							OnDownloadStarted( new TaskEventArgs( task ) );
							backgroundCopyJob.Resume();
							break;
						}
						case BG_JOB_STATE.BG_JOB_STATE_ERROR: {
							//  use utility to:
							//  a)  get error info 
							//  b)  report it
							//  c)  cancel and remove copy job
							OnJobError( task, backgroundCopyJob, null, null );
							
							//  complete loop, but DON'T say it's successful
							isCompleted = true;
							break;
						}
						case BG_JOB_STATE.BG_JOB_STATE_TRANSIENT_ERROR: {							
							//  NOTE:  during debugging + test, transient errors resulted in complete job failure about 90%
							//  of the time.  Therefore we treat transients just like full errors, and CANCEL the job
							//  use utility to manage error etc.
							OnJobError( task, backgroundCopyJob, null, null );
							
							//  stop the loop, set completed to true but not successful
							isCompleted = true;
							break;
						}
						case BG_JOB_STATE.BG_JOB_STATE_TRANSFERRING: {
							OnJobModification( task, backgroundCopyJob );
							break;
						}
						case BG_JOB_STATE.BG_JOB_STATE_TRANSFERRED: {
							OnJobTransferred( task, backgroundCopyJob );

							isCompleted = true;
							isSuccessful = true;
							break;
						}
						default:
							break;
					}

					if ( isCompleted ) {
						break;
					}

					if( endTime < Environment.TickCount ) {
						DownloaderException ex = new DownloaderException("Download attempt timed out");
						OnJobError( task, backgroundCopyJob, null, ex );
						break;
					}

					//  Avoid 100% CPU utilisation with too tight a loop, let download happen.
					Thread.Sleep( TimeToWaitDuringSynchronousDownload );
				}

				if( !isSuccessful ) {
					//  create message + error, package it, publish 
					DownloaderException ex = new DownloaderException(String.Format("Download attempt for {0} failed", task.DownloadItem.ItemId));
					OnJobError( task, backgroundCopyJob, null, ex );
				}
			}
			catch( ThreadInterruptedException tie ) {
				//  if interrupted, clean up job
				OnJobError( task, backgroundCopyJob, null, tie );
			}
		}

		/// <summary>
		/// Prepares a BITS job adding the files and creating the required folders.
		/// </summary>
		/// <param name="backgroundCopyJob">The BITS job information.</param>
		/// <param name="task">The DownloadTask instace.</param>
		private void PrepareJob( IBackgroundCopyJob backgroundCopyJob, DownloadTask task) {
			Guid jobID = Guid.Empty;

			backgroundCopyJob.GetId( out jobID );
			task[ TASK_JOBID_KEY ] = jobID;

			DownloadFile sourceFile =  task.DownloadItem.File;
			string src =  sourceFile.Source; 
			//  to defend against config errors, check to see if the path given is UNC;
			//  if so, throw immediately there's a misconfiguration.  Paths to BITS must be HTTP or HTTPS
			if( FileHelper.IsUncPath( src ) ) {
				Exception ex = new DownloaderException("Download location must be HTTP or HTTPS URL" );
				Logger.Error( ex );
				throw ex;
			}

			//TODO: how about duplicate filenames?			
			string dest = Path.Combine(task.DownloadFilesBase, sourceFile.LocalName);
			
			if ( !Directory.Exists( Path.GetDirectoryName( dest ) ) ) {
				Directory.CreateDirectory( Path.GetDirectoryName( dest ) );
			}

			//  add this file to the job
			backgroundCopyJob.AddFile( src, dest );
			
		}

		/// <summary>
		/// Internal copy-job factory method.  Used to coordinate all aspects of a job set-up, 
		/// which includes creating a copy manager, creating a job within it, setting download
		/// parameters, and adding the job to our tracking collection for cleanup later
		/// </summary>
		/// <param name="copyManager">null reference to copy manager</param>
		/// <param name="copyJob">null reference to copy job</param>
		/// <param name="jobID">null reference to job id guid</param>
		/// <param name="jobName">string. Job name</param>
		/// <param name="jobDesc">string. Job description</param>
		/// <param name="task">DownloadTask. Used to get infos about credentials, proxy, etc.</param>
		private void CreateCopyJob( 
			IBackgroundCopyManager copyManager, 
			out IBackgroundCopyJob copyJob, 
			ref Guid jobID, 
			string jobName, 
			string jobDesc,
			DownloadTask task) {
			
						
			// create the job, set its description, use "out" semantics to get jobid and the actual job object
			copyManager.CreateJob( 
				jobName,
				BG_JOB_TYPE.BG_JOB_TYPE_DOWNLOAD,
				out jobID,
				out copyJob );

			//  set useful description
			copyJob.SetDescription( jobDesc );

			//  ***
			//      SET UP BITS JOB SETTINGS--TIMEOUTS/RETRY ETC           
			//      SEE THE FOLLOWING REFERENCES:
			//  **  http://msdn.microsoft.com/library/default.asp?url=/library/en-us/bits/bits/IBackgroundCopyJob2_setminimumretrydelay.asp?frame=true
			//  **  http://msdn.microsoft.com/library/default.asp?url=/library/en-us/bits/bits/IBackgroundCopyJob2_setnoprogresstimeout.asp?frame=true
			//  **  http://msdn.microsoft.com/library/default.asp?url=/library/en-us/bits/bits/bg_job_priority.asp
			//  ***
			
			//  in constant set to 0; this makes BITS retry as soon as possible after an error
			copyJob.SetMinimumRetryDelay( (uint)BitsMinimumRetryDelay );
			//  in constant set to 5 seconds; BITS will set job to Error status if exceeded
			copyJob.SetNoProgressTimeout( (uint)BitsNoProgressTimeout );
			//  make this job the highest (but still background) priority--
			copyJob.SetPriority( BG_JOB_PRIORITY.BG_JOB_PRIORITY_HIGH );
			//  ***

			//----------------------------------------------------------------------
			//-- Data Management and Research, Inc. - Paul Cox - 8/13/2004
			//-- ADDED the following lines to verify credentials of file copy job
			//----------------------------------------------------------------------

			// Set credentials on the job
			VerifyAndSetBackgroundCopyJobCredentials(copyJob, task);
			// Set proxy infos (incl Proxy Auth.)
			VerifyAndSetBackgroundCopyJobProxy(copyJob, task);

			//----------------------------------------------------------------------
			//-- End ADDED
			//----------------------------------------------------------------------

			//  lock our internal collection of jobs, and add this job--we use this collection in Dispose()
			//  to tell BITS to Cancel() jobs--and remove them from the queue
			//  if we did not do this, BITS would continue for (by default) two weeks to download what we asked!
			lock( bitsDownloaderJobs.SyncRoot ) {
				bitsDownloaderJobs.Add( jobID, jobName );
			}
		}
		/// <summary>
		/// Method responsible for checking the proxy and proxy authentication type and setting the 
		/// appropriate credentials. If the NTLM authentication is used then 
		/// if the username and password are not provided then we use null values. For
		/// all other authentication schemes we need a username and password.
		/// </summary>
		/// <param name="backgroundCopyJob">BackgroundJob on which we need to set the credentials.</param>
		/// <param name="task">DownloadTask. Provides the infos about download credentials</param>
		private void VerifyAndSetBackgroundCopyJobProxy(IBackgroundCopyJob backgroundCopyJob, DownloadTask task) {
			

			// Specify the proxy URL 
			// see also http://msdn.microsoft.com/library/en-us/bits/bits/ibackgroundcopyjob_setproxysettings.asp			
			try{

				IWebProxy  proxy = task.DownloadItem.Proxy;
				Uri sourceUri    = new Uri(task.DownloadItem.File.Source);
				Uri proxyUri     = proxy.GetProxy(sourceUri); 

				if (!proxy.IsBypassed(proxyUri)) {

					//trim trailing '/' because it causes BITS to throw an exception
					string proxyUriStr = proxyUri.ToString().TrimEnd('/');
					backgroundCopyJob.SetProxySettings(BG_JOB_PROXY_USAGE.BG_JOB_PROXY_USAGE_OVERRIDE, proxyUriStr, null);
				}

				//specify proxy credentials
				if(proxy.Credentials != null){
												
					ICredentials creds = proxy.Credentials;
					IBackgroundCopyJob2 copyJob = (IBackgroundCopyJob2) backgroundCopyJob;	

					BG_AUTH_CREDENTIALS credentials = new BG_AUTH_CREDENTIALS();
					credentials.Credentials.Basic.UserName = 
						StringHelper.EmptyOrNull(creds.GetCredential(sourceUri, "NTLM").Domain) ? 
						creds.GetCredential(sourceUri, "NTLM").UserName : 
						creds.GetCredential(sourceUri, "NTLM").Domain + "\\" + creds.GetCredential(sourceUri, "NTLM").UserName ;
					credentials.Credentials.Basic.Password = creds.GetCredential(sourceUri, "NTLM").Password;
					credentials.Scheme                     = BG_AUTH_SCHEME.BG_AUTH_SCHEME_NTLM;
					credentials.Target                     = BG_AUTH_TARGET.BG_AUTH_TARGET_PROXY;
					copyJob.SetCredentials(ref credentials);
				}

			}catch(Exception e){
				Logger.Error("Error in VerifyAndSetBackgroundCopyJobProxy():", e); 
			}
		}

		/// <summary>
		/// Method responsible for checking the authentication type and setting the 
		/// appropriate credentials. If the NTLM authentication is used then 
		/// if the username and password are not provided then we use null values. For
		/// all other authentication schemes we need a username and password.
		/// </summary>
		/// <param name="backgroundCopyJob">BackgroundJob on which we need to set the credentials.</param>
		/// <param name="task">DownloadTask. Provides the infos about download credentials</param>
		private void VerifyAndSetBackgroundCopyJobCredentials(IBackgroundCopyJob backgroundCopyJob, DownloadTask task) {
		
			try{ 

			IBackgroundCopyJob2 copyJob = (IBackgroundCopyJob2) backgroundCopyJob;
			ICredentials creds = task.DownloadItem.Credentials;
			Uri uri            = new Uri(task.DownloadItem.File.Source); 

			if(creds != null){

				//Specify HTTP Authentication (Basic) credentials
				BG_AUTH_CREDENTIALS credentials = new BG_AUTH_CREDENTIALS();
				credentials.Credentials.Basic.UserName = creds.GetCredential(uri, "Basic").UserName;
				credentials.Credentials.Basic.Password = creds.GetCredential(uri, "Basic").Password;
				credentials.Scheme                     = BG_AUTH_SCHEME.BG_AUTH_SCHEME_BASIC;
				credentials.Target                     = BG_AUTH_TARGET.BG_AUTH_TARGET_SERVER;
				copyJob.SetCredentials(ref credentials);
				
				//Specify HTTP Authentication (Digest) credentials
				credentials = new BG_AUTH_CREDENTIALS();
				credentials.Credentials.Basic.UserName = creds.GetCredential(uri, "Digest").UserName;
				credentials.Credentials.Basic.Password = creds.GetCredential(uri, "Digest").Password;
				credentials.Scheme                     = BG_AUTH_SCHEME.BG_AUTH_SCHEME_DIGEST;
				credentials.Target                     = BG_AUTH_TARGET.BG_AUTH_TARGET_SERVER;copyJob.SetCredentials(ref credentials);

				//Specify NTLM credentials
				credentials = new BG_AUTH_CREDENTIALS();
				credentials.Credentials.Basic.UserName = 
					StringHelper.EmptyOrNull(creds.GetCredential(uri, "NTLM").Domain) ? 
					creds.GetCredential(uri, "NTLM").UserName : 
					creds.GetCredential(uri, "NTLM").Domain + "\\" + creds.GetCredential(uri, "NTLM").UserName ;
				credentials.Credentials.Basic.Password = creds.GetCredential(uri, "NTLM").Password;
				credentials.Scheme                     = BG_AUTH_SCHEME.BG_AUTH_SCHEME_NTLM;
				credentials.Target                     = BG_AUTH_TARGET.BG_AUTH_TARGET_SERVER;
				copyJob.SetCredentials(ref credentials);				

			}//if(creds != null)

			}catch(Exception e){
				Logger.Error("Error in VerifyAndSetBackgroundCopyJobCredentials():", e); 
			}
		}
	

		/// <summary>
		/// Removes a copy job from the internal lookup collection.
		/// </summary>
		/// <param name="jobID">GUID identifies of a job (job id).</param>
		private void RemoveCopyJobEntry( Guid jobID ) {
			//  lock our collection of running jobs; remove it from the job collection
			lock( bitsDownloaderJobs.SyncRoot ) {
				bitsDownloaderJobs.Remove( jobID );
			}	
		}

		/// <summary>
		/// Method called by BITS when the job is modified, this method is used to notify progress.
		/// </summary>
		/// <param name="task">The DownloadTask instance.</param>
		/// <param name="pJob">The BITS job reference.</param>
		private void OnJobModification( DownloadTask task, IBackgroundCopyJob pJob ) {
			_BG_JOB_PROGRESS progress;
			pJob.GetProgress( out progress );
			
			DownloadTaskProgressEventArgs args = new DownloadTaskProgressEventArgs( (long)progress.BytesTotal, 
				(long)progress.BytesTransferred, (int)progress.FilesTotal, (int)progress.FilesTransferred, task );

			OnDownloadProgress( args );
		}

		/// <summary>
		/// Centralizes all chores related to stopping and cancelling a copy job, and getting back
		/// from BITS the errors incurred during the job.
		/// </summary>
		/// <param name="task">reference to the job associated task</param>
		/// <param name="pJob">reference to the copy job object (not job id)</param>
		/// <param name="pError">reference to the COM error reported by bits (might be null)</param>
		/// <param name="ex">reference to an exception cosidered as an error (might be null)</param>
		private void OnJobError( DownloadTask task, IBackgroundCopyJob pJob, IBackgroundCopyError pError, Exception ex ) {
			string jobDesc = "";
			string jobName = "";
			Guid jobID = Guid.Empty;

			Exception finalException = ex;
			if ( pJob != null ) {
				//  get information about this job
				pJob.GetDescription( out jobDesc );
				pJob.GetDisplayName( out jobName );
				pJob.GetId( out jobID );

				try {
					// if the error hasn't been reported, try to get it
					if ( pError == null ) {
						pJob.GetError( out pError );
					}
				}
				catch( COMException e ) {
					Logger.Error( e );
					if( e.ErrorCode != ExceptionCodeNotAnError	) {
						throw e;
					}
				}
				
				// If we've got the native error, wrap into a nicer exception
				if ( pError != null ) {
					BitsDownloadErrorException BitsEx = new BitsDownloadErrorException( pError, (uint)CultureIdForGettingComErrorMessages );
					cumulativeErrorMessage += BitsEx.Message + Environment.NewLine;
					finalException = BitsEx;
				}
				

				BG_JOB_STATE state;
				pJob.GetState(out state);
				if( state != BG_JOB_STATE.BG_JOB_STATE_ACKNOWLEDGED 
					&& state != BG_JOB_STATE.BG_JOB_STATE_CANCELLED ) {
					pJob.Cancel();
				}
				RemoveCopyJobEntry( jobID );
			}

			OnDownloadError( new DownloadTaskErrorEventArgs( task, finalException ) );
			Logger.Error( finalException );
			//throw finalException;
		}

		/// <summary>
		/// Method called by BITS when the job is completed.
		/// </summary>
		/// <param name="task">The Updater task instance.</param>
		/// <param name="pJob">The BITS job reeference.</param>
		private void OnJobTransferred( DownloadTask task, IBackgroundCopyJob pJob ) {
			pJob.Complete();
			OnDownloadCompleted( new TaskEventArgs( task ) );
		}

		#endregion

		#region IDisposable Implementation

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
			uint BG_JOB_ENUM_ALL_USERS  = 0x0001;
			uint numJobs;
			uint fetched;
			Guid jobID;
			IBackgroundCopyManager mgr = null;
			IEnumBackgroundCopyJobs jobs = null;
			IBackgroundCopyJob job = null;
			if (isDisposing) {
				try {
					mgr = (IBackgroundCopyManager)( new BackgroundCopyManager() );

					mgr.EnumJobs( BG_JOB_ENUM_ALL_USERS, out jobs );

					jobs.GetCount( out numJobs );

					//  lock the jobs collection for duration of this operation
					lock( bitsDownloaderJobs.SyncRoot) {
						for( int i = 0; i < numJobs; i++ ) {
							//  use jobs interface to walk through getting each job
							jobs.Next( (uint)1, out job, out fetched );
						
							//  get jobid guid
							job.GetId( out jobID );

							//  check if the job is in OUR collection; if so cancel it.  we obviously don't want to get
							//  jobs from other Updater threads/processes, or other BITS jobs on the machine!
							if( bitsDownloaderJobs.Contains( jobID ) ) {
								//  take ownership just in case, and cancel() it
								job.TakeOwnership();
								job.Cancel();	
								// remove from our collection
								bitsDownloaderJobs.Remove( jobID );					
							}
						}
					}
				}
				finally {
					if( null != mgr ) {
						Marshal.ReleaseComObject( mgr );
						mgr = null;
					}
					if( null != jobs ) {
						Marshal.ReleaseComObject( jobs );
						jobs = null;
					}
					if( null != job ) {
						Marshal.ReleaseComObject( job );
						job = null;
					}
				}
			}
		}

		
		/// <summary>
		/// Destructor/Finalizer
		/// </summary>
		~BITSDownloader() {
			// Simply call Dispose(false).
			Dispose(false);
		}

		#endregion

		#region IBackgroundCopyCallback Members

		/// <summary>
		/// BITS notifies about job finished using this method.
		/// </summary>
		/// <param name="pJob">The BITS job reference.</param>
		void IBackgroundCopyCallback.JobTransferred(IBackgroundCopyJob pJob) {
			OnJobTransferred( FindTask( pJob ), pJob );
		}

		/// <summary>
		/// BITS notifies about job error using this method.
		/// </summary>
		/// <param name="pJob">The BITS job reference.</param>
		/// <param name="pError">The error information.</param>
		void IBackgroundCopyCallback.JobError(IBackgroundCopyJob pJob, IBackgroundCopyError pError) {
			OnJobError( FindTask( pJob ), pJob, pError, null );
		}

		/// <summary>
		/// BITS notifies about job finished using this method.
		/// </summary>
		/// <param name="pJob">The BITS job reference.</param>
		/// <param name="dwReserved">Reserved for BITS.</param>
		void IBackgroundCopyCallback.JobModification(IBackgroundCopyJob pJob, uint dwReserved) {
			OnJobModification( FindTask( pJob ), pJob );
		}

		#endregion

	}

	#region BitsDownloadErrorException

	/// <summary>
	/// Exception thrown by BITS downloader when an error is found.
	/// </summary>
	[Serializable]
	public class BitsDownloadErrorException : Exception {
		#region Private members

		/// <summary>
		/// The context for the error.
		/// </summary>
		private BG_ERROR_CONTEXT contextForError;

		/// <summary>
		/// The error code detected.
		/// </summary>
		private int errorCode;

		/// <summary>
		/// The description of the context.
		/// </summary>
		private string contextDescription;
		
		/// <summary>
		/// The description of the error.
		/// </summary>
		private string errorDescription;

		/// <summary>
		/// The protocol name.
		/// </summary>
		private string protocol;

		/// <summary>
		/// The file name where the file will be copied.
		/// </summary>
		private string fileLocalName;

		/// <summary>
		/// The remote file name that was downloaded.
		/// </summary>
		private string fileRemoteName;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor.
		/// </summary>
		public BitsDownloadErrorException() : base() {
		}

		/// <summary>
		/// Creates an exception with the BITS error reference and a language id.
		/// </summary>
		/// <param name="error">The BITS error reference.</param>
		/// <param name="langID">The language Id.</param>
		internal BitsDownloadErrorException( IBackgroundCopyError error, uint langID ) {
			IBackgroundCopyFile file;

			error.GetError(out contextForError, out errorCode );
			
			error.GetErrorContextDescription( langID, out contextDescription );
			error.GetErrorDescription( langID, out errorDescription );
			error.GetFile( out file );
			error.GetProtocol( out protocol );

			file.GetLocalName( out fileLocalName );
			file.GetRemoteName( out fileRemoteName );
		}

		/// <summary>
		/// Creates an exception with the specified message.
		/// </summary>
		/// <param name="message">The message of the exception.</param>
		public BitsDownloadErrorException( string message ) : base( message ) {
			errorDescription = message;
		}

		/// <summary>
		/// Creates an exception with the specified message and the inner exception detected.
		/// </summary>
		/// <param name="message">The message string.</param>
		/// <param name="innerException">The inner exception reference.</param>
		public BitsDownloadErrorException( string message, Exception innerException ) : base( message, innerException ) {
			errorDescription = message;
		}

		/// <summary>
		/// Constructor used by the serialization infrastructure.
		/// </summary>
		/// <param name="info">The serialization information for the object.</param>
		/// <param name="context">The context for the serialization.</param>
		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter=true)]
		protected BitsDownloadErrorException(SerializationInfo info, StreamingContext context) : base( info, context ) {
		}

		#endregion

		#region Public members

		/// <summary>
		/// The error code.
		/// </summary>
		public int Code { 
			get { return errorCode; } 
		}

		/// <summary>
		/// The error context.
		/// </summary>
		public int Context { 
			get { return (int)contextForError; } 
		}

		/// <summary>
		/// The context description.
		/// </summary>
		public string ContextDescription {
			get { return contextDescription; }
		}

		/// <summary>
		/// The error message.
		/// </summary>
		public override string Message {
			get{ return errorDescription; }
		}

		/// <summary>
		/// The protocol used.
		/// </summary>
		public string Protocol {
			get { return protocol; }
		}

		/// <summary>
		/// The local file name.
		/// </summary>
		public string LocalFileName {
			get { return fileLocalName; }
		}

		/// <summary>
		/// The remote file name.
		/// </summary>
		public string RemoteFileName {
			get  { return fileRemoteName; }
		}

		#endregion
		
		#region Public methods

		/// <summary>
		/// Used by the serialization infrastructure.
		/// </summary>
		/// <param name="info">The serialization information.</param>
		/// <param name="context">The serialization context.</param>
		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
			base.GetObjectData ( info, context );
		}

		#endregion
	}
	#endregion

}
