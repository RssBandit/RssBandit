using System;
using System.ComponentModel; 
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace RssBandit.AppServices
{
	/// <summary>
	/// Summary description for IWindowManager.
	/// </summary>
	public interface IDocumentWindowManager
	{
		// Events
		event CancelEventHandler Closing;
		event EventHandler Closed;
		event EventHandler Load;

		// Methods
		void Activate();
		void Close();
		void ShowMessage(string message);

		// Properties
		ICommandBarManager CommandBarManager { get; set; }
		Control Content { get; set; }
		bool Visible { get; set; }
		IDocumentWindowCollection Windows { get; }

	}

	public interface IDocumentWindowCollection : ICollection, IEnumerable {
		// Methods
		IDocumentWindow Add(string identifier, Control content, string caption);
		IDocumentWindow Add(string identifier, Control content, string caption, Image image);
		void Remove(string identifier);

		// Properties
		IDocumentWindow this[string identifier] { get; }
	}

	public interface IDocumentWindow {
		// Properties
		string Caption { get; set; }
		Image Image { get; set; }
		Control Content { get; }
		bool Visible { get; set; }
	}

}
