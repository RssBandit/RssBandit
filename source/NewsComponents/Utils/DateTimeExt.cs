#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;

namespace NewsComponents.Utils
{
	/// <summary>
	/// DateTimeExt is able to parse RFC2822/RFC822 formatted dates.
	/// </summary>
	public static class DateTimeExt
	{
		/// <summary>
		/// Parse is able to parse RFC2822/RFC822 formatted dates with
		/// fallback mechanisms.
		/// </summary>
		/// <param name="dateTimeString">DateTime String to parse</param>
		/// <returns>DateTime instance with date and time converted to Universal Time</returns>
		/// <exception cref="FormatException">
		/// On format errors parsing the <paramref name="dateTimeString"/> if it match RFC 2822 
		/// but another unexpected error occurs, or the <paramref name="dateTimeString"/> could not be parsed.
		/// </exception>
		public static DateTime ParseRfc2822DateTime(string dateTimeString)
		{
			if (dateTimeString == null)
				return DateTime.Now.ToUniversalTime();

			if (dateTimeString.Trim().Length == 0)
				return DateTime.Now.ToUniversalTime();

			return TorSteroids.Common.DateTimeExt.ParseRfc2822DateTime(dateTimeString);
		}
		
	}	

}
