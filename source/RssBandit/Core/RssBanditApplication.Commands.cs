using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using NewsComponents;
using NewsComponents.Feed;
using NewsComponents.Utils;
using RssBandit.Resources;
using RssBandit.WinGui;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Dialogs;
using RssBandit.WinGui.Forms;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Menus;
using RssBandit.WinGui.Utility;

using RssBandit.Common;

namespace RssBandit
{
    internal partial class RssBanditApplication
    {
        /// <summary>
        /// Catch Up the current selected node in subscriptions treeview.
        /// Works for all subscription types (feed, category, all).
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdCatchUpCurrentSelectedNode(ICommand sender)
        {
            TreeFeedsNodeBase tn = guiMain.CurrentSelectedFeedsNode; 
            guiMain.MarkSelectedNodeRead(tn);
            if (guiMain.CurrentSelectedFeedsNode != null)
                FeedWasModified(guiMain.FeedSourceEntryOf(tn), guiMain.CurrentSelectedFeedsNode.DataKey, NewsFeedProperty.FeedItemReadState);
            //this.FeedlistModified = true;
            if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }

        /// <summary>
        /// Called only from subscriptions tree on SmartFolder(s)
        /// </summary>
        /// <param name="sender"></param>
        public void CmdDeleteAllFeedItems(ICommand sender)
        {
            TreeFeedsNodeBase tn = guiMain.CurrentSelectedFeedsNode;

            if (tn != null)
            {
                ISmartFolder isFolder = tn as ISmartFolder;

                if (isFolder != null)
                {
                    if (isFolder is FlaggedItemsNode || isFolder is WatchedItemsNode)
                    {
                        // we need to unflag the items within each subscribed feed:
                        for (int i = 0, j = isFolder.Items.Count; i < j; i++)
                        {
                            INewsItem item = isFolder.Items[0];
                            RemoveItemFromSmartFolder(isFolder, item);
                        }
                    }
                    else
                    {
                        // simply clr all
                        isFolder.Items.Clear();
                    }
                    isFolder.Modified = true;
                    guiMain.PopulateSmartFolder(tn, true);
                    guiMain.UpdateTreeNodeUnreadStatus(tn, 0);
                    return;
                }
            }

            if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }

        /// <summary>
        /// Called from listview context menu or Edit|Delete menu
        /// </summary>
        /// <param name="sender"></param>
        public void CmdDeleteSelectedFeedItems(ICommand sender)
        {
            if (guiMain.CurrentSelectedFeedsNode != null)
            {
                guiMain.RemoveSelectedFeedItems();
            }
        }

        /// <summary>
        /// Called from listview context menu or Edit|Restore items menu
        /// </summary>
        /// <param name="sender"></param>
        public void CmdRestoreSelectedFeedItems(ICommand sender)
        {
            if (guiMain.CurrentSelectedFeedsNode != null)
            {
                guiMain.RestoreSelectedFeedItems();
            }
        }

        /// <summary>
        /// Pops up the NewFeedDialog class and adds a new feed to the list 
        /// of subscribed feeds.
        /// </summary>
        /// <param name="category">Feed category</param>
        /// <param name="feedLink">Feed link</param>
        /// <param name="feedTitle">Feed title</param>
        /// <returns>true, if the dialog succeeds (feed subscribed), else false</returns>
        public bool CmdNewFeed(string category, string feedLink, string feedTitle)
        {
            return SubscribeToFeed(feedLink, category, feedTitle);

            #region not reached (there for ref.)

            /*
			bool success = true;
			NewFeedDialog newFeedDialog = new NewFeedDialog(category, defaultCategory, feedHandler.Categories, feedLink, feedTitle); 
			
			newFeedDialog.btnLookupTitle.Enabled = this.InternetAccessAllowed;
			newFeedDialog.Proxy = this.Proxy; 

			try {
				Win32.SetForegroundWindow(MainForm.Handle);
				newFeedDialog.ShowDialog(guiMain);
			} catch {}

			try{ 

				if(newFeedDialog.DialogResult == DialogResult.OK) {
				
					NewsFeed f = new NewsFeed(); 
					f.link  = newFeedDialog.FeedUrl; 
					f.title = newFeedDialog.FeedTitle;
					f.refreshrate = 60; 
					//f.storiesrecentlyviewed = new ArrayList(); 				
					//f.deletedstories = new ArrayList(); 				

					//handle the common case of feed URI not beginning with HTTP 
					try{ 
						Uri reqUri = new Uri(f.link);
						f.link     = reqUri.ToString().Replace("\r\n", String.Empty); //some weird URLs have newlines						
					}catch(UriFormatException){

						if(!f.link.ToLower().StartsWith("http://")){							
							Uri reqUri = new Uri("http://" + f.link); 
							f.link     = reqUri.ToString().Replace("\r\n", String.Empty); 					
						}
				
					}

					if(feedHandler.GetFeeds().Contains(f.link)) {
						NewsFeed f2 = feedHandler.GetFeeds()[f.link]; 
						this.MessageInfo("RES_GUIFieldLinkRedundantInfo", 
							(f2.category == null? String.Empty : category + "\\") + f2.title, f2.link );
						newFeedDialog.Close(); 
						return success; 
					}

					if((newFeedDialog.FeedCategory != null) && 
						(!newFeedDialog.FeedCategory.Equals(String.Empty)) && 
						(!newFeedDialog.FeedCategory.Equals(defaultCategory))) {
						f.category = newFeedDialog.FeedCategory; 

						if(!feedHandler.Categories.ContainsKey(f.category)) {
							feedHandler.Categories.Add(f.category); 
						}
					}

					if (newFeedDialog.textUser.Text != null && newFeedDialog.textUser.Text.Trim().Length != 0 ) {	// set NewsFeed new credentials
						string u = newFeedDialog.textUser.Text.Trim(), p = null;
						if (newFeedDialog.textPwd.Text != null && newFeedDialog.textPwd.Text.Trim().Length != 0)
							p = newFeedDialog.textPwd.Text.Trim();
						FeedSource.SetFeedCredentials(f, u, p);
					} else {
						FeedSource.SetFeedCredentials(f, null, null);
					}

					f.alertEnabled = f.alertEnabledSpecified = newFeedDialog.checkEnableAlerts.Checked;

					feedHandler.GetFeeds().Add(f.link, f); 
					this.FeedlistModified = true;

					guiMain.AddNewFeedNode(f.category, f);
					guiMain.DelayTask(DelayedTasks.StartRefreshOneFeed, f.link);
					success = true;

				} else {
					success = false;
				}

			}catch(UriFormatException){
				this.MessageError("RES_InvalidFeedURIBoxText");
			}
			
			newFeedDialog.Dispose();
			return success;
*/

            #endregion
        }

        /// <summary>
        /// Exiting the Application.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdExitApp(ICommand sender)
        {
            // really exit app.
            if (guiMain != null)
            {
                guiMain.Close(true);
            }
            Application.Exit();
        }

        /// <summary>
        /// Show Alert Windows Mode: None.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdShowAlertWindowNone(ICommand sender)
        {
            Mediator.SetChecked(
                "+cmdShowAlertWindowNone",
                "-cmdShowAlertWindowConfiguredFeeds",
                "-cmdShowAlertWindowAll");
            Preferences.ShowAlertWindow = DisplayFeedAlertWindow.None;
            SavePreferences();
        }

