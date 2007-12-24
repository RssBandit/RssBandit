#region CVS Version Header
/*
 * $Id: RelationCosmos3.cs 114 2007-11-30 16:23:52Z t_rendelmann $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2007-11-30 08:23:52 -0800 (Fri, 30 Nov 2007) $
 * $Revision: 114 $
 */
#endregion

using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using NewsComponents.Collections;
using Tst;

namespace NewsComponents.RelationCosmos
{
	/// <summary>
	/// RelationCosmos provide a way to look up incoming and outgoing links to 
    /// RelationBase instances in a fast and efficient manner. 
	/// </summary>
	public class RelationCosmos4: IRelationCosmos
	{
		private static readonly log4net.ILog _log = RssBandit.Common.Logging.Log.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
        /// <summary>
        /// List of all RelationBase instances we know about. 
        /// </summary>
		private Relations registeredRelations = new Relations();

        /// <summary>
        /// Table of all outgoing links and the RelationBase instance(s) that link to them. We use Object becase the 
        /// value may be instance of RelationBase or RelationList. 
        /// </summary>
        private Dictionary<string, Object> relationsLinkTo = new Dictionary<string, Object>(50000);

		private object syncRoot = new Object();

		/// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
		public RelationCosmos4(){}


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
		public void AddRange(IEnumerable<RelationBase> relations) {
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
        public void RemoveRange(IEnumerable<RelationBase> relations) {
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
        public RelationList GetIncoming(RelationBase relation, IList<RelationBase> excludeRelations) {
            if (relation == null) return RelationList.Empty;

            lock (syncRoot) {
                RelationList ret = new RelationList(relation.OutgoingRelations.Count);
                string hRef = relation.HRef;
                if (hRef != null && hRef.Length > 0) {
                    //_log.Info("GetIncoming(href):" + hRef);
                    Object value;
                    bool inDictionary = relationsLinkTo.TryGetValue(hRef, out value);
                    if (inDictionary) {
                        RelationList list = value as RelationList;
                        RelationBase item = value as RelationBase;

                        if (list != null) {
                            foreach (RelationBase linkBack in list) {
                                if (!excludeRelations.Contains(linkBack) && !ret.Contains(linkBack)) {
                                    ret.Add(linkBack);
                                }
                            }
                        } else if (item != null) {
                            if (!excludeRelations.Contains(item)) {
                                ret.Add(item);
                           }
                        }
                    }//if(inDictionary)

                }//if (hRef != null && hRef.Length > 0) {
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
        public RelationList GetOutgoing(RelationBase relation, IList<RelationBase> excludeRelations) {
			if (relation == null) return RelationList.Empty;
			IList<string> excludeUrls = GetRelationUrls(excludeRelations);
			
			lock (syncRoot) {
				RelationList ret = new RelationList(relation.OutgoingRelations.Count);
				foreach (string hrefOut in relation.outgoingRelationships) {
					if (excludeUrls.Contains(hrefOut))
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
        public RelationList GetIncomingAndOutgoing(RelationBase relation, IList<RelationBase> excludeRelations) {
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
        public bool HasIncomingOrOutgoing(RelationBase relation, IList<RelationBase> excludeRelations) {
			if (relation == null) return false;
			IList<string> excludeUrls = GetRelationUrls(excludeRelations);

            lock (syncRoot) {
                // check for outgoing:
                foreach (string hrefOut in relation.outgoingRelationships) {
                    if (excludeUrls.Contains(hrefOut))
                        continue;

                    if (hrefOut != relation.HRef && registeredRelations.ContainsKey(hrefOut))
                        return true;
                }

                // check for incoming:
                string hRef = relation.HRef;
                if (hRef != null && hRef.Length > 0) {
                    Object value;
                    bool inDictionary = relationsLinkTo.TryGetValue(hRef, out value);
                    if (inDictionary) {
                        RelationList list = value as RelationList;
                        RelationBase item = value as RelationBase;

                        if (list != null) {
                            foreach (RelationBase linkBack in list) {
                                if (hRef != linkBack.HRef && !excludeRelations.Contains(linkBack)) {
                                    return true;
                                }
                            }
                        } else if (item != null) {
                            if (!excludeRelations.Contains(item)) {
                                return true;
                            }
                        }
                    }//if(inDictionary)
                }//if (hRef != null && hRef.Length > 0)
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
		private void InternalAddRange(IEnumerable<RelationBase> relations) {
			if (relations == null) return;
			lock (syncRoot) {
                foreach(RelationBase relation in relations)
                    InternalAdd(relation);
			}
		}

		private void InternalAdd(RelationBase relation) {
			if (relation == null) return;

			try {
				lock (syncRoot) {
					//TR: causes collection modified exception if called from InternalAddRange:
					// NewsHandler.MergeAndPurgeItems() was the entry point; I added a single 
					// RelationCosmosRemove() instead there directly.
					//InternalRemove(relation);
				
					string href = relation.HRef;
					if (href != null && href.Length > 0 && ! registeredRelations.ContainsKey(href)) {
						registeredRelations.Add(href, relation);
					}

					foreach (string hrefOut in relation.outgoingRelationships) {
						AddToRelationList(hrefOut, relation, relationsLinkTo);
					}

				}

			} catch (Exception ex) {
				_log.Error("InternalAdd() caused exception", ex);
			}
		}

		private void InternalRemove(RelationBase relation) {
			if (relation == null) return;
			lock (syncRoot) {
				foreach (string hrefOut in relation.outgoingRelationships) {
					RemoveFromRelationList(hrefOut, relation, relationsLinkTo);
				}
				if (relation.HRef != null) registeredRelations.Remove(relation.HRef);	
			}
		}

        private void InternalRemoveRange(IEnumerable<RelationBase> relations) {
			if (relations == null) return;
			lock (syncRoot) {
				foreach(RelationBase relation in relations)
					InternalRemove(relation);
			}
		}

		#endregion

		#region private static methods
		private static TstDictionary GetRelationList(string key, TstDictionaries fromRelations) { 
			TstDictionary list = fromRelations[key];
			if (list == null) { 
				list = new TstDictionary();
				fromRelations.Add(key, list);
			}
			return list;  
		}
        
        private static void AddToRelationList(string key, RelationBase relation, Dictionary<string, Object> toRelations) {
            Object value; 
            bool inDictionary  = toRelations.TryGetValue(key, out value);
            string href = relation.HRef;

            //To reduce creating unnecessary objects we add first RelationBase that links to a URL directly
            //into table. Only on subsequent links to a URL do we creat a RelationList. 
            if(href != null){
                if (inDictionary) {
                    RelationList list = value as RelationList;
                    RelationBase item = value as RelationBase; 
                    
                    if (list != null) {
                        list.Add(relation);
                    } else if (item != null) {
                        list = new RelationList();
                        list.Add(item);
                        list.Add(relation); 
                        toRelations[href] = list; 
                    }

                } else {
                    toRelations.Add(href, relation);
                }
            }//if(href != null)

        }

        private static void RemoveFromRelationList(string key, RelationBase relation, Dictionary<string, Object> fromRelations) {
            if (key != null) {
                Object value;
                bool inDictionary = fromRelations.TryGetValue(key, out value);

                if (inDictionary) {
                    RelationList list = value as RelationList;
                    RelationBase item = value as RelationBase; 
                   
                    if (list != null) {
                            list.Remove(relation);
                        if (list.Count == 0) {
                            fromRelations.Remove(key);
                        }
                    } else if (item != null) {
                        fromRelations.Remove(key);
                    }
                }//if(inDictionary)
            }//if(key != null) 
        }

        private static IList<string> GetRelationUrls(IList<RelationBase> relations) {
            List<string> urls = new List<string>();
			foreach (RelationBase relation in relations) {
				string href = relation.HRef;
                if ((href != null) && !urls.Contains(href)) {
					urls.Add(href);
				}
			}
            return urls;
		}

		#endregion
	}
}
