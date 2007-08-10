#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

//#define TRACE_INDEX_OPS

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using log4net;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using NewsComponents.Collections;
using NewsComponents.Utils;
using RssBandit.Common.Logging;
using Directory=System.IO.Directory;

namespace NewsComponents.Search
{

	#region IndexOperation enum

	/// <summary>
	/// This is an enum that describes the set of operations that can be placed in the 
	/// queue of operations to perform on the search index by the index modifying thread. 
	/// </summary>
	internal enum IndexOperation:byte {
		AddSingleDocument = 10, // == queue priority!
		AddMultipleDocuments = 11,	
		// ReIndex operation is a delete(docs) first followed by add(docs)
		// so Delete must have the highest priority for batched index 
		// operations:
		DeleteDocuments = 50, 
		// delete of feeds should follow add/remove docs, or we waste
		// the index with old non existing feed item docs if there is still
		// a pending add/delete docs in the queue.
		DeleteFeed = 2,
		OptimizeIndex = 1,
	}

	#endregion

	#region PendingIndexOperation class

	/// <summary>
	/// This is a class that is used to represent a pending operation on the index in 
	/// that is currently in the pending operation queue. 
	/// </summary>
	internal class PendingIndexOperation{
		public IndexOperation Action;
		public object[] Parameters;

		/// <summary>
		/// No default constructor
		/// </summary>
		private PendingIndexOperation(){;}
			
		/// <summary>
		/// Constructor 
		/// </summary>
		/// <param name="action">The operation to perform on the index</param>
		/// <param name="parameters">The parameters to the operation</param>
		public PendingIndexOperation(IndexOperation action, object[] parameters){
			this.Action = action;
			this.Parameters = parameters; 
		}
	}

	internal delegate void FinishedIndexOperationEventHandler(object sender, FinishedIndexOperationEventArgs e);
	internal class FinishedIndexOperationEventArgs: EventArgs
	{
		public readonly PendingIndexOperation Operation;
		public FinishedIndexOperationEventArgs(PendingIndexOperation op) {
			this.Operation = op;
		}
	}

	#endregion
	
	/// <summary>
	/// Serialize the index modifications (only one index modifier,
	/// IndexReader or IndexWriter can change the index at the same directory
	/// the same time).
	/// </summary>
	internal class LuceneIndexModifier: IDisposable 
	{
		#region fields

		/// <summary>
		/// To be used to synchronize index modifications. Only one
		/// IndexWriter/Reader can modify the index at a time!
		/// </summary>
		public object SyncRoot = new Object();

		public event FinishedIndexOperationEventHandler FinishedIndexOperation;

		private LuceneSettings settings;
		private Lucene.Net.Store.Directory indexBaseDirectory;
		private bool open, flushInprogress = false, threadRunning = false;
		private Thread IndexModifyingThread;
		private PriorityQueue pendingIndexOperations = new PriorityQueue(); 

		// logging/tracing:
		private static readonly ILog _log = Log.GetLogger(typeof(LuceneIndexModifier));	
		private static readonly LuceneInfoWriter _logHelper = new LuceneInfoWriter(_log); 

		protected internal IndexWriter indexWriter = null;
		protected internal IndexReader indexReader = null;

		private const int TimeToDelayBeforeRetry = 50; 

		#endregion

		#region Constructors

