#region CVS Version Header
/*
 * $Id: IdentityNewsServerManager.cs,v 1.18 2006/10/31 13:36:35 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2006/10/31 13:36:35 $
 * $Revision: 1.18 $
 */
#endregion

#region CVS Version Log
/*
 * $Log: IdentityNewsServerManager.cs,v $
 * Revision 1.18  2006/10/31 13:36:35  t_rendelmann
 * fixed: various changes applied to make compile with CLR 2.0 possible without the hassle to convert it all the time again
 *
 * Revision 1.17  2006/09/29 18:14:36  t_rendelmann
 * a) integrated lucene index refreshs;
 * b) now using a centralized defined category separator;
 * c) unified decision about storage relevant changes to feed, feed and feeditem properties;
 * d) fixed: issue [ 1546921 ] Extra Category Folders Created
 * e) fixed: issue [ 1550083 ] Problem when renaming categories
 *
 */
#endregion

using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;

using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.News;
using NewsComponents.Utils;
using RssBandit.Resources;
using RssBandit.WinGui;
using RssBandit.WinGui.Forms;

namespace RssBandit
{

	#region IdentityNewsServerManager
	/// <summary>
	/// Summary description for IdentityNewsServerManager.
	/// </summary>
	public class IdentityNewsServerManager: IServiceProvider
	{
		public event EventHandler NewsServerDefinitionsModified;
		public event EventHandler IdentityDefinitionsModified;

		// logging/tracing:
		private static readonly log4net.ILog _log = Common.Logging.Log.GetLogger(typeof(IdentityNewsServerManager));

		private static UserIdentity anonymous;

		private RssBanditApplication app;
		private string cachePath;

		private IdentityNewsServerManager() {}
		internal IdentityNewsServerManager(RssBanditApplication app) {
			this.app = app;
			this.cachePath = RssBanditApplication.GetFeedFileCachePath();
		}

		#region public methods

		public static UserIdentity AnonymousIdentity {
			get {
				if (anonymous != null)
					return anonymous;

				anonymous = new UserIdentity();
				anonymous.Name = anonymous.RealName = "anonymous";
				anonymous.MailAddress = anonymous.ResponseAddress = String.Empty;
				anonymous.Organization = anonymous.ReferrerUrl = String.Empty;
				anonymous.Signature = String.Empty;

				return anonymous;
			}
		}

		public IDictionary CurrentIdentities {
			get { return this.app.FeedHandler.UserIdentity; }
		}
		public IDictionary CurrentNntpServers {
			get { return this.app.FeedHandler.NntpServers; }
		}

		/// <summary>
		/// Gets (filters) the current subscriptions for Nntp feeds of a specific nntp server.
		/// </summary>
		/// <param name="sd">NntpServerDefinition</param>
		/// <returns>List of feedsFeed objects, that match</returns>
		public IList CurrentSubscriptions(NntpServerDefinition sd) {
			//TODO: impl. CurrentSubscriptions()
			return new ArrayList();
		}

		/// <summary>
		/// Load the list of groups from either the cache, or if not available or forced 
		/// from the nntp server.
		/// </summary>
		/// <param name="owner">Windows handle</param>
		/// <param name="sd">NntpServerDefinition</param>
		/// <param name="forceLoadFromServer">set to true, if a 'fresh' group list should be loaded from the server</param>
		/// <returns>List of groups a server offer</returns>
		public IList LoadNntpNewsGroups(IWin32Window owner, NntpServerDefinition sd, bool forceLoadFromServer) {
			
			IList list = null;
			if (forceLoadFromServer) {
				list = FetchNewsGroupsFromServer(owner, sd);
				if (list != null && list.Count > 0)
					SaveNewsGroupsToCache(sd, list);
				else
					RemoveCachedGroups(sd);
			} else {
				list = LoadNewsGroupsFromCache(sd);
			}

			return list;
		}

