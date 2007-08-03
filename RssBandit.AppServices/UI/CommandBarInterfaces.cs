using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace RssBandit.AppServices
{
	/// <summary>
	/// ICommandBarManager.
	/// </summary>
	public interface ICommandBarManager
	{
		ICommandBarCollection CommandBars { get; } 
	}

	public interface ICommandBarCollection : ICollection, IEnumerable {
		// Methods
		ICommandBar AddContextMenu(string identifier);
		ICommandBar AddMenuBar(string identifier);
		ICommandBar AddToolBar(string identifier);
		void Clear();
		bool Contains(ICommandBar commandBar);
		bool Contains(string identifier);
		void Remove(ICommandBar commandBar);
		void Remove(string identifier);

		// Properties
		ICommandBar this[string identifier] { get; }
	}

	public interface ICommandBar {
		// Properties
		string Identifier { get; set; }
		ICommandBarItemCollection Items { get; }
	}

	public interface ICommandBarItemCollection : ICollection, IEnumerable {
		// Methods
		void Add(ICommandBarItem value);
		ICommandBarButton AddButton(string identifier, string caption, ExecuteCommandHandler clickHandler);
		ICommandBarButton AddButton(string identifier, string caption, Image image, ExecuteCommandHandler clickHandler);
		ICommandBarButton AddButton(string identifier, string caption, ExecuteCommandHandler clickHandler, Keys keyBinding);
		ICommandBarButton AddButton(string identifier, string caption, Image image, ExecuteCommandHandler clickHandler, Keys keyBinding);
		ICommandBarCheckBox AddCheckButton(string identifier, string caption);
		ICommandBarCheckBox AddCheckButton(string identifier, string caption, Image image);
		ICommandBarCheckBox AddCheckButton(string identifier, string caption, Keys keyBinding);
		ICommandBarCheckBox AddCheckButton(string identifier, string caption, Image image, Keys keyBinding);
		ICommandBarComboBox AddComboBox(string identifier, string caption);
		ICommandBarMenu AddMenu(string identifier, string caption);
		ICommandBarMenu AddMenu(string identifier, string caption, Image image);
		void AddRange(ICollection values);
		ICommandBarSeparator AddSeparator();
		void Clear();
		bool Contains(ICommandBarItem item);
		int IndexOf(ICommandBarItem item);
		void Insert(int index, ICommandBarItem value);
		ICommandBarButton InsertButton(int index, string caption, ExecuteCommandHandler clickHandler);
		ICommandBarCheckBox InsertCheckButton(int index, string caption);
		ICommandBarMenu InsertMenu(int index, string identifier, string caption);
		ICommandBarSeparator InsertSeparator(int index);
		void Remove(ICommandBarItem item);
		void RemoveAt(int index);

		// Properties
		ICommandBarItem this[int index] { get; }
	}

	/*
	/// <summary>
	/// Form elements that can send commands have to implement ICommand
	/// </summary>
	interface ICommand {
		void Execute();	// now ICommandBarControl.PerformClick()
		string CommandID { get; }	// now ICommandBarItem.Identifier
		ICommandMediator Mediator { get ; }	// now in ExecuteCommandEventArgs
	}
*/
	public interface ICommandBarItem {
		// Properties
		bool Enabled { get; set; }
		Image Image { get; set; }
		string Text { get; set; }
		object Tag { get; set; }
		bool Visible { get; set; }
		string Identifier { get; }
	}
	public interface ICommandBarControl : ICommandBarItem {
		event ExecuteCommandHandler Click;
		void PerformClick();
	}

	public interface ICommandBarButton : ICommandBarControl, ICommandBarItem {
		Keys KeyBinding { get; set; }
	}

	public interface ICommandBarCheckBox : ICommandBarButton, ICommandBarControl, ICommandBarItem {
		bool Checked { get; set; }
	}

	public interface ICommandBarComboBox : ICommandBarControl, ICommandBarItem {
		ComboBox ComboBox { get; }
	}

	public interface ICommandBarMenu : ICommandBarItem, ICommandBar {
		event EventHandler DropDown;
	}

	public interface ICommandBarSeparator : ICommandBarItem {
	}

	public interface ICommandMediator {
		void SetEnabled(params string[] identifierArgs);
		void SetDisabled(params string[] identifierArgs);
		bool IsEnabled(string identifier);
		void SetVisible(params string[] identifierArgs);
		void SetInvisible(params string[] identifierArgs);
		bool IsVisible(string identifier);
		void SetChecked(params string[] identifierArgs);
		void SetUncheck(params string[] identifierArgs);
		bool IsChecked(string identifier);
	}

	/// <summary>
	/// Delegate used to callback to mediator
	/// </summary>
	public delegate void ExecuteCommandHandler(object sender, ExecuteCommandEventArgs e);

	public class ExecuteCommandEventArgs {
		
		public ExecuteCommandEventArgs(ICommandMediator mediator) {
			this.mediator = mediator;
		}
		private readonly ICommandMediator mediator;
		ICommandMediator Mediator { get { return this.mediator; } }

	}
}
