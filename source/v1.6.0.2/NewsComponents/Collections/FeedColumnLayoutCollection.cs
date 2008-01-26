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
using System.Collections.Specialized;
using NewsComponents.Feed;
using System.Collections.Generic;

namespace NewsComponents.Collections {
	#region Interface IFeedColumnLayoutCollection

	/// <summary>
	/// Defines size, enumerators, and synchronization methods for strongly
	/// typed collections of <see cref="FeedColumnLayout"/> elements.
	/// </summary>
	/// <remarks>
	/// <b>IFeedColumnLayoutCollection</b> provides an <see cref="ICollection"/>
	/// that is strongly typed for <see cref="FeedColumnLayout"/> elements.
	/// </remarks>

	public interface IFeedColumnLayoutCollection {
		#region Properties
		#region Count

		/// <summary>
		/// Gets the number of elements contained in the
		/// <see cref="IFeedColumnLayoutCollection"/>.
		/// </summary>
		/// <value>The number of elements contained in the
		/// <see cref="IFeedColumnLayoutCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ICollection.Count"/> for details.</remarks>

		int Count { get; }

		#endregion
		#region IsSynchronized

		/// <summary>
		/// Gets a value indicating whether access to the
		/// <see cref="IFeedColumnLayoutCollection"/> is synchronized (thread-safe).
		/// </summary>
		/// <value><c>true</c> if access to the <see cref="IFeedColumnLayoutCollection"/> is
		/// synchronized (thread-safe); otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="ICollection.IsSynchronized"/> for details.</remarks>

		bool IsSynchronized { get; }

		#endregion
		#region SyncRoot

		/// <summary>
		/// Gets an object that can be used to synchronize access
		/// to the <see cref="IFeedColumnLayoutCollection"/>.
		/// </summary>
		/// <value>An object that can be used to synchronize access
		/// to the <see cref="IFeedColumnLayoutCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ICollection.SyncRoot"/> for details.</remarks>

		object SyncRoot { get; }

		#endregion
		#endregion
		#region Methods
		#region CopyTo

		/// <summary>
		/// Copies the entire <see cref="IFeedColumnLayoutCollection"/> to a one-dimensional <see cref="Array"/>
		/// of <see cref="FeedColumnLayout"/> elements, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
		/// <see cref="FeedColumnLayout"/> elements copied from the <see cref="IFeedColumnLayoutCollection"/>.
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
		/// The number of elements in the source <see cref="IFeedColumnLayoutCollection"/> is greater
		/// than the available space from <paramref name="arrayIndex"/> to the end of the destination
		/// <paramref name="array"/>.</para></exception>
		/// <remarks>Please refer to <see cref="ICollection.CopyTo"/> for details.</remarks>

		void CopyTo(FeedColumnLayout[] array, int arrayIndex);

		#endregion
		#region GetEnumerator

		/// <summary>
		/// Returns an <see cref="IFeedColumnLayoutEnumerator"/> that can
		/// iterate through the <see cref="IFeedColumnLayoutCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IFeedColumnLayoutEnumerator"/>
		/// for the entire <see cref="IFeedColumnLayoutCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="IEnumerable.GetEnumerator"/> for details.</remarks>

		IFeedColumnLayoutEnumerator GetEnumerator();

		#endregion
		#endregion
	}

	#endregion
	#region Interface IFeedColumnLayoutList

	/// <summary>
	/// Represents a strongly typed collection of <see cref="FeedColumnLayout"/>
	/// objects that can be individually accessed by index.
	/// </summary>
	/// <remarks>
	/// <b>IFeedColumnLayoutList</b> provides an <see cref="IList"/>
	/// that is strongly typed for <see cref="FeedColumnLayout"/> elements.
	/// </remarks>

	public interface
		IFeedColumnLayoutList: IFeedColumnLayoutCollection {
		#region Properties
		#region IsFixedSize

		/// <summary>
		/// Gets a value indicating whether the <see cref="IFeedColumnLayoutList"/> has a fixed size.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IFeedColumnLayoutList"/> has a fixed size;
		/// otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IList.IsFixedSize"/> for details.</remarks>

		bool IsFixedSize { get; }

		#endregion
		#region IsReadOnly

		/// <summary>
		/// Gets a value indicating whether the <see cref="IFeedColumnLayoutList"/> is read-only.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IFeedColumnLayoutList"/> is read-only;
		/// otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IList.IsReadOnly"/> for details.</remarks>

		bool IsReadOnly { get; }

		#endregion
		#region Item

		/// <summary>
		/// Gets or sets the <see cref="FeedColumnLayout"/> element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="FeedColumnLayout"/> element to get or set.</param>
		/// <value>
		/// The <see cref="FeedColumnLayout"/> element at the specified <paramref name="index"/>.
		/// </value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than
		/// <see cref="IFeedColumnLayoutCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The property is set and the <see cref="IFeedColumnLayoutList"/> is read-only.</exception>
		/// <remarks>Please refer to <see cref="IList.this"/> for details.</remarks>

		FeedColumnLayout this[int index] { get; set; }

		#endregion
		#endregion
		#region Methods
		#region Add

		/// <summary>
		/// Adds a <see cref="FeedColumnLayout"/> to the end
		/// of the <see cref="IFeedColumnLayoutList"/>.
		/// </summary>
		/// <param name="value">The <see cref="FeedColumnLayout"/> object
		/// to be added to the end of the <see cref="IFeedColumnLayoutList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns>The <see cref="IFeedColumnLayoutList"/> index at which
		/// the <paramref name="value"/> has been added.</returns>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IFeedColumnLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IFeedColumnLayoutList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Add"/> for details.</remarks>

		int Add(FeedColumnLayout value);

		#endregion
		#region Clear

		/// <summary>
		/// Removes all elements from the <see cref="IFeedColumnLayoutList"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IFeedColumnLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IFeedColumnLayoutList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Clear"/> for details.</remarks>

		void Clear();

		#endregion
		#region Contains

		/// <summary>
		/// Determines whether the <see cref="IFeedColumnLayoutList"/>
		/// contains the specified <see cref="FeedColumnLayout"/> element.
		/// </summary>
		/// <param name="value">The <see cref="FeedColumnLayout"/> object
		/// to locate in the <see cref="IFeedColumnLayoutList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns><c>true</c> if <paramref name="value"/> is found in the
		/// <see cref="IFeedColumnLayoutList"/>; otherwise, <c>false</c>.</returns>
		/// <remarks>Please refer to <see cref="IList.Contains"/> for details.</remarks>

		bool Contains(FeedColumnLayout value);

		#endregion
		#region IndexOf

		/// <summary>
		/// Returns the zero-based index of the first occurrence of the specified
		/// <see cref="FeedColumnLayout"/> in the <see cref="IFeedColumnLayoutList"/>.
		/// </summary>
		/// <param name="value">The <see cref="FeedColumnLayout"/> object
		/// to locate in the <see cref="IFeedColumnLayoutList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="value"/>
		/// in the <see cref="IFeedColumnLayoutList"/>, if found; otherwise, -1.
		/// </returns>
		/// <remarks>Please refer to <see cref="IList.IndexOf"/> for details.</remarks>

		int IndexOf(FeedColumnLayout value);

		#endregion
		#region Insert

		/// <summary>
		/// Inserts a <see cref="FeedColumnLayout"/> element into the
		/// <see cref="IFeedColumnLayoutList"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which
		/// <paramref name="value"/> should be inserted.</param>
		/// <param name="value">The <see cref="FeedColumnLayout"/> object
		/// to insert into the <see cref="IFeedColumnLayoutList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is greater than
		/// <see cref="IFeedColumnLayoutCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IFeedColumnLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IFeedColumnLayoutList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Insert"/> for details.</remarks>

		void Insert(int index, FeedColumnLayout value);

		#endregion
		#region Remove

		/// <summary>
		/// Removes the first occurrence of the specified <see cref="FeedColumnLayout"/>
		/// from the <see cref="IFeedColumnLayoutList"/>.
		/// </summary>
		/// <param name="value">The <see cref="FeedColumnLayout"/> object
		/// to remove from the <see cref="IFeedColumnLayoutList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IFeedColumnLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IFeedColumnLayoutList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Remove"/> for details.</remarks>

		void Remove(FeedColumnLayout value);

		#endregion
		#region RemoveAt

		/// <summary>
		/// Removes the element at the specified index of the
		/// <see cref="IFeedColumnLayoutList"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than
		/// <see cref="IFeedColumnLayoutCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IFeedColumnLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IFeedColumnLayoutList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.RemoveAt"/> for details.</remarks>

		void RemoveAt(int index);

		#endregion
		#endregion
	}

	#endregion
	#region Interface IFeedColumnLayoutEnumerator

	/// <summary>
	/// Supports type-safe iteration over a collection that
	/// contains <see cref="FeedColumnLayout"/> elements.
	/// </summary>
	/// <remarks>
	/// <b>IFeedColumnLayoutEnumerator</b> provides an <see cref="IEnumerator"/>
	/// that is strongly typed for <see cref="FeedColumnLayout"/> elements.
	/// </remarks>

	public interface IFeedColumnLayoutEnumerator {
		#region Properties
		#region Current

		/// <summary>
		/// Gets the current <see cref="FeedColumnLayout"/> element in the collection.
		/// </summary>
		/// <value>The current <see cref="FeedColumnLayout"/> element in the collection.</value>
		/// <exception cref="InvalidOperationException"><para>The enumerator is positioned
		/// before the first element of the collection or after the last element.</para>
		/// <para>-or-</para>
		/// <para>The collection was modified after the enumerator was created.</para></exception>
		/// <remarks>Please refer to <see cref="IEnumerator.Current"/> for details, but note
		/// that <b>Current</b> fails if the collection was modified since the last successful
		/// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>

		FeedColumnLayout Current { get; }

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
	#region Interface IStringFeedColumnLayoutCollection

	/// <summary>
	/// Defines size, enumerators, and synchronization methods for strongly
	/// typed collections of <see cref="FeedColumnLayoutEntry"/> elements.
	/// </summary>
	/// <remarks>
	/// <b>IStringFeedColumnLayoutCollection</b> provides an <see cref="ICollection"/>
	/// that is strongly typed for <see cref="FeedColumnLayoutEntry"/> elements.
	/// </remarks>

	public interface IStringFeedColumnLayoutCollection {
		#region Properties
		#region Count

		/// <summary>
		/// Gets the number of elements contained in the
		/// <see cref="IStringFeedColumnLayoutCollection"/>.
		/// </summary>
		/// <value>The number of elements contained in the
		/// <see cref="IStringFeedColumnLayoutCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ICollection.Count"/> for details.</remarks>

		int Count { get; }

		#endregion
		#region IsSynchronized

		/// <summary>
		/// Gets a value indicating whether access to the
		/// <see cref="IStringFeedColumnLayoutCollection"/> is synchronized (thread-safe).
		/// </summary>
		/// <value><c>true</c> if access to the <see cref="IStringFeedColumnLayoutCollection"/>
		/// is synchronized (thread-safe); otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="ICollection.IsSynchronized"/> for details.</remarks>

		bool IsSynchronized { get; }

		#endregion
		#region SyncRoot

		/// <summary>
		/// Gets an object that can be used to synchronize access
		/// to the <see cref="IStringFeedColumnLayoutCollection"/>.
		/// </summary>
		/// <value>An object that can be used to synchronize access to the
		/// <see cref="IStringFeedColumnLayoutCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ICollection.SyncRoot"/> for details.</remarks>

		object SyncRoot { get; }

		#endregion
		#endregion
		#region Methods
		#region CopyTo

		/// <summary>
		/// Copies the entire <see cref="IStringFeedColumnLayoutCollection"/>
		/// to a one-dimensional <see cref="Array"/> of <see cref="FeedColumnLayoutEntry"/> elements,
		/// starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the
		/// destination of the <see cref="FeedColumnLayoutEntry"/> elements copied from the
		/// <see cref="IStringFeedColumnLayoutCollection"/>.
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
		/// The number of elements in the source <see cref="IStringFeedColumnLayoutCollection"/>
		/// is greater than the available space from <paramref name="arrayIndex"/> to the end of the
		/// destination <paramref name="array"/>.</para></exception>
		/// <remarks>Please refer to <see cref="ICollection.CopyTo"/> for details.</remarks>

		void CopyTo(FeedColumnLayoutEntry[] array, int arrayIndex);

		#endregion
		#region GetEnumerator

		/// <summary>
		/// Returns an <see cref="IStringFeedColumnLayoutEnumerator"/> that can
		/// iterate through the <see cref="IStringFeedColumnLayoutCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IStringFeedColumnLayoutEnumerator"/>
		/// for the entire <see cref="IStringFeedColumnLayoutCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="IEnumerable.GetEnumerator"/> for details.</remarks>

		IStringFeedColumnLayoutEnumerator GetEnumerator();

		#endregion
		#endregion
	}

	#endregion
	#region Interface IStringFeedColumnLayoutDictionary

	/// <summary>
	/// Represents a strongly typed collection of
	/// <see cref="FeedColumnLayoutEntry"/> key-and-value pairs.
	/// </summary>
	/// <remarks>
	/// <b>IStringFeedColumnLayoutDictionary</b> provides an
	/// <see cref="IDictionary"/> that is strongly typed for
	/// <see cref="String"/> keys and <see cref="FeedColumnLayout"/> values.
	/// </remarks>

	public interface
		IStringFeedColumnLayoutDictionary: IStringFeedColumnLayoutCollection {
		#region Properties
		#region IsFixedSize

		/// <summary>
		/// Gets a value indicating whether the
		/// <see cref="IStringFeedColumnLayoutDictionary"/> has a fixed size.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IStringFeedColumnLayoutDictionary"/>
		/// has a fixed size; otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IDictionary.IsFixedSize"/> for details.</remarks>

		bool IsFixedSize { get; }

		#endregion
		#region IsReadOnly

		/// <summary>
		/// Gets a value indicating whether the
		/// <see cref="IStringFeedColumnLayoutDictionary"/> is read-only.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IStringFeedColumnLayoutDictionary"/>
		/// is read-only; otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IDictionary.IsReadOnly"/> for details.</remarks>

		bool IsReadOnly { get; }

		#endregion
		#region Item

		/// <summary>
		/// Gets or sets the <see cref="FeedColumnLayout"/> value
		/// associated with the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key
		/// whose value to get or set.</param>
		/// <value>The <see cref="FeedColumnLayout"/> value associated with the specified
		/// <paramref name="key"/>. If the specified <paramref name="key"/> is not found,
		/// attempting to get it returns
		/// a null reference,
		/// and attempting to set it creates a new element using the specified
		/// <paramref name="key"/>.</value>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The property is set and the
		/// <see cref="IStringFeedColumnLayoutDictionary"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The property is set, <paramref name="key"/> does not exist in the collection,
		/// and the <b>IStringFeedColumnLayoutDictionary</b> has a fixed size.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="IDictionary.this"/> for details.</remarks>

		FeedColumnLayout this[String key] { get; set; }

		#endregion
		#region Keys

		/// <summary>
        /// Gets an ICollection[string] containing the keys
		/// in the <see cref="IStringFeedColumnLayoutDictionary"/>.
		/// </summary>
        /// <value>An ICollection[string] containing the keys
		/// in the <see cref="IStringFeedColumnLayoutDictionary"/>.</value>
		/// <remarks>Please refer to <see cref="IDictionary.Keys"/> for details.</remarks>

		ICollection<string> Keys { get; }

		#endregion
		#region Values

		/// <summary>
		/// Gets an <see cref="IFeedColumnLayoutCollection"/> containing the values
		/// in the <see cref="IStringFeedColumnLayoutDictionary"/>.
		/// </summary>
		/// <value>An <see cref="IFeedColumnLayoutCollection"/> containing the values
		/// in the <see cref="IStringFeedColumnLayoutDictionary"/>.</value>
		/// <remarks>Please refer to <see cref="IDictionary.Values"/> for details.</remarks>

		IFeedColumnLayoutCollection Values { get; }

		#endregion
		#endregion
		#region Methods
		#region Add

		/// <summary>
		/// Adds an element with the specified <see cref="String"/>
		/// key and <see cref="FeedColumnLayout"/> value to the
		/// <see cref="IStringFeedColumnLayoutDictionary"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key of the element
		/// to add to the <see cref="IStringFeedColumnLayoutDictionary"/>.</param>
		/// <param name="value">The <see cref="FeedColumnLayout"/> value of the element
		/// to add to the <see cref="IStringFeedColumnLayoutDictionary"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentException">
		/// <para>An element with the specified <paramref name="key"/> already exists
		/// in the <see cref="IStringFeedColumnLayoutDictionary"/>.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringFeedColumnLayoutDictionary</b> is set to use the
		/// <see cref="IComparable"/> interface, and <paramref name="key"/> does not
		/// implement the <b>IComparable</b> interface.</para></exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringFeedColumnLayoutDictionary"/> is read-only.
		/// </para><para>-or-</para>
		/// <para>The <b>IStringFeedColumnLayoutDictionary</b> has a fixed size.
		/// </para></exception>
		/// <remarks>Please refer to <see cref="IDictionary.Add"/> for details.</remarks>

		void Add(String key, FeedColumnLayout value);

		#endregion
		#region Clear

		/// <summary>
		/// Removes all elements from the <see cref="IStringFeedColumnLayoutDictionary"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringFeedColumnLayoutDictionary"/> is read-only.
		/// </para><para>-or-</para>
		/// <para>The <b>IStringFeedColumnLayoutDictionary</b> has a fixed size.
		/// </para></exception>
		/// <remarks>Please refer to <see cref="IDictionary.Clear"/> for details.</remarks>

		void Clear();

		#endregion
		#region Contains

		/// <summary>
		/// Determines whether the <see cref="IStringFeedColumnLayoutDictionary"/>
		/// contains the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key to locate
		/// in the <see cref="IStringFeedColumnLayoutDictionary"/>.</param>
		/// <returns><c>true</c> if the <see cref="IStringFeedColumnLayoutDictionary"/>
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
		/// from the <see cref="IStringFeedColumnLayoutDictionary"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key of the element to remove
		/// from the <see cref="IStringFeedColumnLayoutDictionary"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringFeedColumnLayoutDictionary"/> is read-only.
		/// </para><para>-or-</para>
		/// <para>The <b>IStringFeedColumnLayoutDictionary</b> has a fixed size.
		/// </para></exception>
		/// <remarks>Please refer to <see cref="IDictionary.Remove"/> for details.</remarks>

		void Remove(String key);

		#endregion
		#endregion
	}

