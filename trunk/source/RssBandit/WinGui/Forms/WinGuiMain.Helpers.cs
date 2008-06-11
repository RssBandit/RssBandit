using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.ThListView;
using System.Windows.Forms.ThListView.Sorting;
using AppInteropServices;
using IEControl;
using Infragistics.Win.UltraWinTree;
using Microsoft.ApplicationBlocks.ExceptionManagement;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Net;
using NewsComponents.News;
using NewsComponents.Utils;
using RssBandit.Resources;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Menus;
using RssBandit.WinGui.Utility;
using Syndication.Extensibility;
using SortOrder=System.Windows.Forms.SortOrder;

namespace RssBandit.WinGui.Forms
{
    internal partial class WinGuiMain
    {
        private static string CurrentToolbarsVersion
        {
            get
            {
                return
                    String.Format("{0}.{1}", StateSerializationHelper.InfragisticsToolbarVersion,
                                  _currentToolbarsVersion);
            }
        }

#if USE_UltraDockManager
		private string CurrentDockingVersion {
			get { 
				return String.Format("{0}.{1}",  StateSerializationHelper.InfragisticsDockingVersion, _currentDockingVersion);
			}
		}
#endif

        private static string CurrentExplorerBarVersion
        {
            get
            {
                return
                    String.Format("{0}.{1}", StateSerializationHelper.InfragisticsExplorerBarVersion,
                                  _currentExplorerBarVersion);
            }
        }

        private ComboBox _urlComboBox;

        internal ComboBox UrlComboBox
        {
            get
            {
                if (_urlComboBox == null)
                {
                    Debug.Assert(false, "UrlComboBox control not yet initialized (by ToolbarHelper)");
                }
                return _urlComboBox;
            }
            set { _urlComboBox = value; }
        }

        private ComboBox _searchComboBox;

        internal ComboBox SearchComboBox
        {
            get
            {
                if (_searchComboBox == null)
                {
                    Debug.Assert(false, "SearchComboBox control not yet initialized (by ToolbarHelper)");
                }
                return _searchComboBox;
            }
            set { _searchComboBox = value; }
        }

        internal void SetTitleText(string newTitle)
        {
            if (newTitle != null && newTitle.Trim().Length != 0)
                Text = RssBanditApplication.CaptionOnly + " - " + newTitle;
            else
                Text = RssBanditApplication.CaptionOnly;

            if (0 != (owner.InternetConnectionState & INetState.Offline))
                Text += " " + SR.MenuAppInternetConnectionModeOffline;
        }

        internal void SetDetailHeaderText(TreeFeedsNodeBase node)
        {
            if (node != null && !string.IsNullOrEmpty(node.Text))
            {
                if (node.UnreadCount > 0)
                    detailHeaderCaption.Text = String.Format("{0} ({1})", node.Text, node.UnreadCount);
                else
                    detailHeaderCaption.Text = node.Text;
                detailHeaderCaption.Appearance.Image = node.ImageResolved;
            }
            else
            {
                detailHeaderCaption.Text = SR.DetailHeaderCaptionWelcome;
                detailHeaderCaption.Appearance.Image = null;
            }
        }

        protected void AddUrlToHistory(string newUrl)
        {
            if (!newUrl.Equals("about:blank"))
            {
                UrlComboBox.Items.Remove(newUrl);
                UrlComboBox.Items.Insert(0, newUrl);
            }
        }

		internal TreeFeedsNodeBase GetRoot(RootFolderType rootFolderType)
        {
			foreach (TreeFeedsNodeBase n in treeFeeds.Nodes)
			{
				if (n.Visible)
				{
					if (rootFolderType == RootFolderType.MyFeeds &&
						n is SubscriptionRootNode)
						return n;
					if (rootFolderType == RootFolderType.Finder &&
						n is FinderRootNode)
						return n;
					if (rootFolderType == RootFolderType.SmartFolders &&
						n is SpecialRootNode)
						return n;
				}
			}
			//if (treeFeeds.Nodes.Count > 0)
			//{
			//    return _roots[(int) rootFolder];
			//}
            return null;
        }

		internal SubscriptionRootNode GetSubscriptionRootNode(string rootFolderName)
		{
			foreach (TreeFeedsNodeBase n in treeFeeds.Nodes)
			{
				if (n is SubscriptionRootNode)
				{
					if (String.Equals(n.Text, rootFolderName, StringComparison.Ordinal))
						return (SubscriptionRootNode)n;	
				}
			}
			
			return null;
		}

		internal SubscriptionRootNode GetSubscriptionRootNode(FeedSourceEntry entry)
		{
			if (entry == null)
				return null;

			foreach (TreeFeedsNodeBase n in treeFeeds.Nodes)
			{
				SubscriptionRootNode root = n as SubscriptionRootNode;
				if (root != null && root.SourceID == entry.ID)
				{
					return root;
				}
			}

			return null;
		}

        public FeedSourceEntry FeedSourceOf(string feedUrl)
        {
            if (StringHelper.EmptyTrimOrNull(feedUrl))
                return null;

            return owner.FeedSources.Sources.FirstOrDefault(fse => fse.Source.IsSubscribed(feedUrl)); 
        }


		public FeedSourceEntry FeedSourceOf(TreeFeedsNodeBase node)
		{
			if (node == null)
				return null;

			SubscriptionRootNode root = TreeHelper.ParentRootNode(node) as SubscriptionRootNode;
			if (root != null)
				return owner.FeedSources[root.SourceID];
			return null;
		}

		internal List<SubscriptionRootNode> GetVisibleSubscriptionRootNodes()
		{
			List<SubscriptionRootNode> list = new List<SubscriptionRootNode>();
			foreach (TreeFeedsNodeBase n in treeFeeds.Nodes)
			{
				if (n is SubscriptionRootNode && n.Visible)
					list.Add((SubscriptionRootNode) n);
			}

			return list;
		}

        internal List<SubscriptionRootNode> GetAllSubscriptionRootNodes()
        {
            List<SubscriptionRootNode> list = new List<SubscriptionRootNode>();
            foreach (TreeFeedsNodeBase n in treeFeeds.Nodes)
            {
                if (n is SubscriptionRootNode)
                    list.Add((SubscriptionRootNode)n);
            }

            return list;
        }

		internal void ShowSubscriptionRootNodes(bool value)
		{
			foreach (TreeFeedsNodeBase n in treeFeeds.Nodes)
			{
				if (n is SubscriptionRootNode)
				{
					n.Visible = value;
				}
			}
		}

        internal RootFolderType GetRootType(TreeFeedsNodeBase feedsNode)
        {
            if (feedsNode == null)
                throw new ArgumentNullException("feedsNode");

			if (feedsNode.Type == FeedNodeType.Root ||
				feedsNode.Parent == null)
			{
				if (feedsNode is SubscriptionRootNode)
					return RootFolderType.MyFeeds;
				if (feedsNode is FinderRootNode)
					return RootFolderType.Finder;
				if (feedsNode is SpecialRootNode)
					return RootFolderType.SmartFolders;

				Debug.Assert(false, "Unknown root folder type: " + feedsNode.GetType().FullName);
				//for (int i = 0; i < _roots.Count; i++)
				//{
				//    if (feedsNode == _roots[i])
				//        return (RootFolderType) i;
				//}
			}
            //else 
			if (feedsNode.Parent != null)
            {
                return GetRootType(feedsNode.Parent);
            }
            return RootFolderType.MyFeeds;
        }

        protected TreeFeedsNodeBase CurrentDragNode { get; set; }

        protected TreeFeedsNodeBase CurrentDragHighlightNode
        {
            get { return _currentDragHighlightFeedsNode; }
            set
            {
                if (_currentDragHighlightFeedsNode != null && _currentDragHighlightFeedsNode != value)
                {
                    // unhighlight old one
                    _currentDragHighlightFeedsNode.Override.NodeAppearance.ResetBackColor();
                    _currentDragHighlightFeedsNode.Override.NodeAppearance.ResetForeColor();
                    //if (_timerTreeNodeExpand.Enabled) 
                    //	_timerTreeNodeExpand.Stop();
                }
                _currentDragHighlightFeedsNode = value;
                if (_currentDragHighlightFeedsNode != null)
                {
                    // highlight new one
                    _currentDragHighlightFeedsNode.Override.NodeAppearance.BackColor = SystemColors.Highlight;
                    _currentDragHighlightFeedsNode.Override.NodeAppearance.ForeColor = SystemColors.HighlightText;
                    //if (_currentDragHighlightFeedsNode.Nodes.Count > 0 && !_currentDragHighlightFeedsNode.Expanded)
                    //	_timerTreeNodeExpand.Start();
                }
            }
        }

        private static void SetFocus2WebBrowser(HtmlControl theBrowser)
        {
            if (theBrowser == null)
                return;
            theBrowser.Focus();
        }

        internal void SetSubscriptionNodeState(INewsFeed f, TreeFeedsNodeBase feedsNode, FeedProcessingState state)
        {
            if (f == null || feedsNode == null) return;
            if (RssHelper.IsNntpUrl(f.link))
            {
                SetNntpNodeState(f, feedsNode, state);
            }
            else
            {
                SetFeedNodeState(f, feedsNode, state);
            }
        }

        private static void SetNntpNodeState(INewsFeed f, TreeFeedsNodeBase feedsNode, FeedProcessingState state)
        {
            if (f == null || feedsNode == null) return;
            switch (state)
            {
                case FeedProcessingState.Normal:
                    if (f.refreshrateSpecified && f.refreshrate <= 0)
                    {
                        feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.NntpDisabled;
                        feedsNode.Override.SelectedNodeAppearance.Image =
                            Resource.SubscriptionTreeImage.NntpDisabledSelected;
                    }
                    else if (f.authUser != null || f.link.StartsWith(NntpWebRequest.NntpsUriScheme))
                    {
                        feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.NntpSecured;
                        feedsNode.Override.SelectedNodeAppearance.Image =
                            Resource.SubscriptionTreeImage.NntpSecuredSelected;
                    }
                    else
                    {
                        feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.Nntp;
                        feedsNode.Override.SelectedNodeAppearance.Image = Resource.SubscriptionTreeImage.NntpSelected;
                    }
                    break;
                case FeedProcessingState.Failure:
                    feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.NntpFailure;
                    feedsNode.Override.SelectedNodeAppearance.Image = Resource.SubscriptionTreeImage.NntpFailureSelected;
                    break;
                case FeedProcessingState.Updating:
                    feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.NntpUpdating;
                    feedsNode.Override.SelectedNodeAppearance.Image =
                        Resource.SubscriptionTreeImage.NntpUpdatingSelected;
                    break;
                default:
                    Trace.WriteLine("Unhandled/unknown FeedProcessingState: " + state);
                    break;
            }
        }

