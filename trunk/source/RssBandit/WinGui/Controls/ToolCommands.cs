#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Infragistics.Win;
using Infragistics.Win.UltraWinToolbars;
using RssBandit.Utility.Keyboard;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Utility;

namespace RssBandit.WinGui.Tools
{
	#region AppToolCommand class
//	/// <summary>
//	/// Colleage base Toolbar Command class, that is controlled by and talks to the mediator
//	/// </summary>
//	public class AppToolCommand : TD.SandBar.ButtonItem, ICommand, ICommandComponent
//	{
//		/// <summary>
//		/// Required designer variable.
//		/// </summary>
//		private System.ComponentModel.Container components = null;
//		protected CommandMediator med;
//		protected event ExecuteCommandHandler OnExecute;
//		//public object Tag;
//
//		public AppToolCommand()
//		{
//			/// <summary>
//			/// Required for Windows.Forms Class Composition Designer support
//			/// </summary>
//			InitializeComponent();
//
//		}
//
//		public AppToolCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string description): 
//			this() {
//
//			base.Text = caption;
//			base.ToolTipText = description;
//			
//			base.Tag = cmdId;
//			med = mediator;
//			OnExecute += executor;
//			med.RegisterCommand (cmdId, this);
//		}
//		public AppToolCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string captionResourceId, string descResourceId, int imageIndex): this(cmdId, mediator, executor, captionResourceId, descResourceId) {
//			this.ImageIndex = imageIndex;
//		}
//
//		#region ICommandComponent implementation: abstract from the concrete Base class
//
//		public new bool Checked 
//		{
//			get { return base.Checked;  }
//			set	{ base.Checked = value; }
//		}
//
//		public new bool Enabled 
//		{
//			get { return base.Enabled;  }
//			set	{ base.Enabled = value; }
//		}
//
//		public new bool Visible 
//		{
//			get { return base.Visible;  }
//			set	{ base.Visible = value; }
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
//		protected override void Dispose( bool disposing )
//		{
//			if( disposing )
//			{
//				if(components != null)
//				{
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
//		private void InitializeComponent()
//		{
//			components = new System.ComponentModel.Container();
//		}
//		#endregion
//
//		#region Implementation of ICommand
//
//		public virtual void Execute()
//		{
//			if (OnExecute != null)
//				OnExecute(this);
//		}
//
//		public virtual void Initialize()
//		{
//			// empty here
//		}
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

	#region AppToolMenuCommand class
//	/// <summary>
//	/// Colleage base Menu class, that is controlled by and talks to the mediator
//	/// </summary>
//	public class AppToolMenuCommand : TD.SandBar.DropDownMenuItem, ICommand, ICommandComponent
//	{
//		/// <summary>
//		/// Required designer variable.
//		/// </summary>
//		private System.ComponentModel.Container components = null;
//		protected CommandMediator med;
//		protected event ExecuteCommandHandler OnExecute;
//		//public object Tag;
//
//		public AppToolMenuCommand()
//		{
//			/// <summary>
//			/// Required for Windows.Forms Class Composition Designer support
//			/// </summary>
//			InitializeComponent();
//
//		}
//
//		public AppToolMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string description): 
//			this()
//		{
//
//			base.Text = caption;
//			base.ToolTipText = description;
//			
//			base.Tag = cmdId;
//			OnExecute += executor;
//			med = mediator;
//			med.RegisterCommand (cmdId, this);
//		}
//		public AppToolMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string captionResourceId, string descResourceId, int imageIndex): this(cmdId, mediator, executor, captionResourceId, descResourceId)
//		{
//			this.ImageIndex = imageIndex;
//		}
//
//		#region ICommandComponent implementation: abstract from the concrete Base class
//
//		public new bool Checked 
//		{
//			get { return base.Checked;  }
//			set	{ base.Checked = value; }
//		}
//
//		public new bool Enabled 
//		{
//			get { return base.Enabled;  }
//			set	{ base.Enabled = value; }
//		}
//
//		public new bool Visible 
//		{
//			get { return base.Visible;  }
//			set	{ base.Visible = value; }
//		}
//
//		public void ClickHandler(object obj, EventArgs e) 
//		{
//			this.Execute();
//		}
//		#endregion
//
//		/// <summary> 
//		/// Clean up any resources being used.
//		/// </summary>
//		protected override void Dispose( bool disposing )
//		{
//			if( disposing )
//			{
//				if(components != null)
//				{
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
//		private void InitializeComponent()
//		{
//			components = new System.ComponentModel.Container();
//		}
//		#endregion
//
//		#region Implementation of ICommand
//
//		public virtual void Execute()
//		{
//			if (OnExecute != null)
//				OnExecute(this);
//		}
//
//		public virtual void Initialize()
//		{
//			// empty here
//		}
//
//		public string CommandID { get { return (string)base.Tag; } }
//
//		public CommandMediator Mediator{
//			get { return med;  }
//			set	{ med = value; }
//		}
//
//		#endregion
//	}

