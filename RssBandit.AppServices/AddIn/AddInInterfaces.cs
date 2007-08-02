using System;
using System.Collections;
using System.ComponentModel;

namespace RssBandit.UIServices
{

	/// <summary>
	/// Summary description for IAddInPackage.
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
	/// A collection of IAddInPackage objects
	/// </summary>
	public interface IAddInPackageCollection : ICollection, IEnumerable 
	{
		/// <summary>
		/// Indexer
		/// </summary>
		IAddInPackage this[int index] { get; }
	}

	/// <summary>
	/// IAddIn represents a AddIn.
	/// </summary>
	public interface IAddIn: IDisposable 
	{
		string Location { get; }
		string Name { get; }
		IAddInPackageCollection AddInPackages { get; }
	}

	/// <summary>
	/// Interface for a Collection of AddIn's
	/// </summary>
	public interface IAddInCollection: ICollection, IEnumerable 
	{
		IAddIn this[int index] { get; }
	}

	public interface IAddInManager 
	{
		IAddIn Load(string fileName);
		void Unload(IAddIn addIn);
		IAddInCollection AddIns { get; }
	}

} //namespace PROCOS.WinControls.CoreGUI.AddIn
