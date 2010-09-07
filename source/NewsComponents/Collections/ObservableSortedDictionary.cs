/* Copyright (c) 2007, Dr. WPF
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *   * Redistributions of source code must retain the above copyright
 *     notice, this list of conditions and the following disclaimer.
 * 
 *   * Redistributions in binary form must reproduce the above copyright
 *     notice, this list of conditions and the following disclaimer in the
 *     documentation and/or other materials provided with the distribution.
 * 
 *   * The name Dr. WPF may not be used to endorse or promote products
 *     derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY Dr. WPF ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL Dr. WPF BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace DrWPF.Windows.Data
{
    [Serializable]
    public class ObservableSortedDictionary<TKey, TValue> : ObservableDictionary<TKey, TValue>, ISerializable, IDeserializationCallback
    {
        #region constructors

        #region public

        public ObservableSortedDictionary(IComparer<DictionaryEntry> comparer)
            : base()
        {
            _comparer = comparer;
        }

        public ObservableSortedDictionary(IComparer<DictionaryEntry> comparer, IDictionary<TKey, TValue> dictionary)
            : base(dictionary)
        {
            _comparer = comparer;
        }

        public ObservableSortedDictionary(IComparer<DictionaryEntry> comparer, IEqualityComparer<TKey> equalityComparer)
            : base(equalityComparer)
        {
            _comparer = comparer;
        }

        public ObservableSortedDictionary(IComparer<DictionaryEntry> comparer, IDictionary<TKey, TValue> dictionary,
            IEqualityComparer<TKey> equalityComparer)
            : base(dictionary, equalityComparer)
        {
            _comparer = comparer;
        }

        protected ObservableSortedDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _siInfo = info;
        }

        #endregion public

        #endregion constructors

        #region methods

        #region protected

        protected override bool AddEntry(TKey key, TValue value)
        {
            DictionaryEntry entry = new DictionaryEntry(key, value);
            int index = GetInsertionIndexForEntry(entry);
            _keyedEntryCollection.Insert(index, entry);
            return true;
        }

        protected virtual int GetInsertionIndexForEntry(DictionaryEntry newEntry)
        {
            return BinaryFindInsertionIndex(0, Count - 1, newEntry);
        }

        protected override bool SetEntry(TKey key, TValue value)
        {
            bool keyExists = _keyedEntryCollection.Contains(key);

            // if identical key/value pair already exists, nothing to do
            if (keyExists && value.Equals((TValue)_keyedEntryCollection[key].Value))
                return false;

            // otherwise, remove the existing entry
            if (keyExists)
                _keyedEntryCollection.Remove(key);

            // add the new entry
            DictionaryEntry entry = new DictionaryEntry(key, value);
            int index = GetInsertionIndexForEntry(entry);
            _keyedEntryCollection.Insert(index, entry);

            return true;
        }

        #endregion protected

        #region private

        private int BinaryFindInsertionIndex(int first, int last, DictionaryEntry entry)
        {
            if (last < first)
                return first;
            else
            {
                int mid = first + (int)((last - first) / 2);
                int result = _comparer.Compare(_keyedEntryCollection[mid], entry);
                if (result == 0)
                    return mid;
                else if (result < 0)
                    return BinaryFindInsertionIndex(mid + 1, last, entry);
                else
                    return BinaryFindInsertionIndex(first, mid - 1, entry);
            }
        }

        #endregion private

        #endregion methods

        #region interfaces

        #region ISerializable

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            if (!_comparer.GetType().IsSerializable)
            {
                throw new NotSupportedException("The supplied Comparer is not serializable.");
            }

            base.GetObjectData(info, context);
            info.AddValue("_comparer", _comparer);
        }

        #endregion ISerializable

        #region IDeserializationCallback

        public override void OnDeserialization(object sender)
        {
            if (_siInfo != null)
            {
                _comparer = (IComparer<DictionaryEntry>)_siInfo.GetValue("_comparer", typeof(IComparer<DictionaryEntry>));
            }
            base.OnDeserialization(sender);
        }

        #endregion IDeserializationCallback

        #endregion interfaces

        #region fields

        private IComparer<DictionaryEntry> _comparer;

        [NonSerialized]
        private SerializationInfo _siInfo = null;

        #endregion fields
    }

}