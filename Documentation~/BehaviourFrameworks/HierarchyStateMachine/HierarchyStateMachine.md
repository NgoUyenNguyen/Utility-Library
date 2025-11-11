# Hierarchy State Machine
Base system to build hierarchical (nested) state logic where each State can own child States and optional Activities that activate/deactivate during transitions.

[Usage](Usage.md)

---
## STATE

A State represents a specific behavior or condition within a hierarchical state machine. Each state can:

- Contain child states to create nested state hierarchies
- Own activities that activate/deactivate during transitions
- Define custom enter/exit logic
- Specify transition conditions to other states
- Handle updates while active

States form parent-child relationships where only one branch of the hierarchy is active at a time, from root to leaf.

### Constructor
```csharp
protected State(State parent)
```
| Parameter  |Description|
|------------|---|
| **parent** | Parent state in the hierarchy (null for root)|

### Properties
|Property|Description|
|---|---|
|**[Machine](State/Machine.md)**| Reference to the owning hierarchical state machine|
|**[Parent](State/Parent.md)**| Parent state in the hierarchy (null for root)|
|**[Activities](State/Activities.md)**| Current active direct child of this state (if any)|
|**[Leaf](State/Leaf.md)**| Deepest active state under this state|
|**[Activities](State/Activities.md)**| Read-only list of Activities attached to this state|

### Public Methods
|Method|Description|
|---|---|
|**[AddActivity](State/AddActivity.md)**|Attach one or more Activities to this state|

### Protected Methods
|Method|Description|
|---|---|
|**[GetInitialState](State/GetInitialState.md)**|Define which child state is auto-entered on Enter|
|**[GetTransition](GetTransition.md)**|Return target state when this state wants to transition|
|**[OnEnter](State/OnEnter.md)**|Called once when this state is entered|
|**[OnExit](State/OnExit.md)**|Called once when this state is exited|
|**[OnUpdate](State/OnUpdate.md)**|Per-frame update while this state is active|

### Static Utility
|Method|Description|
|---|---|
|**[StatePath](State/StatePath.md)**|String path from root to a state (for debugging)|
|**[LowestCommonAncestor](State/LowestCommonAncestor.md)**|Find lowest common ancestor between two states|

---

## STATE MACHINE

StateMachine is the core class that manages hierarchical state behavior and transitions. It maintains a root state and
handles the execution of state updates, transitions, and related activities.

The state machine coordinates transitions between states by managing activity lifecycles, maintains the active state
hierarchy branch, and ensures proper enter/exit behavior. It provides a controlled way to start the machine and advance
time through tick updates.

### Constructor
```csharp
public StateMachine(State root, bool useSequencer = true)
```
|Parameter| Description                                                        |
|---|--------------------------------------------------------------------|
|**root**| Root state of the hierarchy tree                                   |
|**useSequencer**| Activities are executed sequential while activate/deactivate phase |

### Properties
|Property|Description|
|---|---|
|**[Root](StateMachine/Root.md)**| Root state of the hierarchy tree|
|**[CancellationTokenSource](StateMachine/CancellationTokenSource.md)**| Token source used to cancel ongoing transition activities|

### Public Methods
|Method|Description|
|---|---|
|**[Start](StateMachine/Start.md)**|Start the machine|
|**[Tick](StateMachine/Tick.md)**|Advance updates and transitions by deltaTime|

---
## ACTIVITY

Activities are add-on components that can be attached to States.
They are asynchronous and can be activated/deactivated (sequentially or in parallel depending on configuration) during state transitions.
Activities can be used to perform asynchronous tasks such as loading assets, playing animations, or coordinating external systems.

When a transition occurs, Activities on the current active branch are deactivated first (from the current state up to the Lowest Common Ancestor child).
After the state branch switches, Activities on the new branch are activated (from the Lowest Common Ancestor child down to the target state).

### Properties
|Property|Description|
|---|---|
|**[Mode](Activity/Mode.md)**| Current lifecycle mode of the Activity (Inactive, Activating, Active, Deactivating)|

### Public Methods
|Method|Description|
|---|---|
|**[ActivateAsync](Activity/ActivateAsync.md)**|Asynchronously activate the activity|
|**[DeactivateAsync](Activity/DeactivateAsync.md)**|Asynchronously deactivate the activity|
