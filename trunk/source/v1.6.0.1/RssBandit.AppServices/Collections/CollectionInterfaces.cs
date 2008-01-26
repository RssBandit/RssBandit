using System;
using System.Collections;

namespace RssBandit.AppServices
{
	#region Interface IStringCollection

	/// <summary>
	/// Defines size, enumerators, and synchronization methods for strongly
	/// typed collections of <see cref="String"/> elements.
	/// </summary>
	/// <remarks>
	/// <b>IStringCollection</b> provides an <see cref="ICollection"/>
	/// that is strongly typed for <see cref="String"/> elements.
	/// </remarks>

	public interface IStringCollection 
	{
		#region Properties
		#region Count

		/// <summary>
		/// Gets the number of elements contained in the
		/// <see cref="IStringCollection"/>.
		/// </summary>
		/// <value>The number of elements contained in the
		/// <see cref="IStringCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ICollection.Count"/> for details.</remarks>

		int Count { get; }

		#endregion
		#region IsSynchronized

		/// <summary>
		/// Gets a value indicating whether access to the
		/// <see cref="IStringCollection"/> is synchronized (thread-safe).
		/// </summary>
		/// <value><c>true</c> if access to the <see cref="IStringCollection"/> is
		/// synchronized (thread-safe); otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="ICollection.IsSynchronized"/> for details.</remarks>

		bool IsSynchronized { get; }

		#endregion
		#region SyncRoot

		/// <summary>
		/// Gets an object that can be used to synchronize access
		/// to the <see cref="IStringCollection"/>.
		/// </summary>
		/// <value>An object that can be used to synchronize access
		/// to the <see cref="IStringCollection"/>.</value>
		/// <remarks>Please refer to <see cref="ICollection.SyncRoot"/> for details.</remarks>

		object SyncRoot { get; }

		#endregion
		#endregion
		#region Methods
		#region CopyTo

		/// <summary>
		/// Copies the entire <see cref="IStringCollection"/> to a one-dimensional <see cref="Array"/>
		/// of <see cref="String"/> elements, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
		/// <see cref="String"/> elements copied from the <see cref="IStringCollection"/>.
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
		/// The number of elements in the source <see cref="IStringCollection"/> is greater
		/// than the available space from <paramref name="arrayIndex"/> to the end of the destination
		/// <paramref name="array"/>.</para></exception>
		/// <remarks>Please refer to <see cref="ICollection.CopyTo"/> for details.</remarks>

		void CopyTo(String[] array, int arrayIndex);

		#endregion
		#region GetEnumerator

		/// <summary>
		/// Returns an <see cref="IStringEnumerator"/> that can
		/// iterate through the <see cref="IStringCollection"/>.
		/// </summary>
		/// <returns>An <see cref="IStringEnumerator"/>
		/// for the entire <see cref="IStringCollection"/>.</returns>
		/// <remarks>Please refer to <see cref="IEnumerable.GetEnumerator"/> for details.</remarks>

		IStringEnumerator GetEnumerator();

		#endregion
		#endregion
	}

	#endregion

	#region Interface IStringList

	/// <summary>
	/// Represents a strongly typed collection of <see cref="String"/>
	/// objects that can be individually accessed by index.
	/// </summary>
	/// <remarks>
	/// <b>IStringList</b> provides an <see cref="IList"/>
	/// that is strongly typed for <see cref="String"/> elements.
	/// </remarks>

