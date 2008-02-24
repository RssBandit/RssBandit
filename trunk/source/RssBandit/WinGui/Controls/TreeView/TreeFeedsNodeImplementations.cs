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
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Infragistics.Win.UltraWinTree;
using NewsComponents;
using RssBandit.SpecialFeeds;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Utility;

namespace RssBandit.WinGui.Controls
{
	#region RootNode
	internal class RootNode: TreeFeedsNodeBase {
		private static ContextMenu _popup = null;	// share one context menu

		public RootNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu):base(text,FeedNodeType.Root, false, imageIndex, selectedImageIndex) {
			_popup = menu; 
			base.Editable = false;
		}

		public override object Clone() {
			return new RootNode(this.Text, (int)this.Override.NodeAppearance.Image, (int)this.Override.SelectedNodeAppearance.Image, _popup);
		}

		#region Implementation of FeedTreeNodeBase
		public override bool AllowedChild(FeedNodeType nsType) {
			return (nsType == FeedNodeType.Feed || nsType == FeedNodeType.Category);
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
	}
	#endregion

	#region CategoryNode
	internal class CategoryNode: TreeFeedsNodeBase {
		private static ContextMenu _popup = null;	// share one context menu

		public CategoryNode(string text):
			base(text,FeedNodeType.Category, true, 2, 3) 
		{
		}
		public CategoryNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu):base(text,FeedNodeType.Category, true, imageIndex, selectedImageIndex) {
			_popup = menu; 
		}

		public override object Clone() {
			return new CategoryNode(this.Text, (int)this.Override.NodeAppearance.Image, (int)this.Override.SelectedNodeAppearance.Image, _popup);
		}

		#region Implementation of FeedTreeNodeBase
		public override bool AllowedChild(FeedNodeType nsType) {
			return (nsType == FeedNodeType.Category || nsType == FeedNodeType.Feed);
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
	}
	#endregion

	#region FeedNode
	
	internal class FeedNode: TreeFeedsNodeBase {
		private static ContextMenu _popup = null;	// share one context menu

		public FeedNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu):
			this(text, imageIndex, selectedImageIndex, menu, null) {			
		}

