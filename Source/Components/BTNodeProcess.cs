using Unity.Entities;

namespace Nanory.Unity.Entities.BehaviorTree
{
    /// <summary>
    /// Node Indicates that all downstream nodes should be processed "Count" times. -1 is for infinity.
    /// </summary>
    public struct BTNodeProcess : IComponentData
    {
        /// <summary>
        /// Times to repeat. -1 stands for infinity
        /// </summary>
        public int MaxCount;
        /// <summary>
        /// Current processing value
        /// </summary>
        public int Count;

        public const int Infinity = -1;
    }
}
