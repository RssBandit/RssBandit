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
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Genghis;
using JetBrains.Annotations;
using log4net;
using NewsComponents.Utils;
using RssBandit.AppServices.Configuration;
using RssBandit.Common.Logging;

namespace RssBandit.WinGui.Utility
{
	/// <summary>
	/// "P"ersisted "s"etting consts (Global)
	/// </summary>
	internal static class Ps
	{
		public const string LastAutoUpdateCheck = "LastAutoUpdateCheck"; // datetime
		
		// migration keys are for backward compat. (read old formats and convert in background)
		public const string UnreadItemsSearchFoldersMigrationRequired = "UnreadItemsSearchFolders.migrationRequired.to.1.9"; //bool
		public const string WatchedItemsFeedMigrationRequired = "WatchedItemsFeed.migrationRequired.to.1.7";
		public const string SentItemsFeedMigrationRequired = "SentItemsFeed.migrationRequired.to.1.7";
		public const string DeletedItemsFeedMigrationRequired = "DeletedItemsFeed.migrationRequired.to.1.7";
		public const string FlaggedItemsFeedMigrationRequired = "FlaggedItemsFeed.migrationRequired.to.1.7";
		/// <summary>
		/// As long the FlagStatus of NewsItem's wasn't persisted all the time in the feed, 
		/// we have to re-init the feed item's FlagStatus from the flagged items collection:
		/// </summary>
		public const string FlaggedItemsFeedSelfHealingFlagStatusRequired = "FlaggedItemsFeed.RunSelfHealing.FlagStatus"; //bool

		public const string FtpConnectionModePassive = "Ftp.ConnectionMode.Passive"; //bool,default: true

	}

	#region GlobalSettings

	/// <summary>
	/// Helper to save/restore Global Settings (other than User Preferences and Ui State).
	/// </summary>
	public class GlobalSettings : Preferences, IPersistedSettings
	{
		private static StringDictionary _userStore;
		private static bool _userStoreModified;
		private static string _settingsFilePath;

		private static readonly ILog _log = Log.GetLogger(typeof (GlobalSettings));

		#region ctor's

		public GlobalSettings([NotNull] string settingsFilePath, string domain)
			: base(domain)
		{
			settingsFilePath.ExceptionIfNullOrEmpty("settingsFilePath");
			_settingsFilePath = settingsFilePath;

			if (_userStore == null)
			{
				_userStore = new StringDictionary();

				// Load preferences.
				Deserialize();
				_userStoreModified = false;
				// Flush the preferences on application exit.
				Application.ApplicationExit += OnApplicationExit;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UiStateSettings"/> class.
		/// Used within Preferences.GetUserNode()
		/// </summary>
		/// <param name="domain">The domain.</param>
		public GlobalSettings(string domain)
			: base(domain)
		{
			if (_userStore == null)
			{
				_userStore = new StringDictionary();

				// Load preferences.
				Deserialize();
				_userStoreModified = false;
			}
		}

		#endregion

		#region IPersistedSettings

		/// <summary>
		/// Gets the property value.
		/// </summary>
		/// <typeparam name="T">Type of the property value</typeparam>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public T GetProperty<T>(string propertyName, T defaultValue)
		{
			return (T) GetProperty(propertyName, defaultValue, typeof (T));
		}

		#endregion

		#region public overrides

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

			if (value is DateTime)
			{
				_userStore[Path + name] = ((DateTime) value).ToString("u");
			}
			else
			{
				_userStore[Path + name] = Convert.ToString(value);
			}

			_userStoreModified = true;
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
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
				_log.Debug("Settings: The property " + name + " could not be converted to the intended type (" + returnType +
				           ").  Using defaults.");
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

		/// <summary>
		/// Flush any outstanding preferences data on application exit.</summary>
		private static void OnApplicationExit(object sender, EventArgs e)
		{
			Serialize();
		}

		private static Stream CreateSettingsStream()
		{
			return FileHelper.OpenForWrite(_settingsFilePath);
		}

		/// <summary>
		/// Opens a read-only stream on the backing store.</summary>
		/// <returns>
		/// A stream to read from.</returns>
		private static Stream OpenSettingsStream()
		{
			try
			{
				if (File.Exists(_settingsFilePath))
					return FileHelper.OpenForRead(_settingsFilePath);
			}
			catch
			{
				/* all */
			}

			return new MemoryStream();
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
				_log.Debug("Settings: There was an error while deserializing from GlobalSettings Storage. Ignoring.");
				_log.Debug("Settings: The exception was:", e);
			}
		}

		/// <summary>Serializes the userStore hashtable to an storage stream.</summary>
		/// <remarks>Exceptions are silently ignored.</remarks>
		private static void Serialize()
		{
			if (_userStoreModified == false)
				return;

			XmlTextWriter writer = null;
			try
			{
				writer = new XmlTextWriter(CreateSettingsStream(), null /* Encoding.Unicode */);

				// Use indentation for readability.
				writer.Formatting = Formatting.Indented;
				writer.Indentation = 2;

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
				_log.Debug("Settings: There was an error while serializing to Storage. Ignoring.");
				_log.Debug("Settings: The exception was:", e);
			}
		}

		#endregion
	}

