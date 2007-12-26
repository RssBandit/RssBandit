#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using NewsComponents.Resources;

namespace NewsComponents.Utils
{
	/// <summary>
	/// DateTimeExt is able to parse RFC2822/RFC822 formatted dates.
	/// </summary>
	public sealed class DateTimeExt
	{

		private static readonly log4net.ILog _log = RssBandit.Common.Logging.Log.GetLogger(typeof(DateTimeExt));

		private static Regex rfc2822 = new Regex(@"\s*(?:(?:Mon|Tue|Wed|Thu|Fri|Sat|Sun)\s*,\s*)?(\d{1,2})\s+(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s+(\d{2,})\s+(\d{2})\s*:\s*(\d{2})\s*(?::\s*(\d{2}))?\s+([+\-]\d{4}|UT|GMT|EST|EDT|CST|CDT|MST|MDT|PST|PDT|[A-IK-Z])", RegexOptions.Compiled);
		private static ArrayList months = new ArrayList(new string[]{"ZeroIndex","Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec" });
		
		//private static TimeSpan dayLightDelta = TimeZone.CurrentTimeZone.GetDaylightChanges(DateTime.Now.Year).Delta;
	
	
		/// <summary>
		/// Converts an ISO 8601 date to a DateTime object. Helper method needed to 
		/// deal with timezone offset since they are unsupported by the 
		/// .NET Framework. 
		/// </summary>
		/// <param name="datetime">DateTime string</param>
		/// <returns>DateTime instance</returns>
		/// <exception cref="FormatException">On format errors parsing the datetime</exception>
		/// <remarks>
		/// See also W3C note at: http://www.w3.org/TR/NOTE-datetime
		/// </remarks>
		public static DateTime ToDateTime(string datetime){

			//strip trailing 'Z' since we assume UTC
			datetime = (datetime.EndsWith("Z") ? datetime.Substring(0, datetime.Length - 1) : datetime); 

			int timeIndex = datetime.IndexOf(":");    

			if(timeIndex != -1){			

				int tzoneIndex = datetime.IndexOf("-", timeIndex); 

				if(tzoneIndex == -1){ 

					tzoneIndex = datetime.IndexOf("+", timeIndex); 

					if(tzoneIndex != -1){ //timezone is ahead of UTC

						return AddOffset("+", datetime, tzoneIndex); 	  

					}

				}else{ //timezone is behind UTC

					return AddOffset("-", datetime, tzoneIndex); 
				}

			}


			if(timeIndex == datetime.LastIndexOf(":")){ //check if seconds part is missing
				datetime = datetime + ":00"; 
			} 

			return XmlConvert.ToDateTime(datetime);

		}


		/// <summary>
		/// Parse is able to parse RFC2822/RFC822 formatted dates.
		/// It has a fallback mechanism: if the string does not match,
		/// the normal DateTime.Parse() function is called.
		/// </summary>
		/// <param name="dateTimeString">DateTime String to parse</param>
		/// <returns>DateTime instance with date and time converted to Universal Time</returns>
		/// <exception cref="FormatException">On format errors parsing the datetime</exception>
		public static DateTime Parse(string dateTimeString)
		{
			if (dateTimeString == null)
				return DateTime.Now.ToUniversalTime();

			if (dateTimeString.Trim().Length == 0)
				return DateTime.Now.ToUniversalTime();

			Match m = rfc2822.Match(dateTimeString);
			if (m.Success) 
			{
				try
				{
					int dd = Int32.Parse(m.Groups[1].Value); 
					int mth = months.IndexOf(m.Groups[2].Value);
					int yy = Int32.Parse(m.Groups[3].Value); 
					// following year completion is compliant with RFC 2822.
					yy = (yy < 50 ? 2000 + yy: (yy < 1000 ? 1900 + yy: yy));
					int hh = Int32.Parse(m.Groups[4].Value);
					int mm = Int32.Parse(m.Groups[5].Value);
					int ss = Int32.Parse("0" + m.Groups[6].Value);	// optional (may get lenght zero)
					string zone =  m.Groups[7].Value;

					DateTime xd = new DateTime(yy, mth, dd, hh, mm, ss);
					return xd.AddHours(RFCTimeZoneToGMTBias(zone) * -1);
				}
				catch (Exception e)
				{
					throw new FormatException(ComponentsText.ExceptionRFC2822ParseGroupsMessage(e.GetType().Name), e);
				}
			}
			else
			{
				// fallback, if regex does not match:

				//TR fix:Jan,14. 2006 - we have to return Universal Time,
				// but DateTime.Parse(string) will use DateTimeStyles.None by default
				// and return a DateTime in local machine time!
				// old code was:
				//return DateTime.Parse(dateTimeString);
				return DateTime.Parse(dateTimeString, null, DateTimeStyles.AdjustToUniversal);

			}
			// Unreachable code:
			//return DateTime.Now.ToUniversalTime();
		}


