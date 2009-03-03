#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Threading;
using System.Configuration;
using NewsComponents.Collections;
using NewsComponents.Utils;
using RssBandit.Common.Logging;

namespace NewsComponents.Net
{
	/// <summary>
	/// RequestThread runs a thread to manage queued requests. 
	/// </summary>
	internal class RequestThread {

		#region Constants

		/// <summary>
		/// Default maximum number of concurrent async requests per instance.
		/// For debugging multithreaded problems, set this to 1 (one). 
		/// Please remember: it can also be set by the RssBandit.exe.config file!
		/// </summary>
		private const int DefaultMaxDownloadThreads = 4;
		
		#endregion

		#region Member Variables
		/// <summary>
		/// Maximum number of concurrent async requests per instance.
		/// Please remember: it can also be set by the RssBandit.exe.config file!
		/// </summary>
		private static volatile int _maxRequests = DefaultMaxDownloadThreads;	
		private  readonly log4net.ILog _log = Log.GetLogger(typeof(RequestThread));
		/// <summary>Queue of all the requests waiting to be executed.</summary>
		private readonly PriorityQueue _waitingRequests;
		/// <summary>Number of requests currently active.</summary>
		private  volatile int _runningRequests;
		/// <summary>
		/// The AsyncWebRequest associated with this object. 
		/// </summary>
		private readonly AsyncWebRequest myAsyncWebRequest;
	
		#endregion

		#region Construction
		/// <summary>Initialize the request thread.</summary>
		internal RequestThread(AsyncWebRequest asyncWebRequest) 
		{
			myAsyncWebRequest = asyncWebRequest;
			_waitingRequests = new PriorityQueue();
			_runningRequests = 0;

			try
			{
				int newMax = Common.Configuration.ReadAppSettingsEntry("MaxDownloadThreads", DefaultMaxDownloadThreads);
				if (newMax > 0 && newMax < 50)
				{
					_maxRequests = newMax;
				}
			} 
			catch (ConfigurationErrorsException cex)
			{
				_log.Error("Failed to read 'MaxDownloadThreads' from .config", cex);
			}

			ThreadPool.QueueUserWorkItem(this.Run);
		}
		#endregion

		#region Public methods
		public int RunningRequests {
			get {
				lock(_waitingRequests) {
					return _runningRequests;
				}
			}
		}		

		public void QueueRequest(RequestState state, int priority) {
			lock(_waitingRequests) {
				_waitingRequests.Enqueue(priority, state);
				if (_runningRequests < _maxRequests)
					Monitor.Pulse(_waitingRequests);
			}
		}


		public  void EndRequest(RequestState state) {
			lock(_waitingRequests) {
				_runningRequests--;
				if (_runningRequests < _maxRequests)
					Monitor.Pulse(_waitingRequests);
			}
		}

		#endregion

		#region private methods
		private  void Run(object ignored) {
			
			lock(_waitingRequests) { // wait for queued items
			_wait001:
				try {
				
					while ((_waitingRequests.Count == 0) || (_runningRequests >= _maxRequests)) {
						Monitor.Wait(_waitingRequests);
					}

					RequestState state = (RequestState) _waitingRequests.Dequeue();
					try {
						// next call returns true if the real request should be cancelled 
						// (e.g. if no internet connection available)
						if (state.OnRequestStart()) {	
							// signal this state to the worker class
							myAsyncWebRequest.RequestStartCancelled(state);
							goto _wait001;
						}
					}
					catch (Exception signalException) {
						_log.Error("Error during dispatch of StartDownloadCallBack()", signalException);
					}
					state.StartTime = DateTime.Now;
					_runningRequests++;

					try {
						_log.Debug("calling BeginGetResponse for " + state.Request.RequestUri);
						IAsyncResult result = state.Request.BeginGetResponse(myAsyncWebRequest.ResponseCallback, state);
						ThreadPool.RegisterWaitForSingleObject (result.AsyncWaitHandle, myAsyncWebRequest.TimeoutCallback, state, state.Request.Timeout, true);
					}
					catch (Exception responseException) {
						state.OnRequestException(responseException);
						myAsyncWebRequest.FinalizeWebRequest(state);

					}
					goto _wait001;

				} catch (Exception ex) {
					_log.Fatal("Critical exception caught in RequestThread.Run()!", ex);
				}
				goto _wait001;
			}
		}

		#endregion
	}
}
