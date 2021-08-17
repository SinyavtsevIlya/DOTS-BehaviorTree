using System;
using UnityEngine;
using Unity.Entities;

namespace Nanory.Unity.Entities.BehaviorTree
{
    public class BTCachedTransferLinkAuthoting : MonoBehaviour, IConvertGameObjectToEntity
    {
        public GameObject FailLink;
        public GameObject SuccessLink;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new BTCachedTransferLink()
            {
                SuccessLink = conversionSystem.GetPrimaryEntity(SuccessLink),
                FailLink = conversionSystem.GetPrimaryEntity(FailLink)
            });
        }
    }
}