        /// <summary>
        /// Show Alert Windows Mode: Configured Feeds.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdShowAlertWindowConfigPerFeed(ICommand sender)
        {
            Mediator.SetChecked(
                "-cmdShowAlertWindowNone",
                "+cmdShowAlertWindowConfiguredFeeds",
                "-cmdShowAlertWindowAll");
            Preferences.ShowAlertWindow = DisplayFeedAlertWindow.AsConfiguredPerFeed;
            SavePreferences();
        }

        /// <summary>
        /// Show Alert Windows Mode: All.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdShowAlertWindowAll(ICommand sender)
        {
            Mediator.SetChecked(
                "-cmdShowAlertWindowNone",
                "-cmdShowAlertWindowConfiguredFeeds",
                "+cmdShowAlertWindowAll");
            Preferences.ShowAlertWindow = DisplayFeedAlertWindow.All;
            SavePreferences();
        }

        /// <summary>
        /// Toggle the Show New Items Received Balloon Mode.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdToggleShowNewItemsReceivedBalloon(ICommand sender)
        {
            bool currentChecked;

            if (sender != null) // really sent by the Gui Component
                currentChecked = ((ICommandComponent) sender).Checked;
            else
                currentChecked = Mediator.IsChecked("cmdShowNewItemsReceivedBalloon");

            Preferences.ShowNewItemsReceivedBalloon = !currentChecked;
            Mediator.SetChecked(!currentChecked, "cmdShowNewItemsReceivedBalloon");
            SavePreferences();
        }

        /// <summary>
        /// Toggle the Internet Connection Mode.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdToggleInternetConnectionMode(ICommand sender)
        {
            bool currentChecked;

            if (sender != null) // really sent by the Gui Component
                currentChecked = ((ICommandComponent) sender).Checked;
            else
                currentChecked = Mediator.IsChecked("cmdToggleOfflineMode");

            Utils.SetIEOffline(currentChecked); // update IE
            UpdateInternetConnectionState(true); // get new network state, takes a few msecs
        }

        /// <summary>
        /// Display the about box.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdAboutApp(ICommand sender)
        {
            // view about box. 
            // ToDo: add advertising links:
            // ... and to the project workspace/bug report section, license etc.

            MessageBox.Show(Caption + " written by\n\n" +
                            "  * Dare Obasanjo (DareObasanjo, www.25hoursaday.com/weblog/)\n" +
                            "  * Torsten Rendelmann (TorstenR, www.rendelmann.info/blog/)\n" +
                            "  * Oren Novotny\n" +
                            "  * Phil Haack (haacked.com)\n" +
                            "  * and all the active members of RSS Bandit community.\n" +
                            "\nCredits:\n\n" +
                            "  * Mike Krueger (#ZipLib)\n" +
                            "  * Jack Palevich (NntpClient)\n" +
                            "  * NetAdvantage for Windows Forms (c) 2008 by Infragistics, http://www.infragistics.com\n" +
                            "  * SandBar, SandDock (c) 2005 by Divelements Limited, http://www.divil.co.uk/net/\n" +
                            "  * Portions Copyright ©2002-2004 The Genghis Group (www.genghisgroup.com)\n" +
                            "  * sourceforge.net team (Project hosting)", String.Format(SR.WindowAboutCaption,CaptionOnly),
                            MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        /// <summary>
        /// Display the Help Web-Page.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdWebHelp(ICommand sender)
        {
			NavigateToUrlInExternalBrowser(Resource.OutgoingLinks.WebHelpUrl);
        }

        /// <summary>
        /// Display the Bug Report Web-Page.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdReportAppBug(ICommand sender)
        {
			NavigateToUrlAsUserPreferred(Resource.OutgoingLinks.BugReportUrl, CaptionOnly + ": Bug Tracker", true, true);
        }

        /// <summary>
        /// Display the Blog News Web-Page.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdProjectBlogNews(ICommand sender)
        {
            NavigateToUrlAsUserPreferred(Resource.OutgoingLinks.ProjectBlogUrl, CaptionOnly + ": Project Blog", true, true);
        }

        /// <summary>
        /// Display the Project News Web-Page.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdProjectNews(ICommand sender)
        {
			NavigateToUrlAsUserPreferred(Resource.OutgoingLinks.ProjectNewsUrl, CaptionOnly + ": Project News", true, true);
        }

        /// <summary>
        /// Display the Wiki News Web-Page.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdVisitForum(ICommand sender)
        {
            NavigateToUrlAsUserPreferred(Resource.OutgoingLinks.UserForumUrl, CaptionOnly + ": Forum", true, true);
        }

        /// <summary>
        /// Display Donate to project Web-Page.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdDonateToProject(ICommand sender)
        {
			NavigateToUrlAsUserPreferred(Resource.OutgoingLinks.ProjectDonationUrl, CaptionOnly + ": Donate", true, true);
        }


        /// <summary>
        /// Send logs by mail.
        /// </summary>
        /// <param name="sender">The sender.</param>
        public void CmdSendLogsByMail(ICommand sender)
        {
            List<string> files = new List<string>();

            try
            {
                // log files are configured in the RssBandit.exe.log4net.config
                // as to be ${APPDATA}\\RssBandit\\trace.log, NOT always at the possibly reconfigured
                // RssBanditApplciation.GetUserPath() location:
                string logFilesFolder =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Name);
                if (Directory.Exists(logFilesFolder))
                {
                    string[] matches = Directory.GetFiles(logFilesFolder, "trace.log*");

                    if (matches.Length > 0)
                    {
                        files.AddRange(matches);
                        if (Preferences.UseProxy)
                        {
                            // include preferences to get proxy infos 
                            // (pwds are encrypted, so we don't sniff sensitive infos here)
                            files.Add(GetPreferencesFileName());
                        }

                        string zipDest = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                            "RssBandit.logs." + VersionLong + ".zip");

                        if (File.Exists(zipDest))
                            FileHelper.Delete(zipDest);

                        FileHelper.ZipFiles(files.ToArray(), zipDest);
                        // open a mailto:contact@rssbandit.org mail window with
                        // hints how to attach the zip

                        Process.Start(CreateMailUrlMessage(
                                          "contact@rssbandit.org",
                                          "Log files -- RSS Bandit v" + VersionLong,
                                          "Please attach this file from your My Documents folder:\r\n\n" + zipDest +
                                          "\r\n\r\n" +
                                          "and replace this text with some more useful informations about: \r\n" +
                                          "\t* Your system environment and OS version\r\n" +
                                          "\t* Description of the issue to report\r\n" +
                                          "\t* Any hints/links that may help,\r\nplease!"));
                    }
                    else
                    {
                        MessageInfo("No log files at " + logFilesFolder);
                    }
                }
                else
                {
                    MessageInfo("No log files at " + logFilesFolder);
                }
            }
            catch (Exception ex)
            {
                _log.Error("Failed to send log files", ex);
                MessageError("Failed to send log files: \r\n" + ex.Message);
            }
        }

