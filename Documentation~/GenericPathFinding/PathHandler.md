# Generic Path Finding
***Provides pathfinding functionality using customizable parameters for determining neighbors, distances, and heuristics.***
---
## Usage
To using this pathfinding system, first of all, you need to define what is nodes for the system working with by implement IMonoPath interface in which
 class you want to be a node. In the example below, I will use Cell class from GridSystem, 
however, you can make your own class that implement IMonoPath interface.

```csharp
public class TestCell : Cell, IMonoPath
{
    // By implementing this property, you can determine if the cell is walkable or not
    public bool Walkable {get;}
}
```

Now we have nodes for the system to work with, now you can use the system to find path between two nodes.

```csharp
public class ExampleUnit : MonoBehaviour
{
    // This board is a concrete implementation of the Grid<TestCell> 
    [SerializeField] private Board board;
    // Handle to get path between two nodes
    private PathHandler<TestCell> pathHandler;
    
    // Nodes to find path between
    public TestCell startNode;
    public TestCell endNode;
    
    private void Start()
    {
        // To initialize path handler, you need to provide the following parameters
        // getNextNode: Function that returns neighbors for a given node to expand the search
        // getDistanceToGoal: Function that returns the distance between a given node and the goal node
        // getDistanceToNeighbor: Function that returns the distance between a given node and a neighbor node
        pathHandler = new PathHandler<TestCell>(
            getNextNode: (cell) => board.GetNeighbors(cell),
            getDistanceToGoal: (cell, goal) => Mathf.FloorToInt(Vector2.Distance(cell.transform.position, goal.transform.position)),
            getDistanceToNeighbor: (cell, neighbor) => Mathf.FloorToInt(Vector2.Distance(cell.transform.position, neighbor.transform.position))
            );
        // Later on, whenever you handler finds a path, it will use these functions to get the path
    }
    
    private void Update()
    {
        // To get a path between two nodes, you need to call FindPath method
        // This method will return true if it finds a path between the two nodes otherwise, it will return false
        // The path will be stored in the out parameter path, if there is no path, path will be null
        if (pathHandler.FindPath(startNode, endNode, out var path))
        {
            foreach (var node in path)
            {
                // I will using index of the Cell(in GridSystem) to debug
                Debug.Log(node.index);
            }
        }
    }
}
```
Notice that I was using GridSystem because it was explicitly defined these required functions. 
However, this pathfinding system not only works with GridSystem, but any class or system. It only cares about:
- What nodes should it work with, is it walkable or not,
- What function should it use to get neighbors of current node to expand the search, 
- What function should it use to get distance between a node and the goal, 
- And what function should it use to get distance between a node and its neighbors.  

Lastly, because of being generic and running on main-thread, 
the performance of this pathfinding system is much relied on your implementation of getDistanceToGoal, getDistanceToNeighbor, and getNextNode.