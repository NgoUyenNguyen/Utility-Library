# Usage Guide

This guide shows how to use the Hierarchy State Machine (HSM) framework to build nested (hierarchical) gameplay logic.

If you are new to the concepts, read the overview in [HierarchyStateMachine.md](HierarchyStateMachine.md) first.

---

## Concept

The Hierarchy State Machine (HSM) framework is designed around the following key concepts:

1. **Hierarchical Structure**
    - States are organized in a tree structure where each state can have a parent and multiple children
    - Only one branch of states from root to leaf is active at any time
    - States handle their own logic but can coordinate with parent/child states

2. **State Management**
    - Each state defines its behavior through overridable methods (OnEnter, OnExit, OnUpdate)
    - States control transitions by returning target states in GetTransition()
    - Parent states can specify default children via GetInitialState().
      And whenever entering the parent, the default child is automatically entered.

3. **Activity System**
    - States can own async activities that activate/deactivate during transitions
    - Activities run sequentially or in parallel based on configuration
    - Cancellation support for interrupting ongoing activities

4. **Clean Architecture**
    - States are decoupled and communicate through a shared context object
    - No direct dependencies on Unity - framework is engine-agnostic
    - Clear separation between state logic and activity handling

5. **Transition Flow**
    - Framework computes lowest common ancestor between states
    - Handles exit/enter chain including activity lifecycle
    - Maintains consistent state tree during transitions

This architecture provides a flexible foundation for complex nested behaviors while keeping individual state
implementations focused and maintainable.


---

## Quick start

1) Create a context object (plain C# container) to share runtime data between states.
2) Implement your states by deriving from `NgoUyenNguyen.Behaviour.HSM.State`.
3) Compose states into a tree (root -> children -> grandchildren…). Each child holds a reference to its parent via the
   constructor.
4) Create a `StateMachine` with the root state and call `Start()` once.
5) Call `Tick(deltaTime)` every frame (e.g., from a MonoBehaviour Update).

---

## Example setup

### 1. Context shared by states

```csharp
[Serializable]
public class Context
{
    public Transform playerTransform;
    public Vector3 moveDirection;
    public Renderer renderer; // optional, e.g., for color changes via Activities
}
```

### 2. Root and child states

Below is a minimal player hierarchy with `PlayerRoot` containing `Grounded` and `Airborne`. Under `Grounded`, we have
`Idle` and `Move`.

```csharp
using NgoUyenNguyen.Behaviour.HSM;
using UnityEngine;

public class PlayerRoot : State
{
    private readonly Context context;
    public readonly Grounded Grounded;
    public readonly Airborne Airborne;

    public PlayerRoot(Context context) : base(null) // root has no parent
    {
        this.context = context;
        Grounded = new Grounded(context, this);
        Airborne = new Airborne(context, this);
    }

    // Choose which top child is active based on context.
    // Notice here that we don't use GetInitialState() to auto-enter child,
    // so the child state will be entered in the next frame instead. 
    protected override State GetTransition()
    {
        if (context.moveDirection.y != 0 && ActiveChild != Airborne) return Airborne;
        if (context.moveDirection.y == 0 && ActiveChild != Grounded) return Grounded;
        return null;
    }
}

public class Grounded : State
{
    private readonly Context context;
    public readonly Move Move;
    public readonly Idle Idle;

    public Grounded(Context context, PlayerRoot root) : base(root)
    {
        this.context = context;
        Move = new Move(context, this);
        Idle = new Idle(context, this);
    }

    protected override void OnEnter()
    {
        Debug.Log(State.StatePath(this)); // e.g., PlayerRoot > Grounded
    }

    // You can optionally set an initial child to auto-enter when Grounded is entered
    // protected override State GetInitialState() => Idle;

    protected override State GetTransition()
    {
        // Horizontal move makes us choose Move; otherwise Idle.
        if ((context.moveDirection.x != 0 || context.moveDirection.z != 0) && ActiveChild != Move) return Move;
        if (context.moveDirection is { x: 0, z: 0 } && ActiveChild != Idle) return Idle;

        // If jump/fall, go to Airborne
        return context.moveDirection.y != 0 ? (Parent as PlayerRoot)?.Airborne : null;
    }
}

public class Move : State
{
    private readonly Context context;

    public Move(Context context, Grounded parent) : base(parent)
    {
        this.context = context;
    }

    protected override void OnEnter()
    {
        Debug.Log(State.StatePath(this)); // PlayerRoot > Grounded > Move
    }

    protected override void OnUpdate(float dt)
    {
        context.playerTransform.position += context.moveDirection * dt;
    }

    protected override State GetTransition()
    {
        return context.moveDirection == Vector3.zero ? (Parent as Grounded)?.Idle : null;
    }
}

public class Idle : State
{
    private readonly Context context;

    public Idle(Context context, Grounded parent) : base(parent)
    {
        this.context = context;
    }

    protected override void OnEnter()
    {
        Debug.Log(State.StatePath(this)); // PlayerRoot > Grounded > Idle
    }

    protected override State GetTransition()
    {
        return context.moveDirection != Vector3.zero ? (Parent as Grounded)?.Move : null;
    }
}

public class Airborne : State
{
    private readonly Context context;

    public Airborne(Context context, PlayerRoot root) : base(root)
    {
        this.context = context;
    }

    protected override void OnEnter()
    {
        Debug.Log(State.StatePath(this)); // PlayerRoot > Airborne
    }

    protected override State GetTransition()
    {
        return context.moveDirection.y == 0 ? (Parent as PlayerRoot)?.Grounded : null;
    }
}
```

