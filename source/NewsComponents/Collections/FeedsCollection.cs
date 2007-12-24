#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using NewsComponents.Feed;


namespace NewsComponents.Collections {


    #region EmptyList<T>
    /// <summary>
    /// Generic class to return an empty List instance
    /// </summary>
    public sealed class GetList<T> : List<T> {
        private static List<T> _empty = new List<T>(0);
        /// <summary>
        /// Gets the empty/readonly single List instance.
        /// </summary>
        public static List<T> Empty { get { return _empty; } }
    }


    #endregion

    #region EmptyArrayList
    /// <summary>
	/// Helper class to return a empty ArrayList instance
	/// </summary>
	public sealed class GetArrayList:ArrayList
	{
		private static ArrayList _empty = ArrayList.ReadOnly(new ArrayList(0));
		/// <summary>
		/// Gets the empty/readonly single ArrayList instance.
		/// </summary>
		public static ArrayList Empty { get { return _empty; } }	
	}
	#endregion

	#region Interface IfeedsFeedCollection

	/// <summary>
	/// Defines size, enumerators, and synchronization methods for strongly
	/// typed collections of <see cref="feedsFeed"/> elements.
	/// </summary>
	/// <remarks>
	/// <b>IfeedsFeedCollection</b> provides an <see cref="ICollection"/>
	/// that is strongly typed for <see cref="feedsFeed"/> elements.
	/// </remarks>

	public interface IfeedsFeedCollection {
		#region Properties
		#region Count

		/// <summary>
		/// Gets the number of elements contained in the
		/// <see cref="IfeedsFeedCollection"/>.
		/// </summary>
		/// <value>The number of elements contained in the
		/// <see cref="IfeedsFeedCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ICollection.Count"/> for details.</remarks>

		int Count { get; }

		#endregion
		#region IsSynchronized

		/// <summary>
		/// Gets a value indicating whether access to the
		/// <see cref="IfeedsFeedCollection"/> is synchronized (thread-safe).
		/// </summary>
		/// <value><c>true</c> if access to the <see cref="IfeedsFeedCollection"/> is
		/// synchronized (thread-safe); otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="ICollection.IsSynchronized"/> for details.</remarks>

		bool IsSynchronized { get; }

		#endregion
		#region SyncRoot

		/// <summary>
		/// Gets an object that can be used to synchronize access
		/// to the <see cref="IfeedsFeedCollection"/>.
		/// </summary>
		/// <value>An object that can be used to synchronize access
		/// to the <see cref="IfeedsFeedCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ICollection.SyncRoot"/> for details.</remarks>

		object SyncRoot { get; }

		#endregion
		#endregion
		#region Methods
		#region CopyTo

		/// <summary>
		/// Copies the entire <see cref="IfeedsFeedCollection"/> to a one-dimensional <see cref="Array"/>
		/// of <see cref="feedsFeed"/> elements, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
		/// <see cref="feedsFeed"/> elements copied from the <see cref="IfeedsFeedCollection"/>.
		/// The <b>Array</b> must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/>
		/// at which copying begins.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is a null reference.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is less than zero.</exception>
		/// <exception cref="ArgumentException"><para>
		/// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
		/// </para><para>-or-</para><para>
		/// The number of elements in the source <see cref="IfeedsFeedCollection"/> is greater
		/// than the available space from <paramref name="arrayIndex"/> to the end of the destination
		/// <paramref name="array"/>.</para></exception>
		/// <remarks>Please refer to <see cref="ICollection.CopyTo"/> for details.</remarks>

		void CopyTo(feedsFeed[] array, int arrayIndex);

		#endregion
		#region GetEnumerator

		/// <summary>
		/// Returns an <see cref="IfeedsFeedEnumerator"/> that can
		/// iterate through the <see cref="IfeedsFeedCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IfeedsFeedEnumerator"/>
		/// for the entire <see cref="IfeedsFeedCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="IEnumerable.GetEnumerator"/> for details.</remarks>

		IfeedsFeedEnumerator GetEnumerator();

		#endregion
		#endregion
	}

	#endregion
	
	#region Interface IfeedsFeedList

	/// <summary>
	/// Represents a strongly typed collection of <see cref="feedsFeed"/>
	/// objects that can be individually accessed by index.
	/// </summary>
	/// <remarks>
	/// <b>IfeedsFeedList</b> provides an <see cref="IList"/>
	/// that is strongly typed for <see cref="feedsFeed"/> elements.
	/// </remarks>

	public interface
		IfeedsFeedList: IfeedsFeedCollection {
		#region Properties
		#region IsFixedSize

		/// <summary>
		/// Gets a value indicating whether the <see cref="IfeedsFeedList"/> has a fixed size.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IfeedsFeedList"/> has a fixed size;
		/// otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IList.IsFixedSize"/> for details.</remarks>

		bool IsFixedSize { get; }

		#endregion
		#region IsReadOnly

		/// <summary>
		/// Gets a value indicating whether the <see cref="IfeedsFeedList"/> is read-only.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IfeedsFeedList"/> is read-only;
		/// otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IList.IsReadOnly"/> for details.</remarks>

		bool IsReadOnly { get; }

		#endregion
		#region Item

		/// <summary>
		/// Gets or sets the <see cref="feedsFeed"/> element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="feedsFeed"/> element to get or set.</param>
		/// <value>
		/// The <see cref="feedsFeed"/> element at the specified <paramref name="index"/>.
		/// </value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than
		/// <see cref="IfeedsFeedCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The property is set and the <see cref="IfeedsFeedList"/> is read-only.</exception>
		/// <remarks>Please refer to <see cref="IList.this"/> for details.</remarks>

		feedsFeed this[int index] { get; set; }

		#endregion
		#endregion
		#region Methods
		#region Add

		/// <summary>
		/// Adds a <see cref="feedsFeed"/> to the end
		/// of the <see cref="IfeedsFeedList"/>.
		/// </summary>
		/// <param name="value">The <see cref="feedsFeed"/> object
		/// to be added to the end of the <see cref="IfeedsFeedList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns>The <see cref="IfeedsFeedList"/> index at which
		/// the <paramref name="value"/> has been added.</returns>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IfeedsFeedList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IfeedsFeedList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Add"/> for details.</remarks>

		int Add(feedsFeed value);

		#endregion
		#region Clear

		/// <summary>
		/// Removes all elements from the <see cref="IfeedsFeedList"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IfeedsFeedList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IfeedsFeedList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Clear"/> for details.</remarks>

		void Clear();

		#endregion
		#region Contains

		/// <summary>
		/// Determines whether the <see cref="IfeedsFeedList"/>
		/// contains the specified <see cref="feedsFeed"/> element.
		/// </summary>
		/// <param name="value">The <see cref="feedsFeed"/> object
		/// to locate in the <see cref="IfeedsFeedList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns><c>true</c> if <paramref name="value"/> is found in the
		/// <see cref="IfeedsFeedList"/>; otherwise, <c>false</c>.</returns>
		/// <remarks>Please refer to <see cref="IList.Contains"/> for details.</remarks>

		bool Contains(feedsFeed value);

		#endregion
		#region IndexOf

		/// <summary>
		/// Returns the zero-based index of the first occurrence of the specified
		/// <see cref="feedsFeed"/> in the <see cref="IfeedsFeedList"/>.
		/// </summary>
		/// <param name="value">The <see cref="feedsFeed"/> object
		/// to locate in the <see cref="IfeedsFeedList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="value"/>
		/// in the <see cref="IfeedsFeedList"/>, if found; otherwise, -1.
		/// </returns>
		/// <remarks>Please refer to <see cref="IList.IndexOf"/> for details.</remarks>

		int IndexOf(feedsFeed value);

		#endregion
		#region Insert

		/// <summary>
		/// Inserts a <see cref="feedsFeed"/> element into the
		/// <see cref="IfeedsFeedList"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which
		/// <paramref name="value"/> should be inserted.</param>
		/// <param name="value">The <see cref="feedsFeed"/> object
		/// to insert into the <see cref="IfeedsFeedList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is greater than
		/// <see cref="IfeedsFeedCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IfeedsFeedList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IfeedsFeedList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Insert"/> for details.</remarks>

		void Insert(int index, feedsFeed value);

		#endregion
		#region Remove

		/// <summary>
		/// Removes the first occurrence of the specified <see cref="feedsFeed"/>
		/// from the <see cref="IfeedsFeedList"/>.
		/// </summary>
		/// <param name="value">The <see cref="feedsFeed"/> object
		/// to remove from the <see cref="IfeedsFeedList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IfeedsFeedList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IfeedsFeedList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Remove"/> for details.</remarks>

		void Remove(feedsFeed value);

		#endregion
		#region RemoveAt

		/// <summary>
		/// Removes the element at the specified index of the
		/// <see cref="IfeedsFeedList"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than
		/// <see cref="IfeedsFeedCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IfeedsFeedList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IfeedsFeedList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.RemoveAt"/> for details.</remarks>

		void RemoveAt(int index);

		#endregion
		#endregion
	}

	#endregion

	#region Interface IfeedsFeedEnumerator

	/// <summary>
	/// Supports type-safe iteration over a collection that
	/// contains <see cref="feedsFeed"/> elements.
	/// </summary>
	/// <remarks>
	/// <b>IfeedsFeedEnumerator</b> provides an <see cref="IEnumerator"/>
	/// that is strongly typed for <see cref="feedsFeed"/> elements.
	/// </remarks>

	public interface IfeedsFeedEnumerator {
		#region Properties
		#region Current

