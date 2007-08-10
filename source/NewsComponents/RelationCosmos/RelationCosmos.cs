#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using NewsComponents.Collections;
using NewsComponents.Utils;
using NewsComponents.Threading;

namespace NewsComponents.RelationCosmos
{
	/// <summary>
	/// RelationCosmos1: the current impl.
	/// </summary>
	public class RelationCosmos1 : IRelationCosmos
	{
		#region ctor's
		/// <summary>
		/// Initializer
		/// </summary>
		public RelationCosmos1()	{
			worker = new PriorityThread();
			allRelations = new Relations();
			registeredRelations = new Relations();
			allIncomingRelations = new RelationLists();
			registeredIncomingRelations = new RelationLists();
		}
		#endregion

		#region public methods
		/// <summary>
		/// Add a new Relation to the RelationCosmos. 
		/// The relation(s) should be registered in time order oldest first to
		/// prevent lost relationships.
		/// </summary>
		/// <param name="relation">Relation to add</param>
		/// <exception cref="ArgumentNullException">If relation or relation.HRef is null</exception>
		public void Add(RelationBase relation)  {
			
			if (relation == null) 
				throw new ArgumentNullException("relation");
			
			if (relation.HRef == null)
				return;	// nothing we can refer back

			lock(syncRoot) {
				
				string href = relation.HRef;
				
				try {
					if (registeredRelations.Contains(href)) {
						Remove(relation);
					}

					if (deepCosmos) {
						if (!allRelations.ContainsKey(href)) 
							allRelations.Add(href, relation);
						if (!allRelations.ContainsKey(relation.Id)) 
							allRelations.Add(relation.Id, relation);
					}

					// add the real (known) outgoing relations
					if (relation.outgoingRelationships.Count > 0) {
						
						foreach (string hrefOut in relation.outgoingRelationships.Keys) {
							
							if (hrefOut != null && hrefOut.Length > 0) {
								
								if (deepCosmos && !allRelations.Contains(hrefOut) ) {
									allRelations.Add(hrefOut, new RelationProxy(hrefOut, relation.PointInTime, relation.PointInTimeIsAdjustable));
								} 
								
								if (deepCosmos) {
									if (href != hrefOut && allRelations.Contains(hrefOut)) {
										RelationBase known = allRelations[hrefOut];
										// A relation can only contain refs to OLDER relation entries.
										// So we correct the unknown PointInTime to be the same dateTime of 
										// the known entry subtract by the defaultRelationTimeCorrection.
										if (known.PointInTime > relation.PointInTime){
											AdjustRelationPointInTime(relation, known, adjustPointInTime);
										}
										AddToRelationList(relation.HRef, known, allIncomingRelations);
									}
								}
								
								if (href != hrefOut && registeredRelations.Contains(hrefOut)) {
									RelationBase known = registeredRelations[hrefOut];
									// A relation can only contain refs to OLDER relation entries.
									// So we correct the unknown PointInTime to be the same dateTime of 
									// the known entry subtract by the defaultRelationTimeCorrection.
									if (known.PointInTime > relation.PointInTime) {
										AdjustRelationPointInTime(relation, known, adjustPointInTime);
									}
									AddToRelationList(known.HRef, relation, registeredIncomingRelations);
								}
							}
						}//foreach

					}

					// ensure new relation have a valid point in time entry.
					if (relation.PointInTime == RelationCosmos.UnknownPointInTime)
						relation.SetInternalPointInTime(DateTime.UtcNow);	// keep the movable information

					// If we get relation(s) to add in unordered manner, that relation can be older then some known.
					// So we have to loop over yet known relations and test, if the known outgoing contains
					// reference(s) to the new one. If so, we add an entry to registeredIncomingRelations.
					foreach (RelationBase known in registeredRelations.Values) {
						
						if (known.outgoingRelationships.ContainsKey(href)) {
							if (known.CompareTo(relation) <= 0) {	// known is OLDER
								// A relation can only contain refs to OLDER relation entries.
								// so we correct the unknown PointInTime to be the same dateTime of 
								// the known entry subtract by the defaultRelationTimeCorrection.
								AdjustRelationPointInTime(relation, known, adjustPointInTime);
							} // else known is NEWER
							AddToRelationList(relation.HRef, known, registeredIncomingRelations);
						}//contains(relation)
					}//foreach

					//add to registered collection
					registeredRelations.Add(href, relation);
					if (href != relation.Id)
						registeredRelations.Add(relation.Id, relation);

				} catch (Exception ex) {
					Trace.Write("RelationCosmos.Add() exception:"+ ex.Message);
				}
			}			
		}
		
