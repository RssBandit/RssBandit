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
using System.Windows.Forms;
using RssBandit.Utility.Keyboard;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Utility;

namespace RssBandit.WinGui.Menus {

	#region AppMenuCommand class
//	/// <summary>
//	/// Colleage base Menu class, that is controlled by and talks to the mediator
//	/// </summary>
//	public class AppMenuCommand : MenuButtonItem, ICommand, ICommandComponent {
//		/// <summary>
//		/// Required designer variable.
//		/// </summary>
//		private Container components = null;
//		protected CommandMediator med;
//		protected event ExecuteCommandHandler OnExecute;
//		//protected object _tag;
//
//		public AppMenuCommand()	{
//			/// <summary>
//			/// Required for Windows.Forms Class Composition Designer support
//			/// </summary>
//			InitializeComponent();
//
//			//create default click handler
//			this.Activate += new EventHandler(this.ClickHandler);
//		}
//
//		public AppMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string description, ShortcutHandler shortcuts) : 
//			this(cmdId, mediator, executor, caption, description)	
//		{
//			SetShortcuts(cmdId, shortcuts);
//		}
//		public AppMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string description): 
//			this()	{
//			base.Text = caption;
//			base.ToolTipText = description;
//			
//			base.Tag = cmdId;
//			med = mediator;
//			OnExecute += executor;
//			med.RegisterCommand (cmdId, this);
//		}
//
//		public AppMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string description, int imageIndex): 
//			this(cmdId, mediator, executor ,caption, description)	{
//			base.ImageIndex = imageIndex;
//		}
//
//		public AppMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string description, int imageIndex, ShortcutHandler shortcuts): 
//			this(cmdId, mediator, executor ,caption, description, imageIndex)	
//		{
//			SetShortcuts(cmdId, shortcuts);
//		}
//		
//		private void SetShortcuts(string cmdId, ShortcutHandler shortcuts)
//		{
//			if(shortcuts != null)
//			{
//				this.Shortcut = shortcuts.GetShortcut(cmdId);
//			}
//		}
////		public object Tag {
////			get { return _tag; }
////			set { _tag = value;}
////		}
//
//		#region ICommandComponent implementation: abstract from the concrete Base class
//
//		public new bool Checked {
//			get { return base.Checked;  }
//			set { base.Checked = value; }
//		}
//
//		public new bool Enabled {
//			get { return base.Enabled;  }
//			set { base.Enabled = value; }
//		}
//
//		public new bool Visible {
//			get { return base.Visible;  }
//			set { base.Visible = value; }
//		}
//
//		#endregion
//
//		public void ClickHandler(object obj, EventArgs e) {
//			this.Execute();
//		}
//
//		/// <summary> 
//		/// Clean up any resources being used.
//		/// </summary>
//		protected override void Dispose( bool disposing )	{
//			if( disposing )	{
//				if(components != null) {
//					components.Dispose();
//				}
//			}
//			base.Dispose( disposing );
//		}
//
//		#region Component Designer generated code
//		/// <summary>
//		/// Required method for Designer support - do not modify
//		/// the contents of this method with the code editor.
//		/// </summary>
//		private void InitializeComponent() {
//			components = new System.ComponentModel.Container();
//		}
//		#endregion
//
//		#region Implementation of ICommand
//
//		public virtual void Execute()	{
//			if (OnExecute != null)
//				OnExecute(this);
//		}
//
//		public virtual void Initialize(){
//			// empty here
//		}
//
//		public string CommandID { get { return (string)base.Tag; } }
//
//		public CommandMediator Mediator{
//			get { return med;  }
//			set	{ med = value; }
//		}
//		#endregion
//
//	}

	#endregion

	#region AppContextMenuCommand class
	/// <summary>
	/// Colleage base Menu class, that is controlled by and talks to the mediator
	/// </summary>
	public class AppContextMenuCommand : ToolStripMenuItem, ICommand, ICommandComponent	{
	
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Container components = null;
		protected CommandMediator med;
		protected event ExecuteCommandHandler OnExecute;
        
        protected string description = String.Empty;
		protected int imageIndex;

