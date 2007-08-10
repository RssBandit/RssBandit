#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Collections;
using System.Collections.Specialized;
using Infragistics.Win.UltraWinToolbars;
using RssBandit.AppServices;

namespace RssBandit.WinGui
{
	/// <summary>
	/// Impl. by a class, that is able to manage the real
	/// command related object instances (add, remove, etc).
	/// </summary>
	internal interface ICommandBarImplementationSupport
	{
		CommandBar GetToolBarInstance(string id);
		CommandBar GetMenuBarInstance(string id);
		CommandBar AddToolBar(string id);
		CommandBar AddMenuBar(string id);

		CommandBar GetContexMenuInstance(string id);
		CommandBar AddContextMenu(string id);
	}

	#region CommandBarManager
	/// <summary>
	/// CommandBarManager.
	/// </summary>
	internal class CommandBarManager: ICommandBarManager {
		internal ICommandBarImplementationSupport instanceContainer;
		internal CommandBarCollection collection;
		
		public CommandBarManager(ICommandBarImplementationSupport instanceContainer) {
			this.instanceContainer = instanceContainer;
			this.collection = new CommandBarCollection(this);
		}

		#region ICommandBarManager Members

		public ICommandBarCollection CommandBars {
			get {
				return this.collection;
			}
		}

		#endregion
	}
	#endregion

	internal class CommandBarCollection: ICommandBarCollection {

		private ListDictionary collection;
		private CommandBarManager owner;
			
		public CommandBarCollection(CommandBarManager owner) {
			this.owner = owner;
		}

		#region ICommandBarCollection Members

		public ICommandBar AddContextMenu(string identifier) {
			CommandBar c = owner.instanceContainer.AddContextMenu(identifier);
			return c;
		}

		public ICommandBar AddMenuBar(string identifier) {
			// TODO:  Add CommandBarCollection.AddMenuBar implementation
			return null;
		}

		public ICommandBar AddToolBar(string identifier) {
			return owner.instanceContainer.AddToolBar(identifier);
		}

		ICommandBar ICommandBarCollection.this[string identifier] {
			get {
//				foreach (ToolBar tb in owner.toolManager.GetToolBars()) {
//					TagIdentifier ti = tb.Tag as TagIdentifier;
//					if (ti != null && ti.Identifier == identifier)
//						return new CommandBar(tb);
//				}
				return null;
			}
		}


		public void Remove(ICommandBar commandBar) {
//			CommandBar cb = commandBar as CommandBar;
//			if (cb == null) return;
//			owner.toolManager.RemoveToolbar(cb.decoratedItem);
//			owner.toolContainer.Controls.Remove(cb.decoratedItem);
		}

		public void Remove(string identifier) {
			throw new NotImplementedException();
		}

		public bool Contains(ICommandBar commandBar) {
//			CommandBar cb = commandBar as CommandBar;
//			if (cb != null)
//				return owner.toolManager.FindToolbar(cb.decoratedItem.Guid) != null;
			return false;
		}

		public bool Contains(string identifier) {
			throw new NotImplementedException();
		}

		public void Clear() {
			// TODO:  nope
		}


		#endregion

		#region ICollection Members

		public bool IsSynchronized {
			get {
				// TODO:  Add CommandBarCollection.IsSynchronized getter implementation
				return false;
			}
		}

		public int Count {
			get {
				// TODO:  Add CommandBarCollection.Count getter implementation
				return 0;
			}
		}

		public void CopyTo(Array array, int index) {
			// TODO:  Add CommandBarCollection.CopyTo implementation
		}

		public object SyncRoot {
			get {
				// TODO:  Add CommandBarCollection.SyncRoot getter implementation
				return null;
			}
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator() {
			// TODO:  Add CommandBarCollection.GetEnumerator implementation
			return null;
		}

		#endregion

		#region ICommandBarCollection Members

		ICommandBar ICommandBarCollection.AddContextMenu(string identifier)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		ICommandBar ICommandBarCollection.AddMenuBar(string identifier)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		ICommandBar ICommandBarCollection.AddToolBar(string identifier)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void ICommandBarCollection.Clear()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool ICommandBarCollection.Contains(ICommandBar commandBar)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		bool ICommandBarCollection.Contains(string identifier)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void ICommandBarCollection.Remove(ICommandBar commandBar)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		void ICommandBarCollection.Remove(string identifier)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion

		#region ICollection Members

