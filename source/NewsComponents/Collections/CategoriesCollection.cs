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

namespace NewsComponents.Collections {
	#region Interface IcategoryCollection

	/// <summary>
	/// Defines size, enumerators, and synchronization methods for strongly
	/// typed collections of <see cref="category"/> elements.
	/// </summary>
	/// <remarks>
	/// <b>IcategoryCollection</b> provides an <see cref="ICollection"/>
	/// that is strongly typed for <see cref="category"/> elements.
	/// </remarks>

	public interface IcategoryCollection {
		#region Properties
		#region Count

		/// <summary>
		/// Gets the number of elements contained in the
		/// <see cref="IcategoryCollection"/>.
		/// </summary>
		/// <value>The number of elements contained in the
		/// <see cref="IcategoryCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ICollection.Count"/> for details.</remarks>

		int Count { get; }

		#endregion
		#region IsSynchronized

		/// <summary>
		/// Gets a value indicating whether access to the
		/// <see cref="IcategoryCollection"/> is synchronized (thread-safe).
		/// </summary>
		/// <value><c>true</c> if access to the <see cref="IcategoryCollection"/> is
		/// synchronized (thread-safe); otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="ICollection.IsSynchronized"/> for details.</remarks>

		bool IsSynchronized { get; }

		#endregion
		#region SyncRoot

		/// <summary>
		/// Gets an object that can be used to synchronize access
		/// to the <see cref="IcategoryCollection"/>.
		/// </summary>
		/// <value>An object that can be used to synchronize access
		/// to the <see cref="IcategoryCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ICollection.SyncRoot"/> for details.</remarks>

		object SyncRoot { get; }

		#endregion
		#endregion
		#region Methods
		#region CopyTo

		/// <summary>
		/// Copies the entire <see cref="IcategoryCollection"/> to a one-dimensional <see cref="Array"/>
		/// of <see cref="category"/> elements, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
		/// <see cref="category"/> elements copied from the <see cref="IcategoryCollection"/>.
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
		/// The number of elements in the source <see cref="IcategoryCollection"/> is greater
		/// than the available space from <paramref name="arrayIndex"/> to the end of the destination
		/// <paramref name="array"/>.</para></exception>
		/// <remarks>Please refer to <see cref="ICollection.CopyTo"/> for details.</remarks>

		void CopyTo(category[] array, int arrayIndex);

		#endregion
		#region GetEnumerator

		/// <summary>
		/// Returns an <see cref="IcategoryEnumerator"/> that can
		/// iterate through the <see cref="IcategoryCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IcategoryEnumerator"/>
		/// for the entire <see cref="IcategoryCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="IEnumerable.GetEnumerator"/> for details.</remarks>

		IcategoryEnumerator GetEnumerator();

		#endregion
		#endregion
	}

	#endregion
	#region Interface IcategoryList

	/// <summary>
	/// Represents a strongly typed collection of <see cref="category"/>
	/// objects that can be individually accessed by index.
	/// </summary>
	/// <remarks>
	/// <b>IcategoryList</b> provides an <see cref="IList"/>
	/// that is strongly typed for <see cref="category"/> elements.
	/// </remarks>

	public interface
		IcategoryList: IcategoryCollection {
		#region Properties
		#region IsFixedSize

		/// <summary>
		/// Gets a value indicating whether the <see cref="IcategoryList"/> has a fixed size.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IcategoryList"/> has a fixed size;
		/// otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IList.IsFixedSize"/> for details.</remarks>

		bool IsFixedSize { get; }

		#endregion
		#region IsReadOnly

		/// <summary>
		/// Gets a value indicating whether the <see cref="IcategoryList"/> is read-only.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IcategoryList"/> is read-only;
		/// otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IList.IsReadOnly"/> for details.</remarks>

		bool IsReadOnly { get; }

		#endregion
		#region Item

		/// <summary>
		/// Gets or sets the <see cref="category"/> element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="category"/> element to get or set.</param>
		/// <value>
		/// The <see cref="category"/> element at the specified <paramref name="index"/>.
		/// </value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than
		/// <see cref="IcategoryCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The property is set and the <see cref="IcategoryList"/> is read-only.</exception>
		/// <remarks>Please refer to <see cref="IList.this"/> for details.</remarks>

		category this[int index] { get; set; }

		#endregion
		#endregion
		#region Methods
		#region Add

		/// <summary>
		/// Adds a <see cref="category"/> to the end
		/// of the <see cref="IcategoryList"/>.
		/// </summary>
		/// <param name="value">The <see cref="category"/> object
		/// to be added to the end of the <see cref="IcategoryList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns>The <see cref="IcategoryList"/> index at which
		/// the <paramref name="value"/> has been added.</returns>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IcategoryList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IcategoryList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Add"/> for details.</remarks>

		int Add(category value);

		#endregion
		#region Clear

		/// <summary>
		/// Removes all elements from the <see cref="IcategoryList"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IcategoryList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IcategoryList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Clear"/> for details.</remarks>

		void Clear();

		#endregion
		#region Contains

		/// <summary>
		/// Determines whether the <see cref="IcategoryList"/>
		/// contains the specified <see cref="category"/> element.
		/// </summary>
		/// <param name="value">The <see cref="category"/> object
		/// to locate in the <see cref="IcategoryList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns><c>true</c> if <paramref name="value"/> is found in the
		/// <see cref="IcategoryList"/>; otherwise, <c>false</c>.</returns>
		/// <remarks>Please refer to <see cref="IList.Contains"/> for details.</remarks>

		bool Contains(category value);

		#endregion
		#region IndexOf

		/// <summary>
		/// Returns the zero-based index of the first occurrence of the specified
		/// <see cref="category"/> in the <see cref="IcategoryList"/>.
		/// </summary>
		/// <param name="value">The <see cref="category"/> object
		/// to locate in the <see cref="IcategoryList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="value"/>
		/// in the <see cref="IcategoryList"/>, if found; otherwise, -1.
		/// </returns>
		/// <remarks>Please refer to <see cref="IList.IndexOf"/> for details.</remarks>

		int IndexOf(category value);

		#endregion
		#region Insert

		/// <summary>
		/// Inserts a <see cref="category"/> element into the
		/// <see cref="IcategoryList"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which
		/// <paramref name="value"/> should be inserted.</param>
		/// <param name="value">The <see cref="category"/> object
		/// to insert into the <see cref="IcategoryList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is greater than
		/// <see cref="IcategoryCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IcategoryList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IcategoryList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Insert"/> for details.</remarks>

		void Insert(int index, category value);

		#endregion
		#region Remove

		/// <summary>
		/// Removes the first occurrence of the specified <see cref="category"/>
		/// from the <see cref="IcategoryList"/>.
		/// </summary>
		/// <param name="value">The <see cref="category"/> object
		/// to remove from the <see cref="IcategoryList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IcategoryList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IcategoryList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Remove"/> for details.</remarks>

		void Remove(category value);

		#endregion
		#region RemoveAt

		/// <summary>
		/// Removes the element at the specified index of the
		/// <see cref="IcategoryList"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than
		/// <see cref="IcategoryCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IcategoryList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IcategoryList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.RemoveAt"/> for details.</remarks>

		void RemoveAt(int index);

		#endregion
		#endregion
	}

	#endregion
	#region Interface IcategoryEnumerator

	/// <summary>
	/// Supports type-safe iteration over a collection that
	/// contains <see cref="category"/> elements.
	/// </summary>
	/// <remarks>
	/// <b>IcategoryEnumerator</b> provides an <see cref="IEnumerator"/>
	/// that is strongly typed for <see cref="category"/> elements.
	/// </remarks>

	public interface IcategoryEnumerator {
		#region Properties
		#region Current

		/// <summary>
		/// Gets the current <see cref="category"/> element in the collection.
		/// </summary>
		/// <value>The current <see cref="category"/> element in the collection.</value>
		/// <exception cref="InvalidOperationException"><para>The enumerator is positioned
		/// before the first element of the collection or after the last element.</para>
		/// <para>-or-</para>
		/// <para>The collection was modified after the enumerator was created.</para></exception>
		/// <remarks>Please refer to <see cref="IEnumerator.Current"/> for details, but note
		/// that <b>Current</b> fails if the collection was modified since the last successful
		/// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>

		category Current { get; }

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
	#region Interface IStringcategoryCollection

	/// <summary>
	/// Defines size, enumerators, and synchronization methods for strongly
	/// typed collections of <see cref="CategoryEntry"/> elements.
	/// </summary>
	/// <remarks>
	/// <b>IStringcategoryCollection</b> provides an <see cref="ICollection"/>
	/// that is strongly typed for <see cref="CategoryEntry"/> elements.
	/// </remarks>

	public interface IStringcategoryCollection {
		#region Properties
		#region Count

