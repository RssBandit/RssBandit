#region CVS Version Header
/*
 * $Id: Interfaces.cs,v 1.21 2005/05/08 17:03:07 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/05/08 17:03:07 $
 * $Revision: 1.21 $
 */
#endregion

using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

using RssBandit.SpecialFeeds;
using RssBandit.WinGui.Forms;
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
	/// State of our tabbed view
	/// </summary>
	public interface ITabState
	{
		string Title      { get; set; }
		string Url				{ get; set; }
		bool CanClose     { get; set; }
		bool CanGoBack    { get; set; }
		bool CanGoForward { get; set; }
	}

	public interface INewsItemFilter	{
		bool Match(NewsItem item);
		void ApplyAction(NewsItem item, System.Windows.Forms.ThListView.ThreadedListViewItem lvItem);
	}
	
	public interface ISmartFolder {
		bool ContainsNewMessages		{ get ; }
		int NewMessagesCount				{ get ;}
		void MarkItemRead						(NewsItem item);
		void MarkItemUnread					(NewsItem item);
		ArrayList Items							{ get ; }
		void Add										(NewsItem item);
		void Remove									(NewsItem item);
		void UpdateReadStatus 			();
		bool Modified								{ get ; set ; }
	}

	/// <summary>
	/// Provides a base implementation of ISmartFolder.
	/// </summary>
	public class SmartFolderNodeBase: FeedTreeNodeBase, ISmartFolder {
		private ContextMenu _popup = null;
		protected LocalFeedsFeed itemsFeed;

		public SmartFolderNodeBase(LocalFeedsFeed itemStore, int imageIndex, int selectedImageIndex, ContextMenu menu):
			this(itemStore, itemStore.title, imageIndex, selectedImageIndex, menu) {
		}

		public SmartFolderNodeBase(LocalFeedsFeed itemStore, string text, int imageIndex, int selectedImageIndex, ContextMenu menu):
			base(text, FeedNodeType.SmartFolder, true, imageIndex, selectedImageIndex) {
			_popup = menu; 
			itemsFeed = itemStore;
		}

		#region Implementation of FeedTreeNodeBase
		public override bool AllowedChild(FeedNodeType nsType) {
			return false;
		}
		public override void PopupMenu(System.Drawing.Point screenPos) {
			/*
			  if (_popup != null)
				  _popup.TrackPopup(screenPos);
			  */
		}
		public override void UpdateContextMenu() {
			if (base.TreeView != null)
				base.TreeView.ContextMenu = _popup;
		}
		#endregion

		#region ISmartFolder Members

		public virtual bool ContainsNewMessages {
			get {
				foreach (NewsItem ri in itemsFeed.Items) {
					if (!ri.BeenRead) return true;
				}
				return false;
			}
		}

		public virtual int NewMessagesCount {
			get {
				int i = 0;
				foreach (NewsItem ri in itemsFeed.Items) {
					if (!ri.BeenRead) i++;
				}
				return i;
			}
		}

		public virtual void MarkItemRead(NewsItem item) {
			if (item == null) return;
			foreach (NewsItem ri in itemsFeed.Items) {
				if (item.Equals(ri)) {
					ri.BeenRead = true;
					base.UpdateReadStatus(this, -1);
					break;
				}
			}
		}

		public virtual void MarkItemUnread(NewsItem item) {
			if (item == null) return;
			foreach (NewsItem ri in itemsFeed.Items) {
				if (item.Equals(ri)) {
					ri.BeenRead = false;
					break;
				}
			}
		}

		public virtual ArrayList Items {
			get {	return itemsFeed.Items;	}
		}

		public virtual void Add(NewsItem item) {	
			itemsFeed.Add(item);
		}
		public virtual void Remove(NewsItem item) {
			itemsFeed.Remove(item);
		}

		public virtual void UpdateReadStatus() {
			base.UpdateReadStatus(this, this.NewMessagesCount);
		}

		public virtual bool Modified {
			get { return itemsFeed.Modified;  }
			set { itemsFeed.Modified = value; }
		}

		#endregion
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

	/// <summary>
	/// State of the tree view nodes
	/// </summary>
	public abstract class FeedTreeNodeBase: System.Windows.Forms.TreeNode
	{
		public event System.EventHandler ReadCounterZero;

		private FeedNodeType	_type;
		private bool					_editable, _anyUnread;
		private int					_unreadCount;
		private int					_highlightCount;
		private string				_key;

		protected FeedTreeNodeBase() {}	// not anymore allowed
		protected FeedTreeNodeBase(string text, FeedNodeType nodeType):this(text, nodeType, false){}
		protected FeedTreeNodeBase(string text, FeedNodeType nodeType, bool editable):this(text, nodeType, editable, -1, -1){}
		protected FeedTreeNodeBase(string text, FeedNodeType nodeType, bool editable, int imageIndex, int selectedImageIndex):base()
		{
			this.NodeFont = FontColorHelper.NormalFont;
			this.ForeColor = FontColorHelper.NormalColor;
			this.Key = text;
			_unreadCount = _highlightCount = 0;
			if (imageIndex >= 0)
				base.ImageIndex = imageIndex;
			if (selectedImageIndex >= 0)
				base.SelectedImageIndex = selectedImageIndex;
			_type = nodeType;
			_editable = editable;
			_anyUnread = false;
			this.Text = text;	// uses some of the above variables, so keep it at last...
		}

		//TODO: implement/override the ISerializable GetObjectData constructor (FxCop warning)

		public virtual FeedNodeType Type { get { return _type; } set { _type = value; } }
		public virtual bool Editable { get { return _editable; } set { _editable = value; } }

		public virtual new Color ForeColor { 
			get { return base.ForeColor; } 
			set { 
				if (base.ForeColor != value)
				  base.ForeColor = value; 
			} 
		}

		// override some methods so we do not always have to cast explicitly
		public virtual new FeedTreeNodeBase FirstNode	{
			get {	return (FeedTreeNodeBase)base.FirstNode; }
		}
		public virtual new FeedTreeNodeBase NextNode	{
			get {	return (FeedTreeNodeBase)base.NextNode; }
		}
		public virtual new FeedTreeNodeBase LastNode	{
			get {	return (FeedTreeNodeBase)base.LastNode; }
		}
		public virtual new FeedTreeNodeBase Parent	{
			get {	return (FeedTreeNodeBase)base.Parent; }
		}
		
		/// <summary>
		/// Now handles the whole visual node text formatting
		/// </summary>
		/// <value>Returns the <see cref="Key"/> of the node. 
		/// Sets the new displayed text and font of a node according to the
		/// unread items counter info.</value>
		public virtual new string Text
		{
			get { return this.Key; }
			set 
			{ 
				string sInfo = String.Empty;
				bool fontChanged = false;
				if (_highlightCount > 0) {
					if (this.NodeFont.Style != FontColorHelper.HighlightStyle) {
						this.NodeFont = FontColorHelper.HighlightFont;
						fontChanged = true;
					}
					this.ForeColor = FontColorHelper.HighlightColor;
				} else if (_unreadCount > 0 || _anyUnread) {
					if (this.NodeFont.Style != FontColorHelper.UnreadStyle) {
						this.NodeFont =  FontColorHelper.UnreadFont;
						fontChanged = true;
					}
					this.ForeColor = FontColorHelper.UnreadColor;
					if (_unreadCount > 0) {
						sInfo =  String.Concat(" (", _unreadCount.ToString() , ")");
					}
				} else {
					if (this.NodeFont.Style != FontColorHelper.NormalStyle) {
						this.NodeFont =  FontColorHelper.NormalFont;
						fontChanged = true;
					}
					this.ForeColor = FontColorHelper.NormalColor;
				}

				string s;
				if (value == null || value.Trim().Length == 0)
					s = Resource.Manager["RES_GeneralNewItemText"] + sInfo;
				else
					s = value + sInfo;

				try {
					if (fontChanged || s.CompareTo(base.Text) != 0)
						base.Text = s;
				} catch (Exception e) {
					System.Diagnostics.Trace.WriteLine("set base.Text failed with: " + e.Message);
				}
			}
		}

		/// <summary>
		/// Enables to set the editable node text before labelEdit.
		/// </summary>
		public virtual string EditableText
		{
			get { return this.Key;   }
			set { base.Text = value; }
		}
		
		public virtual string Key
		{
			get { return _key; }
			set 
			{ 
				if (value == null || value.Trim().Length == 0)
					_key = Resource.Manager["RES_GeneralNewItemText"];
				else
					_key = value.Trim();

				this.Text = _key;		// refresh visual info
			}
		}

		/// <summary>
		/// AnyUnread and UnreadCount are working interconnected:
		/// if you set AnyUnread to true, this will update the visualized info to
		/// use the Unread Font, but no read counter state info. UnreadCount is NOT
		/// modified anyway. Otherwise, if you set AnyUnread to false, it will 
		/// refresh the caption rendering to default.
		/// </summary>
		public virtual bool AnyUnread {
			get {	return (_anyUnread || _unreadCount > 0);  }
			set {	
				if (_anyUnread != value) { 
					_anyUnread = value; 
					if (_anyUnread) {
						this.Text = this.Key;		// refresh visual info
					} else if (_unreadCount == 0){ // _anyUnread == false
						this.Text = this.Key;		// refresh visual info
					}
				}
			}
		}

		/// <summary>
		/// AnyUnread and UnreadCount are working interconnected:
		/// if you set UnreadCount to non zero, AnyUnread will be set to true and
		/// then updates the visualized info to use the Unread Font and 
		/// read counter state info. If UnreadCount is set to zero,
		/// also AnyUnread is reset to false and refresh the caption to default.
		/// </summary>
		public virtual int UnreadCount
		{
			get {	return _unreadCount;  }
			set 
			{	
				if (value > 0)
				{
					_anyUnread = true;
					if (value != _unreadCount)
					{
						_unreadCount = value; 
						this.Text = this.Key;		// refresh visual info
					}
				}
				else
				{
					_anyUnread = false;
					_unreadCount = 0; 
					this.Text = this.Key;
				}

				if (_unreadCount == 0)
					RaiseReadCounterZero();
			}
		}

		private void RaiseReadCounterZero() {
			if (this.ReadCounterZero != null)
				this.ReadCounterZero(this, EventArgs.Empty);
		}


		public virtual int HighlightCount {
			get {	return _highlightCount;  }
			set {	
				if (value > 0) {
					if (value != _highlightCount) {
						_highlightCount = value; 
						this.Text = this.Key;		// refresh visual info
					}
				}
				else {
					_highlightCount = 0; 
					this.Text = this.Key;
				}
			}
		}

		public virtual new string FullPath
		{
			get 
			{
				FeedTreeNodeBase tn = this.Parent;
				string sep = this.TreeView.PathSeparator;
				StringBuilder sb = new StringBuilder(this.Key);
				while (tn != null)
				{
					sb.Insert(0, tn.Key + sep);
					tn = tn.Parent;
				}
				return sb.ToString(); 
			}
		}

		public void UpdateReadStatus(FeedTreeNodeBase thisNode, int readCounter) {
			if (thisNode == null) return;
			
			if (readCounter <= 0) {			// mark read

				// traverse tree: upwards, one by one. On every step
				// look for unread childs (go down). If there is one, stop walking up.
				// If not, mark it read (normal font state), go on upwards
				if (this.Equals(thisNode)) { 
					// this can happen only once. A Feed can only have category/root as parents
					
					if (thisNode.UnreadCount < Math.Abs(readCounter))
						readCounter = -thisNode.UnreadCount;

					if (readCounter == 0)
						readCounter = -thisNode.UnreadCount;

					thisNode.UnreadCount += readCounter;
					UpdateReadStatus(thisNode.Parent, readCounter);
					
				} 
				else { // Category/Root mark read

					thisNode.UnreadCount += readCounter;
					UpdateReadStatus(thisNode.Parent, readCounter);
				}
			
			} else {	// mark unread (readCounter > 0)

				// traverse tree: upwards, one by one. On each parent
				// mark it unread (bold font state), go on upwards
				if (thisNode.Nodes.Count == 0  /*thisNode.Type != FeedNodeType.Root && thisNode.Type != FeedNodeType.Category */) { 
					
					// this can happen only once. 
					// A Feed can only have category/root as parents
					// we assume here, that the readCounter is a "reset"
					// of the current node.UnreadCounter to the new value.
					// So at first we have to correct all the parents
					UpdateReadStatus(thisNode.Parent, -thisNode.UnreadCount);
					thisNode.UnreadCount = readCounter;

				} else {
					thisNode.UnreadCount += readCounter;
				}
				
				// now we had set the new value, refresh the parent(s)
				UpdateReadStatus(thisNode.Parent, readCounter);

			}

		}
		
		public void UpdateReadStatus(FeedTreeNodeBase thisNode, bool anyUnread) {
			if (thisNode == null) return;
			
			if (!anyUnread) {			// mark read
				// traverse tree: upwards, one by one. On every step
				// look for unread childs (go down). If there is one, stop walking up.
				// If not, mark it read (normal font state), go on upwards
				if (this.Equals(thisNode)) { 
					
					// this can happen only once. 
					// A Feed can only have category/root folder as parents
					thisNode.AnyUnread = false;
					UpdateReadStatus(thisNode, 0);	// correct the counters
					
				} 
				else { // Category/Root mark read

					thisNode.AnyUnread = false;
					UpdateReadStatus(thisNode.Parent, false);
				}
			}
			else {	// mark unread 
				// traverse tree: upwards, one by one. On each parent
				// mark it unread (bold font state), go on upwards
				thisNode.AnyUnread = true;
				UpdateReadStatus(thisNode.Parent, true);

			}

		}

		public abstract bool AllowedChild(FeedNodeType nsType);
		public abstract void PopupMenu(System.Drawing.Point screenPos);
		public abstract void UpdateContextMenu();
	}
}
