# Stack-based State Machine
A compact state machine that uses a LIFO stack to manage nested, temporary, or modal states. It is ideal for workflows like gameplay modes, overlays, and interruption flows where states are pushed on top of the current one and later popped to resume the previous state.

The machine maintains a stack of State objects. Only the top state is active and receives updates. Transitions are performed by pushing a new state or popping the current one, which reveals the previous state.

---
## STATE

A State represents a single behavior that can decide to transition by pushing another state or popping itself. Each state can:

- Define custom enter/exit logic
- Decide transitions in GetTransition by calling Machine.PushState(...) or Machine.PopState()
- Handle updates while active

The top-most state on the stack is considered active and is the only one updated.

### Properties
|Property|Description|
|---|---|
|**[Machine](State/Machine.md)**| Reference to the owning stack-based state machine|

### Protected Methods
|Method|Description|
|---|---|
|**[GetTransition](State/GetTransition.md)**|Called every frame before OnUpdate. Use this to call PushState/PopState to change the stack|
|**[OnEnter](State/OnEnter.md)**|Called when this state becomes the active top of the stack (see AlwaysEnterExit below)|
|**[OnExit](State/OnExit.md)**|Called when this state stops being the active top of the stack (see AlwaysEnterExit below)|
|**[OnUpdate](State/OnUpdate.md)**|Per-frame update while this state is currently at the top of the stack|

### Update and Transition Order
- On each Tick, the machine calls CurrentState.Update().
- Update() first calls GetTransition(). Your GetTransition should perform stack operations (PushState/PopState) if conditions are met.
- After GetTransition, if this state is still the current top, OnUpdate() is called.

Note: Enter/Exit are triggered by stack operations and may be influenced by AlwaysEnterExit (see below).

---

## STATE MACHINE

StateMachine stores a stack of State instances and exposes simple lifecycle and transition APIs.

### Constructor
```csharp
public StateMachine(params State[] states)
```
|Parameter|Description|
|---|---|
|**states**|Initial states to place on the stack (the last item will be on top). These are wired to the machine but not entered until Start() is called|

Constructor details:
- States are pushed in the provided order. The final element in the array becomes the top of the stack at startup.
- Enter is NOT called during construction. Use Start() to begin and enter the current top state.

### Properties
|Property|Description|
|---|---|
|**[CurrentState](StateMachine/CurrentState.md)**|Top state of the stack (null if empty)|
|**[AlwaysEnterExit](StateMachine/AlwaysEnterExit.md)**|When true, entering/exiting occurs on every push/pop interaction with the previous top state (see below)|

### Public Methods
|Method|Description|
|---|---|
|**[Start](StateMachine/Start.md)**|If the stack is not empty, calls Enter() on the current top state|
|**[Tick](StateMachine/Tick.md)**|If the stack is not empty, updates the current top state (GetTransition then possibly OnUpdate)|
|**[PushState](StateMachine/PushState.md)**|Optionally exits the current top (if AlwaysEnterExit is true), pushes the new state, wires it to the machine, and calls its Enter()|
|**[PopState](StateMachine/PopState.md)**|Calls Exit() on the current top, pops it, and optionally re-enters the new top (if AlwaysEnterExit is true)|

### Enter/Exit Policy (AlwaysEnterExit)
- When AlwaysEnterExit is false (default):
  - PushState: Only the new state receives Enter(); the previous top remains paused (no Exit()).
  - PopState: The popped state receives Exit(); the newly exposed state resumes without Enter().
- When AlwaysEnterExit is true:
  - PushState: The previous top receives Exit() before pushing; the new state receives Enter().
  - PopState: The popped state receives Exit(); the newly exposed top receives Enter() again.

This option helps you decide whether underlying states should be considered paused (no Exit/Enter) or fully deactivated/activated across interruptions.