		public LuceneIndexModifier(LuceneSettings settings)
		{
			this.settings = settings;
			
			this.indexBaseDirectory = settings.GetIndexDirectory();
			this.Init();

			CreateIndexerThread();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LuceneIndexModifier"/> class.
		/// </summary>
		/// <param name="baseDirectory">The index base directory.</param>
		public LuceneIndexModifier(string baseDirectory) {
			if (!StringHelper.EmptyOrNull(baseDirectory)) {
				if (!Directory.Exists(baseDirectory))
					Directory.CreateDirectory(baseDirectory);
				this.indexBaseDirectory = Lucene.Net.Store.FSDirectory.GetDirectory(baseDirectory, false);
				this.Init();
			}

			CreateIndexerThread();
		}

		#endregion

		#region public properties/methods (general)
		
		/// <summary>
		/// Gets or sets the base directory for the index.
		/// </summary>
		/// <value>The base directory.</value>
		public Lucene.Net.Store.Directory BaseDirectory
		{
			get { return indexBaseDirectory; }
			set { indexBaseDirectory = value; }
		}

		/// <summary>
		/// Gets true if an Index exists.
		/// </summary>
		/// <returns></returns>
		public bool IndexExists {
			get { return IndexReader.IndexExists(this.BaseDirectory); }
		}

		/// <summary> 
		/// Make sure all changes are written to disk (pending operations
		/// and index).
		/// </summary>
		/// <exception cref="IOException" ></exception>
		public virtual void Flush()
		{
			// force flush all:
			FlushPendingOperations(Int32.MaxValue);
			FlushIndex();
		}

		/// <summary>
		/// Resets the pending operations (clear) and
		/// reset the index (re-create new) as one operation.
		/// </summary>
		public virtual void Reset() {
			ResetPendingOperations();
			ResetIndex();
		}
		
		/// <summary>
		/// Stops the indexer (thread),
		/// performs all pending operations on the index and 
		/// flushes all pending I/O writes to disk. 
		/// </summary>
		public void StopIndexer() 
		{
			this.StopIndexerThread();
			
			// wait for batched indexing tasks:
			while (this.flushInprogress)
				Thread.Sleep(50);
			
			this.Flush();
		}
		
		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return this.settings.ToString();
		}

		#endregion

		#region public methods (PendingIndexOperation related)
		
		/// <summary> Adds a document to this index, using the provided culture. 
		/// If the document contains more than
		/// {@link #SetMaxFieldLength(int)} terms for a given field, the remainder are
		/// discarded.
		/// </summary>
		/// <remarks>This operation is added to the pending index operations queue.</remarks>
		/// <exception cref="InvalidOperationException">If the index is closed </exception>
		public virtual void  Add(Document doc, string culture) {
			lock(this.pendingIndexOperations.SyncRoot){
				this.pendingIndexOperations.Enqueue((int)IndexOperation.AddSingleDocument,
					new PendingIndexOperation(IndexOperation.AddSingleDocument, new object[]{doc, culture}));
			}
		}

		/// <summary> Adds a document to this index, using the provided culture. 
		/// If the document contains more than
		/// {@link #SetMaxFieldLength(int)} terms for a given field, the remainder are
		/// discarded.
		/// </summary>
		/// <exception cref="InvalidOperationException">If the index is closed </exception>
		public virtual void AddRange(Document[] docs, string culture)
		{
			lock (this.pendingIndexOperations.SyncRoot)
			{
				this.pendingIndexOperations.Enqueue((int)IndexOperation.AddMultipleDocuments,
					new PendingIndexOperation(IndexOperation.AddMultipleDocuments, new object[] { docs, culture }));
			}
		}

		/// <summary> Deletes all documents containing <code>term</code>.
		/// This is useful if one uses a document field to hold a unique ID string for
		/// the document.  Then to delete such a document, one merely constructs a
		/// term with the appropriate field and the unique ID string as its text and
		/// passes it to this method.  Returns the number of documents deleted.
		/// </summary>
		/// <returns> the number of documents deleted
		/// </returns>
		/// <seealso cref="IndexReader.DeleteDocuments(Term)">
		/// </seealso>
		/// <exception cref="InvalidOperationException">If the index is closed </exception>
		public virtual void Delete(Term term)
		{
			lock (this.pendingIndexOperations.SyncRoot)
			{
				this.pendingIndexOperations.Enqueue((int)IndexOperation.DeleteDocuments,
					new PendingIndexOperation(IndexOperation.DeleteDocuments, new object[] { term }));
			}
		}
		
		/// <summary>
		/// Deletes the feed from the index. Same as Delete(Term),
		/// but with a lower priority.
		/// </summary>
		/// <param name="term">The term.</param>
		public virtual void DeleteFeed(Term term) {
			lock (this.pendingIndexOperations.SyncRoot) {
				// differs only in the priority, the operation is the same:
				this.pendingIndexOperations.Enqueue((int)IndexOperation.DeleteFeed,
					new PendingIndexOperation(IndexOperation.DeleteDocuments, new object[] { term }));
			}
		}
		
