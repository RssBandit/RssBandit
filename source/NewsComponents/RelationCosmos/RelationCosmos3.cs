#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Reflection;
using System.Collections.Generic;
using Tst;

namespace NewsComponents.RelationCosmos
{
	/// <summary>
	/// RelationCosmos3 provide another alternative impl. to RelationCosmos
	/// It uses the TstDictionaries/TstDictionary classes to speed up lookups.
	/// </summary>
	public class RelationCosmos3: IRelationCosmos
	{
		private static readonly log4net.ILog _log = RssBandit.Common.Logging.Log.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		private Dictionary<string, IRelation> registeredRelations = new Dictionary<string, IRelation>();
		private TstDictionaries relationsLinkTo = new TstDictionaries();

		private object syncRoot = new Object();

		/// <summary>
		/// Initializes a new instance of the <see cref="RelationCosmos3"/> class.
		/// </summary>
		public RelationCosmos3(){}


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
		/// <param name="relations">RelationBase[]</param>
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
					TstDictionary list = relationsLinkTo[hRef];
					if (list != null) {
						foreach (T linkBack in list.Values) {
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
            return new List<T>() ;
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
					TstDictionary list = relationsLinkTo[hRef];
					if (list != null) {
						foreach (T linkBack in list.Values) {
							if (hRef != linkBack.HRef && ! excludeRelations.Contains(linkBack) )
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
		private void InternalAddRange<T>(IEnumerable<T> relations) 
            where T : RelationBase<T>
        {
			if (relations == null) return;
			lock (syncRoot) {
                foreach (T relation in relations)
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
						registeredRelations.Add(href, relation);
						//_log.Info("Add(href):" + href);

					}

					foreach (string hrefOut in relation.outgoingRelationships) {
						AddToRelationList(hrefOut, relation, relationsLinkTo);
						//_log.Info("Add(hOut):" + hrefOut);
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
		private static void AddToRelationList<T>(string key, T relation, TstDictionaries toRelations) 
            where T : RelationBase<T>
        { 
			TstDictionary list = GetRelationList(key, toRelations);
			string href = relation.HRef;
			if (href != null && !list.Contains(href)) { 
				list.Add(href, relation);
			}
		}
		private static void RemoveFromRelationList<T>(string key, T relation, TstDictionaries fromRelations) 
            where T : RelationBase<T>
        { 
			if (key != null) {
				TstDictionary list = fromRelations[key];
				if (list != null)  {
					string href = relation.HRef;
					if (href != null && !list.Contains(href)) 
						list.Remove(href);
					if (list.Count == 0) {
						fromRelations.Remove(key);
					}
				}
			}
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
