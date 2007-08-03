#region CVS Version Header
/*
 * $Id: ExceptionLog4netPublisher.cs,v 1.1 2004/03/02 16:10:44 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2004/03/02 16:10:44 $
 * $Revision: 1.1 $
 */
#endregion

using System;
using Microsoft.ApplicationBlocks.ExceptionManagement;

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
			Common.Logging.Log.Error(exception.ToString() ,exception);
		}

		#endregion
	}
}
