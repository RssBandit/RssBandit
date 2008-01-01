#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
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
		/// Initializes a new instance of the 
		/// <see cref="SerializationInfoReader"/> class.
		/// </summary>
		/// <param name="info">The info.</param>
		public SerializationInfoReader(SerializationInfo info) {
			moInfo = info;
			keys = new StringCollection();
			if (moInfo!=null) {
				foreach (SerializationEntry entry in moInfo) {
					keys.Add(entry.Name);
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
			try {
				if (keys.Contains(name)) {
					return moInfo.GetBoolean(name);
				}
				else {
					return defaultValue;
				}
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
			try {
				if (keys.Contains(name)) {
					return moInfo.GetByte(name);
				}
				else {
					return defaultValue;
				}
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
			try {
				if (keys.Contains(name)) {
					return moInfo.GetChar(name);
				}
				else {
					return defaultValue;
				}
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
			try {
				if (keys.Contains(name)) {
					return moInfo.GetDateTime(name);
				}
				else {
					return defaultValue;
				}

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
			try {
				if (keys.Contains(name)) {
					return moInfo.GetDecimal(name);
				}
				else {
					return defaultValue;
				}
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
			try {
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
				else {
					return defaultValue;
				}
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
			try {
				if (keys.Contains(name)) {
					return moInfo.GetInt16(name);
				}
				else {
					return defaultValue;
				}
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
			try {
				if (keys.Contains(name)) {
					return moInfo.GetInt32(name);
				}
				else {
					return defaultValue;
				}
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
			try {
				if (keys.Contains(name)) {
					return moInfo.GetInt64(name);
				}
				else {
					return defaultValue;
				}
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
			try {
				if (keys.Contains(name)) {
					return moInfo.GetSingle(name);
				}
				else {
					return defaultValue;
				}
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
			try {
				if (keys.Contains(name)) {
					return moInfo.GetString(name);
				}
				else {
					return defaultValue;
				}
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
			try {
				if (keys.Contains(name)) {
					return moInfo.GetValue(name, type);
				}
				else {
					return defaultValue;
				}
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
			try {
				if (keys.Contains(name)) {
					FontConverter oFontConv = new FontConverter();
					string sFont = moInfo.GetString(name);
					return oFontConv.ConvertFromString(null, CultureInfo.InvariantCulture, sFont) as Font;
				}
				else {
					return defaultValue;
				}
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
			try {
				if (keys.Contains(name)) {
					byte[] sImage = (byte[])moInfo.GetValue(name,typeof(byte[]));
					return ConvertBytesToImage(sImage);
				}
				else {
					return defaultValue;
				}
			}
			catch {
				return defaultValue;
			}
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
