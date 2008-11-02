using System;
using System.IO;
using System.Xml;

namespace Microsoft.Office.OneNote
{
	/// <summary>
	/// Encapsulates an Ink object contained within an Outline.
	/// 
	/// Known limitation: it would be nice if all OutlineInk were treated as text, but for
	/// now, both Outline and Page Ink objects are inserted as drawing ink. 
	/// </summary>
	[Serializable]
	public class InkContent : OutlineContent
	{
		/// <summary>
		/// Constructs a new <see cref="InkContent"/> object containing the ISF data
		/// in the specified file.
		/// </summary>
		/// <param name="file">The <see cref="FileInfo"/> containing our ISF data.</param>
		public InkContent(FileInfo file)
		{
			InkData = new FileData(file);
		}

		/// <summary>
		/// Constructs a new <see cref="InkContent"/> object containing the ISF data
		/// in the given byte array.
		/// </summary>
		/// <param name="isf">The byte array containing our ink data.</param>
		public InkContent(byte[] isf)
		{
			InkData = new BinaryData(isf);
		}

		/// <summary>
		/// Constructs a new <see cref="InkContent"/> object containing the ISF data copied 
		/// from the specified InkContent.  
		/// </summary>
		/// <param name="clone">The <see cref="InkContent"/> whose data is to be copied to 
		/// the new InkContent.</param>
		public InkContent(InkContent clone)
		{
			InkData = clone.InkData;
		}

		/// <summary>
		/// Clones the InkContent, returning a new deep copy.
		/// </summary>
		/// <returns>A new InkContent with the same value as the original.</returns>
		public override object Clone()
		{
			return new InkContent(this);
		}

		/// <summary>
		/// Serializes the <see cref="InkContent"/> object to Xml in a format
		/// suitable for import into OneNote.
		/// </summary>
		/// <param name="parentNode">
		/// A reference to the parent XML node that we should serialize ourselves
		/// as a child of.
		/// </param>
		protected internal override void SerializeToXml(XmlNode parentNode)
		{
			XmlDocument xmlDocument = parentNode.OwnerDocument;

			XmlElement ink = xmlDocument.CreateElement("Ink");
			parentNode.AppendChild(ink);

			InkData.SerializeToXml(ink);
		}

		/// <summary>
		/// Gets or sets the Ink Data.
		/// </summary>
		public Data InkData
		{
			get
			{
				return (Data) GetChild("InkData");
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("InkData");

				if (!(value is FileData || value is BinaryData))
					throw new ArgumentException("Incorrect data type.");

				Data inkData = InkData;
				if (inkData != null)
					RemoveChild(inkData);

				AddChild(value, "InkData");
			}
		}
	}

	/* ---------------------------------------------------------------------- */

	/// <summary>
	/// Page level ink object. A concrete implementation of the abstract PageObject class.
	/// Known limitation: ink is imported as drawing ink. It can be handwriting, but functions such as
	/// search and text convert will not work because the ink object is treated as a drawing.
	/// </summary>
	[Serializable]
	public class InkObject : PageObject
	{
		/// <summary>
		/// Constructs a new <see cref="InkObject"/> contaning the ink data
		/// in the specified file.
		/// </summary>
		/// <param name="file">
		/// The <see cref="FileInfo"/> containing our data in ISF format.
		/// </param>
		public InkObject(FileInfo file)
		{
			Ink = new InkContent(file);
		}

		/// <summary>
		/// Constructs a new <see cref="InkObject"/> object containing the ISF data
		/// in the given byte array.
		/// </summary>
		/// <param name="isf">The byte array containing our ink data.</param>
		public InkObject(byte[] isf)
		{
			Ink = new InkContent(isf);
		}

		/// <summary>
		/// Constructs a new <see cref="InkObject"/> object containing the ISF data copied 
		/// from the specified InkObject.  
		/// </summary>
		/// <param name="clone">The <see cref="InkObject"/> whose data is to be copied to 
		/// the new InkObject.</param>
		public InkObject(InkObject clone)
		{
			Width = clone.Width;
			Height = clone.Height;
			Position = clone.Position;

			Ink = clone.Ink;
		}

		/// <summary>
		/// Serializes the <see cref="InkObject"/> object to Xml in a format
		/// suitable for import into OneNote.
		/// </summary>
		/// <param name="parentNode">
		/// A reference to the parent XML node that we should serialize ourselves
		/// as a child of.
		/// </param>
		protected internal override void SerializeObjectToXml(XmlNode parentNode)
		{
			Ink.SerializeToXml(parentNode);

			if (Width != null || Height != null)
			{
				XmlElement ink = (XmlElement) parentNode.LastChild;

				if (Width != null)
					Width.SerializeToXml(ink);
				if (Height != null)
					Height.SerializeToXml(ink);
			}
		}

		/// <summary>
		/// Clones the InkObject, returning a new deep copy.
		/// </summary>
		/// <returns>A new InkObject with the same value as the original.</returns>
		public override object Clone()
		{
			return new InkObject(this);
		}

		/// <summary>
		/// Gets or sets the internal Ink object that we use to delegate all 
		/// functionality to.
		/// </summary>
		private InkContent Ink
		{
			get
			{
				return (InkContent) GetChild("Ink");
			}
			set
			{
				InkContent ink = Ink;
				if (value == ink)
					return;

				if (ink != null)
					RemoveChild(Ink);

				AddChild(value, "Ink");
			}
		}

		/// <summary>
		/// Gets or sets the Ink Data.
		/// </summary>
		public Data InkData
		{
			get
			{
				return Ink.InkData;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("InkData");

				InkContent ink = Ink;
				ink.InkData = value;
			}
		}
	}
}