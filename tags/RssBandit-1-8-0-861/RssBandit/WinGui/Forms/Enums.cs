using System;

namespace RssBandit.WinGui.Forms
{
    /// <summary>
    /// Used to delay execution of some UI tasks by a timer
    /// </summary>
    [Flags]
    internal enum DelayedTasks
    {
        None = 0,
        NavigateToWebUrl = 1,
        StartRefreshOneFeed = 2,
        StartRefreshAllFeeds = 4,
        ShowFeedPropertiesDialog = 8,
        NavigateToFeedNewsItem = 16,
        AutoSubscribeFeedUrl = 32,
        ClearBrowserStatusInfo = 64,
        RefreshTreeUnreadStatus = 128,
        SyncRssSearchTree = 256,
        InitOnFinishLoading = 512,
        SaveUIConfiguration = 1024,
        NavigateToFeed = 2048,
        RefreshTreeCommentStatus = 4096,
    }

    internal enum NavigationPaneView
    {
        LastVisibleSubscription,
        RssSearch,
    }

    /// <summary>
    /// Enumeration that defines the possible embedded web browser actions
    /// to perform from the main application.
    /// </summary>
    internal enum BrowseAction
    {
        NavigateCancel,
        NavigateBack,
        NavigateForward,
        DoRefresh
    }

    /// <summary>
    /// Enumeration that defines the type of the known root folders
    /// of Bandit displayed within the treeview.
    /// </summary>
    internal enum RootFolderType
    {
        MyFeeds,
        SmartFolders,
        Finder
    }

    /// <summary>
    /// Defines the subscription tree node processing states
    /// </summary>
    internal enum FeedProcessingState
    {
        Normal,
        Updating,
        Failure,
    }
}