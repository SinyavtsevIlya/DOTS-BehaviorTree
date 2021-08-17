using System;
using UnityEngine;
using Unity.Entities;

namespace Nanory.Unity.Entities.BehaviorTree
{
    public class BTConditionalAbortAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] BTConditionalAbort.Type _type;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new BTConditionalAbort() { Value  = _type });
        }
    }
}