		/// <summary> Merges all segments together into a single segment, optimizing an index
		/// for search.
		/// </summary>
		/// <seealso cref="IndexWriter.Optimize()">
		/// </seealso>
		/// <exception cref="InvalidOperationException">If the index is closed </exception>
		public virtual void Optimize()
		{
			lock (this.pendingIndexOperations.SyncRoot)
			{
				this.pendingIndexOperations.Enqueue((int)IndexOperation.OptimizeIndex,
					new PendingIndexOperation(IndexOperation.OptimizeIndex, null));
			}
		}

		#endregion

		#region public methods (Index related)
		
		/// <summary>
		/// Creates the index.
		/// </summary>
		public void CreateIndex() {
			IndexWriter writer = new IndexWriter(this.settings.GetIndexDirectory(), new StandardAnalyzer(), true);
			writer.Close();
		}

		/// <summary> Returns the number of documents currently in this index.</summary>
		/// <seealso cref="IndexWriter.DocCount()">
		/// </seealso>
		/// <seealso cref="IndexReader.NumDocs()">
		/// </seealso>
		/// <exception cref="InvalidOperationException">If the index is closed </exception>
		public virtual int NumberOfDocuments()
		{
			lock (SyncRoot)
			{
				AssureOpen();
				if (indexWriter != null)
				{
					return indexWriter.DocCount();
				}
				else
				{
					return indexReader.NumDocs();
				}
			}
		}

		/// <summary>
		/// Deletes the whole index, then creates a new one 
		/// without any documents.
		/// </summary>
		public virtual void ResetIndex()
		{
			this.Close();
			if (this.BaseDirectory is RAMDirectory) {
				// nothing to do...?
			}
			else if (this.BaseDirectory is FSDirectory &&
				settings.IndexPath != null)
			{
				Directory.Delete(settings.IndexPath, true);
				Directory.CreateDirectory(settings.IndexPath);
			} 
			else {
				Debug.Assert(false, "Unhandled BaseDirectory type: " + this.BaseDirectory.GetType().FullName);
			}

			this.BaseDirectory = settings.GetIndexDirectory();
			this.Init();
		}

		/// <summary> 
		/// Close this index, writing all pending changes to disk.
		/// </summary>
		public virtual void Close()
		{
			lock (this.SyncRoot)
			{
				if (!open) return;
				if (indexWriter != null)
				{
					try { indexWriter.Close(); } catch (IOException) { ;}
					indexWriter = null;
				}
				else if (indexReader != null)
				{
					try { indexReader.Close(); } catch (IOException) { ;}
					indexReader = null;
				}
				open = false;
			}
		}

		#endregion

		#region private methods (IndexThread related)

		private void CreateIndexerThread () {
			IndexModifyingThread = new Thread(new ThreadStart(this.ThreadRun));
			IndexModifyingThread.Name = "BanditSearchIndexModifyingThread";
			IndexModifyingThread.IsBackground = true;
			//TR: does not really help to reduce CPU hogging if running on CLR 2.0:
			//IndexModifyingThread.Priority = ThreadPriority.Lowest;
			this.threadRunning = true;
			IndexModifyingThread.Start();
		}
		
		/// <summary>
		/// This thread loops continously popping items from the pendingIndexOperations 
		/// queue and performing the actions. This ensures that there is only one thread
		/// modifying the index at any given time. 
		/// </summary>
		private void ThreadRun() 
		{
			while(threadRunning) {
				if (false == this.flushInprogress && 
					this.pendingIndexOperations.Count > 0)
				{
					// do not calc percentage on a few items:
					FlushPendingOperations(Math.Max(100, this.pendingIndexOperations.Count / 10));
					if (threadRunning)
						Thread.Sleep(1000 * 10); //sleep  10 secs
				}else{
			        Thread.Sleep(1000*30); //sleep  30 secs
			    }
			}//while(true)
			
			//PendingIndexOperation current = null; 
			
			//while(true)
			//{
			//    lock(this.pendingIndexOperations.SyncRoot){
			//        if(this.pendingIndexOperations.Count > 0){
			//            current = this.pendingIndexOperations.Dequeue() as PendingIndexOperation;
			//        }								
			//    }
			//    if(current != null){					
			//        this.PerformOperation(current); 
			//    }else{
			//        Thread.Sleep(1000*1); //sleep  
			//    }

			//    current = null; 
			//}//while(true)
		}

