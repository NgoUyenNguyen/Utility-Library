# [State](../StackStateMachine.md##STATE).OnEnter

---
## Declaration
```csharp
protected virtual void OnEnter()
```

## Parameter

## Return

## Description
Called when this state becomes the active top of the stack. 
Override to perform setup logic for this state. Depending on the machine's [AlwaysEnterExit](../StackStateMachine.md#enterexit-policy-alwaysenterexit) setting, 
OnEnter may be called again when this state is revealed after another state is popped (if AlwaysEnterExit is true).
