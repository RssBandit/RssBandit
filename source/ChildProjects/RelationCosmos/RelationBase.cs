using System;
using System.Collections;
using RelationCosmos.Collections;

namespace RelationCosmos
{
	/// <summary>
	/// Abstract base class used by RelationCosmos to work with relational items. 
	/// </summary>
	public abstract class RelationBase: IComparable {
		
		protected RelationBase() {
			href = String.Empty;
			// for mem perf. we can play with that capacity. In RelationCosmos it is later reducted to
			// the used size by calling TrimToSize().
			outgoingRelations = new RelationHRefList(5);	
			pointInTime = RelationCosmos.UnknownPointInTime;
			pointInTimeIsAdjustable = true;
			externalRelations = null;
		}

		/// <summary>
		/// Return a web reference, a resource ID, mail/message ID, NNTP post ID.
		/// </summary>
		public virtual string HRef { get { return href; } }
		protected string href;

		/// <summary>
		/// Return a list of outgoing Relation objects, e.g. 
		/// links the current relation resource points to.
		/// </summary>
		public virtual RelationHRefList OutgoingRelations { get { return outgoingRelations;} }
		protected RelationHRefList outgoingRelations;

		/// <summary>
		/// The DateTime the item was published/updated. It should be specified as UTC.
		/// </summary>
		public virtual DateTime PointInTime {
			get {	return this.pointInTime; }
			set { 
				this.pointInTime = value; 
				pointInTimeIsAdjustable = (this.pointInTime == RelationCosmos.UnknownPointInTime);
			}
		}
		// Used by RelationCosmos to adjust PointInTime but keep pointInTimeIsAdjustable setting
		internal virtual void SetInternalPointInTime (DateTime newPointInTime){ 
			this.pointInTime = newPointInTime; 
		}
		protected DateTime pointInTime;
		
		// Used by RelationCosmos to adjust PointInTime
		internal virtual bool PointInTimeIsAdjustable {
			get {return pointInTimeIsAdjustable; }
		}
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
				return RelationCosmos.EmptyRelationList; 
			return externalRelations;
		}
		public virtual void SetExternalRelations(RelationList relations) { 
			externalRelations = relations; 
		}
		protected RelationList externalRelations = null;

		#region IComparable Members

		public int CompareTo(object obj) {
			if (Object.ReferenceEquals(this, obj))
				return 0;
			RelationBase r = obj as RelationBase;
			if (r == null)
				return 1;
			
			return this.pointInTime.CompareTo(r.pointInTime);
		}

		#endregion
	}

	public class RelationProxy: RelationBase {
		
		#region ctor(s)
		protected RelationProxy():base() {
			realObject = null;
		}
		public RelationProxy(string href, DateTime pointInTime):
			this(href, null, pointInTime) {}

		public RelationProxy(string href, object realObject, DateTime pointInTime):this() {
			this.realObject = realObject;
			base.href = href;
			base.PointInTime = pointInTime;
		}
		
		public RelationProxy(string href, DateTime pointInTime, bool adjustablePointInTime):
			this(href, null, pointInTime, adjustablePointInTime) {}

		public RelationProxy(string href, object realObject, DateTime pointInTime, bool adjustablePointInTime):this() {
			this.realObject = realObject;
			base.href = href;
			base.SetInternalPointInTime(pointInTime);
			base.pointInTimeIsAdjustable = adjustablePointInTime;
		}
		#endregion

		#region public methods/properties
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
