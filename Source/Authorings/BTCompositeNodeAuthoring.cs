using UnityEngine;
using Unity.Entities;

namespace Nanory.Unity.Entities.BehaviorTree
{
    public abstract class BTCompositeNodeAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [HideInInspector] public Texture2D DefaultThumbnail;

        public abstract void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem);
    }
}