        private void SetFeedNodeState(INewsFeed f, TreeFeedsNodeBase feedsNode, FeedProcessingState state)
        {
            if (f == null || feedsNode == null) return;
            switch (state)
            {
                case FeedProcessingState.Normal:
                    if (!string.IsNullOrEmpty(f.favicon) && feedsNode.HasCustomIcon && owner.Preferences.UseFavicons)
                    {
                        feedsNode.SetIndividualImage(null); //revert to original images
                    }
                    else if (f.refreshrateSpecified && f.refreshrate <= 0)
                    {
                        feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.FeedDisabled;
                        feedsNode.Override.SelectedNodeAppearance.Image =
                            Resource.SubscriptionTreeImage.FeedDisabledSelected;
                    }
                    else if (f.authUser != null || f.link.StartsWith("https"))
                    {
                        feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.FeedSecured;
                        feedsNode.Override.SelectedNodeAppearance.Image =
                            Resource.SubscriptionTreeImage.FeedSecuredSelected;
                    }
                    else
                    {
                        feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.Feed;
                        feedsNode.Override.SelectedNodeAppearance.Image = Resource.SubscriptionTreeImage.FeedSelected;
                    }
                    break;
                case FeedProcessingState.Failure:
                    feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.FeedFailure;
                    feedsNode.Override.SelectedNodeAppearance.Image = Resource.SubscriptionTreeImage.FeedFailureSelected;
                    break;
                case FeedProcessingState.Updating:
                    feedsNode.Override.NodeAppearance.Image = Resource.SubscriptionTreeImage.FeedUpdating;
                    feedsNode.Override.SelectedNodeAppearance.Image =
                        Resource.SubscriptionTreeImage.FeedUpdatingSelected;
                    break;
                default:
                    Trace.WriteLine("Unhandled/unknown FeedProcessingState: " + state);
                    break;
            }
        }

        /// <summary>
        /// Populates the list view with the items for the feed represented by 
        /// the tree node then checks to see if any are unread. If this is the 
        /// case then the unread item is given focus.  
        /// </summary>
        /// <param name="tn"></param>
        /// <returns>True if an unread item exists for this feed and false otherwise</returns>
        private bool FindNextUnreadItem(TreeFeedsNodeBase tn)
        {
            INewsFeed f = null;
            bool repopulated = false, isTopLevel = true;
            ListViewItem foundLVItem = null;

            //long measure = 0;	// used only for profiling...

            if (tn.Type == FeedNodeType.Feed)
                f = owner.GetFeed(FeedSourceOf(tn), tn.DataKey);

            bool containsUnread = ((f != null && f.containsNewMessages) ||
                                   (tn == TreeSelectedFeedsNode && tn.UnreadCount > 0));

            if (containsUnread)
            {
                if (tn != TreeSelectedFeedsNode && f != null)
                {
                    containsUnread = false;
                    FeedSource source = FeedSourceOf(tn).Source;
                    IList<INewsItem> items = source.GetCachedItemsForFeed(f.link);

                    for (int i = 0; i < items.Count; i++)
                    {
                        INewsItem item = items[i];
                        if (!item.BeenRead)
                        {
                            containsUnread = true;
                            break;
                        }
                    }

                    if (containsUnread)
                    {
                        TreeFeedsNodeBase tnSelected = TreeSelectedFeedsNode ?? GetRoot(RootFolderType.MyFeeds);

                        if (tnSelected.Type == FeedNodeType.SmartFolder || tnSelected.Type == FeedNodeType.Finder ||
                            tnSelected.Type == FeedNodeType.Root ||
                            (tnSelected != tn && tnSelected.Type == FeedNodeType.Feed) ||
                            (tnSelected.Type == FeedNodeType.Category && !NodeIsChildOf(tn, tnSelected)))
                        {
                            //ProfilerHelper.StartMeasure(ref measure);

                            //re-populate list view with items for feed with unread messages, if it is not
                            // the current displayed:
                            //							this.treeFeeds.AfterSelect -= new System.Windows.Forms.TreeViewEventHandler(this.OnTreeFeedAfterSelect);		
                            //							this.treeFeeds.BeforeSelect -= new System.Windows.Forms.TreeViewCancelEventHandler(this.OnTreeFeedBeforeSelect);									
                            TreeSelectedFeedsNode = tn;
                            CurrentSelectedFeedsNode = null; // reset
                            //							this.treeFeeds.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnTreeFeedAfterSelect);
                            //							this.treeFeeds.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.OnTreeFeedBeforeSelect);																
                            PopulateListView(tn, items, true);
                            repopulated = true;

                            //_log.Info("Profile FindNextUnreadItem() Re-Populate listview took: "+ProfilerHelper.StopMeasureString(measure));
                        }
                    }
                    else
                    {
                        f.containsNewMessages = false; // correct the property value
                    }
                }
            } //if(f.containsNewMessages)

            for (int i = 0; i < listFeedItems.SelectedItems.Count; i++)
            {
                var tlvi = (ThreadedListViewItem) listFeedItems.SelectedItems[i];
                if ((tlvi != null) && (tlvi.IndentLevel != 0))
                {
                    isTopLevel = false;
                    break;
                }
            }

            //select a list item that hasn't been read. As an optimization, we don't 
            //walk the list view if we are on a top level listview item and there are no
            //unread posts. 
            if ((!isTopLevel) || containsUnread)
            {
                foundLVItem = FindUnreadListViewItem();
            }

            if (foundLVItem != null)
            {
                MoveFeedDetailsToFront();

                listFeedItems.BeginUpdate();
                listFeedItems.SelectedItems.Clear();
                foundLVItem.Selected = true;
                foundLVItem.Focused = true;
                htmlDetail.Activate(); // set focus to html after doc is loaded
                OnFeedListItemActivate(null, EventArgs.Empty); //pass nulls because I don't use params
                SetTitleText(tn.Text);
                SetDetailHeaderText(tn);
                foundLVItem.Selected = true;
                foundLVItem.Focused = true;
                listFeedItems.Focus();
                listFeedItems.EnsureVisible(foundLVItem.Index);
                listFeedItems.EndUpdate();

                //select new position in tree view based on feed with unread messages.
                if (TreeSelectedFeedsNode != tn && repopulated)
                {
                    //we unregister event here to avoid OnTreeFeedAfterSelect() being invoked
                    //					this.treeFeeds.AfterSelect -= new System.Windows.Forms.TreeViewEventHandler(this.OnTreeFeedAfterSelect);		
                    //					this.treeFeeds.BeforeSelect -= new System.Windows.Forms.TreeViewCancelEventHandler(this.OnTreeFeedBeforeSelect);																							
                    //					treeFeeds.BeginUpdate();
                    SelectNode(tn);
                    //					treeFeeds.EndUpdate();
                    //					this.treeFeeds.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnTreeFeedAfterSelect);
                    //					this.treeFeeds.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.OnTreeFeedBeforeSelect);																
                }
                return true;
            }

            return false;
        }

        private ThreadedListViewItem FindUnreadListViewItem()
        {
            bool inComments = false;

            for (int i = 0; i < listFeedItems.SelectedItems.Count; i++)
            {
                var tlvi = (ThreadedListViewItem) listFeedItems.SelectedItems[i];
                if ((tlvi != null) && (tlvi.IsComment))
                {
                    inComments = true;
                    break;
                }
            }


            if (listFeedItems.Items.Count == 0)
                return null;

            NewsItem compareItem = null;
            ThreadedListViewItem foundLVItem = null;

            int pos = 0, incrementor = 1;

            if ((!inComments) && (listFeedItems.SortManager.SortOrder == SortOrder.Descending))
            {
                pos = listFeedItems.Items.Count - 1; // at the end
                incrementor = -1; // decrement
            }

            while (pos >= 0 && pos < listFeedItems.Items.Count)
            {
                // in correct range

                ThreadedListViewItem lvi = listFeedItems.Items[pos];
                var item = lvi.Key as NewsItem;

                // find the oldest unread item
                if (item != null && !item.BeenRead)
                {
                    // item can be null for temp entries like "Load comments,..."
                    if (compareItem == null) compareItem = item;
                    if (foundLVItem == null) foundLVItem = lvi;

                    if (!(listFeedItems.SortManager.GetComparer() is ThreadedListViewDateTimeItemComparer))
                    {
                        if (DateTime.Compare(item.Date, compareItem.Date) < 0)
                        {
                            // worst case: compare all unread
                            // instance item.Date smaller than compareItem.Date. Found one:
                            compareItem = item; // item to compare to 
                            foundLVItem = lvi; // corresponding ListViewItem
                        }
                    }
                    else
                    {
                        // simply the next
                        foundLVItem = lvi; // corresponding ListViewItem
                        break;
                    }
                }

                pos += incrementor; // decrement or increment
            }

            return foundLVItem;
        }

        private void SelectNode(TreeFeedsNodeBase feedsNode)
        {
            TreeSelectedFeedsNode = feedsNode;
            feedsNode.BringIntoView();
            if (feedsNode.Parent != null) feedsNode.Parent.BringIntoView();
        }

        /// <summary>
        /// From the startNode, this function returns the next
        /// FeedNode with UnreadCount > 0 that is hirarchically below the startNode.
        /// </summary>
        /// <param name="startNode">the Node to start with</param>
        /// <returns>FeedTreeNodeBase found or null</returns>
        /// <param name="ignoreStartNode"></param>
        private static TreeFeedsNodeBase NextNearFeedNode(TreeFeedsNodeBase startNode, bool ignoreStartNode)
        {
            TreeFeedsNodeBase found = null;

            if (!ignoreStartNode)
            {
                if (startNode.Type == FeedNodeType.Feed) return startNode;
            }

            // walk childs, go down
            for (TreeFeedsNodeBase sibling = startNode.FirstNode;
                 sibling != null && found == null;
                 sibling = sibling.NextNode)
            {
                if (sibling.Type == FeedNodeType.Feed) return sibling;
                if (sibling.FirstNode != null) // childs?
                    found = NextNearFeedNode(sibling.FirstNode, false);
            }
            if (found != null) return found;

            // walk next siblings. If they have childs, go down
            for (TreeFeedsNodeBase sibling = (ignoreStartNode ? startNode.NextNode : startNode.FirstNode);
                 sibling != null && found == null;
                 sibling = sibling.NextNode)
            {
                if (sibling.Type == FeedNodeType.Feed) return sibling;
                if (sibling.FirstNode != null) // childs?
                    found = NextNearFeedNode(sibling.FirstNode, false);
            }
            if (found != null) return found;
            if (startNode.Parent == null) return null; // top of tree

            // no sibling, no Feed childs.
            // go upwards, as long as the parent itself is lastNode
            for (startNode = startNode.Parent;
                 startNode != null && startNode.NextNode == null;
                 startNode = startNode.Parent)
            {
                // nix to do here
            }
            if (startNode == null) return null;

            // no walk next parent siblings. 
            for (TreeFeedsNodeBase parentSibling = startNode.NextNode;
                 parentSibling != null && found == null;
                 parentSibling = parentSibling.NextNode)
            {
                if (parentSibling.Type == FeedNodeType.Feed) return parentSibling;
                if (parentSibling.FirstNode != null) // childs?
                    found = NextNearFeedNode(parentSibling.FirstNode, false);
            }

            return found;
        }


