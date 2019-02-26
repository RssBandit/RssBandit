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
using System.Linq;
using System.Threading;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Collections.Generic;
using log4net;
using RssBandit.AppServices.Configuration;
using RssBandit.Common.Logging;
using RssBandit.Core.Storage;
using ICSharpCode.SharpZipLib.Zip;
using Logger = RssBandit.Common.Logging;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Utils;
using RssBandit.SpecialFeeds;
using RssBandit.WinGui.Utility;


namespace RssBandit.WinGui
{
    /// <summary>
    /// The format used during synchronization of two instances of RSS Bandit. 
    /// </summary>
    public enum SynchronizationFormat
    {
        /// <summary>
        /// A ZIP file called rssbandit-state.zip containing feedlist.xml, flagitems.xml, replyitems.xml and 
        /// searchfolders.xml is used for synchronization. 
        /// </summary>
        Zip,
        /// <summary>
        /// A SIAM file called feeds.siam is used for synchronization. 
        /// </summary>
        Siam
    }

   

    /// <summary>
    /// Summary description for RemoteFeedlistThreadHandler.
    /// </summary>
    internal class RemoteFeedlistThreadHandler : EntertainmentThreadHandlerBase
    {
        public enum
            Operation
        {
            None,
            Upload,
            Download,
        }

        public RemoteFeedlistThreadHandler(Operation operation, RssBanditApplication rssBanditApp,
                                           RemoteStorageProtocolType protocol, string remoteLocation,
                                           string credentialUser, string credentialPwd, IPersistedSettings settings)
        {
            operationToRun = operation;
            this.rssBanditApp = rssBanditApp;
            remoteProtocol = protocol;
            this.remoteLocation = remoteLocation;
            this.credentialUser = credentialUser;
            credentialPassword = credentialPwd;
            this.settings = settings;
        }

        private static readonly ILog _log = Log.GetLogger(typeof (RemoteFeedlistThreadHandler));

        private readonly Operation operationToRun = Operation.None;
        private readonly RssBanditApplication rssBanditApp;
        private RemoteStorageProtocolType remoteProtocol = RemoteStorageProtocolType.UNC;
        private string remoteLocation;
        private readonly string credentialUser;
        private readonly string credentialPassword;
        private string remoteFileName = "rssbandit-state.zip";
		private readonly IPersistedSettings settings;

        protected override void Run()
        {
            if (operationToRun == Operation.Download)
                RunDownload();
            if (operationToRun == Operation.Upload)
                RunUpload();
        }

	    #region public properties

	    public RemoteStorageProtocolType RemoteProtocol
	    {
		    get
		    {
			    return remoteProtocol;
		    }
		    set
		    {
			    remoteProtocol = value;
		    }
	    }

	    public string RemoteLocation
	    {
		    get
		    {
			    return remoteLocation;
		    }
		    set
		    {
			    remoteLocation = value;
		    }
	    }


	    public string RemoteFileName
	    {
		    get
		    {
			    return remoteFileName;
		    }
		    set
		    {
			    remoteFileName = value;
		    }
	    }

	    #endregion

        /// <summary>
        /// Upload application state either as a ZIP file or as a SIAM file 
        /// </summary>
        public void RunUpload()
        {
            RunUpload(SynchronizationFormat.Zip);
        }

