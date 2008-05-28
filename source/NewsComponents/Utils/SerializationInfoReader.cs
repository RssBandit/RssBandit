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
	public class SerializationInfoReader {
		private readonly SerializationInfo moInfo;
		private readonly StringCollection keys;
		private readonly StreamingContext context;

		/// <summary>
		/// Gets access to the keys.
		/// </summary>
		/// <value>The keys.</value>
		public StringCollection Keys {
			get {
				return keys;
			}
		}

		/// <summary>
		/// Gets the Serialization Info.
		/// </summary>
		/// <value>The info.</value>
		public SerializationInfo Info {
			get {
				return moInfo;
			}
		}

		/// <summary>
		/// Gets the context.
		/// </summary>
		/// <value>The context.</value>
		public StreamingContext Context
		{
			get { return context; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializationInfoReader"/> class.
		/// </summary>
		/// <param name="moInfo">The mo info.</param>
		/// <param name="keys">The keys.</param>
		public SerializationInfoReader(SerializationInfo moInfo, StringCollection keys)
		{
			this.moInfo = moInfo;
			this.keys = keys;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerializationInfoReader"/> class.
		/// </summary>
		/// <param name="info">The info.</param>
		/// <param name="context">The context.</param>
		public SerializationInfoReader(SerializationInfo info, StreamingContext context)
		{
			this.context = context;
			moInfo = info;
			if (context.Context is StringCollection)
			{
				keys = (StringCollection)context.Context;
			}
			else
			{
				keys = new StringCollection();
				if (moInfo != null)
				{
					foreach (SerializationEntry entry in moInfo)
					{
						keys.Add(entry.Name);
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
		public bool Contains(string key) {
			return keys.Contains(key);
		}

		/// <summary>
		/// Gets a boolean.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="defaultValue">the default value.</param>
		/// <returns></returns>
		public bool GetBoolean(string name, bool defaultValue) {
			try
			{
				if (keys.Contains(name)) {
					return moInfo.GetBoolean(name);
				}
				return defaultValue;
			}
			catch {
				return defaultValue;
			}
		}

		/// <summary>
		/// Get Byte
		/// </summary>
		/// <param name="name"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public byte GetByte(string name, byte defaultValue) {
			try
			{
				if (keys.Contains(name)) {
					return moInfo.GetByte(name);
				}
				return defaultValue;
			}
			catch {
				return defaultValue;
			}
		}

		/// <summary>
		/// Get Char
		/// </summary>
		/// <param name="name"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public char GetChar(string name, char defaultValue) {
			try
			{
				if (keys.Contains(name)) {
					return moInfo.GetChar(name);
				}
				return defaultValue;
			}
			catch {
				return defaultValue;
			}
		}

		/// <summary>
		/// Get Date Time
		/// </summary>
		/// <param name="name"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public DateTime GetDateTime(string name, DateTime defaultValue) {
			try
			{
				if (keys.Contains(name)) {
					return moInfo.GetDateTime(name);
				}
				return defaultValue;
			}
			catch {
				return defaultValue;
			}
		}

		/// <summary>
		/// Get Decimal
		/// </summary>
		/// <param name="name"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public decimal GetDecimal(string name, decimal defaultValue) {
			try
			{
				if (keys.Contains(name)) {
					return moInfo.GetDecimal(name);
				}
				return defaultValue;
			}
			catch {
				return defaultValue;
			}
		}

		/// <summary>
		/// Get Double
		/// </summary>
		/// <param name="name"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public double GetDouble(string name, double defaultValue) {
			try
			{
				if (keys.Contains(name)) {
					// avoid exception
					string number = moInfo.GetString(name);
					switch (number) {
						case "NaN":
							return Double.NaN;
						default:
							return Convert.ToDouble(number,CultureInfo.InvariantCulture);
					}
				}
				return defaultValue;
			}
			catch {
				return defaultValue;
			}
		}

		/// <summary>
		/// Get Short
		/// </summary>
		/// <param name="name"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public short GetShort(string name, short defaultValue) {
			try
			{
				if (keys.Contains(name)) {
					return moInfo.GetInt16(name);
				}
				return defaultValue;
			}
			catch {
				return defaultValue;
			}
		}

		/// <summary>
		/// Get Int
		/// </summary>
		/// <param name="name"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public int GetInt(string name, int defaultValue) {
			try
			{
				if (keys.Contains(name)) {
					return moInfo.GetInt32(name);
				}
				return defaultValue;
			}
			catch {
				return defaultValue;
			}
		}

		/// <summary>
		/// Get Long
		/// </summary>
		/// <param name="name"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public long GetLong(string name, long defaultValue) {
			try
			{
				if (keys.Contains(name)) {
					return moInfo.GetInt64(name);
				}
				return defaultValue;
			}
			catch {
				return defaultValue;
			}
		}

		/// <summary>
		/// Get Single
		/// </summary>
		/// <param name="name"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public float GetSingle(string name, float defaultValue) {
			try
			{
				if (keys.Contains(name)) {
					return moInfo.GetSingle(name);
				}
				return defaultValue;
			}
			catch {
				return defaultValue;
			}
		}

		/// <summary>
		/// GetString
		/// </summary>
		/// <param name="name"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public string GetString(string name, string defaultValue) {
			try
			{
				if (keys.Contains(name)) {
					return moInfo.GetString(name);
				}
				return defaultValue;
			}
			catch {
				return defaultValue;
			}
		}

		/// <summary>
		/// Get Value
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public object GetValue(string name, Type type, object defaultValue) {
			try
			{
				if (keys.Contains(name)) {
					return moInfo.GetValue(name, type);
				}
				return defaultValue;
			}
			catch {
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
		public Font GetFont(string name, Font defaultValue) {
			try
			{
				if (keys.Contains(name)) {
					FontConverter oFontConv = new FontConverter();
					string sFont = moInfo.GetString(name);
					return oFontConv.ConvertFromString(null, CultureInfo.InvariantCulture, sFont) as Font;
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
				if (keys.Contains(name)) {
					byte[] sImage = (byte[])moInfo.GetValue(name,typeof(byte[]));
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
		/// <typeparam name="Key">The type of the key.</typeparam>
		/// <typeparam name="Value">The type of the value.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public Dictionary<Key, Value> GetGenericDictionary<Key, Value>(string name, Dictionary<Key, Value> defaultValue)
		{
			try
			{
				return GetGenericDictionary(context, this, name, defaultValue);
			}
			catch
			{
				return defaultValue;
			}
		}

		/// <summary>
		/// Gets the generic dictionary.
		/// </summary>
		/// <typeparam name="Key">The type of the key.</typeparam>
		/// <typeparam name="Value">The type of the value.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public SortedList<Key, Value> GetGenericDictionary<Key, Value>(string name, SortedList<Key, Value> defaultValue)
		{
			try
			{
				return GetGenericDictionary(context, this, name, defaultValue);
			}
			catch
			{
				return defaultValue;
			}
		}
		/// <summary>
		/// Gets the generic list.
		/// </summary>
		/// <typeparam name="Value">The type of the value.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns></returns>
		public List<Value> GetGenericList<Value>(string name, List<Value> defaultValue)
		{
			try
			{
				return GetGenericList(context, this, name, defaultValue);
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
		public static void AddGenericList<Value>(StreamingContext context, SerializationInfo info, string key, List<Value> list)
		{
			// Persitence is soap so don't save generic 
			if (context.State == StreamingContextStates.Persistence)
			{
				Value[] values;
				if (list != null)
				{
					values = new Value[list.Count];
					int pos = 0;
					foreach (Value value in list)
					{
						values[pos] = value;
						pos++;
					}
				}
				else
				{
					values = null;
				}
				info.AddValue(key, values, typeof(Value[]));
			}
			else
			{
				info.AddValue(key, list, typeof(List<Value>));
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
		public static List<Value> GetGenericList<Value>(StreamingContext context, SerializationInfoReader reader, string key, List<Value> defaultValue)
		{
			List<Value> list;
			if (context.State == StreamingContextStates.Persistence)
			{
				Value[] values = (Value[])reader.GetValue(key, typeof(Value[]), null);
				if (values == null)
				{
					return defaultValue;
				}

				list = new List<Value>(values);
			}
			else
			{
				list = (List<Value>)reader.GetValue(key, typeof(List<Value>), defaultValue);
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
		public static void AddGenericDictionary<Key, Value>(StreamingContext context, SerializationInfo info, string key, Dictionary<Key, Value> dictionary)
		{
			// Persitence is soap so don't save generic 
			if (context.State == StreamingContextStates.Persistence)
			{
				Key[] keys;
				Value[] values;
				if (dictionary != null)
				{
					keys = new Key[dictionary.Count];
					values = new Value[dictionary.Count];
					int pos = 0;
					foreach (KeyValuePair<Key, Value> entry in dictionary)
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

				info.AddValue(key + "_" + "value", values, typeof(Value[]));
				info.AddValue(key + "_" + "key", keys, typeof(Key[]));
			}
			else
			{
				info.AddValue(key, dictionary, typeof(Dictionary<Key, Value>));
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
		public static Dictionary<Key, Value> GetGenericDictionary<Key, Value>(StreamingContext context, SerializationInfoReader reader, string key, Dictionary<Key, Value> defaultValue)
		{
			Dictionary<Key, Value> dictionary;
			if (context.State == StreamingContextStates.Persistence)
			{
				Key[] keys = (Key[])reader.GetValue(key + "_" + "key", typeof(Key[]), null);
				if (keys == null)
				{
					return defaultValue;
				}
				Value[] values = (Value[])reader.GetValue(key + "_" + "value", typeof(Value[]), null);
				dictionary = new Dictionary<Key, Value>(keys.Length);
				for (int iPos = 0; iPos < keys.Length; iPos++)
				{
					Key link = keys[iPos];
					Value objectLink = values[iPos];
					dictionary.Add(link, objectLink);
				}
			}
			else
			{
				dictionary = (Dictionary<Key, Value>)reader.GetValue(key, typeof(Dictionary<Key, Value>), defaultValue);
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
		public static void AddGenericDictionary<Key, Value>(StreamingContext context, SerializationInfo info, string key, SortedList<Key, Value> dictionary)
		{
			// Persitence is soap so don't save generic 
			if (context.State == StreamingContextStates.Persistence)
			{
				Key[] keys;
				Value[] values;
				if (dictionary != null)
				{
					keys = new Key[dictionary.Count];
					values = new Value[dictionary.Count];
					int pos = 0;
					foreach (KeyValuePair<Key, Value> entry in dictionary)
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

				info.AddValue(key + "_" + "value", values, typeof(Value[]));
				info.AddValue(key + "_" + "key", keys, typeof(Key[]));
			}
			else
			{
				info.AddValue(key, dictionary, typeof(SortedList<Key, Value>));
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
		public static SortedList<Key, Value> GetGenericDictionary<Key, Value>(StreamingContext context, SerializationInfoReader reader, string key, SortedList<Key, Value> defaultValue)
		{
			SortedList<Key, Value> dictionary;
			if (context.State == StreamingContextStates.Persistence)
			{
				Key[] keys = (Key[])reader.GetValue(key + "_" + "key", typeof(Key[]), null);
				if (keys == null)
				{
					return defaultValue;
				}
				Value[] values = (Value[])reader.GetValue(key + "_" + "value", typeof(Value[]), null);
				dictionary = new SortedList<Key, Value>(keys.Length);
				for (int iPos = 0; iPos < keys.Length; iPos++)
				{
					Key link = keys[iPos];
					Value objectLink = values[iPos];
					dictionary.Add(link, objectLink);
				}
			}
			else
			{
				dictionary = (SortedList<Key, Value>)reader.GetValue(key, typeof(SortedList<Key, Value>), defaultValue);
			}
			return dictionary;
		}
		public void AddValue(string name, object value,Type type) {
			if (!keys.Contains(name)) {
				keys.Add(name);
				moInfo.AddValue(name,value,type);
			}
		}

		public void AddValue(string name, object value) {
			if (!keys.Contains(name)) {
				keys.Add(name);
				moInfo.AddValue(name,value);
			}
		}

		public void AddValue(string name, object value, Type type, object defaultValue) {
			if (value!=defaultValue) {
				this.AddValue(name,value,type);
			}
		}

		public void AddValue(string name, object value, object defaultValue) {
			if (value!=defaultValue) {
				this.AddValue(name,value);
			}
		}

		/// <summary>
		/// Returns the Version Number of the assembly
		/// </summary>
		/// <returns></returns>
		public Version VersionNumber() {
			return GetVersion(moInfo);
		}

		/// <summary>
		/// Gets the version.
		/// </summary>
		/// <param name="info">The info.</param>
		/// <returns></returns>
		static public Version GetVersion(SerializationInfo info) {
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
		public static string ConvertFont(Font font) {
			FontConverter oFontConv = new FontConverter();
			return oFontConv.ConvertToString(null,CultureInfo.InvariantCulture,font);
		}
		
		/// <summary>
		/// Converts byte array to an image.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		/// <returns></returns>
		public static Image ConvertBytesToImage(byte[] bytes) {
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
		public static byte[] ConvertImageToBytes(Image image) {
			if (image!=null) {
				MemoryStream stream = new MemoryStream();
				image.Save(stream,image.RawFormat);
				
				return stream.ToArray();
			}
			return null;
		}
	}

}
