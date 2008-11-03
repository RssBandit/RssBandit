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
 * $Log: UltraTreeExtended.cs,v $
 * Revision 1.6  2007/02/17 20:26:43  carnage4life
 * Incomplete attempt to fix multiselect issues in Outlook 2003 listview
 *
 * Revision 1.5  2006/09/27 00:12:24  carnage4life
 * Fixed issue where items from the previous day were showing up in "Today" and "Yesterday" groups in the Outlook 2003 view
 *
 * Revision 1.4  2006/09/22 15:35:44  t_rendelmann
 * added CVS header and change history
 *
 */
#endregion

using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using RssBandit.WinGui.Controls.ThListView;
using Infragistics.Win.UltraWinTree;
using NewsComponents;
using RssBandit.Resources;

namespace RssBandit.WinGui.Controls
{
	public class UltraTreeNodeExtended : UltraTreeNode
	{
		private INewsItem _newsItem = null;
		private DateTime _datetime = DateTime.MinValue;
		private ThreadedListViewItem _nodeOwner = null;
		private bool _isGroupOneDay = false;
		private bool _isCommentUpdating = false;
		private Rectangle _flagRectangle = Rectangle.Empty;
		private Rectangle _commentsRectangle = Rectangle.Empty;
		private Rectangle _enclosureRectangle = Rectangle.Empty;
		private Rectangle _collapseRectangle   = Rectangle.Empty;

		public Rectangle EnclosureRectangle
		{
			get { return _enclosureRectangle; }
			set { _enclosureRectangle = value; }
		}

		public Rectangle CollapseRectangle {
			get { return _collapseRectangle; }
			set { _collapseRectangle = value; }
		}

		public Rectangle CommentsRectangle
		{
			get { return _commentsRectangle; }
			set { _commentsRectangle = value; }
		}

		public Rectangle FlagRectangle
		{
			get { return _flagRectangle; }
			set { _flagRectangle = value; }
		}

		public bool IsCommentUpdating
		{
			get { return _isCommentUpdating; }
			set { _isCommentUpdating = value; }
		}

		public bool IsGroupOneDay
		{
			get { return _isGroupOneDay; }
			set { _isGroupOneDay = value; }
		}

		public ThreadedListViewItem NodeOwner
		{
			get { return _nodeOwner; }
			set { _nodeOwner = value; }
		}

		public INewsItem NewsItem
		{
			get { return _newsItem; }
			set { _newsItem = value; }
		}

		public DateTime DateTime
		{
			get { return _datetime; }
			set { _datetime = value; }
		}
	}
	
