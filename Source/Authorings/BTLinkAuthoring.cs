using System;
using UnityEngine;
using Unity.Entities;

namespace Nanory.Unity.Entities.BehaviorTree
{
    public class BTLinkAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] GameObject _bt;
        [SerializeField] float _conditionalAbortInterval = .25f;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            BTAuthotingHelper.AuthoriseTree(_bt, entity, dstManager, conversionSystem, _conditionalAbortInterval);
        }
    }
}
