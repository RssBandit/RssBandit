#region CVS Version Header
/*
 * $Id: NavigationHistory.cs,v 1.13 2007/04/03 13:32:24 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2007/04/03 13:32:24 $
 * $Revision: 1.13 $
 */
#endregion

#region usings
using System;
using System.Drawing;
using System.Collections;
using Infragistics.Win.UltraWinToolbars;
using RssBandit.WinGui.Controls;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Tools;
using RssBandit.WinGui.Utility;
using NewsComponents;
using NewsComponents.Utils;
#endregion

namespace RssBandit.WinGui.Utility
{
	#region History 
	/// <summary>
	/// A class with behavior similar to 
	/// IE's history navigation (Back, Forward)
	/// </summary>
	internal class History {
		
		#region ivars
		const int DefaultMaxEntries = 100;
		
		private int _maxEntries;
		private ArrayList _historyEntries;
		private int _currentPosition;
		#endregion

		#region events/delegates
		/// <summary>
		/// Raised if the history state changed (Add,
		/// Clear, GetNext or GetPrevious called)
		/// </summary>
		public event EventHandler StateChanged;
		#endregion

		#region ctor's
		public History():this(DefaultMaxEntries) {}

		public History(int maxEntries) {
			_maxEntries = (maxEntries > 0 ? maxEntries: DefaultMaxEntries);
			_currentPosition = 0;
			_historyEntries = new ArrayList(_maxEntries);	
		}
		#endregion

		#region public members
		/// <summary>
		/// Add an entry to the history
		/// </summary>
		/// <param name="entry"></param>
		public void Add(HistoryEntry entry) { 
			if (entry == null) return;
			// if we moved the position pointer, add resets the list 
			// to backward operations state only:
			TrimToPosition();

			// don't save the same entry twice or more. 
			// Instead we re-order them to end of list:
			int entryFoundAt = _historyEntries.IndexOf(entry);
			if(_historyEntries.Count > 0 && entryFoundAt >= 0) {
				_historyEntries.RemoveAt(entryFoundAt);
			}
			if(_historyEntries.Count > _maxEntries) { // save only the last _maxEntries 
				_historyEntries.RemoveAt(0);
			}

			// add entry and reset position pointer
			_historyEntries.Add(entry);
			_currentPosition = _historyEntries.Count - 1;
			OnStateChanged();
		}

		/// <summary>
		/// Clear the history
		/// </summary>
		public void Clear() {
			_historyEntries.Clear();
			_currentPosition = 0;
			OnStateChanged();
		}

		/// <summary>
		/// Gets the number of entries actually contained in the history.
		/// </summary>
		public int Count {
			get {
				return _historyEntries.Count;	
			}
		}

		/// <summary>
		/// Gets the next entry from history 
		/// (Move forward operation)
		/// </summary>
		public HistoryEntry GetNext() {
			return GetNextAt(0);
		}

		/// <summary>
		/// Gets the next entry from history from the specified 
		/// zero based index (Move Forward operation)
		/// </summary>
		public HistoryEntry GetNextAt(int index) {

			if (index < 0) throw new ArgumentOutOfRangeException();

			int newPos = _currentPosition + index + 1;
			if(newPos < _historyEntries.Count && newPos != _currentPosition) {
				_currentPosition = newPos;
				OnStateChanged();
			}
			
			if(_historyEntries.Count != 0 && _currentPosition < _historyEntries.Count) {
				HistoryEntry ret = (HistoryEntry)_historyEntries[_currentPosition];
				// valid node?
				if (ret.Node == null || ret.Node != null && ret.Node.Control != null)
					return ret;
				else
					return GetNextAt(index);
			} else
				return null;
		}

		/// <summary>
		/// Gets the previous entry from history
		/// (Move back operation)
		/// </summary>
		public HistoryEntry GetPrevious() {
			return GetPreviousAt(0);
		}

		/// <summary>
		/// Gets the previous entry from history from the specified 
		/// zero based index (Move back operation)
		/// </summary>
		public HistoryEntry GetPreviousAt(int index) {

			if (index < 0) throw new ArgumentOutOfRangeException();

			int newPos = _currentPosition - index - 1;
			if(newPos >= 0 && newPos != _currentPosition) {
				_currentPosition = newPos;
				OnStateChanged();
			}
			if(_historyEntries.Count != 0) {
				HistoryEntry ret = (HistoryEntry)_historyEntries[_currentPosition];
				// valid node?
				if (ret.Node == null || ret.Node != null && ret.Node.Control != null)
					return ret;
				else
					return GetPreviousAt(index);
			} else
				return null;
		}

