# [State](../StackStateMachine.md##STATE).OnUpdate

---
## Declaration
```csharp
protected virtual void OnUpdate()
```

## Parameter

## Return

## Description
Per-frame update for this state, invoked only if this state remains the top of the stack after [GetTransition](GetTransition.md) 
runs. Use this for active behavior while the state is current.
