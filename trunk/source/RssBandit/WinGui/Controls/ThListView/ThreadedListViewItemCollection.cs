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
using System.Windows.Forms; 

namespace System.Windows.Forms.ThListView
{
	/// <summary>
	/// Summary description for ThreadedListViewItemCollection.
	/// </summary>
	public class ThreadedListViewItemCollection: System.Windows.Forms.ListView.ListViewItemCollection, IEnumerable<ThreadedListViewItem> {
		
		public delegate void ItemAddedEventHandler(object sender, ListViewItemEventArgs e);
		public delegate void ItemRemovedEventHandler(object sender, ListViewItemEventArgs e);
		public event ItemAddedEventHandler ItemAdded; 
		public event ItemRemovedEventHandler ItemRemoved; 

		private ThreadedListView owner;
		public ThreadedListViewItemCollection(ThreadedListView owner) : base(((ListView)owner)) { 
			this.owner = owner;
		} 

		protected ThreadedListView ListView { get { return this.owner; } }

		#region Add()
		public ThreadedListViewItem Add(ThreadedListViewItem item) { 
			ThreadedListViewItem itm = ((ThreadedListViewItem)base.Add(item)); 
			if (ListView.ShowInGroups) {
				Win32.API.AddItemToGroup(itm.ListView.Handle, itm.Index, itm.GroupIndex); 
				if (ItemAdded != null) { 
					ItemAdded(this, new ListViewItemEventArgs(itm)); 
				} 
			}
			return itm; 
		} 

		public new ThreadedListViewItem Add(string text) { 
			return this.Add(null, text); 
		} 
		public ThreadedListViewItem Add(object key, string text) { 
			ThreadedListViewItem itm = new ThreadedListViewItem(key, text); 
			return Add(itm); 
		} 

		public ThreadedListViewItem Add(string text, int imageIndex, int groupindex) { 
			ThreadedListViewItem itm = new ThreadedListViewItem(null, text, imageIndex, groupindex); 
			return Add(itm); 
		} 

		#endregion

		public void AddRange(ThreadedListViewItem[] values) { 
			base.AddRange(values); 
			if (ListView.ShowInGroups) {
				foreach (ThreadedListViewItem itm in values) { 
					Win32.API.AddItemToGroup(itm.ListView.Handle, itm.Index, itm.GroupIndex); 
					if (ItemAdded != null) { 
						ItemAdded(this, new ListViewItemEventArgs(itm)); 
					} 
				} 
			}
		} 

		public bool Contains(ThreadedListViewItem item) { 
			return base.Contains(item); 
		} 

		public int IndexOf(ThreadedListViewItem item) { 
			return base.IndexOf(item); 
		} 

		public ThreadedListViewItem Insert(int index, ThreadedListViewItem item) { 
			return ((ThreadedListViewItem)base.Insert(index, item)); 
		} 

		public new ThreadedListViewItem this[int displayIndex] { 
			get { 
				return ((ThreadedListViewItem)base[displayIndex]);  //((ThreadedListViewItem)this[displayIndex])
			} 
			set {
				base[displayIndex] = value; 
			} 
		} 

		public void Remove(ThreadedListViewItem item) { 
			if (ItemRemoved != null) { 
				ItemRemoved(this, new ListViewItemEventArgs(item)); 
			} 
			base.Remove(item); 
		} 

		public new void RemoveAt(int index) { 
			if (ItemRemoved != null) { 
				ItemRemoved(this, new ListViewItemEventArgs(this[index])); 
			} 
			base.RemoveAt(index); 
		} 

		public void CopyTo(ThreadedListViewItem[] array, int index) { 
			base.CopyTo(array, index); 
		}

        #region Implementation (IEnumerable)

        public new IEnumerator<ThreadedListViewItem> GetEnumerator() {
            return new Enumerator(this);
        }
        #endregion

        #region Nested enumerator class

        private class Enumerator : IEnumerator<ThreadedListViewItem> {
            #region Implementation (data)

            private ThreadedListViewItemCollection m_collection;
            private int m_index;
            private int m_version;

            #endregion

            #region Construction

            /// <summary>
            ///		Initializes a new instance of the <c>Enumerator</c> class.
            /// </summary>
            /// <param name="tc"></param>
            internal Enumerator(ThreadedListViewItemCollection tc) {
                m_collection = tc;
                m_index = -1;               
            }

            #endregion

            #region Operations (type-safe IEnumerator)

            /// <summary>
            ///		Gets the current element in the collection.
            /// </summary>
            public ThreadedListViewItem Current {
                get { return m_collection[m_index]; }
            }

            /// <summary>
            ///		Advances the enumerator to the next element in the collection.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            ///		The collection was modified after the enumerator was created.
            /// </exception>
            /// <returns>
            ///		<c>true</c> if the enumerator was successfully advanced to the next element; 
            ///		<c>false</c> if the enumerator has passed the end of the collection.
            /// </returns>
            public bool MoveNext() {
            /*     if (m_version != m_collection.m_version)
                    throw new System.InvalidOperationException("Collection was modified; enumeration operation may not execute.");
             */ 

                ++m_index;
                return (m_index < m_collection.Count) ? true : false;
            }

            /// <summary>
            ///		Sets the enumerator to its initial position, before the first element in the collection.
            /// </summary>
            public void Reset() {
                m_index = -1;
            }
            #endregion

            #region Implementation (IEnumerator)

            object IEnumerator.Current {
                get { return this.Current; }
            }

            #endregion

            #region (IDisposable)

            void IDisposable.Dispose() {
                return;
            }

            #endregion
        }


        #endregion 
	}


	public class ListViewItemEventArgs : EventArgs { 
		private ThreadedListViewItem mItem = new ThreadedListViewItem(); 

		public ListViewItemEventArgs(ThreadedListViewItem item) { 
			mItem = item; 
		} 

		public ThreadedListViewItem Item { 
			get { 
				return mItem; 
			} 
			set { 
				mItem = value; 
			}
        }      
    } 



}