		private void StopIndexerThread() {
			threadRunning = false;
		}

		#endregion
		
		#region private methods (PendingIndexOperation related)

		/// <summary>
		/// Performs the specified PendingIndexOperation.
		/// </summary>
		/// <param name="current">The operation to perform</param>
		private void PerformOperation(PendingIndexOperation current){
		
			try { 

				switch(current.Action){
					
					case IndexOperation.AddSingleDocument:
						this.AddSingleDocument((Document)current.Parameters[0], (string)current.Parameters[1]);
						break;

					case IndexOperation.AddMultipleDocuments:
						this.AddMultipleDocuments((Document[])current.Parameters[0], (string)current.Parameters[1]);
						break; 

					case IndexOperation.DeleteDocuments:
						this.DeleteTerm((Term)current.Parameters[0]);
						break;

					case IndexOperation.OptimizeIndex:
						this.OptimizeIndex();
						break;
					default:
						Debug.Assert(false, "Unknown index operation: " + current.Action);
						return;
				}

			}catch(FileNotFoundException fe){ 
				/* index has gotten corrupted and refers to a non-existence index file */
				this.ResetIndex(); 
				_log.Error("Index is corrupted, recreating index:", fe);				
			}catch(IndexOutOfRangeException ioore){
				/* index has gotten corrupted, */ 
				this.ResetIndex(); 
				_log.Error("Index is corrupted, recreating index:", ioore);	
			}
			
			RaiseFinishedIndexOperationEvent(current);
		}

		private void FlushPendingOperations(int batchedItemsAmount) 
		{
			try {
				this.flushInprogress = true;
				
				do {
					PendingIndexOperation pendingOp = null;

					//perform all queued operations on the index
					lock (this.pendingIndexOperations.SyncRoot) {
						if (this.pendingIndexOperations.Count > 0) {
							pendingOp = this.pendingIndexOperations.Dequeue() as PendingIndexOperation;
						}
					} //lock 

					//Optimizing the index is an expensive operation so we don't want to 
					//call it if the queue is being flushed since it may delay application exit. 
					if ((pendingOp != null) && (pendingOp.Action != IndexOperation.OptimizeIndex)) {
						this.PerformOperation(pendingOp);
					}

					batchedItemsAmount--;

					//potential race condition on this.pendingIndexOperations.Count but chances are very low
				} while (this.pendingIndexOperations.Count > 0 && batchedItemsAmount >= 0);
			
			} finally {
				this.flushInprogress = false;
			}
		}

		private void ResetPendingOperations() 
		{
			lock(this.pendingIndexOperations.SyncRoot){
				this.pendingIndexOperations.Clear();
			}
		}

		private void RaiseFinishedIndexOperationEvent(PendingIndexOperation current) {
			if (this.FinishedIndexOperation != null)
				this.FinishedIndexOperation(this, new FinishedIndexOperationEventArgs(current));
		}
		
		#endregion

		#region private methods (Index modification related)

