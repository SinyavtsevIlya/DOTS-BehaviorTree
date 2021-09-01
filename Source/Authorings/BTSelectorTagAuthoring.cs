using System;
using Unity.Entities;

namespace Nanory.Unity.Entities.BehaviorTree
{
    [UnityEngine.AddComponentMenu("Behavior Tree/Selector")]

    public class BTSelectorTagAuthoring : BTCompositeNodeAuthoring
    {
        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<BTSelectorTag>(entity);
        }
    }
}
