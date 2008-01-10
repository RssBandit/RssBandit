using System;
using System.IO;
using System.Xml;

namespace Microsoft.Office.OneNote
{
	/// <summary>
	/// An Image that can be inserted into an Outline by adding it to an OutlineObject.
	/// </summary>
	[Serializable]
	public class ImageContent : OutlineContent
	{
		/// <summary>
		/// Constructs a new Image suitable for insertion into an Outline
		/// representing the data in the specified file.
		/// </summary>
		/// <param name="file">The <see cref="FileInfo"/> containing our image data.</param>
		public ImageContent(FileInfo file)
		{
			ImageData = new FileData(file);
		}

		/// <summary>
		/// Constructs a new Image that can be inserted into an Oultine 
		/// containing the data in the given byte array.
		/// </summary>
		/// <param name="image">The byte array containing our image data.</param>
		public ImageContent(byte[] image)
		{
			ImageData = new BinaryData(image);
		}

		/// <summary>
		/// Constructs a new Image that can be inserted into an Outline 
		/// containing the data copied from the specified Image. 
		/// </summary>
		/// <param name="clone">The <see cref="ImageContent"/> whose data is to be copied to 
		/// the new Image.</param>
		public ImageContent(ImageContent clone)
		{
			ImageData = clone.ImageData;
		}

		/// <summary>
		/// Clones the Image, returning a new deep copy.
		/// </summary>
		/// <returns>A new Image with the same value as the original.</returns>
		public override object Clone()
		{
			return new ImageContent(this);
		}

		/// <summary>
		/// Serializes the <see cref="ImageContent"/> object to Xml in a format
		/// suitable for import into OneNote.
		/// </summary>
		/// <param name="parentNode">
		/// A reference to the parent XML node that we should serialize ourselves
		/// as a child of.
		/// </param>
		protected internal override void SerializeToXml(XmlNode parentNode)
		{
			XmlDocument xmlDocument = parentNode.OwnerDocument;

			XmlElement image = xmlDocument.CreateElement("Image");
			parentNode.AppendChild(image);

			if (alignment != OutlineAlignment.DEFAULT)
			{
				image.SetAttribute("alignment", alignment.ToString());
			}

			ImageData.SerializeToXml(image);
		}

		/// <summary>
		/// Gets or sets the Image Data.
		/// </summary>
		public Data ImageData
		{
			get
			{
				return (Data) GetChild("ImageData");
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("ImageData");

				if (!(value is FileData || value is BinaryData))
					throw new ArgumentException("Incorrect data type.");

				Data imageData = ImageData;
				if (imageData != null)
					RemoveChild(imageData);

				AddChild(value, "ImageData");
			}
		}

		/// <summary>
		/// Gets or sets the alignment of the Image within the Outline.
		/// </summary>
		public OutlineAlignment Alignment
		{
			get
			{
				return alignment;
			}
			set
			{
				if (alignment == value)
					return;

				alignment = value;
				CommitPending = true;
			}
		}

		private OutlineAlignment alignment = OutlineAlignment.DEFAULT;
	}

	/* ---------------------------------------------------------------------- */

	/// <summary>
	/// A page-level image. This is a concrete implementation of the abstract PageObject class.
	/// </summary>
	[Serializable]
	public class ImageObject : PageObject
	{
		/// <summary>
		/// Constructs a new Image that can be added to a Page containing the 
		/// image data in the specified file.
		/// </summary>
		/// <param name="file">
		/// The <see cref="FileInfo"/> containing our image data.
		/// </param>
		public ImageObject(FileInfo file)
		{
			Image = new ImageContent(file);
		}

		/// <summary>
		/// Constructs a new Image that can be added to a Page containing the 
		/// image data in the given byte array.
		/// </summary>
		/// <param name="image">
		/// The byte array containing our image data.
		/// </param>
		public ImageObject(byte[] image)
		{
			Image = new ImageContent(image);
		}

		/// <summary>
		/// Constructs a new Image that can be added to a Page containing the
		/// image data copied from the specified Image.
		/// </summary>
		/// <param name="clone">The <see cref="HtmlContent"/> whose data is 
		/// to be copied to the new HtmlContent.</param>
		public ImageObject(ImageObject clone)
		{
			Width = clone.Width;
			Height = clone.Height;
			Position = clone.Position;

			Image = clone.Image;
		}

		/// <summary>
		/// Clones the Image, returning a new deep copy.
		/// </summary>
		/// <returns>A new ImageObject with the same value as the original.</returns>
		public override object Clone()
		{
			return new ImageObject(this);
		}

		/// <summary>
		/// Serializes the Image to Xml in a format suitable for import into
		/// OneNote.
		/// </summary>
		/// <param name="parentNode">
		/// A reference to the parent XML node that we should serialize ourselves
		/// as a child of.
		/// </param>
		protected internal override void SerializeObjectToXml(XmlNode parentNode)
		{
			Image.SerializeToXml(parentNode);

			XmlElement imageElement = (XmlElement) parentNode.LastChild;

			if (BackgroundImage)
				imageElement.SetAttribute("backgroundImage", "true");

			if (Width != null)
				Width.SerializeToXml(imageElement);
			if (Height != null)
				Height.SerializeToXml(imageElement);
		}

		/// <summary>
		/// Gets or sets the internal Image object that we use to delegate all 
		/// functionality to.
		/// </summary>
		private ImageContent Image
		{
			get
			{
				return (ImageContent) GetChild("Image");
			}
			set
			{
				ImageContent image = Image;
				if (value == image)
					return;

				if (image != null)
					RemoveChild(image);

				AddChild(value, "Image");
			}
		}

		/// <summary>
		/// Gets or sets the Image Data.
		/// </summary>
		public Data ImageData
		{
			get
			{
				return Image.ImageData;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("ImageData");

				ImageContent image = Image;
				image.ImageData = value;
			}
		}

		/// <summary>
		/// Gets or sets the BackgroundImage property.  When true, the
		/// image will be glued to the background of the OneNote page.
		/// </summary>
		public bool BackgroundImage
		{
			get
			{
				return backgroundImage;
			}
			set
			{
				if (backgroundImage == value)
					return;

				backgroundImage = value;
				CommitPending = true;
			}
		}

		private bool backgroundImage = false;
	}
}