		/// <summary>
		/// Add a range of <c>RelationBase</c> objects
		/// </summary>
		/// <param name="relations">RelationBase[]</param>
		public void AddRange(RelationBase[] relations)  {
			this.AddRange((IList<RelationBase>)relations);
		}
		/// <summary>
		/// Add a range of <c>RelationBase</c> objects
		/// </summary>
		/// <param name="relations">IList</param>
		public void AddRange(IList<RelationBase> relations)  {
			RelationBase[] relationsToAdd = new RelationBase[relations.Count];
			relations.CopyTo(relationsToAdd, 0);
			worker.QueueUserWorkItem(new WaitCallback(this.ThreadRunAddRange), relationsToAdd, 0);
		}
		
		/// <summary>
		/// Add a range of <c>RelationBase</c> objects
		/// </summary>
		/// <param name="relations">IList</param>
		protected void InternalAddRange(IList<RelationBase> relations)  {
			if (relations == null) 
				throw new ArgumentNullException("relations");
			
			Hashtable selfIncomingLookup = new Hashtable(relations.Count);
			for (int i = 0, len = relations.Count; i < len; i++) 
			{
				RelationBase relation = (RelationBase)relations[i]; 
				if (null != relation.HRef)
					selfIncomingLookup.Add(relation.HRef, relation);	
				if (relation.Id != relation.HRef)
					selfIncomingLookup.Add(relation.Id, relation);	
			}			

			Thread.Sleep(150);

			for (int i = 0, len = relations.Count; i < len; i++) 
			{
				RelationBase relation = (RelationBase)relations[i]; 

				if (null == relation.HRef)
					continue;	// nothing we can refer back

				string href = relation.HRef;

				lock(syncRoot) {
				
					
					if (registeredRelations.Contains(href)) {
						Remove(relation);
					}

					if (deepCosmos) 
					{
						if (!allRelations.ContainsKey(href)) 
							allRelations.Add(href, relation);
						if (!allRelations.ContainsKey(relation.Id)) 
							allRelations.Add(relation.Id, relation);
					}

					// add the real (known) outgoing relations
					if (relation.outgoingRelationships.Count > 0) 
					{
							
						foreach (string hrefOut in relation.outgoingRelationships.Keys) {
								
							if (hrefOut != null && hrefOut.Length > 0) {
									
								if (deepCosmos && !allRelations.Contains(hrefOut) ) {
									allRelations.Add(hrefOut, new RelationProxy(hrefOut, relation.PointInTime, relation.PointInTimeIsAdjustable));
								} 
									
								if (deepCosmos) {
									if (href != hrefOut && allRelations.Contains(hrefOut)) {
										RelationBase known = allRelations[hrefOut];
										// A relation can only contain refs to OLDER relation entries.
										// So we correct the unknown PointInTime to be the same dateTime of 
										// the known entry subtract by the defaultRelationTimeCorrection.
										if (known.PointInTime > relation.PointInTime){
											AdjustRelationPointInTime(relation, known, adjustPointInTime);
										}
										AddToRelationList(relation.HRef, known, allIncomingRelations);
									}
								}
									
								if (href != hrefOut && registeredRelations.Contains(hrefOut)) {
									RelationBase known = registeredRelations[hrefOut];
									// A relation can only contain refs to OLDER relation entries.
									// So we correct the unknown PointInTime to be the same dateTime of 
									// the known entry subtract by the defaultRelationTimeCorrection.
									if (known.PointInTime > relation.PointInTime) {
										AdjustRelationPointInTime(relation, known, adjustPointInTime);
									}
									AddToRelationList(known.HRef, relation, registeredIncomingRelations);
								}

								if (href != hrefOut && selfIncomingLookup.Contains(hrefOut)) {
									AddToRelationList(hrefOut, relation, registeredIncomingRelations);
								}

							}
						}//foreach

						//reduce mem. usage:
						//relation.OutgoingRelations.TrimToSize();

					}

				}//lock
				
				// ensure new relation have a valid point in time entry.
				if (relation.PointInTime == RelationCosmos.UnknownPointInTime)
					relation.SetInternalPointInTime(DateTime.UtcNow);	// keep the movable information

			}//foreach(RelationBase)	

			Thread.Sleep(150);

			// If we get relation(s) to add in unordered manner, that relation can be older then some known.
			// So we have to loop over yet known relations and test, if the known outgoing contains
			// reference(s) to the new one. If so, we add an entry to registeredIncomingRelations.
			
			// in that case we simply add any new relation:
			if (registeredRelations.Count == 0)	{	
			
				lock(syncRoot) { 
					foreach (RelationBase relation in relations) {
						//add to registered collection
						if (!StringHelper.EmptyOrNull(relation.HRef)) 
						{
							if( !registeredRelations.ContainsKey(relation.HRef))
								registeredRelations.Add(relation.HRef, relation);
							if (relation.HRef != relation.Id && !registeredRelations.ContainsKey(relation.Id))
								registeredRelations.Add(relation.Id, relation);
						}
					}
				}
			
			} 
			else 
			{

				// The "CopyTo()" construct prevents against InvalidOpExceptions/ArgumentOutOfRange, 
				// if we modify registeredRelations here and from other thread(s)
				RelationBase[] values;
			
				//long secs = 0;
				//ProfilerHelper.StartMeasure(ref secs);

				lock (syncRoot) {
					values = new RelationBase[registeredRelations.Count];
					registeredRelations.Values.CopyTo(values, 0);	// takes 0.01 secs to copy 15,000 items...
				}

				//Trace.WriteLine(ProfilerHelper.StopMeasureString(secs), "RelationCosmos copy of values(" + values.Length.ToString() + ")");

				foreach (RelationBase known in values) {

					foreach (RelationBase relation in relations) {
					
						string href = relation.HRef;
						if (href == null || href.Length == 0)
							continue;

						lock(syncRoot) {
							if (known.outgoingRelationships.ContainsKey(href)) {
								if (known.CompareTo(relation) <= 0) {	// known is OLDER
									// A relation can only contain refs to OLDER relation entries.
									// so we correct the unknown PointInTime to be the same dateTime of 
									// the known entry subtract by the defaultRelationTimeCorrection.
									AdjustRelationPointInTime(relation, known, adjustPointInTime);
								}// else known is NEWER
								AddToRelationList(href, known, registeredIncomingRelations);
							}//contains(relation)

							//add to registered collection
							if (!registeredRelations.ContainsKey(href))
								registeredRelations.Add(href, relation);
							if (href != relation.Id && !registeredRelations.ContainsKey(relation.Id))
								registeredRelations.Add(relation.Id, relation);

						}
				
					}//foreach relation
				}//foreach known
			}//if
		}

