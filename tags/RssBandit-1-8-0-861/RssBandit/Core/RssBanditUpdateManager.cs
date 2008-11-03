#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

//#define TEST

using System;
using System.Net;
using System.Windows.Forms;
using System.Threading;
using RssBandit.Resources;
using AppExceptions = Microsoft.ApplicationBlocks.ExceptionManagement;
using Logger = RssBandit.Common.Logging;

using RssBandit.WinGui.Forms;

using ClrMappedWebReference = RssBandit.CLR20.RssBandit.UpdateService;
using System.ComponentModel;


namespace RssBandit
{
	/// <summary>
	/// ApplicationUpdateManager.
	/// </summary>
	public class RssBanditUpdateManager
	{
		public delegate void UpdateAvailableEventHandler(object sender, UpdateAvailableEventArgs e);
		public static event UpdateAvailableEventHandler OnUpdateAvailable;
		public static object Tag;

		private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(RssBanditUpdateManager));
		private static readonly TimeSpan timeout = new TimeSpan(0,0,30);

		private readonly ClrMappedWebReference.UpdateService appUpdateService;
		private readonly AutoResetEvent workDone;
		private bool cancelled;
        private readonly SynchronizationContext _currentContext = AsyncOperationManager.SynchronizationContext;
		
		public RssBanditUpdateManager(){
			this.cancelled = false;
			this.workDone = new AutoResetEvent(false);
			this.appUpdateService = new ClrMappedWebReference.UpdateService();
			this.appUpdateService.Url = Resource.OutgoingLinks.UpdateServiceUrl;
		}

		public static void BeginCheckForUpdates(IWin32Window owner, IWebProxy proxy) {

			RssBanditUpdateManager updater = new RssBanditUpdateManager();
			updater.appUpdateService.Proxy = proxy;
			updater.appUpdateService.AllowAutoRedirect = true;

			if (owner != null) {	// interactive/visual
				
				EntertainmentDialog entertainment = new EntertainmentDialog(updater.WorkDone, timeout, null);
				entertainment.Message = SR.WaitDialogMessageCheckForNewAppVersion;
				Thread theThread = new Thread(updater.Run);
				theThread.Start();

				if (entertainment.ShowDialog( owner ) == DialogResult.Cancel) {	// timeout
					lock (updater) {
						updater.cancelled = true;
					}
					updater.WorkDone.Set();
					_log.Info("UpdateService.DownloadLink() operation timeout");
					updater.RaiseOnUpdateAvailable(false, null);
				}
			
			} else {	// hidden

				Thread theThread = new Thread(updater.Run);
				theThread.Start();
			
				if (updater.WorkDone.WaitOne(timeout, false)){
					// gets signal, done
				} else {
					lock (updater) {
						updater.cancelled = true;
					}
					_log.Info("UpdateService.DownloadLink() operation timeout");
					updater.RaiseOnUpdateAvailable(false, null);
				}
			}
		}

		public AutoResetEvent WorkDone {
			get {	return workDone;	}
		}

		private void Run() 
		{
			try {
#if TEST
				string url = appUpdateService.DownloadLink(RssBanditApplication.AppGuid, new Version(1,6,0,1).ToString(), RssBanditApplication.ApplicationInfos);	
#else
				string url = appUpdateService.DownloadLink(RssBanditApplication.AppGuid, RssBanditApplication.Version.ToString(), RssBanditApplication.ApplicationInfos);	
#endif
				workDone.Set();	// dismiss wait dialog
				lock (this) {
					if (!this.cancelled)
						RaiseOnUpdateAvailable(url != null, url);
				}
			} catch (ThreadAbortException) {	// timeout by dialog
				workDone.Set();
				return;
			} catch (Exception ex) {
				workDone.Set();
				_log.Error("UpdateService.DownloadLink() call failed", ex);
				lock (this) {
					if (!this.cancelled)
						RaiseOnUpdateAvailable(false, null);
				}
			}
		}

		private void RaiseOnUpdateAvailable(bool isNewVersion, string url)
		{
            UpdateAvailableEventHandler evt = OnUpdateAvailable;
            _currentContext.Send(delegate
                                     {
                                         if (evt != null)
                                         {
                                             evt(null, new UpdateAvailableEventArgs(isNewVersion, url));
                                         }
                                     }, null);
		}
	}

	public class UpdateAvailableEventArgs:EventArgs{
		public bool NewVersionAvailable;
		public string DownloadUrl;
		public UpdateAvailableEventArgs(bool isNewVersion, string url){
			this.NewVersionAvailable = isNewVersion;
			this.DownloadUrl = url;
		}
	}
}