		/// <summary>
		/// Gets the current <see cref="feedsFeed"/> element in the collection.
		/// </summary>
		/// <value>The current <see cref="feedsFeed"/> element in the collection.</value>
		/// <exception cref="InvalidOperationException"><para>The enumerator is positioned
		/// before the first element of the collection or after the last element.</para>
		/// <para>-or-</para>
		/// <para>The collection was modified after the enumerator was created.</para></exception>
		/// <remarks>Please refer to <see cref="IEnumerator.Current"/> for details, but note
		/// that <b>Current</b> fails if the collection was modified since the last successful
		/// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>

		feedsFeed Current { get; }

		#endregion
		#endregion
		#region Methods
		#region MoveNext

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns><c>true</c> if the enumerator was successfully advanced to the next element;
		/// <c>false</c> if the enumerator has passed the end of the collection.</returns>
		/// <exception cref="InvalidOperationException">
		/// The collection was modified after the enumerator was created.</exception>
		/// <remarks>Please refer to <see cref="IEnumerator.MoveNext"/> for details.</remarks>

		bool MoveNext();

		#endregion
		#region Reset

		/// <summary>
		/// Sets the enumerator to its initial position,
		/// which is before the first element in the collection.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// The collection was modified after the enumerator was created.</exception>
		/// <remarks>Please refer to <see cref="IEnumerator.Reset"/> for details.</remarks>

		void Reset();

		#endregion
		#endregion
	}

	#endregion

	#region Interface IStringfeedsFeedCollection

	/// <summary>
	/// Defines size, enumerators, and synchronization methods for strongly
	/// typed collections of <see cref="FeedEntry"/> elements.
	/// </summary>
	/// <remarks>
	/// <b>IStringfeedsFeedCollection</b> provides an <see cref="ICollection"/>
	/// that is strongly typed for <see cref="FeedEntry"/> elements.
	/// </remarks>

	public interface IStringfeedsFeedCollection {
		#region Properties
		#region Count

		/// <summary>
		/// Gets the number of elements contained in the
		/// <see cref="IStringfeedsFeedCollection"/>.
		/// </summary>
		/// <value>The number of elements contained in the
		/// <see cref="IStringfeedsFeedCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ICollection.Count"/> for details.</remarks>

		int Count { get; }

		#endregion
		#region IsSynchronized

		/// <summary>
		/// Gets a value indicating whether access to the
		/// <see cref="IStringfeedsFeedCollection"/> is synchronized (thread-safe).
		/// </summary>
		/// <value><c>true</c> if access to the <see cref="IStringfeedsFeedCollection"/>
		/// is synchronized (thread-safe); otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="ICollection.IsSynchronized"/> for details.</remarks>

		bool IsSynchronized { get; }

		#endregion
		#region SyncRoot

		/// <summary>
		/// Gets an object that can be used to synchronize access
		/// to the <see cref="IStringfeedsFeedCollection"/>.
		/// </summary>
		/// <value>An object that can be used to synchronize access to the
		/// <see cref="IStringfeedsFeedCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ICollection.SyncRoot"/> for details.</remarks>

		object SyncRoot { get; }

		#endregion
		#endregion
		#region Methods
		#region CopyTo

		/// <summary>
		/// Copies the entire <see cref="IStringfeedsFeedCollection"/>
		/// to a one-dimensional <see cref="Array"/> of <see cref="FeedEntry"/> elements,
		/// starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the
		/// destination of the <see cref="FeedEntry"/> elements copied from the
		/// <see cref="IStringfeedsFeedCollection"/>.
		/// The <b>Array</b> must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/>
		/// at which copying begins.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is a null reference.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is less than zero.</exception>
		/// <exception cref="ArgumentException"><para>
		/// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
		/// </para><para>-or-</para><para>
		/// The number of elements in the source <see cref="IStringfeedsFeedCollection"/>
		/// is greater than the available space from <paramref name="arrayIndex"/> to the end of the
		/// destination <paramref name="array"/>.</para></exception>
		/// <remarks>Please refer to <see cref="ICollection.CopyTo"/> for details.</remarks>

		void CopyTo(FeedEntry[] array, int arrayIndex);

		#endregion
		#region GetEnumerator

		/// <summary>
		/// Returns an <see cref="IStringfeedsFeedEnumerator"/> that can
		/// iterate through the <see cref="IStringfeedsFeedCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IStringfeedsFeedEnumerator"/>
		/// for the entire <see cref="IStringfeedsFeedCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="IEnumerable.GetEnumerator"/> for details.</remarks>

		IStringfeedsFeedEnumerator GetEnumerator();

		#endregion
		#endregion
	}

	#endregion

	#region Interface IStringfeedsFeedDictionary

	/// <summary>
	/// Represents a strongly typed collection of
	/// <see cref="FeedEntry"/> key-and-value pairs.
	/// </summary>
	/// <remarks>
	/// <b>IStringfeedsFeedDictionary</b> provides an
	/// <see cref="IDictionary"/> that is strongly typed for
	/// <see cref="String"/> keys and <see cref="feedsFeed"/> values.
	/// </remarks>

	public interface
		IStringfeedsFeedDictionary: IStringfeedsFeedCollection {
		#region Properties
		#region IsFixedSize

		/// <summary>
		/// Gets a value indicating whether the
		/// <see cref="IStringfeedsFeedDictionary"/> has a fixed size.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IStringfeedsFeedDictionary"/>
		/// has a fixed size; otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IDictionary.IsFixedSize"/> for details.</remarks>

		bool IsFixedSize { get; }

		#endregion
		#region IsReadOnly

		/// <summary>
		/// Gets a value indicating whether the
		/// <see cref="IStringfeedsFeedDictionary"/> is read-only.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IStringfeedsFeedDictionary"/>
		/// is read-only; otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IDictionary.IsReadOnly"/> for details.</remarks>

		bool IsReadOnly { get; }

		#endregion
		#region Item

		/// <summary>
		/// Gets or sets the <see cref="feedsFeed"/> value
		/// associated with the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key
		/// whose value to get or set.</param>
		/// <value>The <see cref="feedsFeed"/> value associated with the specified
		/// <paramref name="key"/>. If the specified <paramref name="key"/> is not found,
		/// attempting to get it returns
		/// a null reference,
		/// and attempting to set it creates a new element using the specified
		/// <paramref name="key"/>.</value>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The property is set and the
		/// <see cref="IStringfeedsFeedDictionary"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The property is set, <paramref name="key"/> does not exist in the collection,
		/// and the <b>IStringfeedsFeedDictionary</b> has a fixed size.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="IDictionary.this"/> for details.</remarks>

		feedsFeed this[String key] { get; set; }

		#endregion
		#region Keys

		/// <summary>
		/// Gets an <see cref="IStringCollection"/> containing the keys
		/// in the <see cref="IStringfeedsFeedDictionary"/>.
		/// </summary>
		/// <value>An <see cref="IStringCollection"/> containing the keys
		/// in the <see cref="IStringfeedsFeedDictionary"/>.</value>
		/// <remarks>Please refer to <see cref="IDictionary.Keys"/> for details.</remarks>

		ICollection<string> Keys { get; }

		#endregion
		#region Values

		/// <summary>
		/// Gets an <see cref="IfeedsFeedCollection"/> containing the values
		/// in the <see cref="IStringfeedsFeedDictionary"/>.
		/// </summary>
		/// <value>An <see cref="IfeedsFeedCollection"/> containing the values
		/// in the <see cref="IStringfeedsFeedDictionary"/>.</value>
		/// <remarks>Please refer to <see cref="IDictionary.Values"/> for details.</remarks>

		IfeedsFeedCollection Values { get; }

		#endregion
		#endregion
		#region Methods
		#region Add

		/// <summary>
		/// Adds an element with the specified <see cref="String"/>
		/// key and <see cref="feedsFeed"/> value to the
		/// <see cref="IStringfeedsFeedDictionary"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key of the element
		/// to add to the <see cref="IStringfeedsFeedDictionary"/>.</param>
		/// <param name="value">The <see cref="feedsFeed"/> value of the element
		/// to add to the <see cref="IStringfeedsFeedDictionary"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentException">
		/// <para>An element with the specified <paramref name="key"/> already exists
		/// in the <see cref="IStringfeedsFeedDictionary"/>.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringfeedsFeedDictionary</b> is set to use the
		/// <see cref="IComparable"/> interface, and <paramref name="key"/> does not
		/// implement the <b>IComparable</b> interface.</para></exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringfeedsFeedDictionary"/> is read-only.
		/// </para><para>-or-</para>
		/// <para>The <b>IStringfeedsFeedDictionary</b> has a fixed size.
		/// </para></exception>
		/// <remarks>Please refer to <see cref="IDictionary.Add"/> for details.</remarks>

		void Add(String key, feedsFeed value);

		#endregion
		#region Clear

		/// <summary>
		/// Removes all elements from the <see cref="IStringfeedsFeedDictionary"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringfeedsFeedDictionary"/> is read-only.
		/// </para><para>-or-</para>
		/// <para>The <b>IStringfeedsFeedDictionary</b> has a fixed size.
		/// </para></exception>
		/// <remarks>Please refer to <see cref="IDictionary.Clear"/> for details.</remarks>

		void Clear();

		#endregion
		#region Contains

		/// <summary>
		/// Determines whether the <see cref="IStringfeedsFeedDictionary"/>
		/// contains the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key to locate
		/// in the <see cref="IStringfeedsFeedDictionary"/>.</param>
		/// <returns><c>true</c> if the <see cref="IStringfeedsFeedDictionary"/>
		/// contains an element with the specified <paramref name="key"/>; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <remarks>Please refer to <see cref="IDictionary.Contains"/> for details.</remarks>

		bool Contains(String key);

		#endregion
		#region Remove

		/// <summary>
		/// Removes the element with the specified <see cref="String"/> key
		/// from the <see cref="IStringfeedsFeedDictionary"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key of the element to remove
		/// from the <see cref="IStringfeedsFeedDictionary"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringfeedsFeedDictionary"/> is read-only.
		/// </para><para>-or-</para>
		/// <para>The <b>IStringfeedsFeedDictionary</b> has a fixed size.
		/// </para></exception>
		/// <remarks>Please refer to <see cref="IDictionary.Remove"/> for details.</remarks>

		void Remove(String key);

		#endregion
		#endregion
	}

