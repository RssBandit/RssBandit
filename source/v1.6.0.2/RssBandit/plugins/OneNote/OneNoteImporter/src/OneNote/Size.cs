using System;
using System.Xml;

namespace Microsoft.Office.OneNote
{
	/// <summary>
	/// Represents a size in Points. 72 points comprise a single inch.
	/// Sizes are used to specify the height and width of page objects (outline objects, image objects, or ink objects). 
	/// </summary>
	[Serializable]
	public class Size : ImportNode
	{
		/// <summary>
		/// Constructs a new size.
		/// </summary>
		/// <param name="sizeInPoints">The value of the size in points.</param>
		public Size(double sizeInPoints)
		{
			if (sizeInPoints < 0.0 || sizeInPoints > 1000000.00 || Double.IsNaN(sizeInPoints))
			{
				throw new ArgumentException("Invalid Size.");
			}

			this.sizeInPoints = sizeInPoints;
		}

		/// <summary>
		/// Clones the Size object, returning a new deep copy.
		/// </summary>
		/// <returns>A new Size with same value as the original.</returns>
		public override object Clone()
		{
			return MemberwiseClone();
		}

		/// <summary>
		/// Utility method to construct a size in inches.
		/// </summary>
		/// <param name="sizeInInches">The size in inches.</param>
		/// <returns>A new size object.</returns>
		public static Size FromInches(double sizeInInches)
		{
			return new Size(UnitConversions.InchesToPoints(sizeInInches));
		}

		/// <summary>
		/// Returns a <see cref="String"/> representation of the size in points.
		/// </summary>
		/// <returns>A <see cref="String"/> representation of the size in points.</returns>
		public override string ToString()
		{
			return sizeInPoints.ToString();
		}

		/// <summary>
		/// Performs a deep comparison with another object for equality.
		/// </summary>
		/// <param name="obj">The object to compare ourselves with.</param>
		/// <returns>
		/// true if the object is a <see cref="Size"/> object with the same value in points.
		/// </returns>
		public override bool Equals(object obj)
		{
			Size size = obj as Size;
			if (size != null)
			{
				return sizeInPoints.Equals(size.sizeInPoints);
			}

			return false;
		}

		/// <summary>
		/// Hash function for Size objects, suitable for use in hashing algorithms and data structures like a hash table.
		/// </summary>
		/// <returns>A hash code for the current <see cref="Size"/>.</returns>
		public override int GetHashCode()
		{
			return sizeInPoints.GetHashCode();
		}

		/// <summary>
		/// Returns the value of the size in points.
		/// </summary>
		/// <returns>The value of the size in points.</returns>
		public double InPoints()
		{
			return sizeInPoints;
		}

		/// <summary>
		/// Returns the value of the size in inches.
		/// </summary>
		/// <returns>The value of the size in inches.</returns>
		public double InInches()
		{
			return UnitConversions.PointsToInches(sizeInPoints);
		}

		/// <summary>
		/// Serializes the <see cref="Size"/> object to Xml in a format
		/// suitable for import into OneNote.
		/// </summary>
		/// <param name="parentNode">
		/// A reference to the parent XML node that we should serialize ourselves
		/// as a child of.
		/// </param>
		protected internal override void SerializeToXml(XmlNode parentNode)
		{
			XmlDocument xmlDocument = parentNode.OwnerDocument;

			XmlAttribute attribute = xmlDocument.CreateAttribute(Name);
			attribute.Value = ToString();

			parentNode.AppendChild(attribute);
		}

		private double sizeInPoints;
	}
}