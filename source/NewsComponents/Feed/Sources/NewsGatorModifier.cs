using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

using log4net;

using RssBandit.Common.Logging;

using NewsComponents.Collections;

namespace NewsComponents.Feed
{

    #region NewsGatorOperation enum

    /// <summary>
    /// This is an enum that describes the set of operations that can be placed in the 
    /// queue of network operations to perform on NewsGator Online. 
    /// </summary>
    public enum NewsGatorOperation : byte
    {
        AddFeed = 51, // == queue priority!
        AddFolder = 41,
        DeleteFeed = 50,
        DeleteFolder = 40,
        MarkAllItemsRead = 61,
        MarkSingleItemRead = 60,
        MarkSingleItemStarred = 59,
        MoveFeed = 45,
        RenameFeed = 21,
        RenameFolder = 20,
    }

    #endregion

    #region PendingNewsGatorOperation class

    /// <summary>
    /// This is a class that is used to represent a pending operation on the index in 
    /// that is currently in the pending operation queue. 
    /// </summary>
    public class PendingNewsGatorOperation
    {
        public NewsGatorOperation Action;
        public string NewsGatorUserName;
        public object[] Parameters;

        /// <summary>
        /// No default constructor
        /// </summary>
        private PendingNewsGatorOperation() { ;}

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="action">The operation to perform on NewsGator Online</param>
        /// <param name="parameters">The parameters to the operation</param>
        /// <param name="ngUserID">The NewsGator ID of the account under which this operation will be performed.</param>
        public PendingNewsGatorOperation(NewsGatorOperation action, object[] parameters, string ngUserID)
        {
            this.Action = action;
            this.Parameters = parameters;
            this.NewsGatorUserName = ngUserID;
        }
    }

    #endregion 


    #region NewsGatorModifier

    /// <summary>
    /// Class which updates NewsGator Online in the background. 
    /// </summary>
    internal class NewsGatorModifier
    {

        #region private fields


        private Dictionary<string, NewsGatorFeedSource> FeedSources = new Dictionary<string, NewsGatorFeedSource>();

        /// <summary>
        /// The thread in which class primarily runs
        /// </summary>
        private Thread NewsGatorModifyingThread;

        /// <summary>
        /// Indicates that the thread is currently running. 
        /// </summary>
        private bool flushInprogress = false, threadRunning = false;

        /// <summary>
        /// Queue of pending network operations to perform against NewsGator Online
        /// </summary>
        private List<PendingNewsGatorOperation> pendingNewsGatorOperations = new List<PendingNewsGatorOperation>();

        /// <summary>
        /// Name of the file where pending network operations are saved on shut down. 
        /// </summary>
        private readonly string pendingNewsGatorOperationsFile = "pending-newsgator-operations.xml";

        /// <summary>
        /// for logging/tracing:
        /// </summary>
        private static readonly ILog _log = Log.GetLogger(typeof(NewsGatorModifier));

        /// <summary>
        /// Indicates whether there is a network connection. Without one, no Google Reader operations are performed.
        /// </summary>
        private bool Offline
        {
            get
            {
                if (FeedSources.Count > 0)
                {
                    return FeedSources.Values.ElementAt(0).Offline;
                }
                return false;
            }
        }

        #endregion

         #region constructor 

        /// <summary>
        /// Instance of this class must always be created with a path to where to save and load state. 
        /// </summary>
        private NewsGatorModifier() { ;}

        /// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
        public NewsGatorModifier(string applicationDataPath)
        {
            pendingNewsGatorOperationsFile = Path.Combine(applicationDataPath, pendingNewsGatorOperationsFile);
            this.LoadPendingOperations(); 
            this.CreateThread(); 
        }		

        #endregion 


        #region private methods

        /// <summary>
        /// Loads pending operations from disk
        /// </summary>
        private void LoadPendingOperations()
        {
            if (File.Exists(this.pendingNewsGatorOperationsFile))
            {
                XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(List<PendingGoogleReaderOperation>));
                pendingNewsGatorOperations = serializer.Deserialize(XmlReader.Create(this.pendingNewsGatorOperationsFile)) as List<PendingNewsGatorOperation>;
            }
        }


        /// <summary>
        /// Creates the thread in which the class primarily runs. 
        /// </summary>
        private void CreateThread()
        {
            NewsGatorModifyingThread = new Thread(this.ThreadRun);
            NewsGatorModifyingThread.Name = "NewsGatorModifyingThread";
            NewsGatorModifyingThread.IsBackground = true;
            this.threadRunning = true;
            NewsGatorModifyingThread.Start();
        }

