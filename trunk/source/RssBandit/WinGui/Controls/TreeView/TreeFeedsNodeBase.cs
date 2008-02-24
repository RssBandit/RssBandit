#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

#region CVS Version Log
/*
 * $Log: TreeFeedsNodeBase.cs,v $
 * Revision 1.10  2007/02/13 16:23:45  t_rendelmann
 * changed: treestate save/restore is now I18N aware
 *
 * Revision 1.9  2007/02/09 14:54:09  t_rendelmann
 * fixed: added missing configuration option for newComments font style and color;
 * changed: some refactoring in FontColorHelper;
 *
 * Revision 1.8  2006/09/29 18:14:36  t_rendelmann
 * a) integrated lucene index refreshs;
 * b) now using a centralized defined category separator;
 * c) unified decision about storage relevant changes to feed, feed and feeditem properties;
 * d) fixed: issue [ 1546921 ] Extra Category Folders Created
 * e) fixed: issue [ 1550083 ] Problem when renaming categories
 *
 * Revision 1.7  2006/09/07 16:47:44  carnage4life
 * Fixed two issues
 * 1. Added SelectedImageIndex and ImageIndex to FeedsTreeNodeBase
 * 2. Fixed issue where watched comments always were treated as having new comments on HTTP 304
 *
 * Revision 1.6  2006/09/06 02:11:56  carnage4life
 * Fixed some bugs with favicon support
 *
 * Revision 1.5  2006/09/03 19:08:50  carnage4life
 * Added support for favicons
 *
 * Revision 1.4  2006/08/08 14:24:45  t_rendelmann
 * fixed: nullref. exception on "Move to next unread" (if it turns back to treeview top node)
 * fixed: nullref. exception (assertion) on delete feeds/category node
 * changed: refactored usage of node.Tag (object type) to use node.DataKey (string type)
 *
 */
#endregion

using System;
using System.Collections;
using System.Drawing;
using Infragistics.Win.UltraWinTree;
using NewsComponents;
using NewsComponents.Utils;
using RssBandit.Resources;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Utility;

namespace RssBandit.WinGui.Controls
{
	/// <summary>
	/// Base class of the tree view nodes
	/// </summary>
	public abstract class TreeFeedsNodeBase: UltraTreeNode {
		/// <summary>
		/// Gets raised, if the node's read counter reach zero
		/// </summary>
		public event System.EventHandler ReadCounterZero;

		/// <summary>
		/// Used in the node editing events to keep the old node Text
		/// we need in the AfterLabelEditing event to maintain related
		/// data structures.
		/// </summary>
		public string TextBeforeEditing;

		private FeedNodeType	_type;
		private bool			_editable, _anyUnread, _anyNewComments;
		protected bool          m_hasCustomIcon;
		private int				_unreadCount;
		private int             _itemsWithNewCommentsCount; 
		private int				_highlightCount;
		private object			_initialImage, _initialExpandedImage;
		private int             _imageIndex, _selectedImageIndex; 

		private static Image	_clickableAreaExtenderImage;
		#region ctor's

		static TreeFeedsNodeBase() {
			_clickableAreaExtenderImage = new Bitmap(1,1);
		}
		protected TreeFeedsNodeBase() {}	// not anymore allowed
		protected TreeFeedsNodeBase(string text, FeedNodeType nodeType):
			this(text, nodeType, false) {
			}
		protected TreeFeedsNodeBase(string text, FeedNodeType nodeType, bool editable):
			this(text, nodeType, editable, -1, -1) {
			}
		
		protected TreeFeedsNodeBase(string text, FeedNodeType nodeType, bool editable, int imageIndex, int expandedNodeImageIndex):
			this(text, nodeType, editable, imageIndex, expandedNodeImageIndex, null){
		}

