using System;

namespace RssBandit.WinGui.Forms
{
	#region IdleTasks enum
	/// <summary>
	/// Used to delay execution of some UI tasks if the app is idle
	/// </summary>
	[Flags]internal enum IdleTasks {
		None = 0,
		InitOnFinishLoading = 1,
		StartRefreshOneFeed = 2,
		StartRefreshAllFeeds = 4,
		ShowFeedPropertiesDialog = 8,
		NavigateToFeedRssItem = 16, 
		AutoSubscribeFeedUrl = 32,
		IndexAllItems= 64,
	}
	#endregion

	/// <summary>
	/// Manage IdleTasks.
	/// </summary>
	internal sealed class IdleTask
	{
		private static IdleTasks _tasks = IdleTasks.None;
		
		public static IdleTasks Tasks { get { return _tasks; } }
		public static void AddTask(IdleTasks task) { _tasks |= task;	}
		public static void RemoveTask(IdleTasks task) { _tasks &= ~task;	}
		public static void Clear() { _tasks = IdleTasks.None; }
		public static bool IsTask(IdleTasks task) { return ((_tasks & task) == task); }
	}
}