		/// <summary>
		/// Remove a relation from the RelationCosmos.
		/// </summary>
		/// <param name="relation">Relation to be removed</param>
		public void Remove(RelationBase relation) {

			if (relation == null || relation.HRef == null) {
				return;
			}
			
			lock(syncRoot) {
				try {

					if (allRelations.ContainsKey(relation.HRef)) 
						allRelations.Remove(relation.HRef);
					
					if (registeredRelations.ContainsKey(relation.HRef)) 
						registeredRelations.Remove(relation.HRef);

					if (relation.Id != null) {
						if (allRelations.ContainsKey(relation.Id)) 
							allRelations.Remove(relation.Id);
						if (registeredRelations.ContainsKey(relation.Id)) 
							registeredRelations.Remove(relation.Id);
					}

					// remove from the real (known) outgoing relations
					if (relation.OutgoingRelations.Count > 0) 
					{
						foreach (string hrefOut in relation.outgoingRelationships.Keys) {
							if (hrefOut != null && hrefOut.Length > 0) {
								if (allIncomingRelations.ContainsKey(hrefOut)) {
									RemoveFromRelationList(hrefOut, relation, allIncomingRelations);
								}
								if (registeredIncomingRelations.ContainsKey(hrefOut)) {
									RemoveFromRelationList(hrefOut, relation, registeredIncomingRelations);
								}
								if (allRelations.ContainsKey(hrefOut)) 
									allRelations.Remove(hrefOut);
							}
						}
					} else {	
						// no outgoing links
					}

				} catch (Exception ex) {
					Trace.Write("RelationCosmos.Remove() exception:"+ ex.Message);
				}
			}			
		}