		protected TreeFeedsNodeBase(string text, FeedNodeType nodeType, bool editable, int imageIndex, int expandedNodeImageIndex, Image image):
			base() 
		{
			this.RightImages.Add(_clickableAreaExtenderImage);

			FontColorHelper.CopyFromFont( this.Override.NodeAppearance.FontData, FontColorHelper.NormalFont);
			this.ForeColor = FontColorHelper.NormalColor;
			
			_unreadCount = _highlightCount = 0;
			_initialImage = _initialExpandedImage = null;

			if(image != null){
				_initialImage = _initialExpandedImage = base.Override.NodeAppearance.Image = base.Override.ExpandedNodeAppearance.Image = image;
				m_hasCustomIcon = true; 
			}else{
				if (imageIndex >= 0) {
					base.Override.NodeAppearance.Image = imageIndex;
					_imageIndex = imageIndex;
				}
				if (expandedNodeImageIndex >= 0) {
					base.Override.ExpandedNodeAppearance.Image = expandedNodeImageIndex;
					_selectedImageIndex = expandedNodeImageIndex;
				}
			}
			_type = nodeType;
			_editable = editable;
			_anyUnread = false;
			this.Text = text;	// uses some of the above variables, so keep it at last...
		}

		#endregion

		/// <summary>
		/// Gets the data key. Is virtually the same as calling
		/// <see cref="DataKey"/> property on the base class, 
		/// but we prefer to use this to get rid of the casts (to string).
		/// </summary>
		/// <value>The data key (string).</value>
		/// <remarks>Used to store feedUrls for Feed node types, etc.
		/// </remarks>
		public new virtual string DataKey {
			get { return base.DataKey as string; }
			set { base.DataKey = value; }
		}

		/// <summary>
		/// Gets the image of the node. If the node has been assigned a "real"
		/// image, this will be returned. If it has a image index within the imagelist,
		/// this image will be returned. If no image was found, null is returned.
		/// </summary>
		/// <value>The resolved node image.</value>
		public Image ImageResolved {
			get {
				if (this.Override.NodeAppearance.Image != null) {
					if (this.Override.NodeAppearance.Image.GetType().Equals(typeof(Image))) {
						return (Image)this.Override.NodeAppearance.Image;
					} else {	// imagelist (index) based:
						if (this.Control != null && this.Control.ImageList != null)
							return this.Override.NodeAppearance.GetImage(this.Control.ImageList);
					}
				}
				return null;
			}
		}

		/// <summary>
		/// Sets the individual node image. If null is provided,
		/// the node's original image is restored. 
		/// </summary>
		/// <param name="image">The image.</param>
		public void SetIndividualImage(Image image) {
			if (image == null) { 
				// reset to previous
				this.Override.NodeAppearance.Image = this._initialImage;
				this.Override.SelectedNodeAppearance.Image = this._initialImage;
				this.Override.ExpandedNodeAppearance.Image = this._initialExpandedImage;
			} else {
				// set the new image(s)
				this._initialImage = this._initialExpandedImage = image; 
				this.Override.NodeAppearance.Image = image;
				this.Override.SelectedNodeAppearance.Image = image;				
				this.Override.ExpandedNodeAppearance.Image = image;
				this.m_hasCustomIcon = true;
			}
		}

		public virtual bool HasCustomIcon { 
			get { return m_hasCustomIcon; } 
		}

		/// <summary>
		/// Gets the value of the selected image index for this node type. 
		/// </summary>
		public int SelectedImageIndex{
			get{ return this._selectedImageIndex; }
		}

		/// <summary>
		/// Gets the value of the image index for this node type. 
		/// </summary>
		public int ImageIndex{
			get{ return this._imageIndex; }
		}

		public virtual FeedNodeType Type { get { return _type; } set { _type = value; } }
		public virtual bool Editable { get { return _editable; } set { _editable = value; } }

		public virtual Color ForeColor { 
			get { return base.Override.NodeAppearance.ForeColor; } 
			set { 
				if (base.Override.NodeAppearance.ForeColor != value)
					base.Override.NodeAppearance.ForeColor = value; 
			} 
		}

