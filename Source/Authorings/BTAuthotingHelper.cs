using System;
using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Entities;

namespace Nanory.Unity.Entities.BehaviorTree
{
    public static class BTAuthotingHelper
    {
        public static void AuthoriseTree(
            GameObject rootNodeGO,
            Entity agentEntity, 
            EntityManager manager, 
            GameObjectConversionSystem conversionSystem, float conditionalAbortInterval = .25f)
        {
            var authoringPairs = new Dictionary<GameObject, Entity>();
            var beginSimECB = manager.World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>().CreateCommandBuffer();
            var rootNodeEntity = manager.CreateEntity();

            // Necessary send BTResult event to startup the tree.
            beginSimECB.AddComponent(rootNodeEntity, BTResult.Success);

            manager.AddComponentData(rootNodeEntity, new BTDeapth());
            manager.AddComponentData(rootNodeEntity, new BTSiblingIndex());
            manager.AddComponentData(agentEntity, new BTCurrentNodeLink() { Value = rootNodeEntity });
            manager.AddComponentData(agentEntity, new BTActionNodeLink() { Value = Entity.Null });
            manager.AddComponentData(agentEntity, new BTConditionalNodeLink() { Value = Entity.Null });
            manager.AddComponentData(agentEntity, new BTConditionalAbortProcessing());
            manager.AddComponentData(agentEntity, new BTConditionalAbortTimer(conditionalAbortInterval));
            manager.AddBuffer<BTConditionalAbortNodeElement>(agentEntity);
            manager.AddBuffer<BTPathIndexElement>(rootNodeEntity).Add(new BTPathIndexElement() { Index = 0, Entity = rootNodeEntity });
            manager.AddBuffer<BTOverallChildElement>(rootNodeEntity);

            if (!manager.HasComponent<LinkedEntityGroup>(agentEntity))
            {
                beginSimECB.AddBuffer<LinkedEntityGroup>(agentEntity);
                // it's important to add entity itself to it's buffer root
                beginSimECB.AppendToBuffer(agentEntity, new LinkedEntityGroup() { Value = agentEntity });
            }

            // tree should be alse linked with an agent
            beginSimECB.AppendToBuffer(agentEntity, new LinkedEntityGroup() { Value = rootNodeEntity });

            var eventContainerEntity = manager.CreateEntity();
            // Init the buffer with a root
            manager.AddBuffer<LinkedEntityGroup>(eventContainerEntity);
            beginSimECB.AppendToBuffer(eventContainerEntity, new LinkedEntityGroup() { Value = eventContainerEntity });

            // Connect the container with an Agent
            beginSimECB.AppendToBuffer(agentEntity, new LinkedEntityGroup() { Value = eventContainerEntity });
            manager.AddComponentData(agentEntity, new BTEventContainerLink() { Value = eventContainerEntity });

            AuthorizeNodeRecursive(rootNodeGO, rootNodeEntity, agentEntity, manager, conversionSystem, authoringPairs);
            foreach (var pair in authoringPairs)
            {
                manager.GetBuffer<BTOverallChildElement>(rootNodeEntity).Add(new BTOverallChildElement() { Value = pair.Value });

                if (manager.HasComponent<BTCachedTransferLink>(pair.Value))
                {
                    var authiring = pair.Key.GetComponent<BTCachedTransferLinkAuthoting>();

                    var btCachedTransferLink = manager.GetComponentData<BTCachedTransferLink>(pair.Value);
                    btCachedTransferLink.FailLink = authoringPairs[authiring.FailLink];
                    btCachedTransferLink.SuccessLink = authoringPairs[authiring.SuccessLink];
                    manager.SetComponentData(pair.Value, btCachedTransferLink);
                }
            }
        }

        static void AuthorizeNodeRecursive(
            GameObject nodeGO, 
            Entity nodeEntity, 
            Entity agentEntity, 
            EntityManager manager, 
            GameObjectConversionSystem conversionSystem,
            Dictionary<GameObject, Entity> delayedAuthoringPairs)
        {
#if UNITY_EDITOR
                manager.SetName(nodeEntity, $"{nodeGO.name}");
#endif
            manager.AddComponentData(nodeEntity, new AgentLink() { Value = agentEntity });

            // each node should be linked with an Agent
            manager.GetBuffer<LinkedEntityGroup>(agentEntity).Add(new LinkedEntityGroup() { Value = nodeEntity });

            foreach (var convertable in nodeGO.GetComponents<IConvertGameObjectToEntity>())
            {
                convertable.Convert(nodeEntity, manager, conversionSystem);
            }

            delayedAuthoringPairs[nodeGO] = nodeEntity;

            if (nodeGO.transform.childCount > 0)
            {
                var bTChildren = new NativeArray<BTChildElement>(nodeGO.transform.childCount, Allocator.Temp);
                for (int i = 0; i < nodeGO.transform.childCount; i++)
                {
                    var child = nodeGO.transform.GetChild(i);

                    if (child.gameObject.TryGetComponent<IConvertGameObjectToEntity>(out var convertable))
                    {
                        var childEntity = manager.CreateEntity();
                        bTChildren[i] = new BTChildElement() { Value = childEntity };
                        manager.AddComponentData(childEntity, new BTParentLink() { Value = nodeEntity });
                        var deapth = manager.GetComponentData<BTDeapth>(nodeEntity);
                        deapth.Value++;
                        manager.AddComponentData(childEntity, deapth);
                        manager.AddComponentData(childEntity, new BTSiblingIndex() { Value = i });

                        var childPathBuffer = manager.AddBuffer<BTPathIndexElement>(childEntity);
                        var parentPathBuffer = manager.GetBuffer<BTPathIndexElement>(nodeEntity).ToNativeArray(Allocator.Temp);
                        childPathBuffer.CopyFrom(parentPathBuffer);
                        childPathBuffer.Add(new BTPathIndexElement() { Index = i, Entity = childEntity });

                        AuthorizeNodeRecursive(child.gameObject, childEntity, agentEntity, manager, conversionSystem, delayedAuthoringPairs);
                    }
                    else
                    {
                        Debug.LogException(new Exception("Your Behavior Tree prefab has a node game object with no Authorings, " +
                            "which is not allowed. (Click on this message to select it)"), child.gameObject);
                    }
                }
                manager.AddBuffer<BTChildElement>(nodeEntity).AddRange(bTChildren);

                manager.AddComponent<BTCompositeNode>(nodeEntity);
            }
            // expecting a leaf node here
            else
            {
                manager.AddComponent<BTLeafNodeTag>(nodeEntity);
                foreach (var monobehavior in nodeGO.GetComponents<MonoBehaviour>())
                {
                    if (monobehavior is BTConditionalAbortAuthoring btAbortAuthoting)
                        continue;

                    if (monobehavior is BTCachedTransferLinkAuthoting)
                        continue;

                    var authoringType = monobehavior.GetType();
                    var type = authoringType.Assembly.GetType(authoringType.FullName.Replace("Authoring", ""));
                    manager.AddComponentData(nodeEntity, new BTNodeType() { Value = new ComponentType(type) });
                    if (typeof(IConditional).IsAssignableFrom(type))
                    {
                        manager.AddComponent<BTConditionalNodeTag>(nodeEntity);
                    }
                    else
                    {
                        manager.AddComponent<BTActionNodeTag>(nodeEntity);
                    }
                }
                
            }
        }
    }
}
