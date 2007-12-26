#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.ComponentModel;

using NewsComponents;
using NewsComponents.Feed;

namespace System.Windows.Forms.ThListView
{
	/// <summary>
	/// Summary description for ThreadCancelEventArgs.
	/// </summary>
	public class ThreadCancelEventArgs: CancelEventArgs
	{
		private ThreadedListViewItem _tlv = null;

		public ThreadCancelEventArgs(ThreadedListViewItem tlv, bool cancel): base(cancel){
			this._tlv = tlv;
		}
		public ThreadedListViewItem Item {
			get { return _tlv; }
		}
	}

	/// <summary>
	/// Summary description for ThreadEventArgs.
	/// </summary>
	public class ThreadEventArgs: EventArgs {
		private ThreadedListViewItem _tlv = null;
		private ThreadedListViewItem[] _childItems = null;

		public ThreadEventArgs(ThreadedListViewItem tlv): base() {
			this._tlv = tlv;
		}
		public ThreadedListViewItem Item {
			get { return _tlv; }
		}
		public ThreadedListViewItem[] ChildItems {
			get { return _childItems; }
			set { _childItems = value;}
		}
	}

	/// <summary>
	/// Summary description for ListLayoutEventArgs.
	/// </summary>
	public class ListLayoutEventArgs: EventArgs {
		private FeedColumnLayout _layout = null;

		public ListLayoutEventArgs(FeedColumnLayout layout): base(){
			this._layout = layout;
		}
		public FeedColumnLayout Layout {
			get { return _layout; }
		}
	}

	/// <summary>
	/// Summary description for ListLayoutCancelEventArgs.
	/// </summary>
	public class ListLayoutCancelEventArgs: CancelEventArgs {
		private FeedColumnLayout _layout = null;

		public ListLayoutCancelEventArgs(FeedColumnLayout newLayout, bool cancel): base(cancel){
			this._layout = newLayout;
		}
		public FeedColumnLayout NewLayout {
			get { return _layout; }
		}
	}
}