        /// <summary>
        /// This thread loops continously popping items from the pendingGoogleReaderOperations 
        /// queue and performing the actions. This ensures that there is only one thread
        /// modifying the index at any given time. 
        /// </summary>
        private void ThreadRun()
        {
            while (threadRunning)
            {
                if (false == this.Offline && false == this.flushInprogress &&
                    this.pendingNewsGatorOperations.Count > 0)
                {
                    // do not calc percentage on a few items:
                    FlushPendingOperations(Math.Max(10, this.pendingNewsGatorOperations.Count / 10));
                    if (threadRunning)
                        Thread.Sleep(1000 * 5); //sleep  5 secs
                }
                else
                {
                    Thread.Sleep(1000 * 30); //sleep  30 secs
                }
            }//while(true)
        }


        /// <summary>
        /// Performs the specified PendingNewsGatorOperation.
        /// </summary>
        /// <param name="current">The operation to perform</param>
        private void PerformOperation(PendingNewsGatorOperation current)
        {
            NewsGatorFeedSource source = null;
            this.FeedSources.TryGetValue(current.NewsGatorUserName, out source);

            if (source == null)
            {
                return;
            }

            try
            {
                switch (current.Action)
                {
                    case NewsGatorOperation.MarkAllItemsRead:
                        source.MarkAllItemsAsReadInNewsGatorOnline(current.Parameters[0] as string, current.Parameters[1] as string);
                        break;

                    default:
                        Debug.Assert(false, "Unknown NewsGator Online operation: " + current.Action);
                        return;
                }

            }
            catch (Exception e)
            { //TODO: Rethrow to handle time outs and connections cancelled by host
                _log.Error("Error in GoogleReaderModifier.PerformOperation:", e);
            };
        }

        /// <summary>
        /// Performs a set of pending network operations from the pendingGoogleReaderOperations queue on the 
        /// Google Reader service. 
        /// </summary>
        /// <param name="batchedItemsAmount">The number of operations that should be performed.</param>
        private void FlushPendingOperations(int batchedItemsAmount)
        {
            try
            {
                this.flushInprogress = true;

                do
                {
                    PendingNewsGatorOperation pendingOp = null;


                    //perform all queued operations on the index
                    lock (this.pendingNewsGatorOperations)
                    {
                        if (this.pendingNewsGatorOperations.Count > 0)
                        {
                            pendingOp = this.pendingNewsGatorOperations[0];
                        }
                    } //lock 

                    //Optimizing the index is an expensive operation so we don't want to 
                    //call it if the queue is being flushed since it may delay application exit. 
                    if (pendingOp != null)
                    {
                        this.PerformOperation(pendingOp);
                        this.pendingNewsGatorOperations.RemoveAt(0);
                    }

                    batchedItemsAmount--;

                    //potential race condition on this.pendingIndexOperations.Count but chances are very low
                } while (this.pendingNewsGatorOperations.Count > 0 && batchedItemsAmount >= 0);

            }
            finally
            {
                this.flushInprogress = false;
            }
        }

        #endregion 


        #region public methods 


        /// <summary>
        /// Stops the Google Reader thread and saves pending operations to disk. 
        /// </summary>
        public void StopBackgroundThread()
        {
            this.threadRunning = false;

            // wait for current running network operations to finish
            while (this.flushInprogress)
                Thread.Sleep(50);

            this.SavePendingOperations();
        }

        /// <summary>
        /// Saves pending operations to disk
        /// </summary>
        public void SavePendingOperations()
        {
            XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(List<PendingNewsGatorOperation>));
            serializer.Serialize(XmlWriter.Create(this.pendingNewsGatorOperationsFile), this.pendingNewsGatorOperations);
        }

        /// <summary>
        /// Adds the feed source to the list of NewsGatorFeedSources being modified by this class 
        /// </summary>
        /// <param name="source"></param>
        public void RegisterFeedSource(NewsGatorFeedSource source)
        {
            this.FeedSources.Add(source.NewsGatorUserName, source);
        }

        /// <summary>
        /// Removes the feed source from the list of NewsGatorFeedSources being modified by this class. 
        /// </summary>
        /// <param name="source"></param>
        public void UnregisterFeedSource(NewsGatorFeedSource source)
        {
            this.FeedSources.Remove(source.NewsGatorUserName);
        }


      /// <summary>
        /// Marks all items older than the the specified date as read in NewsGatorOnline
        /// </summary>
        /// <param name="newsgatorUserID">The NewsGator User ID of the account under which this operation will be performed.</param>
        /// <param name="feedUrl">The feed URL</param>
        /// <param name="syncToken">The synchronization token that identifies which items should be marked as read</param>
        public void MarkAllItemsAsReadInNewsGatorOnline(string newsgatorUserID, string feedUrl, string syncToken)
        {
            PendingNewsGatorOperation op = new PendingNewsGatorOperation(NewsGatorOperation.MarkAllItemsRead, new object[] { feedUrl, syncToken }, newsgatorUserID);

            lock (this.pendingNewsGatorOperations)
            {
                this.pendingNewsGatorOperations.Add(op);
            }
        }

        #endregion 
    }

#endregion 
}
