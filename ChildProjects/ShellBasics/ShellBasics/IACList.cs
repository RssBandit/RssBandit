using System;
using System.Runtime.InteropServices;

namespace ShellLib
{
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("77A130B0-94FD-11D0-A544-00C04FD7D062")]
	public interface IACList 
	{
		/// <summary>
		/// Requests that the autocompletion client generate candidate strings associated with a specified item in 
		/// its namespace.
		/// </summary>
		[PreserveSig]
		Int32 Expand(
			[MarshalAs(UnmanagedType.LPWStr)]
			String pszExpand);						// Null-terminated string to be expanded by the autocomplete object. 
				
	}
}
	