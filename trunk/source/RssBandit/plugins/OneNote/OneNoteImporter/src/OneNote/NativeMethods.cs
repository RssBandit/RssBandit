using System;
using System.Runtime.InteropServices;

namespace Microsoft.Office.OneNote
{
	/// <summary>
	/// Wraps various Win32 APIs used by the OneNote assembly.
	/// </summary>
	internal class NativeMethods
	{
		private NativeMethods()
		{
		}

		[DllImport("user32.dll")]
		internal static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		internal static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		internal static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32.dll")]
		internal static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		internal static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

		[DllImport("user32.dll")]
		internal static extern IntPtr AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, int fAttach);

		internal const int SW_HIDE = 0;
		internal const int SW_SHOWNORMAL = 1;
		internal const int SW_SHOWMINIMIZED = 2;
		internal const int SW_SHOWMAXIMIZED = 3;
		internal const int SW_SHOWNOACTIVATE = 4;
		internal const int SW_RESTORE = 9;
		internal const int SW_SHOWDEFAULT = 10;
	}
}