		public AppContextMenuCommand()
		{
			
			// Required for Windows.Forms Class Composition Designer support
			
			InitializeComponent();

			//create default click handler
			EventHandler evh = ClickHandler;
			this.Click += evh;
		}

		public AppContextMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string description, ShortcutHandler shortcuts) : 
			this(cmdId, mediator, executor, caption, description)
		{
			SetShortcuts(cmdId, shortcuts);
		}

		public AppContextMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string description): 
			this()	{

			Text = caption;
			this.description = description;
			
			Tag = cmdId;
			med = mediator;
            if (executor != null)
            {
                OnExecute += executor;
                Executor = executor;
            }
			med.RegisterCommand (cmdId, this);
		}
		
		public AppContextMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string description, int imageIndex, ShortcutHandler shortcuts): 
			this(cmdId, mediator, executor, caption, description, imageIndex)
		{
			SetShortcuts(cmdId, shortcuts);
		}
		public AppContextMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string description, int imageIndex): 
			this(cmdId, mediator, executor, caption, description)	{
			this.imageIndex = imageIndex;
		}

		public AppContextMenuCommand(string cmdId, CommandMediator mediator, string caption, string description, ShortcutHandler shortcuts):  
			this(cmdId, mediator, null, caption, description)	
		{
			SetShortcuts(cmdId, shortcuts);
		}

		public AppContextMenuCommand(string cmdId, CommandMediator mediator, string caption, string description): 
			this(cmdId, mediator, null, caption, description)	{
		}
		
		public AppContextMenuCommand(string cmdId, CommandMediator mediator, string caption, string description, int imageIndex, ShortcutHandler shortcuts) : 
			this(cmdId, mediator, caption, description, shortcuts)
		{
			this.imageIndex = imageIndex;
		}

		private void SetShortcuts(string cmdId, ShortcutHandler shortcuts)
		{
			if(shortcuts != null)
			{
				this.ShortcutKeys = (Keys)shortcuts.GetShortcut(cmdId);
                this.ShowShortcutKeys = shortcuts.IsShortcutDisplayed(cmdId);
			}
		}


		#region ICommandComponent implementation: abstract from the concrete Base class

		public new bool Checked 	{
			get { return base.Checked;  }
			set	{ base.Checked = value; }
		}

		public new bool Enabled {
			get { return base.Enabled;  }
			set	{ base.Enabled = value; }
		}

		public new bool Visible {
			get { return base.Visible;  }
			set	{ base.Visible = value; }
		}

		public void ClickHandler(object obj, EventArgs e) {
			this.Execute();
		}
		#endregion

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )	{
			if( disposing )	{
				if(components != null) {
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			components = new System.ComponentModel.Container();
		}
		#endregion

		#region Implementation of ICommand

		public virtual void Execute()	{
			if (OnExecute != null)
				OnExecute(this);
		}

		public virtual void Initialize()	{
			// empty here
		}

		public string CommandID { get { return (string)Tag; } }

        public string Description => description;
        public ExecuteCommandHandler Executor { get; }

        public CommandMediator Mediator	{
			get { return med;  }
			set	{ med = value; }
		}

		#endregion
	}

	#endregion
	
}

#region CVS Version Log
/*
 * $Log: MenuCommands.cs,v $
 * Revision 1.15  2006/12/14 16:34:07  t_rendelmann
 * finished: all toolbar migrations; removed Sandbar toolbars from MainUI
 *
 * Revision 1.14  2006/11/28 18:08:40  t_rendelmann
 * changed; first version with the new menubar and the main toolbar migrated to IG - still work in progress
 *
 * Revision 1.13  2006/11/05 10:54:40  t_rendelmann
 * fixed: surrounded the small diffs between CLR 2.0 and CLR 1.1 with conditional compile defs.
 *
 * Revision 1.12  2006/11/05 01:23:55  carnage4life
 * Reduced time consuming locks in indexing code
 *
 * Revision 1.11  2006/10/31 13:36:35  t_rendelmann
 * fixed: various changes applied to make compile with CLR 2.0 possible without the hassle to convert it all the time again
 *
 */
#endregion
