# [Grid](GridSystem.md##GRID-INCLUDES).GetPath
---
## Declaration
```csharp
public bool GetPath<T>(Cell from, Cell to, out List<T> path)
```

### Parameters
| Parameter  | Description                                   |
|------------|-----------------------------------------------|
| **from**   | The starting [Cell](GridSystem.md##CELL-INCLUDES) of the path|
| **to**     | The destination [Cell](GridSystem.md##CELL-INCLUDES) of the path |
| **path**   | Output list containing the cells that form the path, if found. |

### Returns
True if a valid path is found, otherwise false.

## Declaration
```csharp
public bool GetPath<T>(Cell from, Cell to, NeighborFilter filter, out List<T> path)
```

### Parameters
| Parameter  | Description                                |
|------------|--------------------------------------------|
| **from**   | The starting [Cell](GridSystem.md##CELL-INCLUDES) of the path|
| **to**     | The destination [Cell](GridSystem.md##CELL-INCLUDES) of the path |
| **filter** | The filter to define which neighbors to consider|
| **path**   | Output list containing the cells that form the path, if found. |

### Returns
True if a valid path is found, otherwise false.

## Description
Method to finds a path between two cells in the grid.