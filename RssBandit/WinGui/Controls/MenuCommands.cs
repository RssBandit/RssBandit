#region CVS Version Header
/*
 * $Id: MenuCommands.cs,v 1.8 2005/01/29 16:14:58 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/01/29 16:14:58 $
 * $Revision: 1.8 $
 */
#endregion

using System;
using System.ComponentModel;
using System.Windows.Forms;
using RssBandit.Utility.Keyboard;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Utility;
using TD.SandBar;

namespace RssBandit.WinGui.Menus {

	#region AppMenuCommand class
	/// <summary>
	/// Colleage base Menu class, that is controlled by and talks to the mediator
	/// </summary>
	public class AppMenuCommand : MenuButtonItem, ICommand, ICommandComponent {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Container components = null;
		protected CommandMediator _med;
		protected event ExecuteCommandHandler OnExecute;
		//protected object _tag;

		public AppMenuCommand()	{
			/// <summary>
			/// Required for Windows.Forms Class Composition Designer support
			/// </summary>
			InitializeComponent();

			//create default click handler
			this.Activate += new EventHandler(this.ClickHandler);
		}

		public AppMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string captionResourceId, string descResourceId, ShortcutHandler shortcuts) : this(cmdId, mediator, executor, captionResourceId, descResourceId)	
		{
			SetShortcuts(cmdId, shortcuts);
		}
		public AppMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string captionResourceId, string descResourceId): this()	{
			string t = Resource.Manager[captionResourceId];
			string d = Resource.Manager[descResourceId];

			if (t == null) 
				t = captionResourceId;
			if (d == null) 
				d =descResourceId;

			base.Text = t;
			base.ToolTipText = d;
			
			base.Tag = cmdId;
			_med = mediator;
			OnExecute += executor;
			_med.RegisterCommand (cmdId, this);
		}

		public AppMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string captionResourceId, string descResourceId, int imageIndex): this(cmdId, mediator, executor ,captionResourceId, descResourceId)	{
			base.ImageIndex = imageIndex;
		}

		public AppMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string captionResourceId, string descResourceId, int imageIndex, ShortcutHandler shortcuts): this(cmdId, mediator, executor ,captionResourceId, descResourceId, imageIndex)	
		{
			SetShortcuts(cmdId, shortcuts);
		}
		
		private void SetShortcuts(string cmdId, ShortcutHandler shortcuts)
		{
			if(shortcuts != null)
			{
				this.Shortcut = shortcuts.GetShortcut(cmdId);
			}
		}
//		public object Tag {
//			get { return _tag; }
//			set { _tag = value;}
//		}

		#region ICommandComponent implementation: abstract from the concrete Base class

		public new bool Checked {
			get { return base.Checked;  }
			set { base.Checked = value; }
		}

		public new bool Enabled {
			get { return base.Enabled;  }
			set { base.Enabled = value; }
		}

		public new bool Visible {
			get { return base.Visible;  }
			set { base.Visible = value; }
		}

		#endregion

		public void ClickHandler(object obj, EventArgs e) {
			this.Execute();
		}

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

		public virtual void Initialize(){
			// empty here
		}

		public string CommandID { get { return (string)base.Tag; } }

		public CommandMediator Mediator{
			get { return _med;  }
			set	{ _med = value; }
		}
		#endregion

	}

	#endregion

	#region AppContextMenuCommand class
	/// <summary>
	/// Colleage base Menu class, that is controlled by and talks to the mediator
	/// </summary>
	public class AppContextMenuCommand : MenuItem, ICommand, ICommandComponent	{
	
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Container components = null;
		protected CommandMediator _med;
		protected event ExecuteCommandHandler OnExecute;
		protected string _description = String.Empty;
		protected int _imageIndex;
		private object tag = null;

		public AppContextMenuCommand():base()	{
			/// <summary>
			/// Required for Windows.Forms Class Composition Designer support
			/// </summary>
			InitializeComponent();

			//create default click handler
			EventHandler evh = new EventHandler (this.ClickHandler);
			this.Click += evh;
		}

		public AppContextMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string captionResourceId, string descResourceId, ShortcutHandler shortcuts) : this(cmdId, mediator, executor, captionResourceId, descResourceId)
		{
			SetShortcuts(cmdId, shortcuts);
		}

		public AppContextMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string captionResourceId, string descResourceId): this()	{
			string _text = Resource.Manager[captionResourceId];
			
			if (_text == null) 
				_text = captionResourceId;
			base.Text = _text;

			_description = Resource.Manager[descResourceId];
			if (_description == null) 
				_description =descResourceId;
			
			Tag = cmdId;
			_med = mediator;
			OnExecute += executor;
			_med.RegisterCommand (cmdId, this);
		}
		
		public AppContextMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string captionResourceId, string descResourceId, int imageIndex, ShortcutHandler shortcuts): this(cmdId, mediator, executor, captionResourceId, descResourceId)
		{
			SetShortcuts(cmdId, shortcuts);
		}
		public AppContextMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string captionResourceId, string descResourceId, int imageIndex): this(cmdId, mediator, executor, captionResourceId, descResourceId)	{
			_imageIndex = imageIndex;
		}

		public AppContextMenuCommand(string cmdId, CommandMediator mediator,string caption, string desc, ShortcutHandler shortcuts):  this()	
		{
			SetShortcuts(cmdId, shortcuts);
		}

		public AppContextMenuCommand(string cmdId, CommandMediator mediator, string caption, string desc): this()	{
			base.Text = caption;
			_description = desc;
			Tag = cmdId;
			_med = mediator;
			_med.RegisterCommand (cmdId, this);
		}
		
		public AppContextMenuCommand(string cmdId, CommandMediator mediator, string caption, string desc, int imageIndex, ShortcutHandler shortcuts) : this(cmdId, mediator, caption, desc, shortcuts)
		{
			_imageIndex = imageIndex;
		}

		private void SetShortcuts(string cmdId, ShortcutHandler shortcuts)
		{
			if(shortcuts != null)
			{
				this.Shortcut = shortcuts.GetShortcut(cmdId);
				this.ShowShortcut = shortcuts.IsShortcutDisplayed(cmdId);
			}
		}

		public object Tag {
			get { return tag; }
			set { tag = value; }
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

		public CommandMediator Mediator	{
			get { return _med;  }
			set	{ _med = value; }
		}

		#endregion
	}

	#endregion

}
