using System;
using UnityEngine;
using Unity.Entities;

namespace Nanory.Unity.Entities.BehaviorTree
{
    [UnityEngine.AddComponentMenu("Behavior Tree/Selector")]

    public class BTSelectorTagAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<BTSelectorTag>(entity);
        }
    }
}
