# [State](../StackStateMachine.md##STATE).OnExit

---
## Declaration
```csharp
protected virtual void OnExit()
```

## Parameter

## Return

## Description
Called when this state stops being the active top of the stack. 
Override to perform cleanup logic. If [AlwaysEnterExit](../StackStateMachine.md#enterexit-policy-alwaysenterexit) is enabled, 
the previously active state will receive OnExit when another state is pushed on top; 
otherwise it remains paused without exiting until it is popped back to.
