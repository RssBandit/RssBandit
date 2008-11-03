#region CVS Version Header

/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */

#endregion

#region CVS Version Log

/*
 * $Log: SmartFolderNodeBase.cs,v $
 * Revision 1.5  2006/12/03 01:20:14  carnage4life
 * Made changes to support Watched Items feed showing when new comments found
 *
 * Revision 1.4  2006/11/01 16:03:53  t_rendelmann
 * small optimizations
 *
 * Revision 1.3  2006/10/27 19:09:15  t_rendelmann
 * fixed: some treenode caption are editiable, that should be not
 *
 * Revision 1.2  2006/09/22 15:35:44  t_rendelmann
 * added CVS header and change history
 *
 */

#endregion

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NewsComponents;
using RssBandit.SpecialFeeds;
using RssBandit.WinGui.Interfaces;

namespace RssBandit.WinGui.Controls
{
    /// <summary>
    /// Provides a base implementation of ISmartFolder.
    /// </summary>
    internal class SmartFolderNodeBase : TreeFeedsNodeBase, ISmartFolder
    {
		protected ContextMenu p_popup;
        protected LocalFeedsFeed itemsFeed;

        public SmartFolderNodeBase(LocalFeedsFeed itemStore, int imageIndex, int selectedImageIndex, ContextMenu menu) :
            this(itemStore, itemStore.title, imageIndex, selectedImageIndex, menu)
        {
        }

        public SmartFolderNodeBase(LocalFeedsFeed itemStore, string text, int imageIndex, int selectedImageIndex,
                                   ContextMenu menu) :
                                       base(text, FeedNodeType.SmartFolder, false, imageIndex, selectedImageIndex)
        {
            p_popup = menu;
            itemsFeed = itemStore;
        }

        #region Implementation of FeedTreeNodeBase

        public override bool AllowedChild(FeedNodeType nsType)
        {
            return false;
        }

        public override void PopupMenu(Point screenPos)
        {
            /*
			  if (p_popup != null)
				  p_popup.TrackPopup(screenPos);
			  */
        }

        public override void UpdateContextMenu()
        {
            if (Control != null)
                Control.ContextMenu = p_popup;
        }

        #endregion

        #region ISmartFolder Members

        protected virtual bool ContainsNewMessages
        {
            get
            {
                foreach (var ri in itemsFeed.Items)
                {
                    if (!ri.BeenRead) return true;
                }
                return false;
            }
        }


		protected virtual int NewMessagesCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < itemsFeed.Items.Count; i++)
                {
                    INewsItem ri = itemsFeed.Items[i];
                    if (!ri.BeenRead) count++;
                }
                return count;
            }
        }

		protected virtual bool HasNewComments
        {
            get
            {
                foreach (NewsItem ri in itemsFeed.Items)
                {
                    if (ri.HasNewComments) return true;
                }
                return false;
            }
        }


		protected virtual int NewCommentsCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < itemsFeed.Items.Count; i++)
                {
                    INewsItem ri = itemsFeed.Items[i];
                    if (ri.HasNewComments) count++;
                }
                return count;
            }
        }

		protected virtual void MarkItemRead(INewsItem item)
        {
            if (item == null) return;
            int index = itemsFeed.Items.IndexOf(item);
            if (index >= 0)
            {
                INewsItem ri = itemsFeed.Items[index];
                ri.BeenRead = true;
                UpdateReadStatus(this, -1);
            }
        }

		protected virtual void MarkItemUnread(INewsItem item)
        {
            if (item == null) return;
            int index = itemsFeed.Items.IndexOf(item);
            if (index >= 0)
            {
                INewsItem ri = itemsFeed.Items[index];
                ri.BeenRead = false;
            }
        }

		protected virtual IList<INewsItem> Items
        {
            get { return itemsFeed.Items; }
        }

		protected virtual void Add(INewsItem item)
        {
            itemsFeed.Add(item);
        }

		protected virtual void Remove(INewsItem item)
        {
            itemsFeed.Remove(item);
        }

        public virtual void UpdateReadStatus()
        {
            UpdateReadStatus(this, NewMessagesCount);
        }

		protected virtual void UpdateCommentStatus()
        {
            UpdateCommentStatus(this, NewCommentsCount);
        }

		protected virtual bool Modified
        {
            get { return itemsFeed.Modified; }
            set { itemsFeed.Modified = value; }
        }

        #endregion

		#region ISmartFolder Members

		bool ISmartFolder.ContainsNewMessages
		{
			get { return this.ContainsNewMessages; }
		}

		bool ISmartFolder.HasNewComments
		{
			get { return this.HasNewComments; }
		}

		int ISmartFolder.NewMessagesCount
		{
			get { return this.NewMessagesCount; }
		}

		int ISmartFolder.NewCommentsCount
		{
			get { return this.NewCommentsCount; }
		}

		void ISmartFolder.MarkItemRead(INewsItem item)
		{
			this.MarkItemRead(item);
		}

		void ISmartFolder.MarkItemUnread(INewsItem item)
		{
			this.MarkItemUnread(item);
		}

		IList<INewsItem> ISmartFolder.Items
		{
			get { return this.Items; }
		}

		void ISmartFolder.Add(INewsItem item)
		{
			this.Add(item);
		}

		void ISmartFolder.Remove(INewsItem item)
		{
			this.Remove(item);
		}

		void ISmartFolder.UpdateReadStatus()
		{
			this.UpdateReadStatus();
		}

		void ISmartFolder.UpdateCommentStatus()
		{
			this.UpdateCommentStatus();
		}

		bool ISmartFolder.Modified
		{
			get { return this.Modified; }
			set { this.Modified = value; }
		}

		#endregion
	}
}