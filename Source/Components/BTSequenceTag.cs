using Unity.Entities;

namespace Nanory.Unity.Entities.BehaviorTree
{
    [UnityEngine.AddComponentMenu("Behavior Tree/Sequence")]
    [GenerateAuthoringComponent]
    public struct BTSequenceTag : IComponentData, IComposite { }
}
