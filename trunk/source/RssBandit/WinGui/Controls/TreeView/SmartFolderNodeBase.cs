#region CVS Version Header
/*
 * $Id: SmartFolderNodeBase.cs,v 1.5 2006/12/03 01:20:14 carnage4life Exp $
 * Last modified by $Author: carnage4life $
 * Last modified at $Date: 2006/12/03 01:20:14 $
 * $Revision: 1.5 $
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

using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using NewsComponents;
using RssBandit.SpecialFeeds;
using RssBandit.WinGui.Interfaces;

namespace RssBandit.WinGui.Controls
{
	/// <summary>
	/// Provides a base implementation of ISmartFolder.
	/// </summary>
	public class SmartFolderNodeBase: TreeFeedsNodeBase, ISmartFolder {
		private ContextMenu _popup = null;
		protected LocalFeedsFeed itemsFeed;

		public SmartFolderNodeBase(LocalFeedsFeed itemStore, int imageIndex, int selectedImageIndex, ContextMenu menu):
			this(itemStore, itemStore.title, imageIndex, selectedImageIndex, menu) {
		}

		public SmartFolderNodeBase(LocalFeedsFeed itemStore, string text, int imageIndex, int selectedImageIndex, ContextMenu menu):
			base(text, FeedNodeType.SmartFolder, false, imageIndex, selectedImageIndex) {
			_popup = menu; 
			itemsFeed = itemStore;
		}

		#region Implementation of FeedTreeNodeBase
		public override bool AllowedChild(FeedNodeType nsType) {
			return false;
		}
		
		public override void PopupMenu(System.Drawing.Point screenPos) {
			/*
			  if (_popup != null)
				  _popup.TrackPopup(screenPos);
			  */
		}
		public override void UpdateContextMenu() {
			if (base.Control != null)
				base.Control.ContextMenu = _popup;
		}
		#endregion

		#region ISmartFolder Members

		public virtual bool ContainsNewMessages {
			get {
				foreach (NewsItem ri in itemsFeed.Items) {
					if (!ri.BeenRead) return true;
				}
				return false;
			}
		}


		public virtual int NewMessagesCount {
			get {
				int count = 0;
				for (int i=0; i < itemsFeed.Items.Count; i++) 
				{
					NewsItem ri = (NewsItem)itemsFeed.Items[i];
					if (!ri.BeenRead) count++;
				}
				return count;
			}
		}

		public virtual bool HasNewComments {
			get {
				foreach (NewsItem ri in itemsFeed.Items) {
					if (ri.HasNewComments) return true;
				}
				return false;
			}
		}


		public virtual int NewCommentsCount {
			get {
				int count = 0;
				for (int i=0; i < itemsFeed.Items.Count; i++) {
					NewsItem ri = (NewsItem)itemsFeed.Items[i];
					if (ri.HasNewComments) count++;
				}
				return count;
			}
		}

		public virtual void MarkItemRead(NewsItem item) {
			if (item == null) return;
			int index = itemsFeed.Items.IndexOf(item);
			if (index >= 0)
			{
				NewsItem ri = (NewsItem)itemsFeed.Items[index];
				ri.BeenRead = true;
				base.UpdateReadStatus(this, -1);
			}
		}

		public virtual void MarkItemUnread(NewsItem item) {
			if (item == null) return;
			int index = itemsFeed.Items.IndexOf(item);
			if (index >= 0)
			{
				NewsItem ri = (NewsItem)itemsFeed.Items[index];
				ri.BeenRead = false;
			}
		}

		public virtual List<NewsItem> Items {
			get {	return itemsFeed.Items;	}
		}

		public virtual void Add(NewsItem item) {	
			itemsFeed.Add(item);
		}
		public virtual void Remove(NewsItem item) {
			itemsFeed.Remove(item);
		}

		public virtual void UpdateReadStatus() {
			base.UpdateReadStatus(this, this.NewMessagesCount);
		}

		public virtual void UpdateCommentStatus(){
			base.UpdateCommentStatus(this, this.NewCommentsCount); 
		}

		public virtual bool Modified {
			get { return itemsFeed.Modified;  }
			set { itemsFeed.Modified = value; }
		}

		#endregion
	}
}