		/// <summary>
		/// Gets the number of elements contained in the
		/// <see cref="IStringcategoryCollection"/>.
		/// </summary>
		/// <value>The number of elements contained in the
		/// <see cref="IStringcategoryCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ICollection.Count"/> for details.</remarks>

		int Count { get; }

		#endregion
		#region IsSynchronized

		/// <summary>
		/// Gets a value indicating whether access to the
		/// <see cref="IStringcategoryCollection"/> is synchronized (thread-safe).
		/// </summary>
		/// <value><c>true</c> if access to the <see cref="IStringcategoryCollection"/>
		/// is synchronized (thread-safe); otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="ICollection.IsSynchronized"/> for details.</remarks>

		bool IsSynchronized { get; }

		#endregion
		#region SyncRoot

		/// <summary>
		/// Gets an object that can be used to synchronize access
		/// to the <see cref="IStringcategoryCollection"/>.
		/// </summary>
		/// <value>An object that can be used to synchronize access to the
		/// <see cref="IStringcategoryCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ICollection.SyncRoot"/> for details.</remarks>

		object SyncRoot { get; }

		#endregion
		#endregion
		#region Methods
		#region CopyTo

		/// <summary>
		/// Copies the entire <see cref="IStringcategoryCollection"/>
		/// to a one-dimensional <see cref="Array"/> of <see cref="CategoryEntry"/> elements,
		/// starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the
		/// destination of the <see cref="CategoryEntry"/> elements copied from the
		/// <see cref="IStringcategoryCollection"/>.
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
		/// The number of elements in the source <see cref="IStringcategoryCollection"/>
		/// is greater than the available space from <paramref name="arrayIndex"/> to the end of the
		/// destination <paramref name="array"/>.</para></exception>
		/// <remarks>Please refer to <see cref="ICollection.CopyTo"/> for details.</remarks>

		void CopyTo(CategoryEntry[] array, int arrayIndex);

		#endregion
		#region GetEnumerator

		/// <summary>
		/// Returns an <see cref="IStringcategoryEnumerator"/> that can
		/// iterate through the <see cref="IStringcategoryCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IStringcategoryEnumerator"/>
		/// for the entire <see cref="IStringcategoryCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="IEnumerable.GetEnumerator"/> for details.</remarks>

		IStringcategoryEnumerator GetEnumerator();

		#endregion
		#endregion
	}

	#endregion
	#region Interface IStringcategoryDictionary

	/// <summary>
	/// Represents a strongly typed collection of
	/// <see cref="CategoryEntry"/> key-and-value pairs.
	/// </summary>
	/// <remarks>
	/// <b>IStringcategoryDictionary</b> provides an
	/// <see cref="IDictionary"/> that is strongly typed for
	/// <see cref="String"/> keys and <see cref="category"/> values.
	/// </remarks>

	public interface
		IStringcategoryDictionary: IStringcategoryCollection {
		#region Properties
		#region IsFixedSize

		/// <summary>
		/// Gets a value indicating whether the
		/// <see cref="IStringcategoryDictionary"/> has a fixed size.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IStringcategoryDictionary"/>
		/// has a fixed size; otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IDictionary.IsFixedSize"/> for details.</remarks>

		bool IsFixedSize { get; }

		#endregion
		#region IsReadOnly

		/// <summary>
		/// Gets a value indicating whether the
		/// <see cref="IStringcategoryDictionary"/> is read-only.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IStringcategoryDictionary"/>
		/// is read-only; otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IDictionary.IsReadOnly"/> for details.</remarks>

		bool IsReadOnly { get; }

		#endregion
		#region Item

		/// <summary>
		/// Gets or sets the <see cref="category"/> value
		/// associated with the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key
		/// whose value to get or set.</param>
		/// <value>The <see cref="category"/> value associated with the specified
		/// <paramref name="key"/>. If the specified <paramref name="key"/> is not found,
		/// attempting to get it returns
		/// a null reference,
		/// and attempting to set it creates a new element using the specified
		/// <paramref name="key"/>.</value>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The property is set and the
		/// <see cref="IStringcategoryDictionary"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The property is set, <paramref name="key"/> does not exist in the collection,
		/// and the <b>IStringcategoryDictionary</b> has a fixed size.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="IDictionary.this"/> for details.</remarks>

		category this[String key] { get; set; }

		#endregion
		#region Keys

		/// <summary>
		/// Gets an <see cref="IStringCollection"/> containing the keys
		/// in the <see cref="IStringcategoryDictionary"/>.
		/// </summary>
		/// <value>An <see cref="IStringCollection"/> containing the keys
		/// in the <see cref="IStringcategoryDictionary"/>.</value>
		/// <remarks>Please refer to <see cref="IDictionary.Keys"/> for details.</remarks>

		IStringCollection Keys { get; }

		#endregion
		#region Values

		/// <summary>
		/// Gets an <see cref="IcategoryCollection"/> containing the values
		/// in the <see cref="IStringcategoryDictionary"/>.
		/// </summary>
		/// <value>An <see cref="IcategoryCollection"/> containing the values
		/// in the <see cref="IStringcategoryDictionary"/>.</value>
		/// <remarks>Please refer to <see cref="IDictionary.Values"/> for details.</remarks>

		IcategoryCollection Values { get; }

		#endregion
		#endregion
		#region Methods
		#region Add

		/// <summary>
		/// Adds an element with the specified <see cref="String"/>
		/// key and <see cref="category"/> value to the
		/// <see cref="IStringcategoryDictionary"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key of the element
		/// to add to the <see cref="IStringcategoryDictionary"/>.</param>
		/// <param name="value">The <see cref="category"/> value of the element
		/// to add to the <see cref="IStringcategoryDictionary"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentException">
		/// <para>An element with the specified <paramref name="key"/> already exists
		/// in the <see cref="IStringcategoryDictionary"/>.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringcategoryDictionary</b> is set to use the
		/// <see cref="IComparable"/> interface, and <paramref name="key"/> does not
		/// implement the <b>IComparable</b> interface.</para></exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringcategoryDictionary"/> is read-only.
		/// </para><para>-or-</para>
		/// <para>The <b>IStringcategoryDictionary</b> has a fixed size.
		/// </para></exception>
		/// <remarks>Please refer to <see cref="IDictionary.Add"/> for details.</remarks>

		void Add(String key, category value);

		#endregion
		#region Clear

		/// <summary>
		/// Removes all elements from the <see cref="IStringcategoryDictionary"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringcategoryDictionary"/> is read-only.
		/// </para><para>-or-</para>
		/// <para>The <b>IStringcategoryDictionary</b> has a fixed size.
		/// </para></exception>
		/// <remarks>Please refer to <see cref="IDictionary.Clear"/> for details.</remarks>

		void Clear();

		#endregion
		#region Contains

		/// <summary>
		/// Determines whether the <see cref="IStringcategoryDictionary"/>
		/// contains the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key to locate
		/// in the <see cref="IStringcategoryDictionary"/>.</param>
		/// <returns><c>true</c> if the <see cref="IStringcategoryDictionary"/>
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
		/// from the <see cref="IStringcategoryDictionary"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key of the element to remove
		/// from the <see cref="IStringcategoryDictionary"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="InvalidOperationException">
		/// The comparer throws an exception.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringcategoryDictionary"/> is read-only.
		/// </para><para>-or-</para>
		/// <para>The <b>IStringcategoryDictionary</b> has a fixed size.
		/// </para></exception>
		/// <remarks>Please refer to <see cref="IDictionary.Remove"/> for details.</remarks>

		void Remove(String key);

		#endregion
		#endregion
	}

	#endregion
	#region Interface IStringcategoryList

	/// <summary>
	/// Represents a strongly typed collection of <see cref="CategoryEntry"/>
	/// objects that can be individually accessed by index.
	/// </summary>
	/// <remarks>
	/// <b>IStringcategoryList</b> provides an <see cref="IList"/>
	/// that is strongly typed for <see cref="CategoryEntry"/> elements.
	/// </remarks>

