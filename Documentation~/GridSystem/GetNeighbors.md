# [Grid](GridSystem.md##GRID-INCLUDES).GetNeighbors
---
## Declaration
```csharp
public HashSet<Cell> GetNeighbors(Cell cell)
```

### Parameters
|Parameter|Description|
|---|---|
|**cell**|Origin [Cell](GridSystem.md##CELL-INCLUDES)|

### Returns
Maximum 8 neighbor [Cells](GridSystem.md##CELL-INCLUDES) and ignore null neighbors

## Description
Method to get all neighbors surrounding one [Cell](GridSystem.md##CELL-INCLUDES)