		/// <summary>
		/// Overloaded. Remove a amount of RelationBase objects from the RelationCosmos.
		/// </summary>
		/// <param name="relations">To be removed RelationBase object's</param>
        public void RemoveRange(IList<RelationBase> relations) { 
			foreach (RelationBase r in relations) {
				Remove(r);
			}
		}
		/// <summary>
		/// Overloaded. Remove a amount of RelationBase objects from the RelationCosmos.
		/// </summary>
		/// <param name="relations">To be removed RelationBase object's</param>
		public void RemoveRange(RelationBase[] relations) {
			foreach (RelationBase r in relations) {
				Remove(r);
			}
		}
		/// <summary>
		/// Clear all internal collections.
		/// </summary>
		public void Clear() { 
			allRelations.Clear();
			registeredRelations.Clear();
			allIncomingRelations.Clear();
			registeredIncomingRelations.Clear();
		}

		/// <summary>
		/// Returns a list of relations, that are known in RelationCosmos and pointing to
		/// the relation provided.
		/// </summary>
		/// <param name="relation">The object implementing RelationBase</param>
		/// <param name="excludeRelations">List of relations, 
		/// that should be excluded in that check</param>
		/// <returns>RelationList</returns>
        public RelationList GetIncoming(RelationBase relation, IList<RelationBase> excludeRelations) {
			if (relation == null || relation.HRef == null)
				return RelationList.Empty;

			lock(syncRoot) {
				try {
					if (excludeRelations == null)
						excludeRelations = RelationList.Empty;

					// check incoming (HRef):
					RelationList list = registeredIncomingRelations[relation.HRef];
					if (list != null && list.Count > 0) {
						RelationList returnList = new RelationList(list.Count);
						foreach (RelationBase r in list) {
							if (r != relation && !excludeRelations.Contains(r)) {
								returnList.Add(r);
							}
						}
						return returnList;
					}

					// check incoming (Id):
					if (relation.Id == null || relation.Id == relation.HRef)
						return RelationList.Empty;

					list = registeredIncomingRelations[relation.Id];
					if (list != null && list.Count > 0) {
						RelationList returnList = new RelationList(list.Count);
						foreach (RelationBase r in list) {
							if (r != relation && !excludeRelations.Contains(r)) {
								returnList.Add(r);
							}
						}
						return returnList;
					}


				} 
				catch (Exception ex) 
				{
					Trace.Write("RelationCosmos.GetIncoming() exception:"+ ex.Message);
				}

				return RelationList.Empty;
			}			
		}

