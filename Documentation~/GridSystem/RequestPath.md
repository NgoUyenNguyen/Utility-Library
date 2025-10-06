# [Grid](GridSystem.md##GRID-INCLUDES).RequestPath
---
## Declaration
```csharp
public PathHandle RequestPath(T from, T to, NeighborFilter filter = NeighborFilter.None) where T : Cell
```

### Parameters
| Parameter | Description                                                                                |
|-----------|--------------------------------------------------------------------------------------------|
| **from**  | The starting cell of the path.                                                             |
| **to**    | The ending cell of the path.                                                               |
| **filter** | The filter to use when searching for neighbors<br/>By default it is **NeighborFilter.None**|

### Returns
The [PathHandle.md](PathHandle.md) equivalent to the request. 

## Description
Because pathfinding algorithms can be expensive, so instead of directly calculating and returning a path on the main thread, which can cause performance issues and potentially freeze the application, 
this method requests the grid to find a path between two cells in a different thread. 
This ensures that the main thread remains responsive and the application does not become unresponsive while the pathfinding algorithm runs in the background.
Then after the pathfinding algorithm has finished, the path result will be return to the [PathHandle](PathHandle.md) for accessing.