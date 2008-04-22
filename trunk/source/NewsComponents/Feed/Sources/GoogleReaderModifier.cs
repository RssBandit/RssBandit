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

    #region GoogleReaderOperation enum

    /// <summary>
    /// This is an enum that describes the set of operations that can be placed in the 
    /// queue of network operations to perform on Google Reader. 
    /// </summary>
    public enum GoogleReaderOperation : byte
    {
        AddFeed = 51, // == queue priority!
        AddLabel = 41,
        DeleteFeed = 50,
        DeleteLabel = 40,
        MarkAllItemsRead = 61,
        MarkSingleItemRead = 60,
        MarkSingleItemStarred = 59, 
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
    public class PendingGoogleReaderOperation
    {
        public GoogleReaderOperation Action;
        public string GoogleUserName; 
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
            this.GoogleUserName = googleUserID;
        }

        /// <summary>
        /// Compares two objects for equality
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            PendingGoogleReaderOperation pop = obj as PendingGoogleReaderOperation;

            if (pop == null) return false;

            if (pop.Action != this.Action) return false;

            if (pop.Parameters.Length != this.Parameters.Length) return false;

            for (int i = 0; i < pop.Parameters.Length; i++)
            {
                if (!pop.Parameters[i].Equals(this.Parameters[i]))
                    return false;
            }

            return true;
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
        private List<PendingGoogleReaderOperation> pendingGoogleReaderOperations = new List<PendingGoogleReaderOperation>();

        /// <summary>
        /// Name of the file where pending network operations are saved on shut down. 
        /// </summary>
        private readonly string pendingGoogleOperationsFile = "pending-googlereader-operations.xml"; 

        /// <summary>
        /// for logging/tracing:
        /// </summary>
        private static readonly ILog _log = Log.GetLogger(typeof(GoogleReaderModifier));

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
        private GoogleReaderModifier() { ;}

        /// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
        public GoogleReaderModifier(string applicationDataPath)
        {
            pendingGoogleOperationsFile = Path.Combine(applicationDataPath, pendingGoogleOperationsFile);
            this.LoadPendingOperations(); 
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
                XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(List<PendingGoogleReaderOperation>));
                pendingGoogleReaderOperations = serializer.Deserialize(XmlReader.Create(this.pendingGoogleOperationsFile)) as List<PendingGoogleReaderOperation>;
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
                if (false == this.Offline && false == this.flushInprogress &&
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
        /// Performs the specified PendingGoogleReaderOperation.
        /// </summary>
        /// <param name="current">The operation to perform</param>
        private void PerformOperation(PendingGoogleReaderOperation current)
        {
            GoogleReaderFeedSource source = null;
            this.FeedSources.TryGetValue(current.GoogleUserName, out source);

            if (source == null)
            {
                return; 
            }

            try
            {
                switch (current.Action)
                {
                    case GoogleReaderOperation.AddFeed:
                        source.AddFeedInGoogleReader(current.Parameters[0] as string); 
                        break;

                    case GoogleReaderOperation.AddLabel:
                        source.AddCategoryInGoogleReader(current.Parameters[0] as string);
                        break;

                    case GoogleReaderOperation.DeleteFeed:
                        source.DeleteFeedFromGoogleReader(current.Parameters[0] as string); 
                        break;

                    case GoogleReaderOperation.DeleteLabel:
                        source.DeleteCategoryInGoogleReader(current.Parameters[0] as string);
                        break;

                    case GoogleReaderOperation.MarkAllItemsRead:
                        source.MarkAllItemsAsReadInGoogleReader(current.Parameters[0] as string, (DateTime)current.Parameters[1]);
                        break;

                    case GoogleReaderOperation.MarkSingleItemRead:
                        source.ChangeItemReadStateInGoogleReader(current.Parameters[0] as string, current.Parameters[1] as string, (bool) current.Parameters[2]); 
                        break;

                    case GoogleReaderOperation.MarkSingleItemStarred:
                        source.ChangeItemStarredStateInGoogleReader(current.Parameters[0] as string, current.Parameters[1] as string, (bool)current.Parameters[2]);
                        break;

                    case GoogleReaderOperation.MoveFeed:
                        source.ChangeCategoryInGoogleReader(current.Parameters[0] as string, current.Parameters[1] as string, current.Parameters[2] as string);
                        break;

                    case GoogleReaderOperation.RenameFeed:
                        source.RenameFeedInGoogleReader(current.Parameters[0] as string, current.Parameters[1] as string); 
                        break;

                    case GoogleReaderOperation.RenameLabel:
                        source.RenameCategoryInGoogleReader(current.Parameters[0] as string, current.Parameters[1] as string);
                        break;

                    default:
                        Debug.Assert(false, "Unknown Google Reader operation: " + current.Action);
                        return;
                }

            }
            catch (Exception e) { //TODO: Rethrow to handle time outs and connections cancelled by host
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
                   PendingGoogleReaderOperation pendingOp = null;

                    
                    //perform all queued operations on the index
                    lock (this.pendingGoogleReaderOperations)
                    {
                        if (this.pendingGoogleReaderOperations.Count > 0)
                        {
                            pendingOp = this.pendingGoogleReaderOperations[0];
                        }
                    } //lock 

                    //Optimizing the index is an expensive operation so we don't want to 
                    //call it if the queue is being flushed since it may delay application exit. 
                    if (pendingOp != null)
                    {
                        this.PerformOperation(pendingOp);
                        this.pendingGoogleReaderOperations.RemoveAt(0); 
                    }

                    batchedItemsAmount--;
                     
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
        /// Starts the Google Reader thread
        /// </summary>
        public void StartBackgroundThread()
        {
            if (!this.threadRunning)
            {
                this.CreateThread();
            }
        }

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
            XmlSerializer serializer = XmlHelper.SerializerCache.GetSerializer(typeof(List<PendingGoogleReaderOperation>));
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true; 
            serializer.Serialize(XmlWriter.Create(this.pendingGoogleOperationsFile, settings), this.pendingGoogleReaderOperations);            
        }

        /// <summary>
        /// Adds the feed source to the list of GoogleReaderFeedSources being modified by this class 
        /// </summary>
        /// <param name="source"></param>
        public void RegisterFeedSource(GoogleReaderFeedSource source)
        {
            this.FeedSources.Add(source.GoogleUserName, source); 
        }

        /// <summary>
        /// Removes the feed source from the list of GoogleReaderFeedSources being modified by this class. 
        /// </summary>
        /// <param name="source"></param>
        public void UnregisterFeedSource(GoogleReaderFeedSource source)
        {
            this.FeedSources.Remove(source.GoogleUserName); 
        }

         /// <summary>
        /// Enqueus an item that deletes the category in Google Reader
        /// </summary>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>            
        /// <param name="name">The name of the category to delete</param>
        public void DeleteCategoryInGoogleReader(string googleUserID, string name)
        {
            PendingGoogleReaderOperation op = new PendingGoogleReaderOperation(GoogleReaderOperation.DeleteLabel, new object[] { name }, googleUserID);

            lock (this.pendingGoogleReaderOperations)
            {
                this.pendingGoogleReaderOperations.Add(op);
            }
        }

         /// <summary>
        /// Renames the specified category
        /// </summary>        
        /// <remarks>This method assumes that the caller will rename categories on INewsFeed instances directly instead
        /// of having this method do it automatically.</remarks>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>                   
        /// <param name="oldName">The old name of the category</param>
        /// <param name="newName">The new name of the category</param>        
        public void RenameCategoryInGoogleReader(string googleUserID, string oldName, string newName)
        {          
            lock (this.pendingGoogleReaderOperations)
            {
                //don't bother adding a folder that was later renamed, simply add the final named folder
                PendingGoogleReaderOperation addFolderOp = this.pendingGoogleReaderOperations.Find(oldOp => oldOp.Action == GoogleReaderOperation.AddLabel && oldName.Equals(oldOp.Parameters[0]));

                if (addFolderOp == null)
                {
                    //also check if category was renamed then renamed again
                    PendingGoogleReaderOperation renameOp = this.pendingGoogleReaderOperations.Find(oldOp => oldOp.Action == GoogleReaderOperation.RenameLabel && oldName.Equals(oldOp.Parameters[1]));

                    if (renameOp == null)
                    {
                        PendingGoogleReaderOperation op = new PendingGoogleReaderOperation(GoogleReaderOperation.RenameLabel, new object[] { oldName, newName }, googleUserID);
                        this.pendingGoogleReaderOperations.Add(op);
                    }
                    else
                    {
                        this.pendingGoogleReaderOperations.Remove(renameOp);
                        this.pendingGoogleReaderOperations.Add(new PendingGoogleReaderOperation(GoogleReaderOperation.RenameLabel, new object[] { renameOp.Parameters[0], newName }, googleUserID));
                    }
                }
                else
                {                   
                    this.pendingGoogleReaderOperations.Remove(addFolderOp);
                    this.pendingGoogleReaderOperations.Add(new PendingGoogleReaderOperation(GoogleReaderOperation.AddLabel, new object[] { newName }, googleUserID));
                }
            }
        }

        /// <summary>
        /// Enqueues an event to change the title of a subscribed feed in Google Reader
        /// </summary>
        /// <remarks>This method does nothing if the new title is empty or null</remarks>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>     
        /// <param name="url">The feed URL</param>
        /// <param name="title">The new title</param>        
        public void RenameFeed(string googleUserID, string url, string title)
        {
            PendingGoogleReaderOperation op = new PendingGoogleReaderOperation(GoogleReaderOperation.RenameFeed, new object[] { url, title }, googleUserID);
            
            lock (this.pendingGoogleReaderOperations)
            {
                this.pendingGoogleReaderOperations.Add(op);
            }
        }

        /// <summary>
        /// Enqueues an event to mark all items older than the the specified date as read in Google Reader
        /// </summary>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>     
        /// <param name="feedUrl">The feed URL</param>
        /// <param name="olderThan">The date from which to mark all items older than that date as read</param>
        public void MarkAllItemsAsReadInGoogleReader(string googleUserID, string feedUrl, DateTime olderThan)
        {
             PendingGoogleReaderOperation op = new PendingGoogleReaderOperation(GoogleReaderOperation.MarkAllItemsRead, new object[] { feedUrl, olderThan }, googleUserID);

            lock (this.pendingGoogleReaderOperations)
            {
                this.pendingGoogleReaderOperations.Add(op);
            }
        }

         /// <summary>
        /// Enqueues an event to mark an item as read or unread in Google Reader
        /// </summary>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>            
        /// <param name="feedId">The ID of the parent feed in Google Reader</param>
        /// <param name="itemId">The atom:id of the news item</param>        
        /// <param name="beenRead">Indicates whether the item was marked as read or unread</param>
        public void ChangeItemReadStateInGoogleReader(string googleUserID, string feedId, string itemId, bool beenRead)
        {
           PendingGoogleReaderOperation op = new PendingGoogleReaderOperation(GoogleReaderOperation.MarkSingleItemRead, new object[] { feedId, itemId, beenRead }, googleUserID);

            lock (this.pendingGoogleReaderOperations)
            {
                this.pendingGoogleReaderOperations.Add(op);
            }
        }

        /// <summary>
        /// Enqueues an event to star or unstar an item in Google Reader
        /// </summary>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>            
        /// <param name="feedId">The ID of the parent feed in Google Reader</param>
        /// <param name="itemId">The atom:id of the news item</param>        
        /// <param name="starred">Indicates whether the item was starred or unstarred</param>
        public void ChangeItemStarredStateInGoogleReader(string googleUserID, string feedId, string itemId, bool starred)
        {
            PendingGoogleReaderOperation op = new PendingGoogleReaderOperation(GoogleReaderOperation.MarkSingleItemStarred, new object[] { feedId, itemId, starred }, googleUserID);

            lock (this.pendingGoogleReaderOperations)
            {
                this.pendingGoogleReaderOperations.Add(op);
            }
        }

          /// <summary>
        /// Enqueues an event that changes the category of a feed in Google Reader
        /// </summary>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>                    
        /// <param name="feedUrl">The feed URL</param>
        /// <param name="newCategory">The new category for the feed</param>
        /// <param name="oldCategory">The old category of the feed.</param>       
        public void ChangeCategoryInGoogleReader(string googleUserID, string feedUrl, string newCategory, string oldCategory)
        {
            PendingGoogleReaderOperation op = new PendingGoogleReaderOperation(GoogleReaderOperation.MoveFeed, new object[] { feedUrl, newCategory, oldCategory }, googleUserID);

            lock (this.pendingGoogleReaderOperations)
            {
                this.pendingGoogleReaderOperations.Add(op);
            }
        }

         /// <summary>
        /// Enqueues an event that adds a feed to the list of user's subscriptions in Google Reader
        /// </summary>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>                 
        /// <param name="feedUrl">The URL of the feed to add</param>
        /// <returns>A GoogleReaderSubscription that describes the newly added feed</returns>
        public void AddFeedInGoogleReader(string googleUserID, string feedUrl)
        {
            PendingGoogleReaderOperation op = new PendingGoogleReaderOperation(GoogleReaderOperation.AddFeed, new object[] { feedUrl }, googleUserID);

            lock (this.pendingGoogleReaderOperations)
            {
                this.pendingGoogleReaderOperations.Add(op);
            }
        }


        
        /// <summary>
        /// Adds the category in Google Reader
        /// </summary>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>                         
        /// <param name="name">The name of the category to add</param>
        internal void AddCategoryInGoogleReader(string googleUserID, string name)
        {
            PendingGoogleReaderOperation op = new PendingGoogleReaderOperation(GoogleReaderOperation.AddLabel, new object[] { name }, googleUserID);

            lock (this.pendingGoogleReaderOperations)
            {
                this.pendingGoogleReaderOperations.Add(op);
            }           
        }

        /// <summary>
        /// Enqueues an event that deletes a feed from the list of user's subscriptions in Google Reader
        /// </summary>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>                         
        /// <param name="feedUrl">The URL of the feed to delete</param>
        public void DeleteFeedFromGoogleReader(string googleUserID, string feedUrl)
        {
            PendingGoogleReaderOperation op = new PendingGoogleReaderOperation(GoogleReaderOperation.DeleteFeed, new object[] { feedUrl }, googleUserID);

            lock (this.pendingGoogleReaderOperations)
            {
                this.pendingGoogleReaderOperations.Add(op);
            }
        }

        #endregion

    }
}
