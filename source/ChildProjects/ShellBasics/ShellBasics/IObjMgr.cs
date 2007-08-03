using System;
using System.Runtime.InteropServices;

namespace ShellLib
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("00BB2761-6A77-11D0-A535-00C04FD7D062")]
	public interface IObjMgr 
	{
		/// <summary>
		/// Appends an object to the collection of managed objects.
		/// </summary>
		[PreserveSig]
		Int32 Append(
			[MarshalAs(UnmanagedType.IUnknown)]
			Object punk);	// Address of the IUnknown interface of the object to be added to the list. 

		/// <summary>
		/// Removes an object from the collection of managed objects.
		/// </summary>
		[PreserveSig]
		Int32 Remove(
			[MarshalAs(UnmanagedType.IUnknown)]
			Object punk);	// Address of the IUnknown interface of the object to be removed from the list. 
			
	}
}
	