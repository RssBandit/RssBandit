#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;


namespace NewsComponents.Net
{
	// COM Interop C# classes for accessing BITS API.
	// Refer to MSDN for Details: 
	// http://msdn.microsoft.com/en-us/library/windows/desktop/aa362820(v=vs.85).aspx
	// http://msdn.microsoft.com/msdnmag/issues/03/02/BITS/default.aspx 

	/// <summary>
	/// Entry point to the BITS infraestructure.
	/// </summary>
	[GuidAttribute("4991D34B-80A1-4291-83B6-3328366B9097")]
	[ClassInterfaceAttribute(ClassInterfaceType.None)]
	[ComImportAttribute()]
	internal class BackgroundCopyManager {
	}

	/// <summary>
	/// Use the IBackgroundCopyManager interface to create transfer jobs, 
	/// retrieve an enumerator object that contains the jobs in the queue, 
	/// and to retrieve individual jobs from the queue.
	/// </summary>
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	[GuidAttribute("5CE34C0D-0DC9-4C1F-897C-DAA1B78CEE7C")]
	[ComImportAttribute()]
	internal interface IBackgroundCopyManager {
		
		/// <summary>
		/// Creates a new transfer job
		/// </summary>
		/// <param name="DisplayName">Null-terminated string that contains a display name for the job. Typically, the display name is used to identify the job in a user interface. Note that more than one job may have the same display name. Must not be NULL. The name is limited to 256 characters, not including the null terminator.</param>
		/// <param name="pJobId">Uniquely identifies your job in the queue. Use this identifier when you call the IBackgroundCopyManager::GetJob method to get a job from the queue.</param>
		/// <param name="ppJob">An IBackgroundCopyJob interface pointer that you use to modify the job's properties and specify the files to be transferred. To activate the job in the queue, call the IBackgroundCopyJob::Resume method. Release ppJob when done.</param>
		/// <param name="Type">Type of transfer job, such as BG_JOB_TYPE_DOWNLOAD. For a list of transfer types, see the BG_JOB_TYPE enumeration. </param>
		void CreateJob([MarshalAs(UnmanagedType.LPWStr)] string DisplayName, BG_JOB_TYPE Type, out Guid pJobId, [MarshalAs(UnmanagedType.Interface)] out IBackgroundCopyJob ppJob);
		
		/// <summary>
		/// Retrieves a given job from the queue
		/// </summary>
		/// <param name="jobID">Identifies the job to retrieve from the transfer queue. The CreateJob method returns the job identifier.</param>
		/// <param name="ppJob">An IBackgroundCopyJob interface pointer to the job specified by JobID. When done, release ppJob.</param>
		void GetJob(ref Guid jobID, [MarshalAs(UnmanagedType.Interface)] out IBackgroundCopyJob ppJob);
		
		/// <summary>
		/// Retrieves an enumerator object that you use to enumerate jobs in the queue
		/// </summary>
		/// <param name="dwFlags">Specifies whose jobs to include in the enumeration. If dwFlags is set to 0, the user receives all jobs that they own in the transfer queue. The following table lists the enumeration options. </param>
		/// <param name="ppenum">An IEnumBackgroundCopyJobs interface pointer that you use to enumerate the jobs in the transfer queue. The contents of the enumerator depend on the value of dwFlags. Release ppEnumJobs when done. </param>
		void EnumJobs(uint dwFlags, [MarshalAs(UnmanagedType.Interface)] out IEnumBackgroundCopyJobs ppenum);
		
		/// <summary>
		/// Retrieves a description for the given error code
		/// </summary>
		/// <param name="hResult">Error code from a previous call to a BITS method.</param>
		/// <param name="LanguageId">Identifies the language identifier to use to generate the description. To create the language identifier, use the MAKELANGID macro.</param>
		/// <param name="pErrorDescription">Null-terminated string that contains a description of the error. Call the CoTaskMemFree function to free ppErrorDescription when done. </param>
		void GetErrorDescription([MarshalAs(UnmanagedType.Error)] int hResult, uint LanguageId, [MarshalAs(UnmanagedType.LPWStr)] out string pErrorDescription);
	}

	/// <summary>
	/// Implement the IBackgroundCopyCallback interface to receive notification that a 
	/// job is complete, has been modified, or is in error. Clients use this interface 
	/// instead of polling for the status of the job.
	/// </summary>
	[ComImport]
	[Guid("97EA99C7-0186-4AD4-8DF9-C5B4E0ED6B22")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IBackgroundCopyCallback {
		/// <summary>
		/// Called when all of the files in the job have successfully transferred.
		/// </summary>
		/// <param name="pJob">Contains job-related information, such as the time the job completed, the number of bytes transferred, and the number of files transferred. Do not release pJob; BITS releases the interface when the method returns. </param>
		void JobTransferred([In, MarshalAs(UnmanagedType.Interface)] IBackgroundCopyJob pJob);

		/// <summary>
		/// Called when an error occurs.
		/// </summary>
		/// <param name="pJob">Contains job-related information, such as the number of bytes and files transferred before the error occurred. It also contains the methods to resume and cancel the job. Do not release pJob; BITS releases the interface when the JobError method returns.</param>
		/// <param name="pError">Contains error information, such as the file being processed at the time the fatal error occurred and a description of the error. Do not release pError; BITS releases the interface when the JobError method returns.</param>
		void JobError([In, MarshalAs(UnmanagedType.Interface)] IBackgroundCopyJob pJob, [In, MarshalAs(UnmanagedType.Interface)] IBackgroundCopyError pError);

		/// <summary>
		/// Called when a job is modified.
		/// </summary>
		/// <param name="pJob">Contains the methods for accessing property, progress, and state information of the job. Do not release pJob; BITS releases the interface when the JobModification method returns.</param>
		/// <param name="dwReserved">Reserved for future use.</param>
		void JobModification([In, MarshalAs(UnmanagedType.Interface)] IBackgroundCopyJob pJob, [In] uint dwReserved);
		
	}
 
	/// <summary>
	/// Use the IBackgroundCopyJob interface to add files to the job, 
	/// set the priority level of the job, determine the state of the
	/// job, and to start and stop the job.
	/// </summary>
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	[GuidAttribute("37668D37-507E-4160-9316-26306D150B12")]
	[ComImportAttribute()]
	internal interface IBackgroundCopyJob {
		
		/// <summary>
		/// Adds multiple files to the job
		/// </summary>
		/// <param name="cFileCount">Number of elements in paFileSet. </param>
		/// <param name="pFileSet">Array of BG_FILE_INFO structures that identify the local and remote file names of the files to transfer.</param>
		void AddFileSet(uint cFileCount, [MarshalAs(UnmanagedType.LPArray)] BG_FILE_INFO[] pFileSet);

		/// <summary>
		/// Adds a single file to the job
		/// </summary>
		/// <param name="LocalName">Null-terminated string that contains the name of the file on the server. For information on specifying the remote name, see the RemoteName member and Remarks section of the BG_FILE_INFO structure. </param>
		/// <param name="RemoteUrl">Null-terminated string that contains the name of the file on the client. For information on specifying the local name, see the LocalName member and Remarks section of the BG_FILE_INFO structure. </param>
		void AddFile([MarshalAs(UnmanagedType.LPWStr)] string RemoteUrl, [MarshalAs(UnmanagedType.LPWStr)] string LocalName);
		
		/// <summary>
		/// Returns an interface pointer to an enumerator
		/// object that you use to enumerate the files in the job
		/// </summary>
		/// <param name="pEnum">IEnumBackgroundCopyFiles interface pointer that you use to enumerate the files in the job. Release ppEnumFiles when done. </param>
		void EnumFiles([MarshalAs(UnmanagedType.Interface)] out IEnumBackgroundCopyFiles pEnum);
		
		/// <summary>
		/// Pauses the job
		/// </summary>
		void Suspend();
		
		/// <summary>
		/// Restarts a suspended job
		/// </summary>
		void Resume();
		
		/// <summary>
		/// Cancels the job and removes temporary files from the client
		/// </summary>
		void Cancel();
		
		/// <summary>
		/// Ends the job and saves the transferred files on the client
		/// </summary>
		void Complete();
		
		/// <summary>
		/// Retrieves the identifier of the job in the queue
		/// </summary>
		/// <param name="pVal">GUID that identifies the job within the BITS queue.</param>
		void GetId(out Guid pVal);
		
		/// <summary>
		/// Retrieves the type of transfer being performed, 
		/// such as a file download
		/// </summary>
		/// <param name="pVal">Type of transfer being performed. For a list of transfer types, see the BG_JOB_TYPE enumeration type. </param>
		void GetType(out BG_JOB_TYPE pVal);
		
		/// <summary>
		/// Retrieves job-related progress information, 
		/// such as the number of bytes and files transferred 
		/// to the client
		/// </summary>
		/// <param name="pVal">Contains data that you can use to calculate the percentage of the job that is complete. For more information, see BG_JOB_PROGRESS. </param>
		void GetProgress(out BG_JOB_PROGRESS pVal);
		
		/// <summary>
		/// Retrieves timestamps for activities related
		/// to the job, such as the time the job was created
		/// </summary>
		/// <param name="pVal">Contains job-related time stamps. For available time stamps, see the BG_JOB_TIMES structure.</param>
		void GetTimes(out BG_JOB_TIMES pVal);
		
		/// <summary>
		/// Retrieves the state of the job
		/// </summary>
		/// <param name="pVal">Current state of the job. For example, the state reflects whether the job is in error, transferring data, or suspended. For a list of job states, see the BG_JOB_STATE enumeration type. </param>
		void GetState(out BG_JOB_STATE pVal);
		
		/// <summary>
		/// Retrieves an interface pointer to 
		/// the error object after an error occurs
		/// </summary>
		/// <param name="ppError">Error interface that provides the error code, a description of the error, and the context in which the error occurred. This parameter also identifies the file being transferred at the time the error occurred. Release ppError when done. </param>
		void GetError([MarshalAs(UnmanagedType.Interface)] out IBackgroundCopyError ppError);
		
		/// <summary>
		/// Retrieves the job owner's identity
		/// </summary>
		/// <param name="pVal">Null-terminated string that contains the string version of the SID that identifies the job's owner. Call the CoTaskMemFree function to free ppOwner when done. </param>
		void GetOwner([MarshalAs(UnmanagedType.LPWStr)] out string pVal);
		
		/// <summary>
		/// Specifies a display name that identifies the job in 
		/// a user interface
		/// </summary>
		/// <param name="Val">Null-terminated string that identifies the job. Must not be NULL. The length of the string is limited to 256 characters, not including the null terminator. </param>
		void SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string Val);
		
		/// <summary>
		/// Retrieves the display name that identifies the job
		/// </summary>
		/// <param name="pVal">Null-terminated string that contains the display name that identifies the job. More than one job can have the same display name. Call the CoTaskMemFree function to free ppDisplayName when done.</param>
		void GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pVal);
		