	public class UltraTreeNodeExtendedDateTimeComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			UltraTreeNodeCell xc = (UltraTreeNodeCell) x;
			UltraTreeNodeCell yc = (UltraTreeNodeCell) y;
			UltraTreeNodeExtended n1 = xc.Node as UltraTreeNodeExtended;
			UltraTreeNodeExtended n2 = yc.Node as UltraTreeNodeExtended;
			if(n1==null && n2==null) 
				return 0;
			else if(n1==null)
				return -1;
			else if(n2==null)
				return 1;
			if(n1.Level==2 && n2.Level==2)
				return n1.DateTime.CompareTo(n2.DateTime); //We want the comments to be in ascending order
			else
				return n2.DateTime.CompareTo(n1.DateTime);
		}
	}
	
	public class UltraTreeExtended : UltraTree
	{
		public static int DATETIME_GROUP_HEIGHT = 23;
		public static int COMMENT_HEIGHT = 19;
		
		private Hashtable _lviItems = new Hashtable();
		private bool _isUpdateingSelection = false;

		public IList SelectedItems{

			get{
				ArrayList selectedItems = new ArrayList(); 
				foreach(UltraTreeNodeExtended node in this.SelectedNodes){
					if(node.Level == 0){ //grouping like 'Yesterday' or 'Today'
						foreach(UltraTreeNodeExtended childNode in node.Nodes){
							selectedItems.Add(childNode.NodeOwner); 
						}
					}else{
						selectedItems.Add(node.NodeOwner); 
					}
				}			
				return selectedItems; 
			}
		}
			

		public bool IsUpdatingSelection
		{
			get { return _isUpdateingSelection; }
			set { _isUpdateingSelection = value; }
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			//
			UltraTreeNode node = this.GetNodeFromPoint(e.X, e.Y);
			if(node!=null)
			{

				 if(e.Button == MouseButtons.Left){
				
					if(Control.ModifierKeys == Keys.Control){						
						node.Selected = !node.Selected;										
					}else{
						this.SelectedNodes.Clear();
						node.Selected = true;
						this.ActiveNode = node; 
					}

				}  				
								
			}
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnDoubleClick(e);
			//
			Point p = this.PointToClient(Cursor.Position);
			UltraTreeNode node = this.GetNodeFromPoint(p);
			if(node!=null)
			{
				node.Expanded = !node.Expanded;
			}
		}

		public UltraTreeNodeExtended GetRootNode(DateTime dt)
		{
			DateTime dt0;
			bool isGroupOneDay;
			string res = GetRootString(dt, out dt0, out isGroupOneDay);
			foreach(UltraTreeNodeExtended node in this.Nodes)
			{
				if(node.Text == res)
				{
					return node;
				}
			}
			//Create DateTime Group Node
			UltraTreeNodeExtended newNode = new UltraTreeNodeExtended();
			newNode.DateTime = dt0;
			newNode.IsGroupOneDay = isGroupOneDay;
			this.Nodes.Add(newNode);
			newNode.Cells[0].Value = /*dt0.ToString()+" "+ */res;
			newNode.Cells[0].Appearance.Cursor = Cursors.Hand;
			newNode.Override.NodeAppearance.Cursor = Cursors.Hand;
			newNode.Override.HotTrackingNodeAppearance.Cursor = Cursors.Hand;
			newNode.Override.ItemHeight = DATETIME_GROUP_HEIGHT;
			
			return newNode;
		}
		
		public static string GetRootString(DateTime dt, out DateTime dt0, out bool isGroupOneDay)
		{
			dt0 = new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day,0,0,0);
			if(dt>dt0)
			{
				isGroupOneDay = true;
				return "Today";
			}
			if(dt>dt0.AddDays(-1))
			{
				isGroupOneDay = true;
				dt0 = dt0.AddDays(-1);
				return "Yesterday";
			}
			//
			dt0 = dt0.AddDays(-1);
			for(int i=(int)dt0.DayOfWeek;i<7;i++)
			{
				dt0 = dt0.AddDays(-1);
				if(dt>dt0)
				{
					isGroupOneDay = true;
					return dt0.DayOfWeek.ToString();
				}
			}
			//Last Week
			if(dt>dt0.AddDays(-7))
			{
				isGroupOneDay = false;
				dt0 = dt0.AddDays(-7);
				return "Last Week";
			}
			if(dt>dt0.AddDays(-dt0.Day+1))
			{
				isGroupOneDay = false;
				dt0 = dt0.AddDays(-dt0.Day+1);
				return "Last Month";
			}
			//
			isGroupOneDay = false;
			dt0 = DateTime.MinValue;
			return "Older";
		}
		
		public UltraTreeNodeExtended GetFromLVI(ThreadedListViewItem item)
		{
			if(!_lviItems.ContainsKey(item))
				return null;
			return _lviItems[item] as UltraTreeNodeExtended;
		}
		
		public void Add(ThreadedListViewItem item)
		{
			INewsItem ni = (INewsItem) item.Key;
			UltraTreeNodeExtended root = null;
			if(item.IsComment)
			{
				//IsComment
				root = GetFromLVI(item.Parent);
			}
			else
			{
				root = GetRootNode(ni.Date.ToLocalTime());
			}
			//
			UltraTreeNodeExtended n = new UltraTreeNodeExtended();
			n.NewsItem = ni;
			n.DateTime = ni.Date;
			n.NodeOwner = item;
			_lviItems.Add(item, n);
			root.Nodes.Add(n);
			//
			if(item.IsComment)
				ConfigureComment(n, ni);
			else
				ConfigureArticle(n, ni);
			//Comments
			if(ni.CommentCount>0 && !item.IsComment)
			{
				n.Override.ShowExpansionIndicator = ShowExpansionIndicator.Always;
			}
			//
			if(!item.IsComment)
			{
				root.Expanded = true;
			}
		}
		
		public void ConfigureArticle(UltraTreeNodeExtended n, INewsItem ni)
		{
			n.Cells[0].Value = /*ni.Date.ToString()+" "+ */ni.Title;
			n.Cells[0].Appearance.Cursor = Cursors.Hand;
			n.Override.NodeAppearance.Cursor = Cursors.Hand;
			n.Override.HotTrackingNodeAppearance.Cursor = Cursors.Hand;
		}
			
		public void AddRange(ThreadedListViewItem[] items)
		{
			this.BeginUpdate();
			foreach(ThreadedListViewItem item in items)
			{
				this.Add(item);
			}
			this.EndUpdate();
		}
		
		public void AddCommentUpdating(ThreadedListViewItem lvi)
		{
			UltraTreeNodeExtended node = GetFromLVI(lvi);
			if(node.Nodes.Count>0)
			{
				//It already has comments => don't do anything
				return;
			}
			//
			UltraTreeNodeExtended n = new UltraTreeNodeExtended();
			n.IsCommentUpdating = true;
			n.Override.ItemHeight = COMMENT_HEIGHT;
			node.Nodes.Add(n);
			n.Cells[0].Value = SR.GUIStatusLoadingChildItems;
		}
		
		public void AddRangeComments(ThreadedListViewItem pLVI, ThreadedListViewItem[] items)
		{
			this.BeginUpdate();
			//
			UltraTreeNodeExtended parent = GetFromLVI(pLVI);
			parent.Nodes.Clear();
			//
			foreach(ThreadedListViewItem item in items)
			{
				INewsItem ni = (INewsItem) item.Key;
				UltraTreeNodeExtended n = new UltraTreeNodeExtended();
				n.NewsItem = ni;
				n.DateTime = ni.Date;
				n.NodeOwner = item;
				_lviItems.Add(item, n);
				parent.Nodes.Add(n);
				//
				ConfigureComment(n, ni);
			}
			//
			this.EndUpdate();
		}

		private void ConfigureComment(UltraTreeNodeExtended n, INewsItem ni)
		{
			n.Override.ItemHeight = COMMENT_HEIGHT;
			n.Cells[0].Value = ni.Title;
			n.Cells[0].Appearance.Cursor = Cursors.Hand;
			n.Override.NodeAppearance.Cursor = Cursors.Hand;
			n.Override.HotTrackingNodeAppearance.Cursor = Cursors.Hand;
		}

		public void Clear()
		{
			this.Nodes.Clear();
			_lviItems.Clear();
		}
		
		public void Remove(ThreadedListViewItem lvi)
		{
			UltraTreeNodeExtended node = GetFromLVI(lvi);
			if(node==null) 
				return;
			//
			UltraTreeNode parent = null;
			if(node.Level==1)
			{
				parent = node.Parent;
			}
			_lviItems.Remove(lvi);
			node.Parent.Nodes.Remove(node);
			if(parent!=null)
			{
				if(parent.Nodes.Count==0)
				{
					this.Nodes.Remove(parent);
				}
			}
		}
		
		public UltraTreeNode GetItem(int index)
		{
			int ind = 0;
			for(int i=0; i<Nodes.Count; i++)
			{
				for(int j=0; j<Nodes[i].Nodes.Count; j++)
				{
					if(ind==index)
					{
						return Nodes[i].Nodes[j];
					}
					ind++;
				}
			}
			return null;
		}
		
		public int ItemsCount()
		{
			int ind = 0;
			for(int i=0; i<Nodes.Count; i++)
			{
				for(int j=0; j<Nodes[i].Nodes.Count; j++)
				{
					ind++;
				}
			}
			return ind;
		}
	}
}
