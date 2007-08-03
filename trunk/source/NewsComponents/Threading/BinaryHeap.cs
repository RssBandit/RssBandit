#region CVS Version Header
/*
 * $Id: BinaryHeap.cs,v 1.1 2004/08/20 18:27:10 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2004/08/20 18:27:10 $
 * $Revision: 1.1 $
 */
#endregion

// Stephen Toub
// stoub@microsoft.com
// 
// BinaryHeap.cs
// A C# implementation of a max binary heap.

#region Namespaces
using System;
using System.Collections;
#endregion

namespace NewsComponents.Collections
{
	/// <summary>Collection implemented with the properties of a binary heap.</summary>
	public class BinaryHeap : ICollection, ICloneable
	{
		#region Member Variables
		/// <summary>The underlying array for the heap (ArrayList gives us resizing capability).</summary>
		private ArrayList _list;
		#endregion

		#region Construction
		/// <summary>Initialize the heap with another heap.</summary>
		/// <param name="heap">The heap on which to perform a shallow-copy.</param>
		public BinaryHeap(BinaryHeap heap)
		{
			// Clone the list (the only state we have)
			_list = (ArrayList)heap._list.Clone();
		}

		/// <summary>Initialize the heap.</summary>
		/// <param name="capacity">The initial size of the heap.</param>
		public BinaryHeap(int capacity) { _list = new ArrayList(capacity); }

		/// <summary>Initialize the heap.</summary>
		public BinaryHeap() { _list = new ArrayList(); }
		#endregion

		#region Methods
		/// <summary>Empties the heap.</summary>
		public virtual void Clear() { _list.Clear(); }

		/// <summary>Performs a shallow-copy of the heap.</summary>
		/// <returns>A shallow-copy of the heap.</returns>
		public virtual BinaryHeap Clone() { return new BinaryHeap(this); }

		/// <summary>Determines whether an object is in the heap.</summary>
		/// <param name="value">The object for which we want to search.</param>
		/// <returns>Whether the object is in the heap.</returns>
		public virtual bool Contains(object value)
		{
			foreach(BinaryHeapEntry entry in _list)
			{
				if (entry.Value == value) return true;
			}
			return false;
		}

		/// <summary>Adds an item to the heap.</summary>
		/// <param name="key">The key for this entry.</param>
		/// <param name="value">The value for this entry.</param>
		public virtual void Insert(IComparable key, object value)
		{
			// Create the entry based on the provided key and value
			BinaryHeapEntry entry = new BinaryHeapEntry(key, value);

			// Add the item to the list, making sure to keep track of where it was added.
			int pos = _list.Add(entry); // don't actually need it inserted yet, but want to make sure there's enough space for it

			// If it was added at the beginning, i.e. this is the only item, we're done.
			if (pos == 0) return;

			// Otherwise, perform log(n) operations, walking up the tree, swapping
			// where necessary based on key values
			while(pos > 0)
			{
				// Get the next position to check
				int nextPos = pos / 2;

				// Extract the entry at the next position
				BinaryHeapEntry toCheck = (BinaryHeapEntry)_list[nextPos];

				// Compare that entry to our new one.  If our entry has a larger key, move it up.
				// Otherwise, we're done.
				if (entry.CompareTo(toCheck) > 0) 
				{
					_list[pos] = toCheck;
					pos = nextPos;
				}
				else break;
			}

			// Make sure we put this entry back in, just in case
			_list[pos] = entry;
		}

		/// <summary>Removes the entry at the top of the heap.</summary>
		/// <returns>The removed entry.</returns>
		public virtual object Remove()
		{
			// Get the first item and save it for later (this is what will be returned).
			if (_list.Count == 0) throw new InvalidOperationException("Cannot remove an item from the heap as it is empty.");
			object toReturn = ((BinaryHeapEntry)_list[0]).Value;

			// Remove the first item
			_list.RemoveAt(0);

			// See if we can stop now (if there's only one item or we're empty, we're done)
			if (_list.Count > 1)
			{
				// Move the last element to the beginning
				_list.Insert(0, _list[_list.Count - 1]);
				_list.RemoveAt(_list.Count - 1);

				// Start reheapify
				int current=0, possibleSwap=0;

				// Keep going until the tree is a heap
				while(true) 
				{
					// Get the positions of the node's children
					int leftChildPos = 2*current + 1;
					int rightChildPos = leftChildPos + 1;

					// Should we swap with the left child?
					if (leftChildPos < _list.Count) 
					{
						// Get the two entries to compare (node and its left child)
						BinaryHeapEntry entry1 = (BinaryHeapEntry)_list[current];
						BinaryHeapEntry entry2 = (BinaryHeapEntry)_list[leftChildPos];

						// If the child has a higher key than the parent, set that as a possible swap
						if (entry2.CompareTo(entry1) > 0) possibleSwap = leftChildPos;
					} 
					else break; // if can't swap this, we're done

					// Should we swap with the right child?  Note that now we check with the possible swap
					// position (which might be current and might be left child).
					if (rightChildPos < _list.Count) 
					{
						// Get the two entries to compare (node and its left child)
						BinaryHeapEntry entry1 = (BinaryHeapEntry)_list[possibleSwap];
						BinaryHeapEntry entry2 = (BinaryHeapEntry)_list[rightChildPos];

						// If the child has a higher key than the parent, set that as a possible swap
						if (entry2.CompareTo(entry1) > 0) possibleSwap = rightChildPos;
					}

					// Now swap current and possible swap if necessary
					if (current != possibleSwap) 
					{
						object temp = _list[current];
						_list[current] = _list[possibleSwap];
						_list[possibleSwap] = temp;
					} 
					else break; // if nothing to swap, we're done

					// Update current to the location of the swap
					current = possibleSwap;
				}
			}

			// Return the item from the heap
			return toReturn;
		}
		#endregion