		public static Uri BuildNntpRequestUri(NntpServerDefinition sd) {
			return BuildNntpRequestUri(sd, null);
		}
		public static Uri BuildNntpRequestUri(NntpServerDefinition sd, string nntpGroup) {
			
			// We do not use NntpWebRequest.NewsUriScheme here ("news"),
			// because the UriBuilder build the Uri without slashes...
			string schema = NntpWebRequest.NntpUriScheme;	
			if (sd.UseSSLSpecified && sd.UseSSL)
				schema = NntpWebRequest.NntpsUriScheme;
				
			int port = NntpWebRequest.NntpDefaultServerPort;
			if (sd.PortSpecified && sd.Port > 0 && sd.Port != NntpWebRequest.NntpDefaultServerPort)
				port = sd.Port;				
				
			UriBuilder uriBuilder = null;
			if (StringHelper.EmptyOrNull(nntpGroup))
				uriBuilder = new UriBuilder(schema, sd.Server, port);
			else
				uriBuilder = new UriBuilder(schema, sd.Server, port, nntpGroup);
			return uriBuilder.Uri;
		}
		#endregion

		#region private methods
		private string BuildCacheFileName(NntpServerDefinition sd) {
			return Path.Combine(cachePath, String.Format("{0}_{1}.xml", sd.Server , sd.PortSpecified ? sd.Port : NntpWebRequest.NntpDefaultServerPort) );
		}

		private void RemoveCachedGroups(NntpServerDefinition sd) {
			RemoveCachedGroups(BuildCacheFileName(sd));
		}
		private void RemoveCachedGroups(string cachedFileName) {
			if (File.Exists(cachedFileName)) 
				FileHelper.Delete(cachedFileName);
		}

		private void SaveNewsGroupsToCache(NntpServerDefinition sd, IList list) {

			string fn = BuildCacheFileName(sd);
			RemoveCachedGroups(fn);

			if (list != null && list.Count > 0) {
				try {
					using (StreamWriter w = new StreamWriter(FileHelper.OpenForWrite(fn))) {
						foreach (string line in list)
							w.WriteLine(line);
					}
				} catch (Exception ex) {
					_log.Error("SaveNewsGroupsToCache() failed to save '" + fn + "'", ex);
				}
			}
		}
		
		private IList LoadNewsGroupsFromCache(NntpServerDefinition sd) {
			ArrayList result = new ArrayList();
			string fn = BuildCacheFileName(sd);
			
			if (File.Exists(fn)) {
				try {
					using (StreamReader r = new StreamReader(FileHelper.OpenForRead(fn))) {
						string line = r.ReadLine();
						while (line != null) {
							result.Add(line);
							line = r.ReadLine();
						}
					}
				} catch (Exception ex) {
					_log.Error("LoadNewsGroupsFromCache() failed to load '" + fn + "'", ex);
				}
			}

			return result;
		}

		/// <summary>
		/// Really loads the group list from the server.
		/// </summary>
		/// <param name="owner">Windows handle</param>
		/// <param name="sd">NntpServerDefinition</param>
		/// <returns>List of groups a server offer</returns>
		/// <exception cref="ArgumentNullException">If <see paramref="sd">param sd</see> is null</exception>
		/// <exception cref="Exception">On any failure we get on request</exception>
		IList FetchNewsGroupsFromServer(IWin32Window owner, NntpServerDefinition sd) {
			if (sd == null)
				throw new ArgumentNullException("sd");

			FetchNewsgroupsThreadHandler threadHandler = new FetchNewsgroupsThreadHandler(sd);
			DialogResult result = threadHandler.Start(owner, SR.NntpLoadingGroupsWaitMessage, true);

			if (DialogResult.OK != result)
				return new ArrayList(0);	// cancelled
                    
			if (!threadHandler.OperationSucceeds) {
				MessageBox.Show(
					SR.ExceptionNntpLoadingGroupsFailed(sd.Server, threadHandler.OperationException.Message), 
					SR.GUINntpLoadingGroupsFailedCaption, MessageBoxButtons.OK,MessageBoxIcon.Error);

				return new ArrayList(0);	// failed
			}

			return threadHandler.Newsgroups;
		}

		#endregion

		#region ShowDialog()'s 
		public void ShowIdentityDialog(IWin32Window owner) {
			this.ShowDialog(owner, NewsgroupSettingsView.Identity);
		}
		public void ShowNewsServerSubscriptionsDialog(IWin32Window owner) {
			this.ShowDialog(owner, NewsgroupSettingsView.NewsServerSubscriptions);
		}
		
