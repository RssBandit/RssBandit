using System;
using System.Collections.Generic;
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

    #region GoogleReaderOperation enum

    /// <summary>
    /// This is an enum that describes the set of operations that can be placed in the 
    /// queue of operations to perform on the search index by the index modifying thread. 
    /// </summary>
    internal enum GoogleReaderOperation : byte
    {
        AddFeed = 51, // == queue priority!
        AddLabel = 41,
        DeleteFeed = 50,
        DeleteLabel = 40,
        MarkAllItemsRead = 61,
        MarkSingleItemRead = 60,
        MoveFeed  = 45,
        RenameFeed = 21,
        RenameLabel = 20,
    }

    #endregion

    #region PendingGoogleReaderOperation class

    /// <summary>
    /// This is a class that is used to represent a pending operation on the index in 
    /// that is currently in the pending operation queue. 
    /// </summary>
    internal class PendingGoogleReaderOperation
    {
        public GoogleReaderOperation Action;
        public string GoogleUserId; 
        public object[] Parameters;

        /// <summary>
        /// No default constructor
        /// </summary>
        private PendingGoogleReaderOperation() { ;}

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="action">The operation to perform on the index</param>
        /// <param name="parameters">The parameters to the operation</param>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>
        public PendingGoogleReaderOperation(GoogleReaderOperation action, object[] parameters, string googleUserID)
        {
            this.Action = action;
            this.Parameters = parameters;
            this.GoogleUserId = googleUserId;
        }
    }

    #endregion 

    /// <summary>
    /// Class which updates Google Reader in the background. 
    /// </summary>
    internal class GoogleReaderModifier
    {

        #region private fields 


        private Dictionary<string, GoogleReaderFeedSource> FeedSources = new Dictionary<string, GoogleReaderFeedSource>();

        /// <summary>
        /// The thread in which class primarily runs
        /// </summary>
        private Thread GoogleReaderModifyingThread;

        /// <summary>
        /// Indicates that the thread is currently running. 
        /// </summary>
        private bool flushInprogress = false, threadRunning = false;

        /// <summary>
        /// Queue of pending network operations to perform against Google Reader
        /// </summary>
        private PriorityQueue pendingGoogleReaderOperations = new PriorityQueue();

        private readonly string pendingGoogleOperationsFile = "pending-googlereader-operations.xml"; 

        /// <summary>
        /// for logging/tracing:
        /// </summary>
        private static readonly ILog _log = Log.GetLogger(typeof(GoogleReaderModifier));

        #endregion

        #region constructor 

        /// <summary>
        /// Instance of this class must always be created with a path to where to save and load state. 
        /// </summary>
        private GoogleReaderModifier() { ;}

        /// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
        public GoogleReaderModifier(string applicationDataPath)
        {
            pendingGoogleOperationsFile = Path.Combine(applicationDataPath, pendingGoogleOperationsFile);
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
            if (File.Exists(this.pendingGoogleOperationsFile))
            {
                XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(PriorityQueue));
                pendingGoogleReaderOperations = serializer.Deserialize(XmlReader.Create(this.pendingGoogleOperationsFile)) as PriorityQueue;
            }
        }


        /// <summary>
        /// Creates the thread in which the class primarily runs. 
        /// </summary>
        private void CreateThread()
        {
            GoogleReaderModifyingThread = new Thread(this.ThreadRun);
            GoogleReaderModifyingThread.Name = "GoogleReaderModifyingThread";
            GoogleReaderModifyingThread.IsBackground = true;
            this.threadRunning = true;
            GoogleReaderModifyingThread.Start();
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
                if (false == this.flushInprogress &&
                    this.pendingGoogleReaderOperations.Count > 0)
                {
                    // do not calc percentage on a few items:
                    FlushPendingOperations(Math.Max(10, this.pendingGoogleReaderOperations.Count / 10));
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
                   /* PendingIndexOperation pendingOp = null;

                    //perform all queued operations on the index
                    lock (this.pendingIndexOperations.SyncRoot)
                    {
                        if (this.pendingIndexOperations.Count > 0)
                        {
                            pendingOp = this.pendingIndexOperations.Dequeue() as PendingIndexOperation;
                        }
                    } //lock 

                    //Optimizing the index is an expensive operation so we don't want to 
                    //call it if the queue is being flushed since it may delay application exit. 
                    if ((pendingOp != null) && (pendingOp.Action != IndexOperation.OptimizeIndex))
                    {
                        this.PerformOperation(pendingOp);
                    }

                    batchedItemsAmount--;
                    */ 
                    //potential race condition on this.pendingIndexOperations.Count but chances are very low
                } while (this.pendingGoogleReaderOperations.Count > 0 && batchedItemsAmount >= 0);

            }
            finally
            {
                this.flushInprogress = false;
            }
        }

        #endregion 

        #region public methods

        /// <summary>
        /// Saves pending operations to disk
        /// </summary>
        public void SavePendingOperations()
        {
            XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(PriorityQueue));
            serializer.Serialize(XmlWriter.Create(this.pendingGoogleOperationsFile), this.pendingGoogleReaderOperations);            
        }

        /// <summary>
        /// Adds the feed source to the list of GoogleReaderFeedSources being modified by this class 
        /// </summary>
        /// <param name="source"></param>
        public void RegisterFeedSource(GoogleReaderFeedSource source)
        {
            this.FeedSources.Add(source.GoogleUserId, source); 
        }

        /// <summary>
        /// Removes the feed source from the list of GoogleReaderFeedSources being modified by this class. 
        /// </summary>
        /// <param name="source"></param>
        public void UnregisterFeedSource(GoogleReaderFeedSource source)
        {
            this.FeedSources.Remove(source.GoogleUserId); 
        }

        #endregion

    }
}
