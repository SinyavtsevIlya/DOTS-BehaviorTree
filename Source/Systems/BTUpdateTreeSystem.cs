//#define BT_DEBUG
//#define BT_MULTITHREAD

using Unity.Entities;
using Unity.Collections;
using Unity.Collections.LowLevel;
using Debug = UnityEngine.Debug;

namespace Nanory.Unity.Entities.BehaviorTree
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public sealed class BTUpdateTreeSystem : SystemBase
    {
        private EntityCommandBufferSystem _beginSimECBSystem;

        protected override void OnCreate()
        {
            _beginSimECBSystem = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var beginSimECB = _beginSimECBSystem.CreateCommandBuffer();

            var childrenNodesBFE = GetBufferFromEntity<BTChildElement>();
            var overallChildrenBFE = GetBufferFromEntity<BTOverallChildElement>();
            var abortNodesBFE = GetBufferFromEntity<BTConditionalAbortNodeElement>();
            var pathIndexesBFE = GetBufferFromEntity<BTPathIndexElement>();
            var linkedEntitiesBFE = GetBufferFromEntity<LinkedEntityGroup>();
            var timeData = World.Time;

            Entities
                .WithAll<BTCurrentNodeLink>()
                .ForEach((Entity agentEntity, ref BTEventContainerLink eventContainerLink) =>
                {
                    // Here we clear the event container from references to event entities that were destroyed 
                    // It's necessary to ignore the first element
                    var buffer = linkedEntitiesBFE[eventContainerLink.Value];
                    if (buffer.Length <= 1)
                        return;
                    buffer.RemoveRange(1, buffer.Length - 1);
                })
#if BT_MULTITHREAD
                .ScheduleParallel();
#else
                .WithoutBurst()
                .Run();
#endif


            // "BTResult" firstly adds automatically while agent conversion 
            Entities
                .ForEach((Entity nodeEntity, ref BTNodeProcess nodeProcess, in BTResult result, in AgentLink agentLink) =>
                {
                    var agentEntity = agentLink.Value;

                    Entity nextNodeEntity = DrillDownToLeftLeaf(nodeEntity);


                    if (nodeProcess.Count != BTNodeProcess.Infinity)
                    {
                        if (nodeProcess.Count == 0)
                        {
                            nodeProcess.Count = nodeProcess.MaxCount;

                            if (!HasComponent<BTParentLink>(nodeEntity))
                            {
                                // Tree processing is finished...
                            }
                            else
                                nextNodeEntity = GetComponent<BTParentLink>(nodeEntity).Value;
                        }
                        else
                        {
                            nodeProcess.Count--;
                        }
                    }

                    MoveToNode(result, beginSimECB, agentEntity, nextNodeEntity);

                })
#if BT_MULTITHREAD
                .ScheduleParallel();
#else
                .WithoutBurst()
                .Run();
#endif

            Entities
                .WithAll<BTLeafNodeTag>()
                .ForEach((Entity nodeEntity, in BTResult result, in AgentLink agentLink, in BTParentLink parentLink) =>
                {
#if UNITY_EDITOR && BT_DEBUG
                    Debug.Log($"Handle leaf node: {EntityManager.GetName(nodeEntity)} result: {result.Value}");
#endif

                    var agentEntity = agentLink.Value;

                    beginSimECB.RemoveComponent(agentEntity, GetComponent<BTNodeType>(nodeEntity).Value);

                    var currentNode = GetComponent<BTCurrentNodeLink>(agentEntity).Value;

                    // We don't want it interrupt main 
                    if (!HasComponent<BTLeafNodeTag>(currentNode)) return;

                    Entity nextNodeEntity = parentLink.Value;

                    // BTResult came from the background conditional abort check node...
                    if (nodeEntity != currentNode)
                    {
                        if (currentNode != Entity.Null)
                        {
                            if (HasComponent<BTCachedTransferLink>(nodeEntity))
                            {
                                var cachedTransferLink = GetComponent<BTCachedTransferLink>(nodeEntity);
                                nextNodeEntity = result.Value == BTResult.Type.Success ? cachedTransferLink.SuccessLink : cachedTransferLink.FailLink;

                                var currentNodePath = pathIndexesBFE[currentNode];

                                for (int i = 0; i < currentNodePath.Length; i++)
                                    if (HasComponent<BTParentLink>(currentNodePath[i].Entity))
                                        if (GetComponent<BTParentLink>(currentNodePath[i].Entity).Value ==
                                            GetComponent<BTParentLink>(nextNodeEntity).Value)
                                            return;
                            }

                            // Remove the user Component from the agent of action type node we are leaving from
                            beginSimECB.RemoveComponent(agentEntity, GetComponent<BTNodeType>(currentNode).Value); 
                        }
                    }

                    MoveToNode(result, beginSimECB, agentEntity, nextNodeEntity);
                })
#if BT_MULTITHREAD
                .ScheduleParallel();
#else
                .WithoutBurst()
                .Run();
#endif

            Entities
                .WithAny<BTSelectorTag, BTSequenceTag>()
                .ForEach((Entity nodeEntity, ref BTCompositeNode compositeNode, in BTResult result, in AgentLink agentLink, in BTParentLink parentLink) =>
                {
                    var agentEntity = agentLink.Value;
                    var isSequence = HasComponent<BTSequenceTag>(nodeEntity);

#if UNITY_EDITOR && BT_DEBUG
                    Debug.Log($"Handle node: {EntityManager.GetName(nodeEntity)} result: {result.Value}");
#endif

                    Entity nextNodeEntity = Entity.Null;
                    if (result.Value == BTResult.Type.Success != isSequence)
                    {
                        nextNodeEntity = parentLink.Value;
                        // It's necessary to reset composite node state, when go back to the parent
                        compositeNode.CurrentChildIndex = 0;
                    }
                    else
                    {
                        var childrenNodes = childrenNodesBFE[nodeEntity];

                        var isLastNode = compositeNode.CurrentChildIndex == childrenNodes.Length - 1;

                        compositeNode.CurrentChildIndex = isLastNode ? 0 : compositeNode.CurrentChildIndex + 1;
                        var currentChild = childrenNodes[compositeNode.CurrentChildIndex].Value;
                        nextNodeEntity = isLastNode ? parentLink.Value : DrillDownToLeftLeaf(currentChild);
                    }

                    MoveToNode(result, beginSimECB, agentEntity, nextNodeEntity);
                })
#if BT_MULTITHREAD
                .ScheduleParallel();
#else
                .WithoutBurst()
                .Run();
#endif

            Entities
                .WithAll<BTResult>()
                .ForEach((Entity btResultEntity) =>
                {
                    beginSimECB.RemoveComponent<BTResult>(btResultEntity);
                })
                .WithoutBurst()
                .Run();

            Entities
                .ForEach((Entity agentEntity, ref BTConditionalAbortTimer timer, ref BTConditionalAbortProcessing abortProcessing) =>
                {
                    if (timeData.DeltaTime >= timer.Interval)
                    {
                        Debug.LogWarning("Oops. delta time is larger than timer interval");
                        return;
                    }
                    
                    timer.CurrentTime -= timeData.DeltaTime;
                    if (timer.CurrentTime <= 0f)
                    {
                        beginSimECB.AddComponent<BTRefreshConditionalNodesRequest>(agentEntity);

                        timer.CurrentTime = timer.Interval;

                        var conditionalAbortNodes = abortNodesBFE[agentEntity];

                        if (conditionalAbortNodes.Length == 0) 
                            return;

                        var hasPreviousAbortNode = abortProcessing.CurrentAbortNode != Entity.Null;

                        if (hasPreviousAbortNode)
                            beginSimECB.RemoveComponent(agentEntity, GetComponent<BTNodeType>(abortProcessing.CurrentAbortNode).Value);

                        abortProcessing.CurrentAbortNode = conditionalAbortNodes[abortProcessing.Index].Value;

                        if (HasComponent<BTNodeType>( abortProcessing.CurrentAbortNode))
                            beginSimECB.AddComponent(agentEntity, GetComponent<BTNodeType>( abortProcessing.CurrentAbortNode).Value);

                        beginSimECB.SetComponent(agentEntity, new BTConditionalNodeLink() { Value =  abortProcessing.CurrentAbortNode });

                        abortProcessing.Index = (abortProcessing.Index + 1) % conditionalAbortNodes.Length;
                    }
                })
#if BT_MULTITHREAD
                .ScheduleParallel();
#else
                .WithoutBurst()
                .Run();
#endif

            Entities
                .WithAll<BTRefreshConditionalNodesRequest>()
                .ForEach((Entity agentEntity, ref DynamicBuffer<BTConditionalAbortNodeElement> abortNodes, ref BTConditionalAbortProcessing abortProcessing, in BTCurrentNodeLink nodeLink) =>
                {
                    var actualAbortNodes = new NativeList<BTConditionalAbortNodeElement>(initialCapacity: 32, Allocator.Temp);

                    var path = pathIndexesBFE[nodeLink.Value];

                    // We don't need to check the root node,
                    // Therefore start from 1
                    for (int i = 1; i < path.Length; i++)
                    {
                        var node = path[i];

                        var isLeafNode = i == path.Length - 1;
                        
                        var siblingNodes = childrenNodesBFE[GetComponent<BTParentLink>(node.Entity).Value];
                        for (int j = 0; j < siblingNodes.Length; j++)
                        {
                            if (j > node.Index) break;

                            var drilledDownEntity = DrillDownToLeftLeaf(node.Entity);

                            if (HasComponent<BTConditionalAbort>(drilledDownEntity))
                            {
                                if (GetComponent<BTConditionalAbort>(drilledDownEntity).Value.HasFlag(BTConditionalAbort.Type.LowerPriority))
                                {
                                    if (actualAbortNodes.Length > 0)
                                        if (drilledDownEntity == actualAbortNodes[actualAbortNodes.Length - 1].Value)
                                            continue;
                                    
                                    actualAbortNodes.Add(new BTConditionalAbortNodeElement() { Value = drilledDownEntity });
                                }
                            }
                        }
                    }

                    // Reset the index to prevent breaking the buffer bounds, in case when abort nodes count changed 
                    if (abortNodes.Length != actualAbortNodes.Length)
                    {
                        abortProcessing.Index = 0;
                    }

                    abortNodes.CopyFrom(actualAbortNodes);
                })
#if BT_MULTITHREAD
                .ScheduleParallel();
#else
                .WithoutBurst()
                .Run();
#endif

            Entities
                .WithAll<BTRefreshConditionalNodesRequest>()
                .ForEach((Entity btAgentEntity) =>
                {
                    beginSimECB.RemoveComponent<BTRefreshConditionalNodesRequest>(btAgentEntity);
                })
#if BT_MULTITHREAD
                .ScheduleParallel();
#else
                .WithoutBurst()
                .Run();
#endif

            Entities
                .WithAny<BTStateEnterEvent, BTStateExitEvent>()
                .ForEach((Entity btEventEntity) =>
                {
                    beginSimECB.DestroyEntity(btEventEntity);
                })
            #if BT_MULTITHREAD
                .ScheduleParallel();
            #else
                .WithoutBurst()
                .Run();
            #endif

#if UNITY_EDITOR && BT_DEBUG
            // TODO: BTConditionalNodeLink is always on Entity, however it looks like a background conditional check.
            // Solution - check NodeType and display only current node, and background at the button
            Entities
                .ForEach(
                (Entity agentEntity, 
                UnityEngine.Transform transform, 
                ref BTActionNodeLink bTActionNodeLink, 
                ref BTConditionalNodeLink bTConditionalNodeLink) =>
                {
                    var label = $"action: {EntityManager.GetName(bTActionNodeLink.Value)} {System.Environment.NewLine}  cond: {EntityManager.GetName(bTConditionalNodeLink.Value)}";
                    //HandlesHelpers.Label(transform.position, label);
                })
                .WithoutBurst()
                .Run();
#endif
        }

        private void MoveToNode(BTResult result, EntityCommandBuffer beginSimECB, Entity agentEntity, Entity nextNodeEntity)
        {
            if (HasComponent<BTLeafNodeTag>(nextNodeEntity))
            {
                beginSimECB.AddComponent<BTRefreshConditionalNodesRequest>(agentEntity);

                var eventContainerEntity = GetComponent<BTEventContainerLink>(agentEntity).Value;

#if UNITY_EDITOR && BT_DEBUG
                Debug.Log($"Move to {EntityManager.GetName(nextNodeEntity)} result: {result.Value}"); 
#endif

                var enterEventEntity = beginSimECB.CreateEntity();
                beginSimECB.AddComponent(enterEventEntity, new BTStateEnterEvent() { AgentLink = agentEntity });
                beginSimECB.AddComponent(enterEventEntity, GetComponent<BTNodeType>(nextNodeEntity).Value);

                // We want to automatically destroy an eventEntity when the Agent is destroyed...
                beginSimECB.AppendToBuffer(eventContainerEntity, new LinkedEntityGroup() { Value = enterEventEntity });

                if (HasComponent<BTNodeType>(agentEntity))
                {
                    var exitEventEntity = beginSimECB.CreateEntity();
                    beginSimECB.AddComponent(exitEventEntity, new BTStateEnterEvent() { AgentLink = agentEntity });
                    beginSimECB.AddComponent(exitEventEntity, GetComponent<BTNodeType>(agentEntity).Value);

                    // ...and auto-destroy this one too.
                    beginSimECB.AppendToBuffer(eventContainerEntity, new LinkedEntityGroup() { Value = enterEventEntity });
                }

                if (HasComponent<BTActionNodeTag>(nextNodeEntity))
                {
                    beginSimECB.SetComponent(agentEntity, new BTActionNodeLink() { Value = nextNodeEntity });
                }
                else if (HasComponent<BTConditionalNodeTag>(nextNodeEntity))
                {
                    beginSimECB.SetComponent(agentEntity, new BTConditionalNodeLink() { Value = nextNodeEntity });
                }
            }
            else
            {
                // ...and transfer the result only to composite or decorator nodes
                beginSimECB.AddComponent(nextNodeEntity, result);
            }

            if (HasComponent<BTNodeType>(nextNodeEntity))
                beginSimECB.AddComponent(agentEntity, GetComponent<BTNodeType>(nextNodeEntity).Value);

            beginSimECB.SetComponent(agentEntity, new BTCurrentNodeLink() { Value = nextNodeEntity });
        }

        private Entity DrillDownToLeftLeaf(Entity origin)
        {
            var bfe = GetBufferFromEntity<BTChildElement>(true);
            return bfe.Exists(origin) ? DrillDownToLeftLeaf(bfe[origin][0].Value) : origin;
        }
    }
}