		#region Implementation of ICloneable
		/// <summary>Performs a shallow-copy of the heap.</summary>
		/// <returns>A shallow-copy of the heap.</returns>
		object ICloneable.Clone() { return Clone(); }
		#endregion

		#region Implementation of ICollection
		/// <summary>Copies the entire heap to a compatible one-dimensional array, starting at the given index.</summary>
		/// <param name="array">The array to which the heap should be copied.</param>
		/// <param name="index">The starting index.</param>
		public virtual void CopyTo(System.Array array, int index)
		{
			_list.CopyTo(array, index);
		}

		/// <summary>Gets a value indicating whether this heap is synchronized.</summary>
		public virtual bool IsSynchronized { get { return false; } }

		/// <summary>Gets the number of objects stored in the heap.</summary>
		public virtual int Count { get { return _list.Count; } }

		/// <summary>Gets an object which can be locked in order to synchronize this class.</summary>
		public object SyncRoot { get { return this; } }
		#endregion

		#region Implementation of IEnumerable
		/// <summary>Gets an enumerator for the heap.</summary>
		/// <returns>An enumerator for all elements of the heap.</returns>
		public virtual IEnumerator GetEnumerator() 
		{ 
			return new BinaryHeapEnumerator(_list.GetEnumerator());
		}

		/// <summary>Enumerator for entries in the heap.</summary>
		public class BinaryHeapEnumerator : IEnumerator
		{
			#region Member Variables
			/// <summary>The enumerator of the array list containing BinaryHeapEntry objects.</summary>
			private IEnumerator _enumerator;
			#endregion

			#region Construction
			/// <summary>Initialize the enumerator</summary>
			/// <param name="enumerator">The array list enumerator.</param>
			internal BinaryHeapEnumerator(IEnumerator enumerator)
			{
				_enumerator = enumerator;
			}
			#endregion

			#region Implementation of IEnumerator
			/// <summary>Resets the enumerator.</summary>
			public void Reset() { _enumerator.Reset(); }

			/// <summary>Moves to the next item in the list.</summary>
			/// <returns>Whether there are more items in the list.</returns>
			public bool MoveNext() { return _enumerator.MoveNext(); }

			/// <summary>Gets the current object in the list.</summary>
			public object Current
			{
				get
				{
					// Returns the value from the entry if it exists; otherwise, null.
					BinaryHeapEntry entry = _enumerator.Current as BinaryHeapEntry;
					return entry != null ? entry.Value : null;
				}
			}
			#endregion
		}
		#endregion

		#region Synchronization
		/// <summary>Ensures that heap is wrapped in a synchronous wrapper.</summary>
		/// <param name="heap">The heap to be wrapped.</param>
		/// <returns>A synchronized wrapper for the heap.</returns>
		public static BinaryHeap Synchronize(BinaryHeap heap)
		{
			// Create a synchronization wrapper around the heap and return it.
			if (heap is SyncBinaryHeap) return heap;
			return new SyncBinaryHeap(heap);
		}

		#endregion

		/// <summary>Represents an entry in a binary heap.</summary>
		private class BinaryHeapEntry : IComparable, ICloneable
		{
			#region Member Variables
			/// <summary>The key for this entry.</summary>
			private IComparable _key;
			/// <summary>The value for this entry.</summary>
			private object _value;
			#endregion

			#region Construction
			/// <summary>Initializes an entry to be used in a binary heap.</summary>
			/// <param name="key">The key for this entry.</param>
			/// <param name="value">The value for this entry.</param>
			public BinaryHeapEntry(IComparable key, object value)
			{
				_key = key;
				_value = value;
			}
			#endregion