		/// <summary> Adds a document to this index, using the provided culture. 
		/// If the document contains more than
		/// {@link #SetMaxFieldLength(int)} terms for a given field, the remainder are
		/// discarded.
		/// </summary>
		/// <exception cref="InvalidOperationException">If the index is closed </exception>
		private void AddSingleDocument(Document doc, string culture)
		{
			if (doc == null) return;
//#if TRACE_INDEX_OPS
			_log.DebugFormat("Adding document {0} to the index",  doc.GetField(LuceneSearch.Keyword.ItemLink));
//#endif
			lock (SyncRoot)
			{
				AssureOpen();
				CreateIndexWriter();
				try{
					if (!StringHelper.EmptyOrNull(culture))
						indexWriter.AddDocument(doc, LuceneSearch.GetAnalyzer(culture));
					else
						indexWriter.AddDocument(doc);
				}catch(IOException ioe){
					_log.Error("IOException adding document to the index", ioe); 

                /* see  http://issues.apache.org/jira/browse/LUCENE-665 */ 
					if(ioe.Message.IndexOf("segments.new") != -1){
						FileHelper.MoveFile(Path.Combine(this.settings.IndexPath, "segments.new"), Path.Combine(this.settings.IndexPath, "segments"), MoveFileFlag.ReplaceExisting);
					}else if(ioe.Message.IndexOf("deletable.new") != -1){
						FileHelper.MoveFile(Path.Combine(this.settings.IndexPath, "deletable.new"), Path.Combine(this.settings.IndexPath, "deletable"), MoveFileFlag.ReplaceExisting);
					}
				}catch(UnauthorizedAccessException uae){
					_log.Error("Access denied error while adding document to the index", uae); 

					/* see  http://issues.apache.org/jira/browse/LUCENE-665 */ 
					if(uae.Message.IndexOf("segments.new") != -1){
						FileHelper.MoveFile(Path.Combine(this.settings.IndexPath, "segments.new"), Path.Combine(this.settings.IndexPath, "segments"), MoveFileFlag.ReplaceExisting);
					}else if(uae.Message.IndexOf("deletable.new") != -1){
						FileHelper.MoveFile(Path.Combine(this.settings.IndexPath, "deletable.new"), Path.Combine(this.settings.IndexPath, "deletable"), MoveFileFlag.ReplaceExisting);
					}
				}

			}

		}

		/// <summary> Adds a document to this index, using the provided culture. 
		/// If the document contains more than
		/// {@link #SetMaxFieldLength(int)} terms for a given field, the remainder are
		/// discarded.
		/// </summary>
		/// <exception cref="InvalidOperationException">If the index is closed </exception>
		private void AddMultipleDocuments(Document[] docs, string culture)
		{
			if (docs == null || docs.Length == 0) return;

			Analyzer analyzer = null;
			if (!StringHelper.EmptyOrNull(culture))
				analyzer = LuceneSearch.GetAnalyzer(culture);
#if TRACE_INDEX_OPS
			_log.Info("Add multiple IndexDoc(s)...");
#endif			
			lock (SyncRoot) 
			{
				AssureOpen();
				CreateIndexWriter();
				
				for (int i = 0; i < docs.Length; i++)
					if (analyzer != null)
						indexWriter.AddDocument(docs[i], analyzer);
					else
						indexWriter.AddDocument(docs[i]);
			}
		}

		/// <summary> Deletes all documents containing <code>term</code>.
		/// This is useful if one uses a document field to hold a unique ID string for
		/// the document.  Then to delete such a document, one merely constructs a
		/// term with the appropriate field and the unique ID string as its text and
		/// passes it to this method.  Returns the number of documents deleted.
		/// </summary>
		/// <returns> the number of documents deleted
		/// </returns>
		/// <seealso cref="IndexReader.Delete(Term)">
		/// </seealso>
		/// <exception cref="InvalidOperationException">If the index is closed </exception>
		private int DeleteTerm(Term term)
		{
//#if TRACE_INDEX_OPS
			_log.DebugFormat("Deleting documents that match '{0}' from the index", term.ToString());
//#endif			
			lock (SyncRoot)
			{
				AssureOpen();
				CreateIndexReader();
				return indexReader.DeleteDocuments(term);
			}
		}

		private void FlushIndex() 
		{
#if TRACE_INDEX_OPS
			_log.Info("FlushIndex...");
#endif
			lock (SyncRoot)
			{
				AssureOpen();
				if (indexWriter != null)
				{
					indexWriter.Close();
					indexWriter = null;
					CreateIndexWriter();
				}
				else
				{
					indexReader.Close();
					indexReader = null;
					CreateIndexReader();
				}
			}
		}

