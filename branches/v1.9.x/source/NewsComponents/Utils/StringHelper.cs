#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NewsComponents.Utils
{
	/// <summary>
	/// Summary description for StringHelper.
	/// </summary>
	public static class StringHelper
	{
		// 'The next line is supposed to be an RFC 2822 address compliant validation expression
		private static readonly Regex regexEMail = new Regex(@"(?<prefix>mailto:)?(?<address>(?:[\w\!\#\$\%\&\'\*\+\-\/\=\?\^\`\{\|\}\~]+\.)*[\w\!\#\$\%\&\'\*\+\-\/\=\?\^\`\{\|\}\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-](?!\.)){0,61}[a-zA-Z0-9]?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\[(?:(?:[01]?\d{1,2}|2[0-4]\d|25[0-5])\.){3}(?:[01]?\d{1,2}|2[0-4]\d|25[0-5])\]))$)", RegexOptions.Singleline | RegexOptions.CultureInvariant| RegexOptions.Compiled);
		private static readonly Regex regexWords = new Regex(@"\S+", RegexOptions.Multiline | RegexOptions.CultureInvariant| RegexOptions.Compiled);


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
			return LengthOfStr(s) * 2;	// in byte
		}
		/// <summary>
		/// Return the length of a string. Consider, if it is null.
		/// </summary>
		/// <param name="s">String</param>
		/// <returns>Length</returns>
		public static int LengthOfStr(string s) {
			if (string.IsNullOrEmpty(s))
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
			text = text.Trim();
			if (text.Length > allowedLength + 3) {
				int nlPos = text.IndexOfAny(new char[]{'\n','\r'});
				if (nlPos >= 0 && nlPos < allowedLength)
					return text.Substring(0, nlPos) + "...";
				return text.Substring(0, allowedLength) + "...";
			}
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
			if (string.IsNullOrEmpty(text))
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
			if (string.IsNullOrEmpty(text))
				return String.Empty;

			Match m = regexEMail.Match(text);
			if (m.Success) 
			{
				return m.Groups["address"].Value;
			}
			return text;
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

		private static readonly List<char> validChars = new List<char>(new[] { '\t', '\r', '\n' });
		
		/// <summary>
		/// Used to test whether a string contains any character that is illegal in XML 1.0. 
		/// Specifically it checks for the ASCII control characters except for tab, carriage return 
		/// and newline which are the only ones allowed in XML. 
		/// </summary>
		/// <param name="text">the string to test</param>
		/// <returns>true if the string contains a character that is illegal in XML</returns>
		public static bool ContainsInvalidXmlChars(string text)
		{
			if (text != null)
				foreach (char c in text)
				{
					if (Char.IsControl(c) && !validChars.Contains(c))
					{
						return true;
					}
				}

			return false;
		}
		/// <summary>
		/// Strips the non display chars from text.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <returns></returns>
		public static string StripNonDisplayChars(string text)
		{

			if (String.IsNullOrEmpty(text) )
				return text;
			
			StringBuilder b = new StringBuilder(text.Length);
			
			foreach (char c in text) {
				if (!Char.IsControl(c))
				{
					b.Append(c);
				}
			}
			return b.ToString();
		}

		internal static string Rot13(string value)
		{
			const char c13 = (char)13;
			const char c26 = (char)26;

			var result = new StringBuilder(value.Length);
			foreach (char c in value)
			{
				char rc = c;
				if (c >= 'a' && c <= 'z')
				{
					rc -= 'a';
					rc += c13;
					rc %= c26;
					rc += 'a';
				}
				else if (c >= 'A' && c <= 'Z')
				{
					rc -= 'A';
					rc += c13;
					rc %= c26;
					rc += 'A';
				}
				result.Append(rc);
			}
			return result.ToString();
		}
	}
}