		// override some methods so we do not always have to cast explicitly
		public virtual TreeFeedsNodeBase FirstNode	{
			get {
				if(base.Nodes.Count==0)
					return null;
				else
					return (TreeFeedsNodeBase)base.Nodes[0];
			}
		}
		public virtual TreeFeedsNodeBase NextNode	{
			get {
				for(int i=0; base.Parent != null && i<base.Parent.Nodes.Count-1; i++) {
					if(base.Parent.Nodes[i]==this) {
						return (TreeFeedsNodeBase) base.Parent.Nodes[i + 1];
					}
				}
				return null;
			}
		}
		public virtual TreeFeedsNodeBase LastNode	{
			get {
				if(base.Nodes.Count==0)
					return null;
				else
					return (TreeFeedsNodeBase)base.Nodes[Nodes.Count-1];
			}
		}
		public virtual new TreeFeedsNodeBase Parent	{
			get {	return (TreeFeedsNodeBase)base.Parent; }
		}
		
		/// <summary>
		/// Now handles the whole visual node text formatting
		/// </summary>
		/// <value>Returns the Text (caption) of the node. 
		/// Sets the new displayed text and font of a node according to the
		/// unread items counter info.</value>
		public virtual new string Text {
			get { return base.Text; }
			set { 
				if (StringHelper.EmptyTrimOrNull(value))
					base.Text = SR.GeneralNewItemText;
				else
					base.Text = value;
			}
		}

		/// <summary>
		/// Invalidates the node for UI repainting.
		/// </summary>
		protected void InvalidateNode() {
			
			bool yetInvalidated = false;

			if (_highlightCount > 0) {
				if (!FontColorHelper.StyleEqual(this.Override.NodeAppearance.FontData, FontColorHelper.HighlightStyle)) {
					FontColorHelper.CopyFromFont(this.Override.NodeAppearance.FontData, FontColorHelper.HighlightFont);
					yetInvalidated = true;
				}
				if (this.ForeColor != FontColorHelper.HighlightColor) {
					this.ForeColor = FontColorHelper.HighlightColor;
					yetInvalidated = true;
				}
			} else if (_unreadCount > 0 || _anyUnread) {
				if (!FontColorHelper.StyleEqual(this.Override.NodeAppearance.FontData, FontColorHelper.UnreadStyle)) {
					FontColorHelper.CopyFromFont(this.Override.NodeAppearance.FontData, FontColorHelper.UnreadFont);
					yetInvalidated = true;
				}
				if (this.ForeColor != FontColorHelper.UnreadColor) {
					this.ForeColor = FontColorHelper.UnreadColor;
					yetInvalidated = true;
				}
			} else  {
				if (!FontColorHelper.StyleEqual(this.Override.NodeAppearance.FontData, FontColorHelper.NormalStyle)) {
					FontColorHelper.CopyFromFont(this.Override.NodeAppearance.FontData, FontColorHelper.NormalFont);
					yetInvalidated = true;
				}
				if (this.ForeColor != FontColorHelper.NormalColor) {
					this.ForeColor = FontColorHelper.NormalColor;
					yetInvalidated = true;
				}
			}

			//check if we have new comments and merge style with that of others if needed
			if (this._itemsWithNewCommentsCount > 0 || this._anyNewComments) {
				if (!FontColorHelper.StyleEqual(this.Override.NodeAppearance.FontData, FontColorHelper.NewCommentsStyle)) {
					Font font = FontColorHelper.MergeFontStyles(FontColorHelper.CopyToFont(this.Override.NodeAppearance.FontData), FontColorHelper.NewCommentsStyle);
					FontColorHelper.CopyFromFont(this.Override.NodeAppearance.FontData, font);
					yetInvalidated = true;
				}
				if (this.ForeColor != FontColorHelper.NewCommentsColor) {
					this.ForeColor = FontColorHelper.NewCommentsColor;
					yetInvalidated = true;
				}
			}

			if (!yetInvalidated && this.Control != null)
				this.Control.Invalidate();
		}

		/// <summary>
		/// AnyNewComments and ItemsWithNewCommentsCount are working interconnected:
		/// if you set AnyUnreadComments to true, this will update the visualized info to
		/// use the NewComments Font. Otherwise, if you set AnyUnreadComments to false, it will 
		/// refresh the caption rendering to default.
		/// </summary>
		public virtual bool AnyNewComments {
			get {	return (_anyNewComments || _itemsWithNewCommentsCount > 0);  }
			set {	
				if (_anyNewComments != value) { 
					_anyNewComments = value; 
					InvalidateNode();
				}
			}
		}

