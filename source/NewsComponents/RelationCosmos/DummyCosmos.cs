
using System.Collections.Generic;
using NewsComponents.Collections;

namespace NewsComponents.RelationCosmos
{
    /// <summary>
    /// For testing baseline memory usage only - do not use in production
    /// </summary>
    internal class DummyCosmos : IRelationCosmos
    {
        
        public void Add(RelationBase relation)
        {
        }

        public void AddRange<T>(IEnumerable<T> relations)
            where T : RelationBase
        {
        }

        public void Remove(RelationBase relation)
        {
        }

        public void RemoveRange<T>(IEnumerable<T> relations)
            where T : RelationBase
        {
        }

        public void Clear()
        {
        }

    

        private bool _deepCosmos;
        public bool DeepCosmos
        {
            get
            {
                return _deepCosmos;
            }
            set
            {
                _deepCosmos = value;
            }
        }

        private bool _adjustTime;
        public bool AdjustPointInTime
        {
            get
            {
                return _adjustTime;
            }
            set
            {
                _adjustTime = value;
            }
        }

        public RelationList GetIncoming<T>(RelationBase relation, IList<T> excludeRelations) where T : RelationBase
        {
            return new RelationList();
        }

        public RelationList GetOutgoing<T>(RelationBase relation, IList<T> excludeRelations) where T : RelationBase
        {
            return new RelationList();
        }

        public RelationList GetIncomingAndOutgoing<T>(RelationBase relation, IList<T> excludeRelations) where T : RelationBase
        {
            return new RelationList();
        }

        public bool HasIncomingOrOutgoing<T>(RelationBase relation, IList<T> excludeRelations) where T : RelationBase
        {
            return false;
        }
    }
}