	public interface
		IStringcategoryList: IStringcategoryCollection {
		#region Properties
		#region IsFixedSize

		/// <summary>
		/// Gets a value indicating whether the
		/// <see cref="IStringcategoryList"/> has a fixed size.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IStringcategoryList"/>
		/// has a fixed size; otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IList.IsFixedSize"/> for details.</remarks>

		bool IsFixedSize { get; }

		#endregion
		#region IsReadOnly

		/// <summary>
		/// Gets a value indicating whether the
		/// <see cref="IStringcategoryList"/> is read-only.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IStringcategoryList"/>
		/// is read-only; otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IList.IsReadOnly"/> for details.</remarks>

		bool IsReadOnly { get; }

		#endregion
		#region Item

		/// <summary>
		/// Gets or sets the <see cref="CategoryEntry"/> element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="CategoryEntry"/> element to get or set.</param>
		/// <value>
		/// The <see cref="CategoryEntry"/> element at the specified <paramref name="index"/>.
		/// </value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than
		/// <see cref="IStringcategoryCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">The property is set and the
		/// <see cref="IStringcategoryList"/> is read-only.</exception>
		/// <remarks>Please refer to <see cref="IList.this"/> for details.</remarks>

		CategoryEntry this[int index] { get; set; }

		#endregion
		#endregion
		#region Methods
		#region Add

		/// <summary>
		/// Adds a <see cref="CategoryEntry"/> to the end
		/// of the <see cref="IStringcategoryList"/>.
		/// </summary>
		/// <param name="entry">The <see cref="CategoryEntry"/> object
		/// to be added to the end of the <see cref="IStringcategoryList"/>.
		/// </param>
		/// <returns>The <see cref="IStringcategoryList"/> index at which
		/// the <paramref name="entry"/> has been added.</returns>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringcategoryList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringcategoryList</b> has a fixed size.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="IList.Add"/> for details.</remarks>

		int Add(CategoryEntry entry);

		#endregion
		#region Clear

		/// <summary>
		/// Removes all elements from the <see cref="IStringcategoryList"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringcategoryList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringcategoryList</b> has a fixed size.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="IList.Clear"/> for details.</remarks>

		void Clear();

		#endregion
		#region Contains

		/// <summary>
		/// Determines whether the <see cref="IStringcategoryList"/>
		/// contains the specified <see cref="CategoryEntry"/> element.
		/// </summary>
		/// <param name="entry">The <see cref="CategoryEntry"/> object
		/// to locate in the <see cref="IStringcategoryList"/>.</param>
		/// <returns><c>true</c> if <paramref name="entry"/> is found in the
		/// <see cref="IStringcategoryList"/>; otherwise, <c>false</c>.</returns>
		/// <remarks>Please refer to <see cref="IList.Contains"/> for details.</remarks>

		bool Contains(CategoryEntry entry);

		#endregion
		#region IndexOf

		/// <summary>
		/// Returns the zero-based index of the first occurrence of the specified
		/// <see cref="CategoryEntry"/> in the <see cref="IStringcategoryList"/>.
		/// </summary>
		/// <param name="entry">The <see cref="CategoryEntry"/> object
		/// to locate in the <see cref="IStringcategoryList"/>.</param>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="entry"/>
		/// in the <see cref="IStringcategoryList"/>, if found; otherwise, -1.
		/// </returns>
		/// <remarks>Please refer to <see cref="IList.IndexOf"/> for details.</remarks>

		int IndexOf(CategoryEntry entry);

		#endregion
		#region Insert

		/// <summary>
		/// Inserts a <see cref="CategoryEntry"/> element into the
		/// <see cref="IStringcategoryList"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which
		/// <paramref name="entry"/> should be inserted.</param>
		/// <param name="entry">The <see cref="CategoryEntry"/> object to insert
		/// into the <see cref="IStringcategoryList"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is greater than
		/// <see cref="IStringcategoryCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringcategoryList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringcategoryList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Insert"/> for details.</remarks>

		void Insert(int index, CategoryEntry entry);

		#endregion
		#region Remove

		/// <summary>
		/// Removes the first occurrence of the specified <see cref="CategoryEntry"/>
		/// from the <see cref="IStringcategoryList"/>.
		/// </summary>
		/// <param name="entry">The <see cref="CategoryEntry"/> object to remove
		/// from the <see cref="IStringcategoryList"/>.</param>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringcategoryList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringcategoryList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Remove"/> for details.</remarks>

		void Remove(CategoryEntry entry);

		#endregion
		#region RemoveAt

		/// <summary>
		/// Removes the element at the specified index of the
		/// <see cref="IStringcategoryList"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than
		/// <see cref="IStringcategoryCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringcategoryList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringcategoryList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.RemoveAt"/> for details.</remarks>

		void RemoveAt(int index);

		#endregion
		#endregion
	}

	#endregion
	#region Interface IStringcategoryEnumerator

	/// <summary>
	/// Supports type-safe iteration over a dictionary that
	/// contains <see cref="CategoryEntry"/> elements.
	/// </summary>
	/// <remarks>
	/// <b>IStringcategoryEnumerator</b> provides an
	/// <see cref="IDictionaryEnumerator"/> that is strongly typed for
	/// <see cref="String"/> keys and <see cref="category"/> values.
	/// </remarks>

	public interface IStringcategoryEnumerator {
		#region Properties
		#region Current

		/// <summary>
		/// Gets the current <see cref="CategoryEntry"/> element in the collection.
		/// </summary>
		/// <value>The current <see cref="CategoryEntry"/> element in the collection.</value>
		/// <exception cref="InvalidOperationException"><para>The enumerator is positioned
		/// before the first element of the collection or after the last element.</para>
		/// <para>-or-</para>
		/// <para>The collection was modified after the enumerator was created.</para></exception>
		/// <remarks>Please refer to <see cref="IEnumerator.Current"/> for details, but note
		/// that <b>Current</b> fails if the collection was modified since the last successful
		/// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>

		CategoryEntry Current { get; }

		#endregion
		#region Entry

		/// <summary>
		/// Gets a <see cref="CategoryEntry"/> containing both
		/// the key and the value of the current dictionary entry.
		/// </summary>
		/// <value>A <see cref="CategoryEntry"/> containing both
		/// the key and the value of the current dictionary entry.</value>
		/// <exception cref="InvalidOperationException"><para>The enumerator is positioned
		/// before the first element of the dictionary or after the last element.</para>
		/// <para>-or-</para>
		/// <para>The dictionary was modified after the enumerator was created.</para></exception>
		/// <remarks>Please refer to <see cref="IDictionaryEnumerator.Entry"/> for details, but
		/// note that <b>Entry</b> fails if the collection was modified since the last successful
		/// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>

		CategoryEntry Entry { get; }

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
		/// Gets the <see cref="category"/> value of the current dictionary entry.
		/// </summary>
		/// <value>The <see cref="category"/> value
		/// of the current element of the enumeration.</value>
		/// <exception cref="InvalidOperationException"><para>The enumerator is positioned
		/// before the first element of the dictionary or after the last element.</para>
		/// <para>-or-</para>
		/// <para>The dictionary was modified after the enumerator was created.</para></exception>
		/// <remarks>Please refer to <see cref="IDictionaryEnumerator.Value"/> for details, but
		/// note that <b>Value</b> fails if the collection was modified since the last successful
		/// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>

		category Value { get; }

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
	#region Struct CategoryEntry

	/// <summary>
	/// Implements a strongly typed pair of one <see cref="String"/>
	/// key and one <see cref="category"/> value.
	/// </summary>
	/// <remarks>
	/// <b>CategoryEntry</b> provides a <see cref="DictionaryEntry"/> that is strongly
	/// typed for <see cref="String"/> keys and <see cref="category"/> values.
	/// </remarks>

	[Serializable]
	public struct CategoryEntry {
		#region Private Fields

		private String _key;
		private category _value;

		#endregion
		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CategoryEntry"/>
		/// class with the specified key and value.
		/// </summary>
		/// <param name="key">
		/// The <see cref="String"/> key in the key-and-value pair.</param>
		/// <param name="value">
		/// The <see cref="category"/> value in the key-and-value pair.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>

		public CategoryEntry(String key, category value) {
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
		/// <see cref="CategoryEntry"/> is a value type and therefore has an implicit default
		/// constructor that zeroes all data members. This means that the <b>Key</b> property of
		/// a default-constructed <b>CategoryEntry</b> contains a null reference by default,
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
		/// Gets or sets the <see cref="category"/> value in the key-and-value pair.
		/// </summary>
		/// <value>
		/// The <see cref="category"/> value in the key-and-value pair.
		/// This value can be a null reference, which is also the default.
		/// </value>

		public category Value {
			get { return this._value; }
			set { this._value = value; }
		}

		#endregion
		#endregion
		#region Public Operators
		#region CategoryEntry(DictionaryEntry)

		/// <summary>
		/// Converts a <see cref="DictionaryEntry"/> to a <see cref="CategoryEntry"/>.
		/// </summary>
		/// <param name="entry">A <see cref="DictionaryEntry"/> object to convert.</param>
		/// <returns>A <see cref="CategoryEntry"/> object that represents
		/// the converted <paramref name="entry"/>.</returns>
		/// <exception cref="InvalidCastException">
		/// <para><paramref name="entry"/> contains a key that is not compatible
		/// with <see cref="String"/>.</para>
		/// <para>-or-</para>
		/// <para><paramref name="entry"/> contains a value that is not compatible
		/// with <see cref="category"/>.</para>
		/// </exception>

		public static implicit operator CategoryEntry(DictionaryEntry entry) {
			CategoryEntry pair = new CategoryEntry();
			if (entry.Key != null) pair.Key = (String) entry.Key;
			if (entry.Value != null) pair.Value = (category) entry.Value;
			return pair;
		}

		#endregion
		#region DictionaryEntry(CategoryEntry)

		/// <summary>
		/// Converts a <see cref="CategoryEntry"/> to a <see cref="DictionaryEntry"/>.
		/// </summary>
		/// <param name="pair">A <see cref="CategoryEntry"/> object to convert.</param>
		/// <returns>A <see cref="DictionaryEntry"/> object that
		/// represents the converted <paramref name="pair"/>.</returns>

		public static implicit operator DictionaryEntry(CategoryEntry pair) {
			DictionaryEntry entry = new DictionaryEntry();
			if (pair.Key != null) entry.Key = pair.Key;
			entry.Value = pair.Value;
			return entry;
		}

		#endregion
		#endregion
	}