	#endregion

	#region UiStateSettings

	/// <summary>
	/// Helper to save/restore Gui State Settings (other than User Preferences).
	/// This includes such things like Window size and position, panel sizes, dock layout etc.
	/// </summary>
	public class UiStateSettings : Preferences, IPersistedSettings
	{
		private static StringDictionary _userStore;
		private static bool _userStoreModified;
		private static string _settingsFilePath;

		private static readonly ILog _log = Log.GetLogger(typeof (UiStateSettings));

		#region ctor's

		public UiStateSettings([NotNull] string settingsFilePath, string domain)
			: base(domain)
		{
			settingsFilePath.ExceptionIfNullOrEmpty("settingsFilePath");
			_settingsFilePath = settingsFilePath;

			if (_userStore == null)
			{
				_userStore = new StringDictionary();

				// Load preferences.
				Deserialize();
				_userStoreModified = false;
				// Flush the preferences on application exit.
				Application.ApplicationExit += new EventHandler(OnApplicationExit);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UiStateSettings"/> class.
		/// Used within Preferences.GetUserNode()
		/// </summary>
		/// <param name="domain">The domain.</param>
		public UiStateSettings(string domain)
			: base(domain)
		{
			if (_userStore == null)
			{
				_userStore = new StringDictionary();

				// Load preferences.
				Deserialize();
				_userStoreModified = false;
			}
		}

		#endregion

		#region IPersistedSettings

		/// <summary>
		/// Gets the property value.
		/// </summary>
		/// <typeparam name="T">Type of the property value</typeparam>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public T GetProperty<T>(string propertyName, T defaultValue)
		{
			return (T) GetProperty(propertyName, defaultValue, typeof (T));
		}

		#endregion

		#region public overrides

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

			if (value is DateTime)
			{
				_userStore[Path + name] = ((DateTime) value).ToString("u");
			}
			else
			{
				_userStore[Path + name] = Convert.ToString(value);
			}

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
				_log.Debug("Settings: The property " + name + " could not be converted to the intended type (" + returnType +
				           ").  Using defaults.");
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

		/// <summary>
		/// Flush any outstanding preferences data on application exit.</summary>
		private static void OnApplicationExit(object sender, EventArgs e)
		{
			Serialize();
		}

		private static Stream CreateSettingsStream()
		{
			return FileHelper.OpenForWrite(_settingsFilePath);
		}

		/// <summary>
		/// Opens a read-only stream on the backing store.</summary>
		/// <returns>
		/// A stream to read from.</returns>
		private static Stream OpenSettingsStream()
		{
			try
			{
				if (File.Exists(_settingsFilePath))
					return FileHelper.OpenForRead(_settingsFilePath);
			}
			catch
			{
				/* all */
			}

			return new MemoryStream();
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
				_log.Debug("Settings: There was an error while deserializing from Settings Storage. Ignoring.");
				_log.Debug("Settings: The exception was:", e);
			}
		}

		/// <summary>Serializes the userStore hashtable to an storage stream.</summary>
		/// <remarks>Exceptions are silently ignored.</remarks>
		private static void Serialize()
		{
			if (_userStoreModified == false)
				return;

			XmlTextWriter writer = null;
			try
			{
				writer = new XmlTextWriter(CreateSettingsStream(), null /* Encoding.Unicode */);

				// Use indentation for readability.
				writer.Formatting = Formatting.Indented;
				writer.Indentation = 2;

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

	#endregion

}