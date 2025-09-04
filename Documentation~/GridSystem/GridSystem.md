# Grid System
***Grid extended conponent for managing 3D object (cell prefab) with serialized reference, allow saving as a prefab which still remaind reference to its cells 
and support accessing and iterating like a collection.
For using, set a game object which having Cell conponent as a prefab and set reference in this Grid component.***

---
## CELL INCLUDES:

### Indexer
This Grid component supports accessing its cell like a collection through 2 ways:
```csharp
// x and y are index of cell
Grid[int: x, int: y];
```
```csharp
// index is a Vector2Int, representing index of cell
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
|**[index](index.md)**| Grid index of Cell|
|**[grid](grid.md)**| Grid reference|

### Public Methods
|Method|Description|
|---|---|
|**[RemoveGridReference](RemoveGridReference.md)**|Remove reference to Grid|

---

## GRID INCLUDES:

### Properties
|Property|Description|
|---|---|
|**[cellMap](cellMap.md)**|Represents the collection of cells in the grid|
|**[cellPrefab](cellPrefab.md)**|Prefab of cell spawned in the grid|
|**[size](size.md)**|Size of the grid|
|**[cellSize](cellSize.md)**|Size of each cell|
|**[alignment](alignment.md)**|Alignment of the grid|
|**[space](space.md)**|Coordinate space in which the operation is performed|

### Public Methods
|Method|Description|
|---|---|
|**[PrefabCreate](PrefabCreate.md)**|**USE ONLY IN EDITOR**<br> Method to create grid which remaining its connecting to original cell prefab|
|**[SpawnCellPrefab](SpawnCellPrefab.md)**|**USE ONLY IN EDITOR**<br> Method to spawn prefab at specific index|
|**[Create](Create.md)**|Method to create grid|
|**[CalculateCellsPosition](CalculateCellsPosition.md)**|Method to calculate cells **LocalSpace** position|
|**[CellToLocal](CellToLocal.md)**|Method to get **LocalSpace** position from Cell|
|**[CellToWorld](CellToWorld.md)**|Method to get **WolrdSpace** position from Cell|
|**[LocalToCell](LocalToCell.md)**|Method to get Cell from **LocalSpace** position|
|**[WorldToCell](WorldToCell.md)**|Method to get Cell from **WolrdSpace** position|
|**[LocalToIndex](LocalToIndex.md)**|Method to get index from **LocalSpace** position|
|**[WorldToIndex](WorldToIndex.md)**|Method to get index from **WolrdSpace** position|
|**[IndexToLocal](IndexToLocal.md)**|Get **LocalSpace** position from Index|
|**[IndexToWorld](IndexToWorld.md)**|Get **WolrdSpace** position from Index|
|**[GetNeighbor](GetNeighbor.md)**|Method to get another cell from one cell|
|**[GetNeighbors](GetNeighbors.md)**|Method to get all neighbor cells surrounding one cell|
|**[GetNeighborsInDiagonal](GetNeighborsInDiagonal.md)**|Method to get only diagonal neighbor cells from one cell|
|**[GetNeighborsIgnoreDiagonal](GetNeighborsIgnoreDiagonal.md)**|Method to get neighbor cells from one cell , ignore diagonally|
