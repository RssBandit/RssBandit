
using System;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace NewsComponents.Utils
{
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
                infoBuilder.Append("\n\n- Nested Exception --------------------------------------\n\n");
            }

            infoBuilder.AppendFormat(CultureInfo.InvariantCulture, "Exception:     {0}\n", ex.GetType());
            infoBuilder.AppendFormat(CultureInfo.InvariantCulture, "Message:       {0}\n", ex.Message);
            infoBuilder.AppendFormat(CultureInfo.InvariantCulture, "Source:        {0}\n{1}", ex.Source, ex.StackTrace);

            if (ex is ReflectionTypeLoadException)
            {
                foreach (var ex1 in (ex as ReflectionTypeLoadException).LoaderExceptions)
                {
                    infoBuilder.Append("\n\n- Loader Exception --------------------------------------\n\n");
                    infoBuilder.Append(ex1.ToDescriptiveString());
                }
            }

            return infoBuilder.ToString();
        }
    }
}