		void ICollection.CopyTo(Array array, int index)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		int ICollection.Count
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		bool ICollection.IsSynchronized
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		object ICollection.SyncRoot
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}

	/// <summary>
	/// Decorator for TD.SandBar.ToolBar
	/// </summary>
	internal class CommandBar: ICommandBar {
		
		internal UltraToolbarsManager decoratedItem;
		private TagIdentifier TagID;
		
		public CommandBar(UltraToolbarsManager decoratedItem) {
			this.decoratedItem = decoratedItem;
			this.TagID = new TagIdentifier(Guid.NewGuid().ToString("N"), null);
		}
			
		#region ICommandBar Members

		public string Identifier {
			get { return TagID.Identifier; }
			set { TagID.Identifier = value; }
		}

		public ICommandBarItemCollection Items {
			get { return new CommandBarItemCollection(decoratedItem.Tools); }
		}

		#endregion
	}

	/// <summary>
	/// Decorator for TD.SandBar.ToolBar.Items collection
	/// </summary>
	internal class CommandBarItemCollection: ICommandBarItemCollection {
			
		ToolsCollectionBase decoratedItem;
		public CommandBarItemCollection(ToolsCollectionBase decoratedItem) {
			this.decoratedItem = decoratedItem;
		}
		#region ICommandBarItemCollection Members

		public ICommandBarItem this[int index] {
			get {
				return new CommandBarItem(decoratedItem[index]);
			}
		}

		public void RemoveAt(int index) {
			// TODO:  Add CommandBarItemCollection.RemoveAt implementation
		}

		public void Insert(int index, ICommandBarItem value) {
			// TODO:  Add CommandBarItemCollection.Insert implementation
		}

		public void AddRange(ICollection values) {
			// TODO:  Add CommandBarItemCollection.AddRange implementation
		}

		public ICommandBarSeparator InsertSeparator(int index) {
			// TODO:  Add CommandBarItemCollection.InsertSeparator implementation
			return null;
		}

		public ICommandBarComboBox AddComboBox(string identifier, string caption) {
			// TODO:  Add CommandBarItemCollection.AddComboBox implementation
			return null;
		}

		public void Remove(ICommandBarItem item) {
			// TODO:  Add CommandBarItemCollection.Remove implementation
		}

		public bool Contains(ICommandBarItem item) {
			// TODO:  Add CommandBarItemCollection.Contains implementation
			return false;
		}

		public void Clear() {
			// TODO:  Add CommandBarItemCollection.Clear implementation
		}

		public int IndexOf(ICommandBarItem item) {
			// TODO:  Add CommandBarItemCollection.IndexOf implementation
			return 0;
		}

		public ICommandBarButton AddButton(string identifier, string caption, System.Drawing.Image image, ExecuteCommandHandler clickHandler, System.Windows.Forms.Keys keyBinding) {
			// TODO:  Add CommandBarItemCollection.AddButton implementation
			return null;
		}

		ICommandBarButton ICommandBarItemCollection.AddButton(string identifier, string caption, ExecuteCommandHandler clickHandler, System.Windows.Forms.Keys keyBinding) {
			// TODO:  Add CommandBarItemCollection.RssBandit.AppServices.ICommandBarItemCollection.AddButton implementation
			return null;
		}

		ICommandBarButton ICommandBarItemCollection.AddButton(string identifier, string caption, System.Drawing.Image image, ExecuteCommandHandler clickHandler) {
			// TODO:  Add CommandBarItemCollection.ICommandBarItemCollection.AddButton implementation
			return null;
		}

		ICommandBarButton ICommandBarItemCollection.AddButton(string identifier, string caption, ExecuteCommandHandler clickHandler) {
			// TODO:  Add CommandBarItemCollection.ICommandBarItemCollection.AddButton implementation
			return null;
		}

		public ICommandBarMenu InsertMenu(int index, string identifier, string caption) {
			// TODO:  Add CommandBarItemCollection.InsertMenu implementation
			return null;
		}

		public ICommandBarCheckBox InsertCheckButton(int index, string caption) {
			// TODO:  Add CommandBarItemCollection.InsertCheckButton implementation
			return null;
		}

		public ICommandBarButton InsertButton(int index, string caption, ExecuteCommandHandler clickHandler) {
			// TODO:  Add CommandBarItemCollection.InsertButton implementation
			return null;
		}

		public void Add(ICommandBarItem value) {
			// TODO:  Add CommandBarItemCollection.Add implementation
		}

		public ICommandBarSeparator AddSeparator() {
			// TODO:  Add CommandBarItemCollection.AddSeparator implementation
			return null;
		}

