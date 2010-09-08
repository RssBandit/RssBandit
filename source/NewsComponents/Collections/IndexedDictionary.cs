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
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using NewsComponents.Utils;

namespace NewsComponents.Collections
{

	#region KeyItemChange enum

	/// <summary>
	/// Lists the possible change actions of a IndexedDictionary
	/// </summary>
	public enum IndexedDictionaryChangeAction
	{
		/// <summary>
		/// Entry was removed
		/// </summary>
		Remove,
		/// <summary>
		/// Entry was added
		/// </summary>
		Add,
		/// <summary>
		/// Entry was changed
		/// </summary>
		EntryChanged,
		/// <summary>
		/// Entry order changed
		/// </summary>
		OrderChanged
	}

	#endregion

	/// <summary>
	/// Instance works like a Dictionary with keys and like a List with indexes.
	/// </summary>
	/// <typeparam name="TKey">Key type of the collection</typeparam>
	/// <typeparam name="TValue">Object type of the collection</typeparam>
	[Serializable, DebuggerDisplay("Count = {Count}")]
	public class IndexedDictionary<TKey, TValue> : ISerializable, IXmlSerializable,
		IEnumerable<TValue>, ICloneable, IDictionary<TKey,TValue>, IDictionary,IDeserializationCallback
	{
	    private const int Version_1 = 20080528;

	    private SerializationInfoReader reader;
		private readonly object SyncRoot = new object();
		private const List<TValue> Defaultitems= null;
		private const string Serialitems="items";
		private const Dictionary<TKey, int> Defaultpositions = null;
		private const string Serialpositions="positions";
	    private const int DefaultVersion = Version_1;
	    private const string SerialVersion = "Version";

		private List<TValue> items;
		private Dictionary<TKey,int> positions;

		#region ctor's

		/// <summary>
		/// Initializes a new instance of the <see cref="IndexedDictionary{Key,Object}"/> class.
		/// </summary>
		public IndexedDictionary()
		{
			items = new List<TValue>();
			positions = new Dictionary<TKey, int>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IndexedDictionary{Key,Object}"/> class.
		/// </summary>
		/// <param name="capacity">The initial capacity.</param>
		public IndexedDictionary(int capacity) {
			items = new List<TValue>(capacity);
			positions = new Dictionary<TKey, int>(capacity);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IndexedDictionary{Key,Object}"/> class.
		/// </summary>
		/// <param name="comparer">The key comparer.</param>
		public IndexedDictionary(IEqualityComparer<TKey> comparer)
		{
			items = new List<TValue>();
			positions = new Dictionary<TKey, int>(comparer);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IndexedDictionary{Key,Object}"/> class.
		/// </summary>
		/// <param name="capacity">The capacity.</param>
		/// <param name="comparer">The key comparer.</param>
		public IndexedDictionary(int capacity, IEqualityComparer<TKey> comparer)
		{
			items = new List<TValue>(capacity);
			positions = new Dictionary<TKey, int>(capacity, comparer);
		}
		#endregion

		/// <summary>
		/// Called if Collection was the changed.
		/// </summary>
		/// <param name="changeAction">The change.</param>
		/// <param name="position">The position.</param>
		protected virtual void CollectionWasChanged(IndexedDictionaryChangeAction changeAction, int position)
		{			

		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"></see> contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</param>
		/// <returns>
		/// true if the <see cref="T:System.Collections.Generic.IDictionary`2"></see> contains an element with the key; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">key is null.</exception>
		public virtual bool ContainsKey(TKey key)
		{
			return positions.ContainsKey(key);			
		}

		/// <summary>
		/// Gets the index the of the provided object.
		/// </summary>
		/// <param name="value">The object.</param>
		/// <returns></returns>
		public virtual int IndexOf(TValue value) {
			return items.IndexOf(value);
		}

		/// <summary>
		/// Gets the index the of the provided key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public virtual int IndexOf(TKey key) {
			
			int position;
			if (positions.TryGetValue(key, out position))
				return position;
			
			return -1;
		}

		/// <summary>
		/// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns>
		/// true if the element is successfully removed; otherwise, false.  This method also returns false if key was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
		/// <exception cref="T:System.ArgumentNullException">key is null.</exception>
		public virtual bool Remove(TKey key)
		{
			int position;
			if (positions.TryGetValue(key,out position))
			{
				CollectionWasChanged(IndexedDictionaryChangeAction.Remove, position);
				positions.Remove(key);
				Dictionary<TKey, int> newPositions = new Dictionary<TKey, int>();
				foreach(KeyValuePair<TKey, int> entry in positions)
				{
					if (entry.Value>position)
					{
						newPositions[entry.Key] = entry.Value - 1;
					}				
					else
					{
						newPositions[entry.Key] = entry.Value;
					}
				}
				positions = newPositions;
				items.RemoveAt(position);				
				return true;				
			}
			return false;
		}

		/// <summary>
		/// Removes an entry at the specified position index.
		/// </summary>
		/// <param name="index">The index.</param>
		public virtual void RemoveAt(int index) {
			if (index >= positions.Count || index < 0)
				throw new IndexOutOfRangeException("Parameter 'index' is out of range");

			KeyValuePair<TKey, int> found = new KeyValuePair<TKey, int>(default(TKey), -1);

			foreach (KeyValuePair<TKey, int> entry in positions)
			{
				if (entry.Value == index)
				{
					found = entry;
					break;
				}
				
			}

			if (found.Value != -1)
				this.Remove(found.Key);

		}

		/// <summary>
		/// Adds the specified key and object.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The object.</param>
		public virtual void Add(TKey key, TValue value)
		{
			int position;
			if (positions.TryGetValue(key,out position))
			{
				items[position] = value;
			}
			else
			{
				positions.Add(key,items.Count);
				position = items.Count;
				items.Add(value);
			}
			CollectionWasChanged(IndexedDictionaryChangeAction.Add, position);
		}

		/// <summary>
		/// Gets or sets the Object with the specified position.
		/// </summary>
		/// <value></value>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public virtual TValue this[int position]
		{
			get
			{
				return items[position];
			}
			set
			{
				items[position] = value;
				CollectionWasChanged(IndexedDictionaryChangeAction.EntryChanged, position);
			}
		}

		/// <summary>
		/// Gets or sets the Object with the specified key.
		/// </summary>
		/// <value></value>
		/// <exception cref="KeyNotFoundException">On set</exception>
		public virtual TValue this[TKey key]
		{
			get
			{
				int position;
				if (positions.TryGetValue(key,out position))
				{
					return items[position];	
				}
				return default(TValue);
			}
			set
			{
				int position = positions[key];
				items[position] = value;
				CollectionWasChanged(IndexedDictionaryChangeAction.EntryChanged, position);
				
			}
		}

		/// <summary>
		/// Sorts the collection by items.
		/// </summary>
		/// <param name="itemComparer">The item comparer.</param>
		public virtual void SortByItems(IComparer<TValue> itemComparer)
		{
			TValue[] arrValues = items.ToArray();
			TKey[] arrKeys = new TKey[arrValues.Length];
		    foreach(KeyValuePair<TKey, int> entry in positions)
		    {
				arrKeys[entry.Value] = entry.Key;
		    }
			Array.Sort(arrValues, arrKeys, itemComparer);
			UpdateCollections(arrValues, arrKeys);
		}



		/// <summary>
		/// Sorts the collection by keys.
		/// </summary>
		/// <param name="keyComparer">The key comparer.</param>
		public virtual void SortByKeys(IComparer<TKey> keyComparer)
		{
			TValue[] arrValues = items.ToArray();
			TKey[] arrKeys = new TKey[arrValues.Length];
			foreach (KeyValuePair<TKey, int> entry in positions)
			{
				arrKeys[entry.Value] = entry.Key;
			}
			Array.Sort(arrKeys, arrValues, keyComparer);
			UpdateCollections(arrValues, arrKeys);
		}

		

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
		/// </summary>
		/// <value></value>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
		public virtual int Count
		{
			get { return items.Count; }
		}


		#region private helpers

		private void UpdateCollections(IEnumerable<TValue> arrItems, TKey[] arrKeys)
		{
			items.Clear();
			items.AddRange(arrItems);
			for (int iPosition = 0; iPosition < arrKeys.Length; iPosition++)
			{
				positions[arrKeys[iPosition]] = iPosition;
			}
			CollectionWasChanged(IndexedDictionaryChangeAction.OrderChanged, -1);
		}
		
		internal TKey[] GetSortedKeys()
		{
			if (this.Count > 0) {
				TKey[] arrKeys = new TKey[this.Count];
				foreach (KeyValuePair<TKey, int> entry in positions) {
					arrKeys[entry.Value] = entry.Key;
				}
				return arrKeys;
			}
			return new TKey[] { };
		}

		#endregion

		#region IDictionary(Key,Object)

		/// <summary>
		/// Tries to get the value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns>True on success, else false</returns>
		public bool TryGetValue(TKey key, out TValue value)
		{
			int position;
			if (positions.TryGetValue(key,out position))
			{
				value = items[position];
				return true;
			}
			value = default(TValue);
			return false;
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
		/// </summary>
		/// <value></value>
		/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
		ICollection<TKey> IDictionary<TKey,TValue>.Keys
		{
			get { return this.GetSortedKeys(); }
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
		/// </summary>
		/// <value></value>
		/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
		public ICollection<TKey> Keys
		{
			get { return new List<TKey>(this.GetSortedKeys()); }
		}
		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
		/// </summary>
		/// <value></value>
		/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
		ICollection<TValue> IDictionary<TKey, TValue>.Values
		{
			get { return this.Values; }
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
		/// </summary>
		/// <value></value>
		/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
		public ICollection<TValue> Values
		{
			get { return new List<TValue>(items); }
		}

		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key,item.Value);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
		public virtual void Clear()
		{
			for (int i = 0; i < Count; i++)
			{
				CollectionWasChanged(IndexedDictionaryChangeAction.Remove, i);				
			}
			positions.Clear();
			items.Clear();			
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
		/// <returns>
		/// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
		/// </returns>
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return ContainsKey(item.Key);
		}

		/// <summary>
		/// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
		/// <exception cref="T:System.ArgumentNullException">array is null.</exception>
		/// <exception cref="T:System.ArgumentException">array is multidimensional.-or-<paramref name="arrayIndex"/> is equal to or greater than the length of array.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"></see> is greater than the available space from arrayIndex to the end of the destination array.-or-Type T cannot be cast automatically to the type of the destination array.</exception>
		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			int pos = arrayIndex;
			foreach (KeyValuePair<TKey, TValue> p in (IEnumerable)this)
				array[pos++] = p;
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
		/// <returns>
		/// true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			int position;
			if (positions.TryGetValue(item.Key, out position))
			{
				CollectionWasChanged(IndexedDictionaryChangeAction.Remove, position);
				positions.Remove(item.Key);
				items.RemoveAt(position);
			}
			return false;
		}


		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
		/// </summary>
		/// <value></value>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
		public bool IsReadOnly
		{
			get { return false; }
		}


		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
		/// </returns>
		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return new KeyValuePairEnumerator(this);
		}

		#endregion

		#region Serialization

		/// <summary>
		/// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
		/// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
		/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
		    info.AddValue(SerialVersion, Version_1, typeof (int));	
            SerializationInfoReader.AddGenericList(context, info, Serialitems,items);
            SerializationInfoReader.AddGenericDictionary(context, info, Serialpositions, positions);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IndexedDictionary{Key,Object}"/> class.
		/// </summary>
		/// <param name="info">The info.</param>
		/// <param name="context">The context.</param>
		protected IndexedDictionary(SerializationInfo info, StreamingContext context)
		{
			reader = new SerializationInfoReader(info,context);			
		}

        /// <summary>
        /// Runs when the entire object graph has been deserialized.
        /// </summary>
        /// <param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented.</param>
	    public virtual void OnDeserialization(object sender)
	    {
            if (reader != null)
            {
                int version = (int) reader.GetValue(SerialVersion, typeof (int), DefaultVersion);
                switch (version)
                {
                    
                    case Version_1:
                        items = reader.GetGenericList(Serialitems, Defaultitems);
                        positions = reader.GetGenericDictionary(Serialpositions, Defaultpositions);
                        if (reader.Context.State != StreamingContextStates.Persistence)
                        {
                            positions.OnDeserialization(sender);
                        }
                        break;
                }
                if (items == null) items = new List<TValue>();
                if (positions==null) positions=new Dictionary<TKey, int>();
                reader = null;
            }
	    }

	    #endregion

		#region IXmlSerializable Members
		/*
		 * We HAVE to impl. IXmlSeralizable by ourself,
		 * because we would get a NotSupportedException by the framework:
		 * Reason: we implement IDictionary :-(
		 */

		/// <summary>
		/// Used in IXmlSerializable implementation to get around the XmlSerialization bug
		/// with multiple XmlSerializer instances.
		/// </summary>
		private static XmlSerializerFactory xmlSerializerCache;
		
		System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader r)
		{
			Clear();

			if (xmlSerializerCache == null)
				xmlSerializerCache = new XmlSerializerFactory();

			string itemTypeName = r.GetAttribute("itemType");
			string keyTypeName = r.GetAttribute("keyType");

			if (String.IsNullOrEmpty(keyTypeName) ||
				String.IsNullOrEmpty(itemTypeName))
			{
				r.Read(); // consume container start tag
				return;
			}

			Type itemType = Type.GetType(itemTypeName);
			Type keyType = Type.GetType(keyTypeName);
			XmlSerializer itemSerializer = xmlSerializerCache.CreateSerializer(itemType);
			XmlSerializer keySerializer = xmlSerializerCache.CreateSerializer(keyType);

			List<TKey> keysRead = new List<TKey>();
			List<TValue> itemsRead = new List<TValue>();

			int myLevel = r.Depth;

			while (!r.EOF && r.Read() && myLevel < r.Depth)
			{
				r.MoveToContent();
				if (r.NodeType == XmlNodeType.EndElement)
					continue;

				if (r.NodeType == XmlNodeType.Element) 
				{
					string itemName = r.Name;

					if (0 == String.CompareOrdinal("key", itemName)) 
					{
						string index = r.GetAttribute("index");
						if (String.IsNullOrEmpty(index)) 
							r.Skip();
						else 
							keysRead.Insert(Convert.ToInt32(index), (TKey)GetXmlChild(r, keySerializer));
					} 
					else
					if (0 == String.CompareOrdinal("item", itemName))
					{
						string index = r.GetAttribute("index");
						if (String.IsNullOrEmpty(index))
							r.Skip();
						else {
							XmlSerializer usedSerializer = itemSerializer;
							string subItemTypeName = r.GetAttribute("itemType");
							if (!String.IsNullOrEmpty(subItemTypeName)) {
								Type subType = Type.GetType(subItemTypeName);
								usedSerializer = xmlSerializerCache.CreateSerializer(subType);
								
							}
							itemsRead.Insert(Convert.ToInt32(index), (TValue)GetXmlChild(r, usedSerializer));
						}
					}
					else
					{
						r.Skip();
					}
				} else
				{
					r.Skip();
				}
			}

			if (keysRead.Count == itemsRead.Count) 
			{
				for (int i = 0; i < keysRead.Count; i++) {
					positions.Add(keysRead[i], i);
					items.Insert(i, itemsRead[i]);
				}
			} else {
				throw new XmlException("Read elements keys.Count not equal to items.Count (" + keysRead.Count + ", " + itemsRead.Count + ")");
			}
		}

		private static object GetXmlChild(XmlReader r, XmlSerializer s) 
		{
			// move to child, content:
			r.Read();
			r.MoveToContent();
			return s.Deserialize(r);
		}

		void IXmlSerializable.WriteXml(XmlWriter w)
		{
			if (xmlSerializerCache == null)
				xmlSerializerCache = new XmlSerializerFactory();

			if (this.Count == 0)
				return;

			w.WriteAttributeString("keyType", typeof(TKey).AssemblyQualifiedName);
			w.WriteAttributeString("itemType", typeof(TValue).AssemblyQualifiedName);

			if (w.LookupPrefix(NamespaceXml.Xsd) == null)
			{
				w.WriteAttributeString("xmlns", "xsd", NamespaceXml.XmlNs, NamespaceXml.Xsd);
			}

			if (w.LookupPrefix(NamespaceXml.Xsi) == null)
			{
				w.WriteAttributeString("xmlns", "xsi", NamespaceXml.XmlNs, NamespaceXml.Xsi);
			}

			XmlSerializer keySerializer = xmlSerializerCache.CreateSerializer(typeof(TKey));
			XmlSerializer itemSerializer = xmlSerializerCache.CreateSerializer(typeof(TValue));
			XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

			foreach (TKey key in this.Keys)
			{
				int index = this.positions[key];
				TValue value = this[key];

				w.WriteStartElement("key");
				w.WriteAttributeString("index", index.ToString());
				keySerializer.Serialize(w, key, ns);
				w.WriteEndElement();

				w.WriteStartElement("item");
				w.WriteAttributeString("index", index.ToString());
				if (!typeof(TValue).IsValueType && value != null && !typeof(TValue).Equals(value.GetType())) 
				{
					// items can be inherited objects, so create the XmlSerializer on demand:
					w.WriteAttributeString("itemType", value.GetType().AssemblyQualifiedName);
					XmlSerializer inheritedItemSerializer = xmlSerializerCache.CreateSerializer(value.GetType());
					inheritedItemSerializer.Serialize(w, value, ns);
				} 
				else 
				{
					itemSerializer.Serialize(w, value, ns);
				}

				w.WriteEndElement();

			}
		}
		
		#endregion

		#region IEnumerable<Object> Members

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<TValue> GetEnumerator()
		{
			return new KeyItemEnumerator(this);
		}

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#endregion

		#region ICloneable

		#region ICloneable Members

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		public virtual object Clone()
		{
			IndexedDictionary<TKey,TValue> clone=new IndexedDictionary<TKey, TValue>();
			clone.items=new List<TValue>(items);
			clone.positions=new Dictionary<TKey, int>(positions);
			return clone;
		}

		#endregion

		#endregion
		
		#region IDictionary Members

		/// <summary>
		/// Adds an element with the provided key and value to the <see cref="T:System.Collections.IDictionary"></see> object.
		/// </summary>
		/// <param name="key">The <see cref="T:System.Object"></see> to use as the key of the element to add.</param>
		/// <param name="value">The <see cref="T:System.Object"></see> to use as the value of the element to add.</param>
		/// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.IDictionary"></see> object. </exception>
		/// <exception cref="T:System.ArgumentNullException">key is null. </exception>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IDictionary"></see> is read-only.-or- The <see cref="T:System.Collections.IDictionary"></see> has a fixed size. </exception>
		void IDictionary.Add(object key, object value)
		{
			this.Add((TKey)key, (TValue)value);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
		void IDictionary.Clear()
		{
			this.Clear();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.IDictionary"></see> object contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="T:System.Collections.IDictionary"></see> object.</param>
		/// <returns>
		/// true if the <see cref="T:System.Collections.IDictionary"></see> contains an element with the key; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">key is null. </exception>
		bool IDictionary.Contains(object key)
		{
			return this.ContainsKey((TKey)key);
		}

		/// <summary>
		/// Returns an <see cref="T:System.Collections.IDictionaryEnumerator"></see> object for the <see cref="T:System.Collections.IDictionary"></see> object.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IDictionaryEnumerator"></see> object for the <see cref="T:System.Collections.IDictionary"></see> object.
		/// </returns>
		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return new KeyValueEntryEnumerator(this);
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.IDictionary"></see> object has a fixed size.
		/// </summary>
		/// <value></value>
		/// <returns>true if the <see cref="T:System.Collections.IDictionary"></see> object has a fixed size; otherwise, false.</returns>
		bool IDictionary.IsFixedSize
		{
			get { return false; }
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
		/// </summary>
		/// <value></value>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
		bool IDictionary.IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
		/// </summary>
		/// <value></value>
		/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
		ICollection IDictionary.Keys
		{
			get { return new List<TKey>(this.Keys); }
		}

		/// <summary>
		/// Removes the element with the specified key from the <see cref="T:System.Collections.IDictionary"></see> object.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IDictionary"></see> object is read-only.-or- The <see cref="T:System.Collections.IDictionary"></see> has a fixed size. </exception>
		/// <exception cref="T:System.ArgumentNullException">key is null. </exception>
		void IDictionary.Remove(object key)
		{
			this.Remove((TKey)key);
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
		/// </summary>
		/// <value></value>
		/// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
		ICollection IDictionary.Values
		{
			get { return new List<TValue>(this.Values); }
		}

		/// <summary>
		/// Gets or sets the entry with the specified key.
		/// </summary>
		/// <value></value>
		object IDictionary.this[object key]
		{
			get
			{
				return this[(TKey)key];
			}
			set
			{
				this[(TKey)key] = (TValue)value;
			}
		}

		#endregion

		#region ICollection Members

		/// <summary>
		/// Copies the elements of the <see cref="T:System.Collections.ICollection"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in array at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">array is null. </exception>
		/// <exception cref="T:System.ArgumentException">The type of the source <see cref="T:System.Collections.ICollection"></see> cannot be cast automatically to the type of the destination array. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">index is less than zero. </exception>
		/// <exception cref="T:System.ArgumentException">array is multidimensional.-or- index is equal to or greater than the length of array.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"></see> is greater than the available space from index to the end of the destination array. </exception>
		void ICollection.CopyTo(Array array, int index)
		{
			int pos = index;
			foreach (KeyValuePair<TKey, TValue> p in (IEnumerable)this)
				array.SetValue(p, pos++);
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
		/// </summary>
		/// <value></value>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
		int ICollection.Count
		{
			get { return this.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe).
		/// </summary>
		/// <value></value>
		/// <returns>true if access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe); otherwise, false.</returns>
		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		/// <summary>
		/// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.
		/// </summary>
		/// <value></value>
		/// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.</returns>
		object ICollection.SyncRoot
		{
			get { return this.SyncRoot; }
		}

		#endregion

		#region KeyItemEnumerator

		private class KeyItemEnumerator : IEnumerator<TValue>
		{
			private IndexedDictionary<TKey, TValue> parent;
			private int pos=-1;


			public KeyItemEnumerator(IndexedDictionary<TKey, TValue> parent)
			{
				this.parent = parent;
			}

			#region IEnumerator<Object> Members

			TValue IEnumerator<TValue>.Current
			{
				get 
				{
					return parent.items[pos];
				}
			}

			#endregion

			#region IDisposable Members

			public void Dispose()
			{
				parent = null;
				GC.SuppressFinalize(this);
			}

			#endregion

			#region IEnumerator Members

			public bool MoveNext()
			{
				pos++;
				return pos < parent.items.Count;
			}

			public void Reset()
			{
				pos = -1;
			}

			public object Current
			{
				get { return parent.items[pos];  }
			}

			#endregion
		}

		#endregion

		#region KeyValuePairEnumerator

		private class KeyValuePairEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
		{
			private IndexedDictionary<TKey, TValue> parent;
			private TKey[] arrKeys;
			private int pos = -1;


			public KeyValuePairEnumerator(IndexedDictionary<TKey, TValue> parent)
			{
				this.parent = parent;
				this.Reset();
			}

			#region IEnumerator<Object> Members

			KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current
			{
				get
				{
					return new KeyValuePair<TKey, TValue>(arrKeys[pos], parent.items[pos]); 
				}
			}

			#endregion

			#region IDisposable Members

			public void Dispose()
			{
				parent = null;
				arrKeys = null;
				GC.SuppressFinalize(this);
			}

			#endregion

			#region IEnumerator Members

			public bool MoveNext()
			{
				pos++;
				return pos < parent.items.Count;
			}

			public void Reset()
			{
				pos = -1;
				arrKeys = parent.GetSortedKeys();
			}

			public object Current
			{
				get { return new KeyValuePair<TKey, TValue>(arrKeys[pos], parent.items[pos]); }
			}

			#endregion
		}

		#endregion

		#region KeyValueEntryEnumerator

		private class KeyValueEntryEnumerator : IDictionaryEnumerator
		{
			private readonly IndexedDictionary<TKey, TValue> parent;
			private TKey[] arrKeys;
			private int pos = -1;


			public KeyValueEntryEnumerator(IndexedDictionary<TKey, TValue> parent)
			{
				this.parent = parent;
				((IEnumerator)this).Reset();
			}

			
			#region IDictionaryEnumerator Members

			DictionaryEntry IDictionaryEnumerator.Entry
			{
				get { return new DictionaryEntry(arrKeys[pos], parent.items[pos]); }
			}

			object IDictionaryEnumerator.Key
			{
				get { return arrKeys[pos]; }
			}

			object IDictionaryEnumerator.Value
			{
				get { return parent.items[pos]; }
			}

			#endregion

			#region IEnumerator Members

			object IEnumerator.Current
			{
				get
				{
					return new DictionaryEntry(arrKeys[pos], parent.items[pos]);
				}
			}

			bool IEnumerator.MoveNext()
			{
				pos++;
				return pos < parent.items.Count;
			}

			void IEnumerator.Reset()
			{
				pos = -1;
				arrKeys = parent.GetSortedKeys();
			}

			#endregion
		}

		#endregion


	}

}
