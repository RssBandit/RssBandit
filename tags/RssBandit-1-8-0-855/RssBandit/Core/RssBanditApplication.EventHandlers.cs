using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Microsoft.ApplicationBlocks.ExceptionManagement;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Net;
using NewsComponents.Utils;
using RssBandit.Resources;
using RssBandit.WinGui;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Forms;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Utility;

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
            InvokeOnGui(() => guiMain.OnRequestCertificateIssue(sender, e));
        }

        #endregion

        #region FeedSource events

		/// <summary>
		/// Called by FeedSource if feed moved from outside the application
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="NewsComponents.FeedSource.FeedMovedEventArgs"/> instance containing the event data.</param>
        private void OnMovedFeed(object sender, FeedSource.FeedMovedEventArgs e){
			FeedSourceEntry entry = sourceManager.SourceOf((FeedSource)sender);
			InvokeOnGui(delegate
            {
            	TreeFeedsNodeBase start = guiMain.GetSubscriptionRootNode(entry);
                TreeFeedsNodeBase tn = TreeHelper.FindNode(start, e.FeedUrl);
                TreeFeedsNodeBase parent = TreeHelper.FindCategoryNode(start, e.NewCategory);
                if (tn != null && parent!= null)
                {
                    guiMain.MoveNode(tn, parent, false); 
                    SubscriptionModified(entry, NewsFeedProperty.FeedAdded);
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
			FeedSourceEntry entry = sourceManager.SourceOf((FeedSource)sender);
			InvokeOnGui(delegate
            {
                TreeFeedsNodeBase tn = TreeHelper.FindNode(guiMain.GetSubscriptionRootNode(entry), e.FeedUrl);
                if (tn != null)
                {
                    guiMain.RenameTreeNode(tn, e.NewName);
                    SubscriptionModified(entry, NewsFeedProperty.FeedTitle);
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
			FeedSourceEntry entry = sourceManager.SourceOf((FeedSource)sender);
        	InvokeOnGui(delegate
            {
                RaiseFeedDeleted(entry, e.FeedUrl, e.Title);
                SubscriptionModified(entry, NewsFeedProperty.FeedRemoved); 
            });
        }

        /// <summary>
        /// Called by <see cref="FeedSource"/> if a feed is added from outside the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAddedFeed(object sender, FeedSource.FeedChangedEventArgs e)
        {
			FeedSourceEntry entry = sourceManager.SourceOf((FeedSource)sender);
			InvokeOnGui(delegate
            {
                INewsFeed f;
                entry.Source.GetFeeds().TryGetValue(e.FeedUrl, out f);

                if (f != null)
                {
                    guiMain.AddNewFeedNode(entry, f.category, f);
                    SubscriptionModified(entry, NewsFeedProperty.FeedAdded);
                }
            });
        }

        /// <summary>
        /// Called by <see cref="FeedSource"/> if a category is moved from outside the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMovedCategory(object sender, FeedSource.CategoryChangedEventArgs e)
        {
			FeedSourceEntry entry = sourceManager.SourceOf((FeedSource)sender);
			InvokeOnGui(delegate
            {
                TreeFeedsNodeBase parent, tn = TreeHelper.FindCategoryNode(guiMain.GetSubscriptionRootNode(entry), e.CategoryName);
                int index = e.NewCategoryName.LastIndexOf(FeedSource.CategorySeparator); 

                if(index == -1){
					parent = guiMain.GetSubscriptionRootNode(entry); 
                }else{
					parent = TreeHelper.FindCategoryNode(guiMain.GetSubscriptionRootNode(entry), e.NewCategoryName.Substring(0, index));
                }
                                                                    
                if (tn != null && parent != null)
                {
                    guiMain.MoveNode(tn, parent, false);
                    SubscriptionModified(entry, NewsFeedProperty.FeedCategoryAdded);
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
			FeedSourceEntry entry = sourceManager.SourceOf((FeedSource)sender);
			InvokeOnGui(delegate
            {
				TreeFeedsNodeBase tn = TreeHelper.FindChildNode(guiMain.GetSubscriptionRootNode(entry), e.CategoryName, FeedNodeType.Category);
                if (tn != null)
                {
                    guiMain.RenameTreeNode(tn, e.NewCategoryName);
                    SubscriptionModified(entry, NewsFeedProperty.FeedCategoryAdded);
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
			FeedSourceEntry entry = sourceManager.SourceOf((FeedSource)sender);
			InvokeOnGui(delegate
                {
                    this.AddCategory(entry, e.CategoryName);
                    SubscriptionModified(entry, NewsFeedProperty.FeedCategoryAdded); 
                });
            
        }

        /// <summary>
        /// Called by FeedSource if a category is deleted from outside the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDeletedCategory(object sender, FeedSource.CategoryEventArgs e)
        {
			FeedSourceEntry entry = sourceManager.SourceOf((FeedSource)sender);
			InvokeOnGui(delegate
            {
                TreeFeedsNodeBase tn = TreeHelper.FindChildNode(guiMain.GetRoot(RootFolderType.MyFeeds), e.CategoryName, FeedNodeType.Category);
                if (tn != null)
                {
                    guiMain.DeleteCategory(tn);
                    SubscriptionModified(entry, NewsFeedProperty.FeedCategoryRemoved);
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
                                    stateManager.MoveNewsHandlerStateTo(FeedSourceBusyState.RefreshAllForced);
                                else
                                    stateManager.MoveNewsHandlerStateTo(FeedSourceBusyState.RefreshAllAuto);
                            });
        }

        /// <summary>
        /// Called by FeedSource, if a feed start's to download.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BeforeDownloadFeedStarted(object sender, FeedSource.DownloadFeedCancelEventArgs e)
        {
        	FeedSourceEntry entry = sourceManager.SourceOf((FeedSource)sender);
        	InvokeOnGui(
                delegate
                    {
                        bool cancel = e.Cancel;
						guiMain.OnFeedUpdateStart(entry, e.FeedUri, ref cancel);
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
                                stateManager.MoveNewsHandlerStateTo(FeedSourceBusyState.RefreshOne);
                            });
        }

        /// <summary>
        /// Called by <see cref="FeedSource"/>, after a feed was updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnUpdatedFeed(object sender, FeedSource.UpdatedFeedEventArgs e)
        {
			FeedSourceEntry entry = sourceManager.SourceOf((FeedSource)sender);
			InvokeOnGui(delegate
                            {
                                guiMain.UpdateFeed(entry, e.UpdatedFeedUri, e.NewFeedUri, e.UpdateState == RequestResult.OK);

                                if (e.FirstSuccessfulDownload)
                                {
                                    //new <cacheurl> entry in subscriptions.xml 
                                    SubscriptionModified(entry, NewsFeedProperty.FeedCacheUrl);
                                    //this.FeedlistModified = true; 
                                }
                                stateManager.MoveNewsHandlerStateTo(FeedSourceBusyState.RefreshOneDone);
                            });
        }

        /// <summary>
        /// Called by <see cref="FeedSource"/>, after a comment feed was updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnUpdatedCommentFeed(object sender, FeedSource.UpdatedFeedEventArgs e)
        {
            if (e.UpdateState == RequestResult.OK)
            {
                InvokeOnGui(() => guiMain.UpdateCommentFeed(e.UpdatedFeedUri, e.NewFeedUri));
            }
        }

        /// <summary>
        /// Called by <see cref="FeedSource"/>, after a favicon was updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnUpdatedFavicon(object sender, FeedSource.UpdatedFaviconEventArgs e)
        {
			FeedSourceEntry entry = sourceManager.SourceOf((FeedSource)sender);
			InvokeOnGui(() => guiMain.UpdateFavicon(entry, e.Favicon, e.FeedUrls));
        }

        /// <summary>
        /// Called by FeedSource, after an enclosure has been downloaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnDownloadedEnclosure(object sender, DownloadItemEventArgs e)
        {
			FeedSourceEntry entry = sourceManager.SourceOf((FeedSource)sender);
			
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
                                guiMain.OnEnclosureReceived(entry, e);
                            });
        }

        /// <summary>
        /// Called by <see cref="FeedSource"/>, if update of a feed caused an exception
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnUpdateFeedException(object sender, FeedSource.UpdateFeedExceptionEventArgs e)
        {
			FeedSourceEntry entry = sourceManager.SourceOf((FeedSource)sender);
			InvokeOnGui(delegate
                            {
                                if (e.ExceptionThrown.InnerException is ClientCertificateRequiredException)
                                {
									X509Certificate2 cert = CertificateHelper.SelectCertificate(
                                		String.Format(SR.Certificate_ForFeedSiteRequired_Message, e.FeedUri));

									if (cert != null)
									{
										GetFeed(entry, e.FeedUri).certificateId = cert.Thumbprint;
										SubscriptionModified(entry, NewsFeedProperty.FeedCredentials);
									}
                                }

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
                                            guiMain.UpdateFeed(entry, e.FeedUri, null, false);
                                            stateManager.MoveNewsHandlerStateTo(FeedSourceBusyState.RefreshOneDone);
                                            return;
                                        }
                                    }
                                }
                                Trace.WriteLine(e.ExceptionThrown.StackTrace);
                                UpdateXmlFeedErrorFeed(e.ExceptionThrown, e.FeedUri, true, entry);
                                stateManager.MoveNewsHandlerStateTo(FeedSourceBusyState.RefreshOneDone);
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
			FeedSourceEntry entry = sourceManager.SourceOf((FeedSource)sender);
			InvokeOnGui(delegate
                            {
                                stateManager.MoveNewsHandlerStateTo(FeedSourceBusyState.RefreshAllDone);
                                guiMain.TriggerGUIStateOnNewFeeds(true);
                                guiMain.OnAllAsyncUpdateFeedsFinished(entry);
                                // GC.Collect();
                            });
        }

        #endregion

        #region OnLoadingFeedlistProgress - dead code

        /* 

        private void OnLoadingFeedlistProgress(object sender, ThreadWorkerProgressArgs args)
        {
            if (args.Exception != null)
            {
                // failure(s)
                args.Cancel = true;
                BanditApplicationException ex = args.Exception as BanditApplicationException;
                if (ex != null)
                {
                    ex.PreserveExceptionStackTrace();

                    if (ex.Number == ApplicationExceptions.FeedlistOldFormat)
                    {
                        Application.Exit();
                    }
                    else if (ex.Number == ApplicationExceptions.FeedlistOnRead)
                    {
                        ExceptionManager.Publish(ex);
                        this.MessageError(String.Format(SR.ExceptionReadingFeedlistFile,ex.InnerException.Message, GetLogFileName()));
                        this.SetGuiStateFeedbackText(SR.GUIStatusErrorReadingFeedlistFile);
                    }
                    else if (ex.Number == ApplicationExceptions.FeedlistOnProcessContent)
                    {
                        this.MessageError(String.Format(SR.InvalidFeedlistFileMessage,GetLogFileName()));
                        this.SetGuiStateFeedbackText(SR.GUIStatusValidationErrorReadingFeedlistFile);
                    }
                    else if (ex.Number == ApplicationExceptions.FeedlistNA)
                    {
						if (this.Preferences.RefreshRate < 0)
							this.Preferences.RefreshRate = FeedSource.DefaultRefreshRate;
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
                    //this.guiMain.PopulateFeedSubscriptions(feedHandler.GetCategories().Values, feedHandler.GetFeeds(),
                     //                                      DefaultCategory);
                }
                catch (Exception ex)
                {
                    ex.PreserveExceptionStackTrace();
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
         */

        #endregion 
    }
}