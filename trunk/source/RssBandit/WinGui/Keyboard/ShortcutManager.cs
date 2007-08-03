using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace RssBandit.Utility.Keyboard
{
	/// <summary>
	/// Summary description for ShortcutManager.
	/// </summary>
	public class ShortcutManager : NameObjectCollectionBase
	{
		#region Private Members
		XmlSchema _schema;
		bool _validationErrorOccured;
		StringCollection _shortcutsToShow = new StringCollection();
		#endregion

		/// <summary>
		/// Constructor.  Loads in the schema and Settings.
		/// </summary>
		public ShortcutManager()
		{
			using(Stream stream = Resource.Manager.GetStream("Resources.ShortcutSettings.xsd")) 
			{
				_schema = XmlSchema.Read(stream, null);
				stream.Close();
			}
		}

		/// <summary>
		/// Access by Index.
		/// </summary>
		public System.Windows.Forms.Shortcut this[string commandName] 
		{
			get 
			{
				object shortcut = BaseGet(commandName);
				if(shortcut != null)
                    return (System.Windows.Forms.Shortcut)shortcut; 
				else
					return System.Windows.Forms.Shortcut.None;
			}
			set 
			{
				this.BaseSet(commandName, (System.Windows.Forms.Shortcut)value );
			}
		}

		public bool IsDisplayed(string commandName)
		{
			return this._shortcutsToShow.Contains(commandName);
		}

		/// <summary>
		/// Loads the shortcut settings from an xml file.
		/// </summary>
		/// <param name="path"></param>
		public void LoadSettings(string path)
		{
			if(!File.Exists(path))
				throw new IOException("Settings File '" + path + "' Not Found!");
			
			using(FileStream stream = File.OpenRead(path))
			{
				LoadSettings(stream);
			}
		}

		/// <summary>
		/// Loads the shortcut settings from a stream.
		/// </summary>
		/// <param name="settingsStream"></param>
		public void LoadSettings(Stream settingsStream)
		{
			BaseClear();
				
			XmlDocument doc = LoadValidatedDocument(settingsStream);

			if(_validationErrorOccured)
				throw new ApplicationException("Shortcut Settings File Could not be validated!");

			ShortcutSettings settings = DeserializeSettings(doc);
			PopulateFromSettings(settings);
		}

		private XmlDocument LoadValidatedDocument(Stream stream)
		{
			XmlDocument doc = new XmlDocument();

			XmlValidatingReader reader = new XmlValidatingReader(new XmlTextReader(stream));

			reader.Schemas.Add(_schema); 
			reader.ValidationType = ValidationType.Schema;

			reader.ValidationEventHandler += new ValidationEventHandler(reader_ValidationEventHandler);
			_validationErrorOccured = false; 

			doc.Load(reader);
			reader.Close();
			return doc;
		}

		private ShortcutSettings DeserializeSettings(XmlDocument doc)
		{
			//convert XML to objects 
			XmlNodeReader reader = new XmlNodeReader(doc);		
			XmlSerializer serializer = new XmlSerializer(typeof(ShortcutSettings));
			ShortcutSettings settings = serializer.Deserialize(reader) as ShortcutSettings; 
			reader.Close();
			return settings;
		}

		private void reader_ValidationEventHandler(object sender, ValidationEventArgs e)
		{
			_validationErrorOccured = true;
		}

		private void PopulateFromSettings(ShortcutSettings settings)
		{
			foreach(ShortcutSetting setting in settings.shortcut)
			{
				this[setting.commandName] = (System.Windows.Forms.Shortcut)Enum.Parse(typeof(System.Windows.Forms.Shortcut), setting.keys.ToString(), true);
				if(setting.display)
					_shortcutsToShow.Add(setting.commandName);
			}
		}
	}
}