		private DateTimeExt(){}


		private struct TZB
		{					
			public TZB(string z, int b) { Zone = z; Bias = b; }
			public string Zone;
			public int Bias;
		}

		private const int timeZones = 35;
		private static TZB[] ZoneBias = new TZB[timeZones]
				{
					new TZB("GMT", 0),     new TZB("UT", 0),
					new TZB("EST", -5*60), new TZB("EDT", -4*60),
					new TZB("CST", -6*60), new TZB("CDT", -5*60),
					new TZB("MST", -7*60), new TZB("MDT", -6*60),
					new TZB("PST", -8*60), new TZB("PDT", -7*60),
					new TZB("Z", 0),       new TZB("A", -1*60),
					new TZB("B", -2*60),   new TZB("C", -3*60),
					new TZB("D", -4*60),   new TZB("E", -5*60),
					new TZB("F", -6*60),   new TZB("G", -7*60),
					new TZB("H", -8*60),   new TZB("I", -9*60),
					new TZB("K", -10*60),  new TZB("L", -11*60),
					new TZB("M", -12*60),  new TZB("N", 1*60),
					new TZB("O", 2*60),    new TZB("P", 3*60),
					new TZB("Q", 4*60),    new TZB("R", 3*60),
					new TZB("S", 6*60),    new TZB("T", 3*60),
					new TZB("U", 8*60),    new TZB("V", 3*60),
					new TZB("W", 10*60),   new TZB("X", 3*60),
					new TZB("Y", 12*60)
				};

		private static double RFCTimeZoneToGMTBias(string zone)
		{

			string s;
			
			if ( zone.IndexOfAny(new char[]{'+', '-'}) == 0 )  // +hhmm format
			{
				int fact = (zone.Substring(0,1) == "-"? -1: 1);
				s = zone.Substring(1).TrimEnd();
				double hh = Math.Min(23, Int32.Parse(s.Substring(0,2)));
				double mm = Math.Min(59, Int32.Parse(s.Substring(2,2)))/60;
				return fact * (hh+mm);
			} 
			else
			{ // named format
				s = zone.ToUpper().Trim();
				for (int i = 0; i < timeZones; i++)
					if (ZoneBias[i].Zone.Equals(s)) 
					{
						return ZoneBias[i].Bias / 60;
					}
			}
			return 0.0;
		}

		private static DateTime AddOffset(string offsetOp, string datetime, int tzoneIndex){

			string[]  offset   = datetime.Substring(tzoneIndex + 1).Split(new char[]{':'}); 
			string    original = datetime; 
			datetime           = datetime.Substring(0, tzoneIndex);

			if(datetime.IndexOf(":") == datetime.LastIndexOf(":")){ //check if seconds part is missing
				datetime = datetime + ":00"; 
			} 

			DateTime  toReturn = XmlConvert.ToDateTime(datetime); 

			try{

				switch(offsetOp){

					case "+":
						toReturn = toReturn.Subtract(new TimeSpan(Int32.Parse(offset[0]), Int32.Parse(offset[1]), 0)); 
						break; 

					case "-":
						toReturn = toReturn.Add(new TimeSpan(Int32.Parse(offset[0]), Int32.Parse(offset[1]), 0)); 
						break; 
      
				}
				
				return toReturn; //.ToLocalTime(); //we treat all dates in feeds as if they are local time (later)
			
			}catch(IndexOutOfRangeException){
			  
				throw new FormatException(ComponentsText.ExceptionRFC2822InvalidTimezoneFormatMessage(original));
			}
   
		}
	
		/// <summary>
		/// returns a date as integer in the format YYYYMMDD.
		/// </summary>
		/// <param name="date">The date.</param>
		/// <returns>int</returns>
		public static int DateAsInteger(DateTime date) {
			return date.Year *10000 + date.Month * 100 + date.Day;
		}
		
	}	// end class DateTimeExt

}
