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
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Infragistics.Win.UltraWinTree;
using NewsComponents;
using RssBandit.AppServices;
using RssBandit.SpecialFeeds;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Utility;

namespace RssBandit.WinGui.Controls
{

    #region SubscriptionRootNode

	internal class SubscriptionRootNode : TreeFeedsNodeBase
    {
        private static ContextMenu _popup; // share one context menu

    	public int SourceID;

        public SubscriptionRootNode(int sourceID, string text, int imageIndex, int selectedImageIndex, ContextMenu menu)
            : base(text, FeedNodeType.Root, true, imageIndex, selectedImageIndex)
        {
        	SourceID = sourceID;
            _popup = menu;
        }

        public override object Clone()
        {
            return new SubscriptionRootNode(SourceID, Text, (int) Override.NodeAppearance.Image, (int) Override.SelectedNodeAppearance.Image,
                                _popup);
        }

        #region Implementation of FeedTreeNodeBase

        public override bool AllowedChild(FeedNodeType nsType)
        {
            return (nsType == FeedNodeType.Feed || nsType == FeedNodeType.Category);
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
                Control.ContextMenu = _popup;
        }

        #endregion
    }

    #endregion

    #region CategoryNode

    internal class CategoryNode : TreeFeedsNodeBase
    {
        private static ContextMenu _popup; // share one context menu

        public CategoryNode(string text) :
            base(text, FeedNodeType.Category, true, 2, 3)
        {
        }

        public CategoryNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu)
            : base(text, FeedNodeType.Category, true, imageIndex, selectedImageIndex)
        {
            _popup = menu;
        }

        public override object Clone()
        {
            return new CategoryNode(Text, (int) Override.NodeAppearance.Image,
                                    (int) Override.SelectedNodeAppearance.Image, _popup);
        }

        #region Implementation of FeedTreeNodeBase

        public override bool AllowedChild(FeedNodeType nsType)
        {
            return (nsType == FeedNodeType.Category || nsType == FeedNodeType.Feed);
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
                Control.ContextMenu = _popup;
        }