		/// <summary>
		/// Specifies a description of the job
		/// </summary>
		/// <param name="Val">Null-terminated string that provides additional information about the job. The length of the string is limited to 1,024 characters, not including the null terminator.</param>
		void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string Val);
		
		/// <summary>
		/// Retrieves the description of the job
		/// </summary>
		/// <param name="pVal">Null-terminated string that contains a short description of the job. Call the CoTaskMemFree function to free ppDescription when done. </param>
		void GetDescription([MarshalAs(UnmanagedType.LPWStr)] out string pVal);
		
		/// <summary>
		/// Specifies the priority of the job relative to 
		/// other jobs in the transfer queue
		/// </summary>
		/// <param name="Val">Specifies the priority level of your job relative to other jobs in the transfer queue. The default is BG_JOB_PRIORITY_NORMAL. For a list of priority levels, see the BG_JOB_PRIORITY enumeration. </param>
		void SetPriority(BG_JOB_PRIORITY Val);

		/// <summary>
		/// Retrieves the priority level you have set for the job.
		/// </summary>
		/// <param name="pVal">Priority of the job relative to other jobs in the transfer queue. </param>
		void GetPriority(out BG_JOB_PRIORITY pVal);
		
		/// <summary>
		/// Specifies the type of event notification to receive
		/// </summary>
		/// <param name="Val">Set one or more of the following flags to identify the events that you want to receive. </param>
		void SetNotifyFlags(uint Val);
		
		/// <summary>
		/// Retrieves the event notification (callback) flags 
		/// you have set for your application.
		/// </summary>
		/// <param name="pVal">Identifies the events that your application receives. The following table lists the event notification flag values. </param>
		void GetNotifyFlags(out uint pVal);
		
		/// <summary>
		/// Specifies a pointer to your implementation of the 
		/// IBackgroundCopyCallback interface (callbacks). The 
		/// interface receives notification based on the event 
		/// notification flags you set
		/// </summary>
		/// <param name="Val">An IBackgroundCopyCallback interface pointer. To remove the current callback interface pointer, set this parameter to NULL.</param>
		void SetNotifyInterface([MarshalAs(UnmanagedType.IUnknown)] object Val);
		
		/// <summary>
		/// Retrieves a pointer to your implementation 
		/// of the IBackgroundCopyCallback interface (callbacks).
		/// </summary>
		/// <param name="pVal">Interface pointer to your implementation of the IBackgroundCopyCallback interface. When done, release ppNotifyInterface.</param>
		void GetNotifyInterface([MarshalAs(UnmanagedType.IUnknown)] out object pVal);
		
		/// <summary>
		/// Specifies the minimum length of time that BITS waits after 
		/// encountering a transient error condition before trying to 
		/// transfer the file
		/// </summary>
		/// <param name="Seconds">Minimum length of time, in seconds, that BITS waits after encountering a transient error before trying to transfer the file. The default retry delay is 600 seconds (10 minutes). The minimum retry delay that you can specify is 60 seconds. If you specify a value less than 60 seconds, BITS changes the value to 60 seconds. If the value exceeds the no-progress-timeout value retrieved from the GetNoProgressTimeout method, BITS will not retry the transfer and moves the job to the BG_JOB_STATE_ERROR state. </param>
		void SetMinimumRetryDelay(uint Seconds);
		
		/// <summary>
		/// Retrieves the minimum length of time that BITS waits after 
		/// encountering a transient error condition before trying to 
		/// transfer the file
		/// </summary>
		/// <param name="Seconds">Length of time, in seconds, that the service waits after encountering a transient error before trying to transfer the file. </param>
		void GetMinimumRetryDelay(out uint Seconds);
		
		/// <summary>
		/// Specifies the length of time that BITS continues to try to 
		/// transfer the file after encountering a transient error 
		/// condition
		/// </summary>
		/// <param name="Seconds">Length of time, in seconds, that BITS tries to transfer the file after the first transient error occurs. The default retry period is 1,209,600 seconds (14 days). Set the retry period to 0 to prevent retries and to force the job into the BG_JOB_STATE_ERROR state for all errors. If the retry period value exceeds the JobInactivityTimeout Group Policy value (90-day default), BITS cancels the job after the policy value is exceeded.</param>
		void SetNoProgressTimeout(uint Seconds);
		
		/// <summary>
		/// Retrieves the length of time that BITS continues to try to 
		/// transfer the file after encountering a transient error condition
		/// </summary>
		/// <param name="Seconds">Length of time, in seconds, that the service tries to transfer the file after a transient error occurs. </param>
		void GetNoProgressTimeout(out uint Seconds);
		
		/// <summary>
		/// Retrieves the number of times the job was interrupted by 
		/// network failure or server unavailability
		/// </summary>
		/// <param name="Errors">Number of errors that occurred while BITS tried to transfer the job. The count increases when the job moves from the BG_JOB_STATE_TRANSFERRING state to the BG_JOB_STATE_TRANSIENT_ERROR or BG_JOB_STATE_ERROR state.</param>
		void GetErrorCount(out uint Errors);
		
		/// <summary>
		/// Specifies which proxy to use to transfer the files
		/// </summary>
		/// <param name="ProxyBypassList">Null-terminated string that contains an optional list of host names, IP addresses, or both, that can bypass the proxy. The list is space-delimited. For details on specifying a bypass proxy, see Remarks. This parameter must be NULL if the value of ProxyUsage is BG_JOB_PROXY_USAGE_PRECONFIG, BG_JOB_PROXY_USAGE_NO_PROXY, or BG_JOB_PROXY_USAGE_NO_AUTODETECT. The length of the proxy bypass list is limited to 4,000 characters, not including the null terminator. </param>
		/// <param name="ProxyList">Null-terminated string that contains the proxies to use to transfer files. The list is space-delimited. For details on specifying a proxy, see Remarks. This parameter must be NULL if the value of ProxyUsage is BG_JOB_PROXY_USAGE_PRECONFIG, BG_JOB_PROXY_USAGE_NO_PROXY, or BG_JOB_PROXY_USAGE_NO_AUTODETECT. The length of the proxy list is limited to 4,000 characters, not including the null terminator. </param>
		/// <param name="ProxyUsage">Specifies whether to use the user's proxy settings, not to use a proxy, or to use application-specified proxy settings. The default is to use the user's proxy settings, BG_JOB_PROXY_USAGE_PRECONFIG. For a list of proxy options, see the BG_JOB_PROXY_USAGE enumeration.</param>
		void SetProxySettings(BG_JOB_PROXY_USAGE ProxyUsage, [MarshalAs(UnmanagedType.LPWStr)] string ProxyList, [MarshalAs(UnmanagedType.LPWStr)] string ProxyBypassList);
		
		/// <summary>
		/// Retrieves the proxy settings the job uses to transfer the files
		/// </summary>
		/// <param name="pProxyBypassList">Null-terminated string that contains an optional list of host names or IP addresses, or both, that were not routed through the proxy. The list is space-delimited. For details on the format of the string, see the Listing the Proxy Bypass section of Enabling Internet Functionality. Call the CoTaskMemFree function to free ppProxyBypassList when done.</param>
		/// <param name="pProxyList">Null-terminated string that contains one or more proxies to use to transfer files. The list is space-delimited. For details on the format of the string, see the Listing Proxy Servers section of Enabling Internet Functionality. Call the CoTaskMemFree function to free ppProxyList when done.</param>
		/// <param name="pProxyUsage">Specifies the proxy settings the job uses to transfer the files. For a list of proxy options, see the BG_JOB_PROXY_USAGE enumeration. </param>
		void GetProxySettings(out BG_JOB_PROXY_USAGE pProxyUsage, [MarshalAs(UnmanagedType.LPWStr)] out string pProxyList, [MarshalAs(UnmanagedType.LPWStr)] out string pProxyBypassList);
		
		/// <summary>
		/// Changes the ownership of the job to the current user
		/// </summary>
		void TakeOwnership();
	}

	/// <summary>
	/// Use the IBackgroundCopyJob2 interface to retrieve reply data from an upload-reply job, determine the progress of the reply data transfer to the client, request command line execution, and provide credentials for proxy and remote server authentication requests.
	/// </summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("54B50739-686F-45EB-9DFF-D6A9A0FAA9AF")]
	internal interface IBackgroundCopyJob2
	{
		/// <summary>
		/// Adds multiple files to the job
		/// </summary>
		/// <param name="cFileCount">Number of elements in paFileSet. </param>
		/// <param name="pFileSet">Array of BG_FILE_INFO structures that identify the local and remote file names of the files to transfer.</param>
		void AddFileSet(uint cFileCount, [MarshalAs(UnmanagedType.LPArray)] BG_FILE_INFO[] pFileSet);

		/// <summary>
		/// Adds a single file to the job
		/// </summary>
		/// <param name="LocalName">Null-terminated string that contains the name of the file on the server. For information on specifying the remote name, see the RemoteName member and Remarks section of the BG_FILE_INFO structure. </param>
		/// <param name="RemoteUrl">Null-terminated string that contains the name of the file on the client. For information on specifying the local name, see the LocalName member and Remarks section of the BG_FILE_INFO structure. </param>
		void AddFile([MarshalAs(UnmanagedType.LPWStr)] string RemoteUrl, [MarshalAs(UnmanagedType.LPWStr)] string LocalName);

		/// <summary>
		/// Returns an interface pointer to an enumerator
		/// object that you use to enumerate the files in the job
		/// </summary>
		/// <param name="pEnum">IEnumBackgroundCopyFiles interface pointer that you use to enumerate the files in the job. Release ppEnumFiles when done. </param>
		void EnumFiles([MarshalAs(UnmanagedType.Interface)] out IEnumBackgroundCopyFiles pEnum);

		/// <summary>
		/// Pauses the job
		/// </summary>
		void Suspend();

		/// <summary>
		/// Restarts a suspended job
		/// </summary>
		void Resume();

		/// <summary>
		/// Cancels the job and removes temporary files from the client
		/// </summary>
		void Cancel();

		/// <summary>
		/// Ends the job and saves the transferred files on the client
		/// </summary>
		void Complete();

		/// <summary>
		/// Retrieves the identifier of the job in the queue
		/// </summary>
		/// <param name="pVal">GUID that identifies the job within the BITS queue.</param>
		void GetId(out Guid pVal);

		/// <summary>
		/// Retrieves the type of transfer being performed, 
		/// such as a file download
		/// </summary>
		/// <param name="pVal">Type of transfer being performed. For a list of transfer types, see the BG_JOB_TYPE enumeration type. </param>
		void GetType(out BG_JOB_TYPE pVal);

		/// <summary>
		/// Retrieves job-related progress information, 
		/// such as the number of bytes and files transferred 
		/// to the client
		/// </summary>
		/// <param name="pVal">Contains data that you can use to calculate the percentage of the job that is complete. For more information, see BG_JOB_PROGRESS. </param>
		void GetProgress(out BG_JOB_PROGRESS pVal);

		/// <summary>
		/// Retrieves timestamps for activities related
		/// to the job, such as the time the job was created
		/// </summary>
		/// <param name="pVal">Contains job-related time stamps. For available time stamps, see the BG_JOB_TIMES structure.</param>
		void GetTimes(out BG_JOB_TIMES pVal);

		/// <summary>
		/// Retrieves the state of the job
		/// </summary>
		/// <param name="pVal">Current state of the job. For example, the state reflects whether the job is in error, transferring data, or suspended. For a list of job states, see the BG_JOB_STATE enumeration type. </param>
		void GetState(out BG_JOB_STATE pVal);

		/// <summary>
		/// Retrieves an interface pointer to 
		/// the error object after an error occurs
		/// </summary>
		/// <param name="ppError">Error interface that provides the error code, a description of the error, and the context in which the error occurred. This parameter also identifies the file being transferred at the time the error occurred. Release ppError when done. </param>
		void GetError([MarshalAs(UnmanagedType.Interface)] out IBackgroundCopyError ppError);

		/// <summary>
		/// Retrieves the job owner's identity
		/// </summary>
		/// <param name="pVal">Null-terminated string that contains the string version of the SID that identifies the job's owner. Call the CoTaskMemFree function to free ppOwner when done. </param>
		void GetOwner([MarshalAs(UnmanagedType.LPWStr)] out string pVal);

		/// <summary>
		/// Specifies a display name that identifies the job in 
		/// a user interface
		/// </summary>
		/// <param name="Val">Null-terminated string that identifies the job. Must not be NULL. The length of the string is limited to 256 characters, not including the null terminator. </param>
		void SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string Val);

		/// <summary>
		/// Retrieves the display name that identifies the job
		/// </summary>
		/// <param name="pVal">Null-terminated string that contains the display name that identifies the job. More than one job can have the same display name. Call the CoTaskMemFree function to free ppDisplayName when done.</param>
		void GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pVal);

		/// <summary>
		/// Specifies a description of the job
		/// </summary>
		/// <param name="Val">Null-terminated string that provides additional information about the job. The length of the string is limited to 1,024 characters, not including the null terminator.</param>
		void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string Val);

		/// <summary>
		/// Retrieves the description of the job
		/// </summary>
		/// <param name="pVal">Null-terminated string that contains a short description of the job. Call the CoTaskMemFree function to free ppDescription when done. </param>
		void GetDescription([MarshalAs(UnmanagedType.LPWStr)] out string pVal);

		/// <summary>
		/// Specifies the priority of the job relative to 
		/// other jobs in the transfer queue
		/// </summary>
		/// <param name="Val">Specifies the priority level of your job relative to other jobs in the transfer queue. The default is BG_JOB_PRIORITY_NORMAL. For a list of priority levels, see the BG_JOB_PRIORITY enumeration. </param>
		void SetPriority(BG_JOB_PRIORITY Val);

		/// <summary>
		/// Retrieves the priority level you have set for the job.
		/// </summary>
		/// <param name="pVal">Priority of the job relative to other jobs in the transfer queue. </param>
		void GetPriority(out BG_JOB_PRIORITY pVal);

		/// <summary>
		/// Specifies the type of event notification to receive
		/// </summary>
		/// <param name="Val">Set one or more of the following flags to identify the events that you want to receive. </param>
		void SetNotifyFlags([MarshalAs(UnmanagedType.U4)] BG_JOB_NOTIFICATION_TYPE Val);

		/// <summary>
		/// Retrieves the event notification (callback) flags 
		/// you have set for your application.
		/// </summary>
		/// <param name="pVal">Identifies the events that your application receives. The following table lists the event notification flag values. </param>
		void GetNotifyFlags(out uint pVal);

		/// <summary>
		/// Specifies a pointer to your implementation of the 
		/// IBackgroundCopyCallback interface (callbacks). The 
		/// interface receives notification based on the event 
		/// notification flags you set
		/// </summary>
		/// <param name="Val">An IBackgroundCopyCallback interface pointer. To remove the current callback interface pointer, set this parameter to NULL.</param>
		void SetNotifyInterface([MarshalAs(UnmanagedType.IUnknown)] object Val);

		/// <summary>
		/// Retrieves a pointer to your implementation 
		/// of the IBackgroundCopyCallback interface (callbacks).
		/// </summary>
		/// <param name="pVal">Interface pointer to your implementation of the IBackgroundCopyCallback interface. When done, release ppNotifyInterface.</param>
		void GetNotifyInterface([MarshalAs(UnmanagedType.IUnknown)] out object pVal);

		/// <summary>
		/// Specifies the minimum length of time that BITS waits after 
		/// encountering a transient error condition before trying to 
		/// transfer the file
		/// </summary>
		/// <param name="Seconds">Minimum length of time, in seconds, that BITS waits after encountering a transient error before trying to transfer the file. The default retry delay is 600 seconds (10 minutes). The minimum retry delay that you can specify is 60 seconds. If you specify a value less than 60 seconds, BITS changes the value to 60 seconds. If the value exceeds the no-progress-timeout value retrieved from the GetNoProgressTimeout method, BITS will not retry the transfer and moves the job to the BG_JOB_STATE_ERROR state. </param>
		void SetMinimumRetryDelay(uint Seconds);

		/// <summary>
		/// Retrieves the minimum length of time that BITS waits after 
		/// encountering a transient error condition before trying to 
		/// transfer the file
		/// </summary>
		/// <param name="Seconds">Length of time, in seconds, that the service waits after encountering a transient error before trying to transfer the file. </param>
		void GetMinimumRetryDelay(out uint Seconds);

		/// <summary>
		/// Specifies the length of time that BITS continues to try to 
		/// transfer the file after encountering a transient error 
		/// condition
		/// </summary>
		/// <param name="Seconds">Length of time, in seconds, that BITS tries to transfer the file after the first transient error occurs. The default retry period is 1,209,600 seconds (14 days). Set the retry period to 0 to prevent retries and to force the job into the BG_JOB_STATE_ERROR state for all errors. If the retry period value exceeds the JobInactivityTimeout Group Policy value (90-day default), BITS cancels the job after the policy value is exceeded.</param>
		void SetNoProgressTimeout(uint Seconds);

		/// <summary>
		/// Retrieves the length of time that BITS continues to try to 
		/// transfer the file after encountering a transient error condition
		/// </summary>
		/// <param name="Seconds">Length of time, in seconds, that the service tries to transfer the file after a transient error occurs. </param>
		void GetNoProgressTimeout(out uint Seconds);

		/// <summary>
		/// Retrieves the number of times the job was interrupted by 
		/// network failure or server unavailability
		/// </summary>
		/// <param name="Errors">Number of errors that occurred while BITS tried to transfer the job. The count increases when the job moves from the BG_JOB_STATE_TRANSFERRING state to the BG_JOB_STATE_TRANSIENT_ERROR or BG_JOB_STATE_ERROR state.</param>
		void GetErrorCount(out ulong Errors);

		/// <summary>
		/// Specifies which proxy to use to transfer the files
		/// </summary>
		/// <param name="ProxyBypassList">Null-terminated string that contains an optional list of host names, IP addresses, or both, that can bypass the proxy. The list is space-delimited. For details on specifying a bypass proxy, see Remarks. This parameter must be NULL if the value of ProxyUsage is BG_JOB_PROXY_USAGE_PRECONFIG, BG_JOB_PROXY_USAGE_NO_PROXY, or BG_JOB_PROXY_USAGE_NO_AUTODETECT. The length of the proxy bypass list is limited to 4,000 characters, not including the null terminator. </param>
		/// <param name="ProxyList">Null-terminated string that contains the proxies to use to transfer files. The list is space-delimited. For details on specifying a proxy, see Remarks. This parameter must be NULL if the value of ProxyUsage is BG_JOB_PROXY_USAGE_PRECONFIG, BG_JOB_PROXY_USAGE_NO_PROXY, or BG_JOB_PROXY_USAGE_NO_AUTODETECT. The length of the proxy list is limited to 4,000 characters, not including the null terminator. </param>
		/// <param name="ProxyUsage">Specifies whether to use the user's proxy settings, not to use a proxy, or to use application-specified proxy settings. The default is to use the user's proxy settings, BG_JOB_PROXY_USAGE_PRECONFIG. For a list of proxy options, see the BG_JOB_PROXY_USAGE enumeration.</param>
		void SetProxySettings(BG_JOB_PROXY_USAGE ProxyUsage, [MarshalAs(UnmanagedType.LPWStr)] string ProxyList, [MarshalAs(UnmanagedType.LPWStr)] string ProxyBypassList);

		/// <summary>
		/// Retrieves the proxy settings the job uses to transfer the files
		/// </summary>
		/// <param name="pProxyBypassList">Null-terminated string that contains an optional list of host names or IP addresses, or both, that were not routed through the proxy. The list is space-delimited. For details on the format of the string, see the Listing the Proxy Bypass section of Enabling Internet Functionality. Call the CoTaskMemFree function to free ppProxyBypassList when done.</param>
		/// <param name="pProxyList">Null-terminated string that contains one or more proxies to use to transfer files. The list is space-delimited. For details on the format of the string, see the Listing Proxy Servers section of Enabling Internet Functionality. Call the CoTaskMemFree function to free ppProxyList when done.</param>
		/// <param name="pProxyUsage">Specifies the proxy settings the job uses to transfer the files. For a list of proxy options, see the BG_JOB_PROXY_USAGE enumeration. </param>
		void GetProxySettings(out BG_JOB_PROXY_USAGE pProxyUsage, [MarshalAs(UnmanagedType.LPWStr)] out string pProxyList, [MarshalAs(UnmanagedType.LPWStr)] out string pProxyBypassList);

		/// <summary>
		/// Changes the ownership of the job to the current user
		/// </summary>
		void TakeOwnership();

		/// <summary>
		/// Use the SetNotifyCmdLine method to specify a program to execute if the job enters the BG_JOB_STATE_ERROR or BG_JOB_STATE_TRANSFERRED state. BITS executes the program in the context of the user.
		/// </summary>
		/// <param name="Program">Null-terminated string that contains the program to execute. The pProgram parameter is limited to MAX_PATH characters, not including the null terminator. You should specify a full path to the program; the method will not use the search path to locate the program. To remove command line notification, set pProgram and pParameters to NULL. The method fails if pProgram is NULL and pParameters is non-NULL. </param>
		/// <param name="Parameters">Null-terminated string that contains the parameters of the program in pProgram. The first parameter must be the program in pProgram (use quotes if the path uses long file names). The pParameters parameter is limited to 4,000 characters, not including the null terminator. This parameter can be NULL.</param>
		void SetNotifyCmdLine([MarshalAs(UnmanagedType.LPWStr)] string Program, [MarshalAs(UnmanagedType.LPWStr)] string Parameters);

		/// <summary>
		/// Use the GetNotifyCmdLine method to retrieve the program to execute when the job enters the error or transferred state.
		/// </summary>
		/// <param name="pProgram">Null-terminated string that contains the program to execute when the job enters the error or transferred state. Call the CoTaskMemFree function to free pProgram when done. </param>
		/// <param name="pParameters">Null-terminated string that contains the arguments of the program in pProgram. Call the CoTaskMemFree function to free pParameters when done. </param>
		void GetNotifyCmdLine([MarshalAs(UnmanagedType.LPWStr)] out string pProgram, [MarshalAs(UnmanagedType.LPWStr)] out string pParameters);

		/// <summary>
		/// Use the GetReplyProgress method to retrieve progress information related to the transfer of the reply data from an upload-reply job.
		/// </summary>
		/// <param name="pProgress">Contains information that you use to calculate the percentage of the reply file transfer that is complete. For more information, see BG_JOB_REPLY_PROGRESS.</param>
		void GetReplyProgress([Out] out BG_JOB_REPLY_PROGRESS pProgress);

		/// <summary>
		/// Use the GetReplyData method to retrieve an in-memory copy of the reply data from the server application. Only call this method if the job's type is BG_JOB_TYPE_UPLOAD_REPLY and its state is BG_JOB_STATE_TRANSFERRED.
		/// </summary>
		/// <param name="ppBuffer">Buffer to contain the reply data. The method sets ppBuffer to NULL if the server application did not return a reply. Call the CoTaskMemFree function to free ppBuffer when done.</param>
		/// <param name="pLength">Size, in bytes, of the reply data in ppBuffer.</param>
		void GetReplyData(IntPtr ppBuffer, out ulong pLength);

		/// <summary>
		/// Use the SetReplyFileName method to specify the name of the file to contain the reply data from the server application. Only call this method if the job's type is BG_JOB_TYPE_UPLOAD_REPLY.
		/// </summary>
		/// <param name="ReplyFileName">Null-terminated string that contains the full path to the reply file. BITS generates the file name if ReplyFileNamePathSpec is NULL or an empty string. You cannot use wildcards in the path or file name, and directories in the path must exist. The path is limited to MAX_PATH, not including the null terminator. The user must have permissions to write to the directory. BITS does not support NTFS streams. Instead of using network drives, which are session specific, use UNC paths (for example, \\server\share\path\file). Do not include the \\? prefix in the path. </param>
		void SetReplyFileName([MarshalAs(UnmanagedType.LPWStr)] string ReplyFileName);

		/// <summary>
		/// Use the GetReplyFileName method to retrieve the name of the file that contains the reply data from the server application. Only call this method if the job type is BG_JOB_TYPE_UPLOAD_REPLY.
		/// </summary>
		/// <param name="pReplyFileName">Null-terminated string that contains the full path to the reply file. Call the CoTaskMemFree function to free pReplyFileName when done. </param>
		void GetReplyFileName([MarshalAs(UnmanagedType.LPWStr)] out string pReplyFileName);

		/// <summary>
		/// Use the SetCredentials method to specify the credentials to use for a proxy or remote server user authentication request.
		/// </summary>
		/// <param name="Credentials">Identifies the target (proxy or server), authentication scheme, and the user's credentials to use for user authentication. For details, see the BG_AUTH_CREDENTIALS structure. If the job currently contains credentials with the same target and scheme pair, the existing credentials are replaced with the new credentials. The credentials persist for the life of the job. To remove the credentials from the job, call the IBackgroundCopyJob2::RemoveCredentials method. </param>
		void SetCredentials([In] ref BG_AUTH_CREDENTIALS Credentials);

		/// <summary>
		/// Use the RemoveCredentials method to remove credentials from use. The credentials must match an existing target and scheme pair that you specified using the IBackgroundCopyJob2::SetCredentials method. There is no method to retrieve the credentials you have set.
		/// </summary>
		/// <param name="Target">Identifies whether to use the credentials for proxy or server authentication.</param>
		/// <param name="Scheme">Identifies the authentication scheme to use (basic or one of several challenge-response schemes). For details, see the BG_AUTH_SCHEME enumeration. </param>
		void RemoveCredentials(BG_AUTH_TARGET Target, BG_AUTH_SCHEME Scheme);
	}
    

	/// <summary>
	/// Use the information in the IBackgroundCopyError interface to 
	/// determine the cause of the error and if the transfer process 
	/// can proceed
	/// </summary>
	[GuidAttribute("19C613A0-FCB8-4F28-81AE-897C3D078F81")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImportAttribute()]
	internal interface IBackgroundCopyError {
		/// <summary>
		/// Retrieves the error code and identify the context 
		/// in which the error occurred
		/// </summary>
		/// <param name="pContext">Context in which the error occurred. For a list of context values, see the BG_ERROR_CONTEXT enumeration. </param>
		/// <param name="pCode">Error code of the error that occurred. </param>
		void GetError(out BG_ERROR_CONTEXT pContext, [MarshalAs(UnmanagedType.Error)] out int pCode);

		/// <summary>
		/// Retrieves an interface pointer to the file object 
		/// associated with the error
		/// </summary>
		/// <param name="pVal"> An IBackgroundCopyFile interface pointer whose methods you use to determine the local and remote file names associated with the error. The ppFile parameter is set to NULL if the error is not associated with the local or remote file. When done, release ppFile.</param>
		void GetFile([MarshalAs(UnmanagedType.Interface)] out IBackgroundCopyFile pVal);

		/// <summary>
		/// Retrieves the error text associated with the error
		/// </summary>
		/// <param name="LanguageId">Identifies the locale to use to generate the description. To create the language identifier, use the MAKELANGID macro.</param>
		/// <param name="pErrorDescription">Null-terminated string that contains the error text associated with the error. Call the CoTaskMemFree function to free ppErrorDescription when done.</param>
		void GetErrorDescription(uint LanguageId, [MarshalAs(UnmanagedType.LPWStr)] out string pErrorDescription);

		/// <summary>
		/// Retrieves a description of the context in which the error occurred
		/// </summary>
		/// <param name="LanguageId">Identifies the locale to use to generate the description. To create the language identifier, use the MAKELANGID macro.</param>
		/// <param name="pContextDescription">Null-terminated string that contains the description of the context in which the error occurred. Call the CoTaskMemFree function to free ppContextDescription when done. </param>
		void GetErrorContextDescription(uint LanguageId, [MarshalAs(UnmanagedType.LPWStr)] out string pContextDescription);

		/// <summary>
		/// Retrieves the protocol used to transfer the file
		/// </summary>
		/// <param name="pProtocol">Null-terminated string that contains the protocol used to transfer the file. The string contains http for the HTTP protocol and file for the SMB protocol. The ppProtocol parameter is set to NULL if the error is not related to the transfer protocol. Call the CoTaskMemFree function to free ppProtocol when done. </param>
		void GetProtocol([MarshalAs(UnmanagedType.LPWStr)] out string pProtocol);
	}

	/// <summary>
	/// Use the IEnumBackgroundCopyJobs interface to enumerate the list 
	/// of jobs in the transfer queue. To get an IEnumBackgroundCopyJobs 
	/// interface pointer, call the IBackgroundCopyManager::EnumJobs method
	/// </summary>
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	[GuidAttribute("1AF4F612-3B71-466F-8F58-7B6F73AC57AD")]
	[ComImportAttribute()]
	internal interface IEnumBackgroundCopyJobs {
		/// <summary>
		/// Retrieves a specified number of items in the enumeration sequence
		/// </summary>
		/// <param name="celt">Number of elements requested. </param>
		/// <param name="rgelt">Array of IBackgroundCopyJob objects. You must release each object in rgelt when done. </param>
		/// <param name="pceltFetched">Number of elements returned in rgelt. You can set pceltFetched to NULL if celt is one. Otherwise, initialize the value of pceltFetched to 0 before calling this method.</param>
		void Next(uint celt, [MarshalAs(UnmanagedType.Interface)] out IBackgroundCopyJob rgelt, out uint pceltFetched);

		/// <summary>
		/// Skips a specified number of items in the enumeration sequence
		/// </summary>
		/// <param name="celt">Number of elements to skip. </param>
		void Skip(uint celt);

		/// <summary>
		/// Resets the enumeration sequence to the beginning.
		/// </summary>
		void Reset();

		/// <summary>
		/// Creates another enumerator that contains the same 
		/// enumeration state as the current one
		/// </summary>
		/// <param name="ppenum">Receives the interface pointer to the enumeration object. If the method is unsuccessful, the value of this output variable is undefined. You must release ppEnumJobs when done.</param>
		void Clone([MarshalAs(UnmanagedType.Interface)] out IEnumBackgroundCopyJobs ppenum);

		/// <summary>
		/// Returns the number of items in the enumeration
		/// </summary>
		/// <param name="puCount">Number of jobs in the enumeration.</param>
		void GetCount(out uint puCount);
	}

	/// <summary>
	/// Use the IEnumBackgroundCopyFiles interface to enumerate the files 
	/// that a job contains. To get an IEnumBackgroundCopyFiles interface 
	/// pointer, call the IBackgroundCopyJob::EnumFiles method
	/// </summary>
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	[GuidAttribute("CA51E165-C365-424C-8D41-24AAA4FF3C40")]
	[ComImportAttribute()]
	internal interface IEnumBackgroundCopyFiles {
		/// <summary>
		/// Retrieves a specified number of items in the enumeration sequence
		/// </summary>
		/// <param name="celt">Number of elements requested. </param>
		/// <param name="rgelt">Array of IBackgroundCopyFile objects. You must release each object in rgelt when done.</param>
		/// <param name="pceltFetched">Number of elements returned in rgelt. You can set pceltFetched to NULL if celt is one. Otherwise, initialize the value of pceltFetched to 0 before calling this method. </param>
		void Next(uint celt, [MarshalAs(UnmanagedType.Interface)] out IBackgroundCopyFile rgelt, out uint pceltFetched);
		
		/// <summary>
		/// Skips a specified number of items in the enumeration sequence
		/// </summary>
		/// <param name="celt">Number of elements to skip.</param>
		void Skip(uint celt);

		/// <summary>
		/// Resets the enumeration sequence to the beginning
		/// </summary>
		void Reset();

		/// <summary>
		/// Creates another enumerator that contains the same 
		/// enumeration state as the current enumerator
		/// </summary>
		/// <param name="ppenum">Receives the interface pointer to the enumeration object. If the method is unsuccessful, the value of this output variable is undefined. You must release ppEnumFiles when done.</param>
		void Clone([MarshalAs(UnmanagedType.Interface)] out IEnumBackgroundCopyFiles ppenum);

		/// <summary>
		/// Retrieves the number of items in the enumeration
		/// </summary>
		/// <param name="puCount">Number of files in the enumeration.</param>
		void GetCount(out uint puCount);
	}

	/// <summary>
	///  The IBackgroundCopyFile interface contains information about a file 
	///  that is part of a job. For example, you can use the interfaces methods
	///  to retrieve the local and remote names of the file and transfer progress
	///  information
	/// </summary>
	[GuidAttribute("01B7BD23-FB88-4A77-8490-5891D3E4653A")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImportAttribute()]
	internal interface IBackgroundCopyFile {
		/// <summary>
		/// Retrieves the remote name of the file
		/// </summary>
		/// <param name="pVal">Null-terminated string that contains the remote name of the file to transfer. The name is fully qualified. Call the CoTaskMemFree function to free ppName when done. </param>
		void GetRemoteName([MarshalAs(UnmanagedType.LPWStr)] out string pVal);

		/// <summary>
		/// Retrieves the local name of the file
		/// </summary>
		/// <param name="pVal">Null-terminated string that contains the name of the file on the client. The name is fully qualified. Call the CoTaskMemFree function to free ppName when done. </param>
		void GetLocalName([MarshalAs(UnmanagedType.LPWStr)] out string pVal);
		
		/// <summary>
		/// Retrieves the progress of the file transfer
		/// </summary>
		/// <param name="pVal">Structure whose members indicate the progress of the file transfer. For details on the type of progress information available, see the BG_FILE_PROGRESS structure.</param>
		void GetProgress(out _BG_FILE_PROGRESS pVal);
	}

	/// <summary>
	/// Authentication scheme used for the background job
	/// </summary>
	public enum BG_AUTH_SCHEME {
		/// <summary>
		/// Basic is a scheme in which the user name and password are sent in clear-text to the server or proxy. 
		/// </summary>
		BG_AUTH_SCHEME_BASIC = 1,

		/// <summary>
		/// Digest is a challenge-response scheme that uses a server-specified data string for the challenge.
		/// </summary>
		BG_AUTH_SCHEME_DIGEST = 2,

		/// <summary>
		/// Windows NT LAN Manager (NTLM) is a challenge-response scheme that uses the credentials of the user for authentication in a Windows network environment.
		/// </summary>
		BG_AUTH_SCHEME_NTLM = 3,

		/// <summary>
		/// Simple and Protected Negotiation protocol (Snego) is a challenge-response scheme that negotiates with the server or proxy to determine which scheme to use for authentication. Examples are the Kerberos protocol, Secure Socket Layer (SSL), and NTLM. 
		/// </summary>
		BG_AUTH_SCHEME_NEGOTIATE = 4,

		/// <summary>
		/// Passport is a centralized authentication service provided by Microsoft that offers a single logon for member sites.
		/// </summary>
		BG_AUTH_SCHEME_PASSPORT = 5,
	}

	/// <summary>
	/// The location from which to download the code.
	/// </summary>
	public enum BG_AUTH_TARGET {
		/// <summary>
		/// Use credentials for server requests. 
		/// </summary>
		BG_AUTH_TARGET_SERVER = 1,

		/// <summary>
		/// Use credentials for proxy requests. 
		/// </summary>
		BG_AUTH_TARGET_PROXY = 2,
	}

	/// <summary>
	/// The BG_JOB_STATE enumeration type defines constant values for the 
	/// different states of a job
	/// </summary>
	internal enum BG_JOB_STATE {
		/// <summary>
		/// Specifies that the job is in the queue and waiting to run. 
		/// If a user logs off while their job is transferring, the job 
		/// transitions to the queued state
		/// </summary>
		BG_JOB_STATE_QUEUED = 0,

		/// <summary>
		/// Specifies that BITS is trying to connect to the server. If the 
		/// connection succeeds, the state of the job becomes 
		/// BG_JOB_STATE_TRANSFERRING; otherwise, the state becomes 
		/// BG_JOB_STATE_TRANSIENT_ERROR
		/// </summary>
		BG_JOB_STATE_CONNECTING = 1,

		/// <summary>
		/// Specifies that BITS is transferring data for the job
		/// </summary>
		BG_JOB_STATE_TRANSFERRING = 2,

		/// <summary>
		/// Specifies that the job is suspended (paused)
		/// </summary>
		BG_JOB_STATE_SUSPENDED = 3,

		/// <summary>
		/// Specifies that a non-recoverable error occurred (the service is 
		/// unable to transfer the file). When the error can be corrected, 
		/// such as an access-denied error, call the IBackgroundCopyJob::Resume 
		/// method after the error is fixed. However, if the error cannot be 
		/// corrected, call the IBackgroundCopyJob::Cancel method to cancel 
		/// the job, or call the IBackgroundCopyJob::Complete method to accept 
		/// the portion of a download job that transferred successfully.
		/// </summary>
		BG_JOB_STATE_ERROR = 4,

		/// <summary>
		/// Specifies that a recoverable error occurred. The service tries to 
		/// recover from the transient error until the retry time value that 
		/// you specify using the IBackgroundCopyJob::SetNoProgressTimeout method 
		/// expires. If the retry time expires, the job state changes to 
		/// BG_JOB_STATE_ERROR
		/// </summary>
		BG_JOB_STATE_TRANSIENT_ERROR = 5,

		/// <summary>
		/// Specifies that your job was successfully processed
		/// </summary>
		BG_JOB_STATE_TRANSFERRED = 6,

		/// <summary>
		/// Specifies that you called the IBackgroundCopyJob::Complete method 
		/// to acknowledge that your job completed successfully
		/// </summary>
		BG_JOB_STATE_ACKNOWLEDGED = 7,

		/// <summary>
		/// Specifies that you called the IBackgroundCopyJob::Cancel method to 
		/// cancel the job (remove the job from the transfer queue)
		/// </summary>
		BG_JOB_STATE_CANCELLED = 8,

		/// <summary>
		/// This is custom state not provided by BITS to indicate that an Update
		/// is available for the application.
		/// </summary>
		BG_JOB_STATE_UPDATE_AVAILABLE = 1001, // This is not provided by BITS but is Custom

		/// <summary>
		/// This is custom state not provided by BITS to indicate that an validation
		/// of the application files was successful.
		/// </summary>
		BG_JOB_STATE_VALIDATION_SUCCESS = 1002,

		/// <summary>
		/// This is custom state not provided by BITS to indicate that an validation
		/// of the application files was failed.
		/// </summary>
		BG_JOB_STATE_VALIDATION_FAILED = 1003,
	}

	/// <summary>
	/// The BG_JOB_TYPE enumeration type defines constant values that you 
	/// use to specify the type of transfer job, such as download
	/// </summary>
	internal  enum BG_JOB_TYPE {
		/// <summary>
		/// Specifies that the job downloads files to the client
		/// </summary>
		BG_JOB_TYPE_DOWNLOAD = 0,
	}

	/// <summary>
	/// Used for the SetNotifyFlags method.
	/// </summary>
	[Flags]
	internal enum BG_JOB_NOTIFICATION_TYPE : uint {
		/// <summary>
		/// All of the files in the job have been transferred.
		/// </summary>
		BG_NOTIFY_JOB_TRANSFERRED = 0x0001,

		/// <summary>
		/// An error has occurred.
		/// </summary>
		BG_NOTIFY_JOB_ERROR = 0x0002,

		/// <summary>
		/// Event notification is disabled. BITS ignores the other flags.
		/// </summary>
		BG_NOTIFY_DISABLE = 0x0004,

		/// <summary>
		/// The job has been modified. For example, a property value changed, the state of the job changed, or progress is made transferring the files. This flag is ignored if command line notification is specified.
		/// </summary>
		BG_NOTIFY_JOB_MODIFICATION = 0x0008,
	}


	/// <summary>
	/// The BG_JOB_PROXY_USAGE enumeration type defines constant values 
	/// that you use to specify which proxy to use for file transfers
	/// </summary>
	internal enum BG_JOB_PROXY_USAGE {
		/// <summary>
		/// Use the proxy and proxy bypass list settings defined by each 
		/// user to transfer files
		/// </summary>
		BG_JOB_PROXY_USAGE_PRECONFIG = 0,

		/// <summary>
		/// Do not use a proxy to transfer files
		/// </summary>
		BG_JOB_PROXY_USAGE_NO_PROXY = 1,

		/// <summary>
		/// Use the application's proxy and proxy bypass list to transfer files
		/// </summary>
		BG_JOB_PROXY_USAGE_OVERRIDE = 2,
		/// <summary>
		/// Automatically detect proxy settings. 
		/// BITS detects proxy settings for each file in the job
		/// </summary>
		BG_JOB_PROXY_USAGE_AUTODETECT = 3,
	}

	
	/// <summary>
	/// The BG_JOB_PRIORITY enumeration type defines the constant values 
	/// that you use to specify the priority level of the job
	/// </summary>
	internal enum BG_JOB_PRIORITY {
		/// <summary>
		/// Transfers the job in the foreground
		/// </summary>
		BG_JOB_PRIORITY_FOREGROUND = 0,

		/// <summary>
		/// Transfers the job in the background. This is the highest background 
		/// priority level. 
		/// </summary>
		BG_JOB_PRIORITY_HIGH = 1,

		/// <summary>
		/// Transfers the job in the background. This is the default priority 
		/// level for a job
		/// </summary>
		BG_JOB_PRIORITY_NORMAL = 2,

		/// <summary>
		/// Transfers the job in the background. This is the lowest background 
		/// priority level
		/// </summary>
		BG_JOB_PRIORITY_LOW = 3,
	}

	/// <summary>
	/// The BG_AUTH_CREDENTIALS structure identifies the target (proxy or server), authentication scheme, and the user's credentials to use for user authentication requests. The structure is passed to the IBackgroundCopyJob2::SetCredentials method.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Size = 16)]
	internal struct BG_AUTH_CREDENTIALS {
		/// <summary>
		/// Identifies whether to use the credentials for a proxy or server authentication request. For a list of values, see the BG_AUTH_TARGET enumeration.
		/// </summary>
		public BG_AUTH_TARGET Target;

		/// <summary>
		/// Identifies the scheme to use for authentication (for example, Basic or NTLM). For a list of values, see the BG_AUTH_SCHEME enumeration. 
		/// </summary>
		public BG_AUTH_SCHEME Scheme;

		/// <summary>
		/// Identifies the credentials to use for the specified authentication scheme. For details, see the BG_AUTH_CREDENTIALS_UNION union.
		/// </summary>
		public BG_AUTH_CREDENTIALS_UNION Credentials;
	}

	/// <summary>
	/// The BG_AUTH_CREDENTIALS_UNION union identifies the credentials to use for the authentication scheme specified in the BG_AUTH_CREDENTIALS structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Size = 8)]
	internal struct BG_AUTH_CREDENTIALS_UNION {
		/// <summary>
		/// Identifies the user name and password of the user to authenticate. For details, see the BG_BASIC_CREDENTIALS structure.
		/// </summary>
		public BG_BASIC_CREDENTIALS Basic;
	}

	/// <summary>
	/// The BG_BASIC_CREDENTIALS structure identifies the user name and password to authenticate.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Size = 8)]
	internal struct BG_BASIC_CREDENTIALS {
		/// <summary>
		/// Null-terminated string that contains the user name to authenticate. The user name is limited to 300 characters, not including the null terminator. The format of the user name depends on the authentication scheme requested. For example, for Basic, NTLM, and Negotiate authentication, the user name is of the form "domain\user name" or "user name". For Passport authentication, the user name is an e-mail address. If NULL, default credentials for this session context are used. 
		/// </summary>
		[MarshalAs(UnmanagedType.LPWStr)]
		public string UserName;

		/// <summary>
		/// Null-terminated string that contains the password in clear-text. The password is limited to 300 characters, not including the null terminator. The password can be blank. Set to NULL if UserName is NULL. BITS encrypts the password before persisting the job if a network disconnect occurs or the user logs off. 
		/// </summary>
		[MarshalAs(UnmanagedType.LPWStr)]
		public string Password;
	}

	/// <summary>
	/// The BG_ERROR_CONTEXT enumeration type defines the constant values 
	/// that specify the context in which the error occurred
	/// </summary>
	internal enum BG_ERROR_CONTEXT {
		/// <summary>
		/// An error has not occurred
		/// </summary>
		BG_ERROR_CONTEXT_NONE = 0,

		/// <summary>
		/// The error context is unknown
		/// </summary>
		BG_ERROR_CONTEXT_UNKNOWN = 1,

		/// <summary>
		/// The transfer queue manager generated the error
		/// </summary>
		BG_ERROR_CONTEXT_GENERAL_QUEUE_MANAGER = 2,

		/// <summary>
		/// The error was generated while the queue manager was 
		/// notifying the client of an event
		/// </summary>
		BG_ERROR_CONTEXT_QUEUE_MANAGER_NOTIFICATION = 3,

		/// <summary>
		/// The error was related to the specified local file. For example, 
		/// permission was denied or the volume was unavailable
		/// </summary>
		BG_ERROR_CONTEXT_LOCAL_FILE = 4,

		/// <summary>
		/// The error was related to the specified remote file. 
		/// For example, the URL is not accessible
		/// </summary>
		BG_ERROR_CONTEXT_REMOTE_FILE = 5,

		/// <summary>
		/// The transport layer generated the error. These errors are general 
		/// transport failures; errors not specific to the remote file
		/// </summary>
		BG_ERROR_CONTEXT_GENERAL_TRANSPORT = 6,
	}

	/// <summary>
	/// The BG_FILE_INFO structure provides the local and 
	/// remote names of the file to transfer
	/// </summary>
	[StructLayoutAttribute(LayoutKind.Sequential, Pack=4, Size=0)]
	internal struct BG_FILE_INFO {
		/// <summary>
		/// Remote Name for the File
		/// </summary>
		[MarshalAs(UnmanagedType.LPWStr)]
		public string RemoteName;

		/// <summary>
		/// Local Name for the file
		/// </summary>
		[MarshalAs(UnmanagedType.LPWStr)]
		public string LocalName;
	}

	/// <summary>
	/// The BG_JOB_PROGRESS structure provides job-related progress information, 
	/// such as the number of bytes and files transferred
	/// </summary>
	[StructLayoutAttribute(LayoutKind.Sequential, Pack=8, Size=0)]
	internal struct BG_JOB_PROGRESS {
		/// <summary>
		/// Total number of bytes to transfer for the job.
		/// </summary>
		public ulong BytesTotal;

		/// <summary>
		/// Number of bytes transferred
		/// </summary>
		public ulong BytesTransferred;

		/// <summary>
		/// Total number of files to transfer for this job
		/// </summary>
		public uint FilesTotal;

		/// <summary>
		/// Number of files transferred. 
		/// </summary>
		public uint FilesTransferred;
	}

	/// <summary>
	/// The BG_JOB_REPLY_PROGRESS structure provides progress information related to the reply portion of an upload-reply job.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack=8)]
	internal struct BG_JOB_REPLY_PROGRESS {
		/// <summary>
		/// Size of the file in bytes. The value is BG_SIZE_UNKNOWN if the reply has not begun. 
		/// </summary>
		public ulong BytesTotal;

		/// <summary>
		/// Number of bytes transferred. 
		/// </summary>
		public ulong BytesTransferred;
	}

	/// <summary>
	/// The BG_JOB_TIMES structure provides job-related timestamps
	/// </summary>
	[StructLayoutAttribute(LayoutKind.Sequential, Pack=4, Size=0)]
	internal struct BG_JOB_TIMES {
		/// <summary>
		/// Time the job was created
		/// </summary>
		public _FILETIME CreationTime;

		/// <summary>
		/// Time the job was last modified or bytes were transferred
		/// </summary>
		public _FILETIME ModificationTime;

		/// <summary>
		/// Time the job entered the BG_JOB_STATE_TRANSFERRED state
		/// </summary>
		public _FILETIME TransferCompletionTime;
	}

	
	/// <summary>
	/// This structure is a 64-bit value representing the number of 100-nanosecond intervals since January 1, 1601. 
	/// </summary>
	[StructLayoutAttribute(LayoutKind.Sequential, Pack=4, Size=0)]
	internal struct _FILETIME {
		/// <summary>
		/// Specifies the low 32 bits of the file time. 
		/// </summary>
		public uint dwLowDateTime;

		/// <summary>
		/// Specifies the high 32 bits of the file time. 
		/// </summary>
		public uint dwHighDateTime;
	}

	/// <summary>
	/// The BG_FILE_PROGRESS structure provides file-related progress information, 
	/// such as the number of bytes transferred
	/// </summary>
	[StructLayoutAttribute(LayoutKind.Sequential, Pack=8, Size=0)]
	internal struct _BG_FILE_PROGRESS {
		/// <summary>
		/// Size of the file in bytes
		/// </summary>
		public ulong BytesTotal;

		/// <summary>
		/// Number of bytes transferred. 
		/// </summary>
		public ulong BytesTransferred;

		/// <summary>
		/// For downloads, the value is TRUE if the file is available to the user; 
		/// otherwise, the value is FALSE
		/// </summary>
		public int Completed;
	}
}
