#region CVS Version Header
/*
 * $Id: RelationCosmos3.cs 114 2007-11-30 16:23:52Z t_rendelmann $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2007-11-30 08:23:52 -0800 (Fri, 30 Nov 2007) $
 * $Revision: 114 $
 */
#endregion

using System;
using System.Reflection;
using System.Collections.Generic;
using Tst;

namespace NewsComponents.RelationCosmos
{

    /// <summary>
    /// Since all URLs are interned in StringTable we can use object equality to speed up lookups. 
    /// </summary>
    internal class StringComparer : IEqualityComparer<string> {

        public static StringComparer Comparer = new StringComparer();

        public bool Equals(string s1, string s2) {
            return Object.ReferenceEquals(s1, s2);              
        }

        public int GetHashCode(string s) {
            return s.GetHashCode();
        }


    }
	/// <summary>
	/// RelationCosmos provide a way to look up incoming and outgoing links to 
    /// RelationBase instances in a fast and efficient manner. 
	/// </summary>
	public class RelationCosmos4: IRelationCosmos
	{
		private static readonly log4net.ILog _log = RssBandit.Common.Logging.Log.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
        /// <summary>
        /// List of all IRelation instances we know about. 
        /// </summary>
        private readonly Dictionary<string, IRelation> registeredRelations = new Dictionary<string, IRelation>(StringComparer.Comparer);

        /// <summary>
        /// Table of all outgoing links and the RelationBase instance(s) that link to them. We use Object becase the 
        /// value may be instance of RelationBase or RelationList. 
        /// </summary>
        private readonly Dictionary<string, Object> relationsLinkTo = new Dictionary<string, Object>(50000,StringComparer.Comparer);

		private readonly object syncRoot = new Object();

		/// <summary>
		/// Initializes a new instance of the class.
		/// </summary>
		public RelationCosmos4(){}


		#region IRelationCosmos Members

		/// <summary>
		/// Add a new Relation to the RelationCosmos.
		/// </summary>
		/// <param name="relation">Relation to add</param>
		public void Add<T>(T relation) 
            where T : RelationBase<T>
        {
			InternalAdd(relation);
		}

		/// <summary>
		/// Add a range of <c>RelationBase</c> objects
		/// </summary>
		/// <param name="relations">RelationBase objects</param>
		public void AddRange<T>(IEnumerable<T> relations)
            where T : RelationBase<T>
        {
			InternalAddRange(relations);
		}


		/// <summary>
		/// Remove a relation from the RelationCosmos.
		/// </summary>
		/// <param name="relation">Relation to be removed</param>
		public void Remove<T>(T relation) 
            where T : RelationBase<T>
        {
			InternalRemove(relation);
		}