		/// <summary> Merges all segments together into a single segment, optimizing an index
		/// for search.
		/// </summary>
		/// <seealso cref="IndexWriter.Optimize()">
		/// </seealso>
		/// <exception cref="InvalidOperationException">If the index is closed </exception>
		private void OptimizeIndex()
		{
#if TRACE_INDEX_OPS
			_log.Info("OptimizeIndex...");
#endif
			//since this significantly modifies the index, we don't want other operations 
			//occuring at the same time
			lock (SyncRoot)
			{
				AssureOpen();
				CreateIndexWriter();
				indexWriter.Optimize();
			}
		}

		#endregion

		#region private methods (general)

		/// <summary> Initialize an IndexWriter.</summary>
		/// <exception cref="IOException"></exception>
		protected internal void Init() {
			lock (this.SyncRoot) {
				this.indexWriter =new IndexWriter(this.settings.GetIndexDirectory(), 
					LuceneSearch.GetAnalyzer(LuceneSearch.DefaultLanguage), !this.IndexExists);
				open = true;
			}
		}
		
		/// <summary> Throw an IllegalStateException if the index is closed.</summary>
		/// <exception cref="InvalidOperationException"> If index is closed</exception>
		protected internal virtual void AssureOpen() {
			if (!open) {
				throw new InvalidOperationException("Index is closed");
			}
		}

		/// <summary> Close the IndexReader and open an IndexWriter.</summary>
		/// <exception cref="IOException"></exception>
		protected internal virtual void  CreateIndexWriter() {
			if (this.indexWriter == null) {
				if (this.indexReader != null) {
#if TRACE_INDEX_OPS
					_log.Info("Closing IndexReader...");
#endif
					try { this.indexReader.Close(); }
					catch (IOException) { ;}
					catch(UnauthorizedAccessException uae){
						/* Sometimes we get exceptions here about renaming deletable.new 
						 * to deletable. Found solution at 
						 * http://mail-archives.apache.org/mod_mbox/lucene-java-dev/200608.mbox/%3c22121027.1157066785442.JavaMail.jira@brutus%3e 
						 */ 
						Thread.Sleep(TimeToDelayBeforeRetry); 
						_log.Debug("Error closing index reader:", uae);
						try { this.indexReader.Close(); } catch(Exception e){_log.Debug("Error closing index writer after sleeping:", e);}
					}
					this.indexReader = null;
				}
#if TRACE_INDEX_OPS
				_log.Info("Creating IndexWriter...");
#endif
				this.indexWriter = new IndexWriter(this.BaseDirectory, 
					LuceneSearch.GetAnalyzer(LuceneSearch.DefaultLanguage), false);
				this.indexWriter.SetInfoStream( _logHelper); 
			}
		}
		
