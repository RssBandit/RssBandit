#region Using directives

using System;
using System.Configuration;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml;
using System.Diagnostics;
using Timer = System.Threading.Timer;
using TimerCallback = System.Threading.TimerCallback;
using StringDictionary = System.Collections.Specialized.StringDictionary;

#endregion

namespace Genghis
{
//-----------------------------------------------------------------------------
//<filedescription file="Preferences.cs" company="Microsoft">
//  <copyright>
//     Copyright (c) 2004 Microsoft Corporation.  All rights reserved.
//  </copyright>
//  <purpose>
//  Contains Preferences used in WindowSerializer Class 
//  </purpose>
//  <notes>
//  </notes>
//</filedescription>                                                                
//-----------------------------------------------------------------------------

    /// <summary>
    /// Provides a way to persist user preferences.</summary>
    /// <remarks>
    /// By default, this class uses Isolated Storage to provide provide portable
    /// and safe persistance.<br/>
    /// It is envisioned that in the future, alternate backing stores will be
    /// available (the registry would be an obvious one).
    /// </remarks>
    ///
    /// <example>
    /// Here's an example of how to persist the persist the personal details of a
    /// user (for registration purposes, perhaps).
    /// <code>
    /// Preferences prefWriter = Preferences.GetUserNode("Personal Details");
    /// prefWriter.SetProperty("Name", "Joe Bloggs");
    /// prefWriter.SetProperty("Age", 56);
    /// prefWriter.Close();
    /// </code>
    ///
    /// And here's an example of how to read these properties back in.
    /// <code>
    /// Preferences prefReader = Preferences.GetUserNode("Personal Details");
    /// string name = prefReader.GetString("Name", "Anonymous");
    /// int age = prefReader.GetInt32("Age", 0);
    /// prefReader.Close();
    /// </code>
    /// </example>
    public abstract class Preferences : IDisposable
    {
        static Type backingStore = null;    // The back-end data storage class.
        protected string path;

        /// <summary>
        /// Constructs a preferences writer at the root.</summary>
        protected Preferences()
        {
            path = "";
        }

        /// <summary>
        /// Constructs a preferences writer with a path.</summary>
        /// <param name="domain">
        /// The path under which the preferences will be saved.</param>
        protected Preferences(string domain)
        {
            this.path = ValidatePath(domain, "path");
        }

        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Disposes of any resources.</summary>
        /// <remarks>
        /// Equivalent to calling Dispose().</remarks>
        public void Close()
        {
            Dispose();
        }

		/// <summary>
		/// Validates the path argument.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="argumentName">Name of the argument.</param>
		/// <returns></returns>
        protected string ValidatePath(string path, string argumentName)
        {
            if (path.Length > 0 && path[path.Length - 1] != '/')
            {
                path = path + '/';
            }
			return path;
        }

        public string Path
        {
            get { return path; }
        }

        /// <summary>
        /// Gets a property</summary>
        /// <param name="path">
        /// The property name.<br/>
        /// Use slash (/) to logically separate groups of settings.</param>
        /// <param name="defaultValue">
        /// The default property value.  If no previous property exists, or the
        /// preferences store is unavailable, this value will be returned.</param>
        /// <param name="returnType">
        /// The return type. This must be a type
        /// supported by the System.Convert class.  The supported types are:
        /// Boolean, Char, SByte, Byte, Int16, Int32, Int64, UInt16, UInt32,
        /// UInt64, Single, Double, Decimal, DateTime and String.</param>
        /// <returns>
        /// Returns the property value (with the same type as returnType).</returns>
        public abstract object GetProperty(string path, object defaultValue, Type returnType);

        /// <summary>
        /// Gets a property</summary>
        /// <param name="name">
        /// The property name.<br/>
        /// Use slash (/) to logically separate groups of
        /// settings.</param>
        /// <param name="defaultValue">
        /// The default property value.  If no previous property exists, or the
        /// preferences store is unavailable, this value will be returned.</param>
        /// <returns>
        /// Returns the property value (with the same type as defaultValue).</returns>
        /// <remarks>
        /// The return type is converted to the same type as the defaultValue
        /// argument before it is returned.  Therefore, this must be a type
        /// supported by the System.Convert class.  The supported types are:
        /// Boolean, Char, SByte, Byte, Int16, Int32, Int64, UInt16, UInt32,
        /// UInt64, Single, Double, Decimal, DateTime and String.</remarks>
        public object GetProperty(string name, object defaultValue)
        {
            if (defaultValue == null)
            {
                throw new ArgumentNullException("defaultValue");
            }
            return GetProperty(name, defaultValue, defaultValue.GetType());
        }

        // Convenience helper methods.

        public string GetString(string name, string defaultValue)
        {
            return (string)GetProperty(name, defaultValue, typeof(string));
        }

        public bool GetBoolean(string name, bool defaultValue)
        {
            return (bool)GetProperty(name, defaultValue, typeof(bool));
        }

        public int GetInt32(string name, int defaultValue)
        {
            return (int)GetProperty(name, defaultValue, typeof(int));
        }