        private static string CreateMailUrlMessage(string to, string subject, string text)
        {
            subject = HtmlHelper.UrlEncode(subject);
            string body = HtmlHelper.UrlEncode(text);
            if (body.Length + subject.Length > 900)
            {
                if (subject.Length > 400)
                {
                    subject = subject.Substring(0, 400) + "...";
                }
                body = body.Substring(0, 897 - subject.Length) + "...";
            }

            return "mailto:" + to + "?subject=" + subject + "&body=" + body;
        }

        /// <summary>
        /// Check for program updates.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdCheckForUpdates(ICommand sender)
        {
            CmdCheckForUpdates(AutoUpdateMode.Manually);
        }

        /// <summary>
        /// Check for program updates.
        /// </summary>
        /// <param name="mode">Update Mode</param>
        public void CmdCheckForUpdates(AutoUpdateMode mode)
        {
            if (mode == AutoUpdateMode.Manually)
                CheckForUpdates(mode);
            else
            {
                // called on App Startup.

                if (!InternetAccessAllowed)
                    return;

                // consider preferences settings
                if (Preferences.AutoUpdateFrequency == AutoUpdateMode.Manually)
                    return;

                if (Preferences.AutoUpdateFrequency == AutoUpdateMode.OnApplicationStart &&
                    mode == AutoUpdateMode.OnApplicationStart)
                {
                    CheckForUpdates(mode);
                    return;
                }
                // check, if it is time to check for updates
                DateTime t = LastAutoUpdateCheck;
                if (Preferences.AutoUpdateFrequency == AutoUpdateMode.OnceIn14Days)
                    t = t.AddDays(14);
                else
                    t = t.AddMonths(1);
                if (DateTime.Compare(t, DateTime.Now) < 0)
                    CheckForUpdates(mode);
            }
        }

        private void OnApplicationUpdateAvailable(object sender, UpdateAvailableEventArgs e)
        {
            AutoUpdateMode mode = (AutoUpdateMode) RssBanditUpdateManager.Tag;
            bool hasUpdates = e.NewVersionAvailable;

            if (hasUpdates)
            {
                if (DialogResult.No == MessageQuestion(SR.DialogQuestionNewAppVersionAvailable))
                {
                    LastAutoUpdateCheck = DateTime.Now;
                }
                else
                {
                    //RssBanditUpdateManager.BeginDownload(updateManager.AvailableUpdates);	// Async. Preferences updated in OnUpdateComplete event

                    // for now we do not download anything, just display the SF download page:
					NavigateToUrlAsUserPreferred(Resource.OutgoingLinks.ProjectDownloadUrl, CaptionOnly + ": Download", true, true);
                    LastAutoUpdateCheck = DateTime.Now;
                }
            }
            else
            {
                LastAutoUpdateCheck = DateTime.Now;

                if (mode == AutoUpdateMode.Manually)
                    MessageInfo(SR.DialogMessageNoNewAppVersionAvailable);
            }
        }

        private void CheckForUpdates(AutoUpdateMode mode)
        {
            try
            {
                RssBanditUpdateManager.Tag = mode;
                if (mode == AutoUpdateMode.Manually)
                    RssBanditUpdateManager.BeginCheckForUpdates(guiMain, Proxy);
                else
                    RssBanditUpdateManager.BeginCheckForUpdates(null, Proxy);
            }
            catch (Exception ex)
            {
                _log.Error("RssBanditUpdateManager.BeginCheckForUpdates() failed", ex);
            }
        }

        /// <summary>
        /// Re-Display/Open the main GUI.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdShowMainGui(ICommand sender)
        {
            InvokeOnGui(() => guiMain.DoShow());
        }

        /// <summary>
        /// Refresh all feeds.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdRefreshFeeds(ICommand sender)
        {
            guiMain.UpdateAllFeeds(true);
            if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }

        /// <summary>
        /// Pops up the SubscriptionWizard and adds a new feed to the list 
        /// of subscribed feeds. It uses WizardMode.SubscribeURLDirect.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdNewFeed(ICommand sender)
        {
            string category = guiMain.CategoryOfSelectedNode() ?? DefaultCategory;

        	SubscribeToFeed(null, category.Trim(), null, null, AddSubscriptionWizardMode.SubscribeURLDirect);

            if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }

        /// <summary>
        /// Pops up the SubscriptionWizard and adds a new feed to the list 
        /// of subscribed feeds. WizardMode.Default
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdNewSubscription(ICommand sender)
        {
            string category = guiMain.CategoryOfSelectedNode() ?? DefaultCategory;

        	SubscribeToFeed(null, category.Trim(), null, null, AddSubscriptionWizardMode.Default);

            if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }

        /// <summary>
        /// Pops up the SubscriptionWizard and adds a new feed to the list 
        /// of subscribed feeds. WizardMode.SubscribeNNTPDirect
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdNewNntpFeed(ICommand sender)
        {
            string category = guiMain.CategoryOfSelectedNode() ?? DefaultCategory;

        	SubscribeToFeed(null, category.Trim(), null, null, AddSubscriptionWizardMode.SubscribeNNTPDirect);

            if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }

        /// <summary>
        /// Moves the focus to the next unread feed item if available.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdNextUnreadFeedItem(ICommand sender)
        {
            guiMain.MoveToNextUnreadItem();
            if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }

        /// <summary>
        /// Display a dialog to manage AddIns.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdOpenManageAddInsDialog(ICommand sender)
        {
            using (ManageAddInDialog dialog = new ManageAddInDialog())
            {
            	dialog.ShowDialog(MainForm);
            }
        }


        /// <summary>
        /// Display the most popular recently linked stories in the 
        /// Web browser pane. 
        /// </summary>
        /// <param name="sender"></param>
        public void CmdTopStories(ICommand sender)
        {
            string memeFile = GetTopStoriesFileName();
            TopStoriesThreadHandler th = new TopStoriesThreadHandler(this, memeFile);

            DialogResult result = th.Start(guiMain, SR.ProcessTopStoriesEntertainmentWaitMessage, true);

            if (result != DialogResult.OK)
                return;

            if (!th.OperationSucceeds)
            {
                MessageError("The following error occured while determining Top Stories" +
                             th.OperationException.Message);
                return;
            }

            NavigateToUrl(memeFile, null, true, true);
        }