	#endregion

	#region Interface IStringfeedsFeedList

	/// <summary>
	/// Represents a strongly typed collection of <see cref="FeedEntry"/>
	/// objects that can be individually accessed by index.
	/// </summary>
	/// <remarks>
	/// <b>IStringfeedsFeedList</b> provides an <see cref="IList"/>
	/// that is strongly typed for <see cref="FeedEntry"/> elements.
	/// </remarks>

	public interface
		IStringfeedsFeedList: IStringfeedsFeedCollection {
		#region Properties
		#region IsFixedSize

		/// <summary>
		/// Gets a value indicating whether the
		/// <see cref="IStringfeedsFeedList"/> has a fixed size.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IStringfeedsFeedList"/>
		/// has a fixed size; otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IList.IsFixedSize"/> for details.</remarks>

		bool IsFixedSize { get; }

		#endregion
		#region IsReadOnly

		/// <summary>
		/// Gets a value indicating whether the
		/// <see cref="IStringfeedsFeedList"/> is read-only.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IStringfeedsFeedList"/>
		/// is read-only; otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IList.IsReadOnly"/> for details.</remarks>

		bool IsReadOnly { get; }

		#endregion
		#region Item

		/// <summary>
		/// Gets or sets the <see cref="FeedEntry"/> element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="FeedEntry"/> element to get or set.</param>
		/// <value>
		/// The <see cref="FeedEntry"/> element at the specified <paramref name="index"/>.
		/// </value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than
		/// <see cref="IStringfeedsFeedCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">The property is set and the
		/// <see cref="IStringfeedsFeedList"/> is read-only.</exception>
		/// <remarks>Please refer to <see cref="IList.this"/> for details.</remarks>

		FeedEntry this[int index] { get; set; }

		#endregion
		#endregion
		#region Methods
		#region Add

		/// <summary>
		/// Adds a <see cref="FeedEntry"/> to the end
		/// of the <see cref="IStringfeedsFeedList"/>.
		/// </summary>
		/// <param name="entry">The <see cref="FeedEntry"/> object
		/// to be added to the end of the <see cref="IStringfeedsFeedList"/>.
		/// </param>
		/// <returns>The <see cref="IStringfeedsFeedList"/> index at which
		/// the <paramref name="entry"/> has been added.</returns>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringfeedsFeedList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringfeedsFeedList</b> has a fixed size.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="IList.Add"/> for details.</remarks>

		int Add(FeedEntry entry);

		#endregion
		#region Clear

		/// <summary>
		/// Removes all elements from the <see cref="IStringfeedsFeedList"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringfeedsFeedList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringfeedsFeedList</b> has a fixed size.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="IList.Clear"/> for details.</remarks>

		void Clear();

		#endregion
		#region Contains

		/// <summary>
		/// Determines whether the <see cref="IStringfeedsFeedList"/>
		/// contains the specified <see cref="FeedEntry"/> element.
		/// </summary>
		/// <param name="entry">The <see cref="FeedEntry"/> object
		/// to locate in the <see cref="IStringfeedsFeedList"/>.</param>
		/// <returns><c>true</c> if <paramref name="entry"/> is found in the
		/// <see cref="IStringfeedsFeedList"/>; otherwise, <c>false</c>.</returns>
		/// <remarks>Please refer to <see cref="IList.Contains"/> for details.</remarks>

		bool Contains(FeedEntry entry);

		#endregion
		#region IndexOf

		/// <summary>
		/// Returns the zero-based index of the first occurrence of the specified
		/// <see cref="FeedEntry"/> in the <see cref="IStringfeedsFeedList"/>.
		/// </summary>
		/// <param name="entry">The <see cref="FeedEntry"/> object
		/// to locate in the <see cref="IStringfeedsFeedList"/>.</param>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="entry"/>
		/// in the <see cref="IStringfeedsFeedList"/>, if found; otherwise, -1.
		/// </returns>
		/// <remarks>Please refer to <see cref="IList.IndexOf"/> for details.</remarks>

		int IndexOf(FeedEntry entry);

		#endregion
		#region Insert

		/// <summary>
		/// Inserts a <see cref="FeedEntry"/> element into the
		/// <see cref="IStringfeedsFeedList"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which
		/// <paramref name="entry"/> should be inserted.</param>
		/// <param name="entry">The <see cref="FeedEntry"/> object to insert
		/// into the <see cref="IStringfeedsFeedList"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is greater than
		/// <see cref="IStringfeedsFeedCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringfeedsFeedList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringfeedsFeedList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Insert"/> for details.</remarks>

		void Insert(int index, FeedEntry entry);

		#endregion
		#region Remove

		/// <summary>
		/// Removes the first occurrence of the specified <see cref="FeedEntry"/>
		/// from the <see cref="IStringfeedsFeedList"/>.
		/// </summary>
		/// <param name="entry">The <see cref="FeedEntry"/> object to remove
		/// from the <see cref="IStringfeedsFeedList"/>.</param>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringfeedsFeedList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringfeedsFeedList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Remove"/> for details.</remarks>

		void Remove(FeedEntry entry);

		#endregion
		#region RemoveAt

		/// <summary>
		/// Removes the element at the specified index of the
		/// <see cref="IStringfeedsFeedList"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than
		/// <see cref="IStringfeedsFeedCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringfeedsFeedList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringfeedsFeedList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.RemoveAt"/> for details.</remarks>

		void RemoveAt(int index);

		#endregion
		#endregion
	}

	#endregion

	#region Interface IStringfeedsFeedEnumerator

	/// <summary>
	/// Supports type-safe iteration over a dictionary that
	/// contains <see cref="FeedEntry"/> elements.
	/// </summary>
	/// <remarks>
	/// <b>IStringfeedsFeedEnumerator</b> provides an
	/// <see cref="IDictionaryEnumerator"/> that is strongly typed for
	/// <see cref="String"/> keys and <see cref="feedsFeed"/> values.
	/// </remarks>

	public interface IStringfeedsFeedEnumerator {
		#region Properties
		#region Current

		/// <summary>
		/// Gets the current <see cref="FeedEntry"/> element in the collection.
		/// </summary>
		/// <value>The current <see cref="FeedEntry"/> element in the collection.</value>
		/// <exception cref="InvalidOperationException"><para>The enumerator is positioned
		/// before the first element of the collection or after the last element.</para>
		/// <para>-or-</para>
		/// <para>The collection was modified after the enumerator was created.</para></exception>
		/// <remarks>Please refer to <see cref="IEnumerator.Current"/> for details, but note
		/// that <b>Current</b> fails if the collection was modified since the last successful
		/// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>

		FeedEntry Current { get; }

		#endregion
		#region Entry

		/// <summary>
		/// Gets a <see cref="FeedEntry"/> containing both
		/// the key and the value of the current dictionary entry.
		/// </summary>
		/// <value>A <see cref="FeedEntry"/> containing both
		/// the key and the value of the current dictionary entry.</value>
		/// <exception cref="InvalidOperationException"><para>The enumerator is positioned
		/// before the first element of the dictionary or after the last element.</para>
		/// <para>-or-</para>
		/// <para>The dictionary was modified after the enumerator was created.</para></exception>
		/// <remarks>Please refer to <see cref="IDictionaryEnumerator.Entry"/> for details, but
		/// note that <b>Entry</b> fails if the collection was modified since the last successful
		/// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>

		FeedEntry Entry { get; }

		#endregion
		#region Key

		/// <summary>
		/// Gets the <see cref="String"/> key of the current dictionary entry.
		/// </summary>
		/// <value>The <see cref="String"/> key
		/// of the current element of the enumeration.</value>
		/// <exception cref="InvalidOperationException"><para>The enumerator is positioned
		/// before the first element of the dictionary or after the last element.</para>
		/// <para>-or-</para>
		/// <para>The dictionary was modified after the enumerator was created.</para></exception>
		/// <remarks>Please refer to <see cref="IDictionaryEnumerator.Key"/> for details, but
		/// note that <b>Key</b> fails if the collection was modified since the last successful
		/// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>

		String Key { get; }

		#endregion
		#region Value

		/// <summary>
		/// Gets the <see cref="feedsFeed"/> value of the current dictionary entry.
		/// </summary>
		/// <value>The <see cref="feedsFeed"/> value
		/// of the current element of the enumeration.</value>
		/// <exception cref="InvalidOperationException"><para>The enumerator is positioned
		/// before the first element of the dictionary or after the last element.</para>
		/// <para>-or-</para>
		/// <para>The dictionary was modified after the enumerator was created.</para></exception>
		/// <remarks>Please refer to <see cref="IDictionaryEnumerator.Value"/> for details, but
		/// note that <b>Value</b> fails if the collection was modified since the last successful
		/// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>

		feedsFeed Value { get; }

		#endregion
		#endregion
		#region Methods
		#region MoveNext

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns><c>true</c> if the enumerator was successfully advanced to the next element;
		/// <c>false</c> if the enumerator has passed the end of the collection.</returns>
		/// <exception cref="InvalidOperationException">
		/// The collection was modified after the enumerator was created.</exception>
		/// <remarks>Please refer to <see cref="IEnumerator.MoveNext"/> for details.</remarks>

		bool MoveNext();

		#endregion
		#region Reset

		/// <summary>
		/// Sets the enumerator to its initial position,
		/// which is before the first element in the collection.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// The collection was modified after the enumerator was created.</exception>
		/// <remarks>Please refer to <see cref="IEnumerator.Reset"/> for details.</remarks>

