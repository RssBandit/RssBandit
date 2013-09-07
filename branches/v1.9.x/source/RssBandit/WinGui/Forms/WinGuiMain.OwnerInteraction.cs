using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Genghis.Windows.Forms;
using RssBandit.WinGui.Controls.ThListView;
using IEControl;
using Infragistics.Win.UltraWinTree;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Net;
using NewsComponents.Search;
using NewsComponents.Utils;
using RssBandit.Common;
using RssBandit.Resources;
using RssBandit.SpecialFeeds;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Dialogs;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Menus;
using RssBandit.WinGui.Tools;
using RssBandit.WinGui.Utility;
using TD.SandDock;

using Microsoft.WindowsAPICodePack.Taskbar;

namespace RssBandit.WinGui.Forms
{
    internal partial class WinGuiMain
    {
        /// <summary>
        /// Extended Close.
        /// </summary>
        /// <param name="forceShutdown"></param>
        public void Close(bool forceShutdown)
        {
            //this.SaveUIConfiguration(forceShutdown);
            _forceShutdown = forceShutdown;
            Close();
        }

        public void SaveUIConfiguration(bool forceFlush)
        {
            try
            {
                OnSaveConfig(owner.GuiSettings);
                SaveSubscriptionTreeState();
                SaveBrowserTabState();
                listFeedItems.CheckForLayoutModifications();

                if (forceFlush)
                {
                    owner.GuiSettings.Flush();
                }
            }
            catch (Exception ex)
            {
                _log.Error("Save .settings.xml failed", ex);
            }
        }

