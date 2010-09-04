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
using System.ComponentModel;
using System.Configuration;

namespace NewsComponents.Utils
{
	/// <summary>
	/// Common static helper functions.
	/// </summary>
	public static class Common
	{
		/// <summary>
		/// Gets the current framework version.
		/// </summary>
		public static readonly Version ClrVersion = Environment.Version;

		public class Configuration
		{
			/// <summary>
			/// Reads an app settings entry. Can be used to init the command line
			/// ivars with settings from a app.config or User.App.config.
			/// Preferred calls should be located in the constructor to init the
			/// ivars, so user provided command line params can override that
			/// initialization.
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="name">The name of the entry.</param>
			/// <param name="defaultValue">The default value.</param>
			/// <returns>Value read or defaultValue</returns>
			/// <exception cref="ConfigurationErrorsException">On type conversion failures</exception>
			public static T ReadAppSettingsEntry<T>(string name, T defaultValue)
			{
				if (string.IsNullOrEmpty(name))
					return defaultValue;

				Type t = typeof(T);
				string value = ConfigurationManager.AppSettings[name];

				if (!string.IsNullOrEmpty(value))
				{
					if (t == typeof(string))
						return (T)(object)value;
					try
					{
						TypeConverter converter = TypeDescriptor.GetConverter(t);
						if (converter != null && converter.CanConvertFrom(typeof(string)))
							return (T)converter.ConvertFrom(value);
					}
					catch (Exception ex)
					{
						throw new ConfigurationErrorsException(String.Format("Configuration value '{1}' with key '{0}' cannot be converted to target type '{2}': {3}", name, value, t.FullName, ex.Message), ex);
					}
					throw new ConfigurationErrorsException(String.Format("Configuration value '{1}' with key '{0}' cannot be converted to target type '{2}'", name, value, t.FullName));

				}
				return defaultValue;
			}
		}

	}
}