		/// <summary>
		/// Returns a list of relations, that are known in RelationCosmos and that 
		/// the relation provided points to.
		/// </summary>
		/// <param name="relation">The object implementing RelationBase</param>
		/// <param name="excludeRelations">List of relations, 
		/// that should be excluded in that check</param>
		/// <returns>RelationList</returns>
        public RelationList GetOutgoing(RelationBase relation, IList<RelationBase> excludeRelations) { 
			if (relation == null || relation.HRef == null)
				return RelationList.Empty;

			lock(syncRoot) {
				try {
					if (excludeRelations == null)
						excludeRelations = RelationList.Empty;

					// check incoming:
					RelationHRefDictionary list = relation.outgoingRelationships;
					if (list != null && list.Count > 0) {
						RelationList returnList = new RelationList(list.Count);
						foreach (string hrefOut in list.Keys) {
							if (hrefOut != null && registeredRelations.Contains(hrefOut) && 
								!RelationListContains(excludeRelations, registeredRelations[hrefOut]) ) {
								returnList.Add(registeredRelations[hrefOut]);
							}
						}
						return returnList;
					}

				} catch (Exception ex) {
					Trace.Write("RelationCosmos.GetOutgoing() exception:"+ ex.Message);
				}

				return RelationList.Empty;
			}			
		}
		/// <summary>
		/// Returns a list merged of incoming and outging relations.
		/// </summary>
		/// <param name="relation">The object implementing RelationBase</param>
		/// <param name="excludeRelations">List of relations, 
		/// that should be excluded in that check</param>
		/// <returns>RelationList</returns>
        public RelationList GetIncomingAndOutgoing(RelationBase relation, IList<RelationBase> excludeRelations) { 
			RelationList returnList = new RelationList(this.GetIncoming(relation, excludeRelations));
			returnList.AddRange(this.GetOutgoing(relation, excludeRelations));
			if (returnList.Count > 0) {
				return returnList;
			}
			return RelationList.Empty;
		}
		
		/// <summary>
		/// Return true, if the <c>relation</c> has any incoming or outgoing relations 
		/// (registered/added to RelationCosmos). 
		/// </summary>
		/// <param name="relation">Relation to check</param>
		/// <param name="excludeRelations">List of strings with relation.HRef's, 
		/// that should be excluded in that check</param>
		/// <returns>True, if any relation was found, else false</returns>
        public bool HasIncomingOrOutgoing(RelationBase relation, IList<RelationBase> excludeRelations) { 
			
			if (relation == null || relation.HRef == null)
				return false;

			lock(syncRoot) {
				try {
					if (excludeRelations == null)
						excludeRelations = RelationList.Empty;

					// check outgoing:
					foreach (string hrefOut in relation.outgoingRelationships.Keys) {
						if (hrefOut != null && registeredRelations.Contains(hrefOut) && 
							!RelationListContains(excludeRelations, registeredRelations[hrefOut])) {
							return true;
						}
					}

					// check incoming (HRef):
					RelationList list = registeredIncomingRelations[relation.HRef];
					if (list != null) {
						foreach (RelationBase r in list) {
							if (r != relation && !excludeRelations.Contains(r)) {
								return true;
							}
						}
					}
					
					// check incoming (Id):
					if (relation.Id == null || relation.Id == relation.HRef)
						return false;

					list = registeredIncomingRelations[relation.Id];
					if (list != null) 
					{
						foreach (RelationBase r in list) 
						{
							if (r != relation && !excludeRelations.Contains(r)) 
							{
								return true;
							}
						}
					}

				} 
				catch (Exception ex) 
				{
					Trace.Write("RelationCosmos.HasIncomingOrOutgoing() exception:"+ ex.Message);
				}
				return false;
			}			
		}

		/// <summary>
		/// Not yet fully implemented/supported!
		/// </summary>
		public bool DeepCosmos { get { return deepCosmos; } set { deepCosmos = value; } }
		/// <summary>
		/// Set this to true, if RelationCosmos should try to adjust the PointInTime properties
		/// of added Relations, if they are adjustable. It can do so, because of the knowledge
		/// about the other Relations and their relationships to other.
		/// </summary>
		public bool AdjustPointInTime { get { return adjustPointInTime; } set { adjustPointInTime = value; } }
		#endregion

