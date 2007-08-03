using System;
using System.Runtime.InteropServices;

namespace ShellLib
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("470141A0-5186-11D2-BBB6-0060977B464C")]
	public interface IACList2
	{
		/// <summary>
		/// Sets the current autocomplete options.
		/// </summary>
		[PreserveSig]
		Int32 SetOptions(
			UInt32 dwFlag);			// New option flags. Use these flags to ask the client to include the names 
									// of the files and subfolders of the specified folders the next time the 
									// client's IEnumString interface is called.

		/// <summary>
		/// Retrieves the current autocomplete options.
		/// </summary>
		[PreserveSig]
		Int32 GetOptions(
			out UInt32 pdwFlag);	// Pointer to a value that will hold the current option flag when the 
									// method returns.

	}
}
	