### 3. Driving the machine from MonoBehaviour

```csharp
using NgoUyenNguyen.Behaviour.HSM;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Context context = new();
    private StateMachine stateMachine;

    private void Awake()
    {
        stateMachine = new StateMachine(new PlayerRoot(context));
        context.playerTransform = transform;
        context.renderer = GetComponent<Renderer>();
        stateMachine.Start(); // optional if you call Tick first; Tick auto-starts
    }

    private void Update()
    {
        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.A)) move.x = -1;
        if (Input.GetKey(KeyCode.D)) move.x = 1;
        if (Input.GetKey(KeyCode.W)) move.z = 1;
        if (Input.GetKey(KeyCode.S)) move.z = -1;
        if (Input.GetKey(KeyCode.Space)) move.y = 1;     // airborne
        if (Input.GetKey(KeyCode.LeftShift)) move.y = -1; // descend

        // Cancel ongoing transition activities (if any)
        if (Input.GetKey(KeyCode.LeftControl)) stateMachine.CancellationTokenSource.Cancel();

        context.moveDirection = move.normalized;
        stateMachine.Tick(Time.deltaTime);
    }
}
```

---

## Transitions and updates

- Put transition decisions in `protected override State GetTransition()`.
    - Return the target state to request a transition; return `null` to keep current branch.
    - When transitioning, the framework computes the Lowest Common Ancestor (LCA) between current and target, exits the
      old branch up to LCA, switches, then enters down to the target.
- Per-frame logic for the active leaf (and its ancestors) runs in `protected override void OnUpdate(float deltaTime)`.
- To automatically dive into a default child upon entering a state, override
  `protected override State GetInitialState()` and return that child.

See:

- [State/GetTransition.md](State/GetTransition.md)
- [State/GetInitialState.md](State/GetInitialState.md)
- [State/OnUpdate.md](State/OnUpdate.md)

---

## Activities (optional)

Activities are asynchronous tasks attached to states. They activate/deactivate during transitions.

- Derive from the base [Activity](Activity.md).
- Attach to a state with `AddActivity(myActivity)`.
- During a transition, activities on the exiting chain are deactivated first, then after the state switch, activities on
  the entering chain are activated.
- You control whether these phases run sequentially or in parallel by the `useSequencer` flag in the `StateMachine`
  constructor:
    - `new StateMachine(root, useSequencer: true)` runs activity steps one-by-one (sequential).
    - `new StateMachine(root, useSequencer: false)` runs activity steps in parallel.