        /// <summary>
        /// Upload application state either as a ZIP file or as a SIAM file 
        /// </summary>
        /// <param name="syncFormat">The synchronization format to use</param>
        public void RunUpload(SynchronizationFormat syncFormat)
        {
			List<string> files = new List<string>(new[] {
                RssBanditApplication.GetFeedSourcesFileName(),
                RssBanditApplication.GetFlagItemsFileName(),
                RssBanditApplication.GetSearchFolderFileName(),
                RssBanditApplication.GetSentItemsFileName(),
				RssBanditApplication.GetWatchedItemsFileName()
            });
        	
			// add files managed by Bandit data storage (column layout and user identities):
			IUserRoamingDataService dataService = IoC.Resolve<IUserRoamingDataService>();
	        if (dataService != null)
	        {
		        files.AddRange(dataService.GetUserDataFileNames());
	        }

	        // add files managed by NewComponents feed source data storage (e.g. Nntp Server Definitions filename) :
			rssBanditApp.FeedSources.ForEach(f =>
				{
					files.AddRange(f.GetDataServiceFiles());
					files.Add(f.SubscriptionLocation.Location);
				});

	        try
            {
                rssBanditApp.SaveApplicationState();

				// Older versions support:
				FeedSourceEntry entry = rssBanditApp.FeedSources.Sources.FirstOrDefault(
					fs => fs.SourceType == FeedSourceType.DirectAccess);
	        
				if (entry != null)
				{
					//copy BanditFeedSource feed list to subscriptions.xml (backward versions compatibility support)
					File.Copy(entry.Source.SubscriptionLocation.Location,
						RssBanditApplication.OldVersionSupport.GetSubscriptionsFileName(),
						true);

					//convert subscriptions.xml to feedlist.xml then save to temp folder
					using (Stream xsltStream = Resource.GetStream("Resources.feedlist2subscriptions.xslt"))
					{
						XslCompiledTransform xslt = new XslCompiledTransform();
						xslt.Load(new XmlTextReader(xsltStream));
						xslt.Transform(
							entry.Source.SubscriptionLocation.Location,
							RssBanditApplication.OldVersionSupport.GetFeedListFileName());
					}

					files.AddRange(new[] {
						RssBanditApplication.OldVersionSupport.GetSubscriptionsFileName(),
						RssBanditApplication.OldVersionSupport.GetFeedListFileName()
						});
				}
				
                using (MemoryStream tempStream = new MemoryStream())
                {
	                //ZipOutputStream zos;
	                switch (remoteProtocol)
					{
						case RemoteStorageProtocolType.UNC:

							using (FileStream fs = FileHelper.OpenForWrite(
								     Path.Combine(Environment.ExpandEnvironmentVariables(remoteLocation), remoteFileName)))
							{
								var zos = new ZipOutputStream(fs);
								FileHelper.ZipFiles(files, zos);
							}

							break;
                     
                        //case RemoteStorageProtocolType.dasBlog:

                        //    //save direct access feed list as OPML:
                        //    rssBanditApp.BanditFeedSource.SaveFeedList(tempStream, FeedListFormat.OPML);
                        //    tempStream.Position = 0;

                        //    XmlDocument doc = new XmlDocument();
                        //    doc.Load(tempStream);

                        //    // Send it to the web service
                        //    ConfigEditingService remoteStore = new ConfigEditingService();

                        //    remoteStore.Url = remoteLocation;
                        //    remoteStore.authenticationHeaderValue = new authenticationHeader();
                        //    remoteStore.authenticationHeaderValue.userName = credentialUser;
                        //    remoteStore.authenticationHeaderValue.password = credentialPassword;
                        //    remoteStore.Proxy = rssBanditApp.Proxy;

                        //    //TODO: figure out, when we have to use Credentials????
                        //    //	remoteStore.Credentials = 	FeedSource.CreateCredentialsFrom(credentialUser, credentialPassword);

                        //    remoteStore.PostBlogroll("blogroll", doc.DocumentElement);
                        //        // ".opml" is appended by the service!
                        //    break;


						case RemoteStorageProtocolType.FTP: // Send to FTP server

							var zosFtp = new ZipOutputStream(tempStream);

							FileHelper.ZipFiles(files, zosFtp);

							tempStream.Position = 0;

							var ftpUri = new Uri(remoteLocation);
							UriBuilder builder = new UriBuilder(ftpUri);
							builder.Path += builder.Path.EndsWith("/") ? remoteFileName : "/" + remoteFileName;


							/* set up the FTP connection */
							FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(builder.Uri);
							ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
							ftpRequest.KeepAlive = false;
							ftpRequest.UseBinary = true;
							ftpRequest.UsePassive = settings.GetProperty(Ps.FtpConnectionModePassive, true);
							ftpRequest.Credentials = new NetworkCredential(credentialUser, credentialPassword);
							ftpRequest.ContentLength = tempStream.Length;

							/* perform upload */
							try
							{
								// The buffer size is set to 2kb
								const int buffLength = 2048;
								byte[] buff = new byte[buffLength];

								// Stream to which the file to be upload is written
								using (Stream strm = ftpRequest.GetRequestStream())
								{

									// Read from the file stream 2kb at a time
									int contentLen = tempStream.Read(buff, 0, buffLength);

									// Till Stream content ends
									while (contentLen != 0)
									{
										// Write Content from the file stream to the 
										// FTP Upload Stream
										strm.Write(buff, 0, contentLen);
										contentLen = tempStream.Read(buff, 0, buffLength);
									}
								}
							}
							catch (Exception ex)
							{
								//ToDO: Add support for switching between active and passive mode
								p_operationException = ex;
								_log.Error("FTP Upload Error", ex);
							}

							break;

						case RemoteStorageProtocolType.WebDAV:

							var zosDav = new ZipOutputStream(tempStream);

							FileHelper.ZipFiles(files, zosDav);

							var remoteUri = new Uri(remoteLocation.EndsWith("/")
								? remoteLocation + remoteFileName
								: remoteLocation + "/" + remoteFileName);

							tempStream.Position = 0;

							HttpWebRequest request = (HttpWebRequest)WebRequest.Create(remoteUri);
							request.Method = "PUT";
							request.ContentType = "application/zip";
							request.AllowAutoRedirect = true;
							request.UserAgent = RssBanditApplication.UserAgent;
							request.Proxy = rssBanditApp.Proxy;

							if (!string.IsNullOrEmpty(credentialUser))
							{
								NetworkCredential nc =
									FeedSource.CreateCredentialsFrom(credentialUser, credentialPassword);

								CredentialCache cc = new CredentialCache();
								cc.Add(remoteUri, "Basic", nc);
								cc.Add(remoteUri, "Digest", nc);
								cc.Add(remoteUri, "NTLM", nc);

								request.Credentials = cc;
							}

							byte[] bytes = new byte[tempStream.Length];
							tempStream.Read(bytes, 0, bytes.Length);

							request.ContentLength = bytes.Length;

							Stream requestStream = request.GetRequestStream();
							requestStream.Write(bytes, 0, bytes.Length);
							requestStream.Close();

							break;

						default:

							Debug.Assert(false,
										 "unknown remote protocol: '" + remoteProtocol +
										 "' in RemoteFeedlistThreadHandler");
							break;
					}
                }

	            // Cool, we made it
            }
            catch (ThreadAbortException)
            {
                // eat up
            }
            catch (Exception ex)
            {
                p_operationException = ex;
                _log.Error("RunUpload(" + syncFormat + ") Exception", ex);
                
            }
            finally
            {
                WorkDone.Set();
            }
        }