		/// <summary> Close the IndexWriter and open an IndexReader.</summary>
		/// <exception cref="IOException"></exception>
		protected internal virtual void  CreateIndexReader() {
			if (this.indexReader == null) {
				if (this.indexWriter != null) {
#if TRACE_INDEX_OPS
					_log.Info("Closing IndexWriter...");
#endif
					try { this.indexWriter.Close(); } catch(IOException ioe){_log.Debug("Error closing index writer:", ioe);}
					this.indexWriter = null;
				}
#if TRACE_INDEX_OPS
				_log.Info("Creating IndexReader...");
#endif
				this.indexReader = IndexReader.Open(this.BaseDirectory);
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose() {
			Close();
		}

		#endregion
	}


	/**
	 * Helper class which writes internal Lucene debug info to RSS Bandit trace logs. 
	 */
	internal class LuceneInfoWriter: TextWriter{

		private ILog logger = null; 
		
		/// <summary>
		/// We don't want a default constructor
		/// </summary>
		private LuceneInfoWriter(){;}


		/// <summary>
		/// 
		/// </summary>
		public override System.Text.Encoding Encoding {
			get {
				return null;
			}
		}


		/// <summary>
		/// Constructor accepts logger as input
		/// </summary>
		/// <param name="logger">The logger to which we'll actually write the information</param>
		internal LuceneInfoWriter(ILog logger){
			this.logger = logger; 					
		}

		public override void Write(string value) {
			logger.Debug(value); 			
		}

		public override void Write(string format, params object[] args) {
			logger.DebugFormat(format, args); 		
		}
		
	}
}



#region CVS Version Log
/*
 * $Log: LuceneIndexModifier.cs,v $
 * Revision 1.25  2007/08/02 01:00:06  carnage4life
 * Changes related to shipping ShadowCat beta 2
 *
 * Revision 1.24  2007/07/29 17:38:33  carnage4life
 * Added DTDs for XHTML Strict & Transitional
 *
 * Revision 1.23  2007/07/29 15:20:28  carnage4life
 * Removed calls to InternetGetCookie due to issue reportred at http://www.rssbandit.org/forum/topic.asp?whichpage=1&TOPIC_ID=2080&#4080
 *
 * Revision 1.22  2007/07/29 05:24:09  carnage4life
 * Fixed Lucene crashes when adding docments to the index
 *
 * Revision 1.21  2007/07/26 02:49:30  carnage4life
 * Added debug output for IndexWriter
 *
 * Revision 1.20  2007/07/21 15:49:39  carnage4life
 * Detected another kind of index corruption in Lucene
 *
 * Revision 1.19  2007/07/21 12:26:21  t_rendelmann
 * added support for "portable Bandit" version
 *
 * Revision 1.18  2007/07/11 17:48:22  carnage4life
 * Fixed compile error. :(
 *
 * Revision 1.17  2007/07/11 17:36:52  carnage4life
 * Fixed random crashes due to error renaming file "deleteable.new" to "deletable" in search index folder.
 *
 * Revision 1.16  2007/05/05 10:45:43  t_rendelmann
 * fixed: lucene indexing issues caused by thread race condition
 *
 * Revision 1.15  2007/04/03 13:06:49  t_rendelmann
 * fixed the Flush() impl.
 *
 * Revision 1.14  2007/04/01 16:52:41  t_rendelmann
 * only comments added; CVS history moved to bottom of file
 *
 * Revision 1.13  2007/03/13 17:19:15  t_rendelmann
 * changed: now using a priority queue to index docs
 *
 * Revision 1.12  2007/03/04 17:33:05  carnage4life
 * 1.) Added exception handling to ignore "already closed" exception from IndexReader & IndexWriter.
 * 2.) Prevent a NullReferenceException in Flush() by always checking if IndexReader and IndexWriter are null before calling Close()
 *
 * Revision 1.11  2007/03/03 15:48:24  carnage4life
 * Fixed issue with NullReferenceException being thrown on Close()
 *
 * Revision 1.10  2007/02/17 12:34:33  t_rendelmann
 * fixed: p_parentID can also be the empty string
 *
 * Revision 1.9  2007/02/10 17:22:50  carnage4life
 * Added code to handle FileNotFoundException in LuceneIndexModifier. We now reset the index when this occurs because it indicates that the search index has been corrupted.
 *
 * Revision 1.8  2006/12/20 16:45:58  carnage4life
 * Added lock to OptimizeIndex() to ensure that index operations aren't performed while the index is being optimized
 *
 * Revision 1.7  2006/12/07 13:17:18  t_rendelmann
 * now Lucene.OptimizeIndex() calls are only at startup and triggered by index folder modification datetime
 *
 * Revision 1.6  2006/11/21 06:36:22  t_rendelmann
 * small camelCase parameter name fixes
 *
 * Revision 1.5  2006/11/10 20:33:54  carnage4life
 * Fixed issue where application exit may take too long if OptimizeIndex is in the index operation queue
 *
 * Revision 1.4  2006/11/08 16:30:00  carnage4life
 * Fixed time consuming lock when flushing index operations
 *
 * Revision 1.3  2006/11/05 01:23:55  carnage4life
 * Reduced time consuming locks in indexing code
 *
 * Revision 1.2  2006/10/03 08:27:37  t_rendelmann
 * fixed some code comments
 *
 * Revision 1.1  2006/09/29 18:11:59  t_rendelmann
 * a) integrated lucene index refreshs;
 * b) now using a centralized defined category separator;
 * c) unified decision about storage relevant changes to feed, feed and feeditem properties;
 *
 */
#endregion