		public void ShowDialog(IWin32Window owner, NewsgroupSettingsView view) {
			NewsgroupsConfiguration cfg = new NewsgroupsConfiguration(this, view);
			cfg.DefinitionsModified += new EventHandler(OnCfgDefinitionsModified);
			try {
				if (DialogResult.OK == cfg.ShowDialog(owner)) {
					//TODO: we should differ between the two kinds of general modifications
					RaiseNewsServerDefinitionsModified();
					RaiseIdentityDefinitionsModified();
//					if (!app.FeedlistModified)
//						app.FeedlistModified = true;
					app.SubscriptionModified(NewsFeedProperty.General);
				}
			} catch (Exception ex) {
				Trace.WriteLine("Exception in NewsGroupsConfiguration dialog: "+ex.Message);
			}
		}
		#endregion

		#region private members
		void RaiseNewsServerDefinitionsModified() {
			if (NewsServerDefinitionsModified != null)
				NewsServerDefinitionsModified(this, EventArgs.Empty);
		}
		void RaiseIdentityDefinitionsModified() {
			if (IdentityDefinitionsModified != null)
				IdentityDefinitionsModified(this, EventArgs.Empty);
		}
		#endregion

		#region event handling
		private void OnCfgDefinitionsModified(object sender, EventArgs e) {
			
			NewsgroupsConfiguration cfg = sender as NewsgroupsConfiguration;

			// take over the copies from local userIdentities and nntpServers 
			// to app.FeedHandler.Identity and app.FeedHandler.NntpServers
			lock (app.FeedHandler.UserIdentity.SyncRoot) {
				app.FeedHandler.UserIdentity.Clear();
				if (cfg.ConfiguredIdentities != null) {
					foreach (UserIdentity ui in cfg.ConfiguredIdentities.Values) {
						app.FeedHandler.UserIdentity.Add(ui.Name, (UserIdentity)ui.Clone());
					}
				}
			}

			lock (app.FeedHandler.NntpServers.SyncRoot) {
				app.FeedHandler.NntpServers.Clear();
				if (cfg.ConfiguredNntpServers != null) {
					foreach (NntpServerDefinition sd in cfg.ConfiguredNntpServers.Values) {
						app.FeedHandler.NntpServers.Add(sd.Name, (NntpServerDefinition)sd.Clone());
					}	
				}
			}

			//if (!app.FeedlistModified)
			//	app.FeedlistModified = true;
			app.SubscriptionModified(NewsFeedProperty.General);

			//TODO: we should differ between the two kinds of general modifications
			RaiseNewsServerDefinitionsModified();
			RaiseIdentityDefinitionsModified();

		}
		#endregion

		#region IServiceProvider Members

		public object GetService(Type serviceType)
		{
			IServiceProvider p = this.app as IServiceProvider;
			if (p != null)
				return p.GetService(serviceType);
			
			return null;
		}

		#endregion
	}
	#endregion

	#region FetchNewsgroupsThreadHandler
	internal class FetchNewsgroupsThreadHandler: EntertainmentThreadHandlerBase {
	
		private NntpServerDefinition serverDef;
		public ArrayList Newsgroups;

		public FetchNewsgroupsThreadHandler(NntpServerDefinition sd) {
			this.serverDef = sd;
			this.Newsgroups = new ArrayList(0);
		}

		protected override void Run() {

			ArrayList result = new ArrayList(500);

			try {
				NntpWebRequest request = (NntpWebRequest) WebRequest.Create(IdentityNewsServerManager.BuildNntpRequestUri(serverDef)); 
				request.Method = "LIST"; 
					
				if (!StringHelper.EmptyOrNull(serverDef.AuthUser)) {
					string u = null, p = null;
					NewsHandler.GetNntpServerCredentials(serverDef, ref u, ref p);
					request.Credentials = NewsHandler.CreateCredentialsFrom(u, p);
				}

				//TODO: implement proxy support in NntpWebRequest
				//request.Proxy = app.Proxy;

				request.Timeout = 1000 * 60;	// default timeout: 1 minute
				if (serverDef.TimeoutSpecified)
					request.Timeout = serverDef.Timeout * 1000 * 60;	// sd.Timeout specified in minutes, but we need msecs

				WebResponse response = request.GetResponse();

				foreach(string s in NntpParser.GetNewsgroupList(response.GetResponseStream())){
					result.Add(s);
				}

				this.Newsgroups = result;

			} catch (System.Threading.ThreadAbortException) {
				// eat up
			} catch (Exception ex) {
			
				p_operationException = ex;

			} finally {
				WorkDone.Set();
			}

		}// Run

	}
	#endregion

}