        /// <summary>
        /// Download application state either as a ZIP file or as a SIAM file from a remote location.
        /// </summary>
        public void RunDownload()
        {
            RunDownload(SynchronizationFormat.Zip);
        }


        /// <summary>
        /// Download application state either as a ZIP file or as a SIAM file from a remote location.
        /// </summary>
        /// <param name="syncFormat">The synchronization format to use</param>
        public void RunDownload(SynchronizationFormat syncFormat)
        {
            try
            {
                Stream importStream = null;
                string tempFileName = null;

                switch (remoteProtocol)
                {
                    case RemoteStorageProtocolType.UNC:
                        // Fetch from a UNC path
                        importStream =
                            File.Open(
                                Path.Combine(Environment.ExpandEnvironmentVariables(remoteLocation), remoteFileName),
                                FileMode.Open);
                        break;

                    //case RemoteStorageProtocolType.dasBlog:
                    //    // Fetch from the dasBlog web service
                    //    // Connect to the web service and request an update
                    //    ConfigEditingService remoteStore = new ConfigEditingService();
                    //    remoteStore.Url = remoteLocation;
                    //    remoteStore.authenticationHeaderValue = new authenticationHeader();
                    //    remoteStore.authenticationHeaderValue.userName = credentialUser;
                    //    remoteStore.authenticationHeaderValue.password = credentialPassword;
                    //    remoteStore.Proxy = rssBanditApp.Proxy;

                    //    importStream = new MemoryStream();
                    //    XmlElement xml = remoteStore.GetBlogroll("blogroll"); // ".opml" is appended by the service!

                    //    XmlDocument doc = new XmlDocument();
                    //    doc.LoadXml(xml.OuterXml);
                    //    doc.Save(importStream);
                    //    importStream.Position = 0;
                    //    break;
                   

                    case RemoteStorageProtocolType.FTP:
                        // Fetch from FTP                    
                        Uri remoteUri = new Uri(remoteLocation);           
                        UriBuilder builder = new UriBuilder(remoteUri);
                        builder.Path += builder.Path.EndsWith("/") ? remoteFileName : "/" + remoteFileName;

                        /* set up the FTP connection */
                        FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(builder.Uri);
                        ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                        ftpRequest.KeepAlive = false;
                        ftpRequest.UseBinary = true;
						ftpRequest.UsePassive = settings.GetProperty(Ps.FtpConnectionModePassive, true);
                        ftpRequest.Credentials = new NetworkCredential(credentialUser, credentialPassword);


                        // Create a file stream and get the file into that
                        tempFileName = Path.GetTempFileName();
                        Stream fileStream = File.Create(tempFileName);

                        /* perform download */
                        try
                        {
                            // The buffer size is set to 2kb
                            const int buffLength = 2048;
                            byte[] buff = new byte[buffLength];

	                        var response = (FtpWebResponse)ftpRequest.GetResponse();
	                        
		                    // Stream to which the file to be upload is written
		                    Stream strm = response.GetResponseStream();
		                    if (strm != null)
		                    {
			                    // Read from the FTP stream 2kb at a time
			                    int contentLen = strm.Read(buff, 0, buffLength);

			                    // Till Stream content ends
			                    while (contentLen != 0)
			                    {
				                    // Write Content from the file stream to the 
				                    // FTP Upload Stream
				                    fileStream.Write(buff, 0, contentLen);
				                    contentLen = strm.Read(buff, 0, buffLength);
			                    }

			                    // Close the Response Stream
			                    strm.Close();
		                    }
	                        
                        }
                        catch (Exception ex)
                        {
                            //ToDO: Add support for switching between active and passive mode
                            this.p_operationException = ex; 
                            _log.Error("FTP Download Error", ex);
                        }

                        fileStream.Close();

                        // save new setting:
                        //settings.SetProperty("RemoteFeedlist/Ftp.ConnectionMode.Passive", connectionMode_Passive);

						// Open the temporary file so we can import it
						if (p_operationException == null)
							importStream = File.OpenRead(tempFileName);
                        break;

                    case RemoteStorageProtocolType.WebDAV:

                        remoteUri = new Uri(remoteLocation.EndsWith("/")
                                                ? remoteLocation + remoteFileName
                                                : remoteLocation + "/" + remoteFileName);

                        HttpWebRequest request = (HttpWebRequest) WebRequest.Create(remoteUri);
                        request.Method = "GET";
                        request.AllowAutoRedirect = true;
                        request.UserAgent = RssBanditApplication.UserAgent;
                        request.Proxy = rssBanditApp.Proxy;

                        if (!string.IsNullOrEmpty(credentialUser))
                        {
                            NetworkCredential nc =
                                FeedSource.CreateCredentialsFrom(credentialUser, credentialPassword);

                            CredentialCache cc = new CredentialCache();
                            cc.Add(remoteUri, "Basic", nc);
                            cc.Add(remoteUri, "Digest", nc);
                            cc.Add(remoteUri, "NTLM", nc);

                            request.Credentials = cc;
                        }

                        importStream = request.GetResponse().GetResponseStream();

                        break;

                    default:
                        Debug.Assert(false,
                                     "unknown remote protocol: '" + remoteProtocol + "' in RemoteFeedlistThreadHandler");
                        break;
                }

                // Now import the downloaded feed list
	            if (importStream != null)
	            {
		            try
		            {
			            if ((remoteProtocol == RemoteStorageProtocolType.dasBlog) ||
			                (remoteProtocol == RemoteStorageProtocolType.dasBlog_1_3))
			            {
				            rssBanditApp.BanditFeedSource.ImportFeedlist(importStream);
			            }
			            else
			            {
				            Synchronize(importStream, syncFormat);
			            }
		            }
		            catch (Exception ex)
		            {
			            p_operationException = ex;
		            }
		            finally
		            {
			            importStream.Close();
		            }
	            }

	            if (tempFileName != null)
                    File.Delete(tempFileName);
            }
            catch (ThreadAbortException)
            {
                // eat up
            }
            catch (Exception ex)
            {
                p_operationException = ex;
                _log.Error("RunDownload(" + syncFormat + ") Exception", ex);
            }
            finally
            {
                WorkDone.Set();
            }
        }