	#endregion
	#region Class CategoriesCollection

	/// <summary>
	/// Implements a strongly typed collection of <see cref="CategoryEntry"/> key-and-value
	/// pairs that retain their insertion order and are accessible by index and by key.
	/// </summary>
	/// <remarks><para>
	/// <b>CategoriesCollection</b> provides an <see cref="ArrayList"/> that is strongly
	/// typed for <see cref="CategoryEntry"/> elements and allows direct access to
	/// its <see cref="String"/> keys and <see cref="category"/> values.
	/// </para><para>
	/// The collection may contain multiple identical keys. All key access methods return the
	/// first occurrence of the specified key, if found. Access by index is an O(1) operation
	/// but access by key or value are both O(<em>N</em>) operations, where <em>N</em> is the
	/// current value of the <see cref="CategoriesCollection.Count"/> property.
	/// </para></remarks>

	[Serializable]
	public class CategoriesCollection:
		IStringcategoryList, IList, ICloneable {
		#region Private Fields

		private const int _defaultCapacity = 16;

		private String[] _keys;
		private category[] _values;
		private int _count;

		[NonSerialized]
		private int _version;
		private KeyList _keyList;
		private ValueList _valueList;

		#endregion
		#region Private Constructors

		// helper type to identify private ctor
		private enum Tag { Default }

		private CategoriesCollection(Tag tag) { }

		#endregion
		#region Public Constructors
		#region CategoriesCollection()

		/// <overloads>
		/// Initializes a new instance of the <see cref="CategoriesCollection"/> class.
		/// </overloads>
		/// <summary>
		/// Initializes a new instance of the <see cref="CategoriesCollection"/> class
		/// that is empty and has the default initial capacity.
		/// </summary>
		/// <remarks>Please refer to <see cref="ArrayList()"/> for details.</remarks>

		public CategoriesCollection() {
			this._keys = new String[_defaultCapacity];
			this._values = new category[_defaultCapacity];
		}

		#endregion
		#region CategoriesCollection(Int32)

		/// <summary>
		/// Initializes a new instance of the <see cref="CategoriesCollection"/> class
		/// that is empty and has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">The number of elements that the new
		/// <see cref="CategoriesCollection"/> is initially capable of storing.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="capacity"/> is less than zero.</exception>
		/// <remarks>Please refer to <see cref="ArrayList(Int32)"/> for details.</remarks>

		public CategoriesCollection(int capacity) {
			if (capacity < 0)
				throw new ArgumentOutOfRangeException("capacity",
					capacity, "Argument cannot be negative.");

			this._keys = new String[capacity];
			this._values = new category[capacity];
		}

		#endregion
		#region CategoriesCollection(CategoriesCollection)

		/// <summary>
		/// Initializes a new instance of the <see cref="CategoriesCollection"/> class
		/// that contains elements copied from the specified collection and
		/// that has the same initial capacity as the number of elements copied.
		/// </summary>
		/// <param name="collection">The <see cref="CategoriesCollection"/>
		/// whose elements are copied to the new collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="collection"/> is a null reference.</exception>
		/// <remarks>Please refer to <see cref="ArrayList(ICollection)"/> for details.</remarks>

		public CategoriesCollection(CategoriesCollection collection) {
			if (collection == null)
				throw new ArgumentNullException("collection");

			this._keys = new String[collection.Count];
			this._values = new category[collection.Count];
			AddRange(collection);
		}

		#endregion
		#region CategoriesCollection(CategoryEntry[])

		/// <summary>
		/// Initializes a new instance of the <see cref="CategoriesCollection"/> class
		/// that contains elements copied from the specified <see cref="CategoryEntry"/>
		/// array and that has the same initial capacity as the number of elements copied.
		/// </summary>
		/// <param name="array">An <see cref="Array"/> of <see cref="CategoryEntry"/>
		/// elements that are copied to the new collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is a null reference.</exception>
		/// <remarks>Please refer to <see cref="ArrayList(ICollection)"/> for details.</remarks>

		public CategoriesCollection(CategoryEntry[] array) {
			if (array == null)
				throw new ArgumentNullException("array");

			this._keys = new String[array.Length];
			this._values = new category[array.Length];
			AddRange(array);
		}

		#endregion
		#endregion
		#region Protected Properties
		#region InnerKeys
        
		/// <summary>
		/// Gets the list of keys contained in the <see cref="CategoriesCollection"/> instance.
		/// </summary>
		/// <value>
		/// A one-dimensional <see cref="Array"/> with zero-based indexing that contains all 
		/// <see cref="String"/> elements in the <see cref="CategoriesCollection"/>.
		/// </value>
		/// <remarks>
		/// Use <b>InnerKeys</b> to access the key array of a <see cref="CategoriesCollection"/>
		/// instance that might be a read-only or synchronized wrapper. This is necessary 
		/// because the key and value arrays of wrapper classes are always null references.
		/// </remarks>

		protected virtual String[] InnerKeys {
			get { return this._keys; }
		}

		#endregion
		#region InnerValues
        
		/// <summary>
		/// Gets the list of values contained in the <see cref="CategoriesCollection"/> instance.
		/// </summary>
		/// <value>
		/// A one-dimensional <see cref="Array"/> with zero-based indexing that contains all 
		/// <see cref="category"/> elements in the <see cref="CategoriesCollection"/>.
		/// </value>
		/// <remarks>
		/// Use <b>InnerValues</b> to access the value array of a <see cref="CategoriesCollection"/>
		/// instance that might be a read-only or synchronized wrapper. This is necessary
		/// because the key and value arrays of wrapper classes are always null references.
		/// </remarks>

		protected virtual category[] InnerValues {
			get { return this._values; }
		}

		#endregion
		#endregion
		#region Public Properties
		#region Capacity

		/// <summary>
		/// Gets or sets the capacity of the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <value>The number of elements that the
		/// <see cref="CategoriesCollection"/> can contain.</value>
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
					this._values = new category[_defaultCapacity];
					return;
				}

				String[] newKeys = new String[value];
				category[] newValues = new category[value];

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
		/// in the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <value>The number of key-and-value pairs contained
		/// in the <see cref="CategoriesCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ArrayList.Count"/> for details.</remarks>

		public virtual int Count {
			get { return this._count; }
		}

		#endregion
		#region IsFixedSize

		/// <summary>
		/// Gets a value indicating whether the <see cref="CategoriesCollection"/> has a fixed size.
		/// </summary>
		/// <value><c>true</c> if the <see cref="CategoriesCollection"/> has a fixed size;
		/// otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="ArrayList.IsFixedSize"/> for details.</remarks>

		public virtual bool IsFixedSize {
			get { return false; }
		}

		#endregion
		#region IsReadOnly

		/// <summary>
		/// Gets a value indicating whether the <see cref="CategoriesCollection"/> is read-only.
		/// </summary>
		/// <value><c>true</c> if the <see cref="CategoriesCollection"/> is read-only;
		/// otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="ArrayList.IsReadOnly"/> for details.</remarks>

		public virtual bool IsReadOnly {
			get { return false; }
		}

		#endregion
		#region IsSynchronized

		/// <summary>
		/// Gets a value indicating whether access to the <see cref="CategoriesCollection"/>
		/// is synchronized (thread-safe).
		/// </summary>
		/// <value><c>true</c> if access to the <see cref="CategoriesCollection"/> is
		/// synchronized (thread-safe); otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="ArrayList.IsSynchronized"/> for details.</remarks>

		public virtual bool IsSynchronized {
			get { return false; }
		}

		#endregion
		#region Item: CategoryEntry

		/// <summary>
		/// Gets or sets the <see cref="CategoryEntry"/> element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="CategoryEntry"/> element to get or set.</param>
		/// <value>
		/// The <see cref="CategoryEntry"/> element at the specified <paramref name="index"/>.
		/// </value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The property is set and the <see cref="CategoriesCollection"/> is read-only.</exception>
		/// <remarks>Please refer to <see cref="ArrayList.this"/> for details.</remarks>

