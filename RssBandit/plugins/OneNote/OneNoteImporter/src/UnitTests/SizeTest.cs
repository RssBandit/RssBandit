using System;
using NUnit.Framework;

namespace Microsoft.Office.OneNote.UnitTests
{
	/// <summary>
	/// Tests the Size class.
	/// </summary>
	[TestFixture]
	public class SizeTest
	{
		[Test]
		public void Construction()
		{
			Size s = new Size(10);
			Assert.AreEqual(10, s.InPoints());

			s = new Size(0);
			Assert.AreEqual(0, s.InPoints());

			s = new Size(1000000);
			Assert.AreEqual(1000000, s.InPoints());
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Negative()
		{
			new Size(-10);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void VeryLarge()
		{
			new Size(1000001);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Infinity()
		{
			new Size(Double.PositiveInfinity);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void NegativeInfinity()
		{
			new Size(Double.NegativeInfinity);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void Nan()
		{
			new Size(Double.NaN);
		}
		
		[Test]
		public void Inches()
		{
			Size s = Size.FromInches(1);
			Assert.AreEqual(72, s.InPoints());
			Assert.AreEqual(1, s.InInches());
		}
	}
}