		void Reset();

		#endregion
		#endregion
	}

	#endregion

	#region Struct FeedEntry

	/// <summary>
	/// Implements a strongly typed pair of one <see cref="String"/>
	/// key and one <see cref="feedsFeed"/> value.
	/// </summary>
	/// <remarks>
	/// <b>FeedEntry</b> provides a <see cref="DictionaryEntry"/> that is strongly
	/// typed for <see cref="String"/> keys and <see cref="feedsFeed"/> values.
	/// </remarks>

	[Serializable]
	public struct FeedEntry {
		#region Private Fields

		private String _key;
		private feedsFeed _value;

		#endregion
		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedEntry"/>
		/// class with the specified key and value.
		/// </summary>
		/// <param name="key">
		/// The <see cref="String"/> key in the key-and-value pair.</param>
		/// <param name="value">
		/// The <see cref="feedsFeed"/> value in the key-and-value pair.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>

		public FeedEntry(String key, feedsFeed value) {
			if ((object) key == null)
				throw new ArgumentNullException("key");

			this._key = key;
			this._value = value;
		}

		#endregion
		#region Public Properties
		#region Key

		/// <summary>
		/// Gets or sets the <see cref="String"/> key in the key-and-value pair.
		/// </summary>
		/// <value>
		/// The <see cref="String"/> key in the key-and-value pair.
		/// The default is a null reference.
		/// </value>
		/// <exception cref="ArgumentNullException">
		/// <b>Key</b> is set to a null reference.</exception>
		/// <remarks>
		/// <see cref="FeedEntry"/> is a value type and therefore has an implicit default
		/// constructor that zeroes all data members. This means that the <b>Key</b> property of
		/// a default-constructed <b>FeedEntry</b> contains a null reference by default,
		/// even though it is not possible to explicitly set <b>Key</b> to a null reference.
		/// </remarks>

		public String Key {
			get { return this._key; }
			set {
				if ((object) value == null)
					throw new ArgumentNullException("value");
				this._key = value;
			}
		}

		#endregion
		#region Value

		/// <summary>
		/// Gets or sets the <see cref="feedsFeed"/> value in the key-and-value pair.
		/// </summary>
		/// <value>
		/// The <see cref="feedsFeed"/> value in the key-and-value pair.
		/// This value can be a null reference, which is also the default.
		/// </value>

		public feedsFeed Value {
			get { return this._value; }
			set { this._value = value; }
		}

		#endregion
		#endregion
		#region Public Operators
		#region FeedEntry(DictionaryEntry)

		/// <summary>
		/// Converts a <see cref="DictionaryEntry"/> to a <see cref="FeedEntry"/>.
		/// </summary>
		/// <param name="entry">A <see cref="DictionaryEntry"/> object to convert.</param>
		/// <returns>A <see cref="FeedEntry"/> object that represents
		/// the converted <paramref name="entry"/>.</returns>
		/// <exception cref="InvalidCastException">
		/// <para><paramref name="entry"/> contains a key that is not compatible
		/// with <see cref="String"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entry"/> contains a value that is not compatible
		/// with <see cref="feedsFeed"/>.</para>
		/// </exception>

		public static implicit operator FeedEntry(DictionaryEntry entry) {
			FeedEntry pair = new FeedEntry();
			if (entry.Key != null) pair.Key = (String) entry.Key;
			if (entry.Value != null) pair.Value = (feedsFeed) entry.Value;
			return pair;
		}

		#endregion
		#region DictionaryEntry(FeedEntry)

		/// <summary>
		/// Converts a <see cref="FeedEntry"/> to a <see cref="DictionaryEntry"/>.
		/// </summary>
		/// <param name="pair">A <see cref="FeedEntry"/> object to convert.</param>
		/// <returns>A <see cref="DictionaryEntry"/> object that
		/// represents the converted <paramref name="pair"/>.</returns>

		public static implicit operator DictionaryEntry(FeedEntry pair) {
			DictionaryEntry entry = new DictionaryEntry();
			if (pair.Key != null) entry.Key = pair.Key;
			entry.Value = pair.Value;
			return entry;
		}

		#endregion
		#endregion
	}

	#endregion

	#region Class FeedsCollection

	/// <summary>
	/// Implements a strongly typed collection of <see cref="FeedEntry"/>
	/// key-and-value pairs that are sorted by the keys and are accessible by key and by index.
	/// </summary>
	/// <remarks>
	/// <b>FeedsCollection</b> provides a <see cref="SortedList"/> that is strongly typed
	/// for <see cref="String"/> keys and <see cref="feedsFeed"/> values.
	/// </remarks>

