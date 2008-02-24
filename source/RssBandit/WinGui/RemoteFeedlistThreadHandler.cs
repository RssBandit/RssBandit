#region CVS Version Header

/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */

#endregion

using System;
using System.Threading;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
#if CLR_20
using log4net;
using RssBandit.CLR20.com.newsgator.services;
using RssBandit.CLR20.com.newsgator.services1;
using RssBandit.CLR20.com.newsgator.services2;
using RssBandit.CLR20.com.newsgator.services3;
using RssBandit.CLR20.com.newsgator.services4;
using RssBandit.CLR20.DasBlog;
using RssBandit.Common.Logging;
using ClrMappedWebReference = RssBandit.CLR20.DasBlog;
using NgosLocation = RssBandit.CLR20.com.newsgator.services;
using NgosFolder = RssBandit.CLR20.com.newsgator.services1;
using NgosFeed = RssBandit.CLR20.com.newsgator.services2;
using NgosSubscription = RssBandit.CLR20.com.newsgator.services3;
using NgosPostItem = RssBandit.CLR20.com.newsgator.services4;
#else // CLR_11 
using ClrMappedWebReference = RssBandit.DasBlog;
using NgosLocation = RssBandit.com.newsgator.services;
using NgosFolder = RssBandit.com.newsgator.services1;
using NgosFeed = RssBandit.com.newsgator.services2;
using NgosSubscription = RssBandit.com.newsgator.services3;
using NgosPostItem = RssBandit.com.newsgator.services4;
#endif
using ICSharpCode.SharpZipLib.Zip;
using Logger = RssBandit.Common.Logging;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Utils;
using NewsComponents.Threading;
using RssBandit.SpecialFeeds;
using RssBandit.WinGui.Utility;
using NGAPIToken=RssBandit.CLR20.com.newsgator.services3.NGAPIToken;


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
    /// Helper class used for passing data between callbacks
    /// </summary>
    internal class NgosDownloadFeedState
    {
        public INewsFeed feed;
        public StringCollection readItems2Sync;
        public StringCollection deletedItems2Sync;
        public FeedWebService fws;
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
                                           string credentialUser, string credentialPwd, Settings settings)
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
        private static readonly string NgosProductKey = "7AF62582A5334A9CADF967818E734558";

        private static readonly string NgosLocationName = "RssBandit-" + Environment.MachineName;
                                       //"NewsGator Web Edition"; 

        private static readonly string InvalidFeedUrl =
            "http://www.example.com/no-url-for-rss-feed-provided-in-imported-opml";

        private readonly Operation operationToRun = Operation.None;
        private readonly RssBanditApplication rssBanditApp = null;
        private RemoteStorageProtocolType remoteProtocol = RemoteStorageProtocolType.UNC;
        private string remoteLocation = null;
        private readonly string credentialUser = null;
        private readonly string credentialPassword = null;
        private string remoteFileName = "rssbandit-state.zip";
        private readonly Settings settings = null;
        private ManualResetEvent eventX;
        private int ngosFeedsToDownload = 0;
        private int ngosDownloadedFeeds = 0;
        private static readonly string NgosOpmlNamespace = "http://newsgator.com/schema/opml";

        protected override void Run()
        {
            if (operationToRun == Operation.Download)
                RunDownload();
            if (operationToRun == Operation.Upload)
                RunUpload();
        }

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


        /// <summary>
        /// Upload application state either as a ZIP file or as a SIAM file 
        /// </summary>
        public void RunUpload()
        {
            RunUpload(SynchronizationFormat.Zip);
        }

        /// <summary>
        /// Zips up the files listed into the specified stream
        /// </summary>
        /// <param name="files">The list of files to zip</param>
        /// <param name="zos">The stream to store the zipped files</param>
        private static void ZipFiles(IEnumerable<string> files, ZipOutputStream zos)
        {
            zos.SetLevel(5);

            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    FileStream fs2 = File.OpenRead(file);

                    byte[] buffer = new byte[fs2.Length];
                    fs2.Read(buffer, 0, buffer.Length);

                    ZipEntry entry = new ZipEntry(Path.GetFileName(file));
                    zos.PutNextEntry(entry);
                    zos.Write(buffer, 0, buffer.Length);
                    fs2.Close();
                }
            }

            zos.Finish();
        }

        /// <summary>
        /// Upload application state either as a ZIP file or as a SIAM file 
        /// </summary>
        /// <param name="syncFormat">The synchronization format to use</param>
        public void RunUpload(SynchronizationFormat syncFormat)
        {
            string feedlistXml = Path.Combine(Path.GetTempPath(), "feedlist.xml");

            string[] files = {
                                 RssBanditApplication.GetFeedListFileName(),
                                 RssBanditApplication.GetFlagItemsFileName(),
                                 RssBanditApplication.GetSearchFolderFileName(),
                                 RssBanditApplication.GetSentItemsFileName(),
                                 feedlistXml
                             };

            ZipOutputStream zos;

            try
            {
                rssBanditApp.SaveApplicationState();

                //convert subscriptions.xml to feedlist.xml then save to temp folder
                using (Stream xsltStream = Resource.GetStream("Resources.feedlist2subscriptions.xslt"))
                {
                    XslCompiledTransform xslt = new XslCompiledTransform();
                    xslt.Load(new XmlTextReader(xsltStream));
                    xslt.Transform(RssBanditApplication.GetFeedListFileName(), feedlistXml);
                }

                using (MemoryStream tempStream = new MemoryStream())
                {
                    switch (remoteProtocol)
                    {
                        case RemoteStorageProtocolType.UNC:

                            FileStream fs =
                                FileHelper.OpenForWrite(
                                    Path.Combine(Environment.ExpandEnvironmentVariables(remoteLocation), remoteFileName));
                            zos = new ZipOutputStream(fs);
                            ZipFiles(files, zos);
                            zos.Close();

                            break;

                        case RemoteStorageProtocolType.NewsgatorOnline:

                            StringCollection readItems2Sync = new StringCollection(),
                                             deletedItems2Sync = new StringCollection();
                            bool feedListChanged = false;

                            //initialize resources used for threaded requests
                            eventX = new ManualResetEvent(false);

                            //initialize web services 							
                            FolderWebService fows = new FolderWebService();
                            SubscriptionWebService sws = new SubscriptionWebService();
                            FeedWebService fws = new FeedWebService();
                            PostItem pws = new PostItem();
                            LocationWebService lws = new LocationWebService();
                            fows.Credentials =
                                lws.Credentials =
                                pws.Credentials =
                                fws.Credentials =
                                sws.Credentials = new NetworkCredential(credentialUser, credentialPassword);
                            fows.Proxy = lws.Proxy = pws.Proxy = fws.Proxy = sws.Proxy = rssBanditApp.Proxy;
                            sws.NGAPITokenValue = new NGAPIToken();
                            fws.NGAPITokenValue = new CLR20.com.newsgator.services2.NGAPIToken();
                            pws.NGAPITokenValue = new CLR20.com.newsgator.services4.NGAPIToken();
                            lws.NGAPITokenValue = new CLR20.com.newsgator.services.NGAPIToken();
                            fows.NGAPITokenValue = new CLR20.com.newsgator.services1.NGAPIToken();
                            fows.NGAPITokenValue.Token =
                                lws.NGAPITokenValue.Token =
                                pws.NGAPITokenValue.Token =
                                fws.NGAPITokenValue.Token = sws.NGAPITokenValue.Token = NgosProductKey;

                            //create a location for RSS Bandit if it doesn't exist. If it already does, then this operation is redundant which is fine
                            try
                            {
                                lws.CreateLocation(NgosLocationName, true);
                            }
                            catch (Exception e)
                            {
                                _log.Error(e);
                            }

                            //since we aren't actually syncing our read state with that of Newsgator Online, we don't send a sync token
                            XmlElement opmlFeedList = sws.GetSubscriptionList(NgosLocationName, null);

                            /* convert OPML feeds list to RSS Bandit data structures */
                            XmlDocument opmlDoc = new XmlDocument();
                            opmlDoc.AppendChild(opmlDoc.ImportNode(opmlFeedList, true));

                            XmlNodeReader reader = new XmlNodeReader(rssBanditApp.FeedHandler.ConvertFeedList(opmlDoc));
                            XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof (feeds));
                            feeds myFeeds = (feeds) serializer.Deserialize(reader);
                            reader.Close();

                            ArrayList deletedFeeds = new ArrayList();

                            //foreach(NewsFeed feed in myFeeds.feed){
                            for (int i = myFeeds.feed.Count; i-- > 0; Interlocked.Increment(ref ngosFeedsToDownload))
                            {
                                INewsFeed feed = myFeeds.feed[i];

                                if (!feed.link.Equals(InvalidFeedUrl) &&
                                    rssBanditApp.FeedHandler.IsSubscribed(feed.link))
                                {
                                    NgosDownloadFeedState state = new NgosDownloadFeedState();
                                    state.feed = feed;
                                    state.deletedItems2Sync = deletedItems2Sync;
                                    state.readItems2Sync = readItems2Sync;
                                    state.fws = fws;

                                    PriorityThreadPool.QueueUserWorkItem(
                                        NgosDownloadFeedToSetReadItems, state,
                                        (int) ThreadPriority.Normal);
                                }
                                else
                                {
                                    //feed deleted locally but still in Newsgator Online
                                    feedListChanged = true;
                                    try
                                    {
                                        deletedFeeds.Add(
                                            Int32.Parse(
                                                NewsFeed.GetElementWildCardValue(feed, "http://newsgator.com/schema/opml", "id")));
                                    }
                                    catch (Exception e)
                                    {
                                        _log.Error("Error while trying to parse Newsgator feed ID for" + feed.link, e);
                                    }
                                    //we don't need to download this feed from Newsgator Online
                                    Interlocked.Decrement(ref ngosFeedsToDownload);
                                }
                            } //foreach(NewsFeed feed...)												


                            // Wait until all feed information has been obtained from Newsgator Online
                            // meaning eventX.Set() was called:
                            if (ngosFeedsToDownload != 0)
                            {
                                eventX.WaitOne(System.Threading.Timeout.Infinite, true);
                            }

                            string[] readItems = new string[readItems2Sync.Count];
                            readItems2Sync.CopyTo(readItems, 0);
                            string[] deletedItems = new string[deletedItems2Sync.Count];
                            deletedItems2Sync.CopyTo(deletedItems, 0);
                            pws.SetState(NgosLocationName, null, readItems, null /* unread posts */);

                            ArrayList addedFeeds = new ArrayList();

                            //check if a feed added locally that isn't in Newsgator Online
                            if ((myFeeds.feed.Count != rssBanditApp.FeedHandler.GetFeeds().Count) || feedListChanged)
                            {
                                feedListChanged = true;

                                NewsFeed[] currentFeeds =
                                    new NewsFeed[rssBanditApp.FeedHandler.GetFeeds().Values.Count];
                                rssBanditApp.FeedHandler.GetFeeds().Values.CopyTo(currentFeeds, 0);

                                foreach (NewsFeed f in currentFeeds)
                                {
                                    if (!myFeeds.feed.Contains(f))
                                    {
                                        addedFeeds.Add(f);
                                    }
                                } //foreach
                            } //if(myFeeds.feed.Count != ...)							

                            //update feed list in Newsgator if it differs from local one							
                            if (feedListChanged)
                            {
                                if (deletedFeeds.Count > 0)
                                {
                                    int[] deleted = new int[deletedFeeds.Count];
                                    deletedFeeds.CopyTo(deleted, 0);
                                    sws.DeleteSubscriptions(deleted);
                                }

                                if (addedFeeds.Count > 0)
                                {
                                    foreach (NewsFeed f in addedFeeds)
                                    {
                                        //we should do something better with folders
                                        try
                                        {
                                            sws.AddSubscription(f.link, GetFolderId(fows, f, opmlFeedList), null);
                                        }
                                        catch (Exception e)
                                        {
                                            //intranet or local file system URLs
                                            _log.Error("Error adding subscription " + f.link + " to NewsGator Online", e);
                                        }
                                    }
                                }
                            } //if(feedListChanged)

                            //reset counters
                            ngosDownloadedFeeds = ngosFeedsToDownload = 0;
                            break;

                        case RemoteStorageProtocolType.dasBlog:

                            //save feed list
                            rssBanditApp.FeedHandler.SaveFeedList(tempStream, FeedListFormat.OPML);
                            tempStream.Position = 0;

                            XmlDocument doc = new XmlDocument();
                            doc.Load(tempStream);

                            // Send it to the web service
                            ConfigEditingService remoteStore = new ConfigEditingService();

                            remoteStore.Url = remoteLocation;
                            remoteStore.authenticationHeaderValue = new authenticationHeader();
                            remoteStore.authenticationHeaderValue.userName = credentialUser;
                            remoteStore.authenticationHeaderValue.password = credentialPassword;
                            remoteStore.Proxy = rssBanditApp.Proxy;

                            //TODO: figure out, when we have to use Credentials????
                            //	remoteStore.Credentials = 	FeedSource.CreateCredentialsFrom(credentialUser, credentialPassword);

                            remoteStore.PostBlogroll("blogroll", doc.DocumentElement);
                                // ".opml" is appended by the service!
                            break;


                        case RemoteStorageProtocolType.FTP: // Send to FTP server
                            //save feed list 
                            zos = new ZipOutputStream(tempStream);
                            ZipFiles(files, zos);

                            tempStream.Position = 0;

                            Uri remoteUri = new Uri(remoteLocation);
                       

                            UriBuilder builder = new UriBuilder(remoteUri);
                            builder.Path = remoteFileName;


                            /* set up the FTP connection */
                            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(builder.Uri);
                            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                            ftpRequest.KeepAlive = false;
                            ftpRequest.UseBinary = true;
                            ftpRequest.UsePassive =
                                settings.GetBoolean("RemoteFeedlist/Ftp.ConnectionMode.Passive", true);
                            ftpRequest.Credentials = new NetworkCredential(credentialUser, credentialPassword);
                            ftpRequest.ContentLength = tempStream.Length;

                            /* perform upload */
                            try
                            {
                                // The buffer size is set to 2kb
                                int buffLength = 2048;
                                byte[] buff = new byte[buffLength];
                                int contentLen;

                                // Stream to which the file to be upload is written
                                Stream strm = ftpRequest.GetRequestStream();

                                // Read from the file stream 2kb at a time
                                contentLen = tempStream.Read(buff, 0, buffLength);

                                // Till Stream content ends
                                while (contentLen != 0)
                                {
                                    // Write Content from the file stream to the 
                                    // FTP Upload Stream
                                    strm.Write(buff, 0, contentLen);
                                    contentLen = tempStream.Read(buff, 0, buffLength);
                                }

                                // Close the Request Stream
                                strm.Close();
                            }
                            catch (Exception ex)
                            {
                                //ToDO: Add support for switching between active and passive mode
                                _log.Error("FTP Upload Error", ex);
                            }

                            //close zip stream 
                            zos.Close();

                            break;


                        case RemoteStorageProtocolType.WebDAV:

                            zos = new ZipOutputStream(tempStream);
                            ZipFiles(files, zos);

                            remoteUri = new Uri(remoteLocation.EndsWith("/")
                                                    ?
                                                        remoteLocation + remoteFileName
                                                    :
                                                        remoteLocation + "/" + remoteFileName);

                            tempStream.Position = 0;

                            HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(remoteUri);
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
                            zos.Close();

                            request.ContentLength = bytes.Length;

                            Stream requestStream = request.GetRequestStream();
                            requestStream.Write(bytes, 0, bytes.Length);
                            requestStream.Close();

                            request.GetResponse().Close();

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
                feeds syncedFeeds = null;

                switch (remoteProtocol)
                {
                    case RemoteStorageProtocolType.UNC:
                        // Fetch from a UNC path
                        importStream =
                            File.Open(
                                Path.Combine(Environment.ExpandEnvironmentVariables(remoteLocation), remoteFileName),
                                FileMode.Open);
                        break;

                    case RemoteStorageProtocolType.dasBlog:
                        // Fetch from the dasBlog web service
                        // Connect to the web service and request an update
                        ConfigEditingService remoteStore = new ConfigEditingService();
                        remoteStore.Url = remoteLocation;
                        remoteStore.authenticationHeaderValue = new authenticationHeader();
                        remoteStore.authenticationHeaderValue.userName = credentialUser;
                        remoteStore.authenticationHeaderValue.password = credentialPassword;
                        remoteStore.Proxy = rssBanditApp.Proxy;

                        importStream = new MemoryStream();
                        XmlElement xml = remoteStore.GetBlogroll("blogroll"); // ".opml" is appended by the service!

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(xml.OuterXml);
                        doc.Save(importStream);
                        importStream.Position = 0;
                        break;


                    case RemoteStorageProtocolType.NewsgatorOnline:

                        syncedFeeds = new feeds();
                        string syncToken = rssBanditApp.Preferences.NgosSyncToken;

                        //initialize resources used for threaded requests
                        eventX = new ManualResetEvent(false);

                        SubscriptionWebService sws = new SubscriptionWebService();
                        FeedWebService fws = new FeedWebService();
                        LocationWebService lws = new LocationWebService();
                        lws.Credentials =
                            fws.Credentials =
                            sws.Credentials = new NetworkCredential(credentialUser, credentialPassword);
                        lws.Proxy = fws.Proxy = sws.Proxy = rssBanditApp.Proxy;
                        sws.NGAPITokenValue = new NGAPIToken();
                        fws.NGAPITokenValue = new CLR20.com.newsgator.services2.NGAPIToken();
                        lws.NGAPITokenValue = new CLR20.com.newsgator.services.NGAPIToken();
                        lws.NGAPITokenValue.Token =
                            fws.NGAPITokenValue.Token = sws.NGAPITokenValue.Token = NgosProductKey;

                        //create a location for RSS Bandit if it doesn't exist. If it already does, then this operation is redundant which is fine						
                        try
                        {
                            lws.CreateLocation(NgosLocationName, true);
                        }
                        catch (Exception)
                        {
                            ;
                        }

                        XmlElement opmlFeedList = sws.GetSubscriptionList(NgosLocationName, syncToken);

                        XmlNode tokenNode =
                            opmlFeedList.Attributes.GetNamedItem("token", "http://newsgator.com/schema/opml");
                        if (tokenNode != null)
                        {
                            syncToken = tokenNode.Value;
                        }

                        /* convert OPML feeds list to RSS Bandit data structures */
                        XmlDocument opmlDoc = new XmlDocument();
                        opmlDoc.AppendChild(opmlDoc.ImportNode(opmlFeedList, true));

                        XmlNodeReader reader = new XmlNodeReader(rssBanditApp.FeedHandler.ConvertFeedList(opmlDoc));
                        XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof (feeds));
                        feeds myFeeds = (feeds) serializer.Deserialize(reader);
                        reader.Close();

                        //foreach(NewsFeed feed in myFeeds.feed){
                        for (int i = myFeeds.feed.Count; i-- > 0; Interlocked.Increment(ref ngosFeedsToDownload))
                        {
                            INewsFeed feed = myFeeds.feed[i];

                            string unseen = NewsFeed.GetElementWildCardValue(feed, "http://newsgator.com/schema/opml", "unseen");

                            if (!feed.link.Equals(InvalidFeedUrl) &&
                                ((unseen != null) && unseen.ToLower().Equals("true")))
                            {
                                NgosDownloadFeedState state = new NgosDownloadFeedState();
                                state.feed = feed;
                                state.fws = fws;

                                PriorityThreadPool.QueueUserWorkItem(NgosDownloadFeedToGetReadItems,
                                                                     state, (int) ThreadPriority.Normal);
                            }
                            else
                            {
                                Interlocked.Decrement(ref ngosFeedsToDownload);
                            }

                            if (!feed.link.Equals(InvalidFeedUrl))
                            {
                                syncedFeeds.feed.Add(feed as NewsFeed);
                            }
                        } //foreach

                        //make sure we do not overwrite feeds not supported by NewsGator
                        syncedFeeds.feed.AddRange(rssBanditApp.FeedHandler.GetNonInternetFeeds() as IEnumerable<NewsFeed>);

                        // Wait until all feed information has been obtained from Newsgator Online
                        // meaning eventX.Set() was called:
                        if (ngosFeedsToDownload != 0)
                        {
                            eventX.WaitOne(System.Threading.Timeout.Infinite, true);
                        }

                        rssBanditApp.Preferences.NgosSyncToken = syncToken; //synchronization successful
                        //reset counters
                        ngosDownloadedFeeds = ngosFeedsToDownload = 0;
                        break;


                    case RemoteStorageProtocolType.FTP:
                        // Fetch from FTP

                        /* old code 
                        Uri remoteUri = null;
                        string serverName = remoteLocation;
                        string remotePath = "/";
                        int remotePort = 21;

                        remoteUri = new Uri(remoteLocation);
                        serverName = remoteUri.Host;
                        remotePath = remoteUri.AbsolutePath;
                        if (!remoteUri.IsDefaultPort) {
                            remotePort = remoteUri.Port;
                        }

                        // look here for documentation: http://www.enterprisedt.com/downloads/csftp/doc/com.enterprisedt.net.ftp.html
                        FTPClient ftpClient = new FTPClient(serverName, remotePort);
                        bool connectionMode_Passive = settings.GetBoolean("RemoteFeedlist/Ftp.ConnectionMode.Passive", true);
                        if (connectionMode_Passive) {
                            ftpClient.ConnectMode = FTPConnectMode.PASV;
                        } else {
                            ftpClient.ConnectMode = FTPConnectMode.ACTIVE;
                        }

                        ftpClient.Login(credentialUser, credentialPassword);

                        if (remotePath.Length > 1 && remotePath.StartsWith("/")) {
                            remotePath = remotePath.Substring(1);	// ChDir fails, if it starts with a "/"
                        }

                        if (remotePath.Length > 1) {	// if not at ftp root:
                            ftpClient.ChDir(remotePath);
                        }

                        // Create a memory stream and get the file into that
                        tempFileName = Path.GetTempFileName();
                        Stream fileStream = File.Create(tempFileName);

                        try {
                            ftpClient.TransferType = FTPTransferType.BINARY;
                            ftpClient.Get(fileStream, remoteFileName);

                        } catch (System.Net.Sockets.SocketException soex) {

                            // if (soex.ErrorCode == 10060 || soex.ErrorCode == 10061){  WSAECONNTIMEOUT, WSAECONNREFUSED, see http://msdn.microsoft.com/library/en-us/winsock/winsock/windows_sockets_error_codes_2.asp?frame=true 

                            // try again, with Mode switched:
                            // see also: http://slacksite.com/other/ftp.html
                            if (connectionMode_Passive) {
                                ftpClient.ConnectMode = FTPConnectMode.ACTIVE;
                            } else {
                                ftpClient.ConnectMode = FTPConnectMode.PASV;
                            }
                            connectionMode_Passive = !connectionMode_Passive;

                            // try again:
                            ftpClient.Get(fileStream, remoteFileName);

                            // } else {
                              //  throw;
                           // } 
                        } */

                        Uri remoteUri = new Uri(remoteLocation);
           

                        UriBuilder builder = new UriBuilder(remoteUri);
                        builder.Path = remoteFileName;


                        /* set up the FTP connection */
                        FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(builder.Uri);
                        ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                        ftpRequest.KeepAlive = false;
                        ftpRequest.UseBinary = true;
                        ftpRequest.UsePassive = settings.GetBoolean("RemoteFeedlist/Ftp.ConnectionMode.Passive", true);
                        ftpRequest.Credentials = new NetworkCredential(credentialUser, credentialPassword);


                        // Create a file stream and get the file into that
                        tempFileName = Path.GetTempFileName();
                        Stream fileStream = File.Create(tempFileName);

                        /* perform download */
                        try
                        {
                            // The buffer size is set to 2kb
                            int buffLength = 2048;
                            byte[] buff = new byte[buffLength];
                            int contentLen;


                            // Stream to which the file to be upload is written
                            Stream strm = ftpRequest.GetResponse().GetResponseStream();

                            // Read from the FTP stream 2kb at a time
                            contentLen = strm.Read(buff, 0, buffLength);

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
                        catch (Exception ex)
                        {
                            //ToDO: Add support for switching between active and passive mode
                            _log.Error("FTP Upload Error", ex);
                        }

                        fileStream.Close();

                        // save new setting:
                        //settings.SetProperty("RemoteFeedlist/Ftp.ConnectionMode.Passive", connectionMode_Passive);


                        // Open the temporary file so we can import it
                        importStream = File.OpenRead(tempFileName);
                        break;

                    case RemoteStorageProtocolType.WebDAV:

                        remoteUri = new Uri(remoteLocation.EndsWith("/")
                                                ?
                                                    remoteLocation + remoteFileName
                                                :
                                                    remoteLocation + "/" + remoteFileName);

                        HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(remoteUri);
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
                try
                {
                    if ((remoteProtocol == RemoteStorageProtocolType.dasBlog) ||
                        (remoteProtocol == RemoteStorageProtocolType.dasBlog_1_3))
                    {
                        rssBanditApp.FeedHandler.ImportFeedlist(importStream);
                    }
                    else if (remoteProtocol == RemoteStorageProtocolType.NewsgatorOnline)
                    {
                        rssBanditApp.FeedHandler.ImportFeedlist(syncedFeeds, null, true /* replace */, true
                            /* keepLocalSettings */);
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
                    if (importStream != null)
                        importStream.Close();
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
            /* we support both subscriptions.xml and feedlist.xml */

            string feedlist = Path.GetFileName(RssBanditApplication.GetFeedListFileName());
            string oldschoolfeedlist = Path.GetFileName(RssBanditApplication.GetOldFeedListFileName());
            string flaggeditems = Path.GetFileName(RssBanditApplication.GetFlagItemsFileName());
            string searchfolders = Path.GetFileName(RssBanditApplication.GetSearchFolderFileName());
            string sentitems = Path.GetFileName(RssBanditApplication.GetSentItemsFileName());
            bool subscriptionsXmlSeen = false;


            if (syncFormat == SynchronizationFormat.Zip)
            {
                /* A point to consider is what happens if an exception occurs in the 
                 * middle of this process? 
                 */
                ZipInputStream zis = new ZipInputStream(stream);

                ZipEntry theEntry;
                while ((theEntry = zis.GetNextEntry()) != null)
                {
                    if (theEntry.Name == feedlist)
                    {
                        subscriptionsXmlSeen = true;
                        rssBanditApp.FeedHandler.ReplaceFeedlist(zis);
                    }
                    else if (!subscriptionsXmlSeen && (theEntry.Name == oldschoolfeedlist))
                    {
                        rssBanditApp.FeedHandler.ReplaceFeedlist(zis);
                    }
                    else if (theEntry.Name == flaggeditems)
                    {
                        LocalFeedsFeed flaggedItemsFeed = rssBanditApp.FlaggedItemsFeed;
                        LocalFeedsFeed lff = new LocalFeedsFeed(flaggedItemsFeed.link, flaggedItemsFeed.title,
                                                                flaggedItemsFeed.Description, new XmlTextReader(zis));
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
                        LocalFeedsFeed lff2 = new LocalFeedsFeed(sentItemsFeed.link, sentItemsFeed.title,
                                                                 sentItemsFeed.Description, new XmlTextReader(zis));

                        sentItemsFeed.Add(lff2);
                    }
                    else if (theEntry.Name == searchfolders)
                    {
                        XmlSerializer ser = XmlHelper.SerializerCache.GetSerializer(typeof (FinderSearchNodes));
                        rssBanditApp.FindersSearchRoot = (FinderSearchNodes) ser.Deserialize(zis);
                    }
                } //while

                zis.Close();
            } //if(syncFormat == SynchronizationFormat.Zip
        }

        #region Newsgator Online related methods

        /// <summary>
        /// Helper method which downloads a feed from Newsgator Online so we can 
        /// determine what we've read in NewsgatorOnline that aren't marked as read in RSS Bandit
        /// </summary>
        /// <param name="stateInfo">The state information needed to perform the operation</param>	
        private void NgosDownloadFeedToGetReadItems(object stateInfo)
        {
            try
            {
                NgosDownloadFeedState state = (NgosDownloadFeedState)stateInfo;

                string feedId = NewsFeed.GetElementWildCardValue(state.feed, "http://newsgator.com/schema/opml", "id");
                XmlElement feed2Sync =
                    state.fws.GetNews(Int32.Parse(feedId), NgosLocationName, null
                                      /* this.rssBanditApp.Preferences.NgosSyncToken */, false);
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(new NameTable());

                nsMgr.AddNamespace("ng", "http://newsgator.com/schema/extensions");

                foreach (XmlNode item in feed2Sync.SelectNodes("//item[ng:read='True']", nsMgr))
                {
                    // TODO: Fix this to work for feeds with guids since NewsGator doesn't provide them in the feed
                    XmlNode link = item.SelectSingleNode("./link");
                    XmlNode guid = item.SelectSingleNode("./guid");

                    if (guid != null)
                    {
                        state.feed.AddViewedStory(guid.InnerText);
                    }
                    else if (link != null)
                    {
                        state.feed.AddViewedStory(link.InnerText);
                    }
                } //foreach(XmlNode item...)	
            }
            finally
            {
                Interlocked.Increment(ref ngosDownloadedFeeds);
                if (ngosDownloadedFeeds == ngosFeedsToDownload)
                {
                    eventX.Set();
                }
            }
        }

        /// <summary>
        /// Helper method which downloads a feed from Newsgator Online so we can get 
        /// the Post IDs of items we've read that aren't marked as read on Newsgator Online. 
        /// </summary>
        /// <param name="stateInfo">The state information needed to perform the operation</param>
        private void NgosDownloadFeedToSetReadItems(object stateInfo)
        {
            try
            {
                NgosDownloadFeedState state = (NgosDownloadFeedState)stateInfo;

                INewsFeed feedInBandit = rssBanditApp.FeedHandler.GetFeeds()[state.feed.link];
                List<string> readItems = new List<string>();
                // this.GetReadItemUrls(feedInBandit, readItems); not needed since NewsGator now exposes guids
               // readItems.InsertRange(0, feedInBandit.storiesrecentlyviewed);
                readItems.AddRange(feedInBandit.storiesrecentlyviewed);

                string feedId = NewsFeed.GetElementWildCardValue(state.feed, "http://newsgator.com/schema/opml", "id");

                XmlElement feed2Sync =
                    state.fws.GetNews(Int32.Parse(feedId), NgosLocationName, null
                                      /* this.rssBanditApp.Preferences.NgosSyncToken */, false);
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(new NameTable());
                nsMgr.AddNamespace("ng", "http://newsgator.com/schema/extensions");

                foreach (XmlNode item in feed2Sync.SelectNodes("//item"))
                {
                    XmlNode link = item.SelectSingleNode("./link");
                    XmlNode guid = item.SelectSingleNode("./guid");
                    XmlNode read = item.SelectSingleNode("./ng:read", nsMgr);
                    bool itemRead = false;

                    if (read != null && read.InnerText.ToLower().Equals("true"))
                    {
                        itemRead = true;
                    }

                    string ngosId = item.SelectSingleNode("./ng:postId", nsMgr).InnerText;

                    string id;
                    if (guid != null)
                    {
                        id = guid.InnerText;
                    }
                    else if (link != null)
                    {
                        id = link.InnerText;
                    }
                    else
                    {
                        continue;
                    }

                    //see if we've read the item in RSS Bandit 
                    if (!itemRead && readItems.Contains(id))
                    {
                        state.readItems2Sync.Add(ngosId);
                    }
                        /* else if(feedInBandit.deletedstories.Contains(id)){
						state.deletedItems2Sync.Add(ngosId);
					}*/
                } //foreach(XmlNode item...)		
            }
            finally
            {
                Interlocked.Increment(ref ngosDownloadedFeeds);
                if (ngosDownloadedFeeds == ngosFeedsToDownload)
                {
                    eventX.Set();
                }
            }
        }


        /// <summary>
        /// Helper method which gets the FolderID for the specified folder from NewsGator. 	
        /// </summary>
        /// <param name="fows">The NewsGator folder Web service</param>
        /// <param name="feed">The feed whose folder ID we are seeking</param>
        /// <param name="newsgatorOpml">The NewsGator OPML file</param>
        /// <returns>The NewsGator folder ID of the item</returns>
        private static int GetFolderId(FolderWebService fows, NewsFeed feed, XmlElement newsgatorOpml)
        {
            int folderId;

            //create a category hive in NewsGator 
            folderId = CreateNewsgatorCategoryHive(fows, feed, newsgatorOpml);
            return folderId;
        }

        /// <summary>
        /// Helper method used for constructing folders in NewsGator Online.
        /// </summary>
        /// <param name="fows">The NewsGator folder Web service</param>
        /// <param name="feed">The feed whose folder ID we are seeking</param>
        /// <param name="newsgatorOpml">The NewsGator OPML file</param>
        /// <returns>The NewsGator folder ID of the item</returns>
        private static int CreateNewsgatorCategoryHive(FolderWebService fows, NewsFeed feed, XmlElement newsgatorOpml)
        {
            string category = feed.category;
            if (newsgatorOpml == null)
                return 0;


            if (category == null || category.Length == 0) 
                return 0;

            XmlElement startNode = (XmlElement) newsgatorOpml.ChildNodes[1];
            int folderId = 0;


            try
            {
                string[] catHives = category.Split(FeedSource.CategorySeparator.ToCharArray());
                XmlElement n;
                bool wasNew = false;

                foreach (string catHive in catHives)
                {
                    if (!wasNew)
                    {
                        string xpath = "child::outline[@title=" + FeedSource.buildXPathString(catHive) +
                                       " and (count(@*)= 1)]";
                        n = (XmlElement) startNode.SelectSingleNode(xpath);
                    }
                    else
                    {
                        n = null;
                    }

                    if (n == null)
                    {
                        n = startNode.OwnerDocument.CreateElement("outline");
                        n.SetAttribute("title", catHive);
                        startNode.AppendChild(n);
                        wasNew = true; // shorten search

                        /* get folderId of parent node */
                        int parentId = 0;
                        XmlNode parentFolderIdNode = startNode.Attributes.GetNamedItem("folderId", NgosOpmlNamespace);
                        if (parentFolderIdNode != null)
                        {
                            //check if it is <body> element
                            parentId = Convert.ToInt32(parentFolderIdNode.Value);
                        }
                        /* create folder in NewsGator */
                        folderId = fows.GetOrCreateFolder(catHive, parentId, "MYF");
                        /* add folderId to input OPML */
                        XmlAttribute folderIdNode = n.SetAttributeNode("folderId", NgosOpmlNamespace);
                        folderIdNode.Value = folderId.ToString();
                    }
                    startNode = n;
                } //foreach

                return folderId;
            }
            catch (Exception e)
            {
                _log.Error("Error in CreateNewsgatorCategoryHive() when attempting to create " + category, e);
            }

            return 0;
        }

        #endregion
    }
}

#region CVS Version Log

/*
 * $Log: RemoteFeedlistThreadHandler.cs,v $
 * Revision 1.59  2007/07/26 02:47:20  carnage4life
 * Fixed compile errors from last checkin
 *
 * Revision 1.58  2007/07/26 01:40:36  carnage4life
 * It seems we weren't detecting some of the instances of the passive/active mode issue during FTP upload/download
 *
 * Revision 1.57  2006/11/24 15:29:00  carnage4life
 * Fixed problem caused by the fact we weren't closing filestreams in the ZipFiles() method
 *
 * Revision 1.56  2006/10/31 13:36:35  t_rendelmann
 * fixed: various changes applied to make compile with CLR 2.0 possible without the hassle to convert it all the time again
 *
 */

#endregion