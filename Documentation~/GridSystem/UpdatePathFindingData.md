# [Grid](GridSystem.md##GRID-INCLUDES).UpdatePathFindingData
---
## Declaration
```csharp
public void UpdatePathFindingData()
```

### Parameters

### Returns

## Description
Because of performance reasons, the pathfinding system uses a separate data structure as a snapshot of the grid. 
This snapshot will be automatically built on the first time [RequestPath](RequestPath.md) is called. However, if the grid is modified, 
the pathfinding data will not be updated automatically. 
This method will update the pathfinding data to match the current state of the grid.  

**Notice: Pathfinding operations which are running when this method is called will still use the old data. 
So it is recommended to check [RunningPathfindingCount](RunningPathfindingCount.md) before calling this method.**