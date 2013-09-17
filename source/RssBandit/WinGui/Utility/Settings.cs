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
using System.Collections;
using System.Configuration;
using System.IO;
using System.Xml;
using Genghis;
using JetBrains.Annotations;
using NewsComponents;
using NewsComponents.Utils;
using RssBandit.AppServices.Configuration;
using RssBandit.Common.Logging;

namespace RssBandit.WinGui.Utility
{
	/// <summary>
	/// Helper to save/restore Gui Settings (other than User Preferences).
	/// This includes such things like Window size and position, panel sizes, dock layout etc.
	/// </summary>
	internal class Settings : Preferences, IPersistedSettings
	{

		private readonly System.Collections.Specialized.StringDictionary _userStore;
		private bool _userStoreModified;

		private readonly string _settingsFilePath;

		private static readonly log4net.ILog _log = Log.GetLogger(typeof(Settings));

		public Settings([NotNull]string settingsFilePath, string domain)
			: base(domain)
		{
			settingsFilePath.ExceptionIfNullOrEmpty("settingsFilePath");

			_settingsFilePath = settingsFilePath;

			if (_userStore == null)
			{
				_userStore = new System.Collections.Specialized.StringDictionary();

				// Load preferences.
				Deserialize();
				_userStoreModified = false;
			}
		}
		
		#region IPersistedSettings

		/// <summary>
		/// Gets the property value.
		/// </summary>
		/// <typeparam name="T">Type of the property value</typeparam>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		T IPersistedSettings.GetProperty<T>(string propertyName, T defaultValue)
		{
			return (T)GetProperty(propertyName, defaultValue, typeof(T));
		}

		/// <summary>
		/// Sets a property
		/// </summary>
		/// <param name="name">The property name.<br/>
		/// Use slash (/) to logically separate groups of settings.</param>
		/// <param name="value">The property value.</param>
		void IPersistedSettings.SetProperty(string name, object value)
		{
			this.SetProperty(name, value);
		}

		#endregion

		#region public overrides

		public override Preferences GetSubnode(string subpath)
		{
			// Create a new instance of the same store as this.
			return new Settings(_settingsFilePath, base.path + ValidatePath(subpath, "subpath"));
		}
		
		/// <summary>
		/// Sets a property
		/// </summary>
		/// <param name="name">The property name.<br/>
		/// Use slash (/) to logically separate groups of settings.</param>
		/// <param name="value">The property value.</param>
		/// <remarks>
		/// Currently, the value parameter must be a type supported by the
		/// System.Convert class.  The supported types are: Boolean, Char, SByte,
		/// Byte, Int16, Int32, Int64, UInt16, UInt32, UInt64, Single, Double,
		/// Decimal, DateTime and String.</remarks>
		public override void SetProperty(string name, object value)
		{
			if (value == null && _userStore.ContainsKey(Path + name))
			{
				_userStore.Remove(Path + name);
				return;
			}
			_userStore[Path + name] = Convert.ToString(value);
			_userStoreModified = true;
		}

		public override object GetProperty(string name, object defaultValue, Type returnType)
		{
			string value = _userStore[Path + name];
			if (value == null)
				return defaultValue;
			try
			{
				return Convert.ChangeType(value, returnType);
			}
			catch (Exception e)
			{
				_log.Debug("Settings: The property " + name + " could not be converted to the intended type (" + returnType + ").  Using defaults.");
				_log.Debug("Settings: The exception was:", e);
				return defaultValue;
			}
		}


		/// <summary>
		/// Flushes any outstanding properties to disk.</summary>
		public override void Flush()
		{
			Serialize();
		}

		/// <summary>Close resources and flush content (if needed) </summary>
		public new void Close()
		{
			base.Close();
			Flush();
		}

		#endregion

		#region private

		private FileStream CreateSettingsStream()
		{
			return FileHelper.OpenForWrite(_settingsFilePath); 
		}

		/// <summary>
		/// Opens a read-only stream on the backing store.</summary>
		/// <returns>
		/// A stream to read from.</returns>
		private FileStream OpenSettingsStream()
		{
			return FileHelper.OpenForRead(_settingsFilePath); 
		}

		/// <summary>Deserializes to the userStore hashtable from an storage stream.</summary>
		/// <remarks>Exceptions are silently ignored.</remarks>
		private void Deserialize()
		{
			XmlTextReader reader = null;
			try
			{
				reader = new XmlTextReader(OpenSettingsStream());

				// Read name/value pairs.
				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element && reader.Name == "property")
					{
						string name = reader.GetAttribute("name");
						string value = reader.ReadString();
						_userStore[name] = value;
					}
				}

				reader.Close();
			}
			catch (Exception e)
			{
				// Release all resources held by the XmlTextReader.
				if (reader != null)
					reader.Close();

				// Report exception.
				_log.Debug("Settings: There was an error while deserializing from Settings Storage.  Ignoring.");
				_log.Debug("Settings: The exception was:", e);
			}
		}

		/// <summary>Serializes the userStore hashtable to an storage stream.</summary>
		/// <remarks>Exceptions are silently ignored.</remarks>
		private void Serialize()
		{
			if (_userStoreModified == false)
				return;

			XmlTextWriter writer = null;
			try
			{
				writer = new XmlTextWriter(CreateSettingsStream(), null /* Encoding.Unicode */);

				// Write stream to console.
				//XmlTextWriter writer = new XmlTextWriter(Console.Out);

				// Use indentation for readability.
				writer.Formatting = Formatting.Indented;
				writer.Indentation = 4;

				writer.WriteStartDocument(true);
				writer.WriteStartElement("settings");

				// Write properties.
				foreach (DictionaryEntry entry in _userStore)
				{
					writer.WriteStartElement("property");
					writer.WriteAttributeString("name", (string) entry.Key);
					writer.WriteString((string) entry.Value);
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Close();

				// No longer modified compared to the copy on disk.
				_userStoreModified = false;
			}
			catch (Exception e)
			{
				// Release all resources held by the XmlTextWriter.
				if (writer != null)
					writer.Close();

				// Report exception.
				_log.Debug("Settings: There was an error while serializing to Storage.  Ignoring.");
				_log.Debug("Settings: The exception was:", e);
			}
		}

		#endregion

	}
}