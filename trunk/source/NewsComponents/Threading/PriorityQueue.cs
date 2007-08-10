#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

// Stephen Toub
// stoub@microsoft.com
// 
// PriorityQueue.cs
// A C# implementation of a max priority queue.

#region Namespaces
using System;
using System.Collections;
#endregion

namespace NewsComponents.Collections
{
	/// <summary>A priority queue.</summary>
	public class PriorityQueue : ICollection
	{
		#region Member Variables
		/// <summary>The binary heap on which the priority queue is based.</summary>
		private BinaryHeap _heap;
		#endregion

		#region Construction
		/// <summary>Initialize the queue.</summary>
		public PriorityQueue() { _heap = new BinaryHeap(); }

		/// <summary>Initialize the queue.</summary>
		/// <param name="queue">The queue is intialized with a shalled-copy of this queue.</param>
		public PriorityQueue(PriorityQueue queue)
		{
			_heap = queue._heap.Clone();
		}
		#endregion

		#region Methods
		/// <summary>Enqueues an item to the priority queue.</summary>
		/// <param name="priority">The priority of the object to be enqueued.</param>
		/// <param name="value">The object to be enqueued.</param>
		public virtual void Enqueue(int priority, object value)
		{
			_heap.Insert(priority, value);
		}

		/// <summary>Dequeues an object from the priority queue.</summary>
		/// <returns>The top item (max priority) from the queue.</returns>
		public virtual object Dequeue()
		{
			return _heap.Remove();
		}

		/// <summary>Empties the queue.</summary>
		public virtual void Clear()
		{
			_heap.Clear();
		}
		#endregion

		#region Implementation of ICollection
		/// <summary>Copies the priority queue to an array.</summary>
		/// <param name="array">The array to which the queue should be copied.</param>
		/// <param name="index">The starting index.</param>
		public virtual void CopyTo(System.Array array, int index) { _heap.CopyTo(array, index); }

		/// <summary>Determines whether the priority queue is synchronized.</summary>
		public virtual bool IsSynchronized { get { return _heap.IsSynchronized; } }

		/// <summary>Gets the number of items in the queue.</summary>
		public virtual int Count { get { return _heap.Count; } }

		/// <summary>Gets the synchronization root object for the queue.</summary>
		public object SyncRoot { get { return _heap.SyncRoot; } }
		#endregion

		#region Implementation of IEnumerable
		/// <summary>Gets the enumerator for the queue.</summary>
		/// <returns>An enumerator for the queue.</returns>
		public IEnumerator GetEnumerator() { return _heap.GetEnumerator(); }
		#endregion

		#region Synchronization
		/// <summary>Returns a synchronized wrapper around the queue.</summary>
		/// <param name="queue">The queue to be synchronized.</param>
		/// <returns>A synchronized priority queue.</returns>
		public static PriorityQueue Synchronize(PriorityQueue queue)
		{
			// Return the queue if it is already synchronized.  Otherwise, wrap it
			// with a synchronized wrapper.
			if (queue is SyncPriorityQueue) return queue;
			return new SyncPriorityQueue(queue);
		}
		#endregion

		/// <summary>A synchronized PriorityQueue.</summary>
		public class SyncPriorityQueue : PriorityQueue
		{
			#region Construction
			/// <summary>Initialize the priority queue.</summary>
			/// <param name="queue">The queue to be synchronized.</param>
			internal SyncPriorityQueue(PriorityQueue queue)
			{
				// NOTE: We're synchronizing just be using a synchronized heap!
				// This implementation will need to change if we get more state.
				if (!(_heap is BinaryHeap.SyncBinaryHeap)) 
				{
					_heap = BinaryHeap.Synchronize(_heap);
				}
			}
			#endregion
		}
	}
}