	#endregion

	#region AppPopupMenuCommand class
	/// <summary>
	/// Colleage base Popup Menu class, that is controlled by and talks to the mediator
	/// </summary>
	/// <remarks>
	/// A sample application provided by IG named "ToolProvider Component CS"
	/// shows how to correctly implement custom (inherited) tool classes!
	/// 
	/// we don't need a type converter here, because we don't have custom properties
	/// </remarks>
	[Serializable]
	public class AppPopupMenuCommand : Infragistics.Win.UltraWinToolbars.PopupMenuTool, ICommand, ICommandComponent {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		protected CommandMediator med;
		protected event ExecuteCommandHandler OnExecute;

		public AppPopupMenuCommand(string key):base(key) {
			// Required for Windows.Forms Class Composition Designer support
			InitializeComponent();
		}
		
		/// <summary>
		/// Constructor used for de-serialization
		/// </summary>
		protected AppPopupMenuCommand(SerializationInfo info, StreamingContext context) : 
			base (info, context) {
		}

		public AppPopupMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string description): 
			this(cmdId) {

			base.SharedProps.Caption = caption;
			base.SharedProps.StatusText = description;
			
			OnExecute += executor;
			med = mediator;
			med.RegisterCommand (cmdId, this);
		}
		
		public AppPopupMenuCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string desciption, int imageIndex): 
			this(cmdId, mediator, executor, caption, desciption) {
			Infragistics.Win.Appearance a = new Infragistics.Win.Appearance();
			a.Image = imageIndex;
			base.SharedProps.AppearancesSmall.Appearance = a;
		}

		/// <summary>
		/// Takes over the mediator and executor from cmd.
		/// </summary>
		/// <param name="cmd">The CMD.</param>
		public void ReJoinMediatorFrom(AppPopupMenuCommand cmd) {
			this.Mediator = cmd.Mediator;
			this.OnExecute = cmd.OnExecute;
		}
		
		#region Base Class Overrides

		#region Clone
		/// <summary>
		/// Returns a cloned copy of the tool.
		/// </summary>
		/// <param name="cloneNewInstance">If true, returns a clone of the tool that can serve as an instance of the tool, sharing the same SharedProps object.  If false, returns a tool that can be used as a new tool, with a clone of the original SharedProps object.</param>
		/// <returns></returns>
		protected override Infragistics.Win.UltraWinToolbars.ToolBase Clone(bool cloneNewInstance) {
			AppPopupMenuCommand tool = new AppPopupMenuCommand(this.Key);
			tool.InitializeFrom(this, cloneNewInstance);
			return tool;
		}
		#endregion //Clone

		#region GetObjectData

		/// <summary>
		/// Called from our base class when ISerializable.GetObjectData is called.  Serialize all custom property data here.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		protected override void GetObjectData(SerializationInfo info, StreamingContext context) {
			// Call the base implementation
			base.GetObjectData(info, context);
		}

		#endregion GetObjectData

		#region Initialize

		/// <summary>
		/// Internal inherited method for initializing the tool when de-serialization completes.
		/// Automatically called by toolbars manager.
		/// </summary>
		protected override void Initialize(ToolsCollectionBase parentCollection) {
			// Let the base do its processing.
			base.Initialize(parentCollection);
		}

		#endregion Initialize

		#endregion Base Class Overrides

		#region ICommandComponent implementation: abstract from the concrete Base class

		public new bool Checked {
			get { return base.Checked;  }
			set	{ base.Checked = value; }
		}

		public bool Enabled {
			get { return base.SharedProps.Enabled;  }
			set	{ base.SharedProps.Enabled = value; }
		}

		public bool Visible {
			get { return base.SharedProps.Visible;  }
			set	{ base.SharedProps.Visible = value; }
		}

		public void ClickHandler(object obj, EventArgs e) {
			this.Execute();
		}
		#endregion

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected void Dispose( bool disposing ) {
			if( disposing ) {
				if(components != null) {
					components.Dispose();
				}
				this.Mediator = null;
				this.OnExecute = null;
			}
			base.Dispose();
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

		public virtual void Execute() {
			if (OnExecute != null)
				OnExecute(this);
		}

		public virtual void Initialize() {
			// empty here
		}

		public string CommandID { get { return base.Key; } }

		public CommandMediator Mediator{
			get { return med;  }
			set	{ med = value; }
		}

		#endregion
	}

	#endregion
	
	#region AppButtonToolCommand class
	/// <summary>
	/// Colleage base Tool Button class, that is controlled by and talks to the mediator
	/// </summary>
	/// <remarks>
	/// A sample application provided by IG named "ToolProvider Component CS"
	/// shows how to correctly implement custom (inherited) tool classes!
	/// </remarks>
	[Serializable]
	public class AppButtonToolCommand : Infragistics.Win.UltraWinToolbars.ButtonTool, ICommand, ICommandComponent {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		protected CommandMediator med;
		internal protected event ExecuteCommandHandler OnExecute;

		#region ctor's

		public AppButtonToolCommand(string key):base(key) {
			
			// Required for Windows.Forms Class Composition Designer support
			
			InitializeComponent();
		}
		/// <summary>
		/// Constructor used for de-serialization
		/// </summary>
		protected AppButtonToolCommand(SerializationInfo info, StreamingContext context) : 
			base (info, context) {
			}
		
		public AppButtonToolCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string description): 
			this(cmdId) {

			base.SharedProps.Caption = caption;
			base.SharedProps.StatusText = description;
			
			OnExecute += executor;
			med = mediator;
			med.RegisterCommand (cmdId, this);
			}
		public AppButtonToolCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string desciption, int imageIndex): 
			this(cmdId, mediator, executor, caption, desciption) {
			Appearance a = new Appearance();
			a.Image = imageIndex;
			base.SharedProps.AppearancesSmall.Appearance = a;
			}
		
		public AppButtonToolCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string description, ShortcutHandler shortcuts): 
			this(cmdId, mediator, executor ,caption, description) {
			SetShortcuts(cmdId, shortcuts);
			}
		
		public AppButtonToolCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string description, int imageIndex, ShortcutHandler shortcuts): 
			this(cmdId, mediator, executor ,caption, description, imageIndex) {
			SetShortcuts(cmdId, shortcuts);
			}

		#endregion

		/// <summary>
		/// Takes over the mediator and executor from cmd.
		/// </summary>
		/// <param name="cmd">The CMD.</param>
		public void ReJoinMediatorFrom(AppButtonToolCommand cmd) {
			this.Mediator = cmd.Mediator;
			this.OnExecute = cmd.OnExecute;
		}
		
		#region Base Class Overrides

		#region Clone
		/// <summary>
		/// Returns a cloned copy of the tool.
		/// </summary>
		/// <param name="cloneNewInstance">If true, returns a clone of the tool that can serve as an instance of the tool, sharing the same SharedProps object.  If false, returns a tool that can be used as a new tool, with a clone of the original SharedProps object.</param>
		/// <returns></returns>
		protected override Infragistics.Win.UltraWinToolbars.ToolBase Clone(bool cloneNewInstance) {
			AppButtonToolCommand tool = new AppButtonToolCommand(this.Key);
			tool.InitializeFrom(this, cloneNewInstance);
			return tool;
		}
		#endregion //Clone

		#region GetObjectData

		/// <summary>
		/// Called from our base class when ISerializable.GetObjectData is called.  Serialize all custom property data here.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		protected override void GetObjectData(SerializationInfo info, StreamingContext context) {
			// Call the base implementation
			base.GetObjectData(info, context);
		}

		#endregion GetObjectData

		#region Initialize

		/// <summary>
		/// Internal inherited method for initializing the tool when de-serialization completes.
		/// Automatically called by toolbars manager.
		/// </summary>
		protected override void Initialize(ToolsCollectionBase parentCollection) {
			// Let the base do its processing.
			base.Initialize(parentCollection);
		}

		#endregion Initialize

		#endregion Base Class Overrides
		
		#region ICommandComponent implementation: abstract from the concrete Base class

		public bool Checked {
			get { throw new NotImplementedException();  }
			set	{ throw new NotImplementedException(); }
		}

		public bool Enabled {
			get { return base.SharedProps.Enabled;  }
			set	{ base.SharedProps.Enabled = value; }
		}

		public bool Visible {
			get { return base.SharedProps.Visible;  }
			set	{ base.SharedProps.Visible = value; }
		}

		public void ClickHandler(object obj, EventArgs e) {
			this.Execute();
		}
		#endregion
		
		private void SetShortcuts(string cmdId, ShortcutHandler shortcuts) {
			if(shortcuts != null) {
				this.SharedProps.Shortcut = shortcuts.GetShortcut(cmdId);
			}
		}
		
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected void Dispose( bool disposing ) {
			if( disposing ) {
				if(components != null) {
					components.Dispose();
				}
				this.Mediator = null;
				this.OnExecute = null;
			}
			base.Dispose();
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

		public virtual void Execute() {
			if (OnExecute != null)
				OnExecute(this);
		}

		public virtual void Initialize() {
			// empty here
		}

		public string CommandID { get { return base.Key; } }

		public CommandMediator Mediator{
			get { return med;  }
			set	{ med = value; }
		}

		#endregion
	}

	#endregion
	
	#region AppStateButtonToolCommand class
	/// <summary>
	/// Colleage base Tool Button class, that is controlled by and talks to the mediator
	/// </summary>
	/// <remarks>
	/// A sample application provided by IG named "ToolProvider Component CS"
	/// shows how to correctly implement custom (inherited) tool classes!
	/// </remarks>
	[Serializable]
	public class AppStateButtonToolCommand : Infragistics.Win.UltraWinToolbars.StateButtonTool, ICommand, ICommandComponent {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		protected CommandMediator med;
		protected event ExecuteCommandHandler OnExecute;

		#region ctor's

		public AppStateButtonToolCommand(string key):base(key) {
			
			// Required for Windows.Forms Class Composition Designer support
			
			InitializeComponent();
			base.MenuDisplayStyle = StateButtonMenuDisplayStyle.DisplayCheckmark;
		}
		
		/// <summary>
		/// Constructor used for de-serialization
		/// </summary>
		protected AppStateButtonToolCommand(SerializationInfo info, StreamingContext context) : base (info, context) {
//			foreach(SerializationEntry entry in info) {
//				switch (entry.Name) {
//					case "CustomProp":
//						this.SerializedProps.customProp = (string)Utils.DeserializeProperty(entry, typeof(string), this.SerializedProps.customProp);
//						break;
//				}
//			}		
		}
		
		public AppStateButtonToolCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string description): 
			this(cmdId) {

			base.SharedProps.Caption = caption;
			base.SharedProps.StatusText = description;
			
			OnExecute += executor;
			med = mediator;
			med.RegisterCommand (cmdId, this);
			}
		
		public AppStateButtonToolCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string description, int imageIndex, ShortcutHandler shortcuts): 
			this(cmdId, mediator, executor ,caption, description) 
		{
			Appearance a = new Appearance();
			a.Image = imageIndex;
			base.SharedProps.AppearancesSmall.Appearance = a;
			base.MenuDisplayStyle = StateButtonMenuDisplayStyle.DisplayToolImage;
			SetShortcuts(cmdId, shortcuts);
		}
		
		public AppStateButtonToolCommand(string cmdId, CommandMediator mediator, ExecuteCommandHandler executor, string caption, string description, ShortcutHandler shortcuts): 
			this(cmdId, mediator, executor ,caption, description) 
		{
			SetShortcuts(cmdId, shortcuts);
		}

		#endregion

		/// <summary>
		/// Takes over the mediator and executor from cmd.
		/// </summary>
		/// <param name="cmd">The CMD.</param>
		public void ReJoinMediatorFrom(AppStateButtonToolCommand cmd) {
			this.Mediator = cmd.Mediator;
			this.OnExecute = cmd.OnExecute;
		}
		
		#region Base Class Overrides

		#region Clone
		/// <summary>
		/// Returns a cloned copy of the tool.
		/// </summary>
		/// <param name="cloneNewInstance">If true, returns a clone of the tool that can serve as an instance of the tool, sharing the same SharedProps object.  If false, returns a tool that can be used as a new tool, with a clone of the original SharedProps object.</param>
		/// <returns></returns>
		protected override Infragistics.Win.UltraWinToolbars.ToolBase Clone(bool cloneNewInstance) {
			AppStateButtonToolCommand tool = new AppStateButtonToolCommand(this.Key);
			tool.InitializeFrom(this, cloneNewInstance);
			return tool;
		}
		#endregion //Clone

		#region GetObjectData

		/// <summary>
		/// Called from our base class when ISerializable.GetObjectData is called.  Serialize all custom property data here.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		protected override void GetObjectData(SerializationInfo info, StreamingContext context) {
			// Call the base implementation
			base.GetObjectData(info, context);
		}

		#endregion GetObjectData

		#region Initialize

		/// <summary>
		/// Internal inherited method for initializing the tool when de-serialization completes.
		/// Automatically called by toolbars manager.
		/// </summary>
		protected override void Initialize(ToolsCollectionBase parentCollection) {
			// Let the base do its processing.
			base.Initialize(parentCollection);
		}

		#endregion Initialize

		#endregion Base Class Overrides
		
		#region ICommandComponent implementation: abstract from the concrete Base class

