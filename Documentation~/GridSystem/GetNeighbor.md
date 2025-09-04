# [Grid](GridSystem.md##GRID-INCLUDES).GetNeighbor
---
## Declaration
```csharp
public Cell GetNeighbor(Cell cell, Vector2Int indexDelta)
```

### Parameters
|Parameter|Description|
|---|---|
|**cell**|Origin [Cell](GridSystem.md##CELL-INCLUDES)|
|**indexDelta**|Delta index from target [Cell](GridSystem.md##CELL-INCLUDES) to origin [Cell](GridSystem.md##CELL-INCLUDES)|

### Returns
Neighbor [Cell](GridSystem.md##CELL-INCLUDES) from input [Cell](GridSystem.md##CELL-INCLUDES) plus indexDelta

## Description
Method to get another [Cell](GridSystem.md##CELL-INCLUDES) from one [Cell](GridSystem.md##CELL-INCLUDES) and delta of index.
For example, with indexDelta equals to (1,0) will return the right neighbor and (2,0) will return the right second neighbor.
