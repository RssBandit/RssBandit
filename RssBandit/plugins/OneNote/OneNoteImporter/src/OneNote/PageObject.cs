using System;
using System.Xml;

namespace Microsoft.Office.OneNote
{
	/// <summary>
	/// Abstract class. PageObjects can be added to a page using <see cref="Page.AddObject"/>.  Currently, there are three concrete types 
	/// of PageObjects: <see cref="OutlineObject"/>, <see cref="InkObject"/>, and <see cref="ImageObject"/>.
	/// </summary>
	[Serializable]
	public abstract class PageObject : ImportNode
	{
		/// <summary>
		/// Performs the common functionality in constructing a PageObject, 
		/// in particular: creating the ObjectId and setting the default 
		/// Position.
		/// </summary>
		protected PageObject()
		{
			Id = new ObjectId();
			Position = new Position();
		}

		/// <summary>
		/// Serializes the <see cref="PageObject"/> object to Xml in a format
		/// suitable for import into OneNote.  Handles the commonalities of all
		/// PageObjects, and then calls into the abstract method SerializeObjectToXml 
		/// to serialize the specific object data.
		/// </summary>
		/// <param name="parentNode">
		/// A reference to the parent XML node that we should serialize ourselves
		/// as a child of.
		/// </param>
		protected internal override void SerializeToXml(XmlNode parentNode)
		{
			XmlDocument xmlDocument = parentNode.OwnerDocument;

			XmlElement objectElement = xmlDocument.CreateElement("Object");
			objectElement.SetAttribute("guid", Id.ToString());
			parentNode.AppendChild(objectElement);

			if (DeletePending)
			{
				XmlElement deleteElement = xmlDocument.CreateElement("Delete");
				objectElement.AppendChild(deleteElement);
			}
			else
			{
				Position.SerializeToXml(objectElement);
				SerializeObjectToXml(objectElement);
			}
		}

		/// <summary>
		/// Each PageObject provides a mechanism to serialize the object data
		/// to XML in a manner suitable for import into OneNote.
		/// </summary>
		/// <param name="parentNode">
		/// A reference to the parent XML node that we should serialize ourselves
		/// as a child of.
		/// </param>
		protected internal abstract void SerializeObjectToXml(XmlNode parentNode);

		/// <summary>
		/// Determines whether or not the specified object is equal to the 
		/// current PageObject.  Equality is determined by the 
		/// <see cref="ObjectId"/>.
		/// </summary>
		/// <param name="obj">
		/// The object to compare with the current PageObject
		/// </param>
		/// <returns>
		/// true if the specified object is equal to the PageObject; false otherwise
		/// </returns>
		public override bool Equals(object obj)
		{
			PageObject pageObject = obj as PageObject;
			if (pageObject == null)
				return false;

			return pageObject.Id.Equals(id);
		}

		/// <summary>
		/// Serves as a hash function for the <see cref="PageObject"/>, suitable for use in hashing algorithms and data structures like a hash table.  
		/// </summary>
		/// <returns>A hash code for the current <see cref="PageObject"/>.</returns>
		public override int GetHashCode()
		{
			return id.GetHashCode();
		}

		/// <summary>
		/// Gets or sets the ObjectId.
		/// </summary>
		protected internal ObjectId Id
		{
			get
			{
				return id;
			}
			set
			{
				id = value;
			}
		}

		/// <summary>
		/// Gets or sets the Position of the object on the Page.
		/// 
		/// Note: If the page is in RTL mode, position (0,0) will be at the 
		/// right hand upper corner of the page -- which is both outside of the 
		/// default margin (36pts for the common case) and will still
		/// be left aligned, so you'll need to subtract out the object's 
		/// width to have it right aligned with the right hand page margin.
		/// </summary>
		public Position Position
		{
			get
			{
				return (Position) GetChild("Position");
			}
			set
			{
				Position position = Position;
				if (position == value)
					return;

				if (value == null)
					throw new ArgumentNullException("Position");

				if (position != null)
					RemoveChild(position);

				AddChild(value, "Position");
			}
		}

		/// <summary>
		/// Gets or sets the object's width.
		/// </summary>
		public virtual Size Width
		{
			get
			{
				return (Size) GetChild("width");
			}
			set
			{
				Size width = Width;
				if (value == width)
					return;

				if (width != null)
					RemoveChild(width);

				if (value != null)
					AddChild(value, "width");
			}
		}

		/// <summary>
		/// Gets or sets the object's height.
		/// </summary>
		public virtual Size Height
		{
			get
			{
				return (Size) GetChild("height");
			}
			set
			{
				Size height = Height;
				if (value == height)
					return;

				if (height != null)
					RemoveChild(height);

				if (value != null)
					AddChild(value, "height");
			}
		}

		/// <summary>
		/// Gets or sets whether or not this object is pending for deletion.
		/// </summary>
		protected internal bool DeletePending
		{
			get
			{
				return deletePending;
			}
			set
			{
				if (value == deletePending)
					return;

				deletePending = true;
				CommitPending = true;
			}
		}

		private ObjectId id;
		private bool deletePending;
	}
}