		public FeedNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu, Image image):base(text,FeedNodeType.Feed, true, imageIndex, selectedImageIndex, image) {
			_popup = menu; 
		}

		public override object Clone() {
			return new FeedNode(this.Text, (int)this.Override.NodeAppearance.Image, (int)this.Override.SelectedNodeAppearance.Image, _popup, this.Override.NodeAppearance.Image as Image);
		}

		#region Implementation of FeedTreeNodeBase
		public override bool AllowedChild(FeedNodeType nsType) {
		 	// no childs allowed
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
	}
	#endregion

	#region SpecialRootNode
	/// <summary>
	/// Root node to hold special folders, like Feed Errors and Flagged Item nodes.
	/// </summary>
	internal class SpecialRootNode:TreeFeedsNodeBase {
		private ContextMenu _popup = null;						// context menu

		public SpecialRootNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu):base(text,FeedNodeType.Root, true, imageIndex, selectedImageIndex) {
			_popup = menu; 
			base.Editable = false;
			base.Nodes.Override.Sort = SortType.None;
		}

		#region Implementation of FeedTreeNodeBase
		public override bool AllowedChild(FeedNodeType nsType) {
			// some childs allowed
			if (nsType == FeedNodeType.Finder ||
				nsType == FeedNodeType.SmartFolder
				)
				return true;

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

	}
	#endregion

	#region WasteBasketNode
	/// <summary>
	/// Stores the deleted items.
	/// </summary>
	internal class WasteBasketNode: SmartFolderNodeBase {

		public WasteBasketNode(LocalFeedsFeed itemStore, int imageIndex, int selectedImageIndex, ContextMenu menu):
			base(itemStore, imageIndex, selectedImageIndex, menu) {}

	}
	#endregion

	#region SentItemsNode
	/// <summary>
	/// Stores the sent items (reply comments, NNTP Posts).
	/// </summary>
	internal class SentItemsNode: SmartFolderNodeBase {

		public SentItemsNode(LocalFeedsFeed itemStore, int imageIndex, int selectedImageIndex, ContextMenu menu):
			base(itemStore, imageIndex, selectedImageIndex, menu) {	}

	}
	#endregion

	#region WatchedItemsNode
	/// <summary>
	/// Stores the watched items.
	/// </summary>
	internal class WatchedItemsNode: SmartFolderNodeBase {

		public WatchedItemsNode(LocalFeedsFeed itemStore, int imageIndex, int selectedImageIndex, ContextMenu menu):
			base(itemStore, imageIndex, selectedImageIndex, menu) {	}

		public override void Remove(INewsItem item) {
			base.Remove (item);
			this.UpdateCommentStatus(this, -1); 
		}

	}
	#endregion

	#region ExceptionReportNode
	/// <summary>
	/// Stores the feed failures and exceptions reported while retrieving.
	/// </summary>
	internal class ExceptionReportNode: SmartFolderNodeBase {

		public ExceptionReportNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu):
			base(null, text, imageIndex, selectedImageIndex, menu) {
		}

		#region Overrides of ISmartFolder to delegate item handling to ExceptionManager instance
		public override void MarkItemRead(INewsItem item) {
			if (item == null) return;
			foreach (NewsItem ri in ExceptionManager.GetInstance().Items) {
				if (item.Equals(ri)) {
					ri.BeenRead = true;
					base.UpdateReadStatus(this, -1);
					break;
				}
			}
		}

		public override void MarkItemUnread(INewsItem item) {
			if (item == null) return;
			foreach (NewsItem ri in ExceptionManager.GetInstance().Items) {
				if (item.Equals(ri)) {
					ri.BeenRead = false;
					break;
				}
			}
		}

		public override bool ContainsNewMessages {
			get {
				foreach (NewsItem ri in ExceptionManager.GetInstance().Items) {
					if (!ri.BeenRead) return true;
				}
				return false;
			}
		}

		public override int NewMessagesCount {
			get {
				int i = 0;
				foreach (NewsItem ri in ExceptionManager.GetInstance().Items) {
					if (!ri.BeenRead) i++;
				}
				return i;
			}
		}

		public override List<INewsItem> Items {
			get {	return ExceptionManager.GetInstance().Items as List<INewsItem>;	}
		}

		public override void Add(INewsItem item) {	// not the preferred way to add exceptions, but impl. the interface
			ExceptionManager.GetInstance().Add(item);
		}
		public override void Remove(INewsItem item) {
			ExceptionManager.GetInstance().Remove(item);
		}

		public override bool Modified {
			get { return ExceptionManager.GetInstance().Modified;  }
			set { ExceptionManager.GetInstance().Modified = value; }
		}

		#endregion

	}
	#endregion

	#region FlaggedItemsNode
	
	/// <summary>
	/// Stores the various flagged items.
	/// </summary>
	internal class FlaggedItemsNode: SmartFolderNodeBase {
		private Flagged flagsFiltered = Flagged.None;

		public FlaggedItemsNode(Flagged flag, LocalFeedsFeed itemStore, string text, int imageIndex, int selectedImageIndex, ContextMenu menu):
			base(itemStore, text, imageIndex, selectedImageIndex, menu) {
			flagsFiltered = flag;
		}

		public Flagged FlagFilter {
			get { return flagsFiltered; }
		}

		#region Some Overrides of ISmartFolder to filter items by flags
		public override void MarkItemRead(INewsItem item) {
			if (item == null) return;
			foreach (NewsItem ri in base.itemsFeed.Items) {
				if (item.Equals(ri) && ri.FlagStatus == flagsFiltered) {
					ri.BeenRead = true;
					base.UpdateReadStatus(this, -1);
					break;
				}
			}
		}

		public override void MarkItemUnread(INewsItem item) {
			if (item == null) return;
			foreach (NewsItem ri in base.itemsFeed.Items) {
				if (item.Equals(ri) && ri.FlagStatus == flagsFiltered) {
					ri.BeenRead = false;
					break;
				}
			}
		}

		public override bool ContainsNewMessages {
			get {
				foreach (NewsItem ri in base.itemsFeed.Items) {
					if (!ri.BeenRead  && ri.FlagStatus == flagsFiltered) return true;
				}
				return false;
			}
		}

		public override int NewMessagesCount {
			get {
				int i = 0;
				foreach (NewsItem ri in base.itemsFeed.Items) {
					if (!ri.BeenRead && ri.FlagStatus == flagsFiltered) i++;
				}
				return i;
			}
		}

		public override List<INewsItem> Items {
			get {	
				List<INewsItem> a = new List<INewsItem>(base.itemsFeed.Items.Count);
				foreach (INewsItem ri in base.itemsFeed.Items) {
					if (ri.FlagStatus == flagsFiltered)
						a.Add(ri);	
				}
				return a;
			}
		}

		#endregion

	}
	
	/// <summary>
	/// The flagged items node root
	/// </summary>
	internal class FlaggedItemsRootNode: TreeFeedsNodeBase {

		private static ContextMenu _popup = null;	// share one context menu
		public FlaggedItemsRootNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu):
			base(text, FeedNodeType.Root, false, imageIndex, selectedImageIndex) {
			_popup = menu; 
		}

		#region Implementation of FeedTreeNodeBase
		public override bool AllowedChild(FeedNodeType nsType) {
			return (nsType == FeedNodeType.SmartFolder);
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
	}
	
	#endregion
	
	#region UnreadItemsNode
	/// <summary>
	/// Stores the unread items.
	/// </summary>
	internal class UnreadItemsNode: SmartFolderNodeBase {

		public UnreadItemsNode(LocalFeedsFeed itemStore, int imageIndex, int selectedImageIndex, ContextMenu menu):
			base(itemStore, imageIndex, selectedImageIndex, menu) {	}
		
		public override void MarkItemRead(INewsItem item) {
			if (item == null) return;
			int idx = itemsFeed.Items.IndexOf(item);
			if (idx >= 0) {
				itemsFeed.Items.RemoveAt(idx);
				base.UpdateReadStatus(this, -1);
			}
		}

		public override void MarkItemUnread(INewsItem item) {
			if (item == null) return;
			int idx = itemsFeed.Items.IndexOf(item);
			if (idx < 0) {
				itemsFeed.Items.Add(item);
				base.UpdateReadStatus(this, 1);
			} else {
				// should not happen, just if we get called this way:
				((INewsItem) itemsFeed.Items[idx]).BeenRead = false;
				base.UpdateReadStatus(this, 1);
			}
		}
	}
	#endregion
	
	#region FinderRootNode
	/// <summary>
	/// Root node to hold persistent FinderNodes.
	/// </summary>
	internal class FinderRootNode:TreeFeedsNodeBase {
		private ContextMenu _popup = null;						// context menu

		public FinderRootNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu):
			base(text,FeedNodeType.Root, true, imageIndex, selectedImageIndex) {
			_popup = menu; 
			base.Editable = false;
		}

		public void InitFromFinders(ArrayList finderList, ContextMenu menu) {
			
			Hashtable categories = new Hashtable();
			
			foreach (RssFinder finder in finderList) {
				TreeFeedsNodeBase parent = this;

				finder.Container = null;	// may contain old node references on a re-populate call

				if (finder.FullPath.IndexOf(FeedSource.CategorySeparator) > 0) {	// one with category
					
					string[] a = finder.FullPath.Split(FeedSource.CategorySeparator.ToCharArray());
					int aLen = a.GetLength(0);
					string sCat = String.Join(FeedSource.CategorySeparator,a, 0, aLen-1);

					if (categories.ContainsKey(sCat)) {
						parent = (TreeFeedsNodeBase)categories[sCat];
					} else {	// create category/categories
						
						StringBuilder sb = new StringBuilder();
						sb.Append(a[0]);
						for (int i = 0; i <= aLen - 2; i++) {
							sCat = sb.ToString();
							if (categories.ContainsKey(sCat)) {
								parent = (TreeFeedsNodeBase)categories[sCat];
							} else {
								TreeFeedsNodeBase cn = new FinderCategoryNode(a[i], 2, 3, menu);	// menu???
								categories.Add(sCat, cn);
								parent.Nodes.Add(cn);
								//								cn.Cells[0].Value = cn.Text;
								//								cn.Cells[0].Appearance.Image = cn.Override.NodeAppearance.Image;
								//								cn.Cells[0].Appearance.Cursor = WinGuiMain.CursorHand;
								parent = cn;
							}
							sb.Append(FeedSource.CategorySeparator + a[i+1]);
						}
					}
				}

				FinderNode n = new FinderNode(finder.Text, 10, 10, menu);
				n.Finder = finder;		// interconnect
				finder.Container = n;
				parent.Nodes.Add(n);
				//				n.Cells[0].Value = n.Text;
				//				n.Cells[0].Appearance.Image = n.Override.NodeAppearance.Image;
				//				n.Cells[0].Appearance.Cursor = WinGuiMain.CursorHand;
			}//foreach finder
		
		}


		#region Implementation of FeedTreeNodeBase
		public override bool AllowedChild(FeedNodeType nsType) {
			// some childs allowed
			if (nsType == FeedNodeType.Finder)
				return true;

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

	}
	#endregion

	#region FinderCategoryNode
	internal class FinderCategoryNode: TreeFeedsNodeBase {
		private static ContextMenu _popup = null;	// share one context menu

		public FinderCategoryNode(string text):
			base(text,FeedNodeType.FinderCategory, true, 2, 3) {}
		public FinderCategoryNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu):
			base(text,FeedNodeType.FinderCategory, true, imageIndex, selectedImageIndex) {
			_popup = menu; 
		}


		#region Implementation of FeedTreeNodeBase
		public override bool AllowedChild(FeedNodeType nsType) {
			return (nsType == FeedNodeType.FinderCategory || nsType == FeedNodeType.Finder);
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
	}
	#endregion

	#region FinderNode
	/// <summary>
	/// Represents a search folder in the tree.
	/// </summary>
	public class FinderNode:TreeFeedsNodeBase, ISmartFolder {
		private ContextMenu _popup = null;						// context menu
		private LocalFeedsFeed itemsFeed;
        //use a dictionary because we want fast access to objects
        private List<INewsItem> items = new List<INewsItem>();
		private RssFinder finder = null;

		public FinderNode():base() {}
		public FinderNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu):
			base(text,FeedNodeType.Finder, true, imageIndex, selectedImageIndex) {
			itemsFeed = new LocalFeedsFeed(
				"http://localhost/rssbandit/searchfolder?id="+Guid.NewGuid(),
				text,	String.Empty, false);
			_popup = menu; 
		}
		
		public virtual bool IsTempFinderNode {
			get { return false; }
		}
		
		public virtual RssFinder Finder {
			get { return finder;	}
			set { finder = value;  }
		}

		public string InternalFeedLink {
			get { return itemsFeed.link; }
		}

		public bool Contains(NewsItem item) {	
			return items.Contains(item);
		}

		public void Clear() {	
			items.Clear();
		}

		#region Implementation of FeedTreeNodeBase
		public override bool AllowedChild(FeedNodeType nsType) {
			// no childs allowed
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

		public bool ContainsNewMessages {
			get {
				foreach (INewsItem ri in items) {
					if (!ri.BeenRead) return true;
				}
				return false;
			}
		}

		public int NewMessagesCount {
			get {
				int i = 0;
				foreach (INewsItem ri in items) {
					if (!ri.BeenRead) i++;
				}
				return i;
			}
		}

		public virtual bool HasNewComments {
			get {
				foreach (INewsItem ri in items) {
					if (ri.HasNewComments) return true;
				}
				return false;
			}
		}


		public virtual int NewCommentsCount {
			get {
				int count = 0;
				foreach (INewsItem ri in items) {
					if (ri.HasNewComments) count++;
				}
				return count;
			}
		}

		public void MarkItemRead(INewsItem item) {
			if (item == null) return;
            int index = items.IndexOf(item);
			if (index!= -1){
				INewsItem ri = items[index];
				if (!ri.BeenRead) {
					ri.BeenRead = true;
					base.UpdateReadStatus(this, -1);
				}
			}
		}

		public void MarkItemUnread(INewsItem item) {
			if (item == null) return;
            int index = items.IndexOf(item);
			if (index!= -1){
				INewsItem ri = items[index];
				if (ri.BeenRead) {
					ri.BeenRead = false;
					base.UpdateReadStatus(this, 1);
				}
			}
		}

		public List<INewsItem> Items {
			get {	
				return items;	
			}
		}

		public void Add(INewsItem item) {	
			if (item == null) return;
            if (!items.Contains(item))
				items.Add(item);
		}
		public void AddRange(IList<INewsItem> newItems)
		{	
			if (newItems == null) return;
			for (int i=0; i < newItems.Count; i++) {
				NewsItem item = newItems[i] as NewsItem;
				if (item != null && !items.Contains(item))
					items.Add(item);
			}
		}
		public void Remove(INewsItem item) {
			if (item == null) return;
			if (items.Contains(item))
				items.Remove(item);
		}

		public void UpdateReadStatus() {
			base.UpdateReadStatus(this, this.NewMessagesCount);
		}

		public virtual void UpdateCommentStatus(){
			base.UpdateCommentStatus(this, this.NewCommentsCount); 
		}

		public bool Modified {
			get { return itemsFeed.Modified;  }
			set { itemsFeed.Modified = value; }
		}

		#endregion

	}
	#endregion
	
	#region TempFinderNode
	/// <summary>
	/// Represents a search folder in the tree for temporary
	/// search results (not persisted as a search folder)
	/// </summary>
	public class TempFinderNode : FinderNode
	{
		public TempFinderNode():base() {}
		public TempFinderNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu):
		base(text, imageIndex, selectedImageIndex, menu) {
		}
		
		public override bool IsTempFinderNode {
			get { return true; }
		}
		
		public override RssFinder Finder
		{
			get { return base.Finder; }
			set {
				base.Finder = value;
				if (base.Finder != null)
					base.Finder.ShowFullItemContent = false;
			}
		}
	}
	#endregion
}

