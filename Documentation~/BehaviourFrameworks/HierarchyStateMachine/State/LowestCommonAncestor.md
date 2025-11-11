# [State](../HierarchyStateMachine.md##STATE).LowestCommonAncestor

---
## Declaration
```csharp
public static State LowestCommonAncestor(State a, State b)
```

## Parameters
|Parameter|Description|
|---|---|
|a|First state.|
|b|Second state.|

## Return
The lowest common ancestor between the two states.

## Description
Finds the lowest common ancestor between two states by walking parents of one and checking against the other's lineage. Returns null if no common ancestor exists.