	#endregion
	#region Interface IStringFeedColumnLayoutList

	/// <summary>
	/// Represents a strongly typed collection of <see cref="FeedColumnLayoutEntry"/>
	/// objects that can be individually accessed by index.
	/// </summary>
	/// <remarks>
	/// <b>IStringFeedColumnLayoutList</b> provides an <see cref="IList"/>
	/// that is strongly typed for <see cref="FeedColumnLayoutEntry"/> elements.
	/// </remarks>

	public interface
		IStringFeedColumnLayoutList: IStringFeedColumnLayoutCollection {
		#region Properties
		#region IsFixedSize

		/// <summary>
		/// Gets a value indicating whether the
		/// <see cref="IStringFeedColumnLayoutList"/> has a fixed size.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IStringFeedColumnLayoutList"/>
		/// has a fixed size; otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IList.IsFixedSize"/> for details.</remarks>

		bool IsFixedSize { get; }

		#endregion
		#region IsReadOnly

		/// <summary>
		/// Gets a value indicating whether the
		/// <see cref="IStringFeedColumnLayoutList"/> is read-only.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IStringFeedColumnLayoutList"/>
		/// is read-only; otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IList.IsReadOnly"/> for details.</remarks>

		bool IsReadOnly { get; }

		#endregion
		#region Item

		/// <summary>
		/// Gets or sets the <see cref="FeedColumnLayoutEntry"/> element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="FeedColumnLayoutEntry"/> element to get or set.</param>
		/// <value>
		/// The <see cref="FeedColumnLayoutEntry"/> element at the specified <paramref name="index"/>.
		/// </value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than
		/// <see cref="IStringFeedColumnLayoutCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">The property is set and the
		/// <see cref="IStringFeedColumnLayoutList"/> is read-only.</exception>
		/// <remarks>Please refer to <see cref="IList.this"/> for details.</remarks>

		FeedColumnLayoutEntry this[int index] { get; set; }

		#endregion
		#endregion
		#region Methods
		#region Add

		/// <summary>
		/// Adds a <see cref="FeedColumnLayoutEntry"/> to the end
		/// of the <see cref="IStringFeedColumnLayoutList"/>.
		/// </summary>
		/// <param name="entry">The <see cref="FeedColumnLayoutEntry"/> object
		/// to be added to the end of the <see cref="IStringFeedColumnLayoutList"/>.
		/// </param>
		/// <returns>The <see cref="IStringFeedColumnLayoutList"/> index at which
		/// the <paramref name="entry"/> has been added.</returns>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringFeedColumnLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringFeedColumnLayoutList</b> has a fixed size.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="IList.Add"/> for details.</remarks>

		int Add(FeedColumnLayoutEntry entry);

		#endregion
		#region Clear

		/// <summary>
		/// Removes all elements from the <see cref="IStringFeedColumnLayoutList"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringFeedColumnLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringFeedColumnLayoutList</b> has a fixed size.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="IList.Clear"/> for details.</remarks>

		void Clear();

		#endregion
		#region Contains

		/// <summary>
		/// Determines whether the <see cref="IStringFeedColumnLayoutList"/>
		/// contains the specified <see cref="FeedColumnLayoutEntry"/> element.
		/// </summary>
		/// <param name="entry">The <see cref="FeedColumnLayoutEntry"/> object
		/// to locate in the <see cref="IStringFeedColumnLayoutList"/>.</param>
		/// <returns><c>true</c> if <paramref name="entry"/> is found in the
		/// <see cref="IStringFeedColumnLayoutList"/>; otherwise, <c>false</c>.</returns>
		/// <remarks>Please refer to <see cref="IList.Contains"/> for details.</remarks>

		bool Contains(FeedColumnLayoutEntry entry);

		#endregion
		#region IndexOf

		/// <summary>
		/// Returns the zero-based index of the first occurrence of the specified
		/// <see cref="FeedColumnLayoutEntry"/> in the <see cref="IStringFeedColumnLayoutList"/>.
		/// </summary>
		/// <param name="entry">The <see cref="FeedColumnLayoutEntry"/> object
		/// to locate in the <see cref="IStringFeedColumnLayoutList"/>.</param>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="entry"/>
		/// in the <see cref="IStringFeedColumnLayoutList"/>, if found; otherwise, -1.
		/// </returns>
		/// <remarks>Please refer to <see cref="IList.IndexOf"/> for details.</remarks>

		int IndexOf(FeedColumnLayoutEntry entry);

		#endregion
		#region Insert

		/// <summary>
		/// Inserts a <see cref="FeedColumnLayoutEntry"/> element into the
		/// <see cref="IStringFeedColumnLayoutList"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which
		/// <paramref name="entry"/> should be inserted.</param>
		/// <param name="entry">The <see cref="FeedColumnLayoutEntry"/> object to insert
		/// into the <see cref="IStringFeedColumnLayoutList"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is greater than
		/// <see cref="IStringFeedColumnLayoutCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringFeedColumnLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringFeedColumnLayoutList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Insert"/> for details.</remarks>

		void Insert(int index, FeedColumnLayoutEntry entry);

		#endregion
		#region Remove

		/// <summary>
		/// Removes the first occurrence of the specified <see cref="FeedColumnLayoutEntry"/>
		/// from the <see cref="IStringFeedColumnLayoutList"/>.
		/// </summary>
		/// <param name="entry">The <see cref="FeedColumnLayoutEntry"/> object to remove
		/// from the <see cref="IStringFeedColumnLayoutList"/>.</param>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringFeedColumnLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringFeedColumnLayoutList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Remove"/> for details.</remarks>

		void Remove(FeedColumnLayoutEntry entry);

		#endregion
		#region RemoveAt

		/// <summary>
		/// Removes the element at the specified index of the
		/// <see cref="IStringFeedColumnLayoutList"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than
		/// <see cref="IStringFeedColumnLayoutCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringFeedColumnLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringFeedColumnLayoutList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.RemoveAt"/> for details.</remarks>

		void RemoveAt(int index);

		#endregion
		#endregion
	}

	#endregion
	#region Interface IStringFeedColumnLayoutEnumerator

	/// <summary>
	/// Supports type-safe iteration over a dictionary that
	/// contains <see cref="FeedColumnLayoutEntry"/> elements.
	/// </summary>
	/// <remarks>
	/// <b>IStringFeedColumnLayoutEnumerator</b> provides an
	/// <see cref="IDictionaryEnumerator"/> that is strongly typed for
	/// <see cref="String"/> keys and <see cref="FeedColumnLayout"/> values.
	/// </remarks>

	public interface IStringFeedColumnLayoutEnumerator {
		#region Properties
		#region Current

		/// <summary>
		/// Gets the current <see cref="FeedColumnLayoutEntry"/> element in the collection.
		/// </summary>
		/// <value>The current <see cref="FeedColumnLayoutEntry"/> element in the collection.</value>
		/// <exception cref="InvalidOperationException"><para>The enumerator is positioned
		/// before the first element of the collection or after the last element.</para>
		/// <para>-or-</para>
		/// <para>The collection was modified after the enumerator was created.</para></exception>
		/// <remarks>Please refer to <see cref="IEnumerator.Current"/> for details, but note
		/// that <b>Current</b> fails if the collection was modified since the last successful
		/// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>

		FeedColumnLayoutEntry Current { get; }

		#endregion
		#region Entry

		/// <summary>
		/// Gets a <see cref="FeedColumnLayoutEntry"/> containing both
		/// the key and the value of the current dictionary entry.
		/// </summary>
		/// <value>A <see cref="FeedColumnLayoutEntry"/> containing both
		/// the key and the value of the current dictionary entry.</value>
		/// <exception cref="InvalidOperationException"><para>The enumerator is positioned
		/// before the first element of the dictionary or after the last element.</para>
		/// <para>-or-</para>
		/// <para>The dictionary was modified after the enumerator was created.</para></exception>
		/// <remarks>Please refer to <see cref="IDictionaryEnumerator.Entry"/> for details, but
		/// note that <b>Entry</b> fails if the collection was modified since the last successful
		/// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>

		FeedColumnLayoutEntry Entry { get; }

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
		/// Gets the <see cref="FeedColumnLayout"/> value of the current dictionary entry.
		/// </summary>
		/// <value>The <see cref="FeedColumnLayout"/> value
		/// of the current element of the enumeration.</value>
		/// <exception cref="InvalidOperationException"><para>The enumerator is positioned
		/// before the first element of the dictionary or after the last element.</para>
		/// <para>-or-</para>
		/// <para>The dictionary was modified after the enumerator was created.</para></exception>
		/// <remarks>Please refer to <see cref="IDictionaryEnumerator.Value"/> for details, but
		/// note that <b>Value</b> fails if the collection was modified since the last successful
		/// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>

		FeedColumnLayout Value { get; }

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
	#region Struct FeedColumnLayoutEntry

	/// <summary>
	/// Implements a strongly typed pair of one <see cref="String"/>
	/// key and one <see cref="FeedColumnLayout"/> value.
	/// </summary>
	/// <remarks>
	/// <b>FeedColumnLayoutEntry</b> provides a <see cref="DictionaryEntry"/> that is strongly
	/// typed for <see cref="String"/> keys and <see cref="FeedColumnLayout"/> values.
	/// </remarks>

	[Serializable]
	public struct FeedColumnLayoutEntry {
		#region Private Fields

		private String _key;
		private FeedColumnLayout _value;

		#endregion
		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedColumnLayoutEntry"/>
		/// class with the specified key and value.
		/// </summary>
		/// <param name="key">
		/// The <see cref="String"/> key in the key-and-value pair.</param>
		/// <param name="value">
		/// The <see cref="FeedColumnLayout"/> value in the key-and-value pair.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>

		public FeedColumnLayoutEntry(String key, FeedColumnLayout value) {
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
		/// <see cref="FeedColumnLayoutEntry"/> is a value type and therefore has an implicit default
		/// constructor that zeroes all data members. This means that the <b>Key</b> property of
		/// a default-constructed <b>FeedColumnLayoutEntry</b> contains a null reference by default,
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
		/// Gets or sets the <see cref="FeedColumnLayout"/> value in the key-and-value pair.
		/// </summary>
		/// <value>
		/// The <see cref="FeedColumnLayout"/> value in the key-and-value pair.
		/// This value can be a null reference, which is also the default.
		/// </value>

		public FeedColumnLayout Value {
			get { return this._value; }
			set { this._value = value; }
		}

		#endregion
		#endregion
		#region Public Operators
		#region FeedColumnLayoutEntry(DictionaryEntry)

		/// <summary>
		/// Converts a <see cref="DictionaryEntry"/> to a <see cref="FeedColumnLayoutEntry"/>.
		/// </summary>
		/// <param name="entry">A <see cref="DictionaryEntry"/> object to convert.</param>
		/// <returns>A <see cref="FeedColumnLayoutEntry"/> object that represents
		/// the converted <paramref name="entry"/>.</returns>
		/// <exception cref="InvalidCastException">
		/// <para><paramref name="entry"/> contains a key that is not compatible
		/// with <see cref="String"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entry"/> contains a value that is not compatible
		/// with <see cref="FeedColumnLayout"/>.</para>
		/// </exception>

		public static implicit operator FeedColumnLayoutEntry(DictionaryEntry entry) {
			FeedColumnLayoutEntry pair = new FeedColumnLayoutEntry();
			if (entry.Key != null) pair.Key = (String) entry.Key;
			if (entry.Value != null) pair.Value = (FeedColumnLayout) entry.Value;
			return pair;
		}

		#endregion
		#region DictionaryEntry(FeedColumnLayoutEntry)

		/// <summary>
		/// Converts a <see cref="FeedColumnLayoutEntry"/> to a <see cref="DictionaryEntry"/>.
		/// </summary>
		/// <param name="pair">A <see cref="FeedColumnLayoutEntry"/> object to convert.</param>
		/// <returns>A <see cref="DictionaryEntry"/> object that
		/// represents the converted <paramref name="pair"/>.</returns>

		public static implicit operator DictionaryEntry(FeedColumnLayoutEntry pair) {
			DictionaryEntry entry = new DictionaryEntry();
			if (pair.Key != null) entry.Key = pair.Key;
			entry.Value = pair.Value;
			return entry;
		}

		#endregion
		#endregion
	}

	#endregion
	#region Class FeedColumnLayoutCollection

	/// <summary>
	/// Implements a strongly typed collection of <see cref="FeedColumnLayoutEntry"/> key-and-value
	/// pairs that retain their insertion order and are accessible by index and by key.
	/// </summary>
	/// <remarks><para>
	/// <b>FeedColumnLayoutCollection</b> provides an <see cref="ArrayList"/> that is strongly
	/// typed for <see cref="FeedColumnLayoutEntry"/> elements and allows direct access to
	/// its <see cref="String"/> keys and <see cref="FeedColumnLayout"/> values.
	/// </para><para>
	/// The collection may contain multiple identical keys. All key access methods return the
	/// first occurrence of the specified key, if found. Access by index is an O(1) operation
	/// but access by key or value are both O(<em>N</em>) operations, where <em>N</em> is the
	/// current value of the <see cref="FeedColumnLayoutCollection.Count"/> property.
	/// </para></remarks>

