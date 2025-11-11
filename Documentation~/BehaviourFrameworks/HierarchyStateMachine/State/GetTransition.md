# [State](../HierarchyStateMachine.md##STATE).GetTransition

---
## Declaration
```csharp
protected virtual State GetTransition()
```

## Parameter

## Return
The target state to transition to. Null if no transition is requested this update.

## Description
Override to provide a target state when this state decides to transition. This method is called once per update, before 
[OnUpdate](OnUpdate.md) is called.
