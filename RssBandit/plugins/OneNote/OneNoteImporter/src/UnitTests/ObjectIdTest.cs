using System;
using NUnit.Framework;

namespace Microsoft.Office.OneNote.UnitTests
{
	/// <summary>
	/// Tests the ObjectId class.
	/// </summary>
	[TestFixture]
	public class ObjectIdTest
	{
		[Test]
		public void Construction()
		{
			new ObjectId();
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void EmptyGuid()
		{
			new ObjectId(new Guid());
		}

		[Test]
		[ExpectedException(typeof(NullReferenceException))]
		public void Null()
		{
			new ObjectId(null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void EmptyString()
		{
			new ObjectId("");
		}

		[Test]
		[ExpectedException(typeof(FormatException))]
		public void ShortString()
		{
			new ObjectId("{}");
		}

		[Test]
		[ExpectedException(typeof(FormatException))]
		public void InvalidString()
		{
			new ObjectId("{foo}");
		}

		[Test]
		public void GuidInteroperability()
		{
			Guid guid = Guid.NewGuid();
			ObjectId id1 = new ObjectId(guid);

			ObjectId id2 = new ObjectId(guid);

			Assert.AreEqual(id1, id2);
		}

		[Test]
		public void Equality()
		{
			ObjectId id = new ObjectId();

			// Negative cases:
			ObjectId id2 = new ObjectId();
			Assert.IsFalse(id == id2);
			Assert.IsTrue(id != id2);
			Assert.IsFalse(id.Equals(id2));

			// Positive cases:
			ObjectId idDeserialized = new ObjectId(id.ToString());
			Assert.IsTrue(id.Equals(idDeserialized));
			Assert.IsFalse(id == idDeserialized);
			Assert.IsTrue(id != idDeserialized);
		}
	}

}