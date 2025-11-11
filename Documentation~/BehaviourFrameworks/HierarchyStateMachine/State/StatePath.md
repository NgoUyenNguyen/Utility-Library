# [State](../HierarchyStateMachine.md##STATE).StatePath

---
## Declaration
```csharp
public static string StatePath(State s)
```

## Parameter
|Parameter|Description|
|---|---|
|s|The state to get the path for.|
## Return
The path as a string.

## Description
Returns a readable string path from the root to the given state, with names separated by " > ". Useful for logging and debugging.
