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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using log4net;
using RssBandit.Common.Logging;


namespace NewsComponents.Feed
{

    #region GoogleReaderOperation enum

    /// <summary>
    /// This is an enum that describes the set of operations that can be placed in the 
    /// queue of network operations to perform on Google Reader. 
    /// </summary>
    public enum GoogleReaderOperation : byte
    {
		/// <summary>
		/// 
		/// </summary>
        AddFeed = 51, // == queue priority!
		/// <summary>
		/// 
		/// </summary>
        AddLabel = 41,
		/// <summary>
		/// 
		/// </summary>
        DeleteFeed = 50,
		/// <summary>
		/// 
		/// </summary>
        DeleteLabel = 40,
		/// <summary>
		/// 
		/// </summary>
        MarkAllItemsRead = 61,
		/// <summary>
		/// 
		/// </summary>
        MarkSingleItemRead = 60,
		/// <summary>
		/// 
		/// </summary>
        MarkSingleItemTagged = 59,
		/// <summary>
		/// 
		/// </summary>
        MoveFeed = 45,
		/// <summary>
		/// 
		/// </summary>
        RenameFeed = 21,
		/// <summary>
		/// 
		/// </summary>
        RenameLabel = 20,
    }

    #endregion

    #region PendingGoogleReaderOperation class

    /// <summary>
    /// This is a class that is used to represent a pending operation on the index in 
    /// that is currently in the pending operation queue. 
    /// </summary>
    public class PendingGoogleReaderOperation : IEquatable<PendingGoogleReaderOperation>
    {
		/// <summary>
		/// Google Reader Operation action
		/// </summary>
        public GoogleReaderOperation Action;
		/// <summary>
		/// Google user name
		/// </summary>
        public string GoogleUserName;
		/// <summary>
		/// 
		/// </summary>
        public object[] Parameters;

