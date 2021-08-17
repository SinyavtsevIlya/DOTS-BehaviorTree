using Unity.Entities;

namespace Nanory.Unity.Entities.BehaviorTree
{
    public struct BTConditionalAbort : IComponentData 
    {
        public Type Value;

        [System.Flags]
        public enum Type
        {
            Self            = 1 << 1,
            LowerPriority   = 1 << 2 
        }
    }

}
