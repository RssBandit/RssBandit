using System;
using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace RssBandit.WinGui.Controls
{
	/// <summary>
	/// This control is used for browsing to a Directory and selecting it. 
	/// </summary>
	public class DirectoryBrowser: FolderNameEditor {

		#region private members

		/// <summary>
		/// The path that was chosen by the user on closing the dialog
		/// </summary>
		private string returnPath = String.Empty;

		/// <summary>
		/// The FolderBrowser instance used by this class. 
		/// </summary>
		private FolderBrowser fb = new FolderBrowser();

		#endregion
	
		#region public properties 

		/// <summary>
		/// The path to the Directory that was chosen by the user
		/// </summary>
		public string ReturnPath {
			get { return returnPath; }
		}
		
		/// <summary>
		/// Gets or sets the description displayed on top of the treeview.
		/// </summary>
		/// <value>The description.</value>
		public string Description {
			get { return fb.Description; }
			set { fb.Description = value; }
		}
		#endregion	

		#region Constructor

		/// <summary>
		/// Default constructor
		/// </summary>
		public DirectoryBrowser() { ;}

		#endregion	

		#region Private methods 
			
		/// <summary>
		/// Displays the dialog
		/// </summary>
		/// <returns>Returns the result of closing the dialog</returns>
		private DialogResult RunDialog() {
			
			fb.StartLocation = FolderBrowserFolder.MyComputer;
			fb.Style = FolderBrowserStyles.ShowTextBox;
			
			DialogResult r = fb.ShowDialog();

			if (r == DialogResult.OK)
				returnPath = fb.DirectoryPath;
			else
				returnPath = String.Empty;

			return r;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Shows the dialog
		/// </summary>
		/// <returns>The DialogResult on closing the dialog</returns>
		public DialogResult ShowDialog() {
			return RunDialog();
		}

		#endregion 

	} 
}
