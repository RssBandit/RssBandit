using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace RssBandit.Utility.Keyboard
{
	/// <summary>
	/// Class used to manage Menu shortcuts and Keyboard Combination shortcuts 
	/// for an application (in this case RSS Bandit).
	/// </summary>
	public sealed class ShortcutHandler
	{
		Hashtable _shortcuts = new Hashtable();
		Hashtable _displayedShortcuts = new Hashtable();
		string[] _availableMenuCommands = null;
		string[] _availableComboCommands = null;

		/// <summary>
		/// Creates a new <see cref="ShortcutHandler"/> instance.
		/// </summary>
		public ShortcutHandler()
		{}

		/// <summary>
		/// Loads the specified stream.
		/// </summary>
		/// <param name="stream">Stream.</param>
		public void Load(Stream stream)
		{
			try
			{
				_shortcuts.Clear();
				_displayedShortcuts.Clear();

				XmlTextReader reader = null;
				try
				{
					reader = new XmlTextReader(stream);
					PopulateShortcuts(reader);
				}
				finally
				{
					if(reader != null)
						reader.Close();
				}
			}
			catch(InvalidShortcutSettingsFileException)
			{
				throw;
			}
			catch(Exception e)
			{
				throw new InvalidShortcutSettingsFileException("The Shortcut Settings File is not valid.", e);
			}
		}

		/// <summary>
		/// Loads the specified path.
		/// </summary>
		/// <param name="path">Path.</param>
		public void Load(string path)
		{
			try
			{
				Load(File.OpenRead(path));
			}
			catch(FileNotFoundException e)
			{
				throw new InvalidShortcutSettingsFileException("The Shortcut Settings File was not found.", e);	
			}
		}

		/// <summary>
		/// Writes the specified path.
		/// </summary>
		/// <param name="path">Path.</param>
		public void Write(string path)
		{
			Write(path, Encoding.UTF8);
		}

		/// <summary>
		/// Writes the contents of this instance into a settings file.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="encoding"></param>
		public void Write(string path, Encoding encoding)
		{
			XmlTextWriter writer = null;
			try
			{
				writer = new XmlTextWriter(path, encoding);
				writer.Formatting = Formatting.Indented;
				writer.Indentation = 1;
				writer.IndentChar = '\t';
				writer.WriteStartDocument(true);
				writer.WriteStartElement("shortcuts");
				
				WriteMenuShortcuts(writer);
				WriteKeyComboShortcuts(writer);

				writer.WriteEndElement();
				writer.WriteEndDocument();										  
			}
			finally
			{
				if(writer != null)
					writer.Close();
			}
		}

		void WriteMenuShortcuts(XmlWriter writer)
		{
			writer.WriteStartElement("menu");
			
			foreach(string command in AvailableMenuCommands)
			{
				if(!_shortcuts.ContainsKey(command))
					continue;

				writer.WriteStartElement("shortcut");
				if(_displayedShortcuts.ContainsKey(command))
					writer.WriteAttributeString("display", "true");
				writer.WriteElementString("command", command);
				writer.WriteElementString("shortcutEnumValue", _shortcuts[command].ToString());
				writer.WriteEndElement();
			}

			writer.WriteEndElement();
		}

		void WriteKeyComboShortcuts(XmlWriter writer)
		{
			writer.WriteStartElement("keyboardCombinations");
			
			foreach(string command in AvailableKeyComboCommands)
			{
				if(!_shortcuts.ContainsKey(command))
					continue;

				writer.WriteStartElement("shortcut");
				writer.WriteElementString("command", command);

				ArrayList keyCombos = _shortcuts[command] as ArrayList;
				
				foreach(Keys key in keyCombos)
				{
					writer.WriteElementString("keyCombination", key.ToString());
				}
				
				writer.WriteEndElement();
			}

			writer.WriteEndElement();
		}

		/// <summary>
		/// Returns true if a shortcut for the command is defined.
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public bool IsDefined(string command)
		{
			return _shortcuts.ContainsKey(command);
		}

		/// <summary>
		/// Gets the key combinations associated with 
		/// the command.
		/// </summary>
		/// <param name="command">Command.</param>
		/// <returns></returns>
		public Keys[] GetKeyCombinations(string command)
		{
			ArrayList list = _shortcuts[command] as ArrayList;
			if(list == null)
				return new Keys[] {Keys.None};

			Keys[] keyCombos = new Keys[list.Count];
			int i = 0;
			foreach(Keys key in list)
			{
				keyCombos[i++] = key;
			}
			return keyCombos;
		}

		/// <summary>
		/// Removes the key combination for the specified command.
		/// </summary>
		/// <param name="command">Command.</param>
		/// <param name="index">index of the command.</param>
		public void RemoveKeyCombination(string command, int index)
		{
			ArrayList list = _shortcuts[command] as ArrayList;	
			list.RemoveAt(index);
		}

		/// <summary>
		/// Gets the shortcut for the specified command.
		/// </summary>
		/// <param name="command">Name of the command.</param>
		/// <returns></returns>
		public Shortcut GetShortcut(string command)
		{
			try
			{
				if(_shortcuts[command] != null)
					return (Shortcut)_shortcuts[command];
				else
					return Shortcut.None;
			}
			catch(InvalidCastException e)
			{
				throw new FormatException("The command \"" + command + "\" is incorrect.  Did not expect type " + _shortcuts[command].GetType().FullName, e);
			}
		}

		/// <summary>
		/// Sets the shortcut for the specified command.
		/// </summary>
		/// <param name="command">Command.</param>
		/// <param name="shortcut">Shortcut.</param>
		public void SetShortcut(string command, Shortcut shortcut)
		{
			SetShortcut(command, shortcut, false);
		}

		/// <summary>
		/// Sets the shortcut for the specified command.
		/// </summary>
		/// <param name="command">Command.</param>
		/// <param name="shortcut">Shortcut.</param>
		/// <param name="displayed">True if this shortcut is displayed on the menu</param>
		public void SetShortcut(string command, Shortcut shortcut, bool displayed)
		{
			_shortcuts[command] = shortcut;
			if(displayed)
				_displayedShortcuts[command] = string.Empty;
		}

		/// <summary>
		/// Returns true if the shortcut is visible, otherwise false.
		/// </summary>
		/// <param name="command">Command.</param>
		/// <returns></returns>
		public bool IsShortcutDisplayed(string command)
		{
			return _displayedShortcuts.ContainsKey(command);
		}

		/// <summary>
		/// Given a wParam property of a WM_KEYDOWN Message, returns true if the key combination 
		/// is mapped to the specified command.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="wParam">
		/// The Message wParam containing the Keys 
		/// flag corresponding to a key combination.
		/// </param>
		/// <returns></returns>
		public bool IsCommandInvoked(string command, IntPtr wParam)
		{
			if(!_shortcuts.ContainsKey(command))
				return false;

			Keys pressedKeys = ((Keys)(int)wParam | Control.ModifierKeys);
			return IsCommandInvoked(command, pressedKeys);
		}

		/// <summary>
		/// Given a keys bitflag, returns true if the key combination 
		/// is mapped to the specified command.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="keys"></param>
		/// <returns></returns>
		public bool IsCommandInvoked(string command, Keys keys)
		{
			if(!_shortcuts.ContainsKey(command))
				return false;

			try
			{
				ArrayList keyCombos = (ArrayList)_shortcuts[command];
				foreach(Keys keyCombo in keyCombos)
				{
					if(keys == keyCombo)
						return true;
				}
			}
			catch(InvalidCastException e)
			{
				throw new FormatException("The command \"" + command + "\" is incorrect.  Did not expect type " + _shortcuts[command].GetType().FullName, e);
			}
			return false;
		}

		#region Code to read the XML Settings file
		delegate void NodeReader(XmlReader reader);

		// This method is used to dig down into an XML structure.
		// Once it reaches the specified node, it calls the specified method.
		void ReadNode(XmlReader reader, string thisNodeName, NodeReader subNodeReaderDelegate, string subNodeName)
		{
			while(reader.Read())
			{
				XmlNodeType nodeType = reader.MoveToContent();
				if(nodeType == XmlNodeType.Element && reader.Name == subNodeName)
				{
					subNodeReaderDelegate(reader);
				}
				
				if(nodeType == XmlNodeType.EndElement && reader.Name == thisNodeName)
				{
					return;
				}
			}	
		}
		
		void PopulateShortcuts(XmlReader reader)
		{
			while(reader.Read())
			{
				XmlNodeType currentNodeType = reader.MoveToContent();
				if(currentNodeType == XmlNodeType.Element && reader.Name == "keyboardCombinations")
					ReadNode(reader, "keyboardCombinations", new NodeReader(ReadKeyComboShortcut), "shortcut");
				if(currentNodeType == XmlNodeType.Element && reader.Name == "menu")
					ReadNode(reader, "menu", new NodeReader(ReadMenuShortcut), "shortcut");
			}
		}

		void ReadKeyComboShortcut(XmlReader reader)
		{
			string command = string.Empty;
			Keys keyCombo = Keys.None;
			while(reader.Read())
			{
				XmlNodeType nodeType = reader.MoveToContent();
				if(nodeType == XmlNodeType.Element)
				{
					if(reader.Name == "command")
						command = reader.ReadString();
					else if(reader.Name == "keyCombination")
					{
						keyCombo = ReadKeyCombination(reader);
						this.AddKeyboardCombination(command, keyCombo);
					}
				}
				if(nodeType == XmlNodeType.EndElement && reader.Name == "shortcut")
				{
					return;
				}
			}
			Debug.Assert(false, "Should never reach here");
		}

		void ReadMenuShortcut(XmlReader reader)
		{
			string command = string.Empty;
			Shortcut shortcut = Shortcut.None;
			bool display = false;
			if(reader.GetAttribute("display") != null && reader.GetAttribute("display").Length > 0)
				display = bool.Parse(reader.GetAttribute("display"));
			
			while(reader.Read())
			{
				XmlNodeType nodeType = reader.MoveToContent();
				if(nodeType == XmlNodeType.Element)
				{
					if(reader.Name == "command")
					{
						command = reader.ReadString();
					}
					else if(reader.Name == "shortcutEnumValue")
					{
						try
						{
							shortcut = (Shortcut)Enum.Parse(typeof(Shortcut), reader.ReadString());
							AddShortcut(command, shortcut);
							if(display)
								_displayedShortcuts.Add(command, true);
						}
						catch(System.FormatException e)
						{
							//TODO: Log this.
							Console.WriteLine(e.Message);
						}
					}
				}
				if(nodeType == XmlNodeType.EndElement && reader.Name == "shortcut")
				{
					return;
				}
			}
			Debug.Assert(false, "Should never reach here");
		}

		/// <summary>
		/// Adds the keyboard combination.
		/// </summary>
		/// <param name="command">Command.</param>
		/// <param name="keys">Keys.</param>
		public void AddKeyboardCombination(string command, Keys keys)
		{
			if(!_shortcuts.ContainsKey(command))
			{
				ArrayList combos = new ArrayList();
				combos.Add(keys); //Initial value.
				_shortcuts.Add(command, combos);
				return;
				
			}

			ArrayList keyCombos = _shortcuts[command] as ArrayList;
			
			//Remove any pre-existing Keys.None values.
			for(int i = 0; i < keyCombos.Count; i++)
			{
				if((Keys)keyCombos[i] == Keys.None)
					keyCombos.RemoveAt(i);
			}

			//Only add it if it doesn't exist.
			if(!keyCombos.Contains(keys) && keys != Keys.None || keyCombos.Count == 0)
			{
				((ArrayList)_shortcuts[command]).Add(keys);
			}
		}

		void AddShortcut(string command, Shortcut shortcut)
		{
			if(_shortcuts.ContainsKey(command))
			{
				throw new DuplicateShortcutSettingException("A shortcut \"" + _shortcuts[command] + "\"for the command \"" + command + "\" already exists.", command);
			}
			
			_shortcuts.Add(command, shortcut);
		}
		
		Keys ReadKeyCombination(XmlReader reader)
		{
			return (Keys)Enum.Parse(typeof(Keys), reader.ReadString());
		}
		#endregion

		#region Commands Available for shortcuts.
		/// <summary>
		/// Gets a value indicating whether this instance contains 
		/// a shortcut for every available command.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [is complete]; otherwise, <c>false</c>.
		/// </value>
		public bool IsComplete
		{
			get
			{
				foreach(string command in AvailableMenuCommands)
				{
					if(!_shortcuts.ContainsKey(command))
						return false;
				}
				foreach(string command in AvailableKeyComboCommands)
				{
					if(!_shortcuts.ContainsKey(command))
						return false;
				}
				return true;
			}
		}

		/// <summary>
		/// Gets the missing shortcut commands that have 
		/// not been defined in the settings file.
		/// </summary>
		public string[] GetMissingShortcutCommands()
		{
			StringCollection missingCommands = new StringCollection();
			foreach(string command in AvailableMenuCommands)
			{
				if(!_shortcuts.ContainsKey(command))
					missingCommands.Add(command);
			}
			foreach(string command in AvailableKeyComboCommands)
			{
				if(!_shortcuts.ContainsKey(command))
					missingCommands.Add(command);
			}
			string[] result = new string[missingCommands.Count];
			missingCommands.CopyTo(result, 0);
			return result;
		}

		/// <summary>
		/// Gets the available shortcut commands.
		/// </summary>
		/// <remarks>
		/// This is a list of commands that are configured using 
		/// a <seealso cref="System.Windows.Forms.Shortcut"/> value.  
		/// The shortcut for these commands are used to set the Shortcut 
		/// property of the corresponding menu command.
		/// </remarks>
		public string[] AvailableMenuCommands
		{
			get
			{
				if(_availableMenuCommands == null)
					_availableMenuCommands = new string[] 
					{
						//Menu Commands
						"cmdNewSubscription",	// calls the wizard in general
						"cmdNewFeed",			// wizard for Url
						"cmdNewCategory",		
						"cmdNewNntpFeed",		// wizard for nntp subscription
						"cmdImportFeeds",
						"cmdExportFeeds",
						"cmdCloseExit",
						"cmdToggleOfflineMode",
						"cmdToggleTreeViewState",
						"cmdToggleRssSearchTabState",
						"cmdToggleMainTBViewState",
						"cmdToggleWebTBViewState",
						"cmdToggleWebSearchTBViewState",
                        "cmdLauchDownloadManager",
						"cmdRefreshFeeds",
						"cmdAutoDiscoverFeed",	// wizard for search
						"cmdFeedItemPostReply",
						"cmdUploadFeeds",
						"cmdDownloadFeeds",
						"cmdShowMainAppOptions",
						"cmdUpdateCategory",
						"cmdDeleteAll",
						"cmdRenameCategory",
						"cmdDeleteCategory",
						"cmdUpdateFeed",
						"cmdCatchUpCurrentSelectedNode",
						"cmdRenameFeed",
						"cmdDeleteFeed",
						"cmdCopyFeed",
						"cmdCopyFeedLinkToClipboard",
						"cmdCopyFeedHomepageLinkToClipboard",
						"cmdCopyFeedHomepageTitleLinkToClipboard",
						"cmdShowFeedProperties",
						"cmdHelpWebDoc",
						"cmdWorkspaceNews",
						"cmdReportBug",
						"cmdAbout",
						"cmdWikiNews",
						"cmdVisitForum",
						"cmdNavigateToFeedHome",
						"cmdNavigateToFeedCosmos",
						"cmdViewSourceOfFeed",
						"cmdValidateFeed",
						"cmdMarkFinderItemsRead",
						"cmdNewFinder",
						"cmdRenameFinder",
						"cmdRefreshFinder",
						"cmdDeleteFinder",
						"cmdDeleteAllFinders",
						"cmdShowFinderProperties",
						"cmdMarkSelectedFeedItemsUnread",
						"cmdMarkSelectedFeedItemsRead",
						"cmdCopyNewsItem",
						"cmdRestoreSelectedNewsItem",
						"cmdFlagNewsItem",
						"cmdFlagNewsItemForFollowUp",
						"cmdFlagNewsItemForReview",
						"cmdFlagNewsItemForReply",
						"cmdFlagNewsItemRead",
						"cmdFlagNewsItemForward",
						"cmdFlagNewsItemComplete",
						"cmdFlagNewsItemNone",
						"cmdCopyNewsItemLinkToClipboard",
						"cmdCopyNewsItemTitleLinkToClipboard",
						"cmdCopyNewsItemContentToClipboard",
						"cmdDeleteSelectedNewsItems",
						"cmdDeleteAllNewsItems",
						"cmdDocTabCloseThis",
						"cmdDocTabCloseAllOnStrip",
						"cmdDocTabCloseAll",
						"cmdDocTabLayoutHorizontal",
						"cmdFeedDetailLayoutPosition",
						"cmdFeedDetailLayoutPosTop",
						"cmdFeedDetailLayoutPosLeft",
						"cmdFeedDetailLayoutPosRight",
						"cmdFeedDetailLayoutPosBottom",
						"cmdSelectAllFeedItems",						
						"cmdShowGUI",
						"cmdShowConfiguredAlertWindows",
						"cmdShowAlertWindowNone",
						"cmdShowAlertWindowConfiguredFeeds",
						"cmdShowAlertWindowAll",
						"cmdShowNewItemsReceivedBalloon",
					};
				return _availableMenuCommands;
			}
		}

		/// <summary>
		/// Gets the available Keyboard Combination commands.
		/// </summary>
		/// <remarks>
		/// This is a list of commands that are configured using 
		/// <seealso cref="System.Windows.Forms.Keys"/> values.  
		/// These are called "Keyboard Combination" commands because they configure 
		/// commands to be invoked based on the <seealso cref="IMessageFilter"/> 
		/// filtering of a combination of simultaneous keystrokes.
		/// </remarks>
		public string[] AvailableKeyComboCommands
		{
			get
			{
				if(_availableComboCommands == null)
					_availableComboCommands = new string[] 
					{
						"ExpandListViewItem",
						"CollapseListViewItem",
						"RemoveDocTab",
						"CatchUpCurrentSelectedNode",
						"MarkFeedItemsUnread",
						"MoveToNextUnread",
						"InitiateRenameFeedOrCategory",
						"UpdateFeed",
						"GiveFocusToUrlTextBox",
						"GiveFocusToSearchTextBox",
						"DeleteItem",
						"BrowserCreateNewTab",						
						"Help"
					};
				return _availableComboCommands;
			}
		}
		#endregion
	}

	/// <summary>
	/// Exception thrown when trying to load an invalid 
	/// settings file.
	/// </summary>
	[Serializable]
	public class DuplicateShortcutSettingException : InvalidShortcutSettingsFileException
	{
		string _shortcutKey;

		/// <summary>
		/// Creates a new <see cref="DuplicateShortcutSettingException"/> instance.
		/// </summary>
		public DuplicateShortcutSettingException() : base()
		{}

		/// <summary>
		/// Creates a new <see cref="DuplicateShortcutSettingException"/> instance.
		/// </summary>
		/// <param name="message">Message.</param>
		public DuplicateShortcutSettingException(string message) : base(message)
		{}

		/// <summary>
		/// Creates a new <see cref="DuplicateShortcutSettingException"/> instance.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="shortcutKey">Shortcut key.</param>
		public DuplicateShortcutSettingException(string message, string shortcutKey) : base(message)
		{
			_shortcutKey = shortcutKey;
		}

		/// <summary>
		/// Creates a new <see cref="InvalidShortcutSettingsFileException"/> instance.
		/// </summary>
		/// <param name="info">Info.</param>
		/// <param name="context">Context.</param>
		protected DuplicateShortcutSettingException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_shortcutKey = info.GetString("sk");
		}

		/// <summary>
		/// Gets the shortcut key.
		/// </summary>
		/// <value></value>
		public string ShortcutKey
		{
			get { return _shortcutKey; }
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("sk", _shortcutKey);
		}
	}

	/// <summary>
	/// Exception thrown when trying to load an invalid 
	/// settings file.
	/// </summary>
	[Serializable]
	public class InvalidShortcutSettingsFileException : Exception
	{
		/// <summary>
		/// Creates a new <see cref="InvalidShortcutSettingsFileException"/> instance.
		/// </summary>
		public InvalidShortcutSettingsFileException() : base()
		{}

		/// <summary>
		/// Creates a new <see cref="InvalidShortcutSettingsFileException"/> instance.
		/// </summary>
		/// <param name="message">Message.</param>
		public InvalidShortcutSettingsFileException(string message) : base(message)
		{}

		/// <summary>
		/// Creates a new <see cref="InvalidShortcutSettingsFileException"/> instance.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="innerException">Inner exception.</param>
		public InvalidShortcutSettingsFileException(string message, Exception innerException) : base(message, innerException)
		{}

		/// <summary>
		/// Creates a new <see cref="InvalidShortcutSettingsFileException"/> instance.
		/// </summary>
		/// <param name="info">Info.</param>
		/// <param name="context">Context.</param>
		protected InvalidShortcutSettingsFileException(SerializationInfo info, StreamingContext context) : base(info, context)
		{}
	}
}