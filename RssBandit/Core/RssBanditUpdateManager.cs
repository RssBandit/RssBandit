#region CVS Version Header
/*
 * $Id: RssBanditUpdateManager.cs,v 1.2 2005/03/19 18:21:54 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/03/19 18:21:54 $
 * $Revision: 1.2 $
 */
#endregion

using System;
using System.Windows.Forms;
using System.Threading;
using AppExceptions = Microsoft.ApplicationBlocks.ExceptionManagement;
using Logger = RssBandit.Common.Logging;

using RssBandit.WinGui.Forms;

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
		private static TimeSpan timeout = new TimeSpan(0,0,30);

		private RssBandit.UpdateService.UpdateService appUpdateService;
		private AutoResetEvent workDone;
		private bool cancelled;
		
		public RssBanditUpdateManager(){
			this.cancelled = false;
			this.workDone = new AutoResetEvent(false);
			this.appUpdateService = new RssBandit.UpdateService.UpdateService();
			this.appUpdateService.Url = RssBanditApplication.UpdateServiceUrl;
		}

		public static void BeginCheckForUpdates(IWin32Window owner) {

			RssBanditUpdateManager updater = new RssBanditUpdateManager();
			
			if (owner != null) {	// interactive/visual
				
				EntertainmentDialog entertainment = new EntertainmentDialog(updater.WorkDone, timeout);
				entertainment.Message = Resource.Manager["RES_WaitDialogMessageCheckForNewAppVersion"];
				Thread theThread = new Thread(new ThreadStart(updater.Run));
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

				Thread theThread = new Thread(new ThreadStart(updater.Run));
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

		public static void BeginCheckForUpdates() {
			BeginCheckForUpdates(null);
		}

		public AutoResetEvent WorkDone {
			get {	return workDone;	}
		}

		private void Run() {
			string url = null;
			try {
				url = appUpdateService.DownloadLink(RssBanditApplication.AppGuid, RssBanditApplication.VersionOnly, RssBanditApplication.ApplicationInfos);	
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
			if (OnUpdateAvailable != null)
				OnUpdateAvailable(null, new UpdateAvailableEventArgs(isNewVersion, url));
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
