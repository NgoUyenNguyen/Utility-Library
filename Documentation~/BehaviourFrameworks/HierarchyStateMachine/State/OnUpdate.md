# [State](../HierarchyStateMachine.md##STATE).OnUpdate

---
## Declaration
```csharp
protected virtual void OnUpdate(float deltaTime)
```

## Parameter
|Parameter|Description|
|---|---|
|deltaTime|Time elapsed since the last update||
## Return

## Description
Per-frame update while this state is active. Override to implement runtime logic. 
Notice that not only the current state's but also all its parents state (to root) OnUpdate will be called.
