using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace NgoUyenNguyen.Grid
{
    [BurstCompile]
    internal struct SquarePathfindingJob : IJob
    {
        [ReadOnly] public PathRequest Request;
        [ReadOnly] public int2 MapSize;
        [WriteOnly] public NativeArray<bool> HasPath;
        public NativeList<PathNode> Path;
        public NativeArray<PathNode> GridMap;
        public NativeHeap<PathNode> OpenSet;
        public NativeHashSet<PathNode> ClosedSet;


        public void Execute()
        {
            var startNode = GridMap[Request.From.x + Request.From.y * MapSize.x];
            var goalNode = GridMap[Request.To.x + Request.To.y * MapSize.x];
            if (!startNode.Walkable || !goalNode.Walkable)
            {
                HasPath[0] = false;
                return;
            }

            OpenSet.Push(startNode);
            while (OpenSet.Count > 0)
            {
                var currentNode = OpenSet.Pop();
                ClosedSet.Add(currentNode);

                // If reach the goal, return the path
                if (currentNode.Equals(goalNode))
                {
                    RetracePath(startNode, currentNode);
                    HasPath[0] = true;
                    return;
                }

                // Evaluate each neighbor of the current node
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        // Skip current node itself
                        if (x == 0 && y == 0) continue;
                        // Filter
                        if (Request.Filter == NeighborFilter.DiagonalOnly &&
                            ((x != 0 && y == 0) || (x == 0 && y != 0))) continue;
                        if (Request.Filter == NeighborFilter.OrthogonalOnly &&
                            x != 0 && y != 0) continue;

                        var neighborIndex = currentNode.GridIndex + new int2(x, y);
                        if (neighborIndex.x < 0 ||
                            neighborIndex.x >= MapSize.x ||
                            neighborIndex.y < 0 ||
                            neighborIndex.y >= MapSize.y)
                            continue;
                        var neighborNode = GridMap[neighborIndex.x + neighborIndex.y * MapSize.x];

                        // Skip unwalkable neighbor and neighbor already in the closed set
                        if (!neighborNode.Walkable || ClosedSet.Contains(neighborNode)) continue;

                        // Calculate the cost to reach the neighbor node
                        var newNeighborCost = currentNode.G + GetDistance(currentNode, neighborNode, Request.Filter);
                        if (newNeighborCost < neighborNode.G || !OpenSet.Contains(neighborNode))
                        {
                            neighborNode.G = newNeighborCost;
                            neighborNode.H = GetDistance(neighborNode, goalNode, Request.Filter);
                            neighborNode.ParentIndex = currentNode.GridIndex;
                            GridMap[neighborIndex.x + neighborIndex.y * MapSize.x] = neighborNode;

                            if (!OpenSet.Contains(neighborNode))
                                OpenSet.Push(neighborNode);
                        }
                    }
                }
            }

            HasPath[0] = false;
        }

        private int GetDistance(PathNode a, PathNode b, NeighborFilter filter)
        {
            if (filter == NeighborFilter.None)
            {
                int distX = math.abs(a.GridIndex.x - b.GridIndex.x);
                int distY = math.abs(a.GridIndex.y - b.GridIndex.y);

                int min = math.min(distX, distY);
                int max = math.max(distX, distY);
                return 14 * min + 10 * (max - min);
            }

            return 1;
        }

        private void RetracePath(PathNode startNode, PathNode goalNode)
        {
            var currentNode = goalNode;
            while (!currentNode.Equals(startNode))
            {
                Path.Add(currentNode);
                currentNode = GridMap[currentNode.ParentIndex.x + currentNode.ParentIndex.y * MapSize.x];
            }

            Path.Add(startNode);
            Path.Reverse();
        }
    }
}