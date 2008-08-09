using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using RssBandit.WinGui.Controls.ThListView;
using IEControl;
using Infragistics.Win.UltraWinExplorerBar;
using Infragistics.Win.UltraWinToolbars;
using Infragistics.Win.UltraWinTree;
using Interop.SHDocVw;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.RelationCosmos;
using NewsComponents.Search;
using NewsComponents.Utils;
using RssBandit.Common;
using RssBandit.Resources;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Dialogs;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Menus;
using RssBandit.WinGui.Tools;
using RssBandit.WinGui.Utility;
using Syndication.Extensibility;
using TD.SandDock;
using TD.SandDock.Rendering;

namespace RssBandit.WinGui.Forms
{
    internal partial class WinGuiMain
    {
        #region Callback and event handler routines

        private static void CmdNop(ICommand sender)
        {
            // Nop: no operation here
        }

        internal void CmdOpenLinkInExternalBrowser(ICommand sender)
        {
            owner.NavigateToUrlInExternalBrowser(UrlText);
        }

        internal void CmdToggleMainTBViewState(ICommand sender)
        {
            bool enable = owner.Mediator.IsChecked(sender);
            owner.Mediator.SetChecked(enable, "cmdToggleMainTBViewState");
            toolbarHelper.SetToolbarVisible(Resource.Toolbar.MainTools, enable);
        }

        internal void CmdToggleWebTBViewState(ICommand sender)
        {
            bool enable = owner.Mediator.IsChecked(sender);
            owner.Mediator.SetChecked(enable, "cmdToggleWebTBViewState");
            toolbarHelper.SetToolbarVisible(Resource.Toolbar.WebTools, enable);
        }

        internal void CmdToggleWebSearchTBViewState(ICommand sender)
        {
            bool enable = owner.Mediator.IsChecked(sender);
            owner.Mediator.SetChecked(enable, "cmdToggleWebSearchTBViewState");
            toolbarHelper.SetToolbarVisible(Resource.Toolbar.SearchTools, enable);
        }

        internal void CmdLauchDownloadManager(ICommand sender)
        {
            owner.LaunchDownloadManagerWindow();
        }

        /// <summary>
        /// Called before IG view menu tool dropdown.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Infragistics.Win.UltraWinToolbars.BeforeToolDropdownEventArgs"/> instance containing the event data.</param>
        private void OnToolbarBeforeToolDropdown(object sender, BeforeToolDropdownEventArgs e)
        {
            if (e.Tool.Key == "mnuViewToolbars")
            {
                owner.Mediator.SetChecked(toolbarHelper.IsToolbarVisible(Resource.Toolbar.MainTools),
                                          "cmdToggleMainTBViewState");
                owner.Mediator.SetChecked(toolbarHelper.IsToolbarVisible(Resource.Toolbar.WebTools),
                                          "cmdToggleWebTBViewState");
                owner.Mediator.SetChecked(toolbarHelper.IsToolbarVisible(Resource.Toolbar.SearchTools),
                                          "cmdToggleWebSearchTBViewState");
            }
            else if (e.Tool.Key == "cmdColumnChooserMain")
            {
                RefreshListviewColumnContextMenu();
            }
            else if (e.Tool.Key == "cmdBrowserGoBack" || e.Tool.Key == "cmdBrowserGoForward")
            {
                // we switch the dropdown chevron dynamically.
                // as a result, we now get only the before/afterdropdown events, but
                // not anymore the toolclick. So we simulate the toolClick it here:
                var cmd = e.Tool as AppPopupMenuCommand;
                if (cmd != null)
                {
                    if (cmd.Tools.Count == 0)
                    {
                        e.Cancel = true;
                        OnAnyToolbarToolClick(this, new ToolClickEventArgs(e.Tool, null));
                    }
                }
            }
        }