        internal bool LoadAndRestoreBrowserTabState()
        {
            _browserTabsRestored = true;

            string fileName = RssBanditApplication.GetBrowserTabStateFileName();
            try
            {
                if (!File.Exists(fileName))
                    return false;
                using (Stream stream = FileHelper.OpenForRead(fileName))
                {
                    SerializableWebTabState state = SerializableWebTabState.Load(stream);

                    foreach (string url in state.Urls)
                    {
                        try
                        {
                            DetailTabNavigateToUrl(url, String.Empty /* tab title */, true /* createNewTab */, false
                                /* setFocus */);
                        }
                        catch (AxHost.InvalidActiveXStateException)
                        {
                            /* occurs if we are starting from sys tray because browser not visible */
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Load " + fileName + " failed", ex);
                return false;
            }
        }


        internal void SaveBrowserTabState()
        {
            string fileName = RssBanditApplication.GetBrowserTabStateFileName();
            var state = new SerializableWebTabState();

            try
            {
                foreach (var doc in _docContainer.Documents)
                {
                    var docState = (ITabState) doc.Tag;

                    if ((docState != null) && docState.CanClose)
                    {
                        state.Urls.Add(docState.Url);
                    }
                }

                using (Stream stream = FileHelper.OpenForWrite(fileName))
                {
                    SerializableWebTabState.Save(stream, state);
                }
            }
            catch (Exception ex)
            {
                _log.Error("Save " + fileName + " failed", ex);
                // don't cause a load problem later on if save failed:
                try
                {
                    File.Delete(fileName);
                }
                catch (IOException)
                {
                }
            }
        }

        internal void SaveSubscriptionTreeState()
        {
            string fileName = RssBanditApplication.GetSubscriptionTreeStateFileName();
            try
            {
                using (Stream s = FileHelper.OpenForWrite(fileName))
                {
                    UltraTreeNodeExpansionMemento.Save(s, treeFeeds);
                }
            }
            catch (Exception ex)
            {
                //TR: do not bummer user with this file errors (called on AutoSave).
                //Just log - and try to recover (delete the bad file)
                //RssBanditApplication.PublishException(ex);
                _log.Error("Save " + fileName + " failed", ex);
                // don't cause a load problem later on if save failed:
                try
                {
                    File.Delete(fileName);
                }
                catch (IOException)
                {
                }
            }
        }

        internal bool LoadAndRestoreSubscriptionTreeState()
        {
            string fileName = RssBanditApplication.GetSubscriptionTreeStateFileName();
            try
            {
                if (!File.Exists(fileName))
                    return false;
                using (Stream s = FileHelper.OpenForRead(fileName))
                {
                    UltraTreeNodeExpansionMemento m = UltraTreeNodeExpansionMemento.Load(s);
                    m.Restore(treeFeeds);
                }
                return true;
            }
            catch (Exception ex)
            {
                SetDefaultExpansionTreeNodeState();
                //TR: inform user about file error
                owner.MessageWarn(String.Format(SR.GUILoadFileOperationExceptionMessage, fileName, ex.Message,
                                                SR.GUIUserInfoAboutDefaultTreeState));
                //And log - recover may happen on save (delete the bad file)
                _log.Error("Load " + fileName + " failed", ex);
                return false;
            }
        }

        private static bool IsTreeStateAvailable()
        {
            try
            {
                return File.Exists(RssBanditApplication.GetSubscriptionTreeStateFileName());
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void InitiatePopulateTreeFeeds()
        {
        	
			//Ensure we update the UI in the correct thread. Since this method is likely 
			//to have been called from a thread that is not the UI thread we should ensure 
			//that calls to UI components are actually made from the UI thread or marshalled
			//accordingly. 
            if (this.InvokeRequired)
				Debug.Assert(false, "Ensure, this gets called on the UI thread!");
			
			if (owner == null)
            {
                //Probably should log an error here
                SetGuiStateFeedback(String.Empty, ApplicationTrayState.NormalIdle);
                return;
            }

			// contains unread folder (required immediately, if a feed yet gets updated):
			PopulateTreeSpecialFeeds();

        	List<FeedSourceEntry> visibleSources = new List<FeedSourceEntry>(owner.FeedSources.Count);
			List<FeedSourceEntry> remainingSources = new List<FeedSourceEntry>(owner.FeedSources.Count);

			// for better user experience we populate the visible sources first:
			foreach (SubscriptionRootNode n in GetVisibleSubscriptionRootNodes())
				visibleSources.Add(owner.FeedSources[n.SourceID]);
			
			foreach (FeedSourceEntry entry in owner.FeedSources.Sources)
			{
				if (entry.Source.FeedsListOK)
				{
					if (visibleSources.Contains(entry))
					{
						PopulateFeedSubscriptions(entry, RssBanditApplication.DefaultCategory);
                                
					} else
					{
						remainingSources.Add(entry);
					}
				} 
				else
				{
					_log.Error("Feed source reported list was not OK: " + entry.Name);
				}
			}

			// now the remaining sources:
			foreach (FeedSourceEntry entry in remainingSources)
			{
				if (entry.Source.FeedsListOK)
				{
					PopulateFeedSubscriptions(entry, RssBanditApplication.DefaultCategory);
				}
				else
				{
					_log.Error("Feed source reported list was not OK: " + entry.Name);
				}
			}		
        }

        private void CheckForFlaggedNodeAndCreate(INewsItem ri)
        {
            ISmartFolder isf;
            TreeFeedsNodeBase tn = null;
            TreeFeedsNodeBase root = _flaggedFeedsNodeRoot; //this.GetRootType(RootFolderType.SmartFolders);

            if (ri.FlagStatus == Flagged.FollowUp && _flaggedFeedsNodeFollowUp == null)
            {
                // not yet created
                _flaggedFeedsNodeFollowUp = new FlaggedItemsNode(Flagged.FollowUp, owner.FlaggedItemsFeed,
                                                                 SR.FeedNodeFlaggedForFollowUpCaption,
                                                                 Resource.SubscriptionTreeImage.RedFlag,
                                                                 Resource.SubscriptionTreeImage.RedFlagSelected,
                                                                 _treeLocalFeedContextMenu);
                root.Nodes.Add(_flaggedFeedsNodeFollowUp);
                isf = _flaggedFeedsNodeFollowUp as ISmartFolder;
                tn = _flaggedFeedsNodeFollowUp;
                if (isf != null) isf.UpdateReadStatus();
            }
            else if (ri.FlagStatus == Flagged.Read && _flaggedFeedsNodeRead == null)
            {
                // not yet created
                _flaggedFeedsNodeRead = new FlaggedItemsNode(Flagged.Read, owner.FlaggedItemsFeed,
                                                             SR.FeedNodeFlaggedForReadCaption,
                                                             Resource.SubscriptionTreeImage.GreenFlag,
                                                             Resource.SubscriptionTreeImage.GreenFlagSelected,
                                                             _treeLocalFeedContextMenu);
                root.Nodes.Add(_flaggedFeedsNodeRead);
                isf = _flaggedFeedsNodeRead as ISmartFolder;
                tn = _flaggedFeedsNodeRead;
                if (isf != null) isf.UpdateReadStatus();
            }
            else if (ri.FlagStatus == Flagged.Review && _flaggedFeedsNodeReview == null)
            {
                // not yet created
                _flaggedFeedsNodeReview = new FlaggedItemsNode(Flagged.Review, owner.FlaggedItemsFeed,
                                                               SR.FeedNodeFlaggedForReviewCaption,
                                                               Resource.SubscriptionTreeImage.YellowFlag,
                                                               Resource.SubscriptionTreeImage.YellowFlagSelected,
                                                               _treeLocalFeedContextMenu);
                root.Nodes.Add(_flaggedFeedsNodeReview);
                isf = _flaggedFeedsNodeReview as ISmartFolder;
                tn = _flaggedFeedsNodeReview;
                if (isf != null) isf.UpdateReadStatus();
            }
            else if (ri.FlagStatus == Flagged.Forward && _flaggedFeedsNodeForward == null)
            {
                // not yet created
                _flaggedFeedsNodeForward = new FlaggedItemsNode(Flagged.Forward, owner.FlaggedItemsFeed,
                                                                SR.FeedNodeFlaggedForForwardCaption,
                                                                Resource.SubscriptionTreeImage.BlueFlag,
                                                                Resource.SubscriptionTreeImage.BlueFlagSelected,
                                                                _treeLocalFeedContextMenu);
                root.Nodes.Add(_flaggedFeedsNodeForward);
                isf = _flaggedFeedsNodeForward as ISmartFolder;
                tn = _flaggedFeedsNodeForward;
                if (isf != null) isf.UpdateReadStatus();
            }
            else if (ri.FlagStatus == Flagged.Reply && _flaggedFeedsNodeReply == null)
            {
                // not yet created
                _flaggedFeedsNodeReply = new FlaggedItemsNode(Flagged.Reply, owner.FlaggedItemsFeed,
                                                              SR.FeedNodeFlaggedForReplyCaption,
                                                              Resource.SubscriptionTreeImage.ReplyFlag,
                                                              Resource.SubscriptionTreeImage.ReplyFlagSelected,
                                                              _treeLocalFeedContextMenu);
                root.Nodes.Add(_flaggedFeedsNodeReply);
                isf = _flaggedFeedsNodeReply as ISmartFolder;
                tn = _flaggedFeedsNodeReply;
                if (isf != null) isf.UpdateReadStatus();
            }

            if (tn != null)
            {
                // overall settings
                tn.DataKey = owner.FlaggedItemsFeed.link + "?id=" + ri.FlagStatus;
            }
            //InitFeedDetailsCaption();
        }

        public void PopulateTreeSpecialFeeds()
        {
            treeFeeds.BeginUpdate();

            TreeFeedsNodeBase root = GetRoot(RootFolderType.SmartFolders);
            root.Nodes.Clear();

            _feedExceptionsFeedsNode = new ExceptionReportNode(SR.FeedNodeFeedExceptionsCaption,
                                                               Resource.SubscriptionTreeImage.Exceptions,
                                                               Resource.SubscriptionTreeImage.ExceptionsSelected,
                                                               _treeLocalFeedContextMenu)
                                           {
                                               DataKey = ExceptionManager.GetInstance().link
                                           };
            ExceptionNode.UpdateReadStatus();

            _sentItemsFeedsNode = new SentItemsNode(owner.SentItemsFeed,
                                                    Resource.SubscriptionTreeImage.SentItems,
                                                    Resource.SubscriptionTreeImage.SentItems, _treeLocalFeedContextMenu)
                                      {
                                          DataKey = owner.SentItemsFeed.link
                                      };
            SentItemsNode.UpdateReadStatus();

            _watchedItemsFeedsNode = new WatchedItemsNode(owner.WatchedItemsFeed,
                                                          Resource.SubscriptionTreeImage.WatchedItems,
                                                          Resource.SubscriptionTreeImage.WatchedItemsSelected,
                                                          _treeLocalFeedContextMenu)
                                         {
                                             DataKey = owner.WatchedItemsFeed.link
                                         };
            WatchedItemsNode.UpdateReadStatus();
            WatchedItemsNode.UpdateCommentStatus();

            _unreadItemsFeedsNode = new UnreadItemsNode(owner.UnreadItemsFeed,
                                                        Resource.SubscriptionTreeImage.WatchedItems,
                                                        Resource.SubscriptionTreeImage.WatchedItemsSelected,
                                                        _treeLocalFeedContextMenu)
                                        {
                                            DataKey = owner.UnreadItemsFeed.link
                                        };
            UnreadItemsNode.UpdateReadStatus();

            _deletedItemsFeedsNode = new WasteBasketNode(owner.DeletedItemsFeed,
                                                         Resource.SubscriptionTreeImage.WasteBasketEmpty,
                                                         Resource.SubscriptionTreeImage.WasteBasketEmpty,
                                                         _treeLocalFeedContextMenu)
                                         {
                                             DataKey = owner.DeletedItemsFeed.link
                                         };
            DeletedItemsNode.UpdateReadStatus();

            _flaggedFeedsNodeRoot = new FlaggedItemsRootNode(SR.FeedNodeFlaggedFeedsCaption,
                                                             Resource.SubscriptionTreeImage.SubscriptionsCategory,
                                                             Resource.SubscriptionTreeImage.
                                                                 SubscriptionsCategoryExpanded,
                                                             null);

            root.Nodes.AddRange(
                new UltraTreeNode[]
                    {
                        _unreadItemsFeedsNode,
                        _watchedItemsFeedsNode,
                        _flaggedFeedsNodeRoot,
                        _feedExceptionsFeedsNode,
                        _sentItemsFeedsNode,
                        _deletedItemsFeedsNode
                    });

            // method gets called more than once, reset the nodes:
            _flaggedFeedsNodeFollowUp = _flaggedFeedsNodeRead = null;
            _flaggedFeedsNodeReview = _flaggedFeedsNodeForward = null;
            _flaggedFeedsNodeReply = null;

            foreach (NewsItem ri in owner.FlaggedItemsFeed.Items)
            {
                CheckForFlaggedNodeAndCreate(ri);

                if (_flaggedFeedsNodeFollowUp != null && _flaggedFeedsNodeRead != null &&
                    _flaggedFeedsNodeReview != null && _flaggedFeedsNodeForward != null &&
                    _flaggedFeedsNodeReply != null)
                {
                    break;
                }
            }

            bool expandRoots = !IsTreeStateAvailable();
            root.Expanded = expandRoots;

            var froot = (FinderRootNode) GetRoot(RootFolderType.Finder);
            SyncFinderNodes(froot);
            if (expandRoots)
                froot.ExpandAll();

            treeFeeds.EndUpdate();
        }

        public void SyncFinderNodes()
        {
            SyncFinderNodes((FinderRootNode) GetRoot(RootFolderType.Finder));
        }

        private void SyncFinderNodes(FinderRootNode finderRoot)
        {
            if (finderRoot == null)
                return;
            finderRoot.Nodes.Clear();
            finderRoot.InitFromFinders(owner.FinderList, _treeSearchFolderContextMenu);
        }

        public void PopulateFeedSubscriptions(FeedSourceEntry entry,
                                              string defaultCategory)
        {
            TreeFeedsNodeBase root = GetSubscriptionRootNode(entry.Name);
			try
			{
				ICollection<INewsFeedCategory> categories = entry.Source.GetCategories().Values;
				IDictionary<string, INewsFeed> feedsTable = entry.Source.GetFeeds();

				treeFeeds.BeginUpdate();

                TreeFeedsNodeBase tn;
                
				// reset nodes and unread counter
                root.Nodes.Clear();
                UpdateTreeNodeUnreadStatus(root, 0);

				if (entry.Source.FeedsListOK)
				{
					//UnreadItemsNode.Items.Clear();

					var categoryTable = new Dictionary<string, TreeFeedsNodeBase>();
					var categoryList = new List<INewsFeedCategory>(categories);

					foreach (var f in feedsTable.Values)
					{
						if (Disposing)
							return;

						if (RssHelper.IsNntpUrl(f.link))
						{
							tn = new FeedNode(
								f.title, Resource.SubscriptionTreeImage.Nntp,
								Resource.SubscriptionTreeImage.NntpSelected,
								_treeFeedContextMenu);
						}
						else
						{
							tn = new FeedNode(
								f.title, Resource.SubscriptionTreeImage.Feed,
								Resource.SubscriptionTreeImage.FeedSelected,
								_treeFeedContextMenu,
								(owner.Preferences.UseFavicons ? LoadCachedFavicon(entry.Source, f) : null));
						}

						//interconnect for speed:
						tn.DataKey = f.link;
						f.Tag = tn;

						string category = (f.category ?? String.Empty);

						TreeFeedsNodeBase catnode;
						if (categoryTable.ContainsKey(category))
							catnode = categoryTable[category];
						else
						{
							catnode = TreeHelper.CreateCategoryHive(root, category, _treeCategoryContextMenu);
							categoryTable.Add(category, catnode);
						}

						catnode.Nodes.Add(tn);

						SetSubscriptionNodeState(f, tn, FeedProcessingState.Normal);

						if (f.containsNewMessages)
						{
							IList<INewsItem> unread = FilterUnreadFeedItems(f);
							if (unread.Count > 0)
							{
								// we build up the tree, so the call to 
								// UpdateReadStatus(tn , 0) is not neccesary:
								UpdateTreeNodeUnreadStatus(tn, unread.Count);
								UnreadItemsNode.Items.AddRange(unread);
								UnreadItemsNode.UpdateReadStatus();
							}
						}

						if (f.containsNewComments)
						{
							UpdateCommentStatus(tn, f);
						}

						for (int i = 0; i < categoryList.Count; i++)
						{
							if (categoryList[i].Value.Equals(category))
							{
								categoryList.RemoveAt(i);
								break;
							}
						}
					}

					//add categories, we not already have
					foreach (var c in categoryList)
					{
						TreeHelper.CreateCategoryHive(root, c.Value, _treeCategoryContextMenu);
					}
				} 
				else
				{
					//TODO: indicate the error in the UI
				}

			}
            finally
            {
                treeFeeds.EndUpdate();
            }

            if (Disposing)
                return;

			if (root.Visible)
				TreeSelectedFeedsNode = root;

			//from here these are candidates for the AllSubscriptionsLoaded event:
            if (!IsTreeStateAvailable())
                root.Expanded = true;

            // also this one:
            DelayTask(DelayedTasks.SyncRssSearchTree);

            //we'll need to fetch favicons for the newly loaded/imported feeds
            entry.FaviconsDownloaded = false;
        }

        /// <summary>
        /// Sets the default expansion tree node states.
        /// Currently expand all root nodes.
        /// </summary>
        internal void SetDefaultExpansionTreeNodeState()
        {
			foreach (TreeFeedsNodeBase node in treeFeeds.Nodes)
				node.Expanded = true;
			//foreach (var node in _roots)
			//    node.Expanded = true;
        }

        /// <summary>
        /// This opens the downloaded file in the users target application associated with that 
        /// file type. 
        /// </summary>
        /// <param name="enclosure">The enclosure to launch or play</param>
        private void PlayEnclosure(DownloadItem enclosure)
        {
            if (enclosure == null)
                return;

            string fileName = Path.Combine(enclosure.TargetFolder, enclosure.File.LocalName);

            if (string.IsNullOrEmpty(fileName))
                return;
            try
            {
                using (var p = new Process())
                {
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.FileName = fileName;
                    p.Start();
                }
            }
            catch (Exception ex)
            {
                //we don't want to show the user an error if they cancelled executing the file 
                //after getting a security prompt. 
                var ex32 = ex as Win32Exception;
                if ((ex32 == null) || (ex32.NativeErrorCode != 1223))
                {
                    owner.MessageError(String.Format(SR.ExceptionProcessStartToPlayEnclosure, fileName, ex.Message));
                    RssBanditApplication.PublishException(ex);
                }
            }
        }

		/// <summary>
		/// Loads a favicon from the cache
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="feed">The feed.</param>
		/// <returns>
		/// The favicon as an image or null if there was an error loading the image
		/// </returns>
        private Image LoadCachedFavicon(FeedSource source, INewsFeed feed)
		{
			return FaviconCache.GetImage(source, feed);
		}
		
		private void PopulateTreeRssSearchScope()
        {
            if (searchPanel != null)
				searchPanel.PopulateTreeRssSearchScope(GetSubscriptionRootNode(CurrentSelectedFeedSource), _treeImages);
        }

        /// <summary>
        /// Used to jump/navigate a web-link (url). Function will create 
        /// on demand a tabpage named in parameter <c>tab</c>, move it to front
        /// and open/navigate a web browser with the provided <c>url</c>.
        /// </summary>
        /// <param name="url">Web-Link to navigate to</param>
        /// <param name="tab">tabpage title name</param>
        /// <param name="createNewTab">true to force creation of a new Tab</param>
        /// <param name="setFocus">true to force brower Tab activation (move to foreground, set focus)</param>
        public void DetailTabNavigateToUrl(string url, string tab, bool createNewTab, bool setFocus)
        {
            Debug.Assert(!InvokeRequired, "DetailTabNavigateToUrl() from Non-UI Thread called");

            if (owner.Preferences.OpenNewTabsInBackground)
            {
                setFocus = false;
            }

            if (string.IsNullOrEmpty(url))
                return;

            if (url == "about:blank" && !createNewTab)
                return;

            if (string.IsNullOrEmpty(tab))
                tab = "Web Link";

            HtmlControl hc = null;

            DockControl currentDoc;
            DockControl previousDoc = currentDoc = _docContainer.ActiveDocument;
            var docState = (ITabState) currentDoc.Tag;

            if (!docState.CanClose)
            {
                // Feed Detail doc tab

                if (!createNewTab && owner.Preferences.ReuseFirstBrowserTab)
                {
                    foreach (DockControl c in currentDoc.LayoutSystem.Controls)
                    {
                        if (c != currentDoc)
                        {
                            // reuse first docTab not equal to news item listview container
                            hc = (HtmlControl) c.Controls[0];
                            break;
                        }
                    }
                }
            }
            else if (!createNewTab)
            {
                // web doc tab
                // reuse same tab
                hc = (HtmlControl) _docContainer.ActiveDocument.Controls[0];
            }

            if (hc == null)
            {
                // create new doc tab with a contained web browser

                hc = CreateAndInitIEControl(tab);
                var doc = new DockControl(hc, tab)
                              {
                                  Tag = new WebTabState(tab, url)
                              };
                hc.Tag = doc; // store the doc the browser belongs to
                _docContainer.AddDocument(doc);
                if (Win32.IsOSAtLeastWindowsXP)
                    ColorEx.ColorizeOneNote(doc, ++_webTabCounter);

                //old: do NOT activate, if the focus have not to be set!
                //hc.Activate();	// so users do not have to explicitly click into the browser area after navigation for keyboard scrolling, etc.
                if (setFocus)
                {
                    hc.Activate();
                    // so users do not have to explicitly click into the browser area after navigation for keyboard scrolling, etc.
                    currentDoc = (DockControl) hc.Tag;
                }
                else
                    doc.Activate();
            }
            else
            {
                currentDoc = (DockControl) hc.Tag;
            }

            // move to front, or keep the current			
            currentDoc.Activate();
            _docContainer.ActiveDocument = (setFocus ? currentDoc : previousDoc);

            hc.Navigate(url);
        }

        private HtmlControl CreateAndInitIEControl(string tabName)
        {
            var hc = new HtmlControl();
            var resources = new ComponentResourceManager(typeof (WinGuiMain));

            hc.BeginInit();
            // we just take over some generic resource settings from htmlDetail:
            hc.AllowDrop = true;
            resources.ApplyResources(hc, "htmlDetail");
            hc.Name = tabName ?? String.Empty;
            hc.OcxState = ((AxHost.State) (resources.GetObject("htmlDetail.OcxState")));
            hc.ContainingControl = this;
			hc.EndInit();

            hc.ScriptEnabled = owner.Preferences.BrowserJavascriptAllowed;
            hc.JavaEnabled = owner.Preferences.BrowserJavaAllowed;

            hc.ActiveXEnabled = owner.Preferences.BrowserActiveXAllowed;
            HtmlControl.SetInternetFeatureEnabled(
                InternetFeatureList.FEATURE_RESTRICT_ACTIVEXINSTALL,
                SetFeatureFlag.SET_FEATURE_ON_THREAD_INTERNET,
                hc.ActiveXEnabled);

            hc.BackroundSoundEnabled = owner.Preferences.BrowserBGSoundAllowed;
            hc.VideoEnabled = owner.Preferences.BrowserVideoAllowed;
            hc.ImagesDownloadEnabled = owner.Preferences.BrowserImagesAllowed;
            hc.SilentModeEnabled = true;
            hc.Border3d = true;

        	AttachEvents(hc, true);
            
            return hc;
        }

        private static ITabState GetTabStateFor(HtmlControl control)
        {
            if (control == null) return null;
            var doc = (DockControl) control.Tag;
            if (doc == null) return null;
            var state = (ITabState) doc.Tag;
            return state;
        }

        private bool UrlRequestHandledExternally(string url, bool forceNewTab)
        {
            if (forceNewTab || BrowserBehaviorOnNewWindow.OpenNewTab == owner.Preferences.BrowserOnNewWindow)
            {
                return false;
            }

            if (BrowserBehaviorOnNewWindow.OpenDefaultBrowser == owner.Preferences.BrowserOnNewWindow)
            {
                owner.NavigateToUrlInExternalBrowser(url);
            }
            else if (BrowserBehaviorOnNewWindow.OpenWithCustomExecutable == owner.Preferences.BrowserOnNewWindow)
            {
                try
                {
                    Process.Start(owner.Preferences.BrowserCustomExecOnNewWindow, url);
                }
                catch (Exception ex)
                {
                    if (
                        owner.MessageQuestion(String.Format(
                                                  SR.ExceptionStartBrowserCustomExecMessage,
                                                  owner.Preferences.BrowserCustomExecOnNewWindow,
                                                  ex.Message, url)) == DialogResult.Yes)
                    {
                        DetailTabNavigateToUrl(url, null, true, true);
                    }
                }
            }
            else
            {
                Debug.Assert(false, "Unhandled BrowserBehaviorOnNewWindow");
            }
            return true;
        }

        /// <summary>
        /// Used to initiate a browse action.
        /// </summary>
        /// <param name="action">The specific action to perform</param>
        public void RequestBrowseAction(BrowseAction action)
        {
            if (_docContainer.ActiveDocument == _docFeedDetails)
            {
                switch (action)
                {
                    case BrowseAction.NavigateBack:
#if PHOENIX
						ultraToolbarsManager.NavigationToolbar.NavigateBack();
#else
                        NavigateToHistoryEntry(_feedItemImpressionHistory.GetPrevious());
#endif
                        break;
                    case BrowseAction.NavigateForward:
#if PHOENIX
						ultraToolbarsManager.NavigationToolbar.NavigateForward();
#else
                        NavigateToHistoryEntry(_feedItemImpressionHistory.GetNext());
#endif
                        break;
                    case BrowseAction.DoRefresh:
                        OnTreeFeedAfterSelectManually(TreeSelectedFeedsNode); //??
                        break;
                    default:
                        break;
                }
            }
            else
            {
                // _docContainer.ActiveDocument != _docFeedDetails

                var wb = (HtmlControl) _docContainer.ActiveDocument.Controls[0];
                try
                {
                    switch (action)
                    {
                        case BrowseAction.NavigateCancel:
                            wb.Stop();
                            break;
                        case BrowseAction.NavigateBack:
                            wb.GoBack();
                            break;
                        case BrowseAction.NavigateForward:
                            wb.GoForward();
                            break;
                        case BrowseAction.DoRefresh:
                            object level = 2;
                            wb.Refresh2(ref level);
                            break;
                        default:
                            break;
                    }
                }
                catch
                {
                    /* Can't do command */
                    ;
                }
            }
            DeactivateWebProgressInfo();
        }

        /// <summary>
        /// Renders the context menu and determines which options are enabled/visible. 
        /// </summary>
        public void RefreshListviewContextMenu()
        {
            INewsItem item = null;
            IList<ThreadedListViewItem> selectedItems = GetSelectedLVItems();

            if (selectedItems.Count > 0)
                item = (selectedItems[0]).Key as INewsItem;
            RefreshListviewContextMenu(item, selectedItems.Count > 1);
        }

        /// <summary>
        /// Renders the context menu and determines which options are enabled/visible.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="multipleSelection">if set to <c>true</c> multiple items are selected.</param>
        public void RefreshListviewContextMenu(INewsItem item, bool multipleSelection)
        {
            if (item != null)
            {
                if (!multipleSelection)
                    owner.Mediator.SetVisible("+cmdWatchItemComments", "+cmdFeedItemPostReply");

                owner.Mediator.SetEnabled("+cmdCopyNewsItem", "+cmdFlagNewsItem", "+cmdDeleteSelectedNewsItems");

                if (listFeedItems.Visible)
                {
                    owner.Mediator.SetVisible("+cmdColumnChooserMain");
                }
                else
                {
                    owner.Mediator.SetVisible("-cmdColumnChooserMain");
                }

                if (!multipleSelection)
                {
                    if (item.BeenRead)
                        owner.Mediator.SetVisible("+cmdMarkSelectedFeedItemsUnread", "-cmdMarkSelectedFeedItemsRead");
                    else
                        owner.Mediator.SetVisible("-cmdMarkSelectedFeedItemsUnread", "+cmdMarkSelectedFeedItemsRead");
                }
                else
                    owner.Mediator.SetVisible("+cmdMarkSelectedFeedItemsUnread", "+cmdMarkSelectedFeedItemsRead");

                _listContextMenuDownloadAttachmentsSeparator.Visible = false;
                owner.Mediator.SetVisible("-cmdDownloadAttachment");

                if (!multipleSelection && item.Enclosures != null && item.Enclosures.Count > 0)
                {
                    _listContextMenuDownloadAttachmentsSeparator.Visible = true;
                    owner.Mediator.SetVisible("+cmdDownloadAttachment");
                    _listContextMenuDownloadAttachment.MenuItems.Clear();

                    foreach (Enclosure enc in item.Enclosures)
                    {
                        int index = enc.Url.LastIndexOf("/");
                        string fileName;

                        if ((index != -1) && (index + 1 < enc.Url.Length))
                        {
                            fileName = enc.Url.Substring(index + 1);
                        }
                        else
                        {
                            fileName = enc.Url;
                        }

                        var downloadFileMenuItem =
                            new AppContextMenuCommand("cmdDownloadAttachment<" + fileName,
                                                      owner.Mediator, new ExecuteCommandHandler(CmdDownloadAttachment),
                                                      fileName, fileName, _shortcutHandler);

                        _listContextMenuDownloadAttachment.MenuItems.AddRange(new MenuItem[] {downloadFileMenuItem});
                    }
                }

                owner.Mediator.SetChecked("-cmdWatchItemComments");
                owner.Mediator.SetEnabled("-cmdWatchItemComments");

                if (!multipleSelection)
                {
                    if (!string.IsNullOrEmpty(item.CommentRssUrl) && (item.CommentCount != NewsItem.NoComments))
                    {
                        owner.Mediator.SetEnabled("+cmdWatchItemComments");

                        if (item.WatchComments)
                        {
                            owner.Mediator.SetChecked("+cmdWatchItemComments");
                        }
                    }
                }
            }
            else
            {
                _listContextMenuDownloadAttachmentsSeparator.Visible = false;

                owner.Mediator.SetVisible("-cmdMarkSelectedFeedItemsUnread", "-cmdMarkSelectedFeedItemsRead");
                owner.Mediator.SetVisible("-cmdWatchItemComments", "-cmdColumnChooserMain", "-cmdFeedItemPostReply",
                                          "-cmdDownloadAttachment");
                owner.Mediator.SetEnabled("-cmdCopyNewsItem", "-cmdFlagNewsItem", "-cmdDeleteSelectedNewsItems");
            }

            if (CurrentSelectedFeedsNode is WasteBasketNode)
            {
                owner.Mediator.SetVisible("+cmdRestoreSelectedNewsItems");
            }
            else
            {
                owner.Mediator.SetVisible("-cmdRestoreSelectedNewsItems");
            }
        }

        public void RefreshTreeFeedContextMenus(TreeFeedsNodeBase feedsNode)
        {           

            owner.Mediator.SetEnabled(false, "cmdColumnChooserResetToDefault");
            if (feedsNode.Type == FeedNodeType.Feed || feedsNode.Type == FeedNodeType.Category)
            {
                owner.Mediator.SetEnabled(true, "cmdColumnChooserResetToDefault");
                owner.Mediator.SetEnabled(
                    "+cmdFlagNewsItem", "+cmdNavigateToFeedHome", "+cmdNavigateToFeedCosmos",
                    "+cmdViewSourceOfFeed", "+cmdValidateFeed");

                if (RssHelper.IsNntpUrl(feedsNode.DataKey))
                {
                    _feedInfoContextMenu.Enabled = false;
                }
                else
                {
                    _feedInfoContextMenu.Enabled = true;
                }
            }
            else if (feedsNode.Type == FeedNodeType.SmartFolder)
            {
                owner.Mediator.SetEnabled(
                    "-cmdFlagNewsItem", "-cmdNavigateToFeedHome", "-cmdNavigateToFeedCosmos",
                    "-cmdViewSourceOfFeed", "-cmdValidateFeed");
                if ((feedsNode as FlaggedItemsNode) != null)
                    owner.Mediator.SetEnabled("+cmdFlagNewsItem"); // allow re-flag of items
            }
            else if (feedsNode.Type == FeedNodeType.Finder)
            {
                owner.Mediator.SetEnabled("-cmdDeleteAllFinders", "+cmdDeleteFinder", "+cmdShowFinderProperties",
                                          "+cmdFlagNewsItem", "-cmdSubscribeToFinderResult");
                var agfn = feedsNode as FinderNode;
                if (agfn != null && agfn == _searchResultNode && agfn.Finder != null)
                {
                    bool extResult = !string.IsNullOrEmpty(agfn.Finder.ExternalSearchUrl);
                    owner.Mediator.SetEnabled(extResult, "cmdSubscribeToFinderResult");
                    owner.Mediator.SetEnabled(extResult && agfn.Finder.ExternalResultMerged, "cmdShowFinderProperties");
                }
                if (agfn != null && agfn.Finder != null)
                {
                    owner.Mediator.SetChecked(!agfn.Finder.ShowFullItemContent, "cmdFinderShowFullItemText");
                }
            }
            else if (feedsNode.Type == FeedNodeType.FinderCategory)
            {
                owner.Mediator.SetEnabled("+cmdDeleteAllFinders", "+cmdDeleteFinder", "-cmdShowFinderProperties");
            }
            else if (feedsNode.Type == FeedNodeType.Root)
            {
            	SubscriptionRootNode rootNode = feedsNode as SubscriptionRootNode;
				if (rootNode != null)
				{
					owner.Mediator.SetEnabled(FeedSourceType.DirectAccess != owner.FeedSources[rootNode.SourceID].SourceType, 
						"cmdDeleteFeedSource");
				}
                if ((feedsNode as FinderRootNode) != null)
                    owner.Mediator.SetEnabled("+cmdDeleteAllFinders", "-cmdDeleteFinder", "-cmdShowFinderProperties");
            }

            owner.Mediator.SetEnabled(true, "cmdShowFeedProperties");
            //we don't want people to be able to change properties of Facebook feed source
            FeedSourceEntry fse = FeedSourceEntryOf(feedsNode);
            if ((fse != null) && (fse.SourceType == FeedSourceType.Facebook))
            {
                owner.Mediator.SetEnabled(false, "cmdShowFeedProperties");
            }
        }

        private void MoveFeedDetailsToFront()
        {
            if (_docContainer.ActiveDocument != _docFeedDetails)
                _docContainer.ActiveDocument = _docFeedDetails;
        }
#if PHOENIX
		private void SaveDocumentState(Control doc)
		{
			if (doc == null)
				return;

			var state = doc.Tag as ITabState;
			if (state == null)
				return;
			if (_docContainer.ActiveDocument == doc)
			{
				SetTitleText(state.Title);
				UrlText = state.Url;

				HistoryMenuManager.SaveBrowserHistory(
					ultraToolbarsManager.NavigationToolbar,
					state);
			}
		}
#endif
        private void RefreshDocumentState(Control doc)
        {
            if (doc == null)
                return;

            var state = doc.Tag as ITabState;
            if (state == null)
                return;

            if (state.CanClose)
            {
                // not listview/detail pane doc
                doc.Text = StringHelper.ShortenByEllipsis(state.Title, 30);
            }

            if (_docContainer.ActiveDocument == doc)
            {
                SetTitleText(state.Title);
                UrlText = state.Url;
#if PHOENIX  			
				HistoryMenuManager.ResetBrowserHistory(
					ultraToolbarsManager.NavigationToolbar, 
					state);
#else              
                historyMenuManager.ReBuildBrowserGoBackHistoryCommandItems(state.GoBackHistoryItems(10));
                historyMenuManager.ReBuildBrowserGoForwardHistoryCommandItems(state.GoForwardHistoryItems(10));

                owner.Mediator.SetEnabled(state.CanGoBack, "cmdBrowserGoBack");
                owner.Mediator.SetEnabled(state.CanGoForward, "cmdBrowserGoForward");
#endif
            }
        }

        public void SetGuiStateINetConnected(bool connected)
        {
            try
            {
                StatusBarPanel p = statusBarConnectionState; //_status.Panels[2];
                p.Icon = connected
                             ? Resource.LoadIcon("Resources.Connected.ico")
                             : Resource.LoadIcon("Resources.Disconnected.ico");
            }
            catch
            {
            }
            _status.Refresh();
        }

        public void SetGuiStateFeedback(string text)
        {
            try
            {
                StatusBarPanel p = statusBarRssParser; //_status.Panels[3];
                if (!p.Text.Equals(text))
                {
                    p.Text = p.ToolTipText = text;
                    if (text.Length == 0 && p.Icon != null)
                    {
                        p.Icon = null;
                    }
                    _status.Refresh();
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Converts an application tray state to a particular Windows 7 taskbar progress bar state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private static TaskbarProgressBarState ConvertAppTrayStateToTaskBarState(ApplicationTrayState state)
        {
            return state == ApplicationTrayState.BusyRefreshFeeds ? TaskbarProgressBarState.Indeterminate : TaskbarProgressBarState.NoProgress;
        }

        public void SetGuiStateFeedback(string text, ApplicationTrayState state)
        {
            try
            {
                //set appropriate taskbar and overlay icon behavior in Windows 7
                if (TaskbarManager.IsPlatformSupported)
                {
                    TaskbarManager.Instance.SetProgressState(ConvertAppTrayStateToTaskBarState(state));

                    if (state == ApplicationTrayState.NewUnreadFeeds || state == ApplicationTrayState.NewUnreadFeedsReceived)
                    {
                        TaskbarManager.Instance.SetOverlayIcon(this.Handle, Properties.Resources.envelope_icon, String.Empty);
                    }
                    else if(state == ApplicationTrayState.BusyRefreshFeeds)
                    {
                        TaskbarManager.Instance.SetOverlayIcon(this.Handle, null, null); 
                    }
                }

                StatusBarPanel p = statusBarRssParser; //_status.Panels[3];
                if (state == ApplicationTrayState.NormalIdle)
                {
                    _timerResetStatus.Start();
                    if (!string.IsNullOrEmpty(text))
                    {
                        SetGuiStateFeedback(text);
                    }                   
                }
                else
                {
                    _timerResetStatus.Stop();
                    SetGuiStateFeedback(text);
                    _trayManager.SetState(state);
                    if (state == ApplicationTrayState.BusyRefreshFeeds)
                    {
                        if (p.Icon == null)
                        {
	                        p.Icon = Properties.Resources.Refresh; //Resource.LoadIcon("Resources.feedRefresh.ico");
                            _status.Refresh();
                        }
                    }
                    else
                    {
                        if (p.Icon != null)
                        {
                            p.Icon = null;
                            _status.Refresh();
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public void SetBrowserStatusBarText(string text)
        {
            try
            {
                StatusBarPanel p = statusBarBrowser; //_status.Panels[0];
                if (!p.Text.Equals(text))
                {
                    p.Text = text;
                    _status.Refresh();
                }
            }
            catch
            {
            }
        }

        public void SetSearchStatusText(string text)
        {
            SetGuiStateFeedback(text);
        }

        public void UpdateCategory(bool forceRefresh)
        {
            TreeFeedsNodeBase selectedNode = CurrentSelectedFeedsNode;
            if (selectedNode == null) return;

			owner.BeginRefreshCategoryFeeds(FeedSourceEntryOf(selectedNode), selectedNode.CategoryStoreName, forceRefresh);
        }


        /// <summary>
        /// Initiate a async. call to RssParser.RefreshFeeds(force_download)
        /// </summary>
        /// <param name="force_download"></param>
        public void UpdateAllCommentFeeds(bool force_download)
        {
            if (_timerRefreshCommentFeeds.Enabled)
                _timerRefreshCommentFeeds.Stop();

            owner.BeginRefreshCommentFeeds(force_download);
        }

        /// <summary>
        /// Initiate a async. call to FeedSource.RefreshFeeds(force_download)
        /// </summary>
        /// <param name="force_download"></param>
        public void UpdateAllFeeds(bool force_download)
        {
            List<SubscriptionRootNode> rootNodes = this.GetAllSubscriptionRootNodes();
			if (rootNodes != null)
			{
				if (_timerRefreshFeeds.Enabled)
					_timerRefreshFeeds.Stop();
				_lastUnreadFeedItemCountBeforeRefresh = rootNodes.Sum(n => n.UnreadCount);
				owner.BeginRefreshFeeds(force_download);
			}
        }

        public void OnAllAsyncUpdateCommentFeedsFinished()
        {
#if !NOAUTO_REFRESH
            // restart the feeds auto-refresh timer:
            if (!_timerRefreshCommentFeeds.Enabled)
                _timerRefreshCommentFeeds.Start();
#endif
        }

        public void OnAllAsyncUpdateFeedsFinished(FeedSourceEntry entry)
        {
#if !NOAUTO_REFRESH
            // restart the feeds auto-refresh timer:
            if (!_timerRefreshFeeds.Enabled)
                _timerRefreshFeeds.Start();
#endif
            if (!entry.FaviconsDownloaded && owner.Preferences.UseFavicons)
            {
                try
                {
					entry.FaviconsDownloaded = entry.Source.RefreshFavicons();
                }
                catch (Exception ex)
                {
					_log.Error("RefreshFavicons() failed", ex);
                    entry.FaviconsDownloaded = true;
                }
            }
        }

        private void OnApplicationIdle(object sender, EventArgs e)
        {
            //			if (IdleTask.IsTask(IdleTasks.InitOnFinishLoading)) 
            //			{
            //				
            //				IdleTask.RemoveTask(IdleTasks.InitOnFinishLoading);
            //				Splash.Close();	
            //				owner.BeginLoadingFeedlist();
            //				owner.BeginLoadingSpecialFeeds();
            //			} 
            //			else 
            if (IdleTask.IsTask(IdleTasks.IndexAllItems))
            {
                IdleTask.RemoveTask(IdleTasks.IndexAllItems);
                try
                {
                    FeedSource.SearchHandler.CheckIndex(true);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("LuceneIndexer failed: " + ex);
                }
            }
        }

        /// <summary>
        /// Extracts the category of the selected node within the feeds tree.
        /// </summary>
        /// <returns>Category found, or DefaultCategory</returns>
        public string CategoryOfSelectedNode()
        {
            TreeFeedsNodeBase tn = CurrentSelectedFeedsNode;

            if (tn != null)
            {
                switch (tn.Type)
                {
                    case FeedNodeType.Feed:
                        {
							INewsFeed f = owner.GetFeed(FeedSourceEntryOf(tn), tn.DataKey);
                            if (f != null)
                            {
                                return f.category;
                            }

                            return tn.CategoryStoreName;
                        }
                    case FeedNodeType.Root:
                    case FeedNodeType.Category:
                        return tn.CategoryStoreName;

                    default:
                        return RssBanditApplication.DefaultCategory;
                }
            }
            else
                return RssBanditApplication.DefaultCategory;
        }

		/// <summary>
		/// Add a new feed to the GUI tree view
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="category">Feed Category</param>
		/// <param name="f">Feed</param>
        public void AddNewFeedNode(FeedSourceEntry entry, string category, INewsFeed f)
        {
            TreeFeedsNodeBase tn;

            if (RssHelper.IsNntpUrl(f.link))
            {
                tn = new FeedNode(f.title, Resource.SubscriptionTreeImage.Nntp,
                                  Resource.SubscriptionTreeImage.NntpSelected,
                                  _treeFeedContextMenu);
            }
            else
            {
                tn = new FeedNode(f.title, Resource.SubscriptionTreeImage.Feed,
                                  Resource.SubscriptionTreeImage.FeedSelected,
                                  _treeFeedContextMenu,
                                  (owner.Preferences.UseFavicons ? LoadCachedFavicon(entry.Source, f) : null));
            }

            //interconnect for speed:
            tn.DataKey = f.link;
            f.Tag = tn;

            SetSubscriptionNodeState(f, tn, FeedProcessingState.Normal);

			SubscriptionRootNode root = GetSubscriptionRootNode(entry);
            category = (f.category == RssBanditApplication.DefaultCategory ? null : f.category);
            TreeFeedsNodeBase catnode = TreeHelper.CreateCategoryHive(root, category,
                                                                      _treeCategoryContextMenu);

            if (catnode == null)
                catnode = GetRoot(RootFolderType.MyFeeds);

            catnode.Nodes.Add(tn);
            //			tn.Cells[0].Value = tn.Text;
            //			tn.Cells[0].Appearance.Image = tn.Override.NodeAppearance.Image;
            //			tn.Cells[0].Appearance.Cursor = CursorHand;

            if (f.containsNewMessages)
            {
                IList<INewsItem> unread = FilterUnreadFeedItems(f);
                if (unread.Count > 0)
                {
                    // we build up a new tree node, so the call to 
                    // UpdateReadStatus(tn , 0) is not neccesary:
                    UpdateTreeNodeUnreadStatus(tn, unread.Count);
                    UnreadItemsNode.Items.AddRange(unread);
                    UnreadItemsNode.UpdateReadStatus();
                }
            }

            if (f.containsNewComments)
            {
                UpdateCommentStatus(tn, f);
            }

			if (root.Visible)
				tn.BringIntoView();

            DelayTask(DelayedTasks.SyncRssSearchTree);
        }

        public void InitiateRenameFeedOrCategory()
        {
            if (CurrentSelectedFeedsNode != null)
                DoEditTreeNodeLabel();
        }

        public bool NodeEditingActive
        {
            get { return (CurrentSelectedFeedsNode != null && CurrentSelectedFeedsNode.IsEditing); }
        }
     
        private static bool NodeIsChildOf(UltraTreeNode tn, UltraTreeNode parent)
        {
            if (parent == null)
                return false;

            UltraTreeNode p = tn.Parent;
            while (p != null)
            {
                if (p == parent) return true;
                p = p.Parent;
            }
            return false;
        }


		/// <summary>
		/// Called on each finished successful favicon request.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="favicon">The name of the favicon file</param>
		/// <param name="feedUrls">The list of URLs that will utilize this favicon</param>
        public void UpdateFavicon(FeedSourceEntry entry, string favicon, StringCollection feedUrls)
        {
			if (feedUrls == null || feedUrls.Count == 0)
				return;

            Image icon = null;
            if (!string.IsNullOrEmpty(favicon))
			{
				INewsFeed feed;
				if (entry.Source.GetFeeds().TryGetValue(feedUrls[0], out feed))
				{
					icon = FaviconCache.GetImage(entry.Source, feed);
				}
			}

			// set or reset icon(s) at nodes:
			SubscriptionRootNode root = GetSubscriptionRootNode(entry);
			if (root != null)
			{
				foreach (var feedUrl in feedUrls)
				{
					TreeFeedsNodeBase tn = TreeHelper.FindNode(root, feedUrl);
					if (tn == null)
					{
						_log.Debug("TreeHelper.FindNode() could not find matching tree node for " + feedUrl);
					}
					else
					{
						try
						{
							tn.SetIndividualImage(icon);
						} 
						catch (Exception ex)
						{
							_log.Debug("UpdateFavicon.SetIndividualImage("+ favicon+") failed for node " + tn.Text +" url: " + feedUrl, ex);
						}
					}
				}
			}

			if (icon != null)
            {
                //<favicon> entries added to subscriptions.xml
                owner.SubscriptionModified(entry, NewsFeedProperty.General);
            }
        }


		/// <summary>
		/// Converts the tree view to using favicons as feed icons where available
		/// </summary>
		/// <param name="useFavicons">if set to <c>true</c> [use favicons].</param>
        public void ApplyFavicons(bool useFavicons)
        {
            try
            {
				foreach (FeedSourceEntry entry in owner.FeedSources.Sources)
				{
					var feeds = entry.Source.GetFeeds();
					if (feeds.Count == 0)
						continue;
					
					TreeFeedsNodeBase root = GetSubscriptionRootNode(entry);

					// The "CopyTo()" construct prevents against InvalidOpExceptions/ArgumentOutOfRange
					// exceptions and keep the loop alive if FeedsTable gets modified from other thread(s)					
					string[] keys = new string[feeds.Count];
					feeds.Keys.CopyTo(keys, 0);
					
					for (int i = 0, len = keys.Length; i < len; i++)
					{
						string feedUrl = keys[i];
						INewsFeed f;

						if (!feeds.TryGetValue(feedUrl, out f))
						{
							continue;
						}

						if (useFavicons)
						{
							if (string.IsNullOrEmpty(f.favicon))
								continue;

							Image icon = FaviconCache.GetImage(entry.Source, f);

							if (icon != null)
							{
								TreeFeedsNodeBase tn = TreeHelper.FindNode(root, feedUrl);
								if (tn != null)
								{
									tn.SetIndividualImage(icon);
								}
							}
						}
						else
						{
							TreeFeedsNodeBase tn = TreeHelper.FindNode(root, feedUrl);

							if (tn != null)
							{
								SetSubscriptionNodeState(f, tn, FeedProcessingState.Normal);
							}
						} 
					} //for(int i...)
				} // foreach (FeedSourceEntry...)
            }
            catch (InvalidOperationException ioe)
            {
                // New feeds added to FeedsTable from another thread  
				_log.Error("ApplyFavicons - InvalidOperationException: {0}", ioe);
            }
        }


        /// <summary>
        /// Called on each finished successful comment feed refresh.
        /// </summary>
        /// <param name="feedUri">The original feed Uri</param>
        /// <param name="newFeedUri">The new feed Uri (if permamently moved)</param>
        public void UpdateCommentFeed(Uri feedUri, Uri newFeedUri)
        {
            IList<INewsItem> items;

            string feedUrl = feedUri.CanonicalizedUri();
            INewsFeed feed;
            TreeFeedsNodeBase tn = null;
            INewsItem item = null;
            bool modified = false;

            if (newFeedUri != null)
            {
                items = owner.CommentFeedsHandler.GetCachedItemsForFeed(newFeedUri.CanonicalizedUri());
            }
            else
            {
                items = owner.CommentFeedsHandler.GetCachedItemsForFeed(feedUrl);
            }

            //get the current number of comments on the item
            int commentCount = (items.Count == 0 ? NewsItem.NoComments : items.Count);

            if (!owner.CommentFeedsHandler.IsSubscribed(feedUrl) && (feedUri.IsFile || feedUri.IsUnc))
            {
                feedUrl = feedUri.LocalPath;
            }

            owner.CommentFeedsHandler.GetFeeds().TryGetValue(feedUrl, out feed);

            if (feed != null && feed.Tag != null)
            {
                var itemFeed = (INewsFeed) feed.Tag;
            	FeedSourceEntry entry = owner.FeedSources.SourceOf(itemFeed);
            	FeedInfo itemFeedInfo = null;
				if (entry != null)
				{
					itemFeedInfo = entry.Source.GetFeedDetails(itemFeed.link) as FeedInfo;
					tn = TreeHelper.FindNode(GetSubscriptionRootNode(entry), itemFeed);
				}

            	if (tn != null && itemFeedInfo != null)
                {
                    lock (itemFeedInfo.ItemsList)
                    {
                        //locate NewsItem from original feed 
                        foreach (var ni in itemFeedInfo.ItemsList)
                        {
                            if (!string.IsNullOrEmpty(ni.CommentRssUrl) &&
                                feedUrl.Equals(ni.CommentRssUrl))
                            {
                                item = ni;
                                //some comment feeds place the post as the first entry in the comments feed
                                if (items.Contains(item))
                                {
                                    commentCount--;
                                }
                                break;
                            }
                        } //foreach


                        if (item == null)
                        {
                            //item has been deleted or expired from the cache
                            owner.CommentFeedsHandler.DeleteFeed(feedUrl);
                            owner.WatchedItemsFeed.Remove(feedUrl);
                            return;
                        }

                        if (item.WatchComments)
                        {
                            if (commentCount > item.CommentCount)
                            {
                                itemFeed.containsNewComments = item.HasNewComments = true;
                                item.CommentCount = commentCount;
                                modified = true;
                            }

                            //fix up URL if it has moved 
                            if (newFeedUri != null && newFeedUri != feedUri)
                            {
                                if (newFeedUri.IsFile || newFeedUri.IsUnc)
                                    feedUrl = newFeedUri.LocalPath;
                                else
                                    feedUrl = newFeedUri.ToString();

                                item.CommentRssUrl = feedUrl;
                                modified = true;
                            }
                        } //if(item!= null && item.WatchComments)) 
                    } //lock(itemFeedInfo.ItemsList){


                    //to prevent bandwidth abuse we fetch comment feeds twice a day once 
                    //NewsItem is over a week old since they are rarely updated if ever
                    if ((DateTime.Now.Subtract(item.Date) > SevenDays))
                    {
                        feed.refreshrateSpecified = true;
                        feed.refreshrate = 12*60*60*1000; //twelve hours
                    }

                    if (itemFeed.containsNewComments)
                    {
                        //update tree view
                        UpdateCommentStatus(tn, itemFeedInfo.ItemsList, false);

                        //update list view 
                        bool categorized = false;
                        TreeFeedsNodeBase ftnSelected = TreeSelectedFeedsNode;

                        if (ftnSelected.Type == FeedNodeType.Category && NodeIsChildOf(tn, ftnSelected))
                            categorized = true;

                        if (tn.Selected || categorized)
                        {
                            ThreadedListViewItem lvItem = GetListViewItem(item.Id);
                            if (lvItem != null)
                            {
                                ApplyStyles(lvItem);
                            }
                        } //if (tn.Selected || categorized ) {
                    } //if(itemFeed.containsNewComments){

                    /* we need to write the feed to the cache if the CommentCount or the CommentRssUrl changed 
					 * for the NewsItem changed
					 */
                    if (modified)
                    {
                        entry.Source.ApplyFeedModifications(itemFeed.link);
                    }
                } //if (tn != null && itemFeedInfo != null) {
            } //if (feed != null && feed.Tag != null) {					
        }


		/// <summary>
		/// Called on each finished successful feed refresh.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="feedUrl">The original feed Uri</param>
		/// <param name="newFeedUri">The new feed Uri (if permanently moved)</param>
		/// <param name="modified">Really new items received</param>
		public void UpdateFeed(FeedSourceEntry entry, string feedUrl, Uri newFeedUri, bool modified)
        {
			Uri feedUri;
			if (Uri.TryCreate(feedUrl, UriKind.Absolute, out feedUri))
				UpdateFeed(entry, feedUri, newFeedUri, modified); 
            else
				UpdateFeed(entry, feedUri, newFeedUri, modified);
        }

		/// <summary>
		/// Called on each finished successful feed refresh.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="feedUri">The original feed Uri</param>
		/// <param name="newFeedUri">The new feed Uri (if permanently moved)</param>
		/// <param name="modified">Really new items received</param>
        public void UpdateFeed(FeedSourceEntry entry,Uri feedUri, Uri newFeedUri, bool modified)
        {
            if (feedUri == null)
                return;

            IList<INewsItem> items;
            IList<INewsItem> unread = null;

            string feedUrl = feedUri.CanonicalizedUri();
            TreeFeedsNodeBase tn;
			FeedSource source = entry.Source;

            if (newFeedUri != null)
            {
				items = source.GetCachedItemsForFeed(newFeedUri.CanonicalizedUri());
            }
            else
            {
				items = source.GetCachedItemsForFeed(feedUrl);
            }

			if (!source.IsSubscribed(feedUrl) && (feedUri.IsFile || feedUri.IsUnc))
            {
                feedUrl = feedUri.LocalPath;
            }


            //feed = owner.FeedHandler.GetFeeds()[feedUrl];
            INewsFeed feed;

			if (source.GetFeeds().TryGetValue(feedUrl, out feed) && feed != null)
            {
                tn = TreeHelper.FindNode(GetSubscriptionRootNode(entry), feed);
            }
            else
            {
				tn = TreeHelper.FindNode(GetSubscriptionRootNode(entry), feedUrl);
            }

            if (tn != null)
            {
                if (newFeedUri != null && newFeedUri != feedUri)
                {
                    // changed/moved
                    if (newFeedUri.IsFile || newFeedUri.IsUnc)
                        feedUrl = newFeedUri.LocalPath;
                    else
                        feedUrl = newFeedUri.CanonicalizedUri();
                    tn.DataKey = feedUrl;
					feed = owner.GetFeed(entry, feedUrl);
                    if (feed != null)
                        feed.Tag = tn;
                }

                if (feed != null)
                {
                    SetSubscriptionNodeState(feed, tn, FeedProcessingState.Normal);

                    if (modified || feed.containsNewMessages)
                        // if (feed.containsNewMessages) No longer applies due to syncing state from Google Reader & NewsGator Online
                    {
                        // if (modified)
                        int unreadBefore = tn.UnreadCount;
                        unread = FilterUnreadFeedItems(items, true);
                        UnreadItemsNodeRemoveItems(unread);
                        UnreadItemsNode.Items.AddRange(unread);
                        UpdateTreeNodeUnreadStatus(tn, unread.Count);
                        UnreadItemsNode.UpdateReadStatus();

                        if ((DisplayFeedAlertWindow.All == owner.Preferences.ShowAlertWindow ||
                             (DisplayFeedAlertWindow.AsConfiguredPerFeed == owner.Preferences.ShowAlertWindow &&
                              feed.alertEnabled)) && modified)
                        {
                            // new flag on feed, states if toast is enabled (off by default)
                            toastNotifier.Alert(tn, unreadBefore, unread);
                        }
                    }

                    if (feed.containsNewComments)
                    {
                        if (modified)
                            owner.FeedWasModified(feed, NewsFeedProperty.FeedItemCommentCount);
                        UpdateCommentStatus(tn, items, false);
                    }
                }

                bool categorized = false;
                TreeFeedsNodeBase ftnSelected = TreeSelectedFeedsNode;

                if (ftnSelected != null)
                {
                    if (ftnSelected.Type == FeedNodeType.Category && NodeIsChildOf(tn, ftnSelected))
                        categorized = true;

                    if (ftnSelected is UnreadItemsNode && unread != null && unread.Count > 0)
                    {
                        modified = categorized = true;
                        items = unread;
                    }
                }

                if (modified && (tn.Selected || categorized))
                {
                    INewsItem itemSelected = null;
                    if (listFeedItems.SelectedItems.Count > 0)
                        itemSelected = (INewsItem) ((ThreadedListViewItem) listFeedItems.SelectedItems[0]).Key;

                    PopulateListView(tn, items, false, categorized, ftnSelected);

                    if (itemSelected == null || (!categorized && !itemSelected.Feed.link.Equals(tn.DataKey)))
                    {
                        //clear state
                        CurrentSelectedFeedItem = null;
                        //reload newspaper
                        RefreshFeedDisplay(tn, false);
                    }
                    else
                    {
                        ReSelectListViewItem(itemSelected);
                    }
                }

                // apply finder matches to refresh node unread state(s)
                UpdateFindersReadStatus(items);
            }
            else
            {
                _log.Info("UpdateFeed() could not find node for '" + feedUri + "'...");
            }
        }

        private void UpdateFindersReadStatus(IEnumerable<INewsItem> items)
        {
            // apply finder matches to refresh the read state only
            if (_searchResultNode != null && !_searchResultNode.AnyUnread)
            {
                SearchCriteriaCollection sc = _searchResultNode.Finder.SearchCriterias;
                foreach (var item in items)
                {
                    if (!item.BeenRead && sc.Match(item))
                    {
                        // match unread only is enough here
                        _searchResultNode.UpdateReadStatus(_searchResultNode, true);
                        break;
                    }
                } //foreach
            }

            foreach (RssFinder finder in owner.FinderList)
            {
                if (finder.Container != null && !finder.Container.AnyUnread)
                {
                    SearchCriteriaCollection sc = finder.SearchCriterias;
                    foreach (var item in items)
                    {
                        if (!item.BeenRead && sc.Match(item))
                        {
                            // match unread only is enough here
                            finder.Container.UpdateReadStatus(finder.Container, true);
                            break;
                        }
                    } //foreach
                }
            } //foreach
        }

        private void ResetFindersReadStatus()
        {
            if (_searchResultNode != null)
            {
                UpdateTreeNodeUnreadStatus(_searchResultNode, 0);
            }
            foreach (RssFinder finder in owner.FinderList)
            {
                if (finder.Container != null)
                    UpdateTreeNodeUnreadStatus(finder.Container, 0);
            }
        }

        public void NewCategory()
        {
            if (CurrentSelectedFeedsNode != null && CurrentSelectedFeedsNode.AllowedChild(FeedNodeType.Category))
            {
                TreeFeedsNodeBase curFeedsNode = CurrentSelectedFeedsNode;
                FeedSourceEntry entry = FeedSourceEntryOf(curFeedsNode); 

                int i = 1;
                string s = SR.GeneralNewItemText;
                // check for duplicate names:
                while (TreeHelper.FindChildNode(curFeedsNode, s, FeedNodeType.Category) != null)
                {
                    s = String.Format(SR.GeneralNewItemTextWithCounter, i++);
                }

                TreeFeedsNodeBase newFeedsNode = new CategoryNode(s,
                                                                  Resource.SubscriptionTreeImage.SubscriptionsCategory,
                                                                  Resource.SubscriptionTreeImage.
                                                                      SubscriptionsCategoryExpanded,
                                                                  _treeCategoryContextMenu);

                curFeedsNode.Nodes.Add(newFeedsNode);
                //				newNode.Cells[0].Appearance.Image = newNode.Override.NodeAppearance.Image;
                newFeedsNode.BringIntoView();
                TreeSelectedFeedsNode = newFeedsNode;
                s = newFeedsNode.CategoryStoreName;

                if (!entry.Source.HasCategory(s))
                {
                    entry.Source.AddCategory(s);
                    owner.SubscriptionModified(entry, NewsFeedProperty.FeedCategoryAdded);
                    //owner.FeedlistModified = true;
                }

                if (!treeFeeds.Focused) treeFeeds.Focus();
                newFeedsNode.BeginEdit();
            }
        }

        //		public void DeleteCategory() 
        //		{
        //			this.DeleteCategory(CurrentSelectedFeedsNode);
        //			this.owner.SubscriptionModified(NewsFeedProperty.FeedCategoryRemoved);
        //			//owner.FeedlistModified = true;
        //		}

        internal void UpdateTreeNodeUnreadStatus(TreeFeedsNodeBase node, int newCount)
        {
            if (node != null)
            {
                node.UpdateReadStatus(node, newCount);
                if (node.Selected)
                {
                    SetDetailHeaderText(node);
                }
            }
        }

        /// <summary>
        /// Can be called on every selected tree node.
        /// </summary>
        public void MarkSelectedNodeRead(TreeFeedsNodeBase startNode)
        {
            TreeFeedsNodeBase selectedNode = startNode ?? CurrentSelectedFeedsNode;

            if (selectedNode == null) return;
            FeedSourceEntry entry = FeedSourceEntryOf(selectedNode); 
            INewsFeed f = null;
            if (selectedNode.Type == FeedNodeType.Feed)
            {
				f = owner.GetFeed(entry, selectedNode.DataKey);

                if (f != null)
                {
                    UnreadItemsNodeRemoveItems(f); // BEFORE they get marked read by:
                    entry.Source.MarkAllCachedItemsAsRead(f);
                    owner.FeedWasModified(f, NewsFeedProperty.FeedItemReadState);
                    UpdateTreeNodeUnreadStatus(selectedNode, 0);
                }
            }

            bool selectedIsChild = NodeIsChildOf(TreeSelectedFeedsNode, selectedNode);
            bool isSmartOrAggregated = (selectedNode.Type == FeedNodeType.Finder ||
                                        selectedNode.Type == FeedNodeType.SmartFolder);

			//mark all viewed stories as read 
            // May be we are wrong here: how about a threaded item reference
            // with an ownerfeed, that is not a child of the current selectedNode?
            if (listFeedItems.Items.Count > 0)
            {
				List<INewsItem> unread = new List<INewsItem>(listFeedItems.Items.Count);
            
                listFeedItems.BeginUpdate();

                for (int i = 0; i < listFeedItems.Items.Count; i++)
                {
                    ThreadedListViewItem lvi = listFeedItems.Items[i];
                    var newsItem = (INewsItem) lvi.Key;

                    if (newsItem != null &&
                        (newsItem.Feed == f || selectedIsChild || selectedNode == TreeSelectedFeedsNode ||
                         isSmartOrAggregated || lvi.IndentLevel > 0 || selectedNode.Type == FeedNodeType.Root))
                    {
                        // switch image back
                        if ((lvi.ImageIndex%2) != 0)
                            lvi.ImageIndex--;

                        // switch font back
                        ApplyStyles(lvi, true);

                        if (!newsItem.BeenRead)
                        {
                            newsItem.BeenRead = true;
							unread.Add(newsItem);
							//UnreadItemsNode.Remove(newsItem);

                            // now update tree state of rss items from different
                            // feeds (or also: category selected)
                            if (lvi.IndentLevel > 0 || selectedNode.Type == FeedNodeType.Finder)
                            {
                                // corresponding node can be at any hierarchy level
                                selectedNode.UpdateReadStatus(
                                    TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), newsItem), -1);
                            }
							else if (selectedNode.Type != FeedNodeType.Feed && !(selectedNode is UnreadItemsNode))
                            {
                                // can only be a child node, or SmartFolder
                                if (newsItem.Feed.containsNewMessages)
                                {
                                    //if not yet handled
                                    TreeFeedsNodeBase itemFeedsNode = TreeHelper.FindNode(selectedNode, newsItem);
                                    if (itemFeedsNode != null)
                                    {
                                        UpdateTreeNodeUnreadStatus(itemFeedsNode, 0);
                                        newsItem.Feed.containsNewMessages = false;
                                    }
                                }
                            }
                        } //if (!readed
                    } //item belongs to feed 
                    //else {
                    //Trace.WriteLine("Does not belong to node: "+NewsItem.Title);
                    //}
                } //for (i=0...

                listFeedItems.EndUpdate();
            	
				UnreadItemsNodeRemoveItems(unread);
            }

            if (selectedNode.Type == FeedNodeType.Root)
            {
                // all
				//UnreadItemsNodeRemoveItems(UnreadItemsNode.Items);
                UnreadItemsNodeRemoveAllItems(entry);
				entry.Source.MarkAllCachedItemsAsRead();
				owner.SubscriptionModified(entry, NewsFeedProperty.FeedItemReadState);
                selectedNode.ResetReadStatus();
                //UpdateTreeNodeUnreadStatus(selectedNode, 0);
                
				//ResetFindersReadStatus();
                //SetGuiStateFeedback(String.Empty, ApplicationTrayState.NormalIdle);
            }
            else if (selectedNode.Type == FeedNodeType.Category)
            {
				INewsFeedCategory c;
				entry.Source.GetCategories().TryGetValue(selectedNode.CategoryStoreName, out c);
				if (c != null)
				{
					foreach (var feed in entry.Source.GetDescendantFeeds(c))
					{
						TreeFeedsNodeBase ownerNode = feed.Tag as TreeFeedsNodeBase;
						UnreadItemsNodeRemoveItems(feed); // BEFORE they get marked read by:
						entry.Source.MarkAllCachedItemsAsRead(feed);
						owner.FeedWasModified(feed, NewsFeedProperty.FeedItemReadState);
						UpdateTreeNodeUnreadStatus(ownerNode, 0);
					}
					//UpdateTreeNodeUnreadStatus(selectedNode, 0);
				}

                // category and childs
                //WalkdownAndCatchupCategory(selectedNode);
            }
            if (isSmartOrAggregated)
            {
                var sfNode = startNode as ISmartFolder;
                if (sfNode != null) sfNode.UpdateReadStatus();
            }
        }


        /// <summary>
        /// Toggle's the flag state of the identified RSS item
        /// </summary>
        /// <param name="id">The ID of the RSS item</param>
        public void ToggleItemFlagState(string id)
        {
            ThreadedListViewItem lvItem = GetListViewItem(id);

            if (lvItem != null)
            {
                var item = (INewsItem) lvItem.Key;

                Flagged oldStatus = item.FlagStatus;

                if (oldStatus != Flagged.None)
                {
                    item.FlagStatus = Flagged.None;
                }
                else
                {
                    item.FlagStatus = Flagged.FollowUp;
                }

                if (item.FlagStatus != Flagged.None)
                {
                    lvItem.Font = FontColorHelper.MergeFontStyles(lvItem.Font, FontColorHelper.HighlightStyle);
                    lvItem.ForeColor = FontColorHelper.HighlightColor;
                }
                else
                {
                    lvItem.Font = FontColorHelper.MergeFontStyles(lvItem.Font, FontColorHelper.NormalStyle);
                    lvItem.ForeColor = FontColorHelper.NormalColor;
                }

                //ApplyFlagIconTo(lvItem, item);
                ApplyFlagStateTo(lvItem, item.FlagStatus, listFeedItems.Columns.GetColumnIndexMap());

                CheckForFlaggedNodeAndCreate(item);

                if ((CurrentSelectedFeedsNode as FlaggedItemsNode) != null)
                {
                    owner.ReFlagNewsItem(item);
                }
                else
                {
                    owner.FlagNewsItem(item);
                }

                if (FlaggedFeedsNode(item.FlagStatus) != null)
                {
                    // ReFlag may remove also items
                    FlaggedFeedsNode(item.FlagStatus).UpdateReadStatus();
                }
                if (listFeedItemsO.Visible)
                    listFeedItemsO.Invalidate();
            }
        }


        /// <summary>
        /// Toggles watching a particular item or list of items for new comments by either watching the value of 
        /// slash:comments and thr:replies or subscribing to the comments feed. 
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdWatchItemComments(ICommand sender)
        {
            IList<ThreadedListViewItem> selectedItems = GetSelectedLVItems();

            for (int i = 0; i < selectedItems.Count; i++)
            {
                ThreadedListViewItem selectedItem = selectedItems[i];

                var item = (INewsItem) selectedItem.Key;
                item.WatchComments = !item.WatchComments;
                owner.WatchNewsItem(item);
            }
        }

        /// <summary>
        /// Marks the selected feed items flagged. Called from the listview
        /// context menu.
        /// </summary>
        public void MarkFeedItemsFlagged(Flagged flag)
        {
            INewsItem item;
            ColumnKeyIndexMap colIndex = listFeedItems.Columns.GetColumnIndexMap();
            IList<ThreadedListViewItem> selectedItems = GetSelectedLVItems();
            List<ThreadedListViewItem> toBeRemoved = null;

            for (int i = 0; i < selectedItems.Count; i++)
            {
                ThreadedListViewItem selectedItem = selectedItems[i];

                item = (INewsItem) selectedItem.Key;

                if (item.FlagStatus == flag)
                    continue; // no change

                item.FlagStatus = flag;

                //font styles merged, color overrides
                if (item.FlagStatus != Flagged.None)
                {
                    selectedItem.Font =
                        FontColorHelper.MergeFontStyles(selectedItem.Font, FontColorHelper.HighlightStyle);
                    selectedItem.ForeColor = FontColorHelper.HighlightColor;
                }
                else
                {
                    selectedItem.Font = FontColorHelper.MergeFontStyles(selectedItem.Font, FontColorHelper.NormalStyle);
                    selectedItem.ForeColor = FontColorHelper.NormalColor;
                }

                //ApplyFlagIconTo(selectedItem, item);
                ApplyFlagStateTo(selectedItem, item.FlagStatus, colIndex);

                CheckForFlaggedNodeAndCreate(item);

                if ((CurrentSelectedFeedsNode as FlaggedItemsNode) != null)
                {
                    owner.ReFlagNewsItem(item);
                    if (item.FlagStatus == Flagged.None || item.FlagStatus == Flagged.Complete)
                    {
                        if (toBeRemoved == null)
                            toBeRemoved = new List<ThreadedListViewItem>();
                        toBeRemoved.Add(selectedItem);
                    }
                }
                else
                {
                    owner.FlagNewsItem(item);
                }
            } //for(i=0...

            if (toBeRemoved != null && toBeRemoved.Count > 0)
                RemoveListviewItems(toBeRemoved, false, false, false);

            if (FlaggedFeedsNode(flag) != null)
            {
                // ReFlag may remove also items
                FlaggedFeedsNode(flag).UpdateReadStatus();
            }
        }

        /// <summary>
        /// Removes the provided listview items (collection of ThreadedListViewItem objects). 
        /// Also considers childs.
        /// </summary>
        /// <param name="itemsToRemove">List of items to be removed</param>
        /// <param name="moveItemsToTrash">If true, the corresponding NewsItem(s) will be moved to the Trash SmartFolder</param>
        /// <param name="removeFromSmartFolder">If true, the  corresponding NewsItem(s) will be also removed from any SmartFolder</param>
        /// <param name="updateUnreadCounters">If true, the unread counter(s) will be updated</param>
        public void RemoveListviewItems(IList<ThreadedListViewItem> itemsToRemove,
                                        bool moveItemsToTrash, bool removeFromSmartFolder, bool updateUnreadCounters)
        {
            if (itemsToRemove == null || itemsToRemove.Count == 0)
                return;

            var items = new ThreadedListViewItem[itemsToRemove.Count];
            itemsToRemove.CopyTo(items, 0);

            // where we are?
            TreeFeedsNodeBase thisNode = TreeSelectedFeedsNode;
            var isFolder = thisNode as ISmartFolder;

            int unreadItemsCount = 0;
            int itemIndex = 0;
            bool anyUnreadItem = false;

            try
            {
                listFeedItems.BeginUpdate();

                int delCounter = itemsToRemove.Count;
                while (--delCounter >= 0)
                {
                    ThreadedListViewItem currentItem = items[delCounter];

                    if (currentItem == null || currentItem.IndentLevel > 0)
                        continue; // do not delete selected childs

                    if (currentItem.HasChilds && currentItem.Expanded)
                    {
                        // also remove the childs
                        int j = currentItem.Index + 1;
                        if (j < listFeedItems.Items.Count)
                        {
                            lock (listFeedItems.Items)
                            {
                                ThreadedListViewItem child = listFeedItems.Items[j];
                                while (child != null && child.IndentLevel > currentItem.IndentLevel)
                                {
                                    listFeedItems.Items.Remove(child);
                                    if (listFeedItemsO.Visible)
                                        listFeedItemsO.Remove(child);
                                    if (j < listFeedItems.Items.Count)
                                        child = listFeedItems.Items[j];
                                    else
                                        child = null;
                                }
                            }
                        }
                    }

                    // remember for reselection of the preceeding item.
                    // we just take that of the last iterated item:
                    itemIndex = currentItem.Index;

                    var item = (INewsItem) currentItem.Key;

                    if (item == null)
                        continue;

                    if (moveItemsToTrash)
                        owner.DeleteNewsItem(item);

                    if (!item.BeenRead)
                        UnreadItemsNode.Remove(item);

                    if (item.WatchComments)
                        WatchedItemsNode.Remove(item);

                    if (item.HasNewComments)
                    {
                        TreeFeedsNodeBase n = TreeHelper.FindNode(thisNode, item);
                        n.UpdateCommentStatus(n, -1);
                    }

                    if (thisNode.Type == FeedNodeType.Category)
                    {
                        if (updateUnreadCounters && !item.BeenRead)
                        {
                            // update unread counter(s)
                            anyUnreadItem = true;
                            TreeFeedsNodeBase n = TreeHelper.FindNode(thisNode, item);
                            UpdateTreeNodeUnreadStatus(n, -1);
                        }
                    }
                    else if (isFolder == null)
                    {
                        isFolder = TreeHelper.FindNode(GetRoot(RootFolderType.SmartFolders), item) as ISmartFolder;
                    }

                    if (updateUnreadCounters && !item.BeenRead)
                    {
                        anyUnreadItem = true;
                        unreadItemsCount++;
                    }

                    if (removeFromSmartFolder && isFolder != null)
                        owner.RemoveItemFromSmartFolder(isFolder, item);

                    lock (listFeedItems.Items)
                    {
                        listFeedItems.Items.Remove(currentItem);
                        if (listFeedItemsO.Visible)
                            listFeedItemsO.Remove(currentItem);
                    }
                } //while
            }
            finally
            {
                listFeedItems.EndUpdate();
            }

            if (updateUnreadCounters && unreadItemsCount > 0)
            {
                UpdateTreeNodeUnreadStatus(thisNode, -unreadItemsCount);
            }

            if (moveItemsToTrash && anyUnreadItem)
                DeletedItemsNode.UpdateReadStatus();

            // try to select another item:
            if (listFeedItems.Items.Count > 0 && listFeedItems.SelectedItems.Count == 0)
            {
                /*	itemIndex--;

					if (itemIndex < 0) {
						itemIndex = 0;
					} else */
                if ((itemIndex != 0) && (itemIndex >= listFeedItems.Items.Count))
                {
                    itemIndex = listFeedItems.Items.Count - 1;
                }

                listFeedItems.Items[itemIndex].Selected = true;
                listFeedItems.Items[itemIndex].Focused = true;

                OnFeedListItemActivate(this, EventArgs.Empty);
            }
            else if (listFeedItems.SelectedItems.Count > 0)
            {
                // still selected not deleted items:

                OnFeedListItemActivate(this, EventArgs.Empty);
            }
            else
            {
                // no items:
                htmlDetail.Clear();
            }
        }

        /// <summary>
        /// Remove the selected feed items. 
        /// Called from the listview context menu.
        /// </summary>
        public void RemoveSelectedFeedItems()
        {
            using (new CursorChanger(Cursors.WaitCursor))
            {
                IList<ThreadedListViewItem> selectedItems = GetSelectedLVItems();
                if (selectedItems.Count == 0)
                    return;

                RemoveListviewItems(selectedItems, true, true, true);
            }
        }

        /// <summary>
        /// Restore the selected feed items from the Wastebasket. 
        /// Called from the listview context menu.
        /// </summary>
        public void RestoreSelectedFeedItems()
        {
            IList<ThreadedListViewItem> selectedItems = GetSelectedLVItems();
            if (selectedItems.Count == 0)
                return;

            TreeFeedsNodeBase thisNode = TreeSelectedFeedsNode;
            var isFolder = thisNode as ISmartFolder;

            if (!(isFolder is WasteBasketNode))
                return;

            int itemIndex = 0;
            bool anyUnreadItem = false;

            try
            {
                listFeedItems.BeginUpdate();

                while (selectedItems.Count > 0)
                {
                    ThreadedListViewItem selectedItem = selectedItems[0];

                    if (selectedItem.IndentLevel > 0)
                        continue; // do not delete selected childs

                    if (selectedItem.HasChilds && selectedItem.Expanded)
                    {
                        // also remove the childs
                        int j = selectedItem.Index + 1;
                        if (j < listFeedItems.Items.Count)
                        {
                            lock (listFeedItems.Items)
                            {
                                ThreadedListViewItem child = listFeedItems.Items[j];
                                while (child != null && child.IndentLevel > selectedItem.IndentLevel)
                                {
                                    listFeedItems.Items.Remove(child);
                                    if (listFeedItemsO.Visible)
                                        listFeedItemsO.Remove(child);
                                    if (j < listFeedItems.Items.Count)
                                        child = listFeedItems.Items[j];
                                    else
                                        child = null;
                                }
                            }
                        }
                    }

                    // remember for reselection of the preceeding item.
                    // we just take that of the last iterated item:
                    itemIndex = selectedItem.Index;

                    var item = (INewsItem) selectedItem.Key;

                    if (item == null)
                    {
                        selectedItems.Remove(selectedItem);
                        continue;
                    }

                    TreeFeedsNodeBase originalContainerNode = owner.RestoreNewsItem(item);

                    if (null != originalContainerNode && !item.BeenRead)
                    {
                        anyUnreadItem = true;
                        UpdateTreeNodeUnreadStatus(originalContainerNode, 1);
                        UnreadItemsNode.Add(item);
                    }

                    if (null == originalContainerNode)
                    {
                        // 
                        _log.Error("Item could not be restored, maybe the container feed was removed meanwhile: " +
                                   item.Title);
                    }

                    lock (listFeedItems.Items)
                    {
                        listFeedItems.Items.Remove(selectedItem);
                        if (listFeedItemsO.Visible)
                            listFeedItemsO.Remove(selectedItem);

                        selectedItems.Remove(selectedItem);
                    }
                } //while
            }
            finally
            {
                listFeedItems.EndUpdate();
            }

            if (anyUnreadItem)
                DeletedItemsNode.UpdateReadStatus();

            // try to select another item:
            if (listFeedItems.Items.Count > 0)
            {
                itemIndex--;

                if (itemIndex < 0)
                {
                    itemIndex = 0;
                }
                else if (itemIndex >= listFeedItems.Items.Count)
                {
                    itemIndex = listFeedItems.Items.Count - 1;
                }

                listFeedItems.Items[itemIndex].Selected = true;
                listFeedItems.Items[itemIndex].Focused = true;

                OnFeedListItemActivate(this, EventArgs.Empty);
            }
            else
            {
                // no items:
                htmlDetail.Clear();
            }
        }


        /// <summary>
        /// Helper function which gets the list of selected list view items from the 
        /// currently visible list view. 
        /// </summary>
        /// <returns>The list of selected ThreadedListViewItems</returns>
        private IList<ThreadedListViewItem> GetSelectedLVItems()
        {
            if (listFeedItems.Visible)
            {
                return listFeedItems.SelectedItems.Cast<ThreadedListViewItem>().ToList();
            }

            return listFeedItemsO.SelectedItems.Cast<ThreadedListViewItem>().ToList();
        }


        /// <summary>
        /// Marks the selected listview items read. Called from the listview
        /// context menu.
        /// </summary>
        public void MarkSelectedItemsLVRead()
        {
            SetFeedItemsReadState(GetSelectedLVItems(), true);
        }

        /// <summary>
        /// Marks the selected listview items unread. Called from the listview
        /// context menu.
        /// </summary>
        public void MarkSelectedItemsLVUnread()
        {
            SetFeedItemsReadState(GetSelectedLVItems(), false);
        }

        /// <summary>
        /// Marks the all listview items read. Called from the listview
        /// context menu.
        /// </summary>
        public void MarkAllItemsLVRead()
        {
            SetFeedItemsReadState(listFeedItems.Items, true);
        }

        /// <summary>
        /// Marks the all listview items unread. Called from the listview
        /// context menu.
        /// </summary>
        public void MarkAllItemsLVUnread()
        {
            SetFeedItemsReadState(listFeedItems.Items, false);
        }

        private void ApplyStyles(ThreadedListViewItem item)
        {
            if (item != null)
            {
                var n = (INewsItem) item.Key;
                if (n != null)
                    ApplyStyles(item, n.BeenRead, n.HasNewComments);
            }
        }

        private void ApplyStyles(ThreadedListViewItem item, bool beenRead)
        {
            if (item != null)
            {
                var n = (INewsItem) item.Key;
                if (n != null)
                    ApplyStyles(item, beenRead, n.HasNewComments);
            }
        }

        private void ApplyStyles(ThreadedListViewItem item, bool beenRead, bool newComments)
        {
            if (item != null)
            {
                if (beenRead)
                {
                    item.Font = FontColorHelper.NormalFont;
                    item.ForeColor = FontColorHelper.NormalColor;
                }
                else
                {
                    item.Font = FontColorHelper.UnreadFont;
                    item.ForeColor = FontColorHelper.UnreadColor;
                }

                if (newComments)
                {
                    item.Font = FontColorHelper.MergeFontStyles(item.Font, FontColorHelper.NewCommentsStyle);
                    item.ForeColor = FontColorHelper.NewCommentsColor;
                }

                _filterManager.Apply(item);
                if (listFeedItemsO.Visible)
                    listFeedItemsO.Invalidate();
            }
        }


        /// <summary>
        /// Marks the all feed items related to a particular URL as read. 
        /// </summary>
        /// <param name="storyId">The URL of the story</param>
        public void MarkDiscussionAsRead(string storyId)
        {
            int numDays = RssBanditApplication.ReadAppSettingsEntry("TopStoriesTimeSpanInDays", SevenDays.Days);
            DateTime since = DateTime.Now - new TimeSpan(numDays, 0, 0, 0, 0);

            IList<INewsItem> affectedItems = new List<INewsItem>();
			foreach (FeedSourceEntry entry in owner.FeedSources.Sources)
				affectedItems.AddRange(entry.Source.GetItemsWithIncomingLinks(storyId, since));
            
			var affectedItemsInListView = new List<ThreadedListViewItem>();
            for (int i = 0; i < affectedItems.Count; i++)
            {
                ThreadedListViewItem lvItem = GetListViewItem(affectedItems[i].Id);
                if (lvItem != null)
                {
                    affectedItemsInListView.Add(lvItem);
                    affectedItems.RemoveAt(i);
                }
            }

            SetFeedItemsReadState(affectedItemsInListView, true);
            SetNewsItemsReadState(affectedItems, true);
        }

        /// <summary>
        /// Moves the newspaper view to the next or previous page. 
        /// </summary>
        /// <param name="pageType">Indicates whether the page is a category or feed node</param>
        /// <param name="go2nextPage">Indicates whether we are going to the next or previous page. If true
        /// we are going to the next page, otherwise we are going to the previous page</param>
        public void SwitchPage(string pageType, bool go2nextPage)
        {
            TreeFeedsNodeBase tn = CurrentSelectedFeedsNode;
            FeedSourceEntry entry = FeedSourceEntryOf(tn); 
            if (tn == null || entry == null)
                return;

            if (go2nextPage)
            {
                _currentPageNumber++;
            }
            else
            {
                _currentPageNumber--;
            }

            if (pageType.Equals("feed"))
            {
                FeedInfo fi = GetFeedItemsAtPage(_currentPageNumber);
                if (fi != null)
                {
                    BeginTransformFeed(fi, tn, entry.Source.GetStyleSheet(tn.DataKey));
                }
            }
            else
            {
                //BUGBUG: How do we provide title of FeedInfoList? 
                FeedInfoList fil = GetCategoryItemsAtPage(_currentPageNumber);
                if (fil != null)
                {
                    BeginTransformFeedList(fil, tn, entry.Source.GetCategoryStyleSheet(tn.CategoryStoreName));
                }
            }
        }


        /// <summary>
        /// Launches the post reply form from an item in the reading pane
        /// </summary>
        /// <param name="id">The ID of the RSS item</param>
        public void PostReplyFromReadingPane(string id)
        {
            ThreadedListViewItem lvItem = GetListViewItem(id);

            if (lvItem != null)
            {
                lvItem.Selected = true;
                this.CurrentSelectedFeedItem = lvItem.Key as INewsItem;
                owner.CmdPostReplyToItem(null); 
            }
        }

        /// <summary>
        /// Toggles the identified item's read/unread state. 
        /// </summary>
        /// <param name="id">The ID of the RSS item</param>
        /// <param name="markRead">Indicates that the item should be marked as read NOT toggled</param>
        public void ToggleItemReadState(string id, bool markRead)
        {
            ThreadedListViewItem lvItem = GetListViewItem(id);

            if (lvItem != null)
            {
                bool oldReadState = ((INewsItem) lvItem.Key).BeenRead;
                if (!markRead || (markRead != oldReadState))
                {
                    SetFeedItemsReadState(new[] {lvItem}, !oldReadState);
                }
            }
        }

        /// <summary>
        /// Toggles the identified item's read/unread state. 
        /// </summary>
        /// <param name="id">The ID of the RSS item</param>
        public void ToggleItemReadState(string id)
        {
            ToggleItemReadState(id, false);
        }


        /// <summary>
        /// Toggle's the Google Reader shared state of the identified RSS item
        /// </summary>
        /// <param name="id">The ID of the RSS item</param>
        public void ToggleItemShareState(string id)
        {
            ThreadedListViewItem lvItem = GetListViewItem(id);

            if (lvItem != null)
            {
                var item = (INewsItem) lvItem.Key;
                if (FeedSourceType.Google == owner.FeedSources.SourceTypeOf(item.Feed))
                {
                    var source = owner.FeedSources.GetSourceExtension<IGoogleReaderFeedSource>(item.Feed);
                    source.ShareNewsItem(item);
                }
            }
        }

        /// <summary>
        /// Toggle's the NewsGator Online clipped state of the identified RSS item
        /// </summary>
        /// <param name="id">The ID of the RSS item</param>
        public void ToggleItemClipState(string id)
        {
            ThreadedListViewItem lvItem = GetListViewItem(id);

            if (lvItem != null)
            {
                var item = (INewsItem) lvItem.Key;
                if (FeedSourceType.NewsGator == owner.FeedSources.SourceTypeOf(item.Feed))
                {
                    var source = owner.FeedSources.GetSourceExtension<INewsGatorFeedSource>(item.Feed);
                    source.ClipNewsItem(item);
                }
            }
        }

        /// <summary>
        /// Toggles the identified item's watchd state. 
        /// </summary>
        /// <param name="id">The ID of the RSS item</param>
        public void ToggleItemWatchState(string id)
        {
            ThreadedListViewItem lvItem = GetListViewItem(id);

            if (lvItem != null)
            {
                var item = (INewsItem) lvItem.Key;
                item.WatchComments = !item.WatchComments;
                owner.WatchNewsItem(item);
            }
        }

        private class RefLookupItem
        {
            public readonly TreeFeedsNodeBase Node;
            public readonly INewsFeed Feed;
            public int UnreadCount;

            public RefLookupItem(TreeFeedsNodeBase feedsNode, INewsFeed feed, int unreadCount)
            {
                Node = feedsNode;
                Feed = feed;
                UnreadCount = unreadCount;
            }
        }

        /// <summary>
        /// Marks the selected feed items read/unread. Called from the listview
        /// context menu.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="beenRead">if set to <c>true</c> [been read].</param>
        public void SetFeedItemsReadState(IList<ThreadedListViewItem> items, bool beenRead)
        {
            var modifiedItems = new List<INewsItem>(listFeedItems.SelectedItems.Count);
            int amount = (beenRead ? -1 : 1);

            for (int i = 0; i < items.Count; i++)
            {
                ThreadedListViewItem selectedItem = items[i];
                var item = (INewsItem) selectedItem.Key;
                ApplyStyles(selectedItem, beenRead);

                if (item.BeenRead != beenRead)
                {
                    selectedItem.ImageIndex += amount;
                    modifiedItems.Add(item);
                }
            } //for(int i=0; i < items.Count; i++)

            SetNewsItemsReadState(modifiedItems, beenRead);
        }


        /// <summary>
        /// Marks the specified feed items read/unread. 
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="beenRead">if set to <c>true</c> [been read].</param>
        public void SetNewsItemsReadState(IList<INewsItem> items, bool beenRead)
        {
        	var modifiedSources = new List<FeedSourceEntry>(1);
            var modifiedItems = new List<INewsItem>(listFeedItems.SelectedItems.Count);
            int amount = (beenRead ? -1 : 1);

            for (int i = 0; i < items.Count; i++)
            {
                INewsItem item = items[i];

                if (item.BeenRead != beenRead)
                {
                    item.BeenRead = beenRead;
                    modifiedItems.Add(item);
                	
					FeedSourceEntry src = owner.FeedSources.SourceOf(item.Feed);
					if (src != null && !modifiedSources.Contains(src))
						modifiedSources.Add(src);

                    if (beenRead)
                    {
                        if (!item.Feed.storiesrecentlyviewed.Contains(item.Id))
                        {
                            item.Feed.AddViewedStory(item.Id);
                        }
                    }
                    else
                    {
                        item.Feed.RemoveViewedStory(item.Id);
                    }

                    /* locate actual item if this is a search result */
					var sItem = item as SearchHitNewsItem;
					if (sItem != null)
					{
						owner.FeedSources.ForEach( source =>
							{
								INewsItem realItem = source.FindNewsItem(sItem);
								if (realItem != null)
								{
									realItem.BeenRead = sItem.BeenRead;
									if (modifiedSources.Contains(owner.FeedSources[source.SourceID]))
										modifiedSources.Add(owner.FeedSources[source.SourceID]);

								}
							});
					}
                	//INewsItem realItem = owner.FeedHandler.FindNewsItem(sItem);

					//if (realItem != null && sItem != null)
					//{
					//    realItem.BeenRead = sItem.BeenRead;
					//}
                } //if (item.BeenRead != beenRead)
            } //for(int i=0; i < items.Count; i++) 

            if (modifiedItems.Count > 0)
            {
                var deepModifiedItems = new List<INewsItem>();
                int unexpectedImageState = (beenRead ? 1 : 0);
                // unread-state images always have odd index numbers, read-state are even

                // if there is a self-reference thread, we also have to switch the Gui state for them back
                // these items can also be unselected.
                for (int i = 0; i < listFeedItems.Items.Count; i++)
                {
                    ThreadedListViewItem th = listFeedItems.Items[i];
                    var selfRef = th.Key as INewsItem;

                    foreach (var modifiedItem in modifiedItems)
                    {
                        if (modifiedItem.Equals(selfRef) && (th.ImageIndex%2) == unexpectedImageState)
                        {
                            ApplyStyles(th, beenRead);
                            th.ImageIndex += amount;

                            if (selfRef.BeenRead != beenRead)
                            {
                                // object ref is unequal, but other criteria match the item to be equal...
                                selfRef.BeenRead = beenRead;
                                deepModifiedItems.Add(selfRef);
								FeedSourceEntry src = owner.FeedSources.SourceOf(selfRef.Feed);
								if (src != null && !modifiedSources.Contains(src))
									modifiedSources.Add(src);
                            }
                        }
                    }
                }

                modifiedItems.AddRange(deepModifiedItems);
                
				// we store stories-read in the feedlist, so enable save the new state 
                foreach (FeedSourceEntry src in modifiedSources)
					owner.SubscriptionModified(src, NewsFeedProperty.FeedItemReadState);
                
				// and apply mods. to finders:
                UpdateFindersReadStatus(modifiedItems);

                //TODO: verify correct location  of that code here:
                if (beenRead)
                    UnreadItemsNodeRemoveItems(modifiedItems);
                else
                {
                    UnreadItemsNode.Items.AddRange(modifiedItems);
                    UnreadItemsNode.UpdateReadStatus();
                }
            }

            var sf = CurrentSelectedFeedsNode as ISmartFolder;
            if (sf != null)
            {
                sf.UpdateReadStatus();

                if (!(sf is FinderNode) && !(sf is UnreadItemsNode))
                    return;
            }

            // now update tree state of rss items from any
            // feed (also: category selected)

            var lookup = new Dictionary<string, RefLookupItem>(modifiedItems.Count);

            foreach (var item in modifiedItems)
            {
                string feedurl = item.Feed.link;

                if (feedurl != null)
                {
                    RefLookupItem lookupItem;
                    lookup.TryGetValue(feedurl, out lookupItem);

                    TreeFeedsNodeBase refNode = lookupItem != null ? lookupItem.Node : null;
                    if (refNode == null)
                    {
                        FeedSourceEntry entry = FeedSourceEntryOf(feedurl); 

                        // corresponding node can be at any hierarchy level, or temporary (if commentRss)
                        if (entry != null)
                            refNode = TreeHelper.FindNode(GetSubscriptionRootNode(entry), item);
                        else
                            refNode = TreeHelper.FindNode(GetRoot(RootFolderType.SmartFolders), item);
                    }

                    if (refNode != null)
                    {
                        if (!lookup.ContainsKey(feedurl))
                        {
                            lookup.Add(feedurl, new RefLookupItem(refNode, item.Feed, amount)); // speedup node lookup
                        }
                        else
                        {
                            lookupItem.UnreadCount += amount;
                        }
                        //refNode.UpdateReadStatus(refNode, refNode.UnreadCount + amount);
                        //item.Feed.containsNewMessages = (refNode.UnreadCount > 0);
                    }
                    else
                    {
                        // temp. (item comments)
                        string hash = RssHelper.GetHashCode(item);
                        if (tempFeedItemsRead.ContainsKey(hash))
                            tempFeedItemsRead.Remove(hash);
                    }
                }
            }

            foreach (var item in lookup.Values)
            {
                UpdateTreeNodeUnreadStatus(item.Node, item.Node.UnreadCount + item.UnreadCount);
                item.Feed.containsNewMessages = (item.Node.UnreadCount > 0);
            }
            if (listFeedItemsO.Visible)
                listFeedItemsO.Invalidate();
        }


        /// <summary>
        /// Moves a node to a new parent. 
        /// </summary>
        /// <param name="theNode">FeedTreeNodeBase to move.</param>
        /// <param name="target">New Parent FeedTreeNodeBase.</param>
        /// <param name="userInitiated">Indicates whether the node was moved due to a user initiated action</param>
        public void MoveNode(TreeFeedsNodeBase theNode, TreeFeedsNodeBase target, bool userInitiated)
        {
            if (theNode == null || target == null)
                return;

            if (theNode == target)
                return;

            NewsFeedProperty changes = NewsFeedProperty.None;

            FeedSourceEntry entry = FeedSourceEntryOf(theNode); 

            if (theNode.Type == FeedNodeType.Feed)
            {
				INewsFeed f = owner.GetFeed(entry, theNode.DataKey);
                if (f == null)
                    return;

                if (userInitiated)
                {
                    string category = target.CategoryStoreName;
                    //owner.FeedlistModified = true;
                    if (category != null && !entry.Source.HasCategory(category))
                    {
                        entry.Source.AddCategory(category);
                        changes |= NewsFeedProperty.FeedCategoryAdded;
                    }

                    entry.Source.ChangeCategory(f, category);
                    changes |= NewsFeedProperty.FeedCategory;
                }

                treeFeeds.BeginUpdate();

                if (theNode.UnreadCount > 0)
                    theNode.UpdateReadStatus(theNode.Parent, -theNode.UnreadCount);

                theNode.Parent.Nodes.Remove(theNode);
                target.Nodes.Add(theNode);
                theNode.Control.ActiveNode = theNode;

                if (theNode.UnreadCount > 0)
                    theNode.UpdateReadStatus(theNode.Parent, theNode.UnreadCount);

                theNode.BringIntoView();
                treeFeeds.EndUpdate();

                owner.FeedWasModified(f, changes);
            }
            else if (theNode.Type == FeedNodeType.Category)
            {
                string targetCategory = target.CategoryStoreName;
                string sourceCategory = theNode.CategoryStoreName;

                if (userInitiated)
                {
                    INewsFeedCategory source = null, parent = null;

                    //get source category
                    if (sourceCategory != null && entry.Source.HasCategory(sourceCategory))
                    {
                        source = entry.Source.GetCategories()[sourceCategory];
                        changes |= NewsFeedProperty.FeedCategoryRemoved; //will be removed by ChangeCategory
                    }

                    // get target category if it isn't the rootNodes
                    if (targetCategory != null && entry.Source.HasCategory(targetCategory))
                    {
                        parent = entry.Source.GetCategories()[targetCategory];
                        changes |= NewsFeedProperty.FeedCategoryAdded; //will be added by ChangeCategory
                    }

                    entry.Source.ChangeCategory(source, parent);
                }

                treeFeeds.BeginUpdate();

                if (theNode.UnreadCount > 0)
                    theNode.UpdateReadStatus(theNode.Parent, -theNode.UnreadCount);

                theNode.Parent.Nodes.Remove(theNode);
                target.Nodes.Add(theNode);

                // reset category references on feeds - after moving node to 
                // have the correct FullPath info within this call:
                /* WalkdownThenRenameFeedCategory(theNode, targetCategory); */
                owner.SubscriptionModified(entry, changes);
                //owner.FeedlistModified = true;

                if (theNode.UnreadCount > 0)
                    theNode.UpdateReadStatus(theNode.Parent, theNode.UnreadCount);

                theNode.BringIntoView();
                treeFeeds.EndUpdate();
            }
            else
            {
                Debug.Assert(false, "MoveNode(): unhandled NodeType:'" + theNode.Type);
            }
        }


		///// <summary>
		///// Adds an autodiscovered URL to the auto discovered feeds drop down
		///// </summary>
		///// <param name="info"></param>
		//public void AddAutoDiscoveredUrl(DiscoveredFeedsInfo info)
		//{
		//    var duplicateItem =
		//        new AppButtonToolCommand(
		//            String.Concat("cmdDiscoveredFeed_", ++(AutoDiscoveredFeedsMenuHandler.cmdKeyPostfix)),
		//            owner.BackgroundDiscoverFeedsHandler.mediator,
		//            owner.BackgroundDiscoverFeedsHandler.OnDiscoveredItemClick,
		//            AutoDiscoveredFeedsMenuHandler.StripAndShorten(info.Title), (string) info.FeedLinks[0]);

		//    if (owner.BackgroundDiscoverFeedsHandler.itemDropdown.ToolbarsManager.Tools.Exists(duplicateItem.Key))
		//        owner.BackgroundDiscoverFeedsHandler.itemDropdown.ToolbarsManager.Tools.Remove(duplicateItem);

		//    owner.BackgroundDiscoverFeedsHandler.itemDropdown.ToolbarsManager.Tools.Add(duplicateItem);
		//    duplicateItem.SharedProps.StatusText = info.SiteBaseUrl;
		//    duplicateItem.SharedProps.ShowInCustomizer = false;

		//    Win32.PlaySound(Resource.ApplicationSound.FeedDiscovered);

		//    lock (owner.BackgroundDiscoverFeedsHandler.discoveredFeeds)
		//    {
		//        // add a fresh version of info
		//        owner.BackgroundDiscoverFeedsHandler.discoveredFeeds.Add(duplicateItem, info);
		//    }

		//    lock (owner.BackgroundDiscoverFeedsHandler.newDiscoveredFeeds)
		//    {
		//        // re-order to top of list, in RefreshItemContainer()
		//        owner.BackgroundDiscoverFeedsHandler.newDiscoveredFeeds.Enqueue(duplicateItem);
		//    }
		//}


        /// <summary>
        /// Navigates to the specified URL or local feed item on the GUI thread, if required
        /// </summary>
        /// <param name="url">The webpage or feed item to navigate to</param>
        /// <remarks>If the URL begins with 'http://' then it is a web page, if it begins with 'feed://' then 
        /// it is a reference to a local feed item</remarks>
        public void NavigateToUrlSynchronized(string url){

            if (url.ToLower().StartsWith("http://"))
            {
                InvokeOnGui(delegate
                {
                    //TODO: Add logic to select the specified tab if there is already a tab at that URL
                    this.DetailTabNavigateToUrl(url, String.Empty, false, true);
                });
            }
            else if (url.ToLower().StartsWith("feed://"))
            {
                url = url.Replace("feed://", "http://");
                string[] itemLinks = url.Split('*');

                if (itemLinks.Length == 2)
                {
                   FeedSourceEntry entry = owner.FeedSources.Sources.FirstOrDefault(fse => fse.Source.GetFeeds().ContainsKey(itemLinks[1]));

                   if (entry != null)
                   {
                       INewsFeed feed = entry.Source.GetFeeds()[itemLinks[1]]; 
                       SubscriptionRootNode root = GetSubscriptionRootNode(entry);
                       TreeFeedsNodeBase tn = TreeHelper.FindNode(root, feed);

                       var item = new SearchHitNewsItem(feed, String.Empty, itemLinks[0], String.Empty, String.Empty, DateTime.Now, itemLinks[0]);  

                       InvokeOnGui(delegate
                       {                           
                           this.NavigateToNode(tn, item);
                       });
                   }
                }
            }
        }

        /// <summary>
        /// Calls/opens new feed source wizard on GUI thread, if required
        /// </summary>
        /// <param name="sourceType">the feed source to add</param>
        public void AddFeedSourceSynchronized(FeedSourceType sourceType)
        {
            
            InvokeOnGui(delegate
                            {
                               owner.SynchronizeFeeds(sourceType);
                            });            
        }

        /// <summary>
        /// Calls/Open the newFeedDialog on the GUI thread, if required.
        /// </summary>
        /// <param name="newFeedUrl">Feed Url to add</param>
        public void AddFeedUrlSynchronized(string newFeedUrl)
        {
            InvokeOnGui(delegate
                            {
                                newFeedUrl = owner.HandleUrlFeedProtocol(newFeedUrl);
                                owner.CmdNewFeed(null, newFeedUrl, null);
                            });
        }

        public void OnFeedUpdateStart(FeedSourceEntry entry, Uri feedUri, ref bool cancel)
        {
            string feedUrl;
            TreeFeedsNodeBase feedsNode;

            if (feedUri.IsFile || feedUri.IsUnc)
                feedUrl = feedUri.LocalPath;
            else
                feedUrl = feedUri.CanonicalizedUri();

            INewsFeed f;
            if (entry.Source.GetFeeds().TryGetValue(feedUrl, out f))
            {
                feedsNode = TreeHelper.FindNode(GetSubscriptionRootNode(entry), f);
            }
            else
            {
				feedsNode = TreeHelper.FindNode(GetSubscriptionRootNode(entry), feedUrl);
            }
            if (feedsNode != null)
            {
                SetSubscriptionNodeState(f, feedsNode, FeedProcessingState.Updating);
            }
        }

        public void OnFeedUpdateFinishedWithException(string feedUrl, Exception exception)
        {
            //string feedUrl = null;
            TreeFeedsNodeBase feedsNode;

            /* if (feedUri.IsFile || feedUri.IsUnc) 
				feedUrl = feedUri.LocalPath;
			else
				feedUrl = feedUri.AbsoluteUri; */

            FeedSourceEntry entry = FeedSourceEntryOf(feedUrl);

            if (entry == null) //we couldn't find it's feed source
                return; 

            INewsFeed f = null;
            if (entry.Source.GetFeeds().TryGetValue(feedUrl, out f))
            {
                feedsNode = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), f);
            }
            else
            {
                feedsNode = TreeHelper.FindNode(GetRoot(RootFolderType.MyFeeds), feedUrl);
            }

            if (feedsNode != null)
            {
                SetSubscriptionNodeState(f, feedsNode, FeedProcessingState.Failure);
            }
        }

        public void OnRequestCertificateIssue(object sender, CertificateIssueCancelEventArgs e)
        {
            e.Cancel = true; // by default: do not continue on certificate problems

            if (!Visible) // do not bother if hidden. Just go on an report the issue as a feed error
                return;

            string feedUrl = e.WebRequest.RequestUri.CanonicalizedUri();

            FeedSourceEntry entry = FeedSourceEntryOf(feedUrl);

            if (entry == null) //we couldn't find it's feed source
                return; 

            INewsFeed f = null;
            string issueCaption, issueDesc;

            if (entry.Source.GetFeeds().TryGetValue(feedUrl, out f))
            {
                issueCaption = String.Format(SR.CertificateIssueOnFeedCaption, f.title);
            }
            else
            {
                issueCaption = String.Format(SR.CertificateIssueOnSiteCaption, feedUrl);
            }

            switch (e.CertificateIssue)
            {
                case CertificateIssue.CertCN_NO_MATCH:
                    issueDesc = SR.CertificateIssue_CertCN_NO_MATCH;
                    break;
                case CertificateIssue.CertEXPIRED:
                    issueDesc = String.Format(SR.CertificateIssue_CertEXPIRED, e.Certificate.GetExpirationDateString());
                    break;
                case CertificateIssue.CertREVOKED:
                    issueDesc = SR.CertificateIssue_CertREVOKED;
                    break;
                case CertificateIssue.CertUNTRUSTEDCA:
                    issueDesc = SR.CertificateIssue_CertUNTRUSTEDCA;
                    break;
                case CertificateIssue.CertUNTRUSTEDROOT:
                    issueDesc = SR.CertificateIssue_CertUNTRUSTEDROOT;
                    break;
                case CertificateIssue.CertUNTRUSTEDTESTROOT:
                    issueDesc = SR.CertificateIssue_CertUNTRUSTEDTESTROOT;
                    break;
                case CertificateIssue.CertPURPOSE:
                    issueDesc = SR.CertificateIssue_CertPURPOSE;
                    break;
                case CertificateIssue.CertCHAINING:
                    issueDesc = SR.CertificateIssue_CertCHAINING;
                    break;
                case CertificateIssue.CertCRITICAL:
                    issueDesc = SR.CertificateIssue_CertCRITICAL;
                    break;
                case CertificateIssue.CertISSUERCHAINING:
                    issueDesc = SR.CertificateIssue_CertISSUERCHAINING;
                    break;
                case CertificateIssue.CertMALFORMED:
                    issueDesc = SR.CertificateIssue_CertMALFORMED;
                    break;
                case CertificateIssue.CertPATHLENCONST:
                    issueDesc = SR.CertificateIssue_CertPATHLENCONST;
                    break;
                case CertificateIssue.CertREVOCATION_FAILURE:
                    issueDesc = SR.CertificateIssue_CertREVOCATION_FAILURE;
                    break;
                case CertificateIssue.CertROLE:
                    issueDesc = SR.CertificateIssue_CertROLE;
                    break;
                case CertificateIssue.CertVALIDITYPERIODNESTING:
                    issueDesc = SR.CertificateIssue_CertVALIDITYPERIODNESTING;
                    break;
                case CertificateIssue.CertWRONG_USAGE:
                    issueDesc = SR.CertificateIssue_CertWRONG_USAGE;
                    break;
                default:
                    issueDesc = String.Format(SR.CertificateIssue_Unknown, e.CertificateIssue);
                    break;
            }

            // show cert. issue dialog
            using (var dialog = new SecurityIssueDialog(issueCaption, issueDesc))
            {
                // prepare special command (show certificate):
                dialog.CustomCommand.Tag = e.Certificate;
                dialog.CustomCommand.Click += OnSecurityIssueDialogCustomCommandClick;
                dialog.CustomCommand.Visible = (e.Certificate != null && e.Certificate.Handle != IntPtr.Zero);

                Win32.NativeMethods.SetForegroundWindow(Handle); // ensure, it is in front
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    e.Cancel = false;
                    owner.AddTrustedCertificateIssue(feedUrl, e.CertificateIssue);
                }
            }
        }

        private static void OnSecurityIssueDialogCustomCommandClick(object sender, EventArgs e)
        {
            var cmd = (Button) sender;
            
			cmd.Enabled = false;
            Application.DoEvents();
			CertificateHelper.ShowCertificate((X509Certificate)cmd.Tag);
			cmd.Enabled = true;
			
        }
    }
}