		public virtual CategoryEntry this[int index] {
			get {
				ValidateIndex(index);
				return new CategoryEntry(this._keys[index], this._values[index]);
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
		/// must be compatible with <see cref="CategoryEntry"/>.
		/// </value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <exception cref="InvalidCastException">The property is set to a value
		/// that is not compatible with <see cref="CategoryEntry"/>.</exception>
		/// <exception cref="NotSupportedException">
		/// The property is set and the <see cref="CategoriesCollection"/> is read-only.</exception>
		/// <remarks>Please refer to <see cref="ArrayList.this"/> for details.</remarks>
		object IList.this[int index] {
			get { return (DictionaryEntry) this[index]; }
			set { this[index] = (CategoryEntry) (DictionaryEntry) value; }
		}

		#endregion
		#region Keys

		/// <summary>
		/// Gets an <see cref="IStringCollection"/> containing
		/// the keys in the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <value>An <see cref="IStringCollection"/> containing
		/// the keys in the <see cref="CategoriesCollection"/>.</value>
		/// <remarks>Please refer to <see cref="SortedList.Keys"/> for details.</remarks>

		public virtual IStringCollection Keys {
			get { return GetKeyList(); }
		}

		#endregion
		#region SyncRoot

		/// <summary>
		/// Gets an object that can be used to synchronize
		/// access to the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <value>An object that can be used to synchronize
		/// access to the <see cref="CategoriesCollection"/>.</value>
		/// <remarks>Please refer to <see cref="SortedList.SyncRoot"/> for details.</remarks>

		public virtual object SyncRoot {
			get { return this; }
		}

		#endregion
		#region Values

		/// <summary>
		/// Gets an <see cref="IcategoryCollection"/> containing
		/// the values in the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <value>An <see cref="IcategoryCollection"/> containing
		/// the values in the <see cref="CategoriesCollection"/>.</value>
		/// <remarks>Please refer to <see cref="SortedList.Values"/> for details.</remarks>

		public virtual IcategoryCollection Values {
			get { return GetValueList(); }
		}

		#endregion
		#endregion
		#region Public Methods
		#region static CreateCategory(String)
		/// <summary>
		/// Creates a new category object. 
		/// </summary>
		/// <param name="categoryName">Name of the category. 
		/// Nested categories needs to have backslash delimiters like 'My Cat1\Sub cat'</param>
		/// <returns>category object</returns>
		private static category CreateCategory(string categoryName) {
			category c = new category();
			c.Value = categoryName;


			return c;
		}
		#endregion
		#region Add(CategoryEntry)

		/// <overloads>
		/// Adds an element to the end of the <see cref="CategoriesCollection"/>.
		/// </overloads>
		/// <summary>
		/// Adds a <see cref="CategoryEntry"/> to the end of the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="entry">The <see cref="CategoryEntry"/> object
		/// to be added to the end of the <see cref="CategoriesCollection"/>.</param>
		/// <returns>The <see cref="CategoriesCollection"/> index at which the
		/// <paramref name="entry"/> has been added.</returns>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="CategoriesCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>CategoriesCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.Add"/> for details.</remarks>

		public virtual int Add(CategoryEntry entry) {
			if (this._count == this._keys.Length)
				EnsureCapacity(this._count + 1);

			++this._version;
			this._keys[this._count] = entry.Key;
			this._values[this._count] = entry.Value;
			return this._count++;
		}

		#endregion
		#region Add(String, category)

		/// <summary>
		/// Adds an element with the specified <see cref="String"/> key and
		/// <see cref="category"/> value to the end of the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key of the element
		/// to add to the end of the <see cref="CategoriesCollection"/>.</param>
		/// <param name="value">The <see cref="category"/> value of the element
		/// to add to the end of the <see cref="CategoriesCollection"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns>The <see cref="CategoriesCollection"/> index at which the
		/// new element has been added.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="CategoriesCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>CategoriesCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.Add"/> for details but note that
		/// the <see cref="CategoriesCollection"/> may contain multiple identical keys.</remarks>

		public int Add(String key, category value) {
			if ((object) key == null)
				throw new ArgumentNullException("key");

			value.parent = this.GetParentCategory(key); 

			foreach(category c in this.GetChildCategories(key)){
				c.parent = value; 
			}

			return Add(new CategoryEntry(key, value));
		}

		#endregion
		#region Add(String)

		/// <summary>
		/// Adds an element with the specified <see cref="String"/> key and a newly 
		/// created <see cref="category"/> value to the end of the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key of the element
		/// to add to the end of the <see cref="CategoriesCollection"/>.</param>
		/// <returns>The <see cref="CategoriesCollection"/> index at which the
		/// new element has been added.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="CategoriesCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>CategoriesCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.Add"/> for details but note that
		/// the <see cref="CategoriesCollection"/> may contain multiple identical keys.</remarks>

		public int Add(String key) {
			if ((object) key == null)
				throw new ArgumentNullException("key");

			if(this.ContainsKey(key)){
				return this.IndexOfKey(key); 
			}

			StringCollection ancestors = this.GetAncestors(key); 

			//create rest of category hierarchy if it doesn't exist
			for(int i = ancestors.Count; i-->0;){
			   category c = this.GetByKey(ancestors[i]); 
				if(c == null){
					this.Add(new CategoryEntry(ancestors[i], CreateCategory(ancestors[i])));
				}
			}

			category newCategory = CreateCategory(key); 
			newCategory.parent   = ( ancestors.Count == 0 ? null : this.GetByKey(ancestors[ancestors.Count - 1]) ); 

			return Add(new CategoryEntry(key, newCategory));
		}

		#endregion
		#region Add(category)

		/// <summary>
		/// Adds an <see cref="category"/> element and use the category.Value as the key 
		/// to the end of the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="value">The <see cref="category"/> element
		/// to add to the end of the <see cref="CategoriesCollection"/>.</param>
		/// <returns>The <see cref="CategoriesCollection"/> index at which the
		/// new element has been added.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="category"/> or category.Value is a null reference.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="CategoriesCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>CategoriesCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="SortedList.Add"/> for details but note that
		/// the <see cref="CategoriesCollection"/> may contain multiple identical keys.</remarks>

		public int Add(category value) {
			if ((object) value == null)
				throw new ArgumentNullException("value");

			if(this.ContainsKey(value.Value)){
				return this.IndexOfKey(value.Value); 
			}

			return Add(new CategoryEntry(value.Value, value));
		}

		#endregion
		#region IList.Add(Object)

		/// <summary>
		/// Adds an <see cref="Object"/> to the end of the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="entry">
		/// The object to be added to the end of the <see cref="CategoriesCollection"/>.
		/// This argument must be compatible with <see cref="CategoryEntry"/>.</param>
		/// <returns>The <see cref="CategoriesCollection"/> index at which the
		/// <paramref name="entry"/> has been added.</returns>
		/// <exception cref="InvalidCastException"><paramref name="entry"/>
		/// is not compatible with <see cref="CategoryEntry"/>.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="CategoriesCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>CategoriesCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.Add"/> for details.</remarks>

		int IList.Add(object entry) {
			return Add((CategoryEntry) entry);
		}

		#endregion
		#region AddRange(CategoriesCollection)

		/// <overloads>
		/// Adds a range of elements to the end of the <see cref="CategoriesCollection"/>.
		/// </overloads>
		/// <summary>
		/// Adds the elements of another collection to the end of the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="collection">The <see cref="CategoriesCollection"/> whose elements
		/// should be added to the end of the current collection.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="collection"/> is a null reference.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="CategoriesCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>CategoriesCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.AddRange"/> for details.</remarks>

		public virtual void AddRange(CategoriesCollection collection) {
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
		#region AddRange(CategoryEntry[])

		/// <summary>
		/// Adds the elements of a <see cref="CategoryEntry"/> array
		/// to the end of the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="array">An <see cref="Array"/> of <see cref="CategoryEntry"/> elements
		/// that should be added to the end of the <see cref="CategoriesCollection"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is a null reference.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="CategoriesCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>CategoriesCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.AddRange"/> for details.</remarks>

		public virtual void AddRange(CategoryEntry[] array) {
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
		/// Removes all elements from the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="CategoriesCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>CategoriesCollection</b> has a fixed size.</para></exception>
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
		/// Creates a shallow copy of the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <returns>A shallow copy of the <see cref="CategoriesCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="ArrayList.Clone"/> for details.</remarks>

		public virtual object Clone() {
			CategoriesCollection collection = new CategoriesCollection(this._count);

			Array.Copy(this._keys, 0, collection._keys, 0, this._count);
			Array.Copy(this._values, 0, collection._values, 0, this._count);

			collection._count = this._count;
			collection._version = this._version;

			return collection;
		}

		#endregion
		#region Contains(CategoryEntry)

		/// <summary>
		/// Determines whether the <see cref="CategoriesCollection"/>
		/// contains the specified <see cref="CategoryEntry"/> element.
		/// </summary>
		/// <param name="entry">The <see cref="CategoryEntry"/> object
		/// to locate in the <see cref="CategoriesCollection"/>.</param>
		/// <returns><c>true</c> if <paramref name="entry"/> is found in the
		/// <see cref="CategoriesCollection"/>; otherwise, <c>false</c>.</returns>
		/// <remarks><para>
		/// Please refer to <see cref="ArrayList.Contains"/> for details.
		/// </para><para>
		/// <b>Contains</b> uses the equality operators defined by <see cref="String"/>
		/// and <see cref="category"/> to locate the specified <paramref name="entry"/>.
		/// </para></remarks>

		public virtual bool Contains(CategoryEntry entry) {
			return (IndexOf(entry) >= 0);
		}

		#endregion
		#region IList.Contains(Object)

		/// <summary>
		/// Determines whether the <see cref="CategoriesCollection"/> contains the specified element.
		/// </summary>
		/// <param name="entry">The object to locate in the <see cref="CategoriesCollection"/>.
		/// This argument must be compatible with <see cref="CategoryEntry"/>.</param>
		/// <returns><c>true</c> if <paramref name="entry"/> is found in the
		/// <see cref="CategoriesCollection"/>; otherwise, <c>false</c>.</returns>
		/// <exception cref="InvalidCastException"><paramref name="entry"/>
		/// is not compatible with <see cref="CategoryEntry"/>.</exception>
		/// <remarks>Please refer to <see cref="ArrayList.Contains"/> for details.</remarks>

		bool IList.Contains(object entry) {
			return Contains((CategoryEntry) entry);
		}

		#endregion
		#region ContainsKey

		/// <summary>
		/// Determines whether the <see cref="CategoriesCollection"/>
		/// contains the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key
		/// to locate in the <see cref="CategoriesCollection"/>.</param>
		/// <returns><c>true</c> if the <see cref="CategoriesCollection"/> contains an element
		/// with the specified <paramref name="key"/>; otherwise, <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <remarks>Please refer to <see cref="SortedList.ContainsKey"/> for details but note
		/// that the <see cref="CategoriesCollection"/> requires linear time to locate a key.</remarks>

		public virtual bool ContainsKey(String key) {
			return (IndexOfKey(key) >= 0);
		}

		#endregion
		#region ContainsValue

		/// <summary>
		/// Determines whether the <see cref="CategoriesCollection"/>
		/// contains the specified <see cref="category"/> value.
		/// </summary>
		/// <param name="value">The <see cref="category"/> value
		/// to locate in the <see cref="CategoriesCollection"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns><c>true</c> if the <see cref="CategoriesCollection"/> contains an element
		/// with the specified <paramref name="value"/>; otherwise, <c>false</c>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.ContainsValue"/> for details.</remarks>

		public virtual bool ContainsValue(category value) {
			return (IndexOfValue(value) >= 0);
		}

		#endregion
		#region CopyTo(CategoryEntry[], Int32)

		/// <summary>
		/// Copies the entire <see cref="CategoriesCollection"/> to a one-dimensional <see cref="Array"/> of
		/// <see cref="CategoryEntry"/> elements, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
		/// <see cref="CategoryEntry"/> elements copied from the <see cref="CategoriesCollection"/>.
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
		/// The number of elements in the source <see cref="CategoriesCollection"/> is greater than
		/// the available space from <paramref name="arrayIndex"/> to the end of the destination
		/// <paramref name="array"/>.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.CopyTo"/> for details.</remarks>

		public virtual void CopyTo(CategoryEntry[] array, int arrayIndex) {
			CheckTargetArray(array, arrayIndex);

			for (int i = 0; i < this._count; i++) {
				CategoryEntry entry =
					new CategoryEntry(this._keys[i], this._values[i]);

				array.SetValue(entry, arrayIndex + i);
			}
		}

		#endregion
		#region ICollection.CopyTo(Array, Int32)

		/// <summary>
		/// Copies the entire <see cref="CategoriesCollection"/> to a one-dimensional <see cref="Array"/>,
		/// starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
		/// <see cref="CategoryEntry"/> elements copied from the <see cref="CategoriesCollection"/>.
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
		/// The number of elements in the source <see cref="CategoriesCollection"/> is greater than
		/// the available space from <paramref name="arrayIndex"/> to the end of the destination
		/// <paramref name="array"/>.</para></exception>
		/// <exception cref="InvalidCastException">
		/// The <see cref="CategoryEntry"/> type cannot be cast automatically
		/// to the type of the destination <paramref name="array"/>.</exception>
		/// <remarks>Please refer to <see cref="ArrayList.CopyTo"/> for details.</remarks>

		void ICollection.CopyTo(Array array, int arrayIndex) {
			CopyTo((CategoryEntry[]) array, arrayIndex);
		}

		#endregion
		#region Equals

		/// <summary>
		/// Determines whether the specified <see cref="CategoriesCollection"/>
		/// is equal to the current <b>CategoriesCollection</b>.
		/// </summary>
		/// <param name="collection">The <see cref="CategoriesCollection"/>
		/// to compare with the current <b>CategoriesCollection</b>.</param>
		/// <returns><c>true</c> if the specified <see cref="CategoriesCollection"/> is equal
		/// to the current <b>CategoriesCollection</b>; otherwise, <c>false</c>.</returns>
		/// <remarks><para>
		/// This <b>Equals</b> overload tests for value equality of all <see cref="CategoryEntry"/>
		/// elements contained in the two <see cref="CategoriesCollection"/> collections.
		/// </para><para>
		/// <b>Equals</b> returns <c>false</c> if <paramref name="collection"/> is a null
		/// reference, holds a different number of elements, or holds at least one
		/// <see cref="CategoryEntry"/> element at a given index position whose key and/or
		/// value is different from that of the element at the same index position in this
		/// <b>CategoriesCollection</b>, as determined by the inequality operators defined by
		/// <see cref="String"/> and <see cref="category"/>.
		/// </para></remarks>

		public virtual bool Equals(CategoriesCollection collection) {
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
		/// Gets the <see cref="category"/> value at the
		/// specified index of the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="category"/> value to get.</param>
		/// <returns>The <see cref="category"/> value at the specified
		/// <paramref name="index"/> of the <see cref="CategoriesCollection"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="SortedList.GetByIndex"/> for details.</remarks>

		public virtual category GetByIndex(int index) {
			ValidateIndex(index);
			return this._values[index];
		}

		#endregion
		#region GetByKey

		/// <summary>
		/// Gets the <see cref="category"/> value associated with the
		/// first occurrence of the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key whose value to get.</param>
		/// <returns>The <see cref="category"/> value associated with the first occurrence
		/// of the specified <paramref name="key"/>, if found; otherwise,
		/// a null reference.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <remarks><b>GetByKey</b> and <see cref="SetByKey"/> emulate the indexer of the
		/// <see cref="SortedList"/> class but require linear time to locate a key.</remarks>

		public virtual category GetByKey(String key) {
			int index = IndexOfKey(key);
			if (index >= 0) return this._values[index];
			return null;
		}

		#endregion
		#region GetEnumerator: IStringcategoryEnumerator

		/// <summary>
		/// Returns an <see cref="IStringcategoryEnumerator"/> that can
		/// iterate through the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IStringcategoryEnumerator"/>
		/// for the entire <see cref="CategoriesCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="ArrayList.GetEnumerator"/> for details.</remarks>

		public virtual IStringcategoryEnumerator GetEnumerator() {
			return new Enumerator(this);
		}

		#endregion
		#region IEnumerable.GetEnumerator: IEnumerator

		/// <summary>
		/// Returns an <see cref="IEnumerator"/> that can
		/// iterate through the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IEnumerator"/>
		/// for the entire <see cref="CategoriesCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="ArrayList.GetEnumerator"/> for details.</remarks>

		IEnumerator IEnumerable.GetEnumerator() {
			return (IEnumerator) GetEnumerator();
		}

		#endregion
		#region GetKey

		/// <summary>
		/// Gets the <see cref="String"/> key at the
		/// specified index of the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="String"/> key to get.</param>
		/// <returns>The <see cref="String"/> key at the specified
		/// <paramref name="index"/> of the <see cref="CategoriesCollection"/>.</returns>
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
		/// Gets the keys in the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IStringList"/> containing the keys
		/// in the <see cref="CategoriesCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.GetKeyList"/> for details.</remarks>

		public virtual IStringList GetKeyList() {
			if (this._keyList == null)
				this._keyList = new KeyList(this);
			return this._keyList;
		}

		#endregion
		#region GetValueList

		/// <summary>
		/// Gets the values in the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IcategoryList"/> containing the values
		/// in the <see cref="CategoriesCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="SortedList.GetValueList"/> for details.</remarks>

		public virtual IcategoryList GetValueList() {
			if (this._valueList == null)
				this._valueList = new ValueList(this);
			return this._valueList;
		}

		#endregion
		#region IndexOf(CategoryEntry)

		/// <summary>
		/// Returns the zero-based index of the first occurrence of the specified
		/// <see cref="CategoryEntry"/> in the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="entry">The <see cref="CategoryEntry"/> object
		/// to locate in the <see cref="CategoriesCollection"/>.</param>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="entry"/>
		/// in the <see cref="CategoriesCollection"/>, if found; otherwise, -1.
		/// </returns>
		/// <remarks><para>
		/// Please refer to <see cref="ArrayList.IndexOf"/> for details.
		/// </para><para>
		/// <b>IndexOf</b> uses the equality operators defined by <see cref="String"/>
		/// and <see cref="category"/> to locate the specified <paramref name="entry"/>.
		/// </para></remarks>

		public virtual int IndexOf(CategoryEntry entry) {

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
		/// <see cref="Object"/> in the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="entry">The object to locate in the <see cref="CategoriesCollection"/>.
		/// This argument must be compatible with <see cref="CategoryEntry"/>.</param>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="entry"/>
		/// in the <see cref="CategoriesCollection"/>, if found; otherwise, -1.
		/// </returns>
		/// <exception cref="InvalidCastException"><paramref name="entry"/>
		/// is not compatible with <see cref="CategoryEntry"/>.</exception>
		/// <remarks>Please refer to <see cref="ArrayList.IndexOf"/> for details.</remarks>

		int IList.IndexOf(object entry) {
			return IndexOf((CategoryEntry) entry);
		}

		#endregion
		#region IndexOfKey

		/// <summary>
		/// Returns the zero-based index of the first occurrence of the specified
		/// <see cref="String"/> key in the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key
		/// to locate in the <see cref="CategoriesCollection"/>.</param>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="key"/>
		/// in the <see cref="CategoriesCollection"/>, if found; otherwise, -1.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <remarks>Please refer to <see cref="SortedList.IndexOfKey"/> for details but note
		/// that the <see cref="CategoriesCollection"/> requires linear time to locate a key.</remarks>

		public virtual int IndexOfKey(String key) {
			if ((object) key == null)
				throw new ArgumentNullException("key");
			return Array.IndexOf(this._keys, key, 0, this._count);
		}

		#endregion
		#region IndexOfValue

		/// <summary>
		/// Returns the zero-based index of first occurrence of the specified
		/// <see cref="category"/> value in the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="value">The <see cref="category"/> value
		/// to locate in the <see cref="CategoriesCollection"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="value"/>
		/// in the <see cref="CategoriesCollection"/>, if found; otherwise, -1.
		/// </returns>
		/// <remarks>Please refer to <see cref="SortedList.IndexOfValue"/> for details.</remarks>

		public virtual int IndexOfValue(category value) {
			return Array.IndexOf(this._values, value, 0, this._count);
		}

		#endregion
		#region Insert(Int32, CategoryEntry)

		/// <summary>
		/// Inserts a <see cref="CategoryEntry"/> element into the
		/// <see cref="CategoriesCollection"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="entry"/>
		/// should be inserted.</param>
		/// <param name="entry">The <see cref="CategoryEntry"/> object
		/// to insert into the <see cref="CategoriesCollection"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="CategoriesCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>CategoriesCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.Insert"/> for details.</remarks>

		public virtual void Insert(int index, CategoryEntry entry) {
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
		/// Inserts an element into the <see cref="CategoriesCollection"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="entry"/>
		/// should be inserted.</param>
		/// <param name="entry">The object to insert into the <see cref="CategoriesCollection"/>.
		/// This argument must be compatible with <see cref="CategoryEntry"/>.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <exception cref="InvalidCastException"><paramref name="entry"/>
		/// is not compatible with <see cref="CategoryEntry"/>.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="CategoriesCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>CategoriesCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.Insert"/> for details.</remarks>

		void IList.Insert(int index, object entry) {
			Insert(index, (CategoryEntry) entry);
		}

		#endregion
		#region Remove(String)

		/// <summary>
		/// Removes the first occurrence of the specified key
		/// from the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="key">The key of the <see cref="CategoryEntry"/> object
		/// to remove from the <see cref="CategoriesCollection"/>.</param>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="CategoriesCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>CategoriesCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.Remove"/> for details.</remarks>

		public virtual void Remove(string key) {
			int index = IndexOfKey(key);
			if (index >= 0) RemoveAt(index);
		}

		#endregion
		#region Remove(CategoryEntry)

		/// <summary>
		/// Removes the first occurrence of the specified <see cref="CategoryEntry"/>
		/// from the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="entry">The <see cref="CategoryEntry"/> object
		/// to remove from the <see cref="CategoriesCollection"/>.</param>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="CategoriesCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>CategoriesCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.Remove"/> for details.</remarks>

		public virtual void Remove(CategoryEntry entry) {
			int index = IndexOf(entry);
			if (index >= 0) RemoveAt(index);
		}

		#endregion
		#region IList.Remove(Object)

		/// <summary>
		/// Removes the first occurrence of the specified <see cref="Object"/>
		/// from the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="entry">The object to remove from the <see cref="CategoriesCollection"/>.
		/// This argument must be compatible with <see cref="CategoryEntry"/>.</param>
		/// <exception cref="InvalidCastException"><paramref name="entry"/>
		/// is not compatible with <see cref="CategoryEntry"/>.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="CategoriesCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>CategoriesCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.Remove"/> for details.</remarks>

		void IList.Remove(object entry) {
			Remove((CategoryEntry) entry);
		}

		#endregion
		#region RemoveAt

		/// <summary>
		/// Removes the element at the specified index of the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="CategoriesCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>CategoriesCollection</b> has a fixed size.</para></exception>
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
		/// Sets the <see cref="category"/> value at the
		/// specified index of the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="category"/> value to set.</param>
		/// <param name="value">The <see cref="category"/> object to store
		/// at the specified <paramref name="index"/> of the <see cref="CategoriesCollection"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="Count"/>.</para>
		/// </exception>
		/// <remarks>Please refer to <see cref="SortedList.SetByIndex"/> for details.</remarks>

		public virtual void SetByIndex(int index, category value) {
			ValidateIndex(index);
			++this._version;
			this._values[index] = value;
		}

		#endregion
		#region SetByKey

		/// <summary>
		/// Sets the <see cref="category"/> value associated with the
		/// first occurrence of the specified <see cref="String"/> key.
		/// </summary>
		/// <param name="key">The <see cref="String"/> key whose value to set.</param>
		/// <param name="value">The <see cref="category"/> object to associate with
		/// the first occurrence of the specified <paramref name="key"/>.
		/// This argument can be a null reference.
		/// If the specified <paramref name="key"/> is not found, <b>SetByKey</b> adds a new element
		/// with the specified <paramref name="key"/> and <paramref name="value"/> to the end of the
		/// <see cref="CategoriesCollection"/>.</param>
		/// <returns>The <see cref="CategoriesCollection"/> index of the element
		/// that was changed or added.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="key"/> is a null reference.</exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="CategoriesCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para><paramref name="key"/> does not exist in the collection,
		/// and the <b>CategoriesCollection</b> has a fixed size.</para></exception>
		/// <remarks><b>SetByKey</b> and <see cref="GetByKey"/> emulate the indexer of the
		/// <see cref="SortedList"/> class but require linear time to locate a key.</remarks>

		public virtual int SetByKey(String key, category value) {
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
		/// for the specified <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <param name="collection">The <see cref="CategoriesCollection"/> to synchronize.</param>
		/// <returns>
		/// A synchronized (thread-safe) wrapper around <paramref name="collection"/>.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="collection"/> is a null reference.</exception>
		/// <remarks>Please refer to <see cref="ArrayList.Synchronized"/> for details.</remarks>

		public static CategoriesCollection Synchronized(CategoriesCollection collection) {
			if (collection == null)
				throw new ArgumentNullException("collection");

			return new SyncList(collection);
		}

		#endregion
		#region ToArray

		/// <summary>
		/// Copies the elements of the <see cref="CategoriesCollection"/> to a new
		/// <see cref="Array"/> of <see cref="CategoryEntry"/> elements.
		/// </summary>
		/// <returns>A one-dimensional <see cref="Array"/> of <see cref="CategoryEntry"/>
		/// elements containing copies of the elements of the <see cref="CategoriesCollection"/>.
		/// </returns>
		/// <remarks>Please refer to <see cref="ArrayList.ToArray"/> for details.</remarks>

		public virtual CategoryEntry[] ToArray() {
			CategoryEntry[] array = new CategoryEntry[this._count];
			CopyTo(array, 0);
			return array;
		}

		#endregion
		#region TrimToSize

		/// <summary>
		/// Sets the capacity to the actual number of elements in the <see cref="CategoriesCollection"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="CategoriesCollection"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>CategoriesCollection</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="ArrayList.TrimToSize"/> for details.</remarks>

		public virtual void TrimToSize() {
			Capacity = this._count;
		}

		#endregion
		#endregion
		#region Private Methods

		#region GetParentCategory

		private category GetParentCategory(string key){
			int index = key.LastIndexOf(NewsHandler.CategorySeparator); 
			
			if(index != -1){
				string parentName = key.Substring(0, index);
				return this.GetByKey(parentName); 
			}else{
				return null; 
			}

		}

		#endregion 
		#region GetChildCategories

		private ArrayList GetChildCategories(string key){

			ArrayList list = new ArrayList(); 

			foreach(category c in this.Values){
				if(c.Value.StartsWith(key)){
					list.Add(c); 
				}
			}
		
			return list; 
		}

		#endregion 

		#region GetAncestors 
	
		private StringCollection GetAncestors(string key){

			StringCollection list = new StringCollection();
			string current = String.Empty; 
			string[] s  = key.Split(NewsHandler.CategorySeparator.ToCharArray()); 

			if(s.Length != 1){				
			
				for(int i = 0; i < (s.Length -1) ; i++){
					current += (i == 0 ? s[i] : NewsHandler.CategorySeparator + s[i]); 
					list.Add(current); 
				}

			}

			return list; 
		
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
			IStringcategoryEnumerator, IDictionaryEnumerator {
			#region Private Fields

			private readonly CategoriesCollection _collection;
			private readonly int _version;
			private int _index;

			#endregion
			#region Internal Constructors

			internal Enumerator(CategoriesCollection collection) {
				this._collection = collection;
				this._version = collection._version;
				this._index = -1;
			}

			#endregion
			#region Public Properties

			public CategoryEntry Current {
				get { return Entry; }
			}

			object IEnumerator.Current {
				get { return (DictionaryEntry) Entry; }
			}

			public CategoryEntry Entry {
				get {
					this._collection.CheckEnumIndex(this._index);
					this._collection.CheckEnumVersion(this._version);

					return new CategoryEntry(
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

			public category Value {
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
			private sealed class KeyList: IStringList, IList {
			#region Private Fields

			private CategoriesCollection _collection;

			#endregion
			#region Internal Constructors

			internal KeyList(CategoriesCollection collection) {
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

			public int Add(String key) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			int IList.Add(object key) {
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

			bool IList.Contains(object key) {
				return Contains((String) key);
			}

			public void CopyTo(String[] array, int arrayIndex) {
				this._collection.CheckTargetArray(array, arrayIndex);
				Array.Copy(this._collection._keys, 0,
					array, arrayIndex, this._collection.Count);
			}

			void ICollection.CopyTo(Array array, int arrayIndex) {
				this._collection.CheckTargetArray(array, arrayIndex);
				CopyTo((String[]) array, arrayIndex);
			}

			public IStringEnumerator GetEnumerator() {
				return new KeyEnumerator(this._collection);
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return (IEnumerator) GetEnumerator();
			}

			public int IndexOf(String key) {
				return this._collection.IndexOfKey(key);
			}

			int IList.IndexOf(object key) {
				return IndexOf((String) key);
			}

			public void Insert(int index, String key) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			void IList.Insert(int index, object key) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			public void Remove(String key) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			void IList.Remove(object key) {
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
			private sealed class KeyEnumerator:
			IStringEnumerator, IEnumerator {
			#region Private Fields

			private readonly CategoriesCollection _collection;
			private readonly int _version;
			private int _index;

			#endregion
			#region Internal Constructors

			internal KeyEnumerator(CategoriesCollection collection) {
				this._collection = collection;
				this._version = collection._version;
				this._index = -1;
			}

			#endregion
			#region Public Properties

			public String Current {
				get {
					this._collection.CheckEnumIndex(this._index);
					this._collection.CheckEnumVersion(this._version);
					return this._collection._keys[this._index];
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
		#region Class ValueList

		[Serializable]
			private sealed class ValueList: IcategoryList, IList {
			#region Private Fields

			private CategoriesCollection _collection;

			#endregion
			#region Internal Constructors

			internal ValueList(CategoriesCollection collection) {
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

			public category this[int index] {
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

			public int Add(category value) {
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

			public bool Contains(category value) {
				return this._collection.ContainsValue(value);
			}

			bool IList.Contains(object value) {
				return Contains((category) value);
			}

			public void CopyTo(category[] array, int arrayIndex) {
				this._collection.CheckTargetArray(array, arrayIndex);
				Array.Copy(this._collection._values, 0,
					array, arrayIndex, this._collection.Count);
			}

			void ICollection.CopyTo(Array array, int arrayIndex) {
				this._collection.CheckTargetArray(array, arrayIndex);
				CopyTo((category[]) array, arrayIndex);
			}

			public IcategoryEnumerator GetEnumerator() {
				return new ValueEnumerator(this._collection);
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return (IEnumerator) GetEnumerator();
			}

			public int IndexOf(category value) {
				return this._collection.IndexOfValue(value);
			}

			int IList.IndexOf(object value) {
				return IndexOf((category) value);
			}

			public void Insert(int index, category value) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			void IList.Insert(int index, object value) {
				throw new NotSupportedException(
					"Read-only collections cannot be modified.");
			}

			public void Remove(category value) {
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
			IcategoryEnumerator, IEnumerator {
			#region Private Fields

			private readonly CategoriesCollection _collection;
			private readonly int _version;
			private int _index;

			#endregion
			#region Internal Constructors

			internal ValueEnumerator(CategoriesCollection collection) {
				this._collection = collection;
				this._version = collection._version;
				this._index = -1;
			}

			#endregion
			#region Public Properties

			public category Current {
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
			private sealed class SyncList: CategoriesCollection {
			#region Private Fields

			private CategoriesCollection _collection;
			private object _root;

			#endregion
			#region Internal Constructors

			internal SyncList(CategoriesCollection collection):
				base(Tag.Default) {

				this._collection = collection;
				this._root = collection.SyncRoot;
			}

			#endregion
			#region Protected Properties

			protected override String[] InnerKeys {
				get { lock (this._root) return this._collection.InnerKeys; }
			}

			protected override category[] InnerValues {
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

			public override CategoryEntry this[int index] {
				get { lock (this._root) return this._collection[index]; }
				set { lock (this._root) this._collection[index] = value; }
			}

			public override IStringCollection Keys {
				get { lock (this._root) return this._collection.Keys; }
			}

			public override object SyncRoot {
				get { return this._root; }
			}

			public override IcategoryCollection Values {
				get { lock (this._root) return this._collection.Values; }
			}

			#endregion
			#region Public Methods

			public override int Add(CategoryEntry entry) {
				lock (this._root) return this._collection.Add(entry);
			}

			public override void AddRange(CategoriesCollection collection) {
				lock (this._root) this._collection.AddRange(collection);
			}

			public override void AddRange(CategoryEntry[] array) {
				lock (this._root) this._collection.AddRange(array);
			}

			public override void Clear() {
				lock (this._root) this._collection.Clear();
			}

			public override object Clone() {
				lock (this._root) return this._collection.Clone();
			}

			public override bool Contains(CategoryEntry entry) {
				lock (this._root) return this._collection.Contains(entry);
			}

			public override bool ContainsKey(String key) {
				lock (this._root) return this._collection.ContainsKey(key);
			}

			public override bool ContainsValue(category value) {
				lock (this._root) return this._collection.ContainsValue(value);
			}

			public override void CopyTo(CategoryEntry[] array, int index) {
				lock (this._root) this._collection.CopyTo(array, index);
			}

			public override bool Equals(CategoriesCollection collection) {
				lock (this._root) return this._collection.Equals(collection);
			}

			public override category GetByIndex(int index) {
				lock (this._root) return this._collection.GetByIndex(index);
			}

			public override category GetByKey(String key) {
				lock (this._root) return this._collection.GetByKey(key);
			}

			public override IStringcategoryEnumerator GetEnumerator() {
				lock (this._root) return this._collection.GetEnumerator();
			}

			public override String GetKey(int index) {
				lock (this._root) return this._collection.GetKey(index);
			}

			public override IStringList GetKeyList() {
				lock (this._root) return this._collection.GetKeyList();
			}

			public override IcategoryList GetValueList() {
				lock (this._root) return this._collection.GetValueList();
			}

			public override int IndexOf(CategoryEntry entry) {
				lock (this._root) return this._collection.IndexOf(entry);
			}

			public override int IndexOfKey(String key) {
				lock (this._root) return this._collection.IndexOfKey(key);
			}

			public override int IndexOfValue(category value) {
				lock (this._root) return this._collection.IndexOfValue(value);
			}

			public override void Insert(int index, CategoryEntry entry) {
				lock (this._root) this._collection.Insert(index, entry);
			}

			public override void Remove(CategoryEntry entry) {
				lock (this._root) this._collection.Remove(entry);
			}

			public override void RemoveAt(int index) {
				lock (this._root) this._collection.RemoveAt(index);
			}

			public override void SetByIndex(int index, category value) {
				lock (this._root) this._collection.SetByIndex(index, value);
			}

			public override int SetByKey(String key, category value) {
				lock (this._root) return this._collection.SetByKey(key, value);
			}

			public override CategoryEntry[] ToArray() {
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