        public double GetInt64(string name, long defaultValue)
        {
            return (long)GetProperty(name, defaultValue, typeof(long));
        }

        public float GetSingle(string name, float defaultValue)
        {
            return (float)GetProperty(name, defaultValue, typeof(float));
        }

        public double GetDouble(string name, double defaultValue)
        {
            return (double)GetProperty(name, defaultValue, typeof(double));
        }



        /// <summary>
        /// Sets a property</summary>
        /// <param name="name">
        /// The property name.<br/>
        /// Use slash (/) to logically separate groups of settings.</param>
        /// <param name="value">
        /// The property value.</param>
        /// <remarks>
        /// Currently, the value parameter must be a type supported by the
        /// System.Convert class.  The supported types are: Boolean, Char, SByte,
        /// Byte, Int16, Int32, Int64, UInt16, UInt32, UInt64, Single, Double,
        /// Decimal, DateTime and String.</remarks>
        public abstract void SetProperty(string name, object value);


        /// <summary>
        /// Flushes any outstanding property data to disk.</summary>
        public abstract void Flush();

        /// <summary>
        /// Opens a new subnode to store settings under.
        /// </summary>
        /// <param name="subpath">The path of the new subnode.</param>
        /// <remarks>The subnode is created if it doesn't already exist.</remarks>
        public virtual Preferences GetSubnode(string subpath)
        {
            // Create a new instance of the same store as this.
            return (Preferences)Activator.CreateInstance(GetType(), new object[] { path + ValidatePath(subpath, "subpath") });
        }



        /// <summary>
        /// Constructs a preferences object at the root of the per-user
        /// settings.</summary>
        public static Preferences GetUserRoot()
        {
            return GetUserNode("");
        }

        /// <summary>
        /// Constructs a per-user preferences object from a class.</summary>
        /// <param name="type">
        /// The class you want to store settings for.  All the
        /// periods in the name will be converted to slashes.</param>
        public static Preferences GetUserNode(Type type)
        {
            string path = type.FullName;
            path = path.Replace('.', '/');
            return GetUserNode(path);
        }

        /// <summary>
        /// Constructs a per-user preferences object from a path.</summary>
        /// <param name="path">
        /// Represents the path the preferences are stored under.
        /// Equivalent to a directory path or a registry key path.<br/>
        /// <b>Important Note:</b> The seperator character is slash ('/')
        /// <b>NOT</b> backslash ('\').
        /// </param>
        /// <remarks>The path of the root node is "".</remarks>
        public static Preferences GetUserNode(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (backingStore == null)
            {
                try
                {
                    string backingStoreName = null;
                    try
                    {
                        // Read the fully-qualified type name of the backing store.
                        AppSettingsReader appSettings = new AppSettingsReader();
                        backingStoreName = (string)appSettings.GetValue("CustomPreferencesStore", typeof(string));
                    }
                    catch
                    {
                        Trace.WriteLine("No custom data store specified (in application settings file).  Using default.");
                        throw;
                    }

                    try
                    {
                        // Load the type and create a new instance.
                        backingStore = Type.GetType(backingStoreName, true);
                    }
                    catch
                    {
                        Trace.WriteLine("Could not load custom data store " + backingStoreName + ".  Using default.");
                        throw;
                    }
                }
                catch
                {
                    // Use the default backing store (isolated storage).
                    backingStore = typeof(IsolatedStorageUserPreferencesStore);
                }
            }

            // Create an instance of the backing store.
            return (Preferences)Activator.CreateInstance(backingStore, new object[] { path });
        }
    }

    /// <summary>
    /// A back-end preferences implementation using Isolated Storage as the
    /// underlying storage mechanism.</summary>
    /// <remarks>
    /// This implementation has the following properties:
    /// <list type="bullet">
    /// <item><description>Reads and writes are involve a single hashtable
    /// access, and are thus very fast.</description></item>
    /// <item><description>The backing file is read once on startup, and
    /// written once on shutdown (using the Application.ApplicationExit event).
    /// </description></item>
    /// </list>
    /// </remarks>
    class IsolatedStorageUserPreferencesStore : Preferences
    {
        private readonly StringDictionary _userStore;
        private bool _userStoreModified;
        //static StringDictionary machineStore;
        //static bool machineStoreModified;

        /// <summary>Initializes instance variables and loads initial settings from
        /// the backing store.</summary>
        /// <param name="domain">
        /// Represents the name of the group the preferences are stored under.
        /// Roughly equivalent to a directory path or a registry key path.
        /// You can nest groups using the slash (/) character.
        /// "" (the empty string) represents the top-level group.  A slash (/)
        /// will be added to the end of the path if it is lacking one.</param>
        public IsolatedStorageUserPreferencesStore(string domain) : base(domain)
        {
            if (_userStore == null)
            {
                _userStore = new StringDictionary();

                // Load preferences.
                Deserialize();
                _userStoreModified = false;

                // Flush the preferences on application exit.
                System.Windows.Forms.Application.ApplicationExit += new EventHandler(OnApplicationExit);
            }
        }

