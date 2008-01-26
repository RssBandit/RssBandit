using System;
using System.IO;
using System.Xml;

namespace Microsoft.Office.OneNote
{
	/// <summary>
	/// Abstract Class. Every PageObject and OutlineContent has an underlying Data object. 
	/// You must create an appropriate Data object for your ink, image or html and reference
	/// it in your PageObject or OutlineContent.
	/// 
	/// Currently, there are three concrete classes you can use for this: <see cref="FileData"/>, <see cref="StringData"/>, and
	/// <see cref="BinaryData"/>.
	/// </summary>
	[Serializable]	
	public abstract class Data : ImportNode
	{
		/// <summary>
		///  Initializes a new empty instance of the <see cref="Data"/> class.
		/// </summary>
		protected Data()
		{
		}

		/// <summary>
		/// Clones the Data, returning a new instance with the same value.
		/// </summary>
		/// <returns>A new Data with the same value as the original.</returns>
		public override object Clone()
		{
			return MemberwiseClone();
		}

		/// <summary>
		/// Determines whether or not the specified object is equal to the 
		/// current data.  
		/// </summary>
		/// <param name="obj">
		/// The object to compare with the current data.
		/// </param>
		/// <returns>
		/// true if the specified object contains the same data; false otherwise
		/// </returns>
		public override bool Equals(object obj)
		{
			Data other = obj as Data;
			if (other == null)
				return data.Equals(obj);

			return other.data.Equals(data);
		}

		/// <summary>
		/// Serves as a hash function for the <see cref="Data"/>, suitable for use 
		/// in hashing algorithms and data structures like a hash table.  
		/// </summary>
		/// <returns>A hash code for the current <see cref="Data"/>.</returns>
		public override int GetHashCode()
		{
			return data.GetHashCode();
		}

		/// <summary>
		/// Returns a <see cref="String"/> that represents the current <see cref="Data"/>.  
		/// </summary>
		/// <returns>
		/// A <see cref="String"/> that represents the current <see cref="Data"/>.
		/// </returns>
		public override string ToString()
		{
			return data.ToString();
		}

		/// <summary>
		/// Serializes the <see cref="Data"/> object to Xml in a format
		/// suitable for import into OneNote.
		/// </summary>
		/// <param name="parentNode">
		/// A reference to the parent XML node that we should serialize ourselves
		/// as a child of.
		/// </param>
		protected internal abstract override void SerializeToXml(XmlNode parentNode);

		/// <summary>
		/// While each of the concrete classes below need to handle encoding
		/// differences, they are currently all implemented on a good old 
		/// fashioned string.
		/// </summary>
		protected internal string data = null;
	}

	/* ---------------------------------------------------------------------- */

	/// <summary>
	/// Represents data stored in an external file. This could be an image such as a jpeg, an html file or 
	/// a file containing ISF format ink. This class is a concrete implementation of the abstract <see cref="Data"/> class.
	/// </summary>
	[Serializable]
	public class FileData : Data
	{
		/// <summary>
		/// Constructs a new <see cref="FileData"/> object representing the data
		/// in the specified file.
		/// </summary>
		/// <param name="file">The <see cref="FileInfo"/> containing our data.</param>
		public FileData(FileInfo file)
		{
			data = file.FullName;
		}

		/// <summary>
		/// Serializes the <see cref="FileData"/> object to Xml in a format
		/// suitable for import into OneNote.
		/// </summary>
		/// <param name="parentNode">
		/// A reference to the parent XML node that we should serialize ourselves
		/// as a child of.
		/// </param>
		protected internal override void SerializeToXml(XmlNode parentNode)
		{
			XmlDocument xmlDocument = parentNode.OwnerDocument;

			XmlElement element = xmlDocument.CreateElement("File");
			element.SetAttribute("path", data);

			parentNode.AppendChild(element);
		}
	}

	/* ---------------------------------------------------------------------- */

	/// <summary>
	/// Represents binary data, which is stored as Base64 data. This can be used for ink and image content.  
	/// </summary>
	[Serializable]
	public class BinaryData : Data
	{
		/// <summary>
		/// Constructs a new <see cref="BinaryData"/> object from the specified
		/// byte array.
		/// </summary>
		/// <param name="buffer">A buffer of binary data.</param>
		public BinaryData(byte[] buffer)
		{
			data = Convert.ToBase64String(buffer);
		}

		/// <summary>
		/// Serializes the <see cref="BinaryData"/> object to Xml in a format
		/// suitable for import into OneNote.
		/// </summary>
		/// <param name="parentNode">
		/// A reference to the parent XML node that we should serialize ourselves
		/// as a child of.
		/// </param>
		protected internal override void SerializeToXml(XmlNode parentNode)
		{
			XmlDocument xmlDocument = parentNode.OwnerDocument;

			XmlElement element = xmlDocument.CreateElement("Data");
			parentNode.AppendChild(element);

			XmlText text = xmlDocument.CreateTextNode(data);
			element.AppendChild(text);
		}
	}

	/* ---------------------------------------------------------------------- */

	/// <summary>
	/// Represents string data such as an HTML string. 
	/// This is a concrete implementation of the abstract <see cref="Data"/> class. 
	/// To add an HTML string to a page, you create a StringData with the HTML string, then add the StringData to an OutlineContent, which is then added to an OutlineObject which is added to the page.
	/// This process can be abbreviated using convenience construtors on the <see cref="HtmlContent"/> object.
	/// </summary>
	[Serializable]
	public class StringData : Data
	{
		/// <summary>
		/// Constructs a new <see cref="StringData"/> object from the specified
		/// string.
		/// </summary>
		/// <param name="data">Our data as a string.</param>
		public StringData(String data)
		{
			this.data = data;
		}

		/// <summary>
		/// Serializes the <see cref="StringData"/> object to Xml in a format
		/// suitable for import into OneNote.
		/// </summary>
		/// <param name="parentNode">
		/// A reference to the parent XML node that we should serialize ourselves
		/// as a child of.
		/// </param>
		protected internal override void SerializeToXml(XmlNode parentNode)
		{
			XmlDocument xmlDocument = parentNode.OwnerDocument;

			XmlElement element = xmlDocument.CreateElement("Data");
			parentNode.AppendChild(element);

			XmlCDataSection cdata = xmlDocument.CreateCDataSection(data);
			element.AppendChild(cdata);
		}
	}
}