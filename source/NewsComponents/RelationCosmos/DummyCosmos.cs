
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

        public void AddRange(RelationBase[] relations)
        {
        }

        public void AddRange(IList<RelationBase> relations)
        {
        }

        public void Remove(RelationBase relation)
        {
        }

        public void RemoveRange(IList<RelationBase> relations)
        {
        }

        public void RemoveRange(RelationBase[] relations)
        {
        }

        public void Clear()
        {
        }

        public RelationList GetIncoming(RelationBase relation, IList<RelationBase> excludeRelations)
        {
            return new RelationList();
        }

        public RelationList GetOutgoing(RelationBase relation, IList<RelationBase> excludeRelations)
        {
            return new RelationList();
        }

        public RelationList GetIncomingAndOutgoing(RelationBase relation, IList<RelationBase> excludeRelations)
        {
            return new RelationList();
        }

        public bool HasIncomingOrOutgoing(RelationBase relation, IList<RelationBase> excludeRelations)
        {
            return false;
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
    }
}
