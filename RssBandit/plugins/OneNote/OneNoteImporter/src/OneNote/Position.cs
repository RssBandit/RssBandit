using System;
using System.Xml;

namespace Microsoft.Office.OneNote
{
	/// <summary>
	/// Represents a position on a page as a tuple of (x, y) in Points.
	/// 72 points comprise a single inch. PageObjects have Positions to specify their location on the page.
	/// </summary>
	[Serializable]
	public class Position : ImportNode
	{
		/// <summary>
		/// Creates a new Position corresponding to the upper left hand
		/// corner of the page.
		/// </summary>
		public Position() : this(LEFT_MARGIN, TOP_MARGIN)
		{
		}

		/// <summary>
		/// Creates a new Position corresponding to the specified x and y
		/// coordinates in Points.
		/// </summary>
		/// <param name="xInPoints">X coordinate</param>
		/// <param name="yInPoints">Y coordinate</param>
		public Position(double xInPoints, double yInPoints)
		{
			this.x = xInPoints;
			this.y = yInPoints;
		}

		/// <summary>
		/// Clones this Position.
		/// </summary>
		/// <returns>A new Position with the same value as the original.</returns>
		public override object Clone()
		{
			return MemberwiseClone();
		}

		/// <summary>
		/// Creates a new Position corresponding to the specified x and y 
		/// coordinates in Inches.
		/// </summary>
		/// <param name="xInInches">X coordinate in Inches</param>
		/// <param name="yInInches">Y coordinate in Inches</param>
		public void FromInches(double xInInches, double yInInches)
		{
			this.x = UnitConversions.InchesToPoints(xInInches);
			this.y = UnitConversions.InchesToPoints(yInInches);
		}

		/// <summary>
		/// Serializes the <see cref="Position"/> object to Xml in a format
		/// suitable for import into OneNote.
		/// </summary>
		/// <param name="parentNode">
		/// A reference to the parent XML node that we should serialize ourselves
		/// as a child of.
		/// </param>
		protected internal override void SerializeToXml(XmlNode parentNode)
		{
			XmlDocument xmlDocument = parentNode.OwnerDocument;

			XmlElement positionElement = xmlDocument.CreateElement("Position");
			positionElement.SetAttribute("x", X.ToString());
			positionElement.SetAttribute("y", Y.ToString());
			parentNode.AppendChild(positionElement);
		}

		/// <summary>
		/// Determines whether or not the specified object is equal to the 
		/// current position.  
		/// </summary>
		/// <param name="obj">
		/// The object to compare with the current position.
		/// </param>
		/// <returns>
		/// true if the specified object represents the same position; false otherwise
		/// </returns>
		public override bool Equals(object obj)
		{
			Position other = obj as Position;
			if (other == null)
				return false;

			return other.X.Equals(X) && other.Y.Equals(Y);
		}

		/// <summary>
		/// Serves as a hash function for the <see cref="Position"/>, suitable for use 
		/// in hashing algorithms and data structures like a hash table.  
		/// </summary>
		/// <returns>A hash code for the current <see cref="Position"/>.</returns>
		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}

		/// <summary>
		/// X coordinate
		/// </summary>
		public double X
		{
			get
			{
				return x;
			}
			set
			{
				x = value;
			}
		}

		/// <summary>
		/// Y coordinate
		/// </summary>
		public double Y
		{
			get
			{
				return y;
			}
			set
			{
				y = value;
			}
		}

		private double x;
		private double y;

		// WORKAROUND: By default, (0x0) is actually outside of the page 
		// margins.  Eventually, the DataImporter should align (0x0) with the
		// current page margins for the user, but this will work in most cases.

		// Note that users who use a non-standard page margin may end up with 
		// unintended scroll bars.
		private const int LEFT_MARGIN = 36;
		private const int TOP_MARGIN = 36;
	}
}