        #endregion
    }

    #endregion

    #region FeedNode

    internal class FeedNode : TreeFeedsNodeBase
    {
        private static ContextMenu _popup; // share one context menu

        public FeedNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu) :
            this(text, imageIndex, selectedImageIndex, menu, null)
        {
        }

        public FeedNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu, Image image)
            : base(text, FeedNodeType.Feed, true, imageIndex, selectedImageIndex, image)
        {
            _popup = menu;
        }

        public override object Clone()
        {
            return new FeedNode(Text, (int) Override.NodeAppearance.Image, (int) Override.SelectedNodeAppearance.Image,
                                _popup, Override.NodeAppearance.Image as Image);
        }

        #region Implementation of FeedTreeNodeBase

        public override bool AllowedChild(FeedNodeType nsType)
        {
            // no childs allowed
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
                Control.ContextMenu = _popup;
        }

        #endregion
    }

    #endregion

    #region SpecialRootNode

    /// <summary>
    /// Root node to hold special folders, like Feed Errors and Flagged Item nodes.
    /// </summary>
	internal class SpecialRootNode : TreeFeedsNodeBase
    {
        private readonly ContextMenu _popup; // context menu

        public SpecialRootNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu)
            : base(text, FeedNodeType.Root, true, imageIndex, selectedImageIndex)
        {
            _popup = menu;
            base.Editable = false;
            Nodes.Override.Sort = SortType.None;
        }

        #region Implementation of FeedTreeNodeBase

        public override bool AllowedChild(FeedNodeType nsType)
        {
            // some childs allowed
            if (nsType == FeedNodeType.Finder ||
                nsType == FeedNodeType.SmartFolder
                )
                return true;

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
                Control.ContextMenu = _popup;
        }

        #endregion
    }

    #endregion

    #region WasteBasketNode

    /// <summary>
    /// Stores the deleted items.
    /// </summary>
    internal class WasteBasketNode : SmartFolderNodeBase
    {
        public WasteBasketNode(LocalFeedsFeed itemStore, int imageIndex, int selectedImageIndex, ContextMenu menu) :
            base(itemStore, imageIndex, selectedImageIndex, menu)
        {
        }
    }

    #endregion

    #region SentItemsNode

    /// <summary>
    /// Stores the sent items (reply comments, NNTP Posts).
    /// </summary>
    internal class SentItemsNode : SmartFolderNodeBase
    {
        public SentItemsNode(LocalFeedsFeed itemStore, int imageIndex, int selectedImageIndex, ContextMenu menu) :
            base(itemStore, imageIndex, selectedImageIndex, menu)
        {
        }
    }

    #endregion

    #region WatchedItemsNode

    /// <summary>
    /// Stores the watched items.
    /// </summary>
    internal class WatchedItemsNode : SmartFolderNodeBase
    {
        public WatchedItemsNode(LocalFeedsFeed itemStore, int imageIndex, int selectedImageIndex, ContextMenu menu) :
            base(itemStore, imageIndex, selectedImageIndex, menu)
        {
        }

        protected override void Remove(INewsItem item)
        {
            base.Remove(item);
            UpdateCommentStatus(this, -1);
        }
    }

    #endregion

    #region ExceptionReportNode

    /// <summary>
    /// Stores the feed failures and exceptions reported while retrieving.
    /// </summary>
    internal class ExceptionReportNode : SmartFolderNodeBase
    {
        public ExceptionReportNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu) :
            base(null, text, imageIndex, selectedImageIndex, menu)
        {
        }

        #region Overrides of ISmartFolder to delegate item handling to ExceptionManager instance

        protected override void MarkItemRead(INewsItem item)
        {
            if (item == null) return;
            foreach (NewsItem ri in ExceptionManager.GetInstance().Items)
            {
                if (item.Equals(ri))
                {
                    ri.BeenRead = true;
                    UpdateReadStatus(this, -1);
                    break;
                }
            }
        }

		protected override void MarkItemUnread(INewsItem item)
        {
            if (item == null) return;
            foreach (NewsItem ri in ExceptionManager.GetInstance().Items)
            {
                if (item.Equals(ri))
                {
                    ri.BeenRead = false;
                    break;
                }
            }
        }

		protected override bool ContainsNewMessages
        {
            get
            {
                foreach (NewsItem ri in ExceptionManager.GetInstance().Items)
                {
                    if (!ri.BeenRead) return true;
                }
                return false;
            }
        }

		protected override int NewMessagesCount
        {
            get
            {
                int i = 0;
                foreach (NewsItem ri in ExceptionManager.GetInstance().Items)
                {
                    if (!ri.BeenRead) i++;
                }
                return i;
            }
        }

		protected override IList<INewsItem> Items
        {
            get { return ExceptionManager.GetInstance().Items as List<INewsItem>; }
        }

		protected override void Add(INewsItem item)
        {
            // not the preferred way to add exceptions, but impl. the interface
            ExceptionManager.GetInstance().Add(item);
        }

		protected override void Remove(INewsItem item)
        {
            ExceptionManager.GetInstance().Remove(item);
        }

		protected override bool Modified
        {
            get { return ExceptionManager.GetInstance().Modified; }
            set { ExceptionManager.GetInstance().Modified = value; }
        }

        #endregion
    }

    #endregion

    #region FlaggedItemsNode

    /// <summary>
    /// Stores the various flagged items.
    /// </summary>
    internal class FlaggedItemsNode : SmartFolderNodeBase
    {
        private readonly Flagged flagsFiltered = Flagged.None;

        public FlaggedItemsNode(Flagged flag, LocalFeedsFeed itemStore, string text, int imageIndex,
                                int selectedImageIndex, ContextMenu menu) :
                                    base(itemStore, text, imageIndex, selectedImageIndex, menu)
        {
            flagsFiltered = flag;
        }

        public Flagged FlagFilter
        {
            get { return flagsFiltered; }
        }

        #region Some Overrides of ISmartFolder to filter items by flags

		protected override void MarkItemRead(INewsItem item)
        {
            if (item == null) return;
            foreach (var ri in itemsFeed.Items)
            {
                if (item.Equals(ri) && ri.FlagStatus == flagsFiltered)
                {
                    ri.BeenRead = true;
                    UpdateReadStatus(this, -1);
                    break;
                }
            }
        }

		protected override void MarkItemUnread(INewsItem item)
        {
            if (item == null) return;
            foreach (var ri in itemsFeed.Items)
            {
                if (item.Equals(ri) && ri.FlagStatus == flagsFiltered)
                {
                    ri.BeenRead = false;
                    break;
                }
            }
        }

		protected override bool ContainsNewMessages
        {
            get
            {
                foreach (var ri in itemsFeed.Items)
                {
                    if (!ri.BeenRead && ri.FlagStatus == flagsFiltered) return true;
                }
                return false;
            }
        }

		protected override int NewMessagesCount
        {
            get
            {
                int i = 0;
                foreach (var ri in itemsFeed.Items)
                {
                    if (!ri.BeenRead && ri.FlagStatus == flagsFiltered) i++;
                }
                return i;
            }
        }

		protected override IList<INewsItem> Items
        {
            get
            {
                var a = new List<INewsItem>(itemsFeed.Items.Count);
                foreach (var ri in itemsFeed.Items)
                {
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
    internal class FlaggedItemsRootNode : TreeFeedsNodeBase
    {
        private static ContextMenu _popup; // share one context menu

        public FlaggedItemsRootNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu) :
            base(text, FeedNodeType.Root, false, imageIndex, selectedImageIndex)
        {
            _popup = menu;
        }

        #region Implementation of FeedTreeNodeBase

        public override bool AllowedChild(FeedNodeType nsType)
        {
            return (nsType == FeedNodeType.SmartFolder);
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
                Control.ContextMenu = _popup;
        }

        #endregion
    }

    #endregion

    #region UnreadItemsNode

    /// <summary>
    /// Stores the unread items.
    /// </summary>
    internal class UnreadItemsNode : SmartFolderNodeBase
    {
    	private readonly RssBanditApplication app;
		private readonly Dictionary<int, UnreadItemsNodePerSource> childrenBySourceID = new Dictionary<int, UnreadItemsNodePerSource>(3);
    	
		#region ctor's

    	public UnreadItemsNode(LocalFeedsFeed itemStore, int imageIndex, int selectedImageIndex, ContextMenu menu) :
    		base(itemStore, imageIndex, selectedImageIndex, menu)
    	{
			// TODO: we should extend the interface ICoreApplication
    		app = (RssBanditApplication)IoC.Resolve<ICoreApplication>();

			app.FeedSourceAdded += app_FeedSourceAdded;
			app.FeedSourceChanged += app_FeedSourceChanged;
			app.FeedSourceDeleted += app_FeedSourceDeleted;
			
			if (app.FeedSources.Count > 1)
    			foreach (FeedSourceEntry e in app.FeedSources.Sources)
    			{
    				UnreadItemsNodePerSource child = new UnreadItemsNodePerSource(
    					e, itemStore, imageIndex, selectedImageIndex, menu);
    				this.Nodes.Add(child);
					childrenBySourceID.Add(e.ID, child);
    			}
    	}
		
    	protected UnreadItemsNode(LocalFeedsFeed itemStore, string text, int imageIndex, int selectedImageIndex, ContextMenu menu) :
    		base(itemStore, text, imageIndex, selectedImageIndex, menu)
    	{
    	}

    	#endregion

    	#region base class overrides

    	protected override void MarkItemRead(INewsItem item)
    	{
    		if (item == null) return;
    		int idx = itemsFeed.Items.IndexOf(item);
    		if (idx >= 0)
    		{
    			itemsFeed.Items.RemoveAt(idx);
					
    			if (HasNodes)
    			{
    				// child handle this:
    				FeedSourceEntry entry = app.FeedSources.SourceOf(item.Feed);
    				UnreadItemsNodePerSource child;
					if (childrenBySourceID.TryGetValue(entry.ID, out child))
						child.UpdateReadStatus(child, -1);
    			}
    			else
    			{
    				// handle by myself:
    				UpdateReadStatus(this, -1);
    			}
    		}
    	}

    	protected override void MarkItemUnread(INewsItem item)
    	{
    		if (item == null) return;
    		int idx = itemsFeed.Items.IndexOf(item);
    		if (idx < 0)
    		{
    			itemsFeed.Items.Add(item);
    		}
    		else
    		{
    			// should not happen, just if we get called this way:
    			(itemsFeed.Items[idx]).BeenRead = false;
    		}
			
    		if (HasNodes)
    		{
    			// child handle this:
    			FeedSourceEntry entry = app.FeedSources.SourceOf(item.Feed);
				UnreadItemsNodePerSource child;
				if (childrenBySourceID.TryGetValue(entry.ID, out child))
					child.UpdateReadStatus(child, 1);
    		}
    		else
    		{
    			// handle by myself:
    			UpdateReadStatus(this, 1);
    		}

    	}

    	public override void UpdateReadStatus()
    	{
    		if (HasNodes)
    		{
    			foreach (UnreadItemsNodePerSource child in Nodes)
    			{
    				child.UpdateReadStatus();
    			}
    		} 
    		else
    		{
    			base.UpdateReadStatus();
    		}
    	}

    	#endregion

    	#region app events (feed source related)

    	void app_FeedSourceDeleted(object sender, FeedSourceEventArgs e)
    	{
    		UnreadItemsNodePerSource child;
    		if (childrenBySourceID.TryGetValue(e.Entry.ID, out child))
    		{
    			child.UpdateReadStatus(child, 0);
    			Nodes.Remove(child);
    			childrenBySourceID.Remove(e.Entry.ID);
    		}
    	}

    	void app_FeedSourceChanged(object sender, FeedSourceEventArgs e)
    	{
    		UnreadItemsNodePerSource child;
    		if (childrenBySourceID.TryGetValue(e.Entry.ID, out child))
    		{
    			child.Text = e.Entry.Name;
    		}
    	}

    	void app_FeedSourceAdded(object sender, FeedSourceEventArgs e)
    	{
    		UnreadItemsNodePerSource child = new UnreadItemsNodePerSource(
    			e.Entry, itemsFeed, ImageIndex, SelectedImageIndex, p_popup);
    		this.Nodes.Add(child);
    		childrenBySourceID.Add(e.Entry.ID, child);
    	}

    	#endregion


		/// <summary>
		/// Called by child nodes to filter the items by source entry ID.
		/// </summary>
		/// <param name="entryID">The entry ID.</param>
		/// <returns></returns>
		protected internal IList<INewsItem> FilteredItems(int entryID)
		{
			return itemsFeed.Items.FindAll(
				item =>
				{
                    return (app.FeedSources.SourceOf(item.Feed) != null) && (app.FeedSources.SourceOf(item.Feed).ID == entryID);
				});
		}
    }
	
	internal class UnreadItemsNodePerSource : UnreadItemsNode
	{
		private readonly int entryID;
		public UnreadItemsNodePerSource(FeedSourceEntry entry, LocalFeedsFeed itemStore, int imageIndex, int selectedImageIndex, ContextMenu menu):
			base(itemStore, entry.Name, imageIndex, selectedImageIndex, menu)
		{
			Text = entry.Name;
			entryID = entry.ID;
			DataKey = itemStore.link + "#" + entryID;
		}

		internal int SourceID { get { return entryID; } }
			
		protected override void MarkItemRead(INewsItem item)
		{
			if (item == null) return;
			int idx = itemsFeed.Items.IndexOf(item);
			if (idx >= 0)
			{
				itemsFeed.Items.RemoveAt(idx);
				UpdateReadStatus(this, -1);
			}
		}

		protected override void MarkItemUnread(INewsItem item)
		{
			if (item == null) return;
			int idx = itemsFeed.Items.IndexOf(item);
			if (idx < 0)
			{
				itemsFeed.Items.Add(item);
				UpdateReadStatus(this, 1);
			}
			else
			{
				// should not happen, just if we get called this way:
				(itemsFeed.Items[idx]).BeenRead = false;
				UpdateReadStatus(this, 1);
			}
		}

		protected override IList<INewsItem> Items
		{
			get
			{
				UnreadItemsNode parent = (UnreadItemsNode)this.Parent;
				return parent.FilteredItems(entryID);
			}
		}

		protected override int NewMessagesCount
		{
			get
			{
				int count = 0;
				IList<INewsItem> items = Items;
				for (int i = 0; i < items.Count; i++)
				{
					INewsItem ri = items[i];
					if (!ri.BeenRead) count++;
				}
				return count;
			}
		}
	}
    #endregion

    #region FinderRootNode

    /// <summary>
    /// Root node to hold persistent FinderNodes.
    /// </summary>
	internal class FinderRootNode : TreeFeedsNodeBase
    {
        private readonly ContextMenu _popup; // context menu

        public FinderRootNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu) :
            base(text, FeedNodeType.Root, true, imageIndex, selectedImageIndex)
        {
            _popup = menu;
            base.Editable = false;
        }

        public void InitFromFinders(ArrayList finderList, ContextMenu menu)
        {
            var categories = new Hashtable();

            foreach (RssFinder finder in finderList)
            {
                TreeFeedsNodeBase parent = this;

                finder.Container = null; // may contain old node references on a re-populate call

                if (finder.FullPath.IndexOf(FeedSource.CategorySeparator) > 0)
                {
                    // one with category

                    string[] a = finder.FullPath.Split(FeedSource.CategorySeparator.ToCharArray());
                    int aLen = a.GetLength(0);
                    string sCat = String.Join(FeedSource.CategorySeparator, a, 0, aLen - 1);

                    if (categories.ContainsKey(sCat))
                    {
                        parent = (TreeFeedsNodeBase) categories[sCat];
                    }
                    else
                    {
                        // create category/categories

                        var sb = new StringBuilder();
                        sb.Append(a[0]);
                        for (int i = 0; i <= aLen - 2; i++)
                        {
                            sCat = sb.ToString();
                            if (categories.ContainsKey(sCat))
                            {
                                parent = (TreeFeedsNodeBase) categories[sCat];
                            }
                            else
                            {
                                TreeFeedsNodeBase cn = new FinderCategoryNode(a[i], 2, 3, menu); // menu???
                                categories.Add(sCat, cn);
                                parent.Nodes.Add(cn);
                                //								cn.Cells[0].Value = cn.Text;
                                //								cn.Cells[0].Appearance.Image = cn.Override.NodeAppearance.Image;
                                //								cn.Cells[0].Appearance.Cursor = WinGuiMain.CursorHand;
                                parent = cn;
                            }
                            sb.Append(FeedSource.CategorySeparator + a[i + 1]);
                        }
                    }
                }

                var n = new FinderNode(finder.Text, 10, 10, menu)
                            {
                                Finder = finder
                            };
                finder.Container = n;
                parent.Nodes.Add(n);
                //				n.Cells[0].Value = n.Text;
                //				n.Cells[0].Appearance.Image = n.Override.NodeAppearance.Image;
                //				n.Cells[0].Appearance.Cursor = WinGuiMain.CursorHand;
            } //foreach finder
        }

        #region Implementation of FeedTreeNodeBase

        public override bool AllowedChild(FeedNodeType nsType)
        {
            // some childs allowed
            if (nsType == FeedNodeType.Finder)
                return true;

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
                Control.ContextMenu = _popup;
        }

        #endregion
    }

    #endregion

    #region FinderCategoryNode

    internal class FinderCategoryNode : TreeFeedsNodeBase
    {
        private static ContextMenu _popup; // share one context menu

        public FinderCategoryNode(string text) :
            base(text, FeedNodeType.FinderCategory, true, 2, 3)
        {
        }

        public FinderCategoryNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu) :
            base(text, FeedNodeType.FinderCategory, true, imageIndex, selectedImageIndex)
        {
            _popup = menu;
        }

        #region Implementation of FeedTreeNodeBase

        public override bool AllowedChild(FeedNodeType nsType)
        {
            return (nsType == FeedNodeType.FinderCategory || nsType == FeedNodeType.Finder);
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
                Control.ContextMenu = _popup;
        }

        #endregion
    }

    #endregion

    #region FinderNode

    /// <summary>
    /// Represents a search folder in the tree.
    /// </summary>
    public class FinderNode : TreeFeedsNodeBase, ISmartFolder
    {
        private readonly ContextMenu _popup; // context menu
        private readonly LocalFeedsFeed itemsFeed;
        //use a dictionary because we want fast access to objects
        private readonly List<INewsItem> items = new List<INewsItem>();

        public FinderNode()
        {
        }

        public FinderNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu) :
            base(text, FeedNodeType.Finder, true, imageIndex, selectedImageIndex)
        {
            itemsFeed = new LocalFeedsFeed(null,
                "http://localhost/rssbandit/searchfolder?id=" + Guid.NewGuid(),
                text, String.Empty, false);
            _popup = menu;
        }

        public virtual bool IsTempFinderNode
        {
            get { return false; }
        }

        public virtual RssFinder Finder { get; set; }

        public string InternalFeedLink
        {
            get { return itemsFeed.link; }
        }

        public bool Contains(NewsItem item)
        {
            return items.Contains(item);
        }

        public void Clear()
        {
            items.Clear();
        }

        #region Implementation of FeedTreeNodeBase

        public override bool AllowedChild(FeedNodeType nsType)
        {
            // no childs allowed
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
                Control.ContextMenu = _popup;
        }

        #endregion

        #region ISmartFolder Members

        public bool ContainsNewMessages
        {
            get
            {
                foreach (var ri in items)
                {
                    if (!ri.BeenRead) return true;
                }
                return false;
            }
        }

        public int NewMessagesCount
        {
            get
            {
                int i = 0;
                foreach (var ri in items)
                {
                    if (!ri.BeenRead) i++;
                }
                return i;
            }
        }

        public virtual bool HasNewComments
        {
            get
            {
                foreach (var ri in items)
                {
                    if (ri.HasNewComments) return true;
                }
                return false;
            }
        }


        public virtual int NewCommentsCount
        {
            get
            {
                int count = 0;
                foreach (var ri in items)
                {
                    if (ri.HasNewComments) count++;
                }
                return count;
            }
        }

        public void MarkItemRead(INewsItem item)
        {
            if (item == null) return;
            int index = items.IndexOf(item);
            if (index != -1)
            {
                INewsItem ri = items[index];
                if (!ri.BeenRead)
                {
                    ri.BeenRead = true;
                    UpdateReadStatus(this, -1);
                }
            }
        }

        public void MarkItemUnread(INewsItem item)
        {
            if (item == null) return;
            int index = items.IndexOf(item);
            if (index != -1)
            {
                INewsItem ri = items[index];
                if (ri.BeenRead)
                {
                    ri.BeenRead = false;
                    UpdateReadStatus(this, 1);
                }
            }
        }

        public IList<INewsItem> Items
        {
            get { return items; }
        }

        public void Add(INewsItem item)
        {
            if (item == null) return;
            if (!items.Contains(item))
                items.Add(item);
        }

        public void AddRange(IList<INewsItem> newItems)
        {
            if (newItems == null) return;
            for (int i = 0; i < newItems.Count; i++)
            {
                var item = newItems[i] as NewsItem;
                if (item != null && !items.Contains(item))
                    items.Add(item);
            }
        }

        public void Remove(INewsItem item)
        {
            if (item == null) return;
            if (items.Contains(item))
                items.Remove(item);
        }

        public void UpdateReadStatus()
        {
            UpdateReadStatus(this, NewMessagesCount);
        }

        public virtual void UpdateCommentStatus()
        {
            UpdateCommentStatus(this, NewCommentsCount);
        }

        public bool Modified
        {
            get { return itemsFeed.Modified; }
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
        public TempFinderNode()
        {
        }

        public TempFinderNode(string text, int imageIndex, int selectedImageIndex, ContextMenu menu) :
            base(text, imageIndex, selectedImageIndex, menu)
        {
        }

        public override bool IsTempFinderNode
        {
            get { return true; }
        }

        public override RssFinder Finder
        {
            get { return base.Finder; }
            set
            {
                base.Finder = value;
                if (base.Finder != null)
                    base.Finder.ShowFullItemContent = false;
            }
        }
    }

    #endregion
}