        /// <summary>
        /// Display the wizard dialog to autodiscover feeds.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdAutoDiscoverFeed(ICommand sender)
        {
            if (SearchForFeeds(null))
            {
                //success
            }
            return;
            /*			
                        AutoDiscoverFeedsDialog autoDiscoverFeedsDialog = new AutoDiscoverFeedsDialog(); 
                        autoDiscoverFeedsDialog.WebpageUrl = guiMain.UrlText;
                        autoDiscoverFeedsDialog.ShowDialog(guiMain);

                        if(autoDiscoverFeedsDialog.DialogResult == DialogResult.OK) {														

                            if(!autoDiscoverFeedsDialog.IsKeywordSearch && string.IsNullOrEmpty(autoDiscoverFeedsDialog.WebpageUrl)) {
                                this.MessageError("RES_GUIFieldWebUrlInvalid");
                                autoDiscoverFeedsDialog.Close(); 
                                return; 
                            }

                            if(autoDiscoverFeedsDialog.IsKeywordSearch && string.IsNullOrEmpty(autoDiscoverFeedsDialog.Keywords)) {
                                this.MessageError("RES_GUIFieldKeywordsInvalid");
                                autoDiscoverFeedsDialog.Close(); 
                                return; 
                            }

                            try { 

                                AutoDiscoverFeedsThreadHandler autoDiscover = new AutoDiscoverFeedsThreadHandler();
                                autoDiscover.Proxy = this.Proxy;
                                if (autoDiscoverFeedsDialog.IsKeywordSearch) {
                                    autoDiscover.SearchTerms = autoDiscoverFeedsDialog.Keywords;
                                    autoDiscover.LocationMethod = FeedLocationMethod.Syndic8Search;
                                    autoDiscover.OperationMessage = Resource.Manager.FormatMessage("RES_GUIStatusWaitMessageDetectingFeedsWithKeywords", autoDiscoverFeedsDialog.Keywords);
                                } else {
                                    autoDiscover.WebPageUrl = autoDiscoverFeedsDialog.WebpageUrl;
                                    autoDiscover.LocationMethod = FeedLocationMethod.AutoDiscoverUrl;
                                    autoDiscover.OperationMessage = Resource.Manager.FormatMessage("RES_GUIStatusWaitMessageDetectingFeeds", autoDiscoverFeedsDialog.WebpageUrl);
                                }
					
                                if (DialogResult.OK != autoDiscover.Start( guiMain ))
                                    return;	// cancelled
                    
                                if (!autoDiscover.OperationSucceeds)
                                    return;

                                Hashtable feedUrls = autoDiscover.DiscoveredFeeds;
                    
                                if(feedUrls.Count == 0) {
                                    this.MessageInfo("RES_GUIStatusInfoMessageNoFeedsFound");
                                    return; 
                                }

                                DiscoveredFeedsDialog discoveredFeedsDialog = new DiscoveredFeedsDialog(feedUrls); 
                                discoveredFeedsDialog.ShowDialog(guiMain);

                                if(discoveredFeedsDialog.DialogResult == DialogResult.OK) {
                                    foreach( ListViewItem feedItem in discoveredFeedsDialog.listFeeds.SelectedItems ) {
                                        this.CmdNewFeed(defaultCategory, (string)feedItem.Tag, feedItem.SubItems[0].Text); 
                                    }
                                }

                            }
                            catch(Exception e) {
                                _log.Error("AutoDiscoverFeed exception.", e);
                                this.MessageError("RES_ExceptionGeneral", e.Message);
                            }
                        }		
            */
        }

        /// <summary>
        /// Pops up the NewCategoryDialog class and adds a new category to the list 
        /// of subscribed feeds.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdNewCategory(ICommand sender)
        {
            guiMain.NewCategory();
            if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }

        /// <summary>
        /// Delete all Feeds subscribed from the feed source.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdDeleteAll(ICommand sender)
        {
            TreeFeedsNodeBase root = guiMain.CurrentSelectedFeedsNode;
            if (root != null && !(root is SubscriptionRootNode))
                root = root.RootNode as TreeFeedsNodeBase;

            if (root != null && MessageQuestion(SR.MessageBoxDeleteAllFeedsQuestion) == DialogResult.Yes)
            {
                FeedSourceEntry entry = guiMain.FeedSourceEntryOf(root);
                if (entry != null)
                {
                    entry.Source.DeleteAllFeedsAndCategories(true);
                    FeedSource.SearchHandler.IndexRemoveAll();
                    SubscriptionModified(entry, NewsFeedProperty.General);
                    //this.FeedlistModified = true;
                    guiMain.InitiatePopulateTreeFeeds();
                }
            }
            if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdUpdateCategory(ICommand sender)
        {
            guiMain.UpdateCategory(true);
            if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdRenameCategory(ICommand sender)
        {
            guiMain.InitiateRenameFeedOrCategory();
            // way too early to call this: treeview stays in edit mode
            //this.FeedlistModified = true;

            //We need to know which node is being edited in PreFilterMessage so we 
            //don't set it to null. 
            /* if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null; */
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdDeleteCategory(ICommand sender)
        {
            if (guiMain.NodeEditingActive)
                return;
            // right-click selected:
            TreeFeedsNodeBase tn = guiMain.CurrentSelectedFeedsNode;
            if (tn == null) return;
            if (tn.Type != FeedNodeType.Category) return;

            if (DialogResult.Yes == MessageQuestion(
                                        SR.MessageBoxDeleteAllFeedsInCategoryQuestion,
                                        String.Format(" - {0} ({1})", SR.MenuDeleteCategoryCaption,
                                                      guiMain.CurrentSelectedFeedsNode.Text)))
            {
                // walks down the hierarchy and delete each child feed,
                // removes the node:
                guiMain.DeleteCategory(tn);
                SubscriptionModified(guiMain.FeedSourceEntryOf(tn), NewsFeedProperty.FeedCategoryRemoved);
                //this.FeedlistModified = true;
            }
            if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdUpdateFeed(ICommand sender)
        {
            string feedUrl = guiMain.CurrentSelectedFeedsNode.DataKey;
            FeedSource source = guiMain.FeedSourceOf(guiMain.CurrentSelectedFeedsNode); 

            if (!string.IsNullOrEmpty(feedUrl))
            {
                source.AsyncGetItemsForFeed(feedUrl, true, true);
            }
            if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdRenameFeed(ICommand sender)
        {
            guiMain.InitiateRenameFeedOrCategory();
            // way too early to call this, treeview stays in edit mode:
            //this.FeedlistModified = true;

            //We need to know which node is being edited in PreFilterMessage so we 
            //don't set it to null. 
            /* if (sender is AppContextMenuCommand)
				guiMain.CurrentSelectedFeedsNode = null; */
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdViewSourceOfFeed(ICommand sender)
        {
        	TreeFeedsNodeBase current = guiMain.CurrentSelectedFeedsNode;
			if (current != null && current.DataKey != null)
            {
				string feedUrl = current.DataKey;
				string title = String.Format(SR.TabFeedSourceCaption, current.Text);
            	FeedSourceEntry entry = guiMain.FeedSourceEntryOf(current);
                using (
                    FeedFileContentSourceDialog dialog =
						new FeedFileContentSourceDialog(Proxy, entry.Source.GetFeedCredentials(feedUrl), feedUrl, title))
                {
                    dialog.ShowDialog(guiMain);
                }
            }

            if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }

        /// <summary>
        /// Overloaded. Validates a feed link.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdValidateFeed(ICommand sender)
        {
            CmdValidateFeed(guiMain.CurrentSelectedFeedsNode.DataKey);
            if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }

        /// <summary>
        /// Overloaded. Validates a feed link.
        /// </summary>
        /// <param name="feedLink">Feed link</param>
        public void CmdValidateFeed(string feedLink)
        {
            if (!string.IsNullOrEmpty(feedLink))
            {
				NavigateToUrlAsUserPreferred(Resource.OutgoingLinks.FeedValidationUrlBase + Uri.EscapeUriString(feedLink),
                                             SR.TabValidationResultCaption, true, true);
            }
        }

        /// <summary>
        /// Overloaded. Navigates to feed home page (feed link).
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdNavigateFeedHome(ICommand sender)
        {
        	TreeFeedsNodeBase current = guiMain.CurrentSelectedFeedsNode;
			if (current != null)
			{
				NavigateToFeedHome(guiMain.FeedSourceEntryOf(current), current.DataKey);
			}
        	if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }

		/// <summary>
		/// Overloaded. Navigates to feed home page (feed link).
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="feedLink">Feed link</param>
        public void NavigateToFeedHome(FeedSourceEntry entry, string feedLink)
        {
            if (entry != null && !string.IsNullOrEmpty(feedLink))
            {
                IFeedDetails feedInfo = entry.Source.GetFeedDetails(feedLink);

                if (feedInfo != null)
                {
                    NavigateToUrlAsUserPreferred(feedInfo.Link, String.Format(SR.TabFeedHomeCaption,feedInfo.Title), true, true);
                }
            }
        }

        /// <summary>
        /// Overloaded. Display technorati link cosmos of the feed.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdNavigateFeedLinkCosmos(ICommand sender)
        {
			TreeFeedsNodeBase current = guiMain.CurrentSelectedFeedsNode;
			if (current != null)
			{
				NavigateToFeedLinkCosmos(guiMain.FeedSourceEntryOf(current), current.DataKey);
			}
        	if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }

		/// <summary>
		/// Overloaded. Display technorati link cosmos of the feed.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="feedLink">Feed link</param>
		public void NavigateToFeedLinkCosmos(FeedSourceEntry entry, string feedLink)
        {
            if (entry != null && !string.IsNullOrEmpty(feedLink))
            {
                IFeedDetails feedInfo = entry.Source.GetFeedDetails(feedLink);
                if (feedInfo != null)
                {
					NavigateToUrlAsUserPreferred(Resource.OutgoingLinks.FeedLinkCosmosUrlBase + Uri.EscapeUriString(feedInfo.Link),
                                                 String.Format(SR.TabLinkCosmosCaption,feedInfo.Title), true, true);
                }
            }
        }

        /// <summary>
        /// Uses SaveFileDialog to save the feed file either as a file conforming 
        ///	to feeds.xsd or an OPML file in the format used by Radio Userland, AmphetaDesk, 
        ///	and other news aggregators. 
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdExportFeeds(ICommand sender)
        {
            var sourcesNnodes = new Dictionary<FeedSourceEntry, TreeFeedsNodeBase>();

            foreach (FeedSourceEntry entry in sourceManager.GetOrderedFeedSources())
            {
                sourcesNnodes.Add(entry, guiMain.GetSubscriptionRootNode(entry)); 
            }
            
            ExportFeedsDialog dialog =
                new ExportFeedsDialog(sourcesNnodes, guiMain.CurrentSelectedFeedSource,  Preferences.NormalFont,
                                      guiMain.TreeImageList);
            if (DialogResult.OK == dialog.ShowDialog(guiMain))
            {
                SaveFileDialog sfd = new SaveFileDialog();

                FeedSourceEntry entry = sourceManager[dialog.FeedSource]; 
                ArrayList selections = dialog.GetSelectedFeedUrls();
                IDictionary<string, INewsFeed> fc = new SortedDictionary<string, INewsFeed>();
                foreach (string url in selections)
                {
                    if (entry.Source.IsSubscribed(url))
                        fc.Add(url, entry.Source.GetFeeds()[url]);
                }

                if (fc.Count == 0)
                    fc = entry.Source.GetFeeds();

                bool includeEmptyCategories = false;
                FeedListFormat format = FeedListFormat.OPML;

                String.Format("{0} (*.*)|*.*", SR.FileDialogFilterAllFiles);

                if (dialog.radioFormatOPML.Checked)
                {
                    format = FeedListFormat.OPML;
                    sfd.Filter =
                        String.Format("{0} (*.opml)|*.opml|{1} (*.*)|*.*", SR.FileDialogFilterOPMLFiles,
                                      SR.FileDialogFilterAllFiles);
                    includeEmptyCategories = dialog.checkFormatOPMLIncludeCats.Checked;
                }
                else if (dialog.radioFormatNative.Checked)
                {
                    format = FeedListFormat.NewsHandler;
                    sfd.Filter =
                        String.Format("{0} (*.xml)|*.xml|{1} (*.*)|*.*", SR.FileDialogFilterXMLFiles,
                                      SR.FileDialogFilterAllFiles);
                    if (!dialog.checkFormatNativeFull.Checked)
                        format = FeedListFormat.NewsHandlerLite;
                }

                sfd.FilterIndex = 1;
                sfd.RestoreDirectory = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
						using (Stream myStream = sfd.OpenFile())
                        {
                            entry.Source.SaveFeedList(myStream, format, fc, includeEmptyCategories);
                            myStream.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageError(String.Format(SR.ExceptionSaveFileMessage,sfd.FileName, ex.Message));
                    }
                }
            }

            dialog.Dispose();
        }

        /// <summary>
        /// Loads a feed list using the open file dialog. Either feed lists conforming 
        /// to the feeds.xsd schema or OPML files in the format used by Radio UserLand, 
        /// AmphetaDesk and other news aggregators. 
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdImportFeeds(ICommand sender)
        {
            string category = String.Empty;
            string feedSource = String.Empty;

            TreeFeedsNodeBase n = guiMain.CurrentSelectedFeedsNode;
            if (n != null)
            {
                if (n.Type == FeedNodeType.Category || n.Type == FeedNodeType.Feed)
                {
                    category = n.CategoryStoreName;
                }

                if (n.Type == FeedNodeType.Feed)
                {
                	SubscriptionRootNode root = TreeHelper.ParentRootNode(n) as SubscriptionRootNode;
                    if (root != null)
                    {
                    	FeedSourceEntry fs = this.FeedSources[root.SourceID];
						feedSource = fs.Name;
                    }
                }
            }
            ImportFeeds(String.Empty, category, feedSource);
        }

        /// <summary>
        /// Adds a new feed source
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdSynchronizeFeeds(ICommand sender)
        {
            SynchronizeFeeds(); 
        }

        /// <summary>
        /// Sends the feed list to the location configured on the 
        /// Remote Storage tab of the Options dialog.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdUploadFeeds(ICommand sender)
        {
            if (!Preferences.UseRemoteStorage)
            {
                MessageInfo(SR.RemoteStorageFeature_Info);
                return;
            }

            // Make sure this is what the user wants to do
            if (MessageQuestion(SR.RemoteStorageUpload_Question) == DialogResult.No)
            {
                return;
            }

			string errorMessage;
			if (TryUploadFeedlistAndState(false, out errorMessage))
			{
				MessageError(errorMessage);
			}
        }

        /// <summary>
        /// Loads the feed list from the location configured on the 
        /// Remote Storage tab of the Options dialog.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdDownloadFeeds(ICommand sender)
        {
            if (!Preferences.UseRemoteStorage)
            {
                MessageInfo(SR.RemoteStorageFeature_Info);
                return;
            }

            if (MessageQuestion(SR.RemoteStorageDownload_Question) == DialogResult.No)
            {
                return;
            }

        	string errorMessage;
			if (!TryDownloadFeedlistAndState(false, out errorMessage))
            {
                MessageError(errorMessage);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdShowOptions(ICommand sender)
        {
            ShowOptions(OptionDialogSection.Default, guiMain, null);

            if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdShowFeedProperties(ICommand sender)
        {
            if (guiMain.CurrentSelectedFeedsNode != null && guiMain.CurrentSelectedFeedsNode.DataKey != null)
            {
                TreeFeedsNodeBase tn = guiMain.CurrentSelectedFeedsNode;
                FeedSourceEntry entry = guiMain.FeedSourceEntryOf(tn); 

                INewsFeed f;
				int refreshrate = Preferences.RefreshRate;
                TimeSpan feedMaxItemAge = TimeSpan.Zero;
                bool feedDisabled = false;
                bool feedMarkItemsReadOnExit = false;

                try
                {
                    f = entry.Source.GetFeeds()[tn.DataKey];
                    //refreshrate = (f.refreshrateSpecified ? f.refreshrate : this.refreshRate); 							
                    try
                    {
                        refreshrate = entry.Source.GetRefreshRate(f.link);
                        feedMaxItemAge = entry.Source.GetMaxItemAge(f.link);
                        feedMarkItemsReadOnExit = entry.Source.GetMarkItemsReadOnExit(f.link);
                    }
                    catch
                    {
                        /* ignore this */
                    }
                }
                catch (Exception e)
                {
                    MessageError(String.Format(SR.GUIStatusErrorGeneralFeedMessage,tn.DataKey, e.Message));
                    _log.Error(String.Format(SR.GUIStatusErrorGeneralFeedMessage, tn.DataKey, e.Message), e); 
                    return;
                }

                FeedProperties propertiesDialog =
                    new FeedProperties(f.title, f.link, refreshrate/MilliSecsMultiplier, feedMaxItemAge,
                                       (f.category ?? defaultCategory), defaultCategory,
                                       entry.Source.GetCategories().Keys, entry.Source.GetStyleSheet(f.link));
                propertiesDialog.comboMaxItemAge.Enabled = !feedMaxItemAge.Equals(TimeSpan.Zero);
                propertiesDialog.checkEnableAlerts.Checked = f.alertEnabled;
                propertiesDialog.checkMarkItemsReadOnExit.Checked = feedMarkItemsReadOnExit;
                propertiesDialog.checkDownloadEnclosures.Checked = entry.Source.GetDownloadEnclosures(f.link);
                propertiesDialog.checkEnableEnclosureAlerts.Checked = entry.Source.GetEnclosureAlert(f.link);

                if (f.authUser != null)
                {
                    // NewsFeed has credentials
                    string u = null, p = null;
                    FeedSource.GetFeedCredentials(f, ref u, ref p);
                    propertiesDialog.textUser.Text = u;
                    propertiesDialog.textPwd.Text = p;
                }

				if (!String.IsNullOrEmpty(f.certificateId))
				{
					propertiesDialog.ClientCertificate = entry.Source.GetClientCertificate(f);
				}

                propertiesDialog.ShowDialog(guiMain);

                if (propertiesDialog.DialogResult == DialogResult.OK)
                {
                    NewsFeedProperty changes = NewsFeedProperty.None;
                    bool refreshThisFeed = false;

                    if ((propertiesDialog.textBox1.Text == null) ||
                        (propertiesDialog.textBox2.Text == null) ||
                        propertiesDialog.textBox1.Text.Trim().Equals(String.Empty) ||
                        propertiesDialog.textBox2.Text.Trim().Equals(String.Empty))
                    {
                        MessageError(SR.GUIFieldLinkTitleInvalid);
                    }
                    else
                    {
                        if (!f.link.Equals(propertiesDialog.textBox2.Text.Trim()))
                        {
                            // link was changed	
                            changes |= NewsFeedProperty.FeedLink;

                            string newLink = propertiesDialog.textBox2.Text.Trim();
                            //handle the common case of feed URI not beginning with HTTP 
                            try
                            {
                                Uri reqUri = new Uri(newLink);
                                newLink = reqUri.CanonicalizedUri();
                            }
                            catch (UriFormatException)
                            {
                                if (!newLink.ToLower().StartsWith("http://"))
                                {
                                    newLink = "http://" + newLink;
                                    Uri reqUri = new Uri(newLink);
                                    newLink = reqUri.CanonicalizedUri();
                                }
                            }

                            f = entry.Source.ChangeFeedUrl(f, newLink); 
                            tn.DataKey = f.link;

                            refreshThisFeed = true;
                        }

                        if (!f.title.Equals(propertiesDialog.textBox1.Text.Trim()))
                        {
                            f.title = propertiesDialog.textBox1.Text.Trim();
                            changes |= NewsFeedProperty.FeedTitle;
                            tn.Text = f.title;
                        }
                    }

                    try
                    {
                        if ((!string.IsNullOrEmpty(propertiesDialog.comboBox1.Text.Trim())))
                        {
                            Int32 intIn = Int32.Parse(propertiesDialog.comboBox1.Text.Trim());
                            changes |= NewsFeedProperty.FeedRefreshRate;
                            if (intIn <= 0)
                            {
                                DisableFeed(f, tn);
                                feedDisabled = true;
                            }
                            else
                            {
                                intIn = intIn*MilliSecsMultiplier;
                                entry.Source.SetRefreshRate(f.link, intIn);
                                /*
								f.refreshrate = intIn;
								f.refreshrateSpecified = (this.refreshRate != intIn);// default refresh rate?
								*/
                            }
                        }
                    }
                    catch (FormatException)
                    {
                        MessageError(SR.FormatExceptionRefreshRate);
                    }
                    catch (OverflowException)
                    {
                        MessageError(SR.OverflowExceptionRefreshRate);
                    }

                    string category = null;

                    if ((propertiesDialog.comboBox2.Text != null) &&
                        (!propertiesDialog.comboBox2.Text.Equals(String.Empty)) &&
                        (!propertiesDialog.comboBox2.Text.Equals(defaultCategory)))
                    {
                        category = propertiesDialog.comboBox2.Text.Trim();
                    }

                    if (!String.Equals(category, f.category))
                    {                       
                        changes |= NewsFeedProperty.FeedCategory;
                        if (category != null && !entry.Source.HasCategory(category))
                        {
                            entry.Source.AddCategory(category);
                        }
                        INewsFeedCategory targetCategory = category == null ? null : entry.Source.GetCategories()[category];
                        entry.Source.ChangeCategory(f, targetCategory);
                        // find/create the target node:
                        TreeFeedsNodeBase target =
                            guiMain.CreateSubscriptionsCategoryHive(guiMain.GetRoot(RootFolderType.MyFeeds), category);
                        // move to new location:
                        guiMain.MoveNode(tn, target, true);
                    }

                    if (propertiesDialog.comboMaxItemAge.Enabled)
                    {
                        if (feedMaxItemAge.CompareTo(propertiesDialog.MaxItemAge) != 0)
                        {
                            refreshThisFeed = true;
                            entry.Source.SetMaxItemAge(f.link, propertiesDialog.MaxItemAge);
                            changes |= NewsFeedProperty.FeedMaxItemAge;
                        }
                    }

                    if (propertiesDialog.textUser.Text != null && propertiesDialog.textUser.Text.Trim().Length != 0)
                    {
                        // set NewsFeed new credentials
                        string u = propertiesDialog.textUser.Text.Trim(), p = null;
                        if (!string.IsNullOrEmpty(propertiesDialog.textPwd.Text))
                            p = propertiesDialog.textPwd.Text.Trim();
                        FeedSource.SetFeedCredentials(f, u, p);
                        changes |= NewsFeedProperty.FeedCredentials;
                        refreshThisFeed = true;
                    }
                    else
                    {
                        FeedSource.SetFeedCredentials(f, null, null);
                        changes |= NewsFeedProperty.FeedCredentials;
                    }

					entry.Source.SetClientCertificate(f, propertiesDialog.ClientCertificate);

                    if (f.alertEnabled != propertiesDialog.checkEnableAlerts.Checked)
                        changes |= NewsFeedProperty.FeedAlertOnNewItemsReceived;
                    f.alertEnabledSpecified = f.alertEnabled = propertiesDialog.checkEnableAlerts.Checked;


                    if (propertiesDialog.checkMarkItemsReadOnExit.Checked != entry.Source.GetMarkItemsReadOnExit(f.link))
                    {
                        entry.Source.SetMarkItemsReadOnExit(f.link,
                                                           propertiesDialog.checkMarkItemsReadOnExit.Checked);
                        changes |= NewsFeedProperty.FeedMarkItemsReadOnExit;
                    }

                    if (refreshThisFeed && !feedDisabled)
                    {
                        entry.Source.MarkForDownload(f);
                    }

                    if (entry.Source.GetDownloadEnclosures(f.link) != propertiesDialog.checkDownloadEnclosures.Checked)
                    {
                        entry.Source.SetDownloadEnclosures(f.link, propertiesDialog.checkDownloadEnclosures.Checked);
                    }

                    if (entry.Source.GetEnclosureAlert(f.link) != propertiesDialog.checkEnableEnclosureAlerts.Checked)
                    {
                        entry.Source.SetEnclosureAlert(f.link, propertiesDialog.checkEnableEnclosureAlerts.Checked);
                    }


                    if (propertiesDialog.checkCustomFormatter.Checked)
                    {
                        string stylesheet = propertiesDialog.comboFormatters.Text;

                        if (!stylesheet.Equals(entry.Source.GetStyleSheet(f.link)))
                        {
                            entry.Source.SetStyleSheet(f.link, stylesheet);
                            changes |= NewsFeedProperty.FeedStylesheet;

                            if (!NewsItemFormatter.ContainsXslStyleSheet(stylesheet))
                            {
                                NewsItemFormatter.AddXslStyleSheet(stylesheet,
                                                                   GetNewsItemFormatterTemplate(stylesheet));
                            }
                        }
                    }
                    else
                    {
                        if (!String.Empty.Equals(entry.Source.GetStyleSheet(f.link)))
                        {
                            entry.Source.SetStyleSheet(f.link, String.Empty);
                            changes |= NewsFeedProperty.FeedStylesheet;
                        }
                    }

                    guiMain.SetSubscriptionNodeState(f, tn, FeedProcessingState.Normal);
                    FeedWasModified(f, changes);
                    //this.FeedlistModified = true;
                }


                //cleanup 
                propertiesDialog.Dispose();
            }
            if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }


        /// <summary>Displays the properties dialog for a category </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdShowCategoryProperties(ICommand sender)
        {
            if (guiMain.CurrentSelectedFeedsNode != null &&
                (guiMain.CurrentSelectedFeedsNode.Type == FeedNodeType.Category))
            {
                TreeFeedsNodeBase tn = guiMain.CurrentSelectedFeedsNode;
                FeedSourceEntry entry = guiMain.FeedSourceEntryOf(tn); 

                string category = null, catPlusSep, categoryName;
                int refreshrate = Preferences.RefreshRate;
                TimeSpan feedMaxItemAge = TimeSpan.Zero;
                bool feedMarkItemsReadOnExit = false;

                try
                {
                    category = tn.CategoryStoreName;
                    catPlusSep = category + FeedSource.CategorySeparator;
                    categoryName = tn.Text;

                    try
                    {
                        refreshrate = entry.Source.GetCategoryRefreshRate(category);
                        feedMaxItemAge = entry.Source.GetCategoryMaxItemAge(category);
                        feedMarkItemsReadOnExit = entry.Source.GetCategoryMarkItemsReadOnExit(category);
                    }
                    catch
                    {
                        /* ignore this */
                    }
                }
                catch (Exception e)
                {
                    MessageError(String.Format(SR.GUIStatusErrorGeneralFeedMessage,category, e.Message));
                    return;
                }

                CategoryProperties propertiesDialog =
                    new CategoryProperties(tn.Text, refreshrate/MilliSecsMultiplier, feedMaxItemAge,
                                           entry.Source.GetCategoryStyleSheet(category));
                propertiesDialog.comboMaxItemAge.Enabled = !feedMaxItemAge.Equals(TimeSpan.Zero);
                propertiesDialog.checkMarkItemsReadOnExit.Checked = feedMarkItemsReadOnExit;
                propertiesDialog.checkDownloadEnclosures.Checked =
                    entry.Source.GetCategoryDownloadEnclosures(category);
                propertiesDialog.checkEnableEnclosureAlerts.Checked =
                    entry.Source.GetCategoryEnclosureAlert(category);


                propertiesDialog.ShowDialog(guiMain);

                if (propertiesDialog.DialogResult == DialogResult.OK)
                {
                    NewsFeedProperty changes = NewsFeedProperty.General;

                    if ((propertiesDialog.textBox2.Text == null) ||
                        propertiesDialog.textBox2.Text.Trim().Equals(String.Empty))
                    {
                        MessageError(SR.GUIFieldTitleInvalid);
                    }
                    else
                    {
                        if (!categoryName.Equals(propertiesDialog.textBox2.Text.Trim()))
                        {
                            //string oldCategory = category;
                            categoryName = propertiesDialog.textBox2.Text.Trim();
                            // this call yet cause a FeedModified() notification:
                            guiMain.RenameTreeNode(tn, categoryName);
                            entry.Source.RenameCategory(category, tn.CategoryStoreName);
                            /* 
                            category c = feedHandler.Categories.GetByKey(category);
                            feedHandler.Categories.Remove(category);
                            category = tn.CategoryStoreName;
                            feedHandler.Categories.Add(category, c);
                             */ 
                        }
                    }

                    try
                    {
                        if ((!string.IsNullOrEmpty(propertiesDialog.comboBox1.Text.Trim())))
                        {
                            Int32 intIn = Int32.Parse(propertiesDialog.comboBox1.Text.Trim());
                            if (intIn <= 0)
                            {
                                foreach (NewsFeed f in entry.Source.GetFeeds().Values)
                                {
                                    if ((f.category != null) &&
                                        (f.category.Equals(category) || f.category.StartsWith(catPlusSep)))
                                    {
                                        f.refreshrateSpecified = false;
                                        DisableFeed(f.link, entry);
                                    }
                                }
                                entry.Source.SetCategoryRefreshRate(category, 0);
                            }
                            else
                            {
                                foreach (NewsFeed f in entry.Source.GetFeeds().Values)
                                {
                                    if ((f.category != null) &&
                                        (f.category.Equals(category) || f.category.StartsWith(catPlusSep)))
                                    {
                                        f.refreshrateSpecified = false;
                                        guiMain.SetSubscriptionNodeState(f, TreeHelper.FindNode(tn, f),
                                                                         FeedProcessingState.Normal);
                                    }
                                }

                                intIn = intIn*MilliSecsMultiplier;
                                entry.Source.SetCategoryRefreshRate(category, intIn);
                            }
                        }
                    }
                    catch (FormatException)
                    {
                        MessageError(SR.FormatExceptionRefreshRate);
                    }
                    catch (OverflowException)
                    {
                        MessageError(SR.OverflowExceptionRefreshRate);
                    }


                    //TODO: Merge this loop with the one for refresh rate 
                    if (propertiesDialog.comboMaxItemAge.Enabled)
                    {
                        if (feedMaxItemAge.CompareTo(propertiesDialog.MaxItemAge) != 0)
                        {
                            foreach (NewsFeed f in entry.Source.GetFeeds().Values)
                            {
                                if ((f.category != null) &&
                                    (f.category.Equals(category) || f.category.StartsWith(catPlusSep)))
                                {
                                    f.maxitemage = null;
                                }
                            }

                            entry.Source.SetCategoryMaxItemAge(category, propertiesDialog.MaxItemAge);
                            changes |= NewsFeedProperty.General;
                            //this.FeedWasModified(f.link);
                        }
                    }

                    entry.Source.SetCategoryMarkItemsReadOnExit(category,
                                                               propertiesDialog.checkMarkItemsReadOnExit.Checked);
                    entry.Source.SetCategoryDownloadEnclosures(category,
                                                              propertiesDialog.checkDownloadEnclosures.Checked);
                    entry.Source.SetCategoryEnclosureAlert(category,
                                                          propertiesDialog.checkEnableEnclosureAlerts.Checked);

                    if (propertiesDialog.checkCustomFormatter.Checked)
                    {
                        string stylesheet = propertiesDialog.comboFormatters.Text;
                        entry.Source.SetCategoryStyleSheet(category, stylesheet);

                        if (!NewsItemFormatter.ContainsXslStyleSheet(stylesheet))
                        {
                            NewsItemFormatter.AddXslStyleSheet(stylesheet,
                                                               GetNewsItemFormatterTemplate(stylesheet));
                        }
                    }
                    else
                    {
                        entry.Source.SetCategoryStyleSheet(category, String.Empty);
                    }

                    SubscriptionModified(entry, changes);
                    //this.FeedlistModified = true; 
                }

                //cleanup
                propertiesDialog.Dispose();
            }
            if (sender is AppContextMenuCommand)
                guiMain.CurrentSelectedFeedsNode = null;
        }

        /// <summary>
        /// Listview context menu command
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdMarkFeedItemsUnread(ICommand sender)
        {
            guiMain.MarkSelectedItemsLVUnread();
        }

        /// <summary>
        /// Listview context menu command
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdMarkFeedItemsRead(ICommand sender)
        {
            guiMain.MarkSelectedItemsLVRead();
        }


        /// <summary>
        /// Opens the reply post window to allow a user to
        /// answer to an post (send a comment to a feed item) 
        /// or reply to NNTP group post.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdPostReplyToItem(ICommand sender)
        {
            INewsItem item2reply = guiMain.CurrentSelectedFeedItem;

            if (item2reply == null)
            {
                MessageInfo(SR.GuiStateNoFeedItemSelectedMessage);
                return;
            }

            if ((postReplyForm == null) || (postReplyForm.IsDisposed))
            {
                postReplyForm = new PostReplyForm(Preferences.UserIdentityForComments, IdentityManager);
                postReplyForm.PostReply += OnPostReplyFormPostReply;
            }

            postReplyForm.ReplyToItem = item2reply;

            postReplyForm.Show(); // open non-modal
            Win32.SetForegroundWindow(postReplyForm.Handle);
        }

        /// <summary>
        /// Opens the new post window to allow a user to
        /// create a new post to send to a NNTP group.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdPostNewItem(ICommand sender)
        {
            TreeFeedsNodeBase tn = guiMain.CurrentSelectedFeedsNode;            
            if (tn == null || tn.Type != FeedNodeType.Feed)
            {
                Mediator.SetEnabled("-cmdFeedItemNewPost");
                return;
            }

            string feedUrl = tn.DataKey;
            FeedSource source = guiMain.FeedSourceOf(tn); 

            if (feedUrl == null ||
                !RssHelper.IsNntpUrl(feedUrl) ||
                !source.IsSubscribed(feedUrl))
            {
                Mediator.SetEnabled("-cmdFeedItemNewPost");
                return;
            }

            if ((postReplyForm == null) || (postReplyForm.IsDisposed))
            {
                postReplyForm = new PostReplyForm(Preferences.UserIdentityForComments, IdentityManager);
                postReplyForm.PostReply += OnPostReplyFormPostReply;
            }

            INewsFeed f;

            if (source.GetFeeds().TryGetValue(feedUrl, out f)) {
                postReplyForm.PostToFeed = f;

                postReplyForm.Show(); // open non-modal
                Win32.SetForegroundWindow(postReplyForm.Handle);
            }
        }

        public void CmdBrowserGoBack(ICommand sender)
        {
            guiMain.RequestBrowseAction(BrowseAction.NavigateBack);
        }

        public void CmdBrowserGoForward(ICommand sender)
        {
            guiMain.RequestBrowseAction(BrowseAction.NavigateForward);
        }

        public void CmdBrowserCancelNavigation(ICommand sender)
        {
            guiMain.RequestBrowseAction(BrowseAction.NavigateCancel);
        }

        public void CmdBrowserNavigate(ICommand sender)
        {
            NavigateToUrl(guiMain.UrlText, "Web", (Control.ModifierKeys & Keys.Control) == Keys.Control, true);
        }

        public void CmdBrowserRefresh(ICommand sender)
        {
            guiMain.RequestBrowseAction(BrowseAction.DoRefresh);
        }

        public void CmdBrowserCreateNewTab(ICommand sender)
        {
            NavigateToUrl("about:blank", "New Browser", true, true);
        }


        /// <summary>
        /// Calling a generic listview context menu item command used 
        /// e.g. for plugin's supporting IBlogExtension
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdGenericListviewCommand(ICommand sender)
        {
            AppContextMenuCommand cmd = (AppContextMenuCommand) sender;
            string s = (string) cmd.Tag;
            guiMain.OnGenericListviewCommand(Int32.Parse(s.Substring(s.IndexOf(".") + 1)), false);
        }

        /// <summary>
        /// Calling a generic listview context menu config item command, 
        /// e.g. for plugin's supporting IBlogExtension.
        /// </summary>
        /// <param name="sender">Object that initiates the call</param>
        public void CmdGenericListviewCommandConfig(ICommand sender)
        {
            AppContextMenuCommand cmd = (AppContextMenuCommand) sender;
            string s = (string) cmd.Tag;
            guiMain.OnGenericListviewCommand(Int32.Parse(s.Substring(s.IndexOf(".") + 1)), true);
        }
    }
}