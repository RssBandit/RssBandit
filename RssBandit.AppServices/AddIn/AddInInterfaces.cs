using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace RssBandit.UIServices
{

	/// <summary>
	/// IAddInPackage have to be implemented by a external AddIn to 
	/// be recognized as a RSS Bandit application AddIn.
	/// </summary>
	public interface IAddInPackage
	{
		/// <summary>
		/// Called on loading an AddInPackage.
		/// </summary>
		/// <param name="serviceProvider">IServiceProvider</param>
		void Load(IServiceProvider serviceProvider);
		/// <summary>
		/// Called on unloading an AddInPackage. Use it for cleanup task(s).
		/// </summary>
		void Unload();
	}

	/// <summary>
	/// Interface to be implemented by the same class that implement 
	/// <see cref="IAddInPackage">IAddInPackage</see> or at least one of
	/// the classes that implement IAddInPackage in a AddIn DLL  to
	/// allow users to modify the specific AddIn configuration.
	/// </summary>
	public interface IAddInPackageConfiguration
	{
		/// <summary>
		/// Gets a value indicating whether this instance has a configuration 
		/// UI (User interface).
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance has configuration UI; otherwise, <c>false</c>.
		/// </value>
		bool HasConfigurationUI { get; }
		/// <summary>
		/// Displays the configuration UI (User interface).
		/// </summary>
		/// <param name="parent">The parent window.</param>
		void ShowConfigurationUI(IWin32Window parent);
	}

	/// <summary>
	/// IAddIn represents a AddIn within the parent application.
	/// </summary>
	public interface IAddIn: IDisposable 
	{
		/// <summary>
		/// Gets the location of the AddIn.
		/// </summary>
		/// <value>The location.</value>
		string Location { get; }
		/// <summary>
		/// Gets the name of the AddIn.
		/// </summary>
		/// <value>The name.</value>
		string Name { get; }
		/// <summary>
		/// Gets the AddIn packages collection.
		/// </summary>
		/// <value>The add in packages.</value>
        IList<IAddInPackage> AddInPackages { get; }
	}


	/// <summary>
	/// The AddIn manager as it is used within the parent application.
	/// </summary>
	public interface IAddInManager 
	{
		/// <summary>
		/// Loads the IAddIn from the specified file name.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns></returns>
		IAddIn Load(string fileName);
		/// <summary>
		/// Unloads the specified addIn.
		/// </summary>
		/// <param name="addIn">The add in.</param>
		void Unload(IAddIn addIn);
		/// <summary>
		/// Gets the AddIns collection.
		/// </summary>
		/// <value>The add ins.</value>
        IEnumerable<IAddIn> AddIns { get; }
	}

} //namespace PROCOS.WinControls.CoreGUI.AddIn
