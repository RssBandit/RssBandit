using System;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;
using Microsoft.ApplicationBlocks.ExceptionManagement;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Net;
using RssBandit.Resources;
using RssBandit.WinGui;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Forms;
using RssBandit.WinGui.Interfaces;

namespace RssBandit
{
    internal partial class RssBanditApplication
    {
        #region AsynWebRequest events

        /// <summary>
        /// Called by AsynWebRequest, if a web request caused a certificate problem.
        /// Used to sync. the async. call to the UI thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">CertificateIssueCancelEventArgs</param>
        private void OnRequestCertificateIssue(object sender, CertificateIssueCancelEventArgs e)
        {
            InvokeOnGui(delegate
                            {
                                guiMain.OnRequestCertificateIssue(sender, e);
                            });
        }

        #endregion

        #region FeedSource events

        /// <summary>
        /// Called by FeedSource if feed moved from outside the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="?"></param>
        private void OnMovedFeed(object sender, FeedSource.FeedMovedEventArgs e){
            InvokeOnGui(delegate
            {
                TreeFeedsNodeBase tn = TreeHelper.FindNode(guiMain.GetRoot(RootFolderType.MyFeeds), e.FeedUrl);
                TreeFeedsNodeBase parent = TreeHelper.FindCategoryNode(guiMain.GetRoot(RootFolderType.MyFeeds), e.NewCategory);
                if (tn != null && parent!= null)
                {
                    guiMain.MoveNode(tn, parent, false); 
                    SubscriptionModified(NewsFeedProperty.FeedAdded);
                }
            });
        }

        /// <summary>
        /// Called by FeedSource if feed is renamed from outside the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRenamedFeed(object sender, FeedSource.FeedRenamedEventArgs e)
        {
            InvokeOnGui(delegate
            {
                TreeFeedsNodeBase tn = TreeHelper.FindNode(guiMain.GetRoot(RootFolderType.MyFeeds), e.FeedUrl);
                if (tn != null)
                {
                    guiMain.RenameTreeNode(tn, e.NewName);
                    SubscriptionModified(NewsFeedProperty.FeedTitle);
                }
            });
        }

        /// <summary>
        /// Called by FeedSource if a feed is deleted from outside the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDeletedFeed(object sender, FeedSource.FeedDeletedEventArgs e)
        {
            InvokeOnGui(delegate
            {
                RaiseFeedDeleted(e.FeedUrl, e.Title);
                SubscriptionModified(NewsFeedProperty.FeedRemoved); 
            });
        }

        /// <summary>
        /// Called by FeedSource if a feed is added from outside the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAddedFeed(object sender, FeedSource.FeedChangedEventArgs e)
        {
            InvokeOnGui(delegate
            {
                INewsFeed f = null;
                this.FeedHandler.GetFeeds().TryGetValue(e.FeedUrl, out f);

                if (f != null)
                {
                    guiMain.AddNewFeedNode(f.category, f);
                    SubscriptionModified(NewsFeedProperty.FeedAdded);
                }
            });
        }

        /// <summary>
        /// Called by FeedSource if a category is moved from outside the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMovedCategory(object sender, FeedSource.CategoryChangedEventArgs e)
        {
            InvokeOnGui(delegate
            {
                TreeFeedsNodeBase parent = null, tn = TreeHelper.FindCategoryNode(guiMain.GetRoot(RootFolderType.MyFeeds), e.CategoryName);
                int index = e.NewCategoryName.LastIndexOf(FeedSource.CategorySeparator); 

                if(index == -1){
                    parent = guiMain.GetRoot(RootFolderType.MyFeeds); 
                }else{ 
                    parent = TreeHelper.FindCategoryNode(guiMain.GetRoot(RootFolderType.MyFeeds), e.NewCategoryName.Substring(0, index));
                }
                                                                    
                if (tn != null && parent != null)
                {
                    guiMain.MoveNode(tn, parent, false);
                    SubscriptionModified(NewsFeedProperty.FeedCategoryAdded);
                }
            });

        }


        /// <summary>
        /// Called by FeedSource if a category is renamed from outside the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRenamedCategory(object sender, FeedSource.CategoryChangedEventArgs e)
        {
            InvokeOnGui(delegate
            {
                TreeFeedsNodeBase tn = TreeHelper.FindChildNode(guiMain.GetRoot(RootFolderType.MyFeeds), e.CategoryName, FeedNodeType.Category);
                if (tn != null)
                {
                    guiMain.RenameTreeNode(tn, e.NewCategoryName);
                    SubscriptionModified(NewsFeedProperty.FeedCategoryAdded);
                }
            });

        }


