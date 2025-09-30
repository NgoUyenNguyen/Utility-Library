using System.Collections.Generic;

namespace NgoUyenNguyen.PathFinding
{
    /// <summary>
    /// Represents a delegate that returns an enumerable collection of adjacent or neighboring nodes
    /// from the provided node during pathfinding operations.
    /// </summary>
    /// <typeparam name="T">The type of the nodes in the pathfinding graph.</typeparam>
    /// <param name="node">The node for which neighboring nodes need to be determined.</param>
    /// <returns>An enumerable collection of neighboring nodes.</returns>
    public delegate IEnumerable<T> GetNextNode<T>(T node);

    /// <summary>
    /// Represents a delegate that calculates the estimated distance, or heuristic,
    /// between a specified current node and the goal node in a pathfinding graph.
    /// </summary>
    /// <typeparam name="T">The type of the nodes in the pathfinding graph.</typeparam>
    /// <param name="currentNode">The node from which the distance to the goal is being calculated.</param>
    /// <param name="goalNode">The target node to which the distance is being estimated.</param>
    /// <returns>An integer value representing the estimated distance between the current node and the goal node.</returns>
    public delegate int GetDistanceToGoal<T>(T currentNode, T goalNode);

    /// <summary>
    /// Represents a delegate that calculates and returns the distance or cost
    /// between a specified node and one of its neighboring nodes during pathfinding operations.
    /// </summary>
    /// <typeparam name="T">The type of the nodes in the pathfinding graph.</typeparam>
    /// <param name="currentNode">The current node for which the distance to the neighbor is being calculated.</param>
    /// <param name="neighborNode">The neighboring node for which the distance from the current node is being determined.</param>
    /// <returns>The calculated distance or cost between the current node and the neighboring node.</returns>
    public delegate int GetDistanceToNeighbor<T>(T currentNode, T neighborNode);


    /// <summary>
    /// Provides pathfinding functionality using customizable parameters for determining neighbors, distances, and heuristics.
    /// </summary>
    /// <typeparam name="T">The type of the node used in the pathfinding algorithm. Must implement the IMonoPath interface.</typeparam>
    public class PathHandler<T> where T : class, IMonoPath
    {
        private readonly GetNextNode<T> getNextNode;
        private readonly GetDistanceToGoal<T> getDistanceToGoal;
        private readonly GetDistanceToNeighbor<T> getDistanceToNeighbor;

        /// <summary>
        /// Initializes a new instance of the PathHandler class with the specified path-finding parameters.
        /// This constructor sets up the core functionality needed for pathfinding operations.
        /// </summary>
        /// <param name="getNextNode">A function that returns all possible next nodes from a given node. Used to generate the neighboring nodes during path traversal.</param>
        /// <param name="getDistanceToGoal">A function that calculates and returns the estimated distance (heuristic) from a given node to the goal node.</param>
        /// <param name="getDistanceToNeighbor">A function that calculates and returns the distance between one node and its neighbor.</param>
        public PathHandler(
            GetNextNode<T> getNextNode,
            GetDistanceToGoal<T> getDistanceToGoal,
            GetDistanceToNeighbor<T> getDistanceToNeighbor)
        {
            this.getNextNode = getNextNode;
            this.getDistanceToGoal = getDistanceToGoal;
            this.getDistanceToNeighbor = getDistanceToNeighbor;
        }


        /// <summary>
        /// Method to find a walkable path from the start node to the goal node.
        /// </summary>
        /// <param name="start">The starting node for the pathfinding operation.</param>
        /// <param name="goal">The goal node the pathfinding operation is attempting to reach.</param>
        /// <param name="path">Outputs the list of nodes representing the path from start to goal if a path is found.</param>
        /// <returns>Returns true if a valid path from start to goal was found; otherwise, returns false.</returns>
        public bool FindPath(T start, T goal, out List<T> path)
        {
            path = null;
            if (!start.Walkable || !goal.Walkable) return false;
            
            var startNode = new PathNode<T>(start);
            var openSet = new Heap<PathNode<T>>(HeapType.Min); // The set of nodes to be evaluated
            var closedSet = new HashSet<PathNode<T>>(); // The set of nodes already evaluated
            var allNodes = new Dictionary<T, PathNode<T>>(); // The dictionary to map nodes to their corresponding PathNode
            openSet.Push(startNode);
            allNodes.Add(start, startNode);

            while (openSet.Count > 0)
            {
                var current = openSet.Pop();
                closedSet.Add(current);

                // If reached the goal, return the path
                if (current.Data == goal)
                {
                    path = RetracePath(startNode, current);
                    return true;
                }

                // Evaluate each neighbor of the current node
                foreach (var neighbor in getNextNode(current.Data))
                {
                    // Ignore unwalkable neighbors
                    if (!neighbor.Walkable) continue;
                    
                    // Create a new node for the neighbor if it doesn't exist yet'
                    if (!allNodes.TryGetValue(neighbor, out var neighborNode))
                    {
                        neighborNode = new PathNode<T>(neighbor);
                        allNodes.Add(neighbor, neighborNode);
                    }

                    // Ignore neighbors that have already been evaluated
                    if (closedSet.Contains(neighborNode)) continue;

                    // Calculate the cost to reach the neighbor node  
                    int newGCost = current.GCost + getDistanceToNeighbor(current.Data, neighbor);
                    if (newGCost < neighborNode.GCost || !openSet.Contains(neighborNode))
                    {
                        neighborNode.GCost = newGCost;
                        neighborNode.HCost = getDistanceToGoal(neighbor,goal);
                        neighborNode.Parent = current;

                        if (!openSet.Contains(neighborNode))
                            openSet.Push(neighborNode);
                    }
                }
            }
            return false;
        }

        private List<T> RetracePath(PathNode<T> startNode, PathNode<T> goalNode)
        {
            List<T> path = new();
            PathNode<T> current = goalNode;
            while (current != null)
            {
                path.Add(current.Data);
                current = current.Parent;
            }

            path.Reverse();
            return path;
        }
    }
}