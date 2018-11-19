using System;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace System
{
    internal static class ExceptionHelper
    {

        /// <summary>
        /// Breaks and logs Exception and all its inner exceptions into one neatly formatted string.
        /// </summary>
        public static string ToDescriptiveString(this Exception ex)
        {
            var infoBuilder = new StringBuilder();

            if (ex.InnerException != null)
            {
                infoBuilder.Append(ex.InnerException.ToDescriptiveString());
                infoBuilder.Append(Environment.NewLine + Environment.NewLine +
                                   "- Nested Exception --------------------------------------" + Environment.NewLine +
                                   Environment.NewLine);
            }

            infoBuilder.AppendFormat(CultureInfo.InvariantCulture, "Exception:     {0}" + Environment.NewLine,
                                     ex.GetType());
            infoBuilder.AppendFormat(CultureInfo.InvariantCulture, "Message:       {0}" + Environment.NewLine,
                                     ex.Message);
            infoBuilder.AppendFormat(CultureInfo.InvariantCulture, "Data:          {0}" + Environment.NewLine + "{1}",
                                     ex.Data, ex.StackTrace);

            if (ex is ReflectionTypeLoadException)
            {
                foreach (var ex1 in (ex as ReflectionTypeLoadException).LoaderExceptions)
                {
                    infoBuilder.Append(Environment.NewLine + Environment.NewLine +
                                       "- Loader Exception --------------------------------------" + Environment.NewLine +
                                       Environment.NewLine);
                    infoBuilder.Append(ex1.ToDescriptiveString());
                }
            }

            return infoBuilder.ToString();
        }
    }

}
