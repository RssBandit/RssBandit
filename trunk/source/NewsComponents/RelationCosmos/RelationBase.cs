#region CVS Version Header

/*
 * $Id$
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
        protected static List<string> EmptyList = new List<string>(0);

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

//		/// <summary>
//		/// The unique identifier of the parent.
//		/// </summary>
//		public virtual string ParentId { get { return p_parentId; } }
//		/// <summary>
//		/// A parent resource ID, parent mail/message ID, NNTP parent post ID.
//		/// </summary>
//		protected string p_parentId; 

        /// <summary>
        /// Return a list of outgoing Relation objects, e.g. 
        /// links the current relation resource points to.
        /// </summary>
        public IList<string> OutgoingRelations
        {
            get { return outgoingRelationships; }
        }

        /// <summary>
        /// Stores the outgoing relation(s).
        /// </summary>
        protected internal List<string> outgoingRelationships;

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

        public int CompareTo(RelationBase<T> other)
        {
            if (ReferenceEquals(this, other))
                return 0;

            if (ReferenceEquals(other, null))
                return 1;

            return aPointInTime.CompareTo(other.aPointInTime);
        }

        public int CompareTo(IRelation other)
        {
            return CompareTo(other as RelationBase<T>);
        }

        #endregion
    }

    /// <summary>
    /// A base impl. of RelationBase.
    /// </summary>
    public class RelationProxy<T> : RelationBase<T>
        where T : RelationBase<T>
    {
        #region ctor(s)

        /// <summary>
        /// Internal initializer
        /// </summary>
        protected RelationProxy()
        {
            RealObject = null;
        }

        /// <summary>
        /// Public Initializer
        /// </summary>
        /// <param name="href"></param>
        /// <param name="pointInTime"></param>
        public RelationProxy(string href, DateTime pointInTime) :
            this(href, null, pointInTime)
        {
        }

        /// <summary>
        /// Public Initializer
        /// </summary>
        /// <param name="href"></param>
        /// <param name="realObject"></param>
        /// <param name="pointInTime"></param>
        public RelationProxy(string href, object realObject, DateTime pointInTime) : this()
        {
            RealObject = realObject;
            HRef = href;
            base.PointInTime = pointInTime;
        }

        /// <summary>
        /// Public Initializer
        /// </summary>
        /// <param name="href"></param>
        /// <param name="pointInTime"></param>
        /// <param name="adjustablePointInTime"></param>
        public RelationProxy(string href, DateTime pointInTime, bool adjustablePointInTime) :
            this(href, null, pointInTime, adjustablePointInTime)
        {
        }

        /// <summary>
        /// Public designated initializer
        /// </summary>
        /// <param name="href"></param>
        /// <param name="realObject"></param>
        /// <param name="pointInTime"></param>
        /// <param name="adjustablePointInTime"></param>
        public RelationProxy(string href, object realObject, DateTime pointInTime, bool adjustablePointInTime) : this()
        {
            RealObject = realObject;
            HRef = href;
            SetInternalPointInTime(pointInTime);
            pointInTimeIsAdjustable = adjustablePointInTime;
        }

        #endregion

        #region public methods/properties

        /// <summary>
        /// The accessor to the real object
        /// </summary>
        public object RealObject { get; set; }

        #endregion

        #region private vars

        #endregion
    }
}

#region CVS Version Log

/*
 * $Log: RelationBase.cs,v $
 * Revision 1.6  2006/10/17 15:33:01  t_rendelmann
 * made the Id public writable
 *
 */

#endregion