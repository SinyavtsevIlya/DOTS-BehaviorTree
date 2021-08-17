using Unity.Entities;

namespace Nanory.Unity.Entities.BehaviorTree
{
    /// <summary>
    /// Link from the actor to the <b>active</b> action node in a behavior tree.
    /// </summary>
    public struct BTActionNodeLink : IComponentData
    {
        public Entity Value;
    }

    internal struct BTActionNodeTag : IComponentData { }
    internal struct BTConditionalNodeTag : IComponentData { }

    /// <summary>
    /// Link from the actor to the <b>active</b> conditional node in a behavior tree.
    /// </summary>
    public struct BTConditionalNodeLink : IComponentData
    {
        public Entity Value;
    }

    /// <summary>
    /// Internal pointer to the current processing node in a behavior tree.
    /// </summary>
    internal struct BTCurrentNodeLink : IComponentData
    {
        public Entity Value;
    }

    public struct BTResult : IComponentData 
    {
        public enum Type
        {
            Success,
            Fail
        }

        public Type Value;

        public BTResult(Type value)
        {
            Value = value;
        }

        public static readonly BTResult Success = new BTResult(Type.Success);
        public static readonly BTResult Fail = new BTResult(Type.Fail);
    }

    /// <summary>
    /// Could be User Action/Condition or Composite node type
    /// </summary>
    internal struct BTNodeType : IComponentData
    {
        public ComponentType Value;
    }

    internal struct AgentLink : IComponentData
    {
        public Entity Value;
    }

    internal struct BTCompositeNode : IComponentData
    {
        public int CurrentChildIndex;
    }

    public struct BTLeafNodeTag : IComponentData { }

    internal struct BTChildElement : IBufferElementData 
    {
        public Entity Value;
    }

    internal struct BTParentLink : IComponentData 
    {
        public Entity Value;
    }

    internal struct BTConditionalAbortTimer : IComponentData 
    {
        public float Interval;
        public float CurrentTime;

        public BTConditionalAbortTimer(float interval)
        {
            Interval = interval;
            CurrentTime = interval;
        }
    }

    internal struct BTConditionalAbortProcessing: IComponentData
    {
        public int Index;
        public Entity CurrentAbortNode;
    }

    internal struct BTConditionalAbortNodeElement : IBufferElementData
    {
        public Entity Value;
    }

    /// <summary>
    /// Represent a number of Leaf nodes relative to this node as a root
    /// </summary>
    internal struct BTLeafNodesCount : IComponentData
    {
        public int Value;
    }

    internal struct BTRefreshConditionalNodesRequest : IComponentData { }

    public struct BTStateEnterEvent : IComponentData
    {
        public Entity AgentLink;
    }

    public struct BTStateExitEvent : IComponentData 
    {
        public Entity AgentLink;
    }

    internal struct BTSiblingIndex : IComponentData 
    {
        public int Value;
    }

    internal struct BTDeapth : IComponentData
    {
        public int Value;
    }

    internal struct BTPathIndexElement : IBufferElementData
    {
        public int Index;
        public Entity Entity;
    }

    /// <summary>
    /// Complete BT-Tree nodes list buffer element. Buffer only exists on the tree root
    /// </summary>
    internal struct BTOverallChildElement : IBufferElementData
    {
        public Entity Value;
    }

    /// <summary>
    /// This is an entity reference that is the container for all EnterState / ExitState events.
    /// They must be removed together with the agent, because otherwise the reference to the agent 
    /// inside the event will be invalid. The container itself is also a child of the LinkedEntityGroup.
    /// </summary>
    internal struct BTEventContainerLink : IComponentData
    {
        public Entity Value;
    }
}