        /// <summary>
        /// Gets a property</summary>
        /// <param name="name">
        /// The property name.<br/>
        /// Use slash (/) to logically separate groups of settings.</param>
        /// <param name="defaultValue">
        /// The default property value.  If no previous property exists, or the
        /// preferences store is unavailable, this value will be returned.</param>
        /// <param name="returnType">
        /// The return type. This must be a type
        /// supported by the System.Convert class.  The supported types are:
        /// Boolean, Char, SByte, Byte, Int16, Int32, Int64, UInt16, UInt32,
        /// UInt64, Single, Double, Decimal, DateTime and String.</param>
        /// <returns>
        /// Returns the property value (with the same type as returnType).</returns>
        public override object GetProperty(string name, object defaultValue, Type returnType)
        {
            string value = _userStore[Path + name];
            if (value == null)
            {
                return defaultValue;
            }
            try
            {
                return Convert.ChangeType(value, returnType);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Genghis.Preferences: The property " + name + " could not be converted to the intended type (" + returnType + ").  Using defaults.");
                Trace.WriteLine("Genghis.Preferences: The exception was: " + e.Message);
                return defaultValue;
            }
        }

        /// <summary>
        /// Sets a property</summary>
        /// <param name="name">
        /// The property name.<br/>
        /// Use slash (/) to logically separate groups of settings.</param>
        /// <param name="value">
        /// The property value.</param>
        /// <remarks>
        /// Currently, the value parameter must be a type supported by the
        /// System.Convert class.  The supported types are: Boolean, Char, SByte,
        /// Byte, Int16, Int32, Int64, UInt16, UInt32, UInt64, Single, Double,
        /// Decimal, DateTime and String.</remarks>
        public override void SetProperty(string name, object value)
        {
            _userStore[Path + name] = Convert.ToString(value);
            _userStoreModified = true;
        }


        /// <summary>
        /// Flushes any outstanding properties to disk.</summary>
        public override void Flush()
        {
            Serialize();
        }


        /// <summary>
        /// Flush any outstanding preferences data on application exit.</summary>
        private void OnApplicationExit(object sender, EventArgs e)
        {
            Serialize();
        }

        /// <summary>
        /// Creates a write-only stream on the backing store.</summary>
        /// <returns>
        /// A stream to write to.</returns>
        private static IsolatedStorageFileStream CreateSettingsStream()
        {
            // TODO: Check for permission to do roaming.
            // Roaming stores require higher permissions.
            // If we are not allowed, use IsolatedStorageFile.GetUserStoreForDomain() instead.

            IsolatedStorageFile store =
                IsolatedStorageFile.GetStore(
                    IsolatedStorageScope.User |
                    IsolatedStorageScope.Assembly |
                    IsolatedStorageScope.Domain |
                    IsolatedStorageScope.Roaming,
                    null, null);

            return new IsolatedStorageFileStream("preferences.xml",
                FileMode.Create, store);
        }

        /// <summary>
        /// Opens a read-only stream on the backing store.</summary>
        /// <returns>
        /// A stream to read from.</returns>
        private static IsolatedStorageFileStream OpenSettingsStream()
        {
            // TODO: Check for permission to do roaming.
            // Roaming stores require higher permissions.
            // If we are not allowed, use IsolatedStorageFile.GetUserStoreForDomain() instead.

            IsolatedStorageFile store =
                IsolatedStorageFile.GetStore(
                    IsolatedStorageScope.User |
                    IsolatedStorageScope.Assembly |
                    IsolatedStorageScope.Domain |
                    IsolatedStorageScope.Roaming,
                    null, null);

            return new IsolatedStorageFileStream("preferences.xml",
                FileMode.Open, store);
        }

        /// <summary>Deserializes to the userStore hashtable from an isolated storage stream.</summary>
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
                Trace.WriteLine("Genghis.Preferences: There was an error while deserializing from Isolated Storage.  Ignoring.");
                Trace.WriteLine("Genghis.Preferences: The exception was: " + e.Message);
                Trace.WriteLine(e.StackTrace);
            }
        }

        /// <summary>Serializes the userStore hashtable to an isolated storage stream.</summary>
        /// <remarks>Exceptions are silently ignored.</remarks>
        private void Serialize()
        {
            if (_userStoreModified == false)
            {
                return;
            }

            XmlTextWriter writer = null;
            try
            {
                writer = new XmlTextWriter(CreateSettingsStream(), null);

                // Write stream to console.
                //XmlTextWriter writer = new XmlTextWriter(Console.Out);

                // Use indentation for readability.
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;

                writer.WriteStartDocument(true);
                writer.WriteStartElement("preferences");

                // Write properties.
                foreach (System.Collections.DictionaryEntry entry in _userStore)
                {
                    writer.WriteStartElement("property");
                    writer.WriteAttributeString("name", (string)entry.Key);
                    writer.WriteString((string)entry.Value);
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
                {
                    writer.Close();
                }

                // Report exception.
                Trace.WriteLine("Genghis.Preferences: There was an error while serializing to Isolated Storage.  Ignoring.");
                Trace.WriteLine("Genghis.Preferences: The exception was: " + e.Message);
                Trace.WriteLine(e.StackTrace);
            }
        }
    }
}
