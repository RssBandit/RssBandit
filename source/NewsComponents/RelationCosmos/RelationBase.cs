#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion


using System;
using System.Collections.Generic;

namespace NewsComponents.RelationCosmos
{
    /// <summary>
    /// Abstract base class used by RelationCosmos to work with relational items. 
    /// </summary>
    public abstract class RelationBase<T> : IRelation, IComparable
        where T : IRelation
    {
		/// <summary>
		/// Gets an empty list
		/// </summary>
		protected static List<TitledLink> EmptyList = new List<TitledLink>(0);

        /// <summary>
        /// Internal initializer.
        /// </summary>
        protected RelationBase()
        {
            HRef = String.Empty;
            outgoingRelationships = EmptyList;
            aPointInTime = RelationCosmos.UnknownPointInTime;
            pointInTimeIsAdjustable = true;
            externalRelations = null;
        }

        /// <summary>
        /// Return a web reference, a resource ID, mail/message ID, NNTP post ID.
        /// </summary>
        public string HRef { get; protected set; }

        /// <summary>
        /// The unique identifier.
        /// </summary>
        public virtual string Id
        {
            get { return p_id; }
            set { p_id = value; }
        }

        /// <summary>
        /// A resource ID, mail/message ID, NNTP post ID.
        /// </summary>
        protected string p_id;

        /// <summary>
        /// Return a list of outgoing Relation objects, e.g. 
        /// links the current relation resource points to.
        /// </summary>
        IList<string> IRelation.OutgoingRelations
        {
	        get
	        {
				return outgoingRelationships.ConvertAll(link => link.Url);
	        }
        }

        /// <summary>
        /// Stores the outgoing relation(s).
        /// </summary>
		protected internal List<TitledLink> outgoingRelationships;

        /// <summary>
        /// The DateTime the item was published/updated. It should be specified as UTC.
        /// </summary>
        public virtual DateTime PointInTime
        {
            get { return aPointInTime; }
            set
            {
                aPointInTime = value;
                pointInTimeIsAdjustable = (aPointInTime == RelationCosmos.UnknownPointInTime);
            }
        }

        // Used by RelationCosmos to adjust PointInTime but keep pointInTimeIsAdjustable setting
        internal void SetInternalPointInTime(DateTime newPointInTime)
        {
            aPointInTime = newPointInTime;
        }

        /// <summary>
        /// Internal accessor.
        /// </summary>
        protected DateTime aPointInTime;

        // Used by RelationCosmos to adjust PointInTime
        internal virtual bool PointInTimeIsAdjustable
        {
            get { return pointInTimeIsAdjustable; }
        }

        /// <summary>
        /// Internal accessor.
        /// </summary>
        protected bool pointInTimeIsAdjustable;

        /// <summary>
        /// Return true, if the Relation has some external relations (that are not part
        /// of the RelationCosmos). Default is false;
        /// </summary>
        public virtual bool HasExternalRelations
        {
            get { return false; }
        }

        /// <summary>
        /// Gets called if <see cref="HasExternalRelations">HasExternalRelations</see>
        /// returns true to retrieve the external Relation resource(s).
        /// Default return is the RelationCosmos.EmptyRelationList.
        /// </summary>
        public virtual IList<IRelation> GetExternalRelations()
        {
            if (externalRelations == null)
                return new List<IRelation>();

            //TODO: Implement this in a more elegant manner
            var list = new List<IRelation>();
            foreach (var relation in externalRelations)
            {
                list.Add(relation);
            }
            return list;
        }

        /// <summary>
        /// Should be overridden. Stores a collection of external Relations related
        /// to this RelationBase.
        /// </summary>
        public virtual void SetExternalRelations<X>(IList<X> relations) where X : IRelation
        {
            externalRelations = (IList<T>) relations;
        }

        /// <summary>
        /// Internal accessor.
        /// </summary>
        protected IList<T> externalRelations;

        #region IComparable Members

        /// <summary>
        /// Impl. IComparable.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            return CompareTo(obj as RelationBase<T>);
        }

		/// <summary>
		/// Compares to another instance.
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
        public int CompareTo(RelationBase<T> other)
        {
            if (ReferenceEquals(this, other))
                return 0;

            if (ReferenceEquals(other, null))
                return 1;

            return aPointInTime.CompareTo(other.aPointInTime);
        }

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>.
		/// </returns>
        public int CompareTo(IRelation other)
        {
            return CompareTo(other as RelationBase<T>);
        }

        #endregion
    }

}