		#region private static methods
		private static RelationList GetRelationList(string key, RelationLists fromRelations) { 
			RelationList list = fromRelations[key];
			if (list == null) { 
				list = new RelationList(3);
				fromRelations.Add(key, list);
			}
			return list;  
		}
		private static void AddToRelationList(string key, RelationBase relation, RelationLists toRelations) { 
			RelationList list = GetRelationList(key, toRelations);
			if (!list.Contains(relation)) { 
				list.Add(relation);
			}
		}
		private static void RemoveFromRelationList(string key, RelationBase relation, RelationLists fromRelations) { 
			RelationList list = fromRelations[key];
			if (list != null)  {
				int removeIndex = list.IndexOf(relation);
				if (removeIndex >= 0) {
					list.RemoveAt(removeIndex);
				}
				if (list.Count == 0) {
					fromRelations.Remove(key);
				}
			}
		}
		private static bool RelationListContains(IList<RelationBase> relationList, RelationBase relation) 
		{
			if (relation == null)
				return false;

			foreach (RelationBase r in relationList) 
			{
				if (r.HRef == relation.HRef || r.Id == relation.Id)
					return true;
			}
			return false;
		}
/*
		private static bool RelationListContainsHRef(IList relationList, string href) 
		{
			foreach (RelationBase r in relationList) {
				if (r.HRef == href)
					return true;
			}
			return false;
		}
*/
		/// <summary>
		/// If the PointInTime of parameter <c>relation</c> is greater than <c>registeredRelation</c>,
		/// we adjust the them. We favor to adjust only relations with <c>PointInTimeIsAdjustable</c> set to true.
		/// But if <c>force</c> is true, we always adjust <c>relation</c> also if both <c>PointInTimeIsAdjustable</c>
		/// are false.</summary>
		/// <param name="relation">Relation to adjust</param>
		/// <param name="registeredRelation">Relation to compare to</param>
		/// <param name="force">Force a point in time adjustment.
		/// <c>PointInTimeIsAdjustable</c> of both <c>relation</c> and <c>registeredRelation</c> are
		/// considered before this happens</param>
		private static void AdjustRelationPointInTime(RelationBase relation, RelationBase registeredRelation, bool force) {
			if (relation.PointInTime > registeredRelation.PointInTime) {
				if (relation.PointInTimeIsAdjustable) {
					relation.SetInternalPointInTime(registeredRelation.PointInTime.Subtract(RelationCosmos.DefaultRelationTimeCorrection));
				} else if (registeredRelation.PointInTimeIsAdjustable && relation.PointInTime != RelationCosmos.UnknownPointInTime) {
					registeredRelation.SetInternalPointInTime(relation.PointInTime.Add(RelationCosmos.DefaultRelationTimeCorrection));
				} else if (force) {
					relation.SetInternalPointInTime(registeredRelation.PointInTime.Subtract(RelationCosmos.DefaultRelationTimeCorrection));
				}
			}
		}

		#endregion
		
		#region private methods
		private void ThreadRunAddRange(object state) {
			IList<RelationBase> relations = (IList<RelationBase>)state;
			
			//long secs = 0;
			//ProfilerHelper.StartMeasure(ref secs);
			this.InternalAddRange(relations);
			//Trace.WriteLine(ProfilerHelper.StopMeasureString(secs) + "(" + relations.Count.ToString() + " items)", "RelationCosmos.InternalAddRange(relations) Profiling");
		}
		#endregion

		#region private vars
		//private static readonly log4net.ILog _log = RssBandit.Common.Logging.Log.GetLogger(typeof(RelationCosmos));

		private Relations allRelations;
		private Relations registeredRelations;
		private RelationLists allIncomingRelations;
		private RelationLists registeredIncomingRelations;
		private PriorityThread worker;

		private bool deepCosmos = false;
		private bool adjustPointInTime = false;
		private object syncRoot = new Object();
		#endregion
	}

}
