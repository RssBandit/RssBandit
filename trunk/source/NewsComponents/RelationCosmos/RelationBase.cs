#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;

using NewsComponents.Collections;

namespace NewsComponents.RelationCosmos
{
	/// <summary>
	/// Abstract base class used by RelationCosmos to work with relational items. 
	/// </summary>
	public abstract class RelationBase: IComparable {
		
		/// <summary>
		/// Internal initializer.
		/// </summary>
		protected RelationBase() {
			hReference = String.Empty;
			outgoingRelationships = RelationHRefDictionary.Empty;	
			aPointInTime = RelationCosmos.UnknownPointInTime;
			pointInTimeIsAdjustable = true;
			externalRelations = null;
		}

		/// <summary>
		/// Return a web reference, a resource ID, mail/message ID, NNTP post ID.
		/// </summary>
		public virtual string HRef { get { return hReference; } }
		/// <summary>
		/// Internal: return a web Uri reference 
		/// </summary>
		protected string hReference;

		/// <summary>
		/// The unique identifier.
		/// </summary>
		public virtual string Id {
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
		public RelationHRefDictionary OutgoingRelations { get { return outgoingRelationships;} }
		/// <summary>
		/// Stores the outgoing relation(s).
		/// </summary>
		protected internal RelationHRefDictionary outgoingRelationships;

		/// <summary>
		/// The DateTime the item was published/updated. It should be specified as UTC.
		/// </summary>
		public virtual DateTime PointInTime {
			get {	return this.aPointInTime; }
			set { 
				this.aPointInTime = value; 
				pointInTimeIsAdjustable = (this.aPointInTime == RelationCosmos.UnknownPointInTime);
			}
		}
		// Used by RelationCosmos to adjust PointInTime but keep pointInTimeIsAdjustable setting
		internal void SetInternalPointInTime (DateTime newPointInTime){ 
			this.aPointInTime = newPointInTime; 
		}
		/// <summary>
		/// Internal accessor.
		/// </summary>
		protected DateTime aPointInTime;
		
		// Used by RelationCosmos to adjust PointInTime
		internal virtual bool PointInTimeIsAdjustable {
			get {return pointInTimeIsAdjustable; }
		}
		/// <summary>
		/// Internal accessor.
		/// </summary>
		protected bool pointInTimeIsAdjustable;

		/// <summary>
		/// Return true, if the Relation has some external relations (that are not part
		/// of the RelationCosmos). Default is false;
		/// </summary>
		public virtual bool HasExternalRelations { get { return false; } }
		/// <summary>
		/// Gets called if <see cref="HasExternalRelations">HasExternalRelations</see>
		/// returns true to retrive the external Relation resource(s).
		/// Default return is the RelationCosmos.EmptyRelationList.
		/// </summary>
		public virtual RelationList GetExternalRelations() { 
			if (externalRelations == null)
				return RelationList.Empty; 
			return externalRelations;
		}
		/// <summary>
		/// Should be overridden. Stores a collection of external Relations related
		/// to this RelationBase.
		/// </summary>
		public virtual void SetExternalRelations(RelationList relations) { 
			externalRelations = relations; 
		}
		/// <summary>
		/// Internal accessor.
		/// </summary>
		protected RelationList externalRelations = null;

		#region IComparable Members

		/// <summary>
		/// Impl. IComparable.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int CompareTo(object obj) {
			if (Object.ReferenceEquals(this, obj))
				return 0;
			RelationBase r = obj as RelationBase;
			if (r == null)
				return 1;
			
			return this.aPointInTime.CompareTo(r.aPointInTime);
		}

		#endregion
	}

	/// <summary>
	/// A base impl. of RelationBase.
	/// </summary>
	public class RelationProxy: RelationBase {
		
		#region ctor(s)
		/// <summary>
		/// Internal initializer
		/// </summary>
		protected RelationProxy():base() {
			realObject = null;
		}
		/// <summary>
		/// Public Initializer
		/// </summary>
		/// <param name="href"></param>
		/// <param name="pointInTime"></param>
		public RelationProxy(string href, DateTime pointInTime):
			this(href, null, pointInTime) {}

		/// <summary>
		/// Public Initializer
		/// </summary>
		/// <param name="href"></param>
		/// <param name="realObject"></param>
		/// <param name="pointInTime"></param>
		public RelationProxy(string href, object realObject, DateTime pointInTime):this() {
			this.realObject = realObject;
			base.hReference = href;
			base.PointInTime = pointInTime;
		}
		
		/// <summary>
		/// Public Initializer
		/// </summary>
		/// <param name="href"></param>
		/// <param name="pointInTime"></param>
		/// <param name="adjustablePointInTime"></param>
		public RelationProxy(string href, DateTime pointInTime, bool adjustablePointInTime):
			this(href, null, pointInTime, adjustablePointInTime) {}

		/// <summary>
		/// Public designated initializer
		/// </summary>
		/// <param name="href"></param>
		/// <param name="realObject"></param>
		/// <param name="pointInTime"></param>
		/// <param name="adjustablePointInTime"></param>
		public RelationProxy(string href, object realObject, DateTime pointInTime, bool adjustablePointInTime):this() {
			this.realObject = realObject;
			base.hReference = href;
			base.SetInternalPointInTime(pointInTime);
			base.pointInTimeIsAdjustable = adjustablePointInTime;
		}
		#endregion

		#region public methods/properties
		/// <summary>
		/// The accessor to the real object
		/// </summary>
		public object RealObject { 
			get { return this.realObject; } 
			set { this.realObject = value;} 
		}
		#endregion

		#region private vars
		private object realObject;
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
