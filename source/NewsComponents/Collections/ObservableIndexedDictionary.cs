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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;

namespace NewsComponents.Collections
{
	/// <summary>
	/// A observable IndexedDictionary
	/// </summary>
	/// <typeparam name="TK">The type of the Key.</typeparam>
	/// <typeparam name="TI">The type of the Item.</typeparam>
	[Serializable]
	public class ObservableIndexedDictionary<TK, TI> : IndexedDictionary<TK, TI>,
		INotifyCollectionChanged, INotifyPropertyChanged
	{
		#region ctor's

		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableIndexedDictionaryIndexedDictionary{TK,TI}"/> class.
		/// </summary>
		public ObservableIndexedDictionary()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableIndexedDictionaryIndexedDictionary{TK,TI}"/> class.
		/// </summary>
		/// <param name="capacity">The initial capacity.</param>
		public ObservableIndexedDictionary(int capacity):
			base(capacity) {
			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableIndexedDictionaryIndexedDictionary{TK,TI}"/> class.
		/// </summary>
		/// <param name="comparer">The key comparer.</param>
		public ObservableIndexedDictionary(IEqualityComparer<TK> comparer):
			base(comparer)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableIndexedDictionaryIndexedDictionary{TK,TI}"/> class.
		/// </summary>
		/// <param name="capacity">The capacity.</param>
		/// <param name="comparer">The key comparer.</param>
		public ObservableIndexedDictionary(int capacity, IEqualityComparer<TK> comparer):
			base(capacity, comparer)
		{
		}
		#endregion

		#region INotifyCollectionChanged

		[NonSerialized]
		private NotifyCollectionChangedEventHandler collectionChanged;

		/// <summary>
		/// Occurs when the collection changes.
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged
		{
			add { this.collectionChanged = (NotifyCollectionChangedEventHandler)Delegate.Combine(this.collectionChanged, value); }
			remove { this.collectionChanged = (NotifyCollectionChangedEventHandler)Delegate.Remove(this.collectionChanged, value); }
		}

		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (this.collectionChanged != null)
			{
				this.collectionChanged(this, e);
			}
		}
		protected override void CollectionWasChanged(IndexedDictionaryChangeAction changeAction, int position)
		{
			if (collectionChanged != null)
			{
				switch (changeAction)
				{
					case IndexedDictionaryChangeAction.Add:
						OnPropertyChanged("Item[]");
						OnPropertyChanged("Count");
						OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this[position], position));
						break;
					case IndexedDictionaryChangeAction.Remove:
						OnPropertyChanged("Item[]");
						OnPropertyChanged("Count");
						OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, null, position));
						break;
					case IndexedDictionaryChangeAction.EntryChanged:
						OnPropertyChanged("Item[]");
						OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, this[position], null, position));
						break;
					case IndexedDictionaryChangeAction.OrderChanged:
						OnPropertyChanged("Item[]");
						OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, (IList)this.Values, 0));
						break;
				}
			}
		}

		#endregion

		#region INotifyPropertyChanged

		[NonSerialized]
		private PropertyChangedEventHandler propertyChanged;

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged
		{
			add { this.propertyChanged = (PropertyChangedEventHandler)Delegate.Combine(this.propertyChanged, value); }
			remove { this.propertyChanged = (PropertyChangedEventHandler)Delegate.Remove(this.propertyChanged, value); }
		}

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (this.propertyChanged != null)
			{
				this.propertyChanged(this, e);
			}
		}

		private void OnPropertyChanged(string propertyName)
		{
			this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

        private void OnPropertyChanged(Expression<Func<object>> expression)
        {
            propertyChanged.Notify(expression);
        }

		#endregion

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
		public override void Clear()
		{
			base.Clear();
			OnPropertyChanged("Item[]");    //TODO: make this a strong typed expression
			OnPropertyChanged(() => Count);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));	
		}
	}
}
