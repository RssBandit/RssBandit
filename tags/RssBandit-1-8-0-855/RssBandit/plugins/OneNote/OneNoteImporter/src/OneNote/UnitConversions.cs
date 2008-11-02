using System;

namespace Microsoft.Office.OneNote
{
	/// <summary>
	/// Utility class to convert between various units of measurement (specifically points and inches).
	/// </summary>
	public sealed class UnitConversions
	{
		/// Keep people from creating instances of this static class.
		private UnitConversions()
		{
		}

		/// <summary>
		/// Converts the specified number of inches into points.
		/// </summary>
		/// <param name="inches">
		/// The value (in inches) to be converted (to points).
		/// </param>
		/// <returns>The specified value in points.</returns>
		public static double InchesToPoints(double inches)
		{
			if (inches > (1000000 / POINTS_PER_INCH))
				throw new ArgumentOutOfRangeException("inches", "Points cannot exceed 1000000");

			return inches*POINTS_PER_INCH;
		}

		/// <summary>
		/// Converts the specified number of points into inches.
		/// </summary>
		/// <param name="points">
		/// The value (in points) to be converted (to inches).
		/// </param>
		/// <returns>The specified value in inches.</returns>
		public static double PointsToInches(double points)
		{
			if (points > 1000000)
				throw new ArgumentOutOfRangeException("points", "Points cannot exceed 1000000");

			return points*((double) 1/POINTS_PER_INCH);
		}

		/// <summary>
		/// The number of points per inch.
		/// </summary>
		public const int POINTS_PER_INCH = 72;
	}
}