- Cancellation: `stateMachine.CancellationTokenSource.Cancel()` cancels the current phase's activity tasks.

Come back to the example setup to see how to use activities. Assume that we want to change the color of the GameObject
to red when entering 'Airborne' and back to white when exiting.

```csharp
public class ChangeColorActivity : Activity
{
    private readonly Context context;
    
    public ChangeColorActivity(Context context)
    {
        this.context = context;
    }
    
    public override async Task ActivateAsync(CancellationToken ct)
    {
        if (Mode != ActivityMode.Inactive) return;

        Mode = ActivityMode.Activating; // Set to Activating before awaiting
        try
        {
            await Task.Delay(1000, ct); // simulate async activity
            context.renderer.material.color = Color.red;
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Activity canceled"); //
        }
        finally
        {
            Mode = ActivityMode.Active; // Always set to Active when done
            Debug.Log("Activation done");
        }
    }
    
    public override async Task DeactivateAsync(CancellationToken ct)
    {
        if (Mode != ActivityMode.Active) return;
        
        Mode = ActivityMode.Deactivating; // Set to Deactivating before awaiting
        try
        {
            await Task.Delay(1000, ct); // simulate async activity
            context.renderer.material.color = Color.white;
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Activity canceled"); 
        }
        finally
        {
            Mode = ActivityMode.Inactive; // Always set to Inactive when done
            Debug.Log("Deactivation done");
        }
    }
}
```
Then adding this activity to the `Airborne` state:
```csharp
public class Airborne : State
{
    private readonly Context context;

    public Airborne(Context context, PlayerRoot root) : base(root)
    {
        this.context = context;
        AddActivity(new ChangeColorActivity(context)); // add activity to state
    }

    protected override void OnEnter()
    {
        Debug.Log(State.StatePath(this));
    }

    protected override State GetTransition()
    {
        return context.moveDirection.y == 0 ? (Parent as PlayerRoot)?.Grounded : null;
    }
}
```

See:

- [ActivityMode.md](ActivityMode.md)
- [IActivity.md](IActivity.md)
- [Activity.md](Activity.md)
- [State/Activities.md](State/Activities.md)
- [State/AddActivity.md](State/AddActivity.md)
- [StateMachine/CancellationTokenSource.md](StateMachine/CancellationTokenSource.md)

---

## Debugging helpers

- `State.StatePath(this)` returns a readable "Root > Child > Leaf" string for any state.
- `ActiveChild` and `Leaf` let you inspect which part of the hierarchy is currently active.

See:

- [State/StatePath.md](State/StatePath.md)
- [State/Leaf.md](State/Leaf.md)
- [State/Parent.md](State/Parent.md)
- [State/Machine.md](State/Machine.md)

---

## Tips and patterns

- Make state classes small and focused. Keep only the logic specific to that state.
- Use a context object to avoid tight coupling with engine APIs across many states.
- Prefer returning the same instance fields for transitions (e.g., `return (Parent as Grounded)?.Idle;`) instead of
  creating new state objects.
- If you need default child selection, use `GetInitialState()` instead of manually calling `Enter()`.
- To perform async operations (load, blend, tween…) on transitions without blocking gameplay, use Activities.

---

## API references used here

- `State`
    - [Constructor](HierarchyStateMachine.md#state)
    - [GetTransition](State/GetTransition.md)
    - [GetInitialState](State/GetInitialState.md)
    - [OnEnter](State/OnEnter.md), [OnExit](State/OnExit.md), [OnUpdate](State/OnUpdate.md)
    - [ActiveChild](State/Parent.md), [Leaf](State/Leaf.md)
    - [StatePath](State/StatePath.md), [LowestCommonAncestor](State/LowestCommonAncestor.md)
- `StateMachine`
    - [Constructor](HierarchyStateMachine.md#state-machine)
    - [Start](StateMachine/Start.md), [Tick](StateMachine/Tick.md)
    - [CancellationTokenSource](StateMachine/CancellationTokenSource.md)
- `Activities`
    - [ActivityMode](ActivityMode.md), [IActivity](IActivity.md), [Activity](Activity.md)
