using System;
using System.Collections;
using System.Xml;

namespace Microsoft.Office.OneNote
{
	/// <summary>
	/// An OutlineObject represents an Outline in OneNote. It is a concrete implementation of PageObject and can
	/// be added to a page. It contains any number of <see cref="HtmlContent"/>, <see cref="ImageContent"/> or <see cref="InkContent"/> nodes.
	/// These will be displayed in the order they are added.
	/// </summary>
	[Serializable]
	public class OutlineObject : PageObject, IEnumerable
	{
		/// <summary>
		/// Instantiates a new empty Outline that can be added to a <see cref="Page"/>.
		/// </summary>
		public OutlineObject()
		{
		}

		/// <summary>
		/// Clones the Outline, returning a new deep copy.
		/// </summary>
		/// <returns>A new Outline with the same value as the original.</returns>
		public override object Clone()
		{
			OutlineObject clone = new OutlineObject();
			clone.Width = this.Width;
			clone.Position = this.Position;

			foreach (OutlineContent outlineContent in this)
			{
				clone.AddContent(outlineContent);
			}

			return clone;
		}

		/// <summary>
		/// Adds the specified content to the outline.
		/// </summary>
		/// <param name="outlineContent">The content to be added.</param>
		public void AddContent(OutlineContent outlineContent)
		{
			AddChild(outlineContent);
		}

		/// <summary>
		/// Removes the specified content from the outline.
		/// </summary>
		/// <param name="outlineContent">The content to be removed.</param>
		public void RemoveContent(OutlineContent outlineContent)
		{
			RemoveChild(outlineContent);
		}

		/// <summary>
		/// Returns an <see cref="IEnumerator"/> for the Outline that can
		/// be used to enumerate over all of the content in the Outline.
		/// </summary>
		/// <returns>An <see cref="IEnumerator"/> for the Outline.</returns>
		public IEnumerator GetEnumerator()
		{
			return new OutlineEnumerator(this);
		}

		/// <summary>
		/// Serializes the <see cref="OutlineObject"/> object to Xml in a format
		/// suitable for import into OneNote.
		/// </summary>
		/// <param name="parentNode">
		/// A reference to the parent XML node that we should serialize ourselves
		/// as a child of.
		/// </param>
		protected internal override void SerializeObjectToXml(XmlNode parentNode)
		{
			XmlDocument xmlDocument = parentNode.OwnerDocument;

			XmlElement outlineElement = xmlDocument.CreateElement("Outline");
			if (Width != null)
				Width.SerializeToXml(outlineElement);
			parentNode.AppendChild(outlineElement);

			foreach (OutlineContent content in this)
			{
				content.SerializeToXml(outlineElement);
			}
		}

		/// <summary>
		/// Outlines cannot have an explicit height set on them.  Calling this
		/// will result in an exception!
		/// </summary>
		public override Size Height
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotSupportedException("Outlines do not have explicit heights.");
			}
		}

		/* ------------------------------------------------------------------ */

		class OutlineEnumerator : IEnumerator
		{
			protected internal OutlineEnumerator(OutlineObject outline)
			{
				this.outline = outline;
				Reset();
			}

			public void Reset()
			{
				index = -1;
			}

			public object Current
			{
				get
				{
					if (index < outline.GetChildCount())
					{
						return outline.GetChild(index);
					}

					return null;
				}
			}

			public bool MoveNext()
			{
				while (++index < outline.GetChildCount() &&
					!(outline.GetChild(index) is OutlineContent))
				{
					continue;
				}

				return (index < outline.GetChildCount());
			}

			private OutlineObject outline;
			private int index;
		}
	}

	/* ---------------------------------------------------------------------- */

	/// <summary>
	/// Abstract class defining a node of content within an outline. OutlineObject (the outline) can contain
	/// any number of OutlineContent objects. To create content to add to an outline you must use one of the
	/// three concrete implementations of OutlineContent. These are: <see cref="HtmlContent"/>, <see cref="InkContent"/>, and 
	/// <see cref="ImageContent"/>.
	/// </summary>
	[Serializable]
	public abstract class OutlineContent : ImportNode
	{
		/// <summary>
		/// Serialize the <see cref="OutlineContent"/> object to Xml in a format
		/// suitable for import into OneNote.
		/// </summary>
		/// <param name="parentNode">
		/// A reference to the parent XML node that we should serialize ourselves
		/// as a child of.
		/// </param>
		protected internal override abstract void SerializeToXml(XmlNode parentNode);
	}

	/* ---------------------------------------------------------------------- */

	/// <summary>
	/// Enumerates the possible alignment of an content within an Outline.
	/// </summary>
	public enum OutlineAlignment
	{
		/// <summary>
		/// Default alignment given the directionality of the page.
		/// </summary>
		DEFAULT,

		/// <summary>
		/// Left aligned.
		/// </summary>
		LEFT,

		/// <summary>
		/// Center aligned.
		/// </summary>
		CENTER,

		/// <summary>
		/// Right aligned.
		/// </summary>
		RIGHT
	}
}