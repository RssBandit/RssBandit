#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Runtime.InteropServices;

namespace RssBandit.Common.Logging {
	

	/// <summary>
	///  Helps to profile some calls.
	/// </summary>
	/// <remarks>
	/// Usage:
	///   long secs = 0;
	///   ProfilerHelper.StartMeasure(ref secs);
	///   // ... time consuming task here
	///   Trace.WriteLine(ProfilerHelper.StopMeasureString(secs), "task perf.");
	/// </remarks>
	internal static class ProfilerHelper {
		
		private static long seqFreq = 0;
		
		[System.Runtime.InteropServices.DllImport("KERNEL32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool QueryPerformanceCounter(
			ref long lpPerformanceCount);

		[System.Runtime.InteropServices.DllImport("KERNEL32")]
		[return:MarshalAs(UnmanagedType.Bool)]
		private static extern bool QueryPerformanceFrequency(
			ref long lpFrequency);

		static ProfilerHelper() {
			QueryPerformanceFrequency(ref seqFreq);
		}

		public static void StartMeasure(ref long secStart) {
			QueryPerformanceCounter(ref secStart);
		}

		public static double StopMeasure(long secStart) {
			long secTiming = 0;
			QueryPerformanceCounter(ref secTiming);
			if (seqFreq == 0) return 0.0;	// Handle no high-resolution timer
			return (double)(secTiming - secStart) / (double)seqFreq;
		}

		public static string StopMeasureString(long secStart) {
			return String.Format("{0:0.###} sec(s)", StopMeasure(secStart));
		}
	}
}