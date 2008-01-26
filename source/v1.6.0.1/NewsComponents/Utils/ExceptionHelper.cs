
using System;
using System.Reflection;

namespace NewsComponents.Utils
{
    public sealed class ExceptionHelper
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
        public static void PreserveExceptionStackTrace(Exception e)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            PreserveException.Invoke(e, null);
        }
    }
}
