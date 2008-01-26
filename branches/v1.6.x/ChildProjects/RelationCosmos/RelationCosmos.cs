using System;
using System.Collections;
using RelationCosmos.Collections;

namespace RelationCosmos
{
	/// <summary>
	/// Summary description for RelationCosmos.
	/// </summary>
	public class RelationCosmos
	{
		#region ctor's
		public RelationCosmos()	{
			allRelations = new Relations();
			registeredRelations = new Relations();
			allIncomingRelations = new RelationLists();
			registeredIncomingRelations = new RelationLists();
		}
		#endregion

		#region public static methods
		/// <summary>
		/// Provides a empty RelationList instance.
		/// </summary>
		public static RelationList EmptyRelationList { get { return emptyRelationList; } }
		public static RelationHRefList EmptyRelationHRefList { get { return emptyHRefList; } }
		public static TimeSpan DefaultRelationTimeCorrection { get { return defaultRelationTimeCorrection; } }
		public static DateTime UnknownPointInTime { get { return DateTime.MinValue; } }
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

					if (deepCosmos && !allRelations.ContainsKey(href)) {
						allRelations.Add(href, relation);
					}

					// add the real (known) outgoing relations
					if (relation.OutgoingRelations.Count > 0) {
						
						foreach (string hrefOut in relation.OutgoingRelations) {
							
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

						//reduce mem. usage:
						relation.OutgoingRelations.TrimToSize();

					} else {	
						// no outgoing links
					}

					// ensure new relation have a valid point in time entry.
					if (relation.PointInTime == UnknownPointInTime)
						relation.SetInternalPointInTime(DateTime.UtcNow);	// keep the movable information

					// If we get relation(s) to add in unordered manner, that relation can be older then some known.
					// So we have to loop over yet known relations and test, if the known outgoing contains
					// reference(s) to the new one. If so, we add an entry to registeredIncomingRelations.
					foreach (RelationBase known in registeredRelations.Values) {
						
						if (known.OutgoingRelations.Contains(href)) {
							if (known.CompareTo(relation) <= 0) {	// known is OLDER
								// A relation can only contain refs to OLDER relation entries.
								// so we correct the unknown PointInTime to be the same dateTime of 
								// the known entry subtract by the defaultRelationTimeCorrection.
								AdjustRelationPointInTime(relation, known, adjustPointInTime);
							} else { // known is NEWER
								//
							}
							AddToRelationList(relation.HRef, known, registeredIncomingRelations);
						}//contains(relation)
					}//foreach

					//add to registered collection
					registeredRelations.Add(href, relation);

				} catch (Exception ex) {
					System.Diagnostics.Trace.WriteLine("RelationCosmos.Add() exception: "+ex.Message);
				}
			}			
		}
		
		public void AddRange(RelationBase[] relations)  {
			foreach (RelationBase r in relations) {
				Add(r);
			}
		}
		public void AddRange(IList relations)  {
			foreach (RelationBase r in relations) {
				Add(r);
			}
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

					// remove from the real (known) outgoing relations
					if (relation.OutgoingRelations.Count > 0) {
						foreach (string hrefOut in relation.OutgoingRelations) {
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
					System.Diagnostics.Trace.WriteLine("RelationCosmos.Remove() exception: "+ex.Message);
				}
			}			
		}

		/// <summary>
		/// Overloaded. Remove a amount of RelationBase objects from the RelationCosmos.
		/// </summary>
		/// <param name="relations">To be removed RelationBase object's</param>
		public void RemoveRange(IList relations) { 
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
		public RelationList GetIncoming(RelationBase relation, IList excludeRelations) {
			if (relation == null || relation.HRef == null)
				return RelationCosmos.emptyRelationList;

			lock(syncRoot) {
				try {
					if (excludeRelations == null)
						excludeRelations = (IList) RelationCosmos.emptyRelationList;

					// check incoming:
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

				} catch (Exception ex) {
					System.Diagnostics.Trace.WriteLine("RelationCosmos.GetIncoming() exception: "+ex.Message);
				}

				return RelationCosmos.emptyRelationList;
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
		public RelationList GetOutgoing(RelationBase relation, IList excludeRelations) { 
			if (relation == null || relation.HRef == null)
				return RelationCosmos.emptyRelationList;

			lock(syncRoot) {
				try {
					if (excludeRelations == null)
						excludeRelations = (IList)RelationCosmos.emptyRelationList;

					// check incoming:
					RelationHRefList list = relation.OutgoingRelations;
					if (list != null && list.Count > 0) {
						RelationList returnList = new RelationList(list.Count);
						foreach (string hrefOut in list) {
							if (hrefOut != relation.HRef && !RelationListContainsHRef(excludeRelations, hrefOut) &&
								registeredRelations.Contains(hrefOut)) {
								returnList.Add(registeredRelations[hrefOut]);
							}
						}
						return returnList;
					}

				} catch (Exception ex) {
					System.Diagnostics.Trace.WriteLine("RelationCosmos.GetOutgoing() exception: "+ex.Message);
				}

				return RelationCosmos.emptyRelationList;
			}			
		}
		/// <summary>
		/// Returns a list merged of incoming and outging relations.
		/// </summary>
		/// <param name="relation">The object implementing RelationBase</param>
		/// <param name="excludeRelations">List of relations, 
		/// that should be excluded in that check</param>
		/// <returns>RelationList</returns>
		public RelationList GetIncomingAndOutgoing(RelationBase relation, IList excludeRelations) { 
			RelationList returnList = new RelationList(this.GetIncoming(relation, excludeRelations));
			returnList.AddRange(this.GetOutgoing(relation, excludeRelations));
			if (returnList.Count > 0) {
				return returnList;
			}
			return RelationCosmos.emptyRelationList;
		}
		
		/// <summary>
		/// Return true, if the <c>relation</c> has any incoming or outgoing relations 
		/// (registered/added to RelationCosmos). 
		/// </summary>
		/// <param name="relation">Relation to check</param>
		/// <param name="excludeRelations">List of strings with relation.HRef's, 
		/// that should be excluded in that check</param>
		/// <returns>True, if any relation was found, else false</returns>
		public bool HasIncomingOrOutgoing(RelationBase relation, IList excludeRelations) { 
			
			if (relation == null || relation.HRef == null)
				return false;

			lock(syncRoot) {
				try {
					if (excludeRelations == null)
						excludeRelations = RelationCosmos.emptyRelationList;

					// check outgoing:
					foreach (string hrefOut in relation.OutgoingRelations) {
						if (hrefOut != null && hrefOut != relation.HRef && 
							!RelationListContainsHRef(excludeRelations, hrefOut) && registeredRelations.Contains(hrefOut)) {
							return true;
						}
					}

					// check incoming:
					RelationList list = registeredIncomingRelations[relation.HRef];
					if (list != null) {
						foreach (RelationBase r in list) {
							if (r != relation && !excludeRelations.Contains(r)) {
								return true;
							}
						}
					}

				} catch (Exception ex) {
					System.Diagnostics.Trace.WriteLine("RelationCosmos.HasIncomingOrOutgoing() exception: "+ex.Message);
				}
				return false;
			}			
		}

		/// <summary>
		/// Not yet fully implemented/supported!
		/// </summary>
		public bool DeepCosmos { get { return deepCosmos; } set { deepCosmos = value; } }
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
		private static bool RelationListContainsHRef(IList relationList, string href) {
			foreach (RelationBase r in relationList) {
				if (r.HRef == href)
					return true;
			}
			return false;
		}
		/// <summary>
		/// If the PointInTime of parameter <c>relation</c> is greater than <c>registeredRelation</c>,
		/// we adjust the them. We favor to adjust only relations with <c>PointInTimeIsAdjustable</c> set to true.
		/// But if <c>force</c> is true, we always adjust <c>relation</c> also if both <c>PointInTimeIsAdjustable</c>
		/// are false.</summary>
		/// <param name="relation">Relation to adjust</param>
		/// <param name="registeredRelation">Relation to compare to</param>
		private static void AdjustRelationPointInTime(RelationBase relation, RelationBase registeredRelation, bool force) {
			if (relation.PointInTime > registeredRelation.PointInTime) {
				if (relation.PointInTimeIsAdjustable) {
					relation.SetInternalPointInTime(registeredRelation.PointInTime.Subtract(defaultRelationTimeCorrection));
				} else if (registeredRelation.PointInTimeIsAdjustable && relation.PointInTime != UnknownPointInTime) {
					registeredRelation.SetInternalPointInTime(relation.PointInTime.Add(defaultRelationTimeCorrection));
				} else if (force) {
					relation.SetInternalPointInTime(registeredRelation.PointInTime.Subtract(defaultRelationTimeCorrection));
				}
			}
		}

		#endregion
		
		#region private vars
		private static RelationList emptyRelationList = RelationList.ReadOnly(new RelationList(0));
		private static TimeSpan defaultRelationTimeCorrection = new TimeSpan(100);	// 100 nanosecs == 1 sec
		private static RelationHRefList emptyHRefList = RelationHRefList.ReadOnly(new RelationHRefList(0));

		private Relations allRelations;
		private Relations registeredRelations;
		private RelationLists allIncomingRelations;
		private RelationLists registeredIncomingRelations;

		private bool deepCosmos = false;
		private bool adjustPointInTime = true;
		private object syncRoot = new Object();
		#endregion
	}

}