		/// <summary>
		/// Overloaded. Remove a amount of RelationBase objects from the RelationCosmos.
		/// </summary>
		/// <param name="relations">To be removed RelationBase object's</param>
        public void RemoveRange<T>(IEnumerable<T> relations) 
            where T : RelationBase<T>
        {
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
        /// the URL provided.
        /// </summary>
        /// <param name="hRef">The URL</param>
        /// <param name="since">The date used to filter the returned relations. Only items that have been published since 
        /// that date are return</param>
        /// <returns>RelationList</returns>
        public IList<T> GetIncoming<T>(string hRef, DateTime since)
            where T : RelationBase<T> 
        {
            IList<T> ret = new List<T>();
            if (String.IsNullOrEmpty(hRef)) return ret;

            lock (syncRoot) {
                Object value;
                bool inDictionary = relationsLinkTo.TryGetValue(hRef, out value);
                if (inDictionary) {
                    IList<T> list = value as IList<T>;
                    T item = value as T;

                    if (list != null) {
                        foreach (T linkBack in list) {
                            if (linkBack.PointInTime > since) {
                                ret.Add(linkBack);
                            }
                        }
                    } else if (item != null) {
                        if (item.PointInTime > since) {
                            ret.Add(item);
                        }
                    }
                }//if(inDictionary)
            }

            return ret;         
        }

		/// <summary>
		/// Returns a list of relations, that are known in RelationCosmos and pointing to
		/// the relation provided.
		/// </summary>
		/// <param name="relation">The object implementing RelationBase</param>
		/// <param name="excludeRelations">List of relations,
		/// that should be excluded in that check</param>
		/// <returns>RelationList</returns>
        public IList<T> GetIncoming<T>(T relation, IList<T> excludeRelations)
            where T : RelationBase<T>
        {
            if (relation == null) return new List<T>();

            lock (syncRoot) {
                IList<T> ret = new List<T>(relation.OutgoingRelations.Count);
                string hRef = relation.HRef;
                if (hRef != null && hRef.Length > 0) {
                    //_log.Info("GetIncoming(href):" + hRef);
                    Object value;
                    bool inDictionary = relationsLinkTo.TryGetValue(hRef, out value);
                    if (inDictionary) {
                        IList<T> list = value as IList<T>;
                        T item = value as T;

                        if (list != null) {
                            foreach (T linkBack in list) {
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
        public IList<T> GetOutgoing<T>(T relation, IList<T> excludeRelations) 
            where T : RelationBase<T>
        {
            if (relation == null) return new List<T>();

			IList<string> excludeUrls = GetRelationUrls(excludeRelations);
			
			lock (syncRoot) {
				IList<T> ret = new List<T>(relation.OutgoingRelations.Count);
				foreach (string hrefOut in relation.outgoingRelationships) {
					if (excludeUrls.Contains(hrefOut))
						continue;

                    IRelation r;
                    if (registeredRelations.TryGetValue(hrefOut, out r))
                    {
                        if (r != null)
                            ret.Add((T)r);
                    }
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
        public IList<T> GetIncomingAndOutgoing<T>(T relation, IList<T> excludeRelations) 
            where T : RelationBase<T>
        {
			// not required...?
            return new List<T>();
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
        public bool HasIncomingOrOutgoing<T>(T relation, IList<T> excludeRelations) 
            where T : RelationBase<T>
        {
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
                        IList<T> list = value as IList<T>;
                        T item = value as T;

                        if (list != null) {
                            foreach (T linkBack in list) {
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
		private void InternalAddRange<T>(IEnumerable<T> relations) 
            where T : RelationBase<T>
        {
			if (relations == null) return;
			lock (syncRoot) {
                foreach(T relation in relations)
                    InternalAdd(relation);
			}
		}

		private void InternalAdd<T>(T relation) 
            where T : RelationBase<T>
        {
			if (relation == null) return;

			try {
				lock (syncRoot) {
					//TR: causes collection modified exception if called from InternalAddRange:
					// NewsHandler.MergeAndPurgeItems() was the entry point; I added a single 
					// RelationCosmosRemove() instead there directly.
					//InternalRemove(relation);
				
					string href = relation.HRef;
					if (href != null && href.Length > 0 && ! registeredRelations.ContainsKey(href)) {
						registeredRelations[href] = relation;
					}

					foreach (string hrefOut in relation.outgoingRelationships) {
						AddToRelationList(hrefOut, relation, relationsLinkTo);
					}

				}

			} catch (Exception ex) {
				_log.Error("InternalAdd() caused exception", ex);
			}
		}

		private void InternalRemove<T>(T relation)
            where T : RelationBase<T>
        {
			if (relation == null) return;
			lock (syncRoot) {
				foreach (string hrefOut in relation.outgoingRelationships) {
					RemoveFromRelationList(hrefOut, relation, relationsLinkTo);
				}
				if (relation.HRef != null) registeredRelations.Remove(relation.HRef);	
			}
		}

        private void InternalRemoveRange<T>(IEnumerable<T> relations) 
            where T : RelationBase<T>
        {
			if (relations == null) return;
			lock (syncRoot) {
				foreach(T relation in relations)
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
        
        private static void AddToRelationList<T>(string key, T relation, IDictionary<string, Object> toRelations) 
            where T : RelationBase<T>
        {
            Object value; 
            bool inDictionary  = toRelations.TryGetValue(key, out value);
            string href = relation.HRef;

            //To reduce creating unnecessary objects we add first RelationBase that links to a URL directly
            //into table. Only on subsequent links to a URL do we creat a RelationList. 
            if(href != null){

                if (href.Equals(key)) { //item shouldn't link to itself
                    return;
                }

                if (inDictionary) {
                    IList<T> list = value as IList<T>;
                    T item = value as T; 
                    
                    if (list != null) {
                        list.Add(relation);
                    } else if (item != null) {
                        list = new List<T>();
                        list.Add(item);
                        list.Add(relation); 
                        toRelations[key] = list; 
                    }

                } else {
                    toRelations.Add(key, relation);
                }
            }//if(href != null)

        }

        private static void RemoveFromRelationList<T>(string key, T relation, Dictionary<string, Object> fromRelations) 
            where T : RelationBase<T>
        {
            if (key != null) {
                Object value;
                bool inDictionary = fromRelations.TryGetValue(key, out value);

                if (inDictionary) {
                    IList<T> list = value as IList<T>;
                    T item = value as T; 
                   
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

        private static IList<string> GetRelationUrls<T>(IEnumerable<T> relations) 
            where T : IRelation
        {
            List<string> urls = new List<string>();
			foreach (T relation in relations) {
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