	public interface
		IStringList: IStringCollection 
	{
		#region Properties
		#region IsFixedSize

		/// <summary>
		/// Gets a value indicating whether the <see cref="IStringList"/> has a fixed size.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IStringList"/> has a fixed size;
		/// otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IList.IsFixedSize"/> for details.</remarks>

		bool IsFixedSize { get; }

		#endregion
		#region IsReadOnly

		/// <summary>
		/// Gets a value indicating whether the <see cref="IStringList"/> is read-only.
		/// </summary>
		/// <value><c>true</c> if the <see cref="IStringList"/> is read-only;
		/// otherwise, <c>false</c>. The default is <c>false</c>.</value>
		/// <remarks>Please refer to <see cref="IList.IsReadOnly"/> for details.</remarks>

		bool IsReadOnly { get; }

		#endregion
		#region Item

		/// <summary>
		/// Gets or sets the <see cref="String"/> element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the
		/// <see cref="String"/> element to get or set.</param>
		/// <value>
		/// The <see cref="String"/> element at the specified <paramref name="index"/>.
		/// </value>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than
		/// <see cref="IStringCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The property is set and the <see cref="IStringList"/> is read-only.</exception>
		/// <remarks>Please refer to <see cref="IList.this"/> for details.</remarks>

		String this[int index] { get; set; }

		#endregion
		#endregion
		#region Methods
		#region Add

		/// <summary>
		/// Adds a <see cref="String"/> to the end
		/// of the <see cref="IStringList"/>.
		/// </summary>
		/// <param name="value">The <see cref="String"/> object
		/// to be added to the end of the <see cref="IStringList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns>The <see cref="IStringList"/> index at which
		/// the <paramref name="value"/> has been added.</returns>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Add"/> for details.</remarks>

		int Add(String value);

		#endregion
		#region Clear

		/// <summary>
		/// Removes all elements from the <see cref="IStringList"/>.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Clear"/> for details.</remarks>

		void Clear();

		#endregion
		#region Contains

		/// <summary>
		/// Determines whether the <see cref="IStringList"/>
		/// contains the specified <see cref="String"/> element.
		/// </summary>
		/// <param name="value">The <see cref="String"/> object
		/// to locate in the <see cref="IStringList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns><c>true</c> if <paramref name="value"/> is found in the
		/// <see cref="IStringList"/>; otherwise, <c>false</c>.</returns>
		/// <remarks>Please refer to <see cref="IList.Contains"/> for details.</remarks>

		bool Contains(String value);

		#endregion
		#region IndexOf

		/// <summary>
		/// Returns the zero-based index of the first occurrence of the specified
		/// <see cref="String"/> in the <see cref="IStringList"/>.
		/// </summary>
		/// <param name="value">The <see cref="String"/> object
		/// to locate in the <see cref="IStringList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="value"/>
		/// in the <see cref="IStringList"/>, if found; otherwise, -1.
		/// </returns>
		/// <remarks>Please refer to <see cref="IList.IndexOf"/> for details.</remarks>

		int IndexOf(String value);

		#endregion
		#region Insert

		/// <summary>
		/// Inserts a <see cref="String"/> element into the
		/// <see cref="IStringList"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which
		/// <paramref name="value"/> should be inserted.</param>
		/// <param name="value">The <see cref="String"/> object
		/// to insert into the <see cref="IStringList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is greater than
		/// <see cref="IStringCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Insert"/> for details.</remarks>

		void Insert(int index, String value);

		#endregion
		#region Remove

		/// <summary>
		/// Removes the first occurrence of the specified <see cref="String"/>
		/// from the <see cref="IStringList"/>.
		/// </summary>
		/// <param name="value">The <see cref="String"/> object
		/// to remove from the <see cref="IStringList"/>.
		/// This argument can be a null reference.
		/// </param>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.Remove"/> for details.</remarks>

		void Remove(String value);

		#endregion
		#region RemoveAt

		/// <summary>
		/// Removes the element at the specified index of the
		/// <see cref="IStringList"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero.</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than
		/// <see cref="IStringCollection.Count"/>.</para>
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// <para>The <see cref="IStringList"/> is read-only.</para>
		/// <para>-or-</para>
		/// <para>The <b>IStringList</b> has a fixed size.</para></exception>
		/// <remarks>Please refer to <see cref="IList.RemoveAt"/> for details.</remarks>

		void RemoveAt(int index);

		#endregion
		#endregion
	}

	#endregion

	#region Interface IStringEnumerator

	/// <summary>
	/// Supports type-safe iteration over a collection that
	/// contains <see cref="String"/> elements.
	/// </summary>
	/// <remarks>
	/// <b>IStringEnumerator</b> provides an <see cref="IEnumerator"/>
	/// that is strongly typed for <see cref="String"/> elements.
	/// </remarks>

	public interface IStringEnumerator 
	{
		#region Properties
		#region Current

		/// <summary>
		/// Gets the current <see cref="String"/> element in the collection.
		/// </summary>
		/// <value>The current <see cref="String"/> element in the collection.</value>
		/// <exception cref="InvalidOperationException"><para>The enumerator is positioned
		/// before the first element of the collection or after the last element.</para>
		/// <para>-or-</para>
		/// <para>The collection was modified after the enumerator was created.</para></exception>
		/// <remarks>Please refer to <see cref="IEnumerator.Current"/> for details, but note
		/// that <b>Current</b> fails if the collection was modified since the last successful
		/// call to <see cref="MoveNext"/> or <see cref="Reset"/>.</remarks>

		String Current { get; }

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

	#region Class ReadOnlyDictionary
	/// <summary>
	/// Wrapper to make a class implementing IDictonary readonly
	/// </summary>
	public sealed class ReadOnlyDictionary: IDictionary
	{
		private IDictionary dict;

		public ReadOnlyDictionary(IDictionary dictionary) {
			if (dictionary == null)
				this.dict = new Hashtable(0);
			else
				this.dict = dictionary;
		}

		public void CopyTo(Array array, int index) {
			dict.CopyTo(array, index);
		}

		public int Count {
			get { return dict.Count; }
		}

		public object SyncRoot {
			get { return dict.SyncRoot; }
		}

		public bool IsSynchronized {
			get { return dict.IsSynchronized; }
		}

		public bool Contains(object key) {
			return dict.Contains(key);
		}

		public void Add(object key, object value) {
			ThrowReadOnlyException();
		}

		public void Clear() {
			ThrowReadOnlyException();
		}

		IDictionaryEnumerator IDictionary.GetEnumerator() {
			return ((IDictionary)dict).GetEnumerator();
		}

		public IEnumerator GetEnumerator() {
			return dict.GetEnumerator();
		}

		public void Remove(object key) {
			ThrowReadOnlyException();
		}

		public ICollection Keys {
			get { return dict.Keys; }
		}

		public ICollection Values {
			get { return dict.Values; }
		}

		public bool IsReadOnly {
			get { return true; }
		}

		public bool IsFixedSize {
			get { return true; }
		}

		public object this[object key] {
			get { return dict[key]; }
			set { ThrowReadOnlyException();}
		}

		private void ThrowReadOnlyException() {
			throw new NotSupportedException(
				"Read-only dictionary cannot be modified.");
		}
		#endregion
	}
}
