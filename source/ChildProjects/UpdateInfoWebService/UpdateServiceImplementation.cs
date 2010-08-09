using System;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.Services;

namespace RssBandit.Services {
	/// <summary>
	/// Summary description for UpdateServiceImplementation.
	/// </summary>
	[
	WebService(Namespace="urn:schemas-rssbandit-org:rssbandit:update-services",
		Description="RSS Bandit update services.")
	]
	public class UpdateServiceImplementation : WebService {

		// Create a logger for use in this class
		private static log4net.ILog log = Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		public UpdateServiceImplementation() {
			InitializeComponent();
		}

		#region Component Designer generated code
		
		//Required by the Web Services Designer 
		private IContainer components = null;
				
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if(disposing && components != null) {
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

		[WebMethod]
		public string DownloadLink(string appID, string currentAppVersion, string appKey) {
			
			if (log.IsInfoEnabled)
				log.Info(String.Format(new StringFormatter(), "{0};{1}; DownloadLink({2:T=40},{3:T=40},{4:T=200})", 
					Context.Request.UserHostAddress, Context.Request.UserAgent, 
					appID, currentAppVersion, appKey));

			if (appID != null && appID.Length > 0 && currentAppVersion != null && currentAppVersion.Length > 0) {
				Version reqVersion = null;
				try{
					reqVersion = new Version(currentAppVersion);
				} catch (Exception ex){
					log.Error(String.Format(new StringFormatter(),"Version parse error on parameter <currentAppVersion>  '{0:T=40}': {1}", currentAppVersion, ex.Message));
					return null;
				}
				ListDictionary infos = null; 
				
				Application.Lock();
				infos = (ListDictionary)Application.Get(appID);
				Application.UnLock();
				
				if (infos == null){
					infos = GetUpdateInfos(appID);
					Application.Lock();
					Application.Set(appID, infos);
					Application.UnLock();
				}

				foreach (Version v in infos.Keys){
					if (reqVersion.CompareTo(v) < 0){	// This instance is before version v
						UpdateInfo u = (UpdateInfo)infos[v];
						if (u != null && u.DownloadUri != null)
							return u.DownloadUri.ToString();
					}
				}

			}
			
			return null;
		}

		private static ListDictionary GetUpdateInfos(string appId) {

			try {	
				Hashtable infos = ConfigurationManager.GetSection("rssBandit.UpdateService.UpdateInfos") as Hashtable;
				if (infos != null && infos.ContainsKey(appId))
					return (ListDictionary)infos[appId];
			    return new ListDictionary();
			} catch (Exception ex) {
				log.Error(String.Format(new StringFormatter(), "GetUpdateInfos('{0:T=40}') failed: {1}", appId , ex.Message));
				return new ListDictionary();
			}
		}
	}
}