		/// <summary>
		/// AnyNewComments and ItemsWithNewCommentsCount are working interconnected:
		/// if you set AnyNewComments to non zero, ItemsWithNewCommentsCount will be set to true and
		/// then updates the visualized info to use the NewComments Font and 
		/// read counter state info. If ItemsWithNewCommentsCount is set to zero,
		/// also AnyNewComments is reset to false and refresh the caption to default.
		/// </summary>
		public virtual int ItemsWithNewCommentsCount {
			get {	return _itemsWithNewCommentsCount;  }
			set {	
				if (value != _itemsWithNewCommentsCount) {
					_itemsWithNewCommentsCount = value; 
					_anyNewComments = (_itemsWithNewCommentsCount > 0);
					//TODO: optimize invalidation to only visualized state changes
					InvalidateNode();
				}
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
					InvalidateNode();
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
		public virtual int UnreadCount {
			get {	return _unreadCount;  }
			set {	
				if (value != _unreadCount) {
					_unreadCount = value; 
					_anyUnread = (_unreadCount > 0);
					
					InvalidateNode();
					
					if (_unreadCount == 0)
						RaiseReadCounterZero();
				}
			}
		}



		private void RaiseReadCounterZero() {
			if (this.ReadCounterZero != null)
				this.ReadCounterZero(this, EventArgs.Empty);
		}


		public virtual int HighlightCount {
			get {	return _highlightCount;  }
			set {	
				if (value != _highlightCount) {
					_highlightCount = value; 
					//TODO: optimize invalidation to only visualized state changes
					InvalidateNode();
				}
			}
		}

		/// <summary>
		/// Gets the name of the category store.
		/// </summary>
		/// <value>The name of the category store.</value>
		public virtual string CategoryStoreName {
			get {
				return BuildCategoryStoreName(this); 
			}
		}



		public void UpdateCommentStatus(TreeFeedsNodeBase thisNode, int readCounter) {
			if (thisNode == null) return;
			
			if (readCounter <= 0) {			// mark read

				// traverse tree: upwards, one by one. On every step
				// look for children with new comments (go down). If there is one, stop walking up.
				// If not, mark it read (normal font state), go on upwards
				if (this.Equals(thisNode)) { 
					// this can happen only once. A Feed can only have category/root as parents
					
					if (thisNode.ItemsWithNewCommentsCount < Math.Abs(readCounter))
						readCounter = -thisNode.ItemsWithNewCommentsCount;

					if (readCounter == 0)
						readCounter = -thisNode.ItemsWithNewCommentsCount;

					thisNode.ItemsWithNewCommentsCount += readCounter;
					UpdateCommentStatus(thisNode.Parent, readCounter);
					
				} 
				else { // Category/Root mark read

					thisNode.ItemsWithNewCommentsCount += readCounter;
					UpdateCommentStatus(thisNode.Parent, readCounter);
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
					UpdateCommentStatus(thisNode.Parent, -thisNode.ItemsWithNewCommentsCount);
					thisNode.ItemsWithNewCommentsCount = readCounter;

				} else {
					thisNode.ItemsWithNewCommentsCount += readCounter;
				}
				
				// now we had set the new value, refresh the parent(s)
				UpdateCommentStatus(thisNode.Parent, readCounter);

			}

		}
		
		public void UpdateCommentStatus(TreeFeedsNodeBase thisNode, bool anyNewComments) {
			if (thisNode == null) return;
			
			if (!anyNewComments) {			// mark read
				// traverse tree: upwards, one by one. On every step
				// look for unread childs (go down). If there is one, stop walking up.
				// If not, mark it read (normal font state), go on upwards
				if (this.Equals(thisNode)) { 
					
					// this can happen only once. 
					// A Feed can only have category/root folder as parents
					thisNode.AnyNewComments = false;
					UpdateCommentStatus(thisNode, 0);	// correct the counters
					
				} 
				else { // Category/Root mark read

					thisNode.AnyNewComments = false;
					UpdateCommentStatus(thisNode.Parent, false);
				}
			}
			else {	// mark unread 
				// traverse tree: upwards, one by one. On each parent
				// mark it unread (bold font state), go on upwards
				thisNode.AnyNewComments = true;
				UpdateCommentStatus(thisNode.Parent, true);

			}

		}


		public void UpdateReadStatus(TreeFeedsNodeBase thisNode, int readCounter) {
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
		
		public void UpdateReadStatus(TreeFeedsNodeBase thisNode, bool anyUnread) {
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

		/// <summary>
		/// Helper that builds the full path trimmed category name (without root caption)
		/// </summary>
		/// <param name="node">the FeedTreeNodeBase</param>
		/// <returns>
		/// Category name in this form: 
		///   'Main Category\Sub Category\...\catNode Category'.
		/// Returns null in case node was null or no categories are found.
		/// </returns>
		public static string BuildCategoryStoreName(TreeFeedsNodeBase node) {
			string[] catArray = BuildCategoryStoreNameArray(node);
			if (catArray.Length == 0)
				return null;

			return String.Join(FeedSource.CategorySeparator, catArray);

			#region Old impl. (kept for ref/check the new impl. for regressions)
//			string pathSep = @"\";
//			if (node.Control != null)
//				pathSep = node.Control.PathSeparator;
//
//			string s = node.FullPath.Trim();
//			string[] a = s.Split(pathSep.ToCharArray());
//
//			if (node.Type == FeedNodeType.Feed || node.Type == FeedNodeType.Finder) {
//				if (a.Length > 2)
//					return String.Join(@"\",a, 1, a.Length-2);
//			}
//			else {
//				if (a.Length > 1)
//					return String.Join(@"\",a, 1, a.Length-1);
//			}
//			
//			return null;	// default category (none)
			#endregion
		}
		
		/// <summary>
		/// Helper that builds the full path trimmed category name array (without root caption)
		/// </summary>
		/// <param name="node">the FeedTreeNodeBase</param>
		/// <returns>Category names in this form: ['Main Category', 'Sub Category', ...,'catNode Category'].</returns>
		public static string[] BuildCategoryStoreNameArray(TreeFeedsNodeBase node) {
			
			if (node == null)
				return new string[]{};
			
			if (node.Type == FeedNodeType.Feed || node.Type == FeedNodeType.Finder) {
				return TreeHelper.BuildCategoryStoreNameArray(node.FullPath, true);
			} else {
				return TreeHelper.BuildCategoryStoreNameArray(node.FullPath, false);
			}

//			string s = node.FullPath.Trim();
//			ArrayList a = new ArrayList(s.Split(FeedSource.CategorySeparator.ToCharArray()));
//
//			if (node.Type == FeedNodeType.Feed || node.Type == FeedNodeType.Finder) {
//				if (a.Count > 2) {
//					string[] ret = new string[a.Count-2];
//					a.CopyTo(1, ret, 0, a.Count-2);
//					return ret;
//				}
//			}
//			else {
//				if (a.Count > 1) {
//					string[] ret = new string[a.Count-1];
//					a.CopyTo(1, ret, 0, a.Count-1);
//					return ret;
//				}
//			}
//			
//			return new string[]{};	// default category (none)
		}

		/// <summary>
		/// Gets the tpyed root full path of the node.
		/// The root node is represented by it's FeedNodeType.
		/// As such the returned path is I18N aware (root node captions
		/// are localized!)
		/// </summary>
		/// <value>The tpyed root full path.</value>
		public string TypedRootFullPath {
			get {
				string[] a = this.FullPath.Split(FeedSource.CategorySeparator.ToCharArray());
				if (a.Length > 0) {
					a[0] = this.RootNode.GetType().Name;
				}
				return String.Join(FeedSource.CategorySeparator, a);
			}
		}
		
		public abstract bool AllowedChild(FeedNodeType nsType);
		public abstract void PopupMenu(System.Drawing.Point screenPos);
		public abstract void UpdateContextMenu();
	}

}
