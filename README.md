# DOTS-BT
Unity DOTS Behavior-Tree implementation

![title](https://i.imgur.com/iPY6vuX.png)
 
# Features

- High performance. Zero GC allocations.
- Easy to use, less to code.
- 100% ECS-ish. No blackboards, no unrelated systems to think about.
- Supports all classic composite node types: Selector, Sequence, Repeater.  
- Conditional aborts support.
- Creating (and editing) a tree in Unity Editor.
- Compatible with burst and il2cpp.

# HowTo

## Requirements

`Min. Requirements:` Unity >= 2019.4.18 and entities package >= 0.11.2-preview.1

`Tested on:` Unity 2020.2.3 and entities package 0.17.0-preview.42

## Installation

You can install the repository using `UPM`:
Just add this line in Packages/manifest.json:

"com.nanory.unity.entities.bt": "https://https://github.com/SinyavtsevIlya/DOTS-BehaviorTree.git",

## Usage

### 1) Create a new behavior tree 
By right-clicking the `Hierarchy > Behavior Tree > Root`
### 2) Create any nested nodes you need 
(Selector/Sequence/Repeater) doing the same way as shown in first step.
![nodes-creation](https://i.imgur.com/huR6crY.png)
### 3) Create your own action or conditional node.
To make it you need to create a pair: a component and a system.

Action Node example:

```csharp
// 1) Create an action node component and (optional) add this attribute.
[GenerateAuthoringComponent]
public struct SeekEnemy : IComponentData { }

public sealed class BTSeekEnemySystem : SystemBase
{
   protected override void OnUpdate()
   {
       var beginSimECB = this.CreateBeginSimECB();
       
       Entities
           .WithAll<SeekEnemy>() // 2) Simply add it to your Query.
           .ForEach((Entity agentEntity, 
           // The parameters below are just up to you. 
           in EnemyLink enemyLink, 
           in LocalToWorld ltw,
           in StoppingDistance stoppingDistance, 
           in BTActionNodeLink bTNodeLink) => // 3) But don't forget to add this component in the end.
           {
               var position = GetComponent<LocalToWorld>(enemyLink.Value).Position;

               if (math.length((position - ltw.Position)) < stoppingDistance.Value)
               {
                   // 4) When you decided that the action was completed successfully, then you need to send the result.
                   beginSimECB.AddComponent(bTNodeLink.Value, BTResult.Success); 
               }
               else
                   beginSimECB.AddComponent(agentEntity, new MoveToDestinationRequest() { Position = position, Speed = 1f });
           })
           .ScheduleParallel();
   }
}
```

Conditional Node example is very similar except two things:

```csharp

// 1) NOTE: in this case we need to implement interface IConditional. (It's only for conversion/validation purposes, not for runtime)
[GenerateAuthoringComponent]
public struct BTIsEnemyReachable : IComponentData, IConditional{ }

public sealed class BTIsEnemyReachableSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var beginSimECB = this.CreateBeginSimECB();

        Entities
            .WithAll<BTIsEnemyReachable>()
            .ForEach(
            (DynamicBuffer<InteractableElement> interactableElements, 
            in EnemyLink enemyLink,
            in Name name,
            // 2) NOTE: we need to pass a Conditional Node reference.
            in BTConditionalNodeLink bTNodeLink) =>
            {
                for (int i = 0; i < interactableElements.Length; i++)
                {
                    if (interactableElements[i].value == enemyLink.Value)
                    {
                        beginSimECB.AddComponent(bTNodeLink.Value, BTResult.Success);
                        return;
                    }
                }
                // you also able to send "Fail" results.
                beginSimECB.AddComponent(bTNodeLink.Value, BTResult.Fail);
            })
            .WithoutBurst()
            .Run();
    }
}
```
### 4) Add your newly created nodes in the tree
// TODO
### 5) Connect tree to agent
// TODO

# Advanced Tips
## How to use conditional aborts
// TODO (basic usage, priorities)
## How to react on state changes
// TODO (enter/exit events)

# FAQ

## Is it posible to use nested trees? 
Yes. Since the tree is just a prefab it's easy to make using nested prefabs feature.