        internal void ToggleNavigationPaneView(NavigationPaneView pane)
        {
            if (!Navigator.Visible)
                OnNavigatorExpandImageClick(this, EventArgs.Empty);

            if (pane == NavigationPaneView.RssSearch)
            {
                Navigator.SelectedGroup = Navigator.Groups[Resource.NavigatorGroup.RssSearch];
				//owner.Mediator.SetChecked("+cmdToggleRssSearchTabState");
				//owner.Mediator.SetChecked("-cmdToggleTreeViewState");
            }
            else if (pane == NavigationPaneView.LastVisibleSubscription)
            {
				if (_lastVisualFeedSource != null &&
					owner.FeedSources.Contains(_lastVisualFeedSource))
				{
					string id = owner.FeedSources[_lastVisualFeedSource].ID.ToString();
					if (Navigator.Groups.Exists(id))
					{
						Navigator.SelectedGroup = Navigator.Groups[id];
						return;
					}
				}

				foreach (var o in Navigator.Groups)
				{
					if (o.Key == Resource.NavigatorGroup.RssSearch)
						continue;
					// select first non-rss-search pane:
					Navigator.SelectedGroup = Navigator.Groups[o.Key];
					break;
				}
                //Navigator.SelectedGroup = Navigator.Groups[Resource.NavigatorGroup.Subscriptions];
				//owner.Mediator.SetChecked("-cmdToggleRssSearchTabState");
				//owner.Mediator.SetChecked("+cmdToggleTreeViewState");
            }
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to view the feed subscriptions docked panel.
        internal void CmdDockShowSubscriptions(ICommand sender)
        {
			ToggleNavigationPaneView(NavigationPaneView.LastVisibleSubscription);
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to view the feed descriptions docked panel.
        internal void CmdDockShowRssSearch(ICommand sender)
        {
            ToggleNavigationPaneView(NavigationPaneView.RssSearch);
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to close the selected doc tab.
        private void CmdDocTabCloseSelected(ICommand sender)
        {
            Point pos = (_contextMenuCalledAt != Point.Empty ? _contextMenuCalledAt : Cursor.Position);
            var underMouse =
                _docContainer.GetLayoutSystemAt(_docContainer.PointToClient(pos)) as ControlLayoutSystem;
            if (underMouse != null)
            {
                DockControl docUnderMouse = underMouse.GetControlAt(_docContainer.PointToClient(pos));
                if (docUnderMouse != null)
                {
                    RemoveDocTab(docUnderMouse);
                    return;
                }
            }
            // try simply to remove current active:
            RemoveDocTab(_docContainer.ActiveDocument);
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to close all doc tabs on the current strip.
        private void CmdDocTabCloseAllOnStrip(ICommand sender)
        {
            Point pos = (_contextMenuCalledAt != Point.Empty ? _contextMenuCalledAt : Cursor.Position);
            var underMouse =
                _docContainer.GetLayoutSystemAt(_docContainer.PointToClient(pos)) as ControlLayoutSystem;
            if (underMouse == null)
                underMouse = _docContainer.ActiveDocument.LayoutSystem;

            var docs = new DockControl[underMouse.Controls.Count];
            underMouse.Controls.CopyTo(docs, 0); // prevent InvalidOpException on Collections
            foreach (var doc in docs)
            {
                var state = (ITabState) doc.Tag;
                if (state.CanClose)
                    _docContainer.RemoveDocument(doc);
            }
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to close all doc tabs on all strips.
        private void CmdDocTabCloseAll(ICommand sender)
        {
            var docs = new DockControl[_docContainer.Documents.Length];
            _docContainer.Documents.CopyTo(docs, 0); // prevent InvalidOpException on Collections
            foreach (var doc in docs)
            {
                var state = (ITabState) doc.Tag;
                if (state.CanClose)
                    _docContainer.RemoveDocument(doc);
            }
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to toggle the splitted doc strip layout.
        private void CmdDocTabLayoutHorizontal(ICommand sender)
        {
            if (owner.Mediator.IsChecked("cmdDocTabLayoutHorizontal"))
            {
                _docContainer.LayoutSystem.SplitMode = Orientation.Vertical;
            }
            else
            {
                _docContainer.LayoutSystem.SplitMode = Orientation.Horizontal;
            }
            owner.Mediator.SetChecked((_docContainer.LayoutSystem.SplitMode == Orientation.Horizontal),
                                      "cmdDocTabLayoutHorizontal");
        }

        internal void CmdViewOutlookReadingPane(ICommand sender)
        {
            if (sender != null)
            {
                // check the real command (sender) for current unified state:
                bool enable = owner.Mediator.IsChecked(sender);
                owner.Mediator.SetChecked(enable, "cmdViewOutlookReadingPane");
                ShowOutlookReadingPane(enable);
            }
        }

        private void ShowOutlookReadingPane(bool enable)
        {
            if (enable)
            {
                //Prepare ListView Contents
                var items = new ThreadedListViewItem[listFeedItems.Items.Count];
                int ind = 0;
                foreach (var lvi in listFeedItems.Items)
                {
                    items[ind++] = lvi;
                }
                listFeedItemsO.Clear();
                listFeedItemsO.AddRange(items);
                //
                listFeedItems.Visible = false;
                listFeedItemsO.Visible = true;
            }
            else
            {
                listFeedItems.Visible = true;
                listFeedItemsO.Visible = false;
                //
                listFeedItemsO.Clear();
            }
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to toggle the feed detail layout.
        internal void CmdFeedDetailLayoutPosTop(ICommand sender)
        {
            SetFeedDetailLayout(DockStyle.Top);
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to toggle the feed detail layout.
        internal void CmdFeedDetailLayoutPosBottom(ICommand sender)
        {
            SetFeedDetailLayout(DockStyle.Bottom);
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to toggle the feed detail layout.
        internal void CmdFeedDetailLayoutPosLeft(ICommand sender)
        {
            SetFeedDetailLayout(DockStyle.Left);
        }

        // not needed to be handled by RssBanditApplication.
        // Gets called, if a user want to toggle the feed detail layout.
        internal void CmdFeedDetailLayoutPosRight(ICommand sender)
        {
            SetFeedDetailLayout(DockStyle.Right);
        }

        #region CmdFlag... routines

        public void CmdFlagNewsItemForFollowUp(ICommand sender)
        {
            if (CurrentSelectedFeedItem != null)
            {
                MarkFeedItemsFlagged(Flagged.FollowUp);
            }
        }

        public void CmdFlagNewsItemNone(ICommand sender)
        {
            if (CurrentSelectedFeedItem != null)
            {
                MarkFeedItemsFlagged(Flagged.None);
            }
        }

        public void CmdFlagNewsItemComplete(ICommand sender)
        {
            if (CurrentSelectedFeedItem != null)
            {
                MarkFeedItemsFlagged(Flagged.Complete);
            }
        }

        public void CmdFlagNewsItemForward(ICommand sender)
        {
            if (CurrentSelectedFeedItem != null)
            {
                MarkFeedItemsFlagged(Flagged.Forward);
            }
        }

        public void CmdFlagNewsItemRead(ICommand sender)
        {
            if (CurrentSelectedFeedItem != null)
            {
                MarkFeedItemsFlagged(Flagged.Read);
            }
        }

        public void CmdFlagNewsItemForReply(ICommand sender)
        {
            if (CurrentSelectedFeedItem != null)
            {
                MarkFeedItemsFlagged(Flagged.Reply);
            }
        }

        public void CmdFlagNewsItemForReview(ICommand sender)
        {
            if (CurrentSelectedFeedItem != null)
            {
                MarkFeedItemsFlagged(Flagged.Review);
            }
        }

        #endregion

        #region CmdCopyNewsItemXXX and CmdCopyFeedXXX to Clipboard

        private void CmdCopyFeed(ICommand sender)
        {
            // dummy, just a submenu
            if (sender is AppContextMenuCommand)
                CurrentSelectedFeedsNode = null;
        }

        private void CmdCopyFeedLinkToClipboard(ICommand sender)
        {
            TreeFeedsNodeBase feedsNode = CurrentSelectedFeedsNode;
            if (feedsNode != null && feedsNode.Type == FeedNodeType.Feed && feedsNode.DataKey != null)
            {
                ClipboardHelper.SetString(feedsNode.DataKey, true);
            }

            if (sender is AppContextMenuCommand) // needed at the treeview
                CurrentSelectedFeedsNode = null;
        }

        private void CmdCopyFeedHomeLinkToClipboard(ICommand sender)
        {
            TreeFeedsNodeBase feedsNode = CurrentSelectedFeedsNode;
            if (feedsNode != null && feedsNode.Type == FeedNodeType.Feed)
            {
                IFeedDetails fd = owner.GetFeedDetails(FeedSourceEntryOf(feedsNode), feedsNode.DataKey);

                string link = fd != null ? fd.Link : feedsNode.DataKey;

                if (!string.IsNullOrEmpty(link))
                {
                    ClipboardHelper.SetString(link, true);
                }
            }

            if (sender is AppContextMenuCommand) // needed at the treeview
                CurrentSelectedFeedsNode = null;
        }

        private void CmdCopyFeedHomeTitleLinkToClipboard(ICommand sender)
        {
            TreeFeedsNodeBase feedsNode = CurrentSelectedFeedsNode;
            if (feedsNode != null && feedsNode.Type == FeedNodeType.Feed)
            {

				IFeedDetails fd = owner.GetFeedDetails(FeedSourceEntryOf(feedsNode), feedsNode.DataKey);
				string link, title;

                if (fd != null)
                {
                    link = fd.Link;
                    title = fd.Title;
                }
                else
                {
                    link = feedsNode.DataKey;
                    title = feedsNode.Text;
                }

                if (!string.IsNullOrEmpty(link))
                {
                    string data = String.Format("<a href=\"{0}\" title=\"{1}\">{2}</a>", link, title,feedsNode.Text);
                    ClipboardHelper.SetStringAndHtml(data, data, true);
                }
            }
            if (sender is AppContextMenuCommand) // needed at the treeview
                CurrentSelectedFeedsNode = null;
        }

        private static void CmdCopyNewsItem(ICommand sender)
        {
            // dummy, just a submenu
        }

        private void CmdCopyNewsItemLinkToClipboard(ICommand sender)
        {
            if (listFeedItems.SelectedItems.Count == 0)
                return;

            var data = new StringBuilder();
            for (int i = 0; i < listFeedItems.SelectedItems.Count; i++)
            {
                var item = (INewsItem) ((ThreadedListViewItem) listFeedItems.SelectedItems[i]).Key;

                if (item != null)
                {
                    string link = item.Link;
                    if (!string.IsNullOrEmpty(link))
                    {
                        data.AppendFormat("{0}{1}", (i > 0 ? Environment.NewLine : String.Empty), link);
                    }
                }
            }

            if (data.Length > 0)
                ClipboardHelper.SetString(data.ToString(), true);
        }

        private void CmdCopyNewsItemTitleLinkToClipboard(ICommand sender)
        {
            if (listFeedItems.SelectedItems.Count == 0)
                return;

            var data = new StringBuilder();
            var links = new StringBuilder();
            for (int i = 0; i < listFeedItems.SelectedItems.Count; i++)
            {
                var item = (INewsItem) ((ThreadedListViewItem) listFeedItems.SelectedItems[i]).Key;

                if (item != null)
                {
                    string link = item.Link;
                    if (!string.IsNullOrEmpty(link))
                    {
                        string title = item.Title;
                        if (!string.IsNullOrEmpty(title))
                        {
                            data.AppendFormat("{0}<a href=\"{1}\" title=\"{2}\">{3}</a>",
                                              (i > 0 ? "<br />" + Environment.NewLine : String.Empty), link, title,
                                              title);
                        }
                        else
                        {
                            data.AppendFormat("{0}<a href=\"{1}\">{2}</a>",
                                              (i > 0 ? "<br />" + Environment.NewLine : String.Empty), link, link);
                        }
                    }
                    links.AppendFormat("{0}{1}", (i > 0 ? Environment.NewLine : String.Empty), link);
                }
            }

            if (data.Length > 0)
                ClipboardHelper.SetStringAndHtml(data.ToString(), data.ToString(), true);
        }

        private void CmdCopyNewsItemContentToClipboard(ICommand sender)
        {
            if (listFeedItems.SelectedItems.Count == 0)
                return;

            var data = new StringBuilder();
            for (int i = 0; i < listFeedItems.SelectedItems.Count; i++)
            {
                var item = (INewsItem) ((ThreadedListViewItem) listFeedItems.SelectedItems[i]).Key;

                if (item != null)
                {
                    string link = item.Link;
                    if (string.IsNullOrEmpty(link))
                        link = item.Feed.link;
                    string content = item.Content;
                    string itemUrl = String.Empty;

                    if (!string.IsNullOrEmpty(link))
                    {
                        string title = item.Title;
                        if (string.IsNullOrEmpty(title))
                            title = link;
                        itemUrl = String.Format("<a href=\"{0}\" title=\"{1}\">{2}</a>",
                                                link, item.Feed.title, title);
                    }

                    if (i > 0)
                        data.Append("<hr />");

                    data.Append("<div>");
                    if (!string.IsNullOrEmpty(content))
                    {
                        data.AppendFormat("{0}<br />[{1}]", content, itemUrl);
                    }
                    else
                    {
                        data.AppendFormat("{0} -> {1}", item.Feed.title, itemUrl);
                    }
                    data.Append("</div>");
                }
            }

            if (data.Length > 0)
                ClipboardHelper.SetHtml(data.ToString(), true);
        }

        #endregion

        #region CmdFinder.. routines

        /// <summary>
        /// Re-runs the search and repopulates the search folder.
        /// </summary>
        /// <remarks>Assumes that this is called when the current selected node is a search folder</remarks>
        /// <param name="sender"></param>
        private void CmdRefreshFinder(ICommand sender)
        {
            EmptyListView();
            htmlDetail.Clear();
            var afn = TreeSelectedFeedsNode as FinderNode;
            if (afn != null)
            {
                afn.Clear();
                UpdateTreeNodeUnreadStatus(afn, 0);
                if (afn.Finder != null && !string.IsNullOrEmpty(afn.Finder.ExternalSearchUrl))
                {
                    // does also initiates the local search if merge is true:
                    AsyncStartRssRemoteSearch(afn.Finder.ExternalSearchPhrase, afn.Finder.ExternalSearchUrl,
                                              afn.Finder.ExternalResultMerged, true);
                }
                else
                {
                    AsyncStartNewsSearch(afn);
                }
            }
        }


        /// <summary>
        /// Marks all the items in a search folder as read
        /// </summary>
        /// <param name="sender"></param>
        private void CmdMarkFinderItemsRead(ICommand sender)
        {
			//TODO: this have to be changed!
            SetFeedItemsReadState(listFeedItems.Items, true);
            UpdateTreeStatus(owner.FeedHandler.GetFeeds());
        }

        /// <summary>
        /// Renames a search folder
        /// </summary>
        /// <param name="sender"></param>
        private void CmdRenameFinder(ICommand sender)
        {
            if (CurrentSelectedFeedsNode != null)
                DoEditTreeNodeLabel();
        }

        /// <summary>
        /// Allows the user to create a new search folder
        /// </summary>
        /// <param name="sender"></param>
        private void CmdNewFinder(ICommand sender)
        {
            CmdNewRssSearch(sender);
        }

        /// <summary>
        /// Deletes a search folder
        /// </summary>
        /// <param name="sender"></param>
        private void CmdDeleteFinder(ICommand sender)
        {
            if (owner.MessageQuestion(SR.MessageBoxDeleteThisFinderQuestion) == DialogResult.Yes)
            {
                if (NodeEditingActive)
                    return;

                TreeFeedsNodeBase feedsNode = CurrentSelectedFeedsNode;
                WalkdownThenDeleteFinders(feedsNode);
                UpdateTreeNodeUnreadStatus(feedsNode, 0);

                try
                {
                    feedsNode.Parent.Nodes.Remove(feedsNode);
                }
                catch
                {
                }

                if (sender is AppContextMenuCommand)
                    CurrentSelectedFeedsNode = null;
            }
        }

        private void CmdFinderToggleExcerptsFullItemText(ICommand sender)
        {
            TreeFeedsNodeBase feedsNode = CurrentSelectedFeedsNode;
            if (feedsNode != null && feedsNode is FinderNode)
            {
                var fn = (FinderNode) feedsNode;
                fn.Finder.ShowFullItemContent = !fn.Finder.ShowFullItemContent;
                fn.Clear();
                UpdateTreeNodeUnreadStatus(fn, 0);
                EmptyListView();
                htmlDetail.Clear();
                AsyncStartNewsSearch(fn);
            }
        }

        /// <summary>
        /// Helper. Work recursive on the startNode down to the leaves.
        /// Then delete all child categories and FeedNode refs in owner.FeedHandler.
        /// </summary>
        /// <param name="startNode">new full category name (long name, with all the '\').</param>
        private void WalkdownThenDeleteFinders(TreeFeedsNodeBase startNode)
        {
            if (startNode == null) return;

            if (startNode.Type == FeedNodeType.Finder)
            {
                var agn = startNode as FinderNode;
                if (agn != null)
                {
                    owner.FinderList.Remove(agn.Finder);
                }
            }
            else
            {
                // other
                for (TreeFeedsNodeBase child = startNode.FirstNode; child != null; child = child.NextNode)
                {
                    WalkdownThenDeleteFinders(child);
                }
            }
        }

        private void CmdDeleteAllFinder(ICommand sender)
        {
            if (owner.MessageQuestion(SR.MessageBoxDeleteAllFindersQuestion) == DialogResult.Yes)
            {
                owner.FinderList.Clear();
                owner.SaveSearchFolders();

                var finderRoot = GetRoot(RootFolderType.Finder) as FinderRootNode;

                if (finderRoot != null)
                {
                    finderRoot.Nodes.Clear();
                    UpdateTreeNodeUnreadStatus(finderRoot, 0);
                }
            }

            if (sender is AppContextMenuCommand)
                CurrentSelectedFeedsNode = null;
        }

        private void CmdShowFinderProperties(ICommand sender)
        {
            CmdDockShowRssSearch(null);

            var node = CurrentSelectedFeedsNode as FinderNode;
            if (node != null)
            {
                searchPanel.SearchDialogSetSearchCriterias(node);
            }

            if (sender is AppContextMenuCommand)
                CurrentSelectedFeedsNode = null;
        }

        private void CmdSubscribeToFinderResult(ICommand sender)
        {
            var node = CurrentSelectedFeedsNode as FinderNode;
            if (node != null && node.Finder != null)
            {
                if (!string.IsNullOrEmpty(node.Finder.ExternalSearchUrl))
                {
                    owner.CmdNewFeed(node.Text, node.Finder.ExternalSearchUrl, node.Finder.ExternalSearchPhrase);
                }
            }
        }

        #endregion

        #region CmdListview: Column Layout, selection

        internal void CmdColumnChooserUseFeedLayoutGlobal(ICommand sender)
        {
            SetGlobalFeedColumnLayout(FeedNodeType.Feed, listFeedItems.FeedColumnLayout);
            listFeedItems.ApplyLayoutModifications();
        }

        internal void CmdColumnChooserUseCategoryLayoutGlobal(ICommand sender)
        {
            SetGlobalFeedColumnLayout(FeedNodeType.Category, listFeedItems.FeedColumnLayout);
            listFeedItems.ApplyLayoutModifications();
        }

        internal void CmdColumnChooserResetToDefault(ICommand sender)
        {
            SetFeedColumnLayout(CurrentSelectedFeedsNode, null);
            listFeedItems.ApplyLayoutModifications(); // do not save temp. changes to the node
            IList<INewsItem> items = NewsItemListFrom(listFeedItems.Items);
            listFeedItems.FeedColumnLayout = GetFeedColumnLayout(CurrentSelectedFeedsNode); // also clear's the listview
            RePopulateListviewWithContent(items);
        }

        private void CmdDownloadAttachment(ICommand sender)
        {
            string fileName = sender.CommandID.Split(new[] {'<'})[1];
            INewsItem item = CurrentSelectedFeedItem;
            FeedSource source = FeedSourceOf(CurrentSelectedFeedsNode); 

            try
            {
                if (item != null)
                {                    
                    source.DownloadEnclosure(item, fileName);
                }
            }
            catch (DownloaderException de)
            {
                MessageBox.Show(de.Message, SR.ExceptionEnclosureDownloadError, MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        internal void CmdToggleListviewColumn(ICommand sender)
        {
            if (listFeedItems.Columns.Count > 1)
            {
                // show at least one column
                string[] name = sender.CommandID.Split(new[] {'.'});

                bool enable = owner.Mediator.IsChecked(sender);
                owner.Mediator.SetChecked(enable, sender.CommandID);

                if (!enable)
                {
                    listFeedItems.Columns.Remove(name[1]);
                }
                else
                {
                    AddListviewColumn(name[1], 120);
                    RePopulateListviewWithCurrentContent();
                }

                listFeedItems.CheckForLayoutModifications();
            }
        }

		private void CmdRenameFeedSource(ICommand sender)
		{
			TreeFeedsNodeBase root = CurrentSelectedFeedsNode; 
			if (root != null && !(root is SubscriptionRootNode))
				root = root.RootNode as TreeFeedsNodeBase;
			if (root != null)
			{
				if (treeFeeds.ActiveNode != root)
					treeFeeds.ActiveNode = root;
				root.BeginEdit();
			}
		}

		private void CmdDeleteFeedSource(ICommand sender)
		{
            FeedSourceEntry entry = CurrentSelectedFeedSource;             
			
			if (entry != null && DialogResult.Yes == owner.MessageQuestion(
										String.Format(SR.MessageBoxDeleteThisFeedSourceQuestion, entry.Name),
										SR.MessageBoxDeleteThisFeedSourceCaption))
			{				
					owner.RemoveFeedSource(entry);		
			}
		}

		private void CmdShowFeedSourceProperties(ICommand sender)
		{
            FeedSourceEntry entry = CurrentSelectedFeedSource; 
			if (entry != null)
			{
				using (FeedSourceProperties dialog = new FeedSourceProperties(entry))
				{
					if (DialogResult.Cancel != dialog.ShowDialog(this))
					{
						entry = owner.ChangeFeedSource(entry, dialog.FeedSoureName, dialog.Username, dialog.Password);
						
                        SubscriptionRootNode root = this.GetSubscriptionRootNode(entry);
                        root.Text = entry.Name;
						Navigator.SelectedGroup.Text = entry.Name;
						
					}
				}
			}
		}

    	private void RefreshListviewColumnContextMenu()
        {
            ColumnKeyIndexMap map = listFeedItems.Columns.GetColumnIndexMap();

            foreach (var colID in Enum.GetNames(typeof (NewsItemSortField)))
            {
                owner.Mediator.SetChecked(map.ContainsKey(colID), "cmdListviewColumn." + colID);
            }

            bool enableIndividual = (CurrentSelectedFeedsNode != null &&
                                     (CurrentSelectedFeedsNode.Type == FeedNodeType.Feed ||
                                      CurrentSelectedFeedsNode.Type == FeedNodeType.Category));
            owner.Mediator.SetEnabled(enableIndividual, "cmdColumnChooserResetToDefault");
        }

        private void AddListviewColumn(string colID, int width)
        {
            switch (colID)
            {
                case "Title":
                    listFeedItems.Columns.Add(colID, SR.ListviewColumnCaptionHeadline, typeof (string), width,
                                              HorizontalAlignment.Left);
                    break;
                case "Subject":
                    listFeedItems.Columns.Add(colID, SR.ListviewColumnCaptionTopic, typeof (string), width,
                                              HorizontalAlignment.Left);
                    break;
                case "Date":
                    listFeedItems.Columns.Add(colID, SR.ListviewColumnCaptionDate, typeof (DateTime), width,
                                              HorizontalAlignment.Left);
                    break;
                case "FeedTitle":
                    listFeedItems.Columns.Add(colID, SR.ListviewColumnCaptionFeedTitle, typeof (string), width,
                                              HorizontalAlignment.Left);
                    break;
                case "Author":
                    listFeedItems.Columns.Add(colID, SR.ListviewColumnCaptionCreator, typeof (string), width,
                                              HorizontalAlignment.Left);
                    break;
                case "CommentCount":
                    listFeedItems.Columns.Add(colID, SR.ListviewColumnCaptionCommentCount, typeof (int), width,
                                              HorizontalAlignment.Left);
                    break;
                case "Enclosure":
                    //TODO: should have a paperclip picture, int type may change to a specific state (string)
                    listFeedItems.Columns.Add(colID, SR.ListviewColumnCaptionEnclosure, typeof (int), width,
                                              HorizontalAlignment.Left);
                    break;
                case "Flag":
                    listFeedItems.Columns.Add(colID, SR.ListviewColumnCaptionFlagStatus, typeof (string), width,
                                              HorizontalAlignment.Left);
                    break;
                default:
                    Trace.Assert(false, "AddListviewColumn::NewsItemSortField NOT handled: " + colID);
                    break;
            }
        }

        private void ResetFeedDetailLayoutCmds()
        {
            owner.Mediator.SetChecked(false, "cmdFeedDetailLayoutPosTop", "cmdFeedDetailLayoutPosLeft",
                                      "cmdFeedDetailLayoutPosRight", "cmdFeedDetailLayoutPosBottom");
        }


        internal void CmdFeedDetailTextSizeSmallest(ICommand sender)
        {
            SetFeedDetailTextSize(TextSize.Smallest);
        }

        internal void CmdFeedDetailTextSizeSmaller(ICommand sender)
        {
            SetFeedDetailTextSize(TextSize.Smaller);
        }


        internal void CmdFeedDetailTextSizeMedium(ICommand sender)
        {
            SetFeedDetailTextSize(TextSize.Medium);
        }


        internal void CmdFeedDetailTextSizeLarger(ICommand sender)
        {
            SetFeedDetailTextSize(TextSize.Larger);
        }


        internal void CmdFeedDetailTextSizeLargest(ICommand sender)
        {
            SetFeedDetailTextSize(TextSize.Largest);
        }

        private void SetFeedDetailTextSize(TextSize size)
        {
            try
            {
                var z = (int) owner.Preferences.ReadingPaneTextSize;

                switch (size)
                {
                    case TextSize.Smallest:
                        z = 0;
                        break;
                    case TextSize.Smaller:
                        z = 1;
                        break;
                    case TextSize.Medium:
                        z = 2;
                        break;
                    case TextSize.Larger:
                        z = 3;
                        break;
                    case TextSize.Largest:
                        z = 4;
                        break;
                }

                object Z = z;
                var NULL = new Object();
                htmlDetail.ExecWB(OLECMDID.OLECMDID_ZOOM, OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER, ref Z, ref NULL);

                owner.Preferences.ReadingPaneTextSize = size;
                owner.Mediator.SetChecked(false, "cmdFeedDetailTextSizeLargest", "cmdFeedDetailTextSizeLarger",
                                          "cmdFeedDetailTextSizeMedium", "cmdFeedDetailTextSizeSmaller",
                                          "cmdFeedDetailTextSizeSmallest");
                owner.Mediator.SetChecked(true, "cmdFeedDetailTextSize" + owner.Preferences.ReadingPaneTextSize);
            }
            catch (Exception e)
            {
                _log.Error("Exception while changing reading pane text size", e);
            }
        }

        private void SetFeedDetailLayout(DockStyle style)
        {
            ResetFeedDetailLayoutCmds();
            panelFeedItems.Dock = style;
            if (style == DockStyle.Left || style == DockStyle.Right)
            {
                detailsPaneSplitter.Dock = style; // allowed styles
                detailsPaneSplitter.Cursor = Cursors.VSplit;
                panelFeedItems.Width = panelFeedDetails.Width/3;
            }
            else if (style == DockStyle.Bottom || style == DockStyle.Top)
            {
                detailsPaneSplitter.Dock = style; // allowed styles
                detailsPaneSplitter.Cursor = Cursors.HSplit;
                panelFeedItems.Height = panelFeedDetails.Height/2;
            }
            // TR - just for test with dockstyle.none:
            //panelWebDetail.Visible = detailsPaneSplitter.Visible = (style != DockStyle.None);
            owner.Mediator.SetChecked(true, "cmdFeedDetailLayoutPos" + detailsPaneSplitter.Dock);
        }

        /// <summary>
        /// Select all items of the Feeds ListView.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdSelectAllNewsItems(ICommand sender)
        {
            for (int i = 0; i < listFeedItems.Items.Count; i++)
            {
                listFeedItems.Items[i].Selected = true;
            }
            listFeedItems.Select();
        }

        #endregion

        #region CmdBrowserHistoryItem commands

        private void OnHistoryNavigateGoBackItemClick(object sender, HistoryNavigationEventArgs e)
        {
            NavigateToHistoryEntry(_feedItemImpressionHistory.GetPreviousAt(e.Index));
        }

        private void OnHistoryNavigateGoForwardItemClick(object sender, HistoryNavigationEventArgs e)
        {
            NavigateToHistoryEntry(_feedItemImpressionHistory.GetNextAt(e.Index));
        }

        #endregion

        #region CmdFeed commands

        /// <summary>
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdDeleteFeed(ICommand sender)
        {
            if (NodeEditingActive)
                return;

            // right-click selected:
            TreeFeedsNodeBase tn = CurrentSelectedFeedsNode;
            if (tn == null) return;
            if (tn.Type != FeedNodeType.Feed) return;

            if (DialogResult.Yes == owner.MessageQuestion(
                                        SR.MessageBoxDeleteThisFeedQuestion,
                                        String.Format(" - {0} ({1})", SR.MenuDeleteThisFeedCaption, tn.Text)))
            {
                // raise the OnFeedDeleted event (where we really remove the node):
				SubscriptionRootNode root = (SubscriptionRootNode)TreeHelper.ParentRootNode(tn);
				owner.DeleteFeed(owner.FeedSources[root.SourceID], tn.DataKey);

                if (sender is AppContextMenuCommand)
                    CurrentSelectedFeedsNode = null;


                //select next node in tree view
                //				this.treeFeeds.ActiveNode.Selected = true; 
                //				this.CurrentSelectedFeedsNode = this.treeFeeds.ActiveNode as TreeFeedsNodeBase;
                //				this.RefreshFeedDisplay(this.CurrentSelectedFeedsNode, true); 				
            }
        }

        #endregion

        private ITabState FeedDetailTabState
        {
            get { return (ITabState) _docFeedDetails.Tag; }
        }

        private bool RemoveDocTab(DockControl doc)
        {
            if (doc == null)
                doc = _docContainer.ActiveDocument;

            if (doc == null)
                return false;

            var state = doc.Tag as ITabState;
            if (state != null && state.CanClose)
            {
                try
                {
                    _docContainer.RemoveDocument(doc);
                    var browser = doc.Controls[0] as HtmlControl;
                    if (browser != null)
                    {
                        browser.Tag = null; // remove ref to containing doc
                    	DetachEvents(browser, true);
                        browser.Navigate("about:blank"); /* prevents media from continuing to play */ 
						// browser.Dispose(); - see http://support.microsoft.com/kb/948838 for why we suprress finalization
                        System.Reflection.FieldInfo fi = typeof(System.Windows.Forms.AxHost).GetField("oleSite", 
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        object o = fi.GetValue(browser);
                        GC.SuppressFinalize(o);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("_docContainer.RemoveDocument(doc) caused exception", ex);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called on generic listview commands and used for calling Addin-methods. 
        /// </summary>
        /// <param name="index">Index of the command. Points directly to the
        /// plugin within the arraylist</param>
        /// <param name="hasConfig">true, if we have to call config dialog</param>
        public void OnGenericListviewCommand(int index, bool hasConfig)
        {
            IBlogExtension ibe = blogExtensions[index];
            if (hasConfig)
            {
                try
                {
                    ibe.Configure(this);
                }
                catch (Exception e)
                {
                    _log.Error("IBlogExtension configuration exception", e);
                    owner.MessageError(String.Format(SR.ExceptionIBlogExtensionFunctionCall, "Configure()", e.Message));
                }
            }
            else
            {
                //if (ibe.HasEditingGUI) //TODO...? What we have to do here...?
                if (CurrentSelectedFeedItem != null)
                {
                    try
                    {
                        ibe.BlogItem(CurrentSelectedFeedItem, false);
                    }
                    catch (Exception e)
                    {
                        _log.Error("IBlogExtension command exception", e);
                        owner.MessageError(String.Format(SR.ExceptionIBlogExtensionFunctionCall, "BlogItem()", e.Message));
                    }
                }
            }
        }

        private void OnFormHandleCreated(object sender, EventArgs e)
        {
            // if the form is started minimized (via Shortcut Properties "Run-Minimized"
            // it seems the OnFormLoad event does not gets fired, so we call it here...
            if (InitialStartupState == FormWindowState.Minimized)
                OnFormLoad(this, new EventArgs());
            // init idle task event handler:
            Application.Idle += OnApplicationIdle;
        }

        private void OnFormLoad(object sender, EventArgs eva)
        {
            // do not display the ugly form init/resizing...
            Win32.ShowWindow(Handle, Win32.ShowWindowStyles.SW_HIDE);

            _uiTasksTimer.Tick += OnTasksTimerTick;

            //			InitDrawFilters();

            LoadUIConfiguration();
            SetTitleText(String.Empty);
            SetDetailHeaderText(null);

            InitSearchEngines();
            CheckForAddIns();
            InitFilter();

			// feedsources list was yet loaded, now make it visible:
			VisualizeFeedSources(owner.FeedSources.GetOrderedFeedSources());
			ToggleNavigationPaneView(NavigationPaneView.LastVisibleSubscription);

            SetGuiStateFeedback(SR.GUIStatusLoadingFeedlist);

            DelayTask(DelayedTasks.InitOnFinishLoading);
        }

        /// <summary>
        /// Provide the entry point to the delayed loading of the feed list
        /// </summary>
        private void OnFinishLoading()
        {
            if (owner.CommandLineArgs.StartInTaskbarNotificationAreaOnly || SystemTrayOnlyVisible)
            {
                // forced to show in Taskbar Notification Area
                Win32.ShowWindow(Handle, Win32.ShowWindowStyles.SW_HIDE);
            }
            else
            {
                Activate();
            }

            Splash.Status = SR.GUIStatusRefreshConnectionState;
            // refresh the Offline menu entry checked state
            owner.UpdateInternetConnectionState();

            // refresh the internal browser component, that does not know immediatly
            // about a still existing Offline Mode...
            Utils.SetIEOffline(owner.InternetConnectionOffline);

			RssBanditApplication.CheckAndInitSoundEvents();
            
			owner.CmdCheckForUpdates(AutoUpdateMode.OnApplicationStart);

			Splash.Close();
			
			// contains unread folder (required immediately, if a feed yet gets updated):
			PopulateTreeSpecialFeeds();
			
			// load the subscriptions of each feedsource:
        	owner.LoadAllFeedSourcesSubscriptions();

			#region code moved to OnAllFeedSourceSubscriptionsLoaded():

			//            //remember subscription tree state
//            ApplyAfterAllSubscriptionListsLoaded();

//            SetGuiStateFeedback(SR.GUIStatusDone);
			
//            //owner.BeginLoadingFeedlists();
//            //owner.BeginLoadingSpecialFeeds();

//            //Splash.Close();
//            owner.AskAndCheckForDefaultAggregator();

//            //Trace.WriteLine("ATTENTION!. REFRESH TIMER DISABLED FOR DEBUGGING!");
//#if !NOAUTO_REFRESH
//            // start the refresh timers
//            _timerRefreshFeeds.Start();
//            _timerRefreshCommentFeeds.Start();
			//#endif
			#endregion
		}

		void VisualizeFeedSources(IEnumerable<FeedSourceEntry> sources)
		{
			foreach (FeedSourceEntry source in sources)
			{
				CreateFeedSourceView(source, false);
				AddToSubscriptionTree(source);
			}
		}

        public void CreateFeedSourceView(FeedSourceEntry entry, bool onTop)
		{
			UltraExplorerBarContainerControl view = new UltraExplorerBarContainerControl();
			view.SuspendLayout();
			// this will be dynamically managed:
			//view.Controls.Add(this.treeFeeds);
			
			view.Location = new Point(1, 26);
			view.Name = "FeedSubscriptions." + entry.Name;
			this.helpProvider1.SetShowHelp(view, false);
			view.Size = new Size(228, 376);
			view.TabIndex = 0;

			this.Navigator.Controls.Add(view);

			UltraExplorerBarGroup group = new UltraExplorerBarGroup();
			group.Container = view;
			group.Key = entry.ID.ToString();
			group.Text = entry.Name;

			Infragistics.Win.Appearance small = new Infragistics.Win.Appearance();
			small.Image = Properties.Resources.subscriptions_folder_16;
			group.Settings.AppearancesSmall.HeaderAppearance = small;
			Infragistics.Win.Appearance large = new Infragistics.Win.Appearance();
			large.Image = Properties.Resources.subscriptions_folder_32;
			group.Settings.AppearancesLarge.HeaderAppearance = large;

			if (onTop)
			{
				this.Navigator.Groups.Insert(0, group);
			}
			else
			{
				// add ordered, but before the default "search" group:
				this.Navigator.Groups.Insert(this.Navigator.Groups.Count - 1, group);
			}
        	view.ResumeLayout(false); 
		}

		public void RemoveFeedSourceView(FeedSourceEntry entry)
		{
			var group =
					this.Navigator.Groups.Cast<UltraExplorerBarGroup>().FirstOrDefault(
						g =>
						{
							if (g.Key == entry.ID.ToString())
								return true;
							return false;
						});

			if (group != null)
			{
				this.Navigator.Groups.Remove(group);
			}
		}
		
		public void ApplyOrderToFeedSources(IEnumerable<FeedSourceEntry> entries)
		{
			foreach (FeedSourceEntry entry in entries)
			{
				var group =
					this.Navigator.Groups.Cast<UltraExplorerBarGroup>().FirstOrDefault(
						g =>
						{
							if (g.Key == entry.ID.ToString())
								return true;
							return false;
						});

				if (group != null)
					entry.Ordinal = group.Index;
			}
		}

        public void SelectFeedSource(FeedSourceEntry entry)
        {
            var group = from g in this.Navigator.Groups.Cast<UltraExplorerBarGroup>()
                        where g.Key == entry.ID.ToString() 
                        select g; 
			
            this.Navigator.SelectedGroup = group.FirstOrDefault();
        }

		internal void MaximizeNavigatorSelectedGroup()
		{
			if (this.Navigator.NavigationMaxGroupHeaders <= 0)
				this.Navigator.NavigationMaxGroupHeaders = 1;
		}

		public void AddToSubscriptionTree(FeedSourceEntry entry)
		{
			// create RootFolderType.MyFeeds:
			//TODO: Add feedsource specific context menu's?
			TreeFeedsNodeBase root = new SubscriptionRootNode(entry.ID, entry.Name, Resource.SubscriptionTreeImage.AllSubscriptions,
							 Resource.SubscriptionTreeImage.AllSubscriptionsExpanded, _subscriptionTreeRootContextMenu);
			treeFeeds.Nodes.Insert(0, root);
			root.Visible = false;
			root.ReadCounterZero += OnTreeNodeFeedsRootReadCounterZero;
		}

		public void RemoveFromSubscriptionTree(FeedSourceEntry entry)
		{
			SubscriptionRootNode toRemove = null;
			foreach (TreeFeedsNodeBase root in treeFeeds.Nodes)
			{
				SubscriptionRootNode n = root as SubscriptionRootNode;
				if (n!= null && n.SourceID == entry.ID)
				{
					toRemove = n;
					break;
				}
			}

			if (toRemove != null)
				treeFeeds.Nodes.Remove(toRemove);
		}

    	private void OnFormMove(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                _formRestoreBounds.Location = Location;
            }
        }

        private void OnFormResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                _formRestoreBounds.Size = Size;
            }
            if (WindowState != FormWindowState.Minimized)
            {
                // adjust the MaximumSize of the dock hosts:
                /*leftSandDock.MaximumSize = */
                rightSandDock.MaximumSize = ClientSize.Width - 20;
                topSandDock.MaximumSize = bottomSandDock.MaximumSize = ClientSize.Height - 20;
            }
            if (Visible)
            {
                SystemTrayOnlyVisible = false;
            }
        }

        /// <summary>
        /// Here is the Form minimize event handler
        /// </summary>
        /// <param name="sender">This form</param>
        /// <param name="e">Empty. See WndProc()</param>
        private void OnFormMinimize(object sender, EventArgs e)
        {
            if (owner.Preferences.HideToTrayAction == HideToTray.OnMinimize)
            {
                HideToSystemTray();
            }
        }


        /// <summary>
        /// Implements the IMessageFilter. 
        /// Helps grabbing all the important keys.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public virtual bool PreFilterMessage(ref Message m)
        {
            bool processed = false;

            try
            {
                if (m.Msg == (int) Win32.Message.WM_KEYDOWN ||
                    m.Msg == (int) Win32.Message.WM_SYSKEYDOWN)
                {
                    Keys msgKey = ((Keys) (int) m.WParam & Keys.KeyCode);
#if DEBUG
                    if (msgKey == Keys.F12)
                    {
                        IdleTask.AddTask(IdleTasks.IndexAllItems);

                        // to test tray animation:
                        //this._trayManager.SetState(ApplicationTrayState.NewUnreadFeedsReceived);
                    }
                    else
#endif
                        if ((ModifierKeys == Keys.Alt) && msgKey == Keys.F4)
                        {
                            if (owner.Preferences.HideToTrayAction == HideToTray.OnClose &&
                                _forceShutdown == false)
                            {
                                processed = true;
                                HideToSystemTray();
                            }
                        }
                        else if (msgKey == Keys.Tab)
                        {
                            if (ModifierKeys == 0)
                            {
                                // normal Tab navigation between controls

                                Trace.WriteLine("PreFilterMessage[Tab Only], ");

                                if (treeFeeds.Visible)
                                {
                                    if (treeFeeds.Focused)
                                    {
                                        if (listFeedItems.Visible)
                                        {
                                            listFeedItems.Focus();
                                            if (listFeedItems.Items.Count > 0 && listFeedItems.SelectedItems.Count == 0)
                                            {
                                                listFeedItems.Items[0].Selected = true;
                                                listFeedItems.Items[0].Focused = true;
                                                OnFeedListItemActivate(this, new EventArgs());
                                            }
                                            processed = true;
                                        }
                                        else if (_docContainer.ActiveDocument != _docFeedDetails)
                                        {
                                            // a tabbed browser should get focus
                                            SetFocus2WebBrowser((HtmlControl) _docContainer.ActiveDocument.Controls[0]);
                                            processed = true;
                                        }
                                    }
                                    else if (listFeedItems.Focused)
                                    {
                                        SetFocus2WebBrowser(htmlDetail); // detail browser should get focus
                                        processed = true;
                                    }
                                    else
                                    {
                                    }
                                }
                                else
                                {
                                    // treefeeds.invisible:
                                    if (listFeedItems.Visible)
                                    {
                                        if (listFeedItems.Focused)
                                        {
                                            SetFocus2WebBrowser(htmlDetail); // detail browser should get focus
                                            processed = true;
                                        }
                                        else
                                        {
                                            // a IE browser focused
                                            Trace.WriteLine("PreFilterMessage[Tab Only] IE browser focused?" +
                                                            htmlDetail.Focused);
                                        }
                                    }
                                } // endif treefeeds.visible 
                            }
                            else if ((ModifierKeys & Keys.Shift) == Keys.Shift &&
                                     (ModifierKeys & Keys.Control) == 0)
                            {
                                // Shift-Tab only

                                Trace.WriteLine("PreFilterMessage[Shift-Tab Only]");
                                if (treeFeeds.Visible)
                                {
                                    if (treeFeeds.Focused)
                                    {
                                        if (listFeedItems.Visible)
                                        {
                                            SetFocus2WebBrowser(htmlDetail); // detail browser should get focus
                                            processed = true;
                                        }
                                        else if (_docContainer.ActiveDocument != _docFeedDetails)
                                        {
                                            // a tabbed browser should get focus
                                            SetFocus2WebBrowser((HtmlControl) _docContainer.ActiveDocument.Controls[0]);
                                            processed = true;
                                        }
                                    }
                                    else if (listFeedItems.Focused)
                                    {
                                        treeFeeds.Focus();
                                        processed = true;
                                    }
                                    else
                                    {
                                        // a IE browser focused
                                        Trace.WriteLine("PreFilterMessage[Shift-Tab Only] IE browser focused?" +
                                                        htmlDetail.Focused);
                                    }
                                }
                                else
                                {
                                    // treefeeds.invisible:
                                    if (listFeedItems.Visible)
                                    {
                                        if (listFeedItems.Focused)
                                        {
                                            SetFocus2WebBrowser(htmlDetail); // detail browser should get focus
                                            processed = true;
                                        }
                                        else
                                        {
                                        }
                                    }
                                } //endif treefeeds.visible
                            }
                        }
                        else if (listFeedItems.Focused &&
                                 _shortcutHandler.IsCommandInvoked("ExpandListViewItem", m.WParam))
                        {
                            // "+" on ListView opens the thread
                            if (listFeedItems.Visible && listFeedItems.SelectedItems.Count > 0)
                            {
                                var lvi = (ThreadedListViewItem) listFeedItems.SelectedItems[0];
                                if (lvi.HasChilds && lvi.Collapsed)
                                {
                                    lvi.Expanded = true;
                                    processed = true;
                                }
                            }
                        }
                        else if (listFeedItems.Focused &&
                                 _shortcutHandler.IsCommandInvoked("CollapseListViewItem", m.WParam))
                        {
                            // "-" on ListView close the thread
                            if (listFeedItems.Visible && listFeedItems.SelectedItems.Count > 0)
                            {
                                var lvi = (ThreadedListViewItem) listFeedItems.SelectedItems[0];
                                if (lvi.HasChilds && lvi.Expanded)
                                {
                                    lvi.Collapsed = true;
                                    processed = true;
                                }
                            }
                        }
                        else if (_shortcutHandler.IsCommandInvoked("RemoveDocTab", m.WParam))
                        {
                            // Ctrl-F4: close a tab
                            if (RemoveDocTab(_docContainer.ActiveDocument))
                            {
                                processed = true;
                            }
                        }
                        else if (_shortcutHandler.IsCommandInvoked("CatchUpCurrentSelectedNode", m.WParam))
                        {
                            // Ctrl-Q: Catch up feed
                            owner.CmdCatchUpCurrentSelectedNode(null);
                            processed = true;
                        }
                        else if (_shortcutHandler.IsCommandInvoked("MarkFeedItemsUnread", m.WParam))
                        {
                            // Ctrl-U: close a tab
                            owner.CmdMarkFeedItemsUnread(null);
                            processed = true;

                            //We've hard-coded SPACE as a Move to Next Item
                            //But in that case, make sure there's not a modifier key pressed.
                        }
                        else if ((msgKey == Keys.Space && ModifierKeys == 0) ||
                                 _shortcutHandler.IsCommandInvoked("MoveToNextUnread", m.WParam))
                        {
                            // Space: move to next unread

                            if (listFeedItems.Focused || treeFeeds.Focused &&
                                                         !(TreeSelectedFeedsNode != null &&
                                                           TreeSelectedFeedsNode.IsEditing))
                            {
                                MoveToNextUnreadItem();
                                processed = true;
                            }
                            else if (searchPanel.ContainsFocus || treeFeeds.Focused ||
                                     UrlComboBox.Focused || SearchComboBox.Focused ||
                                     (CurrentSelectedFeedsNode != null && CurrentSelectedFeedsNode.IsEditing))
                            {
                                // ignore
                            }
                            else if (_docContainer.ActiveDocument == _docFeedDetails &&
                                     !listFeedItems.Focused)
                            {
                                // browser detail pane has focus
                                //Trace.WriteLine("htmlDetail.Focused:"+htmlDetail.Focused);
                                IHTMLDocument2 htdoc = htmlDetail.Document2;
                                if (htdoc != null)
                                {
                                    IHTMLElement2 htbody = htdoc.GetBody();
                                    if (htbody != null)
                                    {
                                        int num1 = htbody.getScrollTop();
                                        htbody.setScrollTop(num1 + 20);
                                        int num2 = htbody.getScrollTop();
                                        if (num1 == num2)
                                        {
                                            MoveToNextUnreadItem();
                                            processed = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // ignore, control should handle it
                            }
                        }
                        else if (_shortcutHandler.IsCommandInvoked("InitiateRenameFeedOrCategory", m.WParam))
                        {
                            // rename within treeview
                            if (treeFeeds.Focused)
                            {
                                InitiateRenameFeedOrCategory();
                                processed = true;
                            }
                        }
                        else if (_shortcutHandler.IsCommandInvoked("UpdateFeed", m.WParam))
                        {
                            // F5: UpdateFeed()
                            CurrentSelectedFeedsNode = null;
                            owner.CmdUpdateFeed(null);
                            processed = true;
                        }                     
                        else if (_shortcutHandler.IsCommandInvoked("GiveFocusToUrlTextBox", m.WParam))
                        {
                            // Alt+F4 or F11: move focus to Url textbox
                            UrlComboBox.Focus();
                            processed = true;
                        }
                        else if (_shortcutHandler.IsCommandInvoked("GiveFocusToSearchTextBox", m.WParam))
                        {
                            // F12: move focus to Search textbox
                            SearchComboBox.Focus();
                            processed = true;
                        }
                        else if ((msgKey == Keys.Delete && ModifierKeys == 0) ||
                                 _shortcutHandler.IsCommandInvoked("DeleteItem", m.WParam))
                        {
                            // Delete a feed or category,...
                            // cannot be a shortcut, because then "Del" does not work when edit/rename a node caption :-(
                            // But we can add alternate shortcuts via the config file.
                            if (treeFeeds.Focused && TreeSelectedFeedsNode != null &&
                                !TreeSelectedFeedsNode.IsEditing)
                            {
                                TreeFeedsNodeBase root = GetRoot(RootFolderType.MyFeeds);
                                TreeFeedsNodeBase current = CurrentSelectedFeedsNode;
                                if (NodeIsChildOf(current, root))
                                {
                                    if (current.Type == FeedNodeType.Category)
                                    {
                                        owner.CmdDeleteCategory(null);
                                        processed = true;
                                    }
                                    if (current.Type == FeedNodeType.Feed)
                                    {
                                        CmdDeleteFeed(null);
                                        processed = true;
                                    }
                                }
                            }
                        }
                }
                else if (m.Msg == (int) Win32.Message.WM_LBUTTONDBLCLK ||
                         m.Msg == (int) Win32.Message.WM_RBUTTONDBLCLK ||
                         m.Msg == (int) Win32.Message.WM_MBUTTONDBLCLK ||
                         m.Msg == (int) Win32.Message.WM_LBUTTONUP ||
                         m.Msg == (int) Win32.Message.WM_MBUTTONUP ||
                         m.Msg == (int) Win32.Message.WM_RBUTTONUP ||
                         m.Msg == (int) Win32.Message.WM_XBUTTONDBLCLK ||
                         m.Msg == (int) Win32.Message.WM_XBUTTONUP)
                {
                    _lastMousePosition = new Point(Win32.LOWORD(m.LParam), Win32.HIWORD(m.LParam));

                    Control mouseControl = wheelSupport.GetTopmostVisibleChild(this, MousePosition);
                    _webUserNavigated = (mouseControl is HtmlControl); // set
                    _webForceNewTab = false;
                    if (_webUserNavigated)
                    {
                        // CONTROL-Click opens a new Tab
                        _webForceNewTab = (IEControl.Interop.GetAsyncKeyState(IEControl.Interop.VK_CONTROL) < 0);
                    }
                }
                else if (m.Msg == (int) Win32.Message.WM_MOUSEMOVE)
                {
                    var p = new Point(Win32.LOWORD(m.LParam), Win32.HIWORD(m.LParam));
                    if (Math.Abs(p.X - _lastMousePosition.X) > 5 ||
                        Math.Abs(p.Y - _lastMousePosition.Y) > 5)
                    {
                        //Trace.WriteLine(String.Format("Reset mouse pos. Old: {0} New: {1}", _lastMousePosition, p));
                        _webForceNewTab = _webUserNavigated = false; // reset
                        _lastMousePosition = p;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("PreFilterMessage() failed", ex);
            }

#if TRACE_WIN_MESSAGES				
			if (m.Msg != (int)Win32.Message.WM_TIMER &&
				m.Msg != (int)Win32.Message.WM_MOUSEMOVE)
				Debug.WriteLine("PreFilterMessage(" + m +") handled: "+ processed);
#endif

            return processed;
        }

        /// <summary>
        /// we are interested in an OnMinimized event
        /// </summary>
        /// <param name="m">Native window message</param>
        [SecurityPermission(SecurityAction.LinkDemand)]
        protected override void WndProc(ref Message m)
        {
            try
            {
                if (m.Msg == (int) Win32.Message.WM_SIZE)
                {
                    if (((int) m.WParam) == 1 /*SIZE_MINIMIZED*/&& OnMinimize != null)
                    {
                        OnMinimize(this, EventArgs.Empty);
                    }
                    //			} else if (m.Msg == (int)Win32.Message.WM_MOUSEMOVE) {
                    //				Control ctrl =  this.GetChildAtPoint(this.PointToClient(MousePosition));
                    //				if (ctrl != null && !ctrl.Focused && ctrl.CanFocus) {
                    //					ctrl.Focus();
                    //				}
                }
                else if ( /* m.Msg == (int)WM_CLOSE || */ m.Msg == (int) Win32.Message.WM_QUERYENDSESSION ||
                                                          m.Msg == (int) Win32.Message.WM_ENDSESSION)
                {
                    // This is here to deal with dealing with system shutdown issues
                    // Read http://www.kuro5hin.org/story/2003/4/17/22853/6087#banditshutdown for details
                    // FYI: you could also do so:
                    // Microsoft.Win32.SystemEvents.SessionEnding += new SessionEndingEventHandler(this.OnSessionEnding);
                    // but we already have the WndProc(), so we also handle this message here

                    _forceShutdown = true; // the closing handler ask for that now
                    //this.SaveUIConfiguration(true);
                    owner.SaveApplicationState(true);
                }
                else if (m.Msg == (int) Win32.Message.WM_CLOSE &&
                         owner.Preferences.HideToTrayAction != HideToTray.OnClose)
                {
                    _forceShutdown = true; // the closing handler ask for that now
                    //this.SaveUIConfiguration(true);
                    owner.SaveApplicationState(true);
                }

                base.WndProc(ref m);
            }
            catch (Exception ex)
            {
                _log.Fatal("WndProc() failed with an exception", ex);
            }
        }

        private void OnFormMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.XButton1)
                RequestBrowseAction(BrowseAction.NavigateBack);
            else if (e.Button == MouseButtons.XButton2)
                RequestBrowseAction(BrowseAction.NavigateForward);
        }

        private void OnFormActivated(object sender, EventArgs e)
        {
            Application.AddMessageFilter(this);
            KeyPreview = true;
        }

        private void OnFormDeactivate(object sender, EventArgs e)
        {
            Application.RemoveMessageFilter(this);
            KeyPreview = false;
        }

        private void OnDocContainerShowControlContextMenu(object sender, ShowControlContextMenuEventArgs e)
        {
            _contextMenuCalledAt = Cursor.Position;
            _docTabContextMenu.Show(_docContainer, e.Position);
        }

        private void OnDocContainerMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (_docContainer.Visible)
                {
                    // we can only displ. the context menu on a visible control:
                    _contextMenuCalledAt = Cursor.Position;
                    _docTabContextMenu.Show(_docContainer, new Point(e.X, e.Y));
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                OnDocContainerDoubleClick(sender, e);
            }
        }

        private void OnDocContainerDocumentClosing(object sender, DocumentClosingEventArgs e)
        {
            RemoveDocTab(e.DockControl);
        }

        private void OnDocContainerActiveDocumentChanged(object sender, ActiveDocumentEventArgs e)
        {
            RefreshDocumentState(e.NewActiveDocument);
            DeactivateWebProgressInfo();
        }

        private void OnDocContainerDoubleClick(object sender, EventArgs e)
        {
            Point p = _docContainer.PointToClient(MousePosition);
            var lb = (DocumentLayoutSystem) _docContainer.GetLayoutSystemAt(p);
            if (lb != null)
            {
                DockControl doc = lb.GetControlAt(p);
                if (doc != null)
                    RemoveDocTab(doc);
            }
        }

        /// <summary>
        /// GUI State persistence. Settings are: window position, splitter position, 
        /// floating window sizes, listview column order, sorting direction etc.
        /// This routine writes all of them to a centralized settings dictionary maintained
        /// by the Settings class.
        /// </summary>
        /// <param name="writer"></param>
        protected void OnSaveConfig(Settings writer)
        {
            try
            {
                //NOTE: if we are here, consider that control state is not always
                // correct (at least the .Visible state can be wrong in case Bandit
                // was closed from the system tray icon - and the main form was not 
                // displayed)

                writer.SetProperty("version", 4);

                writer.SetProperty(Name + "/Bounds", BoundsToString(_formRestoreBounds));
                writer.SetProperty(Name + "/WindowState", (int) WindowState);

                // splitter position Listview/Detail Pane: 
                writer.SetProperty(Name + "/panelFeedItems.Height", panelFeedItems.Height);
                writer.SetProperty(Name + "/panelFeedItems.Width", panelFeedItems.Width);

                // splitter position Navigator/Feed Details Pane: 
                writer.SetProperty(Name + "/Navigator.Width", Navigator.Width);

				writer.SetProperty(Name + "/RecentFeedSource/Visible", _lastVisualFeedSource);

                writer.SetProperty(Name + "/docManager.WindowAlignment", (int) _docContainer.LayoutSystem.SplitMode);

                var sdRenderer = sandDockManager.Renderer as Office2003Renderer;
                writer.SetProperty(Name + "/dockManager.LayoutStyle.Office2003", (sdRenderer != null));

                // workaround the issue described here: http://www.divil.co.uk/net/support/kb/article.aspx?id=14
                //using (new CultureChanger("en-US")) 
                using (CultureChanger.InvariantCulture) //TR: SF bug 1532164
                {
                    writer.SetProperty(Name + "/dockManager.LayoutInfo", sandDockManager.GetLayout());
                }

                writer.SetProperty(Name + "/ToolbarsVersion", CurrentToolbarsVersion);
                writer.SetProperty(Name + "/Toolbars",
                                   StateSerializationHelper.SaveControlStateToString(ultraToolbarsManager, true));

#if USE_USE_UltraDockManager				
				writer.SetProperty(Name+"/DockingVersion", CurrentDockingVersion);
				writer.SetProperty(Name+"/Docks", StateSerializationHelper.SaveControlStateToString(this.ultraDockManager));
#endif
                writer.SetProperty(Name + "/ExplorerBarVersion", CurrentExplorerBarVersion);
                StateSerializationHelper.SaveExplorerBar(Navigator, writer, Name + "/" + Navigator.Name);
                writer.SetProperty(Name + "/" + Navigator.Name + "/Visible",
                                   owner.Mediator.IsChecked("cmdToggleTreeViewState") ||
                                   owner.Mediator.IsChecked("cmdToggleRssSearchTabState")
                    );

                writer.SetProperty(Name + "/feedDetail.LayoutInfo.Position", (int) detailsPaneSplitter.Dock);
                writer.SetProperty(Name + "/outlookView.Visible",
                                   owner.Mediator.IsChecked("cmdViewOutlookReadingPane")
                    );
            }
            catch (Exception ex)
            {
                _log.Error("Exception while writing config entries to .settings.xml", ex);
            }
        }

        /// <summary>
        /// GUI State persistence. Restore the control settings like window position,
        /// docked window states, toolbar button layout etc.
        /// </summary>
        /// <param name="reader"></param>
        protected void OnLoadConfig(Settings reader)
        {
            // do not init from stored settings on cmd line reset:
            if (owner.CommandLineArgs.ResetUserInterface)
                return;

            try
            {
                // controls if we should load layouts from user store, or not
                // version will/have to raise, if new toolbars/buttons are available in
                // newer delivered version(s)
                //int version = (int) reader.GetProperty("version", 0, typeof (int));

                // read BEFORE set the WindowState or Bounds (that causes events, where we reset this setting to false)
                //_initialStartupTrayVisibleOnly = reader.GetBoolean(Name+"/TrayOnly.Visible", false);

                Rectangle r = StringToBounds(reader.GetString(Name + "/Bounds", BoundsToString(Bounds)));
                if (r != Rectangle.Empty)
                {
                    if (Screen.AllScreens.Length < 2)
                    {
                        // if only one sreen, correct initial location to fit the screen
                        if (r.X < 0) r.X = 0;
                        if (r.Y < 0) r.Y = 0;
                        if (r.X >= Screen.PrimaryScreen.WorkingArea.Width)
                            r.X -= Screen.PrimaryScreen.WorkingArea.Width;
                        if (r.Y >= Screen.PrimaryScreen.WorkingArea.Height)
                            r.Y -= Screen.PrimaryScreen.WorkingArea.Height;
                    }
                    _formRestoreBounds = r;
                    SetBounds(r.X, r.Y, r.Width, r.Height, BoundsSpecified.All);
                }

                var windowState = (FormWindowState) reader.GetInt32(Name + "/WindowState",
                                                                    (int) WindowState);

                if (InitialStartupState != FormWindowState.Normal &&
                    WindowState != InitialStartupState)
                {
                    WindowState = InitialStartupState;
                }
                else
                {
                    WindowState = windowState;
                }

                var feedDetailLayout =
                    (DockStyle) reader.GetInt32(Name + "/feedDetail.LayoutInfo.Position", (int) DockStyle.Top);
                if (feedDetailLayout != DockStyle.Top && feedDetailLayout != DockStyle.Left &&
                    feedDetailLayout != DockStyle.Right && feedDetailLayout != DockStyle.Bottom)
                    feedDetailLayout = DockStyle.Top;
                SetFeedDetailLayout(feedDetailLayout); // load before restore panelFeedItems dimensions!

                // splitter position Listview/Detail Pane: 
                panelFeedItems.Height = reader.GetInt32(Name + "/panelFeedItems.Height", (panelFeedDetails.Height/2));
                panelFeedItems.Width = reader.GetInt32(Name + "/panelFeedItems.Width", (panelFeedDetails.Width/2));

                // splitter position Navigator/Feed Details Pane: 
                Navigator.Width = reader.GetInt32(Name + "/Navigator.Width", Navigator.Width);

                _docContainer.LayoutSystem.SplitMode =
                    (Orientation)
                    reader.GetInt32(Name + "/docManager.WindowAlignment", (int) _docContainer.LayoutSystem.SplitMode);
                owner.Mediator.SetChecked((_docContainer.LayoutSystem.SplitMode == Orientation.Horizontal),
                                          "cmdDocTabLayoutHorizontal");

                // fallback layouts if something really goes wrong while laading:

                // workaround the issue described here: http://www.divil.co.uk/net/support/kb/article.aspx?id=14
                using (CultureChanger.InvariantCulture)
                {
                    //TR: SF bug 1532164

                    string fallbackSDM = sandDockManager.GetLayout();

                    try
                    {
                        sandDockManager.SetLayout(reader.GetString(Name + "/dockManager.LayoutInfo", fallbackSDM));
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Exception on restore sandDockManager layout", ex);
                        sandDockManager.SetLayout(fallbackSDM);
                    }
                }

                bool office2003 = reader.GetBoolean(Name + "/dockManager.LayoutStyle.Office2003", true);
                if (office2003)
                {
                    var sdRenderer = new Office2003Renderer();
                    sdRenderer.ColorScheme = Office2003Renderer.Office2003ColorScheme.Automatic;
                    if (!RssBanditApplication.AutomaticColorSchemes)
                    {
                        sdRenderer.ColorScheme = Office2003Renderer.Office2003ColorScheme.Standard;
                    }
                    sandDockManager.Renderer = sdRenderer;
                    _docContainer.Renderer = sdRenderer;
                }
                else
                {
                    var sdRenderer = new WhidbeyRenderer();
                    sandDockManager.Renderer = sdRenderer;
                    _docContainer.Renderer = sdRenderer;
                }

                if (reader.GetString(Name + "/ExplorerBarVersion", "0") == CurrentExplorerBarVersion)
                {
                    StateSerializationHelper.LoadExplorerBar(Navigator, reader, Name + "/" + Navigator.Name);
                }

                if (!reader.GetBoolean(Name + "/" + Navigator.Name + "/Visible", true))
                {
                    OnNavigatorCollapseClick(this, EventArgs.Empty);
                }
                
				// remembering/startup with search panel is not a good app start UI state: 
            	_lastVisualFeedSource = reader.GetString(Name + "/RecentFeedSource/Visible", null);
				//if (Navigator.Visible)
				//    ToggleNavigationPaneView(NavigationPaneView.Subscriptions);

                if (reader.GetString(Name + "/ToolbarsVersion", "0") == CurrentToolbarsVersion)
                {
                    // Mediator re-connects to loaded commands:
                    StateSerializationHelper.LoadControlStateFromString(
                        ultraToolbarsManager, reader.GetString(Name + "/Toolbars", null),
                        owner.Mediator);

                    // restore container control references:
                    ultraToolbarsManager.Tools["cmdUrlDropdownContainer"].Control = UrlComboBox;
                    ultraToolbarsManager.Tools["cmdSearchDropdownContainer"].Control = SearchComboBox;

                    // restore the other dynamic menu handlers:
                    historyMenuManager.SetControls(
                        (AppPopupMenuCommand) ultraToolbarsManager.Tools["cmdBrowserGoBack"],
                        (AppPopupMenuCommand) ultraToolbarsManager.Tools["cmdBrowserGoForward"]);
                    owner.BackgroundDiscoverFeedsHandler.SetControls(
                        (AppPopupMenuCommand) ultraToolbarsManager.Tools["cmdDiscoveredFeedsDropDown"],
                        (AppButtonToolCommand) ultraToolbarsManager.Tools["cmdDiscoveredFeedsListClear"]);
                    InitSearchEngines();
                }

#if USE_USE_UltraDockManager				
				if (reader.GetString(Name+"/DockingVersion", "0") == CurrentDockingVersion) {
					StateSerializationHelper.LoadControlStateFromString(this.ultraDockManager, reader.GetString(Name+"/Docks", null));
				}
#endif
                //View Outlook View Reading Pane
                bool outlookView = reader.GetBoolean(Name + "/outlookView.Visible", false);
                owner.Mediator.SetChecked(outlookView, "cmdViewOutlookReadingPane");
                ShowOutlookReadingPane(outlookView);

                // now we can change the tool states:
                owner.Mediator.SetEnabled(
                    SearchIndexBehavior.NoIndexing != owner.FeedHandler.Configuration.SearchIndexBehavior,
                    "cmdNewRssSearch",
                    "cmdToggleRssSearchTabState");
            }
            catch (Exception ex)
            {
                _log.Error("Exception while loading .settings.xml", ex);
            }
        }


        private void OnFormClosing(object sender, CancelEventArgs e)
        {
            if (owner.Preferences.HideToTrayAction == HideToTray.OnClose &&
                _forceShutdown == false)
            {
                e.Cancel = true;
                HideToSystemTray();
            }
            else
            {
                _trayAni.Visible = false;
                toastNotifier.Dispose();
                _uiTasksTimer.Stop();
				owner.SaveFeedSources();
                SaveUIConfiguration(true);
            }
        }

        private bool SystemTrayOnlyVisible
        {
            get { return owner.GuiSettings.GetBoolean(Name + "/TrayOnly.Visible", false); }
            set { owner.GuiSettings.SetProperty(Name + "/TrayOnly.Visible", value); }
        }

        private void HideToSystemTray()
        {
            Win32.ShowWindow(Handle, Win32.ShowWindowStyles.SW_HIDE);
            if (WindowState != FormWindowState.Minimized)
                WindowState = FormWindowState.Minimized;
            SystemTrayOnlyVisible = true;
        }

        private void RestoreFromSystemTray()
        {
            Show();
            Win32.ShowWindow(Handle, Win32.ShowWindowStyles.SW_RESTORE);
            Win32.SetForegroundWindow(Handle);
            SystemTrayOnlyVisible = false;

            //if application was launced in SystemTrayOnlyVisible mode then we have to wait 
            //until now to restore browser tab state. 
            if (!_browserTabsRestored)
            {
                LoadAndRestoreBrowserTabState();
            }
        }

        internal void DoShow()
        {
            if (Visible)
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    Win32.ShowWindow(Handle, Win32.ShowWindowStyles.SW_RESTORE);
                    Win32.SetForegroundWindow(Handle);
                }
                else
                {
                    Activate();
                }
            }
            else
            {
                RestoreFromSystemTray();
            }
        }

        private void LoadUIConfiguration()
        {
            try
            {
                OnLoadConfig(owner.GuiSettings);
            }
            catch (Exception ex)
            {
                _log.Error("Load .settings.xml failed", ex);
            }
        }

        /// <summary>
        /// Called to build and re-build the search engine's Gui representation(s)
        /// </summary>
        public void InitSearchEngines()
        {
            if (!owner.SearchEngineHandler.EnginesLoaded || !owner.SearchEngineHandler.EnginesOK)
                owner.LoadSearchEngines();
            toolbarHelper.BuildSearchMenuDropdown(owner.SearchEngineHandler.Engines,
                                                  owner.Mediator, CmdExecuteSearchEngine);
            owner.Mediator.SetEnabled(owner.SearchEngineHandler.Engines.Count > 0, "cmdSearchGo");
        }

        /// <summary>
        /// Iterates through the treeview and highlights all feed titles that 
        /// have unread messages. 
        /// </summary>
        private void UpdateTreeStatus(IDictionary<string, INewsFeed> feedsTable)
        {
            UpdateTreeStatus(feedsTable, RootFolderType.MyFeeds);
        }

        private void UpdateTreeStatus(IDictionary<string, INewsFeed> feedsTable, RootFolderType rootFolder)
        {
            if (feedsTable == null) return;
            if (feedsTable.Count == 0) return;

            TreeFeedsNodeBase root = GetRoot(rootFolder);

            if (root == null) // no root nodes
                return;

            // traverse driven by feedsTable. Usually the feeds count with
            // new messages should be smaller than the tree nodes count.
            foreach (NewsFeed f in feedsTable.Values)
            {
                TreeFeedsNodeBase tn = TreeHelper.FindNode(root, f);
                if (f.containsNewMessages)
                {
                    UpdateTreeNodeUnreadStatus(tn, CountUnreadFeedItems(f));
                }
                else
                {
                    UpdateTreeNodeUnreadStatus(tn, 0);
                }
                Application.DoEvents(); //??
            }
        }

        #endregion

        #region event handlers for widgets not implementing ICommand

        private void OnTreeFeedMouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                var tv = (UltraTree) sender;
                var selectedNode = (TreeFeedsNodeBase) tv.GetNodeFromPoint(e.X, e.Y);

                if (e.Button == MouseButtons.Right)
                {
                    //if the right click was on a treeview node then display the 
                    //appropriate context node depending on whether it was over 
                    //a feed node, category node or the top-level node. 
                    if (selectedNode != null)
                    {
                        selectedNode.UpdateContextMenu();
                        // refresh context menu items
                        RefreshTreeFeedContextMenus(selectedNode);
                    }
                    else
                    {
                        tv.ContextMenu = null; // no context menu
                    }
                    CurrentSelectedFeedsNode = selectedNode;
                }
                else
                {
                    // cleanup temp node ref., needed if a user dismiss the context menu
                    // without selecting an action
                    if ((CurrentSelectedFeedsNode != null) && (selectedNode != null))
                    {
                        //this handles left click of currently selected feed after selecting
                        //an item in the listview. For some reason no afterselect or beforeselect
                        //events are fired so we do the work here. 
                        if (ReferenceEquals(CurrentSelectedFeedsNode, selectedNode))
                        {
                            // one more test, to prevent duplicate timeconsuming population of the listview/detail pane:
                            if (selectedNode.Type == FeedNodeType.Feed)
                            {
                                // if a feed was selected in the treeview, we display the feed homepage,
                                // not the feed url in the Url dropdown box:
								IFeedDetails fi = owner.GetFeedDetails(FeedSourceEntryOf(selectedNode), selectedNode.DataKey);
                                if (fi != null && fi.Link == FeedDetailTabState.Url)
                                    return; // no user navigation happened in listview/detail pane
                            }
                            else
                            {
                                // other node types does not set the FeedDetailTabState.Url
                                if (string.IsNullOrEmpty(FeedDetailTabState.Url))
                                    return;
                            }

                            OnTreeFeedAfterSelectManually(selectedNode);
                            //							this.listFeedItems.SelectedItems.Clear();
                            //							this.htmlDetail.Clear();
                            //							MoveFeedDetailsToFront();
                            //							this.AddHistoryEntry(selectedNode, null);
                            //							RefreshFeedDisplay(selectedNode, false);
                        }
                        else
                        {
                            CurrentSelectedFeedsNode = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Unexpected exception in OnTreeFeedMouseDown()", ex);
            }
        }

        private void OnTreeFeedMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                var t = (TreeFeedsNodeBase) treeFeeds.GetNodeFromPoint(e.X, e.Y);
                //TR: perf. impact:
                //				if (t == null) 
                //					treeFeeds.Cursor = Cursors.Default;
                //				else
                //					treeFeeds.Cursor = Cursors.Hand;
                if (CurrentDragNode != null)
                {
                    // this code does not have any effect :-((
                    // working around the missing DragHighlight property of the treeview :-(
                    if (t == null)
                        CurrentDragHighlightNode = null;

                    if (t != null)
                    {
                        CurrentDragHighlightNode = t.Type == FeedNodeType.Feed ? t.Parent : t;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Unexpected exception in OnTreeFeedMouseMove()", ex);
            }
        }

        private void OnTreeFeedMouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                CurrentDragHighlightNode = CurrentDragNode = null;

                if (e.Button == MouseButtons.Left)
                {
                    var tv = (UltraTree) sender;
                    var selectedNode = (TreeFeedsNodeBase) tv.GetNodeFromPoint(e.X, e.Y);

                    if (selectedNode != null && TreeSelectedFeedsNode == selectedNode)
                    {
                        SetTitleText(selectedNode.Text);
                        SetDetailHeaderText(selectedNode);
                        MoveFeedDetailsToFront();
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Unexpected exception in OnTreeFeedMouseUp()", ex);
            }
        }

        private void EmptyListView()
        {
            //lock(listFeedItems){
            if (listFeedItems.Items.Count > 0)
            {
                listFeedItems.BeginUpdate();
                listFeedItems.ListViewItemSorter = null;
                listFeedItems.Items.Clear();
                listFeedItems.EndUpdate();

                listFeedItemsO.Clear();
            }
            owner.Mediator.SetEnabled("-cmdFeedItemPostReply");
            //}
        }


        private void OnTreeFeedDoubleClick(object sender, EventArgs e)
        {
            try
            {
                Point point = treeFeeds.PointToClient(MousePosition);
                var feedsNode = (TreeFeedsNodeBase) treeFeeds.GetNodeFromPoint(point);

                if (feedsNode != null)
                {
                    CurrentSelectedFeedsNode = feedsNode;
                    owner.CmdNavigateFeedHome((ICommand) null);
                    CurrentSelectedFeedsNode = null;
                }
            }
            catch (Exception ex)
            {
                _log.Error("Unexpected Error in OnTreeFeedDoubleClick()", ex);
            }
        }


        private void OnTreeFeedBeforeSelect(object sender, BeforeSelectEventArgs e)
        {
            if (treeFeeds.SelectedNodes.Count == 0 || e.NewSelections.Count == 0)
            {
                e.Cancel = true;
                return;
            }
            if (ReferenceEquals(treeFeeds.SelectedNodes[0], e.NewSelections[0]))
            {
                return;
            }

            if (TreeSelectedFeedsNode != null)
            {
                listFeedItems.CheckForLayoutModifications();
                TreeFeedsNodeBase tn = TreeSelectedFeedsNode;
				FeedSourceEntry entry = FeedSourceEntryOf(tn);
                    
                if (tn.Type == FeedNodeType.Category)
                {
                    string category = tn.CategoryStoreName;

					if (entry.Source.GetCategoryMarkItemsReadOnExit(category) &&
                        !TreeHelper.IsChildNode(tn, (TreeFeedsNodeBase) e.NewSelections[0]))
                    {
                        MarkSelectedNodeRead(tn);
                        owner.SubscriptionModified(entry, NewsFeedProperty.FeedItemReadState);
                        //owner.FeedlistModified = true;
                    }
                }
                else if (tn.Type == FeedNodeType.Feed)
                {
                    string feedUrl = tn.DataKey;
                	INewsFeed f = owner.GetFeed(entry, feedUrl);

                    if (f != null && feedUrl != null && entry.Source.GetMarkItemsReadOnExit(feedUrl) &&
                        f.containsNewMessages)
                    {
                        MarkSelectedNodeRead(tn);
                        owner.SubscriptionModified(entry, NewsFeedProperty.FeedItemReadState);
                        //owner.FeedlistModified = true;
                        //this.UpdateTreeStatus(owner.FeedHandler.GetFeeds());					 
                    }
                }
            } //if(this.TreeSelectedNode != null){		
        }

        private void OnTreeFeedAfterSelect(object sender, SelectEventArgs e)
        {
            if (e.NewSelections.Count == 0)
            {
                return;
            }
            var tn = (TreeFeedsNodeBase) e.NewSelections[0];
            OnTreeFeedAfterSelectManually(tn);
        }

        private void OnTreeFeedAfterSelectManually(UltraTreeNode node)
        {
            try
            {
                var tn = (TreeFeedsNodeBase) node;

                if (tn.Type != FeedNodeType.Root)
                {
                    SetTitleText(tn.Text);
                    SetDetailHeaderText(tn);
                    MoveFeedDetailsToFront();
                }

                if (tn.Selected)
                {
                    if (tn.Type != FeedNodeType.Feed)
                    {
                        owner.Mediator.SetEnabled("-cmdFeedItemNewPost");
                    }
                    else
                    {
                        string feedUrl = tn.DataKey;
                    	FeedSourceEntry entry = FeedSourceEntryOf(tn);
						if (feedUrl != null && entry != null && 
							entry.Source.IsSubscribed(feedUrl))
                        {
                            owner.Mediator.SetEnabled(RssHelper.IsNntpUrl(feedUrl), "cmdFeedItemNewPost");
                        }
                        else
                        {
                            owner.Mediator.SetEnabled("-cmdFeedItemNewPost");
                        }
                    }

                    listFeedItems.FeedColumnLayout = GetFeedColumnLayout(tn); // raise events, that build the columns

                    switch (tn.Type)
                    {
                        case FeedNodeType.Feed:
                            MoveFeedDetailsToFront();
                            RefreshFeedDisplay(tn, true); // does also set the FeedDetailTabState.Url
                            AddHistoryEntry(tn, null);
                            break;

                        case FeedNodeType.Category:
                            RefreshCategoryDisplay(tn); // does also set the FeedDetailTabState.Url
                            AddHistoryEntry(tn, null);
                            break;

                        case FeedNodeType.SmartFolder:
                            try
                            {
                                FeedDetailTabState.Url = String.Empty;
                                AddHistoryEntry(tn, null);

                                var isf = tn as ISmartFolder;
                                if (isf != null)
                                    PopulateSmartFolder(tn, true);

								if (tn is UnreadItemsNode && isf != null)
                                {
									// UnreadItemsNode is a SmartFolder:
                                	IList<INewsItem> items = isf.Items;
									if (items.Count > 0)
									{
										FeedInfoList fiList = CreateFeedInfoList(tn.Text, items);
										BeginTransformFeedList(fiList, tn, owner.Stylesheet);
									}
                                }
                            }
                            catch (Exception ex)
                            {
                                _log.Error("Unexpected Error on PopulateSmartFolder()", ex);
                                owner.MessageError(String.Format(SR.ExceptionGeneral, ex.Message));
                            }
                            break;

                        case FeedNodeType.Finder:
                            try
                            {
                                FeedDetailTabState.Url = String.Empty;
                                AddHistoryEntry(tn, null);
                                PopulateFinderNode((FinderNode) tn, true);
                            }
                            catch (Exception ex)
                            {
                                _log.Error("Unexpected Error on PopulateAggregatedFolder()", ex);
                                owner.MessageError(String.Format(SR.ExceptionGeneral, ex.Message));
                            }
                            break;

                        case FeedNodeType.FinderCategory:
                            FeedDetailTabState.Url = String.Empty;
                            AddHistoryEntry(tn, null);
                            break;

                        case FeedNodeType.Root:
                            /* 
							if (this.GetRootType(RootFolderType.MyFeeds).Equals(tn)) 
								AggregateSubFeeds(tn);	// it is slow on startup, nothing is loaded in memory...
							*/
                            htmlDetail.Html = String.Empty;
                            htmlDetail.Navigate(null);
                            FeedDetailTabState.Url = String.Empty;
                            AddHistoryEntry(tn, null);
							FeedSourceEntry entry = FeedSourceEntryOf(tn);
							if (entry != null)
								SetGuiStateFeedback(String.Format(SR.StatisticsAllFeedsCountMessage,
                                                    entry.Source.GetFeeds().Count));
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    owner.Mediator.SetEnabled("-cmdFeedItemNewPost");
                }
            }
            catch (Exception ex)
            {
                _log.Error("Unexpected Error in OnTreeFeedAfterSelect()", ex);
                owner.MessageError(String.Format(SR.ExceptionGeneral, ex.Message));
            }
        }

		/// <summary>
		/// Creates the feed info list. It takes the items and groups the
		/// unread items by feed for display.
		/// </summary>
		/// <param name="title">The title.</param>
		/// <param name="items">The items.</param>
		/// <returns></returns>
        private FeedInfoList CreateFeedInfoList(string title, IList<INewsItem> items)
        {
            var result = new FeedInfoList(title);
            if (items == null || items.Count == 0)
                return result;

            var fiCache = new Dictionary<string, FeedInfo>(items.Count);

            for (int i = 0; i < items.Count; i++)
            {
                INewsItem item = items[i];
                if (item == null || item.Feed == null)
                    continue;

				// unread items contains a mix of feed items from various sources:
            	FeedSourceEntry entry = owner.FeedSources.SourceOf(item.Feed);
                string feedUrl = item.Feed.link;

                if (feedUrl == null || entry == null || !entry.Source.IsSubscribed(feedUrl))
                    continue;

                try
                {
                    FeedInfo fi;
                    if (!fiCache.TryGetValue(feedUrl, out fi))
                    {
                        IFeedDetails fd =  owner.GetFeedDetails(entry, feedUrl);
                        if (fd != null)
                        {
                            fi = new FeedInfo(fd, new List<INewsItem>());
                        }
                    }

                    if (fi == null) // with an error, and the like: ignore
                        continue;

                    if (!item.BeenRead)
                        fi.ItemsList.Add(item);

                    if (fi.ItemsList.Count > 0 && !fiCache.ContainsKey(feedUrl))
                    {
                        fiCache.Add(feedUrl, fi);
                        result.Add(fi);
                    }
                }
                catch (Exception e)
                {
                    owner.PublishXmlFeedError(e, feedUrl, true, entry);
                }
            }

            return result;
        }

        internal void RenameTreeNode(TreeFeedsNodeBase tn, string newName)
        {
            tn.TextBeforeEditing = tn.Text;
            tn.Text = newName;
            OnTreeFeedAfterLabelEdit(this, new NodeEventArgs(tn));
        }

        private static void OnTreeFeedBeforeLabelEdit(object sender, CancelableNodeEventArgs e)
        {
            var editedNode = (TreeFeedsNodeBase) e.TreeNode;
            e.Cancel = !editedNode.Editable;
            if (!e.Cancel)
            {
                editedNode.TextBeforeEditing = editedNode.Text;
            }
        }

        private void OnTreeFeedsValidateLabelEdit(object sender, ValidateLabelEditEventArgs e)
        {
            string newText = e.LabelEditText;
            if (string.IsNullOrEmpty(newText))
            {
                e.StayInEditMode = true;
                return;
            }

            var editedNode = (TreeFeedsNodeBase) e.Node;
            string newLabel = newText.Trim();

            if (editedNode.Type != FeedNodeType.Feed &&
                editedNode.Type != FeedNodeType.Finder)
            {
                //category node 

                TreeFeedsNodeBase existingNode =
                    TreeHelper.FindChildNode(editedNode.Parent, newLabel, FeedNodeType.Category);
                if (existingNode != null && existingNode != editedNode)
                {
                    owner.MessageError(String.Format(SR.ExceptionDuplicateCategoryName, newLabel));
                    e.StayInEditMode = true;
                    return;
                }
            }
        }

        private void OnTreeFeedAfterLabelEdit(object sender, NodeEventArgs e)
        {
            var editedNode = (TreeFeedsNodeBase) e.TreeNode;
            string newLabel = e.TreeNode.Text.Trim();
            string oldLabel = editedNode.TextBeforeEditing;
            editedNode.TextBeforeEditing = null; // reset for safety (only used in editing mode)

            //handle the case where right-click was used to rename a tree node even though another 
            //item was currently selected. This resets the current 
            //this.CurrentSelectedFeedsNode = this.TreeSelectedFeedsNode;

			if (editedNode.Type == FeedNodeType.Root)
			{
				// subscription node (feed source)
				FeedSourceEntry entry = FeedSourceEntryOf(editedNode);
				owner.ChangeFeedSource(entry, newLabel, null, null);
				Navigator.SelectedGroup.Text = newLabel;
			} 
			else if (editedNode.Type == FeedNodeType.Feed)
            {
                //feed node 

                INewsFeed f = owner.GetFeed(FeedSourceEntryOf(editedNode), editedNode.DataKey);
                if (f != null)
                {
                    f.title = newLabel;
                    owner.FeedWasModified(f, NewsFeedProperty.FeedTitle);
                    //owner.FeedlistModified = true;
                }
            }
            else if (editedNode.Type == FeedNodeType.Finder)
            {
                // all yet done
            }
            else
            {
                //category node 

                string oldFullname = oldLabel;
                string[] catArray = TreeFeedsNodeBase.BuildCategoryStoreNameArray(editedNode);
                if (catArray.Length > 0)
                {
                    // build old category store name by replace the new label returned
                    // by the oldLabel kept:
                    catArray[catArray.Length - 1] = oldLabel;
                    oldFullname = String.Join(FeedSource.CategorySeparator, catArray);
                }

                if (GetRootType(editedNode) == RootFolderType.MyFeeds)
                {
                    string newFullname = editedNode.CategoryStoreName;
                    
                    FeedSourceEntry entry = FeedSourceEntryOf(editedNode);
                    entry.Source.RenameCategory(oldFullname, newFullname);

                    owner.SubscriptionModified(entry, NewsFeedProperty.FeedCategory);
                }
            }
        }


        private void OnTreeFeedSelectionDragStart(object sender, EventArgs e)
        {
            TreeFeedsNodeBase tn = TreeSelectedFeedsNode;
            if (tn != null && (tn.Type == FeedNodeType.Feed || tn.Type == FeedNodeType.Category))
            {
                CurrentDragNode = tn;

                if (CurrentDragNode.Expanded)
                    CurrentDragNode.Expanded = false;

                string dragObject = null;

                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    IFeedDetails fd = null;
                    if (CurrentDragNode.Type == FeedNodeType.Feed)
						fd = owner.GetFeedDetails(FeedSourceEntryOf(CurrentDragNode), CurrentDragNode.DataKey);
                    if (fd != null)
                    {
                        dragObject = fd.Link;
                    }
                }
                if (dragObject != null)
                {
                    DoDragDrop(dragObject, DragDropEffects.Copy | DragDropEffects.Link);
                }
                else
                {
                    if (CurrentDragNode.Type == FeedNodeType.Feed)
                    {
                        dragObject = CurrentDragNode.DataKey;
                    }
                    else
                    {
                        dragObject = CurrentDragNode.Text;
                    }
                    DoDragDrop(dragObject, DragDropEffects.Copy | DragDropEffects.Move);
                }
                CurrentDragHighlightNode = CurrentDragNode = null;
            }
        }

        private void OnTreeFeedDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                if ((e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link)
                {
                    e.Effect = DragDropEffects.Link; // we got this on drag urls from IE !
                }
                else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                {
                    e.Effect = DragDropEffects.Move;
                }
                else if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                    CurrentDragHighlightNode = null;
                    return;
                }

                var p = new Point(e.X, e.Y);
                var t = (TreeFeedsNodeBase) treeFeeds.GetNodeFromPoint(treeFeeds.PointToClient(p));

                if (t == null)
                {
                    e.Effect = DragDropEffects.None;
                    CurrentDragHighlightNode = null;
                }
                if (t != null)
                {
                    if (t.Type == FeedNodeType.Feed)
                        CurrentDragHighlightNode = t.Parent;
                    else if (t.Type == FeedNodeType.Category || GetRoot(RootFolderType.MyFeeds).Equals(t))
                        CurrentDragHighlightNode = t;
                    else
                    {
                        e.Effect = DragDropEffects.None;
                        CurrentDragHighlightNode = null;
                    }
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
                CurrentDragHighlightNode = null;
            }
        }

        private static void OnTreeFeedGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            //if we are a drag source, ...
            _log.Debug("OnTreeFeedGiveFeedback() effect:" + e.Effect);
        }

        private static void OnTreeFeedQueryContiueDrag(object sender, QueryContinueDragEventArgs e)
        {
            // keyboard or mouse button state changes
            // we listen to Unpress Ctrl:
            _log.Debug("OnTreeFeedQueryContiueDrag() action:" + e.Action + ", KeyState:" +
                       e.KeyState);
        }

        private void OnTreeFeedDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                if ((e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link)
                {
                    e.Effect = DragDropEffects.Link;
                }
                else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                {
                    e.Effect = DragDropEffects.Move;
                }
                else if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                    CurrentDragHighlightNode = null;
                    return;
                }

                Point p = treeFeeds.PointToClient(new Point(e.X, e.Y));
                var t = (TreeFeedsNodeBase) treeFeeds.GetNodeFromPoint(p);

                if (t == null)
                {
                    e.Effect = DragDropEffects.None;
                    CurrentDragHighlightNode = null;
                }

                if (t != null)
                {
                    if (t.Type == FeedNodeType.Feed)
                        CurrentDragHighlightNode = t.Parent;
                    else if (t.Type == FeedNodeType.Category || GetRoot(RootFolderType.MyFeeds).Equals(t))
                        CurrentDragHighlightNode = t;
                    else
                    {
                        e.Effect = DragDropEffects.None;
                        CurrentDragHighlightNode = null;
                    }
                }

                // UltraTree can scroll automatically, if the mouse
                // is near top/bottom, so this code is just there for
                // reference - how to apply this to a MS Treeview:
                //				int tcsh = this.treeFeeds.ClientSize.Height;
                //				int scrollThreshold = 25;
                //				if (p.Y + scrollThreshold > tcsh)
                //					Win32.SendMessage(this.treeFeeds.Handle, (int)Win32.Message.WM_VSCROLL, 1, 0);
                //				else if (p.Y < scrollThreshold)
                //					Win32.SendMessage(this.treeFeeds.Handle, (int)Win32.Message.WM_VSCROLL, 0, 0);
            }
            else
            {
                e.Effect = DragDropEffects.None;
                CurrentDragHighlightNode = null;
            }
        }

        private void OnTreeFeedDragDrop(object sender, DragEventArgs e)
        {
            CurrentDragHighlightNode = null;

            //get node where feed was dropped 
            var p = new Point(e.X, e.Y);
            var target = (TreeFeedsNodeBase) treeFeeds.GetNodeFromPoint(treeFeeds.PointToClient(p));

            //move node if dropped on a category node (or below)
            if (target != null)
            {
                TreeFeedsNodeBase node2move = CurrentDragNode;

                if (target.Type == FeedNodeType.Feed)
                {
                    // child of a category. Take the parent as target
                    target = target.Parent;
                }


                if (node2move != null)
                {
                    MoveNode(node2move, target, true);
                }
                else
                {
                    // foreign drag/drop op

                    // Bring the main window to the front so the user can
                    // enter the dropped feed details.  Otherwise the feed
                    // details window can pop up underneath the drop source,
                    // which is confusing.
                    Win32.SetForegroundWindow(Handle);

                    var sData = (string) e.Data.GetData(DataFormats.Text);
                    DelayTask(DelayedTasks.AutoSubscribeFeedUrl, new object[] {target, sData});
                }
            }

            CurrentDragNode = null;
        }

        private void OnTimerTreeNodeExpandElapsed(object sender, ElapsedEventArgs e)
        {
            //_timerTreeNodeExpand.Stop();
            //if (CurrentDragHighlightNode != null) 
            //{
            //    if (!CurrentDragHighlightNode.Expanded)
            //        CurrentDragHighlightNode.Expanded = true;
            //}
        }

        private void OnTimerFeedsRefreshElapsed(object sender, ElapsedEventArgs e)
        {
            if (owner.InternetAccessAllowed && owner.CurrentGlobalRefreshRateMinutes > 0)
            {
                UpdateAllFeeds(false);
            }
        }

        private void OnTimerCommentFeedsRefreshElapsed(object sender, ElapsedEventArgs e)
        {
            if (owner.InternetAccessAllowed && owner.CurrentGlobalRefreshRateMinutes > 0)
            {
                UpdateAllCommentFeeds(true);
            }
        }

        /// <summary>
        /// Called when startup timer fires (ca. 45 secs after UI startup).
        /// This delay is required, if Bandit gets started via Windows Auto-Start
        /// to prevent race conditions with WLAN startup/LAN init (we require the
        /// Internet connection to succeed)
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnTimerStartupTick(object sender, EventArgs e)
        {
            _startupTimer.Enabled = false;
            // start load items and refresh from web, force if we have to refresh on startup:
            if (owner.InternetAccessAllowed)
            {
                UpdateAllFeeds(owner.Preferences.FeedRefreshOnStartup);
            }
        }

        private void OnTimerResetStatusTick(object sender, EventArgs e)
        {
            _timerResetStatus.Stop();
            SetGuiStateFeedback(String.Empty);
            if (_trayManager.CurrentState == ApplicationTrayState.BusyRefreshFeeds ||
                GetRoot(RootFolderType.MyFeeds).UnreadCount == 0)
            {
                _trayManager.SetState(ApplicationTrayState.NormalIdle);
            }
        }

        internal void OnFeedListItemActivate(object sender, EventArgs e)
        {
            if (listFeedItems.SelectedItems.Count == 0)
                return;

            if ((MouseButtons & MouseButtons.Right) == MouseButtons.Right)
                return;

            var selectedItem = (ThreadedListViewItem) listFeedItems.SelectedItems[0];
            OnFeedListItemActivateManually(selectedItem);
        }

        internal void OnFeedListItemActivateManually(ThreadedListViewItem selectedItem)
        {
            try
            {
                // get the current item/feedNode
                INewsItem item = CurrentSelectedFeedItem = (INewsItem) selectedItem.Key;
                TreeFeedsNodeBase tn = TreeSelectedFeedsNode;
                FeedSourceEntry entry = FeedSourceEntryOf(tn); 
                string stylesheet;

                //load item content from disk if not in memory
				if (item != null && !item.HasContent && entry != null)
                {                    
                    entry.Source.GetCachedContentForItem(item);
                }

                // refresh context menu items
                RefreshTreeFeedContextMenus(tn);

                if (item != null && tn != _sentItemsFeedsNode &&
                    item.CommentStyle != SupportedCommentStyle.None &&
                    owner.InternetAccessAllowed)
                    owner.Mediator.SetEnabled("+cmdFeedItemPostReply");
                else
                    owner.Mediator.SetEnabled("-cmdFeedItemPostReply");

                SearchCriteriaCollection searchCriterias = null;
                var agNode = CurrentSelectedFeedsNode as FinderNode;
                if (agNode != null && agNode.Finder.DoHighlight)
                    searchCriterias = agNode.Finder.SearchCriterias;


                //mark the item as read
                bool itemJustRead = false;

                if (item != null && !item.BeenRead)
                {
                    itemJustRead = item.BeenRead = true;

                    if (item is SearchHitNewsItem)
                    {

						var sItem = item as SearchHitNewsItem;
						owner.FeedSources.ForEach(source =>
							{
								INewsItem realItem = source.FindNewsItem(sItem);
								if (realItem != null)
								{
									realItem.BeenRead = true;
									item = realItem;
								}
							});
						
						//var sItem = item as SearchHitNewsItem;
						//INewsItem realItem = null;
						//if (entry != null)
						//    realItem= entry.Source.FindNewsItem(sItem);

						//if (realItem != null)
						//{
						//    realItem.BeenRead = true;
						//    item = realItem;
						//}
                    }
                }

                //render 
                if (item == null)
                {
                    // can happen on dummy items ("Loading..."), if the user clicks fast enough

                    htmlDetail.Clear();
                    FeedDetailTabState.Url = String.Empty;
                    RefreshDocumentState(_docContainer.ActiveDocument);
                }
                else if (!item.HasContent && !string.IsNullOrEmpty(item.Link))
                {
                    /* if (this.UrlRequestHandledExternally(item.Link, false)) {
						htmlDetail.Clear();
					} else */
                    if (owner.Preferences.NewsItemOpenLinkInDetailWindow)
                    {
                        htmlDetail.Navigate(item.Link);
                    }
                    else
                    {
                        // not allowed: just display the Read On... 
                        stylesheet = (item.Feed != null && entry != null ? entry.Source.GetStyleSheet(item.Feed.link) : String.Empty);
                        htmlDetail.Html = owner.FormatNewsItem(stylesheet, item, searchCriterias);
                        htmlDetail.Navigate(null);
                    }

                    FeedDetailTabState.Url = item.Link;
                    if (!_navigationActionInProgress)
                    {
                        AddHistoryEntry(tn, item);
                    }
                    else
                    {
                        RefreshDocumentState(_docContainer.ActiveDocument);
                    }
                }
                else
                {
					stylesheet = (item.Feed != null && entry != null ? entry.Source.GetStyleSheet(item.Feed.link) : String.Empty);
                    htmlDetail.Html = owner.FormatNewsItem(stylesheet, item, searchCriterias);
                    htmlDetail.Navigate(null);

                    FeedDetailTabState.Url = item.Link;
                    if (!_navigationActionInProgress)
                    {
                        AddHistoryEntry(tn, item);
                    }
                    else
                    {
                        RefreshDocumentState(_docContainer.ActiveDocument);
                    }
                }


                if (item != null)
                {
                    //assume that clicking on the item indicates viewing new comments 
                    //when no comment feed available
                    if (item.WatchComments && string.IsNullOrEmpty(item.CommentRssUrl))
                    {
                        MarkCommentsAsViewed(tn, item);
                        ApplyStyles(selectedItem, true);
                    } //if(item.WatchComments...)

                    //if item was read on this click then reflect the change in the GUI 

                    if (itemJustRead)
                    {
                        ApplyStyles(selectedItem, true);
                        var sfNode = CurrentSelectedFeedsNode as SmartFolderNodeBase;

                        if (selectedItem.ImageIndex > 0) selectedItem.ImageIndex--;

                        bool isTopLevelItem = (selectedItem.IndentLevel == 0);
                        int equalItemsRead = (isTopLevelItem ? 1 : 0);
                        lock (listFeedItems.Items)
                        {
                            for (int j = 0; j < listFeedItems.Items.Count; j++)
                            {
                                // if there is a self-reference thread, we also have to switch the Gui state for them
                                ThreadedListViewItem th = listFeedItems.Items[j];
                                var selfRef = th.Key as INewsItem;
                                if (item.Equals(selfRef) && (th.ImageIndex%2) != 0)
                                {
                                    // unread-state images always odd index numbers
                                    ApplyStyles(th, true);
                                    th.ImageIndex--;
                                    if (selfRef != null)
                                    {
                                        if (!selfRef.BeenRead)
                                        {
                                            // object ref is unequal, but other criteria match the item to be equal...
                                            selfRef.BeenRead = true;
                                        }
                                    }
                                    if (th.IndentLevel == 0)
                                    {
                                        isTopLevelItem = true;
                                        equalItemsRead++;
                                    }
                                }
                            }
                        }

                        if (isTopLevelItem && tn.Type == FeedNodeType.Feed || tn.Type == FeedNodeType.SmartFolder ||
                            tn.Type == FeedNodeType.Finder)
                        {
                            UpdateTreeNodeUnreadStatus(tn, -equalItemsRead);
                            UnreadItemsNode.MarkItemRead(item);
                            //this.DelayTask(DelayedTasks.RefreshTreeUnreadStatus, new object[]{tn,-equalItemsRead});											
                        }

                        TreeFeedsNodeBase root = GetRoot(RootFolderType.MyFeeds);

                        if (item.Feed.link == tn.DataKey)
                        {
                            // test for catch all on selected node
                            item.Feed.containsNewMessages = (tn.UnreadCount != 0);
                        }
                        else
                        {
                            // other (categorie selected, aggregated or an threaded item from another feed)

                            if (agNode != null) agNode.UpdateReadStatus();
                            if (sfNode != null) sfNode.UpdateReadStatus();
                            if (isTopLevelItem && tn.Type == FeedNodeType.Category)
                                UnreadItemsNode.MarkItemRead(item);

                            // lookup corresponding TreeNode:
                            TreeFeedsNodeBase refNode = TreeHelper.FindNode(root, item.Feed);
                            if (refNode != null)
                            {
                                //refNode.UpdateReadStatus(refNode , -1);
                                DelayTask(DelayedTasks.RefreshTreeUnreadStatus, new object[] {refNode, -1});
                                item.Feed.containsNewMessages = (refNode.UnreadCount != 0);
                            }
                            else
                            {
                                // temp feed item, e.g. from commentRss
                                string hash = RssHelper.GetHashCode(item);
                                if (!tempFeedItemsRead.ContainsKey(hash))
                                    tempFeedItemsRead.Add(hash, null /* item ???*/);
                            }
                        }

                        owner.FeedWasModified(item.Feed, NewsFeedProperty.FeedItemReadState);
                        //owner.FeedlistModified = true;
                    } //itemJustRead
                }
            }
            catch (Exception ex)
            {
                _log.Error("OnFeedListItemActivateManually() failed.", ex);
            }
        }


		///// <summary>
		///// Returns the URL of the original feed for this INewsItem. 
		///// </summary>
		///// <remarks>Assumes the INewsItem is in the flagged or watched items smart folder</remarks>
		///// <param name="currentNewsItem"></param>
		///// <returns>The feed URL of the source feed if  a pointer to it exists and NULL otherwise.</returns>
		//private static string GetOriginalFeedUrl(INewsItem currentNewsItem)
		//{
		//    string feedUrl = null;

		//    if (currentNewsItem.OptionalElements.ContainsKey(AdditionalFeedElements.OriginalFeedOfWatchedItem))
		//    {
		//        string str = currentNewsItem.OptionalElements[AdditionalFeedElements.OriginalFeedOfWatchedItem];

		//        if (
		//            str.StartsWith("<" + AdditionalFeedElements.CurrentPrefix + ":" +
		//                           AdditionalFeedElements.OriginalFeedOfWatchedItem.Name) ||
		//            str.StartsWith("<" + AdditionalFeedElements.ElementPrefix + ":" +
		//                           AdditionalFeedElements.OriginalFeedOfWatchedItem.Name) ||
		//            str.StartsWith("<" + AdditionalFeedElements.OldElementPrefix + ":" +
		//                           AdditionalFeedElements.OriginalFeedOfWatchedItem.Name))
		//        {
		//            int startIndex = str.IndexOf(">") + 1;
		//            int endIndex = str.LastIndexOf("<");
		//            feedUrl = str.Substring(startIndex, endIndex - startIndex);
		//        }
		//    }

		//    return feedUrl;
		//}

        /// <summary>
        /// Marks the comments for a INewsItem as read in a given feed node and across any other feed nodes 
        /// in which it appears. 
        /// </summary>
        /// <param name="tn">The feed node</param>
        /// <param name="currentNewsItem">The item whose comments have been read</param>
        private void MarkCommentsAsViewed(TreeFeedsNodeBase tn, INewsItem currentNewsItem)
        {
            INewsFeed feed = currentNewsItem.Feed;
            bool commentsJustRead = currentNewsItem.HasNewComments;
            currentNewsItem.HasNewComments = false;

            if (commentsJustRead && (CurrentSelectedFeedsNode != null))
            {
                TreeFeedsNodeBase refNode = null;
                owner.FeedWasModified(feed, NewsFeedProperty.FeedItemNewCommentsRead);

                if (tn.Type == FeedNodeType.Feed)
                {
                    UpdateCommentStatus(tn, new List<INewsItem>(new[] {currentNewsItem}), true);
                }
                else
                {
                    //if we are on a category or search folder, then locate node under MyFeeds and update its comment status												
                    if (tn.Type == FeedNodeType.Category || tn.Type == FeedNodeType.Finder)
                    {
                        refNode = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), currentNewsItem.Feed);
                        UpdateCommentStatus(refNode, new List<INewsItem>(new[] {currentNewsItem}), true);

                        //we don't need to do this for a Category node because this should be done when we call this.UpdateCommentStatus()
                        if (tn.Type == FeedNodeType.Finder)
                            tn.UpdateCommentStatus(tn, -1);
                    }
                    else if (tn.Type == FeedNodeType.SmartFolder)
                    {
                        //things are more complicated if we are on a smart folder such as the 'Watched Items' folder

                        /* first get the feed URL */
						int sourceID;
						string feedUrl = OptionalItemElement.GetOriginalFeedReference(currentNewsItem, out sourceID); //GetOriginalFeedUrl(currentNewsItem);
						
						FeedSource source = null;
						if (owner.FeedSources.ContainsKey(sourceID))
							source = owner.FeedSources[sourceID].Source;

						if (source == null)
							source = FeedSourceOf(tn); 
                        
						/* 
						 * now, locate INewsItem in actual feed and mark comments as viewed 
						 * then update tree node comment status. 							 
						 */
						if (feedUrl != null && source != null && source.GetFeeds().TryGetValue(feedUrl, out feed))
                        {
                            IList<INewsItem> newsItems = source.GetCachedItemsForFeed(feedUrl);

                            foreach (var ni in newsItems)
                            {
                                if (currentNewsItem.Equals(ni))
                                {
                                    ni.HasNewComments = false;
                                }
                            }

                            refNode = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), feedUrl);
                            UpdateCommentStatus(refNode, new List<INewsItem>(new[] {currentNewsItem}), true);
                        } //if(feedUrl != null) 
                    }

                    /* if (refNode != null) {
								this.DelayTask(DelayedTasks.RefreshTreeCommentStatus, new object[]{refNode, new ArrayList(new INewsItem[]{currentNewsItem}), true});
							} */


                    if (refNode == null)
                    {
                        feed.containsNewComments = (tn.ItemsWithNewCommentsCount != 0);
                    }
                    else
                    {
                        feed.containsNewComments = (refNode.ItemsWithNewCommentsCount != 0);
                    }
                   
                } //if (tn.Type == FeedNodeType.Feed )
            } //if(commentsJustRead && (this.CurrentSelectedFeedsNode!= null))
        }

        private void OnFeedListExpandThread(object sender, ThreadEventArgs e)
        {
            try
            {
            	FeedSourceEntry entry = FeedSourceEntryOf(CurrentSelectedFeedsNode);
				FeedSource source = entry != null ? entry.Source:null; 
                var currentNewsItem = (INewsItem) e.Item.Key;
                var ikp = new INewsItem[e.Item.KeyPath.Length];
                e.Item.KeyPath.CopyTo(ikp, 0);
                IList<INewsItem> itemKeyPath = ikp;

                // column index map
                ColumnKeyIndexMap colIndex = listFeedItems.Columns.GetColumnIndexMap();

                ICollection<INewsItem> outGoingItems =
                    source.GetItemsFromOutGoingLinks(currentNewsItem, itemKeyPath);
                ICollection<INewsItem> inComingItems =
                    source.GetItemsWithIncomingLinks(currentNewsItem, itemKeyPath);

                var childs = new ArrayList(outGoingItems.Count + inComingItems.Count + 1);
                ThreadedListViewItem newListItem;

                try
                {
                    foreach (INewsItem o in outGoingItems)
                    {
                        bool hasRelations = NewsItemHasRelations(o, itemKeyPath, source);
                        newListItem =
                            CreateThreadedLVItem(o, hasRelations, Resource.NewsItemImage.OutgoingRead, colIndex, false);

                        //does it match any filter? 
                        _filterManager.Apply(newListItem);

                        childs.Add(newListItem);
                    }
                }
                catch (Exception e1)
                {
                    _log.Error("OnFeedListExpandThread exception (iterate outgoing)", e1);
                }

                try
                {
                    foreach (INewsItem o in inComingItems)
                    {
                        bool hasRelations = NewsItemHasRelations(o, itemKeyPath, source);
                        newListItem =
                            CreateThreadedLVItem(o, hasRelations, Resource.NewsItemImage.IncomingRead, colIndex, false);

                        //does it match any filter? 
                        _filterManager.Apply(newListItem);

                        childs.Add(newListItem);
                    } //iterator.MoveNext
                }
                catch (Exception e2)
                {
                    _log.Error("OnFeedListExpandThread exception (iterate incoming)", e2);
                }

                if (currentNewsItem.HasExternalRelations)
                {
                    // includes also commentRss support

                    if (currentNewsItem.GetExternalRelations().Count == 0 ||
                        currentNewsItem.CommentCount != currentNewsItem.GetExternalRelations().Count)
                    {
                        if (owner.InternetAccessAllowed)
                        {
                            var insertionPoint =
                                (ThreadedListViewItemPlaceHolder)
                                CreateThreadedLVItemInfo(SR.GUIStatusLoadingChildItems, false);
                            childs.Add(insertionPoint);
                            BeginLoadCommentFeed(currentNewsItem, insertionPoint.InsertionPointTicket, itemKeyPath);
                        }
                        else
                        {
                            newListItem = CreateThreadedLVItemInfo(SR.GUIStatusChildItemsNA, false);
                            childs.Add(newListItem);
                        }
                    }
                    else
                    {
                        // just take the existing collection

                        // they are sorted as we requested them, so we do not sort again here
                        //currentNewsItem.GetExternalRelations();

                        //List<INewsItem> commentItems 
                        //  = new List<RelationBase>(currentNewsItem.GetExternalRelations()).ConvertAll<INewsItem>(RssBandit.Common.Utils.TypeConverter.DownCast<RelationBase, INewsItem>());
                        //commentItems.Sort(RssHelper.GetComparer(false, NewsItemSortField.Date));

                        foreach (INewsItem o in currentNewsItem.GetExternalRelations())
                        {
                            bool hasRelations = NewsItemHasRelations(o, itemKeyPath, source);

                            o.BeenRead = tempFeedItemsRead.ContainsKey(RssHelper.GetHashCode(o));
                            newListItem =
                                CreateThreadedLVItem(o, hasRelations, Resource.NewsItemImage.CommentRead, colIndex, true);
                            _filterManager.Apply(newListItem);
                            childs.Add(newListItem);
                        } //iterator.MoveNext
                    }
                }

                e.ChildItems = new ThreadedListViewItem[childs.Count];
                childs.CopyTo(e.ChildItems);

                //mark new comments as read once we've successfully loaded comments 				
                MarkCommentsAsViewed(CurrentSelectedFeedsNode, currentNewsItem);
                ApplyStyles(e.Item, currentNewsItem.BeenRead, currentNewsItem.HasNewComments);
            }
            catch (Exception ex)
            {
                _log.Error("OnFeedListExpandThread exception", ex);
            }
        }

        private void OnFeedListAfterExpandThread(object sender, ThreadEventArgs e)
        {
            // here we have the listview handle set and the listview items are member of the list.
            // so we refresh flag icons for new listview thread childs here:
            ApplyNewsItemPropertyImages(e.ChildItems);
        }

        private void OnFeedListLayoutChanged(object sender, ListLayoutEventArgs e)
        {
            // build columns, etc. pp
            if (e.Layout.Columns.Count > 0)
            {
                EmptyListView();
                lock (listFeedItems.Columns)
                {
                    listFeedItems.Columns.Clear();
                    int i = 0;
                    IList<int> colW = e.Layout.ColumnWidths;
                    foreach (var colID in e.Layout.Columns)
                    {
                        AddListviewColumn(colID, colW[i++]);
                    }
                }
            }
            RefreshListviewColumnContextMenu();
        }

        private void OnFeedListLayoutModified(object sender, ListLayoutEventArgs e)
        {
            if (TreeSelectedFeedsNode != null)
                SetFeedColumnLayout(TreeSelectedFeedsNode, e.Layout);
        }

        private void OnFeedListItemsColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (listFeedItems.Items.Count == 0)
                return;

            if (listFeedItems.SelectedItems.Count > 0)
                return;

            TreeFeedsNodeBase feedsNode = CurrentSelectedFeedsNode;
            if (feedsNode == null)
                return;

            bool unreadOnly = true;
            if (feedsNode.Type == FeedNodeType.Finder)
                unreadOnly = false;

            IList<INewsItem> items = NewsItemListFrom(listFeedItems.Items, unreadOnly);
            if (items == null || items.Count <= 1) // no need to re-sort on no or just one item
                return;


            var temp = new Hashtable();

            foreach (INewsItem item in items)
            {
                IFeedDetails fi;
                if (temp.ContainsKey(item.Feed.link))
                {
					fi = (IFeedDetails)temp[item.Feed.link];
                }
                else
                {
					fi = (IFeedDetails)item.FeedDetails.Clone();
                    fi.ItemsList.Clear();
                    temp.Add(item.Feed.link, fi);
                }
                fi.ItemsList.Add(item);
            }

            string category = feedsNode.CategoryStoreName;
            var redispItems = new FeedInfoList(category);

			foreach (IFeedDetails fi in temp.Values)
            {
                if (fi.ItemsList.Count > 0)
                    redispItems.Add(fi);
            }
            FeedSource source = FeedSourceOf(feedsNode); 
            BeginTransformFeedList(redispItems, CurrentSelectedFeedsNode, source != null ?
				source.GetCategoryStyleSheet(category): String.Empty);
        }

        private void OnFeedListMouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                var lv = (ListView) sender;
                ThreadedListViewItem lvi = null;

                try
                {
                    lvi = (ThreadedListViewItem) lv.GetItemAt(e.X, e.Y);
                }
                catch
                {
                }

                if (lvi == null)
                    RefreshListviewContextMenu(null, false);

                if (e.Button == MouseButtons.Right)
                {
                    // behavior similar to Windows Explorer Listview:					
                    if (lv.Items.Count > 0)
                    {
                        if (lvi != null)
                        {
                            // if(Control.ModifierKeys != Keys.Control)
                            //    lv.SelectedItems.Clear();							   														 

                            lvi.Selected = true;
                            lvi.Focused = true;
                            RefreshListviewContextMenu();
                            OnFeedListItemActivateManually(lvi);
                        }
                    }

                    // TR: commented out - incorrect behavior (loosing selection, etc.)					
                    //					if (lv.Items.Count > 0) 
                    //					{
                    //					
                    //						if (lvi != null) 
                    //						{
                    //							lv.SelectedItems.Clear();
                    //							lvi.Selected = true;
                    //							lvi.Focused = true;							
                    //							RefreshListviewContextMenu();
                    //							this.OnFeedListItemActivate(sender, EventArgs.Empty);
                    //						}
                    //
                    //					}
                }
                else
                {
                    // !MouseButtons.Right

                    if (lv.Items.Count <= 0)
                        return;

                    if (lvi != null && e.Clicks > 1)
                    {
                        //DblClick

                        INewsItem item = CurrentSelectedFeedItem = (INewsItem) lvi.Key;

                        lv.SelectedItems.Clear();
                        lvi.Selected = true;
                        lvi.Focused = true;

                        if (item != null && !string.IsNullOrEmpty(item.Link))
                        {
                            if (!UrlRequestHandledExternally(item.Link, false))
                                DetailTabNavigateToUrl(item.Link, null, false, true);
                        }
                    }
                } //! MouseButtons.Right
            }
            catch (Exception ex)
            {
                _log.Error("OnFeedListMouseDown() failed", ex);
            }
        }

        private void OnStatusPanelClick(object sender, StatusBarPanelClickEventArgs e)
        {
            if (e.Clicks > 1 && e.StatusBarPanel == statusBarConnectionState)
            {
                // DblClick to the connection state panel image
                owner.UpdateInternetConnectionState(true); // force a connection check
            }
        }

        private void OnStatusPanelLocationChanged(object sender, EventArgs e)
        {
            progressBrowser.SetBounds(_status.Width -
                                      (statusBarRssParser.Width + statusBarConnectionState.Width +
                                       BrowserProgressBarWidth + 10),
                                      _status.Location.Y + 6, 0, 0, BoundsSpecified.Location);
        }

        private void RePopulateListviewWithCurrentContent()
        {
            RePopulateListviewWithContent(NewsItemListFrom(listFeedItems.Items));
        }

        private void RePopulateListviewWithContent(IList<INewsItem> newsItemList)
        {
            if (newsItemList == null)
                newsItemList = new List<INewsItem>(0);

            ThreadedListViewItem lvLastSelected = null;
            if (listFeedItems.SelectedItems.Count > 0)
                lvLastSelected = (ThreadedListViewItem) listFeedItems.SelectedItems[0];

            bool categorizedView = (CurrentSelectedFeedsNode.Type == FeedNodeType.Category) ||
                                   (CurrentSelectedFeedsNode.Type == FeedNodeType.Finder);
            PopulateListView(CurrentSelectedFeedsNode, newsItemList, true, categorizedView, CurrentSelectedFeedsNode);

            // reselect the last selected
            if (lvLastSelected != null && lvLastSelected.IndentLevel == 0)
            {
                ReSelectListViewItem((INewsItem) lvLastSelected.Key);
            }
        }

        private void ReSelectListViewItem(IRelation item)
        {
            if (item == null) return;

            string selItemId = item.Id;
            if (selItemId != null)
            {
                for (int i = 0; i < listFeedItems.Items.Count; i++)
                {
                    ThreadedListViewItem theItem = listFeedItems.Items[i];
                    string thisItemId = ((INewsItem) theItem.Key).Id;
                    if (selItemId.CompareTo(thisItemId) == 0)
                    {
                        listFeedItems.Items[i].Selected = true;
                        listFeedItems.EnsureVisible(listFeedItems.Items[i].Index);
                        break;
                    }
                }
            }
        }

        private static IList<INewsItem> NewsItemListFrom(ThreadedListViewItemCollection list)
        {
            return NewsItemListFrom(list, false);
        }

        private static IList<INewsItem> NewsItemListFrom(ThreadedListViewItemCollection list, bool unreadOnly)
        {
            var items = new List<INewsItem>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                ThreadedListViewItem tlvi = list[i];
                if (tlvi.IndentLevel == 0)
                {
                    var item = (INewsItem) tlvi.Key;

                    if (unreadOnly && item != null && item.BeenRead)
                        item = null;

                    if (item != null)
                        items.Add(item);
                }
            }
            return items;
        }

        private void OnFeedListItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var item = (ThreadedListViewItem) e.Item;
                var r = (INewsItem) item.Key;
                if (r.Link != null)
                {
                    treeFeeds.AllowDrop = false; // do not drag to tree
                    DoDragDrop(r.Link, DragDropEffects.All | DragDropEffects.Link);
                    treeFeeds.AllowDrop = true;
                }
            }
        }

        /// <summary>
        /// support the keydown/pagedown keyup/pageup listview navigation 
        /// as well as deleting items via the Delete key.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFeedListItemKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!listFeedItems.Focused)
                    return;

#if TRACE_WIN_MESSAGES				
				Debug.WriteLine("OnFeedListItemKeyUp(" + e.KeyData +")");
#endif

                if (e.KeyCode == Keys.Down || e.KeyCode == Keys.PageDown || e.KeyCode == Keys.End)
                {
                    if (listFeedItems.SelectedItems.Count == 1)
                        if (listFeedItems.SelectedItems[0].Index <= listFeedItems.Items.Count)
                            OnFeedListItemActivate(sender, EventArgs.Empty);
                }
                else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.PageUp || e.KeyCode == Keys.Home)
                {
                    if (listFeedItems.SelectedItems.Count == 1)
                        if (listFeedItems.SelectedItems[0].Index >= 0)
                            OnFeedListItemActivate(sender, EventArgs.Empty);
                }
                else if (e.KeyCode == Keys.A && (e.Modifiers & Keys.Control) == Keys.Control)
                {
                    // select all
                    if (listFeedItems.Items.Count > 0 && listFeedItems.Items.Count != listFeedItems.SelectedItems.Count)
                    {
                        try
                        {
                            listFeedItems.BeginUpdate();
                            lock (listFeedItems.Items)
                            {
                                for (int i = 0; i < listFeedItems.Items.Count; i++)
                                {
                                    listFeedItems.Items[i].Selected = true;
                                }
                            }
                        }
                        finally
                        {
                            listFeedItems.EndUpdate();
                        }
                    }
                }
                else if (e.KeyCode == Keys.Delete)
                {
                    RemoveSelectedFeedItems();
                }
            }
            catch (Exception ex)
            {
                _log.Error("OnFeedListItemKeyUp() failed", ex);
            }
        }


        private void OnTrayIconDoubleClick(object sender, EventArgs e)
        {
            owner.CmdShowMainGui(null);
            //user is interested about the message this time
            _beSilentOnBalloonPopupCounter = 0; // reset balloon silent counter
        }

        //called, if the user explicitly closed the balloon
        private void OnTrayAniBalloonTimeoutClose(object sender, EventArgs e)
        {
            //user isn't interested about the message this time
            _beSilentOnBalloonPopupCounter = 12; // 12 * 5 minutes (refresh timer) == 1 hour (minimum)
        }

        #region toolbar combo's events

        internal void OnNavigateComboBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && e.Control == false)
            {
                // CTRL-ENTER is Url expansion
                DetailTabNavigateToUrl(UrlText, null, e.Shift, false);
            }
        }

        internal static void OnNavigateComboBoxDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                if ((e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link)
                {
                    e.Effect = DragDropEffects.Link;
                }
                else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                {
                    e.Effect = DragDropEffects.Move;
                }
                else if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        internal void OnNavigateComboBoxDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                var sData = (string) e.Data.GetData(typeof (string));
                try
                {
                    // accept uri only
                    var uri = new Uri(sData);
                    UrlText = uri.CanonicalizedUri();
                }
                catch
                {
                    //this.UrlText = sData;
                }
            }
        }


        internal void OnSearchComboBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                e.Handled = true;
                StartSearch(null);
            }
        }

        internal void OnSearchComboBoxDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                if ((e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link)
                {
                    e.Effect = DragDropEffects.Link;
                }
                else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                {
                    e.Effect = DragDropEffects.Move;
                }
                else if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        internal void OnSearchComboBoxDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
            {
                var sData = (string) e.Data.GetData(typeof (string));
                WebSearchText = sData;
            }
        }

        #endregion

        #region html control events

        private void OnHtmlWindowError(string description, string url, int line)
        {
            /* don't show script error dialog and don't disable script due to a single script error */
			if (_docContainer.ActiveDocument != null && _docContainer.ActiveDocument.Controls.Count > 0)
			{
				var hc = (HtmlControl)_docContainer.ActiveDocument.Controls[0];

				if (hc != null)
				{
					var window = (IHTMLWindow2)hc.Document2.GetParentWindow();
					IHTMLEventObj eventObj = window.eventobj;
					eventObj.ReturnValue = true;
				}
			}
        }


        private void OnWebStatusTextChanged(object sender, BrowserStatusTextChangeEvent e)
        {
            SetBrowserStatusBarText(e.text);
        }

        private void OnWebBeforeNavigate(object sender, BrowserBeforeNavigate2Event e)
        {
            bool userNavigates = _webUserNavigated;
            bool forceNewTab = _webForceNewTab;

            string url = e.url;

            if (!url.ToLower().StartsWith("javascript:"))
            {
                _webForceNewTab = _webUserNavigated = false; // reset, but keep it for the OnWebBeforeNewWindow event
            }

            if (!url.Equals("about:blank"))
            {
                if (owner.InterceptUrlNavigation(url))
                {
                    e.Cancel = true;
                    return;
                }

                if (url.StartsWith("mailto:") || url.StartsWith("news:"))
                {
                    //TODO: if nntp is impl., InterceptUrlNavigation() should handle "news:"
                    return;
                }

                bool framesAllowed = false;
                bool forceSetFocus = true;
                bool tabCanClose = true;
                // if Ctrl-Click is true, Tab opens in background:
                if ((ModifierKeys & Keys.Control) == Keys.Control)
                    forceSetFocus = false;

                var hc = sender as HtmlControl;
                if (hc != null)
                {
                    var dc = (DockControl) hc.Tag;
                    var ts = (ITabState) dc.Tag;
                    tabCanClose = ts.CanClose;
                    framesAllowed = hc.FrameDownloadEnabled;
                }

                if (userNavigates && UrlRequestHandledExternally(url, forceNewTab))
                {
                    e.Cancel = true;
                    return;
                }

                if (!tabCanClose && !userNavigates && !forceNewTab)
                {
                    if (!framesAllowed)
                        e.Cancel = !e.IsRootPage; // prevent sub-sequent requests of <iframe>'s
                    // else just allow navigate in current browser
                    return;
                }

                if ((!tabCanClose && userNavigates) || forceNewTab)
                {
                    e.Cancel = true;
                    // Delay gives time to the sender control to cancel request
                    DelayTask(DelayedTasks.NavigateToWebUrl, new object[] {url, null, forceNewTab, forceSetFocus});
                }
            }
        }

        private void OnWebNavigateComplete(object sender, BrowserNavigateComplete2Event e)
        {
            // if we cancelled subsequent requests in the WebBeforeNavigate event,
            // we may not receive the OnWebDocumentComplete event for the master page
            // so in general we do the same things here as in OnWebDocumentComplete()
            try
            {
                var hc = (HtmlControl) sender;

                //handle script errors on page
                var window = (HTMLWindowEvents2_Event) hc.Document2.GetParentWindow();
                window.onerror += OnHtmlWindowError;

                if (!string.IsNullOrEmpty(e.url) && e.url != "about:blank" && e.IsRootPage)
                {
                    AddUrlToHistory(e.url);

                    var doc = (DockControl) hc.Tag;
                    var state = (ITabState) doc.Tag;
                    state.Url = e.url;
                    RefreshDocumentState(doc);
                    // we should only discover once per browse action (in OnWebDocumentComplete()):
                    //owner.BackgroundDiscoverFeedsHandler.DiscoverFeedInContent(hc.DocumentOuterHTML, state.Url, null);
                    // do some more things here, because we may also not receive the events...
                    DelayTask(DelayedTasks.ClearBrowserStatusInfo, null, 2000);
                }
            }
            catch (Exception ex)
            {
                _log.Error("OnWebNavigateComplete(): " + e.url, ex);
            }
        }


        private void OnWebDocumentComplete(object sender, BrowserDocumentCompleteEvent e)
        {
            try
            {
                var hc = (HtmlControl) sender;

                //handle script errors on page
                var window = (HTMLWindowEvents2_Event) hc.Document2.GetParentWindow();
                window.onerror += OnHtmlWindowError;

                if (!string.IsNullOrEmpty(e.url) && e.url != "about:blank" && e.IsRootPage)
                {
                    AddUrlToHistory(e.url);

                    var doc = (DockControl) hc.Tag;
                    var state = (ITabState) doc.Tag;
                    state.Url = e.url;
                    RefreshDocumentState(doc);
                    owner.BackgroundDiscoverFeedsHandler.DiscoverFeedInContent(hc.DocumentInnerHTML, state.Url,
                                                                               state.Title);
                }
            }
            catch (Exception ex)
            {
                _log.Error("OnWebDocumentComplete(): " + e.url, ex);
            }
        }

        private void OnWebTitleChanged(object sender, BrowserTitleChangeEvent e)
        {
            try
            {
                var hc = (HtmlControl) sender;
                if (hc == null) return;
                var doc = (DockControl) hc.Tag;
                if (doc == null) return;
                var state = (ITabState) doc.Tag;
                if (state == null) return;

                state.Title = e.text;
                RefreshDocumentState(doc);
            }
            catch (Exception ex)
            {
                _log.Error("OnWebTitleChanged()", ex);
            }
        }

        private static void OnWebCommandStateChanged(object sender, BrowserCommandStateChangeEvent e)
        {
            try
            {
                ITabState state = GetTabStateFor(sender as HtmlControl);
                if (state == null) return;

                if (e.command == CommandStateChangeConstants.CSC_NAVIGATEBACK)
                    state.CanGoBack = e.enable;
                else if (e.command == CommandStateChangeConstants.CSC_NAVIGATEFORWARD)
                    state.CanGoForward = e.enable;
                else if (e.command == CommandStateChangeConstants.CSC_UPDATECOMMANDS)
                {
                    // 
                }
            }
            catch (Exception ex)
            {
                _log.Error("OnWebCommandStateChanged() ", ex);
            }
        }

        private void OnWebNewWindow(object sender, BrowserNewWindowEvent e)
        {
			ConfiguredWebBrowserNewWindowAction(e.url, true);
			e.Cancel = true;
			
			//try
			//{
			//    bool userNavigates = _webUserNavigated;
			//    bool forceNewTab = _webForceNewTab;

			//    _webForceNewTab = _webUserNavigated = false; // reset

			//    e.Cancel = true;

			//    string url = e.url;
			//    _log.Debug("OnWebNewWindow(): '" + url + "'");

			//    bool forceSetFocus = true;
			//    // Tab in background, but IEControl does NOT display/render!!!    !(Interop.GetAsyncKeyState(Interop.VK_MENU) < 0);

			//    if (UrlRequestHandledExternally(url, forceNewTab))
			//    {
			//        return;
			//    }

			//    if (userNavigates)
			//    {
			//        // Delay gives time to the sender control to cancel request
			//        DelayTask(DelayedTasks.NavigateToWebUrl, new object[] {url, null, true, forceSetFocus});
			//    }
			//}
			//catch (Exception ex)
			//{
			//    _log.Error("OnWebNewWindow(): " + e.url, ex);
			//}
        }

		// because this event gets fired without a BeforeNavigate(), we
		// have to handle such things like "Ctrl-Click" again here
		private void OnWebNewWindow3(object sender, BrowserNewWindow3Event e)
		{
			if (IEControl.Interop.NWMF.NWMF_FORCETAB == (e.dwFlags & IEControl.Interop.NWMF.NWMF_FORCETAB))
			{
				bool forceSetFocus = true;
				// if Ctrl-Click is true, Tab should open in background:
				if ((ModifierKeys & Keys.Control) == Keys.Control)
					forceSetFocus = false;
				ConfiguredWebBrowserNewWindowAction(e.bstrUrl, forceSetFocus);
			} 
			else
			{
				owner.NavigateToUrlInExternalBrowser(e.bstrUrl);
			}

			// if we do not cancel here, we would get the OnWebNewWindow event too:
			e.Cancel = true;
		}

		private void ConfiguredWebBrowserNewWindowAction(string url, bool forceSetFocus)
		{
			try
			{
				bool userNavigates = _webUserNavigated;
				bool forceNewTab = _webForceNewTab;

				_webForceNewTab = _webUserNavigated = false; // reset

				//const bool forceSetFocus = true;
				// Tab in background, but IEControl does NOT display/render!!!    !(Interop.GetAsyncKeyState(Interop.VK_MENU) < 0);

				if (UrlRequestHandledExternally(url, forceNewTab))
				{
					return;
				}

				if (userNavigates)
				{
					// Delay gives time to the sender control to cancel request
					DelayTask(DelayedTasks.NavigateToWebUrl, new object[] { url, null, true, forceSetFocus });
				}
			}
			catch (Exception ex)
			{
				_log.Error("ConfiguredWebBrowserNewWindowAction(): " + url, ex);
			}
		}

    	private void OnWebQuit(object sender, EventArgs e)
        {
            try
            {
                // javscript want to close this window: so we have to close the tab
                RemoveDocTab(_docContainer.ActiveDocument);
            }
            catch (Exception ex)
            {
                _log.Error("OnWebQuit()", ex);
            }
        }

        private void OnWebTranslateAccelerator(object sender, KeyEventArgs e)
        {
            try
            {
                // we use Control.ModifierKeys, because e.Shift etc. is not always set!
                bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;
                bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control;
                bool alt = (ModifierKeys & Keys.Alt) == Keys.Alt;
                bool noModifier = (!shift && !ctrl && !alt);

                bool shiftOnly = (shift && !ctrl && !alt);
                bool ctrlOnly = (ctrl && !shift && !alt);
                bool ctrlShift = (ctrl && shift && !alt);

                if (_shortcutHandler.IsCommandInvoked("BrowserCreateNewTab", e.KeyData))
                {
                    // capture Ctrl-N event or whichever combination is configured (new window)
                    owner.CmdBrowserCreateNewTab(null);
                    e.Handled = true;
                }
                if (_shortcutHandler.IsCommandInvoked("Help", e.KeyData))
                {
                    // capture F1 (or whichever keys are configured) event (help)
                    Help.ShowHelp(this, helpProvider1.HelpNamespace, HelpNavigator.TableOfContents);
                    e.Handled = true;
                }

                if (!e.Handled)
                {
                    // prevent double handling of shortcuts:
                    // IE will handle this codes by itself even if a user configures other shortcuts
                    // than Ctrl-N and F1.
                    e.Handled = (e.KeyCode == Keys.N && ctrlOnly ||
                                 e.KeyCode == Keys.F1);
                }

                if (!e.Handled)
                {
                    // support: continue tab order throw the other controls than IEControl
                    if (e.KeyCode == Keys.Tab && noModifier)
                    {
                        if (htmlDetail.Document2 != null && null == htmlDetail.Document2.GetActiveElement())
                        {
                            // one turn around within ALink element classes
                            if (treeFeeds.Visible)
                            {
                                treeFeeds.Focus();
                                e.Handled = true;
                            }
                            else if (listFeedItems.Visible)
                            {
                                listFeedItems.Focus();
                                e.Handled = true;
                            }
                        }
                    }
                    else if (e.KeyCode == Keys.Tab && shiftOnly)
                    {
                        if (htmlDetail.Document2 != null && null == htmlDetail.Document2.GetActiveElement())
                        {
                            // one reverse turn around within ALink element classes
                            if (listFeedItems.Visible)
                            {
                                listFeedItems.Focus();
                                e.Handled = true;
                            }
                            else if (treeFeeds.Visible)
                            {
                                treeFeeds.Focus();
                                e.Handled = true;
                            }
                        }
                    }
                }

                if (!e.Handled)
                {
                    // support: Ctrl-Tab/Shift-Ctrl-Tab switch Browser Tabs
                    if (e.KeyCode == Keys.Tab && ctrlOnly)
                    {
                        // step forward:
                        if (_docContainer.Documents.Length > 1)
                        {
                            InvokeProcessCmdKey(_docContainer.ActiveDocument, Keys.Next | Keys.Control);
                            e.Handled = true;
                        }
                    }
                    else if (e.KeyCode == Keys.Tab && ctrlShift)
                    {
                        // step backward:
                        if (_docContainer.Documents.Length > 1)
                        {
                            InvokeProcessCmdKey(_docContainer.ActiveDocument, Keys.Prior | Keys.Control);
                            e.Handled = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("OnWebTranslateAccelerator(): " + e.KeyCode, ex);
            }
        }

        private void InvokeProcessCmdKey(DockControl c, Keys keyData)
        {
            if (c != null)
            {
                Type cType = c.GetType();
                try
                {
                    // just a dummy message:
                    Message m = Message.Create(Handle, (int) Win32.Message.WM_NULL, IntPtr.Zero, IntPtr.Zero);
                    cType.InvokeMember("ProcessCmdKey",
                                       BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod,
                                       null, c, new object[] {m, keyData});
                }
                catch (Exception ex)
                {
                    _log.Error("InvokeProcessCmdKey() failed: " + ex.Message);
                }
            }
        }

        private void OnWebProgressChanged(object sender, BrowserProgressChangeEvent e)
        {
            try
            {
                if (_lastBrowserThatProgressChanged == null)
                    _lastBrowserThatProgressChanged = sender;

                if (sender != _lastBrowserThatProgressChanged)
                {
                    DeactivateWebProgressInfo();
                    return;
                }

                if (((e.progress < 0) || (e.progressMax <= 0)) || (e.progress >= e.progressMax))
                {
                    DeactivateWebProgressInfo();
                }
                else
                {
                    if (!progressBrowser.Visible) progressBrowser.Visible = true;
                    if (statusBarBrowserProgress.Width < BrowserProgressBarWidth)
                    {
                        statusBarBrowserProgress.Width = BrowserProgressBarWidth;
                        progressBrowser.Width = BrowserProgressBarWidth - 12;
                    }
                    progressBrowser.Minimum = 0;
                    progressBrowser.Maximum = e.progressMax;
                    progressBrowser.Value = e.progress;
                }
            }
            catch (Exception ex)
            {
                _log.Error("OnWebProgressChanged()", ex);
            }
        }

        private object _lastBrowserThatProgressChanged;

        private void DeactivateWebProgressInfo()
        {
            progressBrowser.Minimum = 0;
            progressBrowser.Maximum = 128;
            progressBrowser.Value = 128;
            progressBrowser.Visible = false;
            statusBarBrowserProgress.Width = 0;
            _lastBrowserThatProgressChanged = null;
        }

        #endregion

        #endregion

    	
    }
}