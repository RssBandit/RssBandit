using System;
using System.IO;
using System.Web.Hosting;
using log4net;
using log4net.Config;
using System.Configuration;

namespace RssBandit.Services
{
	/// <summary>
	///  Wrapper around Log4Net.
	///  For configuration options have a look to http://logging.apache.org/log4net/release/manual/configuration.html.
	///  For configuration examples have a look to http://logging.apache.org/log4net/release/config-examples.html.
	/// </summary>
	internal class Log
	{
		private const string LOGCONFIG = "~/RssBandit.Services.log4net.config";
		/// <summary>
		/// static initializer
		/// </summary>
		static Log()
		{
			// Set up a simple configuration that logs on the console.
		    string logConfigFile = Log4NetConfigFile;
            
            if (File.Exists(logConfigFile))
            {
                XmlConfigurator.Configure(new FileInfo(logConfigFile));
			}
			else 
            {
				BasicConfigurator.Configure();
			}
		}
		
		/// <summary>
		///  The Full Path to the Config File
		/// </summary>
		public static string Log4NetConfigFile 
        {
			get { 
				string cfgFile = ConfigurationManager.AppSettings["log4net.config"];
				if (string.IsNullOrEmpty(cfgFile))
					return HostingEnvironment.MapPath(LOGCONFIG);
			    
                return HostingEnvironment.MapPath(cfgFile);
			}
		}

		/// <summary>
		///  If you want to use your own ILog variable in your class, initialize
		///  them with a instance returned by this method.
		/// </summary>
		/// <param name="type">System.Type</param>
		/// <returns>Instance of a logger impl. ILog</returns>
		public static ILog GetLogger(Type type) {
			return LogManager.GetLogger(type);
		}

	}
	/// <summary>
	/// Supports trimmed formatting of string parameters: {0:&lt;T=nnn;}
	/// e.g. {0:T=40} will trim the agument to a length of 40.
	/// </summary>
	internal class StringFormatter: IFormatProvider, ICustomFormatter {
		
		#region IFormatProvider Members

		public object GetFormat(Type formatType) {
			if (formatType == typeof (ICustomFormatter)) {
				return this;
			}
			else {
				return null;
			}
		}

		#endregion

		#region ICustomFormatter Members

		public string Format(string format, object arg, IFormatProvider formatProvider) {
			string s = arg as string;
			if (s == null) {
				return "<null>";
			}
			if (format == null) {
				return String.Format("{0}", s);
			}
			try {
				if (format.StartsWith("T=")) {
					int n = Int32.Parse(format.Substring(2));
					if (n < s.Length)
						return String.Format("{0}...<len:{1}>", s.Substring(0, n), s.Length);
					return String.Format("{0}", s);
				} else {
					return String.Format("{0}", s);
				}
			}
			catch (NotSupportedException) {
				return String.Format("{0}", s);
			}
		}

		#endregion
	}

}
