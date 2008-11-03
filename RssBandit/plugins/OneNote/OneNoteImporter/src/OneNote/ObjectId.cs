using System;

namespace Microsoft.Office.OneNote
{
	/// <exclude/>
	/// <summary>
	/// Each Page and PageObject has it's own ObjectId, which can be used to 
	/// modify and access the Page or Object across different sessions.
	/// </summary>
	[Serializable]
	public class ObjectId
	{
		/// <summary>
		/// Creates a new random <see cref="ObjectId"/>.
		/// </summary>
		public ObjectId()
		{
			guid = Guid.NewGuid();
		}

		/// <summary>
		/// Instantiates an <see cref="ObjectId"/> with the specified Global
		/// Unique Identifier.
		/// </summary>
		/// <param name="guid">The Global Unique Identifier of this ObjectId.</param>
		public ObjectId(Guid guid)
		{
			if (guid.Equals(Guid.Empty))
			{
				throw (new ArgumentException("Invalid GUID argument."));
			}

			this.guid = guid;
		}

		/// <summary>
		/// Instantiates an <see cref="ObjectId"/> from a previously serialized
		/// instance.
		/// </summary>
		/// <param name="serialized">
		/// The serialized ObjectId, in string format.
		/// </param>
		public ObjectId(string serialized)
		{
			if (serialized.Length == 0)
			{
				throw (new ArgumentException("Unable to parse emptry string."));
			}

			char[] delimiters = {'{', '}'};
			serialized.TrimStart(delimiters);
			serialized.TrimEnd(delimiters);

			Guid deserialized = new Guid(serialized);
			if (deserialized.Equals(Guid.Empty))
			{
				throw (new ArgumentException("Invalid GUID argument."));
			}

			this.guid = deserialized;
		}

		/// <summary>
		/// Returns the ObjectId in a string format.
		/// </summary>
		/// <returns>The ObjectId in a string format.</returns>
		public override string ToString()
		{
			return '{' + guid.ToString() + '}';
		}

		/// <summary>
		/// Returns true if the specified object is an ObjectId and represents
		/// the same unique identifier.
		/// </summary>
		/// <param name="obj">The object to be compared.</param>
		/// <returns>
		/// true if the object is an ObjectId representing the same unique 
		/// identifier.
		/// </returns>
		public override bool Equals(object obj)
		{
			ObjectId objId = obj as ObjectId;
			if (objId == null)
				return false;

			return guid.Equals(objId.guid);
		}

		/// <summary>
		/// Serves as a hash function for the <see cref="ObjectId"/>, suitable for use in hashing algorithms and data structures like a hash table.  
		/// </summary>
		/// <returns>A hash code for the current <see cref="ObjectId"/>.</returns>
		public override int GetHashCode()
		{
			return guid.GetHashCode();
		}

		private Guid guid;
	}
}