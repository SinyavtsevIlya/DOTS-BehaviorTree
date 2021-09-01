using Unity.Entities;

namespace Nanory.Unity.Entities.BehaviorTree
{
    public class BTSequenceAuthoring : BTCompositeNodeAuthoring
    {
        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<BTSequenceTag>(entity);
        }
    }
}
