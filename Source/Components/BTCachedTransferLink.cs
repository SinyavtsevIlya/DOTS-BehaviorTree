using Unity.Entities;

namespace Nanory.Unity.Entities.BehaviorTree
{
    internal struct BTCachedTransferLink : IComponentData
    {
        public Entity SuccessLink;
        public Entity FailLink;
    }
}
