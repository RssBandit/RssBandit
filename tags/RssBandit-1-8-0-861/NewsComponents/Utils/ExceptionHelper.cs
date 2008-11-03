#region Version Info Header

/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace NewsComponents.Utils
{
	/// <summary>
	/// Extensions for Exception class and parameter check throw helpers
	/// </summary>
	public static class ExceptionHelper
	{
		/// <summary>
		/// Used to preserve stack traces on rethrow
		/// </summary>
		private static readonly MethodInfo PreserveException = typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);


		/// <summary>
		/// Calls the Exception's internal method to preserve its stack trace prior to rethrow
		/// </summary>
		/// <remarks>
		/// See http://dotnetjunkies.com/WebLog/chris.taylor/archive/2004/03/03/8353.aspx for more info.
		/// </remarks>
		/// <param name="e"></param>
		public static void PreserveExceptionStackTrace(this Exception e)
		{
			if (e == null)
				throw new ArgumentNullException("e");

			PreserveException.Invoke(e, null);
		}

		/// <summary>
		/// Breaks and logs Exception and all its inner exceptions into one neatly formatted string.
		/// </summary>
		public static string ToDescriptiveString(this Exception ex)
		{
			var infoBuilder = new StringBuilder();

			if (ex.InnerException != null)
			{
				infoBuilder.Append(ex.InnerException.ToDescriptiveString());
				infoBuilder.Append(Environment.NewLine + Environment.NewLine + "- Nested Exception --------------------------------------" + Environment.NewLine + Environment.NewLine);
			}

			infoBuilder.AppendFormat(CultureInfo.InvariantCulture, "Exception:     {0}" + Environment.NewLine, ex.GetType());
			infoBuilder.AppendFormat(CultureInfo.InvariantCulture, "Message:       {0}" + Environment.NewLine, ex.Message);
			infoBuilder.AppendFormat(CultureInfo.InvariantCulture, "Source:        {0}" + Environment.NewLine + "{1}", ex.Source, ex.StackTrace);

			if (ex is ReflectionTypeLoadException)
			{
				foreach (var ex1 in (ex as ReflectionTypeLoadException).LoaderExceptions)
				{
					infoBuilder.Append(Environment.NewLine + Environment.NewLine + "- Loader Exception --------------------------------------" + Environment.NewLine + Environment.NewLine);
					infoBuilder.Append(ex1.ToDescriptiveString());
				}
			}

			return infoBuilder.ToString();
		}


		/// <summary>
		/// Throws an <see cref="ArgumentNullException"/> if the value is null.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="value">The value to check for null.</param>
		/// <param name="argumentName">The name of the argument the value represents.</param>
		/// <exception cref="ArgumentNullException">The value is null.</exception>
		public static void ExceptionIfNull<T>(this T value, string argumentName) where T : class
		{
			if (value == null)
			{
				throw new ArgumentNullException(argumentName);
			}
		}

		/// <summary>
		/// Throws an <see cref="ArgumentNullException"/> if the value is null.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="value">The value to check for null.</param>
		/// <param name="argumentName">The name of the argument the value represents.</param>
		/// <param name="message">A message for the exception.</param>
		/// <exception cref="ArgumentNullException">The value is null.</exception>
		public static void ExceptionIfNull<T>(this T value, string argumentName, string message) where T : class
		{
			if (value == null)
			{
				throw new ArgumentNullException(argumentName, message);
			}
		}

		/// <summary>
		/// Throws an <see cref="ArgumentNullException"/> if the specified value is null or
		/// throws an <see cref="ArgumentOutOfRangeException"/> if the specified value is an
		/// empty string.
		/// </summary>
		/// <param name="value">The string to check for null or empty.</param>
		/// <param name="argumentName">The name of the argument the value represents.</param>
		/// <exception cref="ArgumentNullException">The value is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">The value is an empty string.</exception>
		public static void ExceptionIfNullOrEmpty(this string value,
			string argumentName)
		{
			value.ExceptionIfNull("value");

			if (value.Length == 0)
			{
				throw new ArgumentOutOfRangeException(argumentName,
					string.Format(CultureInfo.InvariantCulture,
						"The length of the string '{0}' may not be 0.", argumentName ?? string.Empty));
			}
		}

		/// <summary>
		/// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified value is not within the range
		/// specified by the <paramref name="lowerBound"/> and <paramref name="upperBound"/> parameters.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="value">The value to check that it's not out of range.</param>
		/// <param name="lowerBound">The lowest value that's considered being within the range.</param>
		/// <param name="upperBound">The highest value that's considered being within the range.</param>
		/// <param name="argumentName">The name of the argument the value represents.</param>
		/// <exception cref="ArgumentOutOfRangeException">The value is not within the given range.</exception>
		public static void ExceptionIfOutOfRange<T>(this T value,
			T lowerBound, T upperBound, string argumentName) where T : IComparable<T>
		{
			if (value.CompareTo(lowerBound) < 0 || value.CompareTo(upperBound) > 0)
			{
				throw new ArgumentOutOfRangeException(argumentName);
			}
		}


		/// <summary>
		/// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified value is not within the range
		/// specified by the <paramref name="lowerBound"/> and <paramref name="upperBound"/> parameters.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="value">The value to check that it's not out of range.</param>
		/// <param name="lowerBound">The lowest value that's considered being within the range.</param>
		/// <param name="upperBound">The highest value that's considered being within the range.</param>
		/// <param name="argumentName">The name of the argument the value represents.</param>
		/// <param name="message">A message for the exception.</param>
		/// <exception cref="ArgumentOutOfRangeException">The value is not within the given range.</exception>
		public static void ExceptionIfOutOfRange<T>(this T value,
			T lowerBound, T upperBound, string argumentName, string message) where T : IComparable<T>
		{
			if (value.CompareTo(lowerBound) < 0 || value.CompareTo(upperBound) > 0)
			{
				throw new ArgumentOutOfRangeException(argumentName, message);
			}
		}

		/// <summary>
		/// Throws an <see cref="IndexOutOfRangeException"/> if the specified index is not within the range
		/// specified by the <paramref name="lowerBound"/> and <paramref name="upperBound"/> parameters.
		/// </summary>
		/// <param name="index">The index to check that it's not out of range.</param>
		/// <param name="lowerBound">The lowest value considered being within the range.</param>
		/// <param name="upperBound">The highest value considered being within the range.</param>
		/// <param name="argumentName">The name of the argument the value represents.</param>
		/// <exception cref="IndexOutOfRangeException">The index is not within the given range.</exception>
		public static void ExceptionIfIndexOutOfRange(this int index,
			int lowerBound, int upperBound, string argumentName)
		{
			if (index < lowerBound || index > upperBound)
			{
				throw new IndexOutOfRangeException();
			}
		}

		/// <summary>
		/// Throws an <see cref="IndexOutOfRangeException"/> if the specified index is not within the range
		/// specified by the <paramref name="lowerBound"/> and <paramref name="upperBound"/> parameters.
		/// </summary>
		/// <param name="index">The index to check that it's not out of range.</param>
		/// <param name="lowerBound">The lowest value considered being within the range.</param>
		/// <param name="upperBound">The highest value considered being within the range.</param>
		/// <param name="argumentName">The name of the argument the value represents.</param>
		/// <exception cref="IndexOutOfRangeException">The index is not within the given range.</exception>
		public static void ExceptionIfIndexOutOfRange(this long index,
			long lowerBound, long upperBound, string argumentName)
		{
			if (index < lowerBound || index > upperBound)
			{
				throw new IndexOutOfRangeException();
			}
		}

		/// <summary>
		/// Catches the exception and collect them in the provided list.
		/// </summary>
		/// <typeparam name="ET">The type of the Exception.</typeparam>
		/// <param name="action">The action.</param>
		/// <param name="exceptions">The exceptions list.</param>
		public static void CatchAndCollectExceptions<ET>(Action action, ICollection<ET> exceptions) where ET : Exception
		{
			try { action(); }
			catch (ET ex) { exceptions.Add(ex); }
		}
	}

    
}
