#region CVS Version Header
/*
 * $Id: RequestThread.cs,v 1.7 2006/12/19 04:39:52 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2006/12/19 04:39:52 $
 * $Revision: 1.7 $
 */
#endregion

using System;
using System.Threading;
using System.Net;
using System.Configuration;
using System.Diagnostics;

using NewsComponents.Collections;

namespace NewsComponents.Net
{
	/// <summary>
	/// RequestThread runs a thread to manage queued requests. 
	/// </summary>
	internal class RequestThread {

		#region Constants
		/// <summary>
		/// Maximum number of concurrent async requests.
		/// For debugging multithreaded problems, set this to 1 (one). 
		/// Please remember: it can also be updated by the RssBandit.exe.config file!
		/// </summary>
		private static volatile int _maxRequests = 8;	
		#endregion

		#region Member Variables
		private  readonly log4net.ILog _log = RssBandit.Common.Logging.Log.GetLogger(typeof(RequestThread));
		/// <summary>Queue of all the requests waiting to be executed.</summary>
		private  PriorityQueue _waitingRequests;
		/// <summary>Number of requests currently active.</summary>
		private  volatile int _runningRequests;
		/// <summary>
		/// The AsyncWebRequest associated with this object. 
		/// </summary>
		private AsyncWebRequest myAsyncWebRequest = null;
	
		#endregion

		#region Construction
		/// <summary>Initialize the request thread.</summary>
		internal RequestThread(AsyncWebRequest asyncWebRequest) {
			myAsyncWebRequest = asyncWebRequest;
			_waitingRequests = new PriorityQueue();
			_runningRequests = 0;

			string maxWorkerThreadsFromConfig = ConfigurationSettings.AppSettings["MaxDownloadThreads"];
			if (maxWorkerThreadsFromConfig != null && maxWorkerThreadsFromConfig.Length > 0) {
				try {
					int newMax = Convert.ToInt32(maxWorkerThreadsFromConfig);
					if (newMax > 0 && newMax < 50) {
						_maxRequests = newMax;
					}
				} catch {}
			}
			Thread thread = new Thread(new ThreadStart(Run));
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.Normal;
			thread.Start();
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
		private  void Run() {
			
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
						IAsyncResult result = state.Request.BeginGetResponse(new AsyncCallback(myAsyncWebRequest.ResponseCallback), state);
						ThreadPool.RegisterWaitForSingleObject (result.AsyncWaitHandle, new WaitOrTimerCallback(myAsyncWebRequest.TimeoutCallback), state, state.Request.Timeout, true);
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

			#region OldCode
//			RequestState state = null;
//			bool requestsQueued = false;
//
//			try {
//				goto waitOrNextRequest;
// 
//			waitForNextPulse:
//				lock(_waitingRequests) { // wait for queued items
//					Monitor.Wait(_waitingRequests);
//				}
//
//			waitOrNextRequest:
//				if (_runningRequests >= _maxRequests) {	// no request start, if max. concurrent threads is reached
//					goto waitForNextPulse;
//				}
//
//				lock(_waitingRequests) {
//					state = null;
//					requestsQueued = (_waitingRequests.Count > 0);	// any queue item?
//					if (requestsQueued) {	// yes, get it
//						state = ((RequestState)_waitingRequests.Dequeue());
//					}
//				}
//
//				if (!requestsQueued) {
//					goto waitForNextPulse;
//				}
//
//				try {
//					if (state == null )
//						goto waitOrNextRequest;
//					// next call returns true if the real request should be cancelled 
//					// (e.g. if no internet connection available)
//					if (state.OnRequestStart()) {	
//						// signal this state to the worker class
//						AsyncWebRequest.RequestStartCancelled(state);
//						goto waitOrNextRequest;
//					}
//				}
//				catch (Exception e) {
//					Trace.WriteLine("Error during dispatch of OnRequestStart() callback", e.ToString());
//				}
//					
//				try {
//
//					// start async request:
//					IAsyncResult result = state.Request.BeginGetResponse(new AsyncCallback(AsyncWebRequest.ResponseCallback), state);
//					ThreadPool.RegisterWaitForSingleObject (result.AsyncWaitHandle, new WaitOrTimerCallback(AsyncWebRequest.TimeoutCallback), state, state.Request.Timeout, true);
//					state.StartTime = DateTime.Now;	
//					Interlocked.Increment(ref _runningRequests);
//
//				}
//				catch (WebException we) {	// abort called. Timeout
//					state.OnRequestException(we);
//					AsyncWebRequest.FinalizeWebRequest(state);
//				}
//				catch (Exception e) {
//					state.OnRequestException(e);
//					AsyncWebRequest.FinalizeWebRequest(state);
//				}
//
//				goto waitOrNextRequest;
//
//			}
//			catch (Exception ex) {
//				Trace.WriteLine("Critical exception caught in RequestThread.Run():" + ex.ToString());
//			}
 
			#endregion
		}

		#endregion
	}


