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

namespace NewsComponents.Collections
{
	/// <summary>
	/// A observable KeyItemCollection
	/// </summary>
	/// <typeparam name="TK">The type of the Key.</typeparam>
	/// <typeparam name="TI">The type of the Item.</typeparam>
	[Serializable]
	public class ObservableKeyItemCollection<TK, TI> : KeyItemCollection<TK, TI>,
		INotifyCollectionChanged, INotifyPropertyChanged
	{
		#region ctor's

		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableKeyItemCollection&lt;TK, TI&gt;"/> class.
		/// </summary>
		public ObservableKeyItemCollection()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableKeyItemCollection&lt;TK, TI&gt;"/> class.
		/// </summary>
		/// <param name="capacity">The initial capacity.</param>
		public ObservableKeyItemCollection(int capacity):
			base(capacity) {
			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableKeyItemCollection&lt;TK, TI&gt;"/> class.
		/// </summary>
		/// <param name="comparer">The key comparer.</param>
		public ObservableKeyItemCollection(IEqualityComparer<TK> comparer):
			base(comparer)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableKeyItemCollection&lt;TK, TI&gt;"/> class.
		/// </summary>
		/// <param name="capacity">The capacity.</param>
		/// <param name="comparer">The key comparer.</param>
		public ObservableKeyItemCollection(int capacity, IEqualityComparer<TK> comparer):
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
		protected override void CollectionWasChanged(KeyItemChange change, int position)
		{
			if (collectionChanged != null)
			{
				switch (change)
				{
					case KeyItemChange.Add:
						OnPropertyChanged("Item[]");
						OnPropertyChanged("Count");
						OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this[position], position));
						break;
					case KeyItemChange.Remove:
						OnPropertyChanged("Item[]");
						OnPropertyChanged("Count");
						OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, null, position));
						break;
					case KeyItemChange.Changed:
						OnPropertyChanged("Item[]");
						OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, this[position], null, position));
						break;
					case KeyItemChange.OrderChanged:
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

		#endregion

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
		public override void Clear()
		{
			base.Clear();
			OnPropertyChanged("Item[]");
			OnPropertyChanged("Count");
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));	
		}
	}
}
