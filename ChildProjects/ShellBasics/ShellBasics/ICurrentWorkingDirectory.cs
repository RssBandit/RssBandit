using System;
using System.Runtime.InteropServices;

namespace ShellLib
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("91956D21-9276-11D1-921A-006097DF5BD4")]
	public interface ICurrentWorkingDirectory 
	{
		/// <summary>
		/// Retrieves the current working directory.
		/// </summary>
		[PreserveSig]
		Int32 GetDirectory(
			[MarshalAs(UnmanagedType.LPWStr)]
			String pwzPath,		// Address of a buffer. On return, it will hold a null-terminated Unicode string with 
								// the current working directory's fully qualified path. 
			UInt32 cchSize);	// Size of the buffer in Unicode characters, including the terminating NULL character. 

		/// <summary>
		/// Sets the current working directory.
		/// </summary>
		[PreserveSig]
		Int32 SetDirectory(
			[MarshalAs(UnmanagedType.LPWStr)]
			String pwzPath);	// Address of a null-terminated Unicode string with the fully qualified path of the 
								// new working directory. 

	}
}
	