	#region OldClass impl. (for ref)
	/// <summary>
	/// RequestThread is an alternative to PriorityThreadPool. It just start one
	/// Thread (initiator), that runs only if any worker thread that is activated 
	/// after calling async method WebRequest.BeginGetResponse()
	/// finishes or throws an exeption. So they have to call Monitor.Pulse via 
	/// RequestThread.EndRequest tomake it work correctly.
	/// </summary>
	internal class RequestThread2
	{
		#region Constants
		/// <summary>
		/// Maximum number of concurrent async requests.
		/// For debugging multithreaded problems, set this to 1 (one). 
		/// Please remember: it can also be updated by the RssBandit.exe.config file!
		/// </summary>
		private static int _maxRequests = 8;	
		#endregion

		#region Member Variables
		private static readonly log4net.ILog _log = RssBandit.Common.Logging.Log.GetLogger(typeof(RequestThread));
		/// <summary>Queue of all the requests waiting to be executed.</summary>
		static PriorityQueue _waitingRequests;
		/// <summary>Number of requests currently active.</summary>
		static int _runningRequests;
		private static AsyncWebRequest myAsyncWebRequest = null;		
		#endregion

		#region Construction
		/// <summary>Initialize the request thread.</summary>
		static RequestThread2()
		{
			_waitingRequests = new PriorityQueue();
			_runningRequests = 0;

			string maxWorkerThreadsFromConfig = ConfigurationSettings.AppSettings["MaxDownloadThreads"];
			if (maxWorkerThreadsFromConfig != null && maxWorkerThreadsFromConfig.Length > 0) {
				try {
					int newMax = Convert.ToInt32(maxWorkerThreadsFromConfig);
					if (newMax > 0 && newMax < 50) {
						_maxRequests = newMax;
					}
				} catch {}
			}
			Thread thread = new Thread(new ThreadStart(Run));
			thread.ApartmentState = ApartmentState.MTA;
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.Normal;
			thread.Start();
		}
		#endregion

		#region Public methods
		public static int RunningRequests {
			get {
				lock(_waitingRequests) {
					return _runningRequests;
				}
			}
		}		

		public static void QueueRequest(RequestState state, int priority) {
			lock(_waitingRequests) {
				_waitingRequests.Enqueue(priority, state);
				Monitor.Pulse(_waitingRequests);
			}
		}

		public static void TryActivateNext() {
			lock(_waitingRequests) {
				Monitor.Pulse(_waitingRequests);
 			}
		}

		public static void EndRequest(RequestState state) {
			lock(_waitingRequests) {
				Interlocked.Decrement(ref _runningRequests);
				Monitor.Pulse(_waitingRequests);
			}
		}

		#endregion

