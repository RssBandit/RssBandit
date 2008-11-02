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

namespace NewsComponents.Collections
{
	#region DisposableItemCollection

	/// <summary>
	/// Used to store disposable items
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="DI">The type of the I.</typeparam>
	internal class DisposableItemCollection<T, DI> : KeyItemCollection<T, DI>, IDisposable
		where DI : class, IDisposable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DisposableItemCollection&lt;T, DI&gt;"/> class.
		/// </summary>
		public DisposableItemCollection()
		{
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="DisposableItemCollection&lt;T, DI&gt;"/> class.
		/// </summary>
		/// <param name="capacity">The capacity.</param>
		public DisposableItemCollection(int capacity)
			: base(capacity)
		{
		}

		/// <summary>
		/// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"></see>
		/// and dispose the item.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns>
		/// true if the element is successfully removed; otherwise, false.  This method also returns false if key was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
		/// <exception cref="T:System.ArgumentNullException">key is null.</exception>
		public override bool Remove(T key)
		{
			DI item;
			if (TryGetValue(key, out item))
				item.Dispose();
			return base.Remove(key);
		}

		/// <summary>
		/// Removes an entry at the specified position index and
		/// dispose the item.
		/// </summary>
		/// <param name="index">The index.</param>
		public override void RemoveAt(int index)
		{
			DI item = this[index];
			if (item != null)
				item.Dispose();
			base.RemoveAt(index);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>
		/// and dispose the items.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
		public new void Clear()
		{
			foreach (DI item in Values)
				item.Dispose();
			base.Clear();
		}

		#region Implementation of IDisposable

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Clear();
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}

	#endregion
}