		/// <summary>
		/// Gets true, if the history can get the next entry
		/// (Can move forward)
		/// </summary>
		public bool CanGetNext {
			get {
				return (
					_historyEntries.Count != 0 && 
					_currentPosition < _historyEntries.Count-1);
			}	
		}

		/// <summary>
		/// Gets true, if the history can get the previous entry
		/// (Can move backward)
		/// </summary>
		public bool CanGetPrevious {
			get {return _currentPosition > 0; }	
		}

		/// <summary>
		/// Gets a string list of Next-History entries by calling ToString()
		/// for entries (Move Forward Entries List)
		/// </summary>
		/// <param name="maxEntries">how many entries should be returned at once.
		/// If 0 (zero) or negative, the internal MaxEntries count will be used.</param>
		/// <returns>string[] list</returns>
		public ITextImageItem[] GetHeadOfNextEntries(int maxEntries) {
			if (! CanGetNext)
				return new ITextImageItem[]{};

			if (maxEntries <= 0)
				maxEntries = this._maxEntries;

			ArrayList head = new ArrayList(maxEntries);
			for (int i = _currentPosition + 1; i < Math.Min(_historyEntries.Count, _currentPosition + maxEntries + 1); i++) {
				HistoryEntry he = (HistoryEntry)_historyEntries[i];
				if (he.Node != null) {
					if (he.Node.Control == null)
						continue;	// invalid ref (removed)
					Image img = he.Node.ImageResolved;
					//Image img = he.Node.Control.ImageList.Images[(int)he.Node.Override.NodeAppearance.Image];
					head.Add(new TextImageItem(he.ToString(), img ));
				} else {
					head.Add(new TextImageItem(he.ToString(), null));
				}
			}
			if (head.Count > 0)
				return (ITextImageItem[])head.ToArray(typeof(ITextImageItem));
			return new ITextImageItem[]{};
		}
		
		/// <summary>
		/// Gets a string list of Previous-History entries by calling ToString()
		/// for entries (Move Back Entries List)
		/// </summary>
		/// <param name="maxEntries">how many entries should be returned at once.
		/// If 0 (zero) or negative, the internal MaxEntries count will be used.</param>
		/// <returns>string[] list</returns>
		public ITextImageItem[] GetHeadOfPreviousEntries(int maxEntries) {
			if (! CanGetPrevious)
				return new ITextImageItem[]{};

			if (maxEntries <= 0)
				maxEntries = this._maxEntries;

			ArrayList head = new ArrayList(maxEntries);
			for (int i = _currentPosition - 1; i >= Math.Max(0, _currentPosition - maxEntries); i--) {
				HistoryEntry he = (HistoryEntry)_historyEntries[i];
				if (he.Node != null) {
					if (he.Node.Control == null)
						continue;	// invalid ref (removed)
					
					Image img = null; 
					if(he.Node.Override.NodeAppearance.Image is Image){
						img = (Image) he.Node.Override.NodeAppearance.Image;
					}else if (null != he.Node.Override.NodeAppearance.Image){
						img = he.Node.Control.ImageList.Images[(int)he.Node.Override.NodeAppearance.Image];
					}
					head.Add(new TextImageItem(he.ToString(), img ));
				} else {
					head.Add(new TextImageItem(he.ToString(), null));
				}

			}
			if (head.Count > 0)
				return (ITextImageItem[])head.ToArray(typeof(ITextImageItem));
			return new ITextImageItem[]{};
		}

		/// <summary>
		/// Raises the StateChanged event, if set
		/// </summary>
		protected void OnStateChanged() {
			if (StateChanged != null)
				StateChanged(this, EventArgs.Empty);
		}
		#endregion	

		#region private members

		private void TrimToPosition() {
			if (_historyEntries.Count > 0 && _historyEntries.Count-1 > _currentPosition) {
				_historyEntries.RemoveRange(_currentPosition+1, _historyEntries.Count - _currentPosition - 1);
			}
		}

		#endregion
	}
	#endregion

	#region HistoryEntry
	