	[Serializable]
	public class FeedsCollection:
		IStringfeedsFeedDictionary, IDictionary, ICloneable {
		#region Private Fields

		private const int _defaultCapacity = 16;

		private String[] _keys;
		private feedsFeed[] _values;
		private IComparer _comparer;
		private int _count;

		[NonSerialized]
		private int _version;
		private KeyList _keyList;
		private ValueList _valueList;

		#endregion
		#region Private Constructors

		// helper type to identify private ctor
		private enum Tag { Default }

		private FeedsCollection(Tag tag) { }

		#endregion
		#region Public Constructors
		#region FeedsCollection()

		/// <overloads>
		/// Initializes a new instance of the <see cref="FeedsCollection"/> class.
		/// </overloads>
		/// <summary>
		/// Initializes a new instance of the <see cref="FeedsCollection"/> class that is empty,
		/// has the default initial capacity and is sorted according to the <see cref="IComparable"/>
		/// interface implemented by each key added to the <b>FeedsCollection</b>.
		/// </summary>
		/// <remarks>Please refer to <see cref="SortedList()"/> for details.</remarks>

		public FeedsCollection() {
			this._keys = new String[_defaultCapacity];
			this._values = new feedsFeed[_defaultCapacity];
			this._comparer = Comparer.Default;
		}

		#endregion
		#region FeedsCollection(IComparer)

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedsCollection"/> class that is empty,
		/// has the default initial capacity and is sorted according to the specified
		/// <see cref="IComparer"/> interface.
		/// </summary>
		/// <param name="comparer">
		/// <para>The <see cref="IComparer"/> implementation to use when comparing keys.</para>
		/// <para>-or-</para>
		/// <para>A null reference, to use the <see cref="IComparable"/> implementation of each key.
		/// </para></param>
		/// <remarks>Please refer to <see cref="SortedList(IComparer)"/> for details.</remarks>

		public FeedsCollection(IComparer comparer): this() {
			if (comparer != null) this._comparer = comparer;
		}

		#endregion
		#region FeedsCollection(IDictionary)

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedsCollection"/> class that contains
		/// elements copied from the specified dictionary, has the same initial capacity as the
		/// number of elements copied and is sorted according to the <see cref="IComparable"/>
		/// interface implemented by each key.
		/// </summary>
		/// <param name="dictionary">The <see cref="IDictionary"/>
		/// whose elements are copied to the new collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="dictionary"/> is a null reference.</exception>
		/// <exception cref="InvalidCastException">
		/// <para>One or more elements in <paramref name="dictionary"/> do not implement the
		/// <see cref="IComparable"/> interface.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dictionary"/> contains keys that are not compatible
		/// with <see cref="String"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dictionary"/> contains values that are not compatible
		/// with <see cref="feedsFeed"/>.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="SortedList(IDictionary)"/> for details.</remarks>

		public FeedsCollection(IDictionary dictionary): this(dictionary, null) { }

		#endregion
		#region FeedsCollection(Int32)

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedsCollection"/> class that is empty,
		/// has the specified initial capacity and is sorted according to the <see cref="IComparable"/>
		/// interface implemented by each key added to the <b>FeedsCollection</b>.
		/// </summary>
		/// <param name="capacity">The initial number of elements that the
		/// <see cref="FeedsCollection"/> can contain.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="capacity"/> is less than zero.</exception>
		/// <remarks>Please refer to <see cref="SortedList(Int32)"/> for details.</remarks>

		public FeedsCollection(int capacity) {
			if (capacity < 0)
				throw new ArgumentOutOfRangeException("capacity",
					capacity, "Argument cannot be negative.");

			this._keys = new String[capacity];
			this._values = new feedsFeed[capacity];
			this._comparer = Comparer.Default;
		}

		#endregion
		#region FeedsCollection(IComparer, Int32)

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedsCollection"/> class that is empty,
		/// has the specified initial capacity and is sorted according to the specified
		/// <see cref="IComparer"/> interface.
		/// </summary>
		/// <param name="comparer">
		/// <para>The <see cref="IComparer"/> implementation to use when comparing keys.</para>
		/// <para>-or-</para>
		/// <para>A null reference to use the <see cref="IComparable"/> implementation of each key.
		/// </para></param>
		/// <param name="capacity">The initial number of elements that the
		/// <see cref="FeedsCollection"/> can contain.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="capacity"/> is less than zero.</exception>
		/// <remarks>
		/// Please refer to <see cref="SortedList(IComparer, Int32)"/> for details.
		/// </remarks>

		public FeedsCollection(IComparer comparer, int capacity) : this(capacity) {
			if (comparer != null) this._comparer = comparer;
		}

		#endregion
		#region FeedsCollection(IDictionary, IComparer)

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedsCollection"/> class that contains
		/// elements copied from the specified dictionary, has the same initial capacity as the
		/// number of elements copied and is sorted according to the specified <see cref="IComparer"/>
		/// interface.
		/// </summary>
		/// <param name="dictionary">The <see cref="IDictionary"/>
		/// whose elements are copied to the new collection.</param>
		/// <param name="comparer">
		/// <para>The <see cref="IComparer"/> implementation to use when comparing keys.</para>
		/// <para>-or-</para>
		/// <para>A null reference, to use the <see cref="IComparable"/> implementation of each key.
		/// </para></param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="dictionary"/> is a null reference.</exception>
		/// <exception cref="InvalidCastException">
		/// <para>One or more elements in <paramref name="dictionary"/> do not implement the
		/// <see cref="IComparable"/> interface.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dictionary"/> contains keys that are not compatible
		/// with <see cref="String"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="dictionary"/> contains values that are not compatible
		/// with <see cref="feedsFeed"/>.</para>
		/// </exception>
		/// <remarks>
		/// Please refer to <see cref="SortedList(IDictionary, IComparer)"/> for details.
		/// </remarks>

		public FeedsCollection(IDictionary dictionary, IComparer comparer):
			this(comparer, (dictionary == null ? 0 : dictionary.Count)) {

			if (dictionary == null)
				throw new ArgumentNullException("dictionary");

			dictionary.Keys.CopyTo(this._keys, 0);
			dictionary.Values.CopyTo(this._values, 0);
			Array.Sort(this._keys, this._values, this._comparer);
			this._count = dictionary.Count;
		}

		#endregion
		#endregion
		#region Public Properties
		#region Capacity

		/// <summary>
		/// Gets or sets the capacity of the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <value>The number of elements that the
		/// <see cref="FeedsCollection"/> can contain.</value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <b>Capacity</b> is set to a value that is less than <see cref="Count"/>.</exception>
		/// <remarks>Please refer to <see cref="SortedList.Capacity"/> for details.</remarks>

		public virtual int Capacity {
			get { return this._keys.Length; }
			set {
				if (value == this._keys.Length) return;

				if (value < this._count)
					throw new ArgumentOutOfRangeException("Capacity",
						value, "Value cannot be less than Count.");

				if (value == 0) {
					this._keys = new String[_defaultCapacity];
					this._values = new feedsFeed[_defaultCapacity];
					return;
				}

				String[] newKeys = new String[value];
				feedsFeed[] newValues = new feedsFeed[value];

				Array.Copy(this._keys, 0, newKeys, 0, this._count);
				Array.Copy(this._values, 0, newValues, 0, this._count);

				this._keys = newKeys;
				this._values = newValues;
			}
		}

		#endregion
		#region Count

		/// <summary>
		/// Gets the number of key-and-value pairs contained
		/// in the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <value>The number of key-and-value pairs contained
		/// in the <see cref="FeedsCollection"/>.</value>
		/// <remarks>Please refer to <see cref="SortedList.Count"/> for details.</remarks>

		public virtual int Count {
			get { return this._count; }
		}

		#endregion
		#region IsFixedSize

		/// <summary>
		/// Gets a value indicating whether the <see cref="FeedsCollection"/> has a fixed size.
		/// </summary>
		/// <value><c>true</c> if the <see cref="FeedsCollection"/> has a fixed size;
		/// otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="SortedList.IsFixedSize"/> for details.</remarks>

		public virtual bool IsFixedSize {
			get { return false; }
		}

		#endregion
		#region IsReadOnly

		/// <summary>
		/// Gets a value indicating whether the <see cref="FeedsCollection"/> is read-only.
		/// </summary>
		/// <value><c>true</c> if the <see cref="FeedsCollection"/> is read-only;
		/// otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="SortedList.IsReadOnly"/> for details.</remarks>

		public virtual bool IsReadOnly {
			get { return false; }
		}

		#endregion
		#region IsSynchronized

		/// <summary>
		/// Gets a value indicating whether access to the <see cref="FeedsCollection"/>
		/// is synchronized (thread-safe).
		/// </summary>
		/// <value><c>true</c> if access to the <see cref="FeedsCollection"/> is
		/// synchronized (thread-safe); otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="SortedList.IsSynchronized"/> for details.</remarks>

		public virtual bool IsSynchronized {
			get { return false; }
		}

		#endregion
		#region Item[String]: feedsFeed

		/// <summary>
		/// Gets or sets the <see cref="feedsFeed"/> value
		/// associated with the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key
		/// whose value to get or set.</param>
		/// <value>The <see cref="feedsFeed"/> value associated with the specified
		/// <paramref name="key"/>. If the specified <paramref name="key"/> is not found,
		/// attempting to get it returns
		/// a null reference,
		/// and attempting to set it creates a new element using the specified
		/// <paramref name="key"/>.</value>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The property is set and the <see cref="FeedsCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The property is set, <paramref name="key"/> does not exist in the collection,
		/// and the <b>FeedsCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.this"/> for details.</remarks>

		public virtual feedsFeed this[String key] {
			get {
				if ((object) key == null)
					throw new ArgumentNullException("key");

				int index = BinaryKeySearch(key);
				if (index >= 0) return this._values[index];
				return null;
			}
			set {
				if ((object) key == null)
					throw new ArgumentNullException("key");

				int index = BinaryKeySearch(key);

				if (index >= 0) {
					++this._version;
					this._values[index] = value;
					return;
				}

				Insert(~index, key, value);
			}
		}
		public virtual feedsFeed this[Uri key] {
			get {
				if (key == null)
					throw new ArgumentNullException("key");
				
				string feedUrl = key.AbsoluteUri;
				if (!ContainsKey(feedUrl) && (key.IsFile || key.IsUnc)) {
					feedUrl = key.LocalPath;
				}
				return this[feedUrl];
			}
			set {
				if (key == null)
					throw new ArgumentNullException("key");

				string feedUrl = key.AbsoluteUri;
				if (!ContainsKey(feedUrl) && (key.IsFile || key.IsUnc)) {
					feedUrl = key.LocalPath;
				}
				this[feedUrl] = value;
			}
		}

		#endregion
		#region IDictionary.Item[Object]: Object

		/// <summary>
		/// Gets or sets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get or set.
		/// This argument must be compatible with <see cref="String"/>.</param>
		/// <value>
		/// The value associated with the specified <paramref name="key"/>. If the specified
		/// <paramref name="key"/> is not found, attempting to get it returns
		/// a null reference,
		/// and attempting to set it creates a new element using the specified <paramref name="key"/>.
		/// When set, this value must be compatible with <see cref="feedsFeed"/>.
		/// </value>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidCastException">
		/// <para><paramref name="key"/> is not compatible with <see cref="String"/>.</para>
		/// <para>-or-</para>
		/// <para>The property is set to a value that is not compatible with
		/// <see cref="feedsFeed"/>.</para></exception>
		/// <exception cref="NotSupportedException">
		/// <para>The property is set and the <see cref="FeedsCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The property is set, <paramref name="key"/> does not exist in the collection,
		/// and the <b>FeedsCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.this"/> for details.</remarks>

		object IDictionary.this[object key] {
			get { return this[(String) key]; }
			set { this[(String) key] = (feedsFeed) value; }
		}

		#endregion
		#region Keys: IStringCollection

		/// <summary>
		/// Gets an <see cref="IStringCollection"/> containing
		/// the keys in the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <value>An <see cref="IStringCollection"/> containing
		/// the keys in the <see cref="FeedsCollection"/>.</value>
		/// <remarks>Please refer to <see cref="SortedList.Keys"/> for details.</remarks>

		public virtual ICollection<string> Keys {
			get { return GetKeyList(); }
		}

		#endregion
		#region IDictionary.Keys: ICollection

		/// <summary>
		/// Gets an <see cref="ICollection"/> containing
		/// the keys in the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <value>An <see cref="ICollection"/> containing
		/// the keys in the <see cref="FeedsCollection"/>.</value>
		/// <remarks>Please refer to <see cref="SortedList.Keys"/> for details.</remarks>

		ICollection IDictionary.Keys {
			get { return (ICollection) Keys; }
		}

		#endregion
		#region SyncRoot

		/// <summary>
		/// Gets an object that can be used to synchronize
		/// access to the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <value>An object that can be used to synchronize
		/// access to the <see cref="FeedsCollection"/>.</value>
		/// <remarks>Please refer to <see cref="SortedList.SyncRoot"/> for details.</remarks>

		public virtual object SyncRoot {
			get { return this; }
		}

		#endregion
		#region Values: IfeedsFeedCollection

		/// <summary>
		/// Gets an <see cref="IfeedsFeedCollection"/> containing
		/// the values in the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <value>An <see cref="IfeedsFeedCollection"/> containing
		/// the values in the <see cref="FeedsCollection"/>.</value>
		/// <remarks>Please refer to <see cref="SortedList.Values"/> for details.</remarks>

		public virtual IfeedsFeedCollection Values {
			get { return GetValueList(); }
		}

		#endregion
		#region IDictionary.Values: ICollection

		/// <summary>
		/// Gets an <see cref="ICollection"/> containing
		/// the values in the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <value>An <see cref="ICollection"/> containing
		/// the values in the <see cref="FeedsCollection"/>.</value>
		/// <remarks>Please refer to <see cref="SortedList.Values"/> for details.</remarks>

		ICollection IDictionary.Values {
			get { return (ICollection) Values; }
		}

		#endregion
		#endregion
		#region Public Methods
		#region Add(String, feedsFeed)

		/// <summary>
		/// Adds an element with the specified <see cref="String"/> key and
		/// <see cref="feedsFeed"/> value to the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key of the element
		/// to add to the <see cref="FeedsCollection"/>.</param>
		/// <param name="value">The <see cref="feedsFeed"/> value of the element
		/// to add to the <see cref="FeedsCollection"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentException">
		/// <para>An element with the specified <paramref name="key"/>
		/// already exists in the <see cref="FeedsCollection"/>.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedsCollection</b> is set to use the <see cref="IComparable"/> interface,
		/// and <paramref name="key"/> does not implement the <b>IComparable</b> interface.</para>
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedsCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedsCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.Add"/> for details.</remarks>

		public virtual void Add(String key, feedsFeed value) {
			if ((object) key == null)
				throw new ArgumentNullException("key");

			/* convert the URI to a canonicalized absolute URI */ 
			try{
				Uri uri = new Uri(key); 
				key = uri.AbsoluteUri;
				value.link = key; 
			}catch {}

			int index = BinaryKeySearch(key);

			if (index >= 0)
				throw new ArgumentException(
					"Argument already exists in collection.", "key");

			Insert(~index, key, value);
		}

		#endregion
		#region IDictionary.Add(Object, Object)

		/// <summary>
		/// Adds an element with the specified key and value
		/// to the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <param name="key">The key of the element to add to the <see cref="FeedsCollection"/>.
		/// This argument must be compatible with <see cref="String"/>.</param>
		/// <param name="value">The value of the element to add to the <see cref="FeedsCollection"/>.
		/// This argument must be compatible with <see cref="feedsFeed"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentException">
		/// <para>An element with the specified <paramref name="key"/>
		/// already exists in the <see cref="FeedsCollection"/>.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedsCollection</b> is set to use the <see cref="IComparable"/> interface,
		/// and <paramref name="key"/> does not implement the <b>IComparable</b> interface.</para>
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidCastException">
		/// <para><paramref name="key"/> is not compatible with <see cref="String"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is not compatible with <see cref="feedsFeed"/>.</para>
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedsCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedsCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.Add"/> for details.</remarks>

		void IDictionary.Add(object key, object value) {
			Add((String) key, (feedsFeed) value);
		}

		#endregion
		#region Clear

		/// <summary>
		/// Removes all elements from the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedsCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedsCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.Clear"/> for details.</remarks>

		public virtual void Clear() {
			if (this._count == 0) return;

			++this._version;
			Array.Clear(this._keys, 0, this._count);
			Array.Clear(this._values, 0, this._count);
			this._count = 0;
		}

		#endregion
		#region Clone

		/// <summary>
		/// Creates a shallow copy of the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <returns>A shallow copy of the <see cref="FeedsCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.Clone"/> for details.</remarks>

		public virtual object Clone() {
			FeedsCollection dictionary = new FeedsCollection(this._count);

			Array.Copy(this._keys, 0, dictionary._keys, 0, this._count);
			Array.Copy(this._values, 0, dictionary._values, 0, this._count);

			dictionary._count = this._count;
			dictionary._comparer = this._comparer;
			dictionary._version = this._version;

			return dictionary;
		}

		#endregion
		#region Contains(String)

		/// <summary>
		/// Determines whether the <see cref="FeedsCollection"/>
		/// contains the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key
		/// to locate in the <see cref="FeedsCollection"/>.</param>
		/// <returns><c>true</c> if the <see cref="FeedsCollection"/> contains an element
		/// with the specified <paramref name="key"/>; otherwise, <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <remarks>Please refer to <see cref="SortedList.Contains"/> for details.</remarks>
		public virtual bool Contains(String key) {
			return ContainsKey(key);
		}
		/// <summary>
		/// Overloaded.
		/// </summary>
		/// <param name="key">Uri</param>
		/// <returns></returns>
		public virtual bool Contains(Uri key) {
			return ContainsKey(key);
		}
		

		#endregion
		#region IDictionary.Contains(Object)

		/// <summary>
		/// Determines whether the <see cref="FeedsCollection"/> contains the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="FeedsCollection"/>.
		/// This argument must be compatible with <see cref="String"/>.</param>
		/// <returns><c>true</c> if the <see cref="FeedsCollection"/> contains an element
		/// with the specified <paramref name="key"/>; otherwise, <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidCastException"><paramref name="key"/>
		/// is not compatible with <see cref="String"/>.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <remarks>Please refer to <see cref="SortedList.Contains"/> for details.</remarks>

		bool IDictionary.Contains(object key) {
			return (IndexOfKey((String) key) >= 0);
		}

		#endregion
		#region ContainsKey

		/// <summary>
		/// Determines whether the <see cref="FeedsCollection"/>
		/// contains the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key
		/// to locate in the <see cref="FeedsCollection"/>.</param>
		/// <returns><c>true</c> if the <see cref="FeedsCollection"/> contains an element
		/// with the specified <paramref name="key"/>; otherwise, <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <remarks>Please refer to <see cref="SortedList.ContainsKey"/> for details.</remarks>

		public virtual bool ContainsKey(String key) {

			if ((object) key == null)
				throw new ArgumentNullException("key");

			/* convert the URI to a canonicalized absolute URI */ 
			try{
				Uri uri = new Uri(key); 
				key = uri.AbsoluteUri;				
			}catch {}
		
			return (IndexOfKey(key) >= 0);
		}
		/// <summary>
		/// Overloaded.
		/// </summary>
		/// <param name="key">Uri</param>
		/// <returns></returns>
		public virtual bool ContainsKey(Uri key) {
			if (key == null)
				return false;

			string feedUrl = key.AbsoluteUri;
			if (!ContainsKey(feedUrl) && (key.IsFile || key.IsUnc)) {
				feedUrl = key.LocalPath;
			}

			return ContainsKey(feedUrl);
		
		}
		#endregion
		#region ContainsValue

		/// <summary>
		/// Determines whether the <see cref="FeedsCollection"/>
		/// contains the specified <see cref="feedsFeed"/> value.
		/// </summary>
		/// <param name="value">The <see cref="feedsFeed"/> value
		/// to locate in the <see cref="FeedsCollection"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns><c>true</c> if the <see cref="FeedsCollection"/> contains an element
		/// with the specified <paramref name="value"/>; otherwise, <c>false</c>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.ContainsValue"/> for details.</remarks>

		public virtual bool ContainsValue(feedsFeed value) {
			return (IndexOfValue(value) >= 0);
		}

		#endregion
		#region CopyTo(FeedEntry[], Int32)

		/// <summary>
		/// Copies the entire <see cref="FeedsCollection"/> to a one-dimensional <see cref="Array"/> of
		/// <see cref="FeedEntry"/> elements, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
		/// <see cref="FeedEntry"/> elements copied from the <see cref="FeedsCollection"/>.
		/// The <b>Array</b> must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/>
		/// at which copying begins.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is a null reference.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is less than zero.</exception>
		/// <exception cref="ArgumentException"><para>
		/// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
		/// </para><para>-or-</para><para>
		/// The number of elements in the source <see cref="FeedsCollection"/> is greater than
		/// the available space from <paramref name="arrayIndex"/> to the end of the destination
		/// <paramref name="array"/>.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.CopyTo"/> for details.</remarks>

		public virtual void CopyTo(FeedEntry[] array, int arrayIndex) {
			CheckTargetArray(array, arrayIndex);

			for (int i = 0; i < this._count; i++) {
				FeedEntry entry =
					new FeedEntry(this._keys[i], this._values[i]);

				array.SetValue(entry, arrayIndex + i);
			}
		}

		#endregion
		#region ICollection.CopyTo(Array, Int32)

		/// <summary>
		/// Copies the entire <see cref="FeedsCollection"/> to a one-dimensional <see cref="Array"/>,
		/// starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
		/// <see cref="FeedEntry"/> elements copied from the <see cref="FeedsCollection"/>.
		/// The <b>Array</b> must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/>
		/// at which copying begins.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is a null reference.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is less than zero.</exception>
		/// <exception cref="ArgumentException"><para>
		/// <paramref name="array"/> is multidimensional.
		/// </para><para>-or-</para><para>
		/// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
		/// </para><para>-or-</para><para>
		/// The number of elements in the source <see cref="FeedsCollection"/> is greater than
		/// the available space from <paramref name="arrayIndex"/> to the end of the destination
		/// <paramref name="array"/>.</para></exception>
		/// <exception cref="InvalidCastException">
		/// The <see cref="FeedEntry"/> type cannot be cast automatically
		/// to the type of the destination <paramref name="array"/>.</exception>
		/// <remarks>Please refer to <see cref="SortedList.CopyTo"/> for details.</remarks>

		void ICollection.CopyTo(Array array, int arrayIndex) {
			CopyTo((FeedEntry[]) array, arrayIndex);
		}

		#endregion
		#region GetByIndex

		/// <summary>
		/// Gets the <see cref="feedsFeed"/> value at the
		/// specified index of the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="feedsFeed"/> value to get.</param>
		/// <returns>The <see cref="feedsFeed"/> value at the specified
		/// <paramref name="index"/> of the <see cref="FeedsCollection"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="SortedList.GetByIndex"/> for details.</remarks>

		public virtual feedsFeed GetByIndex(int index) {
			ValidateIndex(index);
			return this._values[index];
		}

		#endregion
		#region GetEnumerator: IStringfeedsFeedEnumerator

		/// <summary>
		/// Returns an <see cref="IStringfeedsFeedEnumerator"/> that can
		/// iterate through the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IStringfeedsFeedEnumerator"/>
		/// for the entire <see cref="FeedsCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.GetEnumerator"/> for details.</remarks>

		public virtual IStringfeedsFeedEnumerator GetEnumerator() {
			return new Enumerator(this);
		}

		#endregion
		#region IDictionary.GetEnumerator: IDictionaryEnumerator

		/// <summary>
		/// Returns an <see cref="IDictionaryEnumerator"/> that can
		/// iterate through the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IDictionaryEnumerator"/>
		/// for the entire <see cref="FeedsCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.GetEnumerator"/> for details.</remarks>

		IDictionaryEnumerator IDictionary.GetEnumerator() {
			return (IDictionaryEnumerator) GetEnumerator();
		}

		#endregion
		#region IEnumerable.GetEnumerator: IEnumerator

		/// <summary>
		/// Returns an <see cref="IEnumerator"/> that can
		/// iterate through the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IEnumerator"/>
		/// for the entire <see cref="FeedsCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.GetEnumerator"/> for details.</remarks>

		IEnumerator IEnumerable.GetEnumerator() {
			return (IEnumerator) GetEnumerator();
		}

		#endregion
		#region GetKey

		/// <summary>
		/// Gets the <see cref="String"/> key at the
		/// specified index of the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="String"/> key to get.</param>
		/// <returns>The <see cref="String"/> key at the specified
		/// <paramref name="index"/> of the <see cref="FeedsCollection"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="SortedList.GetKey"/> for details.</remarks>

		public virtual String GetKey(int index) {
			ValidateIndex(index);
			return this._keys[index];
		}

		#endregion
		#region GetKeyList

		/// <summary>
		/// Gets the keys in the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IStringList"/> containing the keys
		/// in the <see cref="FeedsCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.GetKeyList"/> for details.</remarks>

		public virtual IList<string> GetKeyList() {
			if (this._keyList == null)
				this._keyList = new KeyList(this);
			return this._keyList;
		}

		#endregion
		#region GetValueList

		/// <summary>
		/// Gets the values in the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IfeedsFeedList"/> containing the values
		/// in the <see cref="FeedsCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.GetValueList"/> for details.</remarks>

		public virtual IfeedsFeedList GetValueList() {
			if (this._valueList == null)
				this._valueList = new ValueList(this);
			return this._valueList;
		}

		#endregion
		#region IndexOfKey

		/// <summary>
		/// Returns the zero-based index of the specified <see cref="String"/>
		/// key in the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key
		/// to locate in the <see cref="FeedsCollection"/>.</param>
		/// <returns>The zero-based index of <paramref name="key"/> in the
		/// <see cref="FeedsCollection"/>, if found; otherwise, -1.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <remarks>Please refer to <see cref="SortedList.IndexOfKey"/> for details.</remarks>

		public virtual int IndexOfKey(String key) {
			if ((object) key == null)
				throw new ArgumentNullException("key");

			int index = BinaryKeySearch(key);
			return (index >= 0 ? index : -1);
		}

		#endregion
		#region IndexOfValue

		/// <summary>
		/// Returns the zero-based index of the specified <see cref="feedsFeed"/>
		/// value in the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <param name="value">The <see cref="feedsFeed"/> value
		/// to locate in the <see cref="FeedsCollection"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns>The zero-based index of <paramref name="value"/> in the
		/// <see cref="FeedsCollection"/>, if found; otherwise, -1.</returns>
		/// <remarks>Please refer to <see cref="SortedList.IndexOfValue"/> for details.</remarks>

		public virtual int IndexOfValue(feedsFeed value) {
			return Array.IndexOf(this._values, value, 0, this._count);
		}

		#endregion
		#region Remove(String)

		/// <summary>
		/// Removes the element with the specified <see cref="String"/> key
		/// from the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key of the element
		/// to remove from the <see cref="FeedsCollection"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedsCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedsCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.Remove"/> for details.</remarks>

		public virtual void Remove(String key) {
			int index = IndexOfKey(key);
			if (index >= 0) RemoveAt(index);
		}

		#endregion
		#region IDictionary.Remove(Object)

		/// <summary>
		/// Removes the element with the specified key from the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <param name="key">The key of the element to remove from the <see cref="FeedsCollection"/>.
		/// This argument must be compatible with <see cref="String"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidCastException"><paramref name="key"/>
		/// is not compatible with <see cref="String"/>.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedsCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedsCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.Remove"/> for details.</remarks>

		void IDictionary.Remove(object key) {
			Remove((String) key);
		}

		#endregion
		#region RemoveAt

		/// <summary>
		/// Removes the element at the specified index of the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedsCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedsCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.RemoveAt"/> for details.</remarks>

		public virtual void RemoveAt(int index) {
			ValidateIndex(index);

			++this._version;
			if (index < --this._count) {
				Array.Copy(this._keys, index + 1,
					this._keys, index, this._count - index);

				Array.Copy(this._values, index + 1,
					this._values, index, this._count - index);
			}

			this._keys[this._count] = null;
			this._values[this._count] = null;
		}

		#endregion
		#region SetByIndex

		/// <summary>
		/// Sets the <see cref="feedsFeed"/> value at the
		/// specified index of the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="feedsFeed"/> value to set.</param>
		/// <param name="value">The <see cref="feedsFeed"/> object to store
		/// at the specified <paramref name="index"/> of the <see cref="FeedsCollection"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="SortedList.SetByIndex"/> for details.</remarks>

		public virtual void SetByIndex(int index, feedsFeed value) {
			ValidateIndex(index);
			++this._version;
			this._values[index] = value;
		}

		#endregion
		#region Synchronized

		/// <summary>
		/// Returns a synchronized (thread-safe) wrapper
		/// for the specified <see cref="FeedsCollection"/>.
		/// </summary>
		/// <param name="dictionary">The <see cref="FeedsCollection"/> to synchronize.</param>
		/// <returns>
		/// A synchronized (thread-safe) wrapper around <paramref name="dictionary"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="dictionary"/> is a null reference.</exception>
		/// <remarks>Please refer to <see cref="SortedList.Synchronized"/> for details.</remarks>

		public static FeedsCollection Synchronized(FeedsCollection dictionary) {
			if (dictionary == null)
				throw new ArgumentNullException("dictionary");

			return new SyncDictionary(dictionary);
		}

		#endregion
		#region TrimToSize

		/// <summary>
		/// Sets the capacity to the actual number of elements in the <see cref="FeedsCollection"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedsCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedsCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.TrimToSize"/> for details.</remarks>

		public virtual void TrimToSize() {
			Capacity = this._count;
		}

		#endregion
		#endregion
		#region Private Methods
		#region BinaryKeySearch

		private int BinaryKeySearch(String key) {
			return Array.BinarySearch(this._keys, 0,
				this._count, key, this._comparer);
		}

		#endregion
		#region CheckEnumIndex

		private void CheckEnumIndex(int index) {
			if (index < 0 || index >= this._count)
				throw new InvalidOperationException(
					"Enumerator is not on a collection element.");
		}

		#endregion
		#region CheckEnumVersion

		private void CheckEnumVersion(int version) {
			if (version != this._version)
				throw new InvalidOperationException(
					"Enumerator invalidated by modification to collection.");
		}

		#endregion
		#region CheckTargetArray

		private void CheckTargetArray(Array array, int arrayIndex) {
			if (array == null)
				throw new ArgumentNullException("array");
			if (array.Rank > 1)
				throw new ArgumentException(
					"Argument cannot be multidimensional.", "array");

			if (arrayIndex < 0)
				throw new ArgumentOutOfRangeException("arrayIndex",
					arrayIndex, "Argument cannot be negative.");
			if (arrayIndex >= array.Length)
				throw new ArgumentException(
					"Argument must be less than array length.", "arrayIndex");

			if (this._count > array.Length - arrayIndex)
				throw new ArgumentException(
					"Argument section must be large enough for collection.", "array");
		}

		#endregion
		#region EnsureCapacity

		private void EnsureCapacity(int minimum) {
			int newCapacity = (this._keys.Length == 0 ?
			_defaultCapacity : this._keys.Length * 2);

			if (newCapacity < minimum) newCapacity = minimum;
			Capacity = newCapacity;
		}

		#endregion
		#region Insert

		private void Insert(int index,
			String key, feedsFeed value) {

			if (this._count == this._keys.Length)
				EnsureCapacity(this._count + 1);

			++this._version;
			if (index < this._count) {
				Array.Copy(this._keys, index,
					this._keys, index + 1, this._count - index);

				Array.Copy(this._values, index,
					this._values, index + 1, this._count - index);
			}

			this._keys[index] = key;
			this._values[index] = value;
			++this._count;
		}

		#endregion
		#region ValidateIndex

		private void ValidateIndex(int index) {
			if (index < 0)
				throw new ArgumentOutOfRangeException("index",
					index, "Argument cannot be negative.");

			if (index >= this._count)
				throw new ArgumentOutOfRangeException("index",
					index, "Argument must be less than Count.");
		}

		#endregion
		#endregion
		#region Class Enumerator

		[Serializable]
			private sealed class Enumerator:
			IStringfeedsFeedEnumerator, IDictionaryEnumerator {
			#region Private Fields

			private readonly FeedsCollection _dictionary;
			private readonly int _version;
			private int _index;

			#endregion
			#region Internal Constructors

			internal Enumerator(FeedsCollection dictionary) {
				this._dictionary = dictionary;
				this._version = dictionary._version;
				this._index = -1;
			}

			#endregion
			#region Public Properties

			public FeedEntry Current {
				get { return Entry; }
			}

			object IEnumerator.Current {
				get { return (DictionaryEntry) Entry; }
			}

			public FeedEntry Entry {
				get {
					this._dictionary.CheckEnumIndex(this._index);
					this._dictionary.CheckEnumVersion(this._version);

					return new FeedEntry(
						this._dictionary._keys[this._index],
						this._dictionary._values[this._index]);
				}
			}

			DictionaryEntry IDictionaryEnumerator.Entry {
				get { return Entry; }
			}

			public String Key {
				get {
					this._dictionary.CheckEnumIndex(this._index);
					this._dictionary.CheckEnumVersion(this._version);
					return this._dictionary._keys[this._index];
				}
			}

			object IDictionaryEnumerator.Key {
				get { return Key; }
			}

			public feedsFeed Value {
				get {
					this._dictionary.CheckEnumIndex(this._index);
					this._dictionary.CheckEnumVersion(this._version);
					return this._dictionary._values[this._index];
				}
			}

			object IDictionaryEnumerator.Value {
				get { return Value; }
			}

			#endregion
			#region Public Methods

			public bool MoveNext() {
				this._dictionary.CheckEnumVersion(this._version);
				return (++this._index < this._dictionary.Count);
			}

			public void Reset() {
				this._dictionary.CheckEnumVersion(this._version);
				this._index = -1;
			}

			#endregion
		}

		#endregion
		#region Class KeyList

		[Serializable]
			private sealed class KeyList: IList<string> {
			#region Private Fields

			private FeedsCollection _dictionary;

			#endregion
			#region Internal Constructors

			internal KeyList(FeedsCollection dictionary) {
				this._dictionary = dictionary;
			}

			#endregion
			#region Public Properties

			public int Count {
				get { return this._dictionary.Count; }
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public bool IsFixedSize {
				get { return true; }
			}

			public bool IsSynchronized {
				get { return this._dictionary.IsSynchronized; }
			}

			public String this[int index] {
				get { return this._dictionary.GetKey(index); }
				set { throw new NotSupportedException(
						  "Read-only collections cannot be modified."); }
			}

			public object SyncRoot {
				get { return this._dictionary.SyncRoot; }
			}

			#endregion
			#region Public Methods

			public void Add(String key) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}


			public void Clear() {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			public bool Contains(String key) {
				return this._dictionary.Contains(key);
			}

			public void CopyTo(String[] array, int arrayIndex) {
				this._dictionary.CheckTargetArray(array, arrayIndex);
				Array.Copy(this._dictionary._keys, 0,
					array, arrayIndex, this._dictionary.Count);
			}


			public IEnumerator<string> GetEnumerator() {
				return new KeyEnumerator(this._dictionary);
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return (IEnumerator) GetEnumerator();
			}

			public int IndexOf(String key) {
				return this._dictionary.IndexOfKey(key);
			}

	

			public void Insert(int index, String key) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}


			public bool Remove(String key) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			public void RemoveAt(int index) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			#endregion
		}

		#endregion
		#region Class KeyEnumerator

        [Serializable]
        private sealed class KeyEnumerator :
        IEnumerator<string>
        {
            #region Private Fields

            private readonly FeedsCollection _dictionary;
            private readonly int _version;
            private int _index;

            #endregion
            #region Internal Constructors

            internal KeyEnumerator(FeedsCollection dictionary)
            {
                this._dictionary = dictionary;
                this._version = dictionary._version;
                this._index = -1;
            }

            #endregion
            #region Public Properties

            public String Current
            {
                get
                {
                    this._dictionary.CheckEnumIndex(this._index);
                    this._dictionary.CheckEnumVersion(this._version);
                    return this._dictionary._keys[this._index];
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            #endregion
            #region Public Methods

            public bool MoveNext()
            {
                this._dictionary.CheckEnumVersion(this._version);
                return (++this._index < this._dictionary.Count);
            }

            public void Reset()
            {
                this._dictionary.CheckEnumVersion(this._version);
                this._index = -1;
            }

            #endregion

            public void Dispose()
            {

            }
        }

		#endregion
		#region Class ValueList

		[Serializable]
			private sealed class ValueList: IfeedsFeedList, IList {
			#region Private Fields

			private FeedsCollection _dictionary;

			#endregion
			#region Internal Constructors

			internal ValueList(FeedsCollection dictionary) {
				this._dictionary = dictionary;
			}

			#endregion
			#region Public Properties

			public int Count {
				get { return this._dictionary.Count; }
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public bool IsFixedSize {
				get { return true; }
			}

			public bool IsSynchronized {
				get { return this._dictionary.IsSynchronized; }
			}

			public feedsFeed this[int index] {
				get { return this._dictionary.GetByIndex(index); }
				set { throw new NotSupportedException(
						  "Read-only collections cannot be modified."); }
			}

			object IList.this[int index] {
				get { return this[index]; }
				set { throw new NotSupportedException(
						  "Read-only collections cannot be modified."); }
			}

			public object SyncRoot {
				get { return this._dictionary.SyncRoot; }
			}

			#endregion
			#region Public Methods

			public int Add(feedsFeed value) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			int IList.Add(object value) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			public void Clear() {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			public bool Contains(feedsFeed value) {
				return this._dictionary.ContainsValue(value);
			}

			bool IList.Contains(object value) {
				return Contains((feedsFeed) value);
			}

			public void CopyTo(feedsFeed[] array, int arrayIndex) {
				this._dictionary.CheckTargetArray(array, arrayIndex);
				Array.Copy(this._dictionary._values, 0,
					array, arrayIndex, this._dictionary.Count);
			}

			void ICollection.CopyTo(Array array, int arrayIndex) {
				this._dictionary.CheckTargetArray(array, arrayIndex);
				CopyTo((feedsFeed[]) array, arrayIndex);
			}

			public IfeedsFeedEnumerator GetEnumerator() {
				return new ValueEnumerator(this._dictionary);
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return (IEnumerator) GetEnumerator();
			}

			public int IndexOf(feedsFeed value) {
				return this._dictionary.IndexOfValue(value);
			}

			int IList.IndexOf(object value) {
				return IndexOf((feedsFeed) value);
			}

			public void Insert(int index, feedsFeed value) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			void IList.Insert(int index, object value) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			public void Remove(feedsFeed value) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			void IList.Remove(object value) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			public void RemoveAt(int index) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			#endregion
		}

		#endregion
		#region Class ValueEnumerator

		[Serializable]
			private sealed class ValueEnumerator:
			IfeedsFeedEnumerator, IEnumerator {
			#region Private Fields

			private readonly FeedsCollection _dictionary;
			private readonly int _version;
			private int _index;

			#endregion
			#region Internal Constructors

			internal ValueEnumerator(FeedsCollection dictionary) {
				this._dictionary = dictionary;
				this._version = dictionary._version;
				this._index = -1;
			}

			#endregion
			#region Public Properties

			public feedsFeed Current {
				get {
					this._dictionary.CheckEnumIndex(this._index);
					this._dictionary.CheckEnumVersion(this._version);
					return this._dictionary._values[this._index];
				}
			}

			object IEnumerator.Current {
				get { return Current; }
			}

			#endregion
			#region Public Methods

			public bool MoveNext() {
				this._dictionary.CheckEnumVersion(this._version);
				return (++this._index < this._dictionary.Count);
			}

			public void Reset() {
				this._dictionary.CheckEnumVersion(this._version);
				this._index = -1;
			}

			#endregion
		}

		#endregion
		#region Class SyncDictionary

		[Serializable]
			private sealed class SyncDictionary: FeedsCollection {
			#region Private Fields

			private FeedsCollection _dictionary;
			private object _root;

			#endregion
			#region Internal Constructors

			internal SyncDictionary(FeedsCollection dictionary):
				base(Tag.Default) {

				this._dictionary = dictionary;
				this._root = dictionary.SyncRoot;
			}

			#endregion
			#region Public Properties

			public override int Capacity {
				get { lock (this._root) return this._dictionary.Capacity; }
			}

			public override int Count {
				get { lock (this._root) return this._dictionary.Count; }
			}

			public override bool IsFixedSize {
				get { return this._dictionary.IsFixedSize; }
			}

			public override bool IsReadOnly {
				get { return this._dictionary.IsReadOnly; }
			}

			public override bool IsSynchronized {
				get { return true; }
			}

			public override feedsFeed this[String key] {
				get { lock (this._root) return this._dictionary[key]; }
				set { lock (this._root) this._dictionary[key] = value; }
			}

			public override ICollection<string> Keys {
				get { lock (this._root) return this._dictionary.Keys; }
			}

			public override object SyncRoot {
				get { return this._root; }
			}

			public override IfeedsFeedCollection Values {
				get { lock (this._root) return this._dictionary.Values; }
			}

			#endregion
			#region Public Methods

			public override void Add(String key, feedsFeed value) {
				lock (this._root) this._dictionary.Add(key, value);
			}

			public override void Clear() {
				lock (this._root) this._dictionary.Clear();
			}

			public override object Clone() {
				lock (this._root) return this._dictionary.Clone();
			}

			public override bool Contains(String key) {
				lock (this._root) return this._dictionary.Contains(key);
			}

			public override bool ContainsKey(String key) {
				lock (this._root) return this._dictionary.ContainsKey(key);
			}

			public override bool ContainsValue(feedsFeed value) {
				lock (this._root) return this._dictionary.ContainsValue(value);
			}

			public override void CopyTo(FeedEntry[] array, int index) {
				lock (this._root) this._dictionary.CopyTo(array, index);
			}

			public override feedsFeed GetByIndex(int index) {
				lock (this._root) return this._dictionary.GetByIndex(index);
			}

			public override IStringfeedsFeedEnumerator GetEnumerator() {
				lock (this._root) return this._dictionary.GetEnumerator();
			}

			public override String GetKey(int index) {
				lock (this._root) return this._dictionary.GetKey(index);
			}

			public override IList<string> GetKeyList() {
				lock (this._root) return this._dictionary.GetKeyList();
			}

			public override IfeedsFeedList GetValueList() {
				lock (this._root) return this._dictionary.GetValueList();
			}

			public override int IndexOfKey(String key) {
				lock (this._root) return this._dictionary.IndexOfKey(key);
			}

			public override int IndexOfValue(feedsFeed value) {
				lock (this._root) return this._dictionary.IndexOfValue(value);
			}

			public override void Remove(String key) {
				lock (this._root) this._dictionary.Remove(key);
			}

			public override void RemoveAt(int index) {
				lock (this._root) this._dictionary.RemoveAt(index);
			}

			public override void SetByIndex(int index, feedsFeed value) {
				lock (this._root) this._dictionary.SetByIndex(index, value);
			}

			public override void TrimToSize() {
				lock (this._root) this._dictionary.TrimToSize();
			}

			#endregion
		}

		#endregion
	}

	#endregion
}
