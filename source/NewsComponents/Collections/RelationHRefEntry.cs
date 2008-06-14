using System;
using System.Collections.Generic;


namespace NewsComponents.Collections
{

	/// <summary>
	/// Relation link (href) entry
	/// </summary>
    [Serializable]
    public class RelationHRefEntry : IEquatable<RelationHRefEntry>
    {
        public RelationHRefEntry(string link, string text, float score)
        {
            HRef = (link ?? String.Empty);
            Text = (text ?? String.Empty);
            Score = score;
            References = new List<INewsItem>();
        }
		/// <summary>
		/// Score of the entry
		/// </summary>
        public float Score;
		/// <summary>
		/// Display text
		/// </summary>
        public string Text;
		/// <summary>
		/// link
		/// </summary>
        public string HRef;
		/// <summary>
		/// List of references
		/// </summary>
        public IList<INewsItem> References;


        /// <summary>
        /// Determine if this object is equal to another
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {

            return Equals(obj as RelationHRefEntry);
        }

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
        public bool Equals(RelationHRefEntry other)
        {
            if (ReferenceEquals(this, other)) { return true; }

            if (ReferenceEquals(other, null))
                return false;
            


            if (this.HRef.Equals(other.HRef))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a hash code for this object
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.HRef.GetHashCode();
        }
    }
}
