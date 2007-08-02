#region CVS Version Header/* * $Id: RemoteFeedlistThreadHandler.cs,v 1.34 2005/05/08 17:03:07 t_rendelmann Exp $ * Last modified by $Author: t_rendelmann $ * Last modified at $Date: 2005/05/08 17:03:07 $ * $Revision: 1.34 $ */#endregionusing System;using System.Threading;using System.IO;using System.Net;using System.Xml;using System.Xml.Xsl;using System.Xml.Serialization;using System.Diagnostics;using System.Windows.Forms;using System.Resources;using EnterpriseDT.Net.Ftp;using ICSharpCode.SharpZipLib.Zip;
using Logger = RssBandit.Common.Logging;
using NewsComponents;using NewsComponents.Feed;using NewsComponents.Utils;using RssBandit.SpecialFeeds;using RssBandit.WinGui.Utility;namespace RssBandit.WinGui{	/// <summary>	/// The format used during synchronization of two instances of RSS Bandit. 	/// </summary>	public enum SynchronizationFormat{		/// <summary>		/// A ZIP file called rssbandit-state.zip containing feedlist.xml, flagitems.xml, replyitems.xml and 		/// searchfolders.xml is used for synchronization. 		/// </summary>		Zip, 		/// <summary>		/// A SIAM file called feeds.siam is used for synchronization. 		/// </summary>		Siam		}	/// <summary>	/// Summary description for RemoteFeedlistThreadHandler.	/// </summary>	internal class RemoteFeedlistThreadHandler: EntertainmentThreadHandlerBase	{		public enum Operation {			None,			Upload,			Download,		}		private RemoteFeedlistThreadHandler() {;}		public RemoteFeedlistThreadHandler(Operation operation, RssBanditApplication rssBanditApp, RemoteStorageProtocolType protocol, string remoteLocation, string credentialUser, string credentialPwd, Settings settings) {			this.operationToRun = operation;			this.rssBanditApp = rssBanditApp;						this.remoteProtocol = protocol;			this.remoteLocation = remoteLocation;			this.credentialUser = credentialUser;			this.credentialPassword = credentialPwd;			this.settings = settings;		}		private static readonly log4net.ILog _log = Logger.Log.GetLogger(typeof(RemoteFeedlistThreadHandler));
		private Operation operationToRun = Operation.None;		private RssBanditApplication rssBanditApp = null;		private RemoteStorageProtocolType remoteProtocol = RemoteStorageProtocolType.UNC;		private string remoteLocation = null;		private string credentialUser = null;		private string credentialPassword = null;		private string remoteFileName = "rssbandit-state.zip";		private Settings settings = null;		protected override void Run() {
			if (this.operationToRun == Operation.Download)
				this.RunDownload();
			if (this.operationToRun == Operation.Upload)
				this.RunUpload();
		}
		public RemoteStorageProtocolType RemoteProtocol {			get {	return this.remoteProtocol;	}			set {	this.remoteProtocol = value;	}		}		public string RemoteLocation {			get {	return this.remoteLocation;	}			set {	this.remoteLocation = value;	}		}		public string RemoteFileName {			get {	return this.remoteFileName;	}			set {	this.remoteFileName = value;	}		}		/// <summary>		/// Upload application state either as a ZIP file or as a SIAM file 		/// </summary>		public void RunUpload() {			RunUpload(SynchronizationFormat.Zip); 		}		/// <summary>		/// Zips up the files listed into the specified stream		/// </summary>		/// <param name="files">The list of files to zip</param>		/// <param name="stream">The stream to store the zipped files</param>		private static void ZipFiles(string[] files, ZipOutputStream zos){					zos.SetLevel(5); 			foreach(string file in files){				if(File.Exists(file)){					FileStream fs2 = File.OpenRead(file);            								byte[] buffer = new byte[fs2.Length];					fs2.Read(buffer, 0, buffer.Length);            								ZipEntry entry = new ZipEntry(Path.GetFileName(file));					zos.PutNextEntry(entry);					zos.Write(buffer, 0, buffer.Length);				}			}            					zos.Finish();				}		/// <summary>		/// Upload application state either as a ZIP file or as a SIAM file 		/// </summary>		/// <param name="syncFormat">The synchronization format to use</param>		public void RunUpload(SynchronizationFormat syncFormat) {			string feedlistXml = Path.Combine(Path.GetTempPath(), "feedlist.xml"); 			string[] files = {RssBanditApplication.GetFeedListFileName(), 								 RssBanditApplication.GetFlagItemsFileName(), 								 RssBanditApplication.GetSearchFolderFileName(), 								 RssBanditApplication.GetSentItemsFileName(), 								 feedlistXml}; 			ZipOutputStream zos = null;						try {								rssBanditApp.SaveApplicationState(); 									//convert subscriptions.xml to feedlist.xml then save to temp folder				using (Stream xsltStream = Resource.Manager.GetStream("Resources.feedlist2subscriptions.xslt")) {
					XslTransform xslt = new XslTransform();
					xslt.Load(new XmlTextReader(xsltStream));	
					xslt.Transform(RssBanditApplication.GetFeedListFileName(), feedlistXml);
				}				using (MemoryStream tempStream   = new MemoryStream()) {					switch (remoteProtocol) {						case RemoteStorageProtocolType.UNC:							FileStream fs = FileHelper.OpenForWrite(Path.Combine(Environment.ExpandEnvironmentVariables(remoteLocation), remoteFileName)); 							zos = new ZipOutputStream(fs); 							ZipFiles(files, zos) ;															zos.Close();							break;						case RemoteStorageProtocolType.dasBlog:							//save feed list							this.rssBanditApp.FeedHandler.SaveFeedList(tempStream, FeedListFormat.OPML); 							tempStream.Position = 0; 							XmlDocument doc = new XmlDocument();							doc.Load(tempStream);							// Send it to the web service							DasBlog.ConfigEditingService remoteStore = new DasBlog.ConfigEditingService();							remoteStore.Url = remoteLocation;							remoteStore.authenticationHeaderValue = new DasBlog.authenticationHeader();							remoteStore.authenticationHeaderValue.userName = credentialUser;							remoteStore.authenticationHeaderValue.password = credentialPassword;							//TODO: figure out, when we have to use Credentials????							//							remoteStore.Credentials = 	NewsHandler.CreateCredentialsFrom(credentialUser, credentialPassword);							remoteStore.PostBlogroll("blogroll", doc.DocumentElement);	// ".opml" is appended by the service!							doc = null;							break;						case RemoteStorageProtocolType.dasBlog_1_3:							//save feed list							this.rssBanditApp.FeedHandler.SaveFeedList(tempStream, FeedListFormat.OPML); 							tempStream.Position = 0; 												// Get the bytes into a byte array for sending							byte[] feedBytes2 = new byte[tempStream.Length];							tempStream.Read(feedBytes2, 0, (int)tempStream.Length);												// Send it to the web service							DasBlog_1_3.ConfigEditingService remoteStore2 = new DasBlog_1_3.ConfigEditingService();							remoteStore2.Url = remoteLocation;							remoteStore2.UpdateFile("blogroll.opml", feedBytes2, credentialUser, credentialPassword);							break;						case RemoteStorageProtocolType.FTP:			// Send to FTP server							//save feed list 							zos = new ZipOutputStream(tempStream);							ZipFiles(files, zos); 																					tempStream.Position = 0; 							Uri remoteUri = null;							string serverName = remoteLocation;							string remotePath = "/";							int remotePort = 21;	// default for ftp url scheme							remoteUri = new Uri(remoteLocation);							serverName = remoteUri.Host;							remotePath = remoteUri.AbsolutePath;							if (!remoteUri.IsDefaultPort) {								remotePort = remoteUri.Port;							}							// look here for documentation: http://www.enterprisedt.com/downloads/csftp/doc/com.enterprisedt.net.ftp.html							FTPClient ftpClient = new FTPClient(serverName, remotePort);							bool connectionMode_Passive = settings.GetBoolean("RemoteFeedlist/Ftp.ConnectionMode.Passive", true);							if (connectionMode_Passive) {								ftpClient.ConnectMode = FTPConnectMode.PASV;							} else {								ftpClient.ConnectMode = FTPConnectMode.ACTIVE;							}														ftpClient.Login(credentialUser, credentialPassword);							if (remotePath.Length > 1 && remotePath.StartsWith("/")) {								remotePath = remotePath.Substring(1);	// ChDir fails, if it starts with a "/"							}							if (remotePath.Length > 1) {	// if not at ftp root:								ftpClient.ChDir(remotePath);	// this is a simple command, no data...							}							try {								 								ftpClient.TransferType = FTPTransferType.BINARY; 								ftpClient.Put(tempStream, remoteFileName, false);	// try data transfer (ftp data port)							} catch (System.Net.Sockets.SocketException soex) {																if (soex.ErrorCode == 10060 || soex.ErrorCode == 10061 /* WSAECONNTIMEOUT, WSAECONNREFUSED, see http://msdn.microsoft.com/library/en-us/winsock/winsock/windows_sockets_error_codes_2.asp?frame=true */) {																		// try again, with Mode switched:									// see also: http://slacksite.com/other/ftp.html									if (connectionMode_Passive) {										ftpClient.ConnectMode = FTPConnectMode.ACTIVE;									} else {										ftpClient.ConnectMode = FTPConnectMode.PASV;									}									connectionMode_Passive = !connectionMode_Passive;									// try again:									// since the ftp component closes the stream when it gets an exception
									// we have to re-create the stream before trying again
									using (MemoryStream tempStream2 = new MemoryStream()) {
										zos = new ZipOutputStream(tempStream2);
										ZipFiles(files, zos); 
										tempStream2.Position = 0;
										ftpClient.Put(tempStream2, remoteFileName, false);
									}
									// old impl.									//ftpClient.Put(tempStream, remoteFileName, false);								} else {									throw;								}							}														// save working setting:							settings.SetProperty("RemoteFeedlist/Ftp.ConnectionMode.Passive", connectionMode_Passive);							ftpClient.Quit();							//close zip stream 							zos.Close();							break;						case RemoteStorageProtocolType.WebDAV: 														zos = new ZipOutputStream(tempStream);							ZipFiles(files, zos); 																					remoteUri = new Uri(remoteLocation.EndsWith("/") ? 								remoteLocation + remoteFileName : 								remoteLocation + "/" + remoteFileName); 							tempStream.Position = 0; 							HttpWebRequest request    = (System.Net.HttpWebRequest)HttpWebRequest.Create(remoteUri);							request.Method            = "PUT"; 														request.ContentType       = "application/zip";								request.AllowAutoRedirect = true;							request.UserAgent		= RssBanditApplication.UserAgent;							request.Proxy             = rssBanditApp.Proxy; 							if (!StringHelper.EmptyOrNull(credentialUser)) {								NetworkCredential nc = (NetworkCredential)NewsHandler.CreateCredentialsFrom(credentialUser, credentialPassword);								CredentialCache cc        = new CredentialCache(); 								cc.Add(remoteUri, "Basic", nc); 								cc.Add(remoteUri, "Digest", nc); 								cc.Add(remoteUri, "NTLM", nc); 								request.Credentials   = cc;							}							byte[] bytes          = new byte[tempStream.Length];														tempStream.Read(bytes, 0, bytes.Length); 							zos.Close(); 							request.ContentLength = bytes.Length; 							Stream requestStream = request.GetRequestStream();							requestStream.Write(bytes, 0, bytes.Length);							requestStream.Close();							request.GetResponse().Close(); 							break; 						default:							Debug.Assert(false, "unknown remote protocol: '" + remoteProtocol + "' in RemoteFeedlistThreadHandler");							break;					}				}								// Cool, we made it			} catch (ThreadAbortException) {
				// eat up
			} catch (Exception ex) {				p_operationException = ex;				_log.Debug("RunUpload("+syncFormat.ToString()+") Exception", ex);			}			finally {				WorkDone.Set();			}		}		/// <summary>		/// Download application state either as a ZIP file or as a SIAM file from a remote location.		/// </summary>		public void RunDownload() {			RunDownload(SynchronizationFormat.Zip);		}		/// <summary>		/// Download application state either as a ZIP file or as a SIAM file from a remote location.		/// </summary>		/// <param name="syncFormat">The synchronization format to use</param>		public void RunDownload(SynchronizationFormat syncFormat) {			try {				Stream importStream = null;				string tempFileName = null;				switch (remoteProtocol) {					case RemoteStorageProtocolType.UNC:						// Fetch from a UNC path						importStream = File.Open(Path.Combine(Environment.ExpandEnvironmentVariables(remoteLocation), remoteFileName), FileMode.Open);						break;					case RemoteStorageProtocolType.dasBlog:						// Fetch from the dasBlog web service						// Connect to the web service and request an update						DasBlog.ConfigEditingService remoteStore = new DasBlog.ConfigEditingService();						remoteStore.Url = remoteLocation;						remoteStore.authenticationHeaderValue = new DasBlog.authenticationHeader();						remoteStore.authenticationHeaderValue.userName = credentialUser;						remoteStore.authenticationHeaderValue.password = credentialPassword;						importStream = new MemoryStream();						XmlElement xml = remoteStore.GetBlogroll("blogroll");	// ".opml" is appended by the service!						XmlDocument doc = new XmlDocument();						doc.LoadXml(xml.OuterXml);						doc.Save(importStream);						importStream.Position = 0;						break;					case RemoteStorageProtocolType.dasBlog_1_3:						// Connect to the web service and request an update						DasBlog_1_3.ConfigEditingService remoteStore2 = new DasBlog_1_3.ConfigEditingService();						remoteStore2.Url = remoteLocation;						byte[] feedList = remoteStore2.ReadFile("blogroll.opml", credentialUser, credentialPassword);						importStream = new MemoryStream(feedList);						break;					case RemoteStorageProtocolType.FTP:						// Fetch from FTP						Uri remoteUri = null;						string serverName = remoteLocation;						string remotePath = "/";						int remotePort = 21;						remoteUri = new Uri(remoteLocation);						serverName = remoteUri.Host;						remotePath = remoteUri.AbsolutePath;						if (!remoteUri.IsDefaultPort) {							remotePort = remoteUri.Port;						}												// look here for documentation: http://www.enterprisedt.com/downloads/csftp/doc/com.enterprisedt.net.ftp.html						FTPClient ftpClient = new FTPClient(serverName, remotePort);						bool connectionMode_Passive = settings.GetBoolean("RemoteFeedlist/Ftp.ConnectionMode.Passive", true);						if (connectionMode_Passive) {							ftpClient.ConnectMode = FTPConnectMode.PASV;						} else {							ftpClient.ConnectMode = FTPConnectMode.ACTIVE;						}								ftpClient.Login(credentialUser, credentialPassword);						if (remotePath.Length > 1 && remotePath.StartsWith("/")) {							remotePath = remotePath.Substring(1);	// ChDir fails, if it starts with a "/"						}						if (remotePath.Length > 1) {	// if not at ftp root:							ftpClient.ChDir(remotePath);						}						// Create a memory stream and get the file into that						tempFileName = Path.GetTempFileName();						Stream fileStream = File.Create(tempFileName);						try {							ftpClient.TransferType = FTPTransferType.BINARY; 							ftpClient.Get(fileStream, remoteFileName);						} catch (System.Net.Sockets.SocketException soex) {															if (soex.ErrorCode == 10060 || soex.ErrorCode == 10061 /* WSAECONNTIMEOUT, WSAECONNREFUSED, see http://msdn.microsoft.com/library/en-us/winsock/winsock/windows_sockets_error_codes_2.asp?frame=true */) {																	// try again, with Mode switched:								// see also: http://slacksite.com/other/ftp.html								if (connectionMode_Passive) {									ftpClient.ConnectMode = FTPConnectMode.ACTIVE;								} else {									ftpClient.ConnectMode = FTPConnectMode.PASV;								}								connectionMode_Passive = !connectionMode_Passive;								// try again:								ftpClient.Get(fileStream, remoteFileName);							} else {								throw;							}						}						// save new setting:						settings.SetProperty("RemoteFeedlist/Ftp.ConnectionMode.Passive", connectionMode_Passive);						fileStream.Close();						ftpClient.Quit();						// Open the temporary file so we can import it						importStream = File.OpenRead(tempFileName);						break;					case RemoteStorageProtocolType.WebDAV:						remoteUri = new Uri(remoteLocation.EndsWith("/") ? 							remoteLocation + remoteFileName : 							remoteLocation + "/" + remoteFileName); 						HttpWebRequest request    = (System.Net.HttpWebRequest)HttpWebRequest.Create(remoteUri);						request.Method            = "GET"; 													request.AllowAutoRedirect = true;						request.UserAgent		= RssBanditApplication.UserAgent;						request.Proxy             = rssBanditApp.Proxy; 						if (!StringHelper.EmptyOrNull(credentialUser)) {							NetworkCredential nc = (NetworkCredential)NewsHandler.CreateCredentialsFrom(credentialUser, credentialPassword);							CredentialCache cc        = new CredentialCache(); 							cc.Add(remoteUri, "Basic", nc); 							cc.Add(remoteUri, "Digest", nc); 							cc.Add(remoteUri, "NTLM", nc); 							request.Credentials   = cc;						}						importStream = request.GetResponse().GetResponseStream();						break; 					default:						Debug.Assert(false, "unknown remote protocol: '" + remoteProtocol + "' in RemoteFeedlistThreadHandler");						break;				}				// Now import the downloaded feed list				try {					if ((remoteProtocol == RemoteStorageProtocolType.dasBlog) || 						(remoteProtocol == RemoteStorageProtocolType.dasBlog_1_3)){						this.rssBanditApp.FeedHandler.ImportFeedlist(importStream);					}else{						Synchronize(importStream, syncFormat); 	 					}					importStream.Close();				} catch (Exception ex) {					p_operationException = ex;				} finally {					if (importStream != null)						importStream.Close();				}				if (tempFileName != null)					File.Delete(tempFileName);			} catch (ThreadAbortException) {
				// eat up
			} catch (Exception ex) {				p_operationException = ex;				_log.Debug("RunDownload("+syncFormat.ToString()+") Exception", ex);			}			finally {				WorkDone.Set();			}		}		/// <summary>		/// Synchronizes the current state of RSS Bandit from the data in the stream. 		/// </summary>		/// <param name="stream">The data to synchronize RSS Bandit from</param>		/// <param name="syncFormat">The synchronization format used</param>		public void Synchronize(Stream stream, SynchronizationFormat syncFormat){			/* we support both subscriptions.xml and feedlist.xml */ 					string feedlist = Path.GetFileName(RssBanditApplication.GetFeedListFileName()); 			string oldschoolfeedlist = Path.GetFileName(RssBanditApplication.GetOldFeedListFileName()); 			string flaggeditems = Path.GetFileName(RssBanditApplication.GetFlagItemsFileName()); 			string searchfolders = Path.GetFileName(RssBanditApplication.GetSearchFolderFileName()); 			string sentitems     = Path.GetFileName(RssBanditApplication.GetSentItemsFileName());			bool subscriptionsXmlSeen = false; 						if(syncFormat == SynchronizationFormat.Zip){				/* A point to consider is what happens if an exception occurs in the 				 * middle of this process? 				 */				ZipInputStream zis = new ZipInputStream(stream);            						ZipEntry theEntry;				while ((theEntry = zis.GetNextEntry()) != null) {										if(theEntry.Name == feedlist){												subscriptionsXmlSeen = true;						this.rssBanditApp.FeedHandler.ReplaceFeedlist(zis);					}else if(!subscriptionsXmlSeen && (theEntry.Name == oldschoolfeedlist)){						this.rssBanditApp.FeedHandler.ReplaceFeedlist(zis);					}else if(theEntry.Name == flaggeditems){												LocalFeedsFeed flaggedItemsFeed = this.rssBanditApp.FlaggedItemsFeed;						LocalFeedsFeed lff  = new LocalFeedsFeed(flaggedItemsFeed.link, flaggedItemsFeed.title, 							flaggedItemsFeed.Description, new XmlTextReader(zis));						this.rssBanditApp.ClearFlaggedItems(); 												foreach(NewsItem item in lff.Items){							flaggedItemsFeed.Add(item); 							this.rssBanditApp.ReFlagNewsItem(item);						}										}else if(theEntry.Name == sentitems){						LocalFeedsFeed sentItemsFeed = this.rssBanditApp.SentItemsFeed;												LocalFeedsFeed lff2  = new LocalFeedsFeed(sentItemsFeed.link, sentItemsFeed.title, 							sentItemsFeed.Description, new XmlTextReader(zis));						sentItemsFeed.Add(lff2); 					}else if(theEntry.Name == searchfolders){						XmlSerializer ser = new XmlSerializer(typeof(FinderSearchNodes));						this.rssBanditApp.FindersSearchRoot = (FinderSearchNodes) ser.Deserialize(zis);						 										}																			}//while								zis.Close();						}//if(syncFormat == SynchronizationFormat.Zip		}	}}