        /// <summary>
        /// Moves from the currently selected item to the next unread item. 
        /// If no unread item is left then this method does nothing.
        /// </summary>
        public void MoveToNextUnreadItem()
        {
            TreeFeedsNodeBase startNode = null, foundFeedsNode, rootNode = GetRoot(RootFolderType.MyFeeds);
            bool unreadFound = false;

            if (listFeedItems.Items.Count > 0)
            {
                startNode = TreeSelectedFeedsNode;
                if (startNode != null && startNode.UnreadCount > 0)
                {
                    unreadFound = FindNextUnreadItem(startNode);
                    if (!unreadFound)
                    {
                        startNode = null;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            if (startNode == null)
                startNode = CurrentSelectedFeedsNode;

            if (startNode != null && !NodeIsChildOf(startNode, rootNode))
                startNode = null;

            if (startNode == null)
                startNode = rootNode;


            if (startNode.Type == FeedNodeType.Feed)
            {
                MoveFeedDetailsToFront();

                if (FindNextUnreadItem(startNode))
                {
                    unreadFound = true;
                }
            }


            if (!unreadFound)
            {
                // look for next near down feed node
                foundFeedsNode = NextNearFeedNode(startNode, true);
                while (foundFeedsNode != null && !unreadFound)
                {
                    if (FindNextUnreadItem(foundFeedsNode))
                    {
                        unreadFound = true;
                    }
                    foundFeedsNode = NextNearFeedNode(foundFeedsNode, true);
                }
            }

            if (!unreadFound && startNode != GetRoot(RootFolderType.MyFeeds))
            {
                // if not already applied,
                // look for next near down feed node from top of tree
                foundFeedsNode = NextNearFeedNode(GetRoot(RootFolderType.MyFeeds), true);
                while (foundFeedsNode != null && !unreadFound)
                {
                    if (FindNextUnreadItem(foundFeedsNode))
                    {
                        unreadFound = true;
                    }
                    foundFeedsNode = NextNearFeedNode(foundFeedsNode, true);
                }
            }

            if (!unreadFound)
            {
                if (owner.StateHandler.NewsHandlerState == NewsHandlerState.Idle)
                    SetGuiStateFeedback(SR.GUIStatusNoUnreadFeedItemsLeft, ApplicationTrayState.NormalIdle);
            }
        }

        /// <summary>
        /// Help to simply serialize a bounds rect.
        /// </summary>
        /// <param name="b"></param>
        /// <returns>A ';' separated string: "X;Y;Width;Height".</returns>
        private static string BoundsToString(Rectangle b)
        {
            return string.Format("{0};{1};{2};{3}", b.X, b.Y, b.Width, b.Height);
        }

        private static Rectangle StringToBounds(string b)
        {
            string[] ba = b.Split(new[] {';'});
            Rectangle r = Rectangle.Empty;
            if (ba.GetLength(0) == 4)
            {
                try
                {
                    r = new Rectangle(Int32.Parse(ba[0]), Int32.Parse(ba[1]), Int32.Parse(ba[2]), Int32.Parse(ba[3]));
                }
                catch
                {
                }
            }
            return r;
        }

        /// <summary>
        /// Helper method to populate SmartFolders.
        /// </summary>
        /// <param name="feedNode">The tree node which represents the feed in the tree view</param>
        /// <param name="updateGui">Indicates whether the UI should be altered when the download is completed 
        /// or not. Basically if this flag is true then the list view and browser pane are updated while 
        /// they remain unchanged if this flag is false. </param>
        public void PopulateSmartFolder(TreeFeedsNodeBase feedNode, bool updateGui)
        {
            var isFolder = feedNode as ISmartFolder;

            if (isFolder == null)
                return;

            IList<INewsItem> items = isFolder.Items;

            //Ensure we update the UI in the correct thread. Since this method is likely 
            //to have been called from a thread that is not the UI thread we should ensure 
            //that calls to UI components are actually made from the UI thread or marshalled
            //accordingly. 			

            if (updateGui || TreeSelectedFeedsNode == feedNode)
            {
                INewsItem itemSelected = null;
                if (listFeedItems.SelectedItems.Count > 0)
                    itemSelected = (INewsItem) ((ThreadedListViewItem) listFeedItems.SelectedItems[0]).Key;

                // call them sync., because we want to re-set the previous selected item
                InvokeOnGuiSync(() => PopulateListView(feedNode, items, true, false, feedNode));

                if (updateGui)
                {
                    htmlDetail.Clear(); //clear browser pane 
                    if (itemSelected == null || listFeedItems.Items.Count == 0)
                    {
                        CurrentSelectedFeedItem = null;
                    }
                    else
                        ReSelectListViewItem(itemSelected);
                }
            }
        }

        /// <summary>
        /// Helper method to populate Aggregated Folders.
        /// </summary>
        /// <param name="node">The tree node which represents the feed in the tree view</param>
        /// <param name="updateGui">Indicates whether the UI should be altered when the download is completed 
        /// or not. Basically if this flag is true then the list view and browser pane are updated while 
        /// they remain unchanged if this flag is false. </param>
        public void PopulateFinderNode(FinderNode node, bool updateGui)
        {
            if (node == null)
                return;

            //Ensure we update the UI in the correct thread. Since this method is likely 
            //to have been called from a thread that is not the UI thread we should ensure 
            //that calls to UI components are actually made from the UI thread or marshalled
            //accordingly. 
            if (updateGui || TreeSelectedFeedsNode == node)
            {
                INewsItem itemSelected = null;
                if (listFeedItems.SelectedItems.Count > 0)
                    itemSelected = (INewsItem) ((ThreadedListViewItem) listFeedItems.SelectedItems[0]).Key;

                // now the FinderNode handle refresh of the read state only, so we need to initiate a new search again...:
                node.AnyUnread = false;
                node.Clear();

                //check if Web search or local search
                if (!string.IsNullOrEmpty(node.Finder.ExternalSearchUrl))
                {
                    StartRssRemoteSearch(node.Finder.ExternalSearchUrl, node);
                }
                else
                {
                    AsyncStartNewsSearch(node);
                }


                IList<INewsItem> items = node.Items;

                // call them sync., because we want to re-set the previous selected item
                InvokeOnGuiSync(() => PopulateListView(node, items, true, true, node));

                if (updateGui)
                {
                    if (itemSelected != null)
                    {
                        //clear browser pane 
                        CurrentSelectedFeedItem = null;
                        htmlDetail.Clear();
                    }
                    else
                        ReSelectListViewItem(itemSelected);
                }
            }
        }

        private ThreadedListViewItem CreateThreadedLVItem(INewsItem newsItem, bool hasChilds, int imgOffset,
                                                          ColumnKeyIndexMap colIndex, bool authorInTopicColumn)
        {
            var lvItems = new string[colIndex.Count];

            foreach (string colKey in colIndex.Keys)
            {
                lvItems[colIndex[colKey]] = String.Empty; // init
                switch ((NewsItemSortField) Enum.Parse(typeof (NewsItemSortField), colKey, true))
                {
                    case NewsItemSortField.Title:
                        lvItems[colIndex[colKey]] = StringHelper.ShortenByEllipsis(newsItem.Title, MaxHeadlineWidth);
                        break;
                    case NewsItemSortField.Subject:
                        if (authorInTopicColumn && !colIndex.ContainsKey("Author"))
                        {
                            lvItems[colIndex[colKey]] = newsItem.Author;
                        }
                        else
                        {
                            lvItems[colIndex[colKey]] = newsItem.Subject;
                        }
                        break;
                    case NewsItemSortField.FeedTitle:
                        INewsFeed f = newsItem.Feed;
                        //if we are in a Smart Folder then use the original title of the feed 
                        string feedUrl = GetOriginalFeedUrl(newsItem);
                        if ((feedUrl != null) && owner.FeedHandler.IsSubscribed(feedUrl))
                        {
                            f = owner.FeedHandler.GetFeeds()[feedUrl];
                        }

                        lvItems[colIndex[colKey]] = HtmlHelper.HtmlDecode(f.title);
                        break;
                    case NewsItemSortField.Author:
                        lvItems[colIndex[colKey]] = newsItem.Author;
                        break;
                    case NewsItemSortField.Date:
                        lvItems[colIndex[colKey]] = newsItem.Date.ToLocalTime().ToString();
                        break;
                    case NewsItemSortField.CommentCount:
                        if (newsItem.CommentCount != NewsItem.NoComments)
                            lvItems[colIndex[colKey]] = newsItem.CommentCount.ToString();
                        break;
                    case NewsItemSortField.Enclosure: //TODO: use states. Now it is simply a counter
                        if (null != newsItem.Enclosures && newsItem.Enclosures.Count > 0)
                            lvItems[colIndex[colKey]] = newsItem.Enclosures.Count.ToString();
                        // state should be ("None", "Available", "Scheduled", "Downloaded")
                        break;
                    case NewsItemSortField.Flag:
                        if (newsItem.FlagStatus != Flagged.None)
                            lvItems[colIndex[colKey]] = newsItem.FlagStatus.ToString(); //TODO: localize
                        break;
                    default:
                        Trace.Assert(false, "CreateThreadedLVItem::NewsItemSortField NOT handled: " + colKey);
                        break;
                }
            }

            var lvi = new ThreadedListViewItem(newsItem, lvItems);

            if (!newsItem.BeenRead)
                imgOffset++;
            lvi.ImageIndex = imgOffset;

            // apply leading fonts/colors
            ApplyStyles(lvi, newsItem.BeenRead, newsItem.HasNewComments);

            lvi.HasChilds = hasChilds;
            lvi.IsComment = authorInTopicColumn;

            return lvi;
        }

        /// <summary>call it if items are added to the listview only!</summary>
        /// <param name="items"></param>
        private void ApplyNewsItemPropertyImages(IEnumerable<ThreadedListViewItem> items)
        {
            ColumnKeyIndexMap indexMap = listFeedItems.Columns.GetColumnIndexMap();

            bool applyFlags = indexMap.ContainsKey(NewsItemSortField.Flag.ToString());
            bool applyAttachments = indexMap.ContainsKey(NewsItemSortField.Enclosure.ToString());

            if (!applyFlags && !applyAttachments)
                return;

            foreach (var lvi in items)
            {
                var item = lvi.Key as NewsItem;
                if (item == null) continue;
                if (applyFlags && item.FlagStatus != Flagged.None)
                    ApplyFlagStateTo(lvi, item.FlagStatus, indexMap);
                if (applyAttachments && item.Enclosures != null && item.Enclosures.Count > 0)
                    ApplyAttachmentImageTo(lvi, item.Enclosures.Count, indexMap);
            }
        }

        /// <summary>call it if items are added to the listview only!</summary>
        /// <param name="lvi"></param>
        /// <param name="attachemtCount"></param>
        /// <param name="indexMap"></param>
        private static void ApplyAttachmentImageTo(ThreadedListViewItem lvi, int attachemtCount,
                                                   ColumnKeyIndexMap indexMap)
        {
            if (lvi == null || lvi.ListView == null)
                return;

            string key = NewsItemSortField.Enclosure.ToString();
            if (!indexMap.ContainsKey(key))
                return;

            string text = (attachemtCount > 0 ? attachemtCount.ToString() : String.Empty);

            if (indexMap[key] > 0)
            {
                lvi.SubItems[indexMap[key]].Text = text;
                if (attachemtCount > 0)
                    lvi.SetSubItemImage(indexMap[key], Resource.NewsItemRelatedImage.Attachment);
            }
            else
            {
                lvi.SubItems[indexMap[key]].Text = text;
                //lvi.SetSubItemImage(indexMap[key], imgIndex);
            }
        }

        /// <summary>call it if items are added to the listview only!</summary>
        /// <param name="lvi"></param>
        /// <param name="flagStatus"></param>
        /// <param name="indexMap"></param>
        private static void ApplyFlagStateTo(ThreadedListViewItem lvi, Flagged flagStatus, ColumnKeyIndexMap indexMap)
        {
            if (lvi == null || lvi.ListView == null)
                return;

            string key = NewsItemSortField.Flag.ToString();
            if (!indexMap.ContainsKey(key))
                return;

            int imgIndex = -1;
            Color bkColor = lvi.BackColor;
            string text = flagStatus.ToString(); //TODO: localize!!!
            switch (flagStatus)
            {
                case Flagged.Complete:
                    imgIndex = Resource.FlagImage.Complete;
                    break;
                case Flagged.FollowUp:
                    imgIndex = Resource.FlagImage.Red;
                    bkColor = Resource.ItemFlagBackground.Red;
                    break;
                case Flagged.Forward:
                    imgIndex = Resource.FlagImage.Blue;
                    bkColor = Resource.ItemFlagBackground.Blue;
                    break;
                case Flagged.Read:
                    imgIndex = Resource.FlagImage.Green;
                    bkColor = Resource.ItemFlagBackground.Green;
                    break;
                case Flagged.Review:
                    imgIndex = Resource.FlagImage.Yellow;
                    bkColor = Resource.ItemFlagBackground.Yellow;
                    break;
                case Flagged.Reply:
                    imgIndex = Resource.FlagImage.Purple;
                    bkColor = Resource.ItemFlagBackground.Purple;
                    break;
                case Flagged.None:
                    //imgIndex is already setup, as is bkColor
                    text = String.Empty;
                    break;
            }

            if (indexMap[key] > 0)
            {
                lvi.SubItems[indexMap[key]].Text = text;
                lvi.SetSubItemImage(indexMap[key], imgIndex);
                lvi.SubItems[indexMap[key]].BackColor = bkColor; // no effect :-( - BUGBUG???
            }
            else
            {
                lvi.SubItems[indexMap[key]].Text = text;
                //lvi.SetSubItemImage(indexMap[key], imgIndex);
                //lvi.SubItems[indexMap[key]].BackColor = bkColor;	// no effect :-( - BUGBUG???
            }
        }

        private static ThreadedListViewItem CreateThreadedLVItemInfo(string infoMessage, bool isError)
        {
            ThreadedListViewItem lvi = new ThreadedListViewItemPlaceHolder(infoMessage);
            if (isError)
            {
                lvi.Font = FontColorHelper.FailureFont;
                lvi.ForeColor = FontColorHelper.FailureColor;
                lvi.ImageIndex = Resource.NewsItemRelatedImage.Failure;
            }
            else
            {
                lvi.Font = FontColorHelper.NormalFont;
                lvi.ForeColor = FontColorHelper.NormalColor;
            }
            lvi.HasChilds = false;

            return lvi;
        }

        /// <summary>
        /// Populates the list view with NewsItem's from the ArrayList. 
        /// </summary>
        /// <param name="associatedFeedsNode">The accociated tree Node</param>
        /// <param name="list">A list of NewsItem objects.</param>
        /// <param name="forceReload">Force reload of the listview</param>
        private void PopulateListView(TreeFeedsNodeBase associatedFeedsNode, IList<INewsItem> list, bool forceReload)
        {
            PopulateListView(associatedFeedsNode, list, forceReload, false, associatedFeedsNode);
        }

        /// <summary>
        /// Populates the list view with NewsItem's from the ArrayList. 
        /// </summary>
        /// <param name="associatedFeedsNode">The accociated tree Node to populate</param>
        /// <param name="list">A list of NewsItem objects.</param>
        /// <param name="forceReload">Force reload of the listview</param>
        /// <param name="categorizedView">True, if the feed title should be appended to
        /// each RSS Item title: "...rss item title... (feed title)"</param>
        /// <param name="initialFeedsNode"></param>
        private void PopulateListView(TreeFeedsNodeBase associatedFeedsNode, IList<INewsItem> list, bool forceReload,
                                      bool categorizedView, TreeFeedsNodeBase initialFeedsNode)
        {
            try
            {
                lock (listFeedItems.Items)
                {
                    if ((initialFeedsNode != null) && TreeSelectedFeedsNode != initialFeedsNode)
                    {
                        return;
                    }
                }

                IList<INewsItem> unread;

                // detect, if we should do a smartUpdate

                lock (listFeedItems.Items)
                {
                    //since this is a multithreaded app there could have been a change since the last 
                    //time we checked this at the beginning of the method due to context switching. 
                    if (TreeSelectedFeedsNode != initialFeedsNode)
                    {
                        return;
                    }

                    if (initialFeedsNode != null)
                    {
                        if (initialFeedsNode.Type == FeedNodeType.Category)
                        {
                            if (NodeIsChildOf(associatedFeedsNode, initialFeedsNode))
                            {
                                if (forceReload)
                                {
                                    EmptyListView();
                                    feedsCurrentlyPopulated.Clear();
                                }

                                bool checkForDuplicates =
                                    feedsCurrentlyPopulated.ContainsKey(associatedFeedsNode.DataKey);
                                unread = PopulateSmartListView(list, categorizedView, checkForDuplicates);
                                if (!checkForDuplicates)
                                    feedsCurrentlyPopulated.Add(associatedFeedsNode.DataKey, null);

                                if (unread.Count != associatedFeedsNode.UnreadCount)
                                    UpdateTreeNodeUnreadStatus(associatedFeedsNode, unread.Count);
                            }
                            else if (associatedFeedsNode == initialFeedsNode)
                            {
                                feedsCurrentlyPopulated.Clear();
                                PopulateFullListView(list);
                                if (associatedFeedsNode.DataKey != null)
                                    feedsCurrentlyPopulated.Add(associatedFeedsNode.DataKey, null);
                            }
                        }
                        else if (TreeSelectedFeedsNode is UnreadItemsNode)
                        {
                            if (forceReload)
                            {
                                EmptyListView();
                            }

                            PopulateSmartListView(list, categorizedView, true);
                        }
                        else if (TreeSelectedFeedsNode == associatedFeedsNode)
                        {
                            if (forceReload)
                            {
                                unread = PopulateFullListView(list);
                                if (unread.Count != associatedFeedsNode.UnreadCount)
                                    UpdateTreeNodeUnreadStatus(associatedFeedsNode, unread.Count);
                            }
                            else
                            {
                                unread = PopulateSmartListView(list, categorizedView, true);
                                if (unread.Count > 0)
                                {
                                    int unreadItems = unread.Count;
                                    if (categorizedView) // e.g. AggregatedNodes
                                        unreadItems += associatedFeedsNode.UnreadCount;
                                    UpdateTreeNodeUnreadStatus(associatedFeedsNode, unreadItems);
                                }
                            }
                        }
                    }
                } //lock

                SetGuiStateFeedback(String.Format(SR.StatisticsItemsDisplayedMessage, listFeedItems.Items.Count));
            }
            catch (Exception ex)
            {
                _log.Error("PopulateListView() failed.", ex);
            }
        }

        /// <summary>
        /// Can be called from another thread to populate the listview in the Gui thread.
        /// </summary>
        /// <param name="associatedFeedsNode"></param>
        /// <param name="list"></param>
        /// <param name="forceReload"></param>
        /// <param name="categorizedView"></param>
        /// <param name="initialFeedsNode"></param>
        public void AsyncPopulateListView(TreeFeedsNodeBase associatedFeedsNode, IList<INewsItem> list, bool forceReload,
                                          bool categorizedView, TreeFeedsNodeBase initialFeedsNode)
        {
            InvokeOnGui(
                () => PopulateListView(associatedFeedsNode, list, forceReload, categorizedView, initialFeedsNode));
        }

        /// <summary>
        /// Fully populates the list view with NewsItem's from the ArrayList 
        /// (forced reload).
        /// </summary>
        /// <param name="list">A list of NewsItem objects.</param>
        /// <returns>unread items</returns>
        private IList<INewsItem> PopulateFullListView(IList<INewsItem> list)
        {
            var aNew = new ThreadedListViewItem[list.Count];

            var unread = new List<INewsItem>(list.Count);

            ColumnKeyIndexMap colIndex = listFeedItems.Columns.GetColumnIndexMap();
            INewsItemFilter flagFilter = null;

            if (CurrentSelectedFeedsNode is FlaggedItemsNode)
            {
                // do not apply flag filter on Flagged items node(s)
                flagFilter = _filterManager["NewsItemFlagFilter"];
                _filterManager.Remove("NewsItemFlagFilter");
            }

            EmptyListView();

            listFeedItems.BeginUpdate();

            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    INewsItem item = list[i];

                    if (!item.BeenRead)
                        unread.Add(item);

                    bool hasRelations = NewsItemHasRelations(item);

                    ThreadedListViewItem newItem = CreateThreadedLVItem(item, hasRelations,
                                                                        Resource.NewsItemImage.DefaultRead, colIndex,
                                                                        false);
                    _filterManager.Apply(newItem);

                    aNew[i] = newItem;
                }

                Array.Sort(aNew, listFeedItems.SortManager.GetComparer());
                listFeedItems.Items.AddRange(aNew);
                ApplyNewsItemPropertyImages(aNew);

                //listFeedItems.EndUpdate();
                if (listFeedItemsO.Visible)
                {
                    listFeedItemsO.AddRange(aNew);
                }
                return unread;
            }
            catch (Exception ex)
            {
                _log.Error("PopulateFullListView exception", ex);
                return unread;
            }
            finally
            {
                listFeedItems.EndUpdate();

                if (flagFilter != null)
                {
                    // add back
                    //flagFilter = _filterManager.Add("NewsItemFlagFilter", flagFilter);
                    _filterManager.Add("NewsItemFlagFilter", flagFilter);
                }
            }
        }

        /// <summary>
        /// Add NewsItem's from the ArrayList to the current displayed ListView. 
        /// This contains usually some items, so we have to insert the new items 
        /// at the correct position(s).
        /// </summary>
        /// <param name="list">A list of NewsItem objects.</param>
        /// <param name="categorizedView">True, if the feed title should be appended to
        /// each RSS Item title: "...rss item title... (feed title)"</param>
        /// <param name="checkDuplicates">If true, we check if a NewsItem is allready populated.
        /// This has a perf. impact, if true!</param>
        /// <returns>unread items</returns>
        public IList<INewsItem> PopulateSmartListView(IList<INewsItem> list, bool categorizedView, bool checkDuplicates)
        {
            var items = new List<ThreadedListViewItem>(listFeedItems.Items.Count);
            var newItems = new List<ThreadedListViewItem>(list.Count);
            var unread = new List<INewsItem>(list.Count);

            lock (listFeedItems.Items)
            {
                items.AddRange(listFeedItems.Items);
            }

            // column index map
            ColumnKeyIndexMap colIndexes = listFeedItems.Columns.GetColumnIndexMap();

            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    INewsItem item = list[i];
                    bool hasRelations = NewsItemHasRelations(item);
                    bool isDuplicate = false;
                    ThreadedListViewItem tlvi = null;

                    if (checkDuplicates)
                    {
                        //lock(listFeedItems.Items) {
                        // look, if it is already there
                        for (int j = 0; j < items.Count; j++)
                        {
                            tlvi = items[j];
                            if (item.Equals(tlvi.Key) && tlvi.IndentLevel == 0)
                            {
                                tlvi.Key = item; // update ref
                                isDuplicate = true;
                                break;
                            }
                        }
                        //}
                    }

                    if (isDuplicate)
                    {
                        // do not create a new one, but check if it has new childs 
                        if (!tlvi.HasChilds && hasRelations)
                            tlvi.HasChilds = hasRelations;
                        ApplyStyles(tlvi); //highlight item if it has new comments						
                    }
                    else
                    {
                        ThreadedListViewItem newItem = CreateThreadedLVItem(item, hasRelations,
                                                                            Resource.NewsItemImage.DefaultRead,
                                                                            colIndexes,
                                                                            false);

                        _filterManager.Apply(newItem);
                        newItems.Add(newItem);
                    }


                    if (!item.BeenRead)
                        unread.Add(item);
                } //for(int i)

                if (newItems.Count > 0)
                {
                    try
                    {
                        listFeedItems.BeginUpdate();

                        lock (listFeedItems.Items)
                        {
                            var a = new ThreadedListViewItem[newItems.Count];
                            newItems.CopyTo(a);
                            listFeedItems.ListViewItemSorter = listFeedItems.SortManager.GetComparer();
                            listFeedItems.Items.AddRange(a);
                            if (listFeedItemsO.Visible)
                                listFeedItemsO.AddRange(a);
                            ApplyNewsItemPropertyImages(a);
                            listFeedItems.ListViewItemSorter = null;

                            if (listFeedItems.SelectedItems.Count > 0)
                            {
                                listFeedItems.EnsureVisible(listFeedItems.SelectedItems[0].Index);
                                if (listFeedItemsO.Visible)
                                    listFeedItemsO.GetFromLVI((ThreadedListViewItem) listFeedItems.SelectedItems[0]).
                                        BringIntoView();
                            }
                        }
                    }
                    finally
                    {
                        listFeedItems.EndUpdate();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("PopulateSmartListView exception", ex);
            }

            return unread;
        }

        //		private int GetInsertIndexOfItem(ThreadedListViewItem item) {
        //			
        //			if (this._lvSortHelper.Sorting == SortOrder.Ascending) {
        //				for (int i = 0; i < listFeedItems.Items.Count; i++) {
        //					ThreadedListViewItem tlv = (ThreadedListViewItem) listFeedItems.Items[i];
        //					if (tlv.IndentLevel == 0 && this._lvSortHelper.Compare(item, tlv) >= 0)
        //						return i;
        //				} 
        //
        //				return 0;
        //
        //			} else {
        //				for (int i = 0; i < listFeedItems.Items.Count; i++) {
        //					ThreadedListViewItem tlv = (ThreadedListViewItem) listFeedItems.Items[i];
        //					if (tlv.IndentLevel == 0 && this._lvSortHelper.Compare(item, tlv) <= 0)
        //						return i;
        //				}			
        //
        //			}
        //			// mean: the caller should append the item
        //			return listFeedItems.Items.Count;
        //		}

        private bool NewsItemHasRelations(INewsItem item)
        {
            return NewsItemHasRelations(item, new INewsItem[] {});
        }

        private bool NewsItemHasRelations(INewsItem item, IList<INewsItem> itemKeyPath)
        {
            bool hasRelations = false;
            if (item.Feed != null & owner.FeedHandler.IsSubscribed(item.Feed.link))
            {
                hasRelations = owner.FeedHandler.HasItemAnyRelations(item, itemKeyPath);
            }
            if (!hasRelations) hasRelations = (item.HasExternalRelations && owner.InternetAccessAllowed);
            return hasRelations;
        }

        public void BeginLoadCommentFeed(INewsItem item, string ticket, IList<INewsItem> itemKeyPath)
        {
            owner.MakeAndQueueTask(ThreadWorker.Task.LoadCommentFeed,
                                   OnLoadCommentFeedProgress,
                                   item, ticket, itemKeyPath);
        }

        private void OnLoadCommentFeedProgress(object sender, ThreadWorkerProgressArgs args)
        {
            if (args.Exception != null)
            {
                // failure(s)
                args.Cancel = true;
                ExceptionManager.Publish(args.Exception);
                var results = (object[]) args.Result;
                var insertionPointTicket = (string) results[2];
                var newChildItems =
                    new[] {CreateThreadedLVItemInfo(args.Exception.Message, true)};
                listFeedItems.InsertItemsForPlaceHolder(insertionPointTicket, newChildItems, false);
                if (listFeedItemsO.Visible && newChildItems.Length > 0)
                {
                    listFeedItemsO.AddRangeComments(newChildItems[0].Parent, newChildItems);
                }
            }
            else if (!args.Done)
            {
                // in progress
                // we already have a "loading ..." text listview item
            }
            else if (args.Done)
            {
                // done
                var results = (object[]) args.Result;
                var commentItems = (List<INewsItem>) results[0];
                var item = (INewsItem) results[1];
                var insertionPointTicket = (string) results[2];
                var itemKeyPath = (IList<INewsItem>) results[3];


                if (item.CommentCount != commentItems.Count)
                {
                    item.CommentCount = commentItems.Count;
                    owner.FeedWasModified(item.Feed, NewsFeedProperty.FeedItemCommentCount);
                }

                commentItems.Sort(RssHelper.GetComparer(false, NewsItemSortField.Date));
                item.SetExternalRelations(commentItems);

                ThreadedListViewItem[] newChildItems = null;

                if (commentItems.Count > 0)
                {
                    var newChildItemsArray = new ArrayList(commentItems.Count);

                    // column index map
                    ColumnKeyIndexMap colIndex = listFeedItems.Columns.GetColumnIndexMap();

                    for (int i = 0; i < commentItems.Count; i++)
                    {
                        INewsItem o = commentItems[i];
                        if (itemKeyPath != null && itemKeyPath.Contains(o))
                            continue;


                        bool hasRelations = NewsItemHasRelations(o, itemKeyPath);

                        o.BeenRead = tempFeedItemsRead.ContainsKey(RssHelper.GetHashCode(o));
                        ThreadedListViewItem newListItem =
                            CreateThreadedLVItem(o, hasRelations, Resource.NewsItemImage.CommentRead, colIndex, true);
                        _filterManager.Apply(newListItem);
                        newChildItemsArray.Add(newListItem);
                    } //iterator.MoveNext

                    if (newChildItemsArray.Count > 0)
                    {
                        newChildItems = new ThreadedListViewItem[newChildItemsArray.Count];
                        newChildItemsArray.CopyTo(newChildItems);
                    }
                }

                listFeedItems.InsertItemsForPlaceHolder(insertionPointTicket, newChildItems, false);
                if (newChildItems != null)
                {
                    if (listFeedItemsO.Visible && newChildItems.Length > 0)
                    {
                        listFeedItemsO.AddRangeComments(newChildItems[0].Parent, newChildItems);
                    }
                }
            }
        }

        /// <summary>
        /// Called to refresh the GUI state after refresh of feeds/feed items
        /// </summary>
        public void TriggerGUIStateOnNewFeeds(bool handleNewReceived)
        {
            int unreadFeeds, unreadMessages;
            CountUnread(out unreadFeeds, out unreadMessages);

            if (unreadMessages != 0)
            {
                _timerResetStatus.Stop();
                if (handleNewReceived && unreadMessages > _lastUnreadFeedItemCountBeforeRefresh)
                {
                    string message = String.Format(SR.GUIStatusNewFeedItemsReceivedMessage, unreadFeeds, unreadMessages);
                    if (Visible)
                    {
                        SetGuiStateFeedback(message, ApplicationTrayState.NewUnreadFeeds);
                    }
                    else
                    {
                        // if invisible (tray only): animate
                        SetGuiStateFeedback(message, ApplicationTrayState.NewUnreadFeedsReceived);
                    }
                    if (owner.Preferences.ShowNewItemsReceivedBalloon &&
                        (SystemTrayOnlyVisible || WindowState == FormWindowState.Minimized))
                    {
                        if (_beSilentOnBalloonPopupCounter <= 0)
                        {
                            message = String.Format(SR.GUIStatusNewFeedItemsReceivedMessage,
                                                    unreadFeeds, unreadMessages);
                            _trayAni.ShowBalloon(NotifyIconAnimation.EBalloonIcon.Info, message,
                                                 RssBanditApplication.CaptionOnly + " - " +
                                                 SR.GUIStatusNewFeedItemsReceived);
                        }
                        else
                        {
                            _beSilentOnBalloonPopupCounter--;
                        }
                    }
                }
                else
                {
                    SetGuiStateFeedback(String.Empty, ApplicationTrayState.NewUnreadFeeds);
                }
            }
            else
            {
                SetGuiStateFeedback(String.Empty, ApplicationTrayState.NormalIdle);
            }
        }


        /// <summary>
        /// Updates the comment status for the specified tree node and any related search folders that may 
        /// contain items from this node. 
        /// </summary>
        /// <param name="tn">The tree node whose comment status is being updated</param>
        /// <param name="f">The feed associated with the tree node</param>
        private void UpdateCommentStatus(TreeFeedsNodeBase tn, INewsFeed f)
        {
            IList<INewsItem> itemsWithNewComments = GetFeedItemsWithNewComments(f);
            tn.UpdateCommentStatus(tn, itemsWithNewComments.Count);
            owner.UpdateWatchedItems(itemsWithNewComments);
            WatchedItemsNode.UpdateCommentStatus();
        }

        /// <summary>
        /// Updates the comment status for the specified tree node and any related search folders that may
        /// contain items from this node.
        /// </summary>
        /// <param name="tn">The tree node whose comment status is being updated</param>
        /// <param name="items">The items.</param>
        /// <param name="commentsRead">Indicates that these are new comments or whether the comments were just read</param>
        private void UpdateCommentStatus(TreeFeedsNodeBase tn, IList<INewsItem> items, bool commentsRead)
        {
            int multiplier = (commentsRead ? -1 : 1);

            if (commentsRead)
            {
                tn.UpdateCommentStatus(tn, items.Count*multiplier);
                owner.UpdateWatchedItems(items);
            }
            else
            {
                IList<INewsItem> itemsWithNewComments = GetFeedItemsWithNewComments(items);
                tn.UpdateCommentStatus(tn, itemsWithNewComments.Count*multiplier);
                owner.UpdateWatchedItems(itemsWithNewComments);
            }
            WatchedItemsNode.UpdateCommentStatus();
        }

        /// <summary>
        /// Returns the number of items with new comments in a particular list of items
        /// </summary>
        /// <param name="items">The list of items</param>
        /// <returns>The number of items with new comments</returns>
        private static IList<INewsItem> GetFeedItemsWithNewComments(IList<INewsItem> items)
        {
            var itemsWithNewComments = new List<INewsItem>();

            if (items == null) return itemsWithNewComments;
            if (items.Count == 0) return itemsWithNewComments;

            for (int i = 0; i < items.Count; i++)
            {
                INewsItem item = items[i];
                if (item.HasNewComments) itemsWithNewComments.Add(item);
            }

            return itemsWithNewComments;
        }

        /// <summary>
        /// Remove unread items of the feed from the unread item tree node container.
        /// </summary>
        /// <param name="feedLink">The feed link.</param>
        private void UnreadItemsNodeRemoveItems(string feedLink)
        {
            
           //BUGBUG: This doesn't handle being subscribed to the same feed from multiple feed sources

            if (string.IsNullOrEmpty(feedLink)){
                return; 
            }
                
            FeedSourceEntry fse = FeedSourceOf(feedLink);
            if (fse != null)
            {
                IList<INewsItem> items = fse.Source.GetCachedItemsForFeed(feedLink);
                UnreadItemsNodeRemoveItems(FilterUnreadFeedItems(items));
            }
        }

        /// <summary>
        /// Remove unread items of the feed f from the unread item tree node container.
        /// </summary>
        /// <param name="f">The feed.</param>
        private void UnreadItemsNodeRemoveItems(INewsFeed f)
        {
            if (f == null) return;
            UnreadItemsNodeRemoveItems(FilterUnreadFeedItems(f));
        }

        /// <summary>
        /// Remove items from the unread item tree node container.
        /// The NewsItems in unread list are NOT checked again if they are
        /// unread!
        /// </summary>
        /// <param name="unread">The unread item list.</param>
        private void UnreadItemsNodeRemoveItems(IList<INewsItem> unread)
        {
            if (unread == null) return;
            for (int i = 0; i < unread.Count; i++)
                UnreadItemsNode.Remove(unread[i]);
            UnreadItemsNode.UpdateReadStatus();
        }

        /// <summary>
        /// Gets the list of unread item only from the provided feed.
        /// </summary>
        /// <param name="f">The feed.</param>
        /// <returns></returns>
        private IList<INewsItem> FilterUnreadFeedItems(INewsFeed f)
        {
            var result = new List<INewsItem>();

            if (f == null)
                return result;

            if (f.containsNewMessages)
            {
                IList<INewsItem> items = null;
                try
                {
                    items = FeedSourceOf(f.link).Source.GetCachedItemsForFeed(f.link);
                }
                catch
                {
                    /* ignore cache errors here. On error, it returns always empty list */
                }

                return FilterUnreadFeedItems(items);
            }
            return result;
        }

        private static IList<INewsItem> FilterUnreadFeedItems(IList<INewsItem> items)
        {
            return FilterUnreadFeedItems(items, false);
        }

        /// <summary>
        /// Gets the unread items out of the provided list.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="sorted">if set to <c>true</c> it returns a sorted list (descending by item date,
        /// means newest first).</param>
        /// <returns></returns>
        private static IList<INewsItem> FilterUnreadFeedItems(IList<INewsItem> items, bool sorted)
        {
            var result = new List<INewsItem>();

            if (items == null || items.Count == 0)
                return result;

            for (int i = 0; i < items.Count; i++)
            {
                INewsItem item = items[i];
                if (!item.BeenRead)
                    result.Add(item);
            }

            if (sorted)
                result.Sort(RssHelper.GetComparer(true));
            return result;
        }

        /// <summary>
        /// Returns the number of unread items in a particular feed
        /// </summary>
        /// <param name="f">The target feed</param>
        /// <returns>The number of unread items</returns>
        private int CountUnreadFeedItems(INewsFeed f)
        {
            if (f == null) return 0;
            return FilterUnreadFeedItems(f).Count;
        }

        /// <summary>
        /// Returns the number of items with unread comments for this feed
        /// </summary>
        /// <param name="f">The target feed</param>
        /// <returns>The number of items with unread comments </returns>
        private IList<INewsItem> GetFeedItemsWithNewComments(INewsFeed f)
        {
            var itemsWithNewComments = new List<INewsItem>();

            if (f == null) return itemsWithNewComments;

            if (f.containsNewComments)
            {
                IList<INewsItem> items = null;
                try
                {
                    items = FeedSourceOf(f.link).Source.GetCachedItemsForFeed(f.link);
                }
                catch
                {
                    /* ignore cache errors here. On error, it returns always zero */
                }
                if (items == null) return itemsWithNewComments;

                for (int i = 0; i < items.Count; i++)
                {
                    INewsItem item = items[i];
                    if (item.HasNewComments) itemsWithNewComments.Add(item);
                }
            }
            return itemsWithNewComments;
        }


        /// <summary>
        /// Obtains the number of unread RSS feeds and total unread RSS items
        /// </summary>
        /// <param name="unreadFeeds">Total RSS feeds with at least one unread item</param>
        /// <param name="unreadMessages">Total unread items</param>
        private void CountUnread(out int unreadFeeds, out int unreadMessages)
        {
            /* this code is inefficient because we loop through feeds and items even though 
			 * we probably just did that in a RefreshFeeds(). At least this should happen only
			 * when the app is minimized so this doesn't delay response time to user input. 
			 */

            unreadFeeds = unreadMessages = 0;

            foreach (var f in owner.FeedHandler.GetFeeds().Values)
            {
                if (f.containsNewMessages)
                {
                    unreadFeeds++;
                    int urm = CountUnreadFeedItems(f);
                    unreadMessages += urm;
                }
            }
        }

        private void CheckForAddIns()
        {
            owner.CheckAndLoadAddIns();
            IBlogExtension ibe = null;

            try
            {
                blogExtensions = ServiceManager.SearchForIBlogExtensions(RssBanditApplication.GetPlugInPath());
                if (blogExtensions == null || blogExtensions.Count == 0)
                    return;

                // separator
                _listContextMenu.MenuItems.Add(new MenuItem("-"));

                for (int i = 0; i < blogExtensions.Count; i++)
                {
                    ibe = blogExtensions[i];
                    var m = new AppContextMenuCommand("cmdIBlogExt." + i,
                                                      owner.Mediator,
                                                      new ExecuteCommandHandler(
                                                          owner.CmdGenericListviewCommand),
                                                      ibe.DisplayName,
                                                      SR.MenuIBlogExtensionCommandDesc);
                    _listContextMenu.MenuItems.Add(m);
                    if (ibe.HasConfiguration)
                    {
                        var mc = new AppContextMenuCommand("cmdIBlogExtConfig." + i,
                                                           owner.Mediator,
                                                           new ExecuteCommandHandler(
                                                               owner.CmdGenericListviewCommandConfig),
                                                           ibe.DisplayName + " - " +
                                                           SR.MenuConfigCommandCaption,
                                                           SR.MenuIBlogExtensionConfigCommandDesc);
                        _listContextMenu.MenuItems.Add(mc);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Fatal(
                    "Failed to load IBlogExtension plugin: " + (ibe == null ? String.Empty : ibe.GetType().FullName), ex);
                ExceptionManager.Publish(ex);
            }
            finally
            {
                //unload AppDomain used to load add-ins
                ServiceManager.UnloadLoaderAppDomain();
            }
        }

        private void OnFeedTransformed(object sender, ThreadWorkerProgressArgs args)
        {
            if (args.Exception != null)
            {
                // failure(s)
                args.Cancel = true;
                RssBanditApplication.PublishException(args.Exception);
            }
            else if (!args.Done)
            {
                // in progress
            }
            else if (args.Done)
            {
                // done
                var results = (object[]) args.Result;
                var node = (UltraTreeNode) results[0];
                var html = (string) results[1];
                if ((listFeedItems.SelectedItems.Count == 0) && treeFeeds.SelectedNodes.Count > 0 &&
                    ReferenceEquals(treeFeeds.SelectedNodes[0], node))
                {
                    htmlDetail.Html = html;
                    htmlDetail.Navigate(null);
                }
            }
        }


		/// <summary>
		/// Invoked by RssBanditApplication when an enclosure has been successfully dowbloaded
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="e">The <see cref="NewsComponents.Net.DownloadItemEventArgs"/> instance containing the event data.</param>
		internal void OnEnclosureReceived(FeedSourceEntry entry, DownloadItemEventArgs e)
        {
            /* display alert window on new download available */
            if (entry.Source.IsSubscribed(e.DownloadItem.OwnerFeedId))
            {
				INewsFeed f = entry.Source.GetFeeds()[e.DownloadItem.OwnerFeedId];

				if (entry.Source.GetEnclosureAlert(f.link))
                {
                    e.DownloadItem.OwnerFeed = f;
                    var items = new List<DownloadItem>
                                    {
                                        e.DownloadItem
                                    };
                    toastNotifier.Alert(f.title, 1, items);
                }
            } //if(feedHandler.GetFeeds().Contains(..))
        }


        private void BeginTransformFeed(IFeedDetails feed, UltraTreeNode feedNode, string stylesheet)
        {
            /* perform XSLT transformation in a background thread */
            owner.MakeAndQueueTask(ThreadWorker.Task.TransformFeed, OnFeedTransformed,
                                   ThreadWorkerBase.DuplicateTaskQueued.Abort, feed, feedNode, stylesheet);
        }


        private void BeginTransformFeedList(FeedInfoList feeds, UltraTreeNode feedNode, string stylesheet)
        {
            /* perform XSLT transformation in a background thread */
            owner.MakeAndQueueTask(ThreadWorker.Task.TransformCategory,
                                   OnFeedTransformed,
                                   ThreadWorkerBase.DuplicateTaskQueued.Abort, feeds, feedNode, stylesheet);
        }


        /// <summary>
        /// Returns a FeedInfoList containing the news items which should be displayed on the 
        /// specified page if in the newspaper view. 
        /// </summary>
        /// <param name="pageNum">The page number. If the page number is outside the range 
        /// of valid values then the first page is returned. </param>
        /// <returns>A FeedInfoList containing all the news items that should be displayed on 
        /// the specified page</returns>
        private FeedInfoList GetCategoryItemsAtPage(int pageNum)
        {
            if (_currentCategoryNewsItems == null)
            {
                return null;
            }

            int itemsPerPage = Convert.ToInt32(owner.Preferences.NumNewsItemsPerPage);

            bool validPageNum = (pageNum >= 1) && (pageNum <= _lastPageNumber);

            if (owner.Preferences.LimitNewsItemsPerPage && validPageNum)
            {
                var fil = new FeedInfoList(_currentCategoryNewsItems.Title);

                int endindex = pageNum*itemsPerPage;
                int startindex = endindex - itemsPerPage;
                int counter = 0;
                int numLeft = itemsPerPage;

                foreach (FeedInfo fi in _currentCategoryNewsItems)
                {
                    if (numLeft <= 0)
                    {
                        break;
                    }

                    FeedInfo ficlone = fi.Clone(false);

                    if ((fi.ItemsList.Count + counter) > startindex)
                    {
                        //is this feed on the page?
                        int actualstart = startindex - counter;
                        int actualend = actualstart + numLeft;

                        if (actualend > fi.ItemsList.Count)
                        {
                            //handle case where this feed isn't the last one on the page							
                            int numAdded = fi.ItemsList.Count - actualstart;
                            ficlone.ItemsList.AddRange(fi.ItemsList.GetRange(actualstart, numAdded));
                            numLeft -= numAdded;
                            startindex += numAdded;
                        }
                        else
                        {
                            ficlone.ItemsList.AddRange(fi.ItemsList.GetRange(actualstart, numLeft));
                            numLeft -= numLeft;
                        }
                        fil.Add(ficlone);
                    }
                    counter += fi.ItemsList.Count;
                } //foreach


                return fil;
            }

            return _currentCategoryNewsItems;
        }

        /// <summary>
        /// Returns a FeedInfo containing the news items which should be displayed on the 
        /// specified page if in the newspaper view. 
        /// </summary>
        /// <param name="pageNum">The page number. If the page number is outside the range 
        /// of valid values then the first page is returned. </param>
        /// <returns>A FeedInfo containing all the news items that should be displayed on 
        /// the specified page</returns>
        private FeedInfo GetFeedItemsAtPage(int pageNum)
        {
            if (_currentFeedNewsItems == null)
            {
                return null;
            }

            int itemsPerPage = Convert.ToInt32(owner.Preferences.NumNewsItemsPerPage);
            int numItems = _currentFeedNewsItems.ItemsList.Count;

            bool validPageNum = (pageNum >= 1) && (pageNum <= _lastPageNumber);

            if (owner.Preferences.LimitNewsItemsPerPage && validPageNum)
            {
                FeedInfo fi = _currentFeedNewsItems.Clone(false);

                int endindex = pageNum*itemsPerPage;
                int startindex = endindex - itemsPerPage;

                if (endindex > numItems)
                {
                    //handle if we are on last page and numItems % itemsPerPage != 0
                    fi.ItemsList.AddRange(_currentFeedNewsItems.ItemsList.GetRange(startindex, numItems - startindex));
                }
                else
                {
                    fi.ItemsList.AddRange(_currentFeedNewsItems.ItemsList.GetRange(startindex, itemsPerPage));
                }

                return fi;
            }

            return _currentFeedNewsItems;
        }

        /// <summary>
        /// Reloads the list view if the feed node is selected and renders the newspaper view
        /// </summary>
        /// <param name="tn">the tree node</param>
        /// <param name="populateListview">indicates whether the list view should be repopulated or not</param>
        internal void RefreshFeedDisplay(TreeFeedsNodeBase tn, bool populateListview)
        {
            if (tn == null)
                tn = CurrentSelectedFeedsNode;
            if (tn == null)
                return;
            if (!tn.Selected || tn.Type != FeedNodeType.Feed)
                return;

            FeedSourceEntry entry = FeedSourceOf(tn); 
            INewsFeed f = owner.GetFeed(entry, tn.DataKey);

            if (f != null)
            {
                owner.StateHandler.MoveNewsHandlerStateTo(NewsHandlerState.RefreshOne);
                try
                {
                    htmlDetail.Clear();
                    // Old: call may initiate a web request, if eTag/last retrived is too old:
                    //ArrayList items = owner.FeedHandler.GetItemsForFeed(tn.DataKey, false);
                    // this will just get the items from cache:
                    IList<INewsItem> items = entry.Source.GetCachedItemsForFeed(tn.DataKey);
                    IList<INewsItem> unread = FilterUnreadFeedItems(items, true);

                    if ((DisplayFeedAlertWindow.All == owner.Preferences.ShowAlertWindow ||
                         (DisplayFeedAlertWindow.AsConfiguredPerFeed == owner.Preferences.ShowAlertWindow &&
                          f.alertEnabled)) &&
                        tn.UnreadCount < unread.Count)
                    {
                        //test flag on feed, if toast enabled
                        toastNotifier.Alert(tn.Text, unread.Count, unread);
                    }

                    if (tn.UnreadCount != unread.Count)
                    {
                        UnreadItemsNodeRemoveItems(items);
                        foreach (var ui in unread)
                            UnreadItemsNode.Items.Add(ui);
                        UnreadItemsNode.UpdateReadStatus();
                    }

                    //we don't need to populate the listview if this called from 
                    //RssBanditApplication.ApplyPreferences() since it is already populated
                    if (populateListview)
                    {
                        PopulateListView(tn, items, true, false, tn);
                    }

                    IFeedDetails fi = owner.GetFeedDetails(FeedSourceOf(tn), tn.DataKey);

                    if (fi != null)
                    {
                        FeedDetailTabState.Url = fi.Link;

                        //we use a clone of the FeedInfo because it isn't 
                        //necessarily true that everything in the main FeedInfo is being rendered
                        var fi2 = new FeedInfo(fi);
                        fi2.ItemsList.Clear();

                        fi2.ItemsList.AddRange(unread);

                        //sort news items
                        //TODO: Ensure that there is no chance the code below can throw ArgumentOutOfRangeException 
                        ThreadedListViewColumnHeader colHeader =
                            listFeedItems.Columns[listFeedItems.SortManager.SortColumnIndex];
                        IComparer<INewsItem> newsItemSorter =
                            RssHelper.GetComparer(listFeedItems.SortManager.SortOrder == SortOrder.Descending,
                                                  (NewsItemSortField)
                                                  Enum.Parse(typeof (NewsItemSortField), colHeader.Key));

                        fi2.ItemsList.Sort(newsItemSorter);

                        //store list of unread items then only send one page of results 
                        //to newspaper view. 						
                        _currentFeedNewsItems = fi2;
                        _currentCategoryNewsItems = null;
                        _currentPageNumber = _lastPageNumber = 1;
                        int numItems = _currentFeedNewsItems.ItemsList.Count;
                        string stylesheet = owner.FeedHandler.GetStyleSheet(tn.DataKey);

                        if (numItems > 0)
                        {
                            int itemsPerPage = Convert.ToInt32(owner.Preferences.NumNewsItemsPerPage);
                            _lastPageNumber = (numItems/itemsPerPage) + (numItems%itemsPerPage == 0 ? 0 : 1);

                            //default stylesheet: get first page of items
                            if (string.IsNullOrEmpty(stylesheet))
                            {
                                fi2 = GetFeedItemsAtPage(1);
                            }
                        }

                        //check to see if we still have focus 
                        if (tn.Selected)
                        {
                            BeginTransformFeed(fi2, tn, stylesheet);
                        }
                    }
                }
                catch (Exception e)
                {
                    EmptyListView();
                    owner.PublishXmlFeedError(e, tn.DataKey, true, entry);
                }
                owner.StateHandler.MoveNewsHandlerStateTo(NewsHandlerState.RefreshOneDone);
            }
        }


        /// <summary>
        ///  Reloads the list view if the category node is selected and renders the newspaper view
        /// </summary>
        /// <param name="tn">the tree node</param>
        private void RefreshCategoryDisplay(TreeFeedsNodeBase tn)
        {
            listFeedItems.BeginUpdate();
            string category = tn.CategoryStoreName;
            var unreadItems = new FeedInfoList(category);

            PopulateListView(tn, new List<INewsItem>(), true);
            htmlDetail.Clear();
            WalkdownThenRefreshFeed(tn, false, true, tn, unreadItems);


            listFeedItems.EndUpdate();

            //sort news items
            //TODO: Ensure that there is no chance the code below can throw ArgumentOutOfRangeException 
            ThreadedListViewColumnHeader colHeader = listFeedItems.Columns[listFeedItems.SortManager.SortColumnIndex];
            IComparer<INewsItem> newsItemSorter =
                RssHelper.GetComparer(listFeedItems.SortManager.SortOrder == SortOrder.Descending,
                                      (NewsItemSortField) Enum.Parse(typeof (NewsItemSortField), colHeader.Key));

            foreach (FeedInfo f in unreadItems)
            {
                f.ItemsList.Sort(newsItemSorter);
            }


            //store list of unread items then only send one page of results 
            //to newspaper view. 						
            _currentFeedNewsItems = null;
            FeedInfoList fil2 = _currentCategoryNewsItems = unreadItems;
            _currentPageNumber = _lastPageNumber = 1;
            int numItems = _currentCategoryNewsItems.NewsItemCount;
            string stylesheet = owner.FeedHandler.GetCategoryStyleSheet(category);

            if (numItems > 0)
            {
                int itemsPerPage = Convert.ToInt32(owner.Preferences.NumNewsItemsPerPage);
                _lastPageNumber = (numItems/itemsPerPage) + (numItems%itemsPerPage == 0 ? 0 : 1);

                //default stylesheet: get first page of items
                if (string.IsNullOrEmpty(stylesheet))
                {
                    fil2 = GetCategoryItemsAtPage(1);
                }
            }


            if (tn.Selected)
            {
                FeedDetailTabState.Url = String.Empty;
                BeginTransformFeedList(fil2, tn, stylesheet);
            }
        }


        internal void DeleteCategory(TreeFeedsNodeBase categoryFeedsNode)
        {
            if (categoryFeedsNode == null) categoryFeedsNode = CurrentSelectedFeedsNode;
            if (categoryFeedsNode == null) return;
            if (categoryFeedsNode.Type != FeedNodeType.Category) return;

            TreeFeedsNodeBase cnf = null;

            // if there are feed items displayed, we may have to delete the content
            // if rss items are of a feed with the category to delete
            if (listFeedItems.Items.Count > 0)
                cnf = TreeHelper.FindNode(categoryFeedsNode, (INewsItem) (listFeedItems.Items[0]).Key);
            if (cnf != null)
            {
                EmptyListView();
                htmlDetail.Clear();
            }

            if (categoryFeedsNode.Selected ||
                TreeHelper.IsChildNode(categoryFeedsNode, TreeSelectedFeedsNode))
            {
                TreeSelectedFeedsNode = TreeHelper.GetNewNodeToActivate(categoryFeedsNode);
                RefreshFeedDisplay(TreeSelectedFeedsNode, true);
            }
           
            WalkdownThenDeleteFeedsOrCategories(categoryFeedsNode);
            string catName = TreeFeedsNodeBase.BuildCategoryStoreName(categoryFeedsNode);
            FeedSourceEntry entry = FeedSourceOf(categoryFeedsNode);
            if (entry != null)
            {
                entry.Source.DeleteCategory(catName);
            }

            UpdateTreeNodeUnreadStatus(categoryFeedsNode, 0);

            try
            {
                categoryFeedsNode.Parent.Nodes.Remove(categoryFeedsNode);
            }
            finally
            {
                DelayTask(DelayedTasks.SyncRssSearchTree);
            }
        }

        /// <summary>
        /// Helper. Work recursive on the startNode down to the leaves.
        /// Then call AsyncGetItemsForFeed() for each of them.
        /// </summary>
        /// <param name="startNode">Node to start with</param>
        /// <param name="forceRefresh">true, if refresh should be forced</param>
        /// <param name="categorized">indicates whether this is part of the refresh or click of a category node</param>
        /// <param name="initialFeedsNode">This is the node where the refresh began from</param>
        /// <param name="unreadItems">an array list to place the unread items in the category into. This is needed to render them afterwards 
        /// in a newspaper view</param>
        private void WalkdownThenRefreshFeed(TreeFeedsNodeBase startNode, bool forceRefresh, bool categorized,
                                             TreeFeedsNodeBase initialFeedsNode, ICollection<IFeedDetails> unreadItems)
        {
            if (startNode == null) return;

            if (TreeSelectedFeedsNode != initialFeedsNode)
                return; // do not continue, if selection was changed
			
			// TODO: make it more efficient (we should not get feedsource in each recursion call level)
        	FeedSourceEntry entry = FeedSourceOf(startNode);
            try
            {
                for (TreeFeedsNodeBase child = startNode.FirstNode; child != null; child = child.NextNode)
                {
                    if (Disposing)
                        return;

                    if (child.Type != FeedNodeType.Feed && child.FirstNode != null)
                    {
                        //if (forceRefresh) {
                        WalkdownThenRefreshFeed(child, forceRefresh, categorized, initialFeedsNode, unreadItems);
                        //}
                    }
                    else
                    {
                        string feedUrl = child.DataKey;

                        if (feedUrl == null || entry == null || !entry.Source.IsSubscribed(feedUrl))
                            continue;

                        try
                        {
                            if (forceRefresh)
                            {
                                //owner.FeedHandler.AsyncGetItemsForFeed(feedUrl, forceRefresh);
                                DelayTask(DelayedTasks.StartRefreshOneFeed, feedUrl);
                            }
                            else if (categorized)
                            {
								IList<INewsItem> items = entry.Source.GetCachedItemsForFeed(feedUrl);
                                INewsFeed f = owner.GetFeed(entry, feedUrl);
                                FeedInfo fi;

                                if (f != null)
                                {
                                    IFeedDetails ifd = owner.GetFeedDetails(entry, f.link);

                                    if (ifd == null) // with with an error, and the like: ignore
                                        continue;

                                    fi = new FeedInfo(ifd);
                                    fi.ItemsList.Clear();
                                }
                                else
                                {
                                    fi = FeedInfo.Empty;
                                }

                                foreach (var i in items)
                                {
                                    if (!i.BeenRead)
                                        fi.ItemsList.Add(i);
                                }

                                if (fi.ItemsList.Count > 0)
                                {
                                    unreadItems.Add(fi);
                                    if (fi.ItemsList.Count != child.UnreadCount)
                                    {
                                        UpdateTreeNodeUnreadStatus(child, fi.ItemsList.Count);
                                        UnreadItemsNodeRemoveItems(items);
                                        foreach (var it in fi.ItemsList)
                                            UnreadItemsNode.Items.Add(it);
                                        UnreadItemsNode.UpdateReadStatus();
                                    }
                                }

                                //todo -- build list to add at end
                                PopulateListView(child, items, false, true, initialFeedsNode);
                                Application.DoEvents();
                            }
                        }
                        catch (Exception e)
                        {
                            owner.PublishXmlFeedError(e, feedUrl, true, entry);
                        }
                    }
                } //for
            }
            catch (Exception ex)
            {
                _log.Error("WalkdownThenRefreshFeed() failed.", ex);
            }
        }


        /// <summary>
        /// Helper. Work recursive on the startNode down to the leaves.
        /// Then catchup categories on any child FeedNode. Does not work on
        /// the root Node (there we call FeedHandler.MarkAllCachedItemsAsRead) !
        /// </summary>
        /// <param name="startNode">Node to start with. The startNode itself is 
        /// considered on catchup.</param>
        private void WalkdownAndCatchupCategory(TreeFeedsNodeBase startNode)
        {
            if (startNode == null) return;

            if (startNode.Type == FeedNodeType.Category)
            {
                for (TreeFeedsNodeBase child = startNode.FirstNode; child != null; child = child.NextNode)
                {
                    if (child.Type == FeedNodeType.Category)
                        WalkdownAndCatchupCategory(child);
                    else
                    {
                        // rely on unread cached items:
                        UnreadItemsNodeRemoveItems(child.DataKey);
                        // and now mark cached items read:
                        owner.FeedHandler.MarkAllCachedItemsAsRead(child.DataKey);
                        UpdateTreeNodeUnreadStatus(child, 0);
                    }
                }
            }
            else
            {
                owner.FeedHandler.MarkAllCachedItemsAsRead(startNode.DataKey);
            }
        }

        /// <summary>
        /// Helper. Work recursive on the startNode down to the leaves.
        /// Then delete all child categories and FeedNode refs in owner.FeedHandler.
        /// </summary>
        /// <param name="startNode">new full category name (long name, with all the '\').</param>
        private void WalkdownThenDeleteFeedsOrCategories(TreeFeedsNodeBase startNode)
        {
            if (startNode == null) return;

            if (startNode.Type == FeedNodeType.Feed)
            {
                if (owner.FeedHandler.IsSubscribed(startNode.DataKey))
                {
					// TODO: make it more efficient (we should not get feedsource in each recursion call level)
					INewsFeed f = owner.GetFeed(FeedSourceOf(startNode), startNode.DataKey);
                    if (f != null)
                    {
                        UnreadItemsNodeRemoveItems(f);
                        f.Tag = null; // remove tree node ref.                  
                    }
                }
            }
            else
            {
                // other

                for (TreeFeedsNodeBase child = startNode.FirstNode; child != null; child = child.NextNode)
                {
                    WalkdownThenDeleteFeedsOrCategories(child);
                }
            }
        }

        //		bool StoreFeedColumnLayout(FeedTreeNodeBase startNode, string layout) {
        //			if (layout == null) throw new ArgumentNullException("layout");
        //			if (startNode == null) return false;
        //
        //			if (startNode.Type == FeedNodeType.Feed) {
        //				if (!string.IsNullOrEmpty(owner.FeedHandler.GetFeedColumnLayout(startNode.DataKey)))
        //					owner.FeedHandler.SetFeedColumnLayout(startNode.DataKey, layout);
        //				else
        //					CurrentFeedFeedColumnLayout = layout;
        //			} else if(startNode.Type == FeedNodeType.Category) {
        //				if (!string.IsNullOrEmpty(owner.FeedHandler.GetCategoryFeedColumnLayout(startNode.DataKey)))
        //					owner.FeedHandler.SetCategoryFeedColumnLayout(startNode.DataKey, layout);
        //				else
        //					CurrentCategoryFeedColumnLayout = layout;
        //			} else {
        //				CurrentSmartFolderFeedColumnLayout = layout;
        //			}
        //			
        //			return true;
        //		}


        /// <summary>
        /// Gets the feed column layout.
        /// </summary>
        /// <param name="startNode">The start node.</param>
        /// <returns></returns>
        private FeedColumnLayout GetFeedColumnLayout(TreeFeedsNodeBase startNode)
        {
            if (startNode == null)
                startNode = TreeSelectedFeedsNode;
            if (startNode == null)
                return listFeedItems.FeedColumnLayout;

            FeedColumnLayout layout = listFeedItems.FeedColumnLayout;
            if (startNode.Type == FeedNodeType.Feed)
            {
                layout = owner.GetFeedColumnLayout(startNode.DataKey) ?? owner.GlobalFeedColumnLayout;
            }
            else if (startNode.Type == FeedNodeType.Category)
            {
                layout = owner.GetCategoryColumnLayout(startNode.CategoryStoreName) ?? owner.GlobalCategoryColumnLayout;
            }
            else if (startNode.Type == FeedNodeType.Finder)
            {
                layout = owner.GlobalSearchFolderColumnLayout;
            }
            else if (startNode.Type == FeedNodeType.SmartFolder)
            {
                layout = owner.GlobalSpecialFolderColumnLayout;
            }
            return layout;
        }

        /// <summary>
        /// Sets the feed handler feed column layout.
        /// </summary>
        /// <param name="feedsNode">The feeds node.</param>
        /// <param name="layout">The layout.</param>
        private void SetFeedHandlerFeedColumnLayout(TreeFeedsNodeBase feedsNode, FeedColumnLayout layout)
        {
            if (feedsNode == null) feedsNode = CurrentSelectedFeedsNode;
            if (feedsNode != null)
            {
                if (feedsNode.Type == FeedNodeType.Feed)
                {
                    owner.SetFeedColumnLayout(feedsNode.DataKey, layout);
                }
                else if (feedsNode.Type == FeedNodeType.Category)
                {
                    owner.SetCategoryColumnLayout(feedsNode.CategoryStoreName, layout);
                }
                else if (feedsNode.Type == FeedNodeType.Finder)
                {
                    owner.GlobalSearchFolderColumnLayout = layout;
                }
                else if (feedsNode.Type == FeedNodeType.SmartFolder)
                {
                    owner.GlobalSpecialFolderColumnLayout = layout;
                }
            }
        }

        /// <summary>
        /// Sets the global feed column layout.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="layout">The layout.</param>
        private void SetGlobalFeedColumnLayout(FeedNodeType type, FeedColumnLayout layout)
        {
            if (layout == null) throw new ArgumentNullException("layout");

            if (type == FeedNodeType.Feed)
            {
                owner.GlobalFeedColumnLayout = layout;
            }
            else if (type == FeedNodeType.Category)
            {
                owner.GlobalCategoryColumnLayout = layout;
            }
            else
            {
                //CurrentCategoryFeedColumnLayout = layout;
            }
        }


        /// <summary>
        /// A helper method that locates the ThreadedListViewItem representing
        /// the NewsItem object. 
        /// </summary>
        /// <param name="item">The RSS item</param>
        /// <returns>The ThreadedListViewItem or null if 
        /// it can't be found</returns>
        public ThreadedListViewItem GetListViewItem(NewsItem item)
        {
            ThreadedListViewItem theItem = null;
            for (int i = 0; i < listFeedItems.Items.Count; i++)
            {
                ThreadedListViewItem currentItem = listFeedItems.Items[i];
                if (item.Equals(currentItem.Key))
                {
                    theItem = currentItem;
                    break;
                }
            }
            return theItem;
        }

        /// <summary>
        /// A helper method that locates the ThreadedListViewItem representing
        /// the NewsItem object with the given ID. 
        /// </summary>
        /// <param name="id">The RSS item's ID</param>
        /// <returns>The ThreadedListViewItem or null if 
        /// it can't be found</returns>
        public ThreadedListViewItem GetListViewItem(string id)
        {
            //TR: fix (2007/05/03) provided id can be Url Encoded:
            string normalizedId = HtmlHelper.UrlDecode(id);
            ThreadedListViewItem theItem = null;
            for (int i = 0; i < listFeedItems.Items.Count; i++)
            {
                ThreadedListViewItem currentItem = listFeedItems.Items[i];
                var item = (INewsItem) currentItem.Key;

                if (item.Id.Equals(id) || item.Id.Equals(normalizedId))
                {
                    theItem = currentItem;
                    break;
                }
            }
            return theItem;
        }

        /// <summary>
        /// Traverse down the tree on the path defined by 'category' 
        /// starting with 'startNode'.
        /// </summary>
        /// <param name="startNode">FeedTreeNodeBase to start with</param>
        /// <param name="category">A category path, e.g. 'Category1\SubCategory1'.</param>
        /// <returns>The leave category node.</returns>
        /// <remarks>If one category in the path is not found, it will be created.</remarks>
        internal TreeFeedsNodeBase CreateSubscriptionsCategoryHive(TreeFeedsNodeBase startNode, string category)
        {
            return TreeHelper.CreateCategoryHive(startNode, category, _treeCategoryContextMenu);
        }

        private void DoEditTreeNodeLabel()
        {
            if (CurrentSelectedFeedsNode != null)
            {
                CurrentSelectedFeedsNode.BeginEdit();
            }
        }
    }
}