	/// <summary>
	/// HistoryEntry class. The entries type used by History
	/// </summary>
	internal class HistoryEntry {
		public HistoryEntry():this(null,null) {}
		public HistoryEntry(TreeFeedsNodeBase feedsNode):this(feedsNode, null) {}
		public HistoryEntry(TreeFeedsNodeBase feedsNode, NewsItem item) {
			this.Node = feedsNode;
			this.Item = item;
		}
		public TreeFeedsNodeBase Node;
		public NewsItem Item;
		public override bool Equals(object obj) {
			if (null == (obj as HistoryEntry))
				return false;
			if (Object.ReferenceEquals(this, obj))
				return true;
			HistoryEntry o = obj as HistoryEntry;
			if (Object.ReferenceEquals(this.Node, o.Node) &&
				Object.ReferenceEquals(this.Item, o.Item))
				return true;
			if (this.Node == null || this.Item == null)
				return false;
			if (this.Node.Equals(o.Node) &&
				this.Item.Equals(o.Item))
				return true;
			
			return false;
		}
		public override int GetHashCode() {
			if (this.Node != null) {
				if (this.Item != null)
					return this.Node.GetHashCode() ^ this.Item.GetHashCode();
				return this.Node.GetHashCode();
			} else if (this.Item != null) {
				return this.Item.GetHashCode();
			}
			return base.GetHashCode();
		}

		public override string ToString() {
			if (this.Node != null) {
				if (this.Item != null)
					return StringHelper.ShortenByEllipsis(String.Format("{0} | {1}", this.Node.Text, this.Item.Title), 80);
				return StringHelper.ShortenByEllipsis(this.Node.Text, 80);
			} else if (this.Item != null) {
				return StringHelper.ShortenByEllipsis(String.Format("{0} | {1}", this.Item.FeedDetails.Title, this.Item.Title), 80);
			}
			return "?";
		}


	}
	#endregion

	#region HistoryMenuManager
	internal class HistoryMenuManager
	{
		public event HistoryNavigationEventHandler OnNavigateBack;
		public event HistoryNavigationEventHandler OnNavigateForward;
		
		// only handles our onw created single navigation menu item commands:
		private CommandMediator mediator = null;

		private AppPopupMenuCommand _browserGoBackCommand = null;
		private AppPopupMenuCommand _browserGoForwardCommand = null;

		public HistoryMenuManager() {
			this.mediator = new CommandMediator();
		}
		
		internal void SetControls (AppPopupMenuCommand goBack, AppPopupMenuCommand goForward) {
			this._browserGoBackCommand = goBack; 
			this._browserGoForwardCommand = goForward;
			// init:
			Reset();
			this._browserGoBackCommand.ToolbarsManager.ToolClick -= new ToolClickEventHandler(OnToolbarsManager_ToolClick);
			this._browserGoBackCommand.ToolbarsManager.ToolClick += new ToolClickEventHandler(OnToolbarsManager_ToolClick);
		}
		
		/// <summary>
		/// Resets the control. Should get called after Toolbar.LoadFromXml(), 
		/// because the items maintained dynamically.
		/// </summary>
		internal void Reset() {
			this._browserGoBackCommand.Tools.Clear();
			this._browserGoForwardCommand.Tools.Clear();
		}
		
		internal void ReBuildBrowserGoBackHistoryCommandItems(ITextImageItem[] items) {
			
			this._browserGoBackCommand.Tools.Clear();
			
			for (int i=0; items != null && i < items.Length; i++) 
			{
				ITextImageItem item = items[i];
				string toolKey = "cmdBrowserGoBack_" + i.ToString();
				AppButtonToolCommand cmd = null;
				
				if (this._browserGoBackCommand.ToolbarsManager.Tools.Exists(toolKey))
					cmd = (AppButtonToolCommand)this._browserGoBackCommand.ToolbarsManager.Tools[toolKey];
				
				if (cmd == null) {
					cmd = new AppButtonToolCommand(toolKey, 
						this.mediator, new ExecuteCommandHandler(this.CmdBrowserGoBackHistoryItem),
						item.Text, String.Empty);
					this._browserGoBackCommand.ToolbarsManager.Tools.Add(cmd);
				}
				if (cmd.Mediator == null) {
					cmd.Mediator = this.mediator;
					cmd.OnExecute += new ExecuteCommandHandler(this.CmdBrowserGoBackHistoryItem);
					this.mediator.RegisterCommand(toolKey, cmd);
				} else
				if (cmd.Mediator != this.mediator) {
					this.mediator.ReRegisterCommand(cmd);
				}
				
				cmd.SharedProps.ShowInCustomizer = false;
				cmd.SharedProps.AppearancesSmall.Appearance.Image = item.Image;
				cmd.SharedProps.Caption = item.Text;
				cmd.Tag = i;
				this._browserGoBackCommand.Tools.Add(cmd);

			}//end foreach
			
			if (this._browserGoBackCommand.Tools.Count == 0)
				this._browserGoBackCommand.DropDownArrowStyle = DropDownArrowStyle.None;
			else 
				this._browserGoBackCommand.DropDownArrowStyle = DropDownArrowStyle.Segmented;
		}
		