#region CVS Version Log
/*
 * $Log: TreeFeedsNodeImplementations.cs,v $
 * Revision 1.12  2007/05/18 11:46:45  t_rendelmann
 * fixed: no category context menus displayed after OPML import or remote sync.
 *
 * Revision 1.11  2007/02/17 12:35:28  t_rendelmann
 * new: "Show item full texts" is now a context menu option on search folders
 *
 * Revision 1.10  2007/01/14 19:30:47  t_rendelmann
 * cont. SearchPanel: first main form integration and search working (scope/populate search scope tree is still a TODO)
 *
 * Revision 1.9  2006/12/05 04:06:25  carnage4life
 * Made changes so that when comments for an item are viewed from Watched Items folder, the actual feed is updated and vice versa
 *
 * Revision 1.8  2006/12/03 01:20:14  carnage4life
 * Made changes to support Watched Items feed showing when new comments found
 *
 * Revision 1.7  2006/10/27 19:14:14  t_rendelmann
 * fixed: sorting of root node was again language dependent;
 * changed: added the UnreadItemsNode
 *
 * Revision 1.6  2006/09/29 18:14:36  t_rendelmann
 * a) integrated lucene index refreshs;
 * b) now using a centralized defined category separator;
 * c) unified decision about storage relevant changes to feed, feed and feeditem properties;
 * d) fixed: issue [ 1546921 ] Extra Category Folders Created
 * e) fixed: issue [ 1550083 ] Problem when renaming categories
 *
 * Revision 1.5  2006/09/22 15:35:44  t_rendelmann
 * added CVS header and change history
 *
 */
#endregion
