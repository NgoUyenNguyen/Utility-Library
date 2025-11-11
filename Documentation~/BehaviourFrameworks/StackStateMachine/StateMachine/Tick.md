# [StateMachine](../StackStateMachine.md##STATEMACHINE).Tick

---
## Declaration
```csharp
public void Tick()
```

## Parameter

## Return

## Description
Advances the state machine by one frame.

- If the machine has not been started yet, this will call [Start](Start.md) and return immediately.
- If the stack is empty, nothing happens.
- Otherwise, calls [OnUpdate](../State/OnUpdate.md) on the [CurrentState](CurrentState.md), which in turn runs the state's transition check then OnUpdate if it remains on top.
