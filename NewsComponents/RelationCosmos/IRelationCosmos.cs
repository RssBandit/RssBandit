using System;
using System.Collections.Generic;
using NewsComponents.Collections;

namespace NewsComponents.RelationCosmos
{
	#region RelationCosmosFactory
	/// <summary>
	/// Factory to abstract from the real RelationCosmos 
	/// implementation algorithm(s)
	/// </summary>
	internal sealed class RelationCosmosFactory
	{
		/// <summary>
		/// Factory method to return different implementations
		/// </summary>
		/// <returns>IRelationCosmos</returns>
		public static IRelationCosmos Create() {
            return new RelationCosmos4();
		}

		private RelationCosmosFactory() {}
	}

	#endregion


    #region RelationCosmos
    /// <summary>
	/// Container for defaults, and the global string (url) table
	/// </summary>
	internal sealed class RelationCosmos
	{
	
		#region public static methods
		/// <summary>
		/// A timespan that express the time subtracted from a known point in time to get
		/// another related relation point in time adjusted. Default is one second.
		/// </summary>
		public static TimeSpan DefaultRelationTimeCorrection { get { return defaultRelationTimeCorrection; } }
		/// <summary>
		/// A default date representing the unknown point in time of a relation.
		/// </summary>
		public static DateTime UnknownPointInTime { get { return DateTime.MinValue; } }
		/// <summary>
		/// A hashtable table for adding URLs. This allows us to implement the equivalent 
		/// of string interning without the performance overhead of String.Intern
		/// </summary>
		public static StringTable UrlTable{ get { return urlTable; } }
		
		#endregion

		#region private members
		private readonly static TimeSpan defaultRelationTimeCorrection = new TimeSpan(100);	// 100 nanosecs == 1 sec
		private readonly static StringTable urlTable = new StringTable(); 
		#endregion

		private RelationCosmos(){}
	}
	#endregion

	#region IRelationCosmos
	public interface IRelationCosmos
	{
		/// <summary>
		/// Add a new Relation to the RelationCosmos. 
		/// The relation(s) should be registered in time order oldest first to
		/// prevent lost relationships.
		/// </summary>
		/// <param name="relation">Relation to add</param>
		/// <exception cref="ArgumentNullException">If relation or relation.HRef is null</exception>
		void Add<T>(T relation) where T : RelationBase<T>;

		/// <summary>
		/// Add a range of <c>RelationBase</c> objects
		/// </summary>
		/// <param name="relations">IList</param>
        void AddRange<T>(IEnumerable<T> relations) where T : RelationBase<T>;

		/// <summary>
		/// Remove a relation from the RelationCosmos.
		/// </summary>
		/// <param name="relation">Relation to be removed</param>
		void Remove<T>(T relation) where T : RelationBase<T>;

		/// <summary>
		/// Overloaded. Remove a amount of RelationBase objects from the RelationCosmos.
		/// </summary>
		/// <param name="relations">To be removed RelationBase object's</param>
        void RemoveRange<T>(IEnumerable<T> relations) where T : RelationBase<T>;


		/// <summary>
		/// Clear all internal collections.
		/// </summary>
		void Clear();

		/// <summary>
		/// Returns a list of relations, that are known in RelationCosmos and pointing to
		/// the relation provided.
		/// </summary>
		/// <param name="relation">The object implementing RelationBase</param>
		/// <param name="excludeRelations">List of relations, 
		/// that should be excluded in that check</param>
		/// <returns>RelationList</returns>
        IList<T> GetIncoming<T>(T relation, IList<T> excludeRelations) where T : RelationBase<T>;

        /// <summary>
        /// Returns a list of relations, that are known in RelationCosmos and pointing to
        /// the URL provided.
        /// </summary>
        /// <param name="hRef">The URL</param>
        /// <param name="since">The date used to filter the returned relations. Only items that have been published since 
        /// that date are return</param>
        /// <returns>RelationList</returns>
        IList<T> GetIncoming<T>(string hRef, DateTime since) where T : RelationBase<T>;

		/// <summary>
		/// Returns a list of relations, that are known in RelationCosmos and that 
		/// the relation provided points to.
		/// </summary>
		/// <param name="relation">The object implementing RelationBase</param>
		/// <param name="excludeRelations">List of relations, 
		/// that should be excluded in that check</param>
		/// <returns>RelationList</returns>
        IList<T> GetOutgoing<T>(T relation, IList<T> excludeRelations) where T : RelationBase<T>;

		/// <summary>
		/// Returns a list merged of incoming and outging relations.
		/// </summary>
		/// <param name="relation">The object implementing RelationBase</param>
		/// <param name="excludeRelations">List of relations, 
		/// that should be excluded in that check</param>
		/// <returns>RelationList</returns>
        IList<T> GetIncomingAndOutgoing<T>(T relation, IList<T> excludeRelations) where T : RelationBase<T>;

		/// <summary>
		/// Return true, if the <c>relation</c> has any incoming or outgoing relations 
		/// (registered/added to RelationCosmos). 
		/// </summary>
		/// <param name="relation">Relation to check</param>
		/// <param name="excludeRelations">List of strings with relation.HRef's, 
		/// that should be excluded in that check</param>
		/// <returns>True, if any relation was found, else false</returns>
        bool HasIncomingOrOutgoing<T>(T relation, IList<T> excludeRelations) where T : RelationBase<T>;

		/// <summary>
		/// Not yet fully implemented/supported!
		/// </summary>
		bool DeepCosmos { get; set; }

		/// <summary>
		/// Set this to true, if RelationCosmos should try to adjust the PointInTime properties
		/// of added Relations, if they are adjustable. It can do so, because of the knowledge
		/// about the other Relations and their relationships to other.
		/// </summary>
		bool AdjustPointInTime { get; set; }
	}
	#endregion
}
