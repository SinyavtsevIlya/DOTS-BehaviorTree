using System;
using UnityEngine;
using Unity.Entities;

namespace Nanory.Unity.Entities.BehaviorTree
{
    public class BTNodeProcessAuthoring : BTCompositeNodeAuthoring
    {
        [SerializeField] int _count = -1;

        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new BTNodeProcess() { MaxCount = _count, Count = _count });
        }
    }
}