        /// <summary>
        /// Synchronizes the current state of RSS Bandit from the data in the stream. 
        /// </summary>
        /// <param name="stream">The data to synchronize RSS Bandit from</param>
        /// <param name="syncFormat">The synchronization format used</param>
        public void Synchronize(Stream stream, SynchronizationFormat syncFormat)
        {
            /* we support both old version's subscriptions.xml and feedlist.xml */

            string feedsources = Path.GetFileName(RssBanditApplication.GetFeedSourcesFileName());
            string subscriptionsOld = Path.GetFileName(RssBanditApplication.OldVersionSupport.GetSubscriptionsFileName());
			string feedlistOld = Path.GetFileName(RssBanditApplication.OldVersionSupport.GetFeedListFileName());
            string flaggeditems = Path.GetFileName(RssBanditApplication.GetFlagItemsFileName());
            string searchfolders = Path.GetFileName(RssBanditApplication.GetSearchFolderFileName());
            string sentitems = Path.GetFileName(RssBanditApplication.GetSentItemsFileName());
            string watcheditems = Path.GetFileName(RssBanditApplication.GetWatchedItemsFileName());
            bool subscriptionsXmlSeen = false;


            if (syncFormat == SynchronizationFormat.Zip)
            {
                ZipInputStream zis = new ZipInputStream(stream);

                ZipEntry theEntry;
                while ((theEntry = zis.GetNextEntry()) != null)
                {
	                if (theEntry.Name == feedsources)
	                {
		                rssBanditApp.FeedSources.LoadFeedSources(zis);
	                }
                    else if (!subscriptionsXmlSeen && (theEntry.Name == subscriptionsOld))
                    {
                        subscriptionsXmlSeen = true;
                        rssBanditApp.BanditFeedSource.ReplaceFeedlist(zis);
                    }
                    else if (!subscriptionsXmlSeen && (theEntry.Name == feedlistOld))
                    {
                        rssBanditApp.BanditFeedSource.ReplaceFeedlist(zis);
                    }
                    else if (theEntry.Name == flaggeditems)
                    {
                        LocalFeedsFeed flaggedItemsFeed = rssBanditApp.FlaggedItemsFeed;
						LocalFeedsFeed lff = new FlaggedItemsFeed(rssBanditApp.BanditFeedSourceEntry, new XmlTextReader(zis));
                        rssBanditApp.ClearFlaggedItems();

                        foreach (NewsItem item in lff.Items)
                        {
                            flaggedItemsFeed.Add(item);
                            rssBanditApp.ReFlagNewsItem(item);
                        }
                    }
                    else if (theEntry.Name == sentitems)
                    {
                        LocalFeedsFeed sentItemsFeed = rssBanditApp.SentItemsFeed;
						LocalFeedsFeed lff2 = new SentItemsFeed(rssBanditApp.BanditFeedSourceEntry, new XmlTextReader(zis));

                        sentItemsFeed.Add(lff2);
                    }
					else if (theEntry.Name == watcheditems)
					{
						LocalFeedsFeed watchedItemsFeed = rssBanditApp.WatchedItemsFeed;
						LocalFeedsFeed lff2 = new WatchedItemsFeed(rssBanditApp.BanditFeedSourceEntry, new XmlTextReader(zis));

						watchedItemsFeed.Add(lff2);
					}
                    else if (theEntry.Name == searchfolders)
                    {
                        XmlSerializer ser = XmlHelper.SerializerCache.GetSerializer(typeof (FinderSearchNodes));
                        rssBanditApp.FindersSearchRoot = (FinderSearchNodes) ser.Deserialize(zis);
                    } 
					else
                    {
						// set files managed by Bandit data storage:
						IUserRoamingDataService dataService = IoC.Resolve<IUserRoamingDataService>();
						if (dataService != null)
						{
							if (DataEntityName.None != dataService.SetContentForDataFile(theEntry.Name, zis))
								continue; // was handled here
						}

						// remaining: set files managed by NewComponents feed source data storage:
	                    bool handled = false;
						rssBanditApp.FeedSources.ForEach(
							f =>
							{
								if (!handled && f.SetContentForDataServiceFile(theEntry.Name, zis))
								{
									handled = true;
								}
							});
						
						if (handled)
							continue;

						rssBanditApp.FeedSources.ForEach(
							f =>
							{
								if (!handled && f.SubscriptionLocation.Location.EndsWith(theEntry.Name, StringComparison.OrdinalIgnoreCase))
								{
									if (f.Type == FeedSourceType.DirectAccess)
										subscriptionsXmlSeen = true;	// do not import/replace from older subscription version files

									f.ReplaceFeedlist(zis);
									handled = true;
								}
							});
                    }
                } //while

                zis.Close();
            } //if(syncFormat == SynchronizationFormat.Zip
        }

    }
}