        /// <summary>
        /// No default constructor
        /// </summary>
        private PendingGoogleReaderOperation()
        {
            ;
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="action">The operation to perform on the index</param>
        /// <param name="parameters">The parameters to the operation</param>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>
        public PendingGoogleReaderOperation(GoogleReaderOperation action, object[] parameters, string googleUserID)
        {
            Action = action;
            Parameters = parameters;
            GoogleUserName = googleUserID;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(PendingGoogleReaderOperation p1, PendingGoogleReaderOperation p2)
        {
            return !Equals(p1, p2);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(PendingGoogleReaderOperation p1, PendingGoogleReaderOperation p2)
        {
            return Equals(p1, p2);
        }

        /// <summary>
        /// Compares for equality of the instance and the specified pending google reader operation.
        /// </summary>
        /// <param name="pendingGoogleReaderOperation">The pending google reader operation.</param>
        /// <returns></returns>
        public bool Equals(PendingGoogleReaderOperation pendingGoogleReaderOperation)
        {
            if (pendingGoogleReaderOperation == null)
                return false;

            if (pendingGoogleReaderOperation.Action != Action)
                return false;
            if (!Equals(GoogleUserName, pendingGoogleReaderOperation.GoogleUserName))
                return false;
            if (pendingGoogleReaderOperation.Parameters.Length != Parameters.Length) return false;

            for (int i = 0; i < pendingGoogleReaderOperation.Parameters.Length; i++)
            {
                if (!pendingGoogleReaderOperation.Parameters[i].Equals(Parameters[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as PendingGoogleReaderOperation);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            int result = Action.GetHashCode();
            result = 29*result + (GoogleUserName != null ? GoogleUserName.GetHashCode() : 0);
            result = 29*result + (Parameters != null ? Parameters.GetHashCode() : 0);
            return result;
        }
    }

    #endregion

    /// <summary>
    /// Class which updates Google Reader in the background. 
    /// </summary>
    internal class GoogleReaderModifier
    {
        #region private fields 

        private readonly Dictionary<string, GoogleReaderFeedSource> FeedSources =
            new Dictionary<string, GoogleReaderFeedSource>();

        /// <summary>
        /// The thread in which class primarily runs
        /// </summary>
        private Thread GoogleReaderModifyingThread;

        /// <summary>
        /// Indicates that the thread is currently running. 
        /// </summary>
        private bool flushInprogress, threadRunning;

        /// <summary>
        /// Queue of pending network operations to perform against Google Reader
        /// </summary>
        private List<PendingGoogleReaderOperation> pendingGoogleReaderOperations =
            new List<PendingGoogleReaderOperation>();

        /// <summary>
        /// Name of the file where pending network operations are saved on shut down. 
        /// </summary>
        private readonly string pendingGoogleReaderOperationsFile = "pending-googlereader-operations.xml";

        /// <summary>
        /// for logging/tracing:
        /// </summary>
        private static readonly ILog _log = DefaultLog.GetLogger(typeof (GoogleReaderModifier));

        /// <summary>
        /// Indicates whether there is a network connection. Without one, no Google Reader operations are performed.
        /// </summary>
        private bool Offline
        {
            get
            {
                return FeedSource.Offline;
            }
        }

        #endregion

        #region constructor 

        /// <summary>
        /// Instance of this class must always be created with a path to where to save and load state. 
        /// </summary>
        private GoogleReaderModifier()
        {
            ;
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public GoogleReaderModifier(string applicationDataPath)
        {
            pendingGoogleReaderOperationsFile = Path.Combine(applicationDataPath, pendingGoogleReaderOperationsFile);
            LoadPendingOperations();
        }

        #endregion

        #region private methods

        /// <summary>
        /// Loads pending operations from disk
        /// </summary>
        private void LoadPendingOperations()
        {
            if (File.Exists(pendingGoogleReaderOperationsFile))
            {
                XmlSerializer serializer =
                    XmlHelper.SerializerCache.GetSerializer(typeof (List<PendingGoogleReaderOperation>));
                pendingGoogleReaderOperations =
                    serializer.Deserialize(XmlReader.Create(pendingGoogleReaderOperationsFile)) as
                    List<PendingGoogleReaderOperation>;
            }
        }


        /// <summary>
        /// Creates the thread in which the class primarily runs. 
        /// </summary>
        private void CreateThread()
        {
            GoogleReaderModifyingThread = new Thread(ThreadRun);
            GoogleReaderModifyingThread.Name = "GoogleReaderModifyingThread";
            GoogleReaderModifyingThread.IsBackground = true;
            threadRunning = true;
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
                if (false == Offline && false == flushInprogress &&
                    pendingGoogleReaderOperations.Count > 0)
                {
                    // do not calc percentage on a few items:
                    FlushPendingOperations(Math.Max(5, pendingGoogleReaderOperations.Count));
                    if (threadRunning)
                        Thread.Sleep(1000*1); //sleep  1 second
                }
                else
                {
                    Thread.Sleep(1000*30); //sleep  30 secs
                }
            } //while(true)
        }


        /// <summary>
        /// Performs the specified PendingGoogleReaderOperation.
        /// </summary>
        /// <param name="current">The operation to perform</param>
        private void PerformOperation(PendingGoogleReaderOperation current)
        {
            GoogleReaderFeedSource source = null;
            FeedSources.TryGetValue(current.GoogleUserName, out source);

            if (source == null)
            {
                return;
            }

            try
            {
                switch (current.Action)
                {
                    case GoogleReaderOperation.AddFeed:
                        source.AddFeedInGoogleReader(current.Parameters[0] as string, current.Parameters[1] as string);
                        break;

                    case GoogleReaderOperation.AddLabel:
                        source.AddCategoryInGoogleReader(current.Parameters[0] as string);
                        break;

                    case GoogleReaderOperation.DeleteFeed:
                        source.DeleteFeedFromGoogleReader(current.Parameters[0] as string, current.Parameters[1] as string);
                        break;

                    case GoogleReaderOperation.DeleteLabel:
                        source.DeleteCategoryInGoogleReader(current.Parameters[0] as string);
                        break;

                    case GoogleReaderOperation.MarkAllItemsRead:
                        source.MarkAllItemsAsReadInGoogleReader(current.Parameters[0] as string,
                                                                current.Parameters[1] as string);
                        break;

                    case GoogleReaderOperation.MarkSingleItemRead:
                        source.ChangeItemReadStateInGoogleReader(current.Parameters[0] as string,
                                                                 current.Parameters[1] as string,
                                                                 (bool) current.Parameters[2]);
                        break;

                    case GoogleReaderOperation.MarkSingleItemTagged:
                        source.ChangeItemTaggedStateInGoogleReader(current.Parameters[0] as string,
                                                                   current.Parameters[1] as string,
                                                                   current.Parameters[2] as string,
                                                                   (bool) current.Parameters[3]);
                        break;

                    case GoogleReaderOperation.MoveFeed:
                        source.ChangeCategoryInGoogleReader(current.Parameters[0] as string,
                                                            current.Parameters[1] as string,
                                                            current.Parameters[2] as string);
                        break;

                    case GoogleReaderOperation.RenameFeed:
                        source.RenameFeedInGoogleReader(current.Parameters[0] as string, current.Parameters[1] as string);
                        break;

                    case GoogleReaderOperation.RenameLabel:
                        source.RenameCategoryInGoogleReader(current.Parameters[0] as string,
                                                            current.Parameters[1] as string);
                        break;

                    default:
                        Debug.Assert(false, "Unknown Google Reader operation: " + current.Action);
                        return;
                }
            }
            catch (Exception e)
            {
                //TODO: Rethrow to handle time outs and connections cancelled by host
                _log.Error("Error in GoogleReaderModifier.PerformOperation:", e);
            }
            ;
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
                flushInprogress = true;

                do
                {
                    PendingGoogleReaderOperation pendingOp = null;


                    //perform all queued operations on the index
                    lock (pendingGoogleReaderOperations)
                    {
                        if (pendingGoogleReaderOperations.Count > 0)
                        {
                            pendingOp = pendingGoogleReaderOperations[0];
                        }
                    } //lock 

                    //Optimizing the index is an expensive operation so we don't want to 
                    //call it if the queue is being flushed since it may delay application exit. 
                    if (pendingOp != null)
                    {
                        PerformOperation(pendingOp);
                        pendingGoogleReaderOperations.RemoveAt(0);
                    }

                    batchedItemsAmount--;

                    //potential race condition on this.pendingIndexOperations.Count but chances are very low
                } while (pendingGoogleReaderOperations.Count > 0 && batchedItemsAmount >= 0);
            }
            finally
            {
                flushInprogress = false;
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Starts the Google Reader thread
        /// </summary>
        public void StartBackgroundThread()
        {
            if (!threadRunning)
            {
                CreateThread();
            }
        }

        /// <summary>
        /// Stops the Google Reader thread and saves pending operations to disk. 
        /// </summary>
        public void StopBackgroundThread()
        {
            threadRunning = false;

            // wait for current running network operations to finish
            while (flushInprogress)
                Thread.Sleep(50);

            SavePendingOperations();
        }


        /// <summary>
        /// Empties the pending operations queue and deletes the pending operations file
        /// </summary>
        public void CancelPendingOperations()
        {
            lock (pendingGoogleReaderOperations)
            {
                pendingGoogleReaderOperations.Clear();
            }
            NewsComponents.Utils.FileHelper.Delete(pendingGoogleReaderOperationsFile);
        }

        /// <summary>
        /// Saves pending operations to disk
        /// </summary>
        public void SavePendingOperations()
        {
            XmlSerializer serializer =
                XmlHelper.SerializerCache.GetSerializer(typeof (List<PendingGoogleReaderOperation>));
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            serializer.Serialize(XmlWriter.Create(pendingGoogleReaderOperationsFile, settings), pendingGoogleReaderOperations);
        }

        /// <summary>
        /// Adds the feed source to the list of GoogleReaderFeedSources being modified by this class 
        /// </summary>
        /// <param name="source"></param>
        public void RegisterFeedSource(GoogleReaderFeedSource source)
        {
            FeedSources.Add(source.GoogleUserName, source);
        }

        /// <summary>
        /// Removes the feed source from the list of GoogleReaderFeedSources being modified by this class. 
        /// </summary>
        /// <param name="source"></param>
        public void UnregisterFeedSource(GoogleReaderFeedSource source)
        {
            FeedSources.Remove(source.GoogleUserName);
        }


		/// <summary>
		/// Checks whether the specified URL is in the pending operations queue as a new URL subscription to
		/// Google Reader.
		/// </summary>
		/// <param name="feedUrl">The feed URL.</param>
		/// <returns>
		/// True if there is a pending GoogleReaderOperation.AddFeed for the target URL in the pending operation
		/// queue
		/// </returns>
        internal bool IsPendingSubscription(string feedUrl)
        {
            return
                pendingGoogleReaderOperations.Any(
                    p => p.Action == GoogleReaderOperation.AddFeed && p.Parameters[0].Equals(feedUrl));
        }

        /// <summary>
        /// Enqueus an item that deletes the category in Google Reader
        /// </summary>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>            
        /// <param name="name">The name of the category to delete</param>
        public void DeleteCategoryInGoogleReader(string googleUserID, string name)
        {
            var op = new PendingGoogleReaderOperation(GoogleReaderOperation.DeleteLabel, new object[] {name},
                                                      googleUserID);

            lock (pendingGoogleReaderOperations)
            {
                pendingGoogleReaderOperations.Add(op);
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
            lock (pendingGoogleReaderOperations)
            {
                //don't bother adding a folder that was later renamed, simply add the final named folder
                PendingGoogleReaderOperation addFolderOp =
                    pendingGoogleReaderOperations.Find(
                        oldOp => oldOp.Action == GoogleReaderOperation.AddLabel && oldName.Equals(oldOp.Parameters[0]));

                if (addFolderOp == null)
                {
                    //also check if category was renamed then renamed again
                    PendingGoogleReaderOperation renameOp =
                        pendingGoogleReaderOperations.Find(
                            oldOp =>
                            oldOp.Action == GoogleReaderOperation.RenameLabel && oldName.Equals(oldOp.Parameters[1]));

                    if (renameOp == null)
                    {
                        var op = new PendingGoogleReaderOperation(GoogleReaderOperation.RenameLabel,
                                                                  new object[] {oldName, newName}, googleUserID);
                        pendingGoogleReaderOperations.Add(op);
                    }
                    else
                    {
                        pendingGoogleReaderOperations.Remove(renameOp);
                        pendingGoogleReaderOperations.Add(
                            new PendingGoogleReaderOperation(GoogleReaderOperation.RenameLabel,
                                                             new[] {renameOp.Parameters[0], newName}, googleUserID));
                    }
                }
                else
                {
                    pendingGoogleReaderOperations.Remove(addFolderOp);
                    pendingGoogleReaderOperations.Add(new PendingGoogleReaderOperation(GoogleReaderOperation.AddLabel,
                                                                                       new object[] {newName},
                                                                                       googleUserID));
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
        public void RenameFeedInGoogleReader(string googleUserID, string url, string title)
        {
            var op = new PendingGoogleReaderOperation(GoogleReaderOperation.RenameFeed, new object[] {url, title},
                                                      googleUserID);

            lock (pendingGoogleReaderOperations)
            {
                pendingGoogleReaderOperations.Add(op);
            }
        }

        /// <summary>
        /// Enqueues an event to mark all items older than the the specified date as read in Google Reader
        /// </summary>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>     
        /// <param name="feedUrl">The feed URL</param>
        /// <param name="olderThan">The date from which to mark all items older than that date as read</param>
        public void MarkAllItemsAsReadInGoogleReader(string googleUserID, string feedUrl, string olderThan)
        {
            var op = new PendingGoogleReaderOperation(GoogleReaderOperation.MarkAllItemsRead,
                                                      new object[] {feedUrl, olderThan}, googleUserID);

            lock (pendingGoogleReaderOperations)
            {
                pendingGoogleReaderOperations.Add(op);
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
            var op = new PendingGoogleReaderOperation(GoogleReaderOperation.MarkSingleItemRead,
                                                      new object[] {feedId, itemId, beenRead}, googleUserID);

            lock (pendingGoogleReaderOperations)
            {
                pendingGoogleReaderOperations.Add(op);
            }
        }

        /// <summary>
        /// Enqueues an event to tag or untag an item in Google Reader
        /// </summary>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>            
        /// <param name="feedId">The ID of the parent feed in Google Reader</param>
        /// <param name="itemId">The atom:id of the news item</param>        
        /// <param name="tag">The tag that is being applied or removed</param>
        /// <param name="tagged">Indicates whether the item was tagged or untagged</param>
        public void ChangeItemTaggedStateInGoogleReader(string googleUserID, string feedId, string itemId, string tag,
                                                        bool tagged)
        {
            var op = new PendingGoogleReaderOperation(GoogleReaderOperation.MarkSingleItemTagged,
                                                      new object[] {feedId, itemId, tag, tagged}, googleUserID);

            lock (pendingGoogleReaderOperations)
            {
                pendingGoogleReaderOperations.Add(op);
            }
        }

        /// <summary>
        /// Enqueues an event that changes the category of a feed in Google Reader
        /// </summary>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>                    
        /// <param name="feedUrl">The feed URL</param>
        /// <param name="newCategory">The new category for the feed</param>
        /// <param name="oldCategory">The old category of the feed.</param>       
        public void ChangeCategoryInGoogleReader(string googleUserID, string feedUrl, string newCategory,
                                                 string oldCategory)
        {
            var op = new PendingGoogleReaderOperation(GoogleReaderOperation.MoveFeed,
                                                      new object[] {feedUrl, newCategory, oldCategory}, googleUserID);

            lock (pendingGoogleReaderOperations)
            {
                pendingGoogleReaderOperations.Add(op);
            }
        }

        /// <summary>
        /// Enqueues an event that adds a feed to the list of user's subscriptions in Google Reader
        /// </summary>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>                 
        /// <param name="feedUrl">The URL of the feed to add</param>
        /// <param name="title">The title of the new subscription</param>      
        /// <returns>A GoogleReaderSubscription that describes the newly added feed</returns>
        public void AddFeedInGoogleReader(string googleUserID, string feedUrl, string title)
        {
            var op = new PendingGoogleReaderOperation(GoogleReaderOperation.AddFeed, new object[] {feedUrl, title},
                                                      googleUserID);

            lock (pendingGoogleReaderOperations)
            {
                pendingGoogleReaderOperations.Add(op);
            }
        }


        /// <summary>
        /// Adds the category in Google Reader
        /// </summary>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>                         
        /// <param name="name">The name of the category to add</param>
        internal void AddCategoryInGoogleReader(string googleUserID, string name)
        {
            var op = new PendingGoogleReaderOperation(GoogleReaderOperation.AddLabel, new object[] {name}, googleUserID);

            lock (pendingGoogleReaderOperations)
            {
                pendingGoogleReaderOperations.Add(op);
            }
        }

        /// <summary>
        /// Enqueues an event that deletes a feed from the list of user's subscriptions in Google Reader
        /// </summary>
        /// <param name="googleUserID">The Google User ID of the account under which this operation will be performed.</param>                         
        /// <param name="feedUrl">The URL of the feed to delete</param>
        /// <param name="feedTitle">The title of the feed to delete</param>
        public void DeleteFeedFromGoogleReader(string googleUserID, string feedUrl, string feedTitle)
        {
            var deleteOp = new PendingGoogleReaderOperation(GoogleReaderOperation.DeleteFeed, 
                                                            new object[] {feedUrl, feedTitle},googleUserID);

            lock (pendingGoogleReaderOperations)
            {
                //remove all pending operations related to the feed since it is going to be unsubscribed
                IEnumerable<PendingGoogleReaderOperation> ops2remove 
                  = pendingGoogleReaderOperations.Where(op => op.GoogleUserName.Equals(deleteOp.GoogleUserName)
                                                        && op.Parameters.Contains(feedUrl));

                var ops2remove_array = ops2remove.ToArray(); //prevent collection modified Exceptions

                foreach (PendingGoogleReaderOperation op2remove in ops2remove_array)
                {
                    pendingGoogleReaderOperations.Remove(op2remove); 
                }

                pendingGoogleReaderOperations.Add(deleteOp);   
            }
        }

        #endregion
    }
}