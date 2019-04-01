#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;

namespace NewsComponents.Utils
{
	/// <summary>
	/// Simple class that reads a SerializationInfo object 
	/// with default values
	/// </summary>
	public class SerializationInfoReader 
	{
		private readonly SerializationInfo _info;
		private readonly StringCollection _keys;
		private readonly StreamingContext _context;

		/// <summary>
		/// Gets access to the keys.
		/// </summary>
		/// <value>The keys.</value>
		public StringCollection Keys {
			get {
				return _keys;
			}
		}

		/// <summary>
		/// Gets the Serialization Info.
		/// </summary>
		/// <value>The info.</value>
		public SerializationInfo Info {
			get {
				return _info;
			}
		}

		/// <summary>
		/// Gets the context.
		/// </summary>
		/// <value>The context.</value>
		public StreamingContext Context
		{
			get { return _context; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializationInfoReader"/> class.
		/// </summary>
		/// <param name="info">The mo info.</param>
		/// <param name="keys">The keys.</param>
		public SerializationInfoReader(SerializationInfo info, StringCollection keys)
		{
			this._info = info;
			this._keys = keys;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializationInfoReader"/> class.
		/// </summary>
		/// <param name="info">The info.</param>
		/// <param name="context">The context.</param>
		public SerializationInfoReader(SerializationInfo info, StreamingContext context)
		{
			this._context = context;
			_info = info;
			if (context.Context is StringCollection)
			{
				_keys = (StringCollection)context.Context;
			}
			else
			{
				_keys = new StringCollection();
				if (_info != null)
				{
					foreach (SerializationEntry entry in _info)
					{
						_keys.Add(entry.Name);
					}
				}
			}
		}

		/// <summary>
		/// Determines whether it contains the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>
		/// 	<c>true</c> if it contains the specified key; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(string key) 
		{
			return _keys.Contains(key);
		}

		/// <summary>
		/// Get Value
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		private object GetValue(string name, Type type, object defaultValue) 
		{
			try
			{
				if (_keys.Contains(name)) {
					return _info.GetValue(name, type);
				}
				return defaultValue;
			}
			catch {
				return defaultValue;
			}
		}

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public T Get<T>(string name, T defaultValue)
        {
            try
            {
                if (_keys.Contains(name))
                {
                    if (typeof(T).IsEnum)
                    {
                        var value = (string)_info.GetValue(name, typeof(string));

                        return (T)Enum.Parse(typeof(T), value);
                    }
                    else
                    {
                        return (T)_info.GetValue(name, typeof(T));
                    }
                }
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }


        /// <summary>
        /// Gets a Font, but only if it was serialized with a call to
        /// SerializationInfoReader.ConvertFont().
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public Font GetFont(string name, Font defaultValue) 
		{
			try
			{
				if (_keys.Contains(name)) {
					var oFont = new FontConverter();
					var sFont = _info.GetString(name);
					return oFont.ConvertFromString(null, CultureInfo.InvariantCulture, sFont) as Font;
				}
				return defaultValue;
			}
			catch {
				return defaultValue;
			}
		}

		/// <summary>
		/// Gets a image.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public Image GetImage(string name, Image defaultValue) {
			try
			{
				if (_keys.Contains(name)) {
					byte[] sImage = (byte[])_info.GetValue(name,typeof(byte[]));
					return ConvertBytesToImage(sImage);
				}
				return defaultValue;
			}
			catch {
				return defaultValue;
			}
		}

		/// <summary>
		/// Gets the generic dictionary.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public Dictionary<TKey, TValue> GetGenericDictionary<TKey, TValue>(string name, Dictionary<TKey, TValue> defaultValue)
		{
			try
			{
				return GetGenericDictionary(_context, this, name, defaultValue);
			}
			catch
			{
				return defaultValue;
			}
		}

		/// <summary>
		/// Gets the generic dictionary.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public SortedList<TKey, TValue> GetGenericDictionary<TKey, TValue>(string name, SortedList<TKey, TValue> defaultValue)
		{
			try
			{
				return GetGenericDictionary(_context, this, name, defaultValue);
			}
			catch
			{
				return defaultValue;
			}
		}
		/// <summary>
		/// Gets the generic list.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public List<TValue> GetGenericList<TValue>(string name, List<TValue> defaultValue)
		{
			try
			{
				return GetGenericList(_context, this, name, defaultValue);
			}
			catch
			{
				return defaultValue;
			}
		}

		/// <summary>
		/// Adds the generic list.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="info">The info.</param>
		/// <param name="key">The key.</param>
		/// <param name="list">The list.</param>
		public static void AddGenericList<TValue>(StreamingContext context, SerializationInfo info, string key, List<TValue> list)
		{
			// Persitence is soap so don't save generic 
			if (context.State == StreamingContextStates.Persistence)
			{
				TValue[] values;
				if (list != null)
				{
					values = new TValue[list.Count];
					int pos = 0;
					foreach (TValue value in list)
					{
						values[pos] = value;
						pos++;
					}
				}
				else
				{
					values = null;
				}
				info.AddValue(key, values, typeof(TValue[]));
			}
			else
			{
				info.AddValue(key, list, typeof(List<TValue>));
			}
		}

		/// <summary>
		/// Gets the generic list.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="reader">The reader.</param>
		/// <param name="key">The key.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public static List<TValue> GetGenericList<TValue>(StreamingContext context, SerializationInfoReader reader, string key, List<TValue> defaultValue)
		{
			List<TValue> list;
			if (context.State == StreamingContextStates.Persistence)
			{
				TValue[] values = (TValue[])reader.GetValue(key, typeof(TValue[]), null);
				if (values == null)
				{
					return defaultValue;
				}

				list = new List<TValue>(values);
			}
			else
			{
				list = (List<TValue>)reader.GetValue(key, typeof(List<TValue>), defaultValue);
			}
			return list;
		}

		/// <summary>
		/// Adds the generic dictionary.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="info">The info.</param>
		/// <param name="key">The key.</param>
		/// <param name="dictionary">The dictionary.</param>
		public static void AddGenericDictionary<TKey, TValue>(StreamingContext context, SerializationInfo info, string key, Dictionary<TKey, TValue> dictionary)
		{
			// Persistence is soap so don't save generic 
			if (context.State == StreamingContextStates.Persistence)
			{
				TKey[] keys;
				TValue[] values;
				if (dictionary != null)
				{
					keys = new TKey[dictionary.Count];
					values = new TValue[dictionary.Count];
					int pos = 0;
					foreach (KeyValuePair<TKey, TValue> entry in dictionary)
					{
						keys[pos] = entry.Key;
						values[pos] = entry.Value;
						pos++;
					}
				}
				else
				{
					keys = null;
					values = null;
				}

				info.AddValue(key + "_" + "value", values, typeof(TValue[]));
				info.AddValue(key + "_" + "key", keys, typeof(TKey[]));
			}
			else
			{
				info.AddValue(key, dictionary, typeof(Dictionary<TKey, TValue>));
			}
		}


		/// <summary>
		/// Gets the generic dictionary.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="reader">The reader.</param>
		/// <param name="key">The key.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public static Dictionary<TKey, TValue> GetGenericDictionary<TKey, TValue>(StreamingContext context, SerializationInfoReader reader, string key, Dictionary<TKey, TValue> defaultValue)
		{
			Dictionary<TKey, TValue> dictionary;
			if (context.State == StreamingContextStates.Persistence)
			{
				TKey[] keys = (TKey[])reader.GetValue(key + "_" + "key", typeof(TKey[]), null);
				if (keys == null)
				{
					return defaultValue;
				}
				TValue[] values = (TValue[])reader.GetValue(key + "_" + "value", typeof(TValue[]), null);
				dictionary = new Dictionary<TKey, TValue>(keys.Length);
				for (int iPos = 0; iPos < keys.Length; iPos++)
				{
					TKey link = keys[iPos];
					TValue objectLink = values[iPos];
					dictionary.Add(link, objectLink);
				}
			}
			else
			{
				dictionary = (Dictionary<TKey, TValue>)reader.GetValue(key, typeof(Dictionary<TKey, TValue>), defaultValue);
			}
			return dictionary;
		}

		/// <summary>
		/// Adds the generic dictionary.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="info">The info.</param>
		/// <param name="key">The key.</param>
		/// <param name="dictionary">The dictionary.</param>
		public static void AddGenericDictionary<TKey, TValue>(StreamingContext context, SerializationInfo info, string key, SortedList<TKey, TValue> dictionary)
		{
			// Persistence is soap so don't save generic 
			if (context.State == StreamingContextStates.Persistence)
			{
				TKey[] keys;
				TValue[] values;
				if (dictionary != null)
				{
					keys = new TKey[dictionary.Count];
					values = new TValue[dictionary.Count];
					int pos = 0;
					foreach (KeyValuePair<TKey, TValue> entry in dictionary)
					{
						keys[pos] = entry.Key;
						values[pos] = entry.Value;
						pos++;
					}
				}
				else
				{
					keys = null;
					values = null;
				}

				info.AddValue(key + "_" + "value", values, typeof(TValue[]));
				info.AddValue(key + "_" + "key", keys, typeof(TKey[]));
			}
			else
			{
				info.AddValue(key, dictionary, typeof(SortedList<TKey, TValue>));
			}
		}

		/// <summary>
		/// Gets the generic dictionary.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="reader">The reader.</param>
		/// <param name="key">The key.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public static SortedList<TKey, TValue> GetGenericDictionary<TKey, TValue>(StreamingContext context, SerializationInfoReader reader, string key, SortedList<TKey, TValue> defaultValue)
		{
			SortedList<TKey, TValue> dictionary;
			if (context.State == StreamingContextStates.Persistence)
			{
				TKey[] keys = (TKey[])reader.GetValue(key + "_" + "key", typeof(TKey[]), null);
				if (keys == null)
				{
					return defaultValue;
				}
				TValue[] values = (TValue[])reader.GetValue(key + "_" + "value", typeof(TValue[]), null);
				dictionary = new SortedList<TKey, TValue>(keys.Length);
				for (int iPos = 0; iPos < keys.Length; iPos++)
				{
					TKey link = keys[iPos];
					TValue objectLink = values[iPos];
					dictionary.Add(link, objectLink);
				}
			}
			else
			{
				dictionary = (SortedList<TKey, TValue>)reader.GetValue(key, typeof(SortedList<TKey, TValue>), defaultValue);
			}
			return dictionary;
		}
		/// <summary>
		/// Adds the value.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		/// <param name="type">The type.</param>
		public void AddValue(string name, object value,Type type) 
		{
			if (!_keys.Contains(name)) {
				_keys.Add(name);
				_info.AddValue(name,value,type);
			}
		}

		/// <summary>
		/// Adds the value.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		public void AddValue(string name, object value)
		{
			if (!_keys.Contains(name)) {
				_keys.Add(name);
				_info.AddValue(name,value);
			}
		}

		/// <summary>
		/// Adds the value.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		/// <param name="type">The type.</param>
		/// <param name="defaultValue">The default value.</param>
		public void AddValue(string name, object value, Type type, object defaultValue) 
		{
			if (value!=defaultValue) {
				this.AddValue(name,value,type);
			}
		}

		/// <summary>
		/// Adds the value.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		/// <param name="defaultValue">The default value.</param>
		public void AddValue(string name, object value, object defaultValue) 
		{
			if (value!=defaultValue) {
				this.AddValue(name,value);
			}
		}

		/// <summary>
		/// Returns the Version Number of the assembly
		/// </summary>
		/// <returns></returns>
		public Version VersionNumber() 
		{
			return GetVersion(_info);
		}

		/// <summary>
		/// Gets the version.
		/// </summary>
		/// <param name="info">The info.</param>
		/// <returns></returns>
		static public Version GetVersion(SerializationInfo info) 
		{
			string assemblyName = info.AssemblyName;
			//	assemblyName is in the form of "MyAssembly, Version=1.2.3.4,Culture=neutral, PublicKeyToken=null"	
			char[] separators = { ',', '=' };
			string[] nameParts = assemblyName.Split(separators);
			return new Version(nameParts[2]);
		}

		/// <summary>
		/// Convert Font to a serializable string
		/// </summary>
		/// <param name="font"></param>
		/// <returns></returns>
		public static string ConvertFont(Font font) 
		{
			FontConverter oFontConv = new FontConverter();
			return oFontConv.ConvertToString(null,CultureInfo.InvariantCulture,font);
		}
		
		/// <summary>
		/// Converts byte array to an image.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		/// <returns></returns>
		public static Image ConvertBytesToImage(byte[] bytes) 
		{
			if (bytes!=null) {
				MemoryStream stream = new MemoryStream(bytes);
				return Image.FromStream(stream);
			}
			return null;
		}
		
		/// <summary>
		/// Converts the image to byte array.
		/// </summary>
		/// <param name="image">The image.</param>
		/// <returns></returns>
		public static byte[] ConvertImageToBytes(Image image) 
		{
			if (image!=null) 
			{
				MemoryStream stream = new MemoryStream();
				image.Save(stream,image.RawFormat);
				
				return stream.ToArray();
			}
			return null;
		}
	}

}
