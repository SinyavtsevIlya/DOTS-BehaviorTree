using Unity.Entities;

namespace Nanory.Unity.Entities.BehaviorTree
{
    [UpdateAfter(typeof(BTInternalSystemGroup))]
    public class BTUserSystemGroup : ComponentSystemGroup
    {
    }
}

