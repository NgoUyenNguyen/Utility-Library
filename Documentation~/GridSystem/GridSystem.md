# Grid System
***Grid extended conponent for managing 3D object (cell prefab) with serialized reference, allow saving as a prefab which still remaind reference to its cells 
and support accessing and iterating like a collection.***

**[Usage](Usage.md)**

---
## CELL INCLUDES:

### Properties
|Property| Description           |
|---|-----------------------|
|**[index](index.md)**| Index postion of cell |
|**[grid](grid.md)**| Grid reference        |

### Public Methods
|Method|Description|
|---|---|
|**[RemoveGridReference](RemoveGridReference.md)**|Remove reference to Grid|

---

## GRID INCLUDES:

### Indexer
This Grid component supports accessing its cell like a collection through 2 ways:
```csharp
// x and y are index position of cell
Grid[int: x, int: y];
```
```csharp
// using Vector2Int index position of cell
Grid[Vector2Int: index];
```

### Iterator
This Grid component supports foreach iterating
```csharp
foreach (Cell cell in Grid)
{
	// Iterating through all cell, 
	// including null ones
}
```

### Properties
|Property|Description|
|---|---|
|**[cellMap](cellMap.md)**|Represents the collection of cells in the grid|
|**[cellPrefab](cellPrefab.md)**|Prefab of cell spawned in the grid|
|**[size](size.md)**|Size of the grid|
|**[cellSize](cellSize.md)**|Size of each cell|
|**[alignment](alignment.md)**|Alignment of the grid|
|**[space](space.md)**|Coordinate space in which the operation is performed|
|**[layout](layout.md)**|Layout of <c>Grid</c>|

### Public Methods
| Method                                                  |Description|
|---------------------------------------------------------|---|
| **[PrefabCreate](PrefabCreate.md)**                     |**USE ONLY IN EDITOR**<br> Method to create grid which remaining its connecting to original cell prefab|
| **[Create](Create.md)**                                 |Method to create grid|
| **[CalculateCellsPosition](CalculateCellsPosition.md)** |Method to calculate cells **LocalSpace** position|
| **[CellToLocal](CellToLocal.md)**                       |Method to get **LocalSpace** position from Cell|
| **[CellToWorld](CellToWorld.md)**                       |Method to get **WolrdSpace** position from Cell|
| **[LocalToCell](LocalToCell.md)**                       |Method to get Cell from **LocalSpace** position|
| **[WorldToCell](WorldToCell.md)**                       |Method to get Cell from **WolrdSpace** position|
| **[LocalToIndex](LocalToIndex.md)**                     |Method to get index from **LocalSpace** position|
| **[WorldToIndex](WorldToIndex.md)**                     |Method to get index from **WolrdSpace** position|
| **[IndexToLocal](IndexToLocal.md)**                     |Get **LocalSpace** position from Index|
| **[IndexToWorld](IndexToWorld.md)**                     |Get **WolrdSpace** position from Index|
| **[GetNeighbor](GetNeighbor.md)**                       |Method to get another cell from one cell|
| **[GetNeighbors](GetNeighbors.md)**                     |Method to get all neighbor cells surrounding one cell|
| **[GetRing](GetRing.md)**                               |Method to get all cells in a ring around one cell|
| **[CombineMesh.md](CombineMesh.md)**                    |Combines the meshes of child objects into a single mesh for optimization.|
| **[GetPath.md](GetPath.md)**                            |Finds a path between two cells in the grid|