		internal void ReBuildBrowserGoForwardHistoryCommandItems(ITextImageItem[] items) {
			
			_browserGoForwardCommand.Tools.Clear();
			
			for (int i=0; items != null && i < items.Length; i++) 
			{
				ITextImageItem item = items[i];
				string toolKey = "cmdBrowserGoForward_" + i.ToString();
				AppButtonToolCommand cmd = null;
				if (this._browserGoForwardCommand.ToolbarsManager.Tools.Exists(toolKey))
					cmd = (AppButtonToolCommand)this._browserGoForwardCommand.ToolbarsManager.Tools[toolKey];

				if (cmd == null) 
				{
					cmd = new AppButtonToolCommand(toolKey, 
						this.mediator, new ExecuteCommandHandler(this.CmdBrowserGoForwardHistoryItem),
						item.Text, String.Empty);
					this._browserGoForwardCommand.ToolbarsManager.Tools.Add(cmd);
				}
				
				if (cmd.Mediator == null) {
					cmd.Mediator = this.mediator;
					cmd.OnExecute += new ExecuteCommandHandler(this.CmdBrowserGoForwardHistoryItem);
					this.mediator.RegisterCommand(toolKey, cmd);
				} else
				if (cmd.Mediator != this.mediator) {
					this.mediator.ReRegisterCommand(cmd);
				}
				
				cmd.SharedProps.ShowInCustomizer = false;
				cmd.SharedProps.AppearancesSmall.Appearance.Image = item.Image;
				cmd.SharedProps.Caption = item.Text;
				cmd.Tag = i;
				this._browserGoForwardCommand.Tools.Add(cmd);

			}//end foreach
			
			if (this._browserGoForwardCommand.Tools.Count == 0)
				this._browserGoForwardCommand.DropDownArrowStyle = DropDownArrowStyle.None;
			else 
				this._browserGoForwardCommand.DropDownArrowStyle = DropDownArrowStyle.Segmented;

		}
		
		private void CmdBrowserGoBackHistoryItem(ICommand sender) {
			AppButtonToolCommand cmd = sender as AppButtonToolCommand;
			if (cmd != null)
				RaiseNavigateBackEvent((int)cmd.Tag);
		}
		private void CmdBrowserGoForwardHistoryItem(ICommand sender) {
			AppButtonToolCommand cmd = sender as AppButtonToolCommand;
			if (cmd != null)
				RaiseNavigateForwardEvent((int)cmd.Tag);
		}
		
		private void RaiseNavigateBackEvent(int index) {
			if (this.OnNavigateBack != null)
				this.OnNavigateBack(this, new HistoryNavigationEventArgs(index));
		}
		private void RaiseNavigateForwardEvent(int index) {
			if (this.OnNavigateForward != null)
				this.OnNavigateForward(this, new HistoryNavigationEventArgs(index));
		}

		private void OnToolbarsManager_ToolClick(object sender, ToolClickEventArgs e) {
			this.mediator.Execute(e.Tool.Key);
		}
	}
	#endregion
	
	internal delegate void HistoryNavigationEventHandler(object sender, HistoryNavigationEventArgs e);
	
	/// <summary>
	/// Used as a parameter on the
	/// HistoryNavigationEventHandler delegate
	/// </summary>
	internal class HistoryNavigationEventArgs: EventArgs {
		
		#region ctor's
		public HistoryNavigationEventArgs(int index) {
			this.Index = index;
		}
		#endregion

		#region public properties
		public readonly int Index;
		#endregion

	}

}
