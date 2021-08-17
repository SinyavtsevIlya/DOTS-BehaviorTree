using System;
using UnityEngine;
using Unity.Entities;

namespace Nanory.Unity.Entities.BehaviorTree
{
    public class BTNodeProcessAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] int _count;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new BTNodeProcess() { MaxCount = _count, Count = _count });
        }
    }
}
