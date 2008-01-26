#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion


using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Net.Mail;
using System.IO;

using Microsoft.ApplicationBlocks.ExceptionManagement;
using NewsComponents.Utils;

namespace RssBandit
{
	/// <summary>
	/// Exception Publisher for File and Mail
	/// </summary>
	public class BanditExceptionPublisher : IExceptionPublisher
	{
		private string m_LogName = Path.Combine(Path.GetTempPath(), "rssbandit.error.log");
		private string m_OpMail = String.Empty;

		/// <summary>
		/// Constructor for the Exception publisher
		/// </summary>
		public BanditExceptionPublisher()	{}

		// Provide implementation of the IPublishException interface
		// This contains the single Publish method.
		void IExceptionPublisher.Publish(Exception exception, NameValueCollection AdditionalInfo, NameValueCollection ConfigSettings)
		{
			// Load Config values if they are provided.
			if (ConfigSettings != null)
			{
				if (ConfigSettings["fileName"] != null &&  
					ConfigSettings["fileName"].Length > 0)
				{  
					m_LogName = Environment.ExpandEnvironmentVariables(ConfigSettings["fileName"]);
				}
				if (ConfigSettings["operatorMail"] !=null && 
					ConfigSettings["operatorMail"].Length > 0)
				{
					m_OpMail = ConfigSettings["operatorMail"];
				}
			}
			// Create StringBuilder to maintain publishing information.
			StringBuilder strInfo = new StringBuilder();

			// Record the contents of the AdditionalInfo collection.
			if (AdditionalInfo != null)
			{
				// Record General information.
				strInfo.AppendFormat("{0}General Information{0}", Environment.NewLine);
				strInfo.AppendFormat("{0}{1}", Environment.NewLine, RssBanditApplication.Caption);
				strInfo.AppendFormat("{0}OS Version: {1}", Environment.NewLine, Environment.OSVersion.ToString());
				strInfo.AppendFormat("{0}OS-Culture: {1}", Environment.NewLine, System.Globalization.CultureInfo.InstalledUICulture.Name);
				strInfo.AppendFormat("{0}Framework Version: .NET CLR {1}", Environment.NewLine, System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion());
				strInfo.AppendFormat("{0}Thread-Culture: {1}", Environment.NewLine, System.Threading.Thread.CurrentThread.CurrentCulture.Name);
				strInfo.AppendFormat("{0}UI-Culture: {1}", Environment.NewLine, System.Threading.Thread.CurrentThread.CurrentUICulture.Name);

				strInfo.AppendFormat("{0}Additonal Info:", Environment.NewLine);
				foreach (string i in AdditionalInfo)
				{
					strInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, i, AdditionalInfo.Get(i));
				}
			}
			// Append the exception text
			if (exception != null) {
				strInfo.AppendFormat("{0}{0}Exception Information{0}{1}", Environment.NewLine, exception.ToString());
				// how about exception.InnerException ?
			} else {
				strInfo.AppendFormat("{0}{0}No Exception.{0}", Environment.NewLine);
			}

			// Write the entry to the log file.   
			using ( FileStream fs = FileHelper.OpenForWriteAppend(m_LogName)	) {
				StreamWriter sw = new StreamWriter(fs);
				sw.WriteLine(strInfo.ToString());
				sw.WriteLine("================= End Entry =================");
				sw.Flush();
			}
			// send notification email if operatorMail attribute was provided
			if (m_OpMail.Length > 0)
			{
				string subject = "Exception Notification";
				string body = strInfo.ToString();
                SmtpClient mailer = new SmtpClient(); 
                mailer.Send("CustomSender@mycompany.com", m_OpMail, subject, body);
			}
		}
	}
}