		public ICommandBarMenu AddMenu(string identifier, string caption, System.Drawing.Image image) {
			// TODO:  Add CommandBarItemCollection.AddMenu implementation
			return null;
		}

		ICommandBarMenu ICommandBarItemCollection.AddMenu(string identifier, string caption) {
			// TODO:  Add CommandBarItemCollection.ICommandBarItemCollection.AddMenu implementation
			return null;
		}

		public ICommandBarCheckBox AddCheckButton(string identifier, string caption, System.Drawing.Image image, System.Windows.Forms.Keys keyBinding) {
			// TODO:  Add CommandBarItemCollection.AddCheckButton implementation
			return null;
		}

		ICommandBarCheckBox ICommandBarItemCollection.AddCheckButton(string identifier, string caption, System.Windows.Forms.Keys keyBinding) {
			// TODO:  Add CommandBarItemCollection.ICommandBarItemCollection.AddCheckButton implementation
			return null;
		}

		ICommandBarCheckBox ICommandBarItemCollection.AddCheckButton(string identifier, string caption, System.Drawing.Image image) {
			// TODO:  Add CommandBarItemCollection.ICommandBarItemCollection.AddCheckButton implementation
			return null;
		}

		ICommandBarCheckBox ICommandBarItemCollection.AddCheckButton(string identifier, string caption) {
			// TODO:  Add CommandBarItemCollection.ICommandBarItemCollection.AddCheckButton implementation
			return null;
		}

		#endregion

		#region ICollection Members

		public bool IsSynchronized {
			get {
				// TODO:  Add CommandBarItemCollection.IsSynchronized getter implementation
				return false;
			}
		}

		public int Count {
			get {
				// TODO:  Add CommandBarItemCollection.Count getter implementation
				return 0;
			}
		}

		public void CopyTo(Array array, int index) {
			// TODO:  Add CommandBarItemCollection.CopyTo implementation
		}

		public object SyncRoot {
			get {
				// TODO:  Add CommandBarItemCollection.SyncRoot getter implementation
				return null;
			}
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator() {
			// TODO:  Add CommandBarItemCollection.GetEnumerator implementation
			return null;
		}

		#endregion
	}

	internal class CommandBarItem: ICommandBarItem {
		private ToolBase decoratedItem;
			
		public CommandBarItem(ToolBase decoratedItem) {
			this.decoratedItem = decoratedItem;
		}
		#region ICommandBarItem Members

		public System.Drawing.Image Image {
			get {
				// TODO:  Add CommandBarItem.Image getter implementation
				return null;
			}
			set {
				// TODO:  Add CommandBarItem.Image setter implementation
			}
		}

		public string Identifier {
			get {
				TagIdentifier ti = this.decoratedItem.Tag as TagIdentifier;
				if (ti != null) 
					return ti.Identifier;
				return null;
			}
		}

		public bool Visible {
			get { return decoratedItem.SharedProps.Visible; }
			set { decoratedItem.SharedProps.Visible = value; }
		}

		public object Tag {
			get {
				TagIdentifier ti = this.decoratedItem.Tag as TagIdentifier;
				if (ti != null) 
					return ti.Tag;
				return null;
			}
			set {
				TagIdentifier ti = this.decoratedItem.Tag as TagIdentifier;
				if (ti != null) 
					ti.Tag = value;
			}
		}

		public bool Enabled {
			get { return decoratedItem.SharedProps.Enabled; }
			set { decoratedItem.SharedProps.Enabled = value; }
		}

		public string Text {
			get { return decoratedItem.SharedProps.Caption; }
			set { decoratedItem.SharedProps.Caption = value; }
		}

		#endregion

	}

	/// <summary>
	/// Used to store a Tag AND the identifier at the Tag
	/// property of decorated items
	/// </summary>
	internal class TagIdentifier {
		public TagIdentifier(string id, object tag) {
			Identifier = id;
			Tag = tag;
		}
		public string Identifier;
		public object Tag;
	}
	
}

#region CVS Version Log
/*
 * $Log: CommandBarManager.cs,v $
 * Revision 1.3  2006/12/14 16:34:07  t_rendelmann
 * finished: all toolbar migrations; removed Sandbar toolbars from MainUI
 *
 * Revision 1.2  2006/10/31 13:36:35  t_rendelmann
 * fixed: various changes applied to make compile with CLR 2.0 possible without the hassle to convert it all the time again
 *
 */
#endregion