			#region Properties
			/// <summary>Gets the key for this entry.</summary>
			public IComparable Key { get { return _key; } set { _key = value; } }
			/// <summary>Gets the value for this entry.</summary>
			public object Value { get { return _value; } set { _value = value; } }
			#endregion

			#region Implementation of IComparable
			/// <summary>Compares the current instance with another object of the same type.</summary>
			/// <param name="entry">An object to compare with this instance.</param>
			/// <returns>
			/// Less than 0 if this instance is less than the argument,
			/// 0 if the instances are equal,
			/// Greater than 0 if this instance is greater than the argument.
			/// </returns>
			public int CompareTo(BinaryHeapEntry entry)
			{
				// Make sure we have valid arguments.
				if (entry == null) throw new ArgumentNullException("entry", "Cannot compare to a null value.");

				// Compare the keys
				return _key.CompareTo(entry.Key);
			}

			/// <summary>Compares the current instance with another object of the same type.</summary>
			/// <param name="obj">An object to compare with this instance.</param>
			/// <returns>
			/// Less than 0 if this instance is less than the argument,
			/// 0 if the instances are equal,
			/// Greater than 0 if this instance is greater than the argument.
			/// </returns>
			int IComparable.CompareTo(object obj) 
			{ 
				// Make sure we have valid arguments, then compare.
				if (!(obj is BinaryHeapEntry)) throw new ArgumentException("Object is not a BinaryHeapEntry", "obj");
				return CompareTo((BinaryHeapEntry)obj);
			}
			#endregion

			#region Implementation of ICloneable
			/// <summary>Shallow-copy of the object.</summary>
			/// <returns>A shallow-copy of the object.</returns>
			public BinaryHeapEntry Clone()
			{
				return new BinaryHeapEntry(_key, _value);
			}

			/// <summary>Shallow-copy of the object.</summary>
			/// <returns>A shallow-copy of the object.</returns>
			object ICloneable.Clone()
			{
				return Clone();
			}
			#endregion
		}

		/// <summary>A synchronized BinaryHeap.</summary>
		public class SyncBinaryHeap : BinaryHeap
		{
			#region Member Variables
			/// <summary>The heap to synchronize.</summary>
			private BinaryHeap _heap;
			#endregion

			#region Construction
			/// <summary>Initialize the synchronized heap.</summary>
			/// <param name="heap">The heap to synchronize.</param>
			internal SyncBinaryHeap(BinaryHeap heap) { _heap = heap; }
			#endregion

			#region Methods
			/// <summary>Performs a shallow-copy of the heap.</summary>
			/// <returns>A shallow-copy of the heap.</returns>
			public override BinaryHeap Clone() 
			{ 
				lock(_heap.SyncRoot) return _heap.Clone(); 
			}

			/// <summary>Empties the heap.</summary>
			public override void Clear() 
			{ 
				lock(_heap.SyncRoot) _heap.Clear(); 
			}

			/// <summary>Determines whether an object is in the heap.</summary>
			/// <param name="value">The object for which we want to search.</param>
			/// <returns>Whether the object is in the heap.</returns>
			public override bool Contains(object value) 
			{ 
				lock(_heap.SyncRoot) return _heap.Contains(value); 
			}

			/// <summary>Adds an item to the heap.</summary>
			/// <param name="key">The key for this entry.</param>
			/// <param name="value">The value for this entry.</param>
			public override void Insert(IComparable key, object value)
			{
				lock(_heap.SyncRoot) _heap.Insert(key, value);
			}

			/// <summary>Removes the entry at the top of the heap.</summary>
			/// <returns>The removed entry.</returns>
			public override object Remove()
			{
				lock(_heap.SyncRoot) return _heap.Remove();
			}

			/// <summary>Copies the entire heap to a compatible one-dimensional array, starting at the given index.</summary>
			/// <param name="array">The array to which the heap should be copied.</param>
			/// <param name="index">The starting index.</param>
			public override void CopyTo(System.Array array, int index)
			{
				lock(_heap.SyncRoot) _heap.CopyTo(array, index);
			}

			/// <summary>Gets a value indicating whether this heap is synchronized.</summary>
			public override bool IsSynchronized 
			{ 
				get { return true; } 
			}

			/// <summary>Gets the number of objects stored in the heap.</summary>
			public override int Count 
			{ 
				get { lock(_heap.SyncRoot) return _heap.Count; }
			}
			/// <summary>Gets an enumerator for the heap.</summary>
			/// <returns>An enumerator for all elements of the heap.</returns>
			public override IEnumerator GetEnumerator() 
			{ 
				lock(_heap.SyncRoot) return _heap.GetEnumerator(); 
			}
			#endregion
		}
	}
}
