# [State](../HierarchyStateMachine.md##STATE).GetInitialState

---
## Declaration
```csharp
protected virtual State GetInitialState()
```

## Parameter

## Return
The state that should be entered automatically when this state is entered.

## Description
Override to define the initial child state that should be entered automatically when this state is entered. Return null if this state has no initial child.
