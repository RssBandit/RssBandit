#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using RssBandit.WinGui.Utility;
using NewsComponents;

namespace RssBandit.WinGui.Interfaces
{
	/// <summary>
	/// Form elements that can send commands have to implement ICommand
	/// </summary>
	public interface ICommand
	{
		void Initialize();
		void Execute();
		string CommandID { get; }
		CommandMediator Mediator { get ; }
	}
	
	/// <summary>
	/// General GUI Command Abstraction (from Menubar, Toolbar, ...)
	/// </summary>
	public interface ICommandComponent
	{
		bool Checked { get ; set; }
		bool Enabled { get ; set; }
		bool Visible { get ; set; }
	}

	/// <summary>
	/// Delegate used to callback to mediator
	/// </summary>
	public delegate void ExecuteCommandHandler(ICommand sender);

	/// <summary>
	/// State of our tabbed views
	/// </summary>
	public interface ITabState
	{
		string Title      { get; set; }
		string Url		  { get; set; }
		bool CanClose     { get; set; }
		bool CanGoBack    { get; set; }
		bool CanGoForward { get; set; }
		ITextImageItem[] GoBackHistoryItems(int maxItems);
		ITextImageItem[] GoForwardHistoryItems(int maxItems);
	}

	public interface ITextImageItem {
		Image Image { get; }
		string Text { get; }
	}

	public interface INewsItemFilter	{
		bool Match(NewsItem item);
		void ApplyAction(NewsItem item, System.Windows.Forms.ThListView.ThreadedListViewItem lvItem);
	}
	
	public interface ISmartFolder {
		bool ContainsNewMessages		{ get ; }
		bool HasNewComments             { get ; }
		int NewMessagesCount			{ get ; }
		int NewCommentsCount            { get ; }
		void MarkItemRead				(NewsItem item);
		void MarkItemUnread				(NewsItem item);	
		List<NewsItem> Items					{ get ; }
		void Add						(NewsItem item);
		void Remove						(NewsItem item);
		void UpdateReadStatus 			();
		void UpdateCommentStatus        ();
		bool Modified					{ get ; set ; }
	}

	/// <summary>
	/// Defines the types of the nodes known within the treeview.
	/// </summary>
	public enum FeedNodeType
	{
		/// <summary>
		/// The MyFeeds root node, or Special Feeds.
		/// </summary>
		Root,
		/// <summary>
		/// A feed category node.
		/// </summary>
		Category,
		/// <summary>
		/// A real feed node.
		/// </summary>
		Feed,
		/// <summary>
		/// Smart container node, like Flagged Items, Errors or Sent Items.
		/// Contains copies of the originals.
		/// </summary>
		SmartFolder,
		/// <summary>
		/// Like a normal feed node, but contains different rss items aggregated
		/// on a certain criteria. Mean: holds references to rss items, that are also
		/// contained in the "real" feed. Example: Unread Items
		/// </summary>
		Finder,
		/// <summary>
		/// A search folder category node.
		/// </summary>
		FinderCategory,
	}

}