// we use the impl. of the base class:		
//		public new bool Checked {
//			get { return base.Checked;  }
//			set {
//				// does not cause a ToolClick event:
//				base.InitializeChecked(value);
//			}
//		}

		public bool Enabled {
			get { return base.SharedProps.Enabled;  }
			set	{ base.SharedProps.Enabled = value; }
		}

		public bool Visible {
			get { return base.SharedProps.Visible;  }
			set	{ base.SharedProps.Visible = value; }
		}

		public void ClickHandler(object obj, EventArgs e) {
			this.Execute();
		}
		#endregion
		
		private void SetShortcuts(string cmdId, ShortcutHandler shortcuts) {
			if(shortcuts != null) {
				this.SharedProps.Shortcut = shortcuts.GetShortcut(cmdId);
			}
		}
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected void Dispose( bool disposing ) {
			if( disposing ) {
				if(components != null) {
					components.Dispose();
				}
				this.Mediator = null;
				this.OnExecute = null;
			}
			base.Dispose();
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

		public virtual void Execute() {
			if (OnExecute != null)
				OnExecute(this);
		}

		public virtual void Initialize() {
			// empty here
		}

		public string CommandID { get { return base.Key; } }

		public CommandMediator Mediator{
			get { return med;  }
			set	{ med = value; }
		}

		#endregion
	}

	#endregion
}
