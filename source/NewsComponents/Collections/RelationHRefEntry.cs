using System;
using System.Collections.Generic;
using System.Text;


namespace NewsComponents.Collections
{

    [Serializable]
    public class RelationHRefEntry : IEquatable<RelationHRefEntry>
    {
        public RelationHRefEntry(string link, string text, float score)
        {
            HRef = (link == null ? String.Empty : link);
            Text = (text == null ? String.Empty : text);
            Score = score;
            References = new List<NewsItem>();
        }
        public float Score;
        public string Text;
        public string HRef;
        public IList<NewsItem> References;


        /// <summary>
        /// Determine if this object is equal to another
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {

            return Equals(obj as RelationHRefEntry);
        }

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