        /// <summary>
        /// Called by FeedSource if a category is added from outside the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAddedCategory(object sender, FeedSource.CategoryEventArgs e)
        {
                InvokeOnGui(delegate
                {
                    this.AddCategory(e.CategoryName);
                    SubscriptionModified(NewsFeedProperty.FeedCategoryAdded); 
                });
            
        }

        /// <summary>
        /// Called by FeedSource if a category is deleted from outside the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDeletedCategory(object sender, FeedSource.CategoryEventArgs e)
        {
            InvokeOnGui(delegate
            {
                TreeFeedsNodeBase tn = TreeHelper.FindChildNode(guiMain.GetRoot(RootFolderType.MyFeeds), e.CategoryName, FeedNodeType.Category);
                if (tn != null)
                {
                    guiMain.DeleteCategory(tn);
                    SubscriptionModified(NewsFeedProperty.FeedCategoryRemoved);
                }
            });

        }

        /// <summary>
        /// Called by FeedSource, if a RefreshFeeds() was initiated (all feeds)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpdateFeedsStarted(object sender, FeedSource.UpdateFeedsEventArgs e)
        {
            InvokeOnGui(delegate
                            {
                                if (e.ForcedRefresh)
                                    stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshAllForced);
                                else
                                    stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshAllAuto);
                            });
        }

        /// <summary>
        /// Called by FeedSource, if a feed start's to download.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BeforeDownloadFeedStarted(object sender, FeedSource.DownloadFeedCancelEventArgs e)
        {
            InvokeOnGui(
                delegate
                    {
                        bool cancel = e.Cancel;
                        guiMain.OnFeedUpdateStart(e.FeedUri, ref cancel);
                        e.Cancel = cancel;
                    });
        }

        /// <summary>
        /// Called by FeedSource, if a Refresh of a individual Feed was initiated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnUpdateFeedStarted(object sender, FeedSource.UpdateFeedEventArgs e)
        {
            InvokeOnGui(delegate
                            {
                                stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshOne);
                            });
        }

        /// <summary>
        /// Called by FeedSource, after a feed was updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnUpdatedFeed(object sender, FeedSource.UpdatedFeedEventArgs e)
        {
            InvokeOnGui(delegate
                            {
                                guiMain.UpdateFeed(e.UpdatedFeedUri, e.NewFeedUri, e.UpdateState == RequestResult.OK);

                                if (e.FirstSuccessfulDownload)
                                {
                                    //new <cacheurl> entry in subscriptions.xml 
                                    SubscriptionModified(NewsFeedProperty.FeedCacheUrl);
                                    //this.FeedlistModified = true; 
                                }
                                stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshOneDone);
                            });
        }

        /// <summary>
        /// Called by FeedSource, after a comment feed was updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnUpdatedCommentFeed(object sender, FeedSource.UpdatedFeedEventArgs e)
        {
            if (e.UpdateState == RequestResult.OK)
            {
                InvokeOnGui(delegate
                                {
                                    guiMain.UpdateCommentFeed(e.UpdatedFeedUri, e.NewFeedUri);
                                });
            }
        }

        /// <summary>
        /// Called by FeedSource, after a favicon was updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnUpdatedFavicon(object sender, FeedSource.UpdatedFaviconEventArgs e)
        {
            InvokeOnGui(delegate
                            {
                                guiMain.UpdateFavicon(e.Favicon, e.FeedUrls);
                            });
        }

        /// <summary>
        /// Called by FeedSource, after an enclosure has been downloaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnDownloadedEnclosure(object sender, DownloadItemEventArgs e)
        {
            /* create playlists in media players if that option is selected */
            if (Preferences.AddPodcasts2WMP)
            {
                AddPodcastToWMP(e.DownloadItem);
            }

            if (Preferences.AddPodcasts2ITunes)
            {
                AddPodcastToITunes(e.DownloadItem);
            }

            /* update GUI if needed */
            InvokeOnGui(delegate
                            {
                                guiMain.OnEnclosureReceived(sender, e);
                            });
        }

        /// <summary>
        /// Called by FeedSource, if update of a feed caused an exception
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnUpdateFeedException(object sender, FeedSource.UpdateFeedExceptionEventArgs e)
        {
            InvokeOnGui(delegate
                            {
                                WebException webex = e.ExceptionThrown as WebException;
                                if (webex != null)
                                {
                                    // yes, WebException
                                    if (webex.Status == WebExceptionStatus.NameResolutionFailure ||
                                        webex.Status == WebExceptionStatus.ProxyNameResolutionFailure)
                                    {
                                        // connection lost?
                                        UpdateInternetConnectionState(true); // update connect state
                                        if (!InternetAccessAllowed)
                                        {
                                            guiMain.UpdateFeed(e.FeedUri, null, false);
                                            stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshOneDone);
                                            return;
                                        }
                                    }
                                }
                                Trace.WriteLine(e.ExceptionThrown.StackTrace);
                                UpdateXmlFeedErrorFeed(e.ExceptionThrown, e.FeedUri, true);
                                stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshOneDone);
                            });
        }

        /// <summary>
        /// Called by FeedSource, if all pending comment feed updates are done.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllCommentFeedRequestsCompleted(object sender, EventArgs e)
        {
            InvokeOnGui(delegate
                            {
                                guiMain.OnAllAsyncUpdateCommentFeedsFinished();
                                //GC.Collect();
                            });
        }

        /// <summary>
        /// Called by FeedSource, if all pending feed updates are done.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllRequestsCompleted(object sender, EventArgs e)
        {
            InvokeOnGui(delegate
                            {
                                stateManager.MoveNewsHandlerStateTo(NewsHandlerState.RefreshAllDone);
                                guiMain.TriggerGUIStateOnNewFeeds(true);
                                guiMain.OnAllAsyncUpdateFeedsFinished();
                                // GC.Collect();
                            });
        }

        #endregion

        private void OnLoadingFeedlistProgress(object sender, ThreadWorkerProgressArgs args)
        {
            if (args.Exception != null)
            {
                // failure(s)
                args.Cancel = true;
                BanditApplicationException ex = args.Exception as BanditApplicationException;
                if (ex != null)
                {
                    if (ex.Number == ApplicationExceptions.FeedlistOldFormat)
                    {
                        Application.Exit();
                    }
                    else if (ex.Number == ApplicationExceptions.FeedlistOnRead)
                    {
                        ExceptionManager.Publish(ex.InnerException);
                        this.MessageError(SR.ExceptionReadingFeedlistFile(ex.InnerException.Message, GetLogFileName()));
                        this.SetGuiStateFeedbackText(SR.GUIStatusErrorReadingFeedlistFile);
                    }
                    else if (ex.Number == ApplicationExceptions.FeedlistOnProcessContent)
                    {
                        this.MessageError(SR.InvalidFeedlistFileMessage(GetLogFileName()));
                        this.SetGuiStateFeedbackText(SR.GUIStatusValidationErrorReadingFeedlistFile);
                    }
                    else if (ex.Number == ApplicationExceptions.FeedlistNA)
                    {
                        this.refreshRate = feedHandler.RefreshRate;
                        this.SetGuiStateFeedbackText(SR.GUIStatusNoFeedlistFile);
                    }
                    else
                    {
                        PublishException(args.Exception);
                        this.SetGuiStateFeedbackText(SR.GUIStatusErrorReadingFeedlistFile);
                    }
                }
                else
                {
                    // unhandled
                    PublishException(args.Exception);
                    this.SetGuiStateFeedbackText(SR.GUIStatusErrorReadingFeedlistFile);
                }
            }
            else if (!args.Done)
            {
                // in progress
                if (!IsFormAvailable(guiMain))
                {
                    args.Cancel = true;
                    return;
                }
                this.SetGuiStateFeedbackText(SR.GUIStatusLoadingFeedlist);
            }
            else if (args.Done)
            {
                // done
                this.refreshRate = feedHandler.RefreshRate; // loaded from feedlist

                this.CheckAndMigrateSettingsAndPreferences(); // needs the feedlist to be loaded
                this.CheckAndMigrateListViewLayouts();

                //resume pending enclosure downloads
                this.feedHandler.ResumePendingDownloads();

                if (!IsFormAvailable(guiMain))
                {
                    args.Cancel = true;
                    return;
                }

                try
                {
                    this.guiMain.PopulateFeedSubscriptions(feedHandler.GetCategories().Values, feedHandler.GetFeeds(),
                                                           DefaultCategory);
                }
                catch (Exception ex)
                {
                    PublishException(ex);
                }

                if (FeedlistLoaded != null)
                    FeedlistLoaded(this, EventArgs.Empty);

                this.SetGuiStateFeedbackText(SR.GUIStatusDone);

                //TODO: move this out of the Form code to allow dynamic create/dispose of the main form from the system tray menu
                foreach (string newFeedUrl in this.commandLineOptions.SubscribeTo)
                {
                    if (IsFormAvailable(guiMain))
                        this.guiMain.AddFeedUrlSynchronized(newFeedUrl);
                }

                // start load items and refresh from web, if we have to refresh on startup:
                guiMain.UpdateAllFeeds(this.Preferences.FeedRefreshOnStartup);
            }
        }
    }
}