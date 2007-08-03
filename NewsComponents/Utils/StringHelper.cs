#region CVS Version Header
/*
 * $Id: StringHelper.cs,v 1.8 2007/05/03 15:58:06 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2007/05/03 15:58:06 $
 * $Revision: 1.8 $
 */
#endregion

using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NewsComponents.Utils
{
	/// <summary>
	/// Summary description for StringHelper.
	/// </summary>
	public sealed class StringHelper
	{
		// 'The next line is supposed to be an RFC 2822 address compliant validation expression
		private static Regex regexEMail = new Regex(@"(?<prefix>mailto:)?(?<address>(?:[\w\!\#\$\%\&\'\*\+\-\/\=\?\^\`\{\|\}\~]+\.)*[\w\!\#\$\%\&\'\*\+\-\/\=\?\^\`\{\|\}\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-](?!\.)){0,61}[a-zA-Z0-9]?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\[(?:(?:[01]?\d{1,2}|2[0-4]\d|25[0-5])\.){3}(?:[01]?\d{1,2}|2[0-4]\d|25[0-5])\]))$)", RegexOptions.Singleline | RegexOptions.CultureInvariant| RegexOptions.Compiled);
		private static Regex regexWords = new Regex(@"\S+", RegexOptions.Multiline | RegexOptions.CultureInvariant| RegexOptions.Compiled);

		/// <summary>
		/// Helper to test strings.
		/// </summary>
		/// <param name="text">String to test</param>
		/// <returns>True, if 'text' was null or of length zero</returns>
		/// <remarks>No trimming of the string happens</remarks>
		public static bool EmptyOrNull(string text) {
			return (text == null || text.Length == 0);
		}

		/// <summary>
		/// Helper to test strings.
		/// </summary>
		/// <param name="text">String to test</param>
		/// <returns>True, if 'text' was null or of length zero</returns>
		/// <remarks>Trimming of the string happens</remarks>
		public static bool EmptyTrimOrNull(string text) {
			return (text == null || text.Trim().Length == 0);
		}
		
		/// <summary>
		/// Return the size of a string in byte. Consider, if it is null.
		/// </summary>
		/// <param name="s">String</param>
		/// <returns>Size in bytes</returns>
		public static int SizeOfStr(string s) {
			return StringHelper.LengthOfStr(s) * 2;	// in byte
		}
		/// <summary>
		/// Return the length of a string. Consider, if it is null.
		/// </summary>
		/// <param name="s">String</param>
		/// <returns>Length</returns>
		public static int LengthOfStr(string s) {
			if (StringHelper.EmptyOrNull(s))
				return 0;
			return s.Length;
		}

		/// <summary>
		/// Cuts the provided string at allowedLength - 3 (ellipsis length)
		/// </summary>
		/// <param name="text">string to work on</param>
		/// <param name="allowedLength">Max. length of the string to return</param>
		/// <returns>string that ends with ellipsis (...)</returns>
		/// <remarks>Considers newline and linefeed</remarks>
		public static string ShortenByEllipsis(string text, int allowedLength) {
			if (text == null) return String.Empty;

			if (text.Length > allowedLength + 3) {
				int nlPos = text.IndexOfAny(new char[]{'\n','\r'});
				if (nlPos >= 0 && nlPos < allowedLength)
					return text.Substring(0, nlPos) + "...";
				else
					return text.Substring(0, allowedLength) + "...";
			}
			else
				return text;
		}

		/// <summary>
		/// Return the first amount of words defined by 'wordCount' contained in 'text'.
		/// </summary>
		/// <param name="text">String to work on</param>
		/// <param name="wordCount">Amount of words to look for</param>
		/// <returns>String containing only the first x words.</returns>
		/// <remarks>Word delimiters are: linefeed, carrige return, tab and space</remarks>
		public static string GetFirstWords(string text, int wordCount) 
		{
			if (text == null) return String.Empty;
		
			MatchCollection words = regexWords.Matches(text);

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < Math.Min(words.Count, wordCount); i++) 
			{
				sb.Append(words[i].Value);
				sb.Append(" ");
			}
			return sb.ToString().TrimEnd();
		}

		/// <summary>
		/// Tests the <c>text</c> for a valid e-Mail address and returns true,
		/// if the content match, else false. 
		/// </summary>
		/// <param name="text">String to test</param>
		/// <returns>True if it looks like a valid e-Mail address, else false.</returns>
		public static bool IsEMailAddress(string text) 
		{
			if (EmptyOrNull(text))
				return false;
			return regexEMail.IsMatch(text.Trim());
		}

		/// <summary>
		/// Gets the Email address within a selection of text.
		/// </summary>
		/// <param name="text">Text.</param>
		/// <returns></returns>
		public static string GetEMailAddress(string text) 
		{
			if (EmptyOrNull(text))
				return String.Empty;

			Match m = regexEMail.Match(text);
			if (m.Success) 
			{
				return m.Groups["address"].Value;
			} 
			else 
			{
				return text;
			}
		}

		/// <summary>
		/// Performs case insensitive check on whether the strings are equal.
		/// </summary>
		/// <param name="original">Original.</param>
		/// <param name="comparand">Comparand.</param>
		/// <returns></returns>
		public static bool AreEqualCaseInsensitive(string original, string comparand)
		{
			return string.Compare(original, comparand, true, CultureInfo.InvariantCulture) == 0;
		}

		private StringHelper(){}
	}
}