		#region private methods
		private static void Run() {
			
			RequestState state = null;
			bool requestsQueued = false;

			try {
				goto waitOrNextRequest;
 
waitForNextPulse:
				lock(_waitingRequests) { // wait for queued items
					Monitor.Wait(_waitingRequests);
				}

waitOrNextRequest:
				if (_runningRequests >= _maxRequests) {	// no request start, if max. concurrent threads is reached
					goto waitForNextPulse;
				}

				lock(_waitingRequests) {
					state = null;
					requestsQueued = (_waitingRequests.Count > 0);	// any queue item?
					if (requestsQueued) {	// yes, get it
						state = ((RequestState)_waitingRequests.Dequeue());
					}
				}

				if (!requestsQueued) {
					goto waitForNextPulse;
				}

				try {
					if (state == null )
						goto waitOrNextRequest;
					// next call returns true if the real request should be cancelled 
					// (e.g. if no internet connection available)
					if (state.OnRequestStart()) {	
						// signal this state to the worker class
						myAsyncWebRequest.RequestStartCancelled(state);
						goto waitOrNextRequest;
					}
				}
				catch (Exception e) {
					Trace.WriteLine("Error during dispatch of OnRequestStart() callback", e.ToString());
				}
					
				try {

					// start async request:
					IAsyncResult result = state.Request.BeginGetResponse(new AsyncCallback(myAsyncWebRequest.ResponseCallback), state);
					ThreadPool.RegisterWaitForSingleObject (result.AsyncWaitHandle, new WaitOrTimerCallback(myAsyncWebRequest.TimeoutCallback), state, state.Request.Timeout, true);
					state.StartTime = DateTime.Now;	
					Interlocked.Increment(ref _runningRequests);

				}
				catch (WebException we) {	// abort called. Timeout
					state.OnRequestException(we);
					myAsyncWebRequest.FinalizeWebRequest(state);
				}
				catch (Exception e) {
					state.OnRequestException(e);
					myAsyncWebRequest.FinalizeWebRequest(state);
				}

				goto waitOrNextRequest;

			}
			catch (Exception ex) {
				Trace.WriteLine("Critical exception caught in RequestThread.Run():" + ex.ToString());
			}
 

		}

		#region experimental code
//		/// <summary>
//		/// Experimental version of the method above. It impl. it's own
//		/// timeout behavior as described in the docs to HttpWebRequest.BeginGetResponse().
//		/// It works for one request, but blocks for multiple. Can someone figure out why?
//		/// </summary>
//		private static void RunEx() {
//			
//			RequestState state = null;
//			bool requestsQueued = false;
//
//
//				try {
//					goto waitOrNextRequest;
// 
//				waitForNextPulse:
//					lock(_waitingRequests) {
//						Monitor.Wait(_waitingRequests);
//					}
// 
//				waitOrNextRequest:
//					if (_runningRequests >= _maxRequests) {
//						goto waitForNextPulse;
//					}
//
//					lock(_waitingRequests) {
//						state = null;
//						requestsQueued = (_waitingRequests.Count > 0);
//						if (requestsQueued) {
//							state = ((RequestState)_waitingRequests.Dequeue());
//						}
//					}
//
//					if (!requestsQueued) {
//						goto waitForNextPulse;
//					}
//
//					try {
//						if (state == null )
//							goto waitOrNextRequest;
//						if (state.OnRequestStart()) {	// cancelled (e.g. if no internet connection available)
//							AsyncWebRequest.RequestStartCancelled(state);
//							goto waitOrNextRequest;
//						}
//					}
//					catch (Exception e) {
//						Trace.WriteLine("Error during dispatch of OnRequestStart() callback", e.ToString());
//					}
//					
//					try {
//						bool rc = ThreadPool.QueueUserWorkItem( new WaitCallback (AsyncWebRequest.RunOneResponse), state);
//						if (rc) {	// thread queued
//							state.StartTime = DateTime.Now;	
//							Interlocked.Increment(ref _runningRequests);
//						} else {	// failure, re-queue state
//							lock(_waitingRequests) {
//								_waitingRequests.Enqueue(state.Priority, state);
//							}
//						}
//					}
//					catch (Exception e) {
//						Trace.WriteLine("Error during ThreadPool.QueueUserWorkItem()", e.ToString());
//					}
//				
//					goto waitOrNextRequest;
//
//				}
//				catch (Exception ex) {
//					Trace.WriteLine("Critical Exception caught in RequestThread.Run():" + ex.ToString());
//				}
// 
//
//		}
		#endregion

		#endregion
	}
	#endregion
}
