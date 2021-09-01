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
### 3) Create your own action or conditional node.
To make it you need to create a pair: a component and a system.

Action Node example:

```csharp

// 1) Create an action node component
// and add this attribute (optional).
[GenerateAuthoringComponent]
public struct SeekEnemy : IComponentData { }

public sealed class BTSeekEnemySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var beginSimECB = this.CreateBeginSimECB();

            var visiblesBFE = GetBufferFromEntity<VisibleElement>();

            Entities
                // 2) Simply add it to your Query.
                .WithAll<SeekEnemy>() 
                .ForEach((Entity agentEntity, 
                // The parameters below are just up to you. 
                in EnemyLink enemyLink, 
                in LocalToWorld ltw,
                in StoppingDistance stoppingDistance, 
                // 3) But don't forget to add this component in the end. It's a reference to a current `Action Node`.
                in BTActionNodeLink bTNodeLink) => 
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


# FAQ

## Is it posible to use nested trees? 

Yes. Since the tree is just a prefab it's easy to make using nested prefabs feature.


