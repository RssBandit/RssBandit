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
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RssBandit.WinGui.Controls.ThListView
{
    /// <summary>
    /// Summary description for ThreadedListViewItemCollection.
    /// </summary>
    public class ThreadedListViewItemCollection : ListView.ListViewItemCollection, IList<ThreadedListViewItem>
    {
        public delegate void ItemAddedEventHandler(object sender, ListViewItemEventArgs e);

        public delegate void ItemRemovedEventHandler(object sender, ListViewItemEventArgs e);

        public event ItemAddedEventHandler ItemAdded;
        public event ItemRemovedEventHandler ItemRemoved;

        private readonly ThreadedListView owner;

        public ThreadedListViewItemCollection(ThreadedListView owner) : base(owner)
        {
            this.owner = owner;
        }

        protected ThreadedListView ListView
        {
            get { return owner; }
        }

        #region Add()

        public void Add(ThreadedListViewItem item)
        {
            var itm = ((ThreadedListViewItem) base.Add(item));
            if (ListView.ShowInGroups)
            {
                Win32.API.AddItemToGroup(itm.ListView.Handle, itm.Index, itm.GroupIndex);
                if (ItemAdded != null)
                {
                    ItemAdded(this, new ListViewItemEventArgs(itm));
                }
            }
        }

        public new void Add(string text)
        {
            Add(null, text);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods")]
		public void Add(object key, string text)
        {
            var itm = new ThreadedListViewItem(key, text);
            Add(itm);
        }

        public void Add(string text, int imageIndex, int groupindex)
        {
            var itm = new ThreadedListViewItem(null, text, imageIndex, groupindex);
            Add(itm);
        }

        #endregion

        public void AddRange(ThreadedListViewItem[] values)
        {
            base.AddRange(values);
            if (ListView.ShowInGroups)
            {
                foreach (var itm in values)
                {
                    Win32.API.AddItemToGroup(itm.ListView.Handle, itm.Index, itm.GroupIndex);
                    if (ItemAdded != null)
                    {
                        ItemAdded(this, new ListViewItemEventArgs(itm));
                    }
                }
            }
        }

        public bool Contains(ThreadedListViewItem item)
        {
            return base.Contains(item);
        }

        public int IndexOf(ThreadedListViewItem item)
        {
            return base.IndexOf(item);
        }

        public void Insert(int index, ThreadedListViewItem item)
        {
            base.Insert(index, item);
        }

        public new ThreadedListViewItem this[int displayIndex]
        {
            get { return ((ThreadedListViewItem) base[displayIndex]); //((ThreadedListViewItem)this[displayIndex])
            }
            set { base[displayIndex] = value; }
        }

        public bool Remove(ThreadedListViewItem item)
        {
            if (ItemRemoved != null)
            {
                ItemRemoved(this, new ListViewItemEventArgs(item));
            }


            bool removed = Contains(item);

            if (removed)
                base.Remove(item);

            return removed;
        }

        public new void RemoveAt(int index)
        {
            if (ItemRemoved != null)
            {
                ItemRemoved(this, new ListViewItemEventArgs(this[index]));
            }
            base.RemoveAt(index);
        }

        public void CopyTo(ThreadedListViewItem[] array, int index)
        {
            base.CopyTo(array, index);
        }


        public new IEnumerator<ThreadedListViewItem> GetEnumerator()
        {
            IEnumerator en = base.GetEnumerator();
            while (en.MoveNext())
                yield return (ThreadedListViewItem) en.Current;
        }
    }


    public class ListViewItemEventArgs : EventArgs
    {
        private ThreadedListViewItem mItem = new ThreadedListViewItem();

        public ListViewItemEventArgs(ThreadedListViewItem item)
        {
            mItem = item;
        }

        public ThreadedListViewItem Item
        {
            get { return mItem; }
            set { mItem = value; }
        }
    }
}