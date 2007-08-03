#region CVS Version Header
/*
 * $Id: RelationCosmos2.cs,v 1.5 2005/10/25 16:00:30 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/10/25 16:00:30 $
 * $Revision: 1.5 $
 */
#endregion

using System;
using System.Collections;
using System.Reflection;
using NewsComponents.Collections;

namespace NewsComponents.RelationCosmos
{
	/// <summary>
	/// RelationCosmos2 provide a alternative impl. to RelationCosmos
	/// </summary>
	public class RelationCosmos2: IRelationCosmos
	{
		private static readonly log4net.ILog _log = RssBandit.Common.Logging.Log.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		private Relations registeredRelations = new Relations();
		private RelationLists relationsLinkTo = new RelationLists();

		private object syncRoot = new Object();

		/// <summary>
		/// Initializes a new instance of the <see cref="RelationCosmos2"/> class.
		/// </summary>
		public RelationCosmos2(){}


		#region IRelationCosmos Members

		/// <summary>
		/// Add a new Relation to the RelationCosmos.
		/// </summary>
		/// <param name="relation">Relation to add</param>
		public void Add(RelationBase relation) {
			InternalAdd(relation);
		}

		/// <summary>
		/// Add a range of <c>RelationBase</c> objects
		/// </summary>
		/// <param name="relations">RelationBase[]</param>
		public void AddRange(RelationBase[] relations) {
			InternalAddRange(relations);
		}

		/// <summary>
		/// Add a range of <c>RelationBase</c> objects
		/// </summary>
		/// <param name="relations">IList</param>
		void NewsComponents.RelationCosmos.IRelationCosmos.AddRange(IList relations) {
			InternalAddRange(relations);
		}

		/// <summary>
		/// Remove a relation from the RelationCosmos.
		/// </summary>
		/// <param name="relation">Relation to be removed</param>
		public void Remove(RelationBase relation) {
			InternalRemove(relation);
		}

		/// <summary>
		/// Overloaded. Remove a amount of RelationBase objects from the RelationCosmos.
		/// </summary>
		/// <param name="relations">To be removed RelationBase object's</param>
		public void RemoveRange(System.Collections.IList relations) {
			InternalRemoveRange(relations);
		}

		/// <summary>
		/// Overloaded. Remove a amount of RelationBase objects from the RelationCosmos.
		/// </summary>
		/// <param name="relations">To be removed RelationBase object's</param>
		void NewsComponents.RelationCosmos.IRelationCosmos.RemoveRange(RelationBase[] relations) {
			InternalRemoveRange(relations);
		}

		/// <summary>
		/// Clear all internal collections.
		/// </summary>
		public void Clear() {
			lock (syncRoot) {
				relationsLinkTo.Clear();
				registeredRelations.Clear();
			}
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
			if (relation == null) return RelationList.Empty;
			
			lock (syncRoot) {
				RelationList ret = new RelationList(relation.OutgoingRelations.Count);
				string hRef = relation.HRef;
				if (hRef != null && hRef.Length > 0) {
					//_log.Info("GetIncoming(href):" + hRef);
					IList list = relationsLinkTo[hRef];
					if (list != null) {
						foreach (RelationBase linkBack in list) {
							if ( ! excludeRelations.Contains(linkBack) && ! ret.Contains(linkBack)) {
								ret.Add(linkBack);
								//_log.Info(" ^--:" + linkBack.HRef);
							}
						}
					}
				}
				hRef = relation.Id;
				if (hRef != null && hRef != relation.HRef) {
					//_log.Info("GetIncoming( id ):" + hRef);
					IList list = relationsLinkTo[hRef];
					if (list != null) {
						foreach (RelationBase linkBack in list) {
							if ( ! excludeRelations.Contains(linkBack) && ! ret.Contains(linkBack)) {
								ret.Add(linkBack);
								//_log.Info(" ^--:" + linkBack.HRef);
							}
						}
					}
				}
				return ret;
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
			if (relation == null) return RelationList.Empty;
			Hashtable excludeUrls = GetRelationUrls(excludeRelations);
			
			lock (syncRoot) {
				RelationList ret = new RelationList(relation.OutgoingRelations.Count);
				foreach (string hrefOut in relation.outgoingRelationships.Keys) {
					if (excludeUrls.ContainsKey(hrefOut))
						continue;
					
					RelationBase r = registeredRelations[hrefOut];
					if (r != null) ret.Add(r);
				}
				return ret;
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
			// not required...?
			return RelationList.Empty;
		}

		/// <summary>
		/// Return true, if the <c>relation</c> has any incoming or outgoing relations
		/// (registered/added to RelationCosmos).
		/// </summary>
		/// <param name="relation">Relation to check</param>
		/// <param name="excludeRelations">List of strings with relation.HRef's,
		/// that should be excluded in that check</param>
		/// <returns>
		/// True, if any relation was found, else false
		/// </returns>
		public bool HasIncomingOrOutgoing(RelationBase relation, IList excludeRelations) {
			if (relation == null) return false;
			Hashtable excludeUrls = GetRelationUrls(excludeRelations);
			
			lock (syncRoot) {
				// check for outgoing:
				foreach (string hrefOut in relation.outgoingRelationships.Keys) {
					if (excludeUrls.ContainsKey(hrefOut))
						continue;
					
					if (registeredRelations.ContainsKey(hrefOut)) 
						return true;
				}
				
				// check for incoming:
				string hRef = relation.HRef;
				if (hRef != null && hRef.Length > 0) {
					IList list = relationsLinkTo[hRef];
					if (list != null) {
						foreach (RelationBase linkBack in list) {
							if ( ! excludeRelations.Contains(linkBack) )
								return true;
					
						}
					}
				}

				hRef = relation.Id;
				if (hRef != null && hRef != relation.HRef) {
					IList list = relationsLinkTo[hRef];
					if (list != null) {
						foreach (RelationBase linkBack in list) {
							if ( ! excludeRelations.Contains(linkBack) )
								return true;
					
						}
					}
				}

			}

			return false;
		}

		/// <summary>
		/// Not yet fully implemented/supported!
		/// </summary>
		/// <value></value>
		public bool DeepCosmos {
			get { return false; }
			set {}
		}

		/// <summary>
		/// Set this to true, if RelationCosmos should try to adjust the PointInTime properties
		/// of added Relations, if they are adjustable. It can do so, because of the knowledge
		/// about the other Relations and their relationships to other.
		/// </summary>
		/// <value></value>
		public bool AdjustPointInTime {
			get { return false; }
			set {}
		}

		#endregion

		#region private members
		private void InternalAddRange(IList relations) {
			if (relations == null) return;
			lock (syncRoot) {
				for (int i=0; i< relations.Count; i++)	
					InternalAdd((RelationBase)relations[i]);
			}
		}

		private void InternalAdd(RelationBase relation) {
			if (relation == null) return;

			try {
				lock (syncRoot) {
					InternalRemove(relation);
				
					string href = relation.HRef;
					if (href != null && href.Length > 0 && ! registeredRelations.ContainsKey(href)) {
						registeredRelations.Add(href, relation);
						//_log.Info("Add(href):" + href);

					}
					if (relation.Id != null && href != relation.Id && ! registeredRelations.ContainsKey(relation.Id)) {
						registeredRelations.Add(relation.Id, relation);		
						//_log.Info("Add( id ):" + relation.Id);
					}
					foreach (string hrefOut in relation.outgoingRelationships.Keys) {
						AddToRelationList(hrefOut, relation, relationsLinkTo);
						//_log.Info("Add(hOut):" + hrefOut);
					}

				}

			} catch (Exception ex) {
				_log.Error("InternalAdd() caused exception", ex);
			}
		}

		private void InternalRemove(RelationBase relation) {
			if (relation == null) return;
			lock (syncRoot) {
				foreach (string hrefOut in relation.outgoingRelationships.Keys) {
					RemoveFromRelationList(hrefOut, relation, relationsLinkTo);
				}
				if (relation.HRef != null) registeredRelations.Remove(relation.HRef);	
				if (relation.Id != null) registeredRelations.Remove(relation.Id);	
			}
		}

		private void InternalRemoveRange(IList relations) {
			if (relations == null) return;
			lock (syncRoot) {
				for (int i=0; i< relations.Count; i++)	
					InternalRemove((RelationBase)relations[i]);
			}
		}

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
			if (key != null) {
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
		}
/*
		private static bool RelationListContains(IList relationList, RelationBase relation) {
			if (relation == null)
				return false;

			foreach (RelationBase r in relationList) {
				if (r.HRef == relation.HRef || r.Id == relation.Id)
					return true;
			}
			return false;
		}
*/

		private static Hashtable GetRelationUrls(IList relations) {
			Hashtable hashtable = new Hashtable();
			foreach (RelationBase relation in relations) {
				string text1 = relation.HRef;
				if ((text1 != null) && !hashtable.ContainsKey(text1)) {
					hashtable.Add(text1, null);
				}
				text1 = relation.Id;
				if (text1 != null && !hashtable.ContainsKey(text1)) {
					hashtable.Add(text1, null);
				}
			}
			return hashtable;
		}

		private static bool LooksLikeAnUrl(string s) {
			if (s != null)	
				return s.IndexOf("://") > 0;
			return false;
		}
		#endregion
	}
}
