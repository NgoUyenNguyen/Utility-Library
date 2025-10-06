# [Grid](GridSystem.md##GRID-INCLUDES).GetNeighbors
---
## Declaration
```csharp
public HashSet<T> GetNeighbors(T cell) where T : Cell
```

### Parameters
| Parameter  | Description                                 |
|------------|---------------------------------------------|
| **cell**   | Origin [Cell](GridSystem.md##CELL-INCLUDES) |        

### Returns
All neighbors surrounding original cell.

## Declaration
```csharp
public HashSet<T> GetNeighbors(T cell, NeighborFilter filter) where T : Cell
```

### Parameters
| Parameter  | Description                                   |
|------------|-----------------------------------------------|
| **cell**   | Origin [Cell](GridSystem.md##CELL-INCLUDES)   |
| **filter** | filter to define which neighbors you want to get |

### Returns
Neighbors that match the filter.

## Description
Method to get all neighbors surrounding one [Cell](GridSystem.md##CELL-INCLUDES)<br/>
NOTICE: filter only works with Square Grid. Otherwise, it always returns all neighbors