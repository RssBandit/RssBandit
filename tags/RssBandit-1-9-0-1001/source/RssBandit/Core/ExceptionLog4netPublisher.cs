#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using Microsoft.ApplicationBlocks.ExceptionManagement;
using NewsComponents.Utils;

namespace RssBandit
{
	/// <summary>
	/// Summary description for FormPublisher.
	/// </summary>
	public class ExceptionLog4NetPublisher : IExceptionPublisher
	{
		/// <summary>
		/// Initialize the Exception Log4Net Publisher
		/// </summary>
		public ExceptionLog4NetPublisher() {
		}
		#region IExceptionPublisher Members

		/// <summary>
		/// Log4Net Publisher
		/// </summary>
		/// <param name="exception">Exception</param>
		/// <param name="additionalInfo">Additional Information</param>
		/// <param name="configSettings">Parameters</param>
		public void Publish(Exception exception, System.Collections.Specialized.NameValueCollection additionalInfo, System.Collections.Specialized.NameValueCollection configSettings) {
            Common.Logging.Log.Error(exception.ToDescriptiveString(), exception);
		}

		#endregion
	}
}