	[Serializable]
	public class FeedColumnLayoutCollection:
		IStringFeedColumnLayoutList, IList, ICloneable {
		#region Private Fields

		private const int _defaultCapacity = 16;

		private String[] _keys;
		private FeedColumnLayout[] _values;
		private int _count;

		[NonSerialized]
		private int _version;
		private KeyList _keyList;
		private ValueList _valueList;

		#endregion
		#region Private Constructors

		// helper type to identify private ctor
		private enum Tag { Default }

		private FeedColumnLayoutCollection(Tag tag) { }

		#endregion
		#region Public Constructors
		#region FeedColumnLayoutCollection()

		/// <overloads>
		/// Initializes a new instance of the <see cref="FeedColumnLayoutCollection"/> class.
		/// </overloads>
		/// <summary>
		/// Initializes a new instance of the <see cref="FeedColumnLayoutCollection"/> class
		/// that is empty and has the default initial capacity.
		/// </summary>
		/// <remarks>Please refer to <see cref="ArrayList()"/> for details.</remarks>

		public FeedColumnLayoutCollection() {
			this._keys = new String[_defaultCapacity];
			this._values = new FeedColumnLayout[_defaultCapacity];
		}

		#endregion
		#region FeedColumnLayoutCollection(Int32)

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedColumnLayoutCollection"/> class
		/// that is empty and has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">The number of elements that the new
		/// <see cref="FeedColumnLayoutCollection"/> is initially capable of storing.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="capacity"/> is less than zero.</exception>
		/// <remarks>Please refer to <see cref="ArrayList(Int32)"/> for details.</remarks>

		public FeedColumnLayoutCollection(int capacity) {
			if (capacity < 0)
				throw new ArgumentOutOfRangeException("capacity",
					capacity, "Argument cannot be negative.");

			this._keys = new String[capacity];
			this._values = new FeedColumnLayout[capacity];
		}

		#endregion
		#region FeedColumnLayoutCollection(FeedColumnLayoutCollection)

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedColumnLayoutCollection"/> class
		/// that contains elements copied from the specified collection and
		/// that has the same initial capacity as the number of elements copied.
		/// </summary>
		/// <param name="collection">The <see cref="FeedColumnLayoutCollection"/>
		/// whose elements are copied to the new collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="collection"/> is a null reference.</exception>
		/// <remarks>Please refer to <see cref="ArrayList(ICollection)"/> for details.</remarks>

		public FeedColumnLayoutCollection(FeedColumnLayoutCollection collection) {
			if (collection == null)
				throw new ArgumentNullException("collection");

			this._keys = new String[collection.Count];
			this._values = new FeedColumnLayout[collection.Count];
			AddRange(collection);
		}

		#endregion
		#region FeedColumnLayoutCollection(FeedColumnLayoutEntry[])

		/// <summary>
		/// Initializes a new instance of the <see cref="FeedColumnLayoutCollection"/> class
		/// that contains elements copied from the specified <see cref="FeedColumnLayoutEntry"/>
		/// array and that has the same initial capacity as the number of elements copied.
		/// </summary>
		/// <param name="array">An <see cref="Array"/> of <see cref="FeedColumnLayoutEntry"/>
		/// elements that are copied to the new collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is a null reference.</exception>
		/// <remarks>Please refer to <see cref="ArrayList(ICollection)"/> for details.</remarks>

		public FeedColumnLayoutCollection(FeedColumnLayoutEntry[] array) {
			if (array == null)
				throw new ArgumentNullException("array");

			this._keys = new String[array.Length];
			this._values = new FeedColumnLayout[array.Length];
			AddRange(array);
		}

		#endregion
		#endregion
		#region Protected Properties
		#region InnerKeys
        
		/// <summary>
		/// Gets the list of keys contained in the <see cref="FeedColumnLayoutCollection"/> instance.
		/// </summary>
		/// <value>
		/// A one-dimensional <see cref="Array"/> with zero-based indexing that contains all 
		/// <see cref="String"/> elements in the <see cref="FeedColumnLayoutCollection"/>.
		/// </value>
		/// <remarks>
		/// Use <b>InnerKeys</b> to access the key array of a <see cref="FeedColumnLayoutCollection"/>
		/// instance that might be a read-only or synchronized wrapper. This is necessary 
		/// because the key and value arrays of wrapper classes are always null references.
		/// </remarks>

		protected virtual String[] InnerKeys {
			get { return this._keys; }
		}

		#endregion
		#region InnerValues
        
		/// <summary>
		/// Gets the list of values contained in the <see cref="FeedColumnLayoutCollection"/> instance.
		/// </summary>
		/// <value>
		/// A one-dimensional <see cref="Array"/> with zero-based indexing that contains all 
		/// <see cref="FeedColumnLayout"/> elements in the <see cref="FeedColumnLayoutCollection"/>.
		/// </value>
		/// <remarks>
		/// Use <b>InnerValues</b> to access the value array of a <see cref="FeedColumnLayoutCollection"/>
		/// instance that might be a read-only or synchronized wrapper. This is necessary
		/// because the key and value arrays of wrapper classes are always null references.
		/// </remarks>

		protected virtual FeedColumnLayout[] InnerValues {
			get { return this._values; }
		}

		#endregion
		#endregion
		#region Public Properties
		#region Capacity

		/// <summary>
		/// Gets or sets the capacity of the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <value>The number of elements that the
		/// <see cref="FeedColumnLayoutCollection"/> can contain.</value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <b>Capacity</b> is set to a value that is less than <see cref="Count"/>.</exception>
		/// <remarks>Please refer to <see cref="ArrayList.Capacity"/> for details.</remarks>

		public virtual int Capacity {
			get { return this._keys.Length; }
			set {
				if (value == this._keys.Length) return;

				if (value < this._count)
					throw new ArgumentOutOfRangeException("Capacity",
						value, "Value cannot be less than Count.");

				if (value == 0) {
					this._keys = new String[_defaultCapacity];
					this._values = new FeedColumnLayout[_defaultCapacity];
					return;
				}

				String[] newKeys = new String[value];
				FeedColumnLayout[] newValues = new FeedColumnLayout[value];

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
		/// in the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <value>The number of key-and-value pairs contained
		/// in the <see cref="FeedColumnLayoutCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ArrayList.Count"/> for details.</remarks>

		public virtual int Count {
			get { return this._count; }
		}

		#endregion
		#region IsFixedSize

		/// <summary>
		/// Gets a value indicating whether the <see cref="FeedColumnLayoutCollection"/> has a fixed size.
		/// </summary>
		/// <value><c>true</c> if the <see cref="FeedColumnLayoutCollection"/> has a fixed size;
		/// otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="ArrayList.IsFixedSize"/> for details.</remarks>

		public virtual bool IsFixedSize {
			get { return false; }
		}

		#endregion
		#region IsReadOnly

		/// <summary>
		/// Gets a value indicating whether the <see cref="FeedColumnLayoutCollection"/> is read-only.
		/// </summary>
		/// <value><c>true</c> if the <see cref="FeedColumnLayoutCollection"/> is read-only;
		/// otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="ArrayList.IsReadOnly"/> for details.</remarks>

		public virtual bool IsReadOnly {
			get { return false; }
		}

		#endregion
		#region IsSynchronized

		/// <summary>
		/// Gets a value indicating whether access to the <see cref="FeedColumnLayoutCollection"/>
		/// is synchronized (thread-safe).
		/// </summary>
		/// <value><c>true</c> if access to the <see cref="FeedColumnLayoutCollection"/> is
		/// synchronized (thread-safe); otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="ArrayList.IsSynchronized"/> for details.</remarks>

		public virtual bool IsSynchronized {
			get { return false; }
		}

		#endregion
		#region Item: FeedColumnLayoutEntry

		/// <summary>
		/// Gets or sets the <see cref="FeedColumnLayoutEntry"/> element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="FeedColumnLayoutEntry"/> element to get or set.</param>
		/// <value>
		/// The <see cref="FeedColumnLayoutEntry"/> element at the specified <paramref name="index"/>.
		/// </value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The property is set and the <see cref="FeedColumnLayoutCollection"/> is read-only.</exception>
		/// <remarks>Please refer to <see cref="ArrayList.this"/> for details.</remarks>

		public virtual FeedColumnLayoutEntry this[int index] {
			get {
				ValidateIndex(index);
				return new FeedColumnLayoutEntry(this._keys[index], this._values[index]);
			}
			set {
				ValidateIndex(index);
				++this._version;
				this._keys[index] = value.Key;
				this._values[index] = value.Value;
			}
		}

		#endregion
		#region IList.Item: Object

		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <value>
		/// The element at the specified <paramref name="index"/>, returned as a boxed
		/// <see cref="DictionaryEntry"/> value. When the property is set, this value 
		/// must be compatible with <see cref="FeedColumnLayoutEntry"/>.
		/// </value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <exception cref="InvalidCastException">The property is set to a value
		/// that is not compatible with <see cref="FeedColumnLayoutEntry"/>.</exception>
		/// <exception cref="NotSupportedException">
		/// The property is set and the <see cref="FeedColumnLayoutCollection"/> is read-only.</exception>
		/// <remarks>Please refer to <see cref="ArrayList.this"/> for details.</remarks>
		object IList.this[int index] {
			get { return (DictionaryEntry) this[index]; }
			set { this[index] = (FeedColumnLayoutEntry) (DictionaryEntry) value; }
		}

		#endregion
		#region Keys

		/// <summary>
        /// Gets an ICollection[string] containing
		/// the keys in the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
        /// <value>An ICollection[string] containing
		/// the keys in the <see cref="FeedColumnLayoutCollection"/>.</value>
		/// <remarks>Please refer to <see cref="SortedList.Keys"/> for details.</remarks>

		public virtual ICollection<string> Keys {
			get { return GetKeyList(); }
		}

		#endregion
		#region SyncRoot

		/// <summary>
		/// Gets an object that can be used to synchronize
		/// access to the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <value>An object that can be used to synchronize
		/// access to the <see cref="FeedColumnLayoutCollection"/>.</value>
		/// <remarks>Please refer to <see cref="SortedList.SyncRoot"/> for details.</remarks>

		public virtual object SyncRoot {
			get { return this; }
		}

		#endregion
		#region Values

		/// <summary>
		/// Gets an <see cref="IFeedColumnLayoutCollection"/> containing
		/// the values in the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <value>An <see cref="IFeedColumnLayoutCollection"/> containing
		/// the values in the <see cref="FeedColumnLayoutCollection"/>.</value>
		/// <remarks>Please refer to <see cref="SortedList.Values"/> for details.</remarks>

		public virtual IFeedColumnLayoutCollection Values {
			get { return GetValueList(); }
		}

		#endregion
		#endregion
		#region Public Methods
		#region Similar Layout methods
		/// <summary>
		/// Returns the first similar FeedColumnLayout by calling 
		/// FeedColumnLayout.Equals(layout, true) - ignoring the
		/// ColumnWidths.
		/// </summary>
		/// <param name="layout">FeedColumnLayout</param>
		/// <returns>int index or -1 if not found</returns>
		public int IndexOfSimilar(FeedColumnLayout layout)
		{ 
			for (int i = 0; i < this._count; ++i)
				if (this._values[i] != null && this._values[i].Equals(layout, true))
					return i;
			return -1;
		}
		#endregion
		#region Add(FeedColumnLayoutEntry)

		/// <overloads>
		/// Adds an element to the end of the <see cref="FeedColumnLayoutCollection"/>.
		/// </overloads>
		/// <summary>
		/// Adds a <see cref="FeedColumnLayoutEntry"/> to the end of the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <param name="entry">The <see cref="FeedColumnLayoutEntry"/> object
		/// to be added to the end of the <see cref="FeedColumnLayoutCollection"/>.</param>
		/// <returns>The <see cref="FeedColumnLayoutCollection"/> index at which the
		/// <paramref name="entry"/> has been added.</returns>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedColumnLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedColumnLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.Add"/> for details.</remarks>

		public virtual int Add(FeedColumnLayoutEntry entry) {
			if (this._count == this._keys.Length)
				EnsureCapacity(this._count + 1);

			++this._version;
			this._keys[this._count] = entry.Key;
			this._values[this._count] = entry.Value;
			return this._count++;
		}

		#endregion
		#region Add(String, FeedColumnLayout)

		/// <summary>
		/// Adds an element with the specified <see cref="String"/> key and
		/// <see cref="FeedColumnLayout"/> value to the end of the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key of the element
		/// to add to the end of the <see cref="FeedColumnLayoutCollection"/>.</param>
		/// <param name="value">The <see cref="FeedColumnLayout"/> value of the element
		/// to add to the end of the <see cref="FeedColumnLayoutCollection"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns>The <see cref="FeedColumnLayoutCollection"/> index at which the
		/// new element has been added.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedColumnLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedColumnLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.Add"/> for details but note that
		/// the <see cref="FeedColumnLayoutCollection"/> may contain multiple identical keys.</remarks>

		public int Add(String key, FeedColumnLayout value) {
			if ((object) key == null)
				throw new ArgumentNullException("key");

			return Add(new FeedColumnLayoutEntry(key, value));
		}

		#endregion
		#region IList.Add(Object)

		/// <summary>
		/// Adds an <see cref="Object"/> to the end of the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <param name="entry">
		/// The object to be added to the end of the <see cref="FeedColumnLayoutCollection"/>.
		/// This argument must be compatible with <see cref="FeedColumnLayoutEntry"/>.</param>
		/// <returns>The <see cref="FeedColumnLayoutCollection"/> index at which the
		/// <paramref name="entry"/> has been added.</returns>
		/// <exception cref="InvalidCastException"><paramref name="entry"/>
		/// is not compatible with <see cref="FeedColumnLayoutEntry"/>.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedColumnLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedColumnLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.Add"/> for details.</remarks>

		int IList.Add(object entry) {
			return Add((FeedColumnLayoutEntry) entry);
		}

		#endregion
		#region AddRange(FeedColumnLayoutCollection)

		/// <overloads>
		/// Adds a range of elements to the end of the <see cref="FeedColumnLayoutCollection"/>.
		/// </overloads>
		/// <summary>
		/// Adds the elements of another collection to the end of the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <param name="collection">The <see cref="FeedColumnLayoutCollection"/> whose elements
		/// should be added to the end of the current collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="collection"/> is a null reference.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedColumnLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedColumnLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.AddRange"/> for details.</remarks>

		public virtual void AddRange(FeedColumnLayoutCollection collection) {
			if (collection == null)
				throw new ArgumentNullException("collection");

			if (collection.Count == 0) return;
			if (this._count + collection.Count > this._keys.Length)
				EnsureCapacity(this._count + collection.Count);

			++this._version;
			Array.Copy(collection.InnerKeys, 0, this._keys, this._count, collection.Count);
			Array.Copy(collection.InnerValues, 0, this._values, this._count, collection.Count);
			this._count += collection.Count;
		}

		#endregion
		#region AddRange(FeedColumnLayoutEntry[])

		/// <summary>
		/// Adds the elements of a <see cref="FeedColumnLayoutEntry"/> array
		/// to the end of the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <param name="array">An <see cref="Array"/> of <see cref="FeedColumnLayoutEntry"/> elements
		/// that should be added to the end of the <see cref="FeedColumnLayoutCollection"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is a null reference.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedColumnLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedColumnLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.AddRange"/> for details.</remarks>

		public virtual void AddRange(FeedColumnLayoutEntry[] array) {
			if (array == null)
				throw new ArgumentNullException("array");

			if (array.Length == 0) return;
			if (this._count + array.Length > this._keys.Length)
				EnsureCapacity(this._count + array.Length);

			++this._version;
			for (int i = 0; i < array.Length; ++i, ++this._count) {
				this._keys[this._count] = array[i].Key;
				this._values[this._count] = array[i].Value;
			}
		}

		#endregion
		#region Clear

		/// <summary>
		/// Removes all elements from the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedColumnLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedColumnLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.Clear"/> for details.</remarks>

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
		/// Creates a shallow copy of the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <returns>A shallow copy of the <see cref="FeedColumnLayoutCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="ArrayList.Clone"/> for details.</remarks>

		public virtual object Clone() {
			FeedColumnLayoutCollection collection = new FeedColumnLayoutCollection(this._count);

			Array.Copy(this._keys, 0, collection._keys, 0, this._count);
			Array.Copy(this._values, 0, collection._values, 0, this._count);

			collection._count = this._count;
			collection._version = this._version;

			return collection;
		}

		#endregion
		#region Contains(FeedColumnLayoutEntry)

		/// <summary>
		/// Determines whether the <see cref="FeedColumnLayoutCollection"/>
		/// contains the specified <see cref="FeedColumnLayoutEntry"/> element.
		/// </summary>
		/// <param name="entry">The <see cref="FeedColumnLayoutEntry"/> object
		/// to locate in the <see cref="FeedColumnLayoutCollection"/>.</param>
		/// <returns><c>true</c> if <paramref name="entry"/> is found in the
		/// <see cref="FeedColumnLayoutCollection"/>; otherwise, <c>false</c>.</returns>
		/// <remarks><para>
		/// Please refer to <see cref="ArrayList.Contains"/> for details.
		/// </para><para>
		/// <b>Contains</b> uses the equality operators defined by <see cref="String"/>
		/// and <see cref="FeedColumnLayout"/> to locate the specified <paramref name="entry"/>.
		/// </para></remarks>

		public virtual bool Contains(FeedColumnLayoutEntry entry) {
			return (IndexOf(entry) >= 0);
		}

		#endregion
		#region IList.Contains(Object)

		/// <summary>
		/// Determines whether the <see cref="FeedColumnLayoutCollection"/> contains the specified element.
		/// </summary>
		/// <param name="entry">The object to locate in the <see cref="FeedColumnLayoutCollection"/>.
		/// This argument must be compatible with <see cref="FeedColumnLayoutEntry"/>.</param>
		/// <returns><c>true</c> if <paramref name="entry"/> is found in the
		/// <see cref="FeedColumnLayoutCollection"/>; otherwise, <c>false</c>.</returns>
		/// <exception cref="InvalidCastException"><paramref name="entry"/>
		/// is not compatible with <see cref="FeedColumnLayoutEntry"/>.</exception>
		/// <remarks>Please refer to <see cref="ArrayList.Contains"/> for details.</remarks>

		bool IList.Contains(object entry) {
			return Contains((FeedColumnLayoutEntry) entry);
		}

		#endregion
		#region ContainsKey

		/// <summary>
		/// Determines whether the <see cref="FeedColumnLayoutCollection"/>
		/// contains the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key
		/// to locate in the <see cref="FeedColumnLayoutCollection"/>.</param>
		/// <returns><c>true</c> if the <see cref="FeedColumnLayoutCollection"/> contains an element
		/// with the specified <paramref name="key"/>; otherwise, <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <remarks>Please refer to <see cref="SortedList.ContainsKey"/> for details but note
		/// that the <see cref="FeedColumnLayoutCollection"/> requires linear time to locate a key.</remarks>

		public virtual bool ContainsKey(String key) {
			return (IndexOfKey(key) >= 0);
		}

		#endregion
		#region ContainsValue

		/// <summary>
		/// Determines whether the <see cref="FeedColumnLayoutCollection"/>
		/// contains the specified <see cref="FeedColumnLayout"/> value.
		/// </summary>
		/// <param name="value">The <see cref="FeedColumnLayout"/> value
		/// to locate in the <see cref="FeedColumnLayoutCollection"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns><c>true</c> if the <see cref="FeedColumnLayoutCollection"/> contains an element
		/// with the specified <paramref name="value"/>; otherwise, <c>false</c>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.ContainsValue"/> for details.</remarks>

		public virtual bool ContainsValue(FeedColumnLayout value) {
			return (IndexOfValue(value) >= 0);
		}

		#endregion
		#region CopyTo(FeedColumnLayoutEntry[], Int32)

		/// <summary>
		/// Copies the entire <see cref="FeedColumnLayoutCollection"/> to a one-dimensional <see cref="Array"/> of
		/// <see cref="FeedColumnLayoutEntry"/> elements, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
		/// <see cref="FeedColumnLayoutEntry"/> elements copied from the <see cref="FeedColumnLayoutCollection"/>.
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
		/// The number of elements in the source <see cref="FeedColumnLayoutCollection"/> is greater than
		/// the available space from <paramref name="arrayIndex"/> to the end of the destination
		/// <paramref name="array"/>.</para></exception>
		/// <remarks>Please refer to ArrayList.CopyTo for details.</remarks>

		public virtual void CopyTo(FeedColumnLayoutEntry[] array, int arrayIndex) {
			CheckTargetArray(array, arrayIndex);

			for (int i = 0; i < this._count; i++) {
				FeedColumnLayoutEntry entry =
					new FeedColumnLayoutEntry(this._keys[i], this._values[i]);

				array.SetValue(entry, arrayIndex + i);
			}
		}

		#endregion
		#region ICollection.CopyTo(Array, Int32)

		/// <summary>
		/// Copies the entire <see cref="FeedColumnLayoutCollection"/> to a one-dimensional <see cref="Array"/>,
		/// starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
		/// <see cref="FeedColumnLayoutEntry"/> elements copied from the <see cref="FeedColumnLayoutCollection"/>.
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
		/// The number of elements in the source <see cref="FeedColumnLayoutCollection"/> is greater than
		/// the available space from <paramref name="arrayIndex"/> to the end of the destination
		/// <paramref name="array"/>.</para></exception>
		/// <exception cref="InvalidCastException">
		/// The <see cref="FeedColumnLayoutEntry"/> type cannot be cast automatically
		/// to the type of the destination <paramref name="array"/>.</exception>
		/// <remarks>Please refer to ArrayList.CopyTo for details.</remarks>

		void ICollection.CopyTo(Array array, int arrayIndex) {
			CopyTo((FeedColumnLayoutEntry[]) array, arrayIndex);
		}

		#endregion
		#region Equals

		/// <summary>
		/// Determines whether the specified <see cref="FeedColumnLayoutCollection"/>
		/// is equal to the current <b>FeedColumnLayoutCollection</b>.
		/// </summary>
		/// <param name="collection">The <see cref="FeedColumnLayoutCollection"/>
		/// to compare with the current <b>FeedColumnLayoutCollection</b>.</param>
		/// <returns><c>true</c> if the specified <see cref="FeedColumnLayoutCollection"/> is equal
		/// to the current <b>FeedColumnLayoutCollection</b>; otherwise, <c>false</c>.</returns>
		/// <remarks><para>
		/// This <b>Equals</b> overload tests for value equality of all <see cref="FeedColumnLayoutEntry"/>
		/// elements contained in the two <see cref="FeedColumnLayoutCollection"/> collections.
		/// </para><para>
		/// <b>Equals</b> returns <c>false</c> if <paramref name="collection"/> is a null
		/// reference, holds a different number of elements, or holds at least one
		/// <see cref="FeedColumnLayoutEntry"/> element at a given index position whose key and/or
		/// value is different from that of the element at the same index position in this
		/// <b>FeedColumnLayoutCollection</b>, as determined by the inequality operators defined by
		/// <see cref="String"/> and <see cref="FeedColumnLayout"/>.
		/// </para></remarks>

		public virtual bool Equals(FeedColumnLayoutCollection collection) {
			if (collection == null || this._count != collection.Count)
				return false;

			for (int i = 0; i < this._count; i++)
				if (this._keys[i] != collection.InnerKeys[i] ||
					this._values[i] != collection.InnerValues[i])
					return false;

			return true;
		}

		#endregion
		#region GetByIndex

		/// <summary>
		/// Gets the <see cref="FeedColumnLayout"/> value at the
		/// specified index of the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="FeedColumnLayout"/> value to get.</param>
		/// <returns>The <see cref="FeedColumnLayout"/> value at the specified
		/// <paramref name="index"/> of the <see cref="FeedColumnLayoutCollection"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="SortedList.GetByIndex"/> for details.</remarks>

		public virtual FeedColumnLayout GetByIndex(int index) {
			ValidateIndex(index);
			return this._values[index];
		}

		#endregion
		#region GetByKey

		/// <summary>
		/// Gets the <see cref="FeedColumnLayout"/> value associated with the
		/// first occurrence of the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key whose value to get.</param>
		/// <returns>The <see cref="FeedColumnLayout"/> value associated with the first occurrence
		/// of the specified <paramref name="key"/>, if found; otherwise,
		/// a null reference.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <remarks><b>GetByKey</b> and <see cref="SetByKey"/> emulate the indexer of the
		/// <see cref="SortedList"/> class but require linear time to locate a key.</remarks>

		public virtual FeedColumnLayout GetByKey(String key) {
			int index = IndexOfKey(key);
			if (index >= 0) return this._values[index];
			return null;
		}

		#endregion
		#region GetEnumerator: IStringFeedColumnLayoutEnumerator

		/// <summary>
		/// Returns an <see cref="IStringFeedColumnLayoutEnumerator"/> that can
		/// iterate through the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IStringFeedColumnLayoutEnumerator"/>
		/// for the entire <see cref="FeedColumnLayoutCollection"/>.</returns>
		/// <remarks>Please refer to ArrayList.GetEnumerator for details.</remarks>

		public virtual IStringFeedColumnLayoutEnumerator GetEnumerator() {
			return new Enumerator(this);
		}

		#endregion
		#region IEnumerable.GetEnumerator: IEnumerator

		/// <summary>
		/// Returns an <see cref="IEnumerator"/> that can
		/// iterate through the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IEnumerator"/>
		/// for the entire <see cref="FeedColumnLayoutCollection"/>.</returns>
		/// <remarks>Please refer to ArrayList.GetEnumerator for details.</remarks>

		IEnumerator IEnumerable.GetEnumerator() {
			return (IEnumerator) GetEnumerator();
		}

		#endregion
		#region GetKey

		/// <summary>
		/// Gets the <see cref="String"/> key at the
		/// specified index of the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="String"/> key to get.</param>
		/// <returns>The <see cref="String"/> key at the specified
		/// <paramref name="index"/> of the <see cref="FeedColumnLayoutCollection"/>.</returns>
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
		/// Gets the keys in the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
        /// <returns>An ICollection[string] containing the keys
		/// in the <see cref="FeedColumnLayoutCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.GetKeyList"/> for details.</remarks>

		public virtual ICollection<string> GetKeyList() {
			if (this._keyList == null)
				this._keyList = new KeyList(this);
			return this._keyList;
		}

		#endregion
		#region GetValueList

		/// <summary>
		/// Gets the values in the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IFeedColumnLayoutList"/> containing the values
		/// in the <see cref="FeedColumnLayoutCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.GetValueList"/> for details.</remarks>

		public virtual IFeedColumnLayoutList GetValueList() {
			if (this._valueList == null)
				this._valueList = new ValueList(this);
			return this._valueList;
		}

		#endregion
		#region IndexOf(FeedColumnLayoutEntry)

		/// <summary>
		/// Returns the zero-based index of the first occurrence of the specified
		/// <see cref="FeedColumnLayoutEntry"/> in the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <param name="entry">The <see cref="FeedColumnLayoutEntry"/> object
		/// to locate in the <see cref="FeedColumnLayoutCollection"/>.</param>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="entry"/>
		/// in the <see cref="FeedColumnLayoutCollection"/>, if found; otherwise, -1.
		/// </returns>
		/// <remarks><para>
		/// Please refer to ArrayList.IndexOf for details.
		/// </para><para>
		/// <b>IndexOf</b> uses the equality operators defined by <see cref="String"/>
		/// and <see cref="FeedColumnLayout"/> to locate the specified <paramref name="entry"/>.
		/// </para></remarks>

		public virtual int IndexOf(FeedColumnLayoutEntry entry) {

			for (int i = 0; i < this._count; ++i)
				if (entry.Key == this._keys[i] &&
					entry.Value == this._values[i])
					return i;

			return -1;
		}

		#endregion
		#region IList.IndexOf(Object)

		/// <summary>
		/// Returns the zero-based index of the first occurrence of the specified
		/// <see cref="Object"/> in the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <param name="entry">The object to locate in the <see cref="FeedColumnLayoutCollection"/>.
		/// This argument must be compatible with <see cref="FeedColumnLayoutEntry"/>.</param>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="entry"/>
		/// in the <see cref="FeedColumnLayoutCollection"/>, if found; otherwise, -1.
		/// </returns>
		/// <exception cref="InvalidCastException"><paramref name="entry"/>
		/// is not compatible with <see cref="FeedColumnLayoutEntry"/>.</exception>
		/// <remarks>Please refer to ArrayList.IndexOf for details.</remarks>

		int IList.IndexOf(object entry) {
			return IndexOf((FeedColumnLayoutEntry) entry);
		}

		#endregion
		#region IndexOfKey

		/// <summary>
		/// Returns the zero-based index of the first occurrence of the specified
		/// <see cref="String"/> key in the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key
		/// to locate in the <see cref="FeedColumnLayoutCollection"/>.</param>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="key"/>
		/// in the <see cref="FeedColumnLayoutCollection"/>, if found; otherwise, -1.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <remarks>Please refer to <see cref="SortedList.IndexOfKey"/> for details but note
		/// that the <see cref="FeedColumnLayoutCollection"/> requires linear time to locate a key.</remarks>

		public virtual int IndexOfKey(String key) {
			if ((object) key == null)
				throw new ArgumentNullException("key");
			return Array.IndexOf(this._keys, key, 0, this._count);
		}

		#endregion
		#region IndexOfValue

		/// <summary>
		/// Returns the zero-based index of first occurrence of the specified
		/// <see cref="FeedColumnLayout"/> value in the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <param name="value">The <see cref="FeedColumnLayout"/> value
		/// to locate in the <see cref="FeedColumnLayoutCollection"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="value"/>
		/// in the <see cref="FeedColumnLayoutCollection"/>, if found; otherwise, -1.
		/// </returns>
		/// <remarks>Please refer to <see cref="SortedList.IndexOfValue"/> for details.</remarks>

		public virtual int IndexOfValue(FeedColumnLayout value) {
			return Array.IndexOf(this._values, value, 0, this._count);
		}

		#endregion
		#region Insert(Int32, FeedColumnLayoutEntry)

		/// <summary>
		/// Inserts a <see cref="FeedColumnLayoutEntry"/> element into the
		/// <see cref="FeedColumnLayoutCollection"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="entry"/>
		/// should be inserted.</param>
		/// <param name="entry">The <see cref="FeedColumnLayoutEntry"/> object
		/// to insert into the <see cref="FeedColumnLayoutCollection"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedColumnLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedColumnLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.Insert"/> for details.</remarks>

		public virtual void Insert(int index, FeedColumnLayoutEntry entry) {
			if (index < 0)
				throw new ArgumentOutOfRangeException("index",
					index, "Argument cannot be negative.");

			if (index > this._count)
				throw new ArgumentOutOfRangeException("index",
					index, "Argument cannot exceed Count.");

			if (this._count == this._keys.Length)
				EnsureCapacity(this._count + 1);

			++this._version;
			if (index < this._count) {
				Array.Copy(this._keys, index,
					this._keys, index + 1, this._count - index);

				Array.Copy(this._values, index,
					this._values, index + 1, this._count - index);
			}

			this._keys[index] = entry.Key;
			this._values[index] = entry.Value;
			++this._count;
		}

		#endregion
		#region IList.Insert(Int32, Object)

		/// <summary>
		/// Inserts an element into the <see cref="FeedColumnLayoutCollection"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="entry"/>
		/// should be inserted.</param>
		/// <param name="entry">The object to insert into the <see cref="FeedColumnLayoutCollection"/>.
		/// This argument must be compatible with <see cref="FeedColumnLayoutEntry"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <exception cref="InvalidCastException"><paramref name="entry"/>
		/// is not compatible with <see cref="FeedColumnLayoutEntry"/>.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedColumnLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedColumnLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.Insert"/> for details.</remarks>

		void IList.Insert(int index, object entry) {
			Insert(index, (FeedColumnLayoutEntry) entry);
		}

		#endregion
		#region Remove(FeedColumnLayoutEntry)

		/// <summary>
		/// Removes the first occurrence of the specified <see cref="FeedColumnLayoutEntry"/>
		/// from the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <param name="entry">The <see cref="FeedColumnLayoutEntry"/> object
		/// to remove from the <see cref="FeedColumnLayoutCollection"/>.</param>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedColumnLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedColumnLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.Remove"/> for details.</remarks>

		public virtual void Remove(FeedColumnLayoutEntry entry) {
			int index = IndexOf(entry);
			if (index >= 0) RemoveAt(index);
		}

		#endregion
		#region IList.Remove(Object)

		/// <summary>
		/// Removes the first occurrence of the specified <see cref="Object"/>
		/// from the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <param name="entry">The object to remove from the <see cref="FeedColumnLayoutCollection"/>.
		/// This argument must be compatible with <see cref="FeedColumnLayoutEntry"/>.</param>
		/// <exception cref="InvalidCastException"><paramref name="entry"/>
		/// is not compatible with <see cref="FeedColumnLayoutEntry"/>.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedColumnLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedColumnLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.Remove"/> for details.</remarks>

		void IList.Remove(object entry) {
			Remove((FeedColumnLayoutEntry) entry);
		}

		#endregion
		#region RemoveAt

		/// <summary>
		/// Removes the element at the specified index of the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedColumnLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedColumnLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.RemoveAt"/> for details.</remarks>

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
		/// Sets the <see cref="FeedColumnLayout"/> value at the
		/// specified index of the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="FeedColumnLayout"/> value to set.</param>
		/// <param name="value">The <see cref="FeedColumnLayout"/> object to store
		/// at the specified <paramref name="index"/> of the <see cref="FeedColumnLayoutCollection"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="SortedList.SetByIndex"/> for details.</remarks>

		public virtual void SetByIndex(int index, FeedColumnLayout value) {
			ValidateIndex(index);
			++this._version;
			this._values[index] = value;
		}

		#endregion
		#region SetByKey

		/// <summary>
		/// Sets the <see cref="FeedColumnLayout"/> value associated with the
		/// first occurrence of the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key whose value to set.</param>
		/// <param name="value">The <see cref="FeedColumnLayout"/> object to associate with
		/// the first occurrence of the specified <paramref name="key"/>.
		/// This argument can be a null reference.
		/// If the specified <paramref name="key"/> is not found, <b>SetByKey</b> adds a new element
		/// with the specified <paramref name="key"/> and <paramref name="value"/> to the end of the
		/// <see cref="FeedColumnLayoutCollection"/>.</param>
		/// <returns>The <see cref="FeedColumnLayoutCollection"/> index of the element
		/// that was changed or added.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedColumnLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para><paramref name="key"/> does not exist in the collection,
		/// and the <b>FeedColumnLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks><b>SetByKey</b> and <see cref="GetByKey"/> emulate the indexer of the
		/// <see cref="SortedList"/> class but require linear time to locate a key.</remarks>

		public virtual int SetByKey(String key, FeedColumnLayout value) {
			int index = IndexOfKey(key);

			if (index >= 0) {
				this._values[index] = value;
				return index;
			}

			return Add(key, value);
		}

		#endregion
		#region Synchronized

		/// <summary>
		/// Returns a synchronized (thread-safe) wrapper
		/// for the specified <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <param name="collection">The <see cref="FeedColumnLayoutCollection"/> to synchronize.</param>
		/// <returns>
		/// A synchronized (thread-safe) wrapper around <paramref name="collection"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="collection"/> is a null reference.</exception>
		/// <remarks>Please refer to ArrayList.Synchronized for details.</remarks>

		public static FeedColumnLayoutCollection Synchronized(FeedColumnLayoutCollection collection) {
			if (collection == null)
				throw new ArgumentNullException("collection");

			return new SyncList(collection);
		}

		#endregion
		#region ToArray

		/// <summary>
		/// Copies the elements of the <see cref="FeedColumnLayoutCollection"/> to a new
		/// <see cref="Array"/> of <see cref="FeedColumnLayoutEntry"/> elements.
		/// </summary>
		/// <returns>A one-dimensional <see cref="Array"/> of <see cref="FeedColumnLayoutEntry"/>
		/// elements containing copies of the elements of the <see cref="FeedColumnLayoutCollection"/>.
		/// </returns>
		/// <remarks>Please refer to ArrayList.ToArray for details.</remarks>

		public virtual FeedColumnLayoutEntry[] ToArray() {
			FeedColumnLayoutEntry[] array = new FeedColumnLayoutEntry[this._count];
			CopyTo(array, 0);
			return array;
		}

		#endregion
		#region TrimToSize

		/// <summary>
		/// Sets the capacity to the actual number of elements in the <see cref="FeedColumnLayoutCollection"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="FeedColumnLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>FeedColumnLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.TrimToSize"/> for details.</remarks>

		public virtual void TrimToSize() {
			Capacity = this._count;
		}

		#endregion
		#endregion
		#region Private Methods
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
			IStringFeedColumnLayoutEnumerator, IDictionaryEnumerator {
			#region Private Fields

			private readonly FeedColumnLayoutCollection _collection;
			private readonly int _version;
			private int _index;

			#endregion
			#region Internal Constructors

			internal Enumerator(FeedColumnLayoutCollection collection) {
				this._collection = collection;
				this._version = collection._version;
				this._index = -1;
			}

			#endregion
			#region Public Properties

			public FeedColumnLayoutEntry Current {
				get { return Entry; }
			}

			object IEnumerator.Current {
				get { return (DictionaryEntry) Entry; }
			}

			public FeedColumnLayoutEntry Entry {
				get {
					this._collection.CheckEnumIndex(this._index);
					this._collection.CheckEnumVersion(this._version);

					return new FeedColumnLayoutEntry(
						this._collection._keys[this._index],
						this._collection._values[this._index]);
				}
			}

			DictionaryEntry IDictionaryEnumerator.Entry {
				get { return Entry; }
			}

			public String Key {
				get {
					this._collection.CheckEnumIndex(this._index);
					this._collection.CheckEnumVersion(this._version);
					return this._collection._keys[this._index];
				}
			}

			object IDictionaryEnumerator.Key {
				get { return Key; }
			}

			public FeedColumnLayout Value {
				get {
					this._collection.CheckEnumIndex(this._index);
					this._collection.CheckEnumVersion(this._version);
					return this._collection._values[this._index];
				}
			}

			object IDictionaryEnumerator.Value {
				get { return Value; }
			}

			#endregion
			#region Public Methods

			public bool MoveNext() {
				this._collection.CheckEnumVersion(this._version);
				return (++this._index < this._collection.Count);
			}

			public void Reset() {
				this._collection.CheckEnumVersion(this._version);
				this._index = -1;
			}

			#endregion
		}

		#endregion
		#region Class KeyList

		[Serializable]
			private sealed class KeyList: IList<string> {
			#region Private Fields

			private FeedColumnLayoutCollection _collection;

			#endregion
			#region Internal Constructors

			internal KeyList(FeedColumnLayoutCollection collection) {
				this._collection = collection;
			}

			#endregion
			#region Public Properties

			public int Count {
				get { return this._collection.Count; }
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public bool IsFixedSize {
				get { return true; }
			}

			public bool IsSynchronized {
				get { return this._collection.IsSynchronized; }
			}

			public String this[int index] {
				get { return this._collection.GetKey(index); }
				set { throw new NotSupportedException(
						  "Read-only collections cannot be modified."); }
			}

			string IList<string>.this[int index] {
				get { return this[index]; }
				set { throw new NotSupportedException(
						  "Read-only collections cannot be modified."); }
			}

			public object SyncRoot {
				get { return this._collection.SyncRoot; }
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
				return this._collection.ContainsKey(key);
			}

	
			public void CopyTo(String[] array, int arrayIndex) {
				this._collection.CheckTargetArray(array, arrayIndex);
				Array.Copy(this._collection._keys, 0,
					array, arrayIndex, this._collection.Count);
			}



			public IEnumerator<string> GetEnumerator() {
				return _collection.Keys.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return (IEnumerator) GetEnumerator();
			}

			public int IndexOf(String key) {
				return this._collection.IndexOfKey(key);
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
		
		#region Class ValueList

		[Serializable]
			private sealed class ValueList: IFeedColumnLayoutList, IList {
			#region Private Fields

			private FeedColumnLayoutCollection _collection;

			#endregion
			#region Internal Constructors

			internal ValueList(FeedColumnLayoutCollection collection) {
				this._collection = collection;
			}

			#endregion
			#region Public Properties

			public int Count {
				get { return this._collection.Count; }
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public bool IsFixedSize {
				get { return true; }
			}

			public bool IsSynchronized {
				get { return this._collection.IsSynchronized; }
			}

			public FeedColumnLayout this[int index] {
				get { return this._collection.GetByIndex(index); }
				set { throw new NotSupportedException(
						  "Read-only collections cannot be modified."); }
			}

			object IList.this[int index] {
				get { return this[index]; }
				set { throw new NotSupportedException(
						  "Read-only collections cannot be modified."); }
			}

			public object SyncRoot {
				get { return this._collection.SyncRoot; }
			}

			#endregion
			#region Public Methods

			public int Add(FeedColumnLayout value) {
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

			public bool Contains(FeedColumnLayout value) {
				return this._collection.ContainsValue(value);
			}

			bool IList.Contains(object value) {
				return Contains((FeedColumnLayout) value);
			}

			public void CopyTo(FeedColumnLayout[] array, int arrayIndex) {
				this._collection.CheckTargetArray(array, arrayIndex);
				Array.Copy(this._collection._values, 0,
					array, arrayIndex, this._collection.Count);
			}

			void ICollection.CopyTo(Array array, int arrayIndex) {
				this._collection.CheckTargetArray(array, arrayIndex);
				CopyTo((FeedColumnLayout[]) array, arrayIndex);
			}

			public IFeedColumnLayoutEnumerator GetEnumerator() {
				return new ValueEnumerator(this._collection);
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return (IEnumerator) GetEnumerator();
			}

			public int IndexOf(FeedColumnLayout value) {
				return this._collection.IndexOfValue(value);
			}

			int IList.IndexOf(object value) {
				return IndexOf((FeedColumnLayout) value);
			}

			public void Insert(int index, FeedColumnLayout value) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			void IList.Insert(int index, object value) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			public void Remove(FeedColumnLayout value) {
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
			IFeedColumnLayoutEnumerator, IEnumerator {
			#region Private Fields

			private readonly FeedColumnLayoutCollection _collection;
			private readonly int _version;
			private int _index;

			#endregion
			#region Internal Constructors

			internal ValueEnumerator(FeedColumnLayoutCollection collection) {
				this._collection = collection;
				this._version = collection._version;
				this._index = -1;
			}

			#endregion
			#region Public Properties

			public FeedColumnLayout Current {
				get {
					this._collection.CheckEnumIndex(this._index);
					this._collection.CheckEnumVersion(this._version);
					return this._collection._values[this._index];
				}
			}

			object IEnumerator.Current {
				get { return Current; }
			}

			#endregion
			#region Public Methods

			public bool MoveNext() {
				this._collection.CheckEnumVersion(this._version);
				return (++this._index < this._collection.Count);
			}

			public void Reset() {
				this._collection.CheckEnumVersion(this._version);
				this._index = -1;
			}

			#endregion
		}

		#endregion
		#region Class SyncList

		[Serializable]
			private sealed class SyncList: FeedColumnLayoutCollection {
			#region Private Fields

			private FeedColumnLayoutCollection _collection;
			private object _root;

			#endregion
			#region Internal Constructors

			internal SyncList(FeedColumnLayoutCollection collection):
				base(Tag.Default) {

				this._collection = collection;
				this._root = collection.SyncRoot;
			}

			#endregion
			#region Protected Properties

			protected override String[] InnerKeys {
				get { lock (this._root) return this._collection.InnerKeys; }
			}

			protected override FeedColumnLayout[] InnerValues {
				get { lock (this._root) return this._collection.InnerValues; }
			}

			#endregion
			#region Public Properties

			public override int Capacity {
				get { lock (this._root) return this._collection.Capacity; }
			}

			public override int Count {
				get { lock (this._root) return this._collection.Count; }
			}

			public override bool IsFixedSize {
				get { return this._collection.IsFixedSize; }
			}

			public override bool IsReadOnly {
				get { return this._collection.IsReadOnly; }
			}

			public override bool IsSynchronized {
				get { return true; }
			}

			public override FeedColumnLayoutEntry this[int index] {
				get { lock (this._root) return this._collection[index]; }
				set { lock (this._root) this._collection[index] = value; }
			}

			public override ICollection<string> Keys {
				get { lock (this._root) return this._collection.Keys; }
			}

			public override object SyncRoot {
				get { return this._root; }
			}

			public override IFeedColumnLayoutCollection Values {
				get { lock (this._root) return this._collection.Values; }
			}

			#endregion
			#region Public Methods

			public override int Add(FeedColumnLayoutEntry entry) {
				lock (this._root) return this._collection.Add(entry);
			}

			public override void AddRange(FeedColumnLayoutCollection collection) {
				lock (this._root) this._collection.AddRange(collection);
			}

			public override void AddRange(FeedColumnLayoutEntry[] array) {
				lock (this._root) this._collection.AddRange(array);
			}

			public override void Clear() {
				lock (this._root) this._collection.Clear();
			}

			public override object Clone() {
				lock (this._root) return this._collection.Clone();
			}

			public override bool Contains(FeedColumnLayoutEntry entry) {
				lock (this._root) return this._collection.Contains(entry);
			}

			public override bool ContainsKey(String key) {
				lock (this._root) return this._collection.ContainsKey(key);
			}

			public override bool ContainsValue(FeedColumnLayout value) {
				lock (this._root) return this._collection.ContainsValue(value);
			}

			public override void CopyTo(FeedColumnLayoutEntry[] array, int index) {
				lock (this._root) this._collection.CopyTo(array, index);
			}

			public override bool Equals(FeedColumnLayoutCollection collection) {
				lock (this._root) return this._collection.Equals(collection);
			}

			public override FeedColumnLayout GetByIndex(int index) {
				lock (this._root) return this._collection.GetByIndex(index);
			}

			public override FeedColumnLayout GetByKey(String key) {
				lock (this._root) return this._collection.GetByKey(key);
			}

			public override IStringFeedColumnLayoutEnumerator GetEnumerator() {
				lock (this._root) return this._collection.GetEnumerator();
			}

			public override String GetKey(int index) {
				lock (this._root) return this._collection.GetKey(index);
			}

			public override ICollection<string> GetKeyList() {
				lock (this._root) return this._collection.GetKeyList();
			}

			public override IFeedColumnLayoutList GetValueList() {
				lock (this._root) return this._collection.GetValueList();
			}

			public override int IndexOf(FeedColumnLayoutEntry entry) {
				lock (this._root) return this._collection.IndexOf(entry);
			}

			public override int IndexOfKey(String key) {
				lock (this._root) return this._collection.IndexOfKey(key);
			}

			public override int IndexOfValue(FeedColumnLayout value) {
				lock (this._root) return this._collection.IndexOfValue(value);
			}

			public override void Insert(int index, FeedColumnLayoutEntry entry) {
				lock (this._root) this._collection.Insert(index, entry);
			}

			public override void Remove(FeedColumnLayoutEntry entry) {
				lock (this._root) this._collection.Remove(entry);
			}

			public override void RemoveAt(int index) {
				lock (this._root) this._collection.RemoveAt(index);
			}

			public override void SetByIndex(int index, FeedColumnLayout value) {
				lock (this._root) this._collection.SetByIndex(index, value);
			}

			public override int SetByKey(String key, FeedColumnLayout value) {
				lock (this._root) return this._collection.SetByKey(key, value);
			}

			public override FeedColumnLayoutEntry[] ToArray() {
				lock (this._root) return this._collection.ToArray();
			}

			public override void TrimToSize() {
				lock (this._root) this._collection.TrimToSize();
			}

			#endregion
		}

		#endregion
	}

	#endregion
}



namespace NewsComponents.Collections.Old {

	#region Interface IlistviewLayoutCollection

	/// <summary>
	/// Defines size, enumerators, and synchronization methods for strongly
	/// typed collections of <see cref="listviewLayout"/> elements.
	/// </summary>
	/// <remarks>
	/// <b>IlistviewLayoutCollection</b> provides an <see cref="ICollection"/>
	/// that is strongly typed for <see cref="listviewLayout"/> elements.
	/// </remarks>

	public interface IlistviewLayoutCollection {
		#region Properties
		#region Count

		/// <summary>
		/// Gets the number of elements contained in the
		/// <see cref="IlistviewLayoutCollection"/>.
		/// </summary>
		/// <value>The number of elements contained in the
		/// <see cref="IlistviewLayoutCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ICollection.Count"/> for details.</remarks>

		int Count { get; }

		#endregion
		#region IsSynchronized

		/// <summary>
		/// Gets a value indicating whether access to the
		/// <see cref="IlistviewLayoutCollection"/> is synchronized (thread-safe).
		/// </summary>
		/// <value><c>true</c> if access to the <see cref="IlistviewLayoutCollection"/> is
		/// synchronized (thread-safe); otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="ICollection.IsSynchronized"/> for details.</remarks>

		bool IsSynchronized { get; }

		#endregion
		#region SyncRoot

		/// <summary>
		/// Gets an object that can be used to synchronize access
		/// to the <see cref="IlistviewLayoutCollection"/>.
		/// </summary>
		/// <value>An object that can be used to synchronize access
		/// to the <see cref="IlistviewLayoutCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ICollection.SyncRoot"/> for details.</remarks>

		object SyncRoot { get; }

		#endregion
		#endregion
		#region Methods
		#region CopyTo

		/// <summary>
		/// Copies the entire <see cref="IlistviewLayoutCollection"/> to a one-dimensional <see cref="Array"/>
		/// of <see cref="listviewLayout"/> elements, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
		/// <see cref="listviewLayout"/> elements copied from the <see cref="IlistviewLayoutCollection"/>.
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
		/// The number of elements in the source <see cref="IlistviewLayoutCollection"/> is greater
		/// than the available space from <paramref name="arrayIndex"/> to the end of the destination
		/// <paramref name="array"/>.</para></exception>
		/// <remarks>Please refer to <see cref="ICollection.CopyTo"/> for details.</remarks>

		void CopyTo(listviewLayout[] array, int arrayIndex);

		#endregion
		#region GetEnumerator

		/// <summary>
		/// Returns an <see cref="IlistviewLayoutEnumerator"/> that can
		/// iterate through the <see cref="IlistviewLayoutCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IlistviewLayoutEnumerator"/>
		/// for the entire <see cref="IlistviewLayoutCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="IEnumerable.GetEnumerator"/> for details.</remarks>

		IlistviewLayoutEnumerator GetEnumerator();

		#endregion
		#endregion
	}

	#endregion
	
	#region Interface IlistviewLayoutList

	/// <summary>
	/// Represents a strongly typed collection of <see cref="listviewLayout"/>
	/// objects that can be individually accessed by index.
	/// </summary>
	/// <remarks>
	/// <b>IlistviewLayoutList</b> provides an <see cref="IList"/>
	/// that is strongly typed for <see cref="listviewLayout"/> elements.
	/// </remarks>

	public interface
		IlistviewLayoutList: IlistviewLayoutCollection {
		#region Properties
		#region IsFixedSize

		/// <summary>
		/// Gets a value indicating whether the <see cref="IlistviewLayoutList"/> has a fixed size.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IlistviewLayoutList"/> has a fixed size;
		/// otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IList.IsFixedSize"/> for details.</remarks>

		bool IsFixedSize { get; }

		#endregion
		#region IsReadOnly

		/// <summary>
		/// Gets a value indicating whether the <see cref="IlistviewLayoutList"/> is read-only.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IlistviewLayoutList"/> is read-only;
		/// otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IList.IsReadOnly"/> for details.</remarks>

		bool IsReadOnly { get; }

		#endregion
		#region Item

		/// <summary>
		/// Gets or sets the <see cref="listviewLayout"/> element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="listviewLayout"/> element to get or set.</param>
		/// <value>
		/// The <see cref="listviewLayout"/> element at the specified <paramref name="index"/>.
		/// </value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than
		/// <see cref="IlistviewLayoutCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The property is set and the <see cref="IlistviewLayoutList"/> is read-only.</exception>
		/// <remarks>Please refer to <see cref="IList.this"/> for details.</remarks>

		listviewLayout this[int index] { get; set; }

		#endregion
		#endregion
		#region Methods
		#region Add

		/// <summary>
		/// Adds a <see cref="listviewLayout"/> to the end
		/// of the <see cref="IlistviewLayoutList"/>.
		/// </summary>
		/// <param name="value">The <see cref="listviewLayout"/> object
		/// to be added to the end of the <see cref="IlistviewLayoutList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns>The <see cref="IlistviewLayoutList"/> index at which
		/// the <paramref name="value"/> has been added.</returns>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IlistviewLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IlistviewLayoutList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Add"/> for details.</remarks>

		int Add(listviewLayout value);

		#endregion
		#region Clear

		/// <summary>
		/// Removes all elements from the <see cref="IlistviewLayoutList"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IlistviewLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IlistviewLayoutList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Clear"/> for details.</remarks>

		void Clear();

		#endregion
		#region Contains

		/// <summary>
		/// Determines whether the <see cref="IlistviewLayoutList"/>
		/// contains the specified <see cref="listviewLayout"/> element.
		/// </summary>
		/// <param name="value">The <see cref="listviewLayout"/> object
		/// to locate in the <see cref="IlistviewLayoutList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns><c>true</c> if <paramref name="value"/> is found in the
		/// <see cref="IlistviewLayoutList"/>; otherwise, <c>false</c>.</returns>
		/// <remarks>Please refer to <see cref="IList.Contains"/> for details.</remarks>

		bool Contains(listviewLayout value);

		#endregion
		#region IndexOf

		/// <summary>
		/// Returns the zero-based index of the first occurrence of the specified
		/// <see cref="listviewLayout"/> in the <see cref="IlistviewLayoutList"/>.
		/// </summary>
		/// <param name="value">The <see cref="listviewLayout"/> object
		/// to locate in the <see cref="IlistviewLayoutList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="value"/>
		/// in the <see cref="IlistviewLayoutList"/>, if found; otherwise, -1.
		/// </returns>
		/// <remarks>Please refer to <see cref="IList.IndexOf"/> for details.</remarks>

		int IndexOf(listviewLayout value);

		#endregion
		#region Insert

		/// <summary>
		/// Inserts a <see cref="listviewLayout"/> element into the
		/// <see cref="IlistviewLayoutList"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which
		/// <paramref name="value"/> should be inserted.</param>
		/// <param name="value">The <see cref="listviewLayout"/> object
		/// to insert into the <see cref="IlistviewLayoutList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is greater than
		/// <see cref="IlistviewLayoutCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IlistviewLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IlistviewLayoutList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Insert"/> for details.</remarks>

		void Insert(int index, listviewLayout value);

		#endregion
		#region Remove

		/// <summary>
		/// Removes the first occurrence of the specified <see cref="listviewLayout"/>
		/// from the <see cref="IlistviewLayoutList"/>.
		/// </summary>
		/// <param name="value">The <see cref="listviewLayout"/> object
		/// to remove from the <see cref="IlistviewLayoutList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IlistviewLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IlistviewLayoutList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Remove"/> for details.</remarks>

		void Remove(listviewLayout value);

		#endregion
		#region RemoveAt

		/// <summary>
		/// Removes the element at the specified index of the
		/// <see cref="IlistviewLayoutList"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than
		/// <see cref="IlistviewLayoutCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IlistviewLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IlistviewLayoutList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.RemoveAt"/> for details.</remarks>

		void RemoveAt(int index);

		#endregion
		#endregion
	}

	#endregion

	#region Interface IlistviewLayoutEnumerator

	/// <summary>
	/// Supports type-safe iteration over a collection that
	/// contains <see cref="listviewLayout"/> elements.
	/// </summary>
	/// <remarks>
	/// <b>IlistviewLayoutEnumerator</b> provides an <see cref="IEnumerator"/>
	/// that is strongly typed for <see cref="listviewLayout"/> elements.
	/// </remarks>

	public interface IlistviewLayoutEnumerator {
		#region Properties
		#region Current

		/// <summary>
		/// Gets the current <see cref="listviewLayout"/> element in the collection.
		/// </summary>
		/// <value>The current <see cref="listviewLayout"/> element in the collection.</value>
		/// <exception cref="InvalidOperationException"><para>The enumerator is positioned
		/// before the first element of the collection or after the last element.</para>
		/// <para>-or-</para>
		/// <para>The collection was modified after the enumerator was created.</para></exception>
		/// <remarks>Please refer to <see cref="IEnumerator.Current"/> for details, but note
		/// that <b>Current</b> fails if the collection was modified since the last successful
		/// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>

		listviewLayout Current { get; }

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

	#region Interface IStringlistviewLayoutCollection

	/// <summary>
	/// Defines size, enumerators, and synchronization methods for strongly
	/// typed collections of <see cref="listviewLayoutEntry"/> elements.
	/// </summary>
	/// <remarks>
	/// <b>IStringlistviewLayoutCollection</b> provides an <see cref="ICollection"/>
	/// that is strongly typed for <see cref="listviewLayoutEntry"/> elements.
	/// </remarks>

	public interface IStringlistviewLayoutCollection {
		#region Properties
		#region Count

		/// <summary>
		/// Gets the number of elements contained in the
		/// <see cref="IStringlistviewLayoutCollection"/>.
		/// </summary>
		/// <value>The number of elements contained in the
		/// <see cref="IStringlistviewLayoutCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ICollection.Count"/> for details.</remarks>

		int Count { get; }

		#endregion
		#region IsSynchronized

		/// <summary>
		/// Gets a value indicating whether access to the
		/// <see cref="IStringlistviewLayoutCollection"/> is synchronized (thread-safe).
		/// </summary>
		/// <value><c>true</c> if access to the <see cref="IStringlistviewLayoutCollection"/>
		/// is synchronized (thread-safe); otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="ICollection.IsSynchronized"/> for details.</remarks>

		bool IsSynchronized { get; }

		#endregion
		#region SyncRoot

		/// <summary>
		/// Gets an object that can be used to synchronize access
		/// to the <see cref="IStringlistviewLayoutCollection"/>.
		/// </summary>
		/// <value>An object that can be used to synchronize access to the
		/// <see cref="IStringlistviewLayoutCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ICollection.SyncRoot"/> for details.</remarks>

		object SyncRoot { get; }

		#endregion
		#endregion
		#region Methods
		#region CopyTo

		/// <summary>
		/// Copies the entire <see cref="IStringlistviewLayoutCollection"/>
		/// to a one-dimensional <see cref="Array"/> of <see cref="listviewLayoutEntry"/> elements,
		/// starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the
		/// destination of the <see cref="listviewLayoutEntry"/> elements copied from the
		/// <see cref="IStringlistviewLayoutCollection"/>.
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
		/// The number of elements in the source <see cref="IStringlistviewLayoutCollection"/>
		/// is greater than the available space from <paramref name="arrayIndex"/> to the end of the
		/// destination <paramref name="array"/>.</para></exception>
		/// <remarks>Please refer to <see cref="ICollection.CopyTo"/> for details.</remarks>

		void CopyTo(listviewLayoutEntry[] array, int arrayIndex);

		#endregion
		#region GetEnumerator

		/// <summary>
		/// Returns an <see cref="IStringlistviewLayoutEnumerator"/> that can
		/// iterate through the <see cref="IStringlistviewLayoutCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IStringlistviewLayoutEnumerator"/>
		/// for the entire <see cref="IStringlistviewLayoutCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="IEnumerable.GetEnumerator"/> for details.</remarks>

		IStringlistviewLayoutEnumerator GetEnumerator();

		#endregion
		#endregion
	}

	#endregion

	#region Interface IStringlistviewLayoutDictionary

	/// <summary>
	/// Represents a strongly typed collection of
	/// <see cref="listviewLayoutEntry"/> key-and-value pairs.
	/// </summary>
	/// <remarks>
	/// <b>IStringlistviewLayoutDictionary</b> provides an
	/// <see cref="IDictionary"/> that is strongly typed for
	/// <see cref="String"/> keys and <see cref="listviewLayout"/> values.
	/// </remarks>

	public interface
		IStringlistviewLayoutDictionary: IStringlistviewLayoutCollection {
		#region Properties
		#region IsFixedSize

		/// <summary>
		/// Gets a value indicating whether the
		/// <see cref="IStringlistviewLayoutDictionary"/> has a fixed size.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IStringlistviewLayoutDictionary"/>
		/// has a fixed size; otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IDictionary.IsFixedSize"/> for details.</remarks>

		bool IsFixedSize { get; }

		#endregion
		#region IsReadOnly

		/// <summary>
		/// Gets a value indicating whether the
		/// <see cref="IStringlistviewLayoutDictionary"/> is read-only.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IStringlistviewLayoutDictionary"/>
		/// is read-only; otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IDictionary.IsReadOnly"/> for details.</remarks>

		bool IsReadOnly { get; }

		#endregion
		#region Item

		/// <summary>
		/// Gets or sets the <see cref="listviewLayout"/> value
		/// associated with the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key
		/// whose value to get or set.</param>
		/// <value>The <see cref="listviewLayout"/> value associated with the specified
		/// <paramref name="key"/>. If the specified <paramref name="key"/> is not found,
		/// attempting to get it returns
		/// a null reference,
		/// and attempting to set it creates a new element using the specified
		/// <paramref name="key"/>.</value>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The property is set and the
		/// <see cref="IStringlistviewLayoutDictionary"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The property is set, <paramref name="key"/> does not exist in the collection,
		/// and the <b>IStringlistviewLayoutDictionary</b> has a fixed size.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="IDictionary.this"/> for details.</remarks>

		listviewLayout this[String key] { get; set; }

		#endregion
		#region Keys

		/// <summary>
        /// Gets an ICollection[string] containing the keys
		/// in the <see cref="IStringlistviewLayoutDictionary"/>.
		/// </summary>
        /// <value>An ICollection[string] containing the keys
		/// in the <see cref="IStringlistviewLayoutDictionary"/>.</value>
		/// <remarks>Please refer to <see cref="IDictionary.Keys"/> for details.</remarks>

		ICollection<string> Keys { get; }

		#endregion
		#region Values

		/// <summary>
		/// Gets an <see cref="IlistviewLayoutCollection"/> containing the values
		/// in the <see cref="IStringlistviewLayoutDictionary"/>.
		/// </summary>
		/// <value>An <see cref="IlistviewLayoutCollection"/> containing the values
		/// in the <see cref="IStringlistviewLayoutDictionary"/>.</value>
		/// <remarks>Please refer to <see cref="IDictionary.Values"/> for details.</remarks>

		IlistviewLayoutCollection Values { get; }

		#endregion
		#endregion
		#region Methods
		#region Add

		/// <summary>
		/// Adds an element with the specified <see cref="String"/>
		/// key and <see cref="listviewLayout"/> value to the
		/// <see cref="IStringlistviewLayoutDictionary"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key of the element
		/// to add to the <see cref="IStringlistviewLayoutDictionary"/>.</param>
		/// <param name="value">The <see cref="listviewLayout"/> value of the element
		/// to add to the <see cref="IStringlistviewLayoutDictionary"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentException">
		/// <para>An element with the specified <paramref name="key"/> already exists
		/// in the <see cref="IStringlistviewLayoutDictionary"/>.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringlistviewLayoutDictionary</b> is set to use the
		/// <see cref="IComparable"/> interface, and <paramref name="key"/> does not
		/// implement the <b>IComparable</b> interface.</para></exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringlistviewLayoutDictionary"/> is read-only.
		/// </para><para>-or-</para>
		/// <para>The <b>IStringlistviewLayoutDictionary</b> has a fixed size.
		/// </para></exception>
		/// <remarks>Please refer to <see cref="IDictionary.Add"/> for details.</remarks>

		void Add(String key, listviewLayout value);

		#endregion
		#region Clear

		/// <summary>
		/// Removes all elements from the <see cref="IStringlistviewLayoutDictionary"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringlistviewLayoutDictionary"/> is read-only.
		/// </para><para>-or-</para>
		/// <para>The <b>IStringlistviewLayoutDictionary</b> has a fixed size.
		/// </para></exception>
		/// <remarks>Please refer to <see cref="IDictionary.Clear"/> for details.</remarks>

		void Clear();

		#endregion
		#region Contains

		/// <summary>
		/// Determines whether the <see cref="IStringlistviewLayoutDictionary"/>
		/// contains the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key to locate
		/// in the <see cref="IStringlistviewLayoutDictionary"/>.</param>
		/// <returns><c>true</c> if the <see cref="IStringlistviewLayoutDictionary"/>
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
		/// from the <see cref="IStringlistviewLayoutDictionary"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key of the element to remove
		/// from the <see cref="IStringlistviewLayoutDictionary"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringlistviewLayoutDictionary"/> is read-only.
		/// </para><para>-or-</para>
		/// <para>The <b>IStringlistviewLayoutDictionary</b> has a fixed size.
		/// </para></exception>
		/// <remarks>Please refer to <see cref="IDictionary.Remove"/> for details.</remarks>

		void Remove(String key);

		#endregion
		#endregion
	}

	#endregion

	#region Interface IStringlistviewLayoutList

	/// <summary>
	/// Represents a strongly typed collection of <see cref="listviewLayoutEntry"/>
	/// objects that can be individually accessed by index.
	/// </summary>
	/// <remarks>
	/// <b>IStringlistviewLayoutList</b> provides an <see cref="IList"/>
	/// that is strongly typed for <see cref="listviewLayoutEntry"/> elements.
	/// </remarks>

	public interface
		IStringlistviewLayoutList: IStringlistviewLayoutCollection {
		#region Properties
		#region IsFixedSize

		/// <summary>
		/// Gets a value indicating whether the
		/// <see cref="IStringlistviewLayoutList"/> has a fixed size.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IStringlistviewLayoutList"/>
		/// has a fixed size; otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IList.IsFixedSize"/> for details.</remarks>

		bool IsFixedSize { get; }

		#endregion
		#region IsReadOnly

		/// <summary>
		/// Gets a value indicating whether the
		/// <see cref="IStringlistviewLayoutList"/> is read-only.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IStringlistviewLayoutList"/>
		/// is read-only; otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IList.IsReadOnly"/> for details.</remarks>

		bool IsReadOnly { get; }

		#endregion
		#region Item

		/// <summary>
		/// Gets or sets the <see cref="listviewLayoutEntry"/> element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="listviewLayoutEntry"/> element to get or set.</param>
		/// <value>
		/// The <see cref="listviewLayoutEntry"/> element at the specified <paramref name="index"/>.
		/// </value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than
		/// <see cref="IStringlistviewLayoutCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">The property is set and the
		/// <see cref="IStringlistviewLayoutList"/> is read-only.</exception>
		/// <remarks>Please refer to <see cref="IList.this"/> for details.</remarks>

		listviewLayoutEntry this[int index] { get; set; }

		#endregion
		#endregion
		#region Methods
		#region Add

		/// <summary>
		/// Adds a <see cref="listviewLayoutEntry"/> to the end
		/// of the <see cref="IStringlistviewLayoutList"/>.
		/// </summary>
		/// <param name="entry">The <see cref="listviewLayoutEntry"/> object
		/// to be added to the end of the <see cref="IStringlistviewLayoutList"/>.
		/// </param>
		/// <returns>The <see cref="IStringlistviewLayoutList"/> index at which
		/// the <paramref name="entry"/> has been added.</returns>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringlistviewLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringlistviewLayoutList</b> has a fixed size.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="IList.Add"/> for details.</remarks>

		int Add(listviewLayoutEntry entry);

		#endregion
		#region Clear

		/// <summary>
		/// Removes all elements from the <see cref="IStringlistviewLayoutList"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringlistviewLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringlistviewLayoutList</b> has a fixed size.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="IList.Clear"/> for details.</remarks>

		void Clear();

		#endregion
		#region Contains

		/// <summary>
		/// Determines whether the <see cref="IStringlistviewLayoutList"/>
		/// contains the specified <see cref="listviewLayoutEntry"/> element.
		/// </summary>
		/// <param name="entry">The <see cref="listviewLayoutEntry"/> object
		/// to locate in the <see cref="IStringlistviewLayoutList"/>.</param>
		/// <returns><c>true</c> if <paramref name="entry"/> is found in the
		/// <see cref="IStringlistviewLayoutList"/>; otherwise, <c>false</c>.</returns>
		/// <remarks>Please refer to <see cref="IList.Contains"/> for details.</remarks>

		bool Contains(listviewLayoutEntry entry);

		#endregion
		#region IndexOf

		/// <summary>
		/// Returns the zero-based index of the first occurrence of the specified
		/// <see cref="listviewLayoutEntry"/> in the <see cref="IStringlistviewLayoutList"/>.
		/// </summary>
		/// <param name="entry">The <see cref="listviewLayoutEntry"/> object
		/// to locate in the <see cref="IStringlistviewLayoutList"/>.</param>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="entry"/>
		/// in the <see cref="IStringlistviewLayoutList"/>, if found; otherwise, -1.
		/// </returns>
		/// <remarks>Please refer to <see cref="IList.IndexOf"/> for details.</remarks>

		int IndexOf(listviewLayoutEntry entry);

		#endregion
		#region Insert

		/// <summary>
		/// Inserts a <see cref="listviewLayoutEntry"/> element into the
		/// <see cref="IStringlistviewLayoutList"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which
		/// <paramref name="entry"/> should be inserted.</param>
		/// <param name="entry">The <see cref="listviewLayoutEntry"/> object to insert
		/// into the <see cref="IStringlistviewLayoutList"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is greater than
		/// <see cref="IStringlistviewLayoutCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringlistviewLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringlistviewLayoutList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Insert"/> for details.</remarks>

		void Insert(int index, listviewLayoutEntry entry);

		#endregion
		#region Remove

		/// <summary>
		/// Removes the first occurrence of the specified <see cref="listviewLayoutEntry"/>
		/// from the <see cref="IStringlistviewLayoutList"/>.
		/// </summary>
		/// <param name="entry">The <see cref="listviewLayoutEntry"/> object to remove
		/// from the <see cref="IStringlistviewLayoutList"/>.</param>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringlistviewLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringlistviewLayoutList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Remove"/> for details.</remarks>

		void Remove(listviewLayoutEntry entry);

		#endregion
		#region RemoveAt

		/// <summary>
		/// Removes the element at the specified index of the
		/// <see cref="IStringlistviewLayoutList"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than
		/// <see cref="IStringlistviewLayoutCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringlistviewLayoutList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringlistviewLayoutList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.RemoveAt"/> for details.</remarks>

		void RemoveAt(int index);

		#endregion
		#endregion
	}

	#endregion

	#region Interface IStringlistviewLayoutEnumerator

	/// <summary>
	/// Supports type-safe iteration over a dictionary that
	/// contains <see cref="listviewLayoutEntry"/> elements.
	/// </summary>
	/// <remarks>
	/// <b>IStringlistviewLayoutEnumerator</b> provides an
	/// <see cref="IDictionaryEnumerator"/> that is strongly typed for
	/// <see cref="String"/> keys and <see cref="listviewLayout"/> values.
	/// </remarks>

	public interface IStringlistviewLayoutEnumerator {
		#region Properties
		#region Current

		/// <summary>
		/// Gets the current <see cref="listviewLayoutEntry"/> element in the collection.
		/// </summary>
		/// <value>The current <see cref="listviewLayoutEntry"/> element in the collection.</value>
		/// <exception cref="InvalidOperationException"><para>The enumerator is positioned
		/// before the first element of the collection or after the last element.</para>
		/// <para>-or-</para>
		/// <para>The collection was modified after the enumerator was created.</para></exception>
		/// <remarks>Please refer to <see cref="IEnumerator.Current"/> for details, but note
		/// that <b>Current</b> fails if the collection was modified since the last successful
		/// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>

		listviewLayoutEntry Current { get; }

		#endregion
		#region Entry

		/// <summary>
		/// Gets a <see cref="listviewLayoutEntry"/> containing both
		/// the key and the value of the current dictionary entry.
		/// </summary>
		/// <value>A <see cref="listviewLayoutEntry"/> containing both
		/// the key and the value of the current dictionary entry.</value>
		/// <exception cref="InvalidOperationException"><para>The enumerator is positioned
		/// before the first element of the dictionary or after the last element.</para>
		/// <para>-or-</para>
		/// <para>The dictionary was modified after the enumerator was created.</para></exception>
		/// <remarks>Please refer to <see cref="IDictionaryEnumerator.Entry"/> for details, but
		/// note that <b>Entry</b> fails if the collection was modified since the last successful
		/// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>

		listviewLayoutEntry Entry { get; }

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
		/// Gets the <see cref="listviewLayout"/> value of the current dictionary entry.
		/// </summary>
		/// <value>The <see cref="listviewLayout"/> value
		/// of the current element of the enumeration.</value>
		/// <exception cref="InvalidOperationException"><para>The enumerator is positioned
		/// before the first element of the dictionary or after the last element.</para>
		/// <para>-or-</para>
		/// <para>The dictionary was modified after the enumerator was created.</para></exception>
		/// <remarks>Please refer to <see cref="IDictionaryEnumerator.Value"/> for details, but
		/// note that <b>Value</b> fails if the collection was modified since the last successful
		/// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>

		listviewLayout Value { get; }

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

	#region Struct listviewLayoutEntry

	/// <summary>
	/// Implements a strongly typed pair of one <see cref="String"/>
	/// key and one <see cref="listviewLayout"/> value.
	/// </summary>
	/// <remarks>
	/// <b>listviewLayoutEntry</b> provides a <see cref="DictionaryEntry"/> that is strongly
	/// typed for <see cref="String"/> keys and <see cref="listviewLayout"/> values.
	/// </remarks>

	[Serializable]
	public struct listviewLayoutEntry {
		#region Private Fields

		private String _key;
		private listviewLayout _value;

		#endregion
		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="listviewLayoutEntry"/>
		/// class with the specified key and value.
		/// </summary>
		/// <param name="key">
		/// The <see cref="String"/> key in the key-and-value pair.</param>
		/// <param name="value">
		/// The <see cref="listviewLayout"/> value in the key-and-value pair.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>

		public listviewLayoutEntry(String key, listviewLayout value) {
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
		/// <see cref="listviewLayoutEntry"/> is a value type and therefore has an implicit default
		/// constructor that zeroes all data members. This means that the <b>Key</b> property of
		/// a default-constructed <b>listviewLayoutEntry</b> contains a null reference by default,
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
		/// Gets or sets the <see cref="listviewLayout"/> value in the key-and-value pair.
		/// </summary>
		/// <value>
		/// The <see cref="listviewLayout"/> value in the key-and-value pair.
		/// This value can be a null reference, which is also the default.
		/// </value>

		public listviewLayout Value {
			get { return this._value; }
			set { this._value = value; }
		}

		#endregion
		#endregion
		#region Public Operators
		#region listviewLayoutEntry(DictionaryEntry)

		/// <summary>
		/// Converts a <see cref="DictionaryEntry"/> to a <see cref="listviewLayoutEntry"/>.
		/// </summary>
		/// <param name="entry">A <see cref="DictionaryEntry"/> object to convert.</param>
		/// <returns>A <see cref="listviewLayoutEntry"/> object that represents
		/// the converted <paramref name="entry"/>.</returns>
		/// <exception cref="InvalidCastException">
		/// <para><paramref name="entry"/> contains a key that is not compatible
		/// with <see cref="String"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entry"/> contains a value that is not compatible
		/// with <see cref="listviewLayout"/>.</para>
		/// </exception>

		public static implicit operator listviewLayoutEntry(DictionaryEntry entry) {
			listviewLayoutEntry pair = new listviewLayoutEntry();
			if (entry.Key != null) pair.Key = (String) entry.Key;
			if (entry.Value != null) pair.Value = (listviewLayout) entry.Value;
			return pair;
		}

		#endregion
		#region DictionaryEntry(listviewLayoutEntry)

		/// <summary>
		/// Converts a <see cref="listviewLayoutEntry"/> to a <see cref="DictionaryEntry"/>.
		/// </summary>
		/// <param name="pair">A <see cref="listviewLayoutEntry"/> object to convert.</param>
		/// <returns>A <see cref="DictionaryEntry"/> object that
		/// represents the converted <paramref name="pair"/>.</returns>

		public static implicit operator DictionaryEntry(listviewLayoutEntry pair) {
			DictionaryEntry entry = new DictionaryEntry();
			if (pair.Key != null) entry.Key = pair.Key;
			entry.Value = pair.Value;
			return entry;
		}

		#endregion
		#endregion
	}

	#endregion

	#region Class listviewLayoutCollection

	/// <summary>
	/// Implements a strongly typed collection of <see cref="listviewLayoutEntry"/>
	/// key-and-value pairs that are sorted by the keys and are accessible by key and by index.
	/// </summary>
	/// <remarks>
	/// <b>listviewLayoutCollection</b> provides a <see cref="SortedList"/> that is strongly typed
	/// for <see cref="String"/> keys and <see cref="listviewLayout"/> values.
	/// </remarks>

	[Serializable]
	public class listviewLayoutCollection:
		IStringlistviewLayoutDictionary, IDictionary, ICloneable {
		#region Private Fields

		private const int _defaultCapacity = 16;

		private String[] _keys;
		private listviewLayout[] _values;
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

		private listviewLayoutCollection(Tag tag) { }

		#endregion
		#region Public Constructors
		#region listviewLayoutCollection()

		/// <overloads>
		/// Initializes a new instance of the <see cref="listviewLayoutCollection"/> class.
		/// </overloads>
		/// <summary>
		/// Initializes a new instance of the <see cref="listviewLayoutCollection"/> class that is empty,
		/// has the default initial capacity and is sorted according to the <see cref="IComparable"/>
		/// interface implemented by each key added to the <b>listviewLayoutCollection</b>.
		/// </summary>
		/// <remarks>Please refer to <see cref="SortedList()"/> for details.</remarks>

		public listviewLayoutCollection() {
			this._keys = new String[_defaultCapacity];
			this._values = new listviewLayout[_defaultCapacity];
			this._comparer = Comparer.Default;
		}

		#endregion
		#region listviewLayoutCollection(IComparer)

		/// <summary>
		/// Initializes a new instance of the <see cref="listviewLayoutCollection"/> class that is empty,
		/// has the default initial capacity and is sorted according to the specified
		/// <see cref="IComparer"/> interface.
		/// </summary>
		/// <param name="comparer">
		/// <para>The <see cref="IComparer"/> implementation to use when comparing keys.</para>
		/// <para>-or-</para>
		/// <para>A null reference, to use the <see cref="IComparable"/> implementation of each key.
		/// </para></param>
		/// <remarks>Please refer to <see cref="SortedList(IComparer)"/> for details.</remarks>

		public listviewLayoutCollection(IComparer comparer): this() {
			if (comparer != null) this._comparer = comparer;
		}

		#endregion
		#region listviewLayoutCollection(IDictionary)

		/// <summary>
		/// Initializes a new instance of the <see cref="listviewLayoutCollection"/> class that contains
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
		/// with <see cref="listviewLayout"/>.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="SortedList(IDictionary)"/> for details.</remarks>

		public listviewLayoutCollection(IDictionary dictionary): this(dictionary, null) { }

		#endregion
		#region listviewLayoutCollection(Int32)

		/// <summary>
		/// Initializes a new instance of the <see cref="listviewLayoutCollection"/> class that is empty,
		/// has the specified initial capacity and is sorted according to the <see cref="IComparable"/>
		/// interface implemented by each key added to the <b>listviewLayoutCollection</b>.
		/// </summary>
		/// <param name="capacity">The initial number of elements that the
		/// <see cref="listviewLayoutCollection"/> can contain.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="capacity"/> is less than zero.</exception>
		/// <remarks>Please refer to <see cref="SortedList(Int32)"/> for details.</remarks>

		public listviewLayoutCollection(int capacity) {
			if (capacity < 0)
				throw new ArgumentOutOfRangeException("capacity",
					capacity, "Argument cannot be negative.");

			this._keys = new String[capacity];
			this._values = new listviewLayout[capacity];
			this._comparer = Comparer.Default;
		}

		#endregion
		#region listviewLayoutCollection(IComparer, Int32)

		/// <summary>
		/// Initializes a new instance of the <see cref="listviewLayoutCollection"/> class that is empty,
		/// has the specified initial capacity and is sorted according to the specified
		/// <see cref="IComparer"/> interface.
		/// </summary>
		/// <param name="comparer">
		/// <para>The <see cref="IComparer"/> implementation to use when comparing keys.</para>
		/// <para>-or-</para>
		/// <para>A null reference to use the <see cref="IComparable"/> implementation of each key.
		/// </para></param>
		/// <param name="capacity">The initial number of elements that the
		/// <see cref="listviewLayoutCollection"/> can contain.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="capacity"/> is less than zero.</exception>
		/// <remarks>
		/// Please refer to <see cref="SortedList(IComparer, Int32)"/> for details.
		/// </remarks>

		public listviewLayoutCollection(IComparer comparer, int capacity) : this(capacity) {
			if (comparer != null) this._comparer = comparer;
		}

		#endregion
		#region listviewLayoutCollection(IDictionary, IComparer)

		/// <summary>
		/// Initializes a new instance of the <see cref="listviewLayoutCollection"/> class that contains
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
		/// with <see cref="listviewLayout"/>.</para>
		/// </exception>
		/// <remarks>
		/// Please refer to <see cref="SortedList(IDictionary, IComparer)"/> for details.
		/// </remarks>

		public listviewLayoutCollection(IDictionary dictionary, IComparer comparer):
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
		/// Gets or sets the capacity of the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <value>The number of elements that the
		/// <see cref="listviewLayoutCollection"/> can contain.</value>
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
					this._values = new listviewLayout[_defaultCapacity];
					return;
				}

				String[] newKeys = new String[value];
				listviewLayout[] newValues = new listviewLayout[value];

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
		/// in the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <value>The number of key-and-value pairs contained
		/// in the <see cref="listviewLayoutCollection"/>.</value>
		/// <remarks>Please refer to <see cref="SortedList.Count"/> for details.</remarks>

		public virtual int Count {
			get { return this._count; }
		}

		#endregion
		#region IsFixedSize

		/// <summary>
		/// Gets a value indicating whether the <see cref="listviewLayoutCollection"/> has a fixed size.
		/// </summary>
		/// <value><c>true</c> if the <see cref="listviewLayoutCollection"/> has a fixed size;
		/// otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="SortedList.IsFixedSize"/> for details.</remarks>

		public virtual bool IsFixedSize {
			get { return false; }
		}

		#endregion
		#region IsReadOnly

		/// <summary>
		/// Gets a value indicating whether the <see cref="listviewLayoutCollection"/> is read-only.
		/// </summary>
		/// <value><c>true</c> if the <see cref="listviewLayoutCollection"/> is read-only;
		/// otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="SortedList.IsReadOnly"/> for details.</remarks>

		public virtual bool IsReadOnly {
			get { return false; }
		}

		#endregion
		#region IsSynchronized

		/// <summary>
		/// Gets a value indicating whether access to the <see cref="listviewLayoutCollection"/>
		/// is synchronized (thread-safe).
		/// </summary>
		/// <value><c>true</c> if access to the <see cref="listviewLayoutCollection"/> is
		/// synchronized (thread-safe); otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="SortedList.IsSynchronized"/> for details.</remarks>

		public virtual bool IsSynchronized {
			get { return false; }
		}

		#endregion
		#region Item[String]: listviewLayout

		/// <summary>
		/// Gets or sets the <see cref="listviewLayout"/> value
		/// associated with the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key
		/// whose value to get or set.</param>
		/// <value>The <see cref="listviewLayout"/> value associated with the specified
		/// <paramref name="key"/>. If the specified <paramref name="key"/> is not found,
		/// attempting to get it returns
		/// a null reference,
		/// and attempting to set it creates a new element using the specified
		/// <paramref name="key"/>.</value>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The property is set and the <see cref="listviewLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The property is set, <paramref name="key"/> does not exist in the collection,
		/// and the <b>listviewLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.this"/> for details.</remarks>

		public virtual listviewLayout this[String key] {
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


		public virtual listviewLayoutEntry this[int index] {
			get {
				ValidateIndex(index);
				return new listviewLayoutEntry(this._keys[index], this._values[index]);
			}
			set {
				ValidateIndex(index);
				++this._version;
				this._keys[index] = value.Key;
				this._values[index] = value.Value;
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
		/// When set, this value must be compatible with <see cref="listviewLayout"/>.
		/// </value>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidCastException">
		/// <para><paramref name="key"/> is not compatible with <see cref="String"/>.</para>
		/// <para>-or-</para>
		/// <para>The property is set to a value that is not compatible with
		/// <see cref="listviewLayout"/>.</para></exception>
		/// <exception cref="NotSupportedException">
		/// <para>The property is set and the <see cref="listviewLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The property is set, <paramref name="key"/> does not exist in the collection,
		/// and the <b>listviewLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.this"/> for details.</remarks>

		object IDictionary.this[object key] {
			get { return this[(String) key]; }
			set { this[(String) key] = (listviewLayout) value; }
		}

		#endregion
		#region Keys: IStringCollection

		/// <summary>
        /// Gets an ICollection[string] containing
		/// the keys in the <see cref="listviewLayoutCollection"/>.
		/// </summary>
        /// <value>An ICollection[string] containing
		/// the keys in the <see cref="listviewLayoutCollection"/>.</value>
		/// <remarks>Please refer to <see cref="SortedList.Keys"/> for details.</remarks>

		public virtual ICollection<string> Keys {
			get { return GetKeyList(); }
		}

		#endregion
		#region IDictionary.Keys: ICollection

		/// <summary>
		/// Gets an <see cref="ICollection"/> containing
		/// the keys in the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <value>An <see cref="ICollection"/> containing
		/// the keys in the <see cref="listviewLayoutCollection"/>.</value>
		/// <remarks>Please refer to <see cref="SortedList.Keys"/> for details.</remarks>

		ICollection IDictionary.Keys {
			get { return (ICollection) Keys; }
		}

		#endregion
		#region SyncRoot

		/// <summary>
		/// Gets an object that can be used to synchronize
		/// access to the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <value>An object that can be used to synchronize
		/// access to the <see cref="listviewLayoutCollection"/>.</value>
		/// <remarks>Please refer to <see cref="SortedList.SyncRoot"/> for details.</remarks>

		public virtual object SyncRoot {
			get { return this; }
		}

		#endregion
		#region Values: IlistviewLayoutCollection

		/// <summary>
		/// Gets an <see cref="IlistviewLayoutCollection"/> containing
		/// the values in the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <value>An <see cref="IlistviewLayoutCollection"/> containing
		/// the values in the <see cref="listviewLayoutCollection"/>.</value>
		/// <remarks>Please refer to <see cref="SortedList.Values"/> for details.</remarks>

		public virtual IlistviewLayoutCollection Values {
			get { return GetValueList(); }
		}

		#endregion
		#region IDictionary.Values: ICollection

		/// <summary>
		/// Gets an <see cref="ICollection"/> containing
		/// the values in the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <value>An <see cref="ICollection"/> containing
		/// the values in the <see cref="listviewLayoutCollection"/>.</value>
		/// <remarks>Please refer to <see cref="SortedList.Values"/> for details.</remarks>

		ICollection IDictionary.Values {
			get { return (ICollection) Values; }
		}

		#endregion
		#endregion
		#region Public Methods
		#region Add(String, listviewLayout)

		/// <summary>
		/// Adds an element with the specified <see cref="String"/> key and
		/// <see cref="listviewLayout"/> value to the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key of the element
		/// to add to the <see cref="listviewLayoutCollection"/>.</param>
		/// <param name="value">The <see cref="listviewLayout"/> value of the element
		/// to add to the <see cref="listviewLayoutCollection"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentException">
		/// <para>An element with the specified <paramref name="key"/>
		/// already exists in the <see cref="listviewLayoutCollection"/>.</para>
		/// <para>-or-</para>
		/// <para>The <b>listviewLayoutCollection</b> is set to use the <see cref="IComparable"/> interface,
		/// and <paramref name="key"/> does not implement the <b>IComparable</b> interface.</para>
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="listviewLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>listviewLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.Add"/> for details.</remarks>

		public virtual void Add(String key, listviewLayout value) {
			if ((object) key == null)
				throw new ArgumentNullException("key");

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
		/// to the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <param name="key">The key of the element to add to the <see cref="listviewLayoutCollection"/>.
		/// This argument must be compatible with <see cref="String"/>.</param>
		/// <param name="value">The value of the element to add to the <see cref="listviewLayoutCollection"/>.
		/// This argument must be compatible with <see cref="listviewLayout"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentException">
		/// <para>An element with the specified <paramref name="key"/>
		/// already exists in the <see cref="listviewLayoutCollection"/>.</para>
		/// <para>-or-</para>
		/// <para>The <b>listviewLayoutCollection</b> is set to use the <see cref="IComparable"/> interface,
		/// and <paramref name="key"/> does not implement the <b>IComparable</b> interface.</para>
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidCastException">
		/// <para><paramref name="key"/> is not compatible with <see cref="String"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="value"/> is not compatible with <see cref="listviewLayout"/>.</para>
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="listviewLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>listviewLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.Add"/> for details.</remarks>

		void IDictionary.Add(object key, object value) {
			Add((String) key, (listviewLayout) value);
		}

		#endregion
		#region Clear

		/// <summary>
		/// Removes all elements from the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="listviewLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>listviewLayoutCollection</b> has a fixed size.</para></exception>
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
		/// Creates a shallow copy of the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <returns>A shallow copy of the <see cref="listviewLayoutCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.Clone"/> for details.</remarks>

		public virtual object Clone() {
			listviewLayoutCollection dictionary = new listviewLayoutCollection(this._count);

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
		/// Determines whether the <see cref="listviewLayoutCollection"/>
		/// contains the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key
		/// to locate in the <see cref="listviewLayoutCollection"/>.</param>
		/// <returns><c>true</c> if the <see cref="listviewLayoutCollection"/> contains an element
		/// with the specified <paramref name="key"/>; otherwise, <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <remarks>Please refer to <see cref="SortedList.Contains"/> for details.</remarks>
		public virtual bool Contains(String key) {
			return (IndexOfKey(key) >= 0);
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
		/// Determines whether the <see cref="listviewLayoutCollection"/> contains the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="listviewLayoutCollection"/>.
		/// This argument must be compatible with <see cref="String"/>.</param>
		/// <returns><c>true</c> if the <see cref="listviewLayoutCollection"/> contains an element
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
		/// Determines whether the <see cref="listviewLayoutCollection"/>
		/// contains the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key
		/// to locate in the <see cref="listviewLayoutCollection"/>.</param>
		/// <returns><c>true</c> if the <see cref="listviewLayoutCollection"/> contains an element
		/// with the specified <paramref name="key"/>; otherwise, <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <remarks>Please refer to <see cref="SortedList.ContainsKey"/> for details.</remarks>

		public virtual bool ContainsKey(String key) {
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

			string feedUrl = key.ToString();
			if (!ContainsKey(feedUrl) && (key.IsFile || key.IsUnc)) {
				feedUrl = key.LocalPath;
			}

			return ContainsKey(feedUrl);
		
		}
		#endregion
		#region ContainsValue

		/// <summary>
		/// Determines whether the <see cref="listviewLayoutCollection"/>
		/// contains the specified <see cref="listviewLayout"/> value.
		/// </summary>
		/// <param name="value">The <see cref="listviewLayout"/> value
		/// to locate in the <see cref="listviewLayoutCollection"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns><c>true</c> if the <see cref="listviewLayoutCollection"/> contains an element
		/// with the specified <paramref name="value"/>; otherwise, <c>false</c>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.ContainsValue"/> for details.</remarks>

		public virtual bool ContainsValue(listviewLayout value) {
			return (IndexOfValue(value) >= 0);
		}

		#endregion
		#region CopyTo(listviewLayoutEntry[], Int32)

		/// <summary>
		/// Copies the entire <see cref="listviewLayoutCollection"/> to a one-dimensional <see cref="Array"/> of
		/// <see cref="listviewLayoutEntry"/> elements, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
		/// <see cref="listviewLayoutEntry"/> elements copied from the <see cref="listviewLayoutCollection"/>.
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
		/// The number of elements in the source <see cref="listviewLayoutCollection"/> is greater than
		/// the available space from <paramref name="arrayIndex"/> to the end of the destination
		/// <paramref name="array"/>.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.CopyTo"/> for details.</remarks>

		public virtual void CopyTo(listviewLayoutEntry[] array, int arrayIndex) {
			CheckTargetArray(array, arrayIndex);

			for (int i = 0; i < this._count; i++) {
				listviewLayoutEntry entry =
					new listviewLayoutEntry(this._keys[i], this._values[i]);

				array.SetValue(entry, arrayIndex + i);
			}
		}

		#endregion
		#region ICollection.CopyTo(Array, Int32)

		/// <summary>
		/// Copies the entire <see cref="listviewLayoutCollection"/> to a one-dimensional <see cref="Array"/>,
		/// starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
		/// <see cref="listviewLayoutEntry"/> elements copied from the <see cref="listviewLayoutCollection"/>.
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
		/// The number of elements in the source <see cref="listviewLayoutCollection"/> is greater than
		/// the available space from <paramref name="arrayIndex"/> to the end of the destination
		/// <paramref name="array"/>.</para></exception>
		/// <exception cref="InvalidCastException">
		/// The <see cref="listviewLayoutEntry"/> type cannot be cast automatically
		/// to the type of the destination <paramref name="array"/>.</exception>
		/// <remarks>Please refer to <see cref="SortedList.CopyTo"/> for details.</remarks>

		void ICollection.CopyTo(Array array, int arrayIndex) {
			CopyTo((listviewLayoutEntry[]) array, arrayIndex);
		}

		#endregion
		#region GetByIndex

		/// <summary>
		/// Gets the <see cref="listviewLayout"/> value at the
		/// specified index of the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="listviewLayout"/> value to get.</param>
		/// <returns>The <see cref="listviewLayout"/> value at the specified
		/// <paramref name="index"/> of the <see cref="listviewLayoutCollection"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="SortedList.GetByIndex"/> for details.</remarks>

		public virtual listviewLayout GetByIndex(int index) {
			ValidateIndex(index);
			return this._values[index];
		}

		#endregion
		#region GetEnumerator: IStringlistviewLayoutEnumerator

		/// <summary>
		/// Returns an <see cref="IStringlistviewLayoutEnumerator"/> that can
		/// iterate through the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IStringlistviewLayoutEnumerator"/>
		/// for the entire <see cref="listviewLayoutCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.GetEnumerator"/> for details.</remarks>

		public virtual IStringlistviewLayoutEnumerator GetEnumerator() {
			return new Enumerator(this);
		}

		#endregion
		#region IDictionary.GetEnumerator: IDictionaryEnumerator

		/// <summary>
		/// Returns an <see cref="IDictionaryEnumerator"/> that can
		/// iterate through the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IDictionaryEnumerator"/>
		/// for the entire <see cref="listviewLayoutCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.GetEnumerator"/> for details.</remarks>

		IDictionaryEnumerator IDictionary.GetEnumerator() {
			return (IDictionaryEnumerator) GetEnumerator();
		}

		#endregion
		#region IEnumerable.GetEnumerator: IEnumerator

		/// <summary>
		/// Returns an <see cref="IEnumerator"/> that can
		/// iterate through the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IEnumerator"/>
		/// for the entire <see cref="listviewLayoutCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.GetEnumerator"/> for details.</remarks>

		IEnumerator IEnumerable.GetEnumerator() {
			return (IEnumerator) GetEnumerator();
		}

		#endregion
		#region GetKey

		/// <summary>
		/// Gets the <see cref="String"/> key at the
		/// specified index of the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="String"/> key to get.</param>
		/// <returns>The <see cref="String"/> key at the specified
		/// <paramref name="index"/> of the <see cref="listviewLayoutCollection"/>.</returns>
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
		/// Gets the keys in the <see cref="listviewLayoutCollection"/>.
		/// </summary>
        /// <returns>An ICollection[string] containing the keys
		/// in the <see cref="listviewLayoutCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.GetKeyList"/> for details.</remarks>

		public virtual ICollection<String> GetKeyList() {
			if (this._keyList == null)
				this._keyList = new KeyList(this);
			return this._keyList;
		}

		#endregion
		#region GetValueList

		/// <summary>
		/// Gets the values in the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IlistviewLayoutList"/> containing the values
		/// in the <see cref="listviewLayoutCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.GetValueList"/> for details.</remarks>

		public virtual IlistviewLayoutList GetValueList() {
			if (this._valueList == null)
				this._valueList = new ValueList(this);
			return this._valueList;
		}

		#endregion
		#region IndexOfKey

		/// <summary>
		/// Returns the zero-based index of the specified <see cref="String"/>
		/// key in the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key
		/// to locate in the <see cref="listviewLayoutCollection"/>.</param>
		/// <returns>The zero-based index of <paramref name="key"/> in the
		/// <see cref="listviewLayoutCollection"/>, if found; otherwise, -1.</returns>
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
		/// Returns the zero-based index of the specified <see cref="listviewLayout"/>
		/// value in the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <param name="value">The <see cref="listviewLayout"/> value
		/// to locate in the <see cref="listviewLayoutCollection"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns>The zero-based index of <paramref name="value"/> in the
		/// <see cref="listviewLayoutCollection"/>, if found; otherwise, -1.</returns>
		/// <remarks>Please refer to <see cref="SortedList.IndexOfValue"/> for details.</remarks>

		public virtual int IndexOfValue(listviewLayout value) {
			return Array.IndexOf(this._values, value, 0, this._count);
		}

		#endregion
		#region Remove(String)

		/// <summary>
		/// Removes the element with the specified <see cref="String"/> key
		/// from the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key of the element
		/// to remove from the <see cref="listviewLayoutCollection"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="listviewLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>listviewLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.Remove"/> for details.</remarks>

		public virtual void Remove(String key) {
			int index = IndexOfKey(key);
			if (index >= 0) RemoveAt(index);
		}

		#endregion
		#region IDictionary.Remove(Object)

		/// <summary>
		/// Removes the element with the specified key from the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <param name="key">The key of the element to remove from the <see cref="listviewLayoutCollection"/>.
		/// This argument must be compatible with <see cref="String"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidCastException"><paramref name="key"/>
		/// is not compatible with <see cref="String"/>.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="listviewLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>listviewLayoutCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.Remove"/> for details.</remarks>

		void IDictionary.Remove(object key) {
			Remove((String) key);
		}

		#endregion
		#region RemoveAt

		/// <summary>
		/// Removes the element at the specified index of the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="listviewLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>listviewLayoutCollection</b> has a fixed size.</para></exception>
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
		/// Sets the <see cref="listviewLayout"/> value at the
		/// specified index of the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="listviewLayout"/> value to set.</param>
		/// <param name="value">The <see cref="listviewLayout"/> object to store
		/// at the specified <paramref name="index"/> of the <see cref="listviewLayoutCollection"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="SortedList.SetByIndex"/> for details.</remarks>

		public virtual void SetByIndex(int index, listviewLayout value) {
			ValidateIndex(index);
			++this._version;
			this._values[index] = value;
		}

		#endregion
		#region Synchronized

		/// <summary>
		/// Returns a synchronized (thread-safe) wrapper
		/// for the specified <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <param name="dictionary">The <see cref="listviewLayoutCollection"/> to synchronize.</param>
		/// <returns>
		/// A synchronized (thread-safe) wrapper around <paramref name="dictionary"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="collection"/> is a null reference.</exception>
		/// <remarks>Please refer to <see cref="SortedList.Synchronized"/> for details.</remarks>

		public static listviewLayoutCollection Synchronized(listviewLayoutCollection dictionary) {
			if (dictionary == null)
				throw new ArgumentNullException("dictionary");

			return new SyncDictionary(dictionary);
		}

		#endregion
		#region TrimToSize

		/// <summary>
		/// Sets the capacity to the actual number of elements in the <see cref="listviewLayoutCollection"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="listviewLayoutCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>listviewLayoutCollection</b> has a fixed size.</para></exception>
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
			String key, listviewLayout value) {

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
			IStringlistviewLayoutEnumerator, IDictionaryEnumerator {
			#region Private Fields

			private readonly listviewLayoutCollection _dictionary;
			private readonly int _version;
			private int _index;

			#endregion
			#region Internal Constructors

			internal Enumerator(listviewLayoutCollection dictionary) {
				this._dictionary = dictionary;
				this._version = dictionary._version;
				this._index = -1;
			}

			#endregion
			#region Public Properties

			public listviewLayoutEntry Current {
				get { return Entry; }
			}

			object IEnumerator.Current {
				get { return (DictionaryEntry) Entry; }
			}

			public listviewLayoutEntry Entry {
				get {
					this._dictionary.CheckEnumIndex(this._index);
					this._dictionary.CheckEnumVersion(this._version);

					return new listviewLayoutEntry(
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

			public listviewLayout Value {
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

			private listviewLayoutCollection _dictionary;

			#endregion
			#region Internal Constructors

			internal KeyList(listviewLayoutCollection dictionary) {
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

			string IList<string>.this[int index] {
				get { return this[index]; }
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
                return _dictionary.Keys.GetEnumerator();
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

		#region Class ValueList

		[Serializable]
			private sealed class ValueList: IlistviewLayoutList, IList {
			#region Private Fields

			private listviewLayoutCollection _dictionary;

			#endregion
			#region Internal Constructors

			internal ValueList(listviewLayoutCollection dictionary) {
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

			public listviewLayout this[int index] {
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

			public int Add(listviewLayout value) {
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

			public bool Contains(listviewLayout value) {
				return this._dictionary.ContainsValue(value);
			}

			bool IList.Contains(object value) {
				return Contains((listviewLayout) value);
			}

			public void CopyTo(listviewLayout[] array, int arrayIndex) {
				this._dictionary.CheckTargetArray(array, arrayIndex);
				Array.Copy(this._dictionary._values, 0,
					array, arrayIndex, this._dictionary.Count);
			}

			void ICollection.CopyTo(Array array, int arrayIndex) {
				this._dictionary.CheckTargetArray(array, arrayIndex);
				CopyTo((listviewLayout[]) array, arrayIndex);
			}

			public IlistviewLayoutEnumerator GetEnumerator() {
				return new ValueEnumerator(this._dictionary);
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return (IEnumerator) GetEnumerator();
			}

			public int IndexOf(listviewLayout value) {
				return this._dictionary.IndexOfValue(value);
			}

			int IList.IndexOf(object value) {
				return IndexOf((listviewLayout) value);
			}

			public void Insert(int index, listviewLayout value) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			void IList.Insert(int index, object value) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			public void Remove(listviewLayout value) {
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
			IlistviewLayoutEnumerator, IEnumerator {
			#region Private Fields

			private readonly listviewLayoutCollection _dictionary;
			private readonly int _version;
			private int _index;

			#endregion
			#region Internal Constructors

			internal ValueEnumerator(listviewLayoutCollection dictionary) {
				this._dictionary = dictionary;
				this._version = dictionary._version;
				this._index = -1;
			}

			#endregion
			#region Public Properties

			public listviewLayout Current {
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
			private sealed class SyncDictionary: listviewLayoutCollection {
			#region Private Fields

			private listviewLayoutCollection _dictionary;
			private object _root;

			#endregion
			#region Internal Constructors

			internal SyncDictionary(listviewLayoutCollection dictionary):
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

			public override listviewLayout this[String key] {
				get { lock (this._root) return this._dictionary[key]; }
				set { lock (this._root) this._dictionary[key] = value; }
			}

			public override ICollection<string> Keys {
				get { lock (this._root) return this._dictionary.Keys; }
			}

			public override object SyncRoot {
				get { return this._root; }
			}

			public override IlistviewLayoutCollection Values {
				get { lock (this._root) return this._dictionary.Values; }
			}

			#endregion
			#region Public Methods

			public override void Add(String key, listviewLayout value) {
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

			public override bool ContainsValue(listviewLayout value) {
				lock (this._root) return this._dictionary.ContainsValue(value);
			}

			public override void CopyTo(listviewLayoutEntry[] array, int index) {
				lock (this._root) this._dictionary.CopyTo(array, index);
			}

			public override listviewLayout GetByIndex(int index) {
				lock (this._root) return this._dictionary.GetByIndex(index);
			}

			public override IStringlistviewLayoutEnumerator GetEnumerator() {
				lock (this._root) return this._dictionary.GetEnumerator();
			}

			public override String GetKey(int index) {
				lock (this._root) return this._dictionary.GetKey(index);
			}

			public override ICollection<string> GetKeyList() {
				lock (this._root) return this._dictionary.GetKeyList();
			}

			public override IlistviewLayoutList GetValueList() {
				lock (this._root) return this._dictionary.GetValueList();
			}

			public override int IndexOfKey(String key) {
				lock (this._root) return this._dictionary.IndexOfKey(key);
			}

			public override int IndexOfValue(listviewLayout value) {
				lock (this._root) return this._dictionary.IndexOfValue(value);
			}

			public override void Remove(String key) {
				lock (this._root) this._dictionary.Remove(key);
			}

			public override void RemoveAt(int index) {
				lock (this._root) this._dictionary.RemoveAt(index);
			}

			public override void SetByIndex(int index, listviewLayout value) {
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
