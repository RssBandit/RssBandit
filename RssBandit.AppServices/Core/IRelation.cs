using System;
using System.Collections.Generic;

namespace NewsComponents.RelationCosmos
{
    public interface IRelation : IComparable<IRelation>
    {
        /// <summary>
        /// Return a web reference, a resource ID, mail/message ID, NNTP post ID.
        /// </summary>
        string HRef { get; }

        /// <summary>
        /// The unique identifier.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Return a list of outgoing Relation objects, e.g. 
        /// links the current relation resource points to.
        /// </summary>
        IList<string> OutgoingRelations { get; }

        /// <summary>
        /// The DateTime the item was published/updated. It should be specified as UTC.
        /// </summary>
        DateTime PointInTime { get; set; }

        /// <summary>
        /// Return true, if the Relation has some external relations (that are not part
        /// of the RelationCosmos). Default is false;
        /// </summary>
        bool HasExternalRelations { get; }

        /// <summary>
        /// Gets called if <see cref="HasExternalRelations">HasExternalRelations</see>
        /// returns true to retrieve the external Relation resource(s).
        /// Default return is the RelationCosmos.EmptyRelationList.
        /// </summary>
        IList<IRelation> GetExternalRelations();

        /// <summary>
        /// Should be overridden. Stores a collection of external Relations related
        /// to this RelationBase.
        /// </summary>
        void SetExternalRelations<T>(IList<T> relations